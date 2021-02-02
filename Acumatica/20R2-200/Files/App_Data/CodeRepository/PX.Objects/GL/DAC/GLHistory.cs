using System.Diagnostics;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Formula;

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CM;

	/// <summary>
	/// A General Ledger history record. An instance of this class represents a history record for 
	/// a particular <see cref="BaseGLHistory.LedgerID">ledger</see>, <see cref="BaseGLHistory.BranchID">
	/// branch</see>, <see cref="BaseGLHistory.AccountID">account</see>, <see cref="BaseGLHistory.SubID">
	/// subaccount</see>, and <see cref="BaseGLHistory.FinPeriodID">financial period</see>.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLHistory)]
	[DebuggerDisplay("Account = {AccountID}, FinPeriod = {FinPeriodID}, FinPtdCredit = {FinPtdCredit}, FinPtdDebit = {FinPtdCredit}, FinYtdBalance = {FinYtdBalance}, FinBegBalance = {FinBegBalance}")]
    public partial class GLHistory : BaseGLHistory, PX.Data.IBqlTable
	{
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        #endregion
        #region FinPeriod
	    public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

	    /// <summary>
	    /// The identifier of the <see cref="FinPeriod">financial period</see> of the history record.
	    /// This field is a part of the compound key of the record.
	    /// </summary>
	    /// <value>
	    /// Corresponds to the <see cref="FinPeriod.FinPeriodID"/> field.
	    /// </value>
	    [PXDefault()]
	    [PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
	    [FinPeriodSelector(null, null,
	        branchSourceType: typeof(GLHistory.branchID),
	        SelectorMode = PXSelectorMode.NoAutocomplete, 
	        IsKey = true)]
	    public override String FinPeriodID { get; set; }
	    #endregion
        #region FinYear
        public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }

        /// <summary>
        /// The identifier of the financial year.
        /// </summary>
        /// <value>
        /// Determined from the <see cref="FinPeriodID"/> field.
        /// </value>
        [PXDBCalced(typeof(Substring<finPeriodID, CS.int1, CS.int4>), typeof(string))]
        public virtual string FinYear
        {
            get;
            set;
        }
        #endregion
        #region BalanceType
        public abstract class balanceType : PX.Data.BQL.BqlString.Field<balanceType> { }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region DetDeleted
		[Obsolete(Common.Messages.FieldIsObsoleteNotUsedAnymore)]
		public abstract class detDeleted : PX.Data.BQL.BqlBool.Field<detDeleted> { }
		protected Boolean? _DetDeleted;

        /// <summary>
        /// An unused obsolete field. In the past, it was used to 
		/// indicate that the related <see cref="BaseGLHistory.AccountID">
		/// account</see> or <see cref="BaseGLHistory.SubID">subaccount</see>
		/// had been deleted from the system so as to exclude the corresponding
		/// financial periods from GL balance validation process.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[Obsolete(Common.Messages.FieldIsObsoleteNotUsedAnymore)]
		public virtual Boolean? DetDeleted
		{
			get
			{
				return this._DetDeleted;
			}
			set
			{
				this._DetDeleted = value;
			}
		}
		#endregion
		#region YearClosed
		public abstract class yearClosed : PX.Data.BQL.BqlBool.Field<yearClosed> { }

        /// <summary>
        /// Indicates whether the year, which the history record belongs, is closed.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? YearClosed
		{
			get;
			set;
		}
		#endregion
		#region FinPtdCredit
		public abstract class finPtdCredit : PX.Data.BQL.BqlDecimal.Field<finPtdCredit> { }
		#endregion
		#region FinPtdDebit
		public abstract class finPtdDebit : PX.Data.BQL.BqlDecimal.Field<finPtdDebit> { }
		#endregion
		#region FinYtdBalance
		public abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		#endregion
		#region FinBegBalance
		public abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
		#endregion
		#region FinPtdRevalued
		public abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
		#endregion
		#region TranPtdCredit
		public abstract class tranPtdCredit : PX.Data.BQL.BqlDecimal.Field<tranPtdCredit> { }
		#endregion
		#region TranPtdDebit
		public abstract class tranPtdDebit : PX.Data.BQL.BqlDecimal.Field<tranPtdDebit> { }
		#endregion
		#region TranYtdBalance
		public abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
		#endregion
		#region TranBegBalance
		public abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
		#endregion
		#region CuryFinPtdCredit
		public abstract class curyFinPtdCredit : PX.Data.BQL.BqlDecimal.Field<curyFinPtdCredit> { }
		#endregion
		#region CuryFinPtdDebit
		public abstract class curyFinPtdDebit : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDebit> { }
		#endregion
		#region CuryFinYtdBalance
		public abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
		#endregion
		#region CuryFinBegBalance
		public abstract class curyFinBegBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegBalance> { }
		#endregion
		#region CuryTranPtdCredit
		public abstract class curyTranPtdCredit : PX.Data.BQL.BqlDecimal.Field<curyTranPtdCredit> { }
		#endregion
		#region CuryTranPtdDebit
		public abstract class curyTranPtdDebit : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDebit> { }
		#endregion
		#region CuryTranYtdBalance
		public abstract class curyTranYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdBalance> { }
		#endregion
		#region CuryTranBegBalance
		public abstract class curyTranBegBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegBalance> { }
		#endregion 
		#region AllocPtdBalance
		public abstract class allocPtdBalance : PX.Data.BQL.BqlDecimal.Field<allocPtdBalance> { }
		protected Decimal? _AllocPtdBalance;

        /// <summary>
        /// The period-to-date allocation balance (the amount allocated since the beginning of the period of the history record).
        /// </summary>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AllocPtdBalance
		{
			get
			{
				return this._AllocPtdBalance;
			}
			set
			{
				this._AllocPtdBalance = value;
			}
		}
	#endregion
		#region AllocBegBalance
		public abstract class allocBegBalance : PX.Data.BQL.BqlDecimal.Field<allocBegBalance> { }
		protected Decimal? _AllocBegBalance;

        /// <summary>
        /// The beginning allocation balance (the amount allocated for periods preceding the period of the history record).
        /// </summary>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AllocBegBalance
		{
			get
			{
				return this._AllocBegBalance;
			}
			set
			{
				this._AllocBegBalance = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion

		#region REFlag
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public new abstract class rEFlag : PX.Data.BQL.BqlBool.Field<rEFlag> { }
		#endregion
	}

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(AccountByPeriodEnq), Filter = typeof(AccountByPeriodFilter))]
	public class BaseGLHistory 
	{
		#region LedgerID
		protected Int32? _LedgerID;

        /// <summary>
        /// The identifier of the <see cref="Ledger">ledger</see> associated with the history record.
		/// This field is a part of the compound key of the record.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Ledger.LedgerID"/> field.
        /// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Ledger.ledgerID),
			typeof(Ledger.ledgerCD), typeof(Ledger.baseCuryID), typeof(Ledger.descr), typeof(Ledger.balanceType),
			DescriptionField = typeof(Ledger.descr), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region BranchID
		protected Int32? _BranchID;

        /// <summary>
        /// The identifier of the <see cref="Branch">branch</see> associated with the history record.
		/// This field is a part of the compound key of the record.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Branch.BAccountID"/> field.
        /// </value>
		[Branch(IsKey = true)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region AccountID
		protected Int32? _AccountID;

        /// <summary>
        /// The identifier of the <see cref="Account">account</see> associated with the history record.
		/// This field is a part of the compound key of the record.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[Account(Visibility = PXUIVisibility.Invisible,IsKey = true, DescriptionField = typeof(Account.description))]
		[PXDefault()]
		public virtual Int32? AccountID
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
		protected Int32? _SubID;
        /// <summary>
        /// The identifier of the <see cref="Sub">subaccount</see> associated with the history record.
		/// This field is a part of the compound key of the record.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[SubAccount(IsKey = true, Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		public virtual Int32? SubID
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
		#region FinPeriod
	    /// <summary>
	    /// The identifier of the <see cref="FinPeriod">financial period</see> of the history record.
	    /// This field is a part of the compound key of the record.
	    /// </summary>
	    /// <value>
	    /// Corresponds to the <see cref="FinPeriod.FinPeriodID"/> field.
	    /// </value>
	    public virtual String FinPeriodID { get; set; }
	    #endregion
		#region BalanceType
		protected String _BalanceType;

        /// <summary>
        /// The type of the balance.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// <c>"A"</c> - Actual,
        /// <c>"R"</c> - Reporting,
        /// <c>"S"</c> - Statistical,
        /// <c>"B"</c> - Budget.
        /// See <see cref="Ledger.BalanceType"/>.
        /// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(LedgerBalanceType.Actual)]
		[LedgerBalanceType.List()]
		[PXUIField(DisplayName = "Balance Type")]
		public virtual String BalanceType
		{
			get
			{
				return this._BalanceType;
			}
			set
			{
				this._BalanceType = value;
			}
		}
		#endregion
		#region CuryID
		protected String _CuryID;

        /// <summary>
        /// Identifier of the <see cref="Currency"/> of the history record.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Currency.CuryID"/> field.
        /// </value>
		[PXDBString(5, IsUnicode = true)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region FinPtdCredit
		protected Decimal? _FinPtdCredit;

        /// <summary>
        /// The period-to-date credit total of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, 
		/// which can be overridden by the user. See also <see cref="TranPtdCredit"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName="Fin. PTD Credit")]
		public virtual Decimal? FinPtdCredit
		{
			get
			{
				return this._FinPtdCredit;
			}
			set
			{
				this._FinPtdCredit = value;
			}
		}
		#endregion
		#region FinPtdDebit
		protected Decimal? _FinPtdDebit;

        /// <summary>
        /// The period-to-date debit total of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, 
		/// which can be overridden by the user. See also <see cref="TranPtdDebit"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Fin. PTD Debit")]
		public virtual Decimal? FinPtdDebit
		{
			get
			{
				return this._FinPtdDebit;
			}
			set
			{
				this._FinPtdDebit = value;
			}
		}
		#endregion
		#region FinYtdBalance
		protected Decimal? _FinYtdBalance;

        /// <summary>
        /// The year-to-date balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, which 
		/// can be overridden by the user. See also <see cref="TranYtdBalance"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Fin. YTD Balance")]
		public virtual Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinBegBalance
        protected Decimal? _FinBegBalance;

        /// <summary>
        /// The beginning balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, which 
		/// can be overridden by the user. See also <see cref="TranBegBalance"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Fin. Begining Balance")]
		public virtual Decimal? FinBegBalance
		{
			get
			{
				return this._FinBegBalance;
			}
			set
			{
				this._FinBegBalance = value;
			}
		}
		#endregion
		#region FinPtdRevalued
        protected Decimal? _FinPtdRevalued;

        /// <summary>
        /// The period-to-date revalued balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, 
		/// which can be overridden by the user. There is no corresponding amount field that depends on the 
		/// <see cref="GLTran.TranPeriodID"/> field.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region TranPtdCredit
		protected Decimal? _TranPtdCredit;

        /// <summary>
        /// The period-to-date credit total of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID"/> field,
        /// which cannot be overridden by the user and is determined by the date of the transactions.
        /// See also <see cref="FinPtdCredit"/>.
        /// </value>
        [PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdCredit
		{
			get
			{
				return this._TranPtdCredit;
			}
			set
			{
				this._TranPtdCredit = value;
			}
		}
		#endregion
		#region TranPtdDebit
        protected Decimal? _TranPtdDebit;

        /// <summary>
        /// The period-to-date debit total of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID"/> field,
        /// which cannot be overridden by the user and is determined by the date of the transactions.
        /// See also <see cref="FinPtdDebit"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdDebit
		{
			get
			{
				return this._TranPtdDebit;
			}
			set
			{
				this._TranPtdDebit = value;
			}
		}
		#endregion
		#region TranYtdBalance
        protected Decimal? _TranYtdBalance;

        /// <summary>
        /// The year-to-date balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID"/> field,
        /// which cannot be overridden by the user and is determined by the date of the transactions.
        /// See also <see cref="FinYtdBalance"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdBalance
		{
			get
			{
				return this._TranYtdBalance;
			}
			set
			{
				this._TranYtdBalance = value;
			}
		}
		#endregion
		#region TranBegBalance
        protected Decimal? _TranBegBalance;

        /// <summary>
        /// The beginning balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID"/> field,
        /// which cannot be overridden by the user and is determined by the date of the transactions.
        /// See also <see cref="FinBegBalance"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranBegBalance
		{
			get
			{
				return this._TranBegBalance;
			}
			set
			{
				this._TranBegBalance = value;
			}
		}
		#endregion
		#region CuryFinPtdCredit
        protected Decimal? _CuryFinPtdCredit;

        /// <summary>
        /// The period-to-date credit total of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, 
		/// which can be overridden by the user. See also <see cref="CuryTranPtdCredit"/>.
        /// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinPtdCredit
		{
			get
			{
				return this._CuryFinPtdCredit;
			}
			set
			{
				this._CuryFinPtdCredit = value;
			}
		}
		#endregion
		#region CuryFinPtdDebit
        protected Decimal? _CuryFinPtdDebit;

        /// <summary>
        /// The period-to-date debit total of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, 
		/// which can be overridden by the user. See also <see cref="CuryTranPtdDebit"/>.
        /// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryFinPtdDebit
		{
			get
			{
				return this._CuryFinPtdDebit;
			}
			set
			{
				this._CuryFinPtdDebit = value;
			}
		}
		#endregion
		#region CuryFinYtdBalance
        protected Decimal? _CuryFinYtdBalance;

        /// <summary>
        /// The year-to-date balance of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, 
		/// which can be overridden by the user. See also <see cref="CuryTranYtdBalance"/>.
        /// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CuryFinYtdBalance")]
		public virtual Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region CuryFinBegBalance
        protected Decimal? _CuryFinBegBalance;

        /// <summary>
        /// The beginning balance of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.FinPeriodID"/> field, which can be overridden by the user.
        /// See also <see cref="CuryTranBegBalance"/>.
        /// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CuryFinBegBalance")]
		public virtual Decimal? CuryFinBegBalance
		{
			get
			{
				return this._CuryFinBegBalance;
			}
			set
			{
				this._CuryFinBegBalance = value;
			}
		}
		#endregion
		#region CuryTranPtdCredit
        protected Decimal? _CuryTranPtdCredit;

        /// <summary>
        /// The period-to-date credit of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID"/> field, which cannot be overridden by 
		/// the user and is determined by the date of the transactions. See also <see cref="CuryFinPtdCredit"/>.
        /// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranPtdCredit
		{
			get
			{
				return this._CuryTranPtdCredit;
			}
			set
			{
				this._CuryTranPtdCredit = value;
			}
		}
		#endregion
		#region CuryTranPtdDebit
        protected Decimal? _CuryTranPtdDebit;

        /// <summary>
        /// The period-to-date debit of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID"/> field,
        /// which cannot be overridden by the user and is determined by the date of the transactions.
        /// See also <see cref="CuryFinPtdDebit"/>.
        /// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranPtdDebit
		{
			get
			{
				return this._CuryTranPtdDebit;
			}
			set
			{
				this._CuryTranPtdDebit = value;
			}
		}
		#endregion
		#region CuryTranYtdBalance
        protected Decimal? _CuryTranYtdBalance;

        /// <summary>
        /// The year-to-date balance of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID"/> field,
        /// which cannot be overridden by the user and is determined by the date of the transactions.
        /// See also <see cref="CuryFinYtdBalance"/>.
        /// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranYtdBalance
		{
			get
			{
				return this._CuryTranYtdBalance;
			}
			set
			{
				this._CuryTranYtdBalance = value;
			}
		}
		#endregion
		#region CuryTranBegBalance
        protected Decimal? _CuryTranBegBalance;

        /// <summary>
        /// The beginning balance of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// The value of this field is based on the <see cref="GLTran.TranPeriodID">GLTran.TranPeriodID</see> field,
        /// which cannot be overridden by the user and is determined by the date of the transactions.
        /// See also <see cref="BaseGLHistory.CuryFinBegBalance"/>.
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CuryTranBegBalance
		{
			get
			{
				return this._CuryTranBegBalance;
			}
			set
			{
				this._CuryTranBegBalance = value;
			}
		}
		#endregion
		#region FinFlag
		protected bool? _FinFlag = true;

        /// <summary>
        /// The flag determining the balance fields, to which the <see cref="PtdCredit"/>, <see cref="PtdDebit"/>,
        /// <see cref="YtdBalance"/>, <see cref="BegBalance"/>, <see cref="PtdRevalued"/> and their Cury* counterparts are mapped.
        /// </summary>
        /// <value>
        /// When set to <c>true</c>, the above fields are mapped to their Fin* analogs (e.g. <see cref="PtdDebit"/> will represent - get and set - <see cref="FinPtdDebit"/>),
        /// otherwise they are mapped to their Tran* analogs (e.g. <see cref="PtdDebit"/> corresponds to <see cref="TranPtdDebit"/>
        /// </value>
		[PXBool()]
		public virtual bool? FinFlag
		{
			get
			{
				return this._FinFlag;
			}
			set
			{
				this._FinFlag = value;
			}
		}
		#endregion
		#region REFlag
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public abstract class rEFlag : PX.Data.BQL.BqlBool.Field<rEFlag> { }

        /// <summary>
        /// An obsolete unused field.
        /// </summary>
        [Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		[PXBool()]
		public virtual bool? REFlag
		{
			get;
			set;
		}
		#endregion
		#region PtdCredit

        /// <summary>
        /// The period-to-date credit of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="FinPtdCredit"/> or to <see cref="TranPtdCredit"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? PtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdCredit : this._TranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdCredit = value;
				}
				else
				{
					this._TranPtdCredit = value;
				}
			}
		}
		#endregion
        #region PtdDebit

        /// <summary>
        /// The period-to-date debit of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="FinPtdDebit"/> or to <see cref="TranPtdDebit"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? PtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDebit : this._TranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDebit = value;
				}
				else
				{
					this._TranPtdDebit = value;
				}
			}
		}
		#endregion
        #region YtdBalance

        /// <summary>
        /// The year-to-date balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="FinYtdBalance"/> or to <see cref="TranYtdBalance"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? YtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdBalance : this._TranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdBalance = value;
				}
				else
				{
					this._TranYtdBalance = value;
				}
			}
		}
		#endregion
        #region BegBalance

        /// <summary>
        /// Beginning balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="FinBegBalance"/> or to <see cref="TranBegBalance"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? BegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinBegBalance : this._TranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinBegBalance = value;
				}
				else
				{
					this._TranBegBalance = value;
				}
			}
		}
		#endregion
        #region PtdRevalued

        /// <summary>
        /// The period-to-date revalued balance of the account in the <see cref="Company.baseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="FinPtdRevalued"/> or to <see cref="TranPtdRevalued"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? PtdRevalued
		{
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdRevalued : null;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdRevalued = value;
				}
			}
		}
		#endregion
        #region CuryPtdCredit

        /// <summary>
        /// The period-to-date credit of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="CuryFinPtdCredit"/> or to <see cref="CuryTranPtdCredit"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdCredit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdCredit : this._CuryTranPtdCredit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdCredit = value;
				}
				else
				{
					this._CuryTranPtdCredit = value;
				}
			}
		}
		#endregion
        #region CuryPtdDebit

        /// <summary>
        /// The period-to-date debit of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="CuryFinPtdDebit"/> or to <see cref="CuryTranPtdDebit"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDebit
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDebit : this._CuryTranPtdDebit;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDebit = value;
				}
				else
				{
					this._CuryTranPtdDebit = value;
				}
			}
		}
		#endregion
        #region CuryYtdBalance

        /// <summary>
        /// The year-to-date balance of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="CuryFinYtdBalance"/> or to <see cref="CuryTranYtdBalance"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdBalance : this._CuryTranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdBalance = value;
				}
				else
				{
					this._CuryTranYtdBalance = value;
				}
			}
		}
		#endregion
        #region CuryBegBalance

        /// <summary>
        /// Beginning balance of the account in the <see cref="BaseGLHistory.curyID">currency</see> of the account.
        /// </summary>
        /// <value>
        /// Corresponds either to <see cref="CuryFinBegBalance"/> or to <see cref="CuryTranBegBalance"/> field, depending on the <see cref="FinFlag"/>.
		[PXDecimal(4)]
		public virtual Decimal? CuryBegBalance
		{
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinBegBalance : this._CuryTranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinBegBalance = value;
				}
				else
				{
					this._CuryTranBegBalance = value;
				}
			}
		}
		#endregion
		#region tstamp
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
	}

	[PXHidden]
	public class GLHistoryFilter : IBqlTable
	{
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID()]
		public virtual string FinPeriodID { get; set; }
	}
}
