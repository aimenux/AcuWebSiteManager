using System;

using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// Represents a scope used to shut down <see cref="PXFormulaAttribute"/>
	/// calculation (e.g. for performance reasons). For consistency, the 
	/// field values should be assigned manually within the scope.
	/// </summary>
	public class DisableFormulaCalculationScope : OverrideAttributePropertyScope<PXFormulaAttribute, bool>
	{
		public DisableFormulaCalculationScope(PXCache cache, params Type[] fields)
			: base(
				cache, 
				fields,
				(formula, value) => formula.CancelCalculation = value,
				(formula) => formula.CancelCalculation,
				true)
		{ }
	}
}
