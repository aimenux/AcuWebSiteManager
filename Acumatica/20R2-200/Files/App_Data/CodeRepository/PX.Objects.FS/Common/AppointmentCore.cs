using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public static class AppointmentCore
    {
        #region Selects

        public class AppointmentRecords_View : PXSelectJoin<FSAppointment,
                LeftJoinSingleTable<Customer,
                    On<Customer.bAccountID, Equal<FSAppointment.customerID>>>,
                Where<
                    FSAppointment.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>,
                    And<
                        Where<
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

        [PXDynamicButton(new string[] { DetailsPasteLineCommand, DetailsResetOrderCommand },
                         new string[] { ActionsMessages.PasteLine, ActionsMessages.ResetOrder },
                         TranslationKeyType = typeof(Common.Messages))]
        public class AppointmentDetails_View : PXOrderedSelect<FSAppointment, FSAppointmentDet,
                                               LeftJoin<FSSODet,
                                                    On<FSAppointmentDet.sODetID, Equal<FSSODet.sODetID>>,
                                               LeftJoin<FSPostInfo,
                                                    On<FSPostInfo.postID, Equal<FSAppointmentDet.postID>>>>,
                                               Where<
                                                    FSAppointmentDet.appointmentID, Equal<Current<FSAppointment.appointmentID>>>,
                                               OrderBy<
                                                    Asc<FSAppointmentDet.srvOrdType,
                                                    Asc<FSAppointmentDet.refNbr,
                                                    Asc<FSAppointmentDet.sortOrder,
                                                    Asc<FSAppointmentDet.lineNbr>>>>>>
        {
            public AppointmentDetails_View(PXGraph graph) : base(graph) { }

            public AppointmentDetails_View(PXGraph graph, Delegate handler) : base(graph, handler) { }

            public const string DetailsPasteLineCommand = "DetailsPasteLine";
            public const string DetailsResetOrderCommand = "DetailsResetOrder";

            protected override void AddActions(PXGraph graph)
            {
                AddAction(graph, DetailsPasteLineCommand, ActionsMessages.PasteLine, PasteLine);
                AddAction(graph, DetailsResetOrderCommand, ActionsMessages.ResetOrder, ResetOrder);
            }
        }

        public class AppointmentServiceEmployees_View : PXSelectJoin<FSAppointmentEmployee,
                                                        LeftJoin<BAccount,
                                                            On<FSAppointmentEmployee.employeeID, Equal<BAccount.bAccountID>>,
                                                        LeftJoin<FSAppointmentServiceEmployee,
                                                            On<
                                                                FSAppointmentServiceEmployee.lineRef, Equal<FSAppointmentEmployee.serviceLineRef>,
                                                                And<FSAppointmentServiceEmployee.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>>>>,
                                                        Where<
                                                            FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                                                            And<
                                                                Where<
                                                                    FSAppointmentEmployee.serviceLineRef, IsNull,
                                                                Or<
                                                                    FSAppointmentServiceEmployee.lineType, Equal<ListField_LineType_ALL.Service>>>>>,
                                                        OrderBy<
                                                                Asc<FSAppointmentEmployee.lineRef>>>
        {
            public AppointmentServiceEmployees_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentServiceEmployees_View(PXGraph graph, Delegate handler) : base(graph, handler)
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

        public class AppointmentLog_View : PXSelectJoin<FSAppointmentLog,
                                            LeftJoin<FSAppointmentDet, 
                                                On<FSAppointmentDet.lineRef, Equal<FSAppointmentLog.detLineRef>,
                                                And<FSAppointmentDet.appointmentID, Equal<FSAppointmentLog.docID>>>>,
                                            Where<FSAppointmentLog.docID, Equal<Current<FSAppointment.appointmentID>>>>
        {
            public AppointmentLog_View(PXGraph graph) : base(graph)
            {
            }

            public AppointmentLog_View(PXGraph graph, Delegate handler) : base(graph, handler)
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
        public static void EnableDisable_ServiceActualDateTimes(PXCache appointmentDetCache, 
                                                                FSAppointment fsAppointmentRow, 
                                                                FSAppointmentDet fsAppointmentDetRow,
                                                                bool enableByLineType)
        {
            if (fsAppointmentRow == null || fsAppointmentDetRow == null)
            {
                return;
            }

            //Grouping conditions that affect Service Actual Duration's enable/disable behavior.

            //Enable by Time Behavior
            bool enableByTimeBehavior = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false;

            //Enable by Log Related
            bool enableByLogRelated = (fsAppointmentDetRow.LogRelatedCount ?? 0) == 0;

            bool enableActualDuration = enableByLineType && enableByLogRelated && enableByTimeBehavior;

            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualDuration>(appointmentDetCache, fsAppointmentDetRow, enableActualDuration);
        }

        public static void EnableDisable_TimeRelatedFields(PXCache appointmentEmployeeCache,
                                                           FSSetup fsSetupRow,
                                                           FSSrvOrdType fsSrvOrdType,
                                                           FSAppointment fsAppointmentRow,
                                                           FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentRow == null || fsAppointmentEmployeeRow == null || fsSetupRow == null)
            {
                return;
            }

            bool enableByTimeBehavior = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false;

            bool enableActualStartDateTime = enableByTimeBehavior;

            bool enableTrackTime = fsSetupRow.EnableEmpTimeCardIntegration.Value
                                   && fsSrvOrdType.CreateTimeActivitiesFromAppointment.Value
                                        && fsAppointmentEmployeeRow.Type == BAccountType.EmployeeType
                                            && fsAppointmentEmployeeRow.EmployeeID != null;

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.trackTime>(appointmentEmployeeCache,
                                                                           fsAppointmentEmployeeRow,
                                                                           enableTrackTime);

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.earningType>(appointmentEmployeeCache,
                                                                             fsAppointmentEmployeeRow,
                                                                             enableTrackTime);
        }

        public static void SetVisible_TimeRelatedFields(PXCache appointmentEmployeeCache, FSSrvOrdType fsSrvOrdType)
        {
            PXUIFieldAttribute.SetVisible<FSAppointmentEmployee.trackTime>(appointmentEmployeeCache,
                                                                           null,
                                                                           fsSrvOrdType.CreateTimeActivitiesFromAppointment.Value);

            PXUIFieldAttribute.SetVisible<FSAppointmentEmployee.earningType>(appointmentEmployeeCache,
                                                                             null,
                                                                             fsSrvOrdType.CreateTimeActivitiesFromAppointment.Value);
        }

        public static void SetPersisting_TimeRelatedFields(PXCache appointmentEmployeeCache,
                                                           FSSetup fsSetupRow,
                                                           FSSrvOrdType fsSrvOrdType,
                                                           FSAppointment fsAppointmentRow,
                                                           FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsSetupRow == null)
            {
                throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSetup)));
            }

            bool isTAIntegrationActive = fsSetupRow.EnableEmpTimeCardIntegration.Value 
                                            && fsSrvOrdType.CreateTimeActivitiesFromAppointment.Value
                                            && fsAppointmentEmployeeRow.Type == BAccountType.EmployeeType;

            PXPersistingCheck persistingCheckTAIntegration = isTAIntegrationActive ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;

            PXDefaultAttribute.SetPersistingCheck<FSAppointmentEmployee.trackTime>(appointmentEmployeeCache,
                                                                                   fsAppointmentEmployeeRow,
                                                                                   persistingCheckTAIntegration);

            PXDefaultAttribute.SetPersistingCheck<FSAppointmentEmployee.earningType>(appointmentEmployeeCache,
                                                                                     fsAppointmentEmployeeRow,
                                                                                     persistingCheckTAIntegration);
        }

        public static void EnableDisable_StaffRelatedFields(PXCache appointmentEmployeeCache,
                                                            FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (fsAppointmentEmployeeRow == null)
            {
                return;
            }

            bool enableEmployeeRelatedFields = fsAppointmentEmployeeRow.EmployeeID != null;

            PXUIFieldAttribute.SetEnabled<FSAppointmentEmployee.employeeID>
                    (appointmentEmployeeCache, fsAppointmentEmployeeRow, !enableEmployeeRelatedFields);
        }

        public static void EnableDisable_TimeRelatedLogFields(
                                                              PXCache cache, 
                                                              FSAppointmentLog fsLogRow, 
                                                              FSSetup fsSetupRow,
                                                              FSSrvOrdType fsSrvOrdType,
                                                              FSAppointment fsAppointmentRow)
        {
            if (fsSetupRow == null)
            {
                return;
            }

            bool noEmployee = fsLogRow.BAccountID == null 
                                || fsLogRow.BAccountType != BAccountType.EmployeeType;

            bool enableTEFields = fsSetupRow.EnableEmpTimeCardIntegration == true
                                    && fsSrvOrdType.CreateTimeActivitiesFromAppointment == true
                                    && noEmployee == false;

            bool enableDescription = fsLogRow.ItemType == FSAppointmentLog.itemType.Values.Travel;

            if (cache.GetStatus(fsLogRow) != PXEntryStatus.Inserted)
            {
                PXUIFieldAttribute.SetEnabled<FSAppointmentLog.bAccountID>(cache, fsLogRow, noEmployee == false);
            }

            bool enableTravel = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false;

            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.travel>(cache, fsLogRow, enableTravel && fsLogRow.ItemType != FSAppointmentLog.itemType.Values.NonStock);
            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.laborItemID>(cache, fsLogRow, noEmployee == false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.projectTaskID>(cache, fsLogRow, noEmployee == false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.costCodeID>(cache, fsLogRow, noEmployee == false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.trackTime>(cache, fsLogRow, enableTEFields);
            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.earningType>(cache, fsLogRow, enableTEFields);
            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.descr>(cache, fsLogRow, enableDescription);
            PXUIFieldAttribute.SetEnabled<FSAppointmentLog.trackOnService>(cache, fsLogRow, fsLogRow.ItemType != FSAppointmentLog.itemType.Values.NonStock);
        }

        public static void UpdateStaffRelatedUnboundFields(FSAppointmentDet fsAppointmentDetServiceRow,
                                                            AppointmentServiceEmployees_View appointmentEmployees,
                                                            PXSelectBase<FSAppointmentLog> logView,
                                                            int? numEmployeeLinkedToService)
        {
            if (numEmployeeLinkedToService == null || fsAppointmentDetServiceRow.LogRelatedCount == null)
            {
                using (new PXConnectionScope())
                {
                    if (numEmployeeLinkedToService == null)
                    {
                        numEmployeeLinkedToService = appointmentEmployees.Select().AsEnumerable().RowCast<FSAppointmentEmployee>()
                                                     .Where(y => y.ServiceLineRef == fsAppointmentDetServiceRow.LineRef)
                                                     .Count();
                    }

                    if (fsAppointmentDetServiceRow.LogRelatedCount == null)
                    {
                        if (RunningSelectingScope<FSAppointmentLog>.IsRunningSelecting(logView.Cache.Graph) == false)
                        {
                            using (new RunningSelectingScope<FSAppointmentLog>(logView.Cache.Graph))
                            {
                                fsAppointmentDetServiceRow.LogRelatedCount = logView.Select().AsEnumerable().RowCast<FSAppointmentLog>()
                                                                .Where(y => y.DetLineRef == fsAppointmentDetServiceRow.LineRef)
                                                                .Count();
                            }
                        }
                    }
                }
            }

            fsAppointmentDetServiceRow.StaffRelatedCount = numEmployeeLinkedToService;

            if (numEmployeeLinkedToService == 1)
            {
                fsAppointmentDetServiceRow.StaffRelated = true;
            }
            else
            {
                fsAppointmentDetServiceRow.StaffRelated = numEmployeeLinkedToService != 0;
            }
        }

        public static void InsertUpdateDelete_AppointmentDetService_StaffID(PXCache cache,
                                                                            FSAppointmentDet fsAppointmentDetRow,
                                                                            AppointmentServiceEmployees_View appointmentEmployees,
                                                                            int? oldStaffID)
        {
            if (fsAppointmentDetRow.StaffID != null && oldStaffID != null)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow =
                                            appointmentEmployees.Select()
                                                                .RowCast<FSAppointmentEmployee>()
                                                                .Where(_ => _.ServiceLineRef == fsAppointmentDetRow.LineRef
                                                                         && _.EmployeeID == oldStaffID).FirstOrDefault();
                if (fsAppointmentEmployeeRow != null)
                {
                    fsAppointmentEmployeeRow.EmployeeID = fsAppointmentDetRow.StaffID;
                    appointmentEmployees.Update(fsAppointmentEmployeeRow);
                }
            }
            else if (fsAppointmentDetRow.StaffID != null && oldStaffID == null)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee()
                {
                    ServiceLineRef = fsAppointmentDetRow.LineRef,
                    EmployeeID = fsAppointmentDetRow.StaffID
                };

                appointmentEmployees.Insert(fsAppointmentEmployeeRow);
            }
            else
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow =
                            appointmentEmployees.Select()
                                                .RowCast<FSAppointmentEmployee>()
                                                .Where(_ => _.ServiceLineRef == fsAppointmentDetRow.LineRef
                                                         && _.EmployeeID == oldStaffID).FirstOrDefault();

                appointmentEmployees.Delete(fsAppointmentEmployeeRow);
            }
        }

        [Obsolete("EnableDisable_LineType is deprecated, please use the generic methods X_RowSelected and X_SetPersistingCheck instead.")]
        private static void EnableDisable_LineType(PXCache cache, FSAppointmentDet fsAppointmentDetRow, FSSetup fsSetupRow, FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            // TODO: AC-142850
            // Move all these SetEnabled and SetPersistingCheck calls to the new generic method X_RowSelected.
            // Verify if each field is handled by the generic method before moving it.
            // If the generic method already handles a field, check if the conditions to enable/disable
            // and PersistingCheck are the same.
            // DELETE this method when all fields are moved.

            if (fsAppointmentRow == null)
            {
                return;
            }

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
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isFree>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualQty>(cache, fsAppointmentDetRow, SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cache, fsAppointmentDetRow, true);
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

                case ID.LineType_ServiceTemplate.COMMENT:
                case ID.LineType_ServiceTemplate.INSTRUCTION:

                    PXDefaultAttribute.SetPersistingCheck<FSAppointmentDet.tranDesc>(cache, fsAppointmentDetRow, PXPersistingCheck.NullOrBlank);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSAppointmentDet.inventoryID>(cache, fsAppointmentDetRow, PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isFree>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualQty>(cache, fsAppointmentDetRow, false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cache, fsAppointmentDetRow, false);
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
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isFree>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualQty>(cache, fsAppointmentDetRow, SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.projectTaskID>(cache, fsAppointmentDetRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppointmentDet.staffID>(cache, fsAppointmentDetRow, false);

                    break;
            }

            AppointmentCore.EnableDisable_ServiceActualDateTimes(cache, fsAppointmentRow, fsAppointmentDetRow, enable);
        }
        #endregion

        /// <summary>
        /// Returns true if an Appointment [fsAppointmentRow] can be updated based in its status and the status of the Service Order [fsServiceOrderRow].
        /// </summary>
        public static bool CanUpdateAppointment(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if(fsAppointmentRow == null || fsSrvOrdTypeRow == null)
            {
                return false;
            }

            if (fsAppointmentRow.Status == ID.Status_Appointment.CLOSED
                || fsAppointmentRow.Status == ID.Status_Appointment.CANCELED
                || fsSrvOrdTypeRow.Active == false
                || fsAppointmentRow.IsPosted == true)
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

        public static DateTime? GetDateTimeEnd(DateTime? dateTimeBegin, int hour = 0, int minute = 0, int second = 0, int milisecond = 0)
        {
            if (dateTimeBegin != null)
            {
                return new DateTime(dateTimeBegin.Value.Year,
                                    dateTimeBegin.Value.Month,
                                    dateTimeBegin.Value.Day,
                                    hour,
                                    minute,
                                    second,
                                    milisecond);
            }

            return null;
        }

        public static void GetSODetValues<AppointmentDetType, SODetType>(PXCache cacheAppointmentDet,
                                                                         AppointmentDetType fsAppointmentDetRow,
                                                                         FSServiceOrder fsServiceOrderRow,
                                                                         FSAppointment fsAppointmentRow,
                                                                         FSSODet fsSODetRow)
            where AppointmentDetType : FSAppointmentDet, new()
            where SODetType : FSSODet, new()
        {
            if (fsAppointmentDetRow.SODetID == null)
            {
                return;
            }

            PXCache cacheSODet = new PXCache<FSSODet>(cacheAppointmentDet.Graph);

            if (fsSODetRow == null)
            {
                fsSODetRow = PXSelect<FSSODet,
                                 Where<
                                     FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                                 .Select(cacheAppointmentDet.Graph, fsAppointmentDetRow.SODetID);
            }

            if (fsSODetRow != null)
            {
                var graphAppointment = (AppointmentEntry)cacheAppointmentDet.Graph;

                graphAppointment.CopyAppointmentLineValues<AppointmentDetType, FSSODet>(cacheAppointmentDet,
                                                                                        fsAppointmentDetRow,
                                                                                        cacheSODet,
                                                                                        fsSODetRow,
                                                                                        false,
                                                                                        fsSODetRow.TranDate,
                                                                                        ForceFormulaCalculation: false);

                if (SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow))
                {
                    cacheAppointmentDet.SetValueExtIfDifferent<FSAppointmentDet.actualDuration>(fsAppointmentDetRow, 0);
                    cacheAppointmentDet.SetValueExtIfDifferent<FSAppointmentDet.actualQty>(fsAppointmentDetRow, 0m);
                }
                
                if (fsServiceOrderRow.SourceRefNbr != null
                        && fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER
                            && fsAppointmentDetRow.IsPrepaid == true)
                {
                    fsAppointmentDetRow.LinkedDocRefNbr = fsServiceOrderRow.SourceRefNbr;
                    fsAppointmentDetRow.LinkedDocType = fsServiceOrderRow.SourceDocType;
                    fsAppointmentDetRow.LinkedEntityType = FSAppointmentDet.linkedEntityType.Values.SalesOrder;
                }
            }
        }

        public static void ValidateQty(PXCache cache, FSAppointmentDet fsAppointmentDetRow, PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            if (fsAppointmentDetRow.ActualQty < 0)
            {
                PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualQty>(cache, fsAppointmentDetRow, true);
                cache.RaiseExceptionHandling<FSAppointmentDet.actualQty>(fsAppointmentDetRow,
                                                                   null,
                                                                   new PXSetPropertyException(TX.Error.NEGATIVE_QTY, errorLevel));
            }
        }

        /// <summary>
        /// Determines if a Service line has at least one pickup/delivery item related.
        /// </summary>
        public static bool ServiceLinkedToPickupDeliveryItem(PXGraph graph, FSAppointmentDet fsAppointmentDetRow, FSAppointment fsAppointmentRow)
        {
            int srvLinkedCount = PXSelect<FSAppointmentDet,
                                 Where<
                                     FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                 And<
                                     FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                 And<
                                     FSAppointmentDet.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>>>
                                 .Select(graph, fsAppointmentDetRow.SODetID, fsAppointmentRow.AppointmentID)
                                 .Count;

            return srvLinkedCount > 0;
        }

        public static void UpdatePendingPostFlags(FSAppointment fsAppointmentRow,
                                                  FSServiceOrder fsServiceOrder,
                                                  AppointmentDetails_View appointmentDetails)
        {
            int? servicesToPost = appointmentDetails.Select()
                                                    .AsEnumerable()
                                                    .Where(y => ((FSAppointmentDet)y).IsService 
                                                             && ((FSAppointmentDet)y).needToBePosted() == true)
                                                    .RowCast<FSPostInfo>()
                                                    .Where(x => x.isPosted() == false)
                                                    .Count();

            int? partsToPost = appointmentDetails.Select()
                                                 .AsEnumerable()
                                                 .Where(y => ((FSAppointmentDet)y).IsInventoryItem 
                                                          && ((FSAppointmentDet)y).needToBePosted() == true)
                                                 .RowCast<FSPostInfo>()
                                                 .Where(x => x.isPosted() == false)
                                                 .Count();

            int? pickUpDeliverToPost = appointmentDetails.Select()
                                                         .AsEnumerable()
                                                         .Where(y => ((FSAppointmentDet)y).IsPickupDelivery)
                                                         .RowCast<FSPostInfo>()
                                                         .Where(x => x.isPosted() == false)
                                                         .Count();

            if (fsServiceOrder.PostedBy == ID.Billing_By.SERVICE_ORDER)
            {
                fsAppointmentRow.PendingAPARSOPost = false;
                fsAppointmentRow.PendingINPost = false;
                fsAppointmentRow.PostingStatusAPARSO = fsAppointmentRow.PostingStatusAPARSO != ID.Status_Posting.POSTED ? ID.Status_Posting.NOTHING_TO_POST : ID.Status_Posting.POSTED;
                fsAppointmentRow.PostingStatusIN = fsAppointmentRow.PostingStatusIN != ID.Status_Posting.POSTED ? ID.Status_Posting.NOTHING_TO_POST : ID.Status_Posting.POSTED;
            }
            else if ((fsServiceOrder.PostedBy == null || fsServiceOrder.PostedBy == ID.Billing_By.APPOINTMENT)
                          && (servicesToPost > 0 || partsToPost > 0 || pickUpDeliverToPost > 0) 
                          && fsAppointmentRow.PostingStatusAPARSO != ID.Status_Posting.POSTED)
            {
                fsAppointmentRow.PendingAPARSOPost = true;
                fsAppointmentRow.PostingStatusAPARSO = ID.Status_Posting.PENDING_TO_POST;

                if (pickUpDeliverToPost > 0)
                {
                    fsAppointmentRow.PendingINPost = true;
                    //// @TODO: AC-142850 When IN posting is done, uncomment this line
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
                fsAppointmentRow.PostingStatusAPARSO = fsAppointmentRow.PostingStatusAPARSO != ID.Status_Posting.POSTED ? ID.Status_Posting.NOTHING_TO_POST : ID.Status_Posting.POSTED;
                fsAppointmentRow.PostingStatusIN = fsAppointmentRow.PostingStatusIN != ID.Status_Posting.POSTED ? ID.Status_Posting.NOTHING_TO_POST : ID.Status_Posting.POSTED;
            }
        }

        // TODO:
        [Obsolete("Delete this method in the next major release")]
        public static void UpdateApptUsageStatisticsOnSrvOrdLine<SODetType>(PXCache appCache,
                                                                                FSAppointment fsAppointmentRow,
                                                                                PXCache detCache,
                                                                                FSAppointmentDet fsAppointmentDetRow,
                                                                                PXSelectBase<SODetType> viewSODet)
            where SODetType : FSSODet, new()
        {
            //Validating line status
            PXEntryStatus detStatus = detCache.GetStatus(fsAppointmentDetRow);

            if (detStatus != PXEntryStatus.Inserted
                && detStatus != PXEntryStatus.Updated)
            {
                return;
            }

            //Validating Line type
            if (fsAppointmentDetRow.LineType != ID.LineType_ALL.SERVICE
                    && fsAppointmentDetRow.LineType != ID.LineType_ALL.NONSTOCKITEM
                        && fsAppointmentDetRow.LineType != ID.LineType_ALL.INVENTORY_ITEM)
            {
                return;
            }

            SODetType fsSODetRow = (SODetType)fsAppointmentDetRow.FSSODetRow;

            if (fsSODetRow == null)
            {
                return;
            }

            string oldApptStatus = (string)appCache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow);
            string oldLineStatus = (string)detCache.GetValueOriginal<FSAppointmentDet.status>(fsAppointmentDetRow);
            bool apptNotStarted = SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow);

            decimal? _CuryBillableTranAmt = 0m;
            decimal? _ApptQty = 0m;
            int? _ApptDuration = 0;
            int? _ApptEstimatedDuration = 0;
            int? _ApptNumber = 0;

            decimal? _OriCuryBillableTranAmt = (decimal?)detCache.GetValueOriginal<FSAppointmentDet.curyBillableTranAmt>(fsAppointmentDetRow) ?? 0;

            if (_OriCuryBillableTranAmt != fsAppointmentDetRow.CuryBillableTranAmt)
            {
                _CuryBillableTranAmt = fsAppointmentDetRow.CuryBillableTranAmt - (_OriCuryBillableTranAmt ?? 0);
            }

            decimal? _OriApptQty = (decimal?)detCache.GetValueOriginal<FSAppointmentDet.billableQty>(fsAppointmentDetRow) ?? 0;

            if (_OriApptQty != fsAppointmentDetRow.BillableQty)
            {
                _ApptQty = fsAppointmentDetRow.BillableQty - (_OriApptQty ?? 0);
            }

            int? _OriApptDuration = (int?)detCache.GetValueOriginal<FSAppointmentDet.actualDuration>(fsAppointmentDetRow) ?? 0;

            if (_OriApptDuration != fsAppointmentDetRow.ActualDuration)
            {
                _ApptDuration = fsAppointmentDetRow.ActualDuration - (_OriApptDuration ?? 0);
            }

            int? _OriApptEstimatedDuration = (int?)detCache.GetValueOriginal<FSAppointmentDet.estimatedDuration>(fsAppointmentDetRow) ?? 0;

            if (_OriApptEstimatedDuration != fsAppointmentDetRow.EstimatedDuration)
            {
                _ApptEstimatedDuration = fsAppointmentDetRow.EstimatedDuration - (_OriApptEstimatedDuration ?? 0);
            }

            if (fsSODetRow != null)
            {
                bool updateServiceOrder = false;

                switch (detStatus)
                {
                    case PXEntryStatus.Inserted:

                        if (fsAppointmentDetRow.IsCanceledNotPerformed != true)
                        {
                            _CuryBillableTranAmt = fsAppointmentDetRow.CuryBillableTranAmt;
                            _ApptQty = fsAppointmentDetRow.BillableQty;
                            _ApptDuration = fsAppointmentDetRow.ActualDuration;
                            _ApptEstimatedDuration = fsAppointmentDetRow.EstimatedDuration;
                            _ApptNumber = 1;

                            updateServiceOrder = true;
                        }
                        break;
                    case PXEntryStatus.Updated:

                        //Appointment line status change --> CANCELED
                        if (oldLineStatus != fsAppointmentDetRow.Status
                                && fsAppointmentDetRow.IsCanceledNotPerformed == true
                                && (oldLineStatus != ID.Status_AppointmentDet.CANCELED 
                                    && oldLineStatus != ID.Status_AppointmentDet.NOT_PERFORMED
                                    && oldLineStatus != ID.Status_AppointmentDet.RequestForPO))
                        {
                            _CuryBillableTranAmt = -_OriCuryBillableTranAmt;
                            _ApptQty = -_OriApptQty;
                            _ApptDuration = -_OriApptDuration;
                            _ApptEstimatedDuration = -_OriApptEstimatedDuration;
                            _ApptNumber = -1;
                        }

                        //Appointment line status change: CANCELED --> REOPEN
                        if (oldLineStatus != fsAppointmentDetRow.Status
                                && fsAppointmentDetRow.IsCanceledNotPerformed == false
                                && (oldLineStatus == ID.Status_AppointmentDet.CANCELED
                                    || oldLineStatus == ID.Status_AppointmentDet.NOT_PERFORMED
                                    || oldLineStatus == ID.Status_AppointmentDet.RequestForPO))
                        {
                            _CuryBillableTranAmt = fsAppointmentDetRow.CuryBillableTranAmt;
                            _ApptQty = fsAppointmentDetRow.BillableQty;
                            _ApptDuration = fsAppointmentDetRow.ActualDuration;
                            _ApptEstimatedDuration = fsAppointmentDetRow.EstimatedDuration;
                            _ApptNumber = 1;
                        }

                        updateServiceOrder = true;

                        break;
                }
                
                //Appointment status change: XXXX --> REOPEN
                if (oldApptStatus != fsAppointmentRow.Status
                    && apptNotStarted == true
                        && oldLineStatus == fsAppointmentDetRow.Status)
                {
                    _CuryBillableTranAmt = -_OriCuryBillableTranAmt + fsAppointmentDetRow.CuryBillableTranAmt;
                    _ApptQty = -_OriApptQty + fsAppointmentDetRow.BillableQty;
                    _ApptDuration = -_OriApptDuration;
                    _ApptEstimatedDuration = -_OriApptEstimatedDuration + fsAppointmentDetRow.EstimatedDuration;

                    updateServiceOrder = true;
                }

                if (updateServiceOrder)
                {
                    decimal? curyApptTranAmt = fsSODetRow.CuryApptTranAmt + _CuryBillableTranAmt;
                    decimal? apptQty = fsSODetRow.ApptQty + _ApptQty;
                    int? apptDuration = fsSODetRow.ApptDuration + _ApptDuration;
                    int? apptEstimatedDuration = fsSODetRow.ApptEstimatedDuration + _ApptEstimatedDuration;
                    int? apptNumber = fsSODetRow.ApptCntr + _ApptNumber;

                    // Updating Service Order's detail line
                    fsSODetRow.CuryApptTranAmt = curyApptTranAmt;
                    fsSODetRow.ApptQty = apptQty;
                    fsSODetRow.ApptDuration = apptDuration;
                    fsSODetRow.ApptEstimatedDuration = apptEstimatedDuration;
                    fsSODetRow.ApptCntr = apptNumber;

                    viewSODet.Cache.Update(fsSODetRow);

                    if (fsAppointmentDetRow.Status != ID.Status_AppointmentDet.NOT_FINISHED)
                    {
                        viewSODet.Cache.SetDefaultExt<FSSODet.status>(fsSODetRow);
                    }
                }
            }
        }

        // TODO:
        [Obsolete("Delete this method in the next major release")]
        public static void DeleteAppointmentInfoInServiceOrder(PXCache detCache,
                                                               FSAppointmentDet fsAppointmentDetRow,
                                                               AppointmentCore.ServiceOrderRelated_View viewServiceOrderRelated)

        {
            //Validating Operation type
            PXEntryStatus detStatus = detCache.GetStatus(fsAppointmentDetRow);

            if (detStatus != PXEntryStatus.Deleted)
            {
                return;
            }

            //Validating Line type
            if (fsAppointmentDetRow.LineType != ID.LineType_ALL.SERVICE
                    && fsAppointmentDetRow.LineType != ID.LineType_ALL.NONSTOCKITEM
                        && fsAppointmentDetRow.LineType != ID.LineType_ALL.INVENTORY_ITEM)
            {
                return;
            }

            PXGraph tempGraphAppDet = new PXGraph();
            PXGraph tempGraphSODet = new PXGraph();

            FSSODet fsSODetRow = fsAppointmentDetRow.FSSODetRow;

            if (fsSODetRow != null)
            {
                bool updateServiceOrder = false;
                int? sign = 1;

                if (fsAppointmentDetRow.IsCanceledNotPerformed != true)
                {
                    sign = -1;
                    updateServiceOrder = true;
                }

                if (updateServiceOrder)
                {
                    //// Getting Service Order's header
                    FSServiceOrder fsServiceOrderRow = PXSelect<FSServiceOrder,
                                                       Where<
                                                           FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                                       .Select(tempGraphSODet, fsSODetRow.SOID);

                    if (fsServiceOrderRow != null)
                    {
                        decimal? curyApptTranAmt = fsSODetRow.CuryApptTranAmt + sign * fsAppointmentDetRow.CuryBillableTranAmt;
                        decimal? apptQty = fsSODetRow.ApptQty + sign * fsAppointmentDetRow.BillableQty;
                        int? apptDuration = fsSODetRow.ApptDuration + sign * fsAppointmentDetRow.ActualDuration;
                        int? apptEstimatedDuration = fsSODetRow.ApptEstimatedDuration + sign * fsAppointmentDetRow.EstimatedDuration;
                        int? apptCntr = fsSODetRow.ApptCntr + sign;
                        decimal apptTranAmt;
                        string linkedEntityType = fsSODetRow.IsExpenseReceiptItem == true ? null : fsSODetRow.LinkedEntityType;
                        string linkedDocRefNbr = fsSODetRow.IsExpenseReceiptItem == true ? null : fsSODetRow.LinkedDocRefNbr;

                        CM.PXDBCurrencyAttribute.CuryConvBase(viewServiceOrderRelated.Cache, viewServiceOrderRelated.Current, curyApptTranAmt.Value, out apptTranAmt);

                        string newSODetStatus = ServiceOrderCore.GetSODetStatus(fsSODetRow, apptQty, apptEstimatedDuration);

                        //// Updating Service Order's detail line
                        PXUpdate<
                            Set<FSSODet.apptCntr, Required<FSSODet.apptCntr>,
                            Set<FSSODet.apptQty, Required<FSSODet.apptQty>,
                            Set<FSSODet.apptTranAmt, Required<FSSODet.apptTranAmt>,
                            Set<FSSODet.curyApptTranAmt, Required<FSSODet.curyApptTranAmt>,
                            Set<FSSODet.apptDuration, Required<FSSODet.apptDuration>,
                            Set<FSSODet.apptEstimatedDuration, Required<FSSODet.apptEstimatedDuration>,
                            Set<FSSODet.status, Required<FSSODet.status>,
                            Set<FSSODet.linkedEntityType, Required<FSSODet.linkedEntityType>,
                            Set<FSSODet.linkedDocRefNbr, Required<FSSODet.linkedDocRefNbr>>>>>>>>>>,
                        FSSODet,
                        Where<FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                        .Update(tempGraphAppDet,
                                apptCntr, apptQty, apptTranAmt, curyApptTranAmt, apptDuration, apptEstimatedDuration, newSODetStatus, linkedEntityType, linkedDocRefNbr,
                                fsSODetRow.SODetID);

                        //// Updating Service Order's header
                        int? apptDurationTotal = fsServiceOrderRow.ApptDurationTotal + (fsSODetRow.LineType == ID.LineType_ALL.SERVICE ? -fsSODetRow.ApptDuration + apptDuration : 0);
                        decimal? apptOrderTotal = fsServiceOrderRow.ApptOrderTotal - fsSODetRow.ApptTranAmt + apptTranAmt;
                        decimal? curyApptOrderTotal = fsServiceOrderRow.CuryApptOrderTotal - fsSODetRow.CuryApptTranAmt + curyApptTranAmt;
                        bool newAppointmentsNeeded = newSODetStatus == ID.Status_SODet.SCHEDULED_NEEDED ? true : (bool)fsServiceOrderRow.AppointmentsNeeded;

                        PXUpdate<
                            Set<FSServiceOrder.apptDurationTotal, Required<FSServiceOrder.apptDurationTotal>,
                            Set<FSServiceOrder.apptOrderTotal, Required<FSServiceOrder.apptOrderTotal>,
                            Set<FSServiceOrder.curyApptOrderTotal, Required<FSServiceOrder.curyApptOrderTotal>,
                            Set<FSServiceOrder.appointmentsNeeded, Required<FSServiceOrder.appointmentsNeeded>>>>>,
                        FSServiceOrder,
                        Where<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                        .Update(tempGraphSODet,
                                apptDurationTotal, apptOrderTotal, curyApptOrderTotal, newAppointmentsNeeded,
                                fsSODetRow.SOID);

                        detCache.Graph.SelectTimeStamp();
                    }
                }
            }
        }

        public static bool IsAppointmentReadyToBeInvoiced(FSAppointment fsAppointmentRow, FSServiceOrder fsServiceOrderRow, FSBillingCycle fsBillingCycle, FSSrvOrdType fsSrvOrdTypeRow)
        {
            // @TODO: AC-142850 Improve this, is completely unreadable
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

        public static void UpdateSalesOrderByCompletingAppointment(PXGraph graph, string sourceDocType, string sourceRefNbr)
        {
            SOOrder sOOrderRow = PXSelect<SOOrder,
                                 Where<
                                    SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                                 And<
                                    SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                 .Select(graph, sourceDocType, sourceRefNbr);

            if (sOOrderRow == null)
            {
                throw new PXException(TX.Error.SERVICE_ORDER_SOORDER_INCONSISTENCY);
            }

            //Installed flag lift for Sales Order
            PXUpdate<
                Set<FSxSOOrder.installed, True>,
            SOOrder,
            Where<
                SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
            And<
                SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
            .Update(graph, sourceDocType, sourceRefNbr);

            PXResultset<SOOrderShipment> bqlResultSet = PXSelect<SOOrderShipment,
                                                        Where<
                                                            SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
                                                        And<
                                                            SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>
                                                        .Select(graph, sOOrderRow.OrderType, sOOrderRow.OrderNbr);

            foreach (SOOrderShipment soOrderShipmentRow in bqlResultSet)
            {
                //Installed flag lift for the Shipment
                PXUpdate<
                    Set<FSxSOShipment.installed, True>,
                SOShipment,
                Where
                    <SOShipment.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>
                .Update(graph, soOrderShipmentRow.ShipmentNbr);
            }
        }

        public static void SendNotification(PXCache cache, FSAppointment fsAppointmentRow, string mailing, int? branchID, IList<Guid?> attachments = null)
        {
            AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

            graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);

            PXLongOperation.StartOperation(cache.Graph,
            delegate
            {
                graphAppointmentEntry.SendNotification(graphAppointmentEntry, cache, mailing, branchID, attachments);
            });
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
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.isFree>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.actualQty>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.estimatedQty>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.curyUnitPrice>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.projectTaskID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.siteID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.siteLocationID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.acctID>(cacheAppointmentDet, fsAppointmentDetRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointmentDet.subID>(cacheAppointmentDet, fsAppointmentDetRow, false);
        }

        public static void FSAppointmentDet_RowSelected_PartialHandler(PXCache cacheAppointmentDet,
                                                                       FSAppointmentDet fsAppointmentDetRow,
                                                                       FSSetup fsSetupRow,
                                                                       FSSrvOrdType fsSrvOrdTypeRow,
                                                                       FSServiceOrder fsServiceOrderRow,
                                                                       FSAppointment fsAppointmentRow = null)
        {
            // @TODO: AC-142850 Evaluate if usage of deprecated function can be avoided already
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

        public static void FSAppointmentDet_RowPersisting_PartialHandler(PXCache cacheAppointmentDet,
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

        public static void RefreshSalesPricesInTheWholeDocument(PXSelectBase<FSAppointmentDet> appointmentDetails)
        {
            // TODO: AC-142850
            // This method should run and depend on BillCustomerID changes, and not on CustomerID changes
            // Besides that, check if this is necessary using the Sales-Price graph extension
            foreach (FSAppointmentDet row in appointmentDetails.Select())
            {
                appointmentDetails.Cache.SetDefaultExt<FSAppointmentDet.curyUnitPrice>(row);
                appointmentDetails.Cache.Update(row);
            }
        }
        #endregion
        #endregion
    }
}
