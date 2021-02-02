namespace PX.Objects.PM.BudgetControl
{
	public struct BudgetFilter : IProjectFilter
	{
		public int? AccountGroupID { get; set; }
		public int? ProjectID { get; set; }
		public int? TaskID { get; set; }
		public int? InventoryID { get; set; }
		public int? CostCodeID { get; set; }
	}
}