using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class AcaFTStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { FullTime, PartTime },
				new string[]
				{
					Messages.FullTime,
					Messages.PartTime,
				})
			{
			}
		}

		public class fullTime : PX.Data.BQL.BqlString.Constant<fullTime>
		{
			public fullTime() : base(FullTime) { }
		}

		public class partTime : PX.Data.BQL.BqlString.Constant<partTime>
		{
			public partTime() : base(PartTime) { }
		}

		public const string FullTime = "FTM";
		public const string PartTime = "PTM";

		public const int FullTimeMonthlyHourThreshold = 130;
		public const int FullTimeWeeklyHourThreshold = 30;
		public const int PartTimeMaxHoursImputed = 120;
		public const int FteHours = 120;
	}
}
