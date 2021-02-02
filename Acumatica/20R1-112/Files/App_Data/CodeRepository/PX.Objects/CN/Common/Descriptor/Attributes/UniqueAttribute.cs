using System;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class UniqueAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public string ErrorMessage
		{
			get;
			set;
		} = ErrorMessages.ValueIsNotUnique;

		public Type WhereCondition
		{
			get;
			set;
		}

		public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
		{
			if (args.Operation.IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				ValidateEntity(cache, args.Row);
			}
		}

		private void ValidateEntity(PXCache cache, object entity)
		{
			var value = cache.GetValue(entity, _FieldName);
			if (value != null)
			{
				var view = GetView(cache);
				var records = view.SelectMulti(value.SingleToArray());
				if (records.HasAtLeastTwoItems())
				{
					cache.RaiseException(_FieldName, entity, ErrorMessage, value);
				}
			}
		}

		private PXView GetView(PXCache cache)
		{
			var command = GetBqlCommand(cache);
			if (WhereCondition != null)
			{
				command = command.WhereAnd(WhereCondition);
			}
			return new PXView(cache.Graph, false, command);
		}

		private BqlCommand GetBqlCommand(PXCache cache)
		{
			var fieldType = cache.GetBqlField(_FieldName);
			return BqlCommand.CreateInstance(typeof(Select<,>), BqlTable,
				typeof(Where<,>), fieldType, typeof(Equal<>), typeof(Required<>), fieldType);
		}
	}
}