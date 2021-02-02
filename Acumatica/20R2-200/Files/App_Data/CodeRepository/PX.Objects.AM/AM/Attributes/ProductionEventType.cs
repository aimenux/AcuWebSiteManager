using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Production events
    /// </summary>
    public class ProductionEventType
    {
        /// <summary>
        /// Generic/info/default production event
        /// </summary>
        public const int Info = 0;
        /// <summary>
        /// Production order created
        /// </summary>
        public const int OrderCreated = 1;
        /// <summary>
        /// Production order released
        /// </summary>
        public const int OrderReleased = 2;
        /// <summary>
        /// Production order set on hold
        /// </summary>
        public const int OrderPlaceOnHold = 3;
        /// <summary>
        /// Production order removed from Hold
        /// </summary>
        public const int OrderRemoveFromHold = 4;
        /// <summary>
        /// Production order completed
        /// </summary>
        public const int OrderCompleted = 5;
        /// <summary>
        /// Production order closed
        /// </summary>
        public const int OrderClosed = 6;
        /// <summary>
        /// Production order canceled
        /// </summary>
        public const int OrderCancelled = 7;
        /// <summary>
        /// Production order changed back to a Plan status
        /// </summary>
        public const int OrderResetToPlan = 8;
        /// <summary>
        /// Production order report printed
        /// </summary>
        public const int ReportPrinted = 9;
        /// <summary>
        /// Production transaction event
        /// </summary>
        public const int Transaction = 10;
        /// <summary>
        /// User added comment
        /// </summary>
        public const int Comment = 11;
        /// <summary>
        /// Production order was edited
        /// </summary>
        public const int OrderEdit = 15;
        /// <summary>
        /// Production order operations changed
        /// </summary>
        public const int OperationChange = 16;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Info => Messages.GetLocal(Messages.Info);
            public static string Comment => Messages.GetLocal(Messages.Comment);
            public static string OrderCreated => Messages.GetLocal(Messages.Created);
            public static string OrderReleased => Messages.GetLocal(Messages.Released);
            public static string OrderPlaceOnHold => Messages.GetLocal(Messages.OnHold);
            public static string OrderRemoveFromHold => Messages.GetLocal(Messages.HoldRemoved);
            public static string OrderCompleted => Messages.GetLocal(Messages.Completed);
            public static string OrderClosed => Messages.GetLocal(Messages.Closed);
            public static string OrderCancelled => Messages.GetLocal(Messages.Canceled);
            public static string OrderResetToPlan => Messages.GetLocal(Messages.ResetToPlan);
            public static string ReportPrinted => Messages.GetLocal(Messages.ReportPrinted);
            public static string Transaction => Messages.GetLocal(Messages.Transaction);
            public static string OrderEdit => Messages.GetLocal(Messages.OrderEdit);
            public static string OperationChange => Messages.GetLocal(Messages.OperationChange);
        }

        public static string GetEventTypeDescription(int eventType)
        {
            switch (eventType)
            {
                case Info:
                    return Desc.Info;
                case Comment:
                    return Desc.Comment;
                case OrderCreated:
                    return Desc.OrderCreated;
                case OrderReleased:
                    return Desc.OrderReleased;
                case OrderPlaceOnHold:
                    return Desc.OrderPlaceOnHold;
                case OrderRemoveFromHold:
                    return Desc.OrderRemoveFromHold;
                case OrderCompleted:
                    return Desc.OrderCompleted;
                case OrderClosed:
                    return Desc.OrderClosed;
                case OrderCancelled:
                    return Desc.OrderCancelled;
                case OrderResetToPlan:
                    return Desc.OrderResetToPlan;
                case Transaction:
                    return Desc.Transaction;
                case ReportPrinted:
                    return Desc.ReportPrinted;
                case OperationChange:
                    return Desc.OperationChange;
            }

            return string.Empty;
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new[] {
                      Info,
                      Comment,
                      OrderCreated,
                      OrderReleased,
                      OrderPlaceOnHold,
                      OrderRemoveFromHold,
                      OrderCompleted,
                      OrderClosed,
                      OrderCancelled,
                      OrderResetToPlan,
                      ReportPrinted,
                      Transaction,
                      OrderEdit,
                      OperationChange}, 
                new[] {
                    Messages.Info,
                    Messages.Comment,
                    Messages.Created,
                    Messages.Released,
                    Messages.OnHold,
                    Messages.HoldRemoved,
                    Messages.Completed,
                    Messages.Closed,
                    Messages.Canceled,
                    Messages.ResetToPlan,
                    Messages.ReportPrinted,
                    Messages.Transaction,
                    Messages.OrderEdit,
                    Messages.OperationChange
                }) { }
        }

        public class info : PX.Data.BQL.BqlInt.Constant<info> { public info() : base(Info){} }

        public class comment : PX.Data.BQL.BqlInt.Constant<comment> { public comment() : base(Comment) { } }

        public class orderCreated : PX.Data.BQL.BqlInt.Constant<orderCreated> { public orderCreated() : base(OrderCreated) { } }

        public class orderReleased : PX.Data.BQL.BqlInt.Constant<orderReleased> { public orderReleased() : base(OrderReleased) { } }

        public class orderPlaceOnHold : PX.Data.BQL.BqlInt.Constant<orderPlaceOnHold> { public orderPlaceOnHold() : base(OrderPlaceOnHold) { } }

        public class orderRemoveFromHold : PX.Data.BQL.BqlInt.Constant<orderRemoveFromHold> { public orderRemoveFromHold() : base(OrderRemoveFromHold) { } }

        public class orderCompleted : PX.Data.BQL.BqlInt.Constant<orderCompleted> { public orderCompleted() : base(OrderCompleted) { } } 

        public class orderClosed : PX.Data.BQL.BqlInt.Constant<orderClosed> { public orderClosed() : base(OrderClosed) { } }

        public class orderCancelled : PX.Data.BQL.BqlInt.Constant<orderCancelled> { public orderCancelled() : base(OrderCancelled) { } }

        public class orderResetToPlan : PX.Data.BQL.BqlInt.Constant<orderResetToPlan> { public orderResetToPlan() : base(OrderResetToPlan) { } }

        public class reportPrinted : PX.Data.BQL.BqlInt.Constant<reportPrinted> { public reportPrinted() : base(ReportPrinted) { } }

        public class transaction : PX.Data.BQL.BqlInt.Constant<transaction> { public transaction() : base(Transaction) { } } 

        public class orderEdit : PX.Data.BQL.BqlInt.Constant<orderEdit> { public orderEdit() : base(OrderEdit) { } }

        public class operationChange : PX.Data.BQL.BqlInt.Constant<operationChange> { public operationChange() : base(OperationChange) { } }
    }
}