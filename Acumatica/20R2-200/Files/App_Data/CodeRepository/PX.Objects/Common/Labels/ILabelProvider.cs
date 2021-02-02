using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	public interface ILabelProvider
	{
		IEnumerable<ValueLabelPair> ValueLabelPairs { get; }
	}

	public static class LabelProvider
	{
		public static string GetLabel(this ILabelProvider provider, string value)
		{
			return provider
				.ValueLabelPairs
				.Single(pair => pair.Value == value)
				.Label;
		}

		public static IEnumerable<string> Values(this ILabelProvider provider)
		{
			return provider.ValueLabelPairs.Select(pair => pair.Value);
		}

		public static IEnumerable<string> Labels(this ILabelProvider provider)
		{
			return provider.ValueLabelPairs.Select(pair => pair.Label);
		}
	}
}
