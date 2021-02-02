using System;
using System.Linq;
using PX.Data;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	public class IgnoreUpdateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber
	{
		private readonly Type[] graphTypes;

		public IgnoreUpdateAttribute(params Type[] graphTypes)
		{
			this.graphTypes = graphTypes;
		}

		public void FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs args)
		{
			if (IsGraphApplicable(cache.Graph) && !IsAnyFieldUpdated(cache, args.Row))
			{
				cache.SetStatus(args.Row, PXEntryStatus.Notchanged);
			}
		}

		private bool IsGraphApplicable(PXGraph graph)
		{
			var graphType = graph.GetType();
			return graphTypes.IsEmpty() || graphTypes.Contains(graphType);
		}

		private bool IsAnyFieldUpdated(PXCache cache, object newRow)
		{
			var original = cache.GetOriginal(newRow);
			foreach (var fieldType in cache.BqlFields)
			{
				var fieldName = cache.GetField(fieldType);
				var oldValue = cache.GetValue(original, fieldName)?.ToString();
				var newValue = cache.GetValue(newRow, fieldName)?.ToString();
				if (fieldName != _FieldName && oldValue != newValue)
				{
					return true;
				}
			}
			return false;
		}
	}
}