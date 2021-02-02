using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.FS.Scheduler;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class ContractGenerationEnqBase<TGraph, TPrimary, TFiltering, TRecordType> : PXGraph<TGraph>
        where TGraph : PXGraph
        where TPrimary : class, IBqlTable, new()
        where TFiltering : class, IBqlTable, new()
        where TRecordType : IConstant<string>, IBqlOperand, new()
    {
        public AppointmentEntry graphAppointmentEntry;
        public ServiceOrderEntry graphServiceOrderEntry;
        public int? nextGenerationID;

        #region Generation Functions
        /// <summary>
        /// Generates Appointments (Routes Contract) or Service Orders (Service Contract) for each TimeSlot in the [scheduleRules] List.
        /// </summary>
        public void GenerateAPPSOUpdateContracts(List<Schedule> scheduleRules, string recordType, DateTime? fromDate, DateTime? toDate, FSSchedule fsScheduleRow)
        {
            var generator = new TimeSlotGenerator();
            DateTime processEndDate = (DateTime)GetProcessEndDate(scheduleRules.ElementAt(0), toDate);

            var period = new Period((DateTime)fromDate, processEndDate);

            // Determines the next generationID number
            if (nextGenerationID == null)
            {
                FSProcessIdentity fsProcessIdentityRow = new FSProcessIdentity();
                fsProcessIdentityRow.ProcessType = recordType;
                fsProcessIdentityRow.FilterFromTo = fromDate;
                fsProcessIdentityRow.FilterUpTo = toDate;

                ProcessIdentityMaint graphProcessIdentityMaint = PXGraph.CreateInstance<ProcessIdentityMaint>();
                graphProcessIdentityMaint.processIdentityRecords.Insert(fsProcessIdentityRow);
                graphProcessIdentityMaint.Save.Press();

                nextGenerationID = graphProcessIdentityMaint.processIdentityRecords.Current.ProcessID;
            }

            List<TimeSlot> timeSlots = generator.GenerateCalendar(period, scheduleRules, nextGenerationID);
            DateTime? failsOnDate = null;

            // Transaction to create the appointments (Routes Contract) or Service Orders (Service Contract)
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                Customer customerRow = PXSelect<Customer,
                                       Where<
                                           Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                       .Select(this, fsScheduleRow.CustomerID)
                                       .FirstOrDefault();

                if (customerRow.Status != BAccount.status.Active && customerRow.Status != BAccount.status.OneTime)
                {
                    throw new PXException(PXMessages.LocalizeFormat(AR.Messages.CustomerIsInStatus, new Customer.status.ListAttribute().ValueLabelDic[customerRow.Status]));
                }

                try
                {
                    foreach (var timeSlot in timeSlots)
                    {
                        failsOnDate = timeSlot.DateTimeBegin;

                        if (recordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
                        {
                            bool createAppointmnet = fsScheduleRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.APPOINTMENT;

                            CreateServiceOrder(timeSlot, createAppointmnet);
                        }
                        else if (recordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
                        {
                            CreateServiceOrder(timeSlot, true, true);
                        }
                    }

                    DateTime? lastGenerationDate = null;

                    if (timeSlots.Count > 0)
                    {
                        lastGenerationDate = timeSlots.Max(a => a.DateTimeBegin);
                    }

                    CreateContractGenerationHistory((int)nextGenerationID,
                                                    scheduleRules.ElementAt(0).ScheduleID,
                                                    processEndDate,
                                                    lastGenerationDate,
                                                    recordType);

                    UpdateGeneratedSchedule(scheduleRules.ElementAt(0).ScheduleID, processEndDate, lastGenerationDate, fsScheduleRow);
                }
                catch (Exception e)
                {
                    var exceptionWithContextMessage = ExceptionHelper.GetExceptionWithContextMessage(PXMessages.Localize(TX.Messages.COULD_NOT_PROCESS_RECORD), e);

                    FSGenerationLogError fsGenerationLogErrorRow = new FSGenerationLogError();
                    fsGenerationLogErrorRow.ProcessType = recordType;
                    fsGenerationLogErrorRow.ErrorMessage = exceptionWithContextMessage.Message;
                    fsGenerationLogErrorRow.ScheduleID = scheduleRules.ElementAt(0).ScheduleID;
                    fsGenerationLogErrorRow.GenerationID = nextGenerationID;
                    fsGenerationLogErrorRow.ErrorDate = failsOnDate;

                    ts.Dispose();

                    var grapGenerationLogErrorMaint = PXGraph.CreateInstance<GenerationLogErrorMaint>();

                    grapGenerationLogErrorMaint.LogErrorMessageRecords.Insert(fsGenerationLogErrorRow);
                    grapGenerationLogErrorMaint.Save.Press();
                    throw exceptionWithContextMessage;
                }

                ts.Complete(this);
            }
        }

        /// <summary>
        /// Create an Appointment from a TimeSlot.
        /// </summary>
        protected void CreateAppointment(FSServiceOrder fsServiceOrderRow, 
                                         TimeSlot timeSlotAppointment,
                                         FSSchedule fsScheduleRow,
                                         bool insertingFromServiceOrder,
                                         bool isARouteAppointment,
                                         bool isPrepaidContract)
        {
            if (graphAppointmentEntry != null)
            {
                graphAppointmentEntry.Clear(PXClearOption.ClearAll);
            }
            else
            {
                graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();
                graphAppointmentEntry.CalculateGoogleStats = false;
                graphAppointmentEntry.AvoidCalculateRouteStats = true;
                graphAppointmentEntry.IsGeneratingAppointment = true;
                graphAppointmentEntry.DisableServiceOrderUnboundFieldCalc = true;
            }

            graphAppointmentEntry.SkipManualTimeFlagUpdate = true;

            FSScheduleRoute fsScheduleRouteRow = null;

            if (isARouteAppointment == true)
            { 
                 fsScheduleRouteRow = PXSelect<FSScheduleRoute,
                                      Where<
                                          FSScheduleRoute.scheduleID, Equal<Required<FSScheduleRoute.scheduleID>>>>
                                      .Select(this, fsServiceOrderRow.ScheduleID);
            }

            FSAppointment fsAppointmentRow = new FSAppointment();
            fsAppointmentRow.SrvOrdType = fsServiceOrderRow.SrvOrdType;

            #region Setting flags for recurring appointments
            fsAppointmentRow.ValidatedByDispatcher = false;
            fsAppointmentRow.GeneratedBySystem = true;
            fsAppointmentRow.Status = ID.Status_Appointment.AUTOMATIC_SCHEDULED;

            fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Insert(fsAppointmentRow);

            graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.soRefNbr>(graphAppointmentEntry.AppointmentRecords.Current, fsServiceOrderRow.RefNbr);
            graphAppointmentEntry.AppointmentRecords.Cache.SetValueExt<FSAppointment.docDesc>(fsAppointmentRow, fsServiceOrderRow.DocDesc);
           
            if (isARouteAppointment)
            {
                graphAppointmentEntry.AppointmentRecords.Cache.SetValueExt<FSAppointment.scheduledDateTimeBegin>(fsAppointmentRow, timeSlotAppointment.DateTimeBegin);
                graphAppointmentEntry.AppointmentRecords.Cache.SetValueExt<FSAppointment.scheduledDateTimeEnd>(fsAppointmentRow, timeSlotAppointment.DateTimeEnd);
            }
            else
            {
                DateTime? scheduledDateTimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(timeSlotAppointment.DateTimeBegin, fsScheduleRow.ScheduleStartTime);
                graphAppointmentEntry.AppointmentRecords.Cache.SetValueExt<FSAppointment.scheduledDateTimeBegin>(fsAppointmentRow, scheduledDateTimeBegin);
            }

            graphAppointmentEntry.AppointmentRecords.Cache.SetValueExt<FSAppointment.serviceContractID>(fsAppointmentRow, fsServiceOrderRow.ServiceContractID);

            if (isPrepaidContract == true
                    && graphAppointmentEntry.BillingCycleRelated.Current != null
                    && graphAppointmentEntry.BillingCycleRelated.Current.BillingBy == ID.Billing_By.APPOINTMENT)
            {
                graphAppointmentEntry.AppointmentRecords.Cache.SetValueExt<FSAppointment.billServiceContractID>(fsAppointmentRow, fsServiceOrderRow.ServiceContractID);
            }

            fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Current;

            //Total times get initialized
            fsAppointmentRow.EstimatedDurationTotal = 0;
            fsAppointmentRow.ActualDurationTotal = 0;

            fsAppointmentRow.ScheduleID = fsServiceOrderRow.ScheduleID;
            fsAppointmentRow.ServiceContractID = fsServiceOrderRow.ServiceContractID;
            fsAppointmentRow.GenerationID = fsServiceOrderRow.GenerationID;

            fsAppointmentRow.SalesPersonID = fsServiceOrderRow.SalesPersonID;
            fsAppointmentRow.Commissionable = fsServiceOrderRow.Commissionable;
            #endregion

            #region Setting route
            if (fsScheduleRouteRow != null)
            {
                switch (timeSlotAppointment.DateTimeBegin.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        if (fsScheduleRouteRow.RouteIDSunday != null)
                        {
                            fsAppointmentRow.RouteID = fsScheduleRouteRow.RouteIDSunday;
                        }

                        break;
                    case DayOfWeek.Monday:
                        if (fsScheduleRouteRow.RouteIDMonday != null)
                        {
                            fsAppointmentRow.RouteID = fsScheduleRouteRow.RouteIDMonday;
                        }

                        break;
                    case DayOfWeek.Tuesday:
                        if (fsScheduleRouteRow.RouteIDTuesday != null)
                        {
                            fsAppointmentRow.RouteID = fsScheduleRouteRow.RouteIDTuesday;
                        }

                        break;
                    case DayOfWeek.Wednesday:
                        if (fsScheduleRouteRow.RouteIDWednesday != null)
                        {
                            fsAppointmentRow.RouteID = fsScheduleRouteRow.RouteIDWednesday;
                        }

                        break;
                    case DayOfWeek.Thursday:
                        if (fsScheduleRouteRow.RouteIDThursday != null)
                        {
                            fsAppointmentRow.RouteID = fsScheduleRouteRow.RouteIDThursday;
                        }

                        break;
                    case DayOfWeek.Friday:
                        if (fsScheduleRouteRow.RouteIDFriday != null)
                        {
                            fsAppointmentRow.RouteID = fsScheduleRouteRow.RouteIDFriday;
                        }

                        break;
                    case DayOfWeek.Saturday:
                        if (fsScheduleRouteRow.RouteIDSaturday != null)
                        {
                            fsAppointmentRow.RouteID = fsScheduleRouteRow.RouteIDSaturday;
                        }

                        break;
                }

                if (fsAppointmentRow.RouteID == null)
                {
                    fsAppointmentRow.RouteID = fsScheduleRouteRow.DfltRouteID;
                }

                fsAppointmentRow.RoutePosition = int.Parse(fsScheduleRouteRow.GlobalSequence);

                if (fsAppointmentRow.DeliveryNotes == null)
                {
                    fsAppointmentRow.DeliveryNotes = fsScheduleRouteRow.DeliveryNotes;
                }
            }
            #endregion

            #region Setting Appointment Details
            foreach (FSSODet fsSODetRow in graphServiceOrderEntry.ServiceOrderDetails.Select())
            {
                var fsAppointmentDetRow = new FSAppointmentDet();

                fsAppointmentDetRow.ScheduleID = fsSODetRow.ScheduleID;
                fsAppointmentDetRow.ScheduleDetID = fsSODetRow.ScheduleDetID;

                AppointmentEntry.InsertDetailLine<FSAppointmentDet, FSSODet>(graphAppointmentEntry.AppointmentDetails.Cache,
                                                                             fsAppointmentDetRow,
                                                                             graphServiceOrderEntry.ServiceOrderDetails.Cache,
                                                                             fsSODetRow,
                                                                             fsSODetRow.NoteID,
                                                                             fsSODetRow.SODetID,
                                                                             copyTranDate: false,
                                                                             tranDate: fsSODetRow.TranDate,
                                                                             SetValuesAfterAssigningSODetID: false,
                                                                             copyingFromQuote: false);
            }

            foreach (FSSOEmployee fsSOEmployeeRow in graphServiceOrderEntry.ServiceOrderEmployees.Select())
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();
                fsAppointmentEmployeeRow.EmployeeID = fsSOEmployeeRow.EmployeeID;
                graphAppointmentEntry.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
            }
            #endregion

            if (fsScheduleRow.VendorID != null)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();
                fsAppointmentEmployeeRow.EmployeeID = fsScheduleRow.VendorID;
                graphAppointmentEntry.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
            }

            graphAppointmentEntry.Save.Press();
        }

        /// <summary>
        /// Create a Service Order from a TimeSlot.
        /// </summary>
        protected void CreateServiceOrder(TimeSlot timeSlotServiceOrder, bool createAppointmentFlag = false, bool appointmentsBelongToRoute = false)
        {
            // This action allows to avoid errors related to cache
            if (graphServiceOrderEntry != null)
            {
                graphServiceOrderEntry.Clear(PXClearOption.ClearAll);
            }
            else
            {
                graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();
                graphServiceOrderEntry.DisableServiceOrderUnboundFieldCalc = true;
            }

            FSSchedule fsScheduleRow = ScheduleSelected.Select(timeSlotServiceOrder.ScheduleID);
            FSServiceContract fsServiceContractRow = ServiceContractSelected.Select(fsScheduleRow.EntityID);

            bool isPrepaidContract = fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS;

            //Services from the Schedule
            var fsScheduleDetSet = ScheduleLinesSelected.Select(timeSlotServiceOrder.ScheduleID);

            //Templates from the Schedule
            var fsScheduleDetTemplateSet = ScheduleTemplatesSelected.Select(timeSlotServiceOrder.ScheduleID);
            
            FSServiceOrder fsServiceOrderRow = new FSServiceOrder();
            fsServiceOrderRow.SrvOrdType = fsScheduleRow.SrvOrdType;
            graphServiceOrderEntry.ServiceOrderRecords.Insert(fsServiceOrderRow);

            fsServiceOrderRow = graphServiceOrderEntry.ServiceOrderRecords.Cache.CreateCopy(graphServiceOrderEntry.ServiceOrderRecords.Current) as FSServiceOrder;

            fsServiceOrderRow.BranchID = fsScheduleRow.BranchID;
            fsServiceOrderRow.BranchLocationID = fsScheduleRow.BranchLocationID;
            fsServiceOrderRow.OrderDate = timeSlotServiceOrder.DateTimeBegin.Date;
            fsServiceOrderRow.CustomerID = fsServiceContractRow.CustomerID;
            fsServiceOrderRow.LocationID = fsScheduleRow.CustomerLocationID;

            graphServiceOrderEntry.ServiceOrderRecords.Update(fsServiceOrderRow);
            fsServiceOrderRow = graphServiceOrderEntry.ServiceOrderRecords.Cache.CreateCopy(graphServiceOrderEntry.ServiceOrderRecords.Current) as FSServiceOrder;

            // Currently Service-Management's contracts DO NOT support multi-currency specification
            // therefore their ServiceOrders MUST be created in customer base currency
            if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
            {
                string curyID = null;

                Customer customer = (Customer)PXSelect<Customer, 
                                                Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                            .Select(this, fsServiceContractRow.CustomerID);

                if (customer != null) 
				{
                    curyID = customer.CuryID;
                }

                if (string.IsNullOrEmpty(curyID) == true) 
                {
                    curyID = this.Accessinfo.BaseCuryID ?? new PXSetup<PX.Objects.GL.Company>(this).Current?.BaseCuryID;
                }

                fsServiceOrderRow.CuryID = curyID;
            }

            fsServiceOrderRow.DocDesc = timeSlotServiceOrder.Descr;
            fsServiceOrderRow.BillCustomerID = fsServiceContractRow.BillCustomerID;
            fsServiceOrderRow.BillLocationID = fsServiceContractRow.BillLocationID;
            fsServiceOrderRow.ServiceContractID = fsServiceContractRow.ServiceContractID;
            fsServiceOrderRow.ProjectID = fsServiceContractRow.ProjectID;
            fsServiceOrderRow.DfltProjectTaskID = fsServiceContractRow.DfltProjectTaskID;

            if (isPrepaidContract == true
                && graphServiceOrderEntry.BillingCycleRelated.Current != null
                    && graphServiceOrderEntry.BillingCycleRelated.Current.BillingBy == ID.Billing_By.SERVICE_ORDER)
            {
                fsServiceOrderRow.BillServiceContractID = fsServiceContractRow.ServiceContractID;
            }

            fsServiceOrderRow.ScheduleID = timeSlotServiceOrder.ScheduleID;
            fsServiceOrderRow.ServiceContractID = fsServiceContractRow.ServiceContractID;
            fsServiceOrderRow.GenerationID = timeSlotServiceOrder.GenerationID;

            fsServiceOrderRow.SalesPersonID = fsServiceContractRow.SalesPersonID;
            fsServiceOrderRow.Commissionable = fsServiceContractRow.Commissionable;

            fsServiceOrderRow = graphServiceOrderEntry.ServiceOrderRecords.Update(fsServiceOrderRow);

            if (fsServiceOrderRow.SalesPersonID == null)
            {
                object salesPersonID;
                graphServiceOrderEntry.ServiceOrderRecords.Cache.RaiseFieldDefaulting<FSServiceOrder.salesPersonID>(fsServiceOrderRow, out salesPersonID);
                fsServiceOrderRow.SalesPersonID = (int?)salesPersonID;

                object commissionable;
                graphServiceOrderEntry.ServiceOrderRecords.Cache.RaiseFieldDefaulting<FSServiceOrder.commissionable>(fsServiceOrderRow, out commissionable);
                fsServiceOrderRow.Commissionable  = (bool?)commissionable;
            }

            graphServiceOrderEntry.Answers.CopyAllAttributes(fsServiceOrderRow, fsScheduleRow);

            #region Setting Service Order Details
            foreach (FSScheduleDet fsScheduleDetRow in fsScheduleDetSet)
            {
                if (fsScheduleDetRow.LineType == ID.LineType_ServiceContract.SERVICE_TEMPLATE)
                {
                    foreach (FSScheduleDet fsScheduleDetLocalRow in fsScheduleDetTemplateSet.Where(x => ((FSScheduleDet)x).ServiceTemplateID == fsScheduleDetRow.ServiceTemplateID))
                    {
                        var fsServiceTemplateDetSet_Service = ServiceTemplateSelected.Select(fsScheduleDetRow.ServiceTemplateID);
                            
                        foreach (FSServiceTemplateDet fsServiceTemplateDetRow_Service in fsServiceTemplateDetSet_Service)
                        {
                            FSSODet fsSODetRow = new FSSODet();

                            // Insert the new row with key fields
                            fsSODetRow.ScheduleID = fsScheduleDetLocalRow.ScheduleID;
                            fsSODetRow.ScheduleDetID = fsScheduleDetLocalRow.ScheduleDetID;
                            fsSODetRow.LineType = fsServiceTemplateDetRow_Service.LineType;

                            fsSODetRow = graphServiceOrderEntry.ServiceOrderDetails.Insert(fsSODetRow);

                            fsSODetRow.InventoryID = fsServiceTemplateDetRow_Service.InventoryID;

                            fsSODetRow = graphServiceOrderEntry.ServiceOrderDetails.Update(fsSODetRow);

                            PXNoteAttribute.CopyNoteAndFiles(ScheduleLinesSelected.Cache,
                                                             fsScheduleDetLocalRow,
                                                             graphServiceOrderEntry.ServiceOrderDetails.Cache,
                                                             fsSODetRow,
                                                             copyNotes: true,
                                                             copyFiles: true);

                            // Create a copy to update the other fields
                            fsSODetRow = (FSSODet)graphServiceOrderEntry.ServiceOrderDetails.Cache.CreateCopy(fsSODetRow);

                            fsSODetRow.TranDesc = fsServiceTemplateDetRow_Service.TranDesc;
                            
                            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(this, fsServiceTemplateDetRow_Service.InventoryID);

                            if (inventoryItemRow != null)
                            {
                                FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

                                if (fsxServiceRow != null && fsxServiceRow.BillingRule == ID.BillingRule.TIME)
                                {
                                    int? estimatedDuration = CalculateEstimatedDuration(fsSODetRow, fsServiceTemplateDetRow_Service.Qty);
                                    fsSODetRow.EstimatedDuration = estimatedDuration;
                                }
                                else
                                {
                                    fsSODetRow.EstimatedQty = fsServiceTemplateDetRow_Service.Qty;
                                }
                            }

                            if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                            {
                                fsSODetRow.ManualPrice = true;
                                // TODO: Check where is assigned the contract price?
                            }

                            if (createAppointmentFlag)
                            {
                                fsSODetRow.Scheduled = true;
                                fsSODetRow.Status = ID.Status_SODet.SCHEDULED;
                            }

                            fsSODetRow.EquipmentAction = fsScheduleDetRow.EquipmentAction;
                            fsSODetRow.SMEquipmentID = fsScheduleDetRow.SMEquipmentID;
                            fsSODetRow.ComponentID = fsScheduleDetRow.ComponentID;
                            fsSODetRow.EquipmentLineRef = fsScheduleDetRow.EquipmentLineRef;
                            fsSODetRow.ProjectTaskID = fsScheduleDetRow.ProjectTaskID;
                            fsSODetRow.CostCodeID = fsScheduleDetRow.CostCodeID;

                            if (fsSODetRow.InventoryID != null)
                            {
                                fsSODetRow.ProjectTaskID = fsScheduleDetRow.ProjectTaskID;
                                fsSODetRow.CostCodeID = fsScheduleDetRow.CostCodeID;
                            }

                            graphServiceOrderEntry.ServiceOrderDetails.Update(fsSODetRow);
                        }
                    }
                }
                else
                {
                    FSSODet fsSODetRow = new FSSODet();

                    // Insert the new row with key fields
                    fsSODetRow.ScheduleID = fsScheduleDetRow.ScheduleID;
                    fsSODetRow.ScheduleDetID = fsScheduleDetRow.ScheduleDetID;
                    fsSODetRow.LineType = fsScheduleDetRow.LineType;

                    fsSODetRow = graphServiceOrderEntry.ServiceOrderDetails.Insert(fsSODetRow);

                    fsSODetRow.InventoryID = fsScheduleDetRow.InventoryID;

                    fsSODetRow = graphServiceOrderEntry.ServiceOrderDetails.Update(fsSODetRow);

                    PXNoteAttribute.CopyNoteAndFiles(ScheduleLinesSelected.Cache,
                                                     fsScheduleDetRow,
                                                     graphServiceOrderEntry.ServiceOrderDetails.Cache,
                                                     fsSODetRow,
                                                     copyNotes: true,
                                                     copyFiles: true);

                    // Create a copy to update the other fields
                    fsSODetRow = (FSSODet)graphServiceOrderEntry.ServiceOrderDetails.Cache.CreateCopy(fsSODetRow);

                    fsSODetRow.TranDesc = fsScheduleDetRow.TranDesc;
                    fsSODetRow.BillingRule = fsScheduleDetRow.BillingRule;

                    if (fsSODetRow.BillingRule == ID.BillingRule.TIME)
                    {
                        int? estimatedDuration = CalculateEstimatedDuration(fsSODetRow, fsScheduleDetRow?.Qty);
                        fsSODetRow.EstimatedDuration = estimatedDuration;
                    }
                    else
                    {
                        fsSODetRow.EstimatedQty = fsScheduleDetRow.Qty;
                    }

                    if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                    {
                        fsSODetRow.ManualPrice = true;
                        // TODO: AC-142850 Check where is assigned the contract price?
                    }

                    if (createAppointmentFlag)
                    {
                        fsSODetRow.Scheduled = true;
                        fsSODetRow.Status = ID.Status_SODet.SCHEDULED;
                    }

                    fsSODetRow.EquipmentAction = fsScheduleDetRow.EquipmentAction;
                    fsSODetRow.SMEquipmentID = fsScheduleDetRow.SMEquipmentID;
                    fsSODetRow.ComponentID = fsScheduleDetRow.ComponentID;
                    fsSODetRow.EquipmentLineRef = fsScheduleDetRow.EquipmentLineRef;
                    fsSODetRow.ProjectTaskID = fsScheduleDetRow.ProjectTaskID;
                    fsSODetRow.CostCodeID = fsScheduleDetRow.CostCodeID;

                    if (fsSODetRow.InventoryID != null)
                    {
                        fsSODetRow.ProjectTaskID = fsScheduleDetRow.ProjectTaskID;
                        fsSODetRow.CostCodeID = fsScheduleDetRow.CostCodeID;
                    }

                    graphServiceOrderEntry.ServiceOrderDetails.Update(fsSODetRow);
                }
            }

            if (fsScheduleRow.VendorID != null)
            {
                FSSOEmployee fsSOEmployeeRow = new FSSOEmployee();
                fsSOEmployeeRow.EmployeeID = fsScheduleRow.VendorID;
                graphServiceOrderEntry.ServiceOrderEmployees.Insert(fsSOEmployeeRow);
            }

            #endregion

            graphServiceOrderEntry.Save.Press();

            if (createAppointmentFlag)
            {
                string origStatus = graphServiceOrderEntry.ServiceOrderRecords.Current.Status;

                if (origStatus != ID.Status_ServiceOrder.OPEN)
                {
                    //Due to FSAppointment.sORefNbr selector, Service Order status must be OPEN to allow the SetValueExt<SORefNbr> inside createAppointment() work properly.
                    //PXUpdate is used to avoid raising any ServiceOrderEntry event.
                    PXUpdate<
                        Set<FSServiceOrder.status, FSServiceOrder.status.Open>,
                    FSServiceOrder,
                    Where<
                        FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                    .Update(this, graphServiceOrderEntry.ServiceOrderRecords.Current.SOID);
                }

                CreateAppointment(graphServiceOrderEntry.ServiceOrderRecords.Current, timeSlotServiceOrder, fsScheduleRow, true, appointmentsBelongToRoute, isPrepaidContract);

                if (origStatus != ID.Status_ServiceOrder.OPEN)
                {
                    PXUpdate<
                        Set<FSServiceOrder.status, Required<FSServiceOrder.status>>,
                    FSServiceOrder,
                    Where<
                        FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                    .Update(this, origStatus, graphServiceOrderEntry.ServiceOrderRecords.Current.SOID);
                }
            }
        }

        protected int? CalculateEstimatedDuration(FSSODet fsSODetRow, decimal? qty)
        {
            decimal estimatedDuration = 0m;

            if (fsSODetRow == null)
            {
                return (int?)estimatedDuration;
            }

            if (fsSODetRow.EstimatedQty != null
                    && fsSODetRow.EstimatedDuration != null)
            {
                estimatedDuration = decimal.Divide((decimal)fsSODetRow.EstimatedDuration * (decimal)qty, (decimal)fsSODetRow.EstimatedQty);
            }

            return (int?)estimatedDuration;
        }

        /// <summary>
        /// Create a ContractGenerationHistory.
        /// </summary>
        protected void CreateContractGenerationHistory(int nextGenerationID, int scheduleID, DateTime lastProcessDate, DateTime? lastGeneratedElementDate, string recordType)
        {
            var graphContractGenerationHistory = PXGraph.CreateInstance<ContractGenerationHistoryMaint>();

            FSContractGenerationHistory fsContractGenerationHistoryRow = new FSContractGenerationHistory();

            fsContractGenerationHistoryRow.GenerationID = nextGenerationID;
            fsContractGenerationHistoryRow.ScheduleID = scheduleID;
            fsContractGenerationHistoryRow.LastProcessedDate = lastProcessDate;
            fsContractGenerationHistoryRow.LastGeneratedElementDate = lastGeneratedElementDate;
            fsContractGenerationHistoryRow.EntityType = ID.Schedule_EntityType.CONTRACT;
            fsContractGenerationHistoryRow.RecordType = recordType;

            FSContractGenerationHistory fsContractGenerationHistoryRowPrevious = GetLastGenerationHistoryRowBySchedule(scheduleID);

            if (fsContractGenerationHistoryRowPrevious != null && fsContractGenerationHistoryRowPrevious.ContractGenerationHistoryID != null)
            {
                fsContractGenerationHistoryRow.PreviousGeneratedElementDate = fsContractGenerationHistoryRowPrevious.LastGeneratedElementDate;
                fsContractGenerationHistoryRow.PreviousProcessedDate = fsContractGenerationHistoryRowPrevious.LastProcessedDate;

                if (lastGeneratedElementDate == null)
                {
                    fsContractGenerationHistoryRow.LastGeneratedElementDate = fsContractGenerationHistoryRow.PreviousGeneratedElementDate;
                }                
            }

            graphContractGenerationHistory.ContractGenerationHistoryRecords.Insert(fsContractGenerationHistoryRow);

            graphContractGenerationHistory.Save.Press();
        }

        /// <summary>
        /// Update an Schedule (lastGeneratedAppointmentDate and lastProcessedDate).
        /// </summary>
        protected void UpdateGeneratedSchedule(int scheduleID, DateTime? toDate, DateTime? lastGeneratedElementDate, FSSchedule fsScheduleRow)
        {
            FSSchedule fsScheduleRowInDB = PXSelect<FSSchedule,
                                           Where<
                                                FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                           .SelectSingleBound(this, null, scheduleID);
            if (fsScheduleRowInDB != null)
            {
                if (lastGeneratedElementDate != null || toDate == null)
                {
                    if (fsScheduleRow != null)
                    {
                        fsScheduleRow.LastGeneratedElementDate = lastGeneratedElementDate;
                        fsScheduleRow.NextExecutionDate = SharedFunctions.GetNextExecution(this.ScheduleSelected.Cache, fsScheduleRow);
                    }

                    PXUpdate<
                        Set<FSSchedule.lastGeneratedElementDate, Required<FSSchedule.lastGeneratedElementDate>,
                        Set<FSSchedule.nextExecutionDate, Required<FSSchedule.nextExecutionDate>>>, FSSchedule,
                    Where<
                        FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                    .Update(this, lastGeneratedElementDate, fsScheduleRow.NextExecutionDate, scheduleID);
                }
            }
        }

        /// <summary>
        /// Return the last FSContractGenerationHistory by Schedule.
        /// </summary>
        public FSContractGenerationHistory GetLastGenerationHistoryRowBySchedule(int scheduleID)
        {
            FSContractGenerationHistory fsContractGenerationHistoryRow = PXSelectGroupBy<FSContractGenerationHistory,
                                                                         Where<
                                                                            FSContractGenerationHistory.scheduleID, Equal<Required<FSContractGenerationHistory.scheduleID>>>,
                                                                         Aggregate<
                                                                            Max<FSContractGenerationHistory.generationID>>>
                                                                         .Select(this, scheduleID);

            if (fsContractGenerationHistoryRow != null && fsContractGenerationHistoryRow.GenerationID != null)
            {
                return fsContractGenerationHistoryRow;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Return the smallest date between schedule EndDate and Process EndDate.
        /// </summary>
        protected DateTime? GetProcessEndDate(Schedule fsScheduleRule, DateTime? toDate)
        {
            FSSchedule fsScheduleRow = PXSelect<FSSchedule,
                                       Where<
                                            FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                       .Select(this, fsScheduleRule.ScheduleID);

            if (fsScheduleRow.EnableExpirationDate == false)
            {
                return toDate.Value;
            }
            else
            {
                return (fsScheduleRow.EndDate != null && fsScheduleRow.EndDate < toDate) ? fsScheduleRow.EndDate : toDate.Value;
            }
        }
        #endregion

        #region ViewCtors
        public class ContractHistoryRecords_View : PXSelectJoinGroupBy<FSContractGenerationHistory,
               InnerJoin<FSProcessIdentity, 
               On<
                   FSProcessIdentity.processID, Equal<FSContractGenerationHistory.generationID>>>,
               Where<
                   FSContractGenerationHistory.recordType, Equal<TRecordType>>,
               Aggregate<
                   GroupBy<FSContractGenerationHistory.generationID>>,
               OrderBy<
                                                    Desc<FSContractGenerationHistory.generationID>>>
        {
            public ContractHistoryRecords_View(PXGraph graph) : base(graph)
            {
            }

            public ContractHistoryRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class ErrorMessageRecords_View : PXSelectReadonly2<FSGenerationLogError,
               InnerJoin<FSSchedule,
               On<
                   FSSchedule.scheduleID, Equal<FSGenerationLogError.scheduleID>>,
               InnerJoin<FSServiceContract,
               On<
                   FSServiceContract.serviceContractID, Equal<FSSchedule.entityID>, And<FSServiceContract.recordType, Equal<FSGenerationLogError.processType>>>>>,
               Where<
                   FSGenerationLogError.ignore, Equal<False>,
               And<
                   FSGenerationLogError.processType, Equal<TRecordType>>>,
               OrderBy<
                                                        Desc<FSGenerationLogError.generationID>>>
        {
            public ErrorMessageRecords_View(PXGraph graph) : base(graph)
            {
            }

            public ErrorMessageRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        #endregion

        #region Filter+Select
        public PXFilter<TFiltering> Filter;
        public PXCancel<TFiltering> Cancel;

        public ContractHistoryRecords_View ContractHistoryRecords;

        public ErrorMessageRecords_View ErrorMessageRecords;

        public PXSelect<FSSchedule,
               Where<
                   FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>> ScheduleSelected;

        public PXSelect<FSServiceContract,
                                Where<
                                    FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>> ServiceContractSelected;

        public PXSelectJoin<FSScheduleDet,
               InnerJoin<FSSchedule,
               On<
                   FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>>,
               Where<
                   FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>,
               OrderBy<
                   Asc<FSScheduleDet.sortOrder>>> ScheduleLinesSelected;

        public PXSelectJoin<FSScheduleDet,
               InnerJoin<FSSchedule,
               On<
                   FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>>,
               Where<
                   FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>,
               And<
                   Where<
                       FSScheduleDet.lineType, Equal<ListField_LineType_ALL.Service_Template>>>>,
               OrderBy<
                   Asc<FSScheduleDet.sortOrder>>> ScheduleTemplatesSelected;

        public PXSelect<FSServiceTemplateDet,
               Where<
                   FSServiceTemplateDet.serviceTemplateID, Equal<Required<FSServiceTemplate.serviceTemplateID>>>> ServiceTemplateSelected;
        #endregion

        #region Base Actions
        public PXAction<TFiltering> ClearAll;

        public virtual void clearAll()
        {
        }

        public PXAction<TFiltering> OpenServiceContractScreenBySchedules;

        public virtual void openServiceContractScreenBySchedules()
        {
        }

        public PXAction<TFiltering> OpenServiceContractScreenByGenerationLogError;

        public virtual void openServiceContractScreenByGenerationLogError()
        {
        }

        public PXAction<TFiltering> OpenScheduleScreenBySchedules;

        public virtual void openScheduleScreenBySchedules()
        {
        }

        public PXAction<TFiltering> OpenScheduleScreenByGenerationLogError;

        public virtual void openScheduleScreenByGenerationLogError()
        {
        }
        #endregion
    }
}
