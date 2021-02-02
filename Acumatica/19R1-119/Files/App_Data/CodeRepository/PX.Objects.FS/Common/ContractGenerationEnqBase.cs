using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.FS.Scheduler;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class ContractGenerationEnqBase<TGraph, TPrimary, TFiltering, TRecordType> : PXGraph<TGraph>
        where TGraph : PX.Data.PXGraph
        where TPrimary : class, PX.Data.IBqlTable, new()
        where TFiltering : class, PX.Data.IBqlTable, new()
        where TRecordType : IConstant<string>, IBqlOperand, new()
    {
        public AppointmentEntry graphAppointmentEntry;
        public ServiceOrderEntry graphServiceOrderEntry;
        public int? nextGenerationID;

        #region Generation Functions
        /// <summary>
        /// Generates Appointments (Routes Contract) or Service Orders (Service Contract) for each TimeSlot in the [scheduleRules] List.
        /// </summary>
        public void generate_APP_SO_UpdateContracts(List<Schedule> scheduleRules, string recordType, DateTime? fromDate, DateTime? toDate, FSSchedule fsScheduleRow)
        {
            var generator = new TimeSlotGenerator();
            DateTime processEndDate = (DateTime)getProcessEndDate(scheduleRules.ElementAt(0), toDate);

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
                try
                {
                    foreach (var timeSlot in timeSlots)
                    {
                        failsOnDate = timeSlot.DateTimeBegin;

                        if (recordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
                        {
                            bool createAppointmnet = fsScheduleRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.APPOINTMENT;

                            createServiceOrder(timeSlot, createAppointmnet);
                        }
                        else if (recordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
                        {
                            createServiceOrder(timeSlot, true, true);
                        }
                    }

                    DateTime? lastGenerationDate = null;

                    if (timeSlots.Count > 0)
                    {
                        lastGenerationDate = timeSlots.Max(a => a.DateTimeBegin);
                    }

                    createContractGenerationHistory(
                                                    (int)nextGenerationID,
                                                    scheduleRules.ElementAt(0).ScheduleID,
                                                    processEndDate,
                                                    lastGenerationDate,
                                                    recordType);

                    updateGeneratedSchedule(scheduleRules.ElementAt(0).ScheduleID, processEndDate, lastGenerationDate, fsScheduleRow);
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
        protected void createAppointment(
            FSServiceOrder fsServiceOrderRow, 
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
                 fsScheduleRouteRow = 
                    PXSelect<FSScheduleRoute,
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
                DateTime? scheduledDateTimeBegin = SharedFunctions.GetCustomDateTime(timeSlotAppointment.DateTimeBegin, fsScheduleRow.ScheduleStartTime);
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

            foreach (FSSODetService fsSODetRow in graphServiceOrderEntry.ServiceOrderDetServices.Select())
            {
                var fsAppointmentDetRow = new FSAppointmentDetService();

                fsAppointmentDetRow.ScheduleID = fsSODetRow.ScheduleID;
                fsAppointmentDetRow.ScheduleDetID = fsSODetRow.ScheduleDetID;

                AppointmentEntry.InsertServicePartLine<FSAppointmentDetService, FSSODetService>(
                                                            graphAppointmentEntry.AppointmentDetServices.Cache,
                                                            fsAppointmentDetRow,
                                                            graphServiceOrderEntry.ServiceOrderDetServices.Cache,
                                                            fsSODetRow,
                                                            fsSODetRow.NoteID,
                                                            fsSODetRow.SODetID,
                                                            copyTranDate: false,
                                                            tranDate: fsSODetRow.TranDate,
                                                            SetValuesAfterAssigningSODetID: false,
                                                            copyingFromQuote: false);
            }

            foreach (FSSODetPart fsSODetRow in graphServiceOrderEntry.ServiceOrderDetParts.Select())
            {
                var fsAppointmentDetRow = new FSAppointmentDetPart();

                fsAppointmentDetRow.ScheduleID = fsSODetRow.ScheduleID;
                fsAppointmentDetRow.ScheduleDetID = fsSODetRow.ScheduleDetID;

                AppointmentEntry.InsertServicePartLine<FSAppointmentDetPart, FSSODetPart>(
                                                            graphAppointmentEntry.AppointmentDetParts.Cache,
                                                            fsAppointmentDetRow,
                                                            graphServiceOrderEntry.ServiceOrderDetParts.Cache,
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
                graphAppointmentEntry.AppointmentEmployees.Insert(fsAppointmentEmployeeRow);
            }
            #endregion

            if (fsScheduleRow.VendorID != null)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();
                fsAppointmentEmployeeRow.EmployeeID = fsScheduleRow.VendorID;
                graphAppointmentEntry.AppointmentEmployees.Insert(fsAppointmentEmployeeRow);
            }

            if (insertingFromServiceOrder == true)
            {
                graphAppointmentEntry.SkipServiceOrderUpdate = true;
            }

            try
            {
                graphAppointmentEntry.Save.Press();
            }
            finally
            {
                graphAppointmentEntry.SkipServiceOrderUpdate = false;
            }
        }

        /// <summary>
        /// Create a Service Order from a TimeSlot.
        /// </summary>
        protected void createServiceOrder(TimeSlot timeSlotServiceOrder, bool createAppointmentFlag = false, bool appointmentsBelongToRoute = false)
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
            var fsScheduleDetServiceSet = ScheduleServicesSelected.Select(timeSlotServiceOrder.ScheduleID);

            //Parts from the Schedule
            var fsScheduleDetPartSet = SchedulePartsSelected.Select(timeSlotServiceOrder.ScheduleID);

            //Templates from the Schedule
            var fsScheduleDetTemplateSet = ScheduleTemplatesSelected.Select(timeSlotServiceOrder.ScheduleID);
            
            FSServiceOrder fsServiceOrderRow = new FSServiceOrder();
            fsServiceOrderRow.SrvOrdType = fsScheduleRow.SrvOrdType;
            
            fsServiceOrderRow = graphServiceOrderEntry.ServiceOrderRecords.Insert(fsServiceOrderRow);
            graphServiceOrderEntry.ServiceOrderRecords.Current.SrvOrdType = fsScheduleRow.SrvOrdType;

            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.branchID>(graphServiceOrderEntry.ServiceOrderRecords.Current, fsScheduleRow.BranchID);
            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.branchLocationID>(graphServiceOrderEntry.ServiceOrderRecords.Current, fsScheduleRow.BranchLocationID);

            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.orderDate>(graphServiceOrderEntry.ServiceOrderRecords.Current, timeSlotServiceOrder.DateTimeBegin);
            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.promisedDate>(graphServiceOrderEntry.ServiceOrderRecords.Current, timeSlotServiceOrder.DateTimeBegin);

            //It has to be handled this way because SetValueExt is not triggering the events nor assigning the correct value
            graphServiceOrderEntry.ServiceOrderRecords.Current.CustomerID = fsServiceContractRow.CustomerID;
            graphServiceOrderEntry.ServiceOrderRecords.Cache.RaiseFieldUpdated<FSServiceOrder.customerID>(graphServiceOrderEntry.ServiceOrderRecords.Current, null);
            graphServiceOrderEntry.ServiceOrderRecords.Current.LocationID = fsScheduleRow.CustomerLocationID;
            graphServiceOrderEntry.ServiceOrderRecords.Cache.RaiseFieldUpdated<FSServiceOrder.locationID>(graphServiceOrderEntry.ServiceOrderRecords.Current, null);

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

                graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.curyID>(fsServiceOrderRow, curyID);
            }

            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.docDesc>(fsServiceOrderRow, timeSlotServiceOrder.Descr);

            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.billCustomerID>(fsServiceOrderRow, fsServiceContractRow.BillCustomerID);
            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.billLocationID>(fsServiceOrderRow, fsServiceContractRow.BillLocationID);
            graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.serviceContractID>(fsServiceOrderRow, fsServiceContractRow.ServiceContractID);

            if (isPrepaidContract == true 
                    && graphServiceOrderEntry.BillingCycleRelated.Current != null 
                    && graphServiceOrderEntry.BillingCycleRelated.Current.BillingBy == ID.Billing_By.SERVICE_ORDER)
            {
                graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.billServiceContractID>(fsServiceOrderRow, fsServiceContractRow.ServiceContractID);
            }

            fsServiceOrderRow = graphServiceOrderEntry.ServiceOrderRecords.Current;

            fsServiceOrderRow.ScheduleID = timeSlotServiceOrder.ScheduleID;
            fsServiceOrderRow.ServiceContractID = fsServiceContractRow.ServiceContractID;
            fsServiceOrderRow.GenerationID = timeSlotServiceOrder.GenerationID;

            fsServiceOrderRow.SalesPersonID = fsServiceContractRow.SalesPersonID;
            fsServiceOrderRow.Commissionable = fsServiceContractRow.Commissionable;

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
            foreach (FSScheduleDetService fsScheduleDetServiceRow in fsScheduleDetServiceSet)
            {
                if (fsScheduleDetServiceRow.LineType == ID.LineType_ServiceContract.SERVICE_TEMPLATE)
                {
                    foreach (FSScheduleDet fsScheduleDetRow in fsScheduleDetTemplateSet.Where(x => ((FSScheduleDet)x).ServiceTemplateID == fsScheduleDetServiceRow.ServiceTemplateID))
                    {
                        var fsServiceTemplateDetSet_Service = ServiceTemplateSelected.Select(fsScheduleDetServiceRow.ServiceTemplateID);
                            
                        foreach (FSServiceTemplateDet fsServiceTemplateDetRow_Service in fsServiceTemplateDetSet_Service)
                        {
                            FSSODetService fsSODetServiceRow = new FSSODetService();

                            // Insert the new row with key fields
                            fsSODetServiceRow.ScheduleID = fsScheduleDetRow.ScheduleID;
                            fsSODetServiceRow.ScheduleDetID = fsScheduleDetRow.ScheduleDetID;
                            fsSODetServiceRow.LineType = fsServiceTemplateDetRow_Service.LineType;
                            fsSODetServiceRow = graphServiceOrderEntry.ServiceOrderDetServices.Insert(fsSODetServiceRow);

                            fsSODetServiceRow.InventoryID = fsServiceTemplateDetRow_Service.InventoryID;

                            fsSODetServiceRow = graphServiceOrderEntry.ServiceOrderDetServices.Update(fsSODetServiceRow);

                            PXNoteAttribute.CopyNoteAndFiles(
                                                            ScheduleServicesSelected.Cache,
                                                            fsScheduleDetRow,
                                                            graphServiceOrderEntry.ServiceOrderDetServices.Cache,
                                                            fsSODetServiceRow,
                                                            copyNotes: true,
                                                            copyFiles: true);

                            // Create a copy to update the other fields
                            fsSODetServiceRow = (FSSODetService)graphServiceOrderEntry.ServiceOrderDetServices.Cache.CreateCopy(fsSODetServiceRow);

                            fsSODetServiceRow.TranDesc = fsServiceTemplateDetRow_Service.TranDesc;
                            
                            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(this, fsServiceTemplateDetRow_Service.InventoryID);

                            if (inventoryItemRow != null)
                            {
                                FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

                                if (fsxServiceRow != null && fsxServiceRow.BillingRule == ID.BillingRule.TIME)
                                {
                                    int? estimatedDuration = CalculateEstimatedDuration(fsSODetServiceRow, fsServiceTemplateDetRow_Service.Qty);
                                    fsSODetServiceRow.EstimatedDuration = estimatedDuration;
                                }
                                else
                                {
                                    fsSODetServiceRow.EstimatedQty = fsServiceTemplateDetRow_Service.Qty;
                                }
                            }

                            if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                            {
                                fsSODetServiceRow.ManualPrice = true;
                                // TODO: Check where is assigned the contract price?
                            }

                            if (createAppointmentFlag)
                            {
                                fsSODetServiceRow.Scheduled = true;
                            }

                            fsSODetServiceRow.EquipmentAction = fsScheduleDetServiceRow.EquipmentAction;
                            fsSODetServiceRow.SMEquipmentID = fsScheduleDetServiceRow.SMEquipmentID;
                            fsSODetServiceRow.ComponentID = fsScheduleDetServiceRow.ComponentID;
                            fsSODetServiceRow.EquipmentLineRef = fsScheduleDetServiceRow.EquipmentLineRef;

                            graphServiceOrderEntry.ServiceOrderDetServices.Update(fsSODetServiceRow);
                        }                        

                        var fsServiceTemplateDetSet_Part = PartTemplateSelected.Select(fsScheduleDetServiceRow.ServiceTemplateID);

                        foreach (FSServiceTemplateDet fsServiceTemplateDetRow_Part in fsServiceTemplateDetSet_Part)
                        {
                            FSSODetPart fsSODetPartRow = new FSSODetPart();

                            // Insert the new row with key fields
                            fsSODetPartRow.ScheduleID = fsScheduleDetRow.ScheduleID;
                            fsSODetPartRow.ScheduleDetID = fsScheduleDetRow.ScheduleDetID;
                            fsSODetPartRow.LineType = fsServiceTemplateDetRow_Part.LineType;
                            fsSODetPartRow.InventoryID = fsServiceTemplateDetRow_Part.InventoryID;
                            fsSODetPartRow = graphServiceOrderEntry.ServiceOrderDetParts.Insert(fsSODetPartRow);

                            PXNoteAttribute.CopyNoteAndFiles(
                                                            ScheduleServicesSelected.Cache,
                                                            fsScheduleDetRow,
                                                            graphServiceOrderEntry.ServiceOrderDetParts.Cache,
                                                            fsSODetPartRow,
                                                            copyNotes: true,
                                                            copyFiles: true);

                            // Create a copy to update the other fields
                            fsSODetPartRow = (FSSODetPart)graphServiceOrderEntry.ServiceOrderDetParts.Cache.CreateCopy(fsSODetPartRow);

                            fsSODetPartRow.TranDesc = fsServiceTemplateDetRow_Part.TranDesc;
                            fsSODetPartRow.EstimatedQty = fsServiceTemplateDetRow_Part.Qty;

                            if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                            {
                                fsSODetPartRow.ManualPrice = true;
                                // TODO: Check where is assigned the contract price?
                            }

                            fsSODetPartRow.EquipmentAction = fsScheduleDetServiceRow.EquipmentAction;
                            fsSODetPartRow.SMEquipmentID = fsScheduleDetServiceRow.SMEquipmentID;
                            fsSODetPartRow.ComponentID = fsScheduleDetServiceRow.ComponentID;
                            fsSODetPartRow.EquipmentLineRef = fsScheduleDetServiceRow.EquipmentLineRef;

                            graphServiceOrderEntry.ServiceOrderDetParts.Update(fsSODetPartRow);
                        }
                    }
                }
                else
                {
                    FSSODetService fsSODetServiceRow = new FSSODetService();

                    // Insert the new row with key fields
                    fsSODetServiceRow.ScheduleID = fsScheduleDetServiceRow.ScheduleID;
                    fsSODetServiceRow.ScheduleDetID = fsScheduleDetServiceRow.ScheduleDetID;
                    fsSODetServiceRow.LineType = fsScheduleDetServiceRow.LineType;
                    fsSODetServiceRow = graphServiceOrderEntry.ServiceOrderDetServices.Insert(fsSODetServiceRow);

                    fsSODetServiceRow.InventoryID = fsScheduleDetServiceRow.InventoryID;

                    fsSODetServiceRow = graphServiceOrderEntry.ServiceOrderDetServices.Update(fsSODetServiceRow);


                    PXNoteAttribute.CopyNoteAndFiles(
                                                    ScheduleServicesSelected.Cache,
                                                    fsScheduleDetServiceRow,
                                                    graphServiceOrderEntry.ServiceOrderDetServices.Cache,
                                                    fsSODetServiceRow,
                                                    copyNotes: true,
                                                    copyFiles: true);

                    // Create a copy to update the other fields
                    fsSODetServiceRow = (FSSODetService)graphServiceOrderEntry.ServiceOrderDetServices.Cache.CreateCopy(fsSODetServiceRow);

                    fsSODetServiceRow.TranDesc = fsScheduleDetServiceRow.TranDesc;
                    fsSODetServiceRow.BillingRule = fsScheduleDetServiceRow.BillingRule;

                    if (fsSODetServiceRow.BillingRule == ID.BillingRule.TIME)
                    {
                        int? estimatedDuration = CalculateEstimatedDuration(fsSODetServiceRow, fsScheduleDetServiceRow?.Qty);
                        fsSODetServiceRow.EstimatedDuration = estimatedDuration;
                    }
                    else
                    {
                        fsSODetServiceRow.EstimatedQty = fsScheduleDetServiceRow.Qty;
                    }

                    if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                    {
                        fsSODetServiceRow.ManualPrice = true;
                        // TODO: Check where is assigned the contract price?
                    }

                    if (createAppointmentFlag)
                    {
                        fsSODetServiceRow.Scheduled = true;
                    }

                    fsSODetServiceRow.EquipmentAction = fsScheduleDetServiceRow.EquipmentAction;
                    fsSODetServiceRow.SMEquipmentID = fsScheduleDetServiceRow.SMEquipmentID;
                    fsSODetServiceRow.ComponentID = fsScheduleDetServiceRow.ComponentID;
                    fsSODetServiceRow.EquipmentLineRef = fsScheduleDetServiceRow.EquipmentLineRef;

                    graphServiceOrderEntry.ServiceOrderDetServices.Update(fsSODetServiceRow);
                }
            }

            foreach (FSScheduleDetPart fsScheduleDetPartRow in fsScheduleDetPartSet)
            {
                FSSODetPart fsSODetPartRow = new FSSODetPart();

                // Insert the new row with key fields
                fsSODetPartRow.ScheduleID = fsScheduleDetPartRow.ScheduleID;
                fsSODetPartRow.ScheduleDetID = fsScheduleDetPartRow.ScheduleDetID;
                fsSODetPartRow.LineType = fsScheduleDetPartRow.LineType;
                fsSODetPartRow.InventoryID = fsScheduleDetPartRow.InventoryID;
                fsSODetPartRow = graphServiceOrderEntry.ServiceOrderDetParts.Insert(fsSODetPartRow);

                PXNoteAttribute.CopyNoteAndFiles(
                                                SchedulePartsSelected.Cache,
                                                fsScheduleDetPartRow,
                                                graphServiceOrderEntry.ServiceOrderDetParts.Cache,
                                                fsSODetPartRow,
                                                copyNotes: true,
                                                copyFiles: true);

                // Create a copy to update the other fields
                fsSODetPartRow = (FSSODetPart)graphServiceOrderEntry.ServiceOrderDetParts.Cache.CreateCopy(fsSODetPartRow);

                fsSODetPartRow.TranDesc = fsScheduleDetPartRow.TranDesc;
                fsSODetPartRow.EstimatedQty = fsScheduleDetPartRow.Qty;

                if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                {
                    fsSODetPartRow.ManualPrice = true;
                    // TODO: Check where is assigned the contract price?
                }

                fsSODetPartRow.EquipmentAction = fsScheduleDetPartRow.EquipmentAction;
                fsSODetPartRow.SMEquipmentID = fsScheduleDetPartRow.SMEquipmentID;
                fsSODetPartRow.ComponentID = fsScheduleDetPartRow.ComponentID;
                fsSODetPartRow.EquipmentLineRef = fsScheduleDetPartRow.EquipmentLineRef;

                graphServiceOrderEntry.ServiceOrderDetParts.Update(fsSODetPartRow);
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
                    PXUpdate<Set<FSServiceOrder.status, FSServiceOrder.status.Open>,
                        FSServiceOrder,
                        Where<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                    .Update(this, graphServiceOrderEntry.ServiceOrderRecords.Current.SOID);
                }

                createAppointment(graphServiceOrderEntry.ServiceOrderRecords.Current, timeSlotServiceOrder, fsScheduleRow, true, appointmentsBelongToRoute, isPrepaidContract);

                if (origStatus != ID.Status_ServiceOrder.OPEN)
                {
                    PXUpdate<Set<FSServiceOrder.status, Required<FSServiceOrder.status>>,
                        FSServiceOrder,
                        Where<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                    .Update(this, origStatus, graphServiceOrderEntry.ServiceOrderRecords.Current.SOID);
                }
            }
        }

        protected int? CalculateEstimatedDuration(FSSODetService fsSODetServiceRow, decimal? qty)
        {
            decimal estimatedDuration = 0m;

            if (fsSODetServiceRow == null)
            {
                return (int?)estimatedDuration;
            }

            if (fsSODetServiceRow.EstimatedQty != null
                    && fsSODetServiceRow.EstimatedDuration != null)
            {
                estimatedDuration = decimal.Divide((decimal)fsSODetServiceRow.EstimatedDuration * (decimal)qty, (decimal)fsSODetServiceRow.EstimatedQty);
            }

            return (int?)estimatedDuration;
        }

        /// <summary>
        /// Create a ContractGenerationHistory.
        /// </summary>
        protected void createContractGenerationHistory(int nextGenerationID, int scheduleID, DateTime lastProcessDate, DateTime? lastGeneratedElementDate, string recordType)
        {
            var graphContractGenerationHistory = PXGraph.CreateInstance<ContractGenerationHistoryMaint>();

            FSContractGenerationHistory fsContractGenerationHistoryRow = new FSContractGenerationHistory();

            fsContractGenerationHistoryRow.GenerationID = nextGenerationID;
            fsContractGenerationHistoryRow.ScheduleID = scheduleID;
            fsContractGenerationHistoryRow.LastProcessedDate = lastProcessDate;
            fsContractGenerationHistoryRow.LastGeneratedElementDate = lastGeneratedElementDate;
            fsContractGenerationHistoryRow.EntityType = ID.Schedule_EntityType.CONTRACT;
            fsContractGenerationHistoryRow.RecordType = recordType;

            FSContractGenerationHistory fsContractGenerationHistoryRow_Previous = getLastGenerationHistoryRowBySchedule(scheduleID);

            if (fsContractGenerationHistoryRow_Previous != null && fsContractGenerationHistoryRow_Previous.ContractGenerationHistoryID != null)
            {
                fsContractGenerationHistoryRow.PreviousGeneratedElementDate = fsContractGenerationHistoryRow_Previous.LastGeneratedElementDate;
                fsContractGenerationHistoryRow.PreviousProcessedDate = fsContractGenerationHistoryRow_Previous.LastProcessedDate;

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
        protected void updateGeneratedSchedule(int scheduleID, DateTime? toDate, DateTime? lastGeneratedElementDate, FSSchedule fsScheduleRow)
        {
            FSSchedule fsScheduleRow_InDB = PXSelect<FSSchedule,
                                            Where<
                                                FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                            .SelectSingleBound(this, null, scheduleID);
            if (fsScheduleRow_InDB != null)
            {
                if (lastGeneratedElementDate != null || toDate == null)
                {
                    if (fsScheduleRow != null)
                    {
                        fsScheduleRow.LastGeneratedElementDate = lastGeneratedElementDate;
                        fsScheduleRow.NextExecutionDate = SharedFunctions.GetNextExecution(this.ScheduleSelected.Cache, fsScheduleRow);
                    }

                    PXUpdate<Set<FSSchedule.lastGeneratedElementDate,
                                Required<FSSchedule.lastGeneratedElementDate>,
                            Set<FSSchedule.nextExecutionDate,
                                Required<FSSchedule.nextExecutionDate>>>, FSSchedule,
                            Where<FSSchedule.scheduleID,
                                Equal<Required<FSSchedule.scheduleID>>>>
                            .Update(this, lastGeneratedElementDate, fsScheduleRow.NextExecutionDate, scheduleID);
                }
            }
        }

        /// <summary>
        /// Return the last FSContractGenerationHistory by Schedule.
        /// </summary>
        public FSContractGenerationHistory getLastGenerationHistoryRowBySchedule(int scheduleID)
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
        protected DateTime? getProcessEndDate(Schedule fsScheduleRule, DateTime? toDate)
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

        #region Filter+Select
        public PXFilter<TFiltering> Filter;
        public PXCancel<TFiltering> Cancel;

        public PXSelectJoinGroupBy<FSContractGenerationHistory,
                                InnerJoin<FSProcessIdentity, 
                                    On<FSProcessIdentity.processID, Equal<FSContractGenerationHistory.generationID>>>,
                                Where<
                                    FSContractGenerationHistory.recordType, Equal<TRecordType>>,
                                Aggregate<
                                    GroupBy<FSContractGenerationHistory.generationID>>,
                                OrderBy<
                                    Desc<FSContractGenerationHistory.generationID>>> ContractHistoryRecords;

        public PXSelectReadonly2<FSGenerationLogError,
                                InnerJoin<FSSchedule,
                                    On<FSSchedule.scheduleID, Equal<FSGenerationLogError.scheduleID>>,
                                InnerJoin<FSServiceContract,
                                    On<FSServiceContract.serviceContractID, Equal<FSSchedule.entityID>, And<FSServiceContract.recordType, Equal<FSGenerationLogError.processType>>>>>,
                                Where<
                                    FSGenerationLogError.ignore, Equal<False>,
                                And<
                                    FSGenerationLogError.processType, Equal<TRecordType>>>,
                                OrderBy<
                                    Desc<FSGenerationLogError.generationID>>> ErrorMessageRecords;

        public PXSelect<FSSchedule,
                                Where<
                                    FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>> ScheduleSelected;

        public PXSelect<FSServiceContract,
                                Where<
                                    FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>> ServiceContractSelected;

        public PXSelectJoin<FSScheduleDetService,
                                InnerJoin<FSSchedule,
                                    On<FSSchedule.scheduleID, Equal<FSScheduleDetService.scheduleID>>>,
                                Where<
                                    FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>> ScheduleServicesSelected;

        public PXSelectJoin<FSScheduleDetPart,
                                InnerJoin<FSSchedule,
                                    On<FSSchedule.scheduleID, Equal<FSScheduleDetPart.scheduleID>>>,
                                Where<
                                    FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>> SchedulePartsSelected;

        public PXSelectJoin<FSScheduleDetService,
                                InnerJoin<FSSchedule,
                                    On<FSSchedule.scheduleID, Equal<FSScheduleDetService.scheduleID>>>,
                                Where<
                                    FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>,
                                And<
                                    Where<
                                        FSScheduleDetService.lineType, Equal<ListField_LineType_Service_ServiceContract.Service_Template>>>>> ScheduleTemplatesSelected;

        public PXSelect<FSServiceTemplateDet,
                                 Where<
                                    FSServiceTemplateDet.serviceTemplateID, Equal<Required<FSServiceTemplate.serviceTemplateID>>,
                                And<
                                    Where2<
                                        Where<
                                            FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceContract.Service>>,
                                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceContract.Comment_Service>,
                                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceContract.Instruction_Service>,
                                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceContract.NonStockItem>>>>>>>> ServiceTemplateSelected;

        public PXSelect<FSServiceTemplateDet,
                                Where<
                                    FSServiceTemplateDet.serviceTemplateID, Equal<Required<FSServiceTemplate.serviceTemplateID>>,
                                And<
                                    Where2<
                                        Where<
                                            FSServiceTemplateDet.lineType, Equal<ListField_LineType_Part_ALL.Inventory_Item>>,
                                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Part_ALL.Comment_Part>,
                                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Part_ALL.Instruction_Part>>>>>>> PartTemplateSelected;
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
