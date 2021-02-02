using PX.Data;
using PX.SM;
using System;

namespace PX.Objects.IN.GraphExtensions.RelationGroupsExt
{
	public class HideTemplates : PXGraphExtension<RelationGroups>
	{
		[PXOverride]
		public virtual bool CanBeRestricted(Type entityType, object instance, Func<Type, object, bool> baseMethod)
		{
			if (baseMethod(entityType, instance) == false) return false;

			if (entityType == typeof(InventoryItem))
			{
				InventoryItem item = instance as InventoryItem;
				if (item != null)
				{
					return item.IsTemplate == false;
				}
			}

			return true;
		}
	}
}
