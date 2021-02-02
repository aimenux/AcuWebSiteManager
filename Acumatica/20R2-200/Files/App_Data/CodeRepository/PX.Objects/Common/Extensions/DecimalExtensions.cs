using System;

namespace PX.Objects.Common.Extensions
{
	public static class DecimalExtensions
	{
		public static bool IsNonZero(this decimal? number) 
			=> (number ?? 0m) != 0m;

		public static bool IsNullOrZero(this decimal? number)
			=> number == null || number == 0m;
	}
}
