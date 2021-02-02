using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{

	public class Grouping
	{
		IComparer<PMTran> comparer;

		public Grouping(IComparer<PMTran> comparer)
		{
			this.comparer = comparer;
		}

		public virtual List<Group> BreakIntoGroups(List<PMTran> list)
		{
			list.Sort(comparer);

			List<Group> groups = new List<Group>();

			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					if (comparer.Compare(list[i], list[i - 1]) == 0)
					{
						groups[groups.Count - 1].List.Add(list[i]);
					}
					else
					{
						Group nextGroup = new Group();
						nextGroup.List.Add(list[i]);
						groups.Add(nextGroup);
					}
				}
				else
				{
					Group firstGroup = new Group();
					firstGroup.List.Add(list[i]);
					groups.Add(firstGroup);
				}
			}

			return groups;
		}
	}

	public class Group
	{
		public List<PMTran> List { get; private set; }

		private bool statReady = false;
		private bool hasMixedInventory = false;
		private bool hasMixedUOM = false;
		private bool hasMixedBAccount = false;
		private bool hasMixedBAccountLoc = false;
		private bool hasMixedDescription = false;
		private bool hasMixedAccountID = false;
		private bool hasMixedSubID = false;

		public Group()
		{
			List = new List<PMTran>();
		}

		public bool HasMixedInventory
		{
			get
			{
				if (!statReady)
					InitStatistics();

				return hasMixedInventory;
			}
		}

		public bool HasMixedUOM
		{
			get
			{
				if (!statReady)
					InitStatistics();

				return hasMixedUOM;
			}
		}

		public bool HasMixedBAccount
		{
			get
			{
				if (!statReady)
					InitStatistics();

				return hasMixedBAccount;
			}
		}

		public bool HasMixedBAccountLoc
		{
			get
			{
				if (!statReady)
					InitStatistics();

				return hasMixedBAccountLoc;
			}
		}

		public bool HasMixedDescription
		{
			get
			{
				if (!statReady)
					InitStatistics();

				return hasMixedDescription;
			}
		}

		public bool HasMixedAccountID
		{
			get
			{
				if (!statReady)
					InitStatistics();

				return hasMixedAccountID;
			}
		}

		public bool HasMixedSubID
		{
			get
			{
				if (!statReady)
					InitStatistics();

				return hasMixedSubID;
			}
		}

		private void InitStatistics()
		{
			if (List.Count > 0)
			{
				int lastInvetoryID = List[0].InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
				string lastUOM = List[0].UOM;
				int? lastBAccountID = List[0].BAccountID;
				int? lastLocationID = List[0].LocationID;
				string lastDescription = List[0].InvoicedDescription;
				int? lastAccountID = List[0].AccountID;
				int? lastSubID = List[0].SubID;

				for (int i = 1; i < List.Count; i++)
				{
					if (lastInvetoryID != List[i].InventoryID)
					{
						hasMixedInventory = true;
					}

					if (string.IsNullOrEmpty(lastUOM))
					{
						lastUOM = List[i].UOM;
					}
					else if (!string.IsNullOrEmpty(List[i].UOM))
					{
						if (lastUOM != List[i].UOM)
						{
							hasMixedUOM = true;
						}
					}

					if (lastBAccountID != List[i].BAccountID)
					{
						hasMixedBAccount = true;
					}

					if (lastLocationID != List[i].LocationID)
					{
						hasMixedBAccountLoc = true;
					}

					if (lastDescription != List[i].InvoicedDescription)
					{
						hasMixedDescription = true;
					}

					if (lastAccountID != List[i].AccountID)
					{
						hasMixedAccountID = true;
					}

					if (lastSubID != List[i].SubID)
					{
						hasMixedSubID = true;
					}
				}

				statReady = true;
			}
		}

		
	}

	public class PMTranComparer : IComparer<PMTran>
	{
		private bool ByItem;
		private bool ByEmployee;
		private bool ByVendor;
		private bool ByDate;
		private bool ByAccountGroup;

		public PMTranComparer(bool? byItem, bool? byVendor, bool? byDate, bool? byEmployee, bool byAccountGroup)
		{
			this.ByItem = byItem == true;
			this.ByEmployee = byEmployee == true;
			this.ByVendor = byVendor == true;
			this.ByDate = byDate == true;
			this.ByAccountGroup = byAccountGroup;
		}

		public int Compare(PMTran x, PMTran y)
		{
			//always compare by branch, finperiod and tranCuryID:

			if (x.BranchID != y.BranchID)
				return x.BranchID.Value.CompareTo(y.BranchID.Value);

			if (x.FinPeriodID != y.FinPeriodID)
				return x.FinPeriodID.CompareTo(y.FinPeriodID);

			if (x.TranCuryID != y.TranCuryID)
				return x.TranCuryID.CompareTo(y.TranCuryID);


			if (ByAccountGroup)
			{
				int xAG = x.AccountGroupID ?? 0;
				int yAG = y.AccountGroupID ?? 0;

				if (xAG != yAG)
					return xAG.CompareTo(yAG);
			}

			if (ByItem)
			{
				int xItemID = x.InventoryID ?? 0;
				int yItemID = y.InventoryID ?? 0;

				if (xItemID != yItemID)
					return xItemID.CompareTo(yItemID);
			}

			if (ByEmployee)
			{
				int xEmployeeID = x.ResourceID ?? 0;
				int yEmployeeID = y.ResourceID ?? 0;

				if (xEmployeeID != yEmployeeID)
					return xEmployeeID.CompareTo(yEmployeeID);
			}

			if (ByVendor)
			{
				int xVendorID = x.BAccountID ?? 0;
				int yVendorID = y.BAccountID ?? 0;

				if (xVendorID != yVendorID)
					return xVendorID.CompareTo(yVendorID);
			}

			if (ByDate)
			{
				return x.Date.Value.CompareTo(y.Date.Value);
			}

			bool xBillable = x.Billable == true;
			bool yBillable = y.Billable == true;

			return xBillable.CompareTo(yBillable);
		}
	}
}
