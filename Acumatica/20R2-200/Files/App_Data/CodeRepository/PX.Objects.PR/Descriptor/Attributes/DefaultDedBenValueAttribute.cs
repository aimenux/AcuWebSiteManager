using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class DefaultDedBenValueAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowSelectedSubscriber
	{
		private Type _CodeIDField;
		private Type _CalcTypeField;
		private Type _ContribTypeField;
		private Type _ValueField;
		private List<string> _ExpectedCalcTypes;
		private List<string> _ExpectedContribTypes;

		public DefaultDedBenValueAttribute(Type codeIDField, Type calcTypeField, string[] expectedCalcTypes, Type contribTypeField, string[] expectedContribTypes, Type valueField)
		{
			_CodeIDField = codeIDField;
			_CalcTypeField = calcTypeField;
			_ContribTypeField = contribTypeField;
			_ValueField = valueField;
			_ExpectedCalcTypes = new List<string>(expectedCalcTypes);
			_ExpectedContribTypes = new List<string>(expectedContribTypes);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache definitionCache = sender.Graph.Caches[_CalcTypeField.DeclaringType];
			object definition = PXSelectorAttribute.Select(sender, e.Row, _CodeIDField.Name);
			if (IsFieldApplicable(sender, e.Row))
			{
				e.NewValue = definitionCache.GetValue(definition, _ValueField.Name);
			}
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName, IsFieldApplicable(sender, e.Row));
		}

		private bool IsFieldApplicable(PXCache sender, object row)
		{
			PXCache definitionCache = sender.Graph.Caches[_CalcTypeField.DeclaringType];
			object definition = PXSelectorAttribute.Select(sender, row, _CodeIDField.Name);
			return definition != null && _ExpectedContribTypes.Contains(definitionCache.GetValue(definition, _ContribTypeField.Name)) &&
				_ExpectedCalcTypes.Contains(definitionCache.GetValue(definition, _CalcTypeField.Name));
		}
	}
}
