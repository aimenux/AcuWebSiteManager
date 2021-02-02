
namespace PX.Objects.AR
{
	public static class ARAdjustClassExtensions
	{
		/// <summary>
		/// Returnes <c>true</c> if the same document is entered as an adjusting and adjusted document.
		/// </summary>
		public static bool IsSelfAdjustment(this ARAdjust adj)
		{
			return adj.AdjdDocType == adj.AdjgDocType &&
				adj.AdjdRefNbr == adj.AdjgRefNbr;
		}

		/// <summary>
		/// Returnes <c>true</c> if this is an original application for the CreditWO document.
		/// </summary>
		public static bool IsOrigSmallCreditWOApp(this ARAdjust adj)
		{
			return 
				adj.AdjdDocType == ARDocType.SmallCreditWO &&
				adj.AdjNbr == -1 &&
				adj.VoidAdjNbr == null;
		}
	}
}
