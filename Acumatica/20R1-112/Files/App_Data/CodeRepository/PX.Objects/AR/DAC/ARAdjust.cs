using System;
using System.Diagnostics;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.Common;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.Common.MigrationMode;
using PX.Objects.GL.FinPeriods;
using PX.Objects.TX;

namespace PX.Objects.AR
{
	/// <summary>
	/// Records the fact of application of one Accounts Receivable document to another,
	/// which results in an adjustment of the balances of both documents. It can be either 
	/// an application of a <see cref="ARPayment">payment document</see> to an
	/// <see cref="ARInvoice">invoice document</see> (for example, when a payment
	/// closes an invoice), or an application of one <see cref="ARPayment">payment</see> document 
	/// to another, such as an application of a customer refund to a payment. The entities
	/// of this type are mainly edited on the Payments and Applications (AR302000) form,
	/// which corresponds to the <see cref="ARPaymentEntry"/> graph. They can also be edited
	/// on the Applications tab of the Invoices and Memos (AR301000) form, which corresponds
	/// to the <see cref="ARInvoiceEntry"/> graph.
	/// </summary>
	[PXPrimaryGraph(
		new Type[] { typeof(ARPaymentEntry) },
		new Type[] { typeof(Select<ARPayment, 
			Where<ARPayment.docType, Equal<Current<ARAdjust.adjgDocType>>,
			And<ARPayment.refNbr, Equal<Current<ARAdjust.adjgRefNbr>>>>>)
		})]
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARAdjust)]       
	[DebuggerDisplay("{AdjdDocType}:{AdjdRefNbr}:{AdjdLineNbr} - {AdjgDocType}:{AdjgRefNbr}:{AdjNbr}")]

	public partial class ARAdjust 
		: IBqlTable, IAdjustment, IDocumentAdjustment, IAdjustmentAmount
	{
		#region Selected
		public abstract class selected : IBqlField { }

		/// <summary>
		/// Indicates whether the record is selected for processing.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Enabled = false)]
		public virtual bool? Selected { get; set; }
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDBInt]
		[PXDBDefault(typeof(ARRegister.customerID))]
		[PXUIField(DisplayName = "CustomerID", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region AdjdCustomerID
		public abstract class adjdCustomerID : PX.Data.BQL.BqlInt.Field<adjdCustomerID> { }

		protected Int32? _AdjdCustomerID;
		[PXDefault]
		[Customer(Visible = false, Enabled = false)]
		public virtual Int32? AdjdCustomerID
		{
			get
			{
				return this._AdjdCustomerID;
			}
			set
			{
				this._AdjdCustomerID = value;
			}
		}
		#endregion
		#region AdjType
		public abstract class adjType : PX.Data.BQL.BqlString.Field<adjType>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new [] { Adjusted, Adjusting },
					new [] { Messages.Adjusted, Messages.Adjusting })
				{ }
			}

			/// <summary>
			/// Outgoing application of the current 
			/// document to another document.
			/// </summary>
			public const string Adjusted = "D";
			/// <summary>
			/// Incoming application of another 
			/// document to the current document.
			/// </summary>
			public const string Adjusting = "G";

			public class adjusted : PX.Data.BQL.BqlString.Constant<adjusted>
			{
				public adjusted() : base(Adjusted) { }
			}

			public class adjusting : PX.Data.BQL.BqlString.Constant<adjusting>
			{
				public adjusting() : base(Adjusting) { }
			}
		}

		/// <summary>
		/// Adjustment type - an incoming or outgoing
		/// adjustment. Controlled by specific BLCs, e.g.
		/// filled out in application delegate in <see cref="ARInvoiceEntry"/>.
		/// </summary>
		[PXString(1, IsFixed = true)]
		[ARAdjust.adjType.List]
		[PXDefault(ARAdjust.adjType.Adjusted, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AdjType { get; set; }
		#endregion
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		protected String _AdjgDocType;
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDBDefault(typeof(ARRegister.docType))]
		[PXUIField(DisplayName = "AdjgDocType", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String AdjgDocType
		{
			get
			{
				return this._AdjgDocType;
			}
			set
			{
				this._AdjgDocType = value;
			}
		}
		#endregion
		#region PrintAdjgDocType
		public abstract class printAdjgDocType : PX.Data.BQL.BqlString.Field<printAdjgDocType> { }
		[PXString(3, IsFixed = true)]
		[ARDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintAdjgDocType
		{
			get
			{
				return this._AdjgDocType;
			}
			set
			{
			}
		}
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		protected String _AdjgRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARRegister.refNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<ARRegister, 
			Where<ARRegister.docType, Equal<Current<ARAdjust.adjgDocType>>, 
				And<ARRegister.refNbr, Equal<Current<ARAdjust.adjgRefNbr>>, 
				And<Current<ARAdjust.released>, NotEqual<True>>>>>))]
		public virtual String AdjgRefNbr
		{
			get
			{
				return this._AdjgRefNbr;
			}
			set
			{
				this._AdjgRefNbr = value;
			}
		}
		#endregion
		#region AdjgBranchID
		public abstract class adjgBranchID : PX.Data.BQL.BqlInt.Field<adjgBranchID> { }
		protected Int32? _AdjgBranchID;
		[Branch(typeof(ARRegister.branchID))]
		public virtual Int32? AdjgBranchID
		{
			get
			{
				return this._AdjgBranchID;
			}
			set
			{
				this._AdjgBranchID = value;
			}
		}
		#endregion
		#region AdjdCuryInfoID
		public abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }
		protected Int64? _AdjdCuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AR, CuryIDField = "AdjdCuryID", Enabled = false)]
		public virtual Int64? AdjdCuryInfoID
		{
			get
			{
				return this._AdjdCuryInfoID;
			}
			set
			{
				this._AdjdCuryInfoID = value;
			}
		}
		#endregion
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask="")]
		[PXDefault(ARDocType.Invoice)]
		[PXUIField(DisplayName = Messages.DocType, Visibility = PXUIVisibility.Visible)]
		[ARInvoiceType.AdjdList]
		public virtual string AdjdDocType
			{
			get;
			set;
		}
		#endregion
		#region PrintAdjdDocType
		public abstract class printAdjdDocType : PX.Data.BQL.BqlString.Field<printAdjdDocType> { }
		[PXString(3, IsFixed = true)]
		[ARDocType.PrintList]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		public virtual string PrintAdjdDocType
		{
			get
			{
				return this.AdjdDocType;
			}
			set { }
		}
		#endregion
		#region HistoryAdjdDocType
		public abstract class historyAdjdDocType : PX.Data.BQL.BqlString.Field<historyAdjdDocType> { }
		[PXString(3, IsFixed = true)]
		[ARDocType.List]
		[PXUIField(DisplayName = Messages.Type, Enabled = false)]
		public virtual string HistoryAdjdDocType
		{
			get
			{
				return this.AdjdDocType;
			}
			set { }
		}
		#endregion

		#region DisplayDocType
		public abstract class displayDocType : PX.Data.BQL.BqlString.Field<displayDocType> { }
		[PXString(3, IsFixed = true, InputMask = "")]
		[PXUIField(DisplayName = Messages.DocType)]
		[ARDocType.List]
		public virtual string DisplayDocType { get; set; }
		#endregion

		#region AdjdRefNbr
		[PXHidden()]
		[PXProjection(typeof(Select2<Standalone.ARRegister, 
			LeftJoin<Standalone.ARInvoice, On<Standalone.ARInvoice.docType, Equal<Standalone.ARRegister.docType>, 
				And<Standalone.ARInvoice.refNbr, Equal<Standalone.ARRegister.refNbr>>>>>))]
		[Serializable]
		public partial class ARInvoice : Standalone.ARRegister
		{
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region CustomerID
			public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			#endregion
			#region Released
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			#endregion
			#region PendingPPD
			public new abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }
			#endregion
			#region HasPPDTaxes
			public new abstract class hasPPDTaxes : PX.Data.BQL.BqlBool.Field<hasPPDTaxes> { }
			#endregion
			#region OpenDoc
			public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
			#endregion
			#region DocDate
			public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			#endregion
			#region FinPeriodID
			public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			#endregion
			#region DueDate
			public new abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			#endregion
			#region InvoiceNbr
			public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
			[PXDBString(40, IsUnicode = true, BqlField = typeof(Standalone.ARInvoice.invoiceNbr))]
			[PXUIField(DisplayName = "Customer Order Nbr.")]
			public virtual string InvoiceNbr
			{
				get;
				set;
			}
			#endregion
			#region IsMigratedRecord
			public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
			#endregion
			#region PaymentsByLinesAllowed
			public new abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }
			#endregion
		}
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[ARInvoiceType.AdjdRefNbr(typeof(Search2<ARInvoice.refNbr,
			LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
				And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
				And<ARAdjust.released, NotEqual<True>,
				And<ARAdjust.voided, NotEqual<True>,
				And<Where<ARAdjust.adjgDocType, NotEqual<Current<ARRegister.docType>>,
					Or<ARAdjust.adjgRefNbr, NotEqual<Current<ARRegister.refNbr>>>>>>>>>,
			LeftJoin<ARAdjust2, On<ARAdjust2.adjgDocType, Equal<ARInvoice.docType>,
				And<ARAdjust2.adjgRefNbr, Equal<ARInvoice.refNbr>,
				And<ARAdjust2.released, NotEqual<True>,
				And<ARAdjust2.voided, NotEqual<True>>>>>,
			LeftJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>>, 
			Where<ARInvoice.docType, Equal<Optional<ARAdjust.adjdDocType>>,
				And<ARInvoice.released, Equal<True>, 
				And<ARInvoice.openDoc, Equal<True>, 
				And<ARAdjust.adjgRefNbr, IsNull, 
				And<ARAdjust2.adjdRefNbr, IsNull,
				And<ARInvoice.customerID, In2<Search<Override.BAccount.bAccountID,
					Where<Override.BAccount.bAccountID, Equal<Optional<ARRegister.customerID>>, 
						Or<Override.BAccount.consolidatingBAccountID, Equal<Optional<ARRegister.customerID>>>>>>,
				And2<Where<ARInvoice.pendingPPD, NotEqual<True>,
					Or<Current<ARRegister.pendingPPD>, Equal<True>>>,
				And<Where<
					Current<ARSetup.migrationMode>, NotEqual<True>,
					Or<ARInvoice.isMigratedRecord, Equal<Current<ARRegister.isMigratedRecord>>>>>>>>>>>>>), Filterable = true)]
		public virtual string AdjdRefNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdLineNbr
		public abstract class adjdLineNbr : PX.Data.BQL.BqlInt.Field<adjdLineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXDefault(typeof(Switch<Case<Where<Selector<ARAdjust.adjdRefNbr, ARInvoice.paymentsByLinesAllowed>, NotEqual<True>>, int0>, Null>))]
		[ARInvoiceType.AdjdLineNbr]
		public virtual int? AdjdLineNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdOrderType
		public abstract class adjdOrderType : PX.Data.BQL.BqlString.Field<adjdOrderType> { }
		protected String _AdjdOrderType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type")]
		[PXSelector(typeof(Search<SOOrderType.orderType>))]
		public virtual String AdjdOrderType
		{
			get
			{
				return this._AdjdOrderType;
			}
			set
			{
				this._AdjdOrderType = value;
			}
		}
		#endregion
		#region AdjdOrderNbr
		public abstract class adjdOrderNbr : PX.Data.BQL.BqlString.Field<adjdOrderNbr> { }
		protected String _AdjdOrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr,
			Where<SOOrder.orderType, Equal<Optional<ARAdjust.adjdOrderType>>>>), Filterable = true)]
		[PXParent(typeof(Select<SOAdjust, Where<SOAdjust.adjdOrderType, Equal<Current<ARAdjust.adjdOrderType>>, And<SOAdjust.adjdOrderNbr, Equal<Current<ARAdjust.adjdOrderNbr>>,
			And<SOAdjust.adjgDocType, Equal<Current<ARAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Current<ARAdjust.adjgRefNbr>>>>>>>), LeaveChildren = true)]
		public virtual String AdjdOrderNbr
		{
			get
			{
				return this._AdjdOrderNbr;
			}
			set
			{
				this._AdjdOrderNbr = value;
			}
		}
		#endregion
		#region AdjdBranchID
		public abstract class adjdBranchID : PX.Data.BQL.BqlInt.Field<adjdBranchID> { }
		protected Int32? _AdjdBranchID;
		[Branch(useDefaulting: false, Enabled = false)]
		public virtual Int32? AdjdBranchID
		{
			get
			{
				return this._AdjdBranchID;
			}
			set
			{
				this._AdjdBranchID = value;
			}
		}
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		protected Int32? _AdjNbr;
		[PXDBInt(IsKey = true)]
	[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
	[PXDefault(typeof(ARPayment.adjCntr))]
		public virtual Int32? AdjNbr
		{
			get
			{
				return this._AdjNbr;
			}
			set
			{
				this._AdjNbr = value;
			}
		}
		#endregion
		#region AdjBatchNbr
		public abstract class adjBatchNbr : PX.Data.BQL.BqlString.Field<adjBatchNbr> { }
		protected String _AdjBatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual String AdjBatchNbr
		{
			get
			{
				return this._AdjBatchNbr;
			}
			set
			{
				this._AdjBatchNbr = value;
			}
		}
		#endregion
		#region VoidAdjNbr
		public abstract class voidAdjNbr : PX.Data.BQL.BqlInt.Field<voidAdjNbr> { }
		protected Int32? _VoidAdjNbr;
		[PXDBInt()]
		public virtual Int32? VoidAdjNbr
		{
			get
			{
				return this._VoidAdjNbr;
			}
			set
			{
				this._VoidAdjNbr = value;
			}
		}
		#endregion
		#region AdjdOrigCuryInfoID
		public abstract class adjdOrigCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdOrigCuryInfoID> { }
		protected Int64? _AdjdOrigCuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AR, CuryIDField = "AdjdOrigCuryID")]
		public virtual Int64? AdjdOrigCuryInfoID
		{
			get
			{
				return this._AdjdOrigCuryInfoID;
			}
			set
			{
				this._AdjdOrigCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgCuryInfoID
		public abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID> { }
		protected Int64? _AdjgCuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(ARRegister.curyInfoID), CuryIDField = "AdjgCuryID")]
		public virtual Int64? AdjgCuryInfoID
		{
			get
			{
				return this._AdjgCuryInfoID;
			}
			set
			{
				this._AdjgCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgDocDate
		public abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate> { }
		protected DateTime? _AdjgDocDate;
		[PXDBDate()]
		[PXDBDefault(typeof(ARPayment.adjDate))]
		public virtual DateTime? AdjgDocDate
		{
			get
			{
				return this._AdjgDocDate;
			}
			set
			{
				this._AdjgDocDate = value;
			}
		}
		#endregion
		#region AdjgFinPeriodID
		public abstract class adjgFinPeriodID : PX.Data.BQL.BqlString.Field<adjgFinPeriodID> { }
		protected String _AdjgFinPeriodID;
	    [FinPeriodID(
	        branchSourceType: typeof(ARAdjust.adjgBranchID),
	        masterFinPeriodIDType: typeof(ARAdjust.adjgTranPeriodID),
	        headerMasterFinPeriodIDType: typeof(ARPayment.adjTranPeriodID))]
        [PXUIField(DisplayName = "Application Period", Enabled = false)]
		public virtual String AdjgFinPeriodID
		{
			get
			{
				return this._AdjgFinPeriodID;
			}
			set
			{
				this._AdjgFinPeriodID = value;
			}
		}
		#endregion
		#region AdjgTranPeriodID
		public abstract class adjgTranPeriodID : PX.Data.BQL.BqlString.Field<adjgTranPeriodID> { }
		protected String _AdjgTranPeriodID;
		[PeriodID]
		public virtual String AdjgTranPeriodID
		{
			get
			{
				return this._AdjgTranPeriodID;
			}
			set
			{
				this._AdjgTranPeriodID = value;
			}
		}
		#endregion
		#region AdjdDocDate
		public abstract class adjdDocDate : PX.Data.BQL.BqlDateTime.Field<adjdDocDate> { }
		protected DateTime? _AdjdDocDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? AdjdDocDate
		{
			get
			{
				return this._AdjdDocDate;
			}
			set
			{
				this._AdjdDocDate = value;
			}
		}
		#endregion
		#region AdjdFinPeriodID
		public abstract class adjdFinPeriodID : PX.Data.BQL.BqlString.Field<adjdFinPeriodID> { }
		protected String _AdjdFinPeriodID;
	    [FinPeriodID(
	        branchSourceType: typeof(ARAdjust.adjdBranchID),
	        masterFinPeriodIDType: typeof(ARAdjust.adjdTranPeriodID))]
        [PXUIField(DisplayName = "Post Period", Enabled = false)]
		public virtual String AdjdFinPeriodID
		{
			get
			{
				return this._AdjdFinPeriodID;
			}
			set
			{
				this._AdjdFinPeriodID = value;
			}
		}
		#endregion
		#region AdjdTranPeriodID
		public abstract class adjdTranPeriodID : PX.Data.BQL.BqlString.Field<adjdTranPeriodID> { }
		protected String _AdjdTranPeriodID;
	    [PeriodID]
        public virtual String AdjdTranPeriodID
		{
			get
			{
				return this._AdjdTranPeriodID;
			}
			set
			{
				this._AdjdTranPeriodID = value;
			}
		}
		#endregion
		#region CuryAdjgDiscAmt
		public abstract class curyAdjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgDiscAmt> { }
		protected Decimal? _CuryAdjgDiscAmt;
		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cash Discount Taken", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryAdjgDiscAmt
		{
			get
			{
				return this._CuryAdjgDiscAmt;
			}
			set
			{
				this._CuryAdjgDiscAmt = value;
			}
		}
		#endregion
		#region CuryAdjgPPDAmt
		public abstract class curyAdjgPPDAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgPPDAmt> { }
		protected decimal? _CuryAdjgPPDAmt;

		/// <summary>
		/// The cash discount amount displayed for the document.
		/// Given in the <see cref="CuryID"> currency of the adjusting document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjPPDAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cash Discount Taken", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryAdjgPPDAmt
		{
			get
			{
				return _CuryAdjgPPDAmt;
			}
			set
			{
				_CuryAdjgPPDAmt = value;
			}
		}
		#endregion
		#region CuryAdjgWOAmt
		public abstract class curyAdjgWOAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgWOAmt> { }
		protected Decimal? _CuryAdjgWOAmt;
		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjWOAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.BalanceWriteOff, Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(SumCalc<ARPayment.curyWOAmt>))]
		public virtual Decimal? CuryAdjgWOAmt
		{
			get
			{
				return this._CuryAdjgWOAmt;
			}
			set
			{
				this._CuryAdjgWOAmt = value;
			}
		}
		public virtual Decimal? CuryAdjgWhTaxAmt
		{
			get
			{
				return this._CuryAdjgWOAmt;
			}
			set
			{
				this._CuryAdjgWOAmt = value;
			}
		}
		#endregion
		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
		protected Decimal? _CuryAdjgAmt;
		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.AmountPaid, Visibility = PXUIVisibility.Visible)]
		[PXUnboundFormula(typeof(Mult<ARAdjust.adjgBalSign, ARAdjust.curyAdjgAmt>), typeof(SumCalc<ARPayment.curyApplAmt>))]
		[PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjgBilledAmt>))]
		public virtual Decimal? CuryAdjgAmt
		{
			get
			{
				return this._CuryAdjgAmt;
			}
			set
			{
				this._CuryAdjgAmt = value;
			}
		}
		#endregion
		#region AdjDiscAmt
		public abstract class adjDiscAmt : PX.Data.BQL.BqlDecimal.Field<adjDiscAmt> { }
		protected Decimal? _AdjDiscAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjDiscAmt
		{
			get
			{
				return this._AdjDiscAmt;
			}
			set
			{
				this._AdjDiscAmt = value;
			}
		}
		#endregion
		#region AdjPPDAmt
		public abstract class adjPPDAmt : PX.Data.BQL.BqlDecimal.Field<adjPPDAmt> { }
		protected decimal? _AdjPPDAmt;

		/// <summary>
		/// The cash discount amount displayed for the document.
		/// Given in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AdjPPDAmt
		{
			get
			{
				return _AdjPPDAmt;
			}
			set
			{
				_AdjPPDAmt = value;
			}
		}
		#endregion
		#region CuryAdjdDiscAmt
		public abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdDiscAmt> { }
		protected Decimal? _CuryAdjdDiscAmt;
		[PXDBDecimal(4)]
		//[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.CashDiscountTaken, Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryAdjdDiscAmt
		{
			get
			{
				return this._CuryAdjdDiscAmt;
			}
			set
			{
				this._CuryAdjdDiscAmt = value;
			}
		}
		#endregion
		#region CuryAdjdPPDAmt
		public abstract class curyAdjdPPDAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdPPDAmt> { }
		protected decimal? _CuryAdjdPPDAmt;

		/// <summary>
		/// The cash discount amount displayed for the document.
		/// Given in the <see cref="CuryID"> currency of the adjusted document</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.CashDiscountTaken, Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryAdjdPPDAmt
		{
			get
			{
				return _CuryAdjdPPDAmt;
			}
			set
			{
				_CuryAdjdPPDAmt = value;
			}
		}
		#endregion
		#region AdjWOAmt
		public abstract class adjWOAmt : PX.Data.BQL.BqlDecimal.Field<adjWOAmt> { }
		protected Decimal? _AdjWOAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjWOAmt
		{
			get
			{
				return this._AdjWOAmt;
			}
			set
			{
				this._AdjWOAmt = value;
			}
		}
		public virtual Decimal? AdjWhTaxAmt
		{
			get
			{
				return this._AdjWOAmt;
			}
			set
			{
				this._AdjWOAmt = value;
			}
		}
		#endregion
		#region CuryAdjdWOAmt
		public abstract class curyAdjdWOAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdWOAmt> { }
		protected Decimal? _CuryAdjdWOAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdWOAmt
		{
			get
			{
				return this._CuryAdjdWOAmt;
			}
			set
			{
				this._CuryAdjdWOAmt = value;
			}
		}
		public virtual Decimal? CuryAdjdWhTaxAmt
		{
			get
			{
				return this._CuryAdjdWOAmt;
			}
			set
			{
				this._CuryAdjdWOAmt = value;
			}
		}
		#endregion
		#region AdjAmt
		public abstract class adjAmt : PX.Data.BQL.BqlDecimal.Field<adjAmt> { }
		protected Decimal? _AdjAmt;
		[PXDBDecimal(4)]
		[PXFormula(null, typeof(SumCalc<SOAdjust.adjBilledAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjAmt
		{
			get
			{
				return this._AdjAmt;
			}
			set
			{
				this._AdjAmt = value;
			}
		}
		#endregion
		#region CuryAdjdAmt
		public abstract class curyAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdAmt> { }
		protected Decimal? _CuryAdjdAmt;
		[PXDBDecimal(4)]
		//[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjdBilledAmt>))]
		public virtual Decimal? CuryAdjdAmt
		{
			get
			{
				return this._CuryAdjdAmt;
			}
			set
			{
				this._CuryAdjdAmt = value;
			}
		}
        #endregion
        #region CuryAdjdOrigAmt
        public abstract class curyAdjdOrigAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdOrigAmt> { }
        protected Decimal? _CuryAdjdOrigAmt;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryAdjdOrigAmt
        {
            get
            {
                return this._CuryAdjdOrigAmt;
            }
            set
            {
                this._CuryAdjdOrigAmt = value;
            }
        }
        #endregion
        #region RGOLAmt
		public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }
		protected Decimal? _RGOLAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? RGOLAmt
		{
			get
			{
				return this._RGOLAmt;
			}
			set
			{
				this._RGOLAmt = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		protected Boolean? _Voided;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region AdjdARAcct
		public abstract class adjdARAcct : PX.Data.BQL.BqlInt.Field<adjdARAcct> { }
		protected Int32? _AdjdARAcct;
		[Account()]
		[PXDefault()]
		public virtual Int32? AdjdARAcct
		{
			get
			{
				return this._AdjdARAcct;
			}
			set
			{
				this._AdjdARAcct = value;
			}
		}
		#endregion
		#region AdjdARSub
		public abstract class adjdARSub : PX.Data.BQL.BqlInt.Field<adjdARSub> { }
		protected Int32? _AdjdARSub;
		[SubAccount()]
		[PXDefault()]
		public virtual Int32? AdjdARSub
		{
			get
			{
				return this._AdjdARSub;
			}
			set
			{
				this._AdjdARSub = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region AdjdCuryRate
		public abstract class adjdCuryRate : PX.Data.BQL.BqlDecimal.Field<adjdCuryRate> { }
		protected Decimal? _AdjdCuryRate;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDecimal(8)]
		[PXUIField(DisplayName = "Cross Rate", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? AdjdCuryRate
		{
			get
			{
				return this._AdjdCuryRate;
			}
			set
			{
				this._AdjdCuryRate = value;
			}
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		protected Decimal? _CuryDocBal;
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? CuryDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}
		public virtual Decimal? CuryPayDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}
		#endregion
		#region DocBal
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		protected Decimal? _DocBal;
		[PXDecimal(4)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		public virtual Decimal? PayDocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		#endregion
		#region CuryDiscBal
		public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		protected Decimal? _CuryDiscBal;
		[PXCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.discBal), BaseCalc = false)]
		[PXUnboundDefault()]
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? CuryDiscBal
		{
			get
			{
				return this._CuryDiscBal;
			}
			set
			{
				this._CuryDiscBal = value;
			}
		}
		public virtual Decimal? CuryPayDiscBal
		{
			get
			{
				return this._CuryDiscBal;
			}
			set
			{
				this._CuryDiscBal = value;
			}
		}
		#endregion
		#region DiscBal
		public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		protected Decimal? _DiscBal;
		[PXDecimal(4)]
		[PXUnboundDefault()]
		public virtual Decimal? DiscBal
		{
			get
			{
				return this._DiscBal;
			}
			set
			{
				this._DiscBal = value;
			}
		}
		public virtual Decimal? PayDiscBal
		{
			get
			{
				return this._DiscBal;
			}
			set
			{
				this._DiscBal = value;
			}
		}
		#endregion
		#region CuryWOBal
		public abstract class curyWOBal : PX.Data.BQL.BqlDecimal.Field<curyWOBal> { }
		protected Decimal? _CuryWOBal;
		[PXCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.wOBal), BaseCalc = false)]
		[PXUnboundDefault()]
		[PXUIField(DisplayName = "Write-Off Limit", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? CuryWOBal
		{
			get
			{
				return this._CuryWOBal;
			}
			set
			{
				this._CuryWOBal = value;
			}
		}
		public virtual Decimal? CuryWhTaxBal
		{
			get
			{
				return this._CuryWOBal;
			}
			set
			{
				this._CuryWOBal = value;
			}
		}

		#endregion
		#region WOBal
		public abstract class wOBal : PX.Data.BQL.BqlDecimal.Field<wOBal> { }
		protected Decimal? _WOBal;
		[PXDecimal(4)]
		[PXUnboundDefault()]
		public virtual Decimal? WOBal
		{
			get
			{
				return this._WOBal;
			}
			set
			{
				this._WOBal = value;
			}
		}
		public virtual Decimal? WhTaxBal
		{
			get
			{
				return this._WOBal;
			}
			set
			{
				this._WOBal = value;
			}
		}
		#endregion
		#region WriteOffReasonCode
		public abstract class writeOffReasonCode : PX.Data.BQL.BqlString.Field<writeOffReasonCode> { }
		protected String _WriteOffReasonCode;
		[PXFormula(typeof(Switch<Case<Where<ARAdjust.adjdDocType, NotEqual<ARDocType.creditMemo>>, Current<ARSetup.balanceWriteOff>>>))]
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.creditWriteOff>, 
			Or<ReasonCode.usage, Equal<ReasonCodeUsages.balanceWriteOff>>>>))]
		[PXUIField(DisplayName = "Write-Off Reason Code", Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<ARAdjust.writeOffReasonCode>.IsRelatedTo<ReasonCode.reasonCodeID>))]
		public virtual String WriteOffReasonCode
		{
			get
			{
				return this._WriteOffReasonCode;
			}
			set
			{
				this._WriteOffReasonCode = value;
			}
		}
		#endregion

		#region VoidAppl
		public abstract class voidAppl : PX.Data.BQL.BqlBool.Field<voidAppl> { }
		[PXBool()]
		[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(adjgDocType))]
			get
			{
				return ARPaymentType.VoidAppl(AdjgDocType);
			}
			set
			{
				if ((bool)value && !ARPaymentType.VoidAppl(AdjgDocType))
				{
					AdjgDocType = ARPaymentType.GetVoidingARDocType(AdjgDocType);
					this.Voided = true;
				}
			}
		}
		#endregion
		#region ReverseGainLoss
		public abstract class reverseGainLoss : PX.Data.BQL.BqlBool.Field<reverseGainLoss> { }
		[PXBool()]
		[PXDependsOnFields(typeof(adjgDocType))]
		public virtual Boolean? ReverseGainLoss
		{
			get
			{
				return AdjgTBSign == -1m;
			}
			set
			{
			}
		}
		#endregion
		#region AdjgBalSign
		public abstract class adjgBalSign : PX.Data.BQL.BqlDecimal.Field<adjgBalSign> { }

		[PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
		public virtual decimal? AdjgBalSign
		{
			get
			{
				return AdjgDocType == ARDocType.Payment && AdjdDocType == ARDocType.CreditMemo 
					||	AdjgDocType == ARDocType.VoidPayment && AdjdDocType == ARDocType.CreditMemo 
					||	AdjgDocType == ARDocType.Prepayment && AdjdDocType == ARDocType.CreditMemo 
					? -1m 
					: 1m;
			}
			set { }
		}
		#endregion
		#region AdjgGLSign
		public abstract class adjgGLSign : PX.Data.BQL.BqlDecimal.Field<adjgGLSign> { }
		[PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
		public virtual decimal? AdjgGLSign
		{
			get
			{
				return ARDocType.SignAmount(this.AdjdDocType);
			}
			set { }
		}
		#endregion
		#region AdjgTBSign
		public abstract class adjgTBSign : PX.Data.BQL.BqlDecimal.Field<adjgTBSign> { }

		[PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
		public virtual decimal? AdjgTBSign
		{
			get
			{
				return this.IsSelfAdjustment()
					? -1m * ARDocType.SignBalance(AdjgDocType)
					: ARDocType.SignBalance(AdjdDocType);
			}
		}
		#endregion
		#region AdjdTBSign
		public abstract class adjdTBSign : PX.Data.BQL.BqlDecimal.Field<adjdTBSign> { }

		[PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
		public virtual decimal? AdjdTBSign
		{
			get
			{
				return this.IsSelfAdjustment()
					? 0m
					: AdjgDocType == ARDocType.Payment && AdjdDocType == ARDocType.CreditMemo || 
						AdjgDocType == ARDocType.VoidPayment && AdjdDocType == ARDocType.CreditMemo ||
						AdjgDocType == ARDocType.Prepayment && AdjdDocType == ARDocType.CreditMemo
						? 1m 
						: ARDocType.SignBalance(AdjgDocType);
			}
		}
		#endregion
		#region CuryWhTaxBal
		public virtual Decimal? CuryPayWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
				;
			}
		}
		#endregion
		#region WhTaxBal
		public virtual Decimal? PayWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
				;
			}
		}
		#endregion
		#region SignedRGOLAmt
		public virtual Decimal? SignedRGOLAmt
		{
			[PXDependsOnFields(typeof(reverseGainLoss),typeof(rGOLAmt))]
			get
			{
				return ((bool) this.ReverseGainLoss ? -this._RGOLAmt : this._RGOLAmt);
			}
		}
		#endregion

		#region AdjdBalSign
		public abstract class adjdBalSign : PX.Data.BQL.BqlDecimal.Field<adjdBalSign> { }

		[PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
		public virtual decimal? AdjdBalSign
		{
			get
			{
				return AdjgDocType == ARDocType.Payment && AdjdDocType == ARDocType.CreditMemo || 
					AdjgDocType == ARDocType.VoidPayment && AdjdDocType == ARDocType.CreditMemo ||
					AdjgDocType == ARDocType.Prepayment && AdjdDocType == ARDocType.CreditMemo
					? -1m 
					: 1m;
			}
			set { }
		}
		#endregion

		#region PPDCrMemoRefNbr
		public abstract class pPDCrMemoRefNbr : PX.Data.BQL.BqlString.Field<pPDCrMemoRefNbr> { }
		protected string _PPDCrMemoRefNbr;

		/// <summary>
		/// The reference number of the credit memo, generated on the "Generate VAT Credit Memos" (AR.50.45.00) form.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARInvoice.RefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<AR.ARInvoice.refNbr, Where<AR.ARInvoice.docType, Equal<ARDocType.creditMemo>, 
			And<AR.ARInvoice.pendingPPD, Equal<True>>>>))]
		[PXUIField(DisplayName = "VAT Credit Memo", Enabled = false, Visible = false)]
		public virtual string PPDCrMemoRefNbr
		{
			get
			{
				return _PPDCrMemoRefNbr;
			}
			set
			{
				_PPDCrMemoRefNbr = value;
			}
		}
		#endregion
		#region AdjdHasPPDTaxes
		public abstract class adjdHasPPDTaxes : PX.Data.BQL.BqlBool.Field<adjdHasPPDTaxes> { }
		protected bool? _AdjdHasPPDTaxes;

		/// <summary>
		/// When <c>true</c>, indicates that the linked adjusted document has taxes, that are reduces cash discount taxable amount on early payment.
		/// </summary>
		[PXDBBool]
		[PXDefault]
		[PXFormula(typeof(IsNull<Selector<ARAdjust.adjdRefNbr, ARInvoice.hasPPDTaxes>, False>))]
		public virtual bool? AdjdHasPPDTaxes
		{
			get
			{
				return _AdjdHasPPDTaxes;
			}
			set
			{
				_AdjdHasPPDTaxes = value;
			}
		}
		#endregion
		#region PendingPPD
		public abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }
		protected bool? _PendingPPD;

		/// <summary>
		/// When <c>true</c>, indicates that the linked adjusted document has been paid in full and 
		/// to close the document, you need to apply the cash discount by generating a credit memo on the "Generate VAT Credit Memos" (AR.50.45.00) form.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.PendingPPD, Enabled = false, Visible = false)]
		public virtual bool? PendingPPD
		{
			get
			{
				return _PendingPPD;
			}
			set
			{
				_PendingPPD = value;
			}
		}
		#endregion
		#region StatementDate
		public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		[PXDBDate]
		public virtual DateTime? StatementDate
		{
			get;
			set;
		}
		#endregion

		#region TaxInvoiceNbr
		public abstract class taxInvoiceNbr : PX.Data.BQL.BqlString.Field<taxInvoiceNbr> { }

		/// <summary>
		/// The "Tax Doc. Nbr" of the tax transaction, generated on the "Recognize Output/Input VAT" (TX503000/TX503500) form.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TaxTran.TaxInvoiceNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Doc. Nbr", Enabled = false, Visible = false)]
		public virtual string TaxInvoiceNbr
		{
			get;
			set;
		}
		#endregion
		#region IsMigratedRecord
		public abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the record has been created 
		/// in migration mode without affecting GL module.
		/// </summary>
		[MigratedRecord(typeof(ARSetup.migrationMode))]
		public virtual bool? IsMigratedRecord
		{
			get;
			set;
		}
		#endregion
		#region IsInitialApplication
		public abstract class isInitialApplication : PX.Data.BQL.BqlBool.Field<isInitialApplication> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the application has been created 
		/// for the migrated document to affect all needed balances.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsInitialApplication
		{
			get;
			set;
		}
		#endregion

		/// <summary>
		/// These fields are required to display two-way (incoming and outgoing)
		/// applications in a same grid: e.g. displaying the adjusting document 
		/// number in case of an incoming application, and adjusted document number
		/// in case of an outgoing application. The fields are controlled by specific 
		/// BLCs, e.g. filled out in delegates or by formula.
		/// </summary>
		#region Display Fields

		#region DisplayRefNbr
		public abstract class displayRefNbr : PX.Data.BQL.BqlString.Field<displayRefNbr> { }
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXSelector(typeof(Search<Standalone.ARRegister.refNbr, Where<Standalone.ARRegister.docType, Equal<Current<ARAdjust.displayDocType>>>>), ValidateValue = false)]
		public virtual string DisplayRefNbr { get; set; }
		#endregion

		#region DisplayCustomerID
		public abstract class displayCustomerID : PX.Data.BQL.BqlString.Field<displayCustomerID> { }
		[PXString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer", Enabled = false)]
		public virtual string DisplayCustomerID { get; set; }
		#endregion

		#region DisplayDocDate
		public abstract class displayDocDate : PX.Data.BQL.BqlDateTime.Field<displayDocDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? DisplayDocDate { get; set; }
		#endregion

		#region DisplayDocDesc
		public abstract class displayDocDesc : PX.Data.BQL.BqlString.Field<displayDocDesc> { }
		[PXString(150, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string DisplayDocDesc { get; set; }
		#endregion

		#region DisplayCuryID
		public abstract class displayCuryID : PX.Data.BQL.BqlString.Field<displayCuryID> { }
		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency")]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string DisplayCuryID { get; set; }
		#endregion

		#region DisplayFinPeriodID
		public abstract class displayFinPeriodID : PX.Data.BQL.BqlString.Field<displayFinPeriodID> { }
		[PXString(FinPeriodUtils.FULL_LENGHT, IsFixed = true)]
		[FinPeriodIDFormatting]
		[PXSelector(typeof(Search<MasterFinPeriod.finPeriodID>))]
		[PXUIField(DisplayName = "Post Period")]
		public virtual string DisplayFinPeriodID { get; set; }
		#endregion

		#region DisplayStatus
		public abstract class displayStatus : PX.Data.BQL.BqlString.Field<displayStatus> { }
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[ARDocStatus.List]
		public virtual string DisplayStatus { get; set; }
		#endregion

		#region DisplayCuryInfoID
		public abstract class displayCuryInfoID : PX.Data.BQL.BqlLong.Field<displayCuryInfoID> { }
		[PXLong]
		public virtual long? DisplayCuryInfoID { get; set; }
		#endregion

		#region DisplayCuryAmt
		public abstract class displayCuryAmt : PX.Data.BQL.BqlDecimal.Field<displayCuryAmt> { }
		[PXCurrency(typeof(ARAdjust.displayCuryInfoID), typeof(ARAdjust.adjAmt), BaseCalc = false)]
		[PXUIField(DisplayName = Messages.AmountPaid)]
		public virtual decimal? DisplayCuryAmt { get; set; }
		#endregion

		#region DisplayCuryPPDAmt
		public abstract class displayCuryPPDAmt : PX.Data.BQL.BqlDecimal.Field<displayCuryPPDAmt> { }

		[PXCurrency(typeof(ARAdjust.displayCuryInfoID), typeof(ARAdjust.adjPPDAmt), BaseCalc = false)]
		[PXUIField(DisplayName = Messages.CashDiscountTaken)]
		public virtual decimal? DisplayCuryPPDAmt
		{
			get;
			set;
		}
		#endregion

		#region DisplayCuryWOAmt
		public abstract class displayCuryWOAmt : PX.Data.BQL.BqlDecimal.Field<displayCuryWOAmt> { }
		[PXCurrency(typeof(ARAdjust.displayCuryInfoID), typeof(ARAdjust.adjWOAmt), BaseCalc = false)]
		[PXUIField(DisplayName = Messages.BalanceWriteOff, Enabled = false)]
		public virtual decimal? DisplayCuryWOAmt
		{
			get;
			set;
		}
		#endregion

		#endregion

		#region Explicit Interface Implementations
		string IDocumentAdjustment.Module => BatchModule.AR;
		decimal? IAdjustmentAmount.AdjThirdAmount
		{
			get { return AdjWOAmt; }
			set { AdjWOAmt = value; }
		}
		decimal? IAdjustmentAmount.CuryAdjdThirdAmount
		{
			get { return CuryAdjdWOAmt; }
			set { CuryAdjdWOAmt = value; }
		}
		decimal? IAdjustmentAmount.CuryAdjgThirdAmount
		{
			get { return CuryAdjgWOAmt; }
			set { CuryAdjgWOAmt = value; }
		}
		#endregion
	}

	[Serializable]
	[PXCacheName(Messages.ARAdjust)]
	[PXBreakInheritance()]
	[DebuggerDisplay(nameof(ARAdjust2) + ": Invoice (Type {" + nameof(AdjdDocType) + "}, Number {" + nameof(AdjdRefNbr) + "}), Payment (Type {" + nameof(AdjgDocType) + "}, Number {" + nameof(AdjgRefNbr) + "})")]
	public partial class ARAdjust2 : PX.Objects.AR.ARAdjust
	{

		public new abstract class selected : IBqlField { }

		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDefault]
		[Customer(Visible = false, Enabled = false)]
		public override Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region AdjgDocType
		public new abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[ARPaymentType.List()]
		[PXDefault()]
		[PXUIField(DisplayName = "Doc. Type", Enabled = false)]
		public override String AdjgDocType
		{
			get
			{
				return this._AdjgDocType;
			}
			set
			{
				this._AdjgDocType = value;
			}
		}
		#endregion
		#region AdjgRefNbr
		public new abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		[ARPaymentType.AdjgRefNbr(typeof(Search<ARPayment.refNbr, Where<ARPayment.docType, Equal<Optional<ARAdjust2.adjgDocType>>>>), Filterable = true)]
		public override String AdjgRefNbr
		{
			get
			{
				return this._AdjgRefNbr;
			}
			set
			{
				this._AdjgRefNbr = value;
			}
		}
		#endregion
		#region AdjgBranchID
		public new abstract class adjgBranchID : PX.Data.BQL.BqlInt.Field<adjgBranchID> { }
		[Branch(useDefaulting: false, Enabled = false)]
		public override Int32? AdjgBranchID
		{
			get
			{
				return this._AdjgBranchID;
			}
			set
			{
				this._AdjgBranchID = value;
			}
		}
		#endregion
		#region AdjdCuryInfoID
		public new abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(AR.ARInvoice.curyInfoID), ModuleCode = BatchModule.AR, CuryIDField = "AdjdCuryID")]
		public override Int64? AdjdCuryInfoID
		{
			get
			{
				return this._AdjdCuryInfoID;
			}
			set
			{
				this._AdjdCuryInfoID = value;
			}
		}
		#endregion
		#region AdjdDocType
		public new abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDBDefault(typeof(AR.ARInvoice.docType))]
		[PXUIField(DisplayName = Messages.DocumentType, Visibility = PXUIVisibility.Invisible, Visible = false)]
		public override string AdjdDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjdRefNbr
		public new abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(AR.ARInvoice.refNbr))]
		[PXParent(typeof(Select<AR.ARInvoice, 
			Where<AR.ARInvoice.docType, Equal<Current<ARAdjust2.adjdDocType>>, 
				And<AR.ARInvoice.refNbr, Equal<Current<ARAdjust2.adjdRefNbr>>>>>))]
		[PXParent(typeof(Select<SOInvoice, 
			Where<SOInvoice.docType, Equal<Current<ARAdjust2.adjdDocType>>, 
				And<SOInvoice.refNbr, Equal<Current<ARAdjust2.adjdRefNbr>>>>>))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public override string AdjdRefNbr { get; set; }
		#endregion
		#region AdjdLineNbr
		public new abstract class adjdLineNbr : PX.Data.BQL.BqlInt.Field<adjdLineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXDefault(typeof(Switch<Case<Where<Selector<ARAdjust2.adjdRefNbr, AR.ARInvoice.paymentsByLinesAllowed>, NotEqual<True>>, int0>, Null>))]
		[ARInvoiceType.AdjdLineNbr]
		public override int? AdjdLineNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdBranchID
		public new abstract class adjdBranchID : PX.Data.BQL.BqlInt.Field<adjdBranchID> { }
		[Branch(typeof(AR.ARInvoice.branchID))]
		[PXDBDefault(typeof(AR.ARInvoice.branchID))]
		public override Int32? AdjdBranchID
		{
			get
			{
				return this._AdjdBranchID;
			}
			set
			{
				this._AdjdBranchID = value;
			}
		}
		#endregion
		#region AdjNbr
		public new abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXDefault()]
		public override Int32? AdjNbr
		{
			get
			{
				return this._AdjNbr;
			}
			set
			{
				this._AdjNbr = value;
			}
		}
		#endregion
		#region AdjBatchNbr
		public new abstract class adjBatchNbr : PX.Data.BQL.BqlString.Field<adjBatchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public override String AdjBatchNbr
		{
			get
			{
				return this._AdjBatchNbr;
			}
			set
			{
				this._AdjBatchNbr = value;
			}
		}
		#endregion
		#region AdjdOrigCuryInfoID
		public new abstract class adjdOrigCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdOrigCuryInfoID> { }
		[PXDBLong()]
		[PXDBDefault(typeof(AR.ARInvoice.curyInfoID))]
		public override Int64? AdjdOrigCuryInfoID
		{
			get
			{
				return this._AdjdOrigCuryInfoID;
			}
			set
			{
				this._AdjdOrigCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgCuryInfoID
		public new abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = BatchModule.AR, CuryIDField = "AdjgCuryID")]
		public override Int64? AdjgCuryInfoID
		{
			get
			{
				return this._AdjgCuryInfoID;
			}
			set
			{
				this._AdjgCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgDocDate
		public new abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate> { }
		[PXDBDate()]
		[PXDBDefault(typeof(AR.ARInvoice.docDate))]
		public override DateTime? AdjgDocDate
		{
			get
			{
				return this._AdjgDocDate;
			}
			set
			{
				this._AdjgDocDate = value;
			}
		}
		#endregion
		#region AdjgFinPeriodID
		public new abstract class adjgFinPeriodID : PX.Data.BQL.BqlString.Field<adjgFinPeriodID> { }
		[PXDefault()]
		[FinPeriodID(
			branchSourceType: typeof(ARAdjust2.adjgBranchID),
			masterFinPeriodIDType: typeof(ARAdjust2.adjgTranPeriodID),
			headerMasterFinPeriodIDType: typeof(AR.ARInvoice.tranPeriodID))]
		public override String AdjgFinPeriodID
		{
			get
			{
				return this._AdjgFinPeriodID;
			}
			set
			{
				this._AdjgFinPeriodID = value;
			}
		}
		#endregion
		#region AdjgTranPeriodID
		public new abstract class adjgTranPeriodID : PX.Data.BQL.BqlString.Field<adjgTranPeriodID> { }
		[PeriodID]
		[PXDBDefault()]
		public override String AdjgTranPeriodID
		{
			get
			{
				return this._AdjgTranPeriodID;
			}
			set
			{
				this._AdjgTranPeriodID = value;
			}
		}
		#endregion
		#region AdjdDocDate
		public new abstract class adjdDocDate : PX.Data.BQL.BqlDateTime.Field<adjdDocDate> { }
		[PXDBDate()]
		[PXDBDefault(typeof(AR.ARInvoice.docDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override DateTime? AdjdDocDate
		{
			get
			{
				return this._AdjdDocDate;
			}
			set
			{
				this._AdjdDocDate = value;
			}
		}
		#endregion
		#region AdjdFinPeriodID
		public new abstract class adjdFinPeriodID : PX.Data.BQL.BqlString.Field<adjdFinPeriodID> { }
		[PXDBDefault()]
		[FinPeriodID(
			branchSourceType: typeof(ARAdjust2.adjdBranchID),
			masterFinPeriodIDType: typeof(ARAdjust2.adjdTranPeriodID),
			headerMasterFinPeriodIDType: typeof(AR.ARInvoice.tranPeriodID))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public override String AdjdFinPeriodID
		{
			get
			{
				return this._AdjdFinPeriodID;
			}
			set
			{
				this._AdjdFinPeriodID = value;
			}
		}
		#endregion
		#region AdjdTranPeriodID
		public new abstract class adjdTranPeriodID : PX.Data.BQL.BqlString.Field<adjdTranPeriodID> { }
		[PXDefault()]
		[PeriodID()]
		public override String AdjdTranPeriodID
		{
			get
			{
				return this._AdjdTranPeriodID;
			}
			set
			{
				this._AdjdTranPeriodID = value;
			}
		}
		#endregion
		#region CuryAdjdDiscAmt
		public new abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdDiscAmt> { }
		[PXDBCurrency(typeof(ARAdjust2.adjdCuryInfoID), typeof(ARAdjust2.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.CashDiscountTaken, Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryAdjdDiscAmt
		{
			get
			{
				return this._CuryAdjdDiscAmt;
			}
			set
			{
				this._CuryAdjdDiscAmt = value;
			}
		}
		#endregion
		#region CuryAdjdWOAmt
		public new abstract class curyAdjdWOAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdWOAmt> { }

		[PXDBCurrency(typeof(ARAdjust2.adjdCuryInfoID), typeof(ARAdjust2.adjWOAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.BalanceWriteOff, Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(SumCalc<ARPayment.curyWOAmt>))]
        [PXFormula(null, typeof(SumCalc<AR.ARInvoice.curyBalanceWOTotal>))]
        public override decimal? CuryAdjdWOAmt
		{
			get;
			set;
		}
		public override decimal? CuryAdjdWhTaxAmt
		{
			get
			{
				return CuryAdjdWOAmt;
			}
			set
			{
				CuryAdjdWOAmt = value;
			}
		}
		#endregion
		#region CuryAdjdAmt
		public new abstract class curyAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdAmt> { }

		[PXDBCurrency(typeof(ARAdjust2.adjdCuryInfoID), typeof(ARAdjust2.adjAmt), BaseCalc = false, MinValue = 0)]
		[PXUIField(DisplayName = Messages.AmountPaid, Visibility = PXUIVisibility.Visible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<AR.ARInvoice.curyPaymentTotal>))]
        [PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjdBilledAmt>))]
		public override Decimal? CuryAdjdAmt
		{
			get
			{
				return this._CuryAdjdAmt;
			}
			set
			{
				this._CuryAdjdAmt = value;
			}
		}
        #endregion
        #region CuryAdjdOrigAmt
        public new abstract class curyAdjdOrigAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdOrigAmt> { }
        [PXDBCurrency(typeof(ARAdjust2.adjdCuryInfoID), typeof(ARAdjust2.adjAmt), BaseCalc = false)]
        [PXUIField(DisplayName = "Original Amount Paid", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? CuryAdjdOrigAmt
        {
            get
            {
                return this._CuryAdjdOrigAmt;
            }
            set
            {
                this._CuryAdjdOrigAmt = value;
            }
        }
        #endregion

        #region AdjAmt
		public new abstract class adjAmt : PX.Data.BQL.BqlDecimal.Field<adjAmt> { }
		[PXDBDecimal(4)]
		[PXFormula(null, typeof(SumCalc<SOAdjust.adjBilledAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? AdjAmt
		{
			get
			{
				return this._AdjAmt;
			}
			set
			{
				this._AdjAmt = value;
			}
		}
		#endregion
		#region AdjWOAmt
		public new abstract class adjWOAmt : PX.Data.BQL.BqlDecimal.Field<adjWOAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? AdjWOAmt
		{
			get;
			set;
		}
		public override decimal? AdjWhTaxAmt
		{
			get
			{
				return AdjWOAmt;
			}
			set
			{
				AdjWOAmt = value;
			}
		}
		#endregion
		#region AdjDiscAmt
		public new abstract class adjDiscAmt : PX.Data.BQL.BqlDecimal.Field<adjDiscAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? AdjDiscAmt
		{
			get
			{
				return this._AdjDiscAmt;
			}
			set
			{
				this._AdjDiscAmt = value;
			}
		}
		#endregion
		#region CuryAdjgDiscAmt
		public new abstract class curyAdjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgDiscAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.CashDiscountTaken, Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryAdjgDiscAmt
		{
			get
			{
				return this._CuryAdjgDiscAmt;
			}
			set
			{
				this._CuryAdjgDiscAmt = value;
			}
		}
		#endregion
		#region CuryAdjgWOAmt
		public new abstract class curyAdjgWOAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgWOAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? CuryAdjgWOAmt
		{
			get;
			set;
		}
		public override decimal? CuryAdjgWhTaxAmt
		{
			get
			{
				return CuryAdjgWOAmt;
			}
			set
			{
				CuryAdjgWOAmt = value;
			}
		}
		#endregion
		#region CuryAdjgAmt
		public new abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<SOAdjust.curyAdjgBilledAmt>))]
		public override Decimal? CuryAdjgAmt
		{
			get
			{
				return this._CuryAdjgAmt;
			}
			set
			{
				this._CuryAdjgAmt = value;
			}
		}
		#endregion
		#region RGOLAmt
		public new abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? RGOLAmt
		{
			get
			{
				return this._RGOLAmt;
			}
			set
			{
				this._RGOLAmt = value;
			}
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region VoidAdjNbr
		public new abstract class voidAdjNbr : PX.Data.BQL.BqlInt.Field<voidAdjNbr> { }
		[PXDBInt()]
		public override Int32? VoidAdjNbr
		{
			get
			{
				return this._VoidAdjNbr;
			}
			set
			{
				this._VoidAdjNbr = value;
			}
		}
		#endregion
		#region AdjdARAcct
		public new abstract class adjdARAcct : PX.Data.BQL.BqlInt.Field<adjdARAcct> { }
		[Account()]
		[PXDBDefault(typeof(AR.ARInvoice.aRAccountID))]
		public override Int32? AdjdARAcct
		{
			get
			{
				return this._AdjdARAcct;
			}
			set
			{
				this._AdjdARAcct = value;
			}
		}
		#endregion
		#region AdjdARSub
		public new abstract class adjdARSub : PX.Data.BQL.BqlInt.Field<adjdARSub> { }
		[SubAccount()]
		[PXDBDefault(typeof(AR.ARInvoice.aRSubID))]
		public override Int32? AdjdARSub
		{
			get
			{
				return this._AdjdARSub;
			}
			set
			{
				this._AdjdARSub = value;
			}
		}
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote()]
		public override Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		[PXCurrency(typeof(ARAdjust2.adjdCuryInfoID), typeof(ARAdjust2.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? CuryDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}
		public override Decimal? CuryPayDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}
		#endregion
		#region DocBal
		public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		[PXDecimal(4)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		public override Decimal? PayDocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		#endregion
		#region CuryDiscBal
		public new abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		[PXCurrency(typeof(ARAdjust2.adjdCuryInfoID), typeof(ARAdjust2.discBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? CuryDiscBal
		{
			get
			{
				return this._CuryDiscBal;
			}
			set
			{
				this._CuryDiscBal = value;
			}
		}
		public override Decimal? CuryPayDiscBal
		{
			get
			{
				return this._CuryDiscBal;
			}
			set
			{
				this._CuryDiscBal = value;
			}
		}
		#endregion
		#region DiscBal
		public new abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		[PXDecimal(4)]
		[PXUnboundDefault()]
		public override Decimal? DiscBal
		{
			get
			{
				return this._DiscBal;
			}
			set
			{
				this._DiscBal = value;
			}
		}
		public override Decimal? PayDiscBal
		{
			get
			{
				return this._DiscBal;
			}
			set
			{
				this._DiscBal = value;
			}
		}
		#endregion
		#region CuryWOBal
		public new abstract class curyWOBal : PX.Data.BQL.BqlDecimal.Field<curyWOBal> { }

		[PXCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.wOBal), BaseCalc = false)]
		[PXUnboundDefault]
		[PXUIField(DisplayName = "Write-Off Limit", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override decimal? CuryWOBal
		{
			get
			{
				return _CuryWOBal;
			}
			set
			{
				_CuryWOBal = value;
			}
		}
		public override decimal? CuryWhTaxBal
		{
			get
			{
				return _CuryWOBal;
			}
			set
			{
				_CuryWOBal = value;
			}
		}
		#endregion
		#region WOBal
		public new abstract class wOBal : PX.Data.BQL.BqlDecimal.Field<wOBal> { }

		[PXDecimal(4)]
		[PXUnboundDefault]
		public override decimal? WOBal
		{
			get
			{
				return _WOBal;
			}
			set
			{
				_WOBal = value;
			}
		}
		public override decimal? WhTaxBal
		{
			get
			{
				return _WOBal;
			}
			set
			{
				_WOBal = value;
			}
		}
		#endregion
		#region VoidAppl
		public new abstract class voidAppl : PX.Data.BQL.BqlBool.Field<voidAppl> { }
		[PXBool()]
		[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public override Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(adjgDocType))]
			get
			{
				return ARPaymentType.VoidAppl(AdjgDocType);
			}
			set
			{
				if ((bool)value)
				{
					AdjgDocType = ARPaymentType.GetVoidingARDocType(AdjgDocType) ?? ARDocType.VoidPayment;
					this.Voided = true;
				}
			}
		}
		#endregion
		#region ReverseGainLoss
		public new abstract class reverseGainLoss : PX.Data.BQL.BqlBool.Field<reverseGainLoss> { }
		[PXBool()]
		public override Boolean? ReverseGainLoss
		{
			[PXDependsOnFields(typeof(adjgDocType))]
			get
			{
				return (ARPaymentType.DrCr(this._AdjgDocType) == "C");
			}
			set
			{
			}
		}
		#endregion
	}

	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARAdjust)]
	public partial class ARAdjustReport : ARAdjust
	{
		public new abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }

		public new abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }

		public new abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate> { }

		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		public new abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }

		public new abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }

		public new abstract class curyAdjdAmt : PX.Data.BQL.BqlString.Field<curyAdjdAmt> { }

		public new abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlString.Field<curyAdjdDiscAmt> { }

		public new abstract class curyAdjdWOAmt : PX.Data.BQL.BqlString.Field<curyAdjdWOAmt> { }

		public new abstract class rGOLAmt : PX.Data.BQL.BqlString.Field<rGOLAmt> { }

		public new abstract class adjAmt : PX.Data.BQL.BqlString.Field<adjAmt> { }

		public new abstract class adjDiscAmt : PX.Data.BQL.BqlString.Field<adjDiscAmt> { }

		public new abstract class adjWOAmt : PX.Data.BQL.BqlString.Field<adjWOAmt> { }

		public new abstract class curyAdjgAmt : PX.Data.BQL.BqlString.Field<curyAdjgAmt> { }

		#region LineTotalAdjusted
		public abstract class lineTotalAdjusted : PX.Data.BQL.BqlDecimal.Field<lineTotalAdjusted> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(
			Sub<
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					Add<ARAdjust.adjAmt, Add<ARAdjust.adjDiscAmt, ARAdjust.adjWOAmt>>>,
				ARAdjust.rGOLAmt>
			), typeof(Decimal))]
		public virtual Decimal? LineTotalAdjusted { get; set; }
		#endregion

		#region CuryLineTotalAdjusted
		public abstract class curyLineTotalAdjusted : PX.Data.BQL.BqlDecimal.Field<curyLineTotalAdjusted> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(
			Mult<
				Switch<
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
				Add<ARAdjust.curyAdjdAmt, Add<ARAdjust.curyAdjdDiscAmt, ARAdjust.curyAdjdWOAmt>>>
			), typeof(Decimal))]
		public virtual Decimal? CuryLineTotalAdjusted { get; set; }
		#endregion

		#region LineTotalAdjusting
		public abstract class lineTotalAdjusting : PX.Data.BQL.BqlDecimal.Field<lineTotalAdjusting> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(
			Mult<
				Switch<
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
				ARAdjust.adjAmt>
			), typeof(Decimal))]
		public virtual Decimal? LineTotalAdjusting { get; set; }
		#endregion

		#region CuryLineTotalAdjusting
		public abstract class curyLineTotalAdjusting : PX.Data.BQL.BqlDecimal.Field<curyLineTotalAdjusting> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(
			Mult<
				Switch<
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
					Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
				ARAdjust.curyAdjgAmt>
			), typeof(Decimal))]
		public virtual Decimal? CuryLineTotalAdjusting { get; set; }
		#endregion
	}

	[PXHidden()]
	[Serializable]
	public partial class ARRegister2 : AR.ARRegister
	{
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		public override long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region ClosedFinPeriodID
		public new abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		#endregion
	}
}

namespace PX.Objects.AR.Standalone
{
	[PXHidden]
	[Serializable]
	public class ARAdjust : IBqlTable
	{
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		public virtual string AdjdDocType { get; set; }
		#endregion
		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		public virtual string AdjdRefNbr { get; set; }
		#endregion
		#region AdjdLineNbr
		public abstract class adjdLineNbr : PX.Data.BQL.BqlInt.Field<adjdLineNbr> { }
		[PXDBInt(IsKey = true)]
		public virtual int? AdjdLineNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		public virtual string AdjgDocType { get; set; }
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]

		public virtual string AdjgRefNbr { get; set; }
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		[PXDBInt(IsKey = true)]
		public virtual int? AdjNbr { get; set; }
		#endregion
		#region AdjdCuryInfoID
		public abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }
		protected Int64? _AdjdCuryInfoID;

		/// <summary>
		/// Identifier of the <see cref="CurrencyInfo">Currency Info</see> record associated with the adjusted document.
		/// </summary>
		[PXDBLong()]
		public virtual Int64? AdjdCuryInfoID
		{
			get;
			set;
		}
		#endregion
	}
}
