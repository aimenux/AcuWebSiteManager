using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections;

namespace PX.Objects.FS
{
    public class SM_OpportunityMaint : PXGraphExtension<CR.OpportunityMaint.Discount, OpportunityMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public override void Initialize()
        {
            base.Initialize();
            InqueriesMenu.AddMenuAction(ViewServiceOrder);
        }

        [PXHidden]
        public PXSelect<FSSetup> SetupRecord;

        [PXHidden]
        public PXSelect<CRSetup> CRSetupRecord;

        public PXSetup<FSSrvOrdType>.Where<
                Where<
                    FSSrvOrdType.srvOrdType, Equal<Optional<FSxCROpportunity.srvOrdType>>>> ServiceOrderTypeSelected;

        [PXCopyPasteHiddenView]
        public PXFilter<FSCreateServiceOrderFilter> CreateServiceOrderFilter;

        #region CacheAttached
        [PXDBCurrency(typeof(CROpportunityProducts.curyInfoID), typeof(CROpportunityProducts.extPrice))]
        [PXUIField(DisplayName = "Ext. Price")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        Current<CROpportunityProducts.stockItemType>, Equal<INItemTypes.serviceItem>,
                                        And<
                                            FSxCROpportunityProducts.billingRule, Equal<FSxCROpportunityProducts.billingRule.None>>>,
                                    SharedClasses.decimal_0>,
                                Mult<CROpportunityProducts.curyUnitPrice, CROpportunityProducts.quantity>>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual void CROpportunityProducts_CuryExtPrice_CacheAttached(PXCache sender) { }
        #region CROpportunityProducts_UnitCost
        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
                                                        Search2<INItemSite.tranUnitCost,
                                                            InnerJoin<InventoryItem,
                                                                    On<InventoryItem.inventoryID, Equal<INItemSite.inventoryID>>>,
                                                            Where<INItemSite.inventoryID, Equal<Current<CROpportunityProducts.inventoryID>>,
                                                                And<INItemSite.siteID, Equal<Current<CROpportunityProducts.siteID>>,
                                                                    And<InventoryItem.stkItem, Equal<True>>>>>,
                                                        Search<InventoryItem.stdCost,
                                                            Where<InventoryItem.inventoryID, Equal<Current<CROpportunityProducts.inventoryID>>,
                                                                And<InventoryItem.stkItem, Equal<False>>>>>))]
        public virtual void CROpportunityProducts_UnitCost_CacheAttached(PXCache sender) { }
        #endregion
        #region CROpportunityProducts_CuryUnitCost
        [PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(CROpportunityProducts.curyInfoID), typeof(CROpportunityProducts.unitCost))]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<CROpportunityProducts.pOCreate>))]
        public virtual void CROpportunityProducts_CuryUnitCost_CacheAttached(PXCache sender) { }
        #endregion
        #endregion

        #region Methods

        private FSSetup GetFSSetup()
        {
            if (SetupRecord.Current == null)
            {
                return SetupRecord.Select();
            }
            else
            {
                return SetupRecord.Current;
            }
        }

        private CRSetup GetCRSetup()
        {
            if (CRSetupRecord.Current == null)
            {
                return CRSetupRecord.Select();
            }
            else
            {
                return CRSetupRecord.Current;
            }
        }

        private void EnableDisableCustomFields(
            PXCache cache,
            CROpportunity crOpportunityRow,
            FSxCROpportunity fsxCROpportunityRow,
            FSServiceOrder fsServiceOrderRow,
            FSSrvOrdType fsSrvOrdTypeRow)
        {
            bool isSMSetup = GetFSSetup() != null;

            PXUIFieldAttribute.SetEnabled<FSxCROpportunity.sDEnabled>(cache, null, isSMSetup && fsServiceOrderRow == null);
            PXUIFieldAttribute.SetEnabled<FSxCROpportunity.srvOrdType>(cache, null, isSMSetup && fsxCROpportunityRow.SDEnabled == true && fsServiceOrderRow == null);
            PXUIFieldAttribute.SetEnabled<FSxCROpportunity.branchLocationID>(cache, null, isSMSetup && fsxCROpportunityRow.SDEnabled == true);

            if (fsSrvOrdTypeRow != null)
            {
                bool isAnInternalSrvOrdType = fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT;

                PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(cache, null, isAnInternalSrvOrdType || (!isAnInternalSrvOrdType && fsServiceOrderRow == null));
            }
            else
            {
                PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(cache, null, true);
            }
        }

        private void EnableDisableActions(
            PXCache cache,
            CROpportunity crOpportunityRow,
            FSxCROpportunity fsxCROpportunityRow,
            FSServiceOrder fsServiceOrderRow,
            FSSrvOrdType fsSrvOrdTypeRow)
        {
            bool isSMSetup = GetFSSetup() != null;
            bool insertedStatus = Base.Opportunity.Cache.GetStatus(Base.Opportunity.Current) == PXEntryStatus.Inserted;

            CreateServiceOrder.SetEnabled(isSMSetup && crOpportunityRow != null && insertedStatus == false && fsServiceOrderRow == null);
            ViewServiceOrder.SetEnabled(isSMSetup && crOpportunityRow != null && crOpportunityRow.OpportunityID != null && fsServiceOrderRow != null);
            OpenAppointmentBoard.SetEnabled(fsServiceOrderRow != null);

			if (fsServiceOrderRow != null)
			{
				Base.createSalesOrder.SetEnabled(false);
				Base.createInvoice.SetEnabled(false);
			}
        }

        private void SetPersistingChecks(
            PXCache cache,
            CROpportunity crOpportunityRow,
            FSxCROpportunity fsxCROpportunityRow,
            FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (fsxCROpportunityRow.SDEnabled == true)
            {
                PXDefaultAttribute.SetPersistingCheck<FSxCROpportunity.srvOrdType>(cache, crOpportunityRow, PXPersistingCheck.NullOrBlank);
                PXDefaultAttribute.SetPersistingCheck<FSxCROpportunity.branchLocationID>(cache, crOpportunityRow, PXPersistingCheck.NullOrBlank);

                if (fsSrvOrdTypeRow == null)
                {
                    fsSrvOrdTypeRow = CRExtensionHelper.GetServiceOrderType(Base, fsxCROpportunityRow.SrvOrdType);
                }

                if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
                {
                    PXDefaultAttribute.SetPersistingCheck<CROpportunity.bAccountID>(cache, crOpportunityRow, PXPersistingCheck.NullOrBlank);
                    PXDefaultAttribute.SetPersistingCheck<CROpportunity.locationID>(cache, crOpportunityRow, PXPersistingCheck.NullOrBlank);
                }
            }
            else
            {
                PXDefaultAttribute.SetPersistingCheck<FSxCROpportunity.srvOrdType>(cache, crOpportunityRow, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSxCROpportunity.branchLocationID>(cache, crOpportunityRow, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<CROpportunity.bAccountID>(cache, crOpportunityRow, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<CROpportunity.locationID>(cache, crOpportunityRow, PXPersistingCheck.Nothing);
            }
        }

        private void SetBranchLocationID(PXGraph graph, CROpportunity crOpportunityRow, FSxCROpportunity fsxCROpportunityRow)
        {
            if (crOpportunityRow.BranchID != null)
            {
                UserPreferences userPreferencesRow =
                    PXSelect<UserPreferences,
                    Where<
                        UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>.Select(graph);

                if (userPreferencesRow != null
                        && userPreferencesRow.DefBranchID == crOpportunityRow.BranchID)
                {
                    FSxUserPreferences fsxUserPreferencesRow = PXCache<UserPreferences>.GetExtension<FSxUserPreferences>(userPreferencesRow);

                    if (fsxUserPreferencesRow != null)
                    {
                        fsxCROpportunityRow.BranchLocationID = fsxUserPreferencesRow.DfltBranchLocationID;
                    }
                }
                else
                {
                    fsxCROpportunityRow.BranchLocationID = null;
                }
            }
            else
            {
                fsxCROpportunityRow.BranchLocationID = null;
            }
        }

        private void CreateServiceOrderDocument(OpportunityMaint graphOpportunityMaint, CROpportunity crOpportunityRow, FSCreateServiceOrderFilter fsCreateServiceOrderFilterRow)
        {
            if(graphOpportunityMaint == null
                    ||  crOpportunityRow == null
                        || fsCreateServiceOrderFilterRow == null)
            {
                return;
            }

            ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();

            FSServiceOrder newServiceOrderRow = CRExtensionHelper.InitNewServiceOrder(fsCreateServiceOrderFilterRow.SrvOrdType, ID.SourceType_ServiceOrder.OPPORTUNITY);

            CRSetup crSetupRow = GetCRSetup();

            graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords.Insert(newServiceOrderRow);

            CRExtensionHelper.UpdateServiceOrderHeader(
                                                        graphServiceOrderEntry,
                                                        Base.Opportunity.Cache,
                                                        crOpportunityRow,
                                                        fsCreateServiceOrderFilterRow,
                                                        graphServiceOrderEntry.ServiceOrderRecords.Current,
                                                        Base.Opportunity_Contact.Current,
                                                        Base.Opportunity_Address.Current,
                                                        true);

            SharedFunctions.CopyNotesAndFiles(Base.Opportunity.Cache,
                                            graphServiceOrderEntry.ServiceOrderRecords.Cache,
                                            crOpportunityRow, graphServiceOrderEntry.ServiceOrderRecords.Current,
                                            crSetupRow.CopyNotes,
                                            crSetupRow.CopyFiles);

            foreach (CROpportunityProducts crOpportunityProductsRow in graphOpportunityMaint.Products.Select())
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(graphServiceOrderEntry, crOpportunityProductsRow.InventoryID);

                if (inventoryItemRow.StkItem == true
                        && graphServiceOrderEntry.ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE)
                {
                    throw new PXException(TX.Error.STOCKITEM_NOT_HANDLED_BY_SRVORDTYPE, inventoryItemRow.InventoryCD);
                }

                FSxCROpportunityProducts fsxCROpportunityProductsRow = graphOpportunityMaint.Products.Cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);
                CRExtensionHelper.InsertFSSODetFromOpportunity(graphServiceOrderEntry, graphOpportunityMaint.Products.Cache, crSetupRow, crOpportunityProductsRow, fsxCROpportunityProductsRow, inventoryItemRow);
            }

            graphServiceOrderEntry.ServiceOrderRecords.Current.SourceRefNbr = crOpportunityRow.OpportunityID;

            if (!Base.IsContractBasedAPI)
            {
                throw new PXRedirectRequiredException(graphServiceOrderEntry, null);
            }
        }

        private void HideOrShowFieldsActionsByInventoryFeature()
        {
            bool isInventoryFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.inventory>();

            PXUIVisibility visibility = isInventoryFeatureInstalled ? PXUIVisibility.Visible : PXUIVisibility.Invisible;

            PXUIFieldAttribute.SetVisibility<CROpportunityProducts.pOCreate>(Base.Products.Cache, null, visibility);
            PXUIFieldAttribute.SetVisibility<CROpportunityProducts.vendorID>(Base.Products.Cache, null, visibility);
            PXUIFieldAttribute.SetVisibility<CROpportunityProducts.curyUnitCost>(Base.Products.Cache, null, visibility);
            PXUIFieldAttribute.SetVisibility<FSxCROpportunityProducts.vendorLocationID>(Base.Products.Cache, null, visibility);

            PXUIFieldAttribute.SetVisible<CROpportunityProducts.pOCreate>(Base.Products.Cache, null, isInventoryFeatureInstalled);
            PXUIFieldAttribute.SetVisible<CROpportunityProducts.vendorID>(Base.Products.Cache, null, isInventoryFeatureInstalled);
            PXUIFieldAttribute.SetVisible<CROpportunityProducts.curyUnitCost>(Base.Products.Cache, null, isInventoryFeatureInstalled);
            PXUIFieldAttribute.SetVisible<FSxCROpportunityProducts.vendorLocationID>(Base.Products.Cache, null, isInventoryFeatureInstalled);
        }
        #endregion

        #region Actions
        public PXMenuInquiry<CROpportunity> InqueriesMenu;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.InquiriesFolder)]
        [PXUIField(DisplayName = "Inquiries")]
        public virtual IEnumerable inqueriesMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<CROpportunity> CreateServiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "Create Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void createServiceOrder()
        {
            CROpportunity crOpportunityRow = Base.Opportunity.Current;
            FSxCROpportunity fsxCROpportunityRow = Base.Opportunity.Cache.GetExtension<FSxCROpportunity>(crOpportunityRow);

            if (CreateServiceOrderFilter.AskExt() == WebDialogResult.OK)
            {
                fsxCROpportunityRow.SDEnabled = true;
                fsxCROpportunityRow.BranchLocationID = CreateServiceOrderFilter.Current.BranchLocationID;
                fsxCROpportunityRow.SrvOrdType = CreateServiceOrderFilter.Current.SrvOrdType;

                PXLongOperation.StartOperation(Base, delegate ()
                {
                    CreateServiceOrderDocument(Base, crOpportunityRow, CreateServiceOrderFilter.Current);
                });
            }
        }

        public PXAction<CROpportunity> ViewServiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "View Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void viewServiceOrder()
        {
            if (Base.Opportunity == null || Base.Opportunity.Current == null)
            {
                return;
            }

            if (Base.IsDirty)
            {
                Base.Save.Press();
            }

            FSxCROpportunity fsxCROpportunityRow = Base.Opportunity.Cache.GetExtension<FSxCROpportunity>(Base.Opportunity.Current);

            if (fsxCROpportunityRow != null && fsxCROpportunityRow.SOID != null)
            {
                CRExtensionHelper.LaunchServiceOrderScreen(Base, fsxCROpportunityRow.SOID);
            }
        }

        public PXAction<CROpportunity> OpenAppointmentBoard;
        [PXButton]
        [PXUIField(DisplayName = "Schedule on the Calendar Board", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openAppointmentBoard()
        {
            if (Base.Opportunity == null || Base.Opportunity.Current == null)
            {
                return;
            }

            if (Base.IsDirty)
            {
                Base.Save.Press();
            }

            FSxCROpportunity fsxCROpportunityRow = Base.Opportunity.Cache.GetExtension<FSxCROpportunity>(Base.Opportunity.Current);

            if (fsxCROpportunityRow != null && fsxCROpportunityRow.SOID != null)
            {
                CRExtensionHelper.LaunchEmployeeBoard(Base, fsxCROpportunityRow.SOID);
            }
        }

        #endregion

        #region Events
        #region CROpportunity
        protected void CROpportunity_BranchID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated upd)
        {
            upd?.Invoke(cache, e);

            if (e.Row == null)
            {
                return;
            }
            CROpportunity crOpportunityRow = (CROpportunity)e.Row;
            FSxCROpportunity fsxCROpportunityRow = cache.GetExtension<FSxCROpportunity>(crOpportunityRow);
            SetBranchLocationID(Base, crOpportunityRow, fsxCROpportunityRow);
        }

        protected void CROpportunity_SDEnabled_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunity crOpportunityRow = (CROpportunity)e.Row;
            FSxCROpportunity fsxCROpportunityRow = cache.GetExtension<FSxCROpportunity>(crOpportunityRow);

            if (fsxCROpportunityRow.SDEnabled == true)
            {
                FSSetup fsSetupRow = GetFSSetup();

                if (fsSetupRow != null
                        && fsSetupRow.DfltOpportunitySrvOrdType != null)
                {
                    fsxCROpportunityRow.SrvOrdType = fsSetupRow.DfltOpportunitySrvOrdType;
                }

                SetBranchLocationID(Base, crOpportunityRow, fsxCROpportunityRow);
                Base.Opportunity.SetValueExt<CROpportunity.allowOverrideContactAddress>(crOpportunityRow, true);
            }
            else
            {
                fsxCROpportunityRow.BranchLocationID = null;
            }
        }

        protected void CROpportunity_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected sel)
        {
            sel?.Invoke(cache, e);

            if (e.Row == null)
            {
                return;
            }

            CROpportunity crOpportunityRow = (CROpportunity)e.Row;
            FSxCROpportunity fsxCROpportunityRow = cache.GetExtension<FSxCROpportunity>(crOpportunityRow);

            FSServiceOrder fsServiceOrderRow = CRExtensionHelper.GetRelatedServiceOrder(Base, cache, crOpportunityRow, fsxCROpportunityRow.SOID);
            FSSrvOrdType fsSrvOrdTypeRow = null;

            if (fsServiceOrderRow != null)
            {
                fsSrvOrdTypeRow = CRExtensionHelper.GetServiceOrderType(Base, fsServiceOrderRow.SrvOrdType);
            }

            EnableDisableCustomFields(cache, crOpportunityRow, fsxCROpportunityRow, fsServiceOrderRow, fsSrvOrdTypeRow);
            EnableDisableActions(cache, crOpportunityRow, fsxCROpportunityRow, fsServiceOrderRow, fsSrvOrdTypeRow);
            SetPersistingChecks(cache, crOpportunityRow, fsxCROpportunityRow, fsSrvOrdTypeRow);

            HideOrShowFieldsActionsByInventoryFeature();
        }
        #endregion
        #region CROpportunityProducts
        protected virtual void CROpportunityProducts_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (fsxCROpportunityProductsRow != null && Base.IsImportFromExcel == false)
            {
                cache.SetDefaultExt<CROpportunityProducts.curyUnitCost>(e.Row);
            }
        }
        protected virtual void CROpportunityProducts_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (fsxCROpportunityProductsRow != null && Base.IsImportFromExcel == false)
            {
                cache.SetDefaultExt<CROpportunityProducts.curyUnitCost>(e.Row);
            }
        }
        protected virtual void CROpportunityProducts_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (fsxCROpportunityProductsRow != null)
            {
                if (Base.IsImportFromExcel == false)
                {
                cache.SetDefaultExt<CROpportunityProducts.curyUnitCost>(e.Row);
                }

                cache.SetDefaultExt<FSxCROpportunityProducts.billingRule>(e.Row);
            }
        }
        protected virtual void CROpportunityProducts_CuryUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (string.IsNullOrEmpty(crOpportunityProductsRow.UOM) == false && crOpportunityProductsRow.InventoryID != null && crOpportunityProductsRow.POCreate == true)
            {
                object unitcost;
                cache.RaiseFieldDefaulting<CROpportunityProducts.unitCost>(e.Row, out unitcost);

                if (unitcost != null && (decimal)unitcost != 0m)
                {
                    decimal newval = INUnitAttribute.ConvertToBase<CROpportunityProducts.inventoryID, CROpportunityProducts.uOM>(cache, crOpportunityProductsRow, (decimal)unitcost, INPrecision.NOROUND);

                    IPXCurrencyHelper currencyHelper = Base.FindImplementation<IPXCurrencyHelper>();

                    if (currencyHelper != null)
                    {
                        currencyHelper.CuryConvCury((decimal)unitcost, out newval);
                    }
                    else
                    {
                        CM.PXDBCurrencyAttribute.CuryConvCury(cache, crOpportunityProductsRow, newval, out newval, true);
                    }

                    e.NewValue = Math.Round(newval, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
                    e.Cancel = true;
                }
            }
        }

        protected virtual void CROpportunityProducts_BillingRule_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (crOpportunityProductsRow.InventoryID != null)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(Base, crOpportunityProductsRow.InventoryID);

                if (inventoryItemRow.ItemType == INItemTypes.ServiceItem)
                {
                    FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);
                    e.NewValue = fsxServiceRow?.BillingRule;
                    e.Cancel = true;
                }
                else
                {
                    e.NewValue = ID.BillingRule.FLAT_RATE;
                    e.Cancel = true;
                }
            }
        }
        protected virtual void CROpportunityProducts_VendorLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (crOpportunityProductsRow.POCreate == false || crOpportunityProductsRow.InventoryID == null)
            {
                e.Cancel = true;
            }
        }
        protected virtual void CROpportunityProducts_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected sel)
        {
            sel?.Invoke(cache, e);

            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (fsxCROpportunityProductsRow != null)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(Base, crOpportunityProductsRow.InventoryID);

                if (inventoryItemRow != null)
                {
                    PXUIFieldAttribute.SetEnabled<FSxCROpportunityProducts.billingRule>(cache, crOpportunityProductsRow, inventoryItemRow.ItemType == INItemTypes.ServiceItem);
                    PXUIFieldAttribute.SetEnabled<FSxCROpportunityProducts.estimatedDuration>(cache, crOpportunityProductsRow, inventoryItemRow.ItemType == INItemTypes.ServiceItem || inventoryItemRow.ItemType == INItemTypes.NonStockItem);
                    PXUIFieldAttribute.SetEnabled<CROpportunityProducts.curyUnitCost>(cache, crOpportunityProductsRow, crOpportunityProductsRow.POCreate == true);
                    PXUIFieldAttribute.SetEnabled<FSxCROpportunityProducts.vendorLocationID>(cache, crOpportunityProductsRow, crOpportunityProductsRow.POCreate == true);
                    PXUIFieldAttribute.SetEnabled<CROpportunityProducts.quantity>(cache, crOpportunityProductsRow, fsxCROpportunityProductsRow.BillingRule != ID.BillingRule.TIME);
                }
            }
        }
        protected virtual void CROpportunityProducts_EstimatedDuration_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (fsxCROpportunityProductsRow != null)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(Base, crOpportunityProductsRow.InventoryID);

                if (inventoryItemRow != null)
                {
                    FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

                    if (inventoryItemRow.ItemType == INItemTypes.ServiceItem
                            || inventoryItemRow.ItemType == INItemTypes.NonStockItem)
                    {

                        e.NewValue = fsxServiceRow?.EstimatedDuration;
                        e.Cancel = true;
                    }
                }
            }
        }
        protected virtual void CROpportunityProducts_EstimatedDuration_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (fsxCROpportunityProductsRow.EstimatedDuration != null && fsxCROpportunityProductsRow.BillingRule == ID.BillingRule.TIME)
            {
                cache.SetDefaultExt<CROpportunityProducts.quantity>(crOpportunityProductsRow);
            }
        }

        protected virtual void CROpportunityProducts_BillingRule_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            cache.SetDefaultExt<CROpportunityProducts.quantity>(e.Row);
        }

        protected virtual void CROpportunityProducts_Quantity_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;
            FSxCROpportunityProducts fsxCROpportunityProductsRow = cache.GetExtension<FSxCROpportunityProducts>(crOpportunityProductsRow);

            if (fsxCROpportunityProductsRow != null
                && fsxCROpportunityProductsRow.BillingRule == ID.BillingRule.TIME
                && fsxCROpportunityProductsRow.EstimatedDuration != null 
                && fsxCROpportunityProductsRow.EstimatedDuration > 0)
            {
                e.NewValue = fsxCROpportunityProductsRow.EstimatedDuration / 60m;
            }
            else
            {
                e.NewValue = 0m;
            }
        }
        #endregion
        #endregion
    }
}