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
                    On<FSSrvOrdType.srvOrdType, Equal<FSxCROpportunity.srvOrdType>>,
                LeftJoin<CRCase,
                    On<FSSrvOrdType.srvOrdType, Equal<FSxCRCase.srvOrdType>>>>,
                Where<
                    CROpportunity.noteID, Equal<Current<CRActivity.refNoteID>>,
                Or<
                    Where<
                        CRCase.noteID, Equal<Current<CRActivity.refNoteID>>>>>> ServiceOrderTypeSelected;

        #region Methods

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

        private void UpdateSubject(PXCache cache, PMTimeActivity pmTimeActivityRow, CRActivity crActivityRow)
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

        private FSServiceOrder GetServiceOrderRecord(PXGraph graph, CRActivity crActivityRow)
        {
            return (FSServiceOrder)
                PXSelectJoin<FSServiceOrder,
                LeftJoin<CRCase,
                    On<
                        CRCase.caseID, Equal<FSServiceOrder.sourceID>,
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

        private void UpdateSODetServiceRow(
                                            ServiceOrderEntry graphServiceOrder,
                                            FSSODetService fsSODetServiceRow,
                                            PMTimeActivity pmTimeActivity,
                                            FSxPMTimeActivity fsxPMTimeActivity)
        {
            //TODO SD-7788
            //**************************************************************
            // Update all FSSODetService fields but key fields *************
            //**************************************************************            
            if (fsSODetServiceRow.LineType != ID.LineType_ServiceTemplate.SERVICE)
            {
                graphServiceOrder.ServiceOrderDetServices.SetValueExt<FSSODetService.lineType>(fsSODetServiceRow, ID.LineType_ServiceTemplate.SERVICE);
            }

            if (fsSODetServiceRow.InventoryID != fsxPMTimeActivity.ServiceID)
            {
                graphServiceOrder.ServiceOrderDetServices.SetValueExt<FSSODetService.inventoryID>(fsSODetServiceRow, fsxPMTimeActivity.ServiceID);
            }
	        
            if (fsSODetServiceRow.ProjectID != pmTimeActivity.ProjectID)
            {
                graphServiceOrder.ServiceOrderDetServices.SetValueExt<FSSODetService.projectID>(fsSODetServiceRow, pmTimeActivity.ProjectID);
            }

            if (fsSODetServiceRow.ProjectTaskID != pmTimeActivity.ProjectTaskID)
            {
                graphServiceOrder.ServiceOrderDetServices.SetValueExt<FSSODetService.projectTaskID>(fsSODetServiceRow, pmTimeActivity.ProjectTaskID);
            }

            if (fsSODetServiceRow.CostCodeID != pmTimeActivity.CostCodeID)
            {
                graphServiceOrder.ServiceOrderDetServices.SetValueExt<FSSODetService.costCodeID>(fsSODetServiceRow, pmTimeActivity.CostCodeID);
            }

            graphServiceOrder.ServiceOrderDetServices.Update(fsSODetServiceRow);
        }

        private void InsertUpdateDeleteSODet(
                                                ServiceOrderEntry graphServiceOrder,
                                                PMTimeActivity pmTimeActivityRow,
                                                FSxPMTimeActivity fsxPMTimeActivityRow,
                                                PXDBOperation operation)
        {
            FSSODetService fsSODetServiceRow;

            graphServiceOrder.ServiceOrderDetServices.Current = graphServiceOrder.ServiceOrderDetServices
                                                            .Search<FSSODet.sourceNoteID>(pmTimeActivityRow.NoteID);
            if (graphServiceOrder.ServiceOrderDetServices.Current != null)
            {
                if (operation == PXDBOperation.Delete || fsxPMTimeActivityRow.ServiceID == null)
                {
                    graphServiceOrder.ServiceOrderDetServices.Delete(graphServiceOrder.ServiceOrderDetServices.Current);
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
                fsSODetServiceRow = new FSSODetService();

                //Assign the PMTimeActivity reference to the new FSSODet line
                fsSODetServiceRow.SourceNoteID = pmTimeActivityRow.NoteID;

                graphServiceOrder.ServiceOrderDetServices.Current = graphServiceOrder.ServiceOrderDetServices.Insert(fsSODetServiceRow);
            }

            //Update the SODet line
            UpdateSODetServiceRow(graphServiceOrder, graphServiceOrder.ServiceOrderDetServices.Current, pmTimeActivityRow, fsxPMTimeActivityRow);
        }

        private void UpdateServiceOrderDetail(PXCache cache, CRActivity crActivityRow, PXDBOperation operation)
        {
            FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(cache.Graph, crActivityRow);

            if (fsServiceOrderRow != null)
            {
                PMTimeActivity pmTimeActivityRow =
                    PXSelect<PMTimeActivity,
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
        #region Events

        #region CRActivity Events
        protected virtual void CRActivity_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Operation != PXDBOperation.Delete 
                    && Base.TimeActivity.Current != null 
                        && PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
            {
                UpdateSubject(Base.TimeActivity.Cache, Base.TimeActivity.Current, (CRActivity)e.Row);
            }
        }

        protected virtual void CRActivity_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.TranStatus == PXTranStatus.Open
                    && PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
            {
                CRActivity crActivityRow = (CRActivity)e.Row;

                UpdateServiceOrderDetail(cache, crActivityRow, e.Operation);
            }
        }
        #endregion

        #region PMTimeActivity Events
        protected virtual void PMTimeActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<FSxPMTimeActivity.serviceID>(cache, e.Row, _disableServiceID);

            if (_callerEntity == typeof(CROpportunity))
            {
                PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.serviceID>(cache, e.Row, false);
            }
        }
        #endregion

        #endregion
    }
}
