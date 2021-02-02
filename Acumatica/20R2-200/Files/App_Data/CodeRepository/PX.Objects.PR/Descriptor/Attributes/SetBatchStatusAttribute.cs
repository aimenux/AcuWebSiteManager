using PX.Data;

namespace PX.Objects.PR
{
	public class SetBatchStatusAttribute : PXEventSubscriberAttribute, IPXRowUpdatingSubscriber, IPXRowInsertingSubscriber
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), nameof(PRBatch.hold),
				(cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					var payBatch = e.Row as PRBatch;
					if (payBatch != null)
						StatusSet(cache, payBatch);
				});

			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), nameof(PRBatch.status),
				(cache, e) => 
				{
					e.NewValue = cache.GetValue<PRBatch.status>(e.Row);
				});

			sender.Graph.RowSelected.AddHandler(sender.GetItemType(),
				(cache, e) =>
				{
					var payBatch = e.Row as PRBatch;

					if (payBatch != null)
						StatusSet(cache, payBatch);
				});
		}

		protected virtual void StatusSet(PXCache cache, PRBatch payBatch)
		{
			if (payBatch.Closed == true)
			{
				payBatch.Status = BatchStatus.Closed;
				return;
			}
			if (payBatch.Open == true)
			{
				payBatch.Status = BatchStatus.Open;
				return;
			}
			if (payBatch.Hold == true)
			{
				payBatch.Status = BatchStatus.Hold;
				return;
			}
			payBatch.Status = BatchStatus.Balanced;
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			StatusSet(sender, (PRBatch)e.Row);
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			StatusSet(sender, (PRBatch)e.NewRow);
		}
	}
}
