using PX.Data;

namespace PX.Objects.PM
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class ChangeRequestStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { OnHold, PendingApproval, Open, Closed, Rejected },
				new string[] { PX.Objects.PM.Messages.OnHold, PX.Objects.PM.Messages.PendingApproval, PX.Objects.PM.Messages.Open, PX.Objects.PM.Messages.Closed, PX.Objects.PM.Messages.Rejected })
			{; }
		}
		public const string OnHold = "H";
		public const string PendingApproval = "A";
		public const string Open = "O";
		public const string Closed = "C";
		public const string Rejected = "R";

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) {; }
		}
	}
}
