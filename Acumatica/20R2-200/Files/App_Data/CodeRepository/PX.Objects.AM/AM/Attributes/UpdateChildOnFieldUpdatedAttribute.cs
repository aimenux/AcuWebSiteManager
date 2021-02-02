using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Auto update child row field on change of parent field attached to this attribute.
    /// </summary>
    public class UpdateChildOnFieldUpdatedAttribute : UpdateChildOnFieldUpdatedBaseAttribute
    {
        /// <param name="childType">Type of child row</param>
        /// <param name="childUpdateField">Field in child row to update</param>
        public UpdateChildOnFieldUpdatedAttribute(Type childType, Type childUpdateField) : base(childType, childUpdateField)
        {
        }

        public override void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            UpdateAllChildRows(e.Row, sender.GetValue(e.Row, _FieldName));
        }
    }
}