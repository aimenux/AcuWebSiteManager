using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.CS
{
	public class CSBoxMaint : PXGraph<CSBoxMaint>
	{
		public PXSetup<CommonSetup> Setup;
        public PXSelectJoin<CSBox, CrossJoin<CommonSetup>> Records;
        public PXSavePerRow<CSBox> Save;
        public PXCancel<CSBox> Cancel;


		public CSBoxMaint()
		{
			CommonSetup record = Setup.Current;
		}

		protected virtual void CSBox_BoxWeight_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CSBox row = (CSBox) e.Row;
			if (row == null) return;

			if ( (decimal?)e.NewValue >= row.MaxWeight )
				throw new PXSetPropertyException(Messages.WeightOfEmptyBoxMustBeLessThenMaxWeight);
		}

		protected virtual void CSBox_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CSBox box = (CSBox)e.Row;
			var openShipments = PXSelectJoinGroupBy<SOShipment,
					InnerJoin<SOPackageDetail, On<SOPackageDetail.shipmentNbr, Equal<SOShipment.shipmentNbr>>>,
					Where<SOPackageDetail.boxID, Equal<Required<CSBox.boxID>>,
						And<SOShipment.released, NotEqual<True>>>,
					Aggregate<GroupBy<SOShipment.shipmentNbr>>>
				.SelectWindowed(this, 0, 10, box.BoxID).RowCast<SOShipment>().ToList();

			if (openShipments.Any())
			{
				throw new PXException(Messages.BoxUsedInShipments, string.Join(", ", openShipments.Select(_ => _.ShipmentNbr).Distinct()));
			}

			var openOrders = PXSelectJoinGroupBy<SOOrder,
					InnerJoin<SOPackageInfo, On<SOPackageInfo.FK.Order>>,
					Where<SOPackageInfo.boxID, Equal<Required<CSBox.boxID>>,
						And<SOOrder.completed, NotEqual<True>, And<SOOrder.cancelled, NotEqual<True>>>>,
					Aggregate<GroupBy<SOOrder.orderType, GroupBy<SOOrder.orderNbr>>>>
				.SelectWindowed(this, 0, 10, box.BoxID).RowCast<SOOrder>().ToList();

			if (openOrders.Any())
			{
				var ordersString = new StringBuilder();
				openOrders.ForEach(_ => ordersString.AppendFormat("{0} {1}, ", _.OrderType, _.OrderNbr));
				ordersString.Remove(ordersString.Length - 2, 2);

				throw new PXException(Messages.BoxUsedInOrders, ordersString);
			}
		}
}
}
