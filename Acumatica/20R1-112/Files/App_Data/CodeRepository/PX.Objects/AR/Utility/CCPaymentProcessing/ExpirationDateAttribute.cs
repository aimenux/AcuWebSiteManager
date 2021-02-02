using System;
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing
{
    public class ExpirationDateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber
    {
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(),
                _FieldName, FieldSelectingHandler);
        }

        public void FieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.ReturnValue != null)
            {
                e.ReturnValue = ((DateTime)e.ReturnValue).AddMonths(-1);
            }
        }

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null)
			{
				e.NewValue = ((DateTime)e.NewValue).AddMonths(1);
			}
		}
	}
}