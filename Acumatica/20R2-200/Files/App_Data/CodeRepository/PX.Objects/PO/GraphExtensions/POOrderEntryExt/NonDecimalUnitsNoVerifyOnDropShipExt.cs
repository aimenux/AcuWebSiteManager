using PX.Objects.Extensions;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	/// <summary>
	/// Disabling of validation for decimal values for drop ship lines in PO Order
	/// </summary>
	public class NonDecimalUnitsNoVerifyOnDropShipExt: NonDecimalUnitsNoVerifyOnDropShipExt<POOrderEntry, POLine>
	{
		protected override bool IsDropShipLine(POLine line) => POLineType.IsDropShip(line.LineType);
	}
}
