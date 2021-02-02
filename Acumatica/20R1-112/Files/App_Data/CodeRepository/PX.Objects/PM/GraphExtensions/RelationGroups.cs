using PX.Data;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class RelationGroupsExt : PXGraphExtension<RelationGroups>
	{
		[PXOverride]
		public bool CanBeRestricted(Type entityType, object instance)
		{
			if (entityType == typeof(InventoryItem))
			{
				InventoryItem item = instance as InventoryItem;
				if (item != null)
				{
					return item.ItemStatus != InventoryItemStatus.Unknown;
				}
			}

			if (entityType == typeof(Contract) || entityType == typeof(PMProject))
			{
				Contract item = instance as Contract;
				if (item != null)
				{
					return item.NonProject != true;
				}
			}

			return true;
		}
	}
}
