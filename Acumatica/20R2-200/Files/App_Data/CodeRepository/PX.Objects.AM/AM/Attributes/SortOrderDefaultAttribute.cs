using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// DAC Field level attribute for defaulting the sort order field by a multiplier from the Line Nbr field value
    /// </summary>
    public class SortOrderDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
    {
        protected readonly Type LineNbrField;
        protected readonly int StepMultiplier;
        public const int DEFAULTSTEPMULT = 10;

        public SortOrderDefaultAttribute(Type lineNbrField) 
            : this(lineNbrField, DEFAULTSTEPMULT)
        {
        }

        public SortOrderDefaultAttribute(Type lineNbrField, int stepMultiplier)
        {
            LineNbrField = lineNbrField;
            StepMultiplier = stepMultiplier;

            if (stepMultiplier <= 0)
            {
                StepMultiplier = 1;
            }
        }

        protected virtual int GetLineNbrValue(PXCache cache, object row)
        {
            return row == null ? 0 : (int?)cache.GetValue(row, LineNbrField.Name) ?? 0;
        }

        public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = GetLineNbrValue(sender, e.Row) * StepMultiplier;
        }
    }
}