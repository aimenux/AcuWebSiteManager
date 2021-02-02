namespace PX.Objects.Common.Extensions
{
	public static class SearchesExtensions
	{
		public static object GetSearchValueByPosition(this object[] searches, int position)
		{
			return searches != null && searches.Length > position
				? searches[position]
				: null;
		}
	}
}
