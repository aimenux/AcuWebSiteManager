using System.Collections.Generic;

namespace PX.Objects.IN.PhysicalInventory
{
	public interface IItemLocationComparer : IComparer<PIItemLocationInfo>
	{
		string[] GetSortColumns();
	}
}
