namespace PX.Objects.Common.Extensions
{
	public static class StringExtensions
	{
		public static string Capitalize(this string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			else
			{
				return char.ToUpper(text[0]) + text.Substring(1);
			}
		}

		/// <summary>
		/// Removes all leading occurrences of whitespace from the current string
		/// object. Does not throw an exception if string is null.
		/// </summary>
		/// <param name="str">The string object to remove leading whitespace from. Can safely be null.</param>
		/// <returns>The string that remains after all occurrences of leading whitespace are removed 
		/// from <paramref name="str"/>, or null if <paramref name="str"/> was null itself.</returns>
		public static string SafeTrimStart(this string str)
        {
            if (str == null)
                return str;

            return str.TrimStart();
        }
    }
}