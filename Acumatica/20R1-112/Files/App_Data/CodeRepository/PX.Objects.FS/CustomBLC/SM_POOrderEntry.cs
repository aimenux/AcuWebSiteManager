using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.PO.POOrderEntry;

namespace PX.Objects.FS
{
    public class SM_POOrderEntry : PXGraphExtension<POOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public override void Initialize()
        {
            base.Initialize();
            Base.onCopyPOLineFields = CopyPOLineFields;
        }

        [PXHidden]
        public PXSelect<FSServiceOrder> serviceOrderView;

        [PXHidden]
        public PXSetup<POSetup> POSetupRecord;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSSODetSplit,
               InnerJoin<INItemPlan,
               On<
                   INItemPlan.planID, Equal<FSSODetSplit.planID>,
                   And<INItemPlan.planID, Equal<Optional<POLine.planID>>>>>> 
               FSSODetSplitFixedDemand;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSSODet,
               InnerJoin<FSSODetSplit,
               On<
                   FSSODetSplit.srvOrdType, Equal<FSSODet.srvOrdType>,
                   And<FSSODetSplit.refNbr, Equal<FSSODet.refNbr>,
                   And<FSSODetSplit.lineNbr, Equal<FSSODet.lineNbr>>>>,
                InnerJoin<INItemPlan,
                On<
                    INItemPlan.planID, Equal<FSSODetSplit.planID>,
                    And<INItemPlan.planID, Equal<Optional<POLine.planID>>>>>>> 
                FSSODetFixedDemand;

        #region Protected Methods
        protected virtual void ClearFSSODetReferences(PXGraph graph, POOrder poOrderRow)
        {
            try
            {
                PXUpdateJoin<
                    Set<FSServiceOrder.waitingForParts, Required<FSServiceOrder.waitingForParts>>,
                FSServiceOrder,
                InnerJoin<FSSODet, 
                On<
                    FSSODet.sOID, Equal<FSServiceOrder.sOID>>>,
                Where<
                    FSSODet.poType, Equal<Required<FSSODet.poType>>,
                    And<FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>>>>
                .Update(graph, 1, poOrderRow.OrderType, poOrderRow.OrderNbr);

                PXUpdateJoin<
                    Set<FSAppointment.waitingForParts, Required<FSAppointment.waitingForParts>>,
                FSAppointment,
                InnerJoin<FSAppointmentDet,
                On<
                    FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>,
                InnerJoin<FSSODet,
                On<
                    FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>>,
                Where<
                    FSSODet.poType, Equal<Required<FSSODet.poType>>,
                    And<FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>>>>
                .Update(graph, 1, poOrderRow.OrderType, poOrderRow.OrderNbr);

                PXUpdate<
                    Set<FSSODet.poNbr, Required<FSSODet.poNbr>,
                    Set<FSSODet.poStatus, Required<FSSODet.poStatus>,
                    Set<FSSODet.poCompleted, Required<FSSODet.poCompleted>>>>,
                FSSODet,
                Where<
                    FSSODet.poType, Equal<Required<FSSODet.poType>>,
                    And<FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>>>>
                .Update(graph, null, null, null, poOrderRow.OrderType, poOrderRow.OrderNbr);
            }
            catch (Exception exception)
            {
                throw new PXException(PXMessages.Localize(TX.Error.UPDATING_FSSODET_PO_REFERENCES) + exception.Message);
            }
        }
        #endregion

        #region Event Handlers

