using PX.Data;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.SO.SOOrderEntry;
using System.Text;

namespace PX.Objects.FS
{
    public class SM_SOOrderEntry : PXGraphExtension<SOOrderEntry>, IInvoiceGraph, IInvoiceContractGraph
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        [PXCopyPasteHiddenView]
        public PXFilter<FSCreateServiceOrderFilter> CreateServiceOrderFilter;

        #region SrvOrdType
        [PXDefault(typeof(Search<FSSetup.dfltSOSrvOrdType>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSCreateServiceOrderFilter_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        
        public override void Initialize()
        {
            base.Initialize();
            Base.action.AddMenuAction(CreateServiceOrder);
            Base.action.AddMenuAction(OpenAppointmentBoard);
            Base.inquiry.AddMenuAction(ViewServiceOrder);
        }

        public virtual bool IsFSIntegrationEnabled()
        {
            bool isFSIntegrated = true;

            if (Base.soordertype.Current == null)
            {
                isFSIntegrated = false;
            }
            else
            {
                FSxSOOrderType fsxSOOrderTypeRow = Base.soordertype.Cache.GetExtension<FSxSOOrderType>(Base.soordertype.Current);

                if (fsxSOOrderTypeRow == null || fsxSOOrderTypeRow.EnableFSIntegration != true)
                {
                    isFSIntegrated = false;
                }
            }

            if (Base.Document.Current != null)
            {
                FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

                if (fsxSOOrderRow != null)
                {
                    fsxSOOrderRow.IsFSIntegrated = isFSIntegrated;
                }
            }

            return isFSIntegrated;
        }

        #region Selects
        [PXHidden]
        public PXSelect<FSSetup> SetupRecord;
       
        #endregion

        #region Actions

        public PXAction<SOOrder> OpenAppointmentBoard;
        [PXButton]
        [PXUIField(DisplayName = "Schedule on the Calendar Board", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        public virtual void openAppointmentBoard()
        {
            if (Base.Document == null || Base.Document.Current == null)
            {
                return;
            }

            //Try to save all unsaved changes
		    if (Base.IsDirty)
		    {
                Base.Save.Press();
		    }

            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

            if (fsxSOOrderRow != null && fsxSOOrderRow.SORefNbr != null)
            {
                ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();
                FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(Base.Document.Current);

                if (fsServiceOrderRow != null)
                {
                    graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);
                    graphServiceOrder.OpenEmployeeBoard();
                }
            }
        }

        #region CopyOrder

        [PXOverride]
        public IEnumerable CopyOrder(PXAdapter adapter)
        {
            if (Base.Document == null || Base.Document.Current == null)
            {
                return adapter.Get();
            }

            List<SOOrder> soOrderList = new List<SOOrder>();
            SOOrder soOrderRow = Base.Document.Current;

            FSxSOOrder fsxSOOrderRow = PXCache<SOOrder>.GetExtension<FSxSOOrder>(soOrderRow);
            if (fsxSOOrderRow != null
                    && fsxSOOrderRow.SDEnabled == true)
            {
                fsxSOOrderRow.SORefNbr = null;
            }

            soOrderList.Add(soOrderRow);

            return soOrderList;
        }
        #endregion

        #region CreateServiceOrder
        public PXAction<SOOrder> CreateServiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "Create Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void createServiceOrder()
        {
            SOOrder soOrderRow = Base.Document.Current;
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (CreateServiceOrderFilter.AskExt() == WebDialogResult.OK)
            {
                fsxSOOrderRow.SDEnabled = true;
                fsxSOOrderRow.AssignedEmpID = CreateServiceOrderFilter.Current.AssignedEmpID;
                fsxSOOrderRow.SrvOrdType = CreateServiceOrderFilter.Current.SrvOrdType;
                fsxSOOrderRow.SLAETA = CreateServiceOrderFilter.Current.SLAETA;

                PXLongOperation.StartOperation(Base, delegate ()
                {
                    InsertUpdateDeleteServiceOrderDocument(soOrderRow, fsxSOOrderRow, PXDBOperation.Insert);
                });
            }
        }
        #endregion

        #region ViewServiceOrder
        public PXAction<SOOrder> ViewServiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "View Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void viewServiceOrder()
        {
            if (Base.Document == null || Base.Document.Current == null)
            {
                return;
            }

            if (Base.IsDirty)
            {
                Base.Save.Press();
            }

            SOOrder soOrderRow = Base.Document.Current;
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (fsxSOOrderRow != null
                && fsxSOOrderRow.SDEnabled == true
                && fsxSOOrderRow.SrvOrdType != null
                && fsxSOOrderRow.SORefNbr != null)
            {
                FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
                CRExtensionHelper.LaunchServiceOrderScreen(Base, fsServiceOrderRow?.SOID);
            }
        }
        #endregion
        #endregion

        #region Private Methods
        private bool CanCreateServiceOrder(PXCache cache, SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow)
        {
            if (soOrderRow == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(soOrderRow.OrderType) == true)
            {
                return false;
            }

            FSPostInfo fsPostInfoRow = PXSelect<FSPostInfo,
                                        Where<
                                            FSPostInfo.sOOrderType, Equal<Required<FSPostInfo.sOOrderType>>,
                                        And<
                                            FSPostInfo.sOOrderNbr, Equal<Required<FSPostInfo.sOOrderNbr>>>>>
                                        .Select(Base, soOrderRow.OrderType, soOrderRow.OrderNbr);

            if (fsPostInfoRow != null)
            {
                cache.RaiseExceptionHandling<FSxSOOrder.sDEnabled>(
                    soOrderRow,
                    fsxSOOrderRow.SDEnabled,
                    new PXSetPropertyException(
                                            TX.Warning.CANT_CREATE_A_SERVICE_ORDER_FROM_AN_INVOICED_SALES_ORDER, 
                                            PXErrorLevel.Warning));
                return false;
            }

            return true;
        } 

        /// <summary>
        /// Returns the ServiceOrder record associated to the selected Sales Order.
        /// </summary>
        private FSServiceOrder GetServiceOrderRecord(SOOrder soOrderRow)
        {
            return (FSServiceOrder)PXSelect<FSServiceOrder,
                        Where<
                            FSServiceOrder.sourceType, Equal<ListField_SourceType_ServiceOrder.SalesOrder>,
                            And<FSServiceOrder.sourceDocType, Equal<Required<FSServiceOrder.sourceDocType>>,
                            And<FSServiceOrder.sourceRefNbr, Equal<Required<FSServiceOrder.sourceRefNbr>>>>>>
                        .Select(Base, soOrderRow.OrderType, soOrderRow.OrderNbr);
        }

        /// <summary>
        /// Initializes a new Service Order.
        /// </summary>
        private FSServiceOrder InitNewServiceOrder(ServiceOrderEntry graphServiceOrder, SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow)
        {
            //Initialize the FSServiceOrder with its KEY fieds
            var fsServiceOrderRow = new FSServiceOrder();

            fsServiceOrderRow.SrvOrdType = fsxSOOrderRow.SrvOrdType;

            //Assigns the Sales Order reference to the new Service Order
            fsServiceOrderRow.SourceType = ID.SourceType_ServiceOrder.SALES_ORDER;
            fsServiceOrderRow.SourceDocType = soOrderRow.OrderType;
            fsServiceOrderRow.SourceRefNbr = soOrderRow.OrderNbr;            

            return graphServiceOrder.ServiceOrderRecords.Insert(fsServiceOrderRow);
        }

        /// <summary>
        /// Deletes the Service Order and blanks the <c>fsxSOOrderRow.SOID</c> memory field.
        /// </summary>
        private void DeleteServiceOrder(ServiceOrderEntry graphServiceOrder, FSServiceOrder fsServiceOrderRow, FSxSOOrder fsxSOOrderRow)
        {
            graphServiceOrder.ServiceOrderRecords.Delete(fsServiceOrderRow);
            graphServiceOrder.Save.Press();
            fsxSOOrderRow.SORefNbr = null;
        }

        /// <summary>
        /// Updates the ServiceOrder information using the Sales Order definition.
        /// </summary>
        private void UpdateServiceOrderHeader(
                                            ServiceOrderEntry graphServiceOrder,
                                            FSServiceOrder fsServiceOrderRow,
                                            SOOrder soOrderRow,
                                            FSxSOOrder fsxSOOrderRow,
                                            PXDBOperation operation)
        {
            bool updateRequired = false;
            FSContact fsContactRow = graphServiceOrder.ServiceOrder_Contact.Select();
            FSAddress fsAddressRow = graphServiceOrder.ServiceOrder_Address.Select();

            SOShippingContact soShippingContactRow = Base.Shipping_Contact.Select();
            SOShippingAddress soShippingAddressRow = Base.Shipping_Address.Select();

            //**************************************************************
            // Update all FSServiceOrder fields but key fields *************
            //**************************************************************
            if (fsServiceOrderRow.CustomerID != soOrderRow.CustomerID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.customerID>(fsServiceOrderRow, soOrderRow.CustomerID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.CuryID != soOrderRow.CuryID
                    && operation == PXDBOperation.Insert)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.curyID>(fsServiceOrderRow, soOrderRow.CuryID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.ProjectID != soOrderRow.ProjectID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.projectID>(fsServiceOrderRow, soOrderRow.ProjectID);
                updateRequired = true;
            }
            
            if (fsServiceOrderRow.Hold != soOrderRow.Hold)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.hold>(fsServiceOrderRow, soOrderRow.Hold);
                updateRequired = true;
            }

            if (fsServiceOrderRow.OrderDate != soOrderRow.RequestDate)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.orderDate>(fsServiceOrderRow, soOrderRow.RequestDate);
                updateRequired = true;
            }

            if (soOrderRow.OrderDesc != null && soOrderRow.OrderDesc != fsServiceOrderRow.DocDesc)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.docDesc>(fsServiceOrderRow, soOrderRow.OrderDesc);
                updateRequired = true;
            }

            if (fsServiceOrderRow.LocationID != soOrderRow.CustomerLocationID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.locationID>(fsServiceOrderRow, soOrderRow.CustomerLocationID);
                updateRequired = true;
            }

            if (graphServiceOrder.ServiceOrderRecords.Current.BranchLocationID == null)
            {
                int? branchLocationID = GetBranchLocation(this.Base.Accessinfo.BranchID);
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.branchLocationID>(fsServiceOrderRow, branchLocationID);
            }

            if (soShippingAddressRow != null)
            {
                if (fsAddressRow.CountryID != soShippingAddressRow.CountryID)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.countryID>(fsAddressRow, soShippingAddressRow.CountryID);
                    updateRequired = true;
                }

                if (fsAddressRow.State != soShippingAddressRow.State)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.state>(fsAddressRow, soShippingAddressRow.State);
                    updateRequired = true;
                }

                if (fsAddressRow.City != soShippingAddressRow.City)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.city>(fsAddressRow, soShippingAddressRow.City);
                    updateRequired = true;
                }

                if (fsAddressRow.PostalCode != soShippingAddressRow.PostalCode)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.postalCode>(fsAddressRow, soShippingAddressRow.PostalCode);
                    updateRequired = true;
                }

