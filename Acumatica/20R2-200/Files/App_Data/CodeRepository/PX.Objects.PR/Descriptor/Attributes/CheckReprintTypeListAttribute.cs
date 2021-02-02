using PX.Data;

namespace PX.Objects.PR
{
	public class CheckReprintType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PrinterIssue, Lost, Damaged, Stolen, Corrected },
				new string[] { Messages.PrinterIssue, Messages.Lost, Messages.Damaged, Messages.Stolen, Messages.Corrected })
			{ }
		}

		public const string PrinterIssue = "PRT";
		public const string Lost = "LST";
		public const string Damaged = "DMG";
		public const string Stolen = "STN";
		public const string Corrected = "COR";
	}
}