        #region POOrder
        protected virtual void _(Events.RowPersisted<POOrder> e)
        {
            POOrder poOrderRow = (POOrder)e.Row;
            PXCache cache = e.Cache;

            if (poOrderRow.OrderType != POOrderType.RegularOrder)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                if (e.Operation == PXDBOperation.Delete)
                {
                    ClearFSSODetReferences(cache.Graph, poOrderRow);
                }
                else if (e.Operation == PXDBOperation.Update)
                {
                    string poOrderOldStatus = (string)cache.GetValueOriginal<POOrder.status>(poOrderRow);

                    bool updateLines = false;
                    List<POLine> poLineUpdatedList = new List<POLine>();

                    foreach (object row in Base.Transactions.Cache.Updated)
                    {
                        if ((bool?)Base.Transactions.Cache.GetValue<POLine.completed>(row) != (bool?)Base.Transactions.Cache.GetValueOriginal<POLine.completed>(row))
                        {
                            updateLines = true;
                        }

                        poLineUpdatedList.Add((POLine)row);
                    }

                    if (poOrderOldStatus != poOrderRow.Status || updateLines == true)
                    {
                        if (poOrderRow.Status == POOrderStatus.Cancelled || poOrderRow.Status == POOrderStatus.Voided)
                        {
                            ClearFSSODetReferences(cache.Graph, poOrderRow);
                        }
                        else
                        {
                            SharedFunctions.UpdateFSSODetReferences(cache.Graph, serviceOrderView.Cache, poOrderRow, poLineUpdatedList);
                        }
                    }
                }
            }
        }
        #endregion

        #region INItemPlan
        protected virtual void _(Events.RowPersisted<INItemPlan> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                INItemPlan inItemPlanRow = (INItemPlan)e.Row;

                if (e.Operation == PXDBOperation.Update)
                {
                    if (inItemPlanRow.SupplyPlanID != null && inItemPlanRow.PlanType == INPlanConstants.PlanF6)
                    {
                        FSSODet fsSODetRow = FSSODetFixedDemand.Select(inItemPlanRow.PlanID);
                        if (fsSODetRow != null)
                        {
                            POLine poLineRow = Base.Transactions.Select().Where(x => ((POLine)x).PlanID == inItemPlanRow.SupplyPlanID).First();

                            fsSODetRow.POType = Base.Document.Current.OrderType;
                            fsSODetRow.PONbr = Base.Document.Current.OrderNbr;
                            fsSODetRow.POLineNbr = poLineRow.LineNbr;
                            fsSODetRow.POStatus = Base.Document.Current.Status;

                            FSSODetFixedDemand.Update(fsSODetRow);
                            FSSODetFixedDemand.Cache.Persist(PXDBOperation.Update);

                            FSSODetSplit fsSODetSplitRow = FSSODetSplitFixedDemand.Select(inItemPlanRow.PlanID);
                            if (fsSODetSplitRow != null)
                            {
                                fsSODetSplitRow.POType = Base.Document.Current.OrderType;
                                fsSODetSplitRow.PONbr = Base.Document.Current.OrderNbr;
                                fsSODetSplitRow.POLineNbr = poLineRow.LineNbr;

                                FSSODetSplitFixedDemand.Update(fsSODetSplitRow);
                                FSSODetSplitFixedDemand.Cache.Persist(PXDBOperation.Update);
                            }
                        }
                    }
                }
                else if (e.Operation == PXDBOperation.Delete
                    && (inItemPlanRow.PlanType == INPlanConstants.PlanF7 || inItemPlanRow.PlanType == INPlanConstants.PlanF8)
                )
                {
                    inItemPlanRow = PXSelect<INItemPlan, Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(Base, inItemPlanRow.PlanID);

                    if (inItemPlanRow != null && inItemPlanRow.SupplyPlanID != null)
                    {
                        FSSODet fsSODetRow = FSSODetFixedDemand.Select(inItemPlanRow.PlanID);

                        if (fsSODetRow != null)
                        {
                            fsSODetRow.POType = null;
                            fsSODetRow.PONbr = null;
                            fsSODetRow.POLineNbr = null;
                            fsSODetRow.POStatus = null;

                            FSSODetFixedDemand.Update(fsSODetRow);
                            FSSODetFixedDemand.Cache.Persist(PXDBOperation.Update);

                            inItemPlanRow.SupplyPlanID = null;
                            Base.Caches[typeof(INItemPlan)].Update(inItemPlanRow);
                            Base.Caches[typeof(INItemPlan)].Persist(PXDBOperation.Update);

                            FSSODetSplit fsSODetSplitRow = FSSODetSplitFixedDemand.Select(inItemPlanRow.PlanID);
                            if (fsSODetSplitRow != null)
                            {
                                fsSODetSplitRow.POType = null;
                                fsSODetSplitRow.PONbr = null;
                                fsSODetSplitRow.POLineNbr = null;

                                FSSODetSplitFixedDemand.Update(fsSODetSplitRow);
                                FSSODetSplitFixedDemand.Cache.Persist(PXDBOperation.Update);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #endregion

        public delegate void FillPOLineFromDemandDelegate(POLine dest, POFixedDemand demand, string OrderType, SOLineSplit3 solinesplit);

        [PXOverride]
        public virtual void FillPOLineFromDemand(POLine dest, POFixedDemand demand, string OrderType, SOLineSplit3 solinesplit, FillPOLineFromDemandDelegate del)
        {
            if (demand.PlanType == INPlanConstants.PlanF6)
            {
                PXResult<FSSODetSplit, FSSODet> fsSODetSplitDetRow =
                            (PXResult<FSSODetSplit, FSSODet>)
                            PXSelectJoin<FSSODetSplit,
                            InnerJoin<FSSODet,
                            On<
                                FSSODet.lineNbr, Equal<FSSODetSplit.lineNbr>,
                                And<FSSODet.srvOrdType, Equal<FSSODetSplit.srvOrdType>,
                                And<FSSODet.refNbr, Equal<FSSODetSplit.refNbr>>>>>,
                            Where<
                                FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>>>
                            .Select(Base, demand.PlanID);

                if (fsSODetSplitDetRow != null)
                {
                    FSSODetSplit fsSODetSplitRow = (FSSODetSplit)fsSODetSplitDetRow;
                    FSSODet fsSODetRow = (FSSODet)fsSODetSplitDetRow;

                    dest.LineType = (fsSODetSplitRow.LineType == SO.SOLineType.Inventory
                                            ? POLineType.GoodsForServiceOrder
                                            : POLineType.NonStockForServiceOrder);

                    if (fsSODetRow.ManualCost == true)
                    {
                        dest.CuryUnitCost = fsSODetRow.CuryUnitCost;
                    }
                }
            }

            del(dest, demand, OrderType, solinesplit);
        }

        public virtual void CopyPOLineFields(POFixedDemand demand, POLine line)
        {
            PXResult<FSSODetSplit, FSSODet> fsSODetSplitDetRow =
                            (PXResult<FSSODetSplit, FSSODet>)
                            PXSelectJoin<FSSODetSplit,
                            InnerJoin<FSSODet,
                            On<
                                FSSODet.lineNbr, Equal<FSSODetSplit.lineNbr>,
                                And<FSSODet.srvOrdType, Equal<FSSODetSplit.srvOrdType>,
                                And<FSSODet.refNbr, Equal<FSSODetSplit.refNbr>>>>>,
                            Where<
                                FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>>>
                            .Select(Base, demand.PlanID);

            if (fsSODetSplitDetRow != null)
            {
                FSSODet fsSODetRow = (FSSODet)fsSODetSplitDetRow;

                if (POSetupRecord.Current != null)
                {
                    if (POSetupRecord.Current.CopyLineNotesFromServiceOrder == true
                            || POSetupRecord.Current.CopyLineAttachmentsFromServiceOrder == true)
                    {
                        var fsSODetCache = new PXCache<FSSODet>(Base);
                        fsSODetCache.Update(fsSODetRow);

                        PXNoteAttribute.CopyNoteAndFiles(fsSODetCache,
                                                         fsSODetRow,
                                                         Base.Transactions.Cache,
                                                         line,
                                                         POSetupRecord.Current.CopyLineNotesFromServiceOrder == true,
                                                         POSetupRecord.Current.CopyLineAttachmentsFromServiceOrder == true);
                    }
                }

                line.TranDesc = fsSODetRow.TranDesc;
                line.ProjectID = fsSODetRow.ProjectID;
                line.TaskID = fsSODetRow.ProjectTaskID;
                line.CostCodeID = fsSODetRow.CostCodeID;
            }
        }

        public delegate string GetPOFixDemandSorterDelegate(POFixedDemand line);

        [PXOverride]
        public virtual string GetPOFixDemandSorter(POFixedDemand line, GetPOFixDemandSorterDelegate del)
        {
            if (line.PlanType == INPlanConstants.PlanF6)
            {
                FSSODet row = PXSelectJoin<FSSODet,
                                InnerJoin<FSSODetSplit,
                                    On<FSSODet.lineNbr, Equal<FSSODetSplit.lineNbr>,
                                        And<FSSODet.srvOrdType, Equal<FSSODetSplit.srvOrdType>,
                                        And<FSSODet.refNbr, Equal<FSSODetSplit.refNbr>>>>>,
                                Where<FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>>>.Select(Base, line.PlanID);

                return row == null ? String.Empty : string.Format("{0}.{1}.{2:D7}", row.SrvOrdType, row.RefNbr, row.SortOrder.GetValueOrDefault());
            }
            else
            {
                return del(line);
            }
        }
    }
}