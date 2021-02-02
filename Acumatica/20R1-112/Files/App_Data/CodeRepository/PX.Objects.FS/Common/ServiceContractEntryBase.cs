using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.FS.Scheduler;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.FS
{
    public class ServiceContractEntryBase<TGraph, TPrimary, Where> : PXGraph<TGraph, TPrimary>
        where TGraph : PX.Data.PXGraph
        where TPrimary : class, PX.Data.IBqlTable, new()
        where Where : class, IBqlWhere, new()
    {
        public bool isStatusChanged = false;
        public bool insertContractActionForSchedules = false;
        public bool skipStatusSmartPanels = false;

        public ServiceContractEntryBase()
        {
            menuActions.AddMenuAction(activateContract);
            menuActions.AddMenuAction(suspendContract);
            menuActions.AddMenuAction(cancelContract);

            inquiry.AddMenuAction(viewContractScheduleDetails);
            inquiry.AddMenuAction(viewServiceOrderHistory);
            inquiry.AddMenuAction(viewAppointmentHistory);
            inquiry.AddMenuAction(viewCustomerContracts);
            inquiry.AddMenuAction(viewCustomerContractSchedules);
        }

        #region Selects
        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<Contact> Contact;

        // Baccount workaround
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        public CRAttributeList<FSServiceContract> Answers;

        public PXSelect<FSServiceContract, Where> ServiceContractRecords;

        public PXSelect<FSServiceContract, 
               Where<
                   FSServiceContract.serviceContractID,Equal<Current<FSServiceContract.serviceContractID>>>> ServiceContractSelected;

        [PXCopyPasteHiddenView]
        public PXFilter<FSContractPeriodFilter> ContractPeriodFilter;

        public PXSelect<FSContractPeriod, 
               Where<
                   FSContractPeriod.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>,
               And<
                   Where2<
                       Where<
                           Current<FSContractPeriodFilter.contractPeriodID>, IsNull,
                       And<
                           FSContractPeriod.status, NotEqual<FSContractPeriod.status.Active>>>,
                   Or<
                       FSContractPeriod.contractPeriodID, Equal<Current<FSContractPeriodFilter.contractPeriodID>>>>>>> ContractPeriodRecords;

        public PXSelect<FSContractPeriodDet,
               Where<
                   FSContractPeriodDet.contractPeriodID, Equal<Current<FSContractPeriodFilter.contractPeriodID>>,
               And<
                   FSContractPeriodDet.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>>> ContractPeriodDetRecords;

        public PXSelectReadonly3<FSScheduleDet,
               InnerJoin<FSSchedule,
               On<
                   FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>,
               InnerJoin<FSServiceContract,
               On<
                   FSServiceContract.serviceContractID, Equal<FSSchedule.entityID>,
                   And<FSServiceContract.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>,
                   And<FSScheduleDet.lineType, Equal<FSLineType.Service>>>>>>,
               OrderBy<
                   Asc<FSSchedule.refNbr>>> ScheduleServicesByContract;

        #region ScheduleDetServices

        public PXSelectReadonly2<FSScheduleDet,
               InnerJoin<FSSchedule,
               On<
                   FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>,
               InnerJoin<FSServiceContract,
               On<
                   FSServiceContract.serviceContractID, Equal<FSSchedule.entityID>,
                   And<FSServiceContract.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>>>>,
               Where<
                   FSScheduleDet.lineType, Equal<FSLineType.Service>>,
               OrderBy<
                   Asc<FSSchedule.refNbr>>> ScheduleDetServicesByContract;
        #endregion

        #region ScheduleDetParts

        public PXSelectReadonly2<FSScheduleDet,
                                InnerJoin<FSSchedule,
                                    On<
                                        FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>,
                                InnerJoin<FSServiceContract,
                                    On<
                                        FSServiceContract.serviceContractID, Equal<FSSchedule.entityID>,
                                    And<
                                        FSServiceContract.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>,
                                    And<FSScheduleDet.lineType, Equal<FSLineType.Inventory_Item>>>>>>,
                                Where<True, Equal<True>>,
                                OrderBy<
                                        Asc<FSSchedule.refNbr>>> ScheduleDetPartsByContract;

        #endregion

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSSalesPrice,
                        InnerJoin<InventoryItem,
                            On<InventoryItem.inventoryID, Equal<FSSalesPrice.inventoryID>>,
                        InnerJoin<FSServiceContract,
                            On<
                                FSServiceContract.serviceContractID, Equal<FSSalesPrice.serviceContractID>,
                            And<
                                FSServiceContract.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>>>>> SalesPriceLines;

        [PXCopyPasteHiddenView]
        public PXSelect<FSContractAction, 
               Where<
                   FSContractAction.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>> ContractHistoryItems;

        [PXCopyPasteHiddenView]
        public PXFilter<FSActivationContractFilter> ActivationContractFilter;

        [PXCopyPasteHiddenView]
        public PXSelect<FSContractSchedule,
                                Where<
                                    FSContractSchedule.entityID, Equal<Current<FSServiceContract.serviceContractID>>>> ContractSchedules;

        [PXCopyPasteHiddenView]
        public PXFilter<FSTerminateContractFilter> TerminateContractFilter;

        [PXCopyPasteHiddenView]
        public PXFilter<FSSuspendContractFilter> SuspendContractFilter;

        [PXCopyPasteHiddenView]
        public PXSelect<ActiveSchedule, 
               Where<
                   ActiveSchedule.entityID, Equal<Current<FSServiceContract.serviceContractID>>, 
                   And<ActiveSchedule.active, Equal<True>>>> ActiveScheduleRecords;

        [PXCopyPasteHiddenView]
        public PXSelect<FSContractPostDoc,
                    Where<FSContractPostDoc.contractPeriodID, Equal<Current<FSContractPeriodFilter.contractPeriodID>>,
                        And<FSContractPostDoc.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>>> ContractPostDocRecords;

        #endregion

        #region Actions
        #region AddSchedule
        public PXDBAction<FSServiceContract> addSchedule;
        [PXButton]
        [PXUIField(DisplayName = "Add Schedule")]
        public virtual void AddSchedule()
        {
        }
        #endregion

        #region ViewContractScheduleDetails
        public PXAction<FSServiceContract> viewContractScheduleDetails;
        [PXButton]
        [PXUIField(DisplayName = "Contract Schedule Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void ViewContractScheduleDetails()
        {
            FSServiceContract fsServiceContractRow = ServiceContractRecords.Current;

            if (fsServiceContractRow != null)
            {
                Dictionary<string, string> parameters = GetBaseParameters(fsServiceContractRow, false, false);
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.CONTRACT_SCHEDULE_DETAILS_SUMMARY), parameters);
            }
        }
        #endregion

        #region ViewServiceOrderHistory
        public PXAction<FSServiceContract> viewServiceOrderHistory;
        [PXButton]
        [PXUIField(DisplayName = "Service Order History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void ViewServiceOrderHistory()
        {
            FSServiceContract fsServiceContractRow = ServiceContractRecords.Current;

            if (fsServiceContractRow != null)
            {
                Dictionary<string, string> parameters = GetBaseParameters(fsServiceContractRow, true, true);
                parameters["ContractID"] = fsServiceContractRow.RefNbr;
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.SERVICE_ORDER_HISTORY), parameters);
            }
        }
        #endregion

        #region ViewAppointmentHistory
        public PXAction<FSServiceContract> viewAppointmentHistory;
        [PXButton]
        [PXUIField(DisplayName = "Appointment History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void ViewAppointmentHistory()
        {
            FSServiceContract fsServiceContractRow = ServiceContractRecords.Current;

            if (fsServiceContractRow != null)
            {
                var graphAppointmentInq = PXGraph.CreateInstance<AppointmentInq>();

                AppointmentInq.AppointmentInqFilter appointmentInqFilterRow = new AppointmentInq.AppointmentInqFilter();

                appointmentInqFilterRow.BranchID = fsServiceContractRow.BranchID;
                appointmentInqFilterRow.BranchLocationID = fsServiceContractRow.BranchLocationID;
                appointmentInqFilterRow.CustomerID = fsServiceContractRow.CustomerID;
                appointmentInqFilterRow.CustomerLocationID = fsServiceContractRow.CustomerLocationID;
                appointmentInqFilterRow.ServiceContractID = fsServiceContractRow.ServiceContractID;

                graphAppointmentInq.Filter.Current = graphAppointmentInq.Filter.Insert(appointmentInqFilterRow);

                throw new PXRedirectRequiredException(graphAppointmentInq, null) { Mode = PXBaseRedirectException.WindowMode.Same };
            }
        }
        #endregion

        #region ViewCustomerContracts
        public PXAction<FSServiceContract> viewCustomerContracts;
        [PXButton]
        [PXUIField(DisplayName = "Customer Contracts", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void ViewCustomerContracts()
        {
            FSServiceContract fsServiceContractRow = ServiceContractRecords.Current;

            if (fsServiceContractRow != null)
            {
                Dictionary<string, string> parameters = GetBaseParameters(fsServiceContractRow, false, false);
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.CONTRACT_SUMMARY), parameters);
            }
        }
        #endregion

        #region ViewCustomerContractSchedules
        public PXAction<FSServiceContract> viewCustomerContractSchedules;
        [PXButton]
        [PXUIField(DisplayName = "Customer Contract Schedules", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void ViewCustomerContractSchedules()
        {
            FSServiceContract fsServiceContractRow = ServiceContractRecords.Current;

            if (fsServiceContractRow != null)
            {
                Dictionary<string, string> parameters = GetBaseParameters(fsServiceContractRow, false, false);
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.CONTRACT_SCHEDULE_SUMMARY), parameters);
            }
        }
        #endregion

        #region ActivateContract
        public PXAction<FSServiceContract> activateContract;
        [PXButton]
        [PXUIField(DisplayName = "Activate Contract")]
        public virtual IEnumerable ActivateContract(PXAdapter adapter)
        {
            List<FSServiceContract> fsServiceContracts = new List<FSServiceContract>(adapter.Get().Cast<FSServiceContract>());
            string errorMessage = "";

            foreach (FSServiceContract fsServiceContractRow in fsServiceContracts)
            {
                if (fsServiceContractRow.isEditable())
                {
                    Save.Press();
                }

                if (CheckNewContractStatus(this,
                                           fsServiceContractRow,
                                           ID.Status_ServiceContract.ACTIVE,
                                           ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                if (fsServiceContractRow.Status != ID.Status_ServiceContract.DRAFT)
                {
                    if (skipStatusSmartPanels == true || ActivationContractFilter.AskExt() == WebDialogResult.OK)
                    {
                        if (CheckDatesApplyOrScheduleStatusChange(this.ServiceContractRecords.Cache, fsServiceContractRow, this.Accessinfo.BusinessDate, ActivationContractFilter.Current.ActivationDate))
                        {
                            ApplyOrScheduleStatusChange(this, this.ServiceContractRecords.Cache, fsServiceContractRow, this.Accessinfo.BusinessDate, ActivationContractFilter.Current.ActivationDate, ID.Status_ServiceContract.ACTIVE);
                            UpdateSchedulesByActivateContract();
                            ApplyContractPeriodStatusChange(fsServiceContractRow);
                        }
                    }

                }
                else
                {
                    ApplyOrScheduleStatusChange(this, this.ServiceContractRecords.Cache, fsServiceContractRow, this.Accessinfo.BusinessDate, this.Accessinfo.BusinessDate, ID.Status_ServiceContract.ACTIVE);
                    ApplyContractPeriodStatusChange(fsServiceContractRow);

                    if (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
                    { 
                        ActivateCurrentPeriod();
                    }
                }
            }

            return fsServiceContracts;
        }
        #endregion

        #region SuspendContract
        public PXAction<FSServiceContract> suspendContract;
        [PXButton]
        [PXUIField(DisplayName = "Suspend Contract")]
        public virtual IEnumerable SuspendContract(PXAdapter adapter)
        {
            List<FSServiceContract> fsServiceContracts = new List<FSServiceContract>(adapter.Get().Cast<FSServiceContract>());
            string errorMessage = "";

            foreach (FSServiceContract fsServiceContractRow in fsServiceContracts)
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    if (CheckNewContractStatus(this,
                                               fsServiceContractRow,
                                               ID.Status_ServiceContract.SUSPENDED,
                                               ref errorMessage) == false)
                    {
                        throw new PXException(errorMessage);
                    }

                    if (skipStatusSmartPanels == true || SuspendContractFilter.AskExt() == WebDialogResult.OK)
                    {
                        if (CheckDatesApplyOrScheduleStatusChange(this.ServiceContractRecords.Cache, fsServiceContractRow, this.Accessinfo.BusinessDate, SuspendContractFilter.Current.SuspensionDate))
                        {
                            ApplyOrScheduleStatusChange(this, this.ServiceContractRecords.Cache, fsServiceContractRow, this.Accessinfo.BusinessDate, SuspendContractFilter.Current.SuspensionDate, ID.Status_ServiceContract.SUSPENDED);
                            UpdateSchedulesBySuspendContract(SuspendContractFilter.Current.SuspensionDate);
                            SharedFunctions.SaveUpdatedChanges(ServiceContractRecords.Cache, fsServiceContractRow);
                        }
                    }

                    ts.Complete();
                }
            }

            return fsServiceContracts;
        }
        #endregion

        #region CancelContract
        public PXAction<FSServiceContract> cancelContract;
        [PXButton]
        [PXUIField(DisplayName = "Cancel Contract")]
        public virtual IEnumerable CancelContract(PXAdapter adapter)
        {
            List<FSServiceContract> fsServiceContracts = new List<FSServiceContract>(adapter.Get().Cast<FSServiceContract>());
            string errorMessage = "";

            foreach (FSServiceContract fsServiceContractRow in fsServiceContracts)
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    if (CheckNewContractStatus(this,
                                               fsServiceContractRow,
                                               ID.Status_ServiceContract.CANCELED,
                                               ref errorMessage) == false)
                    {
                        throw new PXException(errorMessage);
                    }

                    if (skipStatusSmartPanels == true || TerminateContractFilter.AskExt() == WebDialogResult.OK)
                    {
                        if (CheckDatesApplyOrScheduleStatusChange(this.ServiceContractRecords.Cache, fsServiceContractRow, this.Accessinfo.BusinessDate, TerminateContractFilter.Current.CancelationDate))
                        {
                            ApplyOrScheduleStatusChange(this, this.ServiceContractRecords.Cache, fsServiceContractRow, this.Accessinfo.BusinessDate, TerminateContractFilter.Current.CancelationDate, ID.Status_ServiceContract.CANCELED);
                            UpdateSchedulesByCancelContract(TerminateContractFilter.Current.CancelationDate);

                            if (fsServiceContractRow.NextBillingInvoiceDate > TerminateContractFilter.Current.CancelationDate)
                            {
                                fsServiceContractRow.NextBillingInvoiceDate = TerminateContractFilter.Current.CancelationDate;
                            }

                            this.ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.actions>(this.ContractPeriodFilter.Current, ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD);
                            //  On the FSContractPeriod of latest entry with status Invoiced or Pending for Invoice, if EndPeriodDate is mayor than Cancellation date, EndPeriodDate as Cancellation Date
                            //  On the FSContractPeriod table of latest entry with status Inactive, change the column Status to P = Pending for Invoice
                            SharedFunctions.SaveUpdatedChanges(ServiceContractRecords.Cache, fsServiceContractRow);
                        }
                    }

                    ts.Complete();
                }
            }

            return fsServiceContracts;
        }
        #endregion

        #region ActivatePeriod
        public PXAction<FSServiceContract> activatePeriod;
        [PXButton]
        [PXUIField(DisplayName = "Activate Period", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void ActivatePeriod()
        {
            ActivateCurrentPeriod();
        }
        #endregion

        #endregion

        #region MenuActions
        public PXMenuAction<FSServiceContract> menuActions;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
        [PXUIField(DisplayName = "Actions")]
        public virtual IEnumerable MenuActions(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXMenuAction<FSServiceContract> inquiry;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.InquiriesFolder)]
        [PXUIField(DisplayName = "Inquiries")]
        public virtual IEnumerable Inquiry(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        #region Virtual Functions

        /// <summary>
        /// Enable or Disable the ServiceContract fields.
        /// </summary>
        public virtual void EnableDisable_Document(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            bool enableInsertUpdate = fsServiceContractRow.isEditable();
            bool enableDelete = CanDeleteServiceContract(fsServiceContractRow);

            this.ServiceContractRecords.Cache.AllowInsert = true;
            this.ServiceContractRecords.Cache.AllowUpdate = enableInsertUpdate;
            this.ServiceContractRecords.Cache.AllowDelete = enableDelete;

            this.ServiceContractSelected.Cache.AllowInsert = true;
            this.ServiceContractSelected.Cache.AllowUpdate = enableInsertUpdate;
            this.ServiceContractSelected.Cache.AllowDelete = enableDelete;

            this.ContractPeriodRecords.Cache.AllowInsert = this.ContractPeriodRecords.Cache.AllowUpdate = enableInsertUpdate;
            this.ContractPeriodRecords.Cache.AllowDelete = enableDelete;

            this.ContractPeriodDetRecords.Cache.AllowInsert = this.ContractPeriodDetRecords.Cache.AllowUpdate = enableInsertUpdate;
            this.ContractPeriodDetRecords.Cache.AllowDelete = enableDelete;

            this.SalesPriceLines.Cache.AllowInsert = this.SalesPriceLines.Cache.AllowUpdate = enableInsertUpdate;
            this.SalesPriceLines.Cache.AllowDelete = enableDelete;

            this.ContractHistoryItems.Cache.AllowInsert = this.ContractHistoryItems.Cache.AllowUpdate = enableInsertUpdate;
            this.ContractHistoryItems.Cache.AllowDelete = enableDelete;

            PXUIFieldAttribute.SetEnabled<FSContractPeriodFilter.actions>(this.ContractPeriodFilter.Cache, this.ContractPeriodFilter.Current, enableInsertUpdate);
            PXUIFieldAttribute.SetEnabled<FSContractPeriodFilter.postDocRefNbr>(this.ContractPeriodFilter.Cache, this.ContractPeriodFilter.Current, enableInsertUpdate);
            PXUIFieldAttribute.SetEnabled<FSContractPeriodFilter.standardizedBillingTotal>(this.ContractPeriodFilter.Cache, this.ContractPeriodFilter.Current, enableInsertUpdate);
            PXUIFieldAttribute.SetEnabled<FSContractPeriodFilter.contractPeriodID>(this.ContractPeriodFilter.Cache, this.ContractPeriodFilter.Current, enableInsertUpdate);

            this.addSchedule.SetEnabled(enableInsertUpdate);
            this.activatePeriod.SetEnabled(EnableDisableActivatePeriodButton(fsServiceContractRow, ContractPeriodRecords.Current));

            if (enableInsertUpdate)
            {
                bool enableStartDate = fsServiceContractRow.Status == ID.Status_ServiceContract.DRAFT;
                bool enableExpirationType = fsServiceContractRow.Status == ID.Status_ServiceContract.DRAFT;
                bool enableExpirationDate = fsServiceContractRow.Status == ID.Status_ServiceContract.DRAFT;
                bool enableBillingType = fsServiceContractRow.Status == ID.Status_ServiceContract.DRAFT;
                bool enableBillingPeriod = fsServiceContractRow.Status == ID.Status_ServiceContract.DRAFT;

                PXUIFieldAttribute.SetEnabled<FSServiceContract.billingType>(cache, fsServiceContractRow, enableBillingType);
                PXUIFieldAttribute.SetEnabled<FSServiceContract.startDate>(cache, fsServiceContractRow, enableStartDate);
                PXUIFieldAttribute.SetEnabled<FSServiceContract.expirationType>(cache, fsServiceContractRow, enableExpirationType);
                PXUIFieldAttribute.SetEnabled<FSServiceContract.billingPeriod>(cache, fsServiceContractRow, enableBillingPeriod);
                PXDefaultAttribute.SetPersistingCheck<FSServiceContract.startDate>(cache,
                                                                                   fsServiceContractRow,
                                                                                   enableStartDate ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                PXUIFieldAttribute.SetEnabled<FSServiceContract.endDate>(cache, fsServiceContractRow, enableExpirationDate || IsCopyPasteContext);
                PXDefaultAttribute.SetPersistingCheck<FSServiceContract.endDate>(cache,
                                                                                 fsServiceContractRow,
                                                                                 enableExpirationDate ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

                bool visibleUsageBillingCycle = SetupRecord.Current != null
                                                    && SetupRecord.Current.CustomerMultipleBillingOptions == false
                                                        && fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS;

                PXUIFieldAttribute.SetVisible<FSServiceContract.usageBillingCycleID>(cache, fsServiceContractRow, visibleUsageBillingCycle);
            }
        }

        /// <summary>
        /// Enables/Disables the actions defined for ServiceContract
        /// It's called by RowSelected event of FSServiceContract.
        /// </summary>
        public virtual void EnableDisable_ActionButtons(PXGraph graph, PXCache cache, FSServiceContract fsServiceContractRow)
        {
            if (cache.GetStatus(fsServiceContractRow) == PXEntryStatus.Inserted)
            {
                activateContract.SetEnabled(false);
                suspendContract.SetEnabled(false);
                cancelContract.SetEnabled(false);
                addSchedule.SetEnabled(false);
                viewContractScheduleDetails.SetEnabled(false);
                viewServiceOrderHistory.SetEnabled(false);
                viewAppointmentHistory.SetEnabled(false);
                viewCustomerContracts.SetEnabled(false);
                viewCustomerContractSchedules.SetEnabled(false);
                activateContract.SetEnabled(false);
            }
            else
            {
                string dummyErrorMessage = string.Empty;

                bool canActivate = CheckNewContractStatus(this, fsServiceContractRow, ID.Status_ServiceContract.ACTIVE, ref dummyErrorMessage);
                canActivate = canActivate &&
                                ((fsServiceContractRow.BillingType == ID.Contract_BillingType.AS_PERFORMED_BILLINGS)
                                    || (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS
                                        && this.ContractPeriodDetRecords.Select().Count > 0));

                bool canSuspend = CheckNewContractStatus(this, fsServiceContractRow, ID.Status_ServiceContract.SUSPENDED, ref dummyErrorMessage);
                bool canCancel = CheckNewContractStatus(this, fsServiceContractRow, ID.Status_ServiceContract.CANCELED, ref dummyErrorMessage);
                bool entryStatusIsNotInserted = cache.GetStatus(fsServiceContractRow) != PXEntryStatus.Inserted;

                activateContract.SetEnabled(canActivate);
                suspendContract.SetEnabled(canSuspend);
                cancelContract.SetEnabled(canCancel);
                addSchedule.SetEnabled(entryStatusIsNotInserted);
                viewContractScheduleDetails.SetEnabled(entryStatusIsNotInserted);
                viewServiceOrderHistory.SetEnabled(entryStatusIsNotInserted);
                viewAppointmentHistory.SetEnabled(entryStatusIsNotInserted);
                viewCustomerContracts.SetEnabled(entryStatusIsNotInserted);
                viewCustomerContractSchedules.SetEnabled(entryStatusIsNotInserted);
                activatePeriod.SetEnabled(EnableDisableActivatePeriodButton(fsServiceContractRow, ContractPeriodRecords.Current));
            }
        }

        /// <summary>
        /// Validates startDate and endDate have correct values.
        /// </summary>
        public virtual void ValidateDates(PXCache cache, FSServiceContract fsServiceContractRow, PXResultset<FSContractSchedule> contractRows)
        {
            if (!fsServiceContractRow.StartDate.HasValue)
            {
                return;
            }

            if (contractRows.AsEnumerable().Where(y => ((FSContractSchedule)y).StartDate < fsServiceContractRow.StartDate).Count() > 0)
            {
                cache.RaiseExceptionHandling
                   <FSServiceContract.startDate>(fsServiceContractRow,
                                                 fsServiceContractRow.StartDate,
                                                 new PXSetPropertyException(TX.Error.CONTRACT_START_DATE_GREATER_THAN_SCHEDULE_START_DATE,
                                                                            PXErrorLevel.Error));
            }

            if (contractRows.AsEnumerable().Where(y => ((FSContractSchedule)y).EndDate > fsServiceContractRow.EndDate).Count() > 0)
            {
                cache.RaiseExceptionHandling
                   <FSServiceContract.endDate>(fsServiceContractRow,
                                               fsServiceContractRow.EndDate,
                                               new PXSetPropertyException(TX.Error.CONTRACT_END_DATE_LESSER_THAN_SCHEDULE_END_DATE,
                                                                          PXErrorLevel.Error));
            }

            if (fsServiceContractRow.EndDate.HasValue 
                    && fsServiceContractRow.StartDate.Value.CompareTo(fsServiceContractRow.EndDate.Value) > 0)
            {
                cache.RaiseExceptionHandling<FSServiceContract.startDate>(fsServiceContractRow,
                                                                          fsServiceContractRow.StartDate,
                                                                          new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE, PXErrorLevel.RowError));

                cache.RaiseExceptionHandling<FSServiceContract.endDate>(fsServiceContractRow,
                                                                        fsServiceContractRow.EndDate,
                                                                        new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE, PXErrorLevel.RowError));
            }

            if (fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                && fsServiceContractRow.UpcomingStatus != null
                && fsServiceContractRow.UpcomingStatus != ID.Status_ServiceContract.EXPIRED
                && fsServiceContractRow.EndDate.Value.Date <= fsServiceContractRow.StatusEffectiveUntilDate.Value.Date)
            {
                cache.RaiseExceptionHandling<FSServiceContract.endDate>(fsServiceContractRow,
                                                                        fsServiceContractRow.EndDate,
                                                                        new PXSetPropertyException(TX.Error.EXPIRATION_DATE_LOWER_UPCOMING_STATUS, PXErrorLevel.RowError));
            }


            if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.EXPIRED
                && fsServiceContractRow.EndDate.Value.Date <= this.Accessinfo.BusinessDate.Value.Date)
            {
                cache.RaiseExceptionHandling<FSServiceContract.endDate>(fsServiceContractRow,
                                                                        fsServiceContractRow.EndDate,
                                                                        new PXSetPropertyException(TX.Error.EXPIRATION_DATE_LOWER_BUSINESS_DATE, PXErrorLevel.RowError));
            }
        }

        /// <summary>
        /// Sets the price configured in Price List for a Service when the <c>SourcePrice</c> is modified.
        /// </summary>
        public virtual decimal? GetSalesPrice(PXCache cache, FSSalesPrice fsSalesPriceRow)
        {
            decimal? serviceSalesPrice = null;
            FSServiceContract fsServiceContractRow = ServiceContractRecords.Current;

            SalesPriceSet salesPriceSet = FSPriceManagement.CalculateSalesPriceWithCustomerContract(cache,
                                                                                                    null,
                                                                                                    null,
                                                                                                    null,
                                                                                                    fsServiceContractRow.CustomerID,
                                                                                                    fsServiceContractRow.CustomerLocationID,
                                                                                                    null,
                                                                                                    fsSalesPriceRow.InventoryID,
                                                                                                    null,
                                                                                                    0m,
                                                                                                    fsSalesPriceRow.UOM,
                                                                                                    (DateTime)(fsServiceContractRow.StartDate ?? cache.Graph.Accessinfo.BusinessDate),
                                                                                                    fsSalesPriceRow.Mem_UnitPrice,
                                                                                                    alwaysFromBaseCurrency: true,
                                                                                                    currencyInfo: null,
                                                                                                    catchSalesPriceException: true);

            switch (salesPriceSet.ErrorCode)
            {
                case ID.PriceErrorCode.OK:
                    serviceSalesPrice = salesPriceSet.Price;
                    break;

                case ID.PriceErrorCode.UOM_INCONSISTENCY:
                    InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, fsSalesPriceRow.InventoryID);
                    cache.RaiseExceptionHandling<FSSalesPrice.uOM>(fsSalesPriceRow, 
                                                                   fsSalesPriceRow.UOM,
                                                                   new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(TX.Error.INVENTORY_ITEM_UOM_INCONSISTENCY, inventoryItemRow.InventoryCD), PXErrorLevel.Error));
                    break;

                default:
                    throw new PXException(salesPriceSet.ErrorCode);
            }

            return serviceSalesPrice;
        }

        /// <summary>
        /// Updates all prices of <c>FSSalesPrice</c> lines.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="fsServiceContractRow">FSServiceContract current row.</param>
        public virtual void UpdateSalesPrices(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            foreach (FSSalesPrice fsSalesPriceRow in SalesPriceLines.Select())
            {
                fsSalesPriceRow.Mem_UnitPrice = GetSalesPrice(SalesPriceLines.Cache, fsSalesPriceRow) ?? 0.0m;
                PXUIFieldAttribute.SetEnabled<FSSalesPrice.mem_UnitPrice>(SalesPriceLines.Cache, fsSalesPriceRow, ServiceContractRecords.Current.SourcePrice == ID.SourcePrice.CONTRACT);
            }
        }

        /// <summary>
        /// Verifies the cache of the views for FSSalesPrice.
        /// </summary>
        public virtual void SetUnitPriceForSalesPricesRows(FSServiceContract fsServiceContractRow)
        {
            if (SalesPriceLines.Cache.IsDirty == true)
            {
                foreach (FSSalesPrice fsSalesPriceRow in SalesPriceLines.Select())
                {
                    if (fsServiceContractRow.SourcePrice == ID.SourcePrice.PRICE_LIST)
                    {
                        fsSalesPriceRow.UnitPrice = null;
                    }
                    else
                    {
                        fsSalesPriceRow.UnitPrice = fsSalesPriceRow.Mem_UnitPrice ?? 0.0m;
                    }

                    SalesPriceLines.Cache.SetStatus(fsSalesPriceRow, PXEntryStatus.Updated);
                }
            }
        }

        public virtual void SetVisibleActivatePeriodButton(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow == null)
            {
                return;
            }

            bool showAPB = fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS
                            && ContractPeriodFilter.Current.Actions == ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD;

            activatePeriod.SetVisible(showAPB);
        }

        public virtual void SetVisibleContractBillingSettings(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            if(fsServiceContractRow == null)
            {
                return;
            }

            bool showAPFB = fsServiceContractRow.BillingType == ID.Contract_BillingType.AS_PERFORMED_BILLINGS;

            PXUIFieldAttribute.SetVisible<FSServiceContract.billingPeriod>(cache, fsServiceContractRow, showAPFB == false);
            PXUIFieldAttribute.SetVisible<FSServiceContract.lastBillingInvoiceDate>(cache, fsServiceContractRow, showAPFB == false);
            PXUIFieldAttribute.SetVisible<FSServiceContract.nextBillingInvoiceDate>(cache, fsServiceContractRow, showAPFB == false);
            PXUIFieldAttribute.SetVisible<FSServiceContract.sourcePrice>(cache, fsServiceContractRow, showAPFB == true);
        }

        public virtual void SetContractEndDate(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow == null)
            {
                return;
            }

            if (fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING)
            {
                if (fsServiceContractRow.StartDate.HasValue)
                {
                    cache.SetValueExt<FSServiceContract.endDate>(fsServiceContractRow, fsServiceContractRow.StartDate.Value.AddYears(1));
                }
            }
            else
            {
                cache.SetValueExt<FSServiceContract.endDate>(fsServiceContractRow, null); ;
            }
        }
        public virtual void SetUpcommingStatus(FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow == null)
            {
                return;
            }

            if (fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.UNLIMITED)
            {
                if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.EXPIRED)
                {
                    fsServiceContractRow.UpcomingStatus = null;
                }

            }else
            {
                if(fsServiceContractRow.UpcomingStatus == null)
                {
                    fsServiceContractRow.UpcomingStatus = ID.Status_ServiceContract.EXPIRED;
                }
            }
        }

        public virtual void SetUsageBillingCycle(FSServiceContract fsServiceContractRow)
        {
            if (SetupRecord.Current != null
                    && SetupRecord.Current.CustomerMultipleBillingOptions == false
                          && fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
            {
                if (fsServiceContractRow.BillTo == ID.Contract_BillTo.CUSTOMERACCT)
                {
                    Customer customerRow = PXSelect<Customer,
                                           Where<
                                               Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                           .Select(this, fsServiceContractRow.CustomerID);

                    if (customerRow != null)
                    {
                        FSxCustomer fsxCustomerRow = PXCache<Customer>.GetExtension<FSxCustomer>(customerRow);
                        fsServiceContractRow.UsageBillingCycleID = fsxCustomerRow.BillingCycleID;
                    }
                }
            }
        }

        public virtual void SetBillInfo(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            bool isCustomerAcct = fsServiceContractRow.BillTo == ID.Contract_BillTo.CUSTOMERACCT;

            if (isCustomerAcct == true)
            {
                fsServiceContractRow.BillCustomerID = fsServiceContractRow.CustomerID;
                fsServiceContractRow.BillLocationID = fsServiceContractRow.CustomerLocationID;
            }

            PXUIFieldAttribute.SetEnabled<FSServiceContract.billCustomerID>(cache, fsServiceContractRow, !isCustomerAcct);
            PXUIFieldAttribute.SetEnabled<FSServiceContract.billLocationID>(cache, fsServiceContractRow, !isCustomerAcct);
            PXDefaultAttribute.SetPersistingCheck<FSServiceContract.billCustomerID>(cache, fsServiceContractRow, isCustomerAcct ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
            PXDefaultAttribute.SetPersistingCheck<FSServiceContract.billLocationID>(cache, fsServiceContractRow, isCustomerAcct ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
        }

        public virtual Dictionary<string, string> GetBaseParameters(FSServiceContract fsServiceContractRow, bool loadBranch, bool loadBranchLocation)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (loadBranch == true)
            {
                Branch branchRow = PXSelect<Branch,
                                   Where<
                                       Branch.branchID, Equal<Required<Branch.branchID>>>>
                                   .Select(this, Accessinfo.BranchID);

                if (branchRow != null)
                {
                    parameters["BranchID"] = branchRow.BranchCD;
                }
            }

            if (loadBranchLocation == true)
            {
                FSBranchLocation fsBranchLocationRow = PXSelect<FSBranchLocation,
                                                       Where<
                                                           FSBranchLocation.branchID, Equal<Required<Branch.branchID>>,
                                                       And<
                                                           FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>>
                                                       .Select(this, Accessinfo.BranchID, fsServiceContractRow.BranchLocationID);

                if (fsBranchLocationRow != null)
                {
                    parameters["BranchLocationID"] = fsBranchLocationRow.BranchLocationCD;
                }
            }

            Customer customerRow = PXSelect<Customer,
                                   Where<
                                       Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                   .Select(this, fsServiceContractRow.CustomerID);

            Location locationRow = PXSelect<Location,
                                   Where<
                                       Location.locationID, Equal<Required<Location.locationID>>>>
                                   .Select(this, fsServiceContractRow.CustomerLocationID);

            if (customerRow != null && locationRow != null)
            {
                parameters["CustomerID"] = customerRow.AcctCD;
                parameters["CustomerLocationID"] = locationRow.LocationCD;
            }

            parameters["ServiceContractRefNbr"] = fsServiceContractRow.RefNbr;

            return parameters;
        }

        public virtual void SetDefaultBillingRule(PXCache cache, FSContractPeriodDet fsContractPeriodDetRow)
        {
            string billingRule = ID.ContractPeriod_BillingRule.TIME;

            if (fsContractPeriodDetRow.LineType == ID.LineType_ContractPeriod.NONSTOCKITEM)
            {
                billingRule = ID.BillingRule.FLAT_RATE;
            }
            else if (fsContractPeriodDetRow.LineType == ID.LineType_ContractPeriod.SERVICE)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(this, fsContractPeriodDetRow.InventoryID);

                if (inventoryItemRow != null)
                {
                    FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

                    if (fsxServiceRow != null)
                    {
                        billingRule = fsxServiceRow.BillingRule;

                        if (fsxServiceRow.BillingRule == ID.BillingRule.NONE)
                        {
                            billingRule = ID.ContractPeriod_BillingRule.TIME;
                        }
                    }
                }
            }

            cache.SetValueExt<FSContractPeriodDet.billingRule>(fsContractPeriodDetRow, billingRule);
        }

        public virtual void SetDefaultQtyTime(PXCache cache, FSContractPeriodDet fsContractPeriodDetRow)
        {
            if (fsContractPeriodDetRow.BillingRule == ID.ContractPeriod_BillingRule.TIME)
            {
                cache.SetValueExt<FSContractPeriodDet.time>(fsContractPeriodDetRow, 60);
                cache.SetValueExt<FSContractPeriodDet.qty>(fsContractPeriodDetRow, 0m);
            }
            else if (fsContractPeriodDetRow.BillingRule == ID.ContractPeriod_BillingRule.FLAT_RATE)
            {
                cache.SetValueExt<FSContractPeriodDet.time>(fsContractPeriodDetRow, 0);
                cache.SetValueExt<FSContractPeriodDet.qty>(fsContractPeriodDetRow, 1.0m);
            }
        }

        public static decimal? GetSalesPriceItemInfo(PXCache cacheDetail,
                                                     FSServiceContract fsServiceContractRow,
                                                     FSContractPeriodDet fsContractPeriodDet)
        {
            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cacheDetail.Graph, fsContractPeriodDet?.InventoryID);

            if (inventoryItemRow == null)
            {
                return null;
            }

            SalesPriceSet salesPriceSet = FSPriceManagement.CalculateSalesPriceWithCustomerContract(cacheDetail,
                                                                                                    fsServiceContractRow.ServiceContractID,
                                                                                                    null,
                                                                                                    null,
                                                                                                    fsServiceContractRow.CustomerID,
                                                                                                    fsServiceContractRow.CustomerLocationID,
                                                                                                    null,
                                                                                                    inventoryItemRow.InventoryID,
                                                                                                    null,
                                                                                                    fsContractPeriodDet?.Qty,
                                                                                                    fsContractPeriodDet?.UOM,
                                                                                                    (DateTime)cacheDetail.Graph.Accessinfo.BusinessDate,
                                                                                                    fsContractPeriodDet.RecurringUnitPrice,
                                                                                                    alwaysFromBaseCurrency: true,
                                                                                                    currencyInfo: null,
                                                                                                    catchSalesPriceException: true);

            if (salesPriceSet.ErrorCode == ID.PriceErrorCode.UOM_INCONSISTENCY)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.INVENTORY_ITEM_UOM_INCONSISTENCY, inventoryItemRow.InventoryCD), PXErrorLevel.Error);
            }

            return salesPriceSet.Price;
        }

        public static decimal? GetTotalPrice(PXCache cache, FSContractPeriodDet fsContractPeriodDetRow)
        {
            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, fsContractPeriodDetRow.InventoryID);

            if (inventoryItemRow == null)
            {
                return 0;
            }

            decimal? qty = fsContractPeriodDetRow.Qty ?? 0;

            FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);
            string billingRule = fsContractPeriodDetRow.BillingRule ?? fsxServiceRow.BillingRule;

            if (billingRule == ID.BillingRule.TIME)
            {
                qty = decimal.Divide((decimal)(fsContractPeriodDetRow.Time ?? 0), 60);
            }

            return qty * (fsContractPeriodDetRow.RecurringUnitPrice ?? 0);
        }

        private static void EnableDisableContractPeriodDet(PXCache cache, FSContractPeriodDet fsContractPeriodDetRow)
        {
            PXUIFieldAttribute.SetEnabled<FSContractPeriodDet.billingRule>(cache, fsContractPeriodDetRow, fsContractPeriodDetRow.LineType == ID.LineType_ContractPeriod.SERVICE);
            PXUIFieldAttribute.SetEnabled<FSContractPeriodDet.time>(cache, fsContractPeriodDetRow, fsContractPeriodDetRow.BillingRule == ID.BillingRule.TIME);
            PXUIFieldAttribute.SetEnabled<FSContractPeriodDet.qty>(cache, fsContractPeriodDetRow, fsContractPeriodDetRow.BillingRule == ID.BillingRule.FLAT_RATE);
        }

        private static string GetContractPeriodFilterDefaultAction(PXGraph graph, int? serviceContractID)
        {
            bool anyInvoicedPeriod = PXSelect<FSContractPeriod,
                                     Where<
                                         FSContractPeriod.serviceContractID, Equal<Required<FSContractPeriod.serviceContractID>>,
                                     And<
                                         FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>
                                     .Select(graph, serviceContractID).Count() > 0;

            return anyInvoicedPeriod == true ? ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD : ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD;
        }

        private void AmountFieldSelectingHandler(PXCache cache, PXFieldSelectingEventArgs e, string name, string billingRule, int? time, decimal? qty)
        {
            if (IsCopyPasteContext)
            {
                return;
            }

            if (billingRule == ID.BillingRule.TIME)
            {
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 6, null, name, false, null, ActionsMessages.TimeSpanLongHM, null, null, null, null);

                TimeSpan span = new TimeSpan(0, 0, time ?? 0, 0);
                int hours = span.Days * 24 + span.Hours;
                e.ReturnValue = string.Format("{1,4}{2:00}", span.Days, hours, span.Minutes);
            }
            else if (billingRule == ID.BillingRule.FLAT_RATE)
            {
                e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, 2, name, false, -1, null, null);
                e.ReturnValue = (qty ?? 0).ToString(CultureInfo.InvariantCulture);
            }
        }

        private static void SetRegularPrice(PXCache cache, FSContractPeriodDet fsContractPeriodDetRow, FSServiceContract fsServiceContractRow)
        {
            decimal? regularPrice = GetSalesPriceItemInfo(cache, fsServiceContractRow, fsContractPeriodDetRow);
            cache.SetValueExt<FSContractPeriodDet.regularPrice>(fsContractPeriodDetRow, regularPrice ?? 0);
        }
        
        private void InsertContractAction(object row, PXDBOperation operation, bool? changeRecurrence = false)
        {
            FSServiceContract fsServiceContractRow = null;
            FSSchedule fsScheduleRow = null;
            this.insertContractActionForSchedules = false;

            if (FSServiceContract.TryParse(row, out fsServiceContractRow) == false
                && FSSchedule.TryParse(row, out fsScheduleRow) == false)
            {
                return;
            }

            FSContractAction fsContractActionRow = new FSContractAction();
            fsContractActionRow.Type = fsServiceContractRow != null ? ID.RecordType_ContractAction.CONTRACT : ID.RecordType_ContractAction.SCHEDULE;
            fsContractActionRow.ServiceContractID = fsServiceContractRow != null ? fsServiceContractRow.ServiceContractID : fsScheduleRow.EntityID;
            fsContractActionRow.ActionBusinessDate = Accessinfo.BusinessDate;

            if (operation == PXDBOperation.Insert)
            {
                fsContractActionRow.Action = ID.Action_ContractAction.CREATE;
            }
            else if (operation == PXDBOperation.Update
                        && (fsServiceContractRow == null
                                || (this.isStatusChanged == true)))
            {
                if (fsServiceContractRow != null)
                {
                    fsContractActionRow.Action = GetActionFromServiceContractStatus(fsServiceContractRow);
                    fsContractActionRow.EffectiveDate = fsServiceContractRow.StatusEffectiveFromDate;

                    if (fsContractActionRow.Action == ID.Action_ContractAction.ACTIVATE)
                    {
                        this.insertContractActionForSchedules = true;
                    }

                    this.isStatusChanged = false;
                }
                else
                {
                    fsContractActionRow.Action = GetActionFromSchedule(fsScheduleRow); // Darwing @TODO we have also need to add deleted Schedule Action.
                    fsContractActionRow.ScheduleNextExecutionDate = fsScheduleRow.StartDate;
                    fsContractActionRow.ScheduleRecurrenceDescr = fsScheduleRow.RecurrenceDescription;
                    fsContractActionRow.ScheduleRefNbr = fsScheduleRow.RefNbr;
                    fsContractActionRow.ScheduleChangeRecurrence = changeRecurrence;
                }
            }
            else return;

            ContractHistoryItems.Cache.Insert(fsContractActionRow);
            ContractHistoryItems.Cache.Persist(PXDBOperation.Insert);
        }

        public virtual void InsertContractActionBySchedules(PXDBOperation operation)
        {
            if (this.insertContractActionForSchedules == true)
            {
                foreach (ActiveSchedule scheduleRow in ActiveScheduleRecords.Select())
                {
                    InsertContractAction(scheduleRow, operation, scheduleRow.ChangeRecurrence);
                }
            }
        }

        public virtual ScheduleProjection GetNextExecutionProjection(PXCache cache, FSSchedule fsScheduleRow, DateTime startDate)
        {
            ScheduleProjection scheduleProjectionRow = new ScheduleProjection();
            DateTime endDate = startDate.AddYears(1);

            if (fsScheduleRow.LastGeneratedElementDate.HasValue == true 
                    && endDate <= fsScheduleRow.LastGeneratedElementDate)
            {
                endDate = fsScheduleRow.LastGeneratedElementDate.Value.AddYears(2);
            }

            var period = new Period(startDate, endDate);

            List<Scheduler.Schedule> mapScheduleResults = new List<Scheduler.Schedule>();
            var generator = new TimeSlotGenerator();

            mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(cache, fsScheduleRow, endDate, ID.RecordType_ServiceContract.SERVICE_CONTRACT, period);

            List<TimeSlot> timeSlots = generator.GenerateCalendar(period, mapScheduleResults);

            scheduleProjectionRow.Date = timeSlots[0].DateTimeBegin;
            scheduleProjectionRow.BeginDateOfWeek = SharedFunctions.StartOfWeek((DateTime)scheduleProjectionRow.Date, DayOfWeek.Monday);

            return scheduleProjectionRow;
        }

        public virtual bool CheckNewContractStatus(PXGraph graph, FSServiceContract fsServiceContractRow, string newContractStatus, ref string errorMessage)
        {
            errorMessage = string.Empty;

            // Draft => Active
            if (fsServiceContractRow.Status == ID.Status_ServiceContract.DRAFT
                    && newContractStatus == ID.Status_ServiceContract.ACTIVE)
            {
                return true;
            }

            // Active => Suspended
            if (fsServiceContractRow.Status == ID.Status_ServiceContract.ACTIVE
                    && newContractStatus == ID.Status_ServiceContract.SUSPENDED)
            {
                return true;
            }

            // Active => Canceled
            if (fsServiceContractRow.Status == ID.Status_ServiceContract.ACTIVE
                    && newContractStatus == ID.Status_ServiceContract.CANCELED)
            {
                return true;
            }

            // Active => Expired
            if (fsServiceContractRow.Status == ID.Status_ServiceContract.ACTIVE
                    && newContractStatus == ID.Status_ServiceContract.EXPIRED)
            {
                return true;
            }

            // Suspended => Active
            if (fsServiceContractRow.Status == ID.Status_ServiceContract.SUSPENDED
                    && newContractStatus == ID.Status_ServiceContract.ACTIVE)
            {
                return true;
            }

            // Suspended => Canceled
            if (fsServiceContractRow.Status == ID.Status_ServiceContract.SUSPENDED
                    && newContractStatus == ID.Status_ServiceContract.CANCELED)
            {
                return true;
            }

            errorMessage = TX.Error.INVALID_APPOINTMENT_STATUS_TRANSITION;
            return false;
        }

        public virtual void ApplyOrScheduleStatusChange(PXGraph graph, PXCache cache, FSServiceContract fsServiceContractRow, DateTime? businessDate, DateTime? effectiveDate, string newStatus)
        {
            if (fsServiceContractRow == null || businessDate.HasValue == false || effectiveDate.HasValue == false)
            {
                return;
            }

            if (effectiveDate.Value.Date == businessDate.Value.Date)
            {

                cache.SetValueExt<FSServiceContract.status>(fsServiceContractRow, newStatus);

                if (newStatus != ID.Status_ServiceContract.EXPIRED && newStatus != ID.Status_ServiceContract.CANCELED)
                {
                    fsServiceContractRow.UpcomingStatus = fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING ? ID.Status_ServiceContract.EXPIRED : null;
                    fsServiceContractRow.StatusEffectiveFromDate = effectiveDate;
                    fsServiceContractRow.StatusEffectiveUntilDate = fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING ? fsServiceContractRow.EndDate : null;
                }
                else
                {
                    fsServiceContractRow.UpcomingStatus = null;
                    fsServiceContractRow.StatusEffectiveFromDate = effectiveDate;
                    fsServiceContractRow.StatusEffectiveUntilDate = null;
                }

                if (newStatus == ID.Status_ServiceContract.CANCELED || newStatus == ID.Status_ServiceContract.SUSPENDED)
                {
                    DeleteScheduledAppSO(fsServiceContractRow, effectiveDate);
                }
            }
            else if(effectiveDate.Value.Date > businessDate.Value.Date)
            {
                fsServiceContractRow.UpcomingStatus = newStatus;
                fsServiceContractRow.StatusEffectiveUntilDate = effectiveDate;
            }
        }

        public void ExpireContract()
        {
            ApplyOrScheduleStatusChange(this,
                                        ServiceContractRecords.Cache,
                                        ServiceContractRecords.Current,
                                        Accessinfo.BusinessDate,
                                        Accessinfo.BusinessDate,
                                        ID.Status_ServiceContract.EXPIRED);

            SharedFunctions.SaveUpdatedChanges(this.ServiceContractRecords.Cache, this.ServiceContractRecords.Current);
        }

        /// <summary>
        /// Return true if the Service Contract [fsServiceContractRow] can be deleted based on its status
        /// </summary>
        public static bool CanDeleteServiceContract(FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow == null
                    || fsServiceContractRow.Status != ID.Status_ServiceContract.DRAFT)
            {
                return false;
            }

            return true;
        }

        public virtual string GetActionFromServiceContractStatus(FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow == null)
            {
                return null;
            }

            switch (fsServiceContractRow.Status)
            {
                case ID.Status_ServiceContract.ACTIVE:
                    return ID.Action_ContractAction.ACTIVATE;
                case ID.Status_ServiceContract.SUSPENDED:
                    return ID.Action_ContractAction.SUSPEND;
                case ID.Status_ServiceContract.EXPIRED:
                    return ID.Action_ContractAction.EXPIRE;
                case ID.Status_ServiceContract.CANCELED:
                    return ID.Action_ContractAction.CANCEL;
                default:
                    return null;
            }
        }

        public virtual string GetActionFromSchedule(FSSchedule fsScheduleRow)
        {
            if (fsScheduleRow == null)
            {
                return null;
            }

            if (fsScheduleRow.Active == true)
            {
                return ID.Action_ContractAction.ACTIVATE;
            }
            else if (fsScheduleRow.Active == false)
            {
                return ID.Action_ContractAction.INACTIVATE_SCHEDULE;
            }
            else
            {
                return null;
            }
        }

        public virtual void ApplyContractPeriodStatusChange(FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
            {
                FSContractPeriod fsContractPeriod = PXSelect<FSContractPeriod,
                                                    Where<
                                                        FSContractPeriod.serviceContractID, Equal<Required<FSContractPeriod.serviceContractID>>>,
                                                    OrderBy<
                                                        Desc<FSContractPeriod.createdDateTime>>>
                                                    .SelectWindowed(this, 0, 1, fsServiceContractRow.ServiceContractID);

                if (fsContractPeriod != null)
                { 
                    fsContractPeriod.Status = ID.Status_ContractPeriod.INACTIVE;
                }
            }

            SharedFunctions.SaveUpdatedChanges(ServiceContractRecords.Cache, fsServiceContractRow);
        }

        public virtual void UpdateSchedulesByActivateContract()
        {
            foreach (ActiveSchedule activeScheduleRow in ActiveScheduleRecords.Select())
            {
                activeScheduleRow.EndDate = null;
                activeScheduleRow.StartDate = activeScheduleRow.EffectiveRecurrenceStartDate ?? activeScheduleRow.StartDate;
                activeScheduleRow.NextExecutionDate = activeScheduleRow.NextExecution ?? SharedFunctions.GetNextExecution(ActiveScheduleRecords.Cache, activeScheduleRow);
                activeScheduleRow.EnableExpirationDate = false;
                ActiveScheduleRecords.Cache.Update(activeScheduleRow);
            }
        }

        public virtual void UpdateSchedulesByCancelContract(DateTime? cancelDate)
        {
            foreach (ActiveSchedule activeScheduleRow in ActiveScheduleRecords.Select())
            {
                activeScheduleRow.EndDate = cancelDate;
                activeScheduleRow.EnableExpirationDate = true;

                if (activeScheduleRow.NextExecutionDate >= cancelDate)
                {
                    activeScheduleRow.NextExecutionDate = null;
                }

                ActiveScheduleRecords.Cache.Update(activeScheduleRow);
            }
        }

        public virtual void UpdateSchedulesBySuspendContract(DateTime? suspendDate)
        {
            foreach (ActiveSchedule activeScheduleRow in ActiveScheduleRecords.Select())
            {
                activeScheduleRow.EndDate = suspendDate;
                activeScheduleRow.EnableExpirationDate = true;

                if (activeScheduleRow.NextExecutionDate >= suspendDate)
                {
                    activeScheduleRow.NextExecutionDate = null;
                }

                ActiveScheduleRecords.Cache.Update(activeScheduleRow);
            }
        }

        public virtual void SetEffectiveUntilDate(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.UNLIMITED
                && string.IsNullOrEmpty(fsServiceContractRow.UpcomingStatus))
            {
                fsServiceContractRow.StatusEffectiveUntilDate = null;
            }
            else
            {
                cache.RaiseFieldUpdated<FSServiceContract.endDate>(fsServiceContractRow, null);
            }
        }

        public virtual bool CheckDatesApplyOrScheduleStatusChange(PXCache cache, FSServiceContract fsServiceContractRow, DateTime? businessDate, DateTime? effectiveDate)
        {
            if (effectiveDate.HasValue
                && ((effectiveDate.Value.Date < businessDate.Value.Date)
                    || (fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                        && effectiveDate.Value.Date >= fsServiceContractRow.EndDate)))
            {
                return false;
            }

            return true;
        }

        public virtual void ActivateCurrentPeriod()
        {
            ContractPeriodFilter.Cache.SetValueExt<FSContractPeriodFilter.actions>(ContractPeriodFilter.Current, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD);
            ContractPeriodFilter.Cache.RaiseRowSelected(ContractPeriodFilter.Current);

            if (ContractPeriodRecords.Current != null
                    && ContractPeriodRecords.Current.Status != ID.Status_ContractPeriod.INVOICED
                    && ServiceContractSelected.Current != null
                    && ServiceContractSelected.Current.Status == ID.Status_ServiceContract.ACTIVE)
            {
                //updating Current Period to active
                FSContractPeriod fsCurrentContractPeriodRow = ContractPeriodRecords.Current;
                ContractPeriodRecords.Cache.SetValueExt<FSContractPeriod.status>(ContractPeriodRecords.Current, ID.Status_ContractPeriod.ACTIVE);
                ContractPeriodRecords.Cache.SetStatus(ContractPeriodRecords.Current, PXEntryStatus.Updated);

                //updating Current Contract Billing Dates
                FSServiceContract fsServiceContractRow = ServiceContractSelected.Current;
                fsServiceContractRow.LastBillingInvoiceDate = fsServiceContractRow.NextBillingInvoiceDate ?? fsServiceContractRow.LastBillingInvoiceDate;
                fsServiceContractRow.NextBillingInvoiceDate = fsCurrentContractPeriodRow.EndPeriodDate != null ? fsCurrentContractPeriodRow.EndPeriodDate : null;
                ServiceContractSelected.Cache.SetStatus(fsServiceContractRow, PXEntryStatus.Updated);

                //Inserting new billing period
                FSContractPeriod fsContractPeriodRow = new FSContractPeriod();
                fsContractPeriodRow.StartPeriodDate = fsCurrentContractPeriodRow.EndPeriodDate.Value.AddDays(1);
                fsContractPeriodRow.EndPeriodDate = SharedFunctions.GetContractPeriodEndDate(ServiceContractSelected.Current, fsContractPeriodRow.StartPeriodDate.Value);

                if (fsContractPeriodRow.EndPeriodDate != null)
                {
                    PXResultset<FSContractPeriodDet> fsContractPeriodDetRows = ContractPeriodDetRecords.Select();
                    fsContractPeriodRow = ContractPeriodRecords.Current = ContractPeriodRecords.Insert(fsContractPeriodRow);

                    if (fsContractPeriodDetRows != null)
                    {
                        foreach (FSContractPeriodDet fsCurrentContractPeriodDetRow in fsContractPeriodDetRows)
                        {
                            FSContractPeriodDet fsContractPeriodDetRow = new FSContractPeriodDet();
                            fsContractPeriodDetRow.ServiceContractID = fsCurrentContractPeriodDetRow.ServiceContractID;
                            fsContractPeriodDetRow.InventoryID = fsCurrentContractPeriodDetRow.InventoryID;
                            fsContractPeriodDetRow.LineType = fsCurrentContractPeriodDetRow.LineType;

                            fsContractPeriodDetRow = PXCache<FSContractPeriodDet>.CreateCopy(ContractPeriodDetRecords.Insert(fsContractPeriodDetRow));

                            fsContractPeriodDetRow.BillingRule = fsCurrentContractPeriodDetRow.BillingRule;
                            fsContractPeriodDetRow.SMEquipmentID = fsCurrentContractPeriodDetRow.SMEquipmentID;

                            if (fsCurrentContractPeriodDetRow.BillingRule == ID.BillingRule.TIME)
                            {
                                fsContractPeriodDetRow.Time = fsCurrentContractPeriodDetRow.Time;
                            }
                            else
                            {
                                fsContractPeriodDetRow.Qty = fsCurrentContractPeriodDetRow.Qty;
                            }

                            fsContractPeriodDetRow.RecurringUnitPrice = fsCurrentContractPeriodDetRow.RecurringUnitPrice;
                            fsContractPeriodDetRow.OverageItemPrice = fsCurrentContractPeriodDetRow.OverageItemPrice;
                            fsContractPeriodDetRow.Rollover = fsCurrentContractPeriodDetRow.Rollover;
                            fsContractPeriodDetRow.ProjectID = fsCurrentContractPeriodDetRow.ProjectID;
                            fsContractPeriodDetRow.ProjectTaskID = fsCurrentContractPeriodDetRow.ProjectTaskID;
                            fsContractPeriodDetRow.CostCodeID = fsCurrentContractPeriodDetRow.CostCodeID;

                            ContractPeriodDetRecords.Update(fsContractPeriodDetRow);
                        }
                    }
                }

                Save.Press();
                ContractPeriodFilter.Cache.SetDefaultExt<FSContractPeriodFilter.actions>(ContractPeriodFilter.Current);
                UnholdAPPSORelatedToContractPeriod(ServiceContractSelected.Current, fsCurrentContractPeriodRow);
            }
        }

        public virtual void DeleteScheduledAppSO(FSServiceContract fsServiceContractRow, DateTime? cancelationDate)
        {
            if (fsServiceContractRow != null && cancelationDate != null)
            {
                ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();
                AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();
                PXResultset<FSServiceOrder> fsServiceOrderRows;
                PXResultset<FSAppointment> fsAppointmentRows;

                if (fsServiceContractRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.SERVICE_ORDER)
                {
                    fsServiceOrderRows = PXSelect<FSServiceOrder,
                                         Where<
                                             FSServiceOrder.serviceContractID, Equal<Required<FSServiceOrder.serviceContractID>>,
                                         And<
                                             FSServiceOrder.orderDate, Greater<Required<FSServiceOrder.orderDate>>,
                                         And<
                                             FSServiceOrder.status, Equal<FSServiceOrder.status.Open>,
                                         And<
                                             FSServiceOrder.allowInvoice, Equal<False>>>>>>
                                         .Select(this, fsServiceContractRow.ServiceContractID, cancelationDate);

                    foreach (FSServiceOrder fsServiceOrderRow in fsServiceOrderRows)
                    {
                        graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords.Search<FSServiceOrder.sOID>(fsServiceOrderRow.SOID, fsServiceOrderRow.SrvOrdType);
                        graphServiceOrderEntry.Delete.Press();
                    }
                }
                else
                {
                    fsAppointmentRows = PXSelect<FSAppointment,
                                        Where<
                                            FSAppointment.serviceContractID, Equal<Required<FSAppointment.serviceContractID>>,
                                        And<
                                            FSAppointment.executionDate, Greater<Required<FSAppointment.executionDate>>,
                                        And<
                                            FSAppointment.status, Equal<FSAppointment.status.AutomaticScheduled>>>>>
                                        .Select(this, fsServiceContractRow.ServiceContractID, cancelationDate);

                    foreach (FSAppointment fsAppointmentRow in fsAppointmentRows)
                    {
                        graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>(fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);
                        graphAppointmentEntry.Delete.Press();
                    }
                }

                fsServiceOrderRows = PXSelect<FSServiceOrder,
                                     Where<
                                         FSServiceOrder.billServiceContractID, Equal<Required<FSServiceOrder.billServiceContractID>>,
                                     And<
                                         FSServiceOrder.orderDate, Greater<Required<FSServiceOrder.orderDate>>,
                                     And<
                                         FSServiceOrder.status, NotEqual<FSServiceOrder.status.Closed>,
                                     And<
                                         FSServiceOrder.allowInvoice, Equal<False>>>>>>
                                     .Select(this, fsServiceContractRow.ServiceContractID, cancelationDate);

                foreach (FSServiceOrder fsServiceOrderRow in fsServiceOrderRows)
                {
                    graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords.Search<FSServiceOrder.sOID>
                                                                                            (fsServiceOrderRow.SOID, fsServiceOrderRow.SrvOrdType);

                    if (graphServiceOrderEntry.BillingCycleRelated.Current?.BillingBy == ID.Billing_By.SERVICE_ORDER)
                    {
                        graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.billServiceContractID>(graphServiceOrderEntry.ServiceOrderRecords.Current, null);

                        graphServiceOrderEntry.Save.Press();
                    }
                }

                fsAppointmentRows = PXSelect<FSAppointment,
                                    Where<
                                        FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                    And<
                                        FSAppointment.executionDate, Greater<Required<FSAppointment.executionDate>>,
                                    And<
                                        FSAppointment.status, NotEqual<FSAppointment.status.Closed>>>>>
                                    .Select(this, fsServiceContractRow.ServiceContractID, cancelationDate);

                foreach (FSAppointment fsAppointmentRow in fsAppointmentRows)
                {
                    graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                                                            (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                    if (graphAppointmentEntry.BillingCycleRelated.Current?.BillingBy == ID.Billing_By.APPOINTMENT)
                    {
                        graphAppointmentEntry.AppointmentRecords.Cache.SetValueExt<FSAppointment.billServiceContractID>(graphAppointmentEntry.AppointmentRecords.Current, null);

                        graphAppointmentEntry.Save.Press();
                    }
                }
            }
        }

        public virtual void SetBillingPeriod(FSServiceContract fsServiceContractRow)
        {
            if (ContractPeriodRecords.Current != null)
            {
                ContractPeriodRecords.Current.StartPeriodDate = fsServiceContractRow.StartDate;
                ContractPeriodRecords.Current.EndPeriodDate = SharedFunctions.GetContractPeriodEndDate(fsServiceContractRow, fsServiceContractRow.StartDate);

                if (ContractPeriodRecords.Current.EndPeriodDate != null)
                {
                    if (ContractPeriodRecords.Cache.GetStatus(ContractPeriodRecords.Current) == PXEntryStatus.Notchanged)
                    {
                        ContractPeriodRecords.Cache.SetStatus(ContractPeriodRecords.Current, PXEntryStatus.Updated);
                    }
                }
                else
                {
                    if (ContractPeriodRecords.Current.Status != ID.Status_ContractPeriod.ACTIVE)
                    {
                        ContractPeriodRecords.Delete(ContractPeriodRecords.Current);
                    }
                }
            }
            else
            {
                if (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
                {
                    FSContractPeriod fsContractPeriodRow = new FSContractPeriod();
                    fsContractPeriodRow.StartPeriodDate = fsServiceContractRow.StartDate ?? Accessinfo.BusinessDate;
                    DateTime? endPeriodDate = SharedFunctions.GetContractPeriodEndDate(fsServiceContractRow, fsContractPeriodRow.StartPeriodDate.Value);

                    if (endPeriodDate != null)
                    {
                        fsContractPeriodRow.EndPeriodDate = endPeriodDate.Value;
                        ContractPeriodRecords.Current = ContractPeriodRecords.Insert(fsContractPeriodRow);
                    }

                    ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.actions>(ContractPeriodFilter.Current, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD);
                }
            }
        }

        public virtual bool EnableDisableActivatePeriodButton(FSServiceContract fsServiceContractRow, FSContractPeriod fsContractPeriodRow)
        {
            return fsServiceContractRow.Status == ID.Status_ServiceContract.ACTIVE 
                        && fsContractPeriodRow != null
                            && fsContractPeriodRow.Status == ID.Status_ContractPeriod.INACTIVE 
                                && AllowActivatePeriod(fsContractPeriodRow);
        }

        public virtual bool AllowActivatePeriod(FSContractPeriod fsContractPeriodRow)
        {
            int activePeriods = PXSelect<FSContractPeriod,
                                Where<
                                    FSContractPeriod.serviceContractID, Equal<Required<FSContractPeriod.serviceContractID>>,
                                And<
                                    FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>
                                .Select(this, ServiceContractSelected.Current.ServiceContractID).Count();

            return activePeriods == 0;
        }

        public virtual void InvoiceBillingPeriod(FSSetup fsSetupRow, FSContractPostDoc fsContractPostDocRow)
        {
            ContractPeriodFilter.Cache.SetValueExt<FSContractPeriodFilter.actions>(ContractPeriodFilter.Current, ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD);
            ContractPeriodFilter.Current.ContractPeriodID = fsContractPostDocRow.ContractPeriodID;
            ContractPeriodFilter.Cache.RaiseRowSelected(ContractPeriodFilter.Current);
            bool allowUpdate = ServiceContractRecords.Cache.AllowUpdate;

            if (ContractPeriodRecords.Current != null
                    && ServiceContractSelected.Current != null)
            {

                ServiceContractRecords.Cache.AllowUpdate = true;

                //updating Current Period to invoiced
                FSContractPeriod fsCurrentContractPeriodRow = ContractPeriodRecords.Current;
                ContractPeriodRecords.Cache.SetValueExt<FSContractPeriod.invoiced>(ContractPeriodRecords.Current, true);
                ContractPeriodRecords.Cache.SetValueExt<FSContractPeriod.contractPostDocID>(ContractPeriodRecords.Current, fsContractPostDocRow.ContractPostDocID);

                string originalStatus = this.ContractPeriodRecords.Current.Status;
                ContractPeriodRecords.Cache.SetValueExt<FSContractPeriod.status>(ContractPeriodRecords.Current, ID.Status_ContractPeriod.INVOICED);

                SharedFunctions.SaveUpdatedChanges(ContractPeriodRecords.Cache, ContractPeriodRecords.Current);

                if (originalStatus != ID.Status_ContractPeriod.PENDING)
                {
                FSServiceContract fsServiceContractRow = ServiceContractSelected.Current;
                fsServiceContractRow.LastBillingInvoiceDate = fsServiceContractRow.NextBillingInvoiceDate ?? fsServiceContractRow.LastBillingInvoiceDate;
                fsServiceContractRow.NextBillingInvoiceDate = null;

                SharedFunctions.SaveUpdatedChanges(ServiceContractSelected.Cache, ServiceContractSelected.Current);

                if (fsSetupRow.EnableContractPeriodWhenInvoice == true)
                {
                    ActivateCurrentPeriod();
                }
                }

                ServiceContractRecords.Cache.AllowUpdate = allowUpdate;
            }
        }

        public virtual void UnholdAPPSORelatedToContractPeriod(FSServiceContract fsServiceContractRow, FSContractPeriod fsContractPeriodRow)
        {

            if (fsServiceContractRow == null && fsContractPeriodRow == null)
            {
                return;
            }


            ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();
            AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();
            PXResultset<FSServiceOrder> fsServiceOrderRows;
            PXResultset<FSAppointment> fsAppointmentRows;

            fsServiceOrderRows = PXSelect<FSServiceOrder,
                                    Where<FSServiceOrder.billServiceContractID, Equal<Required<FSServiceOrder.billServiceContractID>>,
                                    And<FSServiceOrder.orderDate, GreaterEqual<Required<FSServiceOrder.orderDate>>,
                                    And<FSServiceOrder.orderDate, LessEqual<Required<FSServiceOrder.orderDate>>,
                                    And<FSServiceOrder.status, NotEqual<FSServiceOrder.status.Closed>,
                                    And<FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                                    And<FSServiceOrder.allowInvoice, Equal<False>>>>>>>>
                                 .Select(this, fsServiceContractRow.ServiceContractID, fsContractPeriodRow.StartPeriodDate, fsContractPeriodRow.EndPeriodDate);

            foreach (FSServiceOrder fsServiceOrderRow in fsServiceOrderRows)
            {
                graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords.Search<FSServiceOrder.sOID>(fsServiceOrderRow.SOID, fsServiceOrderRow.SrvOrdType);

                graphServiceOrderEntry.ServiceOrderRecords.Cache.SetDefaultExt<FSServiceOrder.billContractPeriodID>(graphServiceOrderEntry.ServiceOrderRecords.Current);
                graphServiceOrderEntry.ServiceOrderRecords.Cache.Update(graphServiceOrderEntry.ServiceOrderRecords.Current);
                graphServiceOrderEntry.Save.Press();
            }

            fsAppointmentRows = PXSelect<FSAppointment,
                                    Where<FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                    And<FSAppointment.executionDate, GreaterEqual<Required<FSAppointment.executionDate>>,
                                    And<FSAppointment.executionDate, LessEqual<Required<FSAppointment.executionDate>>,
                                    And<FSAppointment.status, NotEqual<FSAppointment.status.Closed>,
                                    And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>>>>>>>
                                .Select(this, fsServiceContractRow.ServiceContractID, fsContractPeriodRow.StartPeriodDate, fsContractPeriodRow.EndPeriodDate);

            foreach (FSAppointment fsAppointmentRow in fsAppointmentRows)
            {
                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>(fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                graphAppointmentEntry.AppointmentRecords.Cache.SetDefaultExt<FSAppointment.billContractPeriodID>(graphAppointmentEntry.AppointmentRecords.Current);
                graphAppointmentEntry.AppointmentRecords.Cache.Update(graphAppointmentEntry.AppointmentRecords.Current);
                graphAppointmentEntry.Save.Press();
            }
    }

        public virtual void SetBillCustomerAndLocationID(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            BAccount bAccountRow = PXSelect<BAccount,
                                   Where<
                                       BAccount.bAccountID, Equal<Required<FSServiceOrder.customerID>>>>
                                   .Select(cache.Graph, fsServiceContractRow.CustomerID);

            int? billCustomerID = null;
            int? billLocationID = null;
            string billTo = ID.Contract_BillTo.CUSTOMERACCT;

            if (bAccountRow == null || bAccountRow.Type != BAccountType.ProspectType)
            {
                Customer customerRow = SharedFunctions.GetCustomerRow(cache.Graph, fsServiceContractRow.CustomerID);
                FSxCustomer fsxCustomerRow = PXCache<Customer>.GetExtension<FSxCustomer>(customerRow);

                switch (fsxCustomerRow.DefaultBillingCustomerSource)
                {
                    case ID.Default_Billing_Customer_Source.SERVICE_ORDER_CUSTOMER:
                        billCustomerID = fsServiceContractRow.CustomerID;
                        billLocationID = fsServiceContractRow.CustomerLocationID;
                        break;

                    case ID.Default_Billing_Customer_Source.DEFAULT_CUSTOMER:
                        billTo = ID.Contract_BillTo.SPECIFICACCT;
                        billCustomerID = fsServiceContractRow.CustomerID;
                        billLocationID = ServiceOrderCore.GetDefaultLocationID(cache.Graph, fsServiceContractRow.CustomerID);
                        break;

                    case ID.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER:
                        billTo = ID.Contract_BillTo.SPECIFICACCT;
                        billCustomerID = fsxCustomerRow.BillCustomerID;
                        billLocationID = fsxCustomerRow.BillLocationID;
                        break;
                }
            }

            cache.SetValueExt<FSServiceContract.billTo>(fsServiceContractRow, billTo);
            cache.SetValueExt<FSServiceContract.billCustomerID>(fsServiceContractRow, billCustomerID);
            cache.SetValueExt<FSServiceContract.billLocationID>(fsServiceContractRow, billLocationID);
        }

        #endregion

        #region Event Handlers

        #region FSServiceContract

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSServiceContract, FSServiceContract.scheduleGenType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;
            PXCache cache = e.Cache;

            if (fsServiceContractRow.ScheduleGenType != null)
            {
                switch (fsServiceContractRow.ScheduleGenType)
                {
                    case ID.ScheduleGenType_ServiceContract.NONE:

                        SharedFunctions.DefaultGenerationType(cache, fsServiceContractRow, e.Args);
                        break;

                    case ID.ScheduleGenType_ServiceContract.APPOINTMENT:

                    case ID.ScheduleGenType_ServiceContract.SERVICE_ORDER:

                        e.NewValue = fsServiceContractRow.ScheduleGenType;
                        e.Cancel = true;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                SharedFunctions.DefaultGenerationType(cache, fsServiceContractRow, e.Args);
            }
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.branchID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fSServiceContractRow = (FSServiceContract)e.Row;

            fSServiceContractRow.BranchLocationID = null;
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.billingPeriod> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            SetBillingPeriod(fsServiceContractRow);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.projectID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            if (ContractPeriodDetRecords != null)
            {
                foreach (FSContractPeriodDet fsContractPeriodDetRow in ContractPeriodDetRecords.Select())
                {
                    fsContractPeriodDetRow.ProjectID = fsServiceContractRow.ProjectID;
                    fsContractPeriodDetRow.ProjectTaskID = null;
                    fsContractPeriodDetRow.CostCodeID = null;
                    ContractPeriodDetRecords.Update(fsContractPeriodDetRow);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.startDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            UpdateSalesPrices(e.Cache, fsServiceContractRow);
            SetBillingPeriod(fsServiceContractRow);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.endDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.EXPIRED)
            {
                fsServiceContractRow.StatusEffectiveUntilDate = fsServiceContractRow.EndDate;
            }

            SetBillingPeriod(fsServiceContractRow);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.status> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;
            string oldStatus = (string)e.OldValue;

            this.isStatusChanged = oldStatus != fsServiceContractRow.Status;
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.customerID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            Location locationRowTemp;

            if (fsServiceContractRow.CustomerID == null)
            {
                locationRowTemp = null;
            }
            else
            {
                locationRowTemp = PXSelectJoin<Location,
                                  InnerJoin<BAccount,
                                  On<
                                      BAccount.bAccountID, Equal<Location.bAccountID>,
                                      And<BAccount.defLocationID, Equal<Location.locationID>>>>,
                                  Where<
                                      Location.bAccountID, Equal<Required<Location.bAccountID>>>>
                                  .Select(this, fsServiceContractRow.CustomerID);
            }

            if (locationRowTemp == null)
            {
                fsServiceContractRow.CustomerLocationID = null;
            }
            else
            {
                fsServiceContractRow.CustomerLocationID = locationRowTemp.LocationID;
            }

            SetBillCustomerAndLocationID(e.Cache, fsServiceContractRow);
        }


        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.sourcePrice> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;
            UpdateSalesPrices(e.Cache, fsServiceContractRow);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.expirationType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;
            SetContractEndDate(e.Cache, fsServiceContractRow);
            SetUpcommingStatus(fsServiceContractRow);
            SetEffectiveUntilDate(e.Cache, fsServiceContractRow);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.billingType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            if (fsServiceContractRow.BillingType == ID.Contract_BillingType.AS_PERFORMED_BILLINGS && ContractPeriodFilter.Current != null)
            {
                ContractPeriodFilter.Current.ContractPeriodID = null;
                ContractPeriodFilter.Cache.SetDefaultExt<FSContractPeriodFilter.actions>(ContractPeriodFilter.Current);
            }

            if (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
            {
                FSContractPeriod fsContractPeriodRow = new FSContractPeriod();
                fsContractPeriodRow.StartPeriodDate = fsServiceContractRow.StartDate ?? Accessinfo.BusinessDate;
                DateTime? endPeriodDate = SharedFunctions.GetContractPeriodEndDate(fsServiceContractRow, fsContractPeriodRow.StartPeriodDate);

                if (endPeriodDate != null)
                {
                    fsContractPeriodRow.EndPeriodDate = endPeriodDate.Value;
                    ContractPeriodRecords.Current = ContractPeriodRecords.Insert(fsContractPeriodRow);
                }

                ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.actions>(ContractPeriodFilter.Current, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD);
            }
            else
            {
                if (ContractPeriodRecords.Current != null)
                {
                    ContractPeriodRecords.Delete(ContractPeriodRecords.Current);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.billCustomerID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;
            fsServiceContractRow.BillLocationID = null;
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSServiceContract> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSServiceContract> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;
            PXCache cache = e.Cache;

            SetVisibleActivatePeriodButton(cache, fsServiceContractRow);
            EnableDisable_ActionButtons(this, cache, fsServiceContractRow);
            SetVisibleContractBillingSettings(cache, fsServiceContractRow);
            EnableDisable_Document(cache, fsServiceContractRow);
            SetUsageBillingCycle(fsServiceContractRow);
            SetBillInfo(cache, fsServiceContractRow);

            this.SalesPriceLines.AllowSelect = fsServiceContractRow.Mem_ShowPriceTab == true;
            this.SalesPriceLines.AllowSelect = fsServiceContractRow.Mem_ShowPriceTab == true;
            this.ServiceContractSelected.AllowSelect = fsServiceContractRow.Mem_ShowScheduleTab == false;

            PXUIFieldAttribute.SetEnabled<FSServiceContract.projectID>(cache,
                                                                       fsServiceContractRow,
                                                                       cache.GetStatus(fsServiceContractRow) == PXEntryStatus.Inserted);

            SharedFunctions.SetVisibleEnableProjectTask<FSServiceContract.dfltProjectTaskID>(cache, fsServiceContractRow, fsServiceContractRow.ProjectID);
        }

        protected virtual void _(Events.RowInserting<FSServiceContract> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSServiceContract> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSServiceContract> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSServiceContract> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSServiceContract> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSServiceContract> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSServiceContract> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;
            PXCache cache = e.Cache;

            if (Accessinfo.ScreenID != ID.ScreenID.SERVICE_CONTRACT)
            {
                cache.AllowUpdate = true;
            }

            if (fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.UNLIMITED && fsServiceContractRow.EndDate != null)
            {
                fsServiceContractRow.EndDate = null;
            }

            PXResultset<FSContractSchedule> contractRows = ContractSchedules.Select();

            ValidateDates(cache, fsServiceContractRow, contractRows);

            if (fsServiceContractRow.EndDate != null)
            {
                foreach (FSSchedule fsScheduleRow in contractRows)
                {
                    if (fsScheduleRow.EndDate == null)
                    {
                        fsScheduleRow.EndDate = fsServiceContractRow.EndDate;

                        if (fsScheduleRow.NextExecutionDate > fsServiceContractRow.EndDate)
                        {
                            fsScheduleRow.NextExecutionDate = SharedFunctions.GetNextExecution(ContractSchedules.Cache, fsScheduleRow);
                        }

                        ContractSchedules.Cache.SetStatus(fsScheduleRow, PXEntryStatus.Updated);
                    }
                }
            }

            SetUnitPriceForSalesPricesRows(fsServiceContractRow);

            if (e.Operation == PXDBOperation.Delete)
            {
                PXResultset<FSSchedule> bqlResultSet = PXSelect<FSSchedule,
                                                       Where<
                                                           FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>>>
                                                       .Select(this, fsServiceContractRow.ServiceContractID);

                ServiceContractScheduleEntryBase<RouteServiceContractScheduleEntry,
                                                 FSSchedule,
                                                 FSSchedule.scheduleID,
                                                 FSSchedule.entityID> routeServiceContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntryBase<RouteServiceContractScheduleEntry, FSSchedule, FSSchedule.scheduleID, FSSchedule.entityID>>();

                foreach (FSSchedule fsScheduleRow in bqlResultSet)
                {
                    routeServiceContractScheduleEntry.ContractScheduleRecords.Current = fsScheduleRow;
                    routeServiceContractScheduleEntry.ContractScheduleRecords.Delete(fsScheduleRow);
                    routeServiceContractScheduleEntry.Save.Press();
                }

                //Detaching ServiceOrders and Appointments created by schedule generation linked to this ServiceContract.
                PXUpdate<
                    Set<FSServiceOrder.serviceContractID, Required<FSServiceOrder.serviceContractID>>,
                FSServiceOrder,
                Where<
                    FSServiceOrder.serviceContractID, Equal<Required<FSServiceOrder.serviceContractID>>>>
                .Update(this, null, fsServiceContractRow.ServiceContractID);

                PXUpdate<
                    Set<FSAppointment.serviceContractID, Required<FSAppointment.serviceContractID>>,
                FSAppointment,
                Where<
                    FSAppointment.serviceContractID, Equal<Required<FSAppointment.serviceContractID>>>>
                .Update(this, null, fsServiceContractRow.ServiceContractID);
            }
        }

        protected virtual void _(Events.RowPersisted<FSServiceContract> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            string scheduleGenTypeOriginal = (string)e.Cache.GetValueOriginal<FSServiceContract.scheduleGenType>(fsServiceContractRow);

            if (e.TranStatus == PXTranStatus.Open
                    && (e.Operation == PXDBOperation.Insert
                            || e.Operation == PXDBOperation.Update))
            {
                InsertContractAction(fsServiceContractRow, e.Operation);
                InsertContractActionBySchedules(e.Operation);
            }

            if (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS
                && e.TranStatus == PXTranStatus.Completed)
            {
                ContractPeriodFilter.Cache.SetDefaultExt<FSContractPeriodFilter.contractPeriodID>(ContractPeriodFilter.Current);
            }

            if (e.TranStatus == PXTranStatus.Open
                    && e.Operation == PXDBOperation.Update
                        && fsServiceContractRow.ScheduleGenType != scheduleGenTypeOriginal)
            {
                PXUpdate<
                    Set<FSSchedule.scheduleGenType, Required<FSSchedule.scheduleGenType>>,
                FSSchedule,
                Where<
                    FSSchedule.customerID, Equal<Required<FSSchedule.customerID>>,
                And<
                    FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>>>>
                .Update(this,
                        fsServiceContractRow.ScheduleGenType,
                        fsServiceContractRow.CustomerID,
                        fsServiceContractRow.ServiceContractID);

                if (fsServiceContractRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.NONE)
                {
                    PXUpdate<
                        Set<FSSchedule.active, Required<FSSchedule.active>>,
                    FSSchedule,
                    Where<
                        FSSchedule.customerID, Equal<Required<FSSchedule.customerID>>,
                    And<
                        FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>>>>
                    .Update(this,
                            false,
                            fsServiceContractRow.CustomerID,
                            fsServiceContractRow.ServiceContractID);
                }

                ContractSchedules.Cache.Clear();
                ContractSchedules.View.Clear();
                ContractSchedules.View.RequestRefresh();
            }
        }

        #endregion
        #region FSSalesPrice

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

        protected virtual void _(Events.RowSelecting<FSSalesPrice> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSalesPrice fsSalesPriceRow = (FSSalesPrice)e.Row;

            if (ServiceContractRecords.Current != null)
            {
                bool isContract = ServiceContractRecords.Current.SourcePrice == ID.SourcePrice.CONTRACT;

                if (isContract)
                {
                    fsSalesPriceRow.Mem_UnitPrice = fsSalesPriceRow.UnitPrice;
                }
                else
                {
                    fsSalesPriceRow.Mem_UnitPrice = GetSalesPrice(e.Cache, fsSalesPriceRow) ?? 0.0m;
                }
            }
        }

        protected virtual void _(Events.RowSelected<FSSalesPrice> e)
        {
            if (e.Row == null || ServiceContractRecords.Current == null)
            {
                return;
            }

            FSSalesPrice fsSalesPriceRow = (FSSalesPrice)e.Row;

            bool isContract = ServiceContractRecords.Current.SourcePrice == ID.SourcePrice.CONTRACT;

            PXUIFieldAttribute.SetEnabled<FSSalesPrice.mem_UnitPrice>(e.Cache, fsSalesPriceRow, isContract);
        }

        protected virtual void _(Events.RowInserting<FSSalesPrice> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSSalesPrice> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSSalesPrice> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSSalesPrice> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSSalesPrice> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSSalesPrice> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSSalesPrice> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSSalesPrice> e)
        {
        }

        #endregion
        #region FSContractPeriodFilter

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSContractPeriodFilter, FSContractPeriodFilter.actions> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodFilter fsContractPeriodFilterRow = (FSContractPeriodFilter)e.Row;

            if (ServiceContractSelected.Current != null)
            {
                e.NewValue = GetContractPeriodFilterDefaultAction(this, ServiceContractSelected.Current.ServiceContractID);
            }
        }

        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSContractPeriodFilter, FSContractPeriodFilter.actions> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodFilter fsContractPeriodFilterRow = (FSContractPeriodFilter)e.Row;

            e.Cache.SetDefaultExt<FSContractPeriodFilter.contractPeriodID>(e.Row);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSContractPeriodFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodFilter fsContractPeriodFilterRow = (FSContractPeriodFilter)e.Row;
            PXCache cache = e.Cache;

            PXUIFieldAttribute.SetEnabled<FSContractPeriodFilter.contractPeriodID>(cache, fsContractPeriodFilterRow, fsContractPeriodFilterRow.Actions == ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD);
            PXUIFieldAttribute.SetVisible<FSContractPeriodFilter.postDocRefNbr>(cache, fsContractPeriodFilterRow, fsContractPeriodFilterRow.Actions == ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD);

            FSContractPeriod fsContractPeriodSelectedRow = ContractPeriodRecords.Current;
            FSContractPostDoc fsContractPostDocRow = ContractPostDocRecords.Current;

            if (fsContractPeriodSelectedRow == null)
            {
                fsContractPeriodSelectedRow = ContractPeriodRecords.Current = ContractPeriodRecords.Select();
                ContractPeriodFilter.Cache.SetDefaultExt<FSContractPeriodFilter.contractPeriodID>(ContractPeriodFilter.Current);
            }

            if (fsContractPeriodSelectedRow != null && fsContractPeriodSelectedRow.ContractPeriodID != fsContractPeriodFilterRow.ContractPeriodID)
            {
                fsContractPeriodSelectedRow = ContractPeriodRecords.Current = ContractPeriodRecords.Select();
            }

            if (fsContractPostDocRow == null && fsContractPeriodSelectedRow != null && fsContractPeriodSelectedRow.Invoiced == true)
            {
                fsContractPostDocRow = ContractPostDocRecords.Current = ContractPostDocRecords.Select();
            }

            if (fsContractPostDocRow != null && fsContractPostDocRow.ContractPeriodID != fsContractPeriodFilterRow.ContractPeriodID)
            {
                fsContractPostDocRow = ContractPostDocRecords.Current = ContractPostDocRecords.Select();
            }

            bool allowInsertUpdateDelete = fsContractPeriodSelectedRow != null
                                                && fsContractPeriodFilterRow.Actions != ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD
                                                && fsContractPeriodSelectedRow.Status == ID.Status_ContractPeriod.INACTIVE
                                                && ServiceContractRecords.Current.isEditable();

            this.ContractPeriodDetRecords.Cache.AllowUpdate = allowInsertUpdateDelete;
            this.ContractPeriodDetRecords.Cache.AllowInsert = allowInsertUpdateDelete;
            this.ContractPeriodDetRecords.Cache.AllowDelete = allowInsertUpdateDelete;

            if (fsContractPeriodSelectedRow != null && fsContractPeriodFilterRow.ContractPeriodID != null)
            {
                if (fsContractPostDocRow != null)
                {
                    fsContractPeriodFilterRow.PostDocRefNbr = fsContractPostDocRow.PostRefNbr;
                }
                else
                {
                    fsContractPeriodFilterRow.PostDocRefNbr = string.Empty;
                }

                fsContractPeriodFilterRow.StandardizedBillingTotal = fsContractPeriodSelectedRow.PeriodTotal;
            }
            else
            {
                fsContractPeriodFilterRow.PostDocRefNbr = string.Empty;
                fsContractPeriodFilterRow.StandardizedBillingTotal = 0;
            }

            activatePeriod.SetEnabled(EnableDisableActivatePeriodButton(ServiceContractRecords.Current, ContractPeriodRecords.Current));
        }

        protected virtual void _(Events.RowInserting<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSContractPeriodFilter> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSContractPeriodFilter> e)
        {
        }

        #endregion
        #region FSContractPeriodDet

        #region FieldSelecting

        protected virtual void _(Events.FieldSelecting<FSContractPeriodDet, FSContractPeriodDet.amount> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            AmountFieldSelectingHandler(e.Cache, e.Args, typeof(FSContractPeriodDet.amount).Name, fsContractPeriodDetRow.BillingRule, fsContractPeriodDetRow.Time, fsContractPeriodDetRow.Qty);
        }

        protected virtual void _(Events.FieldSelecting<FSContractPeriodDet, FSContractPeriodDet.usedAmount> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            AmountFieldSelectingHandler(e.Cache, e.Args, typeof(FSContractPeriodDet.usedAmount).Name, fsContractPeriodDetRow.BillingRule, fsContractPeriodDetRow.UsedTime, fsContractPeriodDetRow.UsedQty);
        }

        protected virtual void _(Events.FieldSelecting<FSContractPeriodDet, FSContractPeriodDet.remainingAmount> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            AmountFieldSelectingHandler(e.Cache, e.Args, typeof(FSContractPeriodDet.remainingAmount).Name, fsContractPeriodDetRow.BillingRule, fsContractPeriodDetRow.RemainingTime, fsContractPeriodDetRow.RemainingQty);
        }

        protected virtual void _(Events.FieldSelecting<FSContractPeriodDet, FSContractPeriodDet.scheduledAmount> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            AmountFieldSelectingHandler(e.Cache, e.Args, typeof(FSContractPeriodDet.scheduledAmount).Name, fsContractPeriodDetRow.BillingRule, fsContractPeriodDetRow.ScheduledTime, fsContractPeriodDetRow.ScheduledQty);
        }

        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSContractPeriodDet, FSContractPeriodDet.recurringTotalPrice> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            e.NewValue = GetTotalPrice(e.Cache, fsContractPeriodDetRow);
        }

        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSContractPeriodDet, FSContractPeriodDet.billingRule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            SetDefaultQtyTime(e.Cache, fsContractPeriodDetRow);
        }

        protected virtual void _(Events.FieldUpdated<FSContractPeriodDet, FSContractPeriodDet.amount> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            PXCache cache = e.Cache;

            if (fsContractPeriodDetRow.BillingRule == ID.BillingRule.TIME)
            {
                int time = 0;

                if (int.TryParse(fsContractPeriodDetRow.Amount.Replace(" ", "0"), out time))
                {
                    int minutes = time % 100;
                    int hours = (time - minutes) / 100;
                    TimeSpan span = new TimeSpan(0, hours, minutes, 0);
                    cache.SetValueExt<FSContractPeriodDet.time>(fsContractPeriodDetRow, (int)span.TotalMinutes);
                }
            }
            else if (fsContractPeriodDetRow.BillingRule == ID.BillingRule.FLAT_RATE)
            {
                decimal qty = 0.0m;
                decimal.TryParse(fsContractPeriodDetRow.Amount, out qty);
                cache.SetValueExt<FSContractPeriodDet.qty>(fsContractPeriodDetRow, qty);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSContractPeriodDet, FSContractPeriodDet.inventoryID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            PXCache cache = e.Cache;

            SetDefaultBillingRule(cache, fsContractPeriodDetRow);
            cache.SetDefaultExt<FSContractPeriodDet.uOM>(e.Row);
            SetRegularPrice(cache, fsContractPeriodDetRow, ServiceContractRecords.Current);

            cache.SetValueExt<FSContractPeriodDet.recurringUnitPrice>(fsContractPeriodDetRow, fsContractPeriodDetRow.RegularPrice);
            cache.SetValueExt<FSContractPeriodDet.overageItemPrice>(fsContractPeriodDetRow, fsContractPeriodDetRow.RegularPrice);

            decimal? totalPrice = GetTotalPrice(cache, fsContractPeriodDetRow);
            cache.SetValueExt<FSContractPeriodDet.recurringTotalPrice>(fsContractPeriodDetRow, totalPrice);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSContractPeriodDet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            using (new PXConnectionScope())
            {
                SetRegularPrice(e.Cache, fsContractPeriodDetRow, ServiceContractRecords.Current);

                var fsSODetRows = PXSelectJoin<FSSODet,
                                  InnerJoin<FSServiceOrder,
                                  On<
                                      FSServiceOrder.sOID, Equal<FSSODet.sOID>>>,
                                  Where2<
                                      Where<
                                          FSServiceOrder.billServiceContractID, Equal<Required<FSServiceOrder.billServiceContractID>>,
                                          And<FSServiceOrder.billContractPeriodID, Equal<Required<FSServiceOrder.billContractPeriodID>>,
                                          And<FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                                          And<FSServiceOrder.allowInvoice, Equal<False>,
                                          And<FSSODet.status, NotEqual<FSSODet.status.Canceled>>>>>>,
                                      And<
                                          Where2<
                                              Where<
                                                  FSSODet.inventoryID, Equal<Required<FSSODet.inventoryID>>,
                                                  And<FSSODet.contractRelated, Equal<True>>>,
                                              And<
                                                  Where2<
                                                      Where<
                                                          FSSODet.billingRule, Equal<Required<FSSODet.billingRule>>,
                                                          Or<Required<FSSODet.billingRule>, IsNull>>,
                                                      And<
                                                          Where<
                                                              FSSODet.SMequipmentID, Equal<Required<FSSODet.SMequipmentID>>,
                                                              Or<Required<FSSODet.SMequipmentID>, IsNull>>>>>>>>>
                                  .Select(this, fsContractPeriodDetRow.ServiceContractID,
                                          fsContractPeriodDetRow.ContractPeriodID, fsContractPeriodDetRow.InventoryID,
                                          fsContractPeriodDetRow.BillingRule, fsContractPeriodDetRow.BillingRule,
                                          fsContractPeriodDetRow.SMEquipmentID, fsContractPeriodDetRow.SMEquipmentID)
                                  .AsEnumerable();

                var fsAppointmentDetRows = PXSelectJoin<FSAppointmentDet,
                                           InnerJoin<FSAppointment,
                                           On<
                                               FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                                           InnerJoin<FSSODet,
                                           On<
                                               FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>>,
                                           Where2<
                                               Where<
                                                   FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                                   And<FSAppointment.billContractPeriodID, Equal<Required<FSAppointment.billContractPeriodID>>,
                                                   And<FSAppointment.status, NotEqual<FSAppointment.status.Closed>,
                                                   And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                   And<FSAppointmentDet.isCanceledNotPerformed, NotEqual<True>>>>>>,
                                               And<
                                                   Where2<
                                                       Where<
                                                           FSAppointmentDet.inventoryID, Equal<Required<FSAppointmentDet.inventoryID>>,
                                                           And<FSAppointmentDet.contractRelated, Equal<True>>>,
                                                       And<
                                                           Where2<
                                                               Where<
                                                                   FSSODet.billingRule, Equal<Required<FSSODet.billingRule>>,
                                                                   Or<Required<FSSODet.billingRule>, IsNull>>,
                                                               And<
                                                                   Where<
                                                                       FSAppointmentDet.SMequipmentID, Equal<Required<FSAppointmentDet.SMequipmentID>>,
                                                                       Or<Required<FSAppointmentDet.SMequipmentID>, IsNull>>>>>>>>>
                                           .Select(this, fsContractPeriodDetRow.ServiceContractID,
                                                   fsContractPeriodDetRow.ContractPeriodID, fsContractPeriodDetRow.InventoryID,
                                                   fsContractPeriodDetRow.BillingRule, fsContractPeriodDetRow.BillingRule,
                                                   fsContractPeriodDetRow.SMEquipmentID, fsContractPeriodDetRow.SMEquipmentID)
                                           .AsEnumerable();

                decimal? qtySum = 0;

                if (fsSODetRows.Count() > 0 || fsAppointmentDetRows.Count() > 0)
                {
                    qtySum = fsSODetRows.Sum(x => ((FSSODet)x).EstimatedQty);

                    foreach (PXResult<FSAppointmentDet, FSAppointment, FSSODet> row in fsAppointmentDetRows)
                    {
                        FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)row;
                        FSAppointment fsAppointmentRow = (FSAppointment)row;

                        if (fsAppointmentRow.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED
                                || fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED)
                        {
                            qtySum += fsAppointmentDetRow.EstimatedQty;
                        }
                        else
                        {
                            qtySum += fsAppointmentDetRow.Qty;
                        }
                    }
                }

                if (fsContractPeriodDetRow.BillingRule == ID.BillingRule.FLAT_RATE)
                {
                    fsContractPeriodDetRow.ScheduledQty = qtySum;
                }
                else if (fsContractPeriodDetRow.BillingRule == ID.BillingRule.TIME)
                {
                    fsContractPeriodDetRow.ScheduledTime = (int?)(qtySum * 60);
                }
            }
        }

        protected virtual void _(Events.RowSelected<FSContractPeriodDet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)e.Row;
            EnableDisableContractPeriodDet(e.Cache, fsContractPeriodDetRow);

            SharedFunctions.SetEnableCostCodeProjectTask<FSContractPeriodDet.projectTaskID, FSContractPeriodDet.costCodeID>(e.Cache, fsContractPeriodDetRow, fsContractPeriodDetRow.LineType, ServiceContractRecords.Current?.ProjectID);
        }

        protected virtual void _(Events.RowInserting<FSContractPeriodDet> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSContractPeriodDet> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSContractPeriodDet> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSContractPeriodDet> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSContractPeriodDet> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSContractPeriodDet> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSContractPeriodDet> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSContractPeriodDet> e)
        {
        }

        #endregion
        #region PrepaidContractFilters
        protected virtual void _(Events.FieldUpdated<FSActivationContractFilter, FSActivationContractFilter.activationDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSActivationContractFilter filter = (FSActivationContractFilter)e.Row;
            PXCache cache = e.Cache;

            if (filter.ActivationDate.HasValue == true)
            {
                if (filter.ActivationDate.Value.Date < this.Accessinfo.BusinessDate.Value.Date)
                {
                    cache.RaiseExceptionHandling<FSActivationContractFilter.activationDate>(filter,
                                                                                            filter.ActivationDate,
                                                                                            new PXSetPropertyException(TX.Error.EFFECTIVE_DATE_LOWER_ACTUAL_DATE, PXErrorLevel.Error));
                }

                if (ServiceContractRecords.Current.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                        && filter.ActivationDate.Value.Date >= ServiceContractRecords.Current.EndDate)
                {
                    cache.RaiseExceptionHandling<FSActivationContractFilter.activationDate>(filter,
                                                                                            filter.ActivationDate,
                                                                                            new PXSetPropertyException(TX.Error.EFFECTIVE_DATE_GREATER_END_DATE, PXErrorLevel.Error));
                }

                foreach (ActiveSchedule activeScheduleRow in ActiveScheduleRecords.Select())
                {
                    if (activeScheduleRow.ChangeRecurrence == true)
                    {
                        ActiveScheduleRecords.Cache.SetValueExt<ActiveSchedule.effectiveRecurrenceStartDate>(activeScheduleRow, ActivationContractFilter.Current.ActivationDate);
                    }
                }

            }
            else
            {
                cache.RaiseExceptionHandling<FSActivationContractFilter.activationDate>(filter,
                                                                                        filter.ActivationDate,
                                                                                        new PXSetPropertyException(TX.Error.FIELD_EMPTY, PXErrorLevel.Error));
            }
        }

        protected virtual void _(Events.FieldUpdated<FSTerminateContractFilter, FSTerminateContractFilter.cancelationDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSTerminateContractFilter filter = (FSTerminateContractFilter)e.Row;
            PXCache cache = e.Cache;

            if (filter.CancelationDate.HasValue == true)
            {
                if (filter.CancelationDate.Value.Date < this.Accessinfo.BusinessDate.Value.Date)
                {
                    cache.RaiseExceptionHandling<FSTerminateContractFilter.cancelationDate>(filter,
                                                                                            filter.CancelationDate,
                                                                                            new PXSetPropertyException(TX.Error.EFFECTIVE_DATE_LOWER_ACTUAL_DATE, PXErrorLevel.Error));
                }

                if (ServiceContractRecords.Current.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                        && filter.CancelationDate.Value.Date >= ServiceContractRecords.Current.EndDate)
                {
                    cache.RaiseExceptionHandling<FSTerminateContractFilter.cancelationDate>(filter,
                                                                                            filter.CancelationDate,
                                                                                            new PXSetPropertyException(TX.Error.EFFECTIVE_DATE_GREATER_END_DATE, PXErrorLevel.Error));
                }
            }
            else
            {
                cache.RaiseExceptionHandling<FSTerminateContractFilter.cancelationDate>(filter,
                                                                                        filter.CancelationDate,
                                                                                        new PXSetPropertyException(TX.Error.FIELD_EMPTY, PXErrorLevel.Error));
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSuspendContractFilter, FSSuspendContractFilter.suspensionDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSuspendContractFilter filter = (FSSuspendContractFilter)e.Row;
            PXCache cache = e.Cache;

            if (filter.SuspensionDate.HasValue == true)
            {
                if (filter.SuspensionDate.Value.Date < this.Accessinfo.BusinessDate.Value.Date)
                {
                    cache.RaiseExceptionHandling<FSSuspendContractFilter.suspensionDate>(filter,
                                                                                         filter.SuspensionDate,
                                                                                         new PXSetPropertyException(TX.Error.EFFECTIVE_DATE_LOWER_ACTUAL_DATE, PXErrorLevel.Error));
                }

                if (ServiceContractRecords.Current.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                        && filter.SuspensionDate.Value.Date >= ServiceContractRecords.Current.EndDate)
                {
                    cache.RaiseExceptionHandling<FSSuspendContractFilter.suspensionDate>(filter,
                                                                                         filter.SuspensionDate,
                                                                                         new PXSetPropertyException(TX.Error.EFFECTIVE_DATE_GREATER_END_DATE, PXErrorLevel.Error));
                }
            }
            else
            {
                cache.RaiseExceptionHandling<FSSuspendContractFilter.suspensionDate>(filter,
                                                                                     filter.SuspensionDate,
                                                                                     new PXSetPropertyException(TX.Error.FIELD_EMPTY, PXErrorLevel.Error));
            }
        }

        protected virtual void _(Events.FieldSelecting<ActiveSchedule, ActiveSchedule.effectiveRecurrenceStartDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            ActiveSchedule activeScheduleRow = (ActiveSchedule)e.Row;
            PXCache cache = e.Cache;

            if (ActivationContractFilter.Current.ActivationDate.HasValue == true
                    && activeScheduleRow.EffectiveRecurrenceStartDate == null)
            {
                if (activeScheduleRow.ChangeRecurrence == true)
                {
                    cache.SetValueExt<ActiveSchedule.effectiveRecurrenceStartDate>(activeScheduleRow, ActivationContractFilter.Current.ActivationDate);
                    e.ReturnValue = ActivationContractFilter.Current.ActivationDate;
                }
                else
                {
                    cache.SetValueExt<ActiveSchedule.effectiveRecurrenceStartDate>(activeScheduleRow, activeScheduleRow.StartDate);
                    e.ReturnValue = activeScheduleRow.StartDate;
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<ActiveSchedule, ActiveSchedule.changeRecurrence> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (ActivationContractFilter.Current.ActivationDate.HasValue == true)
            {
                ActiveSchedule activeScheduleRow = (ActiveSchedule)e.Row;
                PXCache cache = e.Cache;

                if (activeScheduleRow.ChangeRecurrence == true)
                {
                    cache.SetValueExt<ActiveSchedule.effectiveRecurrenceStartDate>(activeScheduleRow, ActivationContractFilter.Current.ActivationDate);
                }
                else
                {
                    cache.SetValueExt<ActiveSchedule.effectiveRecurrenceStartDate>(activeScheduleRow, activeScheduleRow.StartDate);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<ActiveSchedule, ActiveSchedule.effectiveRecurrenceStartDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            ActiveSchedule activeScheduleRow = (ActiveSchedule)e.Row;
            PXCache cache = e.Cache;

            if (activeScheduleRow.EffectiveRecurrenceStartDate.HasValue == true)
            {
                ActiveSchedule activeScheduleRowCopy = (ActiveSchedule)cache.CreateCopy(activeScheduleRow);
                activeScheduleRowCopy.EndDate = null;
                activeScheduleRowCopy.StartDate = activeScheduleRow.EffectiveRecurrenceStartDate;
                activeScheduleRow.NextExecution = SharedFunctions.GetNextExecution(cache, activeScheduleRowCopy);
            }
        }
        #endregion

        #endregion
    }
}
