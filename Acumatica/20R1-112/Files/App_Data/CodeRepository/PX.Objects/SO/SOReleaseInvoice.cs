using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.AR.MigrationMode;
using PX.Objects.CR;

namespace PX.Objects.SO
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class SOReleaseInvoice : PXGraph<SOReleaseInvoice>
	{
		public PXCancel<SOInvoiceFilter> Cancel;
		public PXAction<SOInvoiceFilter> viewDocument;
		public PXFilter<SOInvoiceFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<ARInvoice, SOInvoiceFilter> SOInvoiceList;

        #region Cache Attached
        #region ARInvoice
        [PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARDocType.SOEntryList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		protected virtual void ARInvoice_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<SOInvoice.refNbr,
					InnerJoin<AR.Standalone.ARRegisterAlias, On<AR.Standalone.ARRegisterAlias.docType, Equal<SOInvoice.docType>,
						And<AR.Standalone.ARRegisterAlias.refNbr, Equal<SOInvoice.refNbr>>>,
					InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>, 
						And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>,
					InnerJoinSingleTable<Customer, On<AR.Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>>,
					Where<SOInvoice.docType, Equal<Optional<SOInvoice.docType>>,
						And<Match<Customer, Current<AccessInfo.userName>>>>, 
					OrderBy<Desc<SOInvoice.refNbr>>>), Filterable = true)]
		[ARInvoiceType.Numbering()]
		[ARInvoiceNbr()]
		protected virtual void ARInvoice_RefNbr_CacheAttached(PXCache sender)
		{
		}
		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXDefault()]
		protected virtual void ARInvoice_CustomerID_CacheAttached(PXCache sender)
		{
		}
		[SOOpenPeriod(
			sourceType: typeof(ARRegister.docDate),
			branchSourceType: typeof(ARRegister.branchID),
			masterFinPeriodIDType: typeof(ARRegister.tranPeriodID),
			IsHeader = true)]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_FinPeriodID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[SOInvoiceTerms()]
		protected virtual void ARInvoice_TermsID_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DueDate_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DiscDate_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDocAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		protected virtual void ARInvoice_CuryDocBal_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDiscAmt_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#endregion

		public SOReleaseInvoice()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			SOInvoiceList.SetSelected<ARInvoice.selected>();
			PXUIFieldAttribute.SetVisible<SOInvoice.cCCapturedAmt>(Caches[typeof(SOInvoice)], null, false);
			PXUIFieldAttribute.SetVisible<ARInvoice.paymentTotal>(Caches[typeof(ARInvoice)], null, false);
		}

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (SOInvoiceList.Current != null)
			{
				SOInvoiceEntry docgraph = PXGraph.CreateInstance<SOInvoiceEntry>();
				docgraph.Document.Current = docgraph.Document.Search<ARInvoice.refNbr>(SOInvoiceList.Current.RefNbr, SOInvoiceList.Current.DocType);
				throw new PXRedirectRequiredException(docgraph, true, "Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public virtual void SOInvoiceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOInvoiceFilter filter = e.Row as SOInvoiceFilter;
			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(filter);
				SOInvoiceList.SetProcessTarget(null, null, null, filter.Action, parameters);

                string actionID = (string)SOInvoiceList.GetTargetFill(null, null, null, Filter.Current.Action, "@ActionName");
                PXUIFieldAttribute.SetVisible<SOInvoiceFilter.showFailedCCCapture>(sender, e.Row, actionID == "CaptureCCPayment");
				bool showPrintSettings = IsPrintingAllowed(filter);

				PXUIFieldAttribute.SetVisible<SOInvoiceFilter.printWithDeviceHub>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<SOInvoiceFilter.definePrinterManually>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<SOInvoiceFilter.printerID>(sender, filter, showPrintSettings);
				PXUIFieldAttribute.SetVisible<SOInvoiceFilter.numberOfCopies>(sender, filter, showPrintSettings);

				PXUIFieldAttribute.SetEnabled<SOInvoiceFilter.definePrinterManually>(sender, filter, filter.PrintWithDeviceHub == true);
				PXUIFieldAttribute.SetEnabled<SOInvoiceFilter.numberOfCopies>(sender, filter, filter.PrintWithDeviceHub == true);
				PXUIFieldAttribute.SetEnabled<SOInvoiceFilter.printerID>(sender, filter, filter.PrintWithDeviceHub == true && filter.DefinePrinterManually == true);

				if (filter.PrintWithDeviceHub != true || filter.DefinePrinterManually != true)
				{
					filter.PrinterID = null;
				}
			}
		}

		public virtual bool IsPrintingAllowed(SOInvoiceFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				(filter != null && !String.IsNullOrEmpty(filter.Action) && SOReports.GetReportIDByName(SOInvoiceList, filter.Action) == SOReports.PrintInvoiceReport);
		}

		protected bool _ActionChanged = false;

		public virtual void SOInvoiceFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<SOInvoiceFilter.action>(e.Row, e.OldRow);

			if ((_ActionChanged || !sender.ObjectsEqual<PrintInvoicesFilter.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOShipmentFilter.printWithDeviceHub>(e.Row, e.OldRow)) 
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true)
			{
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(ARNotificationSource.Customer, SOReports.PrintInvoiceReport, Accessinfo.BranchID);
			}
		}

		protected virtual void SOInvoiceFilter_PrinterName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOInvoiceFilter row = (SOInvoiceFilter)e.Row;
			if (row != null)
			{
				if (!IsPrintingAllowed(row))
					e.NewValue = null;
			}
		}

		public virtual IEnumerable soInvoiceList()
		{
			if (Filter.Current.Action == "<SELECT>")
			{
				yield break;
			}

			SOInvoiceFilter filter = Filter.Current;


			string actionID = (string)SOInvoiceList.GetTargetFill(null, null, null, Filter.Current.Action, "@ActionName");

			if (_ActionChanged)
			{
				SOInvoiceList.Cache.Clear();
			}

			PXSelectBase<ARInvoice> cmd;

			switch (actionID)
			{
				case "Release":
					cmd = new PXSelectJoin<ARInvoice,
							InnerJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
						Where<ARInvoice.hold, Equal<boolFalse>, And<ARInvoice.origModule, Equal<BatchModule.moduleSO>, And<ARInvoice.released, Equal<boolFalse>, And<Match<Customer, Current<AccessInfo.userName>>>>>>>(this);
					break;

				case "Post":	// the case is obsolete along with the SOInvoiceEntry.Post action
					cmd = new PXSelectJoinGroupBy<ARInvoice, 
						InnerJoin<SOOrderShipment, On<SOOrderShipment.invoiceType, Equal<ARInvoice.docType>, And<SOOrderShipment.invoiceNbr, Equal<ARInvoice.refNbr>>>,
						InnerJoin<SOOrderType, On<SOOrderShipment.FK.OrderType>,
						InnerJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>>,
						Where<ARInvoice.released, Equal<boolTrue>, And<SOOrderType.iNDocType, NotEqual<INTranType.noUpdate>,
						And<ARInvoice.origModule, Equal<BatchModule.moduleSO>,
						And<SOOrderShipment.invtRefNbr, IsNull, And<SOOrderShipment.createINDoc, Equal<boolTrue>,
						And<Match<Customer, Current<AccessInfo.userName>>>>>>>>,
						Aggregate<
							GroupBy<ARInvoice.docType,
							GroupBy<ARInvoice.refNbr,
							GroupBy<ARInvoice.released>>>>>(this);
					break;
				case "CaptureCCPayment":
					cmd = new PXSelectJoin<ARInvoice, 
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
						InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
						InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>, 
							And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>, 
							And<PaymentMethod.aRIsProcessingRequired, Equal<True>>>>,
						InnerJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>>>,
						Where<SOInvoice.isCCCaptureFailed, Equal<Current<SOInvoiceFilter.showFailedCCCapture>>,
						And<SOInvoice.paymentAmt, Greater<decimal0>,
						And<ARInvoice.docType, NotEqual<ARDocType.creditMemo>,
						And2<Where<SOInvoice.isCCCaptured, NotEqual<True>, Or<Current<SOInvoiceFilter.showFailedCCCapture>, Equal<True>>>,
						And<Match<Customer, Current<AccessInfo.userName>>>>>>>>(this);
					break;
				case "CreditHold":
					cmd = new PXSelectJoin<ARInvoice,
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
						LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
						LeftJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
							And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
							And<PaymentMethod.aRIsProcessingRequired, Equal<True>>>>,
						InnerJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>>>,
						Where<PaymentMethod.paymentMethodID, IsNull,
                        And<Match<Customer, Current<AccessInfo.userName>>>>>(this); 
					break;
				case "EmailInvoice":
						cmd = new PXSelectJoin<ARInvoice,
						InnerJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
						Where<ARInvoice.dontEmail, Equal<boolFalse>, And<ARInvoice.emailed, Equal<boolFalse>, And<ARInvoice.creditHold, Equal<boolFalse>,
						And<ARInvoice.origModule, Equal<BatchModule.moduleSO>,
						And<Match<Customer, Current<AccessInfo.userName>>>>>>>>(this);
					break;
				default:
					cmd = new PXSelectJoin<ARInvoice,
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
						InnerJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>,
                        Where<Match<Customer, Current<AccessInfo.userName>>>>(this);
					break;
			}

			cmd.WhereAnd<Where<ARInvoice.docDate, LessEqual<Current<SOInvoiceFilter.endDate>>>>();

			if (filter.StartDate != null)
			{
				cmd.WhereAnd<Where<ARInvoice.docDate, GreaterEqual<Current<SOInvoiceFilter.startDate>>>>();
			}

			if (filter.CustomerID != null)
			{
				cmd.WhereAnd<Where<ARInvoice.customerID, Equal<Current<SOInvoiceFilter.customerID>>>>();
			}

            int startRow = PXView.StartRow;
            int totalRows = 0;

			CommandPreparing.AddHandler<ARInvoice.docType>(ARInvoiceDocTypeCommandPreparing);
			CommandPreparing.AddHandler<ARInvoice.refNbr>(ARInvoiceRefNbrCommandPreparing);

			foreach (PXResult<ARInvoice> res in cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				ARInvoice item = res;
				
				object paymentmethod;
				if ((paymentmethod = res[typeof(PaymentMethod)]) != null)
				{
					item.IsCCPayment = ((PaymentMethod)paymentmethod).ARIsProcessingRequired != null;
				}
				else
				{
					item.IsCCPayment = false;
				}

				ARInvoice cached = (ARInvoice)SOInvoiceList.Cache.Locate(item);
				if (cached != null)
					item.Selected = cached.Selected;
				yield return item;

                PXView.StartRow = 0;
			}

			CommandPreparing.RemoveHandler<ARInvoice.docType>(ARInvoiceDocTypeCommandPreparing);
			CommandPreparing.RemoveHandler<ARInvoice.refNbr>(ARInvoiceRefNbrCommandPreparing);
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
	}

    [Serializable]
	public partial class SOInvoiceFilter : IBqlTable, PX.SM.IPrintable
	{
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
		protected string _Action;
		[PXAutomationMenu]
		public virtual string Action
		{
			get
			{
				return this._Action;
			}
			set
			{
				this._Action = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[PXDefault()]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected int? _CustomerID;
		[PXUIField(DisplayName = "Customer")]
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID
		{
			get
			{
				return _CustomerID;
			}
			set
			{
				_CustomerID = value;
			}
		}
		#endregion
        #region ShowFailedCCCapture
        public abstract class showFailedCCCapture : PX.Data.BQL.BqlBool.Field<showFailedCCCapture> { }
        [PXUIField(DisplayName = "Show Failed CC Capture")]
        [PXDBBool()]
        [PXDefault(false)]
        public bool? ShowFailedCCCapture
        {
            get;
            set;
        }
		#endregion

		#region PrintWithDeviceHub
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		protected bool? _PrintWithDeviceHub;
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub")]
		public virtual bool? PrintWithDeviceHub
		{
			get
			{
				return _PrintWithDeviceHub;
			}
			set
			{
				_PrintWithDeviceHub = value;
			}
		}
		#endregion
		#region DefinePrinterManually
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		protected bool? _DefinePrinterManually = false;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Define Printer Manually")]
		public virtual bool? DefinePrinterManually
		{
			get
			{
				return _DefinePrinterManually;
			}
			set
			{
				_DefinePrinterManually = value;
			}
		}
		#endregion
		#region PrinterID
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		protected Guid? _PrinterID;
		[PX.SM.PXPrinterSelector]
		public virtual Guid? PrinterID
		{
			get
			{
				return this._PrinterID;
			}
			set
			{
				this._PrinterID = value;
			}
		}
		#endregion
		#region NumberOfCopies
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		protected int? _NumberOfCopies;
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<SOInvoiceFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies
		{
			get
			{
				return this._NumberOfCopies;
			}
			set
			{
				this._NumberOfCopies = value;
			}
		}
		#endregion
	}
}
