using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA.BankStatementProtoHelpers;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	[Serializable]
    [PXCacheName(Messages.BankTranAdjustment)]
	public partial class CABankTranAdjustment : IBqlTable, ICADocAdjust, IAdjustment
	{
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }
		
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CABankTran.tranID))]
		public virtual int? TranID
		{
			get;
			set;
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		protected bool? _Selected = false;
		[PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region AdjdModule
		public abstract class adjdModule : PX.Data.BQL.BqlString.Field<adjdModule> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		public virtual string AdjdModule
		{
			get;
			set;
		}
		#endregion
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		[PXDBString(3, IsFixed = true, InputMask = "")]
		[PXDefault(APDocType.Invoice)]
		[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.AdjdList]
		public virtual string AdjdDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[PXInvoiceSelector(typeof(CABankTran.origModule))]
		public virtual string AdjdRefNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdBranchID
		public abstract class adjdBranchID : PX.Data.BQL.BqlInt.Field<adjdBranchID> { }
		[Branch(useDefaulting: false, Enabled = false, Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? AdjdBranchID
        {
            get;
            set;
        }
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(CABankTran.lineCntr))]
		[PXParent(typeof(Select<CABankTran, Where<CABankTran.tranID, Equal<Current<CABankTranAdjustment.tranID>>>>))]
		[PXDefault(TypeCode.Int32, "0")]
		public virtual int? AdjNbr
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjdAmt
		public abstract class curyAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryAdjdAmt
		{
			get;
			set;
		}
        #endregion

		#region SeparateCheck
		public abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		[PXBool]
		[PXUIField(DisplayName = "Pay Separately", Visibility = PXUIVisibility.Visible)]
		public virtual bool? SeparateCheck
		{
			get;
			set;
		}
		#endregion
		#region AdjdCuryInfoID
		public abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }

		[PXDBLong]
		[PXDefault]
		[CurrencyInfo(ModuleCode = BatchModule.AP, CuryIDField = "AdjdCuryID", Enabled = false)]
		public virtual long? AdjdCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region PrintAdjdDocType
		public abstract class printAdjdDocType : PX.Data.BQL.BqlString.Field<printAdjdDocType> { }
		[PXString(3, IsFixed = true)]
		[APDocType.PrintList]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual string PrintAdjdDocType
		{
			get
			{
				return this.AdjdDocType;
			}

			set
			{
			}
		}
		#endregion
		#region StubNbr
		public abstract class stubNbr : PX.Data.BQL.BqlString.Field<stubNbr> { }

		[PXDBString(40, IsUnicode = true)]
		public virtual string StubNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjBatchNbr
		public abstract class adjBatchNbr : PX.Data.BQL.BqlString.Field<adjBatchNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual string AdjBatchNbr
		{
			get;
			set;
		}
		#endregion
		#region VoidAdjNbr
		public abstract class voidAdjNbr : PX.Data.BQL.BqlInt.Field<voidAdjNbr> { }

		[PXDBInt]
		public virtual int? VoidAdjNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdOrigCuryInfoID
		public abstract class adjdOrigCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdOrigCuryInfoID> { }

		[PXDBLong]
		[PXDefault]
		[CurrencyInfo(ModuleCode = BatchModule.AP, CuryIDField = "AdjdOrigCuryID")]
		public virtual long? AdjdOrigCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region AdjgCuryInfoID
		public abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID> { }

		[PXDBLong]
		[CurrencyInfo(CuryIDField = "AdjgCuryID")]
		public virtual long? AdjgCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region AdjgDocDate
		public abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate> { }

		[PXDBDate]
		[PXDBDefault(typeof(CABankTran.tranDate))]
		public virtual DateTime? AdjgDocDate
		{
			get;
			set;
		}
		#endregion
		#region AdjgFinPeriodID
		public abstract class adjgFinPeriodID : PX.Data.BQL.BqlString.Field<adjgFinPeriodID> { }
		[FinPeriodID]
		[PXUIField(DisplayName = "Application Period", Enabled = false)]
		public virtual string AdjgFinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region AdjgTranPeriodID
		public abstract class adjgTranPeriodID : PX.Data.BQL.BqlString.Field<adjgTranPeriodID> { }

		[FinPeriodID]
		public virtual string AdjgTranPeriodID
		{
			get;
			set;
		}
		#endregion
		#region AdjdDocDate
		public abstract class adjdDocDate : PX.Data.BQL.BqlDateTime.Field<adjdDocDate> { }

		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? AdjdDocDate
		{
			get;
			set;
		}
		#endregion
		#region AdjdFinPeriodID
		public abstract class adjdFinPeriodID : PX.Data.BQL.BqlString.Field<adjdFinPeriodID> { }

		[FinPeriodID(
		    sourceType: typeof(CABankTranAdjustment.adjdDocDate),
		    branchSourceType: typeof(CABankTranAdjustment.adjdBranchID),
		    masterFinPeriodIDType: typeof(CABankTranAdjustment.adjdTranPeriodID))]
		[PXUIField(DisplayName = "Post Period", Enabled = false, Visible = false)]
		public virtual string AdjdFinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region AdjdClosedFinPeriodID
		public abstract class adjdClosedFinPeriodID : PX.Data.BQL.BqlString.Field<adjdClosedFinPeriodID> { }

		[PXDBScalar(typeof(Search<APRegister.closedFinPeriodID, Where<APRegister.docType, Equal<CABankTranAdjustment.adjdDocType>, And<APRegister.refNbr, Equal<CABankTranAdjustment.adjdRefNbr>>>>))]
		[PXString]
		public virtual string AdjdClosedFinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region AdjdTranPeriodID
		public abstract class adjdTranPeriodID : PX.Data.BQL.BqlString.Field<adjdTranPeriodID> { }

		[PeriodID]
		public virtual string AdjdTranPeriodID
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjgDiscAmt
		public abstract class curyAdjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgDiscAmt> { }

		[PXDBCurrency(typeof(CABankTranAdjustment.adjgCuryInfoID), typeof(CABankTranAdjustment.adjDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount Taken", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual decimal? CuryAdjgDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjgWhTaxAmt
		public abstract class curyAdjgWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgWhTaxAmt> { }

		[PXDBCurrency(typeof(CABankTranAdjustment.adjgCuryInfoID), typeof(CABankTranAdjustment.adjWhTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "With. Tax", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual decimal? CuryAdjgWhTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }

		[PXDBCurrency(typeof(CABankTranAdjustment.adjgCuryInfoID), typeof(CABankTranAdjustment.adjAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		[PXUnboundFormula(typeof(Mult<CABankTranAdjustment.adjgBalSign, CABankTranAdjustment.curyAdjgAmt>), typeof(SumCalc<CABankTran.curyApplAmt>))]
		public virtual decimal? CuryAdjgAmt
		{
			get;
			set;
		}

		public virtual decimal? CuryAdjgAmount
		{
			get
			{
				return CuryAdjgAmt;
			}

			set
			{
				CuryAdjgAmt = value;
			}
		}
		#endregion
		#region AdjDiscAmt
		public abstract class adjDiscAmt : PX.Data.BQL.BqlDecimal.Field<adjDiscAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AdjDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjdDiscAmt
		public abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdDiscAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryAdjdDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjWhTaxAmt
		public abstract class adjWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<adjWhTaxAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AdjWhTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjdWhTaxAmt
		public abstract class curyAdjdWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdWhTaxAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryAdjdWhTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjAmt
		public abstract class adjAmt : PX.Data.BQL.BqlDecimal.Field<adjAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AdjAmt
		{
			get;
			set;
		}
		#endregion
		#region RGOLAmt
		public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RGOLAmt
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Voided
		{
			get;
			set;
		}
		#endregion
		#region AdjdAPAcct
		public abstract class adjdAPAcct : PX.Data.BQL.BqlInt.Field<adjdAPAcct> { }

		[Account(SuppressCurrencyValidation = true)]
		public virtual int? AdjdAPAcct
		{
			get;
			set;
		}
		#endregion
		#region AdjdAPSub
		public abstract class adjdAPSub : PX.Data.BQL.BqlInt.Field<adjdAPSub> { }

		[SubAccount]
		public virtual int? AdjdAPSub
		{
			get;
			set;
		}
		#endregion
		#region AdjdARAcct
		public abstract class adjdARAcct : PX.Data.BQL.BqlInt.Field<adjdARAcct> { }
		[Account]
		public virtual int? AdjdARAcct
		{
			get;
			set;
		}
		#endregion
		#region AdjdARSub
		public abstract class adjdARSub : PX.Data.BQL.BqlInt.Field<adjdARSub> { }

		[SubAccount]
		public virtual int? AdjdARSub
		{
			get;
			set;
		}
		#endregion
		#region AdjdWhTaxAcctID
		public abstract class adjdWhTaxAcctID : PX.Data.BQL.BqlInt.Field<adjdWhTaxAcctID> { }

		[Account]
		[PXDefault(typeof(Search2<APTaxTran.accountID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.tranType, Equal<Current<CABankTranAdjustment.adjdDocType>>, And<APTaxTran.refNbr, Equal<Current<CABankTranAdjustment.adjdRefNbr>>, And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? AdjdWhTaxAcctID
		{
			get;
			set;
		}
		#endregion
		#region AdjdWhTaxSubID
		public abstract class adjdWhTaxSubID : PX.Data.BQL.BqlInt.Field<adjdWhTaxSubID> { }

		[SubAccount]
		[PXDefault(typeof(Search2<APTaxTran.subID, InnerJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, 
			Where<APTaxTran.tranType, Equal<Current<CABankTranAdjustment.adjdDocType>>, 
				And<APTaxTran.refNbr, Equal<Current<CABankTranAdjustment.adjdRefNbr>>, 
				And<Tax.taxType, Equal<CSTaxType.withholding>>>>, OrderBy<Asc<APTaxTran.taxID>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? AdjdWhTaxSubID
		{
			get;
			set;
		}
		#endregion
		
		#region AdjdCuryRate
		public abstract class adjdCuryRate : PX.Data.BQL.BqlDecimal.Field<adjdCuryRate> { }
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(8)]
		[PXUIField(DisplayName = "Cross Rate", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual decimal? AdjdCuryRate
		{
			get;
			set;
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }

		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? CuryDocBal
		{
			get;
			set;
		}
		#endregion
		#region DocBal
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }

		[PXDecimal(4)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DocBal
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscBal
		public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }

		[PXCury(typeof(CABankTran.curyID))]
		[PXUnboundDefault]
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.Visible, Enabled = false, Visible = false)]
		public virtual decimal? CuryDiscBal
		{
			get;
			set;
		}
		#endregion
		#region DiscBal
		public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }

		[PXDecimal(4)]
		[PXUnboundDefault]
		public virtual decimal? DiscBal
		{
			get;
			set;
		}
		#endregion
		#region CuryWhTaxBal
		public abstract class curyWhTaxBal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxBal> { }

		[PXCury(typeof(CABankTran.curyID))]
		[PXUnboundDefault]
		[PXUIField(DisplayName = "With. Tax Balance", Visibility = PXUIVisibility.Visible, Enabled = false, Visible = false)]
		public virtual decimal? CuryWhTaxBal
		{
			get;
			set;
		}
		#endregion
		#region WhTaxBal
		public abstract class whTaxBal : PX.Data.BQL.BqlDecimal.Field<whTaxBal> { }
		[PXDecimal(4)]
		[PXUnboundDefault]
		public virtual decimal? WhTaxBal
		{
			get;
			set;
		}
		#endregion
		#region WriteOffReasonCode
		public abstract class writeOffReasonCode : PX.Data.BQL.BqlString.Field<writeOffReasonCode> { }

		[PXFormula(typeof(Switch<Case<Where<CABankTranAdjustment.adjdDocType, NotEqual<AR.ARDocType.creditMemo>>, Current<AR.ARSetup.balanceWriteOff>>>))]
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where2<Where<ReasonCode.usage, Equal<ReasonCodeUsages.creditWriteOff>, And<Current<CABankTranAdjustment.adjdDocType>, Equal<AR.ARDocType.creditMemo>>>,
			Or<Where<ReasonCode.usage, Equal<ReasonCodeUsages.balanceWriteOff>, And<Current<CABankTranAdjustment.adjdDocType>, NotEqual<AR.ARDocType.creditMemo>>>>>>))]
		[PXUIField(DisplayName = "Write-Off Reason Code", Visibility = PXUIVisibility.Visible)]
		public virtual string WriteOffReasonCode
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjgWOAmt
		public abstract class curyAdjgWOAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgWOAmt> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Balance Write-Off", Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(SumCalc<CABankTran.curyWOAmt>))]
		public virtual decimal? CuryAdjgWOAmt
		{
			get
			{
				return CuryAdjgWhTaxAmt;
			}

			set
			{
				CuryAdjgWhTaxAmt = value;
			}
		}
		#endregion
		#region AdjgBalSign
		public abstract class adjgBalSign : PX.Data.BQL.BqlDecimal.Field<adjgBalSign> { }

        protected decimal? _AdjgBalSign;

        [PXDecimal(4)]
		public virtual decimal? AdjgBalSign
		{
			get
			{
				return _AdjgBalSign ?? decimal.One;
			}

			set
			{
				_AdjgBalSign = value;
			}
		}
		#endregion
		#region ReverseGainLoss
		public bool? ReverseGainLoss
		{
			get { return false; }
			set { }
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
