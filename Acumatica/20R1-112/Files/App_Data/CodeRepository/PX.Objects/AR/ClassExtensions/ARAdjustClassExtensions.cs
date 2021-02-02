
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
	}
}
