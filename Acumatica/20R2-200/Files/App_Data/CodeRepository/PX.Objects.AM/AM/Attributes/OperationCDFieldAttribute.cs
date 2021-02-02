using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Operation CD field attribute
    /// </summary>
    [PXDBString(OperationFieldLength, IsUnicode = true, InputMask = "#####")]
    [PXUIField(DisplayName = "Operation ID")]
    public class OperationCDFieldAttribute : AcctSubAttribute, IPXFieldUpdatingSubscriber
    {
        public const int OperationFieldLength = 10;

        public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(e.NewValue)))
            {
                return;
            }
            // Prevent silly users from entering leading spaces...
            e.NewValue = Convert.ToString(e.NewValue).TrimStart();
        }
    }
}
