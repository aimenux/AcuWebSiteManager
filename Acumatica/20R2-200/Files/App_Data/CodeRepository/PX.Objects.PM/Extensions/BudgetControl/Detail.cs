using PX.Data;

namespace PX.Objects.PM.BudgetControl
{
	public class Detail : PXMappedCacheExtension
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		public virtual int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		public virtual int? CostCodeID
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
	}
}