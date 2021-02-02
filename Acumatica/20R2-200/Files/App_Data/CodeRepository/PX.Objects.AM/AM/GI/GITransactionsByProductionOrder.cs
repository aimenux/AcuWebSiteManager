using System;
using PX.Data;

namespace PX.Objects.AM
{
    public class GITransactionsByProductionOrder : AMGenericInquiry
    {
        protected const string GIID = "91d4d726-cbcd-42a5-a12b-d5f52986e4ef";

        public GITransactionsByProductionOrder()
            : base(new Guid(GIID))
        {
        }

        /// <summary>
        /// Set the GI call to filter for a specific production order
        /// </summary>
        public virtual void SetFilterByProductionOrder(string orderType, string prodOrdId)
        {
            AddFilter(typeof(AMProdItem), typeof(AMProdItem.orderType), PXCondition.EQ, orderType);
            AddFilter(typeof(AMProdItem), typeof(AMProdItem.prodOrdID), PXCondition.EQ, prodOrdId);
        }

        /// <summary>
        /// Set the GI call to filter for a specific production order status
        /// </summary>
        public virtual void SetFilterByProductionStatus(string status)
        {
            AddFilter(typeof(AMProdItem), typeof(AMProdItem.statusID), PXCondition.EQ, status);
        }

        /// <summary>
        /// Set the GI call to filter for showing only unreleased batches
        /// </summary>
        public virtual void SetFilterByUnreleasedBatches()
        {
            AddFilter(typeof(AMBatch), typeof(AMBatch.released), PXCondition.EQ, false);
        }
    }
}