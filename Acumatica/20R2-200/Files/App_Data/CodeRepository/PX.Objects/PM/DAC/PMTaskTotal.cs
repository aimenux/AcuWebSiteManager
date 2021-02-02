using PX.Data;
using PX.Objects.CM;
using System;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.TaskTotal)]
	[TaskTotalAccum]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMTaskTotal : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected Int32? _ProjectID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID>
		{
		}
		protected Int32? _TaskID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion

		#region CuryAsset
		public abstract class curyAsset : PX.Data.BQL.BqlDecimal.Field<curyAsset> { }
		[PXDBCurrency(typeof(PMProject.curyInfoID), typeof(asset), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Asset", Enabled = false)]
		public virtual decimal? CuryAsset
		{
			get;
			set;
		}
		#endregion
		#region Asset
		public abstract class asset : PX.Data.BQL.BqlDecimal.Field<asset> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Asset
		{
			get;
			set;
		}
		#endregion
		#region CuryLiability
		public abstract class curyLiability : PX.Data.BQL.BqlDecimal.Field<curyLiability> { }
		[PXDBCurrency(typeof(PMProject.curyInfoID), typeof(liability), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Liability", Enabled = false)]
		public virtual decimal? CuryLiability
		{
			get;
			set;
		}
		#endregion
		#region Liability
		public abstract class liability : PX.Data.BQL.BqlDecimal.Field<liability> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Liability
		{
			get;
			set;
		}
		#endregion
		#region CuryIncome
		public abstract class curyIncome : PX.Data.BQL.BqlDecimal.Field<curyIncome> { }
		[PXDBCurrency(typeof(PMProject.curyInfoID), typeof(income), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Income", Enabled = false)]
		public virtual decimal? CuryIncome
		{
			get;
			set;
		}
		#endregion
		#region Income
		public abstract class income : PX.Data.BQL.BqlDecimal.Field<income>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Income
		{
			get;
			set;
		}
		#endregion
		#region CuryExpense
		public abstract class curyExpense : PX.Data.BQL.BqlDecimal.Field<curyExpense> { }
		[PXDBCurrency(typeof(PMProject.curyInfoID), typeof(expense), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Expense", Enabled = false)]
		public virtual decimal? CuryExpense
		{
			get;
			set;
		}
		#endregion
		#region Expense
		public abstract class expense : PX.Data.BQL.BqlDecimal.Field<expense> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Expense
		{
			get;
			set;
		}
		#endregion
		#region CuryMargin
		public abstract class curyMargin : Data.BQL.BqlDecimal.Field<curyMargin> { }
		/// <summary>
		/// Margin in Project Currency
		/// </summary>
		[PXCurrency(typeof(PMProject.curyInfoID), typeof(margin))]
		[PXUIField(DisplayName = "Margin", Enabled = false)]
		[PXFormula(typeof(Sub<curyIncome, curyExpense>))]
		public virtual decimal? CuryMargin
		{
			get;
			set;
		}
		#endregion
		#region Margin
		public abstract class margin : Data.BQL.BqlDecimal.Field<margin> { }
		/// <summary>
		/// Margin in Base Currency
		/// </summary>
		[PXBaseCury]
		public virtual decimal? Margin
		{
			get;
			set;
		}
		#endregion
		#region MarginPct
		public abstract class marginPct : Data.BQL.BqlDecimal.Field<marginPct> { }
		/// <summary>
		/// Margin in percents
		/// </summary>
		[PXDecimal]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual decimal? MarginPct
		{
			[PXDependsOnFields(typeof(curyIncome), typeof(curyExpense))]
			get
			{
				var result = CuryIncome == 0 ? 0 : 100 * (CuryIncome - CuryExpense) / CuryIncome;
				return result;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
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
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class TaskTotalAccumAttribute : PXAccumulatorAttribute
	{
		public TaskTotalAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			PMTaskTotal item = (PMTaskTotal)row;

			columns.Update<PMTaskTotal.curyAsset>(item.CuryAsset, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskTotal.asset>(item.Asset, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskTotal.curyLiability>(item.CuryLiability, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskTotal.liability>(item.Liability, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskTotal.curyIncome>(item.CuryIncome, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskTotal.income>(item.Income, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskTotal.curyExpense>(item.CuryExpense, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskTotal.expense>(item.Expense, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}
}