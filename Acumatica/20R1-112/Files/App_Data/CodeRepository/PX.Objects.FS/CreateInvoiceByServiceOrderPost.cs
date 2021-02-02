using PX.Data;
using PX.Objects.AR;
using PX.Objects.FS.ParallelProcessing;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class CreateInvoiceByServiceOrderPost : CreateInvoiceBase<CreateInvoiceByServiceOrderPost, ServiceOrderToPost>
    {
        #region Selects
        [PXFilterable]
        public new PXFilteredProcessingJoin<ServiceOrderToPost, CreateInvoiceFilter,
                LeftJoinSingleTable<Customer,
                    On<Customer.bAccountID, Equal<ServiceOrderToPost.billCustomerID>>>,
               Where2<
                   Where<Current<FSSetup.filterInvoicingManually>, Equal<False>,
                    Or<Current<CreateInvoiceFilter.loadData>, Equal<True>>>,
               And2<
                   Where<Customer.bAccountID, IsNull,
                    Or<Match<Customer, Current<AccessInfo.userName>>>>,
               And<
                   Where2<
                       Where2<
                           Where<
                                Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.AR_AP>,
                                And<
                                    ServiceOrderToPost.postTo, Equal<ListField_PostTo.AR>>>,
                        Or<
                           Where2< 
                               Where<
                                    Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.SO>,
                                    And<
                                        ServiceOrderToPost.postTo, Equal<ListField_PostTo.SO>>>,
                            Or<
                                Where2<
                                    Where<
                                        Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.SI>,
                                        And<
                                            ServiceOrderToPost.postTo, Equal<ListField_PostTo.SI>>>,
                                    Or<
                                        Where<
                                            Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.PM>,
                                            And<ServiceOrderToPost.postTo, Equal<ListField_PostTo.PM>>>>>>>>>,
                   And<
                       Where2<
                            Where<
                                ServiceOrderToPost.billingBy, Equal<ListField_Billing_By.ServiceOrder>,
                                And<ServiceOrderToPost.postedBy, IsNull,
                                And<ServiceOrderToPost.pendingAPARSOPost, Equal<True>>>>,
                        And<
                            Where2<
                                Where<
                                    Current<CreateInvoiceFilter.billingCycleID>, IsNull,
                                Or<
                                    ServiceOrderToPost.billingCycleID, Equal<Current<CreateInvoiceFilter.billingCycleID>>>>,
                            And<
                                Where2<
                                    Where<
                                        Current<CreateInvoiceFilter.customerID>, IsNull,
                                    Or<
                                        ServiceOrderToPost.billCustomerID, Equal<Current<CreateInvoiceFilter.customerID>>>>,
                                And<
                                    Where2<
                                        Where2<
                                            Where<
                                                ServiceOrderToPost.billingCycleType, NotEqual<ListField_Billing_Cycle_Type.WorkOrder>,
                                                And<ServiceOrderToPost.billingCycleType, NotEqual<ListField_Billing_Cycle_Type.PurchaseOrder>>>,
                                                Or<
                                                    Where2<
                                                        Where<
                                                            ServiceOrderToPost.billingCycleType, Equal<ListField_Billing_Cycle_Type.WorkOrder>>,
                                                    Or<
                                                        Where<
                                                            ServiceOrderToPost.billingCycleType, Equal<ListField_Billing_Cycle_Type.PurchaseOrder>>>>>>,
                                    And<
                                        Where2<
                                            Where<
                                                Current<CreateInvoiceFilter.ignoreBillingCycles>, Equal<False>,
                                            And<
                                                ServiceOrderToPost.cutOffDate, LessEqual<Current<CreateInvoiceFilter.upToDate>>>>,
                                        Or<
                                            Where<
                                                Current<CreateInvoiceFilter.ignoreBillingCycles>, Equal<True>,
                                            And<
                                                ServiceOrderToPost.filterDate, LessEqual<Current<CreateInvoiceFilter.upToDate>>>>>>>>>>>>>>>>>>>> PostLines;
        #endregion

        #region ViewPostBatch
        public PXAction<CreateInvoiceFilter> viewPostBatch;
        [PXUIField(DisplayName = "")]
        public virtual IEnumerable ViewPostBatch(PXAdapter adapter)
        {
            if (PostLines.Current != null)
            {
                ServiceOrderToPost postLineRow = PostLines.Current;
                PostBatchMaint graphPostBatchMaint = PXGraph.CreateInstance<PostBatchMaint>();

                if (postLineRow.BatchID != null)
                {
                    graphPostBatchMaint.BatchRecords.Current = graphPostBatchMaint.BatchRecords.Search<FSPostBatch.batchID>(postLineRow.BatchID);
                    throw new PXRedirectRequiredException(graphPostBatchMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }

            return adapter.Get();
        }
        #endregion

        #region CacheAttached
        #region ServiceOrderToPost_SOID
        [PXDBInt]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(
            Search<ServiceOrderToPost.sOID,
            Where<
                    ServiceOrderToPost.srvOrdType, Equal<Current<ServiceOrderToPost.srvOrdType>>>>),
            SubstituteKey = typeof(ServiceOrderToPost.refNbr))]
        protected virtual void ServiceOrderToPost_SOID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Event Handlers
        protected override void _(Events.RowSelected<CreateInvoiceFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            base._(e);

            CreateInvoiceFilter createInvoiceFilterRow = (CreateInvoiceFilter)e.Row;

            string errorMessage = PXUIFieldAttribute.GetErrorOnly<CreateInvoiceFilter.invoiceFinPeriodID>(e.Cache, createInvoiceFilterRow);
            bool enableProcessButtons = string.IsNullOrEmpty(errorMessage) == true;

            PostLines.SetProcessAllEnabled(enableProcessButtons);
            PostLines.SetProcessEnabled(enableProcessButtons);
        }
        #endregion

        public CreateInvoiceByServiceOrderPost() : base()
        {
            billingBy = ID.Billing_By.SERVICE_ORDER;
            CreateInvoiceByServiceOrderPost graphCreateInvoiceByServiceOrderPost = null;

            PostLines.SetProcessDelegate(
                delegate(List<ServiceOrderToPost> serviceOrderToPostRows)
                {
                    graphCreateInvoiceByServiceOrderPost = PXGraph.CreateInstance<CreateInvoiceByServiceOrderPost>();

                    var jobExecutor = new JobExecutor<InvoicingProcessStepGroupShared>(true);

                    CreateInvoices(graphCreateInvoiceByServiceOrderPost, serviceOrderToPostRows, Filter.Current, this.UID, jobExecutor, PXQuickProcess.ActionFlow.NoFlow);
                    UpdateUnitPricesAndCosts(serviceOrderToPostRows);
                });
        }

        public override List<DocLineExt> GetInvoiceLines(Guid currentProcessID, int billingCycleID, string groupKey, bool getOnlyTotal, out decimal? invoiceTotal, string postTo)
        {
            PXGraph tempGraph = new PXGraph();

            if (getOnlyTotal == true)
            {
                /* Always keep both BQLs with the same Joins and Where conditions */
                FSSODet fsSODetRow =
                        PXSelectJoinGroupBy<FSSODet,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.srvOrdType, Equal<FSSODet.srvOrdType>,
                                    And<FSServiceOrder.refNbr, Equal<FSSODet.refNbr>>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.sOID, Equal<FSSODet.sOID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Service_Order>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSSODet.postID>>>>>>,
                        Where<
                            FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Comment>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Instruction>,
                            And<FSSODet.status, NotEqual<FSSODet.status.Canceled>,
                            And<FSSODet.isPrepaid, Equal<False>,
                            And<FSSODet.isBillable, Equal<True>,
                            And<
                                Where2<
                                    Where<
                                        FSSODet.postID, IsNull>,
                                    Or<
                                        Where<
                                            FSPostInfo.aRPosted, Equal<False>,
                                            And<FSPostInfo.aPPosted, Equal<False>,
                                            And<FSPostInfo.sOPosted, Equal<False>,
                                            And<FSPostInfo.sOInvPosted, Equal<False>,
                                            And<
                                                Where2<
                                                    Where<
                                                        Required<FSPostBatch.postTo>, NotEqual<FSPostBatch.postTo.SO>>,
                                                    Or<
                                                        Where<
                                                            Required<FSPostBatch.postTo>, Equal<FSPostBatch.postTo.SO>,
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>>>>>,
                        Aggregate<
                            Sum<FSSODet.curyBillableTranAmt>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                invoiceTotal = fsSODetRow.CuryBillableTranAmt;

                return null;
            }
            else
            {
                invoiceTotal = null;

                /* Always keep both BQLs with the same Joins and Where conditions */
                var resultSet = PXSelectJoin<FSSODet,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.srvOrdType, Equal<FSSODet.srvOrdType>,
                                    And<FSServiceOrder.refNbr, Equal<FSSODet.refNbr>>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.sOID, Equal<FSSODet.sOID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Service_Order>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSSODet.postID>>,
                            LeftJoin<FSSODetSplit,
                                On<FSSODetSplit.srvOrdType, Equal<FSSODet.srvOrdType>,
                                    And<FSSODetSplit.refNbr, Equal<FSSODet.refNbr>,
                                    And<FSSODetSplit.lineNbr, Equal<FSSODet.lineNbr>,
                                    And<FSSODetSplit.completed, Equal<False>>>>>,
                            LeftJoin<INItemPlan, 
                                On<INItemPlan.planID, Equal <FSSODetSplit.planID>>,
                            LeftJoin<PMTask,
                                On<PMTask.taskID, Equal<FSSODet.projectTaskID>>>>>>>>>,
                        Where<
                            FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Comment>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Instruction>,
                            And<FSSODet.status, NotEqual<FSSODet.status.Canceled>,
                            And<FSSODet.isPrepaid, Equal<False>,
                            And<
                                Where2<
                                    Where<
                                        FSSODet.postID, IsNull>,
                                    Or<
                                        Where<
                                            FSPostInfo.aRPosted, Equal<False>,
                                            And<FSPostInfo.aPPosted, Equal<False>,
                                            And<FSPostInfo.sOPosted, Equal<False>,
                                            And<FSPostInfo.sOInvPosted, Equal<False>,
                                            And<
                                                Where2<
                                                    Where<
                                                        Required<FSPostBatch.postTo>, NotEqual<FSPostBatch.postTo.SO>>,
                                                    Or<
                                                        Where<
                                                            Required<FSPostBatch.postTo>, Equal<FSPostBatch.postTo.SO>,
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>>>>,
                        OrderBy<
                            Asc<FSServiceOrder.orderDate,
                            Asc<FSSODet.sOID,
                            Asc<FSSODet.sODetID>>>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                var docLines = new List<DocLineExt>();

                foreach (PXResult<FSSODet, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, FSSODetSplit, INItemPlan, PMTask> row in resultSet)
                {
                    docLines.Add(new DocLineExt(row));
                }

                return docLines;
            }
        }

        public override void UpdateSourcePostDoc(PXCache<FSPostDet> cacheFSPostDet, FSPostBatch fsPostBatchRow, FSPostDoc fsPostDocRow)
        {
            PXUpdate<
                    Set<FSServiceOrder.finPeriodID, Required<FSServiceOrder.finPeriodID>,
                    Set<FSServiceOrder.postedBy, Required<FSServiceOrder.postedBy>,
                    Set<FSServiceOrder.pendingAPARSOPost, False>>>,
                FSServiceOrder,
                Where<
                    FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>,
                    And<FSServiceOrder.pendingAPARSOPost, Equal<True>>>>
            .Update(cacheFSPostDet.Graph, fsPostBatchRow.FinPeriodID, ID.Billing_By.SERVICE_ORDER, fsPostDocRow.SOID);
        }

        public virtual void UpdateUnitPricesAndCosts(List<ServiceOrderToPost> serviceOrderToPostRows)
        {
            if (serviceOrderToPostRows[0].PostTo != ID.SrvOrdType_PostTo.PROJECTS) return;

            ServiceOrderEntry serviceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();

            foreach (ServiceOrderToPost serviceOrderPosted in serviceOrderToPostRows)
            {
                serviceOrderEntry.ServiceOrderRecords.Current = serviceOrderEntry.ServiceOrderRecords.Search<FSServiceOrder.refNbr>(serviceOrderPosted.RefNbr, serviceOrderPosted.SrvOrdType);

                serviceOrderEntry.UpdateUnitCostsAndUnitPrices(serviceOrderEntry,
                                                               serviceOrderEntry.ServiceOrderDetails.Cache,
                                                               serviceOrderEntry.ServiceOrderDetails.Select().RowCast<FSSODet>(),
                                                               serviceOrderEntry.ServiceOrderDetails.Current.SOID);

                if (serviceOrderEntry.ServiceOrderDetails.Cache.IsDirty)
                {
                    serviceOrderEntry.Save.Press();
                }
            }
        }
    }
}