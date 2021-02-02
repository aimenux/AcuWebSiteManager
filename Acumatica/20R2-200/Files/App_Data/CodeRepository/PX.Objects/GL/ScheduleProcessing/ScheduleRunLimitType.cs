using System.Collections.Generic;

using PX.Data;

using PX.Objects.Common;
using System;

namespace PX.Objects.GL
{
	public class ScheduleRunLimitType : ILabelProvider
	{
		public const string StopOnExecutionDate = "D";
		public const string StopAfterNumberOfExecutions = "M";

		[Obsolete("This constant has been renamed to " + nameof(StopOnExecutionDate) + " and will be removed in Acumatica 8.0")]
		public const string RunTillDate = "D";
		[Obsolete("This constant has been renamed to " + nameof(StopAfterNumberOfExecutions) + " and will be removed in Acumatica 8.0")]
		public const string RunMultipleTimes = "M";
		

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ StopOnExecutionDate, Messages.StopOnExecutionDate },
			{ StopAfterNumberOfExecutions, Messages.StopAfterNumberOfExecutions },
		};

		public class stopOnExecutionDate : PX.Data.BQL.BqlString.Constant<stopOnExecutionDate>
		{
			public stopOnExecutionDate() : base(StopOnExecutionDate) { }
		}

		public class stopAfterNumberOfExecutions : PX.Data.BQL.BqlString.Constant<stopAfterNumberOfExecutions>
		{
			public stopAfterNumberOfExecutions() : base(StopAfterNumberOfExecutions) { }
		}

		[Obsolete("This constant has been renamed to " + nameof(stopOnExecutionDate) + "and will be removed in Acumatica 8.0")]
		public class runTillDate : stopOnExecutionDate { }

		[Obsolete("This constant has been renamed to " + nameof(stopAfterNumberOfExecutions) + " and will be removed in Acumatica 8.0")]
		public class runMultipleTimes : stopAfterNumberOfExecutions { }
	}
}
