using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;

namespace PX.Objects.SM
{
	public class TodayBusinessDate : ITodayUtc
	{
		public DateTime TodayUtc
		{
			get
			{
				DateTime? today = PXContext.GetBusinessDate() ?? PXTimeZoneInfo.Now;

				return today.Value.Date;
			}
		}
	}
}
