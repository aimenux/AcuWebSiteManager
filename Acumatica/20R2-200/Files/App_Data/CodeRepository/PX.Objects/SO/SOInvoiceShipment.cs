using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.AR.MigrationMode;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.SM;

namespace PX.Objects.SO
{
	using static SOInvoiceShipment.WellKnownActions.SOShipmentScreen;

	[GL.TableAndChartDashboardType]
	public class SOInvoiceShipment : PXGraph<SOInvoiceShipment>
	{
		public PXCancel<SOShipmentFilter> Cancel;
		public PXAction<SOShipmentFilter> viewDocument;
		public PXFilter<SOShipmentFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<SOShipment, SOShipmentFilter> Orders;
		public PXSelect<SOShipLine> dummy_select_to_bind_events;
		public PXSetup<SOSetup> sosetup;

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Orders.Current != null)
			{
				if (Orders.Current.ShipmentType == INDocType.DropShip)
				{
					POReceiptEntry docgraph = PXGraph.CreateInstance<POReceiptEntry>();
					docgraph.Document.Current = docgraph.Document.Search<POReceipt.receiptNbr>(Orders.Current.ShipmentNbr);

					throw new PXRedirectRequiredException(docgraph, true, PO.Messages.POReceipt) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
					docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(Orders.Current.ShipmentNbr);

					throw new PXRedirectRequiredException(docgraph, true, Messages.SOShipment) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

        public PXSelect<INSite> INSites;
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = Messages.SiteDescr, Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void _(Events.CacheAttached<INSite.descr> e) { }

        public PXSelect<Carrier> Carriers;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.CarrierDescr, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<Carrier.description> e) { }

        public SOInvoiceShipment()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			Orders.SetSelected<SOShipment.selected>();
            object item = sosetup.Current;
		}

