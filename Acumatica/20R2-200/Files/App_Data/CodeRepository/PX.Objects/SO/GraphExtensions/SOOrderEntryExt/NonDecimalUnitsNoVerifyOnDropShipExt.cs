using PX.Objects.Extensions;
using PX.Objects.IN;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	/// <summary>
	/// Disabling of validation for decimal values for drop ship lines in SO Order
	/// </summary>
	public class NonDecimalUnitsNoVerifyOnDropShipExt: NonDecimalUnitsNoVerifyOnDropShipExt<SOOrderEntry, SOLine>
	{
		protected override bool IsDropShipLine(SOLine line) => line.POCreate == true && line.POSource == INReplenishmentSource.DropShipToOrder;
	}
}
