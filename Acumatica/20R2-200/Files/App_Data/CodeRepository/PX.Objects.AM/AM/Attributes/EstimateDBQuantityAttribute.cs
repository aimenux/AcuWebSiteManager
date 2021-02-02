using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Handle non inventory with UOM conversion issues for the estimate module
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
    public class EstimateDBQuantityAttribute : PXDBQuantityAttribute
    {
        protected Type _InventoryIdField = null;

        public EstimateDBQuantityAttribute(Type keyField, Type resultField, Type inventoryIdField) : base(keyField, resultField)
        {
            _InventoryIdField = inventoryIdField;
        }

        protected override void CalcBaseQty(PXCache sender, QtyConversionArgs e)
        {
            var inventoryId = sender.GetValue(e.Row, _InventoryIdField.Name);

            if (inventoryId == null && e.NewValue != null)
            {
                if (e.ExternalCall)
                {
                    sender.SetValueExt(e.Row, this._ResultField.Name, (decimal)e.NewValue);
                }
                else
                {
                    sender.SetValue(e.Row, this._ResultField.Name, (decimal)e.NewValue);
                }

                return;
            }

            base.CalcBaseQty(sender, e);
        }
    }
}