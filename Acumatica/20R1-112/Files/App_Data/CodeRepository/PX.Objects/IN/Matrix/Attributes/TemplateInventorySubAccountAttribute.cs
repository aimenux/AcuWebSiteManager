using PX.Data;
using PX.Objects.Common.Attributes;
using PX.Objects.GL;
using System;

namespace PX.Objects.IN.Matrix.Attributes
{
	public class TemplateInventorySubAccountAttribute : SubAccountAttribute
	{
		protected bool _expectedStockItemValue;
		protected Type _databaseField;

		public TemplateInventorySubAccountAttribute(bool expectedStockItemValue, Type databaseField, Type accountType)
			: base(accountType)
		{
			_expectedStockItemValue = expectedStockItemValue;
			_databaseField = databaseField;

			if (_DBAttrIndex != -1)
				_Attributes[_DBAttrIndex] = new DBIntConditionAttribute(typeof(InventoryItem.stkItem), expectedStockItemValue, databaseField);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsCurrentStockItemValueEqualsExpectedValue(e.Row as InventoryItem))
			{
				if (string.Compare(FieldName, _databaseField.Name, true) != 0)
				{
					var valueOfCurrentField = sender.GetValue(e.Row, FieldName);
					sender.SetValue(e.Row, _databaseField.Name, valueOfCurrentField);
				}

				base.RowPersisting(sender, e);
			}
		}

		protected virtual bool IsCurrentStockItemValueEqualsExpectedValue(InventoryItem currentRow)
			=> currentRow?.StkItem == _expectedStockItemValue;
	}
}
