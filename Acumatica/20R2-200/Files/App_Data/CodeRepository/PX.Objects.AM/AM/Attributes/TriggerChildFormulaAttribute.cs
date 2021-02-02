using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class TriggerChildFormulaAttribute : UpdateChildOnFieldUpdatedBaseAttribute
    {
        /// <param name="childType">Type of child row</param>
        /// <param name="childUpdateField">Field in child formula used to trigger formula</param>
        public TriggerChildFormulaAttribute(Type childType, Type childUpdateField) : base(childType, childUpdateField)
        {
        }

        protected virtual void RaiseChildFieldUpdated(object childRow, object newValue)
        {
            ChildCache?.RaiseFieldUpdated(_childUpdateField.Name, childRow, newValue);
        }

        public override void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            foreach (var child in SelectChildren( e.Row))
            {
                RaiseChildFieldUpdated(child, GetChildValue(child));
                UpdateChildRow(child);
            }
        }
    }
}