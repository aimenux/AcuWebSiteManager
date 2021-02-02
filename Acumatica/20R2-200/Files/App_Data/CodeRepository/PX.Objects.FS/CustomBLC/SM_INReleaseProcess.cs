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

        #region Views
        public PXSelect<FSServiceOrder> serviceOrderView;
        public PXSelect<FSSODetSplit> soDetSplitView;
        public PXSelect<FSAppointmentDet> apptDetView;
        public PXSelect<FSApptLineSplit> apptSplitView;
        #endregion

        #region CacheAttached
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

        // The selector is removed to avoid validation.
        #region Remove LotSerialNbr Selector
        [PXDBString]
        protected void _(Events.CacheAttached<FSSODetSplit.lotSerialNbr> e) { }

        [PXDBString]
        protected void _(Events.CacheAttached<FSAppointmentDet.lotSerialNbr> e) { }

        [PXDBString]
        protected void _(Events.CacheAttached<FSApptLineSplit.lotSerialNbr> e) { }
        #endregion

        // Attribute PXDBDefault is removed to prevent values ​​explicitly assigned by code from being changed in the Persist.
        #region Remove PXDBDefault
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        protected void _(Events.CacheAttached<FSApptLineSplit.apptNbr> e) { }

        [PXDBString(15, IsUnicode = true, InputMask = "")]
        protected void _(Events.CacheAttached<FSApptLineSplit.origSrvOrdNbr> e) { }

        [PXDBInt()]
        protected void _(Events.CacheAttached<FSApptLineSplit.origLineNbr> e) { }

        [PXDBDate()]
        protected void _(Events.CacheAttached<FSApptLineSplit.apptDate> e) { }
        #endregion

        #endregion

        public virtual List<PXResult<INItemPlan, INPlanType>> ProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr)
        {
            return FSPOReceiptProcess.ProcessPOReceipt(graph, list, POReceiptType, POReceiptNbr, stockItemProcessing: true);
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
