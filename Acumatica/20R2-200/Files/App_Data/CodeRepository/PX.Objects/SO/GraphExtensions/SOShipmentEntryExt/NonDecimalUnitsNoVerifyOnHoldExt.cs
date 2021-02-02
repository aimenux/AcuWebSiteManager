using PX.Data;
using PX.Objects.Extensions;
using System.Collections.Generic;

namespace PX.Objects.SO.GraphExtensions.SOShipmentEntryExt
{
	public class NonDecimalUnitsNoVerifyOnHoldExt : NonDecimalUnitsNoVerifyOnHoldExt<SOShipmentEntry, SOShipment, SOShipLine, SOShipLine.shippedQty, SOShipLineSplit, SOShipLineSplit.qty>
	{
		public override bool HaveHoldStatus(SOShipment doc) => doc.Hold == true;
		public override int? GetLineNbr(SOShipLine line) => line.LineNbr;
		public override int? GetLineNbr(SOShipLineSplit split) => split.LineNbr;

		public override IEnumerable<SOShipLine> GetLines() => Base.Transactions.Select().RowCast<SOShipLine>();

		public override IEnumerable<SOShipLineSplit> GetSplits()
			=> PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>.Select(Base).RowCast<SOShipLineSplit>();

		protected override SOShipLine LocateLine(SOShipLineSplit split) =>
			(SOShipLine)Base.Transactions.Cache.Locate(new SOShipLine
			{
				ShipmentNbr = split.ShipmentNbr,
				ShipmentType = Base.CurrentDocument.Current.ShipmentType,
				LineNbr = split.LineNbr
			});
	}
}
