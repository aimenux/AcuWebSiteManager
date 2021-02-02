using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.AP;


namespace PX.Objects.GL
{  
	[Obsolete(PX.Objects.Common.Messages.ClassIsObsoleteAndWillBeRemoved2019R2)]
    [Serializable]
    [PXHidden]
	public partial class GLDocAdjust : PX.Data.IBqlTable, IAdjustment
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
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
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(Batch))]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.Visible, Visible = false)]
		[BatchModule.List()]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(Batch))]
		[PXParent(typeof(Select<Batch, Where<Batch.module, Equal<Current<GLTranDoc.module>>, And<Batch.batchNbr, Equal<Current<GLTranDoc.batchNbr>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region AdjgLineNbr
		public abstract class adjgLineNbr : PX.Data.BQL.BqlInt.Field<adjgLineNbr> { }
		protected Int32? _AdjgLineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXLineNbr(typeof(Batch.lineCntr))]
		public virtual Int32? AdjgLineNbr
		{
			get
			{
				return this._AdjgLineNbr;
			}
			set
			{
				this._AdjgLineNbr = value;
			}
		}
		#endregion
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		protected String _AdjgDocType;
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDBDefault(typeof(GLTranDoc.tranType))]
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
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		protected String _AdjgRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(APPayment.refNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<GLTranDoc, Where<GLTranDoc.tranType, Equal<Current<GLDocAdjust.adjgDocType>>, And<APPayment.refNbr, Equal<Current<GLDocAdjust.adjgRefNbr>>, And<APPayment.adjCntr, Equal<Current<GLDocAdjust.adjNbr>>>>>>))]
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
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[AP.Vendor(Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDBDefault(typeof(GLTranDoc.bAccountID))]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region AdjdCuryInfoID
		public abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }
		protected Int64? _AdjdCuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = BatchModule.AP, CuryIDField = "AdjdCuryID")]
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
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault(APDocType.Invoice)]
		[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible)]
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
        [Serializable]
		public partial class APRegister : AP.APRegister
		{
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
		}

		[PXHidden()]
		[PXProjection(typeof(Select2<APRegister, LeftJoin<AP.Standalone.APInvoice, On<AP.Standalone.APInvoice.docType, Equal<APRegister.docType>, And<AP.Standalone.APInvoice.refNbr, Equal<APRegister.refNbr>>>>>))]
        [Serializable]
		public partial class APInvoice : APRegister
		{
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region CustomerID
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
			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			protected DateTime? _DueDate;
			[PXDBDate(BqlField = typeof(AP.Standalone.APInvoice.dueDate))]
			public virtual DateTime? DueDate
			{
				get
				{
					return this._DueDate;
				}
				set
				{
					this._DueDate = value;
				}
			}
			#endregion
			#region InvoiceNbr
			public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
			protected String _InvoiceNbr;
			[PXDBString(40, IsUnicode = true, BqlField = typeof(AP.Standalone.APInvoice.invoiceNbr))]
			public virtual String InvoiceNbr
			{
				get
				{
					return this._InvoiceNbr;
				}
				set
				{
					this._InvoiceNbr = value;
				}
			}
			#endregion
		}

		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		protected String _AdjdRefNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.AdjdRefNbr(typeof(Search2<APInvoice.refNbr, LeftJoin<GLDocAdjust, On<GLDocAdjust.adjdDocType, Equal<APInvoice.docType>, And<GLDocAdjust.adjdRefNbr, Equal<APInvoice.refNbr>, And<GLDocAdjust.released, Equal<False>, And<Where<GLDocAdjust.adjgDocType, NotEqual<Current<APPayment.docType>>, Or<GLDocAdjust.adjgRefNbr, NotEqual<Current<APPayment.refNbr>>>>>>>>, LeftJoin<APPayment, On<APPayment.docType, Equal<APInvoice.docType>, And<APPayment.refNbr, Equal<APInvoice.refNbr>, And<Where<APPayment.docType, Equal<APDocType.prepayment>, Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>, Where<APInvoice.vendorID, Equal<Optional<APPayment.vendorID>>, And<APInvoice.docType, Equal<Optional<GLDocAdjust.adjdDocType>>, And2<Where<APInvoice.released, Equal<True>,Or<APInvoice.prebooked,Equal<True>>>, And<APInvoice.openDoc, Equal<True>, And<GLDocAdjust.adjgRefNbr, IsNull, And2<Where<APPayment.refNbr, IsNull, And<Current<APPayment.docType>, NotEqual<APDocType.refund>, Or<APPayment.refNbr, IsNotNull, And<Current<APPayment.docType>, Equal<APDocType.refund>>>>>, And<APInvoice.docDate, LessEqual<Current<APPayment.adjDate>>, And<APInvoice.finPeriodID, LessEqual<Current<APPayment.adjFinPeriodID>>>>>>>>>>>), Filterable = true)]
		public virtual String AdjdRefNbr
		{
			get
			{
				return this._AdjdRefNbr;
			}
			set
			{
				this._AdjdRefNbr = value;
			}
		}
		#endregion
		#region AdjdBranchID
		public abstract class adjdBranchID : PX.Data.BQL.BqlInt.Field<adjdBranchID> { }
		protected Int32? _AdjdBranchID;
		[Branch(useDefaulting: false)]
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
		[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region AdjdOrigCuryInfoID
		public abstract class adjdOrigCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdOrigCuryInfoID> { }
		protected Int64? _AdjdOrigCuryInfoID;
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
		[PXDBDate()]
		[PXDBDefault(typeof(APPayment.adjDate))]
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
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(GLTranDoc.finPeriodID))]
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
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(GLTranDoc.tranPeriodID))]
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
		    sourceType: typeof(GLDocAdjust.adjdDocDate),
		    branchSourceType: typeof(GLDocAdjust.adjdBranchID),
		    masterFinPeriodIDType: typeof(GLDocAdjust.adjdTranPeriodID))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region AdjdClosedFinPeriodID
		public abstract class adjdClosedFinPeriodID : PX.Data.BQL.BqlString.Field<adjdClosedFinPeriodID> { }
		protected String _AdjdClosedFinPeriodID;
		[PXDBScalar(typeof(Search<APRegister.closedFinPeriodID, Where<APRegister.docType, Equal<GLDocAdjust.adjdDocType>, And<APRegister.refNbr, Equal<GLDocAdjust.adjdRefNbr>>>>))]
		[PXString()]
		public virtual String AdjdClosedFinPeriodID
		{
			get
			{
				return this._AdjdClosedFinPeriodID;
			}
			set
			{
				this._AdjdClosedFinPeriodID = value;
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
		[PXDBCurrency(typeof(GLDocAdjust.adjgCuryInfoID), typeof(GLDocAdjust.adjDiscAmt))]
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
		[PXDBCurrency(typeof(GLDocAdjust.adjgCuryInfoID), typeof(GLDocAdjust.adjWhTaxAmt))]
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
		[PXDBCurrency(typeof(GLDocAdjust.adjgCuryInfoID), typeof(GLDocAdjust.adjAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(SumCalc<APPayment.curyApplAmt>))]
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
		#region CuryAdjdDiscAmt
		public abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdDiscAmt> { }
		protected Decimal? _CuryAdjdDiscAmt;
		[PXDBDecimal(4)]
		//[PXDBCurrency(typeof(GLDocAdjust.adjdCuryInfoID), typeof(GLDocAdjust.adjDiscAmt))]
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
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDBDecimal(4)]
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
		//[PXDBCurrency(typeof(GLDocAdjust.adjdCuryInfoID), typeof(GLDocAdjust.adjAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXBool()]
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
		#region AdjdAcct
		public abstract class adjdAPAcct : PX.Data.BQL.BqlInt.Field<adjdAPAcct> { }
		protected Int32? _AdjdAPAcct;
		[Account()]
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
		#region AdjdSub
		public abstract class adjdSub : PX.Data.BQL.BqlInt.Field<adjdSub> { }
		protected Int32? _AdjdSub;
		[SubAccount()]
		[PXDefault()]
		public virtual Int32? AdjdSub
		{
			get
			{
				return this._AdjdSub;
			}
			set
			{
				this._AdjdSub = value;
			}
		}
		#endregion
		#region AdjdWhTaxAcctID
		public abstract class adjdWhTaxAcctID : PX.Data.BQL.BqlInt.Field<adjdWhTaxAcctID> { }
		protected Int32? _AdjdWhTaxAcctID;
		[Account()]
		[PXDefault(typeof(Search2<APTaxTran.accountID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<GLDocAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<GLDocAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[SubAccount()]
		[PXDefault(typeof(Search2<APTaxTran.subID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<GLDocAdjust.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<GLDocAdjust.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXCurrency(typeof(GLDocAdjust.adjgCuryInfoID), typeof(GLDocAdjust.docBal), BaseCalc = false)]
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
		[PXCurrency(typeof(GLDocAdjust.adjgCuryInfoID), typeof(GLDocAdjust.discBal), BaseCalc = false)]
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
		[PXCurrency(typeof(GLDocAdjust.adjgCuryInfoID), typeof(GLDocAdjust.whTaxBal), BaseCalc = false)]
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
				if ((bool)value)
				{
					this._AdjgDocType = APPaymentType.GetVoidingAPDocType(AdjgDocType) ?? APDocType.VoidCheck;
					this.Voided = true;
				}
			}
		}
		#endregion
		#region ReverseGainLoss
		public abstract class reverseGainLoss : PX.Data.BQL.BqlBool.Field<reverseGainLoss> { }
		[PXBool()]
		public virtual Boolean? ReverseGainLoss
		{
			[PXDependsOnFields(typeof(adjgDocType))]
			get
			{
				return (APPaymentType.DrCr(this._AdjgDocType) == DrCr.Debit);
			}
			set
			{
			}
		}
		#endregion
		#region TakeDiscAlways
		public abstract class takeDiscAlways : PX.Data.BQL.BqlBool.Field<takeDiscAlways> { }
		protected Boolean? _TakeDiscAlways = false;
		[PXBool()]
		public virtual Boolean? TakeDiscAlways
		{
			get
			{
				return this._TakeDiscAlways;
			}
			set
			{
				this._TakeDiscAlways = value;
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
	}

}
