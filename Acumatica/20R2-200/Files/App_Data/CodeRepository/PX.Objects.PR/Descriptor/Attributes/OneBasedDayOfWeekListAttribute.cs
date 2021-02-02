using PX.Data;
using PX.Data.BQL;
using System;

namespace PX.Objects.PR
{
	public class OneBasedDayOfWeek
	{
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(GetOneBasedDaysOfWeek())
			{
			}

			private static Tuple<int, string>[] GetOneBasedDaysOfWeek()
			{
				const byte weekLength = 7;
				Tuple<int, string>[] result = new Tuple<int, string>[weekLength];
				for (byte i = 0; i < weekLength; i++)
				{
					DayOfWeek dayOfWeek = (DayOfWeek)i;
					int oneBasedDayOfWeek = GetOneBasedDayOfWeek(dayOfWeek);
					result[i] = new Tuple<int, string>(oneBasedDayOfWeek, dayOfWeek.ToString());
				}

				return result;
			}
		}

		public class saturday : BqlInt.Constant<saturday>
		{
			public saturday() : base(GetOneBasedDayOfWeek(DayOfWeek.Saturday)) { }
		}

		public static int GetOneBasedDayOfWeek(DayOfWeek zeroBasedDayOfWeek)
		{
			return (int)zeroBasedDayOfWeek + 1;
		}

		public static DayOfWeek GetZeroBasedDayOfWeek(int oneBasedDayOfWeek)
		{
			if (oneBasedDayOfWeek < 1 || oneBasedDayOfWeek > 7)
				throw new PXArgumentException(Messages.OneBasedDayOfWeekIncorrectValue, nameof(oneBasedDayOfWeek));

			return (DayOfWeek)(oneBasedDayOfWeek - 1);
		}
	}
}
