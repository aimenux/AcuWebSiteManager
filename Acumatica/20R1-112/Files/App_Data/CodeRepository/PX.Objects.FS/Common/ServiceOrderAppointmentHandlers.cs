using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using System.Runtime.CompilerServices;

namespace PX.Objects.FS
{
    public static class ServiceOrderAppointmentHandlers
    {
        public abstract class fakeField : PX.Data.IBqlField { }

        public enum EventType
        {
            RowSelectedEvent,
            RowPersistingEvent
        }

        /// <summary>
        /// Calls SetEnabled and SetPersistingCheck for the specified field depending on the event that is running.
        /// </summary>
        /// <typeparam name="Field">The field to set properties.</typeparam>
        /// <param name="cache">The cache that is executing the event.</param>
        /// <param name="row">The row for which the event is executed.</param>
        /// <param name="eventType">The type of the event that is running.</param>
        /// <param name="isEnabled">True to enable the field. False to disable it.</param>
        /// <param name="persistingCheck">
        /// <para>The type of PersistingCheck for the field.</para>
        /// <para>Pass NULL if you don't want to set the PersistingCheck property for the field.</para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEnabledAndPersistingCheck<Field>(PXCache cache,
                                                               object row,
                                                               EventType eventType,
                                                               bool isEnabled,
                                                               PXPersistingCheck? persistingCheck)
            where Field : class, IBqlField
        {
            if (eventType == EventType.RowSelectedEvent)
            {
                PXUIFieldAttribute.SetEnabled<Field>(cache, row, isEnabled);
            }

            if (persistingCheck != null)
            {
                PXDefaultAttribute.SetPersistingCheck<Field>(cache, row, (PXPersistingCheck)persistingCheck);
            }
        }

