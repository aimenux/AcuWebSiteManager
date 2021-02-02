using PX.Data;

namespace PX.Objects.PR
{
	public class DeductionSplitType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Even, ProRata },
				new string[] { Messages.Even, Messages.ProRata })
			{ }
		}

		public const string Even = "EVN";
		public const string ProRata = "PRO";
	}
}
