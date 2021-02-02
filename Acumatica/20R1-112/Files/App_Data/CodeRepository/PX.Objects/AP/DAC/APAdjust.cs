using System;
using System.Diagnostics;
using PX.Data;

using PX.Objects.Common.MigrationMode;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.AP.Standalone;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.AP
{
    /// <summary>
	/// Records the fact of application of one Accounts Payable document to another,
	/// which results in an adjustment of the balances of both documents. It can be either 
	/// an application of a <see cref="APPayment">payment document</see> to an
	/// <see cref="APInvoice">invoice document</see> (for example, when a check
	/// closes a bill), or an application of one <see cref="APPayment">payment</see> document 
	/// to another, such as an application of a vendor refund to a prepayment. The entities
	/// of this type are mainly edited on the Checks and Payments (AP302000) form,
	/// which corresponds to the <see cref="APPaymentEntry"/> graph. They can also be edited
	/// on the Applications tab of the Bills and Adjustments (AP301000) form, which corresponds
	/// to the <see cref="APInvoiceEntry"/> graph.
    /// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] 
		{
			typeof(APQuickCheckEntry),
			typeof(APPaymentEntry)
		},
		new Type[] 
		{
			typeof(Select<APQuickCheck,
				Where<APQuickCheck.docType, Equal<Current<APAdjust.adjgDocType>>,
				And<APQuickCheck.refNbr, Equal<Current<APAdjust.adjgRefNbr>>>>>),
			typeof(Select<APPayment, 
			Where<APPayment.docType, Equal<Current<APAdjust.adjgDocType>>,
			And<APPayment.refNbr, Equal<Current<APAdjust.adjgRefNbr>>>>>)
		})]
	[PXCacheName(Messages.APAdjust)]    
	[DebuggerDisplay("{AdjdDocType}:{AdjdRefNbr}:{AdjdLineNbr} - {AdjgDocType}:{AdjgRefNbr}:{AdjNbr}")]

	public partial class APAdjust 
		: IBqlTable, IAdjustment, IDocumentAdjustment, IAdjustmentAmount, IAdjustmentStub
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

        /// <summary>
        /// Indicates whether the record is selected for mass processing or not.
        /// </summary>
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		protected Boolean? _SeparateCheck;

        /// <summary>
        /// When set to <c>true</c> indicates that the adjusted document should be paid for by a separate check.
        /// (See <see cref="APInvoice.SeparateCheck" />)
        /// </summary>
		[PXBool()]
		[PXUIField(DisplayName = "Pay Separately", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

        /// <summary>
        /// Identifier of the <see cref="Vendor"/>, whom the related documents belongs.
        /// </summary>
		[Vendor(Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDBDefault(typeof(APPayment.vendorID))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		protected String _AdjgDocType;

        /// <summary>
        /// [key] The type of the adjusting document.
        /// </summary>
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDBDefault(typeof(APPayment.docType))]
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

        /// <summary>
        /// The type of the adjusting document for printing. Internal representation is the same as for <see cref="AdjgDocType"/>,
        /// but the user-friendly values of these two fields differ.
        /// </summary>
		[PXString(3, IsFixed = true)]
		[APDocType.PrintList()]
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
        
        /// <summary>
        /// [key] Reference number of the adjusting document.
        /// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(APPayment.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<APRegister,
			Where<APRegister.docType, Equal<Current<APAdjust.adjgDocType>>, 
				And<APRegister.refNbr,Equal<Current<APAdjust.adjgRefNbr>>, 
				And<Current<APAdjust.released>, NotEqual<True>>>>>))]
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

        /// <summary>
        /// Identifier of the <see cref="Branch"/>, to which the adjusting document belongs.
        /// </summary>
		[Branch(typeof(APPayment.branchID))]
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

        /// <summary>
        /// Identifier of the <see cref="CurrencyInfo">Currency Info</see> record associated with the adjusted document.
        /// </summary>
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AP, CuryIDField = "AdjdCuryID", Enabled = false)]
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
		protected String _AdjdDocType;

        /// <summary>
        /// [key] The type of the adjusted document.
        /// </summary>
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault(APDocType.Invoice)]
		[PXUIField(DisplayName = "Document Type", Visibility=PXUIVisibility.Visible)]
		[APInvoiceType.AdjdList()]
		public virtual String AdjdDocType
		{
			get
			{
				return this._AdjdDocType;
			}
			set
			{
				this._AdjdDocType = value;
			}
		}
		#endregion
		#region PrintAdjdDocType
		public abstract class printAdjdDocType : PX.Data.BQL.BqlString.Field<printAdjdDocType> { }

        /// <summary>
        /// The type of the adjusted document for printing. Internal representation is the same as for <see cref="AdjgDocType"/>,
        /// but the user-friendly values of these two fields differ.
        /// </summary>
		[PXString(3, IsFixed = true)]
		[APDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintAdjdDocType
		{
			get
			{
				return this._AdjdDocType;
			}
			set
			{
			}
		}
		#endregion
		#region AdjdRefNbr
		[PXHidden()]
		[PXProjection(typeof(Select2<Standalone.APRegister, 
			LeftJoin<Standalone.APInvoice, On<Standalone.APInvoice.docType, Equal<Standalone.APRegister.docType>, 
				And<Standalone.APInvoice.refNbr, Equal<Standalone.APRegister.refNbr>>>>>))]
        [Serializable]
		public partial class APInvoice : Standalone.APRegister
		{
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region VendorID
			public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			#endregion
			#region Released
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
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
		    #region TranPeriodID
		    public new abstract class tranPeriodID : PX.Data.IBqlField
		    {
		    }
            #endregion
			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			[PXDBDate(BqlField = typeof(Standalone.APInvoice.dueDate))]
			[PXUIField(DisplayName = "Due Date")]
			public virtual DateTime? DueDate
			{
				get;
				set;
			}
			#endregion
			#region InvoiceNbr
			public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
			[PXDBString(40, IsUnicode = true, BqlField = typeof(Standalone.APInvoice.invoiceNbr))]
			[PXUIField(DisplayName = "Vendor Ref.")]
			public virtual string InvoiceNbr
			{
				get;
				set;
			}
			#endregion
			#region IsMigratedRecord
			public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
			#endregion
			#region PendingPPD
			public new abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }
			#endregion
			#region PaymentsByLinesAllowed
			public new abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }
			#endregion
		}
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }

        /// <summary>
        /// [key] Reference number of the adjusted document.
        /// </summary>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.AdjdRefNbr(typeof(Search2<APInvoice.refNbr, 
			LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>, 
				And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>, 
				And<APAdjust.released, Equal<False>, 
				And<Where<APAdjust.adjgDocType, NotEqual<Current<APPayment.docType>>, 
					Or<APAdjust.adjgRefNbr, NotEqual<Current<APPayment.refNbr>>>>>>>>,
			LeftJoin<APAdjust2, On<APAdjust2.adjgDocType, Equal<APInvoice.docType>, 
				And<APAdjust2.adjgRefNbr, Equal<APInvoice.refNbr>, 
				And<APAdjust2.released, Equal<False>, 
				And<APAdjust2.voided, Equal<False>>>>>,
			LeftJoin<APPayment, On<APPayment.docType, Equal<APInvoice.docType>,
				And<APPayment.refNbr, Equal<APInvoice.refNbr>, 
				And<Where<APPayment.docType, Equal<APDocType.prepayment>, 
					Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>>,
			Where<APInvoice.vendorID, Equal<Optional<APPayment.vendorID>>, 
				And<APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>, 
				And2<Where<APInvoice.released, Equal<True>, 
					Or<APInvoice.prebooked, Equal<True>>>, 
				And<APInvoice.openDoc, Equal<True>,
				And<APInvoice.hold, Equal<False>,
				And<APAdjust.adjgRefNbr, IsNull, 
				And<APAdjust2.adjdRefNbr, IsNull,
				And2<Where<APPayment.refNbr, IsNull, 
					And<Current<APPayment.docType>, NotEqual<APDocType.refund>, 
					Or<APPayment.refNbr, IsNotNull, 
					And<Current<APPayment.docType>, Equal<APDocType.refund>, 
					Or<APPayment.docType, Equal<APDocType.debitAdj>, 
					And<Current<APPayment.docType>, Equal<APDocType.check>, 
					Or<APPayment.docType, Equal<APDocType.debitAdj>, 
					And<Current<APPayment.docType>, Equal<APDocType.voidCheck>>>>>>>>>,
				And2<Where<APInvoice.docDate, LessEqual<Current<APPayment.adjDate>>, 
					And<APInvoice.tranPeriodID, LessEqual<Current<APPayment.adjTranPeriodID>>, 
					Or<Current<APPayment.adjTranPeriodID>, IsNull, 
					Or<Current<APPayment.docType>, Equal<APDocType.check>, 
					And<Current<APSetup.earlyChecks>, Equal<True>, 
					Or<Current<APPayment.docType>, Equal<APDocType.voidCheck>, 
					And<Current<APSetup.earlyChecks>, Equal<True>,
					Or<Current<APPayment.docType>, Equal<APDocType.prepayment>, 
					And<Current<APSetup.earlyChecks>, Equal<True>>>>>>>>>>,
				And2<Where<
					Current<APSetup.migrationMode>, NotEqual<True>,
					Or<APInvoice.isMigratedRecord, Equal<Current<APRegister.isMigratedRecord>>>>,
				And<Where<APInvoice.pendingPPD, NotEqual<True>,
					Or<Current<APRegister.pendingPPD>, Equal<True>>>>>>>>>>>>>>>), Filterable = true)]
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
		[PXDefault(typeof(Switch<Case<Where<Selector<APAdjust.adjdRefNbr, APInvoice.paymentsByLinesAllowed>, NotEqual<True>>, int0>, Null>))]
		[APInvoiceType.AdjdLineNbr]
		public virtual int? AdjdLineNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdBranchID
		public abstract class adjdBranchID : PX.Data.BQL.BqlInt.Field<adjdBranchID> { }
		protected Int32? _AdjdBranchID;

        /// <summary>
        /// Identifier of the <see cref="Branch"/>, to which the adjusted document belongs.
        /// </summary>
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

        /// <summary>
        /// The number of the adjustment.
        /// </summary>
        /// <value>
        /// Defaults to the current <see cref="APPayment.AdjCntr">number of lines</see> in the related payment document.
        /// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXDefault(typeof(APPayment.adjCntr))]
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
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[PXDBInt]
		public virtual int? CashAccountID { get; set; }
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual string PaymentMethodID { get; set; }
		#endregion
		#region StubNbr
		public abstract class stubNbr : PX.Data.BQL.BqlString.Field<stubNbr> { }
		protected String _StubNbr;

        /// <summary>
        /// The number of the payment stub.
        /// </summary>
		[PXDBString(40, IsUnicode = true)]
		public virtual String StubNbr
		{
			get
			{
				return this._StubNbr;
			}
			set
			{
				this._StubNbr = value;
			}
		}
		#endregion
		#region AdjBatchNbr
		public abstract class adjBatchNbr : PX.Data.BQL.BqlString.Field<adjBatchNbr> { }
		protected String _AdjBatchNbr;

        /// <summary>
        /// The number of the <see cref="Batch"/> generated from the adjustment.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Batch.BatchNbr"/> field.
        /// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="Batch Number", Visibility=PXUIVisibility.Visible, Visible=true, Enabled=false)]
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

        /// <summary>
        /// The reference number of the voiding adjustment.
        /// </summary>
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

        /// <summary>
        /// Identifier of the original <see cref="CurrencyInfo">Currency Info</see> record associated with the adjusted document.
        /// </summary>
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AP, CuryIDField = "AdjdOrigCuryID")]
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

        /// <summary>
        /// Identifier of the <see cref="CurrencyInfo">Currency Info</see> record associated with the adjusting document.
        /// </summary>
		[PXDBLong()]
		[CurrencyInfo(typeof(APPayment.curyInfoID), CuryIDField = "AdjgCuryID")]
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

        /// <summary>
        /// The date when the payment is applied.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="APPayment.AdjDate">application date specified on the adjusting document</see>.
        /// </value>
		[PXDBDate()]
		[PXDBDefault(typeof(APPayment.adjDate))]
        [PXUIField(DisplayName="Transaction Date")]
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

        /// <summary>
        /// Financial period of payment application.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="APPayment.AdjFinPeriodID">application period specified on the adjusting document</see>.
        /// </value>
		[FinPeriodID(
		    branchSourceType: typeof(APAdjust.adjgBranchID),
            masterFinPeriodIDType :typeof(APAdjust.adjgTranPeriodID),
		    headerMasterFinPeriodIDType :typeof(APPayment.adjTranPeriodID))]
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

        /// <summary>
        /// Financial period of the adjusting document. 
        /// This field corresponds to the <see cref="APPayment.AdjTranPeriodID"/> (not user-overridable) field of the adjusting document.
        /// </summary>
        [PeriodID]
        [PXUIField(DisplayName = "Post Period")]
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

        /// <summary>
        /// Either the date when the adjusted document was created or the date of the original vendor’s document.
        /// </summary>
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled=false)]
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

        /// <summary>
        /// <see cref="FinPeriod">Financial period</see> of the adjusted document. Corresponds to the <see cref="APRegister.FinPeriodID"/> field.
        /// </summary>
        /// <value>
        /// The value of this field is determined from the <see cref="APAdjust.AdjdDocDate"/> field.
        /// </value>
		[FinPeriodID(
		    branchSourceType: typeof(APAdjust.adjdBranchID),
		    masterFinPeriodIDType: typeof(APAdjust.adjdTranPeriodID))]
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

        /// <summary>
        /// <see cref="FinPeriod">Financial period</see> of the adjusted document. Corresponds to the <see cref="APRegister.TranPeriodID"/> field.
        /// </summary>
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

        /// <summary>
        /// The amount of the cash discount taken for the adjusting document.
        /// Presented in the currency of the document, see <see cref="APRegister.CuryID"/>.
        /// </summary>
		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjDiscAmt))]
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
		#region CuryAdjgWhTaxAmt
		public abstract class curyAdjgWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgWhTaxAmt> { }
		protected Decimal? _CuryAdjgWhTaxAmt;

        /// <summary>
        /// The amount of withholding tax calculated for the adjusting document, if applicable.
        /// Presented in the currency of the document, see <see cref="APRegister.CuryID"/>.
        /// </summary>
		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjWhTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "With. Tax", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryAdjgWhTaxAmt
		{
			get
			{
				return this._CuryAdjgWhTaxAmt;
			}
			set
			{
				this._CuryAdjgWhTaxAmt = value;
			}
		}
		#endregion
		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
		protected Decimal? _CuryAdjgAmt;

        /// <summary>
        /// The actual amount paid on the document.
        /// Presented in the currency of the document, see <see cref="APRegister.CuryID"/>.
        /// </summary>
		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjAmt), BaseCalc=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		//[PXFormula(null, typeof(SumCalc<APPayment.curyApplAmt>))]
        [PXUnboundFormula(typeof(Mult<APAdjust.adjgBalSign, APAdjust.curyAdjgAmt>), typeof(SumCalc<APPayment.curyApplAmt>))]
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

        /// <summary>
        /// The amount of the cash discount for the adjusted document.
        /// Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Cash Discount Amount")]
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
		#region CuryAdjdDiscAmt
		public abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdDiscAmt> { }
		protected Decimal? _CuryAdjdDiscAmt;

        /// <summary>
        /// The amount of the cash discount for the adjusted document.
        /// Presented in the currency of the document, see <see cref="CuryID"/>.
        /// </summary>
		[PXDBDecimal(4)]
		//[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region AdjWhTaxAmt
		public abstract class adjWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<adjWhTaxAmt> { }
		protected Decimal? _AdjWhTaxAmt;

        /// <summary>
        /// The amount of tax withheld from the payments to the adjusted document.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName="Withholding Tax Amount")]
		public virtual Decimal? AdjWhTaxAmt
		{
			get
			{
				return this._AdjWhTaxAmt;
			}
			set
			{
				this._AdjWhTaxAmt = value;
			}
		}
		#endregion
		#region CuryAdjdWhTaxAmt
		public abstract class curyAdjdWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdWhTaxAmt> { }
		protected Decimal? _CuryAdjdWhTaxAmt;

        /// <summary>
        /// The amount of tax withheld from the payments to the adjusted document.
        /// (Presented in the currency of the document, see <see cref="APRegister.CuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdWhTaxAmt
		{
			get
			{
				return this._CuryAdjdWhTaxAmt;
			}
			set
			{
				this._CuryAdjdWhTaxAmt = value;
			}
		}
		#endregion
		#region AdjAmt
		public abstract class adjAmt : PX.Data.BQL.BqlDecimal.Field<adjAmt> { }
		protected Decimal? _AdjAmt;

        /// <summary>
        /// The amount to be paid for the adjusted document. (See <see cref="APRegister.OrigDocAmt"/>)
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName="Amount")]
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

        /// <summary>
        /// The amount to be paid for the adjusted document. (See <see cref="APRegister.CuryOrigDocAmt"/>)
        /// (Presented in the currency of the document, see <see cref="APRegister.CuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		//[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdAmt
		{
			get; set;
		}
		#endregion
		#region RGOLAmt
		public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }
		protected Decimal? _RGOLAmt;

        /// <summary>
        /// Realized Gain and Loss amount associated with the adjustment.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Realized Gain/Loss Amount")]
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

        /// <summary>
        /// When set to <c>true</c> indicates that the adjustment was released.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName="Released")]
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

        /// <summary>
        /// When set to <c>true</c> indicates that the adjustment is on hold.
        /// </summary>
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

        /// <summary>
        /// When set to <c>true</c> indicates that the adjustment was voided.
        /// </summary>
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
		#region AdjdAPAcct
		public abstract class adjdAPAcct : PX.Data.BQL.BqlInt.Field<adjdAPAcct> { }
		protected Int32? _AdjdAPAcct;

        /// <summary>
        /// Identifier of the AP account, to which the adjusted document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[Account(SuppressCurrencyValidation = true)]
		[PXDefault()]
		public virtual Int32? AdjdAPAcct
		{
			get
			{
				return this._AdjdAPAcct;
			}
			set
			{
				this._AdjdAPAcct = value;
			}
		}
		#endregion
		#region AdjdAPSub
		public abstract class adjdAPSub : PX.Data.BQL.BqlInt.Field<adjdAPSub> { }
		protected Int32? _AdjdAPSub;

        /// <summary>
        /// Identifier of the AP subaccount, to which the adjusted document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[SubAccount()]
		[PXDefault()]
		public virtual Int32? AdjdAPSub
		{
			get
			{
				return this._AdjdAPSub;
			}
			set
			{
				this._AdjdAPSub = value;
			}
		}
		#endregion
		#region AdjdWhTaxAcctID
		public abstract class adjdWhTaxAcctID : PX.Data.BQL.BqlInt.Field<adjdWhTaxAcctID> { }
		protected Int32? _AdjdWhTaxAcctID;

        /// <summary>
        /// Identifier of the account associated with withholding tax for the adjusted document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[Account()]
		[PXDefault(typeof(Search2<APTaxTran.accountID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? AdjdWhTaxAcctID
		{
			get
			{
				return this._AdjdWhTaxAcctID;
			}
			set
			{
				this._AdjdWhTaxAcctID = value;
			}
		}
		#endregion
		#region AdjdWhTaxSubID
		public abstract class adjdWhTaxSubID : PX.Data.BQL.BqlInt.Field<adjdWhTaxSubID> { }
		protected Int32? _AdjdWhTaxSubID;

        /// <summary>
        /// Identifier of the subaccount associated with withholding tax for the adjusted document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[SubAccount()]
		[PXDefault(typeof(Search2<APTaxTran.subID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<APAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<APAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? AdjdWhTaxSubID
		{
			get
			{
				return this._AdjdWhTaxSubID;
			}
			set
			{
				this._AdjdWhTaxSubID = value;
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

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the adjustment.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
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

        /// <summary>
        /// An optional cross rate that can be specified between the currency of the payment and currency of the original document.
        /// </summary>
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

        /// <summary>
        /// The amount of the adjustment before the discount is taken.
        /// (Presented in the currency of the document, see <see cref="APRegister.CuryID"/>)
        /// </summary>
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.docBal), BaseCalc=false)]
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
		#endregion
		#region DocBal
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		protected Decimal? _DocBal;

        /// <summary>
        /// The amount of the adjustment before the discount is taken.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
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
		#endregion
		#region CuryDiscBal
		public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		protected Decimal? _CuryDiscBal;

        /// <summary>
        /// The difference between the cash discount that was available and the actual amount of cash discount taken.
        /// (Presented in the currency of the document, see <see cref="APRegister.CuryID"/>)
        /// </summary>
		[PXCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.discBal), BaseCalc=false)]
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
		#endregion
		#region DiscBal
		public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		protected Decimal? _DiscBal;

        /// <summary>
        /// The difference between the cash discount that was available and the actual amount of cash discount taken.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
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
		#endregion
		#region CuryWhTaxBal
		public abstract class curyWhTaxBal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxBal> { }
		protected Decimal? _CuryWhTaxBal;

        /// <summary>
        /// The difference between the amount of the tax to be withheld and the actual withheld amount (if any withholding taxes are applicable).
        /// (Presented in the currency of the document, see <see cref="APRegister.CuryID"/>)
        /// </summary>
		[PXCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.whTaxBal), BaseCalc = false)]
		[PXUnboundDefault()]
		[PXUIField(DisplayName = "With. Tax Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? CuryWhTaxBal
		{
			get
			{
				return this._CuryWhTaxBal;
			}
			set
			{
				this._CuryWhTaxBal = value;
			}
		}
		#endregion
		#region WhTaxBal
		public abstract class whTaxBal : PX.Data.BQL.BqlDecimal.Field<whTaxBal> { }
		protected Decimal? _WhTaxBal;

        /// <summary>
        /// The difference between the amount of the tax to be withheld and the actual withheld amount (if any withholding taxes are applicable).
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDecimal(4)]
		[PXUnboundDefault()]
		public virtual Decimal? WhTaxBal
		{
			get
			{
				return this._WhTaxBal;
			}
			set
			{
				this._WhTaxBal = value;
			}
		}
		#endregion
		#region VoidAppl
		public abstract class voidAppl : PX.Data.BQL.BqlBool.Field<voidAppl> { }

        /// <summary>
        /// When equal to <c>true</c>, indicates that the payment was voided or cancelled.
        /// </summary>
        /// <value>
        /// Setting this field to <c>true</c> will change the <see cref="AdjgDocType">type of the adjusting document</see> to Void Check (<c>"VCK"</c>)
        /// and set <see cref="Voided"/> to <c>true</c>.
        /// </value>
		[PXBool()]
		[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(adjgDocType))]
			get
			{
				return APPaymentType.VoidAppl(this._AdjgDocType);
			}
			set
			{
				if ((bool) value && !APPaymentType.VoidAppl(AdjgDocType))
				{
					this._AdjgDocType = APPaymentType.GetVoidingAPDocType(AdjgDocType);
					this.Voided = true;
				}
			}
		}
		#endregion
		#region ReverseGainLoss
		public abstract class reverseGainLoss : PX.Data.BQL.BqlBool.Field<reverseGainLoss> { }

        /// <summary>
        /// A read-only field, which when equal to <c>true</c>, indicates that the sign of the <see cref="RGOLAmt">gain and loss</see> is reversed for the adjustment.
        /// </summary>
		[PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
		public virtual Boolean? ReverseGainLoss
		{

            get
            {

				return (AdjgTBSign == -1m);
			}
			set
			{
			}
		}
		#endregion
        #region AdjgBalSign
		public abstract class adjgBalSign : PX.Data.BQL.BqlDecimal.Field<adjgBalSign> { }

        /// <summary>
        /// A read-only field showing the sign of the impact of the adjusting document on the balance.
        /// Depends only on the values of the <see cref="AdjgDocType"/> and <see cref="AdjdDocType"/> fields.
        /// </summary>
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjgBalSign
        {
            get
            {
				return AdjgDocType == APDocType.Check && AdjdDocType == APDocType.DebitAdj || 
					AdjgDocType == APDocType.VoidCheck && AdjdDocType == APDocType.DebitAdj 
					? -1m 
					: 1m;
            }
            set { }
        }
        #endregion
		#region AdjgGLSign
		public abstract class adjgGLSign : PX.Data.BQL.BqlDecimal.Field<adjgGLSign> { }

        /// <summary>
        /// !REV! A read-only field showing the sign of impact of the adjusting document on the General Ledger.
        /// Depends only on the values of the <see cref="AdjgDocType"/> and <see cref="AdjdDocType"/> fields.
        /// </summary>
		[PXDependsOnFields(typeof(adjgDocType), typeof(adjgRefNbr), typeof(adjdDocType), typeof(adjdRefNbr))]
		public virtual decimal? AdjgGLSign
		{
			get
			{
				bool appliedToPrepayment = (this.AdjdDocType == APDocType.Prepayment);
				bool appliedCheck = (this.AdjgDocType == APDocType.Check || this.AdjgDocType == APDocType.VoidCheck);
				bool appliedNotSelfPrepayment = (this.AdjgDocType == APDocType.Prepayment && this.AdjgRefNbr != this.AdjdRefNbr);
				return appliedToPrepayment && (appliedCheck || appliedNotSelfPrepayment)
					? 1m
					: APDocType.SignAmount(this.AdjdDocType);
			}
			set { }
		}
		#endregion
        #region AdjgTBSign
		public abstract class adjgTBSign : PX.Data.BQL.BqlDecimal.Field<adjgTBSign> { }

        /// <summary>
        /// A read-only field showing the sign of impact of the adjusting document on the trial balance.
        /// Depends only on the values of the <see cref="AdjgDocType"/> and <see cref="AdjdDocType"/> fields.
        /// </summary>
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjgTBSign
        {
            get
            {
				return this.IsSelfAdjustment()
					? -1m * APDocType.SignBalance(AdjgDocType)
					: AdjdDocType == APDocType.Prepayment
						&& (AdjgDocType == APDocType.Check || AdjgDocType == APDocType.VoidCheck || AdjgDocType == APDocType.Prepayment)
						? 1m 
						: APDocType.SignBalance(AdjdDocType);
            }
            set { }
        }
        #endregion
        #region AdjdTBSign
		public abstract class adjdTBSign : PX.Data.BQL.BqlDecimal.Field<adjdTBSign> { }

        /// <summary>
        /// A read-only field showing the sign of impact of the adjusted document on the trial balance.
        /// Depends only on the values of the <see cref="AdjgDocType"/> and <see cref="AdjdDocType"/> fields.
        /// </summary>
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjdTBSign
        {
            get
            {
				return this.IsSelfAdjustment()
					? 0m
					: AdjgDocType == APDocType.Check && AdjdDocType == APDocType.DebitAdj || 
						AdjgDocType == APDocType.VoidCheck && AdjdDocType == APDocType.DebitAdj 
						? 1m 
						: APDocType.SignBalance(AdjgDocType);
            }
            set { }
        }
        #endregion
		#region SignedRGOLAmt

        /// <summary>
        /// A read-only field showing the Realized Gain and Loss amount associated with the adjustment with proper sign.
        /// Depends on the <see cref="RGOLAmt"/> and <see cref="ReverseGainLoss"/> fields.
        /// </summary>
		public virtual Decimal? SignedRGOLAmt
		{
			[PXDependsOnFields(typeof(reverseGainLoss),typeof(rGOLAmt))]
			get
			{
				return ((bool)this.ReverseGainLoss ? -this._RGOLAmt : this._RGOLAmt);
			}			
		}
		#endregion

        #region AdjdBalSign
        public abstract class adjdBalSign : PX.Data.BQL.BqlDecimal.Field<adjdBalSign> { }

        /// <summary>
        /// A read-only field showing the sign of the impact of the adjusted document on the balance.
        /// Depends only on the values of the <see cref="AdjgDocType"/> and <see cref="AdjdDocType"/> fields.
        /// </summary>
        [PXDependsOnFields(typeof(adjgDocType), typeof(adjdDocType))]
        public virtual decimal? AdjdBalSign
        {
            get
            {                                
                return this.AdjgDocType == APDocType.Check && this.AdjdDocType == APDocType.DebitAdj || this.AdjgDocType == APDocType.VoidCheck && this.AdjdDocType == APDocType.DebitAdj ? -1m : 1m;
            }
            set { }
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

		#region AdjType
		public abstract class adjType : PX.Data.BQL.BqlString.Field<adjType> { }

		[PXString(1, IsFixed = true)]
		[ARAdjust.adjType.List]
		public virtual string AdjType { get; set; }
		#endregion

		#region IsMigratedRecord
		public abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the record has been created 
		/// in migration mode without affecting GL module.
		/// </summary>
		[MigratedRecord(typeof(APSetup.migrationMode))]
		public virtual bool? IsMigratedRecord
		{
			get;
			set;
		}
		#endregion
		#region IsInitialApplication
		public abstract class isInitialApplication : PX.Data.BQL.BqlBool.Field<isInitialApplication> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that this is the initial application
		/// created for a migrated document to affect all needed balances.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsInitialApplication
		{
			get;
			set;
		}
		#endregion
		#region PendingPPD
		public abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }


		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.PendingPPD, Enabled = false, Visible = false)]
		public virtual bool? PendingPPD
		{
			get; set;
		}
		#endregion
		#region PPDDebitAdjRefNbr
		public abstract class pPDDebitAdjRefNbr : PX.Data.BQL.BqlString.Field<pPDDebitAdjRefNbr> { }

		/// <summary>
		/// The reference number of the debit adjustment, generated on the "Generate VAT Debit Adjustments" (AP.50.45.00) form.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="APInvoice.RefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<APDocType.debitAdj>,
			And<APInvoice.pendingPPD, Equal<True>>>>))]
		[PXUIField(DisplayName = "VAT Debit Adj.", Enabled = false, Visible = false)]
		public virtual string PPDDebitAdjRefNbr
		{
			get; set;
		}
		#endregion
		#region AdjdHasPPDTaxes
		public abstract class adjdHasPPDTaxes : PX.Data.BQL.BqlBool.Field<adjdHasPPDTaxes> { }


		[PXDBBool]
		[PXDefault]
		[PXFormula(typeof(Switch<Case<Where<APAdjust.adjdDocType, IsNotNull, And<APAdjust.adjdRefNbr, IsNotNull>>, 
			IsNull<Selector<APAdjust.adjdRefNbr, APInvoice.hasPPDTaxes>, False>>,
			False>))]
		public virtual bool? AdjdHasPPDTaxes
		{
			get; set;
		}
		#endregion
		#region AdjPPDAmt
		public abstract class adjPPDAmt : PX.Data.BQL.BqlDecimal.Field<adjPPDAmt> { }


		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AdjPPDAmt
		{
			get; set;
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
		#region CuryAdjgPPDAmt
		public abstract class curyAdjgPPDAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgPPDAmt> { }


		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjPPDAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.CashDiscountTaken, Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryAdjgPPDAmt
		{
			get; set;
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

		#region DisplayDocType
		public abstract class displayDocType : PX.Data.BQL.BqlString.Field<displayDocType> { }
		[PXString(3, IsFixed = true, InputMask = "")]
		[PXUIField(DisplayName = "Doc. Type")]
		[APDocType.List]
		public virtual string DisplayDocType { get; set; }
		#endregion

		#region DisplayRefNbr
		public abstract class displayRefNbr : PX.Data.BQL.BqlString.Field<displayRefNbr> { }
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXVirtualSelector(typeof(Search<Standalone.APRegister.refNbr, Where<Standalone.APRegister.docType, Equal<Current<APAdjust.displayDocType>>>>))]
		public virtual string DisplayRefNbr { get; set; }
		#endregion

		#region DisplayDocDate
		public abstract class displayDocDate : PX.Data.BQL.BqlDateTime.Field<displayDocDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Date", Enabled = false)]
		public virtual DateTime? DisplayDocDate { get; set; }
		#endregion

		#region DisplayDocDesc
		public abstract class displayDocDesc : PX.Data.BQL.BqlString.Field<displayDocDesc> { }
		[PXString(150, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual string DisplayDocDesc { get; set; }
		#endregion

		#region DisplayCuryID
		public abstract class displayCuryID : PX.Data.BQL.BqlString.Field<displayCuryID> { }
		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string DisplayCuryID { get; set; }
		#endregion

		#region DisplayCuryInfoID
		public abstract class displayCuryInfoID : PX.Data.BQL.BqlLong.Field<displayCuryInfoID> { }
		[PXLong]
	    public virtual long? DisplayCuryInfoID
	    {
		    get;
			set;
	    }
		#endregion

		#region DisplayFinPeriodID
		public abstract class displayFinPeriodID : PX.Data.BQL.BqlString.Field<displayFinPeriodID> { }
		[PXString(FinPeriodUtils.FULL_LENGHT, IsFixed = true)]
		[FinPeriodIDFormatting]
		[PXSelector(typeof(Search<MasterFinPeriod.finPeriodID>))]
		[PXUIField(DisplayName = "Post Period", Enabled = false)]
		public virtual string DisplayFinPeriodID { get; set; }
		#endregion

		#region DisplayStatus
		public abstract class displayStatus : PX.Data.BQL.BqlString.Field<displayStatus> { }
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[APDocStatus.List]
		public virtual string DisplayStatus { get; set; }
		#endregion

		#region DisplayCuryAmt
		public abstract class displayCuryAmt : PX.Data.BQL.BqlDecimal.Field<displayCuryAmt> { }
		[PXCurrency(typeof(APAdjust.displayCuryInfoID), typeof(APAdjust.adjAmt), BaseCalc = false)]
		[PXUIField(DisplayName = "Amount Paid", Enabled = false)]
	    public virtual decimal? DisplayCuryAmt
	    {
			get;
			set;
	    }
		#endregion

		#region DisplayCuryDiscAmt
		public abstract class displayCuryDiscAmt : PX.Data.BQL.BqlDecimal.Field<displayCuryDiscAmt> { }
		[PXCurrency(typeof(APAdjust.displayCuryInfoID), typeof(APAdjust.adjDiscAmt), BaseCalc = false)]
		[PXUIField(DisplayName = "Cash Discount Taken", Enabled = false)]
		public virtual decimal? DisplayCuryDiscAmt
	    {
		    get;
			set;
	    }
		#endregion

		#region DisplayCuryPPDAmt
		public abstract class displayCuryPPDAmt : PX.Data.BQL.BqlDecimal.Field<displayCuryPPDAmt> { }

		[PXCurrency(typeof(APAdjust.displayCuryInfoID), typeof(APAdjust.adjPPDAmt), BaseCalc = false)]
		[PXUIField(DisplayName = Messages.CashDiscountTaken)]
		public virtual decimal? DisplayCuryPPDAmt
		{
			get;
			set;
		}
		#endregion
		#region DisplayCuryWhTaxAmt
		public abstract class displayCuryWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<displayCuryWhTaxAmt> { }
		[PXCurrency(typeof(APAdjust.displayCuryInfoID), typeof(APAdjust.adjWhTaxAmt), BaseCalc = false)]
		[PXUIField(DisplayName = "With. Tax", Enabled = false)]
	    public virtual decimal? DisplayCuryWhTaxAmt
	    {
			get;
			set;
	    }
		#endregion		
		#endregion

		#region Explicit Interface Implementations
		string IDocumentAdjustment.Module => BatchModule.AP;
		decimal? IAdjustmentAmount.AdjThirdAmount
		{
			get { return AdjWhTaxAmt; }
			set { AdjWhTaxAmt = value; }
		}
		decimal? IAdjustmentAmount.CuryAdjgThirdAmount
		{
			get { return CuryAdjgWhTaxAmt; }
			set { CuryAdjgWhTaxAmt = value; }
		}
		decimal? IAdjustmentAmount.CuryAdjdThirdAmount
		{
			get { return CuryAdjdWhTaxAmt; }
			set { CuryAdjdWhTaxAmt = value; }
		}

		#region IAdjustmentStub
		public bool Persistent => true;
		public decimal? CuryOutstandingBalance => null;
		public DateTime? OutstandingBalanceDate => null;
		public bool? IsRequest => null;
		#endregion
		#endregion
	}

    [Serializable]
    [PXHidden]
    public partial class APAdjust2 : APAdjust
    {
        public new abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
        public new abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
        public new abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
        public new abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		public new abstract class adjdLineNbr : PX.Data.BQL.BqlInt.Field<adjdLineNbr> { }
		public new abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		public new abstract class adjgFinPeriodID : PX.Data.BQL.BqlString.Field<adjgFinPeriodID> { }
		public new abstract class adjgTranPeriodID : PX.Data.BQL.BqlString.Field<adjgTranPeriodID> { }
		public new abstract class voidAdjNbr : PX.Data.BQL.BqlInt.Field<voidAdjNbr> { }
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
    }
}

namespace PX.Objects.AP.Standalone
{
	[PXHidden]
	[Serializable]
	public partial class APRegister : AP.APRegister
	{
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region ClosedFinPeriodID
		public new abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		#endregion

		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		#region APAccountID
		public new abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }

		/// <summary>
		/// This property was copied from the <see cref="APRegister">APRegister</see> class. It is overriden only for correct order of the fields 
		/// in _ClassFields list of PXCache<TNode> class. The order of the fields is important for proper Restriction Groups operation.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>

		[PXDBInt]
		public override Int32? APAccountID
		{
			get; set;
		}
		#endregion
		#region APSubID
		public new abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }

		/// <summary>
		/// This property was copied from the <see cref="APRegister">APRegister</see> class. It is overriden only for correct order of the fields 
		/// in _ClassFields list of PXCache<TNode> class. The order of the fields is important for proper Restriction Groups operation.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>

		[PXDBInt]
		public override Int32? APSubID
		{
			get; set;
		}
		#endregion
	}

	[PXHidden]
	[Serializable]
	public partial class APRegister2 : AP.APRegister
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
		#region ClosedTranPeriodID
		public new abstract class closedTranPeriodID : PX.Data.BQL.BqlString.Field<closedTranPeriodID> { }
		#endregion
	}
	[PXHidden]
	[Serializable]
	public class APAdjust : IBqlTable
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

		public virtual int? AdjdLineNbr { get; set; }
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