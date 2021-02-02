using PX.Objects.FA.DepreciationMethods.Parameters;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods
{
	/// <exclude/>
	public abstract class DepreciationMethodBase
	{
		public class FADepreciationScheduleItem
		{
			public string FinPeriodID { get; set; }
			public decimal DepreciationAmount { get; set; }
			public bool IsSuspended { get; set; }
		}

		protected abstract string CalculationMethod { get; }

		protected abstract string[] ApplicableAveragingConventions { get; }

		public bool IsStraightLine => CalculationMethod == FADepreciationMethod.depreciationMethod.StraightLine;
		public ICollection<FADepreciationScheduleItem> CalculateDepreciation(string maxPeriodID = null)
		{
			return CalculateDepreciation(new CalculationParameters(IncomingParameters, maxPeriodID));
		}

		protected abstract ICollection<FADepreciationScheduleItem> Calculate();
		public ICollection<FADepreciationScheduleItem> CalculateDepreciation(CalculationParameters parameters)
		{
			CalculationParameters = parameters;
			return Calculate();
		}

		public IncomingCalculationParameters IncomingParameters { get; private set; }
		public CalculationParameters CalculationParameters { get; set; }

		public bool IsSuitable(IncomingCalculationParameters incomingParameters)
		{
			IncomingParameters = incomingParameters;
			return CalculationMethod == incomingParameters.CalculationMethod
				&& ApplicableAveragingConventions.Contains(incomingParameters.AveragingConvention);
		}
	}
}
