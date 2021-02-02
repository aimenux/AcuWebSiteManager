using System;

namespace PX.Objects.PR
{
	public static class PRDateTime
	{
		/// <summary>
		/// See https://stackoverflow.com/a/32034722/2528023
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static int GetQuarter(this DateTime date)
		{
			return (date.Month + 2) / 3;
		}

		public static int[] GetQuarterMonths(this DateTime date)
		{
			var quarter = GetQuarter(date);
			return new int[] { quarter * 3 - 2, quarter * 3 - 1, quarter * 3 };
		}
	}
}
