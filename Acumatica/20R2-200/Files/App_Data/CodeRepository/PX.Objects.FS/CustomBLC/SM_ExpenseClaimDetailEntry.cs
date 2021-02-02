using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;
using System;

namespace PX.Objects.FS
{
    public class SM_ExpenseClaimDetailEntry : PXGraphExtension<ExpenseClaimDetailEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Private Members
        private ServiceOrderEntry _ServiceOrderGraph;
        protected ServiceOrderEntry GetServiceOrderGraph(bool clearGraph)
        {
            if (_ServiceOrderGraph == null)
            {
                _ServiceOrderGraph = PXGraph.CreateInstance<ServiceOrderEntry>();
            }
            else if (clearGraph == true)
            {
                _ServiceOrderGraph.Clear();
            }

            return _ServiceOrderGraph;
        }

        private AppointmentEntry _AppointmentGraph;
        protected AppointmentEntry GetAppointmentGraph(bool clearGraph)
        {
            if (_AppointmentGraph == null)
            {
                _AppointmentGraph = PXGraph.CreateInstance<AppointmentEntry>();
            }
            else if (clearGraph == true)
            {
                _AppointmentGraph.Clear();
            }

            return _AppointmentGraph;
        }
        #endregion

        #region Views
        public PXSelect<FSServiceOrder> ServiceOrderRecords;
        public PXSelect<FSAppointment> AppointmentRecords;
        #endregion

