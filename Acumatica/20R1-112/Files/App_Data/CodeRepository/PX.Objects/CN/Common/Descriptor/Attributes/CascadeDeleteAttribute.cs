using System;
using PX.Data;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	/// <summary>
	/// Attribute used for cascade deleting child entities.
	/// Unlike <see cref="PXParentAttribute"/> not require DataView of the child entity in related Graph.
	/// Should be set on key property as use FieldName parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class CascadeDeleteAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		private readonly Type childReferenceType;

		private readonly Type childReferenceKeyType;

		public CascadeDeleteAttribute(Type childReferenceType, Type childReferenceKeyType)
		{
			this.childReferenceType = childReferenceType;
			this.childReferenceKeyType = childReferenceKeyType;
		}

		public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
		{
			if (args.Operation == PXDBOperation.Delete)
			{
				var view = GetChildView(cache);
				var childEntities = view.SelectMulti(cache.GetValue(args.Row, FieldName));
				childEntities.ForEach(e => view.Cache.Delete(e));
				view.Cache.Persist(PXDBOperation.Delete);
				view.Cache.Clear();
			}
		}

		private PXView GetChildView(PXCache cache)
		{
			var command = BqlCommand.CreateInstance(
				typeof(Select<,>), childReferenceType,
				typeof(Where<,>), childReferenceKeyType, typeof(Equal<>), typeof(Required<>), childReferenceKeyType);
			return new PXView(cache.Graph, false, command);
		}
	}
}
