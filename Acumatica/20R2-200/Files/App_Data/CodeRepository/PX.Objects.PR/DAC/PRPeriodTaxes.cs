using PX.Data;
using PX.Data.BQL.Fluent;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRPeriodTaxes)]
	[Serializable]
	[PeriodTaxesAccumulator]
	public class PRPeriodTaxes : IBqlTable
	{
		#region Year
		[PXDBString(4, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Year")]
		[PXParent(typeof(
			Select<PRYtdTaxes,
				Where<PRYtdTaxes.year, Equal<Current<PRPeriodTaxes.year>>,
				And<PRYtdTaxes.employeeID, Equal<Current<PRPeriodTaxes.employeeID>>,
				And<PRYtdTaxes.taxID, Equal<Current<PRPeriodTaxes.taxID>>>>>>))]
		public virtual string Year { get; set; }
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		#endregion

		#region EmployeeID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee")]
		public virtual int? EmployeeID { get; set; }
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		#endregion

		#region TaxID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Tax Code")]
		public virtual int? TaxID { get; set; }
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		#endregion

		#region PeriodNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Period")]
		public virtual int? PeriodNbr { get; set; }
		public abstract class periodNbr : PX.Data.BQL.BqlInt.Field<periodNbr> { }
		#endregion

		#region Amount
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXFormula(null, typeof(SumCalc<PRYtdTaxes.amount>))]
		public virtual decimal? Amount { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		#endregion

		#region Week
		[PXDBInt]
		[PXUIField(DisplayName = "Week")]
		public virtual int? Week { get; set; }
		public abstract class week : PX.Data.BQL.BqlInt.Field<week> { }
		#endregion

		#region Month
		[PXDBInt]
		[PXUIField(DisplayName = "Month")]
		public virtual int? Month { get; set; }
		public abstract class month : PX.Data.BQL.BqlInt.Field<month> { }
		#endregion

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}

	public class PeriodTaxesAccumulatorAttribute : PXAccumulatorAttribute
	{
		public PeriodTaxesAccumulatorAttribute()
		{
			SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			var record = row as PRPeriodTaxes;
			if (record == null)
			{
				return false;
			}

			columns.Update<PRPeriodTaxes.amount>(record.Amount, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}
}