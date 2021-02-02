using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class SM_INReleaseProcess : PXGraphExtension<INReleaseProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public override void Initialize()
        {
            base.Initialize();
            Base.onBeforeSalesOrderProcessPOReceipt = ProcessPOReceipt;
        }

        public bool updateCosts = false;
        public delegate void PersistDelegate();
        public PXSelect<FSServiceOrder> serviceOrderView;
        public PXSelect<FSSODetSplit> soDetSplitView;

        [PXDBString(IsKey = true, IsUnicode = true)]
        [PXParent(typeof(Select<FSServiceOrder, Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODetSplit.srvOrdType>>, And<FSServiceOrder.refNbr, Equal<Current<FSSODetSplit.refNbr>>>>>))]
        [PXDefault()]
        protected virtual void FSSODetSplit_RefNbr_CacheAttached(PXCache sender)
        {
        }

        [PXDBDate()]
        [PXDefault()]
        protected virtual void FSSODetSplit_OrderDate_CacheAttached(PXCache sender)
        {
        }

        [PXDBLong()]
        [INItemPlanIDSimple()]
        protected virtual void FSSODetSplit_PlanID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void FSSODetSplit_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void FSSODetSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

        public virtual List<PXResult<INItemPlan, INPlanType>> ProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr)
        {
            var serviceOrder = new PXSelect<FSServiceOrder>(graph);

            if (!graph.Views.Caches.Contains(typeof(FSServiceOrder)))
                graph.Views.Caches.Add(typeof(FSServiceOrder));

            var soDetSplit = new PXSelect<FSSODetSplit>(graph);

            if (!graph.Views.Caches.Contains(typeof(FSSODetSplit)))
                graph.Views.Caches.Add(typeof(FSSODetSplit));

            var initemplan = new PXSelect<INItemPlan>(graph);

            List<PXResult<INItemPlan, INPlanType>> returnList = new List<PXResult<INItemPlan, INPlanType>>();

            Base.FieldVerifying.AddHandler<FSSODetSplit.lotSerialNbr>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

            List<FSSODetSplit> splitsToDeletePlanID = new List<FSSODetSplit>();

            List<FSSODetSplit> insertedSchedules = new List<FSSODetSplit>();
            List<INItemPlan> deletedPlans = new List<INItemPlan>();

            foreach (PXResult<INItemPlan, INPlanType> res in list)
            {
                bool includeInReturnList = true;
                INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res);
                INPlanType plantype = res;

                //avoid ReadItem()
                if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
                {
                    initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);
                }

                //Original Schedule Marked for PO / Allocated on Remote Whse
                //FSSODetSplit schedule = PXSelect<FSSODetSplit, Where<FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>, And<FSSODetSplit.completed, Equal<False>>>>.Select(this, plan.DemandPlanID);
                FSSODetSplit schedule = PXSelect<FSSODetSplit, Where<FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>>>.Select(graph, plan.DemandPlanID);

                if (schedule != null && (schedule.Completed == false || soDetSplit.Cache.GetStatus(schedule) == PXEntryStatus.Updated))
                {
                    includeInReturnList = false;
                    schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);

                    schedule.BaseReceivedQty += plan.PlanQty;
                    schedule.ReceivedQty = INUnitAttribute.ConvertFromBase(soDetSplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseReceivedQty, INPrecision.QUANTITY);

                    soDetSplit.Cache.Update(schedule);

                    INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.DemandPlanID);

                    if (origplan != null)
                    {
                        origplan.PlanQty = schedule.BaseQty - schedule.BaseReceivedQty;
                        initemplan.Cache.Update(origplan);
                    }

                    //select Allocated line if any, exclude allocated on Remote Whse
                    PXSelectBase<INItemPlan> cmd = new PXSelectJoin<INItemPlan, InnerJoin<FSSODetSplit, On<FSSODetSplit.planID, Equal<INItemPlan.planID>>>, Where<INItemPlan.demandPlanID, Equal<Required<INItemPlan.demandPlanID>>, And<FSSODetSplit.isAllocated, Equal<True>, And<FSSODetSplit.siteID, Equal<Required<FSSODetSplit.siteID>>>>>>(graph);

                    if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                    {
                        cmd.WhereAnd<Where<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>();
                    }

                    PXResult<INItemPlan> allocres = cmd.Select(plan.DemandPlanID, plan.SiteID, plan.LotSerialNbr);

                    if (allocres != null)
                    {
                        schedule = PXResult.Unwrap<FSSODetSplit>(allocres);
                        soDetSplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
                        schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);
                        schedule.BaseQty += plan.PlanQty;
                        schedule.Qty = INUnitAttribute.ConvertFromBase(soDetSplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);
                        schedule.POReceiptType = POReceiptType;
                        schedule.POReceiptNbr = POReceiptNbr;

                        soDetSplit.Cache.Update(schedule);

                        INItemPlan allocplan = PXCache<INItemPlan>.CreateCopy(res);
                        allocplan.PlanQty += plan.PlanQty;

                        initemplan.Cache.Update(allocplan);

                        plantype = PXCache<INPlanType>.CreateCopy(plantype);
                        plantype.ReplanOnEvent = null;
                        plantype.DeleteOnEvent = true;
                    }
                    else
                    {
                        serviceOrder.Current = (FSServiceOrder)PXParentAttribute.SelectParent(soDetSplit.Cache, schedule, typeof(FSServiceOrder));
                        schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);

                        long? oldPlanID = schedule.PlanID;
                        ClearScheduleReferences(ref schedule);

                        schedule.IsAllocated = (plantype.ReplanOnEvent != INPlanConstants.Plan60);
                        schedule.LotSerialNbr = plan.LotSerialNbr;
                        schedule.POCreate = false;
                        schedule.POSource = null;
                        schedule.POReceiptType = POReceiptType;
                        schedule.POReceiptNbr = POReceiptNbr;
                        schedule.SiteID = plan.SiteID;
                        schedule.VendorID = null;

                        schedule.BaseReceivedQty = 0m;
                        schedule.ReceivedQty = 0m;
                        schedule.BaseQty = plan.PlanQty;
                        schedule.Qty = INUnitAttribute.ConvertFromBase(soDetSplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);

                        //update SupplyPlanID in existing item plans (replenishment)
                        foreach (PXResult<INItemPlan> demand_res in PXSelect<INItemPlan,
                                                                    Where<
                                                                        INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>
                                                                    .Select(graph, oldPlanID))
                        {
                            INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(demand_res);
                            initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);
                            demand_plan.SupplyPlanID = plan.PlanID;
                            initemplan.Cache.Update(demand_plan);
                        }

                        schedule.PlanID = plan.PlanID;

                        schedule = (FSSODetSplit)soDetSplit.Cache.Insert(schedule);
                        insertedSchedules.Add(schedule);
                    }
                }
                else if (plan.DemandPlanID == null)
                {
                    //Original schedule Marked for PO
                    //TODO: AC-142850 verify this is sufficient for Original SO marked for TR.
                    schedule = PXSelect<FSSODetSplit, Where<FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>, And<FSSODetSplit.completed, Equal<False>>>>.Select(graph, plan.PlanID);

                    if (schedule != null)
                    {
                        includeInReturnList = false;
                        soDetSplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
                        schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);

                        schedule.Completed = true;
                        schedule.POCompleted = true;
                        splitsToDeletePlanID.Add(schedule);
                        soDetSplit.Cache.Update(schedule);

                        INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.PlanID);
                        deletedPlans.Add(origplan);

                        initemplan.Cache.Delete(origplan);
                    }
                }

                if (includeInReturnList == true)
                {
                    returnList.Add(res);
                }
                else
                {
                    if (plantype.ReplanOnEvent != null)
                    {
                        plan.PlanType = plantype.ReplanOnEvent;
                        plan.SupplyPlanID = null;
                        plan.DemandPlanID = null;
                        initemplan.Cache.Update(plan);
                    }
                    else if (plantype.DeleteOnEvent == true)
                    {
                        initemplan.Delete(plan);
                    }
                }
            }

            //Create new schedules for partially received schedules marked for PO.
            FSSODetSplit prevSplit = null;

            foreach (FSSODetSplit newsplit in insertedSchedules)
            {
                if (prevSplit != null && prevSplit.SrvOrdType == newsplit.SrvOrdType && prevSplit.RefNbr == newsplit.RefNbr
                    && prevSplit.LineNbr == newsplit.LineNbr && prevSplit.InventoryID == newsplit.InventoryID
                    && prevSplit.SubItemID == newsplit.SubItemID && prevSplit.ParentSplitLineNbr == newsplit.ParentSplitLineNbr
                    && prevSplit.LotSerialNbr != null && newsplit.LotSerialNbr != null)
                    continue;

                FSSODetSplit parentschedule = PXSelect<FSSODetSplit,
                                              Where<
                                                  FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                  And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                  And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>,
                                                  And<FSSODetSplit.splitLineNbr, Equal<Required<FSSODetSplit.parentSplitLineNbr>>>>>>>
                                              .Select(graph, newsplit.SrvOrdType, newsplit.RefNbr, newsplit.LineNbr, newsplit.ParentSplitLineNbr);

                if (parentschedule != null 
                    && parentschedule.Completed == true 
                    && parentschedule.POCompleted == true 
                    && parentschedule.BaseQty > parentschedule.BaseReceivedQty
                    && deletedPlans.Exists(x => x.PlanID == parentschedule.PlanID))
                {
                    serviceOrder.Current = (FSServiceOrder)PXParentAttribute.SelectParent(soDetSplit.Cache, parentschedule, typeof(FSServiceOrder));

                    parentschedule = PXCache<FSSODetSplit>.CreateCopy(parentschedule);
                    INItemPlan demand = PXCache<INItemPlan>.CreateCopy(deletedPlans.First(x => x.PlanID == parentschedule.PlanID));

                    UpdateSchedulesFromCompletedPO(graph, soDetSplit, initemplan, parentschedule, serviceOrder, demand);
                }
                prevSplit = newsplit;
            }

            //Added because of MySql AutoIncrement counters behavior
            foreach (FSSODetSplit split in splitsToDeletePlanID)
            {
                FSSODetSplit schedule = (FSSODetSplit)soDetSplit.Cache.Locate(split);
                if (schedule != null)
                {
                    schedule.PlanID = null;
                    soDetSplit.Cache.Update(schedule);
                }
            }

            Base.SelectTimeStamp();
            return returnList;
        }

        public virtual void UpdateSchedulesFromCompletedPO(PXGraph graph, PXSelect<FSSODetSplit> fsSODetSplit, PXSelect<INItemPlan> initemplan, FSSODetSplit parentschedule, PXSelect<FSServiceOrder> fsServiceOrder, INItemPlan demand)
        {
            graph.FieldDefaulting.AddHandler<FSSODetSplit.locationID>((sender, e) =>
            {
                if (e.Row != null && ((FSSODetSplit)e.Row).RequireLocation != true)
                {
                    e.NewValue = null;
                    e.Cancel = true;
                }
            });

            FSSODetSplit newschedule = PXCache<FSSODetSplit>.CreateCopy(parentschedule);

            ClearScheduleReferences(ref newschedule);

            newschedule.LotSerialNbr = demand.LotSerialNbr;
            newschedule.SiteID = demand.SiteID;

            newschedule.BaseQty = parentschedule.BaseQty - parentschedule.BaseReceivedQty;
            newschedule.Qty = INUnitAttribute.ConvertFromBase(fsSODetSplit.Cache, newschedule.InventoryID, newschedule.UOM, (decimal)newschedule.BaseQty, INPrecision.QUANTITY);
            newschedule.BaseReceivedQty = 0m;
            newschedule.ReceivedQty = 0m;

            //creating new plan
            INItemPlan newPlan = PXCache<INItemPlan>.CreateCopy(demand);
            newPlan.PlanID = null;
            newPlan.SupplyPlanID = null;
            newPlan.DemandPlanID = null;
            newPlan.PlanQty = newschedule.BaseQty;
            newPlan.VendorID = null;
            newPlan.VendorLocationID = null;
            newPlan.FixedSource = INReplenishmentSource.None;
            newPlan.PlanType = (fsServiceOrder.Current != null && fsServiceOrder.Current.Hold == true) ? INPlanConstants.Plan69 : INPlanConstants.Plan60;
            newPlan = (INItemPlan)initemplan.Cache.Insert(newPlan);

            newschedule.PlanID = newPlan.PlanID;
            fsSODetSplit.Cache.Insert(newschedule);
        }

        public virtual void ClearScheduleReferences(ref FSSODetSplit schedule)
        {
            schedule.ParentSplitLineNbr = schedule.SplitLineNbr;
            schedule.SplitLineNbr = null;
            schedule.Completed = false;
            schedule.PlanID = null;
            //clear PO references
            schedule.POCompleted = false;
            schedule.POCancelled = false;

            schedule.POCreate = false;
            schedule.POSource = INReplenishmentSource.None;

            schedule.POType = null;
            schedule.PONbr = null;
            schedule.POLineNbr = null;
            schedule.POReceiptType = null;
            schedule.POReceiptNbr = null;
            //clear SO references
            schedule.SOOrderType = null;
            schedule.SOOrderNbr = null;
            schedule.SOLineNbr = null;
            schedule.SOSplitLineNbr = null;
            schedule.RefNoteID = null;
        }

        public virtual bool IsSMRelated()
        {
            if (Base.inregister.Current == null)
            {
                return false;
            }

            int count = PXSelectJoin<FSPostDet,
                        InnerJoin<SOOrder,
                            On<SOOrder.orderNbr, Equal<FSPostDet.sOOrderNbr>,
                            And<SOOrder.orderType, Equal<FSPostDet.sOOrderType>>>>,
                        Where<
                            SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                        And<
                            SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                       .Select(Base, Base.inregister.Current.SOOrderType, Base.inregister.Current.SOOrderNbr)
                       .Count();

            return count > 0;
        }

        #region Overrides
        [PXOverride]
        public virtual void Persist(PersistDelegate baseMethod)
        {
            if (SharedFunctions.isFSSetupSet(Base) == false)
            {
                baseMethod();
                return;
            }
            
            baseMethod();
            UpdateCosts();
        }

        public delegate void ReleaseDocProcDelegate(JournalEntry je, INRegister doc);

        [PXOverride]
        public virtual void ReleaseDocProc(JournalEntry je, INRegister doc, ReleaseDocProcDelegate del)
        {
            ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.IN, doc.DocType, doc.RefNbr);

            if (del != null)
            {
                del(je, doc);
            }
        }
        #endregion

        public virtual void UpdateCosts()
        {
            if (IsSMRelated() == true)
            {
                Dictionary<int?, decimal?> appointmentIDs = new Dictionary<int?, decimal?>();

                var appointmentSet = PXSelectJoin<FSAppointmentDet,
                                     InnerJoin<FSAppointment,
                                        On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                                     InnerJoin<FSSODet,
                                        On<FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>,
                                     InnerJoin<FSPostDet,
                                        On<
                                           Where<FSPostDet.postID, Equal<FSAppointmentDet.postID>, 
                                              Or<FSPostDet.postID, Equal<FSSODet.postID>>>>,
                                     LeftJoin<INTran,
                                        On<INTran.sOOrderType, Equal<FSPostDet.sOOrderType>,
                                        And<INTran.sOOrderNbr, Equal<FSPostDet.sOOrderNbr>,
                                        And<INTran.sOOrderLineNbr, Equal<FSPostDet.sOLineNbr>>>>>>>>,
                                     Where<
                                        FSPostDet.sOOrderType, Equal<Required<FSPostDet.sOOrderType>>,
                                     And<
                                        FSPostDet.sOOrderNbr, Equal<Required<FSPostDet.sOOrderNbr>>>>>
                                     .Select(Base, Base.inregister.Current.SOOrderType, Base.inregister.Current.SOOrderNbr);

                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    foreach (PXResult<FSAppointmentDet, FSAppointment, FSSODet, FSPostDet, INTran> bqlResult in appointmentSet)
                    {
                        FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)bqlResult;
                        FSAppointment fsAppointmentRow = (FSAppointment)bqlResult;
                        INTran inTranRow = (INTran)bqlResult;

                        if (inTranRow.RefNbr != null)
                        {
                            PXUpdate<
                                Set<FSAppointmentDet.unitCost, Required<FSAppointmentDet.unitCost>,
                                Set<FSAppointmentDet.curyUnitCost, Required<FSAppointmentDet.curyUnitCost>,
                                Set<FSAppointmentDet.extCost, Required<FSAppointmentDet.extCost>,
                                Set<FSAppointmentDet.curyExtCost, Required<FSAppointmentDet.curyExtCost>>>>>,
                            FSAppointmentDet,
                            Where<
                                FSAppointmentDet.appDetID, Equal<Required<FSAppointmentDet.appDetID>>>>
                            .Update(
                                Base,
                                inTranRow.UnitCost,
                                inTranRow.UnitCost,
                                inTranRow.TranCost,
                                inTranRow.TranCost,
                                fsAppointmentDetRow.AppDetID);
                        }

                        decimal? costTotal = 0.0m;

                        if (appointmentIDs.ContainsKey(fsAppointmentDetRow.AppointmentID) == false)
                        {
                            appointmentIDs.Add(fsAppointmentDetRow.AppointmentID, inTranRow.TranCost ?? fsAppointmentDetRow.ExtCost);
                        }
                        else if (appointmentIDs.TryGetValue(fsAppointmentDetRow.AppointmentID, out costTotal))
                        {
                            costTotal += inTranRow.TranCost ?? fsAppointmentDetRow.ExtCost;
                            appointmentIDs.Remove(fsAppointmentDetRow.AppointmentID);
                            appointmentIDs.Add(fsAppointmentDetRow.AppointmentID, costTotal);
                        }
                    }

                    foreach (KeyValuePair<int?, decimal?> pair in appointmentIDs.ToList())
                    {
                        PXUpdate<
                            Set<FSAppointment.costTotal, Required<FSAppointment.costTotal>,
                            Set<FSAppointment.curyCostTotal, Required<FSAppointment.curyCostTotal>>>,
                        FSAppointment,
                        Where<
                            FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                        .Update(
                                new PXGraph(),
                                pair.Value,
                                pair.Value,
                                pair.Key);
                    }

                    ts.Complete();
                }
            }
        }

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}
