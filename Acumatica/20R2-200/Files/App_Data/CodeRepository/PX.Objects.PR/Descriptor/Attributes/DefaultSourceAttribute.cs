using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultSourceAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private Type _DefaultFlagField;
		private Type _DefaultSourceField;
		private Type[] _SourceKeys;
		private Type[] _DestinationKeys;

		private Type[] Search
		{
			get
			{
				return new Type[]
				{
					typeof(Search<>),
					_DefaultSourceField,
				};
			}
		}

		public DefaultSourceAttribute(Type defaultFlagField, Type defaultSourceField, Type[] sourceKeys, Type[] destinationKeys)
		{
			_DefaultFlagField = defaultFlagField;
			_DefaultSourceField = defaultSourceField;
			_SourceKeys = sourceKeys;
			_DestinationKeys = destinationKeys;
		}

		public void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			var flagValue = (bool?)cache.GetValue(e.Row, _DefaultFlagField.Name);
			if (flagValue == true)
			{
				Type searchBql = BqlCommand.Compose(Search);
				BqlCommand cmd = BqlCommand.CreateInstance(searchBql);
				System.Collections.Generic.IEnumerable<object> keyValues = _DestinationKeys.Select(x => cache.GetValue(e.Row, x.Name));
				foreach (Type key in _SourceKeys)
				{
					cmd = cmd.WhereAnd(BqlCommand.Compose(typeof(Where<,>), key, typeof(Equal<>), typeof(Required<>), key));
				}
				PXView view = new PXView(cache.Graph, false, cmd);
				object result = view.SelectSingle(keyValues.ToArray());

				object value = cache.Graph.Caches[_DefaultSourceField.DeclaringType.Name].GetValue(result, _DefaultSourceField.Name);
				cache.SetValue(e.Row, FieldName, value);
				e.ReturnValue = value;
			}
		}
	}
}