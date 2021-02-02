using PX.Objects.AR;
using PX.Objects.Extensions;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	/// <summary>
	/// Disabling of validation for decimal values for drop ship lines in SO Invoice
	/// </summary>
	public class NonDecimalUnitsNoVerifyOnDropShipExt: NonDecimalUnitsNoVerifyOnDropShipExt<SOInvoiceEntry, ARTran>
	{
		protected override bool IsDropShipLine(ARTran line) => line.SOShipmentType == SOShipmentType.DropShip;
	}
}
