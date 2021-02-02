using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    public class SM_CRTaskMaint : PXGraphExtension<CRTaskMaint>
    {
        protected bool _disableServiceID = true;
        protected Type _callerEntity = null;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public override void Initialize()
        {
            base.Initialize();
            GetCallerEntity(Base.Tasks.Current?.RefNoteID);
        }

        public PXSetup<FSSrvOrdType,
               LeftJoin<CROpportunity,
               On<
                   FSSrvOrdType.srvOrdType, Equal<FSxCROpportunity.srvOrdType>>,
               LeftJoin<CRCase,
               On<
                   FSSrvOrdType.srvOrdType, Equal<FSxCRCase.srvOrdType>>>>,
               Where<
                   CROpportunity.noteID, Equal<Current<CRActivity.refNoteID>>,
                   Or<
                       Where<
                           CRCase.noteID, Equal<Current<CRActivity.refNoteID>>>>>> ServiceOrderTypeSelected;

        #region Virtual Methods

        protected virtual void GetCallerEntity(Guid? refNoteID)
        {
            if (refNoteID != null)
            {
                CRCase crCaseRow = PXSelect<CRCase,
                                   Where<
                                       CRCase.noteID, Equal<Required<CRCase.noteID>>>>
                                   .Select(Base, refNoteID);

                if (crCaseRow != null)
                {
                    FSxCRCase fsxCRCaseRow = PXCache<CRCase>.GetExtension<FSxCRCase>(crCaseRow);

                    if (fsxCRCaseRow != null && fsxCRCaseRow.SDEnabled == false)
                    {
                        _disableServiceID = false;
                    }

                    _callerEntity = typeof(CRCase);
                }
                else
                {
                    CROpportunity crOpportuniyRow = PXSelect<CROpportunity,
                                                    Where<
                                                        CROpportunity.noteID, Equal<Required<CROpportunity.noteID>>>>
                                                    .Select(Base, refNoteID);

                    if (crOpportuniyRow != null)
                    {
                        FSxCROpportunity fsxCROpportunityRow = PXCache<CROpportunity>.GetExtension<FSxCROpportunity>(crOpportuniyRow);

                        if (fsxCROpportunityRow != null && fsxCROpportunityRow.SDEnabled == true)
                        {
                            _disableServiceID = false;
                        }

                        _callerEntity = typeof(CROpportunity);
                    }
                }
            }
        }

        public virtual void UpdateSubject(PXCache cache, PMTimeActivity pmTimeActivityRow, CRActivity crActivityRow)
        {
            FSxPMTimeActivity fsxPMTimeActivityRow = cache.GetExtension<FSxPMTimeActivity>(pmTimeActivityRow);

            //Clean the Subject
            if (crActivityRow.Subject != null)
            {
                int positionPipe = crActivityRow.Subject.IndexOf("|");

                if (positionPipe != -1)
                {
                    crActivityRow.Subject = crActivityRow.Subject.Substring(positionPipe + 1).Trim();

                    if (crActivityRow.Subject == string.Empty)
                    {
                        crActivityRow.Subject = null;
                    }
                }
            }

            if (fsxPMTimeActivityRow.ServiceID != null)
            {
                InventoryItem inventoryItemRow = PXSelect<InventoryItem,
                                                 Where<
                                                     InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                                 .Select(Base, fsxPMTimeActivityRow.ServiceID);

                if (inventoryItemRow != null)
                {
                    if (inventoryItemRow.ItemType == INItemTypes.ServiceItem)
                    {
                        if (string.IsNullOrWhiteSpace(crActivityRow.Subject))
                        {
                            crActivityRow.Subject = string.Empty;
                        }

                        crActivityRow.Subject = TX.ModuleName.SERVICE_DESCRIPTOR + inventoryItemRow.Descr + " | " + crActivityRow.Subject;
                    }
                }
            }
        }

        public virtual FSServiceOrder GetServiceOrderRecord(PXGraph graph, CRActivity crActivityRow)
        {
            return (FSServiceOrder)
                PXSelectJoin<FSServiceOrder,
                LeftJoin<CRCase,
                    On<
                        CRCase.caseCD, Equal<FSServiceOrder.sourceRefNbr>,
                        And<FSServiceOrder.sourceType, Equal<FSServiceOrder.sourceType.Case>>>,
                LeftJoin<CROpportunity,
                    On<
                        CROpportunity.opportunityID, Equal<FSServiceOrder.sourceRefNbr>,
                        And<FSServiceOrder.sourceType, Equal<FSServiceOrder.sourceType.Opportunity>>>,
                InnerJoin<CRActivity,
                    On<
                        CRActivity.refNoteID, Equal<CRCase.noteID>,
                        Or<CRActivity.refNoteID, Equal<CROpportunity.noteID>>>>>>,
                Where<
                    CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>
                .Select(graph, crActivityRow.NoteID);
        }

        public virtual void UpdateSODetServiceRow(ServiceOrderEntry graphServiceOrder,
                                                  FSSODet fsSODetRow,
                                                  PMTimeActivity pmTimeActivity,
                                                  FSxPMTimeActivity fsxPMTimeActivity)
        {
            //TODO AC-142850 SD-7788
            //**************************************************************
            // Update all FSSODetService fields but key fields *************
            //**************************************************************            
            if (fsSODetRow.LineType != ID.LineType_ServiceTemplate.SERVICE)
            {
                graphServiceOrder.ServiceOrderDetails.SetValueExt<FSSODet.lineType>(fsSODetRow, ID.LineType_ServiceTemplate.SERVICE);
            }

            if (fsSODetRow.InventoryID != fsxPMTimeActivity.ServiceID)
            {
                graphServiceOrder.ServiceOrderDetails.SetValueExt<FSSODet.inventoryID>(fsSODetRow, fsxPMTimeActivity.ServiceID);
            }
	        
            if (fsSODetRow.ProjectID != pmTimeActivity.ProjectID)
            {
                graphServiceOrder.ServiceOrderDetails.SetValueExt<FSSODet.projectID>(fsSODetRow, pmTimeActivity.ProjectID);
            }

            if (fsSODetRow.ProjectTaskID != pmTimeActivity.ProjectTaskID)
            {
                graphServiceOrder.ServiceOrderDetails.SetValueExt<FSSODet.projectTaskID>(fsSODetRow, pmTimeActivity.ProjectTaskID);
            }

            if (fsSODetRow.CostCodeID != pmTimeActivity.CostCodeID)
            {
                graphServiceOrder.ServiceOrderDetails.SetValueExt<FSSODet.costCodeID>(fsSODetRow, pmTimeActivity.CostCodeID);
            }

            graphServiceOrder.ServiceOrderDetails.Update(fsSODetRow);
        }

        public virtual void InsertUpdateDeleteSODet(ServiceOrderEntry graphServiceOrder,
                                                    PMTimeActivity pmTimeActivityRow,
                                                    FSxPMTimeActivity fsxPMTimeActivityRow,
                                                    PXDBOperation operation)
        {
            FSSODet fsSODetServiceRow;

            graphServiceOrder.ServiceOrderDetails.Current = graphServiceOrder.ServiceOrderDetails.Search<FSSODet.sourceNoteID>(pmTimeActivityRow.NoteID);

            if (graphServiceOrder.ServiceOrderDetails.Current != null)
            {
                if (operation == PXDBOperation.Delete || fsxPMTimeActivityRow.ServiceID == null)
                {
                    graphServiceOrder.ServiceOrderDetails.Delete(graphServiceOrder.ServiceOrderDetails.Current);
                    return;
                }
            }
            else
            {
                //This line does not require appointment
                if (fsxPMTimeActivityRow.ServiceID == null)
                {
                    return;
                }

                //Insert a new SODet line
                fsSODetServiceRow = new FSSODet();

                //Assign the PMTimeActivity reference to the new FSSODet line
                fsSODetServiceRow.SourceNoteID = pmTimeActivityRow.NoteID;

                graphServiceOrder.ServiceOrderDetails.Current = graphServiceOrder.ServiceOrderDetails.Insert(fsSODetServiceRow);
            }

            //Update the SODet line
            UpdateSODetServiceRow(graphServiceOrder, graphServiceOrder.ServiceOrderDetails.Current, pmTimeActivityRow, fsxPMTimeActivityRow);
        }

        public virtual void UpdateServiceOrderDetail(PXCache cache, CRActivity crActivityRow, PXDBOperation operation)
        {
            FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(cache.Graph, crActivityRow);

            if (fsServiceOrderRow != null)
            {
                PMTimeActivity pmTimeActivityRow = PXSelect<PMTimeActivity,
                                                   Where<
                                                       PMTimeActivity.refNoteID, Equal<Required<PMTimeActivity.refNoteID>>>>
                                                   .Select(Base, crActivityRow.NoteID);

                if (pmTimeActivityRow == null)
                {
                    return;
                }

                FSxPMTimeActivity fsxPMTimeActivityRow = PXCache<PMTimeActivity>.GetExtension<FSxPMTimeActivity>(pmTimeActivityRow);

                ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

                //Load existing ServiceOrder
                graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

                //Update ServiceOrder detail
                InsertUpdateDeleteSODet(graphServiceOrder, pmTimeActivityRow, fsxPMTimeActivityRow, operation);

                if (graphServiceOrder.IsDirty)
                {
                    graphServiceOrder.Save.Press();
                }
            }
        }

        #endregion

        #region Event Handlers

        #region CRActivity

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowSelected<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowInserting<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowInserted<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowUpdating<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowUpdated<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowDeleting<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowDeleted<CRActivity> e)
        {
        }

        protected virtual void _(Events.RowPersisting<CRActivity> e)
        {
            if (e.Operation != PXDBOperation.Delete
                    && Base.TimeActivity.Current != null
                        && PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
            {
                UpdateSubject(Base.TimeActivity.Cache, Base.TimeActivity.Current, (CRActivity)e.Row);
            }
        }

        protected virtual void _(Events.RowPersisted<CRActivity> e)
        {
            if (e.TranStatus == PXTranStatus.Open
                    && PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
            {
                CRActivity crActivityRow = (CRActivity)e.Row;

                UpdateServiceOrderDetail(e.Cache, crActivityRow, e.Operation);
            }
        }

        #endregion

        #region PMTimeActivity Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowSelected<PMTimeActivity> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (_callerEntity == typeof(CRCase))
            {
                PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.serviceID>(e.Cache, e.Row, _disableServiceID);
            }
            else if (_callerEntity == null || _callerEntity == typeof(CROpportunity))
            {
                PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.serviceID>(e.Cache, e.Row, false);
            }
        }

        protected virtual void _(Events.RowInserting<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowInserted<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowUpdating<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowUpdated<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowDeleting<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowDeleted<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowPersisting<PMTimeActivity> e)
        {
        }

        protected virtual void _(Events.RowPersisted<PMTimeActivity> e)
        {
        }

        #endregion

        #endregion
    }
}
