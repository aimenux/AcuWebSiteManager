using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	public class ValueLabelList : IEnumerable<ValueLabelPair>
	{
		private readonly List<ValueLabelPair> _valueLabelPairs = new List<ValueLabelPair>();
		public ValueLabelList()
		{ }
		public ValueLabelList(IEnumerable<ValueLabelPair> valueLabelPairs)
		{
			_valueLabelPairs = valueLabelPairs.ToList();
		}
		public void Add(string value, string label)
		{
			_valueLabelPairs.Add(new ValueLabelPair(value, label));
		}
		public void Add(IEnumerable<ValueLabelPair> list)
		{
			_valueLabelPairs.AddRange(list);
		}
		public IEnumerator<ValueLabelPair> GetEnumerator() =>
			_valueLabelPairs.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			_valueLabelPairs.GetEnumerator();
	}
}
