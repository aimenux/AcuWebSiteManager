using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;

namespace PX.Objects.Common
{
	public class LabelListAttribute : PXStringListAttribute
	{
		public LabelListAttribute(Type labelProviderType)
		{
			if (!typeof(ILabelProvider).IsAssignableFrom(labelProviderType))
			{
				throw new PXException(
					Messages.TypeMustImplementLabelProvider, 
					labelProviderType.Name);
			}

			try
			{
				ILabelProvider labelProvider =
					Activator.CreateInstance(labelProviderType) as ILabelProvider;

				List<string> values = new List<string>();
				List<string> labels = new List<string>();

				labelProvider.ValueLabelPairs.ForEach(pair =>
				{
					values.Add(pair.Value);
					labels.Add(pair.Label);
				});

				_AllowedValues = values.ToArray();
				_AllowedLabels = labels.ToArray();
				_NeutralAllowedLabels = _AllowedLabels;
			}
			catch (MissingMethodException exception)
			{
				throw new PXException(
					exception,
					Messages.LabelProviderMustHaveParameterlessConstructor,
					labelProviderType.Name);
			}
		}

		public LabelListAttribute(IEnumerable<ValueLabelPair> valueLabelPairs)
			: base(
				  valueLabelPairs.Select(pair => pair.Value).ToArray(),
				  valueLabelPairs.Select(pair => pair.Label).ToArray())
		{ }
	}
}
