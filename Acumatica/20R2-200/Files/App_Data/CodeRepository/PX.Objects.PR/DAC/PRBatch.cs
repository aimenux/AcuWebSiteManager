using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRBatch)]
	[PXPrimaryGraph(typeof(PRPayBatchEntry))]
	[Serializable]
	public class PRBatch : IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Batch ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(PRSetup.batchNumberingCD), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search<PRBatch.batchNbr>), SelectorMode = PXSelectorMode.DisplayMode)]
		public string BatchNbr { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PR.BatchStatus.Hold)]
		[BatchStatus.List]
		[SetBatchStatus]
		public string Status { get; set; }
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		/// <summary>
		/// When set to <c>true</c> indicates that the document is on hold and thus cannot be released.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Hold { get; set; }
		#endregion
		#region Open
		public abstract class open : PX.Data.BQL.BqlBool.Field<open> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Open { get; set; }
		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Closed { get; set; }
		#endregion
		#region PayrollType
		public abstract class payrollType : PX.Data.BQL.BqlString.Field<payrollType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(PR.PayrollType.Regular)]
		[PXUIField(DisplayName = "Payroll Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PayrollType.BatchList]
		public string PayrollType { get; set; }
		#endregion
		#region PayGroupID
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Pay Group", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXUIEnabled(typeof(Where<numberOfEmployees.IsNull.Or<numberOfEmployees.IsEqual<Zero>>>))]
		[PXSelector(typeof(Search<PRPayGroup.payGroupID>), DescriptionField = typeof(PRPayGroup.description))]
		public string PayGroupID { get; set; }
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DocDesc { get; set; }
		#endregion
		#region IsWeekOrBiWeekPeriod
		public abstract class isWeeklyOrBiWeeklyPeriod : PX.Data.BQL.BqlBool.Field<isWeeklyOrBiWeeklyPeriod> { }
		[PXBool]
		[PXFormula(typeof(Default<payGroupID>))]
		[PXUnboundDefault(typeof(SearchFor<PRPayGroupYearSetup.isWeeklyOrBiWeeklyPeriod>.Where<PRPayGroupYearSetup.payGroupID.IsEqual<payGroupID.FromCurrent>>))]
		[PXUIField(DisplayName = "Is Weekly or Biweekly Period", Visible = false)]
		public bool? IsWeeklyOrBiWeeklyPeriod { get; set; }
		#endregion
		#region PayPeriodID
		public abstract class payPeriodID : PX.Data.BQL.BqlString.Field<payPeriodID> { }
		[PXUIField(DisplayName = "Pay Period", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXUIEnabled(typeof(Where<numberOfEmployees.IsNull.Or<numberOfEmployees.IsEqual<Zero>>>))]
		[PRPayGroupPeriodID(typeof(payGroupID), typeof(startDate), null, typeof(endDate), typeof(transactionDate), false,
			typeof(Where<payrollType.IsEqual<PayrollType.special>.And<numberOfEmployees.IsNull.Or<numberOfEmployees.IsEqual<Zero>>>>))]
		public string PayPeriodID { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Period Start", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? StartDate { get; set; }
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Period End", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? EndDate { get; set; }
		#endregion
		#region TransactionDate
		public abstract class transactionDate : PX.Data.BQL.BqlDateTime.Field<transactionDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? TransactionDate { get; set; }
		#endregion
		#region NumberOfEmployees
		public abstract class numberOfEmployees : PX.Data.BQL.BqlInt.Field<numberOfEmployees> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Number of Employees", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Int32? NumberOfEmployees { set; get; }
		#endregion
		#region TotalHourQty
		public abstract class totalHourQty : PX.Data.BQL.BqlInt.Field<totalHourQty> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Total Hour Qty", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? TotalHourQty { set; get; }
		#endregion
		#region ApplyOvertimeRules
		public abstract class applyOvertimeRules : PX.Data.BQL.BqlBool.Field<applyOvertimeRules> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Apply Overtime Rules for the Document")]
		[PXUIEnabled(typeof(status.IsEqual<BatchStatus.hold>.Or<status.IsEqual<BatchStatus.balanced>>))]
		public virtual bool? ApplyOvertimeRules { get; set; }
		#endregion
		#region TotalEarnings
		public abstract class totalEarnings : PX.Data.BQL.BqlDecimal.Field<totalEarnings> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Total Earnings", Enabled = false)]
		public virtual decimal? TotalEarnings { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : IBqlField { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}