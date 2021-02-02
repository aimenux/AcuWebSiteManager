using PX.Data;
using PX.Objects.Extensions;
using PX.Objects.IN;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	/// <summary>
	/// Disabling of validation for decimal values for drop ship lines in PO Receipt
	/// </summary>
	public class NonDecimalUnitsNoVerifyOnDropShipExt: NonDecimalUnitsNoVerifyOnDropShipExt<POReceiptEntry, POReceiptLine>
	{
		protected override bool IsDropShipLine(POReceiptLine line) => POLineType.IsDropShip(line.LineType);

		protected override DecimalVerifyMode GetLineVerifyMode(PXCache cache, POReceiptLine line)
		{
			if (!IsDropShipLine(line))
			{
				var cacheAttribute = cache.GetAttributesOfType<PXDBQuantityAttribute>(null, nameof(POReceiptLine.receiptQty)).FirstOrDefault();
				if (cacheAttribute != null)
					return cacheAttribute.DecimalVerifyMode;
			}
			return DecimalVerifyMode.Off;
		}
	}
}
