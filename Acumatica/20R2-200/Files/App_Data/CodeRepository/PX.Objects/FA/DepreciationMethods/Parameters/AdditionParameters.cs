using PX.Objects.Common;
using System;
using System.Collections.Generic;

namespace PX.Objects.FA.DepreciationMethods.Parameters
{
	/// <exclude/>
	public class AdditionParameters
	{
		public string DepreciateFromPeriodID;
		public string DepreciateToPeriodID;
	}

	/// <exclude/>
	public class SLMethodAdditionParameters : AdditionParameters
	{
		public DateTime PlacedInServiceDate;
		public DateTime DepreciateFromDate;

		public decimal DepreciationBasis;

		public HashSet<string> SuspendedPeriodIDs;
	}

	/// <exclude/>
	public class FAAddition
	{
		public FAAddition(decimal amount, string finPeriodID, DateTime date, int precision)
		{
			Amount = amount;
			PeriodID = finPeriodID;
			Date = date;
			Precision = precision;
		}

		public string PeriodID { get; protected set; }
		public DateTime Date { get; protected set; }
		public decimal Amount { get; set; }
		public bool IsOriginal { get; protected set; }

		public decimal SalvageAmount { get; protected set; } = 0m;
		public decimal Section179Amount { get; protected set; } = 0m;
		public decimal BonusAmount { get; protected set; } = 0m;
		public decimal BusinessUse { get; protected set; } = 1m;

		protected int Precision;

		public decimal DepreciationBasis => PXRounder.Round(Amount * BusinessUse, Precision) - Section179Amount - BonusAmount - SalvageAmount;

		public AdditionParameters CalculatedAdditionParameters;

		public void MarkOriginal(FABookBalance bookBalance)
		{
			IsOriginal = true;
			BusinessUse = bookBalance.BusinessUse * 0.01m ?? 1m;
			SalvageAmount = bookBalance.SalvageAmount ?? 0m;
			Section179Amount = bookBalance.Tax179Amount ?? 0m;
			BonusAmount = bookBalance.BonusAmount ?? 0m;
		}
	}
}
