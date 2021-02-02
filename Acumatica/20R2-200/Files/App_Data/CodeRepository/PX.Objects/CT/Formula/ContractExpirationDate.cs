using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CT
{
	public class ContractExpirationDate<ContractType, DurationType, StartDate, Duration> : BqlFormulaEvaluator<StartDate, ContractType, DurationType, StartDate, Duration>, IBqlOperand
		where ContractType : IBqlField
		where DurationType : IBqlField
		where StartDate : IBqlField
		where Duration : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			DateTime? startDate = (DateTime?)parameters[typeof(StartDate)];
			string contractType = (string)parameters[typeof(ContractType)];
			string durationType = (string)parameters[typeof(DurationType)];
			int? duration = (int?)parameters[typeof(Duration)];

			if (contractType != Contract.type.Unlimited &&
				startDate != null && !string.IsNullOrEmpty(durationType) && duration != null)
			{
				DateTime origin = (DateTime)startDate;
				switch (durationType)
				{
					case Contract.durationType.Annual:
						return origin.AddYears((int) duration).AddDays(-1);
					case Contract.durationType.Monthly:
						return origin.AddMonths((int) duration).AddDays(-1);
					case Contract.durationType.Quarterly:
						return origin.AddMonths(3 * (int) duration).AddDays(-1);
					case Contract.durationType.Custom:
						return origin.AddDays((double) duration).AddDays(-1);
				}
			}
			return null;
		}
	}
}