		public virtual void SOShipmentFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			SOShipmentFilter filter = e.Row as SOShipmentFilter;
			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(filter);
				Orders.SetProcessWorkflowAction(filter.Action, parameters);
			}

			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.invoiceDate>(sender, filter, filter.Action.IsIn(CreateInvoice, CreateDropshipInvoice));
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.packagingType>(sender, filter, filter.Action != CreateDropshipInvoice);
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.siteID>(sender, filter, filter.Action != CreateDropshipInvoice);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.showPrinted>(sender, filter, filter.Action.IsIn(PrintLabels, PrintPickList));

			PXUIFieldAttribute.SetDisplayName<SOShipment.shipmentNbr>(Orders.Cache, filter.Action == CreateDropshipInvoice ? Messages.ReceiptNbr : Messages.ShipmentNbr);
			PXUIFieldAttribute.SetDisplayName<SOShipment.shipDate>(Orders.Cache, filter.Action == CreateDropshipInvoice ? Messages.ReceiptDate : Messages.ShipmentDate);

			if (sosetup.Current.UseShipDateForInvoiceDate == true)
			{
				sender.RaiseExceptionHandling<SOShipmentFilter.invoiceDate>(filter, null, new PXSetPropertyException(Messages.UseInvoiceDateFromShipmentDateWarning, PXErrorLevel.Warning));
				PXUIFieldAttribute.SetEnabled<SOShipmentFilter.invoiceDate>(sender, filter, false);
			}

			bool warnShipNotInvoiced =
				filter.Action == UpdateIN && SOShipmentEntry.NeedWarningShipNotInvoicedUpdateIN(sosetup.Current, Orders.SelectMain());
			Exception warnShipNotInvoicedExc = warnShipNotInvoiced
				? new PXSetPropertyException(Messages.ShipNotInvoicedWarning, PXErrorLevel.Warning)
				: null;
			sender.RaiseExceptionHandling<SOShipmentFilter.action>(filter, null, warnShipNotInvoicedExc);

			bool showPrintSettings = IsPrintingAllowed(filter);

			PXUIFieldAttribute.SetVisible<SOShipmentFilter.printWithDeviceHub>(sender, filter, showPrintSettings);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.definePrinterManually>(sender, filter, showPrintSettings);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.printerID>(sender, filter, showPrintSettings);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.numberOfCopies>(sender, filter, showPrintSettings);

			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.definePrinterManually>(sender, filter, filter.PrintWithDeviceHub == true);
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.numberOfCopies>(sender, filter, filter.PrintWithDeviceHub == true);
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.printerID>(sender, filter, filter.PrintWithDeviceHub == true && filter.DefinePrinterManually == true);

			if (filter.PrintWithDeviceHub != true || filter.DefinePrinterManually != true)
			{
				filter.PrinterID = null;
			}

			bool showInvoiceSeparately = filter.Action.IsIn(CreateInvoice, CreateDropshipInvoice);
			PXUIFieldAttribute.SetEnabled<SOShipment.billSeparately>(Orders.Cache, null, showInvoiceSeparately
				&& PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.NotExists);
			PXUIFieldAttribute.SetVisible<SOShipment.billSeparately>(Orders.Cache, null, showInvoiceSeparately);
		}

		protected virtual bool IsPrintingAllowed(SOShipmentFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				filter.Action.IsIn(
					PrintLabels,
					PrintPickList,
					PrintShipmentConfirmation);
		}

		protected bool _ActionChanged = false;

		public virtual void SOShipmentFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<SOShipmentFilter.action>(e.Row, e.OldRow);
			if (_ActionChanged && e.Row != null)
			{
				var row = ((SOShipmentFilter)e.Row);

				row.PackagingType = SOPackageType.ForFiltering.Both;

				if (row.Action == CreateDropshipInvoice)
					row.SiteID = null;
			}

			if ((_ActionChanged || !sender.ObjectsEqual<SOShipmentFilter.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOShipmentFilter.printWithDeviceHub>(e.Row, e.OldRow))
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true)
			{
				string actualReportID = SOReports.GetReportID(null, Filter.Current.Action);
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(ARNotificationSource.Customer, actualReportID, Accessinfo.BranchID);
			}
		}
				
		protected virtual void SOShipmentFilter_PrinterName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOShipmentFilter row = (SOShipmentFilter)e.Row;
			if (row != null)
			{
				if (!IsPrintingAllowed(row))
					e.NewValue = null;
			}
		}

		public virtual IEnumerable orders()
		{
			PXUIFieldAttribute.SetDisplayName<SOShipment.customerID>(Caches[typeof(SOShipment)], Messages.CustomerID);

			if (Filter.Current.Action == PX.Data.Automation.PXWorkflowMassProcessingAttribute.Undefined)
				yield break;

			if (_ActionChanged)
				Orders.Cache.Clear();

			PXSelectBase cmd = GetShipmentsSelectCommand(Filter.Current);

			if (cmd is PXSelectBase<SOShipment> shCmd)
			{
				ApplyShipmentFilters(shCmd, Filter.Current);

				int startRow = PXView.StartRow;
				int totalRows = 0;

				foreach (object res in shCmd.View.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					SOShipment shipment = PXResult.Unwrap<SOShipment>(res);
					SOShipment cached = (SOShipment)Orders.Cache.Locate(shipment);
					if (cached != null)
					{
						shipment.Selected = cached.Selected;
						shipment.BillSeparately = cached.BillSeparately;
					}
					yield return shipment;
				}
				PXView.StartRow = 0;
			}

			if (cmd is PXSelectBase<POReceipt> rtCmd)
			{
				ApplyReceiptFilters(rtCmd, Filter.Current);

				foreach (PXResult<POReceipt, SOOrderShipment, SOOrder> res in rtCmd.Select())
				{
					SOShipment shipment = res;
					SOShipment cached = (SOShipment)Orders.Cache.Locate(shipment);

					if (cached == null)
						Orders.Cache.SetStatus(shipment, PXEntryStatus.Held);
					else
					{
						shipment.Selected = cached.Selected;
						shipment.BillSeparately = cached.BillSeparately;
					}
					yield return shipment;
				}
			}
			Orders.Cache.IsDirty = false;
		}

		protected virtual PXSelectBase GetShipmentsSelectCommand(SOShipmentFilter filter)
		{
			PXSelectBase cmd;

			switch (filter.Action)
			{
				case CreateInvoice:
				{
					cmd = new
						SelectFrom<SOShipment>.
						InnerJoin<INSite>.On<SOShipment.FK.Site>.
						InnerJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
						LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
						Where<
							SOShipment.confirmed.IsEqual<True>.
							And<Match<Customer, AccessInfo.userName.FromCurrent>>.
							And<Match<INSite, AccessInfo.userName.FromCurrent>>.
							And<Exists<
								SelectFrom<SOOrderShipment>.
								Where<
									SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
									And<SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>>.
									And<SOOrderShipment.invoiceNbr.IsNull>.
									And<SOOrderShipment.createARDoc.IsEqual<True>>>>>>.
						View(this);
					break;
				}
				case UpdateIN:
				{
					cmd = new
						SelectFrom<SOShipment>.
						InnerJoin<INSite>.On<SOShipment.FK.Site>.
						LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
						LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
						Where<
							SOShipment.confirmed.IsEqual<True>.
							And<
								Customer.bAccountID.IsNull.
								Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
							And<Match<INSite, AccessInfo.userName.FromCurrent>>.
							And<Exists<
								SelectFrom<SOOrderShipment>.
								Where<
									SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
									And<SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>>.
									And<SOOrderShipment.invtRefNbr.IsNull>.
									And<SOOrderShipment.createINDoc.IsEqual<True>>>>>>.
						View(this);
					break;
				}
				case CreateDropshipInvoice:
				{
					cmd = new
						SelectFrom<POReceipt>.
						InnerJoin<SOOrderShipment>.On<
							SOOrderShipment.shipmentNbr.IsEqual<POReceipt.receiptNbr>.
							And<SOOrderShipment.shipmentType.IsEqual<SOShipmentType.dropShip>>>.
						InnerJoin<SOOrder>.On<SOOrderShipment.FK.Order>.
						InnerJoin<SOOrderType>.On<SOOrder.FK.OrderType>.
						InnerJoin<Customer>.On<SOOrder.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
						Where<
							POReceipt.released.IsEqual<True>.
							//And<
							//	POReceiptLine.lineType.IsEqual<POLineType.goodsForDropShip>.
							//	Or<POReceiptLine.lineType.IsEqual<POLineType.nonStockForDropShip>>>.
							And<SOOrderType.aRDocType.IsNotEqual<ARDocType.noUpdate>>.
							And<SOOrderShipment.invoiceNbr.IsNull>.
							And<Match<Customer, AccessInfo.userName.FromCurrent>>>.
						AggregateTo<
							GroupBy<POReceipt.receiptNbr>,
							GroupBy<POReceipt.createdByID>,
							GroupBy<POReceipt.lastModifiedByID>,
							GroupBy<POReceipt.released>,
							GroupBy<POReceipt.ownerID>,
							GroupBy<POReceipt.hold>>.
						View(this);
					break;
				}

				//case CancelReturn: // looks like legacy code
				//	cmd = new PXSelectJoinGroupBy<SOShipment,
				//		InnerJoin<INSite, On<SOShipment.FK.Site>,
				//		LeftJoin<Carrier, On<SOShipment.FK.Carrier>,
				//		InnerJoin<SOOrderShipment, On<SOOrderShipment.shipmentType, Equal<SOShipment.shipmentType>, And<SOOrderShipment.shipmentNbr, Equal<SOShipment.shipmentNbr>>>,
				//		InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
				//		InnerJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>>>>>,
				//	Where<SOShipment.labelsPrinted, Equal<False>,
				//	And2<Match<Customer, Current<AccessInfo.userName>>,
				//	And<Match<INSite, Current<AccessInfo.userName>>>>>,
				//	Aggregate<GroupBy<SOShipment.shipmentNbr,
				//		GroupBy<SOShipment.createdByID,
				//		GroupBy<SOShipment.lastModifiedByID,
				//		GroupBy<SOShipment.confirmed,
				//		GroupBy<SOShipment.ownerID,
				//		GroupBy<SOShipment.released,
				//		GroupBy<SOShipment.hold,
				//		GroupBy<SOShipment.resedential,
				//		GroupBy<SOShipment.saturdayDelivery,
				//		GroupBy<SOShipment.groundCollect,
				//		GroupBy<SOShipment.saturdayDelivery,
				//		GroupBy<SOShipment.shippedViaCarrier,
				//		GroupBy<SOShipment.labelsPrinted>>>>>>>>>>>>>>>(this);
				//	break;

				default:
				{
					cmd = new
						SelectFrom<SOShipment>.
						InnerJoin<INSite>.On<SOShipment.FK.Site>.
						LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
						LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
						Where<
							Match<INSite, AccessInfo.userName.FromCurrent>.
							And<
								Customer.bAccountID.IsNull.
								Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
							And<Exists<
								SelectFrom<SOOrderShipment>.
								Where<
									SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
									And<SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>>>>>>.
						View(this);
					break;
				}
			}

			return cmd;
		}

		protected virtual void ApplyShipmentFilters(PXSelectBase<SOShipment> shCmd, SOShipmentFilter filter)
		{
			shCmd.WhereAnd<Where<SOShipment.shipDate.IsLessEqual<SOShipmentFilter.endDate.FromCurrent>>>();
			shCmd.WhereAnd<Where<WhereWorkflowActionEnabled<SOShipment, SOShipmentFilter.action>>>();

			if (filter.CustomerID != null)
				shCmd.WhereAnd<Where<SOShipment.customerID.IsEqual<SOShipmentFilter.customerID.FromCurrent>>>();

			if (!string.IsNullOrEmpty(filter.ShipVia))
				shCmd.WhereAnd<Where<SOShipment.shipVia.IsEqual<SOShipmentFilter.shipVia.FromCurrent>>>();

			if (filter.StartDate != null)
				shCmd.WhereAnd<Where<SOShipment.shipDate.IsGreaterEqual<SOShipmentFilter.startDate.FromCurrent>>>();

			if (!string.IsNullOrEmpty(filter.CarrierPluginID))
				shCmd.WhereAnd<Where<Carrier.carrierPluginID.IsEqual<SOShipmentFilter.carrierPluginID.FromCurrent>>>();

			if (filter.SiteID != null)
				shCmd.WhereAnd<Where<SOShipment.siteID.IsEqual<SOShipmentFilter.siteID.FromCurrent>>>();

			if (filter.Action == PrintShipmentConfirmation)
				shCmd.WhereAnd<Where<SOShipment.confirmed.IsNotEqual<True>>>();

			if (filter.Action == PrintLabels && filter.ShowPrinted == false)
				shCmd.WhereAnd<Where<SOShipment.labelsPrinted.IsEqual<False>>>();

			if (filter.Action == PrintPickList && filter.ShowPrinted == false)
				shCmd.WhereAnd<Where<SOShipment.pickListPrinted.IsEqual<False>>>();

			if (filter.PackagingType == SOPackageType.ForFiltering.Manual)
				shCmd.WhereAnd<Where<SOShipment.isManualPackage.IsEqual<True>>>();
			else if (filter.PackagingType == SOPackageType.ForFiltering.Auto)
				shCmd.WhereAnd<Where<SOShipment.isManualPackage.IsEqual<False>>>();
		}

		protected virtual void ApplyReceiptFilters(PXSelectBase<POReceipt> rtCmd, SOShipmentFilter filter)
		{
			rtCmd.WhereAnd<Where<POReceipt.receiptDate.IsLessEqual<SOShipmentFilter.endDate.FromCurrent>>>();

			if (filter.CustomerID != null)
				rtCmd.WhereAnd<Where<SOOrderShipment.customerID.IsEqual<SOShipmentFilter.customerID.FromCurrent>>>();

			if (filter.ShipVia != null)
				rtCmd.WhereAnd<Where<SOOrder.shipVia.IsEqual<SOShipmentFilter.shipVia.FromCurrent>>>();

			if (filter.CarrierPluginID != null)
			{
				rtCmd.Join<InnerJoin<Carrier, On<Carrier.carrierID.IsEqual<SOOrder.shipVia>>>>();
				rtCmd.WhereAnd<Where<Carrier.carrierPluginID.IsEqual<SOShipmentFilter.carrierPluginID.FromCurrent>>>();
			}

			if (filter.StartDate != null)
				rtCmd.WhereAnd<Where<POReceipt.receiptDate.IsGreaterEqual<SOShipmentFilter.startDate.FromCurrent>>>();
		}

		public class WellKnownActions
		{
			public class SOShipmentScreen
			{
				public const string ScreenID = "SO302000";

				public const string ConfirmShipment
					= ScreenID + "$" + nameof(SOShipmentEntry.confirmShipmentAction);

				public const string CreateInvoice
					= ScreenID + "$" + nameof(SOShipmentEntry.createInvoice);

				public const string CreateDropshipInvoice
					= ScreenID + "$" + nameof(SOShipmentEntry.createDropshipInvoice);

				public const string UpdateIN
					= ScreenID + "$" + nameof(SOShipmentEntry.UpdateIN);

				public const string PrintLabels
					= ScreenID + "$" + nameof(SOShipmentEntry.printLabels);

				public const string EmailShipment
					= ScreenID + "$" + nameof(SOShipmentEntry.emailShipment);

				public const string PrintPickList
					= ScreenID + "$" + nameof(SOShipmentEntry.printPickListAction);

				public const string PrintShipmentConfirmation
					= ScreenID + "$" + nameof(SOShipmentEntry.printShipmentConfirmation);
			}
		}
	}

    [Serializable]
	public partial class SOShipmentFilter : IBqlTable, PX.SM.IPrintable
	{
		#region Action
		[PX.Data.Automation.PXWorkflowMassProcessing(DisplayName = "Action")]
		public virtual string Action { get; set; }
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
		#endregion
		#region SiteID
		[Site(DescriptionField = typeof(INSite.descr))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region StartDate
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDBDate]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion
		#region CarrierPluginID
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Carrier", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>))]
		public virtual String CarrierPluginID { get; set; }
		public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
		#endregion
		#region ShipVia
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual String ShipVia { get; set; }
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		#endregion
		#region CustomerID
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region InvoiceDate
		[PXDBDate]
		[PXUIField(DisplayName = "Invoice Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? InvoiceDate { get; set; }
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		#endregion
		#region PackagingType
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOPackageType.ForFiltering.Both)]
		[SOPackageType.ForFiltering.List]
		[PXUIField(DisplayName = "Packaging Type")]
		public virtual String PackagingType { get; set; }
		public abstract class packagingType : PX.Data.BQL.BqlString.Field<packagingType> { }
		#endregion
		#region ShowPrinted
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Printed")]
		public virtual bool? ShowPrinted { get; set; }
		public abstract class showPrinted : PX.Data.BQL.BqlBool.Field<showPrinted> { }
		#endregion
		#region PrintWithDeviceHub
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub")]
		public virtual bool? PrintWithDeviceHub { get; set; }
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		#endregion
		#region DefinePrinterManually
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Define Printer Manually")]
		public virtual bool? DefinePrinterManually { get; set; } = false;
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		#endregion
		#region PrinterID
		[PXPrinterSelector]
		public virtual Guid? PrinterID { get; set; }
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		#endregion
		#region NumberOfCopies
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<SOShipmentFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies { get; set; }
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		#endregion

		public class PackagingTypeListAttribute : SOPackageType.ForFiltering.ListAttribute { }
	}
}