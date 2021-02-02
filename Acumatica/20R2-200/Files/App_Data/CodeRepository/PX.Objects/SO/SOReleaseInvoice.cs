using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Automation;
using PX.SM;
using PX.Objects.AR;
using PX.Objects.AR.MigrationMode;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt;
using PX.Common;

namespace PX.Objects.SO
{
	[TableAndChartDashboardType]
	public class SOReleaseInvoice : PXGraph<SOReleaseInvoice>
	{
		public PXCancel<SOInvoiceFilter> Cancel;
		public PXAction<SOInvoiceFilter> viewDocument;
		public PXFilter<SOInvoiceFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessing<ARInvoice, SOInvoiceFilter> SOInvoiceList;
		public virtual IEnumerable soInvoiceList() => GetInvoices();

		#region Cache Attached
		#region ARInvoice
		[PXRemoveBaseAttribute(typeof(ARDocType.ListAttribute))]
		[PXMergeAttributes]
		[ARDocType.SOEntryList]
		protected virtual void _(Events.CacheAttached<ARInvoice.docType> e) { }

		[PXRemoveBaseAttribute(typeof(ARInvoiceType.RefNbrAttribute))]
		[PXMergeAttributes]
		[ARInvoiceType.RefNbr(typeof(
			Search2<SOInvoice.refNbr,
			InnerJoin<AR.Standalone.ARRegisterAlias, On<
				AR.Standalone.ARRegisterAlias.docType, Equal<SOInvoice.docType>,
				And<AR.Standalone.ARRegisterAlias.refNbr, Equal<SOInvoice.refNbr>>>,
			InnerJoinSingleTable<ARInvoice, On<
				ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>,
				And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Customer, On<AR.Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>>,
			Where<
				SOInvoice.docType, Equal<Optional<SOInvoice.docType>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<SOInvoice.refNbr>>>), Filterable = true)]
		protected virtual void _(Events.CacheAttached<ARInvoice.refNbr> e) { }

		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(AROpenPeriodAttribute))]
		[PXMergeAttributes]
		[SOOpenPeriod(
			sourceType: typeof(ARRegister.docDate),
			branchSourceType: typeof(ARRegister.branchID),
			masterFinPeriodIDType: typeof(ARRegister.tranPeriodID),
			IsHeader = true)]
		protected virtual void _(Events.CacheAttached<ARInvoice.finPeriodID> e) { }

		[PXRemoveBaseAttribute(typeof(ARTermsSelectorAttribute))]
		[PXRemoveBaseAttribute(typeof(TermsAttribute))]
		[PXMergeAttributes]
		[SOInvoiceTerms]
		[PXSelector(typeof(
			Search<Terms.termsID,
			Where<Terms.visibleTo.IsIn<TermsVisibleTo.all, TermsVisibleTo.customer>>>),
			DescriptionField = typeof(Terms.descr), Filterable = true)]
		protected virtual void _(Events.CacheAttached<ARInvoice.termsID> e) { }

		[PXRemoveBaseAttribute(typeof(PXDBCurrencyAttribute))]
		[PXMergeAttributes]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDocAmt))]
		protected virtual void _(Events.CacheAttached<ARInvoice.curyOrigDocAmt> e) { }

		[PXRemoveBaseAttribute(typeof(PXDBCurrencyAttribute))]
		[PXMergeAttributes]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.docBal), BaseCalc = false)]
		protected virtual void _(Events.CacheAttached<ARInvoice.curyDocBal> e) { }

		[PXRemoveBaseAttribute(typeof(PXDBCurrencyAttribute))]
		[PXMergeAttributes]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDiscAmt))]
		protected virtual void _(Events.CacheAttached<ARInvoice.curyOrigDiscAmt> e) { }
		#endregion
		#endregion

		public SOReleaseInvoice()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			SOInvoiceList.SetSelected<ARInvoice.selected>();
			Caches[typeof(SOInvoice)].AdjustUI().For<SOInvoice.cCCapturedAmt>(a => a.Visible = false);
			Caches[typeof(ARInvoice)].AdjustUI().For<ARInvoice.paymentTotal>(a => a.Visible = false);
		}
		
		[PXEditDetailButton, PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (SOInvoiceList.Current != null)
			{
				var docgraph = CreateInstance<SOInvoiceEntry>();
				docgraph.Document.Current = docgraph.Document.Search<ARInvoice.refNbr>(SOInvoiceList.Current.RefNbr, SOInvoiceList.Current.DocType);
				throw new PXRedirectRequiredException(docgraph, true, "Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public virtual void _(Events.RowSelected<SOInvoiceFilter> e)
		{
			if (!string.IsNullOrEmpty(e.Row?.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(e.Row);
				SOInvoiceList.SetProcessWorkflowAction(e.Row.Action, parameters);

				bool showPrintSettings = IsPrintingAllowed(e.Row);

				e.Cache
					.AdjustUI(e.Row)
					.For<SOInvoiceFilter.printWithDeviceHub>(a =>
						a.Visible = showPrintSettings)
					.For<SOInvoiceFilter.definePrinterManually>(a =>
					{
						a.Visible = showPrintSettings;
						a.Enabled = e.Row.PrintWithDeviceHub == true;
					})
					.SameFor<SOInvoiceFilter.numberOfCopies>()
					.For<SOInvoiceFilter.printerID>(a =>
					{
						a.Visible = showPrintSettings;
						a.Enabled = e.Row.PrintWithDeviceHub == true && e.Row.DefinePrinterManually == true;
					});

				SOInvoiceList.Cache.AdjustUI()
					.For<ARInvoice.curyPaymentTotal>(attr =>
					{
						attr.Visible = e.Row.Action == WellKnownActions.SOInvoiceScreen.CreateAndCapturePayment;
					})
					.SameFor<ARInvoice.curyUnpaidBalance>();
			}
		}

		public virtual bool IsPrintingAllowed(SOInvoiceFilter filter)
		{
			return
				PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				filter?.Action == WellKnownActions.SOInvoiceScreen.PrintInvoice;
		}

		protected bool _ActionChanged = false;

		public virtual void _(Events.RowUpdated<SOInvoiceFilter> e)
		{
			_ActionChanged = !e.Cache.ObjectsEqual<SOInvoiceFilter.action>(e.Row, e.OldRow);

			if (!e.Cache.ObjectsEqual<SOInvoiceFilter.action, SOInvoiceFilter.definePrinterManually, SOInvoiceFilter.printWithDeviceHub>(e.Row, e.OldRow)
				&& PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.With(r => r.PrintWithDeviceHub == true && r.DefinePrinterManually == true))
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(ARNotificationSource.Customer, SOReports.PrintInvoiceReport, Accessinfo.BranchID);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOInvoiceFilter, SOInvoiceFilter.printerID> e)
		{
			if (e.Row != null)
			{
				if (!IsPrintingAllowed(e.Row))
					e.NewValue = null;
			}
		}

		public virtual IEnumerable<ARInvoice> GetInvoices()
		{
			if (Filter.Current.Action == PXWorkflowMassProcessingAttribute.Undefined)
				yield break;

			if (_ActionChanged)
				SOInvoiceList.Cache.Clear();

			PXSelectBase<ARInvoice> select = GetInvoiceListSelectCommand(Filter.Current);
			ApplyAdditionalFilters(select, Filter.Current);

			int startRow = PXView.StartRow;
			int totalRows = 0;

			CommandPreparing.AddHandler<ARInvoice.docType>(ARInvoiceDocTypeCommandPreparing);
			CommandPreparing.AddHandler<ARInvoice.refNbr>(ARInvoiceRefNbrCommandPreparing);

			foreach (PXResult<ARInvoice> res in select.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				ARInvoice item = res;
				item.IsCCPayment = res[typeof(PaymentMethod)] is PaymentMethod paymentMethod && paymentMethod.ARIsProcessingRequired != null;

				ARInvoice cached = (ARInvoice)SOInvoiceList.Cache.Locate(item);
				if (cached != null)
					item.Selected = cached.Selected;
				yield return item;

				PXView.StartRow = 0;
			}

			CommandPreparing.RemoveHandler<ARInvoice.refNbr>(ARInvoiceRefNbrCommandPreparing);
			CommandPreparing.RemoveHandler<ARInvoice.docType>(ARInvoiceDocTypeCommandPreparing);
		}

		protected virtual PXSelectBase<ARInvoice> GetInvoiceListSelectCommand(SOInvoiceFilter filter)
		{
			switch (filter.Action)
			{
				case WellKnownActions.SOInvoiceScreen.PostInvoiceToInventory: // the case is obsolete along with the SOInvoiceEntry.Post action
				{
					return new
						SelectFrom<ARInvoice>.
						InnerJoin<SOOrderShipment>.On<SOOrderShipment.FK.ARInvoice>.
						InnerJoin<SOOrderType>.On<SOOrderShipment.FK.OrderType>.
						Where<
							ARInvoice.released.IsEqual<True>.
							And<SOOrderType.iNDocType.IsNotEqual<INTranType.noUpdate>>.
							And<SOOrderShipment.invtRefNbr.IsNull>.
							And<SOOrderShipment.createINDoc.IsEqual<True>>>.
						AggregateTo<
							GroupBy<ARInvoice.docType>,
							GroupBy<ARInvoice.refNbr>,
							GroupBy<ARInvoice.released>>.
						View(this);
				}
				case WellKnownActions.SOInvoiceScreen.CreateAndCapturePayment:
				{
					return new
						SelectFrom<ARInvoice>.
						InnerJoin<PaymentMethod>.On<PaymentMethod.paymentMethodID.IsEqual<ARInvoice.paymentMethodID>>.
						Where<
							PaymentMethod.paymentType.IsEqual<PaymentMethodType.creditCard>.
							And<PaymentMethod.isActive.IsEqual<True>>.
							And<PaymentMethod.useForAR.IsEqual<True>>.
							And<ARInvoice.pMInstanceID.IsNotNull>.
							And<ARInvoice.curyUnpaidBalance.IsGreater<decimal0>>>.
						View(this);
				}
				case WellKnownActions.SOInvoiceScreen.EmailInvoice:
				{
					return new
						SelectFrom<ARInvoice>.
						Where<ARInvoice.emailInvoice.IsEqual<True>>.
						View(this);
				}
				default:
				{
					return new
						SelectFrom<ARInvoice>.
						View(this);
				}
			}
		}

		protected virtual void ApplyAdditionalFilters(PXSelectBase<ARInvoice> command, SOInvoiceFilter filter)
		{
			command.WhereAnd<Where<ARInvoice.origModule.IsEqual<BatchModule.moduleSO>>>();
			command.WhereAnd<Where<WhereWorkflowActionEnabled<ARInvoice, SOInvoiceFilter.action>>>();

			command.Join<InnerJoinSingleTable<Customer, On<ARInvoice.FK.Customer>>>();
			command.WhereAnd<Where<Match<Customer, AccessInfo.userName.FromCurrent>>>();

			command.WhereAnd<Where<ARInvoice.docDate.IsLessEqual<SOInvoiceFilter.endDate.FromCurrent>>>();

			if (filter.StartDate != null)
				command.WhereAnd<Where<ARInvoice.docDate.IsGreaterEqual<SOInvoiceFilter.startDate.FromCurrent>>>();

			if (filter.CustomerID != null)
				command.WhereAnd<Where<ARInvoice.customerID.IsEqual<SOInvoiceFilter.customerID.FromCurrent>>>();
		}

		//Needed to remove Order By UI values
		protected virtual void ARInvoiceDocTypeCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.External) == PXDBOperation.External && e.Value == null)
			{
				e.Expr = new Data.SQLTree.Column<ARInvoice.docType>();
				e.Value = 0;
				e.Cancel = true;
			}
		}

		protected virtual void ARInvoiceRefNbrCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.External) == PXDBOperation.External && e.Value == null)
			{
				e.Expr = new Data.SQLTree.Column<ARInvoice.refNbr>();
				e.Value = "";
				e.Cancel = true;
			}
		}

		public class WellKnownActions
		{
			public class SOInvoiceScreen
			{
				public const string ScreenID = "SO303000";

				public const string ReleaseInvoice
					= ScreenID + "$" + nameof(SOInvoiceEntry.release);

				public const string PostInvoiceToInventory
					= ScreenID + "$" + nameof(SOInvoiceEntry.post);

				public const string EmailInvoice
					= ScreenID + "$" + nameof(SOInvoiceEntry.emailInvoice);

				public const string PrintInvoice
					= ScreenID + "$" + nameof(SOInvoiceEntry.printInvoice);

				public const string CreateAndCapturePayment
					= ScreenID + "$" + nameof(CreatePaymentExt.createAndCapturePayment);

				public const string ReleaseFromCreditHold
					= ScreenID + "$" + nameof(SOInvoiceEntry.releaseFromCreditHold);
			}
		}
	}

	public partial class SOInvoiceFilter : IBqlTable, IPrintable
	{
		#region Action
		[PXWorkflowMassProcessing(DisplayName = "Action")]
		public virtual string Action { get; set; }
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
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
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion
		#region CustomerID
		[PXUIField(DisplayName = "Customer")]
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
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
		[PXFormula(typeof(
			Null.When<
				printWithDeviceHub.IsNotEqual<True>.
				Or<definePrinterManually.IsNotEqual<True>>>.
			Else<printerID>))]
		public virtual Guid? PrinterID { get; set; }
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		#endregion
		#region NumberOfCopies
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<printerID, SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies { get; set; }
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		#endregion
	}
}