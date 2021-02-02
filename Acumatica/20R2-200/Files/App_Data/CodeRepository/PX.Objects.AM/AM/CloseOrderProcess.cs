using System;
using System.Collections;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.GL;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Close production orders process graph
    /// </summary>
public class CloseOrderProcess : PXGraph<CloseOrderProcess>
    {
        public PXCancel<AMProdItem> Cancel;
        [PXFilterable]
        public PXProcessing<AMProdItem, Where<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>> CompletedOrders;
        public PXFilter<FinancialPeriod> FinancialPeriod;
        public PXSetup<AMPSetup> ampsetup;

        public CloseOrderProcess()
        {
            FinancialPeriod financialPeriod = FinancialPeriod.Current;
            CompletedOrders.SetProcessDelegate(delegate(List<AMProdItem> list)
            {
                CloseOrders(list, true, financialPeriod);
            });
            
            InquiresDropMenu.AddMenuAction(TransactionsByProductionOrderInq);
        }

        public PXAction<AMProdItem> InquiresDropMenu;
        [PXUIField(DisplayName = Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable inquiresDropMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMProdItem> TransactionsByProductionOrderInq;
        [PXUIField(DisplayName = "Unreleased Transactions", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable transactionsByProductionOrderInq(PXAdapter adapter)
        {
            CallTransactionsByProductionOrderGenericInquiry();

            return adapter.Get();
        }

        protected virtual void CallTransactionsByProductionOrderGenericInquiry()
        {
            var gi = new GITransactionsByProductionOrder();
            gi.SetFilterByProductionStatus(ProductionOrderStatus.Completed);
            gi.SetFilterByUnreleasedBatches();
            gi.CallGenericInquiry();
        }

        public static void CloseOrders(List<AMProdItem> list, bool isMassProcess, FinancialPeriod financialPeriod)
        {
            var closeOrderGraph = PXGraph.CreateInstance<CloseOrderProcess>();
            closeOrderGraph.Clear();
            closeOrderGraph.ReleaseOrder(list, isMassProcess, financialPeriod);

            try
            {
                closeOrderGraph.RunCleanup();
            }
            catch (Exception exception)
            {
                PXTrace.WriteError(exception);
            }
        }

        /// <summary>
        /// Cleanup process for records no longer required or used.
        /// The process is called as the end of the end of closing all production orders.
        /// The processed records are independent of any orders that might or might not have been processed.
        /// </summary>
        protected virtual void RunCleanup()
        {
            APSMaintenanceProcess.RunHistoryCleanupProcess();
        }

        protected virtual void ReleaseOrder(List<AMProdItem> list, bool isMassProcess, FinancialPeriod financialPeriod)
        {
            ProductionStatus.SetStatusTranScope(list, ProductionOrderStatus.Closed, financialPeriod, isMassProcess, true);
        }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1 + " Use ReleaseOrder(List<AMProdItem>, bool, FinancialPeriod)")]
        protected virtual void ReleaseOrder(AMProdItem doc, FinancialPeriod financialPeriod)
        {
            if (ProductionTransactionHelper.ProductionOrderHasUnreleasedTransactions(this, doc, out var unreleasedMsg))
            {
                throw new PXException(unreleasedMsg);
            }

            ProductionStatus.SetStatus(new List<AMProdItem> { doc }, ProductionOrderStatus.Closed, financialPeriod);
        }
    }

    [Serializable]
    [PXCacheName("Financial Period")]
    public class FinancialPeriod : IBqlTable
    {
        #region FinancialPeriodID
        public abstract class financialPeriodID : PX.Data.BQL.BqlString.Field<financialPeriodID> { }

        protected String _FinancialPeriodID;
        [OpenPeriod(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
        public virtual String FinancialPeriodID
        {
            get
            {
                return this._FinancialPeriodID;
            }
            set
            {
                this._FinancialPeriodID = value;
            }
        }
        #endregion
    }
}