using System;

namespace PX.Objects.AP
{
	public class ProjectKey
	{
		public int? BranchID { get; set; }
		public int? AccountID { get; set; }
		public int? SubID { get; set; }
		public int? ProjectID { get; set; }
		public int? TaskID { get; set; }
		public int? CostCodeID { get; set; }
		public int? InventoryID { get; set; }

		public ProjectKey(int? branchID, int? accountID, int? subID, int? projectID, int? taskID, int? costCodeID, int? inventoryID)
		{
			BranchID = branchID;
			AccountID = accountID;
			SubID = subID;
			ProjectID = projectID;
			TaskID = taskID;
			CostCodeID = costCodeID;
			InventoryID = inventoryID;
		}

		public override int GetHashCode()
		{
			return
				BranchID.GetHashCode() ^
				AccountID.GetHashCode() ^
				SubID.GetHashCode() ^
				ProjectID.GetHashCode() ^
				TaskID.GetHashCode() ^
				CostCodeID.GetHashCode() ^
				InventoryID.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ProjectKey))
				return false;

			var projectKey = (ProjectKey)obj;
			return 
				(this.BranchID == projectKey.BranchID) &&
				(this.AccountID == projectKey.AccountID) &&
				(this.SubID == projectKey.SubID) &&
				(this.ProjectID == projectKey.ProjectID) &&
				(this.TaskID == projectKey.TaskID) &&
				(this.CostCodeID == projectKey.CostCodeID) &&
				(this.InventoryID == projectKey.InventoryID);
		}
	}
}
