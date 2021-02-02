using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.PO.LandedCosts.Attributes
{
	public class LandedCostDocSetStatusAttribute : PXEventSubscriberAttribute, IPXRowUpdatingSubscriber, IPXRowInsertingSubscriber
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.AddHandler(
				sender.GetItemType(),
				nameof(POLandedCostDoc.hold),
				(cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					var item = e.Row as POLandedCostDoc;
					if (item != null)
					{
						StatusSet(cache, item, (bool?)e.NewValue);
					}
				});

			sender.Graph.FieldVerifying.AddHandler(
				sender.GetItemType(),
				nameof(POLandedCostDoc.status),
				(cache, e) => { e.NewValue = cache.GetValue<POLandedCostDoc.status>(e.Row); });

			sender.Graph.RowSelected.AddHandler(
				sender.GetItemType(),
				(cache, e) =>
				{
					var document = e.Row as POLandedCostDoc;

					if (document != null)
					{
						StatusSet(cache, document, document.Hold);
					}
				});
		}

		protected virtual void StatusSet(PXCache cache, POLandedCostDoc item, bool? HoldVal)
		{
			if (item.Released == true)
			{
				item.Status = POLandedCostDocStatus.Released;
			}
			 else if (item.Hold == true)
			{
				item.Status = POLandedCostDocStatus.Hold;
			}			
			else
			{
				item.Status = POLandedCostDocStatus.Balanced;
			}
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var item = (POLandedCostDoc)e.Row;
			StatusSet(sender, item, item.Hold);
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var item = (POLandedCostDoc)e.NewRow;
			StatusSet(sender, item, item.Hold);
		}
	}
}