        #region CacheAttached
        #region FSServiceOrder_SrvOrdType
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), SubstituteKey = typeof(FSSrvOrdType.srvOrdType), DescriptionField = typeof(FSSrvOrdType.srvOrdType))]
        protected virtual void FSServiceOrder_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_SrvOrdType
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Service Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), SubstituteKey = typeof(FSSrvOrdType.srvOrdType), DescriptionField = typeof(FSSrvOrdType.srvOrdType))]
        protected virtual void FSAppointment_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSServiceOrder_CustomerID
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXSelector(typeof(Search<Customer.bAccountID>), SubstituteKey = typeof(Customer.acctCD), DescriptionField = typeof(Customer.acctName))]
        protected virtual void FSServiceOrder_CustomerID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_CustomerID
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXSelector(typeof(Search<Customer.bAccountID>), SubstituteKey = typeof(Customer.acctCD), DescriptionField = typeof(Customer.acctName))]
        protected virtual void FSAppointment_CustomerID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<FSxEPExpenseClaimDetails.fsEntityType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSxEPExpenseClaimDetails row = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.Row);

            if (string.IsNullOrEmpty(row.FSEntityTypeUI) == false)
            {
                e.NewValue = row.FSEntityTypeUI;
            }
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<EPExpenseClaimDetails.billable> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSxEPExpenseClaimDetails extRow = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.Row);

            if (extRow.FSBillable == true && (bool?)e.NewValue == true)
            {
                e.Cache.SetValueExt<FSxEPExpenseClaimDetails.fsBillable>(e.Row, false);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSxEPExpenseClaimDetails.fsBillable> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;

            if (row.Billable == true && (bool?)e.NewValue == true)
            {
                e.Cache.SetValueExt<EPExpenseClaimDetails.billable>(e.Row, false);
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<EPExpenseClaimDetails> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSxEPExpenseClaimDetails extRow = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.Row);

            if (extRow.FSEntityNoteID != null)
            {
                using (new PXConnectionScope())
                {
                    var item = new EntityHelper(Base).GetEntityRow(extRow.FSEntityNoteID, true);

                    if (item != null) 
                    {
                        if (extRow.FSEntityTypeUI == null)
                        {
                            extRow.FSEntityTypeUI = item.GetType().GetLongName();
                            extRow.FSEntityType = extRow.FSEntityTypeUI;
                        }

                        extRow.IsDocBilledOrClosed = IsFSDocumentBilledOrClosed(item, extRow.FSEntityType);
                        extRow.IsDocRelatedToProject = IsFSDocumentRelatedToProjects(item, extRow.FSEntityType);
                    }
                }
            }
        }

        protected virtual void _(Events.RowSelected<EPExpenseClaimDetails> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSxEPExpenseClaimDetails extRow = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.Row);
            bool setRequired = extRow != null && string.IsNullOrEmpty(extRow.FSEntityTypeUI) == false;

            PXDefaultAttribute.SetPersistingCheck<FSxEPExpenseClaimDetails.fsEntityNoteID>
                                (e.Cache, e.Row, setRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSxEPExpenseClaimDetails.fsEntityNoteID>(e.Cache, e.Row, setRequired);
            PXUIFieldAttribute.SetEnabled<FSxEPExpenseClaimDetails.fsBillable>(e.Cache, e.Row, extRow.FSEntityNoteID != null && extRow.IsDocBilledOrClosed == false && extRow.IsDocRelatedToProject == false);

            if (extRow.FSBillable == true && extRow.IsDocBilledOrClosed == true)
            {
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.inventoryID>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.customerID>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.customerLocationID>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.billable>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.qty>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.uOM>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.curyUnitCost>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.curyExtCost>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.curyEmployeePart>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.contractID>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.taskID>(e.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.costCodeID>(e.Cache, e.Row, false);
            }
        }

        protected virtual void _(Events.RowUpdated<EPExpenseClaimDetails> e)
        {
            FSxEPExpenseClaimDetails extRow = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.Row);
            FSxEPExpenseClaimDetails extOldRow = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.OldRow);

            if (extRow.FSEntityTypeUI != extOldRow.FSEntityTypeUI && extRow.FSEntityNoteID == extOldRow.FSEntityNoteID)
            {
                extRow.FSEntityNoteID = null;
                extRow.IsDocBilledOrClosed = false;
                extRow.IsDocRelatedToProject = false;
            }

            if (extRow.FSEntityNoteID != null && extRow.FSEntityNoteID != extOldRow.FSEntityNoteID)
            {
                var item = new EntityHelper(Base).GetEntityRow(extRow.FSEntityNoteID, true);
                extRow.IsDocBilledOrClosed = IsFSDocumentBilledOrClosed(item, extRow.FSEntityType);
                extRow.IsDocRelatedToProject = IsFSDocumentRelatedToProjects(item, extRow.FSEntityType);
            }
        }

        protected virtual void _(Events.RowPersisting<EPExpenseClaimDetails> e)
        {
            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                EPExpenseClaimDetails row = e.Row;
                FSxEPExpenseClaimDetails extRow = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.Row);
                string error = string.Empty;

                if (extRow != null && extRow.FSEntityNoteID != null) 
                {
                    if (extRow.FSEntityType == ID.FSEntityType.ServiceOrder)
                    {
                        FSServiceOrder serviceOrder = (FSServiceOrder)new EntityHelper(Base).GetEntityRow(typeof(FSServiceOrder), extRow.FSEntityNoteID);

                        if (serviceOrder != null) 
                        {
                            if (serviceOrder.BranchID != row.BranchID)
                                error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentBranch;

                            if (serviceOrder.ProjectID != row.ContractID)
                                error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentProject;

                            if (serviceOrder.BillCustomerID != row.CustomerID)
                                error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentCustomer;

                            if (serviceOrder.BillLocationID != row.CustomerLocationID)
                                error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentLocation;
                        }
                    }

                    if (extRow.FSEntityType == ID.FSEntityType.Appointment)
                    {
                        FSAppointment appointment = (FSAppointment)new EntityHelper(Base).GetEntityRow(typeof(FSAppointment), extRow.FSEntityNoteID);

                        if (appointment != null)
                        {
                            FSServiceOrder fsServiceOrderRow = GetServiceOrderRelated(Base, appointment.SrvOrdType, appointment.SORefNbr);

                            if (fsServiceOrderRow != null) 
                            { 
                                if (appointment.BranchID != row.BranchID)
                                    error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentBranch;

                                if (appointment.ProjectID != row.ContractID)
                                    error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentProject;

                                if (fsServiceOrderRow.BillCustomerID != row.CustomerID)
                                    error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentCustomer;

                                if (fsServiceOrderRow.BillLocationID != row.CustomerLocationID)
                                    error = TX.Error.FSRelatedDocumentNotMatchCurrentDocumentLocation;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(error) == false)
                        e.Cache.RaiseExceptionHandling<FSxEPExpenseClaimDetails.fsEntityNoteID>(e.Row,
                                                                   extRow.FSEntityNoteID,
                                                                   new PXSetPropertyException(error, PXErrorLevel.RowError));
                }
            }
        }

        protected virtual void _(Events.RowPersisted<EPExpenseClaimDetails> e)
        {
            PXGraph graph = e.Cache.Graph;
            EPExpenseClaimDetails row = e.Row;

            if (row != null && e.TranStatus == PXTranStatus.Open) 
            { 
                FSxEPExpenseClaimDetails extRow = e.Cache.GetExtension<FSxEPExpenseClaimDetails>(e.Row);

                if (extRow != null)
                {
                    int? oriInventoryID = (int?)e.Cache.GetValueOriginal<EPExpenseClaimDetails.inventoryID>(e.Row);
                    Guid? oriFSEntityNoteID = (Guid?)e.Cache.GetValueOriginal<FSxEPExpenseClaimDetails.fsEntityNoteID>(e.Row);

                    AppointmentEntry graphAppointment = null;
                    ServiceOrderEntry graphServiceOrder = null;
                   
                    //Delete related AppointmentDet or FSSODet 
                    if (e.Operation == PXDBOperation.Delete || oriInventoryID != row.InventoryID || oriFSEntityNoteID != extRow.FSEntityNoteID) 
                    {
                        PXResult<FSSODet, FSAppointmentDet> result = (PXResult<FSSODet, FSAppointmentDet>)
                                                                            SelectFrom<FSSODet>
                                                                            .LeftJoin<FSAppointmentDet>
                                                                                .On<FSAppointmentDet.sODetID.IsEqual<FSSODet.sODetID>>
                                                                            .Where<FSSODet.linkedEntityType.IsEqual<@P.AsString>
                                                                            .And<FSSODet.linkedDocRefNbr.IsEqual<@P.AsString>>>
                                                                            .View
                                                                            .SelectSingleBound(graph, null, FSSODet.linkedEntityType.Values.ExpenseReceipt, row.ClaimDetailCD);

                        if (result != null) 
                        { 
                            //Delete Appointment if needed
                            FSAppointmentDet oldFSAppointmentDet = (FSAppointmentDet)result;
                            if (oldFSAppointmentDet != null && string.IsNullOrEmpty(oldFSAppointmentDet.LineRef) == false) 
                            {
                                graphAppointment = GetAppointmentGraph(true);

                                graphAppointment.AppointmentRecords.Current = graphAppointment.AppointmentRecords
                                                .Search<FSAppointment.refNbr>(oldFSAppointmentDet.RefNbr, oldFSAppointmentDet.SrvOrdType);

                                graphAppointment.AppointmentDetails.Delete(oldFSAppointmentDet);

                                if (graphAppointment.IsDirty)
                                {
                                    graphAppointment.Save.Press();
                                }

                                if (graphAppointment.AppointmentRecords.Current != null && Base.Caches[typeof(FSAppointment)].GetStatus(graphAppointment.AppointmentRecords.Current) == PXEntryStatus.Updated)
                                {
                                    graph.Caches[typeof(FSAppointment)].Update(graphAppointment.AppointmentRecords.Current);
                                }
                            }

                            //Delete Service Order If needed
                            FSSODet oldFSSODet = (FSSODet)result;
                            if (oldFSSODet != null && string.IsNullOrEmpty(oldFSSODet.LineRef) == false)
                            {
                                graphServiceOrder = GetServiceOrderGraph(true);

                                //Load existing ServiceOrder
                                if (graphServiceOrder.ServiceOrderRecords.Current == null
                                    || graphServiceOrder.ServiceOrderRecords.Current.RefNbr != oldFSAppointmentDet.RefNbr
                                    || graphServiceOrder.ServiceOrderRecords.Current.SrvOrdType != oldFSAppointmentDet.SrvOrdType)
                                {
                                    graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                                    .Search<FSServiceOrder.refNbr>(oldFSSODet.RefNbr, oldFSSODet.SrvOrdType);
                                }

                                graphServiceOrder.ServiceOrderDetails.Delete(oldFSSODet);

                                if (graphServiceOrder.IsDirty)
                                {
                                    graphServiceOrder.Save.Press();
                                }

                                if (graphServiceOrder.ServiceOrderRecords.Current != null && Base.Caches[typeof(FSServiceOrder)].GetStatus(graphServiceOrder.ServiceOrderRecords.Current) == PXEntryStatus.Updated)
                                {
                                    graph.Caches[typeof(FSServiceOrder)].Update(graphServiceOrder.ServiceOrderRecords.Current);
                                }
                            }
                        }
                    }

                    if(e.Operation != PXDBOperation.Delete && extRow.FSEntityNoteID != null) 
                    { 
                        if (extRow.FSEntityType == ID.FSEntityType.ServiceOrder)
                        {
                            FSServiceOrder serviceOrder = (FSServiceOrder)new EntityHelper(Base).GetEntityRow(typeof(FSServiceOrder), extRow.FSEntityNoteID);
                            FSServiceOrder result = UpdateServiceOrderDetail(graphServiceOrder, serviceOrder, row, extRow);

                            if (serviceOrder != null && Base.Caches[typeof(FSServiceOrder)].GetStatus(serviceOrder) == PXEntryStatus.Updated)
                            {
                                graph.Caches[typeof(FSServiceOrder)].Update(result);
                                graph.SelectTimeStamp();
                            }
                        }

                        if (extRow.FSEntityType == ID.FSEntityType.Appointment)
                        {
                            FSAppointment appointment = (FSAppointment)new EntityHelper(Base).GetEntityRow(typeof(FSAppointment), extRow.FSEntityNoteID);
                            FSAppointment result = UpdateAppointmentDetail(graphAppointment, appointment, row, extRow);

                            if (appointment != null && Base.Caches[typeof(FSAppointment)].GetStatus(appointment) == PXEntryStatus.Updated) { 
                                graph.Caches[typeof(FSAppointment)].Update(result);
                                graph.SelectTimeStamp();
                            }
                        }
                    }
                }
            }

            if (row == null || e.TranStatus != PXTranStatus.Open) return;
            Note note = PXSelect<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>.SelectSingleBound(e.Cache.Graph, null, e.Cache.GetValue(row, typeof(FSxEPExpenseClaimDetails.fsEntityNoteID).Name));
            if (note?.EntityType != null)
            {
                var item = note.NoteID.With(id => new EntityHelper(Base).GetEntityRow(id.Value, true));
                Type itemType = item.GetType();
                if (itemType != null)
                {
                    if (graph.Views.Caches.Contains(itemType)) return;
                    PXCache itemCache = graph.Caches[itemType];
                    object entity = new EntityHelper(graph).GetEntityRow(itemType, note.NoteID);
                    if (itemCache.GetStatus(entity) == PXEntryStatus.Updated)
                        itemCache.PersistUpdated(entity);
                }
            }
        }

        public virtual bool IsFSDocumentBilledOrClosed(object row, string fsEntityType) 
        {
            if (row == null)
            {
                return false;
            }

            if (fsEntityType == ID.FSEntityType.Appointment)
            {
                FSAppointment appointment = (FSAppointment)row;
                return appointment.IsPosted == true
                            || appointment.Status == FSAppointment.status.CLOSED;
            }

            if (fsEntityType == ID.FSEntityType.ServiceOrder)
            {
                FSServiceOrder serviceOrder = ((FSServiceOrder)row);
                return serviceOrder.Status == ID.Status_ServiceOrder.CLOSED
                        || serviceOrder.AllowInvoice == true
                        || (serviceOrder.PostedBy != null
                            && serviceOrder.PostedBy == ID.Billing_By.SERVICE_ORDER);
            }

            return false;
        }

        public virtual bool IsFSDocumentRelatedToProjects(object row, string fsEntityType)
        {
            if (row == null)
            {
                return false;
            }

            int? projectID = null;

            if (fsEntityType == ID.FSEntityType.Appointment)
            {
                projectID = ((FSAppointment)row).ProjectID;
            }

            if (fsEntityType == ID.FSEntityType.ServiceOrder)
            {
                projectID = ((FSServiceOrder)row).ProjectID;
            }

            return projectID == null ? false : !ProjectDefaultAttribute.IsNonProject(projectID);
        }

        public virtual FSServiceOrder GetServiceOrderRelated(PXGraph graph, string srvOrdType, string refNbr)
        {
            if (String.IsNullOrEmpty(srvOrdType) == true
                || String.IsNullOrEmpty(refNbr) == true)
            {
                return null;
            }

            FSServiceOrder fsServiceOrderRow = null;

            fsServiceOrderRow = PXSelect<FSServiceOrder,
                                Where<
                                    FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                    And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                                .Select(graph, srvOrdType, refNbr);

            return fsServiceOrderRow;
        }

        public virtual FSServiceOrder UpdateServiceOrderDetail(ServiceOrderEntry graph, 
                                                     FSServiceOrder serviceOrder, 
                                                     EPExpenseClaimDetails row, 
                                                     FSxEPExpenseClaimDetails extRow)
        {
            if (serviceOrder != null)
            {
                graph = GetServiceOrderGraph(false);

                if (graph.ServiceOrderRecords.Current == null
                    || graph.ServiceOrderRecords.Current.RefNbr != serviceOrder.RefNbr
                    || graph.ServiceOrderRecords.Current.SrvOrdType != serviceOrder.SrvOrdType)
                {
                    //Load existing ServiceOrder
                    graph.ServiceOrderRecords.Current = graph.ServiceOrderRecords
                                                                .Search<FSServiceOrder.refNbr>(serviceOrder.RefNbr, serviceOrder.SrvOrdType);
                }

                FSSODet sODet = PXSelect<FSSODet,
                                        Where<FSSODet.linkedEntityType, Equal<Required<FSSODet.linkedEntityType>>,
                                        And<FSSODet.linkedDocRefNbr, Equal<Required<FSSODet.linkedDocRefNbr>>>>>
                                .Select(graph, FSSODet.linkedEntityType.Values.ExpenseReceipt, row.ClaimDetailCD);

                //Update ServiceOrder detail
                InsertUpdateDocDetail<FSSODet>(graph.ServiceOrderDetails, sODet, row, extRow);

                if (graph.IsDirty)
                {
                    graph.Save.Press();
                    return graph.ServiceOrderRecords.Current;
                }
            }

            return serviceOrder;
        }

        public virtual void InsertUpdateDocDetail<DAC>(PXSelectBase dacView, object dacRow, EPExpenseClaimDetails epExpenseClaimRow, FSxEPExpenseClaimDetails fsxEPExpenseClaimDetails)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            DAC itemRow = (DAC)dacRow;

            if (itemRow == null)
            {
                //Insert a new SODet line
                itemRow = new DAC();

                //Assign the EPExpenseClaimDetails reference to the new FSSODet line
                itemRow.Status = ID.Status_SODet.COMPLETED;
                itemRow.LineType = ID.LineType_ALL.NONSTOCKITEM;
                itemRow.BillingRule = ID.BillingRule.FLAT_RATE;
                itemRow.InventoryID = epExpenseClaimRow.InventoryID;
                itemRow.ProjectID = epExpenseClaimRow.ContractID;
                itemRow.LinkedEntityType = FSSODet.linkedEntityType.Values.ExpenseReceipt;
                itemRow.LinkedDocRefNbr = epExpenseClaimRow.ClaimDetailCD;

                itemRow = (DAC)dacView.Cache.Insert(itemRow);
            }

            if (itemRow.TranDesc == epExpenseClaimRow.TranDesc
                && itemRow.UOM == epExpenseClaimRow.UOM
                && itemRow.EstimatedDuration == 0
                && itemRow.EstimatedQty == epExpenseClaimRow.Qty
                && itemRow.UnitPrice == 0m
                && itemRow.CuryUnitCost == epExpenseClaimRow.CuryUnitCost
                && itemRow.CuryExtCost == epExpenseClaimRow.CuryTranAmtWithTaxes
                && itemRow.Qty == epExpenseClaimRow.Qty
                && itemRow.IsBillable == fsxEPExpenseClaimDetails.FSBillable
                && itemRow.ProjectTaskID == epExpenseClaimRow.TaskID
                && itemRow.CostCodeID == epExpenseClaimRow.CostCodeID) 
            { 
                return;
            }

            itemRow = (DAC)dacView.Cache.CreateCopy(itemRow);
            itemRow.Status = ID.Status_AppointmentDet.COMPLETED;
            itemRow.TranDesc = epExpenseClaimRow.TranDesc;
            itemRow.UOM = epExpenseClaimRow.UOM;
            itemRow.EstimatedDuration = 0;
            itemRow.EstimatedQty = epExpenseClaimRow.Qty;
            itemRow.UnitPrice = 0m;
            itemRow.CuryUnitCost = epExpenseClaimRow.CuryUnitCost;
            itemRow.CuryExtCost = epExpenseClaimRow.CuryTranAmtWithTaxes;
            itemRow.Qty = epExpenseClaimRow.Qty;
            itemRow.IsBillable = fsxEPExpenseClaimDetails.FSBillable;

            if (fsxEPExpenseClaimDetails.FSBillable == true)
            {
                itemRow.CuryBillableExtPrice = epExpenseClaimRow.CuryTranAmtWithTaxes;
            }

            itemRow.ProjectTaskID = epExpenseClaimRow.TaskID;
            itemRow.CostCodeID = epExpenseClaimRow.CostCodeID;

            dacView.Cache.Update(itemRow);
        }

        public virtual FSAppointment UpdateAppointmentDetail(AppointmentEntry graph,
                                                    FSAppointment appointment, 
                                                    EPExpenseClaimDetails row, 
                                                    FSxEPExpenseClaimDetails extRow)
        {
            if (appointment != null)
            {
                graph = GetAppointmentGraph(false);  

                if (graph.AppointmentRecords.Current == null
                    || graph.AppointmentRecords.Current.RefNbr != appointment.RefNbr
                    || graph.AppointmentRecords.Current.SrvOrdType != appointment.SrvOrdType)
                {
                    //Load existing Appointment
                    graph.AppointmentRecords.Current = graph.AppointmentRecords
                                        .Search<FSAppointment.refNbr>(appointment.RefNbr, appointment.SrvOrdType);
                }

                //Update Appointment detail
                FSAppointmentDet appDet = PXSelect<FSAppointmentDet,
                                                Where<FSAppointmentDet.linkedEntityType, Equal<Required<FSAppointmentDet.linkedEntityType>>,
                                                And<FSAppointmentDet.linkedDocRefNbr, Equal<Required<FSAppointmentDet.linkedDocRefNbr>>>>>
                                         .Select(graph, FSAppointmentDet.linkedEntityType.Values.ExpenseReceipt, row.ClaimDetailCD);

                //Update the SODet line
                InsertUpdateDocDetail<FSAppointmentDet>(graph.AppointmentDetails, appDet, row, extRow);

                if (graph.IsDirty)
                {
                    graph.Save.Press();
                    return graph.AppointmentRecords.Current;
                }
            }

            return appointment;
        }
    }
}