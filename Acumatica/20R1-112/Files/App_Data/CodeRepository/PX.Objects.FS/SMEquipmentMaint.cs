using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class SMEquipmentMaint : PXGraph<SMEquipmentMaint, FSEquipment>
    {
        public SMEquipmentMaint()
            : base()
        {
            if (SetupRecord.Current == null
                    || SetupRecord.Current.EquipmentNumberingID == null)
            {
                throw new PXSetupNotEnteredException(TX.Error.EQUIPMENT_NUMBERING_SEQUENCE_MISSING_IN_X, typeof(FSEquipmentSetup), DACHelper.GetDisplayName(typeof(FSEquipmentSetup)));
            }

            action.AddMenuAction(openEmployeeBoard);
            action.AddMenuAction(openUserCalendar);
            inqueriesMenu.AddMenuAction(targetEquipmentInquiry);
            inqueriesMenu.AddMenuAction(resourceEquipmentInquiry);
        }

        #region Selects

        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<FSAppointment> AppointmentRecords;
        [PXHidden]
        public PXSelect<FSAppointmentDet> AppointmentDetRecords;

        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        [PXViewName(CR.Messages.Answers)]
        public CRAttributeList<FSEquipment> Answers;

        public PXSelect<FSEquipment> EquipmentRecords;

        public PXSelect<FSEquipment,
               Where<
                   FSEquipment.SMequipmentID, Equal<Current<FSEquipment.SMequipmentID>>>>
               EquipmentSelected;

        public PXSelect<FSEquipmentComponent,
               Where<
                     FSEquipmentComponent.SMequipmentID, Equal<Current<FSEquipment.SMequipmentID>>>,
               OrderBy<
                   Asc<FSEquipmentComponent.lineNbr>>>
               EquipmentWarranties;

        public
            PXSelectReadonly<FSEquipmentComponent,
            Where<
                FSEquipmentComponent.SMequipmentID, Equal<Current<Filter.smEquipmentID>>,
                And<FSEquipmentComponent.lineNbr, Equal<Current<Filter.lineNbr>>>>>
            ComponentSelected;

        public PXFilter<RComponent> ReplaceComponentInfo;

        #endregion

        #region CacheAttached
        #region FSEquipment_RefNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Equipment Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorSMEquipmentRefNbr]
        [AutoNumber(typeof(Search<FSSetup.equipmentNumberingID>), typeof(AccessInfo.businessDate))]
        [PX.Data.EP.PXFieldDescription]
        public virtual void FSEquipment_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region BAccount_AcctName
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Name", Enabled = false)]
        protected virtual void BAccount_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_SrvOrdType
        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorSrvOrdTypeNOTQuote]
        protected virtual void FSAppointment_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_SORefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [FSSelectorSORefNbr_Appointment]
        protected virtual void FSAppointment_SORefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_Status
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Status_Appointment.MANUAL_SCHEDULED)]
        [FSAppointment.status.ListAtrribute]
        [PXUIField(DisplayName = "Appointment Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        protected virtual void FSAppointment_Status_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_ActualDateTimeBegin
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Actual Date", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Date", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSAppointment_ActualDateTimeBegin_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_ActualDateTimeEnd
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Actual Date", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual Date End", Visibility = PXUIVisibility.Invisible)]
        protected virtual void FSAppointment_ActualDateTimeEnd_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointmentDet_Status
        [PXDBString(1, IsFixed = true)]
        [FSAppointmentDet.status.ListAtrribute]
        [PXDefault(ID.Status_AppointmentDet.NOT_STARTED)]
        [PXUIField(DisplayName = "Detail Status")]
        protected virtual void FSAppointmentDet_Status_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointmentDet_InventoryID
        [Service(CacheGlobal = false)]
        protected virtual void FSAppointmentDet_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        public class Filter : IBqlTable
        {
            #region SMequipmentID
            public abstract class smEquipmentID : PX.Data.BQL.BqlInt.Field<smEquipmentID> { }

            [PXInt]
            public virtual int? SMequipmentID { get; set; }
            #endregion
            #region LineNbr
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

            [PXInt]
            public virtual int? LineNbr { get; set; }
            #endregion
        }

        public class RComponent : IBqlTable
        {
            #region InstallationDate
            public abstract class installationDate : PX.Data.BQL.BqlDateTime.Field<installationDate> { }

            [PXDate]
            [PXUIField(DisplayName = "Installation Date", Required = true)]
            public virtual DateTime? InstallationDate { get; set; }
            #endregion
            #region SalesDate
            public abstract class salesDate : PX.Data.BQL.BqlDateTime.Field<salesDate> { }

            [PXDate]
            [PXUIField(DisplayName = "Sales Date", Required = true)]
            public virtual DateTime? SalesDate { get; set; }
            #endregion
            #region ComponentID
            public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

            [PXInt]
            [PXDefault(typeof(FSEquipmentComponent.componentID))]
            [FSSelectorComponentIDEquipment]
            [PXUIField(DisplayName = "Component ID", Required = true)]
            public virtual int? ComponentID { get; set; }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

            [PXInt]
            [PXSelector(typeof(
            Search<InventoryItem.inventoryID,
            Where2<
                Where<
                    Current<FSEquipmentComponent.itemClassID>, IsNotNull,
                    And<InventoryItem.itemClassID, Equal<Current<FSEquipmentComponent.itemClassID>>,
                    Or<
                Where<Current<FSEquipmentComponent.itemClassID>, IsNull>>>>,
                And<FSxEquipmentModel.equipmentItemClass, Equal<ListField_EquipmentItemClass.Component>>>>),
            SubstituteKey = typeof(InventoryItem.inventoryCD))]
            [PXUIField(DisplayName = "Inventory ID")]
            public virtual int? InventoryID { get; set; }
            #endregion
        }

        public PXFilter<Filter> compFilter;

        #region Actions

        #region Action Menu
        public PXMenuAction<FSEquipment> action;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
        [PXUIField(DisplayName = "Actions")]
        public virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        #region Inqueries Menu
        public PXMenuAction<FSEquipment> inqueriesMenu;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.InquiriesFolder)]
        [PXUIField(DisplayName = "Inquiries")]
        public virtual IEnumerable InqueriesMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        #region ReplaceComponent
        public PXAction<FSEquipment> replaceComponent;
        [PXUIField(DisplayName = "Replace Component", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ReplaceComponent()
        {
            if (EquipmentWarranties.Current == null)
            {
                return;
            }

            if (EquipmentWarranties.Current.ComponentReplaced != null)
            {
                throw new PXException(TX.Messages.COMPONENT_ALREADY_REPLACED);
            }

            Filter filterRow = new Filter()
            {
                SMequipmentID = EquipmentWarranties.Current.SMEquipmentID,
                LineNbr = EquipmentWarranties.Current.LineNbr
            };

            compFilter.Reset();
            compFilter.Current = compFilter.Insert(filterRow);

            if (ReplaceComponentInfo.AskExt() == WebDialogResult.OK)
            {
                if (IsTheReplacementInfoValid())
                {
                    FSEquipmentComponent fsEquipmentComponentRow = new FSEquipmentComponent()
                    {
                        SalesDate = ReplaceComponentInfo.Current.SalesDate,
                        InstallationDate = ReplaceComponentInfo.Current.InstallationDate,
                        ComponentID = ReplaceComponentInfo.Current.ComponentID,
                        InventoryID = ReplaceComponentInfo.Current.InventoryID,

                        CpnyWarrantyDuration = EquipmentWarranties.Current.CpnyWarrantyDuration,
                        CpnyWarrantyEndDate = EquipmentWarranties.Current.CpnyWarrantyEndDate,
                        CpnyWarrantyType = EquipmentWarranties.Current.CpnyWarrantyType,
                        VendorWarrantyDuration = EquipmentWarranties.Current.VendorWarrantyDuration,
                        VendorWarrantyEndDate = EquipmentWarranties.Current.VendorWarrantyEndDate,
                        VendorWarrantyType = EquipmentWarranties.Current.VendorWarrantyType,
                        VendorID = EquipmentWarranties.Current.VendorID,
                        ItemClassID = EquipmentWarranties.Current.ItemClassID
                    };

                    ApplyComponentReplacement(EquipmentWarranties.Current, fsEquipmentComponentRow);
                }
            }
        }
        #endregion

        #region OpenEmployeeBoard
        public PXAction<FSEquipment> openEmployeeBoard;
        [PXUIField(DisplayName = TX.ActionCalendarBoardAccess.MULTI_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenEmployeeBoard()
        {
            if (EquipmentRecords.Current.CustomerID != null
                    && EquipmentRecords.Current.SMEquipmentID != null)
            {
                KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(typeof(FSEquipment.customerID).Name, EquipmentRecords.Current.CustomerID.Value.ToString()),
                    new KeyValuePair<string, string>(typeof(FSEquipment.SMequipmentID).Name, EquipmentRecords.Current.SMEquipmentID.Value.ToString())
                };

                throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.MULTI_EMPLOYEE_DISPATCH, parameters);
            }
        }
        #endregion

        #region OpenUserCalendar
        public PXAction<FSEquipment> openUserCalendar;
        [PXUIField(DisplayName = TX.ActionCalendarBoardAccess.SINGLE_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenUserCalendar()
        {
            if (EquipmentRecords.Current.CustomerID != null
                    && EquipmentRecords.Current.SMEquipmentID != null)
            {
                KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(typeof(FSEquipment.customerID).Name, EquipmentRecords.Current.CustomerID.Value.ToString()),
                    new KeyValuePair<string, string>(typeof(FSEquipment.SMequipmentID).Name, EquipmentRecords.Current.SMEquipmentID.Value.ToString())
                };

                throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.SINGLE_EMPLOYEE_DISPATCH, parameters);
            }
        }
        #endregion

        #region OpenSource
        public PXAction<FSEquipment> openSource;
        [PXUIField(DisplayName = "Open Source", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual void OpenSource()
        {
            if (EquipmentRecords.Current == null)
            {
                return;
            }

            switch (EquipmentRecords.Current.SourceType)
            {
                case ID.SourceType_Equipment.AR_INVOICE:
                    SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();

                    graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>
                                                            (EquipmentRecords.Current.SourceRefNbr, EquipmentRecords.Current.SourceDocType);

                    throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                case ID.SourceType_Equipment.EP_EQUIPMENT:
                    PX.Objects.EP.EquipmentMaint graphEquipmentMaint = PXGraph.CreateInstance<PX.Objects.EP.EquipmentMaint>();

                    graphEquipmentMaint.Equipment.Current =
                        graphEquipmentMaint.Equipment.Search<EPEquipment.equipmentID>(EquipmentRecords.Current.SourceID);

                    throw new PXRedirectRequiredException(graphEquipmentMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                case ID.SourceType_Equipment.VEHICLE:

                    VehicleMaint graphVehicleMaint = PXGraph.CreateInstance<VehicleMaint>();

                    graphVehicleMaint.EPEquipmentRecords.Current =
                        graphVehicleMaint.EPEquipmentRecords.Search<EPEquipment.equipmentID>(EquipmentRecords.Current.SourceID);

                    throw new PXRedirectRequiredException(graphVehicleMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };

                default:
                    return;
            }
        }
        #endregion

        #region TargetEquipmentInquiry
        public PXAction<FSEquipment> targetEquipmentInquiry;
        [PXUIField(DisplayName = "Target Equipment History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void TargetEquipmentInquiry()
        {
            OpenInquiry();
        }
        #endregion

        #region ResourceEquipmentInquiry
        public PXAction<FSEquipment> resourceEquipmentInquiry;
        [PXUIField(DisplayName = "Resource Equipment History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ResourceEquipmentInquiry()
        {
            if (EquipmentRecords.Current != null && EquipmentRecords.Current.SMEquipmentID != null)
            {
                AppointmentInq graphAppointmentInq = PXGraph.CreateInstance<AppointmentInq>();

                graphAppointmentInq.Filter.Current = graphAppointmentInq.Filter.Insert(new AppointmentInq.AppointmentInqFilter());
                graphAppointmentInq.Filter.Current.SMEquipmentID = EquipmentRecords.Current.SMEquipmentID;

                throw new PXRedirectRequiredException(graphAppointmentInq, null) { Mode = PXBaseRedirectException.WindowMode.New };
            }
        }
        #endregion
        #endregion

        #region Methods
        public virtual EPEquipment GetRelatedEPEquipmentRow(PXGraph graph, int? epEquipmentID)
        {
            return PXSelect<EPEquipment,
                   Where<
                       EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>
                   .Select(graph, epEquipmentID);
        }

        /// <summary>
        /// Allows to enable/disable the vehicle controls depending on the selection of the isVehicle checkbox.
        /// </summary>
        public virtual void EnableDisableVehicleControls(PXCache cache, FSEquipment fsEquipmentRow)
        {
            bool enableVehicleControls = fsEquipmentRow != null && fsEquipmentRow.IsVehicle == true;

            PXUIFieldAttribute.SetEnabled<FSEquipment.axles>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.fuelType>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.fuelTank1>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.fuelTank2>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.grossVehicleWeight>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.maxMiles>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.tareWeight>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.weightCapacity>(cache, fsEquipmentRow, enableVehicleControls);
            PXUIFieldAttribute.SetEnabled<FSEquipment.engineNo>(cache, fsEquipmentRow, enableVehicleControls);
        }

        /// <summary>
        /// Enable or Disable Cache Update/Delete.
        /// </summary>
        public virtual void EnableDisableCache(PXCache cache, FSEquipment fsEquipmentRow)
        {
            cache.AllowDelete = true;
            cache.AllowUpdate = true;

            if (fsEquipmentRow.SourceType == ID.SourceType_Equipment.VEHICLE)
            {
                cache.AllowDelete = false;
                cache.AllowUpdate = false;
            }
        }

        /// <summary>
        /// Enable/Disable Document fields.
        /// </summary>
        public virtual void EnableDisableEquipment(PXCache cache, FSEquipment fsEquipmentRow)
        {
            PXUIFieldAttribute.SetEnabled<FSEquipment.ownerID>(cache, fsEquipmentRow, fsEquipmentRow.OwnerType == ID.OwnerType_Equipment.CUSTOMER);
            PXUIFieldAttribute.SetEnabled<FSEquipment.resourceEquipment>(cache, fsEquipmentRow, fsEquipmentRow.OwnerType == ID.OwnerType_Equipment.OWN_COMPANY);
            PXUIFieldAttribute.SetEnabled<FSEquipment.dateInstalled>(cache, fsEquipmentRow, !AreThereAnyReplacements());
            PXUIFieldAttribute.SetEnabled<FSEquipment.inventoryID>(cache, fsEquipmentRow, !AreThereAnyReplacements());

            PXUIFieldAttribute.SetEnabled<FSEquipment.customerID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.CUSTOMER);
            PXUIFieldAttribute.SetEnabled<FSEquipment.customerLocationID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.CUSTOMER);
            PXUIFieldAttribute.SetEnabled<FSEquipment.branchID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.COMPANY);
            PXUIFieldAttribute.SetEnabled<FSEquipment.branchLocationID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.COMPANY);

            PXUIFieldAttribute.SetVisible<FSEquipment.customerID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.CUSTOMER);
            PXUIFieldAttribute.SetVisible<FSEquipment.customerLocationID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.CUSTOMER);
            PXUIFieldAttribute.SetVisible<FSEquipment.branchID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.COMPANY);
            PXUIFieldAttribute.SetVisible<FSEquipment.branchLocationID>(cache, fsEquipmentRow, fsEquipmentRow.LocationType == ID.LocationType.COMPANY);

            PXUIFieldAttribute.SetEnabled<FSEquipment.manufacturerID>(cache, fsEquipmentRow, fsEquipmentRow.InventoryID == null);
            PXUIFieldAttribute.SetEnabled<FSEquipment.manufacturerModelID>(cache, fsEquipmentRow, fsEquipmentRow.InventoryID == null);
        }

        public virtual bool AreThereAnyReplacements()
        {
            bool replacementFound = false;

            foreach (FSEquipmentComponent fsEquipmentComponentRow in EquipmentWarranties.Select())
            {
                if (fsEquipmentComponentRow.LastReplacementDate != null)
                {
                    replacementFound = true;
                    break;
                }
            }

            return replacementFound;
        }

        public virtual void SetEquipmentPersistingChecksAndWarnings(PXCache cache, FSEquipment fsEquipmentRow, FSSetup fsSetupRow)
        {
            PXPersistingCheck ownerTypePersistingCheck = (fsEquipmentRow.OwnerType == ID.OwnerType_Equipment.CUSTOMER) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
            PXPersistingCheck locationTypeCustomerPersistingCheck = (fsEquipmentRow.LocationType == ID.LocationType.CUSTOMER) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
            PXPersistingCheck locationTypeCompanyPersistingCheck = (fsEquipmentRow.LocationType == ID.LocationType.COMPANY) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;

            PXUIFieldAttribute.SetRequired<FSEquipment.ownerID>(cache, fsEquipmentRow.OwnerType == ID.OwnerType_Equipment.CUSTOMER);
            PXUIFieldAttribute.SetRequired<FSEquipment.customerID>(cache, fsEquipmentRow.LocationType == ID.LocationType.CUSTOMER);
            PXUIFieldAttribute.SetRequired<FSEquipment.branchID>(cache, fsEquipmentRow.LocationType == ID.LocationType.COMPANY);

            PXDefaultAttribute.SetPersistingCheck<FSEquipment.ownerID>(cache, fsEquipmentRow, ownerTypePersistingCheck);
            PXDefaultAttribute.SetPersistingCheck<FSEquipment.customerID>(cache, fsEquipmentRow, locationTypeCustomerPersistingCheck);
            PXDefaultAttribute.SetPersistingCheck<FSEquipment.branchID>(cache, fsEquipmentRow, locationTypeCompanyPersistingCheck);

            PXSetPropertyException exception = null;

            #region cpnyWarrantyEndDate & vendorWarrantyEndDate
            exception = null;

            if (fsSetupRow != null)
            {
                if (fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.INSTALLATION_DATE
                    && fsEquipmentRow.DateInstalled == null)
                {
                    exception = new PXSetPropertyException(TX.Warning.WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_INSTALLATIONDATE, PXErrorLevel.Error);
                }

                if (fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.SALES_ORDER_DATE
                    && fsEquipmentRow.SalesDate == null)
                {
                    exception = new PXSetPropertyException(TX.Warning.WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_SALEDATE, PXErrorLevel.Error);
                }

                if ((fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.EARLIEST_BOTH_DATE
                        || fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.LATEST_BOTH_DATE)
                    && fsEquipmentRow.DateInstalled == null
                    && fsEquipmentRow.SalesDate == null)
                {
                    exception = new PXSetPropertyException(TX.Warning.WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_INSTALLATIONDATE_AND_SALEDATE, PXErrorLevel.Error);
                }
            }

            if (fsEquipmentRow.CpnyWarrantyValue != null && fsEquipmentRow.CpnyWarrantyValue != 0)
            {
                cache.RaiseExceptionHandling<FSEquipment.cpnyWarrantyEndDate>(fsEquipmentRow, null, exception);
            }
            if (fsEquipmentRow.VendorWarrantyValue != null && fsEquipmentRow.VendorWarrantyValue != 0)
            {
                cache.RaiseExceptionHandling<FSEquipment.vendorWarrantyEndDate>(fsEquipmentRow, null, exception);
            }
            #endregion
        }

        /// <summary>
        /// Checks that at least one checkbox of the equipment Type is selected (Is Vehicle, Is Target Equipment, Is Resource Equipment).
        /// </summary>
        public virtual void SetRequiredEquipmentTypeError(PXCache cache, FSEquipment fsEquipmentRow)
        {
            if (fsEquipmentRow == null)
            {
                return;
            }

            PXSetPropertyException exception = null;

            if (fsEquipmentRow.IsVehicle == false
                    && fsEquipmentRow.RequireMaintenance == false
                        && fsEquipmentRow.ResourceEquipment == false)
            {
                exception = new PXSetPropertyException(TX.Error.SELECT_AT_LEAST_ONE_OPTION, PXErrorLevel.Error);
            }

            cache.RaiseExceptionHandling<FSEquipment.requireMaintenance>(fsEquipmentRow, fsEquipmentRow.RequireMaintenance, exception);
            cache.RaiseExceptionHandling<FSEquipment.resourceEquipment>(fsEquipmentRow, fsEquipmentRow.ResourceEquipment, exception);
        }

        public virtual void SetValuesFromInventoryItem(PXCache cache, FSEquipment fsEquipmentRow)
        {
            InventoryItem inventoryItemRow = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, fsEquipmentRow.InventoryID);

            if (inventoryItemRow != null)
            {
                FSxEquipmentModel fsxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);

                if (fsxEquipmentModelRow != null)
                {
                    if (fsxEquipmentModelRow.EQEnabled == true)
                    {
                        cache.SetValueExt<FSEquipment.manufacturerID>(fsEquipmentRow, fsxEquipmentModelRow.ManufacturerID);
                        cache.SetValueExt<FSEquipment.manufacturerModelID>(fsEquipmentRow, fsxEquipmentModelRow.ManufacturerModelID);

                        if (fsxEquipmentModelRow.EquipmentTypeID != null)
                        {
                            cache.SetValueExt<FSEquipment.equipmentTypeID>(fsEquipmentRow, fsxEquipmentModelRow.EquipmentTypeID);
                        }

                        cache.SetValueExt<FSEquipment.cpnyWarrantyType>(fsEquipmentRow, fsxEquipmentModelRow.CpnyWarrantyType);
                        cache.SetValueExt<FSEquipment.cpnyWarrantyValue>(fsEquipmentRow, fsxEquipmentModelRow.CpnyWarrantyValue);
                        cache.SetValueExt<FSEquipment.vendorWarrantyType>(fsEquipmentRow, fsxEquipmentModelRow.VendorWarrantyType);
                        cache.SetValueExt<FSEquipment.vendorWarrantyValue>(fsEquipmentRow, fsxEquipmentModelRow.VendorWarrantyValue);

                        //Attributes
                        this.Answers.CopyAllAttributes(this.EquipmentRecords.Current, inventoryItemRow);

                        //Image
                        PXNoteAttribute.CopyNoteAndFiles(this.Caches[typeof(InventoryItem)], inventoryItemRow, this.Caches[typeof(FSEquipment)], this.EquipmentRecords.Current, false, true);
                        fsEquipmentRow.ImageUrl = inventoryItemRow.ImageUrl;


                        /*fsEquipmentRow.CpnyWarrantyType = fsxEquipmentModelRow.CpnyWarrantyType;
                        fsEquipmentRow.CpnyWarrantyValue = fsxEquipmentModelRow.CpnyWarrantyValue;
                        fsEquipmentRow.VendorWarrantyType = fsxEquipmentModelRow.VendorWarrantyType;
                        fsEquipmentRow.VendorWarrantyValue = fsxEquipmentModelRow.VendorWarrantyValue;*/

                        if (fsxEquipmentModelRow.ModelType == ID.ModelType.EQUIPMENT)
                        {
                            ClearComponents();
                            InsertEquipmentModelComponents(fsEquipmentRow, inventoryItemRow);
                        }
                    }
                }
            }
        }

        public virtual void ClearComponents()
        {
            foreach (FSEquipmentComponent fsEquipmentComponentRow in EquipmentWarranties.Select())
            {
                EquipmentWarranties.Delete(fsEquipmentComponentRow);
            }
        }

        public virtual void InsertEquipmentModelComponents(FSEquipment fsEquipmentRow, InventoryItem inventoryItemRow)
        {
            PXResultset<FSModelComponent> bqlResultSet = PXSelect<FSModelComponent,
                                                         Where<
                                                             FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>,
                                                             And<FSModelComponent.active, Equal<True>,
                                                             And<FSModelComponent.optional, Equal<False>>>>>
                                                         .Select(this, inventoryItemRow.InventoryID);

            foreach (FSModelComponent fsModelComponentRow in bqlResultSet)
            {
                for (int i = 0; i < fsModelComponentRow.Qty; i++)
                {
                    FSEquipmentComponent fsEquipmentComponentRow = new FSEquipmentComponent()
                    {
                        ComponentID = fsModelComponentRow.ComponentID,
                        ItemClassID = fsModelComponentRow.ClassID,
                        InventoryID = fsModelComponentRow.InventoryID,
                        RequireSerial = fsModelComponentRow.RequireSerial,
                        LongDescr = fsModelComponentRow.Descr
                    };
                    fsEquipmentComponentRow = EquipmentWarranties.Insert(fsEquipmentComponentRow);

                    EquipmentWarranties.Cache.SetValueExt<FSEquipmentComponent.vendorID>(fsEquipmentComponentRow, fsModelComponentRow.VendorID);
                    EquipmentWarranties.Cache.SetValueExt<FSEquipmentComponent.vendorWarrantyDuration>(fsEquipmentComponentRow, fsModelComponentRow.VendorWarrantyValue);
                    EquipmentWarranties.Cache.SetValueExt<FSEquipmentComponent.vendorWarrantyType>(fsEquipmentComponentRow, fsModelComponentRow.VendorWarrantyType);
                    EquipmentWarranties.Cache.SetValueExt<FSEquipmentComponent.cpnyWarrantyDuration>(fsEquipmentComponentRow, fsModelComponentRow.CpnyWarrantyValue);
                    EquipmentWarranties.Cache.SetValueExt<FSEquipmentComponent.cpnyWarrantyType>(fsEquipmentComponentRow, fsModelComponentRow.CpnyWarrantyType);
                }
            }
        }

        public virtual void SetComponentPersistingChecksAndWarnings(PXCache cache, FSEquipmentComponent fsEquipmentComponentRow, FSSetup fsSetupRow)
        {
            PXSetPropertyException exception = null;

            #region SerialNumber
            exception = null;

            if (fsEquipmentComponentRow.RequireSerial == true
                    && fsEquipmentComponentRow.SerialNumber == null)
            {
                exception = new PXSetPropertyException(TX.Warning.REQUIRES_SERIAL_NUMBER, PXErrorLevel.Warning);
            }

            cache.RaiseExceptionHandling<FSEquipmentComponent.serialNumber>(fsEquipmentComponentRow, null, exception);
            #endregion

            #region cpnyWarrantyEndDate & vendorWarrantyEndDate
            exception = null;

            if (fsSetupRow != null)
            {
                if (fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.INSTALLATION_DATE
                    && fsEquipmentComponentRow.InstallationDate == null)
                {
                    exception = new PXSetPropertyException(TX.Warning.WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_INSTALLATIONDATE, PXErrorLevel.Warning);
                }

                if (fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.SALES_ORDER_DATE
                    && fsEquipmentComponentRow.SalesDate == null)
                {
                    exception = new PXSetPropertyException(TX.Warning.WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_SALEDATE, PXErrorLevel.Warning);
                }

                if ((fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.EARLIEST_BOTH_DATE
                        || fsSetupRow.EquipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.LATEST_BOTH_DATE)
                    && fsEquipmentComponentRow.InstallationDate == null
                    && fsEquipmentComponentRow.SalesDate == null)
                {
                    exception = new PXSetPropertyException(TX.Warning.WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_INSTALLATIONDATE_AND_SALEDATE, PXErrorLevel.Warning);
                }
            }

            if (fsEquipmentComponentRow.CpnyWarrantyDuration != null && fsEquipmentComponentRow.CpnyWarrantyDuration != 0)
            {
                cache.RaiseExceptionHandling<FSEquipmentComponent.cpnyWarrantyEndDate>(fsEquipmentComponentRow, null, exception);
            }
            if (fsEquipmentComponentRow.VendorWarrantyDuration != null && fsEquipmentComponentRow.VendorWarrantyDuration != 0)
            {
                cache.RaiseExceptionHandling<FSEquipmentComponent.vendorWarrantyEndDate>(fsEquipmentComponentRow, null, exception);
            }
            #endregion
        }

        public virtual void VerifySource(PXCache cache, FSEquipment fsEquipmentRow)
        {
            if (fsEquipmentRow.SourceID != null &&
                    fsEquipmentRow.SourceType == ID.SourceType_Equipment_ALL.EP_EQUIPMENT)
            {
                EPEquipment epEquipmentRow = PXSelect<EPEquipment,
                                             Where<
                                                EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>
                                             .Select(this, fsEquipmentRow.SourceID);

                if (epEquipmentRow == null)
                {
                    cache.RaiseExceptionHandling<FSEquipment.refNbr>(
                        fsEquipmentRow,
                        fsEquipmentRow.RefNbr,
                        new PXSetPropertyException(TX.Error.EQUIPMENT_SOURCE_REFERENCE_DELETED, PXErrorLevel.Warning));
                }
            }
        }

        public virtual void EnableDisable_ActionButtons(PXCache cache, FSEquipment fsEquipmentRow)
        {
            bool masterEnable = true;
            bool calendarsEnable = true;
            bool targetInqEnable = true;
            bool resourceInqEnable = true;

            if (fsEquipmentRow == null
                    || cache.GetStatus(fsEquipmentRow) == PXEntryStatus.Inserted)
            {
                masterEnable = false;
            }
            else
            {
                if (fsEquipmentRow.CustomerID == null)
                {
                    calendarsEnable = false;
                }

                targetInqEnable = fsEquipmentRow.RequireMaintenance.Value;
                resourceInqEnable = fsEquipmentRow.ResourceEquipment.Value;
            }

            openUserCalendar.SetEnabled(masterEnable && calendarsEnable);
            openEmployeeBoard.SetEnabled(masterEnable && calendarsEnable);
            targetEquipmentInquiry.SetEnabled(masterEnable && targetInqEnable);
            resourceEquipmentInquiry.SetEnabled(masterEnable && resourceInqEnable);
            replaceComponent.SetEnabled(masterEnable);
        }

        public virtual void OpenInquiry()
        {
            if (EquipmentRecords.Current != null && EquipmentRecords.Current.RefNbr != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    ["TargetEquipment"] = EquipmentRecords.Current.RefNbr
                };

                string design = TX.GenericInquiries_GUID.APPOINTMENT_DETAILS_HISTORY;
                string url = PXRedirectToGIWithParametersRequiredException.BuildUrl(new Guid(design), parameters);
                PXGenericInqGrph grph = PXGenericInqGrph.CreateInstance(design, null, parameters);

                throw new PXRedirectRequiredException(url, grph, null)
                {
                    ScreenId = ID.ScreenID.APPOINTMENT_DETAILS
                };
            }
        }

        public virtual void CalculateEndWarrantyDate(FSSetup fsSetupRow, FSEquipment fsEquipmentRow, FSEquipmentComponent fsEquipmentComponentRow, string warrantySource)
        {
            if (fsSetupRow == null)
            {
                return;
            }

            DateTime? salesDate = GetComponentSalesDate(fsEquipmentRow, fsEquipmentComponentRow);
            DateTime? dateInstalled = GetComponentInstallationDate(fsEquipmentRow, fsEquipmentComponentRow);

            switch (fsSetupRow.EquipmentCalculateWarrantyFrom)
            {
                case ID.EquipmentWarrantyFrom.SALES_ORDER_DATE:

                    this.SetWarrantyBySource(fsEquipmentRow, fsEquipmentComponentRow, warrantySource, salesDate);

                    break;
                case ID.EquipmentWarrantyFrom.INSTALLATION_DATE:

                    this.SetWarrantyBySource(fsEquipmentRow, fsEquipmentComponentRow, warrantySource, dateInstalled);

                    break;

                case ID.EquipmentWarrantyFrom.EARLIEST_BOTH_DATE:
                case ID.EquipmentWarrantyFrom.LATEST_BOTH_DATE:

                    DateTime? startDate = this.GetWarrantyStartDate(salesDate, dateInstalled, fsSetupRow.EquipmentCalculateWarrantyFrom);

                    this.SetWarrantyBySource(fsEquipmentRow, fsEquipmentComponentRow, warrantySource, startDate);

                    break;
            }
        }

        public virtual DateTime? GetComponentSalesDate(FSEquipment fsEquipmentRow, FSEquipmentComponent fsEquipmentComponentRow)
        {
            if (fsEquipmentRow == null && fsEquipmentComponentRow == null)
            {
                return null;
            }

            if (fsEquipmentComponentRow == null)
            {
                return fsEquipmentRow.SalesDate;
            }
            else
            {
                return fsEquipmentComponentRow.SalesDate;
            }
        }

        public virtual DateTime? GetComponentInstallationDate(FSEquipment fsEquipmentRow, FSEquipmentComponent fsEquipmentComponentRow)
        {
            if (fsEquipmentRow == null && fsEquipmentComponentRow == null)
            {
                return null;
            }

            if (fsEquipmentComponentRow == null)
            {
                return fsEquipmentRow.DateInstalled;
            }
            else
            {
                return fsEquipmentComponentRow.InstallationDate;
            }
        }

        public virtual DateTime? GetWarrantyStartDate(
            DateTime? salesDate,
            DateTime? dateInstalled,
            string equipmentCalculateWarrantyFrom)
        {
            if (salesDate == null && dateInstalled == null)
            {
                return null;
            }
            else if (salesDate != null && dateInstalled == null)
            {
                return salesDate;
            }
            else if (salesDate == null && dateInstalled != null)
            {
                return dateInstalled;
            }
            else
            {
                int result = DateTime.Compare(salesDate.Value, dateInstalled.Value);

                if (result < 0 && equipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.EARLIEST_BOTH_DATE)
                {
                    return salesDate;
                }
                else if (result < 0 && equipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.LATEST_BOTH_DATE)
                {
                    return dateInstalled;
                }
                else if (result > 0 && equipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.EARLIEST_BOTH_DATE)
                {
                    return dateInstalled;
                }
                else if (result > 0 && equipmentCalculateWarrantyFrom == ID.EquipmentWarrantyFrom.LATEST_BOTH_DATE)
                {
                    return salesDate;
                }
                else if (result == 0)
                {
                    return salesDate;
                }
            }

            return null;
        }

        public virtual DateTime? GetWarrantyEndDate(DateTime? originalDate, DateTime? startDate, int? duration, string warrantyDurationType)
        {
            if (duration == null || duration == 0)
            {
                return null;
            }

            if (startDate == null)
            {
                return null;
            }

            switch (warrantyDurationType)
            {
                case ID.WarrantyDurationType.MONTH:
                    return startDate.Value.AddMonths(duration.Value);
                case ID.WarrantyDurationType.YEAR:
                    return startDate.Value.AddYears(duration.Value);
                case ID.WarrantyDurationType.DAY:
                    return startDate.Value.AddDays(duration.Value);
                default:
                    return originalDate;
            }
        }

        public virtual void SetWarrantyBySource(FSEquipment fsEquipmentRow, FSEquipmentComponent fsEquipmentComponentRow, string warrantySource, DateTime? startDate)
        {
            switch (warrantySource)
            {
                case ID.WarratySource.COMPANY:

                    if (fsEquipmentRow != null && fsEquipmentComponentRow == null)
                    {
                        fsEquipmentRow.CpnyWarrantyEndDate = this.GetWarrantyEndDate(fsEquipmentRow.CpnyWarrantyEndDate,
                                                                                     startDate,
                                                                                     fsEquipmentRow.CpnyWarrantyValue,
                                                                                     fsEquipmentRow.CpnyWarrantyType);
                    }

                    if (fsEquipmentComponentRow != null)
                    {
                        fsEquipmentComponentRow.CpnyWarrantyEndDate = this.GetWarrantyEndDate(fsEquipmentComponentRow.CpnyWarrantyEndDate,
                                                                                              startDate,
                                                                                              fsEquipmentComponentRow.CpnyWarrantyDuration,
                                                                                              fsEquipmentComponentRow.CpnyWarrantyType);
                    }

                    break;
                case ID.WarratySource.VENDOR:
                    if (fsEquipmentRow != null && fsEquipmentComponentRow == null)
                    {
                        fsEquipmentRow.VendorWarrantyEndDate = this.GetWarrantyEndDate(fsEquipmentRow.VendorWarrantyEndDate,
                                                                                       startDate,
                                                                                       fsEquipmentRow.VendorWarrantyValue,
                                                                                       fsEquipmentRow.VendorWarrantyType);
                    }

                    if (fsEquipmentComponentRow != null)
                    {
                        fsEquipmentComponentRow.VendorWarrantyEndDate = this.GetWarrantyEndDate(fsEquipmentComponentRow.VendorWarrantyEndDate,
                                                                                                startDate,
                                                                                                fsEquipmentComponentRow.VendorWarrantyDuration,
                                                                                                fsEquipmentComponentRow.VendorWarrantyType);
                    }

                    break;
            }
        }

        public virtual bool ItemBelongsToClass(int? inventoryID, int? itemClassID, out int? newItemClassID)
        {
            InventoryItem inventoryItemRow = PXSelect<InventoryItem,
                                             Where<
                                                 InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                             .Select(this, inventoryID);

            if (inventoryItemRow != null)
            {
                newItemClassID = inventoryItemRow.ItemClassID;
                return itemClassID == inventoryItemRow.ItemClassID;
            }

            newItemClassID = null;
            return false;
        }

        public virtual void SetComponentValuesFromInventory(PXCache cache, FSEquipmentComponent fsEquipmentComponentRow)
        {
            if (fsEquipmentComponentRow == null ||
                    fsEquipmentComponentRow.InventoryID == null)
            {
                return;
            }

            InventoryItem inventoryItemRow = PXSelect<InventoryItem,
                                             Where<
                                                 InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                             .Select(this, fsEquipmentComponentRow.InventoryID);

            if (inventoryItemRow != null)
            {
                FSxEquipmentModel fsxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);

                if (fsxEquipmentModelRow != null)
                {
                    if (fsxEquipmentModelRow.CpnyWarrantyType != null)
                    {
                        fsEquipmentComponentRow.CpnyWarrantyType = fsxEquipmentModelRow.CpnyWarrantyType;
                    }

                    if (fsxEquipmentModelRow.CpnyWarrantyValue != null)
                    {
                        cache.SetValueExt<FSEquipmentComponent.cpnyWarrantyDuration>(fsEquipmentComponentRow, fsxEquipmentModelRow.CpnyWarrantyValue);
                    }

                    if (fsxEquipmentModelRow.VendorWarrantyType != null)
                    {
                        fsEquipmentComponentRow.VendorWarrantyType = fsxEquipmentModelRow.VendorWarrantyType;
                    }

                    if (fsxEquipmentModelRow.VendorWarrantyValue != null)
                    {
                        cache.SetValueExt<FSEquipmentComponent.vendorWarrantyDuration>(fsEquipmentComponentRow, fsxEquipmentModelRow.VendorWarrantyValue);
                    }
                }

                POVendorInventory poVendorInventoryRow = PXSelect<POVendorInventory,
                                                         Where<
                                                             POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
                                                             And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>>>>
                                                         .SelectWindowed(this, 0, 1, inventoryItemRow.InventoryID, inventoryItemRow.PreferredVendorID);

                if (poVendorInventoryRow != null)
                {
                    fsEquipmentComponentRow.VendorID = poVendorInventoryRow.VendorID;
                }
            }
        }

        public virtual bool IsTheReplacementInfoValid()
        {
            bool isTheInfoValid = true;

            if (ReplaceComponentInfo.Current.SalesDate == null)
            {
                ReplaceComponentInfo.Cache.RaiseExceptionHandling<RComponent.salesDate>(
                                    ReplaceComponentInfo.Current,
                                    ReplaceComponentInfo.Current.SalesDate,
                                    new PXException(PXMessages.LocalizeFormatNoPrefix(
                                                        TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                                        PXUIFieldAttribute.GetDisplayName<RComponent.salesDate>(ReplaceComponentInfo.Cache))));
                isTheInfoValid = false;
            }

            if (ReplaceComponentInfo.Current.InstallationDate == null)
            {
                ReplaceComponentInfo.Cache.RaiseExceptionHandling<RComponent.installationDate>(
                                    ReplaceComponentInfo.Current,
                                    ReplaceComponentInfo.Current.InstallationDate,
                                    new PXException(PXMessages.LocalizeFormatNoPrefix(
                                                        TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                                        PXUIFieldAttribute.GetDisplayName<RComponent.installationDate>(ReplaceComponentInfo.Cache))));
                isTheInfoValid = false;
            }

            return isTheInfoValid;
        }

        public virtual FSEquipmentComponent ApplyComponentReplacement(FSEquipmentComponent replacedCompt, FSEquipmentComponent replacementCompt)
        {
            replacementCompt = EquipmentWarranties.Insert(replacementCompt);
            replacedCompt.ComponentReplaced = replacementCompt.LineNbr;
            replacedCompt.Status = ID.Equipment_Status.DISPOSED;
            
            EquipmentWarranties.Update(replacedCompt);

            this.Save.Press();
            return replacementCompt;
        }

        public virtual void VerifyQtyEquipmentComponents(PXCache cache, int? modelComponentID)
        {
            if (modelComponentID == null)
            {
                return;
            }

            bool isThereError = false;

            var modelComponents = PXSelect<FSModelComponent,
                                  Where<
                                        FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>>>
                                  .Select(cache.Graph, modelComponentID);

            foreach (FSModelComponent fsModelComponentRow in modelComponents)
            {
                if (fsModelComponentRow.Qty != null)
                {
                    var EquipmentWarrantiesRows = EquipmentWarranties.Select().AsEnumerable()
                                                                     .Where(x => ((FSEquipmentComponent)x).ComponentID == fsModelComponentRow.ComponentID
                                                                                    && ((FSEquipmentComponent)x).Status == ID.Equipment_Status.ACTIVE);
                    {
                        if (EquipmentWarrantiesRows.Count() > fsModelComponentRow.Qty)
                        {
                            EquipmentWarranties.Cache.RaiseExceptionHandling<FSEquipmentComponent.lineRef>(
                                (FSEquipmentComponent)EquipmentWarrantiesRows.First(),
                                null,
                                new PXSetPropertyException(TX.Error.EQUIPMENT_COMPONENT_ROW_QTY_EXCEEDED, PXErrorLevel.Error));

                            isThereError = true;
                        }
                    }
                }
            }

            if (isThereError == true)
            {
                throw new PXException(TX.Error.EQUIPMENT_COMPONENTS_QTY_ERROR);
            }
        }
        #endregion

        #region Event Handlers

        #region FSEquipment

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.inventoryID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            SetValuesFromInventoryItem(e.Cache, fsEquipmentRow);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.locationType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            if (fsEquipmentRow.LocationType == ID.LocationType.COMPANY)
            {
                fsEquipmentRow.CustomerID = null;
                fsEquipmentRow.CustomerLocationID = null;
            }
            else
            {
                fsEquipmentRow.BranchID = null;
                fsEquipmentRow.BranchLocationID = null;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.manufacturerID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            fsEquipmentRow.ManufacturerModelID = null;
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.manufacturerModelID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            var fsManufacturerModelRow = (FSManufacturerModel)PXSelectorAttribute.Select<FSEquipment.manufacturerModelID>(e.Cache, fsEquipmentRow, fsEquipmentRow.ManufacturerModelID);
            if (fsManufacturerModelRow != null)
            {
                fsEquipmentRow.EquipmentTypeID = fsManufacturerModelRow.EquipmentTypeID;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.dateInstalled> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.COMPANY);
            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.VENDOR);

            foreach (FSEquipmentComponent fsEquipmentComponentRow in EquipmentWarranties.Select())
            {
                EquipmentWarranties.Cache.SetValueExt<FSEquipmentComponent.installationDate>(fsEquipmentComponentRow, fsEquipmentRow.DateInstalled);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.salesDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.COMPANY);
            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.VENDOR);

            foreach (FSEquipmentComponent fsEquipmentComponentRow in EquipmentWarranties.Select())
            {
                EquipmentWarranties.Cache.SetValueExt<FSEquipmentComponent.salesDate>(fsEquipmentComponentRow, fsEquipmentRow.SalesDate);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.cpnyWarrantyValue> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.COMPANY);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.cpnyWarrantyType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.COMPANY);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.vendorWarrantyValue> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.VENDOR);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.vendorWarrantyType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, fsEquipmentRow, null, ID.WarratySource.VENDOR);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipment, FSEquipment.equipmentTypeID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;
            e.Cache.SetDefaultExt<FSEquipment.equipmentTypeCD>(fsEquipmentRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSEquipment> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSEquipment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;
            PXCache cache = e.Cache;

            VerifySource(cache, fsEquipmentRow);
            EnableDisableCache(cache, fsEquipmentRow);
            EnableDisableEquipment(cache, fsEquipmentRow);
            SetEquipmentPersistingChecksAndWarnings(cache, fsEquipmentRow, SetupRecord.Current);
            EnableDisableVehicleControls(cache, fsEquipmentRow);
            EnableDisable_ActionButtons(cache, fsEquipmentRow);
            SetRequiredEquipmentTypeError(cache, fsEquipmentRow);

            if (fsEquipmentRow.LocationType == ID.LocationType.CUSTOMER)
            {
                fsEquipmentRow.BranchID = null;
            }
        }

        protected virtual void _(Events.RowInserting<FSEquipment> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSEquipment> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSEquipment> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSEquipment> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSEquipment> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSEquipment> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSEquipment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipment fsEquipmentRow = (FSEquipment)e.Row;
            PXCache cache = e.Cache;

            if (e.Operation != PXDBOperation.Delete && fsEquipmentRow.IsVehicle == true)
            {
                LicenseHelper.CheckVehiclesLicense(cache.Graph, fsEquipmentRow.SMEquipmentID, fsEquipmentRow.Status);
            }

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                VerifyQtyEquipmentComponents(cache, fsEquipmentRow.InventoryID);
            }
        }

        protected virtual void _(Events.RowPersisted<FSEquipment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                FSEquipment fsEquipmentRow = (FSEquipment)e.Row;
                PXCache cache = e.Cache;

                if ((fsEquipmentRow.SourceType == ID.SourceType_Equipment.EP_EQUIPMENT || fsEquipmentRow.SourceType == ID.SourceType_Equipment.VEHICLE)
                        && fsEquipmentRow.SourceID != null)
                {
                    EPEquipment epEquipmentRow = GetRelatedEPEquipmentRow(cache.Graph, fsEquipmentRow.SourceID);
                    PXCache<EPEquipment> cacheEPEquipment = new PXCache<EPEquipment>(cache.Graph);

                    if (EquipmentHelper.UpdateEPEquipmentWithFSEquipment(cacheEPEquipment, epEquipmentRow, cache, fsEquipmentRow))
                    {
                        cacheEPEquipment.Update(epEquipmentRow);
                        cacheEPEquipment.Persist(PXDBOperation.Update);
                    }
                }
            }
        }

        #endregion

        #region FSEquipmentComponent

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying

        protected virtual void _(Events.FieldVerifying<FSEquipmentComponent, FSEquipmentComponent.cpnyWarrantyDuration> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if ((int?)e.NewValue < 0)
            {
                throw new PXSetPropertyException(TX.Error.INVALID_WARRANTY_DURATION);
            }
        }

        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.installationDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, null, fsEquipmentComponentRow, ID.WarratySource.COMPANY);
            this.CalculateEndWarrantyDate(SetupRecord.Current, null, fsEquipmentComponentRow, ID.WarratySource.VENDOR);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.salesDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, null, fsEquipmentComponentRow, ID.WarratySource.COMPANY);
            this.CalculateEndWarrantyDate(SetupRecord.Current, null, fsEquipmentComponentRow, ID.WarratySource.VENDOR);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.componentID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            if (fsEquipmentComponentRow.ComponentID != null)
            {
                FSModelComponent fsModelComponentRow = PXSelect<FSModelComponent,
                                                       Where<
                                                           FSModelComponent.componentID, Equal<Required<FSModelComponent.componentID>>>>
                                                       .Select(this, fsEquipmentComponentRow.ComponentID);

                fsEquipmentComponentRow.LongDescr = fsModelComponentRow.Descr;
                fsEquipmentComponentRow.ItemClassID = fsModelComponentRow.ClassID;
                fsEquipmentComponentRow.CpnyWarrantyDuration = fsModelComponentRow.CpnyWarrantyValue;
                fsEquipmentComponentRow.CpnyWarrantyType = fsModelComponentRow.CpnyWarrantyType;
                fsEquipmentComponentRow.VendorWarrantyDuration = fsModelComponentRow.VendorWarrantyValue;
                fsEquipmentComponentRow.VendorWarrantyType = fsModelComponentRow.VendorWarrantyType;
                fsEquipmentComponentRow.VendorID = fsModelComponentRow.VendorID;
                fsEquipmentComponentRow.RequireSerial = fsModelComponentRow.RequireSerial;

                this.CalculateEndWarrantyDate(SetupRecord.Current, EquipmentRecords.Current, fsEquipmentComponentRow, ID.WarratySource.COMPANY);
                this.CalculateEndWarrantyDate(SetupRecord.Current, EquipmentRecords.Current, fsEquipmentComponentRow, ID.WarratySource.VENDOR);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.cpnyWarrantyDuration> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            if (fsEquipmentComponentRow.CpnyWarrantyDuration == null)
            {
                fsEquipmentComponentRow.CpnyWarrantyEndDate = null;
                return;
            }

            this.CalculateEndWarrantyDate(SetupRecord.Current, EquipmentRecords.Current, fsEquipmentComponentRow, ID.WarratySource.COMPANY);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.cpnyWarrantyType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, EquipmentRecords.Current, fsEquipmentComponentRow, ID.WarratySource.COMPANY);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.vendorWarrantyDuration> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            if (fsEquipmentComponentRow.VendorWarrantyDuration == null)
            {
                fsEquipmentComponentRow.VendorWarrantyEndDate = null;
                return;
            }

            this.CalculateEndWarrantyDate(SetupRecord.Current, EquipmentRecords.Current, fsEquipmentComponentRow, ID.WarratySource.VENDOR);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.vendorWarrantyType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            this.CalculateEndWarrantyDate(SetupRecord.Current, EquipmentRecords.Current, fsEquipmentComponentRow, ID.WarratySource.VENDOR);
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.itemClassID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;
            int? newItemClassID;

            if (fsEquipmentComponentRow.InventoryID != null
                    && !ItemBelongsToClass(fsEquipmentComponentRow.InventoryID, fsEquipmentComponentRow.ItemClassID, out newItemClassID))
            {
                fsEquipmentComponentRow.InventoryID = null;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSEquipmentComponent, FSEquipmentComponent.inventoryID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            int? newItemClassID;

            if (fsEquipmentComponentRow.InventoryID != null
                    && !ItemBelongsToClass(fsEquipmentComponentRow.InventoryID, fsEquipmentComponentRow.ItemClassID, out newItemClassID))
            {
                fsEquipmentComponentRow.ItemClassID = newItemClassID;
            }

            this.SetComponentValuesFromInventory(e.Cache, fsEquipmentComponentRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSEquipmentComponent> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSEquipmentComponent> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;
            PXUIFieldAttribute.SetEnabled<FSEquipmentComponent.itemClassID>(e.Cache, fsEquipmentComponentRow, fsEquipmentComponentRow.ItemClassID == null);
            SetComponentPersistingChecksAndWarnings(e.Cache, fsEquipmentComponentRow, SetupRecord.Current);
        }

        protected virtual void _(Events.RowInserting<FSEquipmentComponent> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEquipmentComponent fsEquipmentComponentRow = (FSEquipmentComponent)e.Row;

            if (fsEquipmentComponentRow.LineRef == null)
            {
                fsEquipmentComponentRow.LineRef = fsEquipmentComponentRow.LineNbr.Value.ToString("00000");
            }
        }

        protected virtual void _(Events.RowInserted<FSEquipmentComponent> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSEquipmentComponent> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSEquipmentComponent> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSEquipmentComponent> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSEquipmentComponent> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSEquipmentComponent> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSEquipmentComponent> e)
        {
        }

        #endregion

        #endregion
    }
}         