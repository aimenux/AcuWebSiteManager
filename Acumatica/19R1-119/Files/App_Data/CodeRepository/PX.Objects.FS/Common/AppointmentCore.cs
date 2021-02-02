using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public static class AppointmentCore
    {
        #region Selects

        public class AppointmentRecords_View : PXSelectJoin<FSAppointment,
                LeftJoin<FSServiceOrder,
                    On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>,
                Where2<
                                                Where<
                        FSAppointment.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>>,
                    And<Where<
                        Customer.bAccountID, IsNull,
                        Or<Match<Customer, Current<AccessInfo.userName>>>>>>>
        {
            public AppointmentRecords_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class AppointmentSelected_View : PXSelect<FSAppointment,
                                                Where<
                                                    FSAppointment.appointmentID, Equal<Current<FSAppointment.appointmentID>>>>
        {
            public AppointmentSelected_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentSelected_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class AppointmentDetServices_View : PXSelectJoin<FSAppointmentDetService,
                                                    LeftJoin<FSSODet,
                                                       On<FSAppointmentDetService.sODetID, Equal<FSSODet.sODetID>>,
                                                    LeftJoin<FSPostInfo,
                                                       On<FSPostInfo.postID, Equal<FSAppointmentDetService.postID>>>>,
                                                    Where<
                                                        FSAppointmentDetService.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                                                        And<Where<
                                                            FSAppointmentDetService.lineType, Equal<FSAppointmentDetService.lineType.Service>,                                                                                                                        
                                                            Or<FSAppointmentDetService.lineType, Equal<FSAppointmentDetService.lineType.Comment_Service>,
                                                            Or<FSAppointmentDetService.lineType, Equal<FSAppointmentDetService.lineType.Instruction_Service>,
                                                            Or<FSAppointmentDetService.lineType, Equal<FSAppointmentDetService.lineType.NonStockItem>>>>>>>>
        {
            public AppointmentDetServices_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentDetServices_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class AppointmentDetParts_View : PXSelectJoin<FSAppointmentDetPart,
                                                LeftJoin<FSSODet,
                                                       On<FSAppointmentDetPart.sODetID, Equal<FSSODet.sODetID>>,
                                                LeftJoin<FSPostInfo,
                                                    On<FSPostInfo.postID, Equal<FSAppointmentDetPart.postID>>>>,
                                                Where<
                                                    FSAppointmentDetPart.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                                                And<
                                                    Where<
                                                        FSAppointmentDetPart.lineType, Equal<FSAppointmentDetPart.lineType.Inventory_Item>,
                                                        Or<FSAppointmentDetPart.lineType, Equal<FSAppointmentDetPart.lineType.Comment_Part>,
                                                        Or<FSAppointmentDetPart.lineType, Equal<FSAppointmentDetPart.lineType.Instruction_Part>>>>>>>                                                    

        //@TODO:SD-5764 (Sale product and Consumable)
        {
            public AppointmentDetParts_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentDetParts_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class AppointmentEmployees_View : PXSelectJoin<FSAppointmentEmployee,
                                                        LeftJoin<BAccount,
                                                            On<
                                                                FSAppointmentEmployee.employeeID, Equal<BAccount.bAccountID>>,
                                                    LeftJoin<FSAppointmentDetEmployee,
                                                                On<
                                                                    FSAppointmentDetEmployee.lineRef, Equal<FSAppointmentEmployee.serviceLineRef>,
                                                                    And<FSAppointmentDetEmployee.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>>>>,
                                                        Where<
                                                                FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                                                                And<
                                                                    Where<FSAppointmentEmployee.serviceLineRef, IsNull,
                                                                        Or<FSAppointmentDetEmployee.lineType, Equal<FSAppointmentDet.lineType.Service>>>>>,
                                                        OrderBy<
                                                                Asc<FSAppointmentEmployee.lineRef>>>
        {
            public AppointmentEmployees_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentEmployees_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class AppointmentResources_View : PXSelectJoin<FSAppointmentResource,
                                                        LeftJoin<FSEquipment,
                                                                On<FSEquipment.SMequipmentID, Equal<FSAppointmentResource.SMequipmentID>>>,
                                                    Where<
                                                        FSAppointmentResource.appointmentID, Equal<Current<FSAppointment.appointmentID>>>>
        {
            public AppointmentResources_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentResources_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class AppointmentAttendees_View : PXSelect<FSAppointmentAttendee,
                                                    Where<
                                                        FSAppointmentAttendee.appointmentID, Equal<Current<FSAppointment.appointmentID>>>>
        {
            public AppointmentAttendees_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentAttendees_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public class AppointmentPickupDeliveryItems_View : PXSelectJoin<FSAppointmentInventoryItem,
                                                            LeftJoin<FSPostInfo,
                                                                On<FSPostInfo.postID, Equal<FSAppointmentInventoryItem.postID>>>,
                                                            Where<
                                                                FSAppointmentInventoryItem.lineType, Equal<FSAppointmentInventoryItem.lineType.Pickup_Delivery>,
                                                                And<FSAppointmentInventoryItem.appointmentID, Equal<Current<FSAppointment.appointmentID>>>>>
        {
            public AppointmentPickupDeliveryItems_View(PXGraph graph)
                : base(graph)
            {
            }

            public AppointmentPickupDeliveryItems_View(PXGraph graph, Delegate handler)
                : base(graph, handler)
            {
            }
        }

        public class ServiceOrderRelated_View : PXSelect<FSServiceOrder,
                                                    Where<
                                                        FSServiceOrder.sOID, Equal<Optional<FSAppointment.sOID>>>>
        {
            public ServiceOrderRelated_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceOrderRelated_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }
        #endregion

        #region Methods

        public static KeyValuePair<string, string>[] GetAppointmentUrlArguments(FSAppointment fsAppointmentRow)
        {
            KeyValuePair<string, string>[] urlArgs = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(typeof(FSAppointment.refNbr).Name, fsAppointmentRow.RefNbr),
                new KeyValuePair<string, string>("Date", fsAppointmentRow.ScheduledDateBegin.ToString()),
                new KeyValuePair<string, string>("AppSource", "1")
            };

            return urlArgs;
        }

        #region EnableDisable
        /// <summary>
        /// Enable / Disable the document depending of the Status of the Service Order [fsServiceOrderRow].
        /// </summary>
        public static void EnableDisable_Document(
                                                  PXGraph graph,
                                                  FSAppointment fsAppointmentRow,
                                                  FSServiceOrder fsServiceOrderRow,
                                                  FSSetup fsSetupRow,
                                                  FSBillingCycle fsBillingCycleRow,
                                                  ServiceOrderRelated_View serviceOrderRelated,
                                                  AppointmentRecords_View appointmentRecords,
                                                  AppointmentSelected_View appointmentSelected,
                                                  AppointmentDetServices_View appointmentDetailService,
                                                  AppointmentDetParts_View appointmentPart,
                                                  AppointmentEmployees_View appointmentEmployees,
                                                  AppointmentResources_View appointmentResources,
                                                  AppointmentAttendees_View appointmentAttendees,
                                                  AppointmentPickupDeliveryItems_View appointmentPickupDeliveryItems,
                                                  ServiceOrderCore.FSContact_View appointment_Contact,
                                                  ServiceOrderCore.FSAddress_View appointment_Address,
                                                  FSSrvOrdType fsSrvOrdTypeRow,
                                                  bool skipTimeCardUpdate,
                                                  bool? isBeingCalledFromQuickProcess)
        {
            bool enablePartsTab = true;
            bool enableServicesTab = true;
            bool enablePickupTab = false;

            if (fsServiceOrderRow != null
                && fsSrvOrdTypeRow != null)
            {              
                if (fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
                {
                    enablePartsTab = fsServiceOrderRow.CustomerID != null && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
                    enableServicesTab = fsServiceOrderRow.CustomerID != null;
                }
            }
            
            bool enableInsertUpdate = CanUpdateAppointment(fsAppointmentRow, fsSrvOrdTypeRow) || skipTimeCardUpdate || (isBeingCalledFromQuickProcess ?? false);
            bool enableDelete = CanDeleteAppointment(fsAppointmentRow, fsServiceOrderRow, fsSrvOrdTypeRow);

            enablePickupTab = IsARouteSrvOrderType(graph, fsAppointmentRow);

            //TODO SD-7592
            // Enable/disable all input controls
            //PXUIFieldAttribute.SetEnabled(AppointmentRecords.Cache, fsAppointmentRow, enableInsertUpdate);
            EnableDisable_ScheduleDateTimes(appointmentRecords.Cache, fsAppointmentRow, enableInsertUpdate && !enablePickupTab);
            EnableDisable_UnreachedCustomer(appointmentRecords.Cache, fsAppointmentRow, enableInsertUpdate);
            EnableDisable_AppointmentActualDateTimes(appointmentRecords.Cache, fsSetupRow, fsAppointmentRow, fsSrvOrdTypeRow);

            if (fsServiceOrderRow != null)
            {
                bool nonProject = ProjectDefaultAttribute.IsNonProject(fsServiceOrderRow.ProjectID);
                PXUIFieldAttribute.SetVisible<FSAppointment.dfltProjectTaskID>(appointmentRecords.Cache, fsAppointmentRow, !nonProject);
                PXUIFieldAttribute.SetEnabled<FSAppointment.dfltProjectTaskID>(appointmentRecords.Cache, fsAppointmentRow, enableInsertUpdate && !nonProject);
                PXUIFieldAttribute.SetRequired<FSAppointment.dfltProjectTaskID>(appointmentRecords.Cache, !nonProject);
                PXDefaultAttribute.SetPersistingCheck<FSAppointment.dfltProjectTaskID>(
                    appointmentRecords.Cache,
                    fsAppointmentRow, 
                    !nonProject ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);   
            }

            // This is needed for Apppointment Closing Screen because the navigation functionality fails there 
            // if the caches for these views are enable/disable.
            if (graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.ROUTE_CLOSING))
            {
                appointmentSelected.Cache.AllowInsert = enableInsertUpdate;
                appointmentSelected.Cache.AllowUpdate = enableInsertUpdate || graph.IsMobile == true;
                appointmentSelected.Cache.AllowDelete = enableDelete;

                appointmentDetailService.Cache.AllowInsert = enableInsertUpdate && enableServicesTab;
                appointmentDetailService.Cache.AllowUpdate = enableInsertUpdate && enableServicesTab;
                appointmentDetailService.Cache.AllowDelete = enableInsertUpdate && enableServicesTab;

                appointmentRecords.Cache.AllowInsert = true;
                appointmentRecords.Cache.AllowUpdate = enableInsertUpdate || graph.IsMobile == true;
                appointmentRecords.Cache.AllowDelete = enableDelete;
            }

            bool isInventoryFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.inventory>();

            // Enable/disable save/insert/delete buttons for all views
            appointmentPart.Cache.AllowInsert = isInventoryFeatureInstalled && enableInsertUpdate && enablePartsTab;
            appointmentPart.Cache.AllowUpdate = isInventoryFeatureInstalled && enableInsertUpdate && enablePartsTab;
            appointmentPart.Cache.AllowDelete = isInventoryFeatureInstalled && enableInsertUpdate && enablePartsTab;

            appointmentEmployees.Cache.AllowInsert = enableInsertUpdate;
            appointmentEmployees.Cache.AllowUpdate = enableInsertUpdate;
            appointmentEmployees.Cache.AllowDelete = enableInsertUpdate;

            appointment_Contact.Cache.AllowInsert = enableInsertUpdate;
            appointment_Contact.Cache.AllowUpdate = enableInsertUpdate;
            appointment_Contact.Cache.AllowDelete = enableInsertUpdate;

            appointment_Address.Cache.AllowInsert = enableInsertUpdate;
            appointment_Address.Cache.AllowUpdate = enableInsertUpdate;
            appointment_Address.Cache.AllowDelete = enableInsertUpdate;

            appointmentResources.Cache.AllowInsert = enableInsertUpdate;
            appointmentResources.Cache.AllowUpdate = enableInsertUpdate;
            appointmentResources.Cache.AllowDelete = enableInsertUpdate;

            appointmentAttendees.Cache.AllowInsert = enableInsertUpdate;
            appointmentAttendees.Cache.AllowUpdate = enableInsertUpdate;
            appointmentAttendees.Cache.AllowDelete = enableInsertUpdate;

            appointmentPickupDeliveryItems.Cache.AllowInsert = enableInsertUpdate && enablePickupTab;
            appointmentPickupDeliveryItems.Cache.AllowUpdate = enableInsertUpdate && enablePickupTab;
            appointmentPickupDeliveryItems.Cache.AllowDelete = enableInsertUpdate && enablePickupTab;

            if (fsAppointmentRow != null)
            {
                PXUIFieldAttribute.SetEnabled<FSAppointment.soRefNbr>(appointmentRecords.Cache, fsAppointmentRow, fsAppointmentRow.SOID != null && fsAppointmentRow.SOID < 0);
            }

            PXUIFieldAttribute.SetEnabled<FSAppointment.routeDocumentID>(appointmentRecords.Cache, fsAppointmentRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointment.executionDate>(appointmentRecords.Cache, fsAppointmentRow, enableInsertUpdate);

            bool enableHold = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow)
                                                    || fsAppointmentRow.Status == ID.Status_Appointment.ON_HOLD;
            enableHold = enableHold 
                            && (fsAppointmentRow.BillServiceContractID == null 
                                    || (fsAppointmentRow.BillServiceContractID != null && fsAppointmentRow.BillContractPeriodID != null));

            PXUIFieldAttribute.SetEnabled<FSAppointment.hold>(appointmentRecords.Cache, fsAppointmentRow, enableHold);

            bool enableServiceContractFields = fsBillingCycleRow != null
                                                    && fsBillingCycleRow.BillingBy == ID.Billing_By.APPOINTMENT
                                                    && (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() 
                                                        || PXAccess.FeatureInstalled<FeaturesSet.routeManagementModule>());

            PXUIFieldAttribute.SetEnabled<FSAppointment.billServiceContractID>(appointmentRecords.Cache, fsAppointmentRow, enableServiceContractFields);
            PXUIFieldAttribute.SetVisible<FSAppointment.billServiceContractID>(appointmentRecords.Cache, fsAppointmentRow, enableServiceContractFields);
            PXUIFieldAttribute.SetVisible<FSAppointment.billContractPeriodID>(appointmentRecords.Cache, fsAppointmentRow, enableServiceContractFields && fsAppointmentRow.BillServiceContractID != null);
        }

        private static void EnableDisable_AppointmentActualDateTimes(PXCache appointmentCache, FSSetup fsSetupRow, FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (fsSetupRow == null
                    || fsAppointmentRow == null
                        || fsSrvOrdTypeRow == null)
            {
                return;
            }


            bool enableActualStartDateTime = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false
            											&& fsAppointmentRow.Status != ID.Status_Appointment.ON_HOLD;

            bool enableActualEndDateTime = enableActualStartDateTime
                                                && fsAppointmentRow.ActualDateTimeBegin.HasValue;

            PXUIFieldAttribute.SetEnabled<FSAppointment.actualDateTimeBegin>(appointmentCache, fsAppointmentRow, enableActualStartDateTime);
            PXUIFieldAttribute.SetEnabled<FSAppointment.actualDateTimeEnd>(appointmentCache, fsAppointmentRow, enableActualEndDateTime);

            // TODO:
            // RowSelected is not the place/event to assign any value.
            // If you change any value here, the user should be warned using a PXSetPropertyException.
            if (enableActualStartDateTime == false) 
            {
                fsAppointmentRow.ActualDateTimeBegin = null;
            }

            if (enableActualEndDateTime == false) 
            {
                fsAppointmentRow.ActualDateTimeEnd = null;
            }
        }

        public static void EnableDisable_ServiceActualDateTimes(
                                                                PXCache appointmentDetCache, 
                                                                FSAppointment fsAppointmentRow, 
                                                                FSAppointmentDet fsAppointmentDetRow,
                                                                FSSetup fsSetupRow,
                                                                bool enableByLineType)
        {
            if (fsAppointmentRow == null
                    || fsAppointmentDetRow == null)
            {
                return;
            }

            //Grouping conditions that affect Service Actual Date Time's enable/disable behavior.

            //Enable by Time Behavior
            bool enableByTimeBehavior = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false;

            //Enable by Staff Related
            bool enableByStaffRelated = fsAppointmentDetRow.StaffRelatedCount < 2;

            bool enableActualStartDateTime = enableByLineType
                                                && enableByTimeBehavior
                                                    && enableByStaffRelated;

            bool enableActualEndDateTime = fsAppointmentRow.ActualDateTimeBegin.HasValue
                                                && fsAppointmentDetRow.ActualDateTimeBegin.HasValue
                                                    && enableActualStartDateTime;

            bool enableActualDuration = enableByLineType && enableByStaffRelated && enableByTimeBehavior;

            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualDuration>(appointmentDetCache, fsAppointmentDetRow, enableActualDuration);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualDateTimeBegin>(appointmentDetCache, fsAppointmentDetRow, enableActualStartDateTime);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualDateTimeEnd>(appointmentDetCache, fsAppointmentDetRow, enableActualEndDateTime);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.keepActualDateTimes>(appointmentDetCache, fsAppointmentDetRow, enableActualStartDateTime);
        }

        public static void EnableDisable_TimeRelatedFields(
                                                              PXCache appointmentEmployeeCache,
                                                              FSSetup fsSetupRow,
                                                              FSSrvOrdType fsSrvOrdType,
                                                              FSAppointment fsAppointmentRow,
                                                              FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentRow == null
                    || fsAppointmentEmployeeRow == null
                    || fsSetupRow == null)
            {
                return;
            }

            bool enableByTimeBehavior = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false;

            bool enableActualStartDateTime = enableByTimeBehavior;

            bool enableActualEndDateTime = fsAppointmentRow.ActualDateTimeBegin.HasValue
                                                && fsAppointmentEmployeeRow.ActualDateTimeBegin.HasValue
                                                    && enableActualStartDateTime;

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.actualDateTimeBegin>
                    (appointmentEmployeeCache, fsAppointmentEmployeeRow, enableActualStartDateTime);

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.actualDateTimeEnd>
                    (appointmentEmployeeCache, fsAppointmentEmployeeRow, enableActualEndDateTime);

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.actualDuration>
                    (appointmentEmployeeCache, fsAppointmentEmployeeRow, enableActualStartDateTime);

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.trackTime>(
                    appointmentEmployeeCache,
                    fsAppointmentEmployeeRow,
                    (bool)fsSetupRow.EnableEmpTimeCardIntegration
                        && (bool)fsSrvOrdType.CreateTimeActivitiesFromAppointment
                        && fsAppointmentEmployeeRow.Type == BAccountType.EmployeeType
                        && fsAppointmentEmployeeRow.EmployeeID != null);

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.earningType>(
                    appointmentEmployeeCache,
                    fsAppointmentEmployeeRow,
                    (bool)fsSetupRow.EnableEmpTimeCardIntegration
                        && (bool)fsSrvOrdType.CreateTimeActivitiesFromAppointment
                        && fsAppointmentEmployeeRow.Type == BAccountType.EmployeeType
                        && fsAppointmentEmployeeRow.EmployeeID != null);
        }

        public static void SetVisible_TimeRelatedFields(
                                                              PXCache appointmentEmployeeCache,
                                                              FSSrvOrdType fsSrvOrdType,
                                                              FSAppointment fsAppointmentRow,
                                                              FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            PXUIFieldAttribute.SetVisible<FSAppointmentEmployee.trackTime>(
                    appointmentEmployeeCache,
                    null,
                    (bool)fsSrvOrdType.CreateTimeActivitiesFromAppointment);

            PXUIFieldAttribute.SetVisible<FSAppointmentEmployee.earningType>(
                    appointmentEmployeeCache,
                    null,
                    (bool)fsSrvOrdType.CreateTimeActivitiesFromAppointment);
        }

        public static void SetPersisting_TimeRelatedFields(
                                                              PXCache appointmentEmployeeCache,
                                                              FSSetup fsSetupRow,
                                                              FSSrvOrdType fsSrvOrdType,
                                                              FSAppointment fsAppointmentRow,
                                                              FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsSetupRow == null)
            {
                throw new PXException(TX.Error.SETUP_NOT_DEFINED);
            }

            bool isTAIntegrationActive = (bool)fsSetupRow.EnableEmpTimeCardIntegration && (bool)fsSrvOrdType.CreateTimeActivitiesFromAppointment;

            PXDefaultAttribute.SetPersistingCheck<FSAppointmentEmployee.trackTime>(
                appointmentEmployeeCache,
                fsAppointmentEmployeeRow,
                isTAIntegrationActive ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXDefaultAttribute.SetPersistingCheck<FSAppointmentEmployee.earningType>(
                appointmentEmployeeCache,
                fsAppointmentEmployeeRow,
                isTAIntegrationActive ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXDefaultAttribute.SetPersistingCheck<FSAppointmentEmployee.actualDateTimeBegin>(
                appointmentEmployeeCache,
                fsAppointmentEmployeeRow,
                fsAppointmentRow?.Status == ID.Status_Appointment.COMPLETED ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXDefaultAttribute.SetPersistingCheck<FSAppointmentEmployee.actualDateTimeEnd>(
                appointmentEmployeeCache,
                fsAppointmentEmployeeRow,
                fsAppointmentRow?.Status == ID.Status_Appointment.COMPLETED ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        public static void EnableDisable_StaffRelatedFields(
                                                            PXCache appointmentEmployeeCache,
                                                            FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentEmployeeRow == null)
            {
                return;
            }

            bool enableEmployeeRelatedFields = fsAppointmentEmployeeRow.EmployeeID != null;

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.employeeID>
                    (appointmentEmployeeCache, fsAppointmentEmployeeRow, !enableEmployeeRelatedFields);

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.comment>
                    (appointmentEmployeeCache, fsAppointmentEmployeeRow, enableEmployeeRelatedFields);
        }

        public static void UpdateStaffRelatedUnboundFields(FSAppointmentDet fsAppointmentDetServiceRow, AppointmentEmployees_View appointmentEmployees, int? numEmployeeLinkedToService = null)
        {
            if (numEmployeeLinkedToService == null)
            {
                using (new PXConnectionScope())
                {
                    var empReferencingLineRef = appointmentEmployees.Select().AsEnumerable()
                                                                    .Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == fsAppointmentDetServiceRow.LineRef);

                    numEmployeeLinkedToService = empReferencingLineRef.Count();
                }
            }

            fsAppointmentDetServiceRow.StaffRelatedCount = numEmployeeLinkedToService;

            if (numEmployeeLinkedToService == 1)
            {
                fsAppointmentDetServiceRow.EnableStaffID = true;
                fsAppointmentDetServiceRow.StaffRelated = true;
            }
            else
            {
                fsAppointmentDetServiceRow.StaffID = null;
                fsAppointmentDetServiceRow.EnableStaffID = numEmployeeLinkedToService == 0;
                fsAppointmentDetServiceRow.StaffRelated = numEmployeeLinkedToService != 0;
            }
        }

        public static void InsertUpdateDelete_AppointmentDetService_StaffID(PXCache cache, FSAppointmentDet fsAppointmentDetServiceRow, AppointmentEmployees_View appointmentEmployees, int? oldStaffID)
        {
            if (fsAppointmentDetServiceRow.SODetID != null && fsAppointmentDetServiceRow.SODetID > 0)
            {
                FSSODet fsSODetRow = ServiceOrderCore.GetSODetFromAppointmentDet(cache.Graph, fsAppointmentDetServiceRow);

                if (fsAppointmentDetServiceRow.StaffID != null && oldStaffID != null)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = PXSelect<FSAppointmentEmployee,
                                                                        Where<
                                                                            FSAppointmentEmployee.serviceLineRef, Equal<Required<FSAppointmentEmployee.serviceLineRef>>,
                                                                            And<FSAppointmentEmployee.employeeID, Equal<Required<FSAppointmentEmployee.employeeID>>>>>.Select(cache.Graph, fsSODetRow.LineRef, oldStaffID);

                    if (fsAppointmentEmployeeRow != null)
                    {
                        fsAppointmentEmployeeRow.EmployeeID = fsAppointmentDetServiceRow.StaffID;
                        appointmentEmployees.Update(fsAppointmentEmployeeRow);
                    }
                }
                else if (fsAppointmentDetServiceRow.StaffID != null && oldStaffID == null)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee()
                    {
                        ServiceLineRef = fsSODetRow.LineRef,
                        EmployeeID = fsAppointmentDetServiceRow.StaffID
                    };
                    appointmentEmployees.Insert(fsAppointmentEmployeeRow);
                }
                else
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = PXSelect<FSAppointmentEmployee,
                                                                        Where<
                                                                            FSAppointmentEmployee.serviceLineRef, Equal<Required<FSAppointmentEmployee.serviceLineRef>>,
                                                                            And<FSAppointmentEmployee.employeeID, Equal<Required<FSAppointmentEmployee.employeeID>>>>>.Select(cache.Graph, fsSODetRow.LineRef, oldStaffID);

                    appointmentEmployees.Delete(fsAppointmentEmployeeRow);
                }
            }
        }

        /// <summary>
        /// Determines if the given appointment belongs to a Service Order Type with Route behavior.
        /// </summary>
        private static bool IsARouteSrvOrderType(PXGraph graph, FSAppointment fsAppointmentRow)
        {
            FSSrvOrdType fsSrvOrdTypeRow = 
                                            PXSelect<FSSrvOrdType, 
                                            Where<
                                                FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>, 
                                                And<FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.RouteAppointment>>>>
                                            .Select(graph, fsAppointmentRow.SrvOrdType);
            return fsSrvOrdTypeRow != null;
        }

        private static void EnableDisable_ScheduleDateTimes(PXCache cache, FSAppointment fsAppointmentRow, bool masterEnable)
        {
            PXUIFieldAttribute.SetEnabled<FSAppointment.scheduledDateTimeBegin>(cache, fsAppointmentRow, masterEnable);
            PXUIFieldAttribute.SetEnabled<FSAppointment.scheduledDateTimeEnd>(cache, fsAppointmentRow, masterEnable);
        }
        
        private static void EnableDisable_UnreachedCustomer(PXCache cache, FSAppointment fsAppointmentRow, bool masterEnable)
        {
            bool enable = false;

            switch (fsAppointmentRow.Status)
            {
                case ID.Status_Appointment.MANUAL_SCHEDULED:
                    enable = true;
                    break;

                case ID.Status_Appointment.AUTOMATIC_SCHEDULED:
                    enable = true;
                    break;

                default:
                    break;
            }

            PXUIFieldAttribute.SetEnabled<FSAppointment.unreachedCustomer>(cache, fsAppointmentRow, enable && masterEnable);
        }

        [Obsolete("EnableDisable_LineType is deprecated, please use the generic methods X_RowSelected and X_SetPersistingCheck instead.")]
        private static void EnableDisable_LineType(PXCache cache, FSAppointmentDet fsAppointmentDetRow, FSSetup fsSetupRow, FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            // TODO:
            // Move all these SetEnabled and SetPersistingCheck calls to the new generic method X_RowSelected.
            // Verify if each field is handled by the generic method before moving it.
            // If the generic method already handles a field, check if the conditions to enable/disable
            // and PersistingCheck are the same.
            // DELETE this method when all fields are moved.

            bool enable = fsSrvOrdTypeRow.RequireTimeApprovalToInvoice == false;
            bool equipmentOrRouteModuleEnabled = PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>()
                                                        || PXAccess.FeatureInstalled<FeaturesSet.routeManagementModule>();
            bool showStandardContractColumns = fsAppointmentRow.BillServiceContractID != null
                                                && equipmentOrRouteModuleEnabled;
            switch (fsAppointmentDetRow.LineType)
            {
                case ID.LineType_ServiceTemplate.SERVICE:
                case ID.LineType_ServiceTemplate.NONSTOCKITEM:

                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, PXPersistingCheck.NullOrBlank);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isBillable>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.qty>(cache, fsAppointmentDetRow, SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.curyUnitPrice>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.projectTaskID>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedDuration>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetVisible<FSSODet.contractRelated>(cache, null, showStandardContractColumns);
                    PXUIFieldAttribute.SetVisible<FSSODet.coveredQty>(cache, null, showStandardContractColumns);
                    PXUIFieldAttribute.SetVisible<FSSODet.extraUsageQty>(cache, null, showStandardContractColumns);
                    PXUIFieldAttribute.SetVisible<FSSODet.curyExtraUsageUnitPrice>(cache, null, showStandardContractColumns);
                    PXUIFieldAttribute.SetVisibility<FSSODet.contractRelated>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
                    PXUIFieldAttribute.SetVisibility<FSSODet.coveredQty>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
                    PXUIFieldAttribute.SetVisibility<FSSODet.extraUsageQty>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
                    PXUIFieldAttribute.SetVisibility<FSSODet.curyExtraUsageUnitPrice>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

                    break;
                case ID.LineType_ServiceTemplate.INSTRUCTION_PART:
                case ID.LineType_ServiceTemplate.INSTRUCTION_SERVICE:
                case ID.LineType_ServiceTemplate.COMMENT_PART:
                case ID.LineType_ServiceTemplate.COMMENT_SERVICE:

                    PXDefaultAttribute.SetPersistingCheck<FSAppointmentDet.tranDesc>(cache, fsAppointmentDetRow, PXPersistingCheck.NullOrBlank);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isBillable>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.qty>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.curyUnitPrice>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.projectTaskID>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.staffID>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.billingRule>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedDuration>(cache, fsAppointmentDetRow, false);

                    enable = false;

                    break;

                case ID.LineType_ServiceTemplate.INVENTORY_ITEM:
                default:
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, PXPersistingCheck.NullOrBlank);                    
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isBillable>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.qty>(cache, fsAppointmentDetRow, SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.curyUnitPrice>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.projectTaskID>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.staffID>(cache, fsAppointmentDetRow, false);

                    break;
            }

            AppointmentCore.EnableDisable_ServiceActualDateTimes(cache, fsAppointmentRow, fsAppointmentDetRow, fsSetupRow, enable);
        }
        #endregion

        /// <summary>
        /// Returns true if an Appointment [fsAppointmentRow] can change its status to [newAppointmentStatus] based in the current status of the Appointment [fsAppointmentRow]
        /// <para>and the status of the Service Order [fsServiceOrderRow]</para>. If an error is detected is going to be assigned to the [errorMessage].
        /// </summary>
        public static bool CheckNewAppointmentStatus(
                                                    PXGraph graph,
                                                    FSAppointment fsAppointmentRow,
                                                    FSServiceOrder fsServiceOrderRow,
                                                    string newAppointmentStatus,
                                                    ref string errorMessage)
        {
            errorMessage = string.Empty;

            // Schedule => In Process
            if ((fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED 
                    || fsAppointmentRow.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED)
                        && newAppointmentStatus == ID.Status_Appointment.IN_PROCESS)
            {
                return true;
            }

            // In Process => Schedule
            if (fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS
                    && newAppointmentStatus == ID.Status_Appointment.MANUAL_SCHEDULED)
            {
                return true;
            }
            
            // Schedule => Canceled
            if ((fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED
                    || fsAppointmentRow.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED)
                        && newAppointmentStatus == ID.Status_Appointment.CANCELED)
            {
                return true;
            }

            // Canceled => Schedule
            if (fsAppointmentRow.Status == ID.Status_Appointment.CANCELED
                    && newAppointmentStatus == ID.Status_Appointment.MANUAL_SCHEDULED)
            {
                return true;
            }

            // Schedule => Completed
            if ((fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED
                    || fsAppointmentRow.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED)
                        && newAppointmentStatus == ID.Status_Appointment.COMPLETED)
            {
                return true;
            }

            // Completed => Schedule
            if (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED
                    && newAppointmentStatus == ID.Status_Appointment.MANUAL_SCHEDULED)
            {
                return true;
            }

            // In Process => Completed
            if (fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS
                    && newAppointmentStatus == ID.Status_Appointment.COMPLETED)
            {
                return true;
            }
            
            // Completed => Closed (Disable edit document)
            if (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED
                    && newAppointmentStatus == ID.Status_Appointment.CLOSED)
            {
                return true;
            }

            errorMessage = TX.Error.INVALID_APPOINTMENT_STATUS_TRANSITION;
            return false;
        }

        /// <summary>
        /// Returns true if an Appointment [fsAppointmentRow] can be updated based in its status and the status of the Service Order [fsServiceOrderRow].
        /// </summary>
        public static bool CanUpdateAppointment(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if(fsAppointmentRow == null
                    || fsSrvOrdTypeRow == null)
            {
                return false;
            }

            if ((fsAppointmentRow.Status == ID.Status_Appointment.CLOSED
                        || fsAppointmentRow.Status == ID.Status_Appointment.CANCELED)
                        || fsSrvOrdTypeRow.Active == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if an Appointment [fsAppointmentRow] can be deleted based in its status and the status of the Service Order [fsServiceOrderRow].
        /// </summary>
        public static bool CanDeleteAppointment(FSAppointment fsAppointmentRow, FSServiceOrder fsServiceOrderRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            bool canDeleteServiceOrder = true;
            if (fsServiceOrderRow != null)
            {
                canDeleteServiceOrder = ServiceOrderCore.CanUpdateServiceOrder(fsServiceOrderRow, fsSrvOrdTypeRow);
            }

            if (fsAppointmentRow == null
                    || (fsAppointmentRow.Status != ID.Status_Appointment.AUTOMATIC_SCHEDULED
                        && fsAppointmentRow.Status != ID.Status_Appointment.IN_PROCESS
                        && fsAppointmentRow.Status != ID.Status_Appointment.MANUAL_SCHEDULED))
            {
                return false;
            }

            return canDeleteServiceOrder;
        }

        public static DateTime? GetDateTimeEnd(DateTime? dateTimeBegin, DateTime? dateTimeEnd)
        {
            if (dateTimeBegin != null && dateTimeEnd != null)
            {
                return new DateTime(
                        dateTimeBegin.Value.Year,
                        dateTimeBegin.Value.Month,
                        dateTimeBegin.Value.Day,
                        dateTimeEnd.Value.Hour,
                        dateTimeEnd.Value.Minute,
                        dateTimeEnd.Value.Second,
                        dateTimeEnd.Value.Millisecond);
            }

            return dateTimeEnd;
        }

        public static DateTime? GetDateTimeEnd(DateTime? dateTimeBegin, int hour = 0, int minute = 0, int second = 0, int milisecond = 0)
        {
            if (dateTimeBegin != null)
            {
                return new DateTime(
                        dateTimeBegin.Value.Year,
                        dateTimeBegin.Value.Month,
                        dateTimeBegin.Value.Day,
                        hour,
                        minute,
                        second,
                        milisecond);
            }

            return null;
        }

        /// <summary>
        /// Sets properly the right values for ActualDuration & Qty for services and inventory items and the
        /// ActualDateTimeBegin & ActualDateTimeEnd for the staff members when starting & re-opening the appointment.
        /// </summary>
        public static void UpdateAppointmentDetActualFields(
                                                            AppointmentDetServices_View appointmentDetailService,
                                                            AppointmentDetParts_View appointmentPart,
                                                            AppointmentEmployees_View appointmentEmployees,
                                                            bool isReOpen = false)
        {
            foreach (FSAppointmentDetService fsAppointmentDetServiceRow in appointmentDetailService.Select())
            {
                if (fsAppointmentDetServiceRow.IsService == true)
                {
                    if (fsAppointmentDetServiceRow.StaffRelated == true)
                    {
                        foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployees.Select()
                                                                                .RowCast<FSAppointmentEmployee>()
                                                                                .Where(_ => _.ServiceLineRef == fsAppointmentDetServiceRow.LineRef))
                        {
                            fsAppointmentEmployeeRow.ActualDuration = isReOpen ? 0 : fsAppointmentDetServiceRow.EstimatedDuration;

                    if (isReOpen)
                    {
                                fsAppointmentEmployeeRow.ActualDateTimeBegin = null;
                                fsAppointmentEmployeeRow.ActualDateTimeEnd = null;
                    }

                            appointmentEmployees.Update(fsAppointmentEmployeeRow);
                        }

                        AppointmentDateTimeHelper.CalculateAppointmentDetServiceActualDuration(appointmentDetailService.Cache, fsAppointmentDetServiceRow);
                    }

                    if (fsAppointmentDetServiceRow.StaffRelated == false)
                    {
                        fsAppointmentDetServiceRow.ActualDuration = isReOpen ? 0 : fsAppointmentDetServiceRow.EstimatedDuration;
                    }

                    if (fsAppointmentDetServiceRow.BillingRule == ID.BillingRule.FLAT_RATE ||
                        fsAppointmentDetServiceRow.BillingRule == ID.BillingRule.NONE)
                    {
                        fsAppointmentDetServiceRow.Qty = isReOpen ? 0 : fsAppointmentDetServiceRow.EstimatedQty;
                    }

                    if (isReOpen)
                    {
                        fsAppointmentDetServiceRow.ActualDateTimeBegin = null;
                        fsAppointmentDetServiceRow.ActualDateTimeEnd = null;
                    }

                    appointmentDetailService.Update(fsAppointmentDetServiceRow);
                }
            }

            foreach (FSAppointmentDetPart fsAppointmentDetPartRow in appointmentPart.Select())
            {
                if (fsAppointmentDetPartRow.LineType == ID.LineType_All.INVENTORY_ITEM)
                {
                    fsAppointmentDetPartRow.Qty = isReOpen ? 0 : fsAppointmentDetPartRow.EstimatedQty;
                    appointmentPart.Update(fsAppointmentDetPartRow);
                }
            }

            if (isReOpen)
            {
                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appointmentEmployees.Select())
                {
                    fsAppointmentEmployeeRow.ActualDateTimeBegin = null;
                    fsAppointmentEmployeeRow.ActualDateTimeEnd = null;
                    fsAppointmentEmployeeRow.ActualDuration = 0;
                    appointmentEmployees.Update(fsAppointmentEmployeeRow);
                }
            }
        }

        public static void GetSODetValues<AppointmentDetType, SODetType>(PXCache cacheAppointmentDet, AppointmentDetType fsAppointmentDetRow, FSServiceOrder fsServiceOrderRow, FSAppointment fsAppointmentRow)
                where AppointmentDetType : FSAppointmentDet, new()
                where SODetType : FSSODet, new()
        {
            if (fsAppointmentDetRow.SODetID == null)
            {
                return;
            }

            PXCache cacheSODet = new PXCache<FSSODet>(cacheAppointmentDet.Graph);

            FSSODet fsSODetRow = PXSelect<FSSODet,
                                 Where<
                                       FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                                 .Select(cacheAppointmentDet.Graph, fsAppointmentDetRow.SODetID);

            if (fsSODetRow != null)
            {
                fsAppointmentDetRow.LineRef = fsSODetRow.LineRef;

                var graphAppointment = (AppointmentEntry)cacheAppointmentDet.Graph;

                graphAppointment.CopyAppointmentLineValues<AppointmentDetType, FSSODet>(cacheAppointmentDet, fsAppointmentDetRow, cacheSODet, fsSODetRow, false, fsSODetRow.TranDate, ForceFormulaCalculation: false);

                if (SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow))
                {
                    cacheAppointmentDet.SetValueExt<FSAppointmentDet.actualDuration>(fsAppointmentDetRow, 0);
                    cacheAppointmentDet.SetValueExt<FSAppointmentDet.qty>(fsAppointmentDetRow, 0m);
                }

                if (fsServiceOrderRow.SourceRefNbr != null
                        && fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER
                            && fsAppointmentDetRow.IsPrepaid == true)
                {
                    fsAppointmentDetRow.SourceSalesOrderRefNbr = fsServiceOrderRow.SourceRefNbr;
                    fsAppointmentDetRow.SourceSalesOrderType = fsServiceOrderRow.SourceDocType;
                }
            }
        }

        public static void ValidateQty(PXCache cache, FSAppointmentDet fsAppointmentDetRow, PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            if (fsAppointmentDetRow.Qty < 0)
            {
                PXUIFieldAttribute.SetEnabled<FSAppointmentDet.qty>(cache, fsAppointmentDetRow, true);
                cache.RaiseExceptionHandling<FSAppointmentDet.qty>(
                                                                fsAppointmentDetRow,
                                                                null,
                                                                new PXSetPropertyException(TX.Error.NEGATIVE_QTY, errorLevel));
            }
        }

        /// <summary>
        /// Determines if a Service line has at least one pickup/delivery item related.
        /// </summary>
        public static bool ServiceLinkedToPickupDeliveryItem(PXGraph graph, FSAppointmentDetService fsAppointmentDetServiceRow, FSAppointment fsAppointmentRow)
        {
            PXResultset<FSAppointmentInventoryItem> bqlResultSet = PXSelect<FSAppointmentInventoryItem,
                                                                        Where<
                                                                            FSAppointmentInventoryItem.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                                                            And<FSAppointmentInventoryItem.sODetID, Equal<Required<FSAppointmentDetService.sODetID>>,
                                                                            And<FSAppointmentInventoryItem.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>>>
                                                                   .Select(graph, fsAppointmentDetServiceRow.SODetID, fsAppointmentRow.AppointmentID);

            if (bqlResultSet.Count > 0)
            {
                return true;
            }

            return false;
        }

        public static void UpdatePendingPostFlags(
            FSAppointment fsAppointmentRow,
            FSServiceOrder fsServiceOrder,
            AppointmentCore.AppointmentDetServices_View appointmentServiceDetail,
            AppointmentCore.AppointmentDetParts_View appointmentPartDetail,
            AppointmentCore.AppointmentPickupDeliveryItems_View appointmentPickUpDeliverDetail)
        {
            int? servicesToPost = appointmentServiceDetail.Select().AsEnumerable().Where(y => ((FSAppointmentDetService)y).needToBePosted() == true)
                                                                   .RowCast<FSPostInfo>().Where(x => x.isPosted() == false).Count();

            int? partsToPost = appointmentPartDetail.Select().AsEnumerable().Where(y => ((FSAppointmentDetPart)y).needToBePosted() == true)
                                                             .RowCast<FSPostInfo>().Where(x => x.isPosted() == false).Count();

            int? pickUpDeliverToPost = appointmentPickUpDeliverDetail.Select().RowCast<FSPostInfo>().Where(x => x.isPosted() == false).Count();

            if (fsServiceOrder.PostedBy == ID.Billing_By.SERVICE_ORDER)
            {
                fsAppointmentRow.PendingAPARSOPost = false;
                fsAppointmentRow.PendingINPost = false;
                fsAppointmentRow.PostingStatusAPARSO = ID.Status_Posting.NOTHING_TO_POST;
                fsAppointmentRow.PostingStatusIN = ID.Status_Posting.NOTHING_TO_POST;
            }
            else if ((fsServiceOrder.PostedBy == null || fsServiceOrder.PostedBy == ID.Billing_By.APPOINTMENT)
                          && (servicesToPost > 0 || partsToPost > 0 || pickUpDeliverToPost > 0))
            {
                fsAppointmentRow.PendingAPARSOPost = true;
                fsAppointmentRow.PostingStatusAPARSO = ID.Status_Posting.PENDING_TO_POST;

                if (pickUpDeliverToPost > 0)
                {
                    fsAppointmentRow.PendingINPost = true;
                    //// @TODO: When IN posting is done, uncomment this line
                    ////fsAppointmentRow.PostingStatusIN = ID.Status_Posting.PENDING_TO_POST;
                }
                else
                {
                    fsAppointmentRow.PendingINPost = false;
                    fsAppointmentRow.PostingStatusIN = ID.Status_Posting.NOTHING_TO_POST;
                }
            }
            else
            {
                fsAppointmentRow.PendingAPARSOPost = false;
                fsAppointmentRow.PendingINPost = false;
                fsAppointmentRow.PostingStatusAPARSO = ID.Status_Posting.NOTHING_TO_POST;
                fsAppointmentRow.PostingStatusIN = ID.Status_Posting.NOTHING_TO_POST;
            }
        }

        public static void UpdateWaitingForPartsFlag(
            FSAppointment fsAppointmentRow,
            FSServiceOrder fsServiceOrder,
            AppointmentCore.AppointmentDetServices_View appointmentServiceDetail,
            AppointmentCore.AppointmentDetParts_View appointmentPartDetail)
        {

            int? serviceWaitingForParts = appointmentServiceDetail.Select().RowCast<FSSODet>().Where(x => x.waitingForParts() == true).Count();

            int? partsWaitingForParts = appointmentPartDetail.Select().RowCast<FSSODet>().Where(x => x.waitingForParts() == true).Count();

            fsAppointmentRow.WaitingForParts = (serviceWaitingForParts > 0 || partsWaitingForParts > 0);
        }

        /// <summary>
        /// Updates appointment fields in the Service Order. 
        /// </summary>
        public static void UpdateAppointmentsInfoInServiceOrder(
            PXCache cache,
            FSAppointmentDet fsAppointmentDetRow,
            AppointmentCore.ServiceOrderRelated_View viewServiceOrderRelated,
            Dictionary<int?, string> originalStatus = null,
            PXDBOperation apptLineOperation = PXDBOperation.Insert)
        {
            //Validating Operation type
            if (apptLineOperation != PXDBOperation.Insert
                    && apptLineOperation != PXDBOperation.Delete
                        && apptLineOperation != PXDBOperation.Update)
            {
                return;
            }

            //Validating Line type
            if (fsAppointmentDetRow.LineType != ID.LineType_All.SERVICE
                    && fsAppointmentDetRow.LineType != ID.LineType_All.NONSTOCKITEM
                        && fsAppointmentDetRow.LineType != ID.LineType_All.INVENTORY_ITEM)
            {
                return;
            }

            string oldLineStatus = string.Empty;
            if (originalStatus != null)
            {
                originalStatus.TryGetValue(fsAppointmentDetRow.SODetID, out oldLineStatus);
            }

            PXGraph tempGraphAppDet = new PXGraph();
            PXGraph tempGraphSODet = new PXGraph();

            FSSODet fsSODetRow = ServiceOrderCore.GetSODetFromAppointmentDet(tempGraphAppDet, fsAppointmentDetRow);

            if (fsSODetRow != null)
            {
                bool updateServiceOrder = false;
                bool lineStatusChange = false;

                int? sign = 1;

                switch(apptLineOperation)
                {
                    case PXDBOperation.Insert:

                        if(fsAppointmentDetRow.Status != ID.Status_AppointmentDet.CANCELED)
                        {
                            sign = 1;
                            updateServiceOrder = true;
                        }
                        break;

                    case PXDBOperation.Update:

                        //Appointment line status change --> CANCELED
                        if (oldLineStatus != fsAppointmentDetRow.Status
                                && fsAppointmentDetRow.Status == ID.Status_AppointmentDet.CANCELED)
                        {
                            sign = -1;
                            lineStatusChange = true;
                        }

                        //Appointment line status change: CANCELED --> REOPEN
                        if (oldLineStatus != fsAppointmentDetRow.Status
                                && oldLineStatus == ID.Status_AppointmentDet.CANCELED)
                        {
                            sign = 1;
                            lineStatusChange = true;
                        }

                        updateServiceOrder = true;

                        break;
                    case PXDBOperation.Delete:

                        if (fsAppointmentDetRow.Status != ID.Status_AppointmentDet.CANCELED)
                        {
                            sign = -1;
                            updateServiceOrder = true;
                        }
                        break;
                }

                if (updateServiceOrder)
                {
                    decimal? curyApptTranAmt = fsSODetRow.CuryApptTranAmt + sign * fsAppointmentDetRow.CuryTranAmt;
                    decimal? apptQty         = fsSODetRow.ApptQty         + sign * fsAppointmentDetRow.Qty;
                    int? apptDuration        = fsSODetRow.ApptDuration    + sign * fsAppointmentDetRow.ActualDuration;
                    int? apptNumber          = fsSODetRow.ApptNumber      + sign;
                    decimal apptTranAmt;

                    if (apptLineOperation == PXDBOperation.Update)
                    {
                        PXResultset<FSAppointmentDet> appDetailLineRows = PXSelect<FSAppointmentDet, 
                                                                            Where<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>>>
                                                                          .Select(tempGraphAppDet, fsAppointmentDetRow.SODetID);

                        apptQty         = appDetailLineRows.RowCast<FSAppointmentDet>().Where(x => x.Status != ID.Status_AppointmentDet.CANCELED).Select(x => x.Qty).Sum();
                        apptDuration    = appDetailLineRows.RowCast<FSAppointmentDet>().Where(x => x.Status != ID.Status_AppointmentDet.CANCELED).Select(x => x.ActualDuration).Sum();
                        curyApptTranAmt = appDetailLineRows.RowCast<FSAppointmentDet>().Where(x => x.Status != ID.Status_AppointmentDet.CANCELED).Select(x => x.CuryTranAmt).Sum();
                        apptNumber      = lineStatusChange ? apptNumber : fsSODetRow.ApptNumber;
                    }

                    CM.PXDBCurrencyAttribute.CuryConvBase(viewServiceOrderRelated.Cache, viewServiceOrderRelated.Current, curyApptTranAmt.Value, out apptTranAmt);
                    //// Updating Service Order's detail line
                    PXUpdate<
                        Set<FSSODet.apptNumber, Required<FSSODet.apptNumber>,
                        Set<FSSODet.apptQty, Required<FSSODet.apptQty>,
                        Set<FSSODet.apptTranAmt, Required<FSSODet.apptTranAmt>,
                        Set<FSSODet.curyApptTranAmt, Required<FSSODet.curyApptTranAmt>,
                        Set<FSSODet.apptDuration, Required<FSSODet.apptDuration>>>>>>,
                        FSSODet,
                        Where<FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                    .Update(tempGraphAppDet, apptNumber, apptQty, apptTranAmt, curyApptTranAmt, apptDuration, fsSODetRow.SODetID);

                    //// Updating Service Order's header
                    FSServiceOrder fsServiceOrderRow = PXSelect<FSServiceOrder,
                                                                Where<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                                              .Select(tempGraphSODet, fsSODetRow.SOID);

                    int? apptDurationTotal = fsServiceOrderRow.ApptDurationTotal + (fsSODetRow.LineType == ID.LineType_All.SERVICE ? - fsSODetRow.ApptDuration + apptDuration : 0);
                    decimal? apptOrderTotal = fsServiceOrderRow.ApptOrderTotal - fsSODetRow.ApptTranAmt + apptTranAmt;
                    decimal? curyApptOrderTotal = fsServiceOrderRow.CuryApptOrderTotal - fsSODetRow.CuryApptTranAmt + curyApptTranAmt;

                    PXUpdate<
                        Set<FSServiceOrder.apptDurationTotal, Required<FSServiceOrder.apptDurationTotal>,
                        Set<FSServiceOrder.apptOrderTotal, Required<FSServiceOrder.apptOrderTotal>,
                        Set<FSServiceOrder.curyApptOrderTotal, Required<FSServiceOrder.curyApptOrderTotal>>>>,
                        FSServiceOrder,
                        Where<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                    .Update(tempGraphSODet, apptDurationTotal, apptOrderTotal, curyApptOrderTotal, fsSODetRow.SOID);

                    cache.Graph.SelectTimeStamp();
                }
            }
        }

        public static void UpdateAppointmentDetLinesStatus<AppointmentDetType>(PXSelectBase<AppointmentDetType> viewAppointmentDet, string status)
            where AppointmentDetType : FSAppointmentDet, new()
        {
            foreach (AppointmentDetType fsAppointmentDetRow in viewAppointmentDet.Select())
            {
                fsAppointmentDetRow.Status = status;
                viewAppointmentDet.Update(fsAppointmentDetRow);
            }
        }

        public static bool IsAppointmentReadyToBeInvoiced(FSAppointment fsAppointmentRow, FSServiceOrder fsServiceOrderRow, FSBillingCycle fsBillingCycle, FSSrvOrdType fsSrvOrdTypeRow)
        {
            return fsAppointmentRow != null
                    && fsServiceOrderRow != null
                    && fsBillingCycle != null
                    && fsSrvOrdTypeRow != null
                    && fsAppointmentRow.PendingAPARSOPost == true
                    && fsAppointmentRow.BillContractPeriodID == null
                    && fsAppointmentRow.BillContractPeriodID == null
                    && fsServiceOrderRow.BillContractPeriodID == null
                    && fsServiceOrderRow.BillContractPeriodID == null
                    && fsServiceOrderRow.CBID != null
                    && fsBillingCycle.BillingBy == ID.Billing_By.APPOINTMENT
                    && (fsAppointmentRow.Status == ID.Status_Appointment.CLOSED
                            || (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED
                                    && fsSrvOrdTypeRow.AllowInvoiceOnlyClosedAppointment == false
                                    && fsAppointmentRow.TimeRegistered == true))
                    && (fsBillingCycle.InvoiceOnlyCompletedServiceOrder == false
                            || (fsBillingCycle.InvoiceOnlyCompletedServiceOrder == true
                                    && (fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED
                                            || fsServiceOrderRow.Status == ID.Status_ServiceOrder.CLOSED)));
        }
        #endregion

        #region Event Handlers
        #region FSAppointmentDetail

        /// <summary>
        /// If the given line is prepaid then disable all its editable fields.
        /// </summary>
        /// <param name="cacheAppointmentDet">Cache of the Appointment Detail.</param>
        /// <param name="fsAppointmentDetRow">Appointment Detail row.</param>
        private static void DisablePrepaidLine(PXCache cacheAppointmentDet, FSAppointmentDet fsAppointmentDetRow)
        {
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.tranDesc>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.lineType>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.inventoryID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.billingRule>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isBillable>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.qty>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.curyUnitPrice>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.projectTaskID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.siteID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.siteLocationID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.acctID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.subID>(cacheAppointmentDet, fsAppointmentDetRow, false);
        }

        public static void FSAppointmentDet_RowSelected_PartialHandler(
                                                                    PXCache cacheAppointmentDet,
                                                                    FSAppointmentDet fsAppointmentDetRow,
                                                                    FSSetup fsSetupRow,
                                                                    FSSrvOrdType fsSrvOrdTypeRow,
                                                                    FSServiceOrder fsServiceOrderRow,
                                                                    FSAppointment fsAppointmentRow = null)
        {
            EnableDisable_LineType(cacheAppointmentDet, fsAppointmentDetRow, fsSetupRow, fsAppointmentRow, fsSrvOrdTypeRow);

            if (fsAppointmentDetRow.IsPrepaid == true)
            {
                DisablePrepaidLine(cacheAppointmentDet, fsAppointmentDetRow);
            }
            else
            {
                ServiceOrderCore.EnableDisable_Acct_Sub(cacheAppointmentDet, fsAppointmentDetRow, fsSrvOrdTypeRow, fsServiceOrderRow);
            } 
        }

        public static void FSAppointmentDet_RowPersisting_PartialHandler(
                                                                        PXCache cacheAppointmentDet,
                                                                        FSAppointmentDet fsAppointmentDetRow,
                                                                        FSAppointment fsAppointmentRow,
                                                                        FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (fsAppointmentDetRow.LineType == ID.LineType_ServiceTemplate.INVENTORY_ITEM
                && fsSrvOrdTypeRow.PostTo == ID.SourceType_ServiceOrder.SALES_ORDER
                && fsAppointmentDetRow.LastModifiedByScreenID != ID.ScreenID.GENERATE_SERVICE_CONTRACT_APPOINTMENT
                && fsAppointmentDetRow.SiteID == null)
            {
                cacheAppointmentDet.RaiseExceptionHandling<FSAppointmentDet.siteID>(fsAppointmentDetRow, null, new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, PXErrorLevel.Error));
            }

            ValidateQty(cacheAppointmentDet, fsAppointmentDetRow);

            fsAppointmentDetRow.RefNbr = fsAppointmentDetRow.RefNbr == null
                                                || fsAppointmentDetRow.RefNbr != fsAppointmentRow.RefNbr ? fsAppointmentRow.RefNbr : fsAppointmentDetRow.RefNbr;
        }

        public static void RefreshSalesPricesInTheWholeDocument(
                        AppointmentCore.AppointmentDetServices_View appointmentDetServices,
                        AppointmentCore.AppointmentDetParts_View appointmentDetParts,
                        AppointmentCore.AppointmentPickupDeliveryItems_View appointmentDetPickupDeliveries)
        {
            // TODO:
            // This method should run and depend on BillCustomerID changes, and not on CustomerID changes
            // Besides that, check if this is necessary using the Sales-Price graph extension

            foreach (FSAppointmentDetService row in appointmentDetServices.Select())
            {
                appointmentDetServices.Cache.SetDefaultExt<FSAppointmentDetService.curyUnitPrice>(row);
                appointmentDetServices.Cache.Update(row);
            }

            foreach (FSAppointmentDetPart row in appointmentDetParts.Select())
            {
                appointmentDetParts.Cache.SetDefaultExt<FSAppointmentDetPart.curyUnitPrice>(row);
                appointmentDetParts.Cache.Update(row);
            }

            foreach (FSAppointmentInventoryItem row in appointmentDetPickupDeliveries.Select())
            {
                appointmentDetPickupDeliveries.Cache.SetDefaultExt<FSAppointmentInventoryItem.curyUnitPrice>(row);
                appointmentDetPickupDeliveries.Cache.Update(row);
            }
        }

        #endregion
        #endregion
    }
}
