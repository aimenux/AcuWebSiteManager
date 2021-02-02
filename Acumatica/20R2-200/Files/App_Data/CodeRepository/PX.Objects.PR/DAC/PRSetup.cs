using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRSetup)]
	public partial class PRSetup : IBqlTable
	{
		#region BatchNumberingID
		public abstract class batchNumberingID : IBqlField
		{
		}
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual String BatchNumberingID
		{
			get
			{
				return this._BatchNumberingID;
			}
			set
			{
				this._BatchNumberingID = value;
			}
		}
		#endregion
		#region BatchNumberingCD
		public abstract class batchNumberingCD : PX.Data.IBqlField { }
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Payroll Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual string BatchNumberingCD { get; set; }
		#endregion
		#region TranNumberingCD
		public abstract class tranNumberingCD : PX.Data.IBqlField { }
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Transaction Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual string TranNumberingCD { get; set; }
		#endregion
		#region UpdateGL
		public abstract class updateGL : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXUIField(DisplayName = "Update GL")]
		[PXDefault(false)]
		public virtual bool? UpdateGL { get; set; }
		#endregion
		#region SummPost
		public abstract class summPost : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Summary on Updating GL")]
		public virtual bool? SummPost { get; set; }
		#endregion
		#region AutoPost
		public abstract class autoPost : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Post on Release")]
		public virtual bool? AutoPost { get; set; }
		#endregion
		#region DisableGLWarnings
		public abstract class disableGLWarnings : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable GL Account Warnings on Payment Release")]
		public virtual bool? DisableGLWarnings { get; set; }
		#endregion
		#region PayPeriodDateChangeAllowed
		public abstract class payPeriodDateChangeAllowed : PX.Data.BQL.BqlBool.Field<payPeriodDateChangeAllowed> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Allow Changing Pay Period Dates")]
		[PXDefault(false)]
		public virtual bool? PayPeriodDateChangeAllowed { get; set; }
		#endregion
		#region PayRateDecimalPlaces
		public abstract class payRateDecimalPlaces : PX.Data.IBqlField { }
		[PXDBShort(MinValue = 0, MaxValue = 6)]
		[PXDefault((short)2)]
		[PXUIField(DisplayName = "Pay Rate Decimal Places")]
		public virtual short? PayRateDecimalPlaces { get; set; }
		#endregion
		#region EarningsAcctDefault
		public abstract class earningsAcctDefault : PX.Data.BQL.BqlString.Field<earningsAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Earnings Account from")]
		[PREarningsAcctSubDefault.AcctList]
		[PXDefault(PREarningsAcctSubDefault.MaskEarningType)]
		[ExpenseAcctSubVerifier(typeof(earningsSubMask), PREarningsAcctSubDefault.MaskEarningType, PREarningsAcctSubDefault.MaskLaborItem)]
		public virtual String EarningsAcctDefault { get; set; }
		#endregion
		#region EarningsSubMask
		public abstract class earningsSubMask : PX.Data.BQL.BqlString.Field<earningsSubMask> { }
		[PXDefault]
		[PREarningsSubAccountMask(DisplayName = "Combine Earnings Sub. From")]
		public virtual String EarningsSubMask { get; set; }
		#endregion
		#region DeductLiabilityAcctDefault
		public abstract class deductLiabilityAcctDefault : PX.Data.BQL.BqlString.Field<deductLiabilityAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Deduction Liability Account from")]
		[PRDeductAcctSubDefault.AcctList]
		[PXDefault(PRDeductAcctSubDefault.MaskDeductionCode)]
		public virtual String DeductLiabilityAcctDefault { get; set; }
		#endregion
		#region DeductLiabilitySubMask
		public abstract class deductLiabilitySubMask : PX.Data.BQL.BqlString.Field<deductLiabilitySubMask> { }
		[PXDefault]
		[PRDeductSubAccountMask(DisplayName = "Combine Deduction Liability Sub. From")]
		public virtual String DeductLiabilitySubMask { get; set; }
		#endregion
		#region BenefitExpenseAcctDefault
		public abstract class benefitExpenseAcctDefault : PX.Data.BQL.BqlString.Field<benefitExpenseAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Benefit Expense Account from")]
		[PRBenefitExpenseAcctSubDefault.AcctList]
		[PXDefault(PRDeductAcctSubDefault.MaskDeductionCode)]
		[ExpenseAcctSubVerifier(typeof(benefitExpenseSubMask), PRBenefitExpenseAcctSubDefault.MaskEarningType, PRBenefitExpenseAcctSubDefault.MaskLaborItem)]
		public virtual String BenefitExpenseAcctDefault { get; set; }
		#endregion
		#region BenefitExpenseSubMask
		public abstract class benefitExpenseSubMask : PX.Data.BQL.BqlString.Field<benefitExpenseSubMask> { }
		[PXDefault]
		[PRBenefitExpenseSubAccountMask(DisplayName = "Combine Benefit Expense Sub. From")]
		public virtual String BenefitExpenseSubMask { get; set; }
		#endregion
		#region BenefitLiabilityAcctDefault
		public abstract class benefitLiabilityAcctDefault : PX.Data.BQL.BqlString.Field<benefitLiabilityAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Benefit Liability Account from")]
		[PRDeductAcctSubDefault.AcctList]
		[PXDefault(PRDeductAcctSubDefault.MaskDeductionCode)]
		public virtual String BenefitLiabilityAcctDefault { get; set; }
		#endregion
		#region BenefitLiabilitySubMask
		public abstract class benefitLiabilitySubMask : PX.Data.BQL.BqlString.Field<benefitLiabilitySubMask> { }
		[PXDefault]
		[PRDeductSubAccountMask(DisplayName = "Combine Benefit Liability Sub. From")]
		public virtual String BenefitLiabilitySubMask { get; set; }
		#endregion
		#region TaxExpenseAcctDefault
		public abstract class taxExpenseAcctDefault : PX.Data.BQL.BqlString.Field<taxExpenseAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Tax Expense Account from")]
		[PRTaxExpenseAcctSubDefault.AcctList]
		[PXDefault(PRTaxAcctSubDefault.MaskTaxCode)]
		[ExpenseAcctSubVerifier(typeof(taxExpenseSubMask), PRTaxExpenseAcctSubDefault.MaskEarningType, PRTaxExpenseAcctSubDefault.MaskLaborItem)]
		public virtual String TaxExpenseAcctDefault { get; set; }
		#endregion
		#region TaxExpenseSubMask
		public abstract class taxExpenseSubMask : PX.Data.BQL.BqlString.Field<taxExpenseSubMask> { }
		[PXDefault]
		[PRTaxExpenseSubAccountMask(DisplayName = "Combine Tax Expense Sub. From")]
		public virtual String TaxExpenseSubMask { get; set; }
		#endregion
		#region TaxLiabilityAcctDefault
		public abstract class taxLiabilityAcctDefault : PX.Data.BQL.BqlString.Field<taxLiabilityAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Tax Liability Account from")]
		[PRTaxAcctSubDefault.AcctList]
		[PXDefault(PRTaxAcctSubDefault.MaskTaxCode)]
		public virtual String TaxLiabilityAcctDefault { get; set; }
		#endregion
		#region TaxLiabilitySubMask
		public abstract class taxLiabilitySubMask : PX.Data.BQL.BqlString.Field<taxLiabilitySubMask> { }
		[PXDefault]
		[PRTaxSubAccountMask(DisplayName = "Combine Tax Liability Sub. From")]
		public virtual String TaxLiabilitySubMask { get; set; }
		#endregion
		#region SummarizeTimeCard
		public abstract class summarizeTimeCard : PX.Data.BQL.BqlBool.Field<summarizeTimeCard> { }
		[Obsolete]
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Summarize Time Card Data", Visible = false)]
		public virtual bool? SummarizeTimeCard { get; set; }
		#endregion
		#region RegularHoursType
		public abstract class regularHoursType : PX.Data.BQL.BqlString.Field<regularHoursType> { }
		[PXDBString(2, IsUnicode = true, IsFixed = true)]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>.
			Where<EPEarningType.isActive.IsEqual<True>.
				And<EPEarningType.isOvertime.IsNotEqual<True>>.
				And<PREarningType.isPiecework.IsNotEqual<True>>.
				And<PREarningType.isAmountBased.IsNotEqual<True>>.
				And<PREarningType.isPTO.IsNotEqual<True>>>), 
			DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Regular Hours Earning Type for Quick Pay")]
		public virtual string RegularHoursType { get; set; }
		#endregion
		#region HolidaysType
		public abstract class holidaysType : PX.Data.BQL.BqlString.Field<holidaysType> { }
		[PXDBString(2, IsUnicode = true, IsFixed = true)]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>.
			Where<EPEarningType.isActive.IsEqual<True>.
				And<EPEarningType.typeCD.IsNotInSubselect<SearchFor<PRPTOBank.earningTypeCD>>>.
				And<PREarningType.isPTO.IsEqual<True>>>), 
			DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Holiday Earning Type for Quick Pay")]
		public virtual string HolidaysType { get; set; }
		#endregion
		#region CommissionType
		public abstract class commissionType : PX.Data.BQL.BqlString.Field<commissionType> { }
		[PXDBString(2, IsUnicode = true, IsFixed = true)]
		[PXUIField(DisplayName = "Commission Earning Type")]
		[PXSelector(typeof(SelectFrom<EPEarningType>.
				Where<PREarningType.isAmountBased.IsEqual<True>>.
				OrderBy<EPEarningType.typeCD.Asc>.
				SearchFor<EPEarningType.typeCD>), 
			typeof(EPEarningType.typeCD), typeof(EPEarningType.description), 
			SelectorMode = PXSelectorMode.MaskAutocomplete, 
			DescriptionField = typeof(EPEarningType.description))]
		public virtual string CommissionType { get; set; }
		#endregion
		#region LeaveType
		public abstract class leaveType : PX.Data.BQL.BqlString.Field<leaveType> { }
		[Obsolete]
		[PXDBString(2, IsUnicode = true)]
		[PXUIField(DisplayName = "Leave Earning Type", Visible = false)]
		public virtual string LeaveType { get; set; }
		#endregion
		#region EnablePieceworkEarningType
		public abstract class enablePieceworkEarningType : PX.Data.BQL.BqlBool.Field<enablePieceworkEarningType> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Piecework as an Earning Type")]
		public virtual bool? EnablePieceworkEarningType { get; set; }
		#endregion
		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Hold Paycheck on Entry")]
		[PXDefault(false)]
		public virtual bool? HoldEntry { get; set; }
		#endregion
		#region NoWeekendTransactionDate
		public abstract class noWeekendTransactionDate : PX.Data.BQL.BqlBool.Field<noWeekendTransactionDate> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Transaction Date cannot be on weekend")]
		public virtual bool? NoWeekendTransactionDate { get; set; }
		#endregion
		#region HideEmployeeInfo
		public abstract class hideEmployeeInfo : PX.Data.BQL.BqlBool.Field<hideEmployeeInfo> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Hide Employee Name on Bank Reconciliation")]
		[PXDefault(false)]
		public virtual bool? HideEmployeeInfo { get; set; }
		#endregion HideEmployeeInfo
		#region ProjectCostAssignment
		public abstract class projectCostAssignment : PX.Data.BQL.BqlString.Field<projectCostAssignment> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(ProjectCostAssignmentType.WageCostAssigned)]
		[PXUIField(DisplayName = "Project Cost Assignment")]
		[ProjectCostAssignmentType.List]
		public virtual string ProjectCostAssignment { get; set; }
		#endregion
		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
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
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
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
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
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
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
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
		#endregion
	}
}