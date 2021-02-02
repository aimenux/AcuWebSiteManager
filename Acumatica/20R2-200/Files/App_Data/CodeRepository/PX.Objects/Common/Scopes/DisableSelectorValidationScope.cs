using System;

using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// Represents a scope used to shut down <see cref="PXSelectorAttribute"/>
	/// verification (e.g. for performance reasons).
	/// </summary>
	public class DisableSelectorValidationScope : OverrideAttributePropertyScope<PXSelectorAttribute, bool>
	{
		public DisableSelectorValidationScope(PXCache cache, params Type[] fields)
			: base(
				cache, 
				fields,
				(attribute, validateValue) => attribute.ValidateValue = validateValue,
				(attribute) => attribute.ValidateValue,
				false)
		{ }
	}
}
