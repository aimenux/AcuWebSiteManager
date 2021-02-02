using PX.Data;

namespace PX.Objects.PR
{
	public class CheckVoidType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PrinterIssue, Lost, Damaged, Stolen, ErrorCorrection },
				new string[] { Messages.PrinterIssueCVT, Messages.LostCVT, Messages.DamagedCVT, Messages.StolenCVT, Messages.ErrorCorrection })
			{ }
		}

		public const string PrinterIssue = "PRT";
		public const string Lost = "LST";
		public const string Damaged = "DMG";
		public const string Stolen = "STN";
		public const string ErrorCorrection = "COR";
	}
}
