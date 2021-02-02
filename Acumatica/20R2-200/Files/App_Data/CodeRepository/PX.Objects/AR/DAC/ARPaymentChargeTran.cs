using System;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents a charge or a fee applied by the bank for processing 
	/// of an <see cref="ARPayment">Accounts Receivable payment</see>.
	/// Entities of this type can be edited on the Finance Charges tab
	/// of the Payments and Applications (AR302000) form, which corresponds
	/// to the <see cref="ARPaymentEntry"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARPaymentChargeTran)]
	public class ARPaymentChargeTran : AP.IPaymentCharge, PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected string _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARRegister.docType))]
		[PXUIField(DisplayName = "DocType")]
		public virtual string DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected string _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARRegister.refNbr))]
		[PXParent(typeof(Select<ARRegister, Where<ARRegister.docType, Equal<Current<ARPaymentChargeTran.docType>>, And<ARRegister.refNbr, Equal<Current<ARPaymentChargeTran.refNbr>>>>>))]
		public virtual string RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(ARPayment.chargeCntr), DecrementOnDelete = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected int? _CashAccountID;

		[PXDBInt]
		[PXDBDefault(typeof(ARPayment.cashAccountID))]
		[PXSelector(typeof(Search<CashAccount.cashAccountID>), ValidateValue = false)]
        [PXUIField(DisplayName = "Cash Account ID")]
		public virtual int? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		protected string _DrCr;
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<ARPaymentChargeTran.entryTypeID, CAEntryType.drCr>))]
		[CADrCr.List()]
		[PXUIField(DisplayName = "Disb./Receipt", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DrCr
		{
			get
			{
				return this._DrCr;
			}
			set
			{
				this._DrCr = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected string _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "ExtRefNbr")]
		public virtual string ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region EntryTypeID
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }
		protected string _EntryTypeID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
							InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
							Where<CashAccountETDetail.accountID, Equal<Current<ARPayment.cashAccountID>>,
							And<CAEntryType.drCr, Equal<CADrCr.cACredit>,
							And<CAEntryType.module, Equal<BatchModule.moduleCA>>>>>))]
		[PXUIField(DisplayName = "Entry Type")]
		public virtual string EntryTypeID
		{
			get
			{
				return this._EntryTypeID;
			}
			set
			{
				this._EntryTypeID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected int? _AccountID;

		[PXDefault]
		[PXFormula(typeof(Selector<ARPaymentChargeTran.entryTypeID, Selector<CAEntryType.accountID, Account.accountCD>>))]
		[Account(DisplayName = "Offset Account",Required=true, AvoidControlAccounts = true)]
		public virtual int? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion

		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected int? _SubID;

		[PXDefault]
		[PXFormula(typeof(Selector<ARPaymentChargeTran.entryTypeID, CAEntryType.subID>))]
		[SubAccount(typeof(ARPaymentChargeTran.accountID), DisplayName = "Offset Subaccount", Required=true)]
		public virtual int? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDefault(typeof(ARPayment.docDate))]
		[PXUIField(DisplayName = "TranDate")]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected string _FinPeriodID;

		[FinPeriodID(
		    branchSourceType: typeof(ARPaymentChargeTran.cashAccountID),
		    branchSourceFormulaType: typeof(Selector<ARPaymentChargeTran.cashAccountID, CashAccount.branchID>),
		    masterFinPeriodIDType: typeof(ARPaymentChargeTran.tranPeriodID),
		    headerMasterFinPeriodIDType: typeof(ARPayment.tranPeriodID))]
        [PXUIField(DisplayName = "FinPeriodID")]
		public virtual string FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		protected string _TranPeriodID;

	    [PeriodID]
		[PXUIField(DisplayName = "TranPeriodID")]
		public virtual string TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected string _TranDesc;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXFormula(typeof(Selector<ARPaymentChargeTran.entryTypeID, CAEntryType.descr>))]
		[PXUIField(DisplayName = "Description")]
		public virtual string TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected long? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(ARRegister.curyInfoID))]
		public virtual long? CuryInfoID
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
		#region CashTranID
		public abstract class cashTranID : PX.Data.BQL.BqlLong.Field<cashTranID> { }
		protected long? _CashTranID;
		[PXDBLong()]
		[ARPaymentChargeCashTranID()]
		public virtual long? CashTranID
		{
			get
			{
				return this._CashTranID;
			}
			set
			{
				this._CashTranID = value;
			}
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		protected decimal? _CuryTranAmt;

		[PXDBCurrency(typeof(ARPaymentChargeTran.curyInfoID), typeof(ARPaymentChargeTran.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXFormula(null, typeof(SumCalc<ARRegister.curyChargeAmt>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<ARPaymentChargeTran.consolidate, Equal<True>>, ARPaymentChargeTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARPayment.curyConsolidateChargeTotal>))]
		public virtual decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected decimal? _TranAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected bool? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual bool? Released
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
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
		protected bool? _Cleared;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared")]
		public virtual bool? Cleared
		{
			get
			{
				return this._Cleared;
			}
			set
			{
				this._Cleared = value;
			}
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
		protected DateTime? _ClearDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "ClearDate")]
		public virtual DateTime? ClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
				this._ClearDate = value;
			}
		}
		#endregion
		#region Consolidate
		public abstract class consolidate : PX.Data.BQL.BqlBool.Field<consolidate> { }
		protected Boolean? _Consolidate;
		[PXDBBool()]
		[PXFormula(typeof(Selector<ARPaymentChargeTran.entryTypeID, CAEntryType.consolidate>))]
		public Boolean? Consolidate
		{
			get
			{
				return _Consolidate;
			}
			set
			{
				_Consolidate = value;
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
		protected string _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID
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
		protected string _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual byte[] tstamp
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
		#region GetCASign
		public int GetCASign()
		{
			return
				(ARPaymentType.DrCr(DocType) == GL.DrCr.Debit ? 1 : -1) *
				(DrCr == GL.DrCr.Debit ? 1 : -1);
		}
		#endregion
	}
}