        public static void X_RowSelected<DAC>(PXCache cache,
                                              PXRowSelectedEventArgs e,
                                              FSServiceOrder fsServiceOrderRow,
                                              FSSrvOrdType fsSrvOrdTypeRow,
                                              bool disableSODetReferenceFields,
                                              bool docAllowsActualFieldEdition)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            X_RowSelected<DAC>(cache,
                               e.Row,
                               EventType.RowSelectedEvent,
                               fsServiceOrderRow,
                               fsSrvOrdTypeRow,
                               disableSODetReferenceFields,
                               docAllowsActualFieldEdition);
        }

        public static void X_SetPersistingCheck<DAC>(PXCache cache,
                                                     PXRowPersistingEventArgs e,
                                                     FSServiceOrder fsServiceOrderRow,
                                                     FSSrvOrdType fsSrvOrdTypeRow)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            X_RowSelected<DAC>(cache,
                               e.Row,
                               EventType.RowPersistingEvent,
                               fsServiceOrderRow,
                               fsSrvOrdTypeRow,
                               disableSODetReferenceFields: true,
                               docAllowsActualFieldEdition: false);
        }

        public static void X_LineType_FieldUpdated<DAC>(PXCache cache, PXFieldUpdatedEventArgs e)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            Type dacType = typeof(DAC);
            FSSODet fsSODetRow = null;
            FSAppointmentDet fsAppointmentDetRow = null;

            if (dacType == typeof(FSAppointmentDet))
            {
                fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            }
            else
            {
                fsSODetRow = (FSSODet)e.Row;
            }


            if (IsInventoryLine(row.LineType) == false)
            {
                // Clear fields for non-inventory lines
                row.IsBillable = false;
                row.ManualPrice = false;

                row.InventoryID = null;
                row.SubItemID = null;

                row.BillingRule = ID.BillingRule.NONE;
                cache.SetDefaultExt<FSSODet.uOM>(e.Row);

                row.CuryUnitPrice = 0;
                row.SetDuration(FieldType.EstimatedField, 0, cache, false);
                row.SetQty(FieldType.EstimatedField, 0, cache, false);
                row.SiteID = null;
                row.SiteLocationID = null;

                row.ProjectTaskID = null;

                row.AcctID = null;
                row.SubID = null;

                if (fsAppointmentDetRow != null)
                {
                    fsAppointmentDetRow.ActualDuration = 0;
                    fsAppointmentDetRow.Qty = 0;
                }

                if (fsSODetRow != null)
                {
                    fsSODetRow.EnablePO = false;
                    fsSODetRow.POVendorID = null;
                    fsSODetRow.POVendorLocationID = null;
                }

                return;
            }

            // Set default values for common fields of FSSODet and FSAppointmentDet
            cache.SetDefaultExt<FSSODet.isBillable>(e.Row);
            cache.SetDefaultExt<FSSODet.projectTaskID>(e.Row);

            // Set default values for specific fields of FSSODet
            if (fsSODetRow != null)
            {
                cache.SetDefaultExt<FSSODet.enablePO>(e.Row);
            }

            // Set default values for specific fields of FSAppointmentDet
            if (fsAppointmentDetRow != null)
            {
            }
        }

        private static void X_RowSelected<DAC>(PXCache cache,
                                               object eRow,
                                               EventType eventType,
                                               FSServiceOrder fsServiceOrderRow,
                                               FSSrvOrdType fsSrvOrdTypeRow,
                                               bool disableSODetReferenceFields,
                                               bool docAllowsActualFieldEdition)
                where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            if (eRow == null)
            {
                return;
            }

            var row = (DAC)eRow;

            Type dacType = typeof(DAC);
            FSSODet fsSODetRow = null;
            bool calledFromSO = false;
            bool calledFromAPP = false;
            FSAppointmentDet fsAppointmentDetRow = null;

            if (dacType == typeof(FSAppointmentDet))
            {
                fsAppointmentDetRow = (FSAppointmentDet)eRow;
                calledFromAPP = true;
            }
            else
            {
                fsSODetRow = (FSSODet)eRow;
                calledFromSO = true;
            }

            bool isEnabled;
            PXPersistingCheck persistingCheck;
            bool isInventoryLine = IsInventoryLine(row.LineType);
            bool isStockItem;
            bool isPickupDeliveryItem;

            if (isInventoryLine == false)
            {
                isStockItem = false;
            }
            else
            {
                if (row.LineType != ID.LineType_ALL.SERVICE
                        && row.LineType != ID.LineType_ALL.NONSTOCKITEM)
                {
                    isStockItem = true;
                }
                else
                {
                    isStockItem = false;
                }
            }

            isPickupDeliveryItem = row.LineType == ID.LineType_ALL.PICKUP_DELIVERY;

            // Enable/Disable SODetID
            SetEnabledAndPersistingCheck<FSSODet.sODetID>(cache, eRow, eventType,
                                            isEnabled: !disableSODetReferenceFields && !isPickupDeliveryItem, persistingCheck: null);

            // Enable/Disable LineType
            SetEnabledAndPersistingCheck<FSSODet.lineType>(cache, eRow, eventType,
                                            isEnabled: !disableSODetReferenceFields, persistingCheck: null);

            // Set InventoryID properties (SetEnabled and SetPersistingCheck)
            isEnabled = true;
            persistingCheck = PXPersistingCheck.NullOrBlank;

            if (isInventoryLine == false)
            {
                isEnabled = false;
                persistingCheck = PXPersistingCheck.Nothing;
            }
            else if (row.IsPrepaid == true
                    || (disableSODetReferenceFields == true && isPickupDeliveryItem == false)
                    || (calledFromSO == true && fsServiceOrderRow.AllowInvoice == true))
            {
                isEnabled = false;
            }

            SetEnabledAndPersistingCheck<FSSODet.inventoryID>(cache, eRow, eventType, isEnabled, persistingCheck);

            if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                // Set SubItemID properties BASED ON InventoryID and LineType
                SetEnabledAndPersistingCheck<FSSODet.subItemID>(cache, eRow, eventType,
                                                isEnabled: isEnabled && isStockItem,
                                                persistingCheck: isStockItem == true ? persistingCheck : PXPersistingCheck.Nothing);
            }

            // Set UOM properties SAME AS InventoryID
            SetEnabledAndPersistingCheck<FSSODet.uOM>(cache, eRow, eventType, isEnabled, persistingCheck);

            // Enable/Disable billingRule
            isEnabled = false;

            if ((row.LineType == ID.LineType_ALL.SERVICE)
                && row.IsPrepaid == false
                && (fsSODetRow == null || fsSODetRow.Mem_LastReferencedBy == null)
                && (calledFromAPP ==  true || (calledFromSO == true && fsServiceOrderRow?.AllowInvoice == false)))
            {
                isEnabled = true;
            }

            SetEnabledAndPersistingCheck<FSSODet.billingRule>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            // Enable/Disable ManualPrice
            isEnabled = true;

            if (row.IsPrepaid == true
                || isInventoryLine == false
                || row.InventoryID == null
                || (calledFromSO && fsServiceOrderRow?.AllowInvoice == true))
            {
                isEnabled = false;
            }

            isEnabled = isEnabled && (fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS || (fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS && fsSrvOrdTypeRow.BillingType != ID.SrvOrdType_BillingType.COST_AS_COST));

            SetEnabledAndPersistingCheck<FSSODet.manualPrice>(cache, eRow, eventType,
                                            isEnabled, persistingCheck: null);

            // Set IsBillable properties (SetEnabled and SetPersistingCheck)
            // Enable/Disable IsBillable Same as ManualPrice
            SetEnabledAndPersistingCheck<FSSODet.isBillable>(cache, eRow, eventType,
                                            isEnabled && row.ContractRelated == false, persistingCheck: null);

            // Enable/Disable Discount Fields
            isEnabled = true;

            if (row.BillingRule == ID.BillingRule.NONE == true
                || row.IsPrepaid == true
                || row.IsBillable == false
                || row.ContractRelated == true
                || isInventoryLine == false
                || row.InventoryID == null
                || (calledFromSO && fsServiceOrderRow?.AllowInvoice == true))
            {
                isEnabled = false;
            }
            
            isEnabled = isEnabled && (fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS || (fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS && fsSrvOrdTypeRow.BillingType != ID.SrvOrdType_BillingType.COST_AS_COST));

            SetEnabledAndPersistingCheck<FSSODet.discPct>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            SetEnabledAndPersistingCheck<FSSODet.curyBillableExtPrice>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            SetEnabledAndPersistingCheck<FSSODet.curyDiscAmt>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            // Enable/Disable CuryUnitPrice
            isEnabled = false;

            if (row.BillingRule != ID.BillingRule.NONE 
                && row.IsPrepaid == false
                && row.ContractRelated == false
                && row.InventoryID != null 
                && (calledFromAPP == true || (calledFromSO == true && fsServiceOrderRow?.AllowInvoice == false))
                && (fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS || (fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS && fsSrvOrdTypeRow.BillingType != ID.SrvOrdType_BillingType.COST_AS_COST)))
            {
                isEnabled = true;
            }
            
            SetEnabledAndPersistingCheck<FSSODet.curyUnitPrice>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            // Enable/Disable EstimatedDuration and ActualDuration
            isEnabled = false;

            if ((row.LineType == ID.LineType_ALL.SERVICE || row.LineType == ID.LineType_ALL.NONSTOCKITEM)
                && row.InventoryID != null
                && (calledFromAPP ==  true || (calledFromSO == true && fsServiceOrderRow?.AllowInvoice == false)))
            {
                isEnabled = true;
            }

            SetEnabledAndPersistingCheck<FSSODet.estimatedDuration>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            if (fsAppointmentDetRow != null)
            {
                //Enable when there is only 1 or 0 staff related to the service
                bool enableByLogRelated = fsAppointmentDetRow.LogRelatedCount == 0;
                SetEnabledAndPersistingCheck<FSAppointmentDet.actualDuration>(cache, eRow, eventType,
                                            isEnabled: isEnabled && docAllowsActualFieldEdition && enableByLogRelated, persistingCheck: null);
            }


            // Enable/Disable EstimatedQty and ActualQty
            isEnabled = false;

            if (isInventoryLine == true
                && row.BillingRule != ID.BillingRule.TIME
                && row.IsPrepaid == false
                && row.InventoryID != null
                && (fsAppointmentDetRow == null || (string.IsNullOrEmpty(fsAppointmentDetRow.LotSerialNbr) == true 
                                                        || (string.IsNullOrEmpty(fsAppointmentDetRow.LotSerialNbr) == false
                                                            && fsAppointmentDetRow.LotSerTrack != INLotSerTrack.SerialNumbered)))
                && (calledFromAPP == true || (calledFromSO == true && fsServiceOrderRow?.AllowInvoice == false)))
            {
                isEnabled = true;
            }

            SetEnabledAndPersistingCheck<FSSODet.estimatedQty>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            if (fsAppointmentDetRow != null)
            {
                SetEnabledAndPersistingCheck<FSAppointmentDet.qty>(cache, eRow, eventType,
                                            isEnabled: isEnabled && docAllowsActualFieldEdition, persistingCheck: null);
            }

            // Set SiteID properties (SetEnabled and SetPersistingCheck)
            isEnabled = false;
            persistingCheck = PXPersistingCheck.Nothing;

            if (row.InventoryID != null
                    && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE)
            {
                if (row.IsPrepaid == false)
                {
                    if (fsAppointmentDetRow != null)
                    {
                        //Disable if line exists in more than 1 appointment. 
                        FSSODet fsSODetSelectorRow = (FSSODet)PXSelectorAttribute.Select<FSAppointmentDet.sODetID>(cache, fsAppointmentDetRow);
                        isEnabled = fsSODetSelectorRow == null
                                        || (fsSODetSelectorRow.ApptCntr == 1 && fsAppointmentDetRow.AppDetID > 0);
                    }
                    else
                    {
                        isEnabled = true;
                    }
                }
                
                persistingCheck = PXPersistingCheck.NullOrBlank;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
            {
                SetEnabledAndPersistingCheck<FSSODet.siteID>(cache, eRow, eventType, isEnabled, persistingCheck);

                // LocationID is always disabled for non-stocks items
                // in other case is enabled/disabled as SiteID
                SetEnabledAndPersistingCheck<FSSODet.siteLocationID>(cache, eRow, eventType, isStockItem && isEnabled, persistingCheck);
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
            {
                // Set ProjectID properties (SetEnabled and SetPersistingCheck)
                isEnabled = false;
                persistingCheck = PXPersistingCheck.Nothing;

                if (isInventoryLine == true && row.InventoryID != null)
                {
                    isEnabled = true;
                    persistingCheck = PXPersistingCheck.NullOrBlank;
                }

                SetEnabledAndPersistingCheck<FSSODet.projectID>(cache, eRow, eventType, isEnabled, persistingCheck);

                // Set ProjectTaskID properties (SetEnabled and SetPersistingCheck)
                isEnabled = true;
                persistingCheck = PXPersistingCheck.Nothing;

                if (isInventoryLine == false || row.InventoryID == null || row.ContractRelated == true)
                {
                    isEnabled = false;
                }
                else if (ProjectDefaultAttribute.IsProject(cache.Graph, row.ProjectID) == true)
                {
                    persistingCheck = PXPersistingCheck.NullOrBlank;
                }

                SetEnabledAndPersistingCheck<FSSODet.projectTaskID>(cache, eRow, eventType, isEnabled, persistingCheck);
            }

            // Set CostCodeID properties (SetEnabled and SetPersistingCheck)
            isEnabled = true;
            persistingCheck = PXPersistingCheck.Nothing;

            if (isInventoryLine == false || row.InventoryID == null || row.ContractRelated == true)
            {
                isEnabled = false;
            }
            else if (ProjectDefaultAttribute.IsProject(cache.Graph, row.ProjectID) == true)
            {
                persistingCheck = PXPersistingCheck.NullOrBlank;
            }

            SetEnabledAndPersistingCheck<FSSODet.costCodeID>(cache, eRow, eventType, isEnabled, persistingCheck);

            // Set AcctID properties (SetEnabled and SetPersistingCheck)
            isEnabled = false;
            persistingCheck = PXPersistingCheck.Nothing;

            if (isInventoryLine == true 
                && row.InventoryID != null 
                && fsServiceOrderRow?.Quote == false 
                && fsSrvOrdTypeRow?.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
                isEnabled = true;
                persistingCheck = PXPersistingCheck.NullOrBlank;
            }

            SetEnabledAndPersistingCheck<FSSODet.acctID>(cache, eRow, eventType, isEnabled, persistingCheck);

            // Set SubID properties SAME AS AcctID
            SetEnabledAndPersistingCheck<FSSODet.subID>(cache, eRow, eventType, isEnabled, persistingCheck);

            // Set PickupDeliveryServiceID properties (SetEnabled and SetPersistingCheck)
            if (fsAppointmentDetRow != null)
            {
                isEnabled = false;
                persistingCheck = PXPersistingCheck.Nothing;

                if (isPickupDeliveryItem)
                {
                    isEnabled = true;
                    persistingCheck = PXPersistingCheck.NullOrBlank;
                }

                SetEnabledAndPersistingCheck<FSAppointmentDet.pickupDeliveryAppLineRef>(cache, eRow, eventType, isEnabled: isEnabled, persistingCheck: persistingCheck);
            }

            // Set LotSerial properties (SetEnabled and SetPersistingCheck)
            if (fsAppointmentDetRow != null)
            {
                isEnabled = false;
                persistingCheck = PXPersistingCheck.Nothing;

                if (row.LineType == ID.LineType_ALL.INVENTORY_ITEM)
                {
                    isEnabled = true;
                }

                SetEnabledAndPersistingCheck<FSSODet.lotSerialNbr>(cache, eRow, eventType, isEnabled, persistingCheck);
            }

            // Set Tax Category (SetEnabled and SetPersistingCheck)
            isEnabled = true;

            if (row.IsPrepaid == true)
            {
                isEnabled = false;
            }

            SetEnabledAndPersistingCheck<FSSODet.taxCategoryID>(cache, eRow, eventType, isEnabled, persistingCheck: null);
     
            // Disable field in Pickup Delivery item
            if (isPickupDeliveryItem == true)
            {
                isEnabled = false;
                persistingCheck = PXPersistingCheck.Nothing;

                // Set StaffID (SetEnabled and SetPersistingCheck)
                SetEnabledAndPersistingCheck<FSSODet.staffID>(cache, eRow, eventType, isEnabled, persistingCheck: null);
            }

            // Set TranDesc properties (SetEnabled and SetPersistingCheck)
            isEnabled = true;
            persistingCheck = PXPersistingCheck.Nothing;

            if (isInventoryLine == false)
            {
                persistingCheck = PXPersistingCheck.NullOrBlank;
            }

            SetEnabledAndPersistingCheck<FSSODet.tranDesc>(cache, eRow, eventType, isEnabled, persistingCheck);

            // Set TaxCategory properties (SetEnabled and SetPersistingCheck)
            isEnabled = false;

            if (row.InventoryID != null && isInventoryLine == true)
            {
                isEnabled = true;
            }

            SetEnabledAndPersistingCheck<FSSODet.taxCategoryID>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            isEnabled = false;
            persistingCheck = PXPersistingCheck.Nothing;

            if (fsSODetRow != null && fsSODetRow.EnablePO == true)
            {
                persistingCheck = PXPersistingCheck.NullOrBlank;
                isEnabled = true;
            }

            isEnabled = isEnabled && (fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS || (fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS && fsSrvOrdTypeRow.BillingType != ID.SrvOrdType_BillingType.COST_AS_COST));

            SetEnabledAndPersistingCheck<FSSODet.curyUnitCost>(cache, eRow, eventType, isEnabled, persistingCheck);

            isEnabled = false;

            if (row.InventoryID != null && row.LineType == ID.LineType_ALL.INVENTORY_ITEM)
            {
                isEnabled = true;
            }

            SetEnabledAndPersistingCheck<FSSODet.equipmentAction>(cache, eRow, eventType, isEnabled, persistingCheck: null);

            SharedFunctions.UpdateEquipmentAction(cache, eRow);

            // Additional Enable/Disable for Equipment fields
            if ((calledFromSO == true && fsServiceOrderRow?.AllowInvoice == true))
            {
                isEnabled = false;
                SetEnabledAndPersistingCheck<FSSODet.SMequipmentID>(cache, eRow, eventType, isEnabled, persistingCheck: null);
            }

            // Enable/Disable PO fields
            isEnabled = false;

            if (isInventoryLine == true
                    && row.IsPrepaid == false
                    && (fsSrvOrdTypeRow.PostTo == ID.Batch_PostTo.SO ||
                        fsSrvOrdTypeRow.PostTo == ID.Batch_PostTo.SI ||
                        fsSrvOrdTypeRow.PostTo == ID.Batch_PostTo.PM))
            {
                isEnabled = true;
            }

            SetEnabledAndPersistingCheck<FSSODet.enablePO>(cache, eRow, eventType, isEnabled, persistingCheck: null);
            SetEnabledAndPersistingCheck<FSSODet.pOCreate>(cache, eRow, eventType, isEnabled, persistingCheck: null);
            SetEnabledAndPersistingCheck<FSSODet.poVendorID>(cache, eRow, eventType, isEnabled && row.EnablePO == true, persistingCheck: null);
            SetEnabledAndPersistingCheck<FSSODet.poVendorLocationID>(cache, eRow, eventType, isEnabled && row.EnablePO == true, persistingCheck: null);
        }

        public static void X_IsPrepaid_FieldUpdated<DAC,
                                                    ManualPrice,
                                                    IsBillable,
                                                    EstimatedDuration,
                                                    ActualDuration>(PXCache cache,
                                                                    PXFieldUpdatedEventArgs e,
                                                                    bool useActualField)
            where DAC : class, IBqlTable, IFSSODetBase, new()
            where ManualPrice : class, IBqlField
            where IsBillable : class, IBqlField
            where EstimatedDuration : class, IBqlField
            where ActualDuration : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            if (row.IsPrepaid == true)
            {
                cache.SetValueExt<ManualPrice>(e.Row, true);
                cache.SetValueExt<IsBillable>(e.Row, false);

                cache.RaiseFieldUpdated<EstimatedDuration>(e.Row, 0);

                if (useActualField == true)
                {
                    cache.RaiseFieldUpdated<ActualDuration>(e.Row, 0);
                }
            }
        }

        public static void X_InventoryID_FieldUpdated<DAC,
                                                      SubItemID,
                                                      SiteID,
                                                      SiteLocationID,
                                                      UOM,
                                                      EstimatedDuration,
                                                      EstimatedQty,
                                                      BillingRule,
                                                      ActualDuration,
                                                      ActualQty>(PXCache cache,
                                                                 PXFieldUpdatedEventArgs e,
                                                                 int? branchLocationID,
                                                                 Customer billCustomerRow,
                                                                 bool useActualFields)
            where DAC : class, IBqlTable, IFSSODetBase, new()
            where SubItemID : class, IBqlField
            where SiteID : class, IBqlField
            where SiteLocationID : class, IBqlField
            where UOM : class, IBqlField
            where EstimatedDuration : class, IBqlField
            where EstimatedQty : class, IBqlField
            where BillingRule : class, IBqlField
            where ActualDuration : class, IBqlField
            where ActualQty : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            // This is required in Inventory FieldUpdated events
            if (e.ExternalCall)
            {
                row.CuryUnitPrice = 0m;
            }

            if (IsInventoryLine(row.LineType) == false
                    || (row.InventoryID == null && row.LineType != ID.LineType_ALL.PICKUP_DELIVERY))
            {
                // Clear fields for non-inventory lines
                row.IsBillable = false;
                row.ManualPrice = false;

                row.TranDesc = null;
                row.SubItemID = null;
                row.SiteID = null;
                row.SiteLocationID = null;

                cache.SetDefaultExt<FSSODet.uOM>(e.Row);

                cache.RaiseExceptionHandling<UOM>(e.Row, null, null);

                row.SetDuration(FieldType.EstimatedField, 0, cache, false);
                row.SetQty(FieldType.EstimatedField, 0, cache, false);

                if (useActualFields == true)
                {
                    row.SetDuration(FieldType.ActualField, 0, cache, false);
                    row.SetQty(FieldType.ActualField, 0, cache, false);
                }

                row.BillingRule = ID.BillingRule.NONE;

                return;
            }

            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, row.InventoryID);

            row.TranDesc = null;
            if (inventoryItemRow != null)
            {
                row.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(cache.Graph.Caches[typeof(InventoryItem)], inventoryItemRow, "Descr", billCustomerRow?.LocaleName);
            }

            FSxService fsxServiceRow = null;

            // UOM is assigned to null here to avoid price calculation while duration and qty fields are defaulted.
            row.UOM = null;
            cache.RaiseExceptionHandling<UOM>(e.Row, null, null);

            if (row.LineType == ID.LineType_ALL.SERVICE && inventoryItemRow != null)
            {
                fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

                cache.SetValueExt<BillingRule>(e.Row, fsxServiceRow.BillingRule);
            }
            else
            {
                cache.SetValueExt<BillingRule>(e.Row, ID.BillingRule.FLAT_RATE);
            }

            cache.SetDefaultExt<SubItemID>(e.Row);

            object newValue = null;
            cache.RaiseFieldDefaulting<SiteID>(e.Row, out newValue);
            int? defaultSiteID = (int?)newValue;
            bool validateSiteID = true;

            if (inventoryItemRow != null && row.SiteLocationID == null && row.LineType == ID.LineType_ALL.INVENTORY_ITEM)
            { 
                if (defaultSiteID == null)
                {
                    defaultSiteID = inventoryItemRow.DfltSiteID;
                }

                if (defaultSiteID == null)
                {
                    //TODO: this query may return multiple records
                    INItemSite inItemSiteRow = PXSelectJoin<INItemSite,
                                                    InnerJoin<INSite,
                                                        On<INSite.siteID, Equal<INItemSite.siteID>>>,
                                               Where<
                                                    INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                                                    And<Match<INSite, Current<AccessInfo.userName>>>>>
                                               .Select(cache.Graph, inventoryItemRow.InventoryID);
                    if (inItemSiteRow != null)
                    {
                        defaultSiteID = inItemSiteRow.SiteID;
                        validateSiteID = false;
                    }
                }
            }

            int? defaultSubItemID = row.SubItemID;
            newValue = null;
            cache.RaiseFieldDefaulting<UOM>(e.Row, out newValue);
            string defaultUOM = (string)newValue;

            CompleteItemInfoUsingBranchLocation(cache.Graph,
                                                branchLocationID,
                                                inventoryItemRow != null ? inventoryItemRow.DefaultSubItemOnEntry : false,
                                                ref defaultSubItemID,
                                                ref defaultUOM,
                                                ref defaultSiteID);
            row.SubItemID = defaultSubItemID;

            string postTo = string.Empty;
            if (cache.Graph is ServiceOrderEntry)
            {
                ServiceOrderEntry graph = cache.Graph as ServiceOrderEntry;
                postTo = graph.ServiceOrderTypeSelected.Current?.PostTo;
            }
            else if (cache.Graph is AppointmentEntry)
            {
                AppointmentEntry graph = cache.Graph as AppointmentEntry;
                postTo = graph.ServiceOrderTypeSelected.Current?.PostTo;
            }

            if (validateSiteID)
            {
                defaultSiteID = GetValidatedSiteID(cache.Graph, defaultSiteID);
            }

            if (defaultSiteID != null && postTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE)
            {
                cache.SetValueExt<SiteID>(e.Row, defaultSiteID);
            }

            if (row.SiteLocationID == null && inventoryItemRow != null && postTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE)
            {
                if (row.SiteID == inventoryItemRow.DfltSiteID && row.LineType == ID.LineType_ALL.INVENTORY_ITEM)
                {
                    row.SiteLocationID = inventoryItemRow.DfltShipLocationID;
                }
            }

            cache.SetValueExt<UOM>(e.Row, defaultUOM);

            row.SetDuration(FieldType.EstimatedField, 0, cache, false);

            // EstimatedQty MUST be assigned after BillingRule BUT before EstimatedDuration.
            cache.SetDefaultExt<EstimatedQty>(e.Row);

            if (row.LineType == ID.LineType_ALL.SERVICE && fsxServiceRow != null)
            {
                cache.SetValueExt<EstimatedDuration>(e.Row, fsxServiceRow.EstimatedDuration ?? 0);
            }

            if (useActualFields == true)
            {
                cache.SetDefaultExt<ActualQty>(e.Row);
                cache.SetDefaultExt<ActualDuration>(e.Row);
            }
        }

        public static void X_BillingRule_FieldVerifying<DAC>(PXCache cache, PXFieldVerifyingEventArgs e)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            if (IsInventoryLine(row.LineType) == false
                || row.InventoryID == null)
            {
                e.NewValue = ID.BillingRule.NONE;
            }
            else if (row.LineType == ID.LineType_ALL.NONSTOCKITEM
                    || row.LineType == ID.LineType_ALL.INVENTORY_ITEM
                    || row.LineType == ID.LineType_ALL.PICKUP_DELIVERY)
            {
                e.NewValue = ID.BillingRule.FLAT_RATE;
            }
        }

        public static void X_BillingRule_FieldUpdated<DAC,
                                                      EstimatedDuration,
                                                      ActualDuration,
                                                      CuryUnitPrice>(PXCache cache,
                                                                     PXFieldUpdatedEventArgs e,
                                                                     bool useActualField)
            where DAC : class, IBqlTable, IFSSODetBase, new()
            where EstimatedDuration : class, IBqlField
            where ActualDuration : class, IBqlField
            where CuryUnitPrice : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            // IsFree is an Unbound field so we need to calculate it.
            row.IsFree = IsFree(row.BillingRule, row.ManualPrice, row.LineType);

            if (row.LineType == ID.LineType_ALL.SERVICE && row.BillingRule == ID.BillingRule.TIME)
            {
                cache.RaiseFieldUpdated<EstimatedDuration>(e.Row, 0);

                if (useActualField == true)
                {
                    cache.RaiseFieldUpdated<ActualDuration>(e.Row, 0);
                }
            }
            else
            {
                cache.SetDefaultExt<CuryUnitPrice>(e.Row);
            }
        }

        public static void X_UOM_FieldUpdated<CuryUnitPrice>(PXCache cache, PXFieldUpdatedEventArgs e)
            where CuryUnitPrice : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            cache.SetDefaultExt<CuryUnitPrice>(e.Row);
        }

        [Obsolete("REMOVE THIS METHOD NO LONGER NEEDED")]
        public static void X_SiteID_FieldUpdated<CuryUnitPrice>(PXCache cache, PXFieldUpdatedEventArgs e)
            where CuryUnitPrice : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            cache.SetDefaultExt<CuryUnitPrice>(e.Row);
        }

        public static void X_SiteID_FieldUpdated<CuryUnitPrice, AcctID, SubID>(PXCache cache, PXFieldUpdatedEventArgs e)
                where CuryUnitPrice : class, IBqlField
                where AcctID : class, IBqlField
                where SubID : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            cache.SetDefaultExt<CuryUnitPrice>(e.Row);
            cache.SetDefaultExt<AcctID>(e.Row);

            try
            {
                cache.SetDefaultExt<SubID>(e.Row);
            }
            catch (PXSetPropertyException)
            {
                cache.SetValue<SubID>(e.Row, null);
            }
        }

        public static void X_Qty_FieldUpdated<CuryUnitPrice>(PXCache cache, PXFieldUpdatedEventArgs e)
            where CuryUnitPrice : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            cache.SetDefaultExt<CuryUnitPrice>(e.Row);
        }

        public static void X_ManualPrice_FieldUpdated<DAC, CuryUnitPrice>(PXCache cache, PXFieldUpdatedEventArgs e)
            where DAC : class, IFSSODetBase, new()
            where CuryUnitPrice : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            // IsFree is an Unbound field so we need to calculate it.
            row.IsFree = IsFree(row.BillingRule, row.ManualPrice, row.LineType);

            cache.SetDefaultExt<CuryUnitPrice>(e.Row);
        }

        public static void X_CuryUnitPrice_FieldDefaulting<DAC, CuryUnitPrice>(PXCache cache,
                                                                               PXFieldDefaultingEventArgs e,
                                                                               decimal? qty,
                                                                               DateTime? docDate,
                                                                               FSServiceOrder fsServiceOrderRow,
                                                                               FSAppointment fsAppointmentRow,
                                                                               CurrencyInfo currencyInfo)
            where DAC : class, IBqlTable, IFSSODetBase, new()
            where CuryUnitPrice : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            // TODO: AC-142850 AC-97482
            // FSSODet does not have PriceType nor PriceCode.
            FSAppointmentDet fsAppointmentDetRow = null;
            Type dacType = typeof(DAC);

            if (dacType == typeof(FSAppointmentDet))
            {
                fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            }

            if (row.InventoryID == null ||
                row.UOM == null ||
                    (row.BillingRule == ID.BillingRule.NONE && row.ManualPrice != true))
            {
                // Special cases with price 0
                PXUIFieldAttribute.SetWarning<CuryUnitPrice>(cache, row, null);
                e.NewValue = 0m;

                // TODO: AC-142850 AC-97482
                if (fsAppointmentDetRow != null)
                {
                    fsAppointmentDetRow.PriceType = null;
                    fsAppointmentDetRow.PriceCode = null;
                }
            }
            else if (row.ManualPrice != true && !cache.Graph.IsCopyPasteContext)
            {
                SalesPriceSet salesPriceSet = FSPriceManagement.CalculateSalesPriceWithCustomerContract(
                                                        cache,
                                                        fsServiceOrderRow.ServiceContractID,
                                                        fsAppointmentRow != null ? fsAppointmentRow.BillServiceContractID : fsServiceOrderRow.BillServiceContractID,
                                                        fsAppointmentRow != null ? fsAppointmentRow.BillContractPeriodID : fsServiceOrderRow.BillContractPeriodID,
                                                        fsServiceOrderRow.BillCustomerID,
                                                        fsServiceOrderRow.BillLocationID,
                                                        row.ContractRelated,
                                                        row.InventoryID,
                                                        row.SiteID,
                                                        qty,
                                                        row.UOM,
                                                        (DateTime)(docDate ?? cache.Graph.Accessinfo.BusinessDate),
                                                        row.CuryUnitPrice,
                                                        alwaysFromBaseCurrency: false,
                                                        currencyInfo: currencyInfo.GetCM(),
                                                        catchSalesPriceException: false);

                if (salesPriceSet.ErrorCode == ID.PriceErrorCode.UOM_INCONSISTENCY)
                {
                    InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, row.InventoryID);
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.INVENTORY_ITEM_UOM_INCONSISTENCY, inventoryItemRow.InventoryCD), PXErrorLevel.Error);
                }

                e.NewValue = salesPriceSet.Price ?? 0m;

                if (fsAppointmentDetRow != null)
                {
                    // These fields are just report fields so they wouldn't have associated events
                    // and therefore they don't need be assigned with SetValueExt.
                    fsAppointmentDetRow.PriceType = salesPriceSet.PriceType;
                    fsAppointmentDetRow.PriceCode = salesPriceSet.PriceCode;
                }

                ARSalesPriceMaint.CheckNewUnitPrice<DAC, CuryUnitPrice>(cache, row, salesPriceSet.Price);
            }
            else
            {
                e.NewValue = row.CuryUnitPrice ?? 0m;
                e.Cancel = row.CuryUnitPrice != null;
                return;
            }
        }

        public static void X_Duration_FieldUpdated<DAC, Qty>(PXCache cache, PXFieldUpdatedEventArgs e, int? duration)
            where DAC : class, IBqlTable, IFSSODetBase, new()
            where Qty : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            if (row.LineType == ID.LineType_ALL.SERVICE && row.BillingRule == ID.BillingRule.TIME
                    && row.IsPrepaid == false)
            {
                cache.SetValueExt<Qty>(e.Row, PXDBQuantityAttribute.Round(decimal.Divide((decimal)(duration ?? 0), 60)));
            }
        }

        private static void CompleteItemInfoUsingBranchLocation(PXGraph graph,
                                                                int? branchLocationID,
                                                                bool? defaultSubItemOnEntry,
                                                                ref int? SubItemID,
                                                                ref string UOM,
                                                                ref int? SiteID)
        {
            if (branchLocationID == null)
            {
                return;
            }

            if ((SubItemID == null && defaultSubItemOnEntry == true)
                    || string.IsNullOrEmpty(UOM)
                    || SiteID == null)
            {
                FSBranchLocation fsBranchLocationRow = PXSelect<FSBranchLocation,
                                                       Where<
                                                            FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                                       .Select(graph, branchLocationID);

                if (fsBranchLocationRow != null)
                {
                    if (SubItemID == null && defaultSubItemOnEntry == true)
                    {
                        SubItemID = fsBranchLocationRow.DfltSubItemID;
                    }

                    if (string.IsNullOrEmpty(UOM))
                    {
                        UOM = fsBranchLocationRow.DfltUOM;
                    }

                    if (SiteID == null)
                    {
                        SiteID = fsBranchLocationRow.DfltSiteID;
                    }
                }
            }
        }

        public static bool IsFree(string billingRule, bool? manualPrice, string lineType)
        {
            if (billingRule == ID.BillingRule.NONE && manualPrice != true && IsInventoryLine(lineType) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsInventoryLine(string lineType)
        {
            if (lineType == null
                || lineType == ID.LineType_ALL.COMMENT
                || lineType == ID.LineType_ALL.INSTRUCTION
                || lineType == ID.LineType_ALL.SERVICE_TEMPLATE)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void CheckIfManualPrice<DAC, Qty>(PXCache cache, PXRowUpdatedEventArgs e)
            where DAC : class, IBqlTable, IFSSODetBase, new()
            where Qty : class, IBqlField
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            if ((e.ExternalCall || cache.Graph.IsImport)
                && cache.ObjectsEqual<FSSODet.branchID>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.inventoryID>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.uOM>(e.Row, e.OldRow)
                && cache.ObjectsEqual<Qty>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.siteID>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.manualPrice>(e.Row, e.OldRow)
                && (!cache.ObjectsEqual<FSSODet.curyUnitPrice>(e.Row, e.OldRow) || !cache.ObjectsEqual<FSSODet.curyBillableTranAmt>(e.Row, e.OldRow)))
            {
                row.ManualPrice = true;
            }
        }

        public static void CheckSOIfManualCost(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null && e.Row is FSSODet == false)
            {
                return;
            }

            var row = (FSSODet)e.Row;

            if ((e.ExternalCall || cache.Graph.IsImport)
                && cache.ObjectsEqual<FSSODet.branchID>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.inventoryID>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.uOM>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.siteID>(e.Row, e.OldRow)
                && cache.ObjectsEqual<FSSODet.manualCost>(e.Row, e.OldRow)
                && !cache.ObjectsEqual<FSSODet.curyUnitCost>(e.Row, e.OldRow))
            {
                row.ManualCost = true;
            }
        }

        public static void X_AcctID_FieldDefaulting<DAC>(PXCache cache,
                                                         PXFieldDefaultingEventArgs e,
                                                         FSSrvOrdType fsSrvOrdTypeRow,
                                                         FSServiceOrder fsServiceOrderRow)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            if (e.Row == null || fsSrvOrdTypeRow == null || fsServiceOrderRow == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            if (IsInventoryLine(row.LineType) == false)
            {
                e.NewValue = null;
            }
            else
            {
                e.NewValue = ServiceOrderCore.Get_TranAcctID_DefaultValue(cache.Graph, fsSrvOrdTypeRow.SalesAcctSource, row.InventoryID, row.SiteID, fsServiceOrderRow);
            }
        }

        public static void X_SubID_FieldDefaulting<DAC>(PXCache cache,
                                                        PXFieldDefaultingEventArgs e,
                                                        FSSrvOrdType fsSrvOrdTypeRow,
                                                        FSServiceOrder fsServiceOrderRow)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            if (e.Row == null || fsSrvOrdTypeRow == null || fsServiceOrderRow == null)
            {
                return;
            }

            var row = (DAC)e.Row;

            if (row.AcctID == null)
            {
                return;
            }

            e.NewValue = ServiceOrderCore.Get_IFSSODetBase_SubID_DefaultValue(cache, row, fsServiceOrderRow, fsSrvOrdTypeRow);
        }

        public static void X_UOM_FieldDefaulting<DAC>(PXCache cache, PXFieldDefaultingEventArgs e)
            where DAC : class, IBqlTable, IFSSODetBase, new()
        {
            if (e.Row == null)
            {
                return;
            }

            IFSSODetBase fsSODetBaseRow = (IFSSODetBase)e.Row;

            string returnUOM = ((CommonSetup)PXSelect<CommonSetup>.Select(cache.Graph))?.WeightUOM;

            if (fsSODetBaseRow.InventoryID != null)
            {
                returnUOM = ((InventoryItem)
                              PXSelect<InventoryItem,
                              Where<
                                  InventoryItem.inventoryID, Equal<Required<FSSODet.inventoryID>>>>
                              .Select(cache.Graph, fsSODetBaseRow.InventoryID))?.SalesUnit;
            }

            e.NewValue = returnUOM;
        }

        public static void SetVisiblePODetFields(PXCache cache, bool showPOFields)
        {
            PXUIFieldAttribute.SetVisible<FSSODet.poNbr>(cache, null, showPOFields);
            PXUIFieldAttribute.SetVisible<FSSODet.pOCreate>(cache, null, showPOFields);
            PXUIFieldAttribute.SetVisible<FSSODet.poStatus>(cache, null, showPOFields);
            PXUIFieldAttribute.SetVisible<FSSODet.poVendorID>(cache, null, showPOFields);
            PXUIFieldAttribute.SetVisible<FSSODet.poVendorLocationID>(cache, null, showPOFields);
            PXUIFieldAttribute.SetVisible<FSSODet.enablePO>(cache, null, showPOFields);
            PXUIFieldAttribute.SetVisible<FSSODet.curyUnitCost>(cache, null, showPOFields);
        }
    
        public static Int32? GetValidatedSiteID(PXGraph graph, int? siteID)
        {
            if (siteID != null)
            {
                INSite inSite = PXSelect<INSite,
                                    Where<INSite.siteID, Equal<Required<INSite.siteID>>,
                                        And<Match<INSite, Current<AccessInfo.userName>>>>>.Select(graph, siteID);
                if (inSite != null)
                {
                    return inSite.SiteID;
                }
            }

            return null;
        }
    }
}
