using PX.Objects.Extensions;

namespace PX.Objects.IN.GraphExtensions.INIssueEntryExt
{
	/// <summary>
	/// Disabling of validation for decimal values for drop ship lines in IN Issue
	/// </summary>
	public class NonDecimalUnitsNoVerifyOnDropShipExt : NonDecimalUnitsNoVerifyOnDropShipExt<INIssueEntry, INTran>
	{
		protected override bool IsDropShipLine(INTran line) => line.SOShipmentType == SO.SOShipmentType.DropShip;
	}
}
