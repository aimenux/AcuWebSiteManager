using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class WCDeductionColumnVisibilityEvaluator : BqlFormulaEvaluator, IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			return new SelectFrom<PRDeductCode>
				.Where<PRDeductCode.isWorkersCompensation.IsEqual<True>
					.And<PRDeductCode.contribType.IsNotEqual<ContributionType.employerContribution>>>.View(cache.Graph).SelectSingle() != null;
		}
	}
}
