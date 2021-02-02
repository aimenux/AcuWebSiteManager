using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class AggRecord : AufRecord
	{
		public AggRecord(int? highestMonthlyFteCount) : base(AufRecordType.Agg)
		{
			HighestMonthlyFteCount = highestMonthlyFteCount ?? 0;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				MemberName,
				MemberEin,
				HighestMonthlyFteCount
			};

			return FormatLine(lineData);
		}

		public virtual string MemberName { get; set; }
		public virtual string MemberEin { get; set; }
		public virtual int HighestMonthlyFteCount { get; set; }
	}
}
