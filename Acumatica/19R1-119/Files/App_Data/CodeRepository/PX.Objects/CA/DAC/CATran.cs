using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP.Standalone;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using APPayment = PX.Objects.AP.APPayment;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;

namespace PX.Objects.CA
{	
	[Serializable]
	[PXPrimaryGraph(new Type[] {
						typeof(CATranEntry),
						typeof(CashTransferEntry),
						typeof(CADepositEntry),
						typeof(AR.ARCashSaleEntry),
						typeof(AR.ARPaymentEntry),
						typeof(AP.APQuickCheckEntry),
						typeof(AP.APPaymentEntry),
						typeof(CABatchEntry),
						typeof(JournalEntry)
						},
					new Type[] {
						typeof(Select<CAAdj, Where<CAAdj.tranID, Equal<Current<CATran.tranID>>, And<CAAdj.adjTranType, NotEqual<CATranType.cATransferExp>>>>),
						typeof(Select<CATransfer, Where<CATransfer.tranIDIn, Equal<Current<CATran.tranID>>,
								Or<CATransfer.tranIDOut, Equal<Current<CATran.tranID>>,
								Or<Where<CATransfer.transferNbr, Equal<Current<CATran.origRefNbr>>, And<Current<CATran.origTranType>, Equal<CATranType.cATransferExp>>>>>>>),
						typeof(Select<CADeposit, Where<CADeposit.tranType, Equal<Current<CATran.origTranType>>,
								And<CADeposit.refNbr, Equal<Current<CATran.origRefNbr>>>>>),
						typeof(Select<ARCashSale, Where<ARCashSale.docType, Equal<Current<CATran.origTranType>>,
							And<ARCashSale.refNbr,Equal<Current<CATran.origRefNbr>>>>>),
						typeof(Select<ARPayment,Where<ARPayment.docType, Equal<Current<CATran.origTranType>>,
							And<ARPayment.refNbr, Equal<Current<CATran.origRefNbr>>,
							And<Current<CATran.origModule>, Equal<BatchModule.moduleAR>>>>>),
						typeof(Select<APQuickCheck, Where<APQuickCheck.docType, Equal<Current<CATran.origTranType>>,
							And<APQuickCheck.refNbr, Equal<Current<CATran.origRefNbr>>>>>),
						typeof(Select<APPayment, Where<APPayment.docType, Equal<Current<CATran.origTranType>>,
							And<APPayment.refNbr, Equal<Current<CATran.origRefNbr>>,
							And<Current<CATran.origModule>, Equal<BatchModule.moduleAP>>>>>),
						typeof(Select<CABatch, Where<CABatch.batchNbr, Equal<Current<CATran.origRefNbr>>,
							And<Current<CATran.origModule>, Equal<BatchModule.moduleAP>, And<Current<CATran.origTranType>, Equal<CATranType.cABatch>>>>>),
						typeof(Select<Batch, Where<Batch.module, Equal<Current<CATran.origModule>>,
							And<Batch.batchNbr, Equal<Current<CATran.origRefNbr>>>>>)
					})]
	[PXCacheName(Messages.CATran)]
	public partial class CATran : IBqlTable, GL.DAC.Abstract.IAccountable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion

		#region BegBal
		public abstract class begBal : PX.Data.BQL.BqlDecimal.Field<begBal>
		{
		}
		[PXCury(typeof(CATran.curyID))]
		[PXUIField(Visible = false)]
		public virtual decimal? BegBal
		{
			get;
			set;
		}
		#endregion
		#region EndBal
		public abstract class endBal : PX.Data.BQL.BqlDecimal.Field<endBal>
		{
		}
		[PXCury(typeof(CATran.curyID))]
		[PXUIField(DisplayName = "Ending Balance", Enabled = false)]
		public virtual decimal? EndBal
		{
			get;
			set;
		}
		#endregion
		#region DayDesc
		public abstract class dayDesc : PX.Data.BQL.BqlString.Field<dayDesc>
		{
		}
		[PXString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "Day of Week", Enabled = false)]
		public virtual string DayDesc
		{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule>
		{
		}
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
        [BatchModule.List]
		[PXUIField(DisplayName = "Module")]
		public virtual string OrigModule
			{
			get;
			set;
		}
		#endregion
		#region OrigTranType
		public abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType>
		{
		}
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tran. Type")]
        [CAAPARTranType.ListByModule(typeof(origModule))]
        public virtual string OrigTranType
		{
			get;
			set;
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Orig. Doc. Number")]
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion
		#region IsPaymentChargeTran
		public abstract class isPaymentChargeTran : PX.Data.BQL.BqlBool.Field<isPaymentChargeTran>
		{
		}

		/// <summary>
		/// Indicates that CATran was created by ARPaymentChargeTran or APPaymentChargeTran
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsPaymentChargeTran
		{
			get;
			set;
		}
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>
		{
		}
		[PXDBInt]
		public virtual int? OrigLineNbr
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr>
		{
		}
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Ref.", Visibility = PXUIVisibility.Visible)]
		public virtual string ExtRefNbr
			{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
		{
		}
		[PXDefault]
		[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? CashAccountID
			{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID>
		{
		}
		[PXFormula(typeof(Default<cashAccountID>))]
		[PXDefault(typeof(Search<CashAccount.branchID, Where<CashAccount.cashAccountID, Equal<Current<cashAccountID>>>>))]
		[PXDBInt]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID>
		{
		}
		[PXDBLongIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Document Number")]
		[PXVerifySelector(typeof(Search<CATran.tranID, Where<CATran.cashAccountID, Equal<Current<CARecon.cashAccountID>>, And<Where<CATran.reconNbr, IsNull, Or<CATran.reconNbr, Equal<Current<CARecon.reconNbr>>>>>>>),
			typeof(CATran.extRefNbr),
			typeof(CATran.tranDate),
			typeof(CATran.origModule),
			typeof(CATran.origTranType),
			typeof(CATran.origRefNbr),
			typeof(CATran.status),
			typeof(CATran.curyDebitAmt),
			typeof(CATran.curyCreditAmt),
			typeof(CATran.tranDesc),
			typeof(CATran.cleared),
			typeof(CATran.clearDate), 
			VerifyField = false, DescriptionField = typeof(CATran.extRefNbr))]
		public virtual long? TranID
			{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate>
		{
		}
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Doc. Date")]
		[CADailyAccumulator]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr>
		{
		}
		[PXDefault]
		[PXDBString(1, IsFixed = true)]
		[CADrCr.List]
		[PXUIField(DisplayName = "Disb. / Receipt")]
		public virtual string DrCr
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID>
		{
		}
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccountR.bAccountID),
						SubstituteKey = typeof(BAccountR.acctCD),
					 DescriptionField = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.Visible)]
		public virtual int? ReferenceID
			{
			get;
			set;
		}
		#endregion
		#region ReferenceName
		public abstract class referenceName : PX.Data.BQL.BqlString.Field<referenceName>
		{
		}
		[PXUIField(DisplayName = "Business Name", Visibility = PXUIVisibility.Visible)]
		[PXString(60, IsUnicode = true)]
		public virtual string ReferenceName
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc>
		{
		}
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		[PXFieldDescription]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID>
		{
		}
		[PeriodID]
		public virtual string TranPeriodID
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID>
		{
		}

		[FinPeriodID(
		    branchSourceType: typeof(cashAccountID),
		    branchSourceFormulaType: typeof(Selector<cashAccountID, CashAccount.branchID>),
		    masterFinPeriodIDType: typeof(tranPeriodID))]
		[PXDefault]
		[PXUIField(DisplayName = "Post Period")]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}
		[PXDBLong]
		public virtual long? CuryInfoID
			{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold>
		{
		}
		[PXDBBool]
		[PXDefault(typeof(Search<CASetup.holdEntry>))]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Released
			{
			get;
			set;
		}
		#endregion
		#region Posted
		public abstract class posted : PX.Data.BQL.BqlBool.Field<posted>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Posted
			{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{ 
		}
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[BatchStatus.List]
		public virtual string Status
		{
			[PXDependsOnFields(typeof(posted), typeof(released), typeof(hold))]
			get
			{
				if (this.Posted == true)
				{
					if (this.Released == true)
					{
						return GL.BatchStatus.Posted;
					}
					else
					{
						return GL.BatchStatus.Unposted;
				}
				}
				else if (this.Released == true && this.Posted != true)
				{
					return GL.BatchStatus.Released;
				}
				else if (this.Hold == true)
				{
					return GL.BatchStatus.Hold;
				}
				else
				{
					return GL.BatchStatus.Balanced;
				}
			}

			set
			{ 
			}
		}
		#endregion
		#region Reconciled
		public abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Reconciled")]
		public virtual bool? Reconciled
			{
			get;
			set;
		}
		#endregion
		#region ReconDate
		public abstract class reconDate : PX.Data.BQL.BqlDateTime.Field<reconDate>
		{
		}
		[PXDBDate]
		public virtual DateTime? ReconDate
		{
			get;
			set;
		}
		#endregion
		#region ReconNbr
		public abstract class reconNbr : PX.Data.BQL.BqlString.Field<reconNbr>
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Reconciled Number", Enabled = false)]
		[PXParent(typeof(Select<CARecon, Where<CARecon.reconNbr, Equal<Current<CATran.reconNbr>>>>), UseCurrent = true, LeaveChildren = true)]
		public virtual string ReconNbr
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt>
		{
		}
		[PXDBCurrency(typeof(CATran.curyInfoID), typeof(CATran.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryTranAmt
			{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount")]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr>
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		public virtual string BatchNbr
			{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID>
		{
		}
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CATran.cashAccountID>>>>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
			{
			get;
			set;
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared")]
		public virtual bool? Cleared
			{
			get;
			set;
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate>
		{
		}
		[PXDBDate]
		[PXUIField(DisplayName = "Clear Date")]
		public virtual DateTime? ClearDate
		{
			get;
			set;
		}
		#endregion
		#region CuryDebitAmt
		public abstract class curyDebitAmt : PX.Data.BQL.BqlDecimal.Field<curyDebitAmt>
		{
		}
		[PXDecimal]
		[PXUIField(DisplayName = "Receipt")]
		public virtual decimal? CuryDebitAmt
		{
			[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
			get
			{
					return (this.DrCr == CADrCr.CADebit) ? this.CuryTranAmt : 0m;
				}

			set
			{
			}
		}
		#endregion
		#region CuryCreditAmt
		public abstract class curyCreditAmt : PX.Data.BQL.BqlDecimal.Field<curyCreditAmt>
		{
		}
		[PXDecimal]
		[PXUIField(DisplayName = "Disbursement")]
		public virtual decimal? CuryCreditAmt
		{
			[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
			get
			{
					return (this.DrCr == CADrCr.CACredit) ? -this.CuryTranAmt : 0m;
				}

			set
			{
			}
		}
		#endregion
		#region CuryClearedDebitAmt
		public abstract class curyClearedDebitAmt : PX.Data.BQL.BqlDecimal.Field<curyClearedDebitAmt>
		{
		}
		[PXDecimal]
		[PXUIField(DisplayName = "Receipt")]
		public virtual decimal? CuryClearedDebitAmt
		{
			[PXDependsOnFields(typeof(cleared), typeof(drCr), typeof(curyTranAmt))]
			get
			{
				return (this.Cleared == true && this.DrCr == CADrCr.CADebit) ? this.CuryTranAmt : 0m;
			}

			set
			{
			}
		}
		#endregion
		#region CuryClearedCreditAmt
		public abstract class curyClearedCreditAmt : PX.Data.BQL.BqlDecimal.Field<curyClearedCreditAmt>
		{
		}
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Disbursement")]
		public virtual decimal? CuryClearedCreditAmt
		{
			[PXDependsOnFields(typeof(cleared), typeof(drCr), typeof(curyTranAmt))]
			get
			{
				return (this.Cleared == true && this.DrCr == CADrCr.CACredit) ? -this.CuryTranAmt : 0m;
			}

			set
			{
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
		{
		}
		[PXNote(DescriptionField = typeof(CATran.tranID))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region RefTranAccountID
		public abstract class refTranAccountID : PX.Data.BQL.BqlInt.Field<refTranAccountID>
		{
		}
		[CashAccount(DisplayName = "ChildTran Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? RefTranAccountID
		{
			get;
			set;
		}
		#endregion
		#region RefTranID
		public abstract class refTranID : PX.Data.BQL.BqlLong.Field<refTranID>
		{
		}
		[PXDBLong]		
		public virtual long? RefTranID
			{
			get;
			set;
		}
		#endregion
        #region RefSplitLineNbr
        public abstract class refSplitLineNbr : PX.Data.BQL.BqlInt.Field<refSplitLineNbr>
		{
        }
        [PXDBInt]
        public virtual int? RefSplitLineNbr
            {
			get;
			set;
        }
        #endregion
        #region VoidedTranID
        public abstract class voidedTranID : PX.Data.BQL.BqlLong.Field<voidedTranID>
		{
        }
        [PXDBLong]
        public virtual long? VoidedTranID
            {
			get;
			set;
        }
        #endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion 
        #region Methods

        public static void Redirect(PXCache sender, CATran catran)
		{
            if (catran == null)
                return;

            if (sender != null)
                sender.IsDirty = false;

            Common.RedirectionToOrigDoc.TryRedirect(catran.OrigTranType, catran.OrigRefNbr, catran.OrigModule);
        }
		#endregion
	}
}
