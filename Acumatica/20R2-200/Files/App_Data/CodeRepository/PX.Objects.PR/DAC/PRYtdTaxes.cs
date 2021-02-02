using PX.Data;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRYtdTaxes)]
	[Serializable]
	[YTDTaxesAccumulator]
	public class PRYtdTaxes : IBqlTable
	{
		#region Year
		[PXDBString(4, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Year")]
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

		#region Amount
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? Amount { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		#endregion

		#region TaxableWages
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxableWages { get; set; }
		public abstract class taxableWages : PX.Data.BQL.BqlDecimal.Field<taxableWages> { }
		#endregion

		#region MostRecentWH
		[PRCurrency(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? MostRecentWH { get; set; }
		public abstract class mostRecentWH : PX.Data.BQL.BqlDecimal.Field<mostRecentWH> { }
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

	public class YTDTaxesAccumulatorAttribute : PXAccumulatorAttribute
	{
		public YTDTaxesAccumulatorAttribute()
		{
			SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			var record = row as PRYtdTaxes;
			if (record == null)
			{
				return false;
			}

			columns.Update<PRYtdTaxes.mostRecentWH>(record.MostRecentWH, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<PRYtdTaxes.taxableWages>(record.TaxableWages, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}
}