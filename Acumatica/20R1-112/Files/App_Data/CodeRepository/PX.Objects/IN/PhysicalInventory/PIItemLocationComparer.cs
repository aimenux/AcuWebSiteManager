using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.IN.PhysicalInventory
{
	public class PIItemLocationComparer : IItemLocationComparer
	{
		private readonly INPIClass piClass;

		public PIItemLocationComparer(INPIClass piClass)
		{
			this.piClass = piClass;
		}

		public int Compare(PIItemLocationInfo x, PIItemLocationInfo y)
		{
			var xKeys = GetSortingKey(x);
			var yKeys = GetSortingKey(y);

			int result = 0;
			for (int i = 0; i < xKeys.Length; i++)
			{
				result = string.Compare(xKeys[i], yKeys[i], StringComparison.InvariantCultureIgnoreCase);
				if (result != 0)
					break;
			}
			return result;
		}

		public string[] GetSortColumns()
		{
			List<string> sortColumns = new List<string>();
			AddSortColumnToListIfSet(sortColumns, piClass.NAO1);
			AddSortColumnToListIfSet(sortColumns, piClass.NAO2);
			AddSortColumnToListIfSet(sortColumns, piClass.NAO3);
			AddSortColumnToListIfSet(sortColumns, piClass.NAO4);
			return sortColumns.ToArray();
		}

		private void AddSortColumnToListIfSet(List<string> sortColumns, string naoCode)
		{
			string fieldName = NAOCodeToFieldName(naoCode);
			if (fieldName != null)
			{
				sortColumns.Add(fieldName);
			}
		}

		private string NAOCodeToFieldName(string naoCode)
		{
			switch (naoCode)
			{
				case PINumberAssignmentOrder.EmptySort:
					return null;
				case PINumberAssignmentOrder.ByLocationID:
					return "INLocation__locationCD";
				case PINumberAssignmentOrder.ByInventoryID:
					return "InventoryItem__inventoryCD";
				case PINumberAssignmentOrder.BySubItem:
					return "INSubItem__subItemCD";
				case PINumberAssignmentOrder.ByLotSerial:
					return "INLotSerialStatus__lotSerialNbr";
				case PINumberAssignmentOrder.ByInventoryDescription:
					return "InventoryItem__descr";
				default:
					throw new PXException(Messages.UnknownPiTagSortOrder);
			}
		}

		private string[] GetSortingKey(PIItemLocationInfo il)
		{
			var keys = new List<string>();
			AddFieldIfNAOCodeSet(keys, piClass.NAO1, il);
			AddFieldIfNAOCodeSet(keys, piClass.NAO2, il);
			AddFieldIfNAOCodeSet(keys, piClass.NAO3, il);
			AddFieldIfNAOCodeSet(keys, piClass.NAO4, il);
			return keys.ToArray();
		}

		private void AddFieldIfNAOCodeSet(List<string> sortColumns, string naoCode, PIItemLocationInfo il)
		{
			string fieldValue = NAOCodeToFieldValue(naoCode, il);
			if (fieldValue != null)
			{
				sortColumns.Add(fieldValue);
			}
		}

		private string NAOCodeToFieldValue(string naoCode, PIItemLocationInfo il)
		{
			switch (naoCode)
			{
				case PINumberAssignmentOrder.EmptySort:
					return null;
				case PINumberAssignmentOrder.ByLocationID:
					return il.LocationCD ?? string.Empty;
				case PINumberAssignmentOrder.ByInventoryID:
					return il.InventoryCD ?? string.Empty;
				case PINumberAssignmentOrder.BySubItem:
					return il.SubItemCD ?? string.Empty;
				case PINumberAssignmentOrder.ByLotSerial:
					return il.LotSerialNbr ?? string.Empty;
				case PINumberAssignmentOrder.ByInventoryDescription:
					return il.Description ?? string.Empty;
				default:
					throw new PXException(Messages.UnknownPiTagSortOrder);
			}
		}
	}
}
