using System;
using PX.Common;
using PX.Data;

namespace PX.Objects.GL
{
    /// <summary>
    /// Returns null field value for zero value.
    /// </summary>
    public class ReclassRemainingAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
    {
        public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if ((decimal?)e.ReturnValue == decimal.Zero)
            {
                e.ReturnValue = null;
            }
        }
    }
}
