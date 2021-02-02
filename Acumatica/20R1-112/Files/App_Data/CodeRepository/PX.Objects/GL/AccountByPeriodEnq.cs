using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.DR;
using PX.Objects.FA;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.BQL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.Reclassification.UI;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Data.BQL;

namespace PX.Objects.GL
{
	#region FilterClass
	[Serializable]
	public partial class AccountByPeriodFilter : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(
			onlyActive: false, 
			Required = false)]
		public int? OrganizationID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[BranchOfOrganization(
			typeof(AccountByPeriodFilter.organizationID), 
			onlyActive: false,
			Required = false)]
		public int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }

		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		[PXDBInt]
		[PXDefault(
			typeof(Coalesce<Coalesce<
				Search<Organization.actualLedgerID,
					Where<Organization.bAccountID, Equal<Current2<AccountByPeriodFilter.orgBAccountID>>>>,
				Search<Branch.ledgerID,
					Where<Branch.bAccountID, Equal<Current2<AccountByPeriodFilter.orgBAccountID>>>>>,
				Search<Branch.ledgerID,
					Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Ledger.ledgerID, Where<Ledger.balanceType, NotEqual<LedgerBalanceType.budget>>>), SubstituteKey = typeof(Ledger.ledgerCD), DescriptionField = typeof(Ledger.descr))]
		public virtual int? LedgerID { get; set; }
		#endregion

		#region StartPeriodID
		public abstract class startPeriodID : PX.Data.BQL.BqlString.Field<startPeriodID> { }
		protected String _StartPeriodID;
		[PXDefault()]
		[AnyPeriodFilterable(null,
			typeof(AccessInfo.businessDate), 
			branchSourceType: typeof(AccountByPeriodFilter.branchID),
			organizationSourceType: typeof(AccountByPeriodFilter.organizationID),
			useMasterCalendarSourceType: typeof(AccountByPeriodFilter.useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "From Period", Visibility = PXUIVisibility.Visible)]
		public virtual String StartPeriodID
		{
			get
			{
				return this._StartPeriodID;
			}
			set
			{
				this._StartPeriodID = value;
			}
		}
		#endregion
		#region UseMasterCalendar
		public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

		[PXBool]
		[PXUIField(DisplayName = Common.Messages.UseMasterCalendar)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.multipleCalendarsSupport>))]
		public bool? UseMasterCalendar { get; set; }
		#endregion
		#region EndPeriodID
		public abstract class endPeriodID : PX.Data.BQL.BqlString.Field<endPeriodID> { }
		protected String _EndPeriodID;
		[PXDefault()]
		[AnyPeriodFilterable(null, 
			typeof(AccessInfo.businessDate),
			branchSourceType: typeof(AccountByPeriodFilter.branchID),
			organizationSourceType: typeof(AccountByPeriodFilter.organizationID),
			useMasterCalendarSourceType: typeof(AccountByPeriodFilter.useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "To Period", Visibility = PXUIVisibility.Visible)]
		public virtual String EndPeriodID
		{
			get
			{
				return this._EndPeriodID;
			}
			set
			{
				this._EndPeriodID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate()]
		//[PXDefault(typeof(Search<FinPeriod.startDate, Where<FinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.finPeriodID>>>>))]
		[PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
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
		//[PXDefault(typeof(Search<FinPeriod.endDate, Where<FinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.finPeriodID>>>>))]
		//[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[AccountAny]
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
		public abstract class subID : PX.Data.BQL.BqlString.Field<subID> { }
		protected String _SubID;
		[SubAccountRestrictedRaw(DisplayName = "Subaccount", SuppressValidation = true)]
		public virtual String SubID
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
		#region ShowSummary
		public abstract class showSummary : PX.Data.BQL.BqlBool.Field<showSummary> { }
		protected Boolean? _ShowSummary;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Summary")]
		public virtual Boolean? ShowSummary
		{
			get
			{
				return this._ShowSummary;
			}
			set
			{
				this._ShowSummary = value;
			}
		}
		#endregion
		#region IncludeUnreleased
		public abstract class includeUnreleased : PX.Data.BQL.BqlBool.Field<includeUnreleased> { }
		protected Boolean? _IncludeUnreleased;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Unreleased")]
		public virtual Boolean? IncludeUnreleased
		{
			get
			{
				return this._IncludeUnreleased;
			}
			set
			{
				this._IncludeUnreleased = value;
			}
		}
		#endregion
		#region IncludeUnposted
		public abstract class includeUnposted : PX.Data.BQL.BqlBool.Field<includeUnposted> { }
		protected Boolean? _IncludeUnposted;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Unposted")]
		public virtual Boolean? IncludeUnposted
		{
			get
			{
				return this._IncludeUnposted;
			}
			set
			{
				this._IncludeUnposted = value;
			}
		}
		#endregion
		#region IncludeReclassified
		public abstract class includeReclassified : PX.Data.BQL.BqlBool.Field<includeReclassified> { }
		protected Boolean? _IncludeReclassified;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Include Reclassified")]
		public virtual Boolean? IncludeReclassified
		{
			get
			{
				return this._IncludeReclassified;
			}
			set
			{
				this._IncludeReclassified = value;
			}
		}
		#endregion
		#region BegBal
		public abstract class begBal : PX.Data.BQL.BqlDecimal.Field<begBal> { }
		protected Decimal? _BegBal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Balance", Enabled = false, Visible = true)]
		public virtual Decimal? BegBal
		{
			get
			{
				return this._BegBal;
			}
			set
			{
				this._BegBal = value;
			}
		}
		#endregion
		#region CreditTotal
		public abstract class creditTotal : PX.Data.BQL.BqlDecimal.Field<creditTotal> { }
		protected Decimal? _CreditTotal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Credit Total", Enabled = false, Visible = false)]
		public virtual Decimal? CreditTotal
		{
			get
			{
				return this._CreditTotal;
			}
			set
			{
				this._CreditTotal = value;
			}
		}
		#endregion
		#region DebitTotal
		public abstract class debitTotal : PX.Data.BQL.BqlDecimal.Field<debitTotal> { }
		protected Decimal? _DebitTotal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Debit Total", Enabled = false, Visible = false)]
		public virtual Decimal? DebitTotal
		{
			get
			{
				return this._DebitTotal;
			}
			set
			{
				this._DebitTotal = value;
			}
		}
		#endregion
		#region EndBal
		public abstract class endBal : PX.Data.BQL.BqlDecimal.Field<endBal> { }
		protected Decimal? _EndBal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Enabled = false, Visible = true)]
		public virtual Decimal? EndBal
		{
			get
			{
				return this._EndBal;
			}
			set
			{
				this._EndBal = value;
			}
		}
		#endregion
		#region TranCreditTotal
		public abstract class tranCreditTotal : PX.Data.BQL.BqlDecimal.Field<tranCreditTotal> { }
		protected Decimal? _TranCreditTotal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total", Enabled = false, Visible = false)]
		public virtual Decimal? TranCreditTotal
		{
			get
			{
				return this._TranCreditTotal;
			}
			set
			{
				this._TranCreditTotal = value;
			}
		}
		#endregion
		#region TranDebitTotal
		public abstract class tranDebitTotal : PX.Data.BQL.BqlDecimal.Field<tranDebitTotal> { }
		protected Decimal? _TranDebitTotal;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total", Enabled = false, Visible = false)]
		public virtual Decimal? TranDebitTotal
		{
			get
			{
				return this._TranDebitTotal;
			}
			set
			{
				this._TranDebitTotal = value;
			}
		}
		#endregion
		#region TurnOver
		public abstract class turnOver : PX.Data.BQL.BqlDecimal.Field<turnOver> { }
		protected Decimal? _turnOver;
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Turnover", Enabled = false, Visible = true)]
		public virtual Decimal? TurnOver
		{
			get
			{
				return this._turnOver;
			}
			set
			{
				this._turnOver = value;
			}
		}
		#endregion
		#region UnsignedBegBal
		public abstract class unsignedBegBal : PX.Data.BQL.BqlDecimal.Field<unsignedBegBal> { }
		[PXDBBaseCury(typeof(AccountByPeriodFilter.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnsignedBegBal { get; set; }
		#endregion
		#region UnsignedCuryBegBal
		public abstract class unsignedCuryBegBal : PX.Data.BQL.BqlDecimal.Field<unsignedCuryBegBal> { }
		[PXDBCury(typeof(GLTranR.curyID))]
		public virtual decimal? UnsignedCuryBegBal { get; set; }
		#endregion
		#region ShowCuryDetails
		public abstract class showCuryDetail : PX.Data.BQL.BqlBool.Field<showCuryDetail> { }
		protected bool? _ShowCuryDetail;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Show Currency Details", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ShowCuryDetail
		{
			get
			{
				return this._ShowCuryDetail;
			}
			set
			{
				this._ShowCuryDetail = value;
			}
		}
		#endregion
		#region SubCD Wildcard
		public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubCDWildcard
		{
			[PXDependsOnFields(typeof(subID))]
			get
			{
				return SubCDUtils.CreateSubCDWildcard(this._SubID, SubAccountAttribute.DimensionName);
			}
		}
		#endregion
		#region PeriodStartDate
		public abstract class periodStartDate : PX.Data.BQL.BqlDateTime.Field<periodStartDate> { }
		protected DateTime? _PeriodStartDate;

		[PXDBDate()]
		[PXDefault(typeof(Search<MasterFinPeriod.startDate, 
								Where<MasterFinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.startPeriodID>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Period Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodStartDate
		{
			get
			{
				return this._PeriodStartDate;
			}
			set
			{
				this._PeriodStartDate = value;
			}
		}
		#endregion
		#region PeriodEndDate
		public abstract class periodEndDate : PX.Data.BQL.BqlDateTime.Field<periodEndDate> { }
		protected DateTime? _PeriodEndDate;
		[PXDBDate()]
		[PXDefault(typeof(Search<MasterFinPeriod.endDate,
									Where<MasterFinPeriod.finPeriodID, Equal<Current<AccountByPeriodFilter.endPeriodID>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? PeriodEndDate
		{
			get
			{
				return this._PeriodEndDate;
			}
			set
			{
				this._PeriodEndDate = value;
			}
		}
        #endregion
        #region PeriodStartDateUI
        public abstract class periodstartDateUI : PX.Data.BQL.BqlDateTime.Field<periodstartDateUI> { }

        [PXDate]
        [PXUIField(DisplayName = "Period Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? PeriodStartDateUI
        {
            [PXDependsOnFields(typeof(periodStartDate), typeof(periodEndDate))]
            get
            {
                return (_PeriodStartDate != null && _PeriodEndDate != null && _PeriodStartDate == _PeriodEndDate) ? _PeriodStartDate.Value.AddDays(-1) : _PeriodStartDate;
            }
            set
			{ }
        }
        #endregion
        #region PeriodEndDateUI
        public abstract class periodEndDateUI : PX.Data.BQL.BqlDateTime.Field<periodEndDateUI> { }

		[PXDate()]
		[PXUIField(DisplayName = "Period End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? PeriodEndDateUI => _PeriodEndDate?.AddDays(-1);

        #endregion
        #region StartDateUI
        public abstract class startDateUI : PX.Data.BQL.BqlDateTime.Field<startDateUI> { }

		[PXDate]
		[PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? StartDateUI
        {
            [PXDependsOnFields(typeof(startDate), typeof(endDate))]
            get
            {
                return (_StartDate != null && _EndDate != null && _StartDate == _EndDate) ? _StartDate.Value.AddDays(-1) : _StartDate;
            }
            set
            {
                _StartDate = (value != null && _EndDate != null && value == EndDateUI) ? value.Value.AddDays(1) : value;
            }
        }
        #endregion
        #region EndDateUI
        public abstract class endDateUI : PX.Data.BQL.BqlDateTime.Field<endDateUI> { }

		[PXDate()]
		[PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? EndDateUI
		{
			get
			{
				return _EndDate?.AddDays(-1);
			}
			set
			{
				_EndDate = value?.AddDays(1);
			}
		}
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodIDFormatting]
		/// <summary>
		/// This field is used to pass navigation params from GL632000 report 
		/// </summary>
		public String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
				if (value != null)
				{					
					//Reset periods only when a specified value passed, for clearing all periods do it explicitly by fields.					
				StartPeriodID = value;
				EndPeriodID = value;
			}
		}
		}
		#endregion
	}
	#endregion
	#region DAC class extension - additional fields
	[Serializable]
	public partial class GLTranR : GLTran, ISignedBalances
	{
		public new abstract class selected : IBqlField { }
		public new abstract class branchID : IBqlField { }
		public new abstract class tranDate : IBqlField { }
		public new abstract class module : IBqlField { }
		public new abstract class refNbr: IBqlField { }
		public new abstract class lineNbr : IBqlField { }
		public new abstract class batchNbr : IBqlField { }
		public new abstract class debitAmt : IBqlField { }
		public new abstract class creditAmt : IBqlField { }
		public new abstract class curyDebitAmt : IBqlField { }
		public new abstract class curyCreditAmt : IBqlField { }
        public new abstract class curyReclassRemainingAmt : IBqlField { }
		public new abstract class subID : IBqlField { }
		public new abstract class tranDesc : IBqlField { }
		public new abstract class tranType : IBqlField { }
		public new abstract class ledgerID : IBqlField { }
		public new abstract class accountID : IBqlField { }
		public new abstract class finPeriodID : IBqlField { }
		public new abstract class tranPeriodID : IBqlField { }
		public new abstract class posted : IBqlField { }
		public new abstract class released : IBqlField { }
		public new abstract class reclassified : IBqlField { }
		public new abstract class isReclassReverse : IBqlField { }
		public new abstract class reclassOrigTranDate : IBqlField { }
		public new abstract class reclassBatchNbr : IBqlField { }
		public new abstract class noteID : IBqlField { }

		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[IN.Inventory(Visible = false)]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region TranDate
		[PXDBDate()]
		[PXDBDefault(typeof(Batch.dateEntered))]
		[PXUIField(DisplayName = "Tran. Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override DateTime? TranDate
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
		#region RefNbr
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("", typeof(Search<GLTran.refNbr, Where<GLTran.module, Equal<Current<GLTran.module>>, And<GLTran.batchNbr, Equal<Current<GLTran.batchNbr>>>>>))]
		[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		public override String RefNbr
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
		#region TranDesc
		[PXDBString(256, IsUnicode = true)]
		[PXDefault(typeof(Search<GLTran.tranDesc, Where<GLTran.module, Equal<Current<GLTran.module>>, And<GLTran.batchNbr, Equal<Current<GLTran.batchNbr>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String TranDesc
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
		#region BegBalance
		public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
		protected Decimal? _BegBalance;
		[PXBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beg. Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? BegBalance
		{
			get
			{
				return this._BegBalance;
			}
			set
			{
				this._BegBalance = value;
			}
		}
		#endregion
		#region DebitAmt
		[PXDBBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? DebitAmt
		{
			get
			{
				return this._DebitAmt;
			}
			set
			{
				this._DebitAmt = value;
			}
		}
		#endregion
		#region CreditAmt
		[PXDBBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CreditAmt
		{
			get
			{
				return this._CreditAmt;
			}
			set
			{
				this._CreditAmt = value;
			}
		}
		#endregion
		#region CuryDebitAmt
		[PXDBCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Debit Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryDebitAmt
		{
			get
			{
				return this._CuryDebitAmt;
			}
			set
			{
				this._CuryDebitAmt = value;
			}
		}
		#endregion
		#region CuryCreditAmt
		[PXDBCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Credit Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryCreditAmt
		{
			get
			{
				return this._CuryCreditAmt;
			}
			set
			{
				this._CuryCreditAmt = value;
			}
		}
		#endregion
		#region SubID
		[SubAccount()]
		public override Int32? SubID
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected string _CuryID;
		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency")]
		[PXSelector(typeof(Currency.curyID))]

		public virtual string CuryID
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
		#region EndBalance
		public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
		[PXBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? EndBalance
		{
			[PXDependsOnFields(typeof(begBalance), typeof(type), typeof(debitAmt), typeof(creditAmt))]
			get
			{
				return this.Type == null ? null : this._BegBalance + AccountRules.CalcSaldo(this.Type, this.DebitAmt ?? 0m, this.CreditAmt ?? 0m);
			}

		}
		#endregion
		#region CuryBegBalance
		public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }
		protected Decimal? _CuryBegBalance;
		[PXCury(typeof(GLTranR.curyID))]
		[PXUIField(DisplayName = "Curr. Beg. Balance", Visible = true)]
		public virtual Decimal? CuryBegBalance
		{
			get
			{
				return this._CuryBegBalance;
			}
			set
			{
				this._CuryBegBalance = value;
			}
		}
		#endregion
		#region CuryEndBalance
		public abstract class curyEndBalance : PX.Data.BQL.BqlDecimal.Field<curyEndBalance> { }
		[PXCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Ending Balance", Visible = true)]
		public virtual Decimal? CuryEndBalance
		{
			[PXDependsOnFields(typeof(curyBegBalance), typeof(type), typeof(curyDebitAmt), typeof(curyCreditAmt))]
			get
			{
				return this.Type == null ? null : this._CuryBegBalance + AccountRules.CalcSaldo(this.Type, this.CuryDebitAmt ?? 0m, this.CuryCreditAmt ?? 0m);
			}

		}
		#endregion
		#region SignBegBalance
		public abstract class signBegBalance : PX.Data.BQL.BqlDecimal.Field<signBegBalance> { }
		[PXBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beg. Balance")]
		public virtual Decimal? SignBegBalance { get; set; }
		#endregion
		#region SignEndBalance
		public abstract class signEndBalance : PX.Data.BQL.BqlDecimal.Field<signEndBalance> { }
		[PXBaseCury(typeof(GLTranR.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance")]
		public virtual Decimal? SignEndBalance { get; set; }
		#endregion
		#region SignCuryBegBalance
		public abstract class signCuryBegBalance : PX.Data.BQL.BqlDecimal.Field<signCuryBegBalance> { }
		[PXCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Beg. Balance")]
		public virtual Decimal? SignCuryBegBalance { get; set; }
		#endregion
		#region SignCuryEndBalance
		public abstract class signCuryEndBalance : PX.Data.BQL.BqlDecimal.Field<signCuryEndBalance> { }
		[PXCury(typeof(GLTranR.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Ending Balance")]
		public virtual Decimal? SignCuryEndBalance { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected string _Type;
		[PXString(1)]
		[PXFormula(typeof(Selector<GLTran.accountID, ADL.Account.type>))]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region ReferenceID
		public new abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }
		[PXDBInt()]
		[PXDimensionSelector("BIZACCT", typeof(Search<BAccountR.bAccountID>), typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctName), DirtyRead = true)]
		[PXUIField(DisplayName = CR.Messages.BAccountCD, Enabled = false, Visible = false)]
		public override Int32? ReferenceID
		{
			get
			{
				return this._ReferenceID;
			}
			set
			{
				this._ReferenceID = value;
			}
		}
		#endregion

		public virtual string BatchType { get; set; }

        public decimal? OrigDebitAmt { get; set; }
        public decimal? OrigCreditAmt { get; set; }
        public decimal? CuryOrigDebitAmt { get; set; }
        public decimal? CuryOrigCreditAmt { get; set; }
	}
	#endregion

	#region Additional DACs

	public partial class ReclassBatch : Batch
	{
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		public new abstract class posted : PX.Data.BQL.BqlBool.Field<posted> { }
	}

	#endregion

	[TableAndChartDashboardType]
	public class AccountByPeriodEnq : PXGraph<AccountByPeriodEnq>
	{
		#region declaration

		public PXCancel<AccountByPeriodFilter> Cancel;
		public PXAction<AccountByPeriodFilter> PreviousPeriod;
		public PXAction<AccountByPeriodFilter> NextPeriod;

		public PXAction<AccountByPeriodFilter> DoubleClick;
		public PXAction<AccountByPeriodFilter> ViewBatch;
		public PXAction<AccountByPeriodFilter> ViewDocument;

		public PXAction<AccountByPeriodFilter> reclassify;
		public PXAction<AccountByPeriodFilter> reclassifyAll;
		public PXAction<AccountByPeriodFilter> reclassificationHistory;

		public PXFilter<AccountByPeriodFilter> Filter;

		[PXFilterable]
		public PXSelectOrderBy<GLTranR, 
			OrderBy<Asc<GLTranR.tranDate,
				Asc<GLTranR.refNbr,
				Asc<GLTranR.batchNbr,
				Asc<GLTranR.module,
				Asc<GLTranR.lineNbr>>>>>>> GLTranEnq;

		public PXSetup<GLSetup> glsetup;
		public PXSelect<Account, Where<Account.accountID, Equal<Current<AccountByPeriodFilter.accountID>>>> AccountInfo;

		public FinPeriod CurrentStartPeriod 
		{
			get
			{
				int? calendarOrganizationID = 
					FinPeriodRepository.GetCalendarOrganizationID(Filter.Current.OrganizationID,
																			Filter.Current.BranchID, 
																			Filter.Current.UseMasterCalendar);

				return FinPeriodRepository.FindByID(calendarOrganizationID, Filter.Current.StartPeriodID);
			}
		}

		protected Ledger CurrentLedger => PXSelectReadonly<Ledger, Where<Ledger.ledgerID, Equal<Current<AccountByPeriodFilter.ledgerID>>>>.Select(this);

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		protected virtual int[] FilteringBranchIDs
		{
			get
			{
				int[] branchIDs = null;
				if (Filter.Current.BranchID != null)
				{
					branchIDs = new int[] { (int)Filter.Current.BranchID };
				}
				else if (Filter.Current.OrganizationID != null)
				{
					branchIDs = PXAccess.GetChildBranchIDs(Filter.Current.OrganizationID, false);
				}
				return branchIDs;
			}
		}

		protected virtual PXSelectBase<GLTranR> Command
		{
			get
			{
				AccountByPeriodFilter filter = Filter.Current;
				PXSelectBase<GLTranR> cmd = new PXSelectJoin<GLTranR,
						InnerJoin<ADL.Sub, On<GLTranR.subID, Equal<Sub.subID>,
							And<Match<ADL.Sub, Current<AccessInfo.userName>>>>,
						InnerJoin<ADL.Account, On<GLTranR.accountID, Equal<ADL.Account.accountID>,
							And<Match<ADL.Account, Current<AccessInfo.userName>>>>,
						LeftJoin<ADL.Batch, On<GLTranR.module, Equal<ADL.Batch.module>,
							And<GLTranR.batchNbr, Equal<ADL.Batch.batchNbr>>>>>>,
						Where<GLTranR.ledgerID, Equal<Current<AccountByPeriodFilter.ledgerID>>,
							And<GLTranR.accountID, Equal<Current<AccountByPeriodFilter.accountID>>,
							And<ADL.Batch.voided, NotEqual<True>,
							And<ADL.Batch.scheduled, NotEqual<True>>>>>>(this);

				#region Filters
				if (filter.UseMasterCalendar == true)
				{
					cmd.WhereAnd<Where<GLTranR.tranPeriodID, GreaterEqual<Current<AccountByPeriodFilter.startPeriodID>>,
								And<GLTranR.tranPeriodID, LessEqual<Current<AccountByPeriodFilter.endPeriodID>>>>>();
				}
				else
				{
					cmd.WhereAnd<Where<GLTranR.finPeriodID, GreaterEqual<Current<AccountByPeriodFilter.startPeriodID>>,
								And<GLTranR.finPeriodID, LessEqual<Current<AccountByPeriodFilter.endPeriodID>>>>>();
				}

				if (filter.IncludeUnposted != true)
				{
					cmd.WhereAnd<Where<GLTranR.posted, Equal<True>>>();
				}
				else if (filter.IncludeUnposted == true && filter.IncludeUnreleased != true)
				{
					cmd.WhereAnd<Where<GLTranR.released, Equal<True>>>();
				}

				if (filter.IncludeReclassified != true && filter.ShowSummary != true)
				{
					cmd.Join<LeftJoin<ReclassifyingGLTranAggregate, 
							On<GLTranR.module, Equal<ReclassifyingGLTranAggregate.module>,
							   And<GLTranR.batchNbr, Equal<ReclassifyingGLTranAggregate.batchNbr>,
							   And<GLTranR.lineNbr, Equal<ReclassifyingGLTranAggregate.lineNbr>>>>>>();

                    if(filter.StartDate == null && filter.EndDate == null)
                    {
                        cmd.WhereAnd<Where2<Where<GLTranR.isReclassReverse, Equal<False>>, And<Where<GLTranR.reclassified, Equal<False>,
							Or<Where<GLTranR.reclassified, Equal<True>,
                                    And<Where<Sub<GLTranR.curyDebitAmt, IsNull<ReclassifyingGLTranAggregate.curyCreditAmt, decimal0>>, NotEqual<Zero>,
                                        Or<Sub<GLTranR.curyCreditAmt, IsNull<ReclassifyingGLTranAggregate.curyDebitAmt, decimal0>>, NotEqual<Zero>>>>>>>>>>();
                    }
                    if (filter.StartDate != null && filter.EndDate == null)
                    {
                        cmd.WhereAnd<Where2<Where<GLTranR.isReclassReverse, Equal<False>,
                                    Or<Where<GLTranR.isReclassReverse, Equal<True>,
                                    And<Where<GLTranR.reclassOrigTranDate, LessEqual<Current<AccountByPeriodFilter.startDate>>>>>>>, And<Where<GLTranR.reclassified, Equal<False>,
                                Or<Where<GLTranR.reclassified, Equal<True>,
                                    And<Where<Sub<GLTranR.curyDebitAmt, IsNull<ReclassifyingGLTranAggregate.curyCreditAmt, decimal0>>, NotEqual<Zero>,
                                        Or<Sub<GLTranR.curyCreditAmt, IsNull<ReclassifyingGLTranAggregate.curyDebitAmt, decimal0>>, NotEqual<Zero>>>>>>>>>>();
                    }
                    if (filter.StartDate == null && filter.EndDate != null)
                    {
                        cmd.WhereAnd<Where2<Where<GLTranR.isReclassReverse, Equal<False>,
                                    Or<Where<GLTranR.isReclassReverse, Equal<True>,
                                    And<Where<GLTranR.reclassOrigTranDate, GreaterEqual<Current<AccountByPeriodFilter.periodEndDate>>>>>>>, And<Where<GLTranR.reclassified, Equal<False>,
                                Or<Where<GLTranR.reclassified, Equal<True>,
                                    And<Where<Sub<GLTranR.curyDebitAmt, IsNull<ReclassifyingGLTranAggregate.curyCreditAmt, decimal0>>, NotEqual<Zero>,
                                        Or<Sub<GLTranR.curyCreditAmt, IsNull<ReclassifyingGLTranAggregate.curyDebitAmt, decimal0>>, NotEqual<Zero>>>>>>>>>>();
                    }
                    if (filter.StartDate != null && filter.EndDate != null)
                    {
                        cmd.WhereAnd<Where2<Where<GLTranR.isReclassReverse, Equal<False>,
                                    Or<Where<GLTranR.isReclassReverse, Equal<True>,
                                    And<Where2<Where<GLTranR.reclassOrigTranDate, LessEqual<Current<AccountByPeriodFilter.startDate>>>,
                                        Or<Where<GLTranR.reclassOrigTranDate, GreaterEqual<Current<AccountByPeriodFilter.periodEndDate>>>>>>>>>, And<Where<GLTranR.reclassified, Equal<False>,
                                Or<Where<GLTranR.reclassified, Equal<True>,
                                    And<Where<Sub<GLTranR.curyDebitAmt, IsNull<ReclassifyingGLTranAggregate.curyCreditAmt, decimal0>>, NotEqual<Zero>,
                                        Or<Sub<GLTranR.curyCreditAmt, IsNull<ReclassifyingGLTranAggregate.curyDebitAmt, decimal0>>, NotEqual<Zero>>>>>>>>>>();
                    }
				}

				if (filter.StartDate != null)
				{
					cmd.WhereAnd<Where<GLTranR.tranDate, GreaterEqual<Current<AccountByPeriodFilter.startDate>>>>();
				}

				if (filter.EndDate != null)
				{
					cmd.WhereAnd<Where<GLTranR.tranDate, Less<Current<AccountByPeriodFilter.endDate>>>>();
				}

				if (FilteringBranchIDs != null)
				{
					cmd.WhereAnd<Where<GLTranR.branchID, In<Required<AccountByPeriodFilter.branchID>>,
						And<MatchWithBranch<GLTranR.branchID>>>>();
				}

				if (!SubCDUtils.IsSubCDEmpty(filter.SubID))
				{
					cmd.WhereAnd<Where<ADL.Sub.subCD, Like<Current<AccountByPeriodFilter.subCDWildcard>>>>();
				}
				#endregion

				if (filter.ShowSummary == true)
				{
					Type aggregate = BqlTemplate.FromType(
						typeof(Aggregate<
							Sum<GLTranR.creditAmt,
							Sum<GLTranR.debitAmt,
							Sum<GLTranR.curyCreditAmt,
							Sum<GLTranR.curyDebitAmt,
							GroupBy<GLTranR.ledgerID,
							GroupBy<GLTranR.accountID,
							GroupBy<BqlPlaceholder.A,
							GroupBy<GLTranR.tranDate>>>>>>>>>)
					)
					.Replace<BqlPlaceholder.A>(filter?.UseMasterCalendar == true 
						? typeof(GLTranR.tranPeriodID) 
						: typeof(GLTranR.finPeriodID))
						.ToType();
					cmd.View = new PXView(this, false, cmd.View.BqlSelect.AggregateNew(aggregate));
				}
				return cmd;
			}
		}

		protected bool InReclassifyAllSelectingContext;

		#endregion
		#region Ctor
		public AccountByPeriodEnq()
		{
			GLSetup setup = glsetup.Current;
			GLTranEnq.Cache.AllowInsert = false;
			GLTranEnq.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetReadOnly(GLTranEnq.Cache, null, true);
			PXUIFieldAttribute.SetReadOnly<GLTranR.selected>(GLTranEnq.Cache, null, false);

			PXUIFieldAttribute.SetVisible<GLTranR.selected>(GLTranEnq.Cache, null, true);

			PXCache cacheBAccountR = Caches[typeof(BAccountR)];
			PXUIFieldAttribute.SetDisplayName<BAccountR.acctName>(cacheBAccountR, CR.Messages.BAccountName);
			PXUIFieldAttribute.SetVisible<BAccountR.acctName>(cacheBAccountR, null, false);
			PXUIFieldAttribute.SetVisible<GLTranR.finPeriodID>(GLTranEnq.Cache, null, true);
		}
		#endregion
		#region Button Delegates
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			AccountByPeriodFilter filter = Filter.Current;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod prevStartPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.StartPeriodID, looped: true);
			filter.StartPeriodID = prevStartPeriod?.FinPeriodID;

			FinPeriod prevEndPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.EndPeriodID, looped: true);
			filter.EndPeriodID = prevEndPeriod?.FinPeriodID;

			ResetFilterDates(filter);

			return adapter.Get();
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			AccountByPeriodFilter filter = Filter.Current;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod nextStartPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.StartPeriodID, looped: true);
			filter.StartPeriodID = nextStartPeriod?.FinPeriodID;

			FinPeriod nextEndPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.EndPeriodID, looped: true);
			filter.EndPeriodID = nextEndPeriod?.FinPeriodID;

			ResetFilterDates(filter);

			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.Reclassify, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = false)]
		[PXButton]
		public virtual IEnumerable Reclassify(PXAdapter adapter)
		{
			IReadOnlyCollection<GLTranR> selectedTrans = GetSelectedTrans();

			if (selectedTrans.Any())
			{
				ReclassifyTransactionsProcess.OpenForReclassification(selectedTrans);
			}
			else
			{
				throw new PXException(InfoMessages.NoReclassifiableTransactionsHaveBeenSelected);
			}

			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.ReclassifyAll, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = false)]
		[PXButton]
		public virtual IEnumerable ReclassifyAll(PXAdapter adapter)
		{
			IEnumerable<GLTranR> trans;

			try
			{
				InReclassifyAllSelectingContext = true;

				trans = GLTranEnq.Select().RowCast<GLTranR>().ToArray();
			}
			finally
			{
				InReclassifyAllSelectingContext = false;
			}

			ReclassifyTransactionsProcess.TryOpenForReclassification<GLTranR>(trans,
				CurrentLedger,
				tran => tran.BatchType,
				GLTranEnq.View,
				InfoMessages.SomeTransactionsCannotBeReclassified,
				InfoMessages.NoReclassifiableTransactionsHaveBeenFoundToMatchTheCriteria);

			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.ReclassificationHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ReclassificationHistory(PXAdapter adapter)
		{
			if (GLTranEnq.Current != null)
			{
				ReclassificationHistoryInq.OpenForTransaction(GLTranEnq.Current);
			}

			return Filter.Select();
		}


		#region Redirection buttons
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewBatch(PXAdapter adapter)
		{
			GLTranR tran = GLTranEnq.Current;

			if (tran != null)
			{
				RedirectToBatch(tran);
			}

			return Filter.Select();
		}

		[Obsolete("The type is obsolete and will be removed in Acumatica 8.0. Use JournalEntry.OpenDocumentByTran method.")]
		public class GraphFactory
		{
			public IDocGraphCreator this[string tranModule]
			{
				get
				{
					switch(tranModule)
					{
						case BatchModule.AP: return new APDocGraphCreator();
						case BatchModule.AR: return new ARDocGraphCreator();
						case BatchModule.CA: return new CADocGraphCreator();
						case BatchModule.DR: return new DRDocGraphCreator();
						case BatchModule.IN: return new INDocGraphCreator();
						case BatchModule.FA: return new FADocGraphCreator();
						case BatchModule.PM: return new PMDocGraphCreator();
						default: return null;
					}
				}
			}
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			GLTranR tran = GLTranEnq.Current;

			if (tran != null)
			{
				Batch batch = JournalEntry.FindBatch(this, tran);

				PXGraph.CreateInstance<JournalEntry>().RedirectToDocumentByTran(tran, batch);
			}

			return Filter.Select();
		}

		public PXAction<AccountByPeriodFilter> ViewReclassBatch;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewReclassBatch(PXAdapter adapter)
		{
			GLTranR tran = GLTranEnq.Current;

			if (tran != null)
			{
				JournalEntry.RedirectToBatch(this, tran.ReclassBatchModule, tran.ReclassBatchNbr);
			}

			return Filter.Select();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable doubleClick(PXAdapter adapter)
		{
			GLTranR tran = GLTranEnq.Current;
			AccountByPeriodFilter filter = Filter.Current;

			if (tran != null && filter != null)
			{
				if (filter.ShowSummary == true)
				{
					SwitchToDetailsOfGroupedRow(filter);
				}
				else
				{
					RedirectToBatch(tran);
				}
			}

			return Filter.Select();
		}

		#endregion
		#endregion
		#region View Delegates

		protected PXFilterRow[] TransactionFilters => GetApplicableFilters(GLTranEnq.View.GetExternalFilters() ?? new PXFilterRow[0]);

		protected virtual IEnumerable glTranEnq()
		{
			AccountByPeriodFilter filter = Filter.Current;

			if (filter.AccountID == null 
				|| filter.LedgerID == null
				|| filter.StartPeriodID == null 
				|| filter.EndPeriodID == null)
				return new GLTranR[0];

			PXUIFieldAttribute.SetVisible<GLTranR.begBalance>(GLTranEnq.Cache, null, true);
			PXUIFieldAttribute.SetVisible<GLTranR.endBalance>(GLTranEnq.Cache, null, true);

			decimal runningBalance = filter.UnsignedBegBal ?? 0m;
			decimal? runningCuryBalance = filter.UnsignedCuryBegBal;

			int startRow = 0;
			int totalRows = 0;

			bool isSyncPosition = PXView.Searches.Any(search => search != null);

			List<GLTranR> satisfiedUpdated = new List<GLTranR>();

			PXNoteAttribute.SetTextFilesActivitiesRequired<GLTranR.noteID>(GLTranEnq.Cache, null); // to prevent repeated database selection instead usage of query cache on grid synchronization

            IEnumerable<GLTranR> records;
			
			int?[] branchIDs = PXAccess.GetBranchIDs().Cast<int?>().ToArray();
			using (new PXReadBranchRestrictedScope(null, branchIDs) { SpecificBranchTable = typeof(GLTran).Name })
			{
				if (filter.ShowSummary == false)
				{
					if (filter.IncludeReclassified == false)
					{
						records = Command.View.Select(
							currents: null, 
							parameters: new object[] {FilteringBranchIDs}, 
							searches: PXView.Searches, 
							sortcolumns: PXView.SortColumns, 
							descendings: PXView.Descendings, 
							filters: TransactionFilters, 
							startRow: ref startRow, 
							maximumRows: isSyncPosition ? PXView.MaximumRows : 0, 
							totalRows: ref totalRows)

							.Cast<PXResult<GLTranR, ADL.Sub, ADL.Account, ADL.Batch, ReclassifyingGLTranAggregate>>()
						.Select(result =>
						{
							GLTranR tran = result;
							ADL.Batch batch = result;
							ADL.Account account = result;

							SetOrigAmounts(tran);

							tran.BatchType = batch.BatchType;
							tran.IncludedInReclassHistory = JournalEntry.CanShowReclassHistory(tran, tran.BatchType);

							PrepareDetailRow(ref runningBalance, ref runningCuryBalance, isSyncPosition, satisfiedUpdated, tran, account);

							if (tran.Reclassified == true)
							{
								ResetAmounts(result, tran);
							}

							return tran;
						});
					}
					else
					{
						records = Command.View.Select(
							currents: null,
							parameters: new object[] { FilteringBranchIDs },
							searches: PXView.Searches,
							sortcolumns: PXView.SortColumns,
							descendings: PXView.Descendings,
							filters: TransactionFilters,
							startRow: ref startRow,
							maximumRows: isSyncPosition ? PXView.MaximumRows : 0,
							totalRows: ref totalRows)

							.Cast<PXResult<GLTranR, ADL.Sub, ADL.Account, ADL.Batch>>()
							.Select(result =>
							{
								GLTranR tran = result;
								ADL.Batch batch = result;
								ADL.Account account = result;

								SetOrigAmounts(tran);

								tran.BatchType = batch.BatchType;
								tran.IncludedInReclassHistory = JournalEntry.CanShowReclassHistory(tran, tran.BatchType);

								PrepareDetailRow(ref runningBalance, ref runningCuryBalance, isSyncPosition, satisfiedUpdated, tran, account);

								if (tran.Reclassified == true)
								{
									tran.CreditAmt = tran.OrigCreditAmt;
									tran.DebitAmt = tran.OrigDebitAmt;
									tran.CuryCreditAmt = tran.CuryOrigCreditAmt;
									tran.CuryDebitAmt = tran.CuryOrigDebitAmt;
								}

								return tran;
							});
					}
				}
				else
				{
					records = Command.View.Select(
						currents: null,
						parameters: new object[] { FilteringBranchIDs },
						searches: PXView.Searches,
						sortcolumns: PXView.SortColumns,
						descendings: PXView.Descendings,
						filters: TransactionFilters,
						startRow: ref startRow,
						maximumRows: isSyncPosition ? PXView.MaximumRows : 0,
						totalRows: ref totalRows)

						.Cast<PXResult<GLTranR, ADL.Sub, ADL.Account, ADL.Batch>>()
						.Select(result =>
						{
							GLTranR tran = result;
							ADL.Batch batch = result;
							ADL.Account account = result;

							tran.BatchType = batch.BatchType;
							tran.IncludedInReclassHistory = JournalEntry.CanShowReclassHistory(tran, tran.BatchType);

							PrepareDetailRow(ref runningBalance, ref runningCuryBalance, isSyncPosition, satisfiedUpdated, tran, account);

							return tran;
						});
				}
				if (!isSyncPosition)
				{
					//Updated (selected in the grid) records are accumulated in the cache.
					//We need to delete records which do not satisfy to current filter conditions.
					GLTranEnq.Cache.Clear();
					satisfiedUpdated.ForEach(tran => GLTranEnq.Cache.SetStatus(tran, PXEntryStatus.Updated));
				}
			}

			return records;
		}

		private void PrepareDetailRow(ref decimal runningBalance, ref decimal? runningCuryBalance, bool isSyncPosition, List<GLTranR> satisfiedUpdated, GLTranR tran, ADL.Account account)
		{
			if (isSyncPosition)
			{
				return;
			}

			if (GLTranEnq.Cache.GetStatus(tran) == PXEntryStatus.Updated)
			{
				satisfiedUpdated.Add(tran);
			}

			tran.Type = account.Type;
			tran.BegBalance = runningBalance;
			runningBalance = tran.EndBalance ?? 0m;

			if (account.CuryID != null)
			{
				tran.CuryID = account.CuryID;
			}
			else
			{
				tran.CuryID = null;
				tran.CuryCreditAmt = null;
				tran.CuryDebitAmt = null;
			}

			if (!string.IsNullOrEmpty(tran.CuryID))
			{
				tran.CuryBegBalance = runningCuryBalance ?? 0m;
				runningCuryBalance = tran.CuryEndBalance ?? 0m;
			}
			GLHistoryEnquiryResult.recalculateSignAmount(tran, glsetup.Current?.TrialBalanceSign == GLSetup.trialBalanceSign.Reversed);
		}

		private void ResetAmounts(PXResult<GLTranR, ADL.Sub, ADL.Account, ADL.Batch, ReclassifyingGLTranAggregate> result, GLTranR tran)
        {
			ReclassifyingGLTranAggregate aggregate = result;

            if ((tran.CuryCreditAmt - aggregate.CuryCreditAmt != 0m) || (tran.CuryDebitAmt - aggregate.CuryDebitAmt != 0m))
            {
                tran.CreditAmt = tran.OrigCreditAmt != 0m ? tran.OrigCreditAmt - (aggregate.DebitAmt ?? 0m) : 0m;
                tran.DebitAmt = tran.OrigDebitAmt != 0m ? tran.OrigDebitAmt - (aggregate.CreditAmt ?? 0m) : 0m;
                tran.CuryCreditAmt = tran.CuryOrigCreditAmt != 0m ? tran.CuryOrigCreditAmt - (aggregate.CuryDebitAmt ?? 0m) : 0m;
                tran.CuryDebitAmt = tran.CuryOrigDebitAmt != 0m ? tran.CuryOrigDebitAmt - (aggregate.CuryCreditAmt ?? 0m) : 0m;
            }
            else
			{
                tran.CreditAmt = tran.OrigCreditAmt;
                tran.DebitAmt = tran.OrigDebitAmt;
                tran.CuryCreditAmt = tran.CuryOrigCreditAmt;
                tran.CuryDebitAmt = tran.CuryOrigDebitAmt;
			}
            tran.SignCuryEndBalance = tran.SignCuryBegBalance + tran.CuryDebitAmt - tran.CuryCreditAmt;
            tran.SignEndBalance = tran.SignBegBalance + tran.DebitAmt - tran.CreditAmt;
		}

		protected void SetVisibleCuryFields(PXCache cache, bool showCurrency)
		{
			PXUIFieldAttribute.SetVisible<GLTranR.curyCreditAmt>(cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLTranR.curyDebitAmt>(cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLTranR.curyBegBalance>(cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLTranR.curyEndBalance>(cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLTranR.signCuryBegBalance>(cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLTranR.signCuryEndBalance>(cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLTranR.curyID>(cache, null, showCurrency);
		}

		protected virtual IEnumerable filter()
		{
			AccountByPeriodFilter filter = Filter.Current;

			SetVisibleCuryFields(GLTranEnq.Cache, filter.ShowCuryDetail == true);
			if (filter.AccountID != null && filter.LedgerID != null && filter.StartPeriodID != null && filter.EndPeriodID != null)
			{
				int startRow = 0;
				int totalRows = 0;
				Account account = AccountInfo.SelectSingle();
				if (account != null)
				{
					string curyID;
					decimal beginningBalance;
					decimal? curyBeginningBalance;
					RetrieveStartingBalance(out beginningBalance, out curyBeginningBalance, out curyID);

					PXNoteAttribute.SetTextFilesActivitiesRequired<GLTranR.noteID>(GLTranEnq.Cache, null); // to prevent repeated database selection instead usage of query cahe on grid selection

					int?[] branchIDs = PXAccess.GetBranchIDs().Cast<int?>().ToArray();
					using (new PXReadBranchRestrictedScope(null, branchIDs) { SpecificBranchTable = typeof(GLTran).Name })
					{
                    decimal turnover = 0m;


                    if (filter.ShowSummary == false)
                    {
						if (filter.IncludeReclassified == false)
						{
							turnover = Command.View.Select(
								currents: null, 
								parameters: new object[] { FilteringBranchIDs },
								searches: new object[PXView.SortColumns.Length],
								sortcolumns: PXView.SortColumns, 
								descendings: PXView.Descendings, 
								filters: TransactionFilters, 
								startRow: ref startRow,
								maximumRows:  0, 
								totalRows: ref totalRows)
								
								.Cast<PXResult<GLTranR, ADL.Sub, ADL.Account, ADL.Batch, ReclassifyingGLTranAggregate>>()
								.Sum(result =>
								{
									GLTranR tran = result;

									SetOrigAmounts(tran);

									if (tran.Reclassified == true)
									{
										ResetAmounts(result, tran);
									}

									return AccountRules.CalcSaldo(account.Type, tran.DebitAmt ?? 0m, tran.CreditAmt ?? 0m);
								});
						}
						else
						{
							turnover = Command.View.Select(
								currents: null,
								parameters: new object[] { FilteringBranchIDs },
								searches: new object[PXView.SortColumns.Length],
								sortcolumns: PXView.SortColumns,
								descendings: PXView.Descendings,
								filters: TransactionFilters,
								startRow: ref startRow,
								maximumRows: 0,
								totalRows: ref totalRows)

								.Cast<PXResult<GLTranR, ADL.Sub, ADL.Account, ADL.Batch>>()
								.Sum(result =>
								{
									GLTranR tran = result;

									SetOrigAmounts(tran);

									if (tran.Reclassified == true)
									{
										tran.CreditAmt = tran.OrigCreditAmt;
										tran.DebitAmt = tran.OrigDebitAmt;
										tran.CuryCreditAmt = tran.CuryOrigCreditAmt;
										tran.CuryDebitAmt = tran.CuryOrigDebitAmt;
									}

									return AccountRules.CalcSaldo(account.Type, tran.DebitAmt ?? 0m, tran.CreditAmt ?? 0m);
								});
						}
                    }
                    else
                    {
						turnover = Command.View.Select(
							currents: null,
							parameters: new object[] { FilteringBranchIDs },
							searches: new object[PXView.SortColumns.Length],
							sortcolumns: PXView.SortColumns,
							descendings: PXView.Descendings,
							filters: TransactionFilters,
							startRow: ref startRow,
							maximumRows: 0,
							totalRows: ref totalRows)
						.RowCast<GLTranR>()
						.Sum(tran => AccountRules.CalcSaldo(account.Type, tran.DebitAmt ?? 0m, tran.CreditAmt ?? 0m));
                    }

					filter.UnsignedBegBal = beginningBalance;
					filter.UnsignedCuryBegBal = curyBeginningBalance;

					int reverseSign = glsetup.Current?.TrialBalanceSign == GLSetup.trialBalanceSign.Reversed 
						&& (account.Type == AccountType.Income || account.Type == AccountType.Liability) ? -1 : 1;

					filter.TurnOver = reverseSign * turnover;

					if (account.Type != AccountType.Income && account.Type != AccountType.Expense
						&& account.AccountID != glsetup.Current?.RetEarnAccountID
						|| string.Equals(filter.StartPeriodID?.Substring(0, 4), filter.EndPeriodID?.Substring(0, 4)))
					{
						filter.BegBal = reverseSign * beginningBalance;
						filter.EndBal = reverseSign * (beginningBalance + turnover);
						Filter.Cache.RaiseExceptionHandling<AccountByPeriodFilter.endBal>(filter, filter.EndBal, null);
					}
					else
					{
						filter.BegBal = 0m;
						filter.EndBal = 0m;
						Filter.Cache.RaiseExceptionHandling<AccountByPeriodFilter.endBal>(filter, filter.EndBal, new PXSetPropertyException(Messages.CantShowBalancesInMultipleYears, PXErrorLevel.Warning));
					}

				Filter.Cache.IsDirty = false;
			}
				}
			}
			yield return filter;

		}
		#endregion
		#region Filter Events Handlers
		protected virtual void AccountByPeriodFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			AccountByPeriodFilter row = e.Row as AccountByPeriodFilter;
			if(row == null) return;

			ShowDetailColumns(row.ShowSummary != true);

			row.IncludeUnreleased = row.IncludeUnposted == true && row.IncludeUnreleased == true;

			PXUIFieldAttribute.SetEnabled<AccountByPeriodFilter.includeUnreleased>(cache, row, row.IncludeUnposted == true);
			PXUIFieldAttribute.SetEnabled<AccountByPeriodFilter.includeReclassified>(cache, row, row.ShowSummary != true);

			if (row.AccountID != null)
			{
				Account acctDef = AccountInfo.Current == null || row.AccountID != AccountInfo.Current.AccountID ? AccountInfo.Select() : AccountInfo.Current;
				bool isDenominated = !string.IsNullOrEmpty(acctDef.CuryID);
				PXUIFieldAttribute.SetEnabled<AccountByPeriodFilter.showCuryDetail>(cache, e.Row, isDenominated);
				if(!isDenominated)
				{
					row.ShowCuryDetail = false;
				}
				if (row.EndDate.HasValue && row.PeriodEndDate.HasValue && row.EndDate > row.PeriodEndDate)
				{
					cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, row.EndDateUI, new PXSetPropertyException(Messages.DateMustBeSetWithingPeriodDatesRange, PXErrorLevel.Warning));
				}
				else
				{
					cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, null, null);
				}
			}

			SetVisibleCuryFields(GLTranEnq.Cache, row.ShowCuryDetail == true);

		    bool showMasterPeriodColumn = Filter.Current?.UseMasterCalendar == true &&
		                                  PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>();


            PXUIFieldAttribute.SetVisible<GLTranR.finPeriodID>(GLTranEnq.Cache, null, !showMasterPeriodColumn);
			PXUIFieldAttribute.SetVisible<GLTranR.tranPeriodID>(GLTranEnq.Cache, null, showMasterPeriodColumn);
			
			if (row.EndDate.HasValue && ((row.PeriodEndDate.HasValue && row.EndDate > row.PeriodEndDate) || (row.PeriodStartDate.HasValue && row.EndDate < row.PeriodStartDate)))
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, row.EndDateUI, new PXSetPropertyException(Messages.DateMustBeSetWithingPeriodDatesRange, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.endDateUI>(e.Row, null, null);
			}

			if (row.StartDate.HasValue && ((row.PeriodStartDate.HasValue && row.StartDate < row.PeriodStartDate) || (row.PeriodEndDate.HasValue && row.StartDate >= row.PeriodEndDate)))
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.startDateUI>(e.Row, row.StartDateUI, new PXSetPropertyException(Messages.DateMustBeSetWithingPeriodDatesRange, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<AccountByPeriodFilter.startDateUI>(e.Row, null, null);
			}
		}
		protected virtual void AccountByPeriodFilter_StartPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;

			if (string.CompareOrdinal(row.StartPeriodID, row.EndPeriodID) > 0)
			{
				cache.SetValue<AccountByPeriodFilter.endPeriodID>(e.Row, row.StartPeriodID);
			}
			ResetFilterDates(row);
		}
		protected virtual void AccountByPeriodFilter_EndPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;
			if (string.CompareOrdinal(row.StartPeriodID, row.EndPeriodID) > 0)
			{
				cache.SetValue<AccountByPeriodFilter.startPeriodID>(e.Row, row.EndPeriodID);
			}
			ResetFilterDates(row);
		}

		protected virtual void _(Events.FieldUpdating<AccountByPeriodFilter.includeReclassified> e)
		{
			// Temporary workaround (until AC-151803 platform fix) because 
			// navigation from the report ignores filter default values.
			if (e.NewValue == null)
			{
				object newValue;
				e.Cache.RaiseFieldDefaulting<AccountByPeriodFilter.includeReclassified>(e.Row, out newValue);
				e.NewValue = newValue;
			}
		}

		protected virtual void AccountByPeriodFilter_UseMasterCalendar_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;
			if (row !=null && (bool?)e.OldValue != row?.UseMasterCalendar)
			{
				ResetFilterDates(row);
			}
		}
		protected virtual void AccountByPeriodFilter_ShowSummary_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AccountByPeriodFilter row = (AccountByPeriodFilter)e.Row;
			if (row.ShowSummary ?? false)
			{
				ResetFilterDates(row);
				row.IncludeReclassified = false;
			}
			GLTranEnq.Cache.Clear();
			GLTranEnq.Cache.ClearQueryCacheObsolete();
		}
		protected virtual void AccountByPeriodFilter_SubID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldUpdated<AccountByPeriodFilter, AccountByPeriodFilter.orgBAccountID> e)
		{
			e.Cache.SetDefaultExt<AccountByPeriodFilter.ledgerID>(e.Row);
		}

		public override bool IsDirty => false;

		#endregion
		#region GLTranR EventHandlers

		protected virtual void GLTranR_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLTranR tran = (GLTranR)e.Row;
			if (tran == null) return;

			JournalEntry.SetReclassTranWarningsIfNeed(sender, tran);

			bool tranSelectableForReclass = !JournalEntry.IsTransactionReclassifiable(tran, tran.BatchType, CurrentLedger.BalanceType, ProjectDefaultAttribute.NonProject()) 
				&& Filter.Current.ShowSummary == false;

			PXUIFieldAttribute.SetReadOnly<GLTran.selected>(sender, tran, tranSelectableForReclass);
		}

		protected virtual void GLTranR_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			reclassify.SetEnabled(GetSelectedTrans().Any());
		}

		#endregion
		#region Utility Functions

		private IReadOnlyCollection<GLTranR> GetSelectedTrans()
		{
			return GLTranEnq.Cache.Updated
				.Cast<GLTranR>()
				.Where(tran => tran.Selected == true)
				.ToArray();
		}

		private PXFilterRow[] GetApplicableFilters(IEnumerable<PXFilterRow> filters)
		{
			return filters
				.Where(f => !(Filter.Current.ShowSummary == true && string.Equals(f.DataField, typeof(GLTranR.batchNbr).Name, StringComparison.OrdinalIgnoreCase)) 
					&& !string.Equals(f.DataField, typeof(GLTranR.begBalance).Name, StringComparison.OrdinalIgnoreCase) 
					&& !string.Equals(f.DataField, typeof(GLTranR.endBalance).Name, StringComparison.OrdinalIgnoreCase) 
					&& !string.Equals(f.DataField, typeof(GLTranR.curyEndBalance).Name, StringComparison.OrdinalIgnoreCase) 
					&& !string.Equals(f.DataField, typeof(GLTranR.curyBegBalance).Name, StringComparison.OrdinalIgnoreCase))
				.ToArray();
		}

		public static void Copy(GLHistoryEnqFilter aDest, AccountByPeriodFilter aSrc)
		{
			aDest.AccountID = aSrc.AccountID;
			aDest.SubCD = aSrc.SubID;
			aDest.LedgerID = aSrc.LedgerID;
			aDest.FinPeriodID = aSrc.StartPeriodID;
			aDest.BranchID = aSrc.BranchID;
			aDest.OrganizationID = aSrc.OrganizationID;
			aDest.OrgBAccountID = aSrc.OrgBAccountID;
			aDest.UseMasterCalendar = aSrc.UseMasterCalendar;
		}

		protected virtual void RetrieveStartingBalance(out decimal balance, out decimal? aCuryBalance, out string aCuryID)
		{
			balance = 0m;
			aCuryBalance = 0m;
			aCuryID = null;
			AccountByPeriodFilter filter = Filter.Current;
			if (filter?.AccountID != null && filter.LedgerID != null)
			{
				AccountHistoryEnq acctHistoryBO = CreateInstance<AccountHistoryEnq>();
				GLHistoryEnqFilter histFilter = acctHistoryBO.Filter.Current;
				Copy(histFilter, filter);
				acctHistoryBO.Filter.Update(histFilter);
				bool isFirst = true;
				foreach (GLHistoryEnquiryResult iRes in acctHistoryBO.EnqResult.Select())
				{
					balance += iRes.BegBalance ?? 0m;
					if (isFirst)
					{
						aCuryID = iRes.CuryID;
						isFirst = false;
					}
					if (aCuryID != null && aCuryID == iRes.CuryID)
					{
						aCuryBalance += iRes.CuryBegBalance;
					}
					else
					{
						aCuryID = null;
						aCuryBalance = null;
					}
				}
				decimal balAdjustment;
				decimal? curyBalAdjustment;
				string adjCuryID = aCuryID;
				RetrieveStartingBalanceAdjustment(out balAdjustment, out curyBalAdjustment, ref adjCuryID);
				balance += balAdjustment;
				if (aCuryID != null && adjCuryID == aCuryID)
				{
					aCuryBalance += curyBalAdjustment;
				}
				else
				{
					aCuryBalance = null;
					aCuryID = null;
				}

			}
		}
		protected virtual void RetrieveStartingBalanceAdjustment(out decimal aAjust, out decimal? aCuryAdjust, ref string aCuryID)
		{
			aAjust = 0m;
			aCuryAdjust = 0m;
			AccountByPeriodFilter filter = Filter.Current;
			if (filter?.AccountID != null && filter.LedgerID != null)
			{
				FinPeriod period = CurrentStartPeriod;
				if (period != null)
				{
					decimal adjCredit = 0m;
					decimal adjDebit = 0m;
					decimal adjCuryDebit = 0m;
					decimal adjCuryCredit = 0m;
					string acctType = AccountType.Expense; //Exact type doesn't metter - only for empty list;
					bool sameCury = true;
					bool isFirst = true;
					if (filter.StartDate.HasValue)
					{
						PXSelectBase<GLTran> cmd = new PXSelectJoinGroupBy<GLTran,
							InnerJoin<ADL.Account, On<GLTran.accountID, Equal<ADL.Account.accountID>>>,
							Where<GLTran.ledgerID, Equal<Current<AccountByPeriodFilter.ledgerID>>,
								And<GLTran.accountID, Equal<Current<AccountByPeriodFilter.accountID>>,
								And<GLTran.finPeriodID, GreaterEqual<Current<AccountByPeriodFilter.startPeriodID>>,
								And<GLTran.finPeriodID, LessEqual<Current<AccountByPeriodFilter.endPeriodID>>,
								And<GLTran.tranDate, Less<Current<AccountByPeriodFilter.startDate>>>>>>>,
							Aggregate<Sum<GLTran.debitAmt,
								Sum<GLTran.creditAmt,
								Sum<GLTran.curyCreditAmt,
								Sum<GLTran.curyDebitAmt,
								GroupBy<GLTran.accountID>>>>>>>(this);

						int[] filteringBranchIDs = FilteringBranchIDs;
						if (filteringBranchIDs != null)
						{
							cmd.WhereAnd<Where<GLTran.branchID, In<Required<AccountByPeriodFilter.branchID>>,
								And<MatchWithBranch<GLTran.branchID>>>>();
						}

						if (!SubCDUtils.IsSubCDEmpty(filter.SubID))
						{
							cmd.Join<InnerJoin<Sub, On<GLTran.subID, Equal<Sub.subID>>>>();
							cmd.WhereAnd<Where<Sub.subCD, Like<Current<AccountByPeriodFilter.subCDWildcard>>>>();
						}

						foreach (PXResult<GLTran, ADL.Account> iRes in cmd.Select(filteringBranchIDs == null ? null : new object[] { filteringBranchIDs }))
						{
							GLTran it = iRes;
							ADL.Account iAcct = iRes;
							adjDebit += it.DebitAmt ?? 0m;
							adjCredit += it.CreditAmt ?? 0m;
							if (isFirst)
							{
								aCuryID = iAcct.CuryID;
								isFirst = false;
							}
							if (sameCury && iAcct.CuryID == aCuryID)
							{
								adjCuryDebit += it.CuryDebitAmt ?? 0m;
								adjCuryCredit += it.CuryCreditAmt ?? 0m;
							}
							else
							{
								sameCury = false;
							}
							acctType = iAcct.Type;
						}
					}
					aAjust = AccountRules.CalcSaldo(acctType, adjDebit, adjCredit);
					if (sameCury)
					{
						aCuryAdjust = AccountRules.CalcSaldo(acctType, adjCuryDebit, adjCuryCredit);
					}
					else
					{
						aCuryAdjust = null;
						aCuryID = null;
					}

				}
			}
		}
		protected virtual void ResetFilterDates(AccountByPeriodFilter aRow)
		{
			int? calendarOrganizationID =
				FinPeriodRepository.GetCalendarOrganizationID(aRow.OrganizationID,
					aRow.BranchID,
					aRow.UseMasterCalendar);

			FinPeriod period = FinPeriodRepository.FindByID(calendarOrganizationID, aRow.StartPeriodID);
			FinPeriod endPeriod = FinPeriodRepository.FindByID(calendarOrganizationID, aRow.EndPeriodID);

			if (period != null && endPeriod != null)
			{
				aRow.PeriodStartDate = period.StartDate <= endPeriod.StartDate ? period.StartDate : endPeriod.StartDate;
				aRow.PeriodEndDate = endPeriod.EndDate >= period.EndDate ? endPeriod.EndDate : period.EndDate;
				aRow.EndDate = null;
				aRow.StartDate = null;
			}
			else if (period != null || endPeriod != null)
			{
				var datesPeriod = period ?? endPeriod;
				aRow.PeriodStartDate = datesPeriod.StartDate;
				aRow.PeriodEndDate = datesPeriod.EndDate;
			}
			else
			{
				aRow.PeriodStartDate = null;
				aRow.PeriodEndDate = null;
			}

		}

		private void ShowDetailColumns(bool needShow)
		{
			PXUIFieldAttribute.SetVisible<GLTranR.module>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.batchNbr>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.accountID>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.subID>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.refNbr>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.tranDesc>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.selected>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.reclassBatchNbr>(GLTranEnq.Cache, null, needShow);
			PXUIFieldAttribute.SetVisible<GLTranR.branchID>(GLTranEnq.Cache, null, needShow);
		}

		private void SwitchToDetailsOfGroupedRow(AccountByPeriodFilter filter)
		{
			GLTranR row = GLTranEnq.Current;
			filter.ShowSummary = false;
			filter.StartDate = new DateTime(row.TranDate.Value.Year, row.TranDate.Value.Month, row.TranDate.Value.Day);
			filter.EndDate = filter.StartDate.Value.AddDays(1);
			Filter.Update(filter);
		}

		private void RedirectToBatch(GLTran tran)
		{
			Batch batch = JournalEntry.FindBatch(this, tran);

			if (batch != null)
			{
				JournalEntry.RedirectToBatch(batch);
			}
		}

		private void SetOrigAmounts(GLTranR tran)
		{
			if (tran.OrigDebitAmt == null || tran.OrigCreditAmt == null || tran.CuryOrigDebitAmt == null || tran.CuryOrigCreditAmt == null)
			{
				tran.OrigDebitAmt = tran.DebitAmt;
				tran.OrigCreditAmt = tran.CreditAmt;
				tran.CuryOrigDebitAmt = tran.CuryDebitAmt;
				tran.CuryOrigCreditAmt = tran.CuryCreditAmt;
			}
		}

		#endregion
	}

	#region DocGraphCreator
	public interface IDocGraphCreator
	{
		PXGraph Create(GLTran tran);
		PXGraph Create(string aTranType, string aRefNbr, int? referenceID);

	}


	public class ARDocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			PXGraph graph = null;
			bool? isInvoiceType = ARDocType.Payable(aTranType);
			bool combined = (aTranType == ARDocType.CashSale) || (aTranType == ARDocType.CashReturn);
			if (combined)
			{
				ARCashSaleEntry destGraph = PXGraph.CreateInstance<ARCashSaleEntry>();
				destGraph.Document.Current = destGraph.Document.Search<ARRegister.refNbr>(aRefNbr, aTranType);
				graph = destGraph;
			}
			else
			{
				if (isInvoiceType.HasValue)
				{
					if (isInvoiceType.Value)
					{
						//Invoice Or CreditAdjustment
						ARInvoiceEntry destGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
						destGraph.Document.Current = destGraph.Document.Search<ARRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;
					}
					else
					{
						//Paymnet Or DebitAdjustment
						ARPaymentEntry destGraph = PXGraph.CreateInstance<ARPaymentEntry>();
						destGraph.Document.Current = destGraph.Document.Search<ARRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;
					}
				}
			}
			return graph;
		}

	}
	public class APDocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			PXGraph graph = null;
			bool? isInvoiceType = APDocType.Payable(aTranType);
			bool combined = (aTranType == APDocType.QuickCheck) || (aTranType == APDocType.VoidQuickCheck);
			if (combined)
			{
				APQuickCheckEntry destGraph = PXGraph.CreateInstance<APQuickCheckEntry>();
				destGraph.Document.Current = destGraph.Document.Search<APRegister.refNbr>(aRefNbr, aTranType);
				graph = destGraph;
			}
			else
			{
				if (isInvoiceType.HasValue)
				{
					if (isInvoiceType.Value)
					{
						//Invoice Or CreditAdjustment
						APInvoiceEntry destGraph = PXGraph.CreateInstance<APInvoiceEntry>();
						destGraph.Document.Current = destGraph.Document.Search<APRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;

					}
					else
					{
						//Paymnet Or DebitAdjustment
						APPaymentEntry destGraph = PXGraph.CreateInstance<APPaymentEntry>();
						destGraph.Document.Current = destGraph.Document.Search<APRegister.refNbr>(aRefNbr, aTranType);
						graph = destGraph;
					}
				}
			}
			return graph;
		}

	}
	public class CADocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			switch(aTranType)
			{
				case CATranType.CATransferIn:
				case CATranType.CATransferOut:
				case CATranType.CATransferExp:
				case CATranType.CATransferRGOL:
				{
					CashTransferEntry destGraph = PXGraph.CreateInstance<CashTransferEntry>();
					destGraph.Transfer.Current = destGraph.Transfer.Search<CATransfer.transferNbr>(aRefNbr);
					return destGraph;
				}
				case CATranType.CAAdjustment:
				{
					CATranEntry destGraph = PXGraph.CreateInstance<CATranEntry>();
					destGraph.CAAdjRecords.Current = destGraph.CAAdjRecords.Search<CAAdj.adjRefNbr, CAAdj.adjTranType>(aRefNbr, aTranType);
					return destGraph;
				}
				case CATranType.CADeposit:
				case CATranType.CAVoidDeposit:
				{
					CADepositEntry destGraph = PXGraph.CreateInstance<CADepositEntry>();
					destGraph.Document.Current = CADeposit.PK.Find(destGraph, aTranType, aRefNbr);
					return destGraph;
				}
				default:
					return null;
			}
		}

	}
	public class DRDocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}
		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
				DraftScheduleMaint destGraph = PXGraph.CreateInstance<DraftScheduleMaint>();
			destGraph.Schedule.Current = destGraph.Schedule.Search<DRSchedule.scheduleNbr>(aRefNbr);
				return destGraph;
			}
	}
	public class FADocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}
		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			if (!string.IsNullOrEmpty(aRefNbr))
			{
				TransactionEntry destGraph = PXGraph.CreateInstance<TransactionEntry>();
				destGraph.Document.Current = destGraph.Document.Search<FARegister.refNbr>(aRefNbr);
				return destGraph;
			}

			return null;
		}
	}
	public class INDocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			//return Create(tran.TranType, tran.RefNbr, null);

			string aTranType = tran.TranType;
			string aRefNbr = tran.RefNbr;

			switch (aTranType)
			{
				case INTranType.Receipt:
				{
					INReceiptEntry destGraph = PXGraph.CreateInstance<INReceiptEntry>();
					destGraph.receipt.Current = destGraph.receipt.Search<INRegister.refNbr>(aRefNbr);
					return destGraph;
				}
				case INTranType.Issue:
				case INTranType.Return:
				case INTranType.DebitMemo:
				case INTranType.CreditMemo:
				case INTranType.Invoice:
				{
					INIssueEntry destGraph = PXGraph.CreateInstance<INIssueEntry>();
					destGraph.issue.Current = destGraph.issue.Search<INRegister.refNbr>(aRefNbr);
					return destGraph;
				}
				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.ReceiptCostAdjustment:
				{
					INAdjustmentEntry destGraph = PXGraph.CreateInstance<INAdjustmentEntry>();
					destGraph.adjustment.Current = destGraph.adjustment.Search<INRegister.refNbr>(aRefNbr);
					return destGraph;
				}
				case INTranType.Transfer:
				{
						var transferGraph = PXGraph.CreateInstance<INTransferEntry>();
						INTran sourcetran = PXSelectReadonly<INTran, Where<INTran.tranType, Equal<Required<INTran.tranType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>>>>.SelectSingleBound(transferGraph, new object[] { }, tran.TranType, tran.RefNbr);

						if (sourcetran != null && sourcetran.DocType == INDocType.Receipt)
						{
							var receiptGraph = PXGraph.CreateInstance<INReceiptEntry>();
							receiptGraph.receipt.Current = receiptGraph.receipt.Search<INRegister.refNbr>(aRefNbr);
							return receiptGraph;
						}
						transferGraph.transfer.Current = transferGraph.transfer.Search<INRegister.refNbr>(aRefNbr);
						return transferGraph;
				}
				case INTranType.Assembly:
				case INTranType.Disassembly:
				{
					KitAssemblyEntry destGraph = PXGraph.CreateInstance<KitAssemblyEntry>();
					destGraph.Document.Current = destGraph.Document.Search<INKitRegister.refNbr>(aRefNbr, aTranType == INTranType.Assembly ? INDocType.Production : INDocType.Disassembly);
					return destGraph;
				}
				default:
					return null;
			}

		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			throw new NotImplementedException();
		}

	}
	public class GLDocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			JournalEntry destGraph = PXGraph.CreateInstance<JournalEntry>();
			destGraph.BatchModule.Current = destGraph.BatchModule.Search<Batch.batchNbr>(aRefNbr);
			return destGraph;
		}

	}
	public class PMDocGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			throw new PXException(Messages.InterfaceMethodNotSupported);
		}

		public virtual PXGraph Create(GLTran tran)
		{
			if (tran.PMTranID != null)
			{
				RegisterEntry destGraph = PXGraph.CreateInstance<RegisterEntry>();

				PMTran pmTran = PXSelect<PMTran, Where<PMTran.tranID, Equal<Required<PMTran.tranID>>>>.Select(destGraph, tran.PMTranID);

				if (pmTran != null)
				{
					destGraph.Document.Current = PXSelect<PMRegister,
						Where<PMRegister.module, Equal<Required<PMRegister.module>>,
							And<PMRegister.refNbr, Equal<Required<PMRegister.refNbr>>>>>.Select(destGraph, pmTran.TranType, pmTran.RefNbr);
					return destGraph;
				}

			}

			return null;
		}
	}

	public class JournalEntryImportGraphCreator : IDocGraphCreator
	{
		public virtual PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public virtual PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			JournalEntryImport destGraph = PXGraph.CreateInstance<JournalEntryImport>();
			destGraph.Map.Current = destGraph.Map.Search<GLTrialBalanceImportMap.number>(aRefNbr);
			return destGraph;
		}

	}
	#endregion
}