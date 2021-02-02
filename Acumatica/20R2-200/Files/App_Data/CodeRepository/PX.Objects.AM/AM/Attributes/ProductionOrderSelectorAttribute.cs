using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing production order number selector
    /// </summary>
    public class ProductionOrderSelectorAttribute : PXSelectorAttribute
    {
        /// <summary>
        /// Production order number selector showing active production orders only
        /// </summary>
        /// <example> 
        /// [ProductionOrderSelector(typeof(AMProdItem.orderType))]
        /// </example>
        /// <param name="orderTypeField">field containing the order type to use for the order selection</param>
        public ProductionOrderSelectorAttribute(Type orderTypeField) 
            : this(orderTypeField, false)
        { }

        /// <summary>
        /// Production order number selector
        /// </summary>
        /// <example> 
        /// [ProductionOrderSelector(typeof(AMProdItem.orderType), true)]
        /// </example>
        /// <param name="orderTypeField">field containing the order type to use for the order selection</param>
        /// <param name="includeAll">Indicates if the selector should return all orders (when true) or only the active orders (when false)</param>
        public ProductionOrderSelectorAttribute(Type orderTypeField, bool includeAll)
            : base(GetOrders(orderTypeField, includeAll), CustomerColumnList)
        {
            _DescriptionField = typeof(AMProdItem.descr);
        }

        /// <summary>
        /// Production order number selector
        /// </summary>
        /// <param name="orderTypeField">field containing the order type to use for the order selection</param>
        /// <param name="includeAll">Indicates if the selector should return all orders (when true) or only the active orders (when false)</param>
        /// <param name="fieldList">Fields list for display as the selector columns</param>
        public ProductionOrderSelectorAttribute(Type orderTypeField, bool includeAll, params Type[] fieldList)
            : base(GetOrders(orderTypeField, includeAll), fieldList)
        {
            _DescriptionField = typeof(AMProdItem.descr);
        }

        /// <summary>
        /// Builds the selector query
        /// </summary>
        /// <param name="orderTypeField">field containing the order type to use for the order selection</param>
        /// <param name="additional">additional where statement</param>
        private static Type GetQueryCommand(Type orderTypeField, Type additional)
        {
            Type join = typeof(LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMProdItem.inventoryID>>
                    , LeftJoin<Customer, On<Customer.bAccountID, Equal<AMProdItem.customerID>>>>);

            Type where = BqlCommand.Compose(typeof(Where<,>), typeof(AMProdItem.orderType), typeof(Equal<>), typeof(Optional<>), orderTypeField);

            Type order = typeof(OrderBy<Asc<AMProdItem.prodOrdID>>);

            if (additional != null)
            {
                where = BqlCommand.Compose(typeof(Where2<,>), where, typeof(And<>), additional);
            }

            return BqlCommand.Compose(typeof(Search2<,,,>), typeof(AMProdItem.prodOrdID), join, where, order);
        }

        private static Type GetOrders(Type orderTypeField, bool includeAll)
        {
            if (includeAll)
            {
                return AllOrders(orderTypeField);
            }

            return ActiveOrdersOnly(orderTypeField);
        }

        private static Type ActiveOrdersOnly(Type orderTypeField)
        {
            return GetQueryCommand(orderTypeField, typeof(Where<AMProdItem.hold, NotEqual<True>,
                        And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                            Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>));
        }

        private static Type AllOrders(Type orderTypeField)
        {
            return GetQueryCommand(orderTypeField, null);
        }

        /// <summary>
        /// Column list for production order selector including customer/inventory dacs
        /// </summary>
        public static Type[] CustomerColumnList
        {
            get
            {
                return new Type[] {typeof(AMProdItem.prodOrdID)
                    , typeof(AMProdItem.inventoryID)
                    , typeof(AMProdItem.subItemID)
                    , typeof(AMProdItem.prodDate)
                    , typeof(AMProdItem.statusID)
                    , typeof(AMProdItem.siteID)
                    , typeof(AMProdItem.qtytoProd)
                    , typeof(AMProdItem.uOM)
                    , typeof(AMProdItem.hold)
                    , typeof(AMProdItem.customerID)
                    , typeof(Customer.acctName)
                    , typeof(AMProdItem.orderType)
                    , typeof(AMProdItem.descr)
                    , typeof(AMProdItem.projectID)
                    , typeof(AMProdItem.taskID)
                    , typeof(AMProdItem.costCodeID)
                };
            }}
    }
}