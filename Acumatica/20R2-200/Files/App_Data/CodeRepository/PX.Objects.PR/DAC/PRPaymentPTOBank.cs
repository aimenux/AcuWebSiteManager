using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRPaymentPTOBank)]
	public class PRPaymentPTOBank : IBqlTable, PTOHelper.IPTOHistory
	{
		#region DocType
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		[PayrollType.List]
		public string DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, 
			Where<PRPayment.docType, Equal<Current<PRPaymentPTOBank.docType>>,
			And<PRPayment.refNbr, Equal<Current<PRPaymentPTOBank.refNbr>>>>>))]
		public String RefNbr { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region BankID
		[PXDBString(3, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "PTO Bank", Enabled = false)]
		[PXSelector(typeof(SearchFor<PRPTOBank.bankID>), DescriptionField = typeof(PRPTOBank.description))]
		public virtual string BankID { get; set; }
		public abstract class bankID : PX.Data.BQL.BqlString.Field<bankID> { }
		#endregion

		#region EarningTypeCD
		[PXString(2, IsFixed = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Disbursing Earning Type", Visible = false)]
		[PXFormula(typeof(Selector<PRPaymentPTOBank.bankID, PRPTOBank.earningTypeCD>))]
		public virtual string EarningTypeCD { get; set; }
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		#endregion

		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion

		#region IsCertifiedJob
		[PXDBBool]
		[PXUIField(DisplayName = "Applies to Certified Job Only")]
		[PXDefault(false)]
		[PXUIEnabled(typeof(PRPaymentPTOBank.isActive))]
		public virtual bool? IsCertifiedJob { get; set; }
		public abstract class isCertifiedJob : PX.Data.BQL.BqlBool.Field<isCertifiedJob> { }
		#endregion

		#region AccrualRate
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Accrual %")]
		[PXUIEnabled(typeof(PRPaymentPTOBank.isActive))]
		public virtual Decimal? AccrualRate { get; set; }
		public abstract class accrualRate : PX.Data.BQL.BqlDecimal.Field<accrualRate> { }
		#endregion

		#region AccrualLimit
		[PXDBDecimal]
		[PXUIField(DisplayName = "Accrual Limit", Enabled = false)]
		public virtual Decimal? AccrualLimit
		{
			get => _AccrualLimit != 0 ? _AccrualLimit : null;
			set => _AccrualLimit = value;
		}
		private decimal? _AccrualLimit;
		public abstract class accrualLimit : PX.Data.BQL.BqlDecimal.Field<accrualLimit> { }
		#endregion

		#region AccumulatedAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Hours Accrued", Enabled = false)]
		public virtual Decimal? AccumulatedAmount { get; set; }
		public abstract class accumulatedAmount : PX.Data.BQL.BqlDecimal.Field<accumulatedAmount> { }
		#endregion

		#region UsedAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Hours Used", Enabled = false)]
		public virtual Decimal? UsedAmount { get; set; }
		public abstract class usedAmount : PX.Data.BQL.BqlDecimal.Field<usedAmount> { }
		#endregion

		#region AvailableAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Hours Available", Enabled = false)]
		public virtual Decimal? AvailableAmount { get; set; }
		public abstract class availableAmount : PX.Data.BQL.BqlDecimal.Field<availableAmount> { }
		#endregion

		#region AccrualAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Accrual Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? AccrualAmount { get; set; }
		public abstract class accrualAmount : PX.Data.BQL.BqlDecimal.Field<accrualAmount> { }
		#endregion

		#region DisbursementAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Disbursement Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? DisbursementAmount { get; set; }
		public abstract class disbursementAmount : PX.Data.BQL.BqlDecimal.Field<disbursementAmount> { }
		#endregion

		#region ProcessedFrontLoading
		[PXDBBool]
		[PXUIField(DisplayName = "Processed Front Loading", Visible = false)]
		[PXDefault(false)]
		public virtual bool? ProcessedFrontLoading { get; set; }
		public abstract class processedFrontLoading : PX.Data.BQL.BqlBool.Field<processedFrontLoading> { }
		#endregion

		#region FrontLoadingAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Front Loading Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? FrontLoadingAmount { get; set; }
		public abstract class frontLoadingAmount : PX.Data.BQL.BqlDecimal.Field<frontLoadingAmount> { }
		#endregion

		#region ProcessedCarryover
		[PXDBBool]
		[PXUIField(DisplayName = "Processed Carryover", Visible = false)]
		[PXDefault(false)]
		public virtual bool? ProcessedCarryover { get; set; }
		public abstract class processedCarryover : PX.Data.BQL.BqlBool.Field<processedCarryover> { }
		#endregion

		#region CarryoverAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Carryover Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? CarryoverAmount { get; set; }
		public abstract class carryoverAmount : PX.Data.BQL.BqlDecimal.Field<carryoverAmount> { }
		#endregion

		#region ProcessedPaidCarryover
		[PXDBBool]
		[PXUIField(DisplayName = "Processed Paid Carryover", Visible = false)]
		[PXDefault(false)]
		public virtual bool? ProcessedPaidCarryover { get; set; }
		public abstract class processedPaidCarryover : PX.Data.BQL.BqlBool.Field<processedPaidCarryover> { }
		#endregion

		#region PaidCarryoverAmount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Paid Carryover Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? PaidCarryoverAmount { get; set; }
		public abstract class paidCarryoverAmount : PX.Data.BQL.BqlDecimal.Field<paidCarryoverAmount> { }
		#endregion

		#region TotalAccrual
		[PXDecimal]
		[PXUIField(DisplayName = "Total Accrual Hours", Enabled = false)]
		[PXFormula(typeof(Add<Add<PRPaymentPTOBank.accrualAmount, PRPaymentPTOBank.frontLoadingAmount>, PRPaymentPTOBank.carryoverAmount>))]
		public virtual Decimal? TotalAccrual { get; set; }
		public abstract class totalAccrual : PX.Data.BQL.BqlDecimal.Field<totalAccrual> { }
		#endregion

		#region TotalDisbursement
		[PXDecimal]
		[PXUIField(DisplayName = "Total Disbursement Hours", Enabled = false)]
		[PXFormula(typeof(Add<PRPaymentPTOBank.disbursementAmount, PRPaymentPTOBank.paidCarryoverAmount>))]
		public virtual Decimal? TotalDisbursement { get; set; }
		public abstract class totalDisbursement : PX.Data.BQL.BqlDecimal.Field<totalDisbursement> { }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}