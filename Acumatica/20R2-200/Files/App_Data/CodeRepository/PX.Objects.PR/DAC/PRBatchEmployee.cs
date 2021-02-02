using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRBatchEmployee)]
	[Serializable]
	public class PRBatchEmployee : IBqlTable
	{
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr))]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<PRBatchEmployee.batchNbr>>>>))]
		public string BatchNbr { get; set; }
		#endregion
		#region BatchStatus
		public abstract class batchStatus : PX.Data.BQL.BqlString.Field<batchStatus> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(Parent<PRBatch.status>))]
		[BatchStatus.List]
		public virtual string BatchStatus { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[EmployeeActiveInPayGroup(IsKey = true)]
		[PXFormula(null, typeof(CountCalc<PRBatch.numberOfEmployees>))]
		[PXForeignReference(typeof(Field<PRBatchEmployee.employeeID>.IsRelatedTo<PREmployee.bAccountID>))]
		public int? EmployeeID { get; set; }
		#endregion
		#region EmpType
		public abstract class empType : PX.Data.BQL.BqlString.Field<empType> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(Visible = false)]
		[PXUnboundDefault(typeof(Selector<employeeID, PREmployee.empType>))]
		[EmployeeType.List]
		public virtual string EmpType { get; set; }
		#endregion
		#region HourQty
		public abstract class hourQty : PX.Data.BQL.BqlDecimal.Field<hourQty> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Hours")]
		[PXFormula(null, typeof(SumCalc<PRBatch.totalHourQty>))]
		public virtual Decimal? HourQty { get; set; }
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Rate")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(amount.Divide<hourQty>.When<hourQty.IsGreater<decimal0>>.Else<decimal0>))]
		public Decimal? Rate { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<PRBatch.totalEarnings>))]
		public virtual Decimal? Amount { get; set; }
		#endregion
		#region SalariedNonExempt
		public abstract class salariedNonExempt : PX.Data.BQL.BqlBool.Field<salariedNonExempt> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Salaried Non-Exempt")]
		[PXUIEnabled(typeof(Where<empType.IsEqual<EmployeeType.salariedNonExempt>.And<batchStatus.IsEqual<BatchStatus.hold>.Or<batchStatus.IsEqual<BatchStatus.balanced>>>>))]
		[PXDefault(typeof(True.When<empType.IsEqual<EmployeeType.salariedNonExempt>.And<Where<Parent<PRBatch.payrollType>, Equal<PayrollType.regular>>>>.Else<False>))]
		public virtual bool? SalariedNonExempt { get; set; }
		#endregion
		#region RegularAmount
		public abstract class regularAmount : PX.Data.BQL.BqlDecimal.Field<regularAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Regular Amount to be paid")]
		[PXUIEnabled(typeof(Where<batchStatus.IsEqual<BatchStatus.hold>.Or<batchStatus.IsEqual<BatchStatus.balanced>>>))]
		[BatchEmployeeRegularAmount]
		public virtual Decimal? RegularAmount { get; set; }
		#endregion
		#region ManualRegularAmount
		public abstract class manualRegularAmount : PX.Data.BQL.BqlBool.Field<manualRegularAmount> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Manual Amount")]
		[PXUIEnabled(typeof(Where<batchStatus.IsEqual<BatchStatus.hold>.Or<batchStatus.IsEqual<BatchStatus.balanced>>>))]
		[PXDefault(false)]
		public virtual bool? ManualRegularAmount { get; set; }
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