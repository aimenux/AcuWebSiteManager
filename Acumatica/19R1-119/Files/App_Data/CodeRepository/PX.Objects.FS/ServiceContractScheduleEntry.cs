using PX.Common;
using PX.Data;
using PX.Objects.FS.Scheduler;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;

namespace PX.Objects.FS
{
    public class ServiceContractScheduleEntry : ServiceContractScheduleEntryBase<ServiceContractScheduleEntry, 
                                                        FSContractSchedule, FSContractSchedule.scheduleID, 
                                                        FSContractSchedule.entityID>, IGraphWithInitialization
    {
        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }

        void IGraphWithInitialization.Initialize()
        {
            if (_licenseLimits != null)
            {
                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSServiceContract>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSSchedule), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                                new PXDataFieldValue<FSSchedule.customerID>(((ServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.CustomerID),
                                new PXDataFieldValue<FSSchedule.entityID>(((ServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.EntityID)
                        };
                    }));

                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSSchedule>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSScheduleDet), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                                new PXDataFieldValue<FSScheduleDet.scheduleID>(((ServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.ScheduleID)
                        };
                    }));
            }
        }
        #region Delegates
        public virtual IEnumerable scheduleProjectionRecords()
        {
            return Delegate_ScheduleProjectionRecords(ContractScheduleRecords.Cache, ContractScheduleRecords.Current, FromToFilter.Current, ID.RecordType_ServiceContract.SERVICE_CONTRACT);
        }
        #endregion

        #region CacheAttached
        #region FSScheduleDetService_ServiceTemplateID
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Service Template ID", Enabled = false)]
        [PXSelector(typeof(Search<FSServiceTemplate.serviceTemplateID,
                            Where<FSServiceTemplate.srvOrdType,
                                Equal<Current<FSSchedule.srvOrdType>>>>),
                SubstituteKey = typeof(FSServiceTemplate.serviceTemplateCD),
                DescriptionField = typeof(FSServiceTemplate.descr))]
        protected virtual void FSScheduleDetService_ServiceTemplateID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_BranchID
        [PXDBInt]
        [PXUIField(DisplayName = "Branch ID", Enabled = false)]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        protected virtual void FSContractSchedule_BranchID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_BranchLocationID
        [PXDBInt]
        [PXUIField(DisplayName = "Branch Location ID", Enabled = false)]
        [FSSelectorBranchLocationByFSSchedule]
        [PXFormula(typeof(Default<FSSchedule.branchID>))]
        protected virtual void FSContractSchedule_BranchLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_CreatedByScreenID
        [PXUIField(Visible = false)]
        [PXDBCreatedByScreenID]
        protected virtual void FSContractSchedule_CreatedByScreenID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSScheduleDetPart_InventoryID
        [StockItem]
        protected virtual void FSScheduleDetPart_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_ScheduleStartTime
        [PXDBDateAndTime(PreserveTime = true, DisplayNameTime = "Schedule Start Time")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void FSScheduleDetPart_ScheduleStartTime_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #endregion

        #region Methods
        private bool IsTheScheduleExpired(FSContractSchedule fsContractScheduleRow)
        {
            if (fsContractScheduleRow == null
                    || fsContractScheduleRow.EndDate == null)
            {
                return false;
            }

            return fsContractScheduleRow.EndDate < Accessinfo.BusinessDate;
        }
        #endregion 

        #region Actions

        #region OpenServiceContractInq
        public PXAction<FSContractSchedule> openServiceContractInq;
        [PXUIField(DisplayName = "Generate from Service Contracts", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual void OpenServiceContractInq()
        {
            ServiceContractInq serviceContractInqGraph = PXGraph.CreateInstance<ServiceContractInq>();

            ServiceContractFilter filter = new ServiceContractFilter();
            filter.ScheduleID = ContractScheduleRecords.Current.ScheduleID;
            filter.ToDate = ContractScheduleRecords.Current.EndDate ?? ContractScheduleRecords.Current.StartDate;
            serviceContractInqGraph.Filter.Insert(filter);

            throw new PXRedirectRequiredException(serviceContractInqGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #endregion

        #region Events

        #region FSContractSchedule Events

        protected virtual void FSContractSchedule_ScheduleStartTime_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = PXTimeZoneInfo.Now;
        }

        protected virtual void FSContractSchedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsContractScheduleRow = (FSContractSchedule)e.Row;
            ContractSchedule_RowSelected_PartialHandler(cache, fsContractScheduleRow);

            bool existAnyGenerationProcess = SharedFunctions.ShowWarningScheduleNotProcessed(cache, fsContractScheduleRow);
            openServiceContractInq.SetEnabled(existAnyGenerationProcess == false && !IsTheScheduleExpired(fsContractScheduleRow));

            bool enableScheduleStartTime = fsContractScheduleRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.APPOINTMENT;

            PXUIFieldAttribute.SetEnabled<FSContractSchedule.scheduleStartTime>(cache, fsContractScheduleRow, enableScheduleStartTime);
            PXDefaultAttribute.SetPersistingCheck<FSContractSchedule.scheduleStartTime>(cache, fsContractScheduleRow, enableScheduleStartTime ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void FSContractSchedule_EntityID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsScheduleRow = (FSContractSchedule)e.Row;

            if (fsScheduleRow.EntityID != null)
            {
                FSServiceContract fsServiceContract =
                PXSelect<FSServiceContract,
                    Where<
                        FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                .Select(this, fsScheduleRow.EntityID);

                if (fsServiceContract != null)
                {
                    fsScheduleRow.CustomerID = fsServiceContract.CustomerID;
                    fsScheduleRow.CustomerLocationID = fsServiceContract.CustomerLocationID;
                    fsScheduleRow.BranchID = fsServiceContract.BranchID;
                    fsScheduleRow.BranchLocationID = fsServiceContract.BranchLocationID;
                    fsScheduleRow.StartDate = fsServiceContract.StartDate;
                    fsScheduleRow.EndDate = fsServiceContract.EndDate;
                }
            }
        }

        protected virtual void FSContractSchedule_BranchID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsScheduleRow = (FSContractSchedule)e.Row;
            fsScheduleRow.BranchLocationID = null;
        }

        protected virtual void FSContractSchedule_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsScheduleRow = (FSContractSchedule)e.Row;
            FSServiceContract fsServiceContractRow = (FSServiceContract)ContractSelected.Current;

            ContractSchedule_RowPersisting_PartialHandler(cache, fsServiceContractRow, fsScheduleRow, e.Operation, TX.ModuleName.EQUIPMENT_MODULE);
        }

        protected virtual void FSContractSchedule_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            FSSchedule_Row_Deleted_PartialHandler(cache, e);
        }

        #endregion

        #endregion
    }
}
