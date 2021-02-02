using PX.Common;
using PX.Data;

namespace PX.Objects.SO.Attributes
{
	public class CustomerOrderNbrAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXFieldVerifyingSubscriber, IPXRowUpdatedSubscriber
	{
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var newValue = (string)e.NewValue;
			var order = (SOOrder)e.Row;
			if (order?.OrderType == null || order.CustomerID == null)
				return;

			var orderType = SOOrderType.PK.Find(sender.Graph, order.OrderType);
			if (orderType.CustomerOrderIsRequired != true || string.IsNullOrWhiteSpace(newValue) ||
				orderType.CustomerOrderValidation.IsNotIn(CustomerOrderValidationType.Warn, CustomerOrderValidationType.Error))
				return;

			SOOrder duplicateOrder = FindCustomerOrderDuplicate(sender, newValue, order);
			if (duplicateOrder == null)
				return;

			if (orderType.CustomerOrderValidation == CustomerOrderValidationType.Error)
			{
				throw new PXSetPropertyException(Messages.CustomerOrderHasDuplicateError, PXErrorLevel.Error, duplicateOrder.OrderNbr);
			}

			sender.RaiseExceptionHandling<SOOrder.customerOrderNbr>(order, newValue,
				new PXSetPropertyException(Messages.CustomerOrderHasDuplicateWarning, PXErrorLevel.Warning, duplicateOrder.OrderNbr));
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var order = (SOOrder)e.Row;
			var oldOrder = (SOOrder)e.OldRow;
			if (order?.OrderType == null || order.CustomerID == null || oldOrder.CustomerID == order.CustomerID)
				return;

			var orderType = SOOrderType.PK.Find(sender.Graph, order.OrderType);
			if (orderType.CustomerOrderIsRequired != true || string.IsNullOrWhiteSpace(order.CustomerOrderNbr) ||
				orderType.CustomerOrderValidation.IsNotIn(CustomerOrderValidationType.Warn, CustomerOrderValidationType.Error))
				return;

			SOOrder duplicateOrder = FindCustomerOrderDuplicate(sender, order.CustomerOrderNbr, order);
			if (duplicateOrder == null)
				return;

			var exception = orderType.CustomerOrderValidation == CustomerOrderValidationType.Error
				? new PXSetPropertyException(Messages.CustomerOrderHasDuplicateError, PXErrorLevel.Error, duplicateOrder.OrderNbr)
				: new PXSetPropertyException(Messages.CustomerOrderHasDuplicateWarning, PXErrorLevel.Warning, duplicateOrder.OrderNbr);
			sender.RaiseExceptionHandling<SOOrder.customerOrderNbr>(order, order.CustomerOrderNbr, exception);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var order = (SOOrder)e.Row;
			if (order?.OrderType == null || order.CustomerID == null || e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			var orderType = SOOrderType.PK.Find(sender.Graph, order.OrderType);
			if (orderType.CustomerOrderIsRequired != true)
				return;

			if (string.IsNullOrWhiteSpace(order.CustomerOrderNbr) &&
				sender.RaiseExceptionHandling<SOOrder.customerOrderNbr>(
					order,
					null,
					new PXSetPropertyKeepPreviousException(Messages.CustomerOrderIsEmpty)))
			{
				throw new PXRowPersistingException(nameof(SOOrder.customerOrderNbr), null, Messages.CustomerOrderIsEmpty);
			}

			if (orderType.CustomerOrderValidation != CustomerOrderValidationType.Error)
				return;

			SOOrder duplicateOrder = FindCustomerOrderDuplicate(sender, order.CustomerOrderNbr, order);
			if (duplicateOrder != null &&
				sender.RaiseExceptionHandling<SOOrder.customerOrderNbr>(
					order,
					order.CustomerOrderNbr,
					new PXSetPropertyException(Messages.CustomerOrderHasDuplicateError, PXErrorLevel.Error, duplicateOrder.OrderNbr)))
			{
				throw new PXRowPersistingException(
					nameof(SOOrder.customerOrderNbr),
					null,
					Messages.CustomerOrderHasDuplicateError,
					duplicateOrder.OrderNbr);
			}
		}

		protected virtual SOOrder FindCustomerOrderDuplicate(PXCache orderCache, string customerOrderNbr, SOOrder order)
		{
			return PXSelectReadonly<SOOrder,
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrder.customerID, Equal<Required<SOOrder.customerID>>,
					And<SOOrder.customerOrderNbr, Equal<Required<SOOrder.customerOrderNbr>>,
					And<SOOrder.orderNbr, NotEqual<Required<SOOrder.orderNbr>>>>>>>
				.SelectWindowed(orderCache.Graph, 0, 1, order.OrderType, order.CustomerID, customerOrderNbr, order.OrderNbr);
		}
	}
}
