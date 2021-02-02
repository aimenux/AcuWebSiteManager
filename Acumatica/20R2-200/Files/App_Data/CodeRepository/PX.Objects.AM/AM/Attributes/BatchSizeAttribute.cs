using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    [PXDBQuantity(MinValue = 0.0)]
    [PXUIField(DisplayName = "Batch Size", Visible = false)]
    public class BatchSizeAttribute : AcctSubAttribute, IPXRowSelectedSubscriber
    {
        public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var materialType = sender.GetValue(e.Row, "MaterialType");

            if (materialType == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled(sender, e.Row, FieldName, (int)materialType != AMMaterialType.Phantom);
        }
    }
}
