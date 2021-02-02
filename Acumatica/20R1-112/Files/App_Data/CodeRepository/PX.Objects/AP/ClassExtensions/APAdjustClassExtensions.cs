
namespace PX.Objects.AP
{
	public static class APAdjustClassExtensions
	{
		/// <summary>
		/// Returnes <c>true</c> if the same document is entered as an adjusting and adjusted document.
		/// </summary>
		public static bool IsSelfAdjustment(this APAdjust adj)
		{
			return adj.AdjdDocType == adj.AdjgDocType &&
				adj.AdjdRefNbr == adj.AdjgRefNbr;
		}
	}
}