                if (fsAddressRow.AddressLine1 != soShippingAddressRow.AddressLine1)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.addressLine1>(fsAddressRow, soShippingAddressRow.AddressLine1);
                    updateRequired = true;
                }

                if (fsAddressRow.AddressLine2 != soShippingAddressRow.AddressLine2)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.addressLine2>(fsAddressRow, soShippingAddressRow.AddressLine2);
                    updateRequired = true;
                }

                if (fsAddressRow.AddressLine3 != soShippingAddressRow.AddressLine3)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.addressLine3>(fsAddressRow, soShippingAddressRow.AddressLine3);
                    updateRequired = true;
                }

                if (fsAddressRow.IsValidated != soShippingAddressRow.IsValidated)
                {
                    graphServiceOrder.ServiceOrder_Address.SetValueExt<FSAddress.isValidated>
                                    (fsAddressRow, soShippingAddressRow.IsValidated);
                    updateRequired = true;
                }
            }

            if (soShippingContactRow != null)
            {
                if (fsContactRow.Attention != soShippingContactRow.Salutation)
                {
                    graphServiceOrder.ServiceOrder_Contact.SetValueExt<FSContact.attention>(fsContactRow, soShippingContactRow.Salutation);
                    updateRequired = true;
                }

                if (fsContactRow.Phone1 != soShippingContactRow.Phone1)
                {
                    graphServiceOrder.ServiceOrder_Contact.SetValueExt<FSContact.phone1>(fsContactRow, soShippingContactRow.Phone1);
                    updateRequired = true;
                }

                if (fsContactRow.Phone2 != soShippingContactRow.Phone2)
                {
                    graphServiceOrder.ServiceOrder_Contact.SetValueExt<FSContact.phone2>(fsContactRow, soShippingContactRow.Phone2);
                    updateRequired = true;
                }

                if (fsContactRow.Phone3 != soShippingContactRow.Phone3)
                {
                    graphServiceOrder.ServiceOrder_Contact.SetValueExt<FSContact.phone3>(fsContactRow, soShippingContactRow.Phone3);
                    updateRequired = true;
                }

                if (fsContactRow.Fax != soShippingContactRow.Fax)
                {
                    graphServiceOrder.ServiceOrder_Contact.SetValueExt<FSContact.fax>(fsContactRow, soShippingContactRow.Fax);
                    updateRequired = true;
                }

                if (fsContactRow.Email != soShippingContactRow.Email)
                {
                    graphServiceOrder.ServiceOrder_Contact.SetValueExt<FSContact.email>(fsContactRow, soShippingContactRow.Email);
                    updateRequired = true;
                }
            }

            if (fsServiceOrderRow.AssignedEmpID != fsxSOOrderRow.AssignedEmpID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.assignedEmpID>(fsServiceOrderRow, fsxSOOrderRow.AssignedEmpID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.SLAETA != fsxSOOrderRow.SLAETA)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.sLAETA>(fsServiceOrderRow, fsxSOOrderRow.SLAETA);
                updateRequired = true;
            }

            if (operation == PXDBOperation.Update && updateRequired)
            {
                graphServiceOrder.ServiceOrderRecords.Cache.SetStatus(graphServiceOrder.ServiceOrderRecords.Current, PXEntryStatus.Updated);
            }
        }

        private int? GetBranchLocation(int? branchID)
        {
            int? branchLocationID = null;

            FSBranchLocation fSBranchLocationRow = PXSelect<FSBranchLocation,
                                                            Where<
                                                                  FSBranchLocation.branchID, Equal<Required<FSBranchLocation.branchID>>>>
                                                   .Select(Base, branchID);
            if (fSBranchLocationRow != null)
            {
                branchLocationID = fSBranchLocationRow.BranchLocationID; 
            }

            if (branchLocationID == null)
            {
                throw new PXException(TX.Error.NO_AVAILABLE_BRANCH_LOCATION_IN_CURRENT_BRANCH);
            }

            return branchLocationID;
        }

        /// <summary>
        /// Adjusts the Header after removing, updating or creating the Sales Order.
        /// </summary>
        private FSServiceOrder InsertUpdateDeleteServiceOrderDocument(SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow, PXDBOperation salesOrderOperation)
        {
            ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();
            FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
            PXDBOperation serviceOrderOperation = PXDBOperation.Insert;

            if (fsServiceOrderRow != null)
            {
                //Load existing ServiceOrder
                graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

                //Delete existing ServiceOrder
                if (salesOrderOperation == PXDBOperation.Delete || salesOrderOperation == (PXDBOperation.Insert | PXDBOperation.Update) || fsxSOOrderRow.SDEnabled == false)
                {
                    DeleteServiceOrder(graphServiceOrder, graphServiceOrder.ServiceOrderRecords.Current, fsxSOOrderRow);
                    return null;
                }

                serviceOrderOperation = PXDBOperation.Update;
                graphServiceOrder.SelectTimeStamp();
            }
            else
            {
                //This Sales Order does not require Service Order
                if (fsxSOOrderRow.SDEnabled == false)
                {
                    return null;
                }

                //Init a NEW Service Order
                graphServiceOrder.ServiceOrderRecords.Current = InitNewServiceOrder(graphServiceOrder, soOrderRow, fsxSOOrderRow);
            }

            //Update ServiceOrder header
            UpdateServiceOrderHeader(
                                    graphServiceOrder, 
                                    graphServiceOrder.ServiceOrderRecords.Current,
                                    soOrderRow, 
                                    fsxSOOrderRow,
                                    serviceOrderOperation);

            ////Update ServiceOrder detail
            InsertUpdateDeleteServiceOrderDetail(graphServiceOrder);

            if (graphServiceOrder.IsDirty == true
                    || graphServiceOrder.ServiceOrderRecords.Cache.GetStatus(graphServiceOrder.ServiceOrderRecords.Current) == PXEntryStatus.Updated)
            {
                if (!Base.IsContractBasedAPI 
                        && graphServiceOrder.ServiceOrderRecords.Cache.GetStatus(graphServiceOrder.ServiceOrderRecords.Current) == PXEntryStatus.Inserted)
                {
                    throw new PXRedirectRequiredException(graphServiceOrder, null);
                }

                graphServiceOrder.Save.Press();
            }

            return graphServiceOrder.ServiceOrderRecords.Current;
        }

        private void InsertUpdateDeleteFSSODet<SODetType>(
                                        PXCache cacheFSSODet,
                                        SOLine soLineRow,
                                        FSxSOLine fsxSOLineRow,
                                        FSSODet fsSODetRow,
                                        string lineType)
                where SODetType : FSSODet, IBqlTable, new()
        {
            if (fsSODetRow != null)
            {
                //Delete an existing appointment requirement
                if (fsxSOLineRow.SDSelected == false)
                {
                    cacheFSSODet.Delete(fsSODetRow);
                    return;
                }
                else
                {
                    cacheFSSODet.Current = fsSODetRow;

                    // This copy is to edit the copy row and after update the record in the cache with cache.Update
                    fsSODetRow = (SODetType)cacheFSSODet.CreateCopy(fsSODetRow);

                    InsertUpdateServiceOrderLine<SODetType>(cacheFSSODet, fsSODetRow, false, lineType, soLineRow, fsxSOLineRow);
                    return;
                }
            }
            else
            {
                //This line does not require appointment
                if (fsxSOLineRow.SDSelected == false)
                {
                    return;
                }

                InsertUpdateServiceOrderLine<SODetType>(cacheFSSODet, fsSODetRow, true, lineType, soLineRow, fsxSOLineRow);
            }
        }

        /// <summary>
        /// Enable or Disable Sales Order lines fields.
        /// </summary>
        /// <param name="cache">Sales Order line cache.</param>
        /// <param name="soLineRow">Sales Order line row.</param>
        private void EnableDisableSOline(PXCache cache, SOLine soLineRow, bool fsIntegrationEnabled)
        {
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

            PXUIFieldAttribute.SetEnabled<FSxSOLine.sDSelected>(cache, soLineRow, fsIntegrationEnabled);
            PXUIFieldAttribute.SetEnabled<FSxSOLine.equipmentAction>(cache, soLineRow, soLineRow.InventoryID != null && fsIntegrationEnabled);
        }

        private void DeleteFSSODetLinesFromDeletedSOlines(ServiceOrderEntry graphServiceOrder)
        {
            FSxSOLine fsxSOLineRow = null;

            //Delete SODet lines based on deleted-SOLine collection
            foreach (SOLine soLineRow in Base.Transactions.Cache.Deleted)
            {
                fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);

                //There is nothing to do with a posted line
                if (fsxSOLineRow.SDPosted == true)
                {
                    continue;
                }

                graphServiceOrder.ServiceOrderDetServices.Current = graphServiceOrder.ServiceOrderDetServices
                                                                .Search<FSSODet.sourceLineNbr>(soLineRow.LineNbr);
                if (graphServiceOrder.ServiceOrderDetServices.Current != null)
                {
                    graphServiceOrder.ServiceOrderDetServices.Delete(graphServiceOrder.ServiceOrderDetServices.Current);
                }
                else
                {
                    graphServiceOrder.ServiceOrderDetParts.Current = graphServiceOrder.ServiceOrderDetParts
                                                                    .Search<FSSODet.sourceLineNbr>(soLineRow.LineNbr);
                    if (graphServiceOrder.ServiceOrderDetParts.Current != null)
                    {
                        graphServiceOrder.ServiceOrderDetParts.Delete(graphServiceOrder.ServiceOrderDetParts.Current);
                    }
                }
            }
        }

        private void InsertUpdateDeleteServiceOrderDetail(ServiceOrderEntry graphServiceOrder)
        {
            DeleteFSSODetLinesFromDeletedSOlines(graphServiceOrder);

            FSxSOLine fsxSOLineRow;

            //Insert/Update SODet lines based on existing SOLine lines
            foreach (SOLine soLineRow in Base.Transactions.Select())
            {
                fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);

                //There is nothing to do with a posted line
                if (fsxSOLineRow.SDPosted == true)
                {
                    continue;
                }
                
                InventoryItem inventoryItemRow = 
                    PXSelect<InventoryItem, 
                    Where<
                        InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(this.Base, soLineRow.InventoryID);

                PXCache cacheSODet = null;
                FSSODet fsSODetRow = null;
                string lineType = null;

                switch (inventoryItemRow.ItemType)
                {
                    case INItemTypes.NonStockItem:
                    case INItemTypes.LaborItem:
                    case INItemTypes.ExpenseItem:
                    case INItemTypes.ChargeItem:
                    case INItemTypes.ServiceItem:
                        cacheSODet = graphServiceOrder.ServiceOrderDetServices.Cache;
                        fsSODetRow = graphServiceOrder.ServiceOrderDetServices.Search<FSSODet.sourceLineNbr>(soLineRow.LineNbr);

                        if (inventoryItemRow.ItemType == INItemTypes.ServiceItem)
                        {
                            lineType = ID.LineType_All.SERVICE;
                        }
                        else
                        {
                            lineType = ID.LineType_All.NONSTOCKITEM;
                        }

                        InsertUpdateDeleteFSSODet<FSSODetService>(cacheSODet, soLineRow, fsxSOLineRow, fsSODetRow, lineType);

                        break;

                    case INItemTypes.FinishedGood:
                    case INItemTypes.Component:
                    case INItemTypes.SubAssembly:
                        cacheSODet = graphServiceOrder.ServiceOrderDetParts.Cache;
                        fsSODetRow = graphServiceOrder.ServiceOrderDetParts.Search<FSSODet.sourceLineNbr>(soLineRow.LineNbr);
                        lineType = ID.LineType_All.INVENTORY_ITEM;

                        InsertUpdateDeleteFSSODet<FSSODetPart>(cacheSODet, soLineRow, fsxSOLineRow, fsSODetRow, lineType);

                        break;

                    default:
                        //not supported type
                        continue;
                }
            }

            if (graphServiceOrder.ServiceOrderDetParts.Cache != null 
                    && graphServiceOrder.ServiceOrderDetParts.Cache.Inserted != null)
            { 
                //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                foreach (FSSODetPart fsSODetPartModelRow in graphServiceOrder.ServiceOrderDetParts
                                                                                .Cache.Cached
                                                                                    .Cast<FSSODetPart>()
                                                                                        .Where(x => x.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT).ToList())
                {
                    foreach (FSSODetPart fsSODetPartComponentRow in graphServiceOrder.ServiceOrderDetParts
                                                                                            .Cache.Cached
                                                                                                .Cast<FSSODetPart>()
                                                                                                    .Where(x => (x.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                                                                                                || x.EquipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT)).ToList())
                    {
                        if (fsSODetPartComponentRow.SONewTargetEquipmentLineNbr == fsSODetPartModelRow.SourceLineNbr)
                        {
                            if (fsSODetPartComponentRow.NewTargetEquipmentLineNbr == null)
                            {
                                fsSODetPartComponentRow.NewTargetEquipmentLineNbr = fsSODetPartModelRow.LineRef;
                            }
                        }
                    }
                }
            }
        }

        protected void InsertUpdateServiceOrderLine<SODetType>(
                                    PXCache cacheSODet,
                                    FSSODet fsSODetRow,
                                    bool insertRow,
                                    string lineType,
                                    SOLine soLineRow,
                                    FSxSOLine fsxSOLineRow)
                where SODetType : FSSODet, IBqlTable, new()
        {
            bool updateRequired = false;

            if (insertRow == true)
            {
                fsSODetRow = new SODetType();
                fsSODetRow.SourceLineNbr = soLineRow.LineNbr;
                fsSODetRow.IsPrepaid = true;
                fsSODetRow.ManualPrice = true;
            }

            if (fsSODetRow.InventoryID != soLineRow.InventoryID)
            {
                fsSODetRow.InventoryID = soLineRow.InventoryID;
                updateRequired = true;

                if (insertRow)
                {
                    fsSODetRow.LineType = lineType;
                    if (lineType == ID.LineType_All.SERVICE)
                    {
                        fsSODetRow.BillingRule = ID.BillingRule.FLAT_RATE;
                    }

                    fsSODetRow = (SODetType)cacheSODet.Insert(fsSODetRow);
                }
            }

            if (fsSODetRow.SubItemID != soLineRow.SubItemID)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                {
                    fsSODetRow.SubItemID = soLineRow.SubItemID;
                    updateRequired = true;
                }
            }

            if (fsSODetRow.SiteID != soLineRow.SiteID)
            {
                fsSODetRow.SiteID = soLineRow.SiteID;
                updateRequired = true;
            }

            if (fsSODetRow.SiteLocationID != soLineRow.LocationID)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                {
                    fsSODetRow.SiteLocationID = soLineRow.LocationID;
                    updateRequired = true;
                }
            }

            if (fsSODetRow.UOM != soLineRow.UOM)
            {
                fsSODetRow.UOM = soLineRow.UOM;
                updateRequired = true;
            }

            if (fsSODetRow.ProjectID != soLineRow.ProjectID
                    && ProjectDefaultAttribute.IsNonProject(soLineRow.ProjectID) == false)
            {
                fsSODetRow.ProjectID = soLineRow.ProjectID;
                updateRequired = true;
            }

            if (fsSODetRow.ProjectTaskID != soLineRow.TaskID)
            {
                fsSODetRow.ProjectTaskID = soLineRow.TaskID;
                updateRequired = true;
            }

            if (fsSODetRow.CostCodeID != soLineRow.CostCodeID)
            {
                fsSODetRow.CostCodeID = soLineRow.CostCodeID;
                updateRequired = true;
            }

            if (fsSODetRow.TranDesc != soLineRow.TranDesc)
            {
                fsSODetRow.TranDesc = soLineRow.TranDesc;
                updateRequired = true;
            }

            if (fsSODetRow.EstimatedQty != soLineRow.OrderQty
                || fsSODetRow.CuryUnitPrice != soLineRow.CuryUnitPrice
                || fsSODetRow.CuryEstimatedTranAmt != soLineRow.CuryExtPrice)
            {
                updateRequired = true;
            }

            if (updateRequired == true || insertRow == true)
            {
                fsSODetRow.LineType = lineType;
                if (lineType == ID.LineType_All.SERVICE)
                {
                    fsSODetRow.BillingRule = ID.BillingRule.FLAT_RATE;
                }

                fsSODetRow.IsBillable = false;

                fsSODetRow.EstimatedQty = soLineRow.OrderQty;
                fsSODetRow.CuryUnitPrice = soLineRow.CuryUnitPrice;

                if (lineType == ID.LineType_All.INVENTORY_ITEM)
                {
                    fsSODetRow.EquipmentAction = fsxSOLineRow.EquipmentAction ?? ID.Equipment_Action.NONE;
                    fsSODetRow.SMEquipmentID = fsxSOLineRow.SMEquipmentID;
                    fsSODetRow.EquipmentLineRef = fsxSOLineRow.EquipmentLineRef;
                    fsSODetRow.ComponentID = fsxSOLineRow.ComponentID;

                    //Saving original values from SalesOrder to allow assign the proper NewTargetEquipmentLineNbr value in ServiceOrder
                    fsSODetRow.SONewTargetEquipmentLineNbr = fsxSOLineRow.NewTargetEquipmentLineNbr;
                }

                cacheSODet.Update(fsSODetRow);
            }
        }

        /// <summary>
        /// Clean SM fields depending if SDEnabled checkbox is not selected.
        /// </summary>
        private void CleanSMFieldsBeforeSaving(SOOrder soOrderRow)
        {
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (fsxSOOrderRow.SDEnabled == false)
            {
                fsxSOOrderRow.SrvOrdType = null;
                fsxSOOrderRow.SORefNbr = null;
                fsxSOOrderRow.AssignedEmpID = null;
                fsxSOOrderRow.SLAETA = (System.DateTime?)null;
                fsxSOOrderRow.Installed = false;

                foreach (SOLine soLineRow in Base.Transactions.Select())
                {
                    FSxSOLine fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);
                    fsxSOLineRow.SDSelected = false;
                }
            }
        }

        /// <summary>
        /// Check if the current selected project belongs to the current customer.
        /// This applies only to a Sales Order that will be related with a Service Order.
        /// </summary>
        private void CheckIfCurrentProjectBelongsToCustomer(PXCache cache, SOOrder soOrderRow)
        {
            FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);
            PXSetPropertyException exception = null;

            if (soOrderRow.ProjectID != null
                    && soOrderRow.CustomerID != null
                        && soOrderRow.ProjectID != 0
                            && fsxSOOrderRow.SDEnabled == true)
            {
                Contract contract = PXSelect<Contract,
                            Where<
                                Contract.contractID, Equal<Required<Contract.contractID>>,
                            And<
                                Contract.customerID, Equal<Required<Contract.contractID>>>>>
                          .Select(Base, soOrderRow.ProjectID, soOrderRow.CustomerID);

                if (contract == null)
                {
                    exception = new PXSetPropertyException(TX.Error.PROJECT_MUST_BELONG_TO_CUSTOMER, PXErrorLevel.RowError);
                }

                if (PXUIFieldAttribute.GetError<SOOrder.customerID>(cache, soOrderRow) == string.Empty
                            || PXUIFieldAttribute.GetError<SOOrder.customerID>(cache, soOrderRow) == TX.Error.PROJECT_MUST_BELONG_TO_CUSTOMER)
                {
                    cache.RaiseExceptionHandling<SOOrder.customerID>(
                                                            soOrderRow,
                                                            soOrderRow.CustomerID,
                                                            exception);
                }
            }
        }

        /// <summary>
        /// Check if the given Sales Order line is related with any appointment details.
        /// </summary>
        /// <param name="soOrderRow">Sales Order row.</param>
        /// <param name="soLineRow">Sales Order line.</param>
        /// <returns>Returns true if the Sales Order Line is related with at least one appointment detail.</returns>
        private bool IsLineSourceForAppoinmentLine(SOOrder soOrderRow, SOLine soLineRow, FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow == null)
            {
                fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
            }

            if (fsServiceOrderRow != null)
            {
                FSAppointmentDet fsAppointmentDetRow =
                    PXSelectJoin<FSAppointmentDet,
                    InnerJoin<FSSODet,
                        On<FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>,
                    Where<
                        FSSODet.sourceLineNbr, Equal<Required<FSSODet.sourceLineNbr>>,
                        And<FSSODet.sOID, Equal<Required<FSSODet.sOID>>>>>
                    .SelectWindowed(Base, 0, 1, soLineRow.LineNbr, fsServiceOrderRow.SOID);

                return fsAppointmentDetRow != null;
            }

            return false;
        }

        /// <summary>
        /// Check if the given Sales Order Lines are different in Service Management prepaid related fields.
        /// </summary>
        private bool ArePrepaidFieldsBeingModified(SOLine soLineRow, SOLine newSOLineRow)
        {
            FSxSOLine fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);
            FSxSOLine newFSxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(newSOLineRow);

            return soLineRow.InventoryID        != newSOLineRow.InventoryID
                   || soLineRow.SubItemID       != newSOLineRow.SubItemID
                   || soLineRow.SiteID          != newSOLineRow.SiteID
                   || soLineRow.UOM             != newSOLineRow.UOM
                   || soLineRow.UnitPrice       != newSOLineRow.UnitPrice
                   || soLineRow.OrderQty        != newSOLineRow.OrderQty
                   || soLineRow.ProjectID       != newSOLineRow.ProjectID
                   || soLineRow.TaskID          != newSOLineRow.TaskID
                   || soLineRow.TranDesc        != newSOLineRow.TranDesc
                   || fsxSOLineRow.SDSelected   != newFSxSOLineRow.SDSelected;
        }

        #region EnableDisable functions
        /// <summary>
        /// Enables/Disables the "Open Appointment Board" button.
        /// </summary>
        protected virtual void EnableDisableActions(PXCache cache, SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow, bool allowCreateServiceOrder, bool fsIntegrationEnabled)
        {
            bool enabledBoard = false;
            bool enabledCreateSO = false;

            if (soOrderRow != null && cache.GetStatus(soOrderRow) != PXEntryStatus.Inserted
                                && fsxSOOrderRow != null)
            {
                if (fsxSOOrderRow.SORefNbr != null)
                {
                    enabledBoard = true;
                }
                else
                {
                    enabledCreateSO = true;
                }
            }

            OpenAppointmentBoard.SetEnabled(enabledBoard && fsIntegrationEnabled);
            CreateServiceOrder.SetEnabled(enabledCreateSO && allowCreateServiceOrder && fsIntegrationEnabled);
            ViewServiceOrder.SetEnabled(enabledBoard && fsIntegrationEnabled);
        }

        /// <summary>
        /// Enables the Fields if they can be edited.
        /// </summary>
        protected virtual void EnableDisable_All(PXCache cache, SOOrder soOrderRow, bool fsIntegrationEnabled)
        {
            FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);

            bool allowCreateServiceOrder = CanCreateServiceOrder(cache, soOrderRow, fsxSOOrderRow);

            EnableDisableActions(cache, soOrderRow, fsxSOOrderRow, allowCreateServiceOrder, fsIntegrationEnabled);

            bool areReferencesVisible = allowCreateServiceOrder == false && fsIntegrationEnabled;
            PXUIFieldAttribute.SetVisible<FSxSOLine.appointmentID>(Base.Transactions.Cache, null, areReferencesVisible);
            PXUIFieldAttribute.SetVisible<FSxSOLine.sOID>(Base.Transactions.Cache, null, areReferencesVisible);
        }
        #endregion

        #region Equipment Customization

        protected virtual void UpdateQty(PXCache cache, SOLine soLineRow)
        {
            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            bool updateQty = false;
            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);

            if (fsxSOLineRow == null)
            {
                return;
            }

            updateQty = fsxSOLineRow.EquipmentAction == ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT;

            updateQty = updateQty || ((fsxSOLineRow.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT || fsxSOLineRow.EquipmentAction == ID.Equipment_Action.REPLACING_COMPONENT)
                                            && ((fsxSOLineRow.SMEquipmentID != null && fsxSOLineRow.ComponentID != null)
                                                    || (fsxSOLineRow.NewTargetEquipmentLineNbr != null && fsxSOLineRow.ComponentID != null)));
            if (updateQty)
            {
                cache.SetValueExt<SOLine.orderQty>(soLineRow, (decimal)1.0);
            }
        }

        #endregion
        #endregion

        #region Event Handlers
        #region SOOrder Event Handlers

        protected virtual void SOOrder_SDEnabled_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOOrder soOrderRow = (SOOrder)e.Row;
            FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (IsFSIntegrationEnabled() == false)
            {
                fsxSOOrderRow.SDEnabled = false;
                return;
            }

            if (fsxSOOrderRow.SDEnabled == false)
            {
                FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
                fsxSOOrderRow.SrvOrdType = null;

                if (fsServiceOrderRow != null)
                {
                    foreach (SOLine soLineRow in Base.Transactions.Select())
                    {
                        if (IsLineSourceForAppoinmentLine(soOrderRow, soLineRow, fsServiceOrderRow))
                        {
                            throw new PXException(TX.Error.ERROR_DELETING_RELATED_SERVICE_ORDER + " " + TX.Error.SOME_SOLINES_HAVE_RELATED_APPOINTMENT_DETAILS, fsServiceOrderRow.RefNbr);
                        }
                    }
                }

                CleanSMFieldsBeforeSaving(soOrderRow);
            }
            else
            {
                if (fsxSOOrderRow.SrvOrdType == null)
                {
                    FSSetup fsSetupRow = PXSelect<FSSetup>.Select(Base);
                    if (fsSetupRow != null)
                    {
                        fsxSOOrderRow.SrvOrdType = fsSetupRow.DfltSOSrvOrdType;
                    }
                }
            }
        }

        protected virtual void SOOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOOrder soOrderRow = (SOOrder)e.Row;

            bool fsIntegrationEnabled = IsFSIntegrationEnabled();
            DACHelper.SetExtensionVisibleInvisible<FSxSOOrder>(cache, e, fsIntegrationEnabled, false);
            OpenAppointmentBoard.SetVisible(fsIntegrationEnabled);
            CreateServiceOrder.SetVisible(fsIntegrationEnabled);
            ViewServiceOrder.SetVisible(fsIntegrationEnabled);

            EnableDisable_All(cache, soOrderRow, fsIntegrationEnabled);

            if (fsIntegrationEnabled == true)
            {
                CheckIfCurrentProjectBelongsToCustomer(cache, soOrderRow);
            }
        }

        protected virtual void SOOrder_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOOrder soOrderRow = (SOOrder)e.Row;
            FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (e.TranStatus == PXTranStatus.Open)
            {
                if (e.Operation == PXDBOperation.Delete)
                {
                    InvoicingFunctions.CleanPostingInfoLinkedToDoc(soOrderRow);
                    InvoicingFunctions.CleanContractPostingInfoLinkedToDoc(soOrderRow);
                }

                if (CanCreateServiceOrder(cache, soOrderRow, fsxSOOrderRow) == false)
                {
                    return;
                }

                if (fsxSOOrderRow.SDEnabled == true
                        && fsxSOOrderRow.SrvOrdType != null
                            && fsxSOOrderRow.SORefNbr != null)
                {
                    PXUpdate<
                        Set<FSServiceOrder.sourceDocType, Required<FSServiceOrder.sourceDocType>,
                        Set<FSServiceOrder.sourceRefNbr, Required<FSServiceOrder.sourceRefNbr>>>,
                    FSServiceOrder,
                    Where<
                        FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                        And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                    .Update(cache.Graph, soOrderRow.OrderType, soOrderRow.OrderNbr, fsxSOOrderRow.SrvOrdType, fsxSOOrderRow.SORefNbr);
                }
            }

            if (e.TranStatus == PXTranStatus.Aborted)
            {
                Base.Document.SetValueExt<FSxSOOrder.soRefNbr>(soOrderRow, null);
            }
        }

        protected virtual void SOOrder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOOrder soOrderRow = (SOOrder)e.Row;
            FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (e.Operation == PXDBOperation.Delete)
            {
                if (fsxSOOrderRow != null
                        && fsxSOOrderRow.SORefNbr != null)
                {
                    throw new PXException(TX.Error.CANNOT_DELETE_SALES_ORDER_BECAUSE_IT_HAS_A_SERVICE_ORDER);
                }
            }
            else
            {
                ValidatePostBatchStatus(e.Operation, ID.Batch_PostTo.SO, soOrderRow.OrderType, soOrderRow.RefNbr);
            }

            if (soOrderRow.Status != SOOrderStatus.Completed)
            {
                var fsServiceOrderRow = InsertUpdateDeleteServiceOrderDocument(soOrderRow, fsxSOOrderRow, e.Operation);

                if (fsServiceOrderRow != null
                        && fsxSOOrderRow.SORefNbr != fsServiceOrderRow.RefNbr)
                {
                    Base.Document.SetValueExt<FSxSOOrder.soRefNbr>(soOrderRow, fsServiceOrderRow.RefNbr);
                }
            }
        }
        #endregion

        #region SOLine Event Handlers

        protected virtual void SOLine_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLine = (SOLine)e.Row;

            bool updateQty = SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SALES_ORDER) == Base.Accessinfo.ScreenID;
            SharedFunctions.UpdateEquipmentFields(Base, cache, soLine, soLine.InventoryID, updateQty);
        }

        protected virtual void SOLine_EquipmentAction_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLine = (SOLine)e.Row;

            if (e.ExternalCall == true)
            {
                SharedFunctions.ResetEquipmentFields(cache, soLine);
                SharedFunctions.UpdateEquipmentAction(cache, soLine);
                UpdateQty(cache, soLine);
            }
        }

        protected virtual void SOLine_SMEquipmentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            UpdateQty(cache, soLineRow);
        }

        protected virtual void SOLine_ComponentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            UpdateQty(cache, soLineRow);
        }

        protected virtual void SOLine_EquipmentLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);

            if (fsxSOLineRow.ComponentID == null)
            {
                fsxSOLineRow.ComponentID = SharedFunctions.GetEquipmentComponentID(Base, fsxSOLineRow.SMEquipmentID, fsxSOLineRow.EquipmentLineRef);
            }
        }

        protected virtual void SOLine_SDSelected_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLine = (SOLine)e.Row;
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

            e.NewValue = (bool)fsxSOOrderRow.SDEnabled && fsxSOOrderRow.SORefNbr != null;
        }

        protected virtual void SOLine_OrderQty_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;

            if (IsLineCreatedFromAppSO(Base, Base.Document.Current, soLineRow, typeof(SOLine.orderQty).Name) == true)
            {
                throw new PXSetPropertyException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
            }
        }

        protected virtual void SOLine_UOM_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;

            if (IsLineCreatedFromAppSO(Base, Base.Document.Current, soLineRow, typeof(SOLine.uOM).Name) == true)
            {
                throw new PXSetPropertyException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
            }
        }

        protected virtual void SOLine_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;

            if (IsLineSourceForAppoinmentLine(Base.Document.Current, soLineRow, null))
            {
                throw new PXException(TX.Error.SOLINE_HAS_RELATED_APPOINTMENT_DETAILS);
            }

            if (e.ExternalCall == true)
            {
                if (IsLineCreatedFromAppSO(Base, Base.Document.Current, soLineRow, null) == true)
                {
                    throw new PXException(TX.Error.NO_DELETION_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
                }
            }
        }

        protected virtual void SOLine_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);

            SOLine newSOLineRow = (SOLine)e.NewRow;
            FSxSOLine newfsxSOLineRow = cache.GetExtension<FSxSOLine>(newSOLineRow);

            if (e.ExternalCall == true)
            {
                if (ArePrepaidFieldsBeingModified(soLineRow, newSOLineRow))
                {
                    if (IsLineSourceForAppoinmentLine(Base.Document.Current, soLineRow, null))
                    {
                        e.Cancel = true;
                        throw new PXException(TX.Error.SOLINE_HAS_RELATED_APPOINTMENT_DETAILS);
                    }
                }

                if ((soLineRow.InventoryID != newSOLineRow.InventoryID 
                        ||  soLineRow.SubItemID != newSOLineRow.SubItemID)
                            && IsLineCreatedFromAppSO(Base, Base.Document.Current, soLineRow, null) == true)
                {
                    throw new PXException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
                }
            }
        }

        protected virtual void SOLine_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            bool fsIntegrationEnabled = IsFSIntegrationEnabled();
            DACHelper.SetExtensionVisibleInvisible<FSxSOLine>(cache, e, fsIntegrationEnabled, true);

            if (e.Row == null)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            EnableDisableSOline(cache, soLineRow, fsIntegrationEnabled);

            if (fsIntegrationEnabled == true)
            {
                bool updateQty = SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SALES_ORDER) == Base.Accessinfo.ScreenID;
                SharedFunctions.UpdateEquipmentAction(cache, soLineRow, updateQty);
            }
        }

        protected virtual void SOLine_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);
            using (new PXConnectionScope())
            {
                SharedFunctions.SetInventoryItemExtensionInfo(Base, soLineRow.InventoryID, fsxSOLineRow);
            }
        }

        protected virtual void SOLine_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);
            string errorMessage = string.Empty;

            if (e.Operation != PXDBOperation.Delete 
                    && !SharedFunctions.AreEquipmentFieldsValid(cache, soLineRow.InventoryID, fsxSOLineRow.SMEquipmentID, fsxSOLineRow.NewTargetEquipmentLineNbr, fsxSOLineRow.EquipmentAction, ref errorMessage))
            {
                cache.RaiseExceptionHandling<FSxSOLine.equipmentAction>(soLineRow, fsxSOLineRow.EquipmentAction, new PXSetPropertyException(errorMessage));
            }
        }

        protected virtual void SOLine_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;

            //The CheckCacheState needs to be run when the transaction is aborted to verify the status of the fields
            //for the current row in order to catch exceptions thrown by other modules to allow them be shown in the
            //Generate Invoice screen.
            if (e.TranStatus == PXTranStatus.Aborted && IsInvoiceProcessRunning == true)
            {
                SharedFunctions.CheckCacheState(cache, soLineRow);
            }
        }

        #endregion
        #endregion

        #region Invoicing Methods
        public virtual bool IsInvoiceProcessRunning { get; set; }

        public virtual List<ErrorInfo> GetErrorInfo()
        {
            List<ErrorInfo> errorList = new List<ErrorInfo>();
            ErrorInfo errorInfo;

            string headerErrorMessage = SharedFunctions.CheckCacheState(Base.Document.Cache, Base.Document.Current, true);
            bool headerHasError = string.IsNullOrEmpty(headerErrorMessage) == false;

            foreach (SOLine soLineRow in Base.Transactions.Select())
            {
                string errorMessage = SharedFunctions.CheckCacheState(Base.Transactions.Cache, soLineRow, true, true, true);

                if (string.IsNullOrEmpty(errorMessage) == false || headerHasError)
                {
                    FSxSOLine fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);

                    errorInfo = new ErrorInfo()
                    {
                        HeaderError = headerHasError,
                        SOID = fsxSOLineRow.SOID,
                        AppointmentID = fsxSOLineRow.AppointmentID,
                        ErrorMessage = (headerHasError ? headerErrorMessage : errorMessage) + Environment.NewLine
                    };

                    errorList.Add(errorInfo);
                    if (headerHasError)
                    {
                        break;
                    }
                }
            }

            return errorList;
        }

        public virtual void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, List<DocLineExt> docLinesGrouped, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow)
        {
            if (docLines.Count == 0)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = docLines[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLines[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLines[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLines[0].fsAppointment;

            bool? initialHold = false;

            //// This event is raised so the Sales Order page can be opened with the branch indicated in the Appointment
            //// WARNING: Assigning the BranchID directly, behaves incorrectly
            //// Note: The AddHandler method is run before the corresponding method defined in the original graph
            Base.FieldDefaulting.AddHandler<SOOrder.branchID>((sender, e) =>
            {
                e.NewValue = fsServiceOrderRow.BranchID;
                e.Cancel = true;
            });

            SOOrder sOOrderRow = new SOOrder();

            if (invtMult >= 0)
            {
                if (string.IsNullOrEmpty(fsPostDocRow.PostOrderType))
                {
                    throw new PXException(TX.Error.POST_ORDER_TYPE_MISSING_IN_SETUP);
                }

                sOOrderRow.OrderType = fsPostDocRow.PostOrderType;
            }
            else
            {
                if (string.IsNullOrEmpty(fsPostDocRow.PostOrderTypeNegativeBalance))
                {
                    throw new PXException(TX.Error.POST_ORDER_NEGATIVE_BALANCE_TYPE_MISSING_IN_SETUP);
                }

                sOOrderRow.OrderType = fsPostDocRow.PostOrderTypeNegativeBalance;
            }

            sOOrderRow.InclCustOpenOrders = true;
            sOOrderRow.CustomerOrderNbr = fsServiceOrderRow.CustPORefNbr;
            sOOrderRow = Base.Document.Current = Base.Document.Insert(sOOrderRow);
            initialHold = sOOrderRow.Hold;
            sOOrderRow.NoteID = null;
            PXNoteAttribute.GetNoteIDNow(Base.Document.Cache, sOOrderRow);
            Base.Document.Cache.SetValueExt<SOOrder.hold>(sOOrderRow, true);
            Base.Document.Cache.SetValueExt<SOOrder.orderDate>(sOOrderRow, invoiceDate);
            Base.Document.Cache.SetValueExt<SOOrder.requestDate>(sOOrderRow, fsServiceOrderRow.OrderDate);
            Base.Document.Cache.SetValueExt<SOOrder.customerID>(sOOrderRow, fsServiceOrderRow.BillCustomerID);
            Base.Document.Cache.SetValueExt<SOOrder.customerLocationID>(sOOrderRow, fsServiceOrderRow.BillLocationID);
            Base.Document.Cache.SetValueExt<SOOrder.curyID>(sOOrderRow, fsServiceOrderRow.CuryID);

            Base.Document.Cache.SetValueExt<SOOrder.overrideTaxZone>(sOOrderRow, true);
            Base.Document.Cache.SetValueExt<SOOrder.taxZoneID>(sOOrderRow, fsAppointmentRow != null ? fsAppointmentRow.TaxZoneID : fsServiceOrderRow.TaxZoneID);

            Base.Document.Cache.SetValueExt<SOOrder.orderDesc>(sOOrderRow, fsServiceOrderRow.DocDesc);

            if (fsServiceOrderRow.ProjectID != null)
            {
                Base.Document.Cache.SetValueExt<SOOrder.projectID>(sOOrderRow, fsServiceOrderRow.ProjectID);
            }

            string termsID = InvoicingFunctions.GetTermsIDFromCustomerOrVendor(Base, fsServiceOrderRow.BillCustomerID, null);
            if (termsID != null)
            {
                Base.Document.Cache.SetValueExt<SOOrder.termsID>(sOOrderRow, termsID);
            }
            else
            {
                Base.Document.Cache.SetValueExt<SOOrder.termsID>(sOOrderRow, fsSrvOrdTypeRow.DfltTermIDARSO);
            }

            Base.Document.Cache.SetValueExt<SOOrder.ownerID>(sOOrderRow, null);

            sOOrderRow = Base.Document.Update(sOOrderRow);

            InvoicingFunctions.SetContactAndAddress(Base, fsServiceOrderRow);

            if (onDocumentHeaderInserted != null)
            {
                onDocumentHeaderInserted(Base, sOOrderRow);
            }

            IDocLine docLine = null;
            SOLine sOLineRow = null;
            FSxSOLine fSxSOLineRow = null;
            List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();

            List<GroupDocLineExt> singleLines = 
                    docLines.GroupBy(
                        x => new { x.docLine.DocID , x.docLine.LineID },
                        (key, group) 
                        => new GroupDocLineExt(key.DocID, key.LineID , group.ToList())).ToList();

            foreach (GroupDocLineExt singleLine in singleLines)
            {
                DocLineExt docLineExt = singleLine.Group.First();

                docLine = docLineExt.docLine;
                fsPostDocRow = docLineExt.fsPostDoc;
                fsServiceOrderRow = docLineExt.fsServiceOrder;
                fsSrvOrdTypeRow = docLineExt.fsSrvOrdType;
                fsAppointmentRow = docLineExt.fsAppointment;

                sOLineRow = new SOLine();
                sOLineRow = Base.Transactions.Current = Base.Transactions.Insert(sOLineRow);
                SOLine copySoLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

                copySoLineRow.BranchID = docLine.BranchID;
                copySoLineRow.InventoryID = docLine.InventoryID;

                if (sOLineRow.LineType == SOLineType.Inventory && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                {
                    copySoLineRow.SubItemID = docLine.SubItemID;
                }

                copySoLineRow.SiteID = docLine.SiteID;
                if (docLine.SiteLocationID != null)
                {
                    copySoLineRow.LocationID = docLine.SiteLocationID;
                }

                copySoLineRow.UOM = docLine.UOM;

                if (docLine.ProjectID != null && docLine.ProjectTaskID != null)
                {
                    copySoLineRow.TaskID = docLine.ProjectTaskID;
                }

                copySoLineRow.SalesAcctID = docLine.AcctID;
                copySoLineRow.TaxCategoryID =  docLine.TaxCategoryID;
                copySoLineRow.TranDesc = docLine.TranDesc;
                copySoLineRow.ManualDisc = docLine.IsBillable == false;
                sOLineRow = Base.Transactions.Update(copySoLineRow);

                if (fsAppointmentRow == null)
                {
                    bool qtyAssigned = false;

                    foreach (DocLineExt splitLine in singleLine.Group)
                    {
                        if (splitLine.fsSODetSplit.SplitLineNbr != null && splitLine.fsSODetSplit.Completed == false)
                        {
                            SOLineSplit split = new SOLineSplit();
                            split = Base.splits.Insert(split);
                            SOLineSplit copySplit = (SOLineSplit)Base.splits.Cache.CreateCopy(split);

                            copySplit.SiteID = splitLine.fsSODetSplit.SiteID != null ? splitLine.fsSODetSplit.SiteID : copySplit.SiteID;
                            copySplit.LocationID = splitLine.fsSODetSplit.LocationID != null ? splitLine.fsSODetSplit.LocationID : copySplit.LocationID;
                            copySplit.LotSerialNbr = splitLine.fsSODetSplit.LotSerialNbr;
                            copySplit.Qty = splitLine.fsSODetSplit.Qty;

                            split = Base.splits.Update(copySplit);
                            qtyAssigned = true;
                        }
                    }

                    copySoLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

                    if (qtyAssigned == false)
                    {
                        copySoLineRow.OrderQty = docLine.GetQty(FieldType.BillableField);
                    }
                    else if (sOLineRow.OrderQty != docLine.GetQty(FieldType.BillableField))
                    {
                        throw new PXException(TX.Error.QTY_POSTED_ERROR);
                    }
                }
                else
                {
                    bool qtyAssigned = false;
                    if (string.IsNullOrEmpty(docLine.LotSerialNbr) == false)
                    {
                        SOLineSplit split = new SOLineSplit();
                        split = Base.splits.Insert(split);
                        SOLineSplit copySplit = (SOLineSplit)Base.splits.Cache.CreateCopy(split);

                        copySplit.SiteID = docLine.SiteID;
                        copySplit.LocationID = docLine.SiteLocationID != null ? docLine.SiteLocationID : copySplit.LocationID;
                        copySplit.LotSerialNbr = docLine.LotSerialNbr != null ? docLine.LotSerialNbr : copySplit.LotSerialNbr;
                        copySplit.Qty = docLine.GetQty(FieldType.BillableField);

                        split = Base.splits.Update(copySplit);
                        qtyAssigned = true;
                    }

                    copySoLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

                    if (qtyAssigned == false)
                    {
                        copySoLineRow.OrderQty = docLine.GetQty(FieldType.BillableField);
                    }
                    else if (sOLineRow.OrderQty != docLine.GetQty(FieldType.BillableField))
                    {
                        throw new PXException(TX.Error.QTY_POSTED_ERROR);
                    }
                }

                copySoLineRow.ManualPrice = docLine.ManualPrice;
                copySoLineRow.CuryUnitPrice = docLine.CuryUnitPrice * invtMult;

                if (docLine.IsBillable == false)
                {
                    copySoLineRow.IsFree = true;
                }

                sOLineRow = Base.Transactions.Update(copySoLineRow);
                copySoLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

                copySoLineRow.SalesPersonID = fsAppointmentRow == null ? fsServiceOrderRow.SalesPersonID : fsAppointmentRow.SalesPersonID;

                sOLineRow = Base.Transactions.Update(copySoLineRow);

                if (docLine.SubID != null)
                {
                    try
                    {
                        Base.Transactions.Cache.SetValueExt<SOLine.salesSubID>(sOLineRow, docLine.SubID);
                    }
                    catch (PXException)
                    {
                        sOLineRow.SalesSubID = null;
                        sOLineRow = Base.Transactions.Update(sOLineRow);
                    }
                }
                else
                {
                    InvoicingFunctions.SetCombinedSubID(
                                        Base,
                                        Base.Transactions.Cache,
                                        null,
                                        null,
                                        sOLineRow,
                                        fsSrvOrdTypeRow,
                                        sOLineRow.BranchID,
                                        sOLineRow.InventoryID,
                                        fsServiceOrderRow.BillLocationID,
                                        fsServiceOrderRow.BranchLocationID,
                                        fsServiceOrderRow.SalesPersonID,
                                        docLine.IsService);

                    sOLineRow = Base.Transactions.Update(sOLineRow);
                }

                Base.Transactions.Cache.SetValueExt<SOLine.commissionable>(sOLineRow, fsAppointmentRow?.Commissionable ?? fsServiceOrderRow.Commissionable ?? false);

                Base.Transactions.SetValueExt<SOLine.costCodeID>(sOLineRow, docLine.CostCodeID);

                //Set the line as a posted
                fSxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(sOLineRow);

                fSxSOLineRow.SDPosted = true;

                fSxSOLineRow.Source = docLine.BillingBy;
                fSxSOLineRow.SOID = fsServiceOrderRow.SOID;
                fSxSOLineRow.ServiceOrderDate = fsServiceOrderRow?.OrderDate;
                fSxSOLineRow.BillCustomerID = fsServiceOrderRow.BillCustomerID;
                fSxSOLineRow.CustomerLocationID = fsServiceOrderRow.BillLocationID;
                fSxSOLineRow.SODetID = docLine.PostSODetID;
                fSxSOLineRow.AppointmentID = docLine.PostAppointmentID;
                fSxSOLineRow.AppDetID = docLine.PostAppDetID;
                fSxSOLineRow.AppointmentDate = fsAppointmentRow?.ExecutionDate;

                if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
                {
                    if (docLine.EquipmentAction != null)
                    {
                        Base.Transactions.SetValueExt<FSxSOLine.equipmentAction>(sOLineRow, docLine.EquipmentAction);
                        Base.Transactions.SetValueExt<FSxSOLine.sMEquipmentID>(sOLineRow, docLine.SMEquipmentID);
                        Base.Transactions.SetValueExt<FSxSOLine.equipmentLineRef>(sOLineRow, docLine.EquipmentLineRef);

                        fSxSOLineRow.Comment = docLine.Comment;

                        if (docLine.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT
                            || ((docLine.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.NONE)
                                    && string.IsNullOrEmpty(docLine.NewTargetEquipmentLineNbr) == false))
                        {
                            componentList.Add(new SharedClasses.SOARLineEquipmentComponent(docLine, sOLineRow, fSxSOLineRow));
                        }
                        else
                        {
                            fSxSOLineRow.ComponentID = docLine.ComponentID;
                        }
                    }
                }

                SharedFunctions.CopyNotesAndFiles(Base.Transactions.Cache, sOLineRow, docLine, fsSrvOrdTypeRow);
                fsPostDocRow.DocLineRef = sOLineRow = Base.Transactions.Update(sOLineRow);

                if (onTransactionInserted != null)
                {
                    onTransactionInserted(Base, sOLineRow);
                }
            }

            if(componentList.Count > 0)
            { 
                //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                foreach (SharedClasses.SOARLineEquipmentComponent currLineModel in componentList.Where(x => x.equipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT))
                {
                    foreach (SharedClasses.SOARLineEquipmentComponent currLineComponent in componentList.Where(x => (x.equipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.NONE)))
                    {
                        if (currLineComponent.sourceNewTargetEquipmentLineNbr == currLineModel.sourceLineRef)
                        {
                            currLineComponent.fsxSOLineRow.ComponentID = currLineComponent.componentID;
                            currLineComponent.fsxSOLineRow.NewTargetEquipmentLineNbr = currLineModel.currentLineRef;
                        }
                    }
                }
            }

            if (Base.soordertype.Current.RequireControlTotal == true)
            {
                Base.Document.Cache.SetValueExt<SOOrder.curyControlTotal>(sOOrderRow, sOOrderRow.CuryOrderTotal);
            }

            if (initialHold != true)
            {
                Base.Document.Cache.SetValueExt<SOOrder.hold>(sOOrderRow, false);
            }

            sOOrderRow = Base.Document.Update(sOOrderRow);
        }

        public virtual FSCreatedDoc PressSave(int batchID, BeforeSaveDelegate beforeSave)
        {
            if (Base.Document.Current == null)
            {
                throw new SharedClasses.TransactionScopeException();
            }

            if (beforeSave != null)
            {
                beforeSave(Base);
            }

            Base.SelectTimeStamp();

            SOOrderEntryExternalTax TaxGraphExt = Base.GetExtension<SOOrderEntryExternalTax>();

            if (TaxGraphExt != null)
            {
                TaxGraphExt.SkipTaxCalcAndSave();
            }
            else
            {
            Base.Save.Press();
            }

            string orderType = Base.Document.Current.OrderType;
            string orderNbr = Base.Document.Current.OrderNbr;

            // Reload SOOrder to get the current value of IsTaxValid
            Base.Clear();
            Base.Document.Current = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(Base, orderType, orderNbr);
            SOOrder soOrderRow = Base.Document.Current;

            var fsCreatedDocRow = new FSCreatedDoc()
            {
                BatchID = batchID,
                PostTo = ID.Batch_PostTo.SO,
                CreatedDocType = soOrderRow.OrderType,
                CreatedRefNbr = soOrderRow.OrderNbr
            };

            return fsCreatedDocRow;
        }

        public virtual void Clear()
        {
            Base.Clear(PXClearOption.ClearAll);
        }

        public virtual PXGraph GetGraph()
        {
            return Base;
        }

        public virtual void DeleteDocument(FSCreatedDoc fsCreatedDocRow)
        {
            Base.Document.Current = Base.Document.Search<SOOrder.orderNbr>(fsCreatedDocRow.CreatedRefNbr, fsCreatedDocRow.CreatedDocType);

            if (Base.Document.Current != null)
            {
                if (Base.Document.Current.OrderNbr == fsCreatedDocRow.CreatedRefNbr
                        && Base.Document.Current.OrderType == fsCreatedDocRow.CreatedDocType)
                {
                    Base.Delete.Press();
                }
            }
        }

        public virtual void CleanPostInfo(PXGraph cleanerGraph, FSPostDet fsPostDetRow)
        {
            PXUpdate<
                Set<FSPostInfo.sOLineNbr, Null,
                Set<FSPostInfo.sOOrderNbr, Null,
                Set<FSPostInfo.sOOrderType, Null,
                Set<FSPostInfo.sOPosted, False>>>>,
            FSPostInfo,
            Where<
                FSPostInfo.postID, Equal<Required<FSPostInfo.postID>>,
                And<FSPostInfo.sOPosted, Equal<True>>>>
            .Update(cleanerGraph, fsPostDetRow.PostID);
        }

        public virtual bool IsLineCreatedFromAppSO(PXGraph cleanerGraph, object document, object lineDoc, string fieldName)
        {
            if (document == null || lineDoc == null)
            {
                return false;
            }

            string refNbr = ((SOOrder)document).RefNbr;
            string docType = ((SOOrder)document).OrderType;
            int? lineNbr = ((SOLine)lineDoc).LineNbr;

            return PXSelect<FSPostInfo, 
                    Where<FSPostInfo.sOOrderNbr, Equal<Required<FSPostInfo.sOOrderNbr>>,
                      And<FSPostInfo.sOOrderType, Equal<Required<FSPostInfo.sOOrderType>>,
                      And<FSPostInfo.sOLineNbr, Equal<Required<FSPostInfo.sOLineNbr>>,
                      And<FSPostInfo.sOPosted, Equal<True>>>>>>
            .Select(cleanerGraph, refNbr, docType, lineNbr).Count() > 0;
        }

        #region Invoice By Contract Period Methods 
        public virtual FSContractPostDoc CreateInvoiceByContract(PXGraph graphProcess, DateTime? invoiceDate, string invoiceFinPeriodID, FSContractPostBatch fsContractPostBatchRow, FSServiceContract fsServiceContractRow, FSContractPeriod fsContractPeriodRow, List<ContractInvoiceLine> docLines)
        {
            if (docLines.Count == 0)
            {
                return null;
            }

            FSSetup fsSetupRow = ServiceManagementSetup.GetServiceManagementSetup(graphProcess);

            //// This event is raised so the Sales Order page can be opened with the branch indicated in the Appointment
            //// WARNING: Assigning the BranchID directly, behaves incorrectly
            //// Note: The AddHandler method is run before the corresponding method defined in the original graph
            Base.FieldDefaulting.AddHandler<SOOrder.branchID>((sender, e) =>
            {
                e.NewValue = fsServiceContractRow.BranchID;
                e.Cancel = true;
            });

            FSBranchLocation fSBranchLocationRow = PXSelect<FSBranchLocation,
                                                   Where<
                                                        FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                                   .Select(graphProcess, fsServiceContractRow.BranchLocationID);

            Base.Document.Cache.Clear();
            Base.Document.Cache.ClearQueryCache();
            Base.Document.View.Clear();

            SOOrder sOOrderRow = new SOOrder();

            sOOrderRow.OrderType = fsSetupRow.ContractPostOrderType;

            sOOrderRow.InclCustOpenOrders = true;
            sOOrderRow.Hold = true;
            sOOrderRow = Base.Document.Current = Base.Document.Insert(sOOrderRow);

            Base.Document.Cache.SetValueExt<SOOrder.orderDate>(sOOrderRow, invoiceDate);
            Base.Document.Cache.SetValueExt<SOOrder.requestDate>(sOOrderRow, invoiceDate);
            Base.Document.Cache.SetValueExt<SOOrder.customerID>(sOOrderRow, fsServiceContractRow.BillCustomerID);
            Base.Document.Cache.SetValueExt<SOOrder.customerLocationID>(sOOrderRow, fsServiceContractRow.BillLocationID);

            Base.Document.Cache.SetValueExt<SOOrder.orderDesc>(sOOrderRow, "Contract Standardized Billing: " + fsServiceContractRow.RefNbr + " " + fsServiceContractRow.DocDesc);

            string termsID = InvoicingFunctions.GetTermsIDFromCustomerOrVendor(Base, fsServiceContractRow.BillCustomerID, null);

            if (termsID != null)
            {
                Base.Document.Cache.SetValueExt<SOOrder.termsID>(sOOrderRow, termsID);
            }
            else
            {
                Base.Document.Cache.SetValueExt<SOOrder.termsID>(sOOrderRow, fsSetupRow.DfltContractTermIDARSO);
            }

            Base.Document.Cache.SetValueExt<SOOrder.ownerID>(sOOrderRow, null);

            sOOrderRow = Base.Document.Update(sOOrderRow);

            Base.Transactions.Cache.Clear();
            Base.Transactions.Cache.ClearQueryCache();
            Base.Transactions.View.Clear();

            SOLine sOLineRow = null;
            FSxSOLine fSxSOLineRow = null;
            int? acctID;
            List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();

            foreach (ContractInvoiceLine docLine in docLines)
            {
                sOLineRow = new SOLine();
                sOLineRow = Base.Transactions.Current = Base.Transactions.Insert(sOLineRow);

                Base.Transactions.SetValueExt<SOLine.inventoryID>(sOLineRow, docLine.InventoryID);

                if (sOLineRow.SubItemID == null && docLine.SubItemID != null 
                        && sOLineRow.LineType == SOLineType.Inventory && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                {
                    Base.Transactions.SetValueExt<SOLine.subItemID>(sOLineRow, docLine.SubItemID);
                }

                if (sOLineRow.SiteID == null && docLine.SiteID != null)
                {
                    Base.Transactions.SetValueExt<SOLine.siteID>(sOLineRow, docLine.SiteID);
                }

                if (docLine.SiteLocationID != null)
                {
                    Base.Transactions.SetValueExt<SOLine.locationID>(sOLineRow, docLine.SiteLocationID);
                }

                Base.Transactions.SetValueExt<SOLine.uOM>(sOLineRow, docLine.UOM);

                Base.Transactions.Cache.SetValueExt<SOLine.salesPersonID>(sOLineRow, docLine.SalesPersonID);

                sOLineRow = Base.Transactions.Update(sOLineRow);

                if (docLine.AcctID != null)
                {
                    acctID = docLine.AcctID;
                }
                else
                {
                    acctID = (int?)ServiceOrderCore.Get_INItemAcctID_DefaultValue(
                        graphProcess,
                        fsSetupRow.ContractSalesAcctSource,
                        docLine.InventoryID,
                        fsServiceContractRow);
                }

                Base.Transactions.Cache.SetValueExt<SOLine.salesAcctID>(sOLineRow, acctID);

                if (docLine.SubID != null)
                {
                    try
                    {
                        Base.Transactions.Cache.SetValueExt<SOLine.salesSubID>(sOLineRow, docLine.SubID);
                    }
                    catch (PXException)
                    {
                        sOLineRow.SalesSubID = null;
                    }
                }
                else
                {
                    InvoicingFunctions.SetCombinedSubID(
                                        Base,
                                        Base.Transactions.Cache,
                                        null,
                                        null,
                                        sOLineRow,
                                        fsSetupRow,
                                        sOLineRow.BranchID,
                                        sOLineRow.InventoryID,
                                        fsServiceContractRow.BillLocationID,
                                        fsServiceContractRow.BranchLocationID);
                }

                Base.Transactions.SetValueExt<SOLine.orderQty>(sOLineRow, docLine.Qty);

                Base.Transactions.SetValueExt<SOLine.curyUnitPrice>(sOLineRow, docLine.CuryUnitPrice);

                Base.Transactions.SetValueExt<SOLine.tranDesc>(sOLineRow, docLine.TranDescPrefix + sOLineRow.TranDesc);

                sOLineRow = Base.Transactions.Update(sOLineRow);

                Base.Transactions.Cache.SetValueExt<SOLine.commissionable>(sOLineRow, docLine.Commissionable ?? false);

                Base.Transactions.SetValueExt<SOLine.manualDisc>(sOLineRow, docLine.IsBillable == false);

                if (docLine.IsBillable == false)
                {
                    Base.Transactions.SetValueExt<SOLine.isFree>(sOLineRow, true);
                }

                //Set the line as a posted
                fSxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(sOLineRow);

                fSxSOLineRow.SDPosted = true;

                fSxSOLineRow.Source = ID.DocumentSource.INVOICE_FROM_SERVICECONTRACT;
                fSxSOLineRow.ServiceContractID = fsServiceContractRow.ServiceContractID;
                fSxSOLineRow.ContractPeriodID = fsContractPeriodRow.ContractPeriodID;
                fSxSOLineRow.BillCustomerID = fsServiceContractRow.BillCustomerID;
                fSxSOLineRow.CustomerLocationID = fsServiceContractRow.BillLocationID;

                if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
                {
                    if (docLine.EquipmentAction != null)
                    {
                        Base.Transactions.SetValueExt<FSxSOLine.equipmentAction>(sOLineRow, docLine.EquipmentAction);
                        Base.Transactions.SetValueExt<FSxSOLine.sMEquipmentID>(sOLineRow, docLine.SMEquipmentID);
                        Base.Transactions.SetValueExt<FSxSOLine.equipmentLineRef>(sOLineRow, docLine.EquipmentLineRef);

                        if (docLine.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT
                            || ((docLine.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.NONE)
                                    && string.IsNullOrEmpty(docLine.NewTargetEquipmentLineNbr) == false))
                        {
                            componentList.Add(new SharedClasses.SOARLineEquipmentComponent(docLine, sOLineRow, fSxSOLineRow));
                        }
                        else
                        {
                            fSxSOLineRow.ComponentID = docLine.ComponentID;
                        }
                    }
                }

                sOLineRow = Base.Transactions.Update(sOLineRow);
            }

            if (componentList.Count > 0)
            {
                //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                foreach (SharedClasses.SOARLineEquipmentComponent currLineModel in componentList.Where(x => x.equipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT))
                {
                    foreach (SharedClasses.SOARLineEquipmentComponent currLineComponent in componentList.Where(x => (x.equipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.NONE)))
                    {
                        if (currLineComponent.sourceNewTargetEquipmentLineNbr == currLineModel.sourceLineRef)
                        {
                            currLineComponent.fsxSOLineRow.ComponentID = currLineComponent.componentID;
                            currLineComponent.fsxSOLineRow.NewTargetEquipmentLineNbr = currLineModel.currentLineRef;
                        }
                    }
                }
            }

            if (Base.soordertype.Current.RequireControlTotal == true)
            {
                Base.Document.Cache.SetValueExt<SOOrder.curyControlTotal>(sOOrderRow, sOOrderRow.CuryOrderTotal);
            }

            Base.Document.Cache.SetValueExt<SOOrder.hold>(sOOrderRow, false);

            sOOrderRow = Base.Document.Update(sOOrderRow);

            sOOrderRow.Status = SOOrderStatus.Open;

            Exception newException = null;

            try
            {
            Base.Save.Press();
            }
            catch (Exception e)
            {
                List<ErrorInfo> errorList = this.GetErrorInfo();

                var exceptionWithContextMessage = ExceptionHelper.GetExceptionWithContextMessage(
                                    PXMessages.Localize(TX.Messages.ERROR_CREATING_INVOICE_IN_POSTING_BATCH),
                                    e);

                newException = InvoicingFunctions.GetErrorInfoInLines(errorList, exceptionWithContextMessage);
            }

            if (newException != null)
            {
                throw newException;
            }

            FSContractPostDoc fsContractCreatedDocRow = new FSContractPostDoc()
            {
                ContractPeriodID = fsContractPeriodRow.ContractPeriodID,
                ContractPostBatchID = fsContractPostBatchRow.ContractPostBatchID,
                PostDocType = sOOrderRow.OrderType,
                PostedTO = ID.Batch_PostTo.SO,
                PostRefNbr = sOOrderRow.RefNbr,
                ServiceContractID = fsServiceContractRow.ServiceContractID
            };

            return fsContractCreatedDocRow;
        }
        #endregion
        #endregion

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}