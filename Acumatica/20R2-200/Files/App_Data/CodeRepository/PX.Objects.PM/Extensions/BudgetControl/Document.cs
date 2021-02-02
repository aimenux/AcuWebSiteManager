using PX.Data;
using System;

namespace PX.Objects.PM.BudgetControl
{
	public class Document : PXMappedCacheExtension
	{
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region WarningAmount
		public abstract class warningAmount : PX.Data.BQL.BqlDecimal.Field<warningAmount> { }
		public virtual decimal? WarningAmount
		{
			get;
			set;
		}
		#endregion
		#region BudgetControlLinesInitialized
		public abstract class budgetControlLinesInitialized : PX.Data.BQL.BqlBool.Field<budgetControlLinesInitialized> { }
		public virtual bool? BudgetControlLinesInitialized
		{
			get;
			set;
		}
		#endregion
	}
}