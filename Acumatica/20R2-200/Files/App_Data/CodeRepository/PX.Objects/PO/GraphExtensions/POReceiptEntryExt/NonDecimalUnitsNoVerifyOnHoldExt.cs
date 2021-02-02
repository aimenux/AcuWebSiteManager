using PX.Data;
using PX.Objects.Extensions;
using System.Collections.Generic;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class NonDecimalUnitsNoVerifyOnHoldExt : NonDecimalUnitsNoVerifyOnHoldExt<POReceiptEntry, POReceipt, POReceiptLine, POReceiptLine.receiptQty, POReceiptLineSplit, POReceiptLineSplit.qty>
	{
		private NonDecimalUnitsNoVerifyOnDropShipExt _nonDecimalUnitsNoVerifyOnDropShipExt;

		protected NonDecimalUnitsNoVerifyOnDropShipExt NonDecimalUnitsNoVerifyOnDropShipExt
			=> _nonDecimalUnitsNoVerifyOnDropShipExt = _nonDecimalUnitsNoVerifyOnDropShipExt ?? Base.FindImplementation<NonDecimalUnitsNoVerifyOnDropShipExt>();

		public override bool HaveHoldStatus(POReceipt doc) => doc.Hold == true;
		public override int? GetLineNbr(POReceiptLine line) => line.LineNbr;
		public override int? GetLineNbr(POReceiptLineSplit split) => split.LineNbr;
		public override IEnumerable<POReceiptLine> GetLines() => Base.transactions.Select().RowCast<POReceiptLine>();
		protected override POReceiptLine LocateLine(POReceiptLineSplit split) =>
			(POReceiptLine)Base.transactions.Cache.Locate(new POReceiptLine
			{
				ReceiptNbr = split.ReceiptNbr,
				LineNbr = split.LineNbr
			});

		public override IEnumerable<POReceiptLineSplit> GetSplits()
			=> PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>.Select(Base).RowCast<POReceiptLineSplit>();

		protected override void VerifyLine(PXCache lineCache, POReceiptLine line)
		{
			NonDecimalUnitsNoVerifyOnDropShipExt.SetDecimalVerifyMode(lineCache, line);
			base.VerifyLine(lineCache, line);
		}
	}
}
