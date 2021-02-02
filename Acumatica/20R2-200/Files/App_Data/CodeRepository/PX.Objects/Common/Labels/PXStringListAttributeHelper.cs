using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.Labels
{
	public class PXStringListAttributeHelper
	{
		public static void SetList<TField>(PXCache cache, object row, ILabelProvider labelProvider)
			where TField : IBqlField
		{
			PXStringListAttribute.SetList<TField>(
				cache,
				row,
				labelProvider.ValueLabelPairs.Select(kvp => kvp.Value).ToArray(),
				labelProvider.ValueLabelPairs.Select(kvp => kvp.Label).ToArray());
		}
	}
}
