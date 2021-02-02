using System;
using PX.Data;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	public class DependsOnFieldAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		public bool ShouldDisable = true;
		private readonly Type fieldType;

		public DependsOnFieldAttribute(Type fieldType)
		{
			this.fieldType = fieldType;
		}

		public override void CacheAttached(PXCache cache)
		{
			cache.Graph.RowUpdated.AddHandler(BqlTable, RowUpdated);
		}

		public void RowSelected(PXCache cache, PXRowSelectedEventArgs args)
		{
			if (args.Row != null && ShouldDisable)
			{
				var field = cache.GetField(fieldType);
				var fieldValue = cache.GetValue(args.Row, field);
				PXUIFieldAttribute.SetEnabled(cache, args.Row, FieldName, fieldValue != null);
			}
		}

		private void RowUpdated(PXCache cache, PXRowUpdatedEventArgs args)
		{
			var field = cache.GetField(fieldType);
			var oldValue = cache.GetValue(args.OldRow, field);
			var newValue = cache.GetValue(args.Row, field);
			if (!Equals(oldValue, newValue))
			{
				cache.SetValue(args.Row, FieldName, null);
				cache.RaiseExceptionHandling(FieldName, args.Row, null, null);
			}
		}
	}
}