using PX.Data;
using PX.Objects.Common;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Represents a scope used to shut down <see cref="PXFormulaAttribute"/>
	/// calculation of parent aggregate (e.g. for performance reasons, parent creation of children).
	/// </summary>
	public class DisableFormulaAggregateScope : OverrideAttributePropertyScope<PXFormulaAttribute, Type>
	{
		public DisableFormulaAggregateScope(PXCache cache, params Type[] fields)
			: base(
				cache,
				fields,
				(formula, value) => formula.Aggregate = value,
				(formula) => formula.Aggregate,
				null)
		{ }
	}
}