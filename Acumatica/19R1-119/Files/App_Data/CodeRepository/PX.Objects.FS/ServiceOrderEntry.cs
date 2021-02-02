using Autofac;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.FS.ParallelProcessing;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;

namespace PX.Objects.FS
{
    public class ServiceOrderEntry : PXGraph<ServiceOrderEntry, FSServiceOrder>, IGraphWithInitialization
    {
        /// <summary>
        /// This allows to have access to the Appointment document that began the Save operation in ServiceOrderEntry.
        /// </summary>
        public AppointmentEntry GraphAppointmentEntryCaller = null;

        public bool ForceAppointmentCheckings = false;
        public bool DisableServiceOrderUnboundFieldCalc = false;
        public bool RecalculateExternalTaxesSync { get; set; }
        public virtual void RecalculateExternalTaxes()
        {
        }

        public void SkipTaxCalcAndSave()
        {
            var ServiceOrderExternalTax = this.GetExtension<ServiceOrderEntryExternalTax>();

            if (ServiceOrderExternalTax != null)
            {
                this.GetExtension<ServiceOrderEntryExternalTax>().PublicSkipTaxCalcAndSave();
            }
            else
            {
                Save.Press();
            }
        }

        public ServiceOrderEntry()
            : base()
        {
            FSSetup fsSetupRow = SetupRecord.Current;

            actionsMenu.AddMenuAction(completeOrder);
            actionsMenu.AddMenuAction(cancelOrder);
            actionsMenu.AddMenuAction(reopenOrder);
            actionsMenu.AddMenuAction(closeOrder);
            actionsMenu.AddMenuAction(uncloseOrder);
            actionsMenu.AddMenuAction(invoiceOrder);
            actionsMenu.AddMenuAction(convertToServiceOrder);
            actionsMenu.AddMenuAction(createPurchaseOrder);
            actionsMenu.AddMenuAction(allowInvoice);
            actionsMenu.AddMenuAction(openAppointmentScreen);
            actionsMenu.AddMenuAction(validateAddress);
            actionsMenu.AddMenuAction(openEmployeeBoard);
            actionsMenu.AddMenuAction(openUserCalendar);

            if (fsSetupRow != null && fsSetupRow.ManageRooms == true)
            {
                actionsMenu.AddMenuAction(openRoomBoard);
            }

            reportsMenu.AddMenuAction(printServiceOrder);
            reportsMenu.AddMenuAction(serviceOrderAppointmentsReport);
            reportsMenu.AddMenuAction(printServiceTimeActivityReport);

            PXUIFieldAttribute.SetDisplayName<FSxService.actionType>(InventoryItemHelper.Cache, "Pickup/Deliver Items");
        }

        #region External Tax Provider

        public virtual bool IsExternalTax(string taxZoneID)
        {
            return false;
        }

        public virtual FSServiceOrder CalculateExternalTax(FSServiceOrder fsServiceOrderRow)
        {
            return fsServiceOrderRow;
        }

        public void ClearTaxes(FSServiceOrder serviceOrderRow)
        {
            if(serviceOrderRow == null)
                return;

            if (IsExternalTax(serviceOrderRow.TaxZoneID))
            {
                foreach (PXResult<FSServiceOrderTaxTran, Tax> res in Taxes.View.SelectMultiBound(new object[] { serviceOrderRow }))
                {
                    FSServiceOrderTaxTran taxTran = (FSServiceOrderTaxTran)res;
                    Taxes.Delete(taxTran);
                }

                serviceOrderRow.CuryTaxTotal = 0;
                serviceOrderRow.CuryDocTotal = GetCuryDocTotal(serviceOrderRow.CuryBillableOrderTotal, serviceOrderRow.CuryDiscTot,
                                                0, 0);
            }
        }
        #endregion

        #region Overrides
        public override void Persist()
        {
            if (RecalculateExternalTaxesSync)
            {
                RecalculateExternalTaxes();
                this.SelectTimeStamp();
            }
                        
            base.Persist();

            if (!RecalculateExternalTaxesSync) //When the calling process is the 'UI' thread.
                RecalculateExternalTaxes();

            ServiceOrderDetServices.Cache.Clear();
            ServiceOrderDetServices.View.Clear();
            ServiceOrderDetServices.View.RequestRefresh();
        }
        #endregion

        #region Private Members

        private FSSOAttendee fsSOAttendee_Inserting;
        private bool replicateAttendees = false;
        private bool updateSOCstmAssigneeEmpID = false;
        public bool allowCustomerChange = false;
        protected bool updateContractPeriod = false;

        private FSSelectorHelper fsSelectorHelper;

        private FSSelectorHelper GetFsSelectorHelperInstance
        {
            get
            {
                if (this.fsSelectorHelper == null)
                {
                    this.fsSelectorHelper = new FSSelectorHelper();
                }

                return this.fsSelectorHelper;
            }
        }

        #endregion

        #region CacheAttached
        #region FSServiceOrder_CustomerID
        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSServiceOrder_CustomerID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region BAccount_AcctName
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Staff Member Name", Enabled = false)]
        protected virtual void BAccount_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PlanID
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [FSSODetPartSplitPlanID(typeof(FSServiceOrder.noteID), typeof(FSServiceOrder.hold), typeof(FSServiceOrder.orderDate))]
        protected virtual void FSSODetPartSplit_PlanID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PlanID
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [FSSODetServiceSplitPlanID(typeof(FSServiceOrder.noteID), typeof(FSServiceOrder.hold), typeof(FSServiceOrder.orderDate))]
        protected virtual void FSSODetServiceSplit_PlanID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSBillingCycle_BillingCycleCD
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXDefault]
        [PXUIField(DisplayName = "Billing Cycle", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(FSBillingCycle.billingCycleCD))]
        [NormalizeWhiteSpace]
        protected virtual void FSBillingCycle_BillingCycleCD_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ARPayment_CashAccountID
        [PXDBInt]
        [PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, Visible = false)]
        protected virtual void ARPayment_CashAccountID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSSODetService CacheAttached
        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSSODetService_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region FSSODetPart CacheAttached
        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSSODetPart_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Selects / Views
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        [PXHidden]
        public PXSelect<BAccount> BAccounts;

        [PXHidden]
        public PXSelect<BAccountSelectorBase> BAccountSelectorBaseView;

        [PXHidden]
        public PXSelect<Vendor> Vendors;

        [PXHidden]
        public PXSetup<FSSelectorHelper> Helper;

        [PXHidden]
        public PXSelect<InventoryItem> InventoryItemHelper;

        public ServiceOrderCore.ServiceOrderRecords_View ServiceOrderRecords;

        [PXHidden]
        public PXSelect<FSSrvOrdTypeAttributeList> AttributeListRecords;

        protected virtual IEnumerable attributeListRecords()
        {
            FSSrvOrdTypeAttributeList fsSrvOrdTypeAttributeRow = AttributeListRecords.Current;

            if (fsSrvOrdTypeAttributeRow == null)
            {
                fsSrvOrdTypeAttributeRow = new FSSrvOrdTypeAttributeList();
            }

            fsSrvOrdTypeAttributeRow.SOID = ServiceOrderRecords.Current.SOID;
            fsSrvOrdTypeAttributeRow.SrvOrdType = ServiceOrderRecords.Current.SrvOrdType;
            fsSrvOrdTypeAttributeRow.NoteID = ServiceOrderRecords.Current.NoteID;

            yield return fsSrvOrdTypeAttributeRow;
        }

        [PXViewName(CR.Messages.Answers)]
        public FSAttributeList<FSSrvOrdTypeAttributeList, FSServiceOrder> Answers;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_RELATED)]
        public ServiceOrderCore.CurrentServiceOrder_View CurrentServiceOrder;

        [PXHidden]
        public PXSetup<BAccount>.Where<
        Where<
            BAccount.bAccountID, Equal<Optional<FSServiceOrder.customerID>>>> BAccount;

        [PXHidden]
        public PXSetup<Customer>.Where<
        Where<
            Customer.bAccountID, Equal<Optional<FSServiceOrder.billCustomerID>>>> TaxCustomer;

        [PXHidden]
        public PXSetup<Location>.Where<
        Where<
            Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>, 
            And<Location.locationID, Equal<Optional<FSServiceOrder.billLocationID>>>>> TaxLocation;

        [PXHidden]
        public PXSetup<TaxZone>.Where<
        Where<TaxZone.taxZoneID, Equal<Current<FSServiceOrder.taxZoneID>>>> TaxZone;

        [PXViewName(TX.TableName.FSCONTACT)]
        public ServiceOrderCore.FSContact_View ServiceOrder_Contact;

        [PXViewName(TX.TableName.FSADDRESS)]
        public ServiceOrderCore.FSAddress_View ServiceOrder_Address;

        [PXHidden]
        public PXSelect<Contract,
        Where<
            Contract.contractID, Equal<Required<Contract.contractID>>>> ContractRelatedToProject;

        public PXSelect<FSWFStage,
        Where<
            FSWFStage.wFStageID, Equal<Required<FSWFStage.wFStageID>>>> WFStageRelated;


        [PXViewName(TX.FriendlyViewName.Common.SERVICEORDERTYPE_SELECTED)]
        public PXSetup<FSSrvOrdType>.Where<
                Where<
                    FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>> ServiceOrderTypeSelected;

        public PXSetup<FSBranchLocation>.Where<
                Where<
                    FSBranchLocation.branchLocationID, Equal<Current<FSServiceOrder.branchLocationID>>>> CurrentBranchLocation;

        public PXSetup<SOOrderType>.Where<
                Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>> AllocationSOOrderTypeSelected;

        [PXHidden]
        public PXSetup<Customer>.Where<
                Where<Customer.bAccountID, Equal<Optional<FSServiceOrder.billCustomerID>>>> BillCustomer;

        [PXHidden]
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<FSServiceOrder.curyInfoID>>>> currencyInfoView;


        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_DET_SERVICES)]
        public PXSelectJoin<FSSODetService,
                        LeftJoin<FSPostInfo,
                            On<FSPostInfo.postID, Equal<FSSODetService.postID>>>,
                        Where<
                            FSSODetService.sOID, Equal<Current<FSServiceOrder.sOID>>>> ServiceOrderDetServices;


        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_DET_PARTS)]
        public PXSelectJoin<FSSODetPart,
                        LeftJoin<FSPostInfo,
                            On<FSPostInfo.postID, Equal<FSSODetPart.postID>>>,
                        Where<
                            FSSODetPart.sOID, Equal<Current<FSServiceOrder.sOID>>>> ServiceOrderDetParts;

        [PXHidden]
        public PXSelect<FSSODetUNION, Where<FSSODetUNION.sOID, Equal<Current<FSServiceOrder.sOID>>>> TaxSourceLines;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_APPOINTMENTS)]
        [PXCopyPasteHiddenView]
        public ServiceOrderCore.ServiceOrderAppointments_View ServiceOrderAppointments;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_EMPLOYEES)]
        public ServiceOrderCore.ServiceOrderEmployees_View ServiceOrderEmployees;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_EQUIPMENT)]
        public ServiceOrderCore.ServiceOrderEquipment_View ServiceOrderEquipment;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_ATTENDEES)]
        public ServiceOrderCore.ServiceOrderAttendees_View ServiceOrderAttendees;

        public PXSelect<INSiteStatus> sitestatusview_dummy;
        public PXSelect<SiteStatus> sitestatusview;
        public PXSelect<INItemSite> initemsite;

        [PXCopyPasteHiddenView()]
        public PlanningHelper<FSSODetPartSplit> PartPlanning;

        [PXCopyPasteHiddenView()]
        public PlanningHelper<FSSODetServiceSplit> ServicePlanning;

        [PXCopyPasteHiddenView()]
        [PXFilterable]
        public PXSelect<FSSODetPartSplit, Where<FSSODetPartSplit.srvOrdType, Equal<Current<FSSODetPart.srvOrdType>>, And<FSSODetPartSplit.refNbr, Equal<Current<FSSODetPart.refNbr>>, And<FSSODetPartSplit.lineNbr, Equal<Current<FSSODetPart.lineNbr>>>>>> partSplits;

        [PXCopyPasteHiddenView()]
        [PXFilterable]
        public PXSelect<FSSODetServiceSplit, Where<FSSODetServiceSplit.srvOrdType, Equal<Current<FSSODetService.srvOrdType>>, And<FSSODetServiceSplit.refNbr, Equal<Current<FSSODetService.refNbr>>, And<FSSODetServiceSplit.lineNbr, Equal<Current<FSSODetService.lineNbr>>>>>> serviceSplits;

        public LSFSSODetServiceLine lsServiceSelect;

        public LSFSSODetPartLine lsPartSelect;

        public PXSelect<INTranSplit> intransplit;

        [PXFilterable]
        public PXFilter<SrvOrderTypeAux> ServiceOrderTypeSelector;

        public ServiceOrderCore.RelatedServiceOrders_View RelatedServiceOrders;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_POSTED_IN)]
        [PXCopyPasteHiddenView]
        public PXSelectJoinGroupBy<FSPostDet,
                InnerJoin<FSSODet,
                    On<FSSODet.postID, Equal<FSPostDet.postID>>,
                InnerJoin<FSPostBatch,
                    On<FSPostBatch.batchID, Equal<FSPostDet.batchID>>>>,
                Where<
                    FSSODet.sOID, Equal<Current<FSServiceOrder.sOID>>>,
                Aggregate<
                    GroupBy<FSPostDet.batchID,
                    GroupBy<FSPostDet.aRPosted,
                    GroupBy<FSPostDet.aPPosted,
                    GroupBy<FSPostDet.iNPosted,
                    GroupBy<FSPostDet.sOPosted,
                    GroupBy<FSPostDet.sOInvPosted>>>>>>>> ServiceOrderPostedIn;

        [PXCopyPasteHiddenView]
        public PXSelect<FSSchedule,
                Where<FSSchedule.scheduleID, Equal<Current<FSServiceOrder.scheduleID>>>> ScheduleRecord;

        [PXCopyPasteHiddenView]
        public PXSetup<FSBillingCycle,
                InnerJoin<FSCustomerBillingSetup,
                    On<FSBillingCycle.billingCycleID, Equal<FSCustomerBillingSetup.billingCycleID>>>,
                Where<
                    FSCustomerBillingSetup.cBID, Equal<Current<FSServiceOrder.cBID>>>> BillingCycleRelated;

        public PXSetup<FSServiceContract>.Where<
                    Where<
                        FSServiceContract.serviceContractID, Equal<Current<FSServiceOrder.billServiceContractID>>,
                        And<FSServiceContract.billCustomerID, Equal<Current<FSServiceOrder.billCustomerID>>>>> StandardContractRelated;

        public PXSetup<FSContractPeriod>.Where<
                    Where<
                        FSContractPeriod.contractPeriodID, Equal<Current<FSServiceOrder.billContractPeriodID>>,
                        And<FSContractPeriod.serviceContractID, Equal<Current<FSServiceOrder.billServiceContractID>>,
                        And<Current<FSBillingCycle.billingBy>, Equal<FSBillingCycle.billingBy.ServiceOrder>>>>> StandardContractPeriod;

        public PXSelect<FSContractPeriodDet,
                    Where<FSContractPeriodDet.contractPeriodID, Equal<Current<FSContractPeriod.contractPeriodID>>,
                        And<FSContractPeriodDet.serviceContractID, Equal<Current<FSContractPeriod.serviceContractID>>>>> StandardContractPeriodDetail;

        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }
        
        [PXCopyPasteHiddenView()]
        public PXSelectReadonly2<ARPayment,
                            InnerJoin<FSAdjust,
                                On<ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                                    And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
                            Where<FSAdjust.adjdOrderType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                And<FSAdjust.adjdOrderNbr, Equal<Current<FSServiceOrder.refNbr>>>>> Adjustments;
        #endregion

        void IGraphWithInitialization.Initialize()
        {
            if (_licenseLimits != null)
            {
                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSServiceOrder>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSSODet), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                                new PXDataFieldValue<FSSODet.srvOrdType>(PXDbType.Char, ((ServiceOrderEntry)graph).ServiceOrderRecords.Current?.SrvOrdType),
                                new PXDataFieldValue<FSSODet.refNbr>(((ServiceOrderEntry)graph).ServiceOrderRecords.Current?.RefNbr)
                        };
                    }));
            }
        }

        #region Tax Extension Views
        public PXSelect<FSServiceOrderTax,
                Where<
                    FSServiceOrderTax.entityID, Equal<Current<FSServiceOrder.sOID>>,
                    And<FSServiceOrderTax.lineNbr, Less<intMax>>>,
                OrderBy<
                    Asc<FSServiceOrderTax.taxID>>> TaxLines;

        [PXViewName(TX.Messages.ServiceOrderTax)]
        public PXSelectJoin<FSServiceOrderTaxTran,
                InnerJoin<Tax,
                    On<Tax.taxID, Equal<FSServiceOrderTaxTran.taxID>>>,
                Where<
                    FSServiceOrderTaxTran.entityID, Equal<Current<FSServiceOrder.sOID>>>,
                OrderBy<
                    Asc<FSServiceOrderTaxTran.lineNbr,
                    Asc<FSServiceOrderTaxTran.taxID>>>> Taxes;
        #endregion

        #region FSSODetUNION events and sync methods
        private FSSODetUNION CreateTaxRow(FSSODet baseRow)
        {
            var taxRow = new FSSODetUNION()
            {
                SrvOrdType = baseRow.SrvOrdType,
                RefNbr = baseRow.RefNbr,
                SOID = baseRow.SOID,
                LineNbr = baseRow.LineNbr,
                LineRef = baseRow.LineRef,
                SODetID = baseRow.SODetID,

                CuryInfoID = baseRow.CuryInfoID,
                TaxCategoryID = baseRow.TaxCategoryID,
                GroupDiscountRate = baseRow.GroupDiscountRate,
                DocumentDiscountRate = baseRow.DocumentDiscountRate,
                CuryEstimatedTranAmt = baseRow.CuryEstimatedTranAmt,
                CuryBillableTranAmt = baseRow.CuryBillableTranAmt,
            };

            return taxRow;
        }

        protected virtual void _(Events.RowPersisting<FSSODetUNION> e)
        {
            e.Cancel = true;
        }
        #endregion

        #region Workflow Stages tree
        public TreeWFStageHelper.TreeWFStageView TreeWFStages;

        protected virtual IEnumerable treeWFStages(
                [PXInt]
                int? wFStageID)
        {
            if (ServiceOrderRecords.Current == null)
            {
                return null;
            }

            return TreeWFStageHelper.treeWFStages(this, ServiceOrderRecords.Current.SrvOrdType, wFStageID);
        }
        #endregion

        #region Menus

        public PXMenuAction<FSServiceOrder> actionsMenu;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
        [PXUIField(DisplayName = "Actions")]
        public virtual IEnumerable ActionsMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXMenuAction<FSServiceOrder> reportsMenu;
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports")]
        public virtual IEnumerable ReportsMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        #endregion

        #region Non-Shared Actions
        #region OpenAppointmentScreen
        public PXAction<FSServiceOrder> openAppointmentScreen;
        [PXUIField(DisplayName = "Schedule Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual void OpenAppointmentScreen()
        {
            if (ServiceOrderRecords.Current.SOID > 0
                    && !SharedFunctions.isThisAProspect(this, ServiceOrderRecords.Current.CustomerID))
            {
                var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                try
                {
                    Save.Press();
                    FSAppointment fsAppointmentRow = new FSAppointment()
                    {
                        SrvOrdType = ServiceOrderRecords.Current.SrvOrdType
                    };

                    fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Insert(fsAppointmentRow);

                    graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.soRefNbr>(graphAppointmentEntry.AppointmentRecords.Current, ServiceOrderRecords.Current.RefNbr);

                    throw new PXRedirectRequiredException(graphAppointmentEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion
        #region OpenSource
        public PXAction<FSServiceOrder> openSource;
        [PXUIField(DisplayName = "Open source", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable OpenSource(PXAdapter adapter)
        {
            FSServiceOrder fsServiceOrderRow = ServiceOrderRecords.Current;
            if (fsServiceOrderRow != null)
            {
                switch (fsServiceOrderRow.SourceType)
                {
                    case ID.SourceType_ServiceOrder.CASE:
                        var graphCRCase = PXGraph.CreateInstance<CRCaseMaint>();
                        CRCase crCase = (CRCase)PXSelect<CRCase,
                                                Where<CRCase.caseID,
                                                    Equal<Required<CRCase.caseID>>>>
                                                .Select(graphCRCase, fsServiceOrderRow.SourceID);
                        if (crCase != null)
                        {
                            graphCRCase.Case.Current = crCase;
                            throw new PXRedirectRequiredException(graphCRCase, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }

                        break;

                    case ID.SourceType_ServiceOrder.OPPORTUNITY:
                        var graphOpportunityMaint = PXGraph.CreateInstance<OpportunityMaint>();
                        CROpportunity crOpportunityRow = (CROpportunity)
                            PXSelect<CROpportunity,
                            Where<
                                CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>
                            .Select(graphOpportunityMaint, fsServiceOrderRow.SourceRefNbr);

                        if (crOpportunityRow != null)
                        {
                            graphOpportunityMaint.Opportunity.Current = crOpportunityRow;
                            throw new PXRedirectRequiredException(graphOpportunityMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }

                        break;

                    case ID.SourceType_ServiceOrder.SALES_ORDER:

                        var graphSOOrder = PXGraph.CreateInstance<SOOrderEntry>();
                        SOOrder soOrder = (SOOrder)PXSelect<SOOrder,
                                                    Where<SOOrder.orderType,
                                                        Equal<Required<SOOrder.orderType>>,
                                                            And<SOOrder.orderNbr,
                                                                Equal<Required<SOOrder.orderNbr>>>>>
                                                    .Select(graphSOOrder, fsServiceOrderRow.SourceDocType, fsServiceOrderRow.SourceRefNbr);
                        if (soOrder != null)
                        {
                            graphSOOrder.Document.Current = soOrder;
                            throw new PXRedirectRequiredException(graphSOOrder, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }

                        break;

                    case ID.SourceType_ServiceOrder.QUOTE:

                        var graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

                        graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.SourceRefNbr, fsServiceOrderRow.SourceDocType);

                        throw new PXRedirectRequiredException(graphServiceOrder, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }

            return adapter.Get();
        }

        #endregion
        #region OpenBatch
        public PXAction<FSServiceOrder> openBatch;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable OpenBatch(PXAdapter adapter)
        {
            FSServiceOrder fsServiceOrderRow = ServiceOrderRecords.Current;
            if (fsServiceOrderRow != null)
            {
                FSPostDet fsPostDetRow = ServiceOrderPostedIn.Select();

                if (fsPostDetRow != null)
                {
                    var graphPostBatchEntry = PXGraph.CreateInstance<PostBatchMaint>();

                    graphPostBatchEntry.BatchRecords.Current = graphPostBatchEntry.BatchRecords
                                .Search<FSPostBatch.batchID>(fsPostDetRow.BatchID);

                    throw new PXRedirectRequiredException(graphPostBatchEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }

            return adapter.Get();
        }
        #endregion

        public PXAction<FSServiceOrder> openServiceOrderScreen;
        [PXUIField(DisplayName = "Open Service Order Screen", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenServiceOrderScreen()
        {
            if (RelatedServiceOrders.Current != null)
            {
                var graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

                graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                            .Search<FSServiceOrder.refNbr>(RelatedServiceOrders.Current.RefNbr, RelatedServiceOrders.Current.SrvOrdType);

                throw new PXRedirectRequiredException(graphServiceOrder, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }

        public PXAction<FSServiceOrder> createPurchaseOrder;
        [PXUIField(DisplayName = "Create Purchase Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = "DISTINV")]
        [PXButton]
        public virtual void CreatePurchaseOrder()
        {
            FSServiceOrder fsServiceORderRow = ServiceOrderRecords.Current;

            if (fsServiceORderRow == null)
            {
                return;
            }

            POCreate graphPOCreate = PXGraph.CreateInstance<POCreate>();
            FSxPOCreateFilter fSxPOCreateFilterRow = graphPOCreate.Filter.Cache.GetExtension<FSxPOCreateFilter>(graphPOCreate.Filter.Current);
            fSxPOCreateFilterRow.SrvOrdType = fsServiceORderRow.SrvOrdType;
            fSxPOCreateFilterRow.ServiceOrderRefNbr = fsServiceORderRow.RefNbr;

            throw new PXRedirectRequiredException(graphPOCreate, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #region Actions

        #region CompleteOrder
        public PXAction<FSServiceOrder> completeOrder;
        [PXUIField(DisplayName = "Complete Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void CompleteOrder()
        {
            ServiceOrderCore.ChangeStatus_Handler(this, ServiceOrderRecords.Current, BillingCycleRelated.Current, ID.Status_ServiceOrder.COMPLETED);
        }
        #endregion
        #region CancelOrder
        public PXAction<FSServiceOrder> cancelOrder;
        [PXUIField(DisplayName = "Cancel Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void CancelOrder()
        {
            ServiceOrderCore.ChangeStatus_Handler(this, ServiceOrderRecords.Current, BillingCycleRelated.Current, ID.Status_ServiceOrder.CANCELED);
        }
        #endregion  
        #region CloseOrder
        public PXAction<FSServiceOrder> closeOrder;
        [PXUIField(DisplayName = "Close Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void CloseOrder()
        {
            using (var ts = new PXTransactionScope())
            {
                ServiceOrderCore.ChangeStatus_Handler(this, ServiceOrderRecords.Current, BillingCycleRelated.Current, ID.Status_ServiceOrder.CLOSED);

                DeallocateUnusedParts(ServiceOrderRecords.Current);

                ts.Complete();
            }
        }
        #endregion
        #region UncloseOrder
        public PXAction<FSServiceOrder> uncloseOrder;
        [PXUIField(DisplayName = "Unclose Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void UncloseOrder()
        {
            UncloseOrderWithOptions(true);
        }

        public virtual void UncloseOrderWithOptions(bool askConfirmation)
        {
            FSServiceOrder fsServiceOrderRow = ServiceOrderRecords.Current;

            if (fsServiceOrderRow != null)
            {
                if (askConfirmation == false || WebDialogResult.Yes == ServiceOrderRecords.Ask(
                                                            TX.WebDialogTitles.CONFIRM_SERVICE_ORDER_UNCLOSING,
                                                            TX.Messages.ASK_CONFIRM_SERVICE_ORDER_UNCLOSING,
                                                            MessageButtons.YesNo))
                {
                    fsServiceOrderRow.Status = ID.Status_ServiceOrder.COMPLETED;
                    ServiceOrderRecords.Update(fsServiceOrderRow);

                    this.Save.Press();
                }
            }
        }
        #endregion
        #region InvoiceOrder
        public PXAction<FSServiceOrder> invoiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "Generate Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public IEnumerable InvoiceOrder(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();
            List<ServiceOrderToPost> rows = new List<ServiceOrderToPost>();

            if (!adapter.MassProcess)
            {
                try
                {
                    RecalculateExternalTaxesSync = true;
                    Save.Press();
                }
                finally
                {
                    RecalculateExternalTaxesSync = false;
                }
            }

            foreach (FSServiceOrder fSServiceOrder in list)
            {
                if (fSServiceOrder.AllowInvoice == true)
                {
                    PXLongOperation.StartOperation(
                    this,
                    delegate ()
                    {
                        CreateInvoiceByServiceOrderPost graphCreateInvoiceByServiceOrderPost = PXGraph.CreateInstance<CreateInvoiceByServiceOrderPost>();
                        graphCreateInvoiceByServiceOrderPost.Filter.Current.PostTo = ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE ? ID.Batch_PostTo.AR_AP : ServiceOrderTypeSelected.Current.PostTo;
                        graphCreateInvoiceByServiceOrderPost.Filter.Current.IgnoreBillingCycles = true;
                        graphCreateInvoiceByServiceOrderPost.Filter.Current.BranchID = fSServiceOrder.BranchID;
                        graphCreateInvoiceByServiceOrderPost.Filter.Current.LoadData = true;
                        graphCreateInvoiceByServiceOrderPost.Filter.Current.IsGenerateInvoiceScreen = false;
                        if (fSServiceOrder.OrderDate > Accessinfo.BusinessDate)
                        {
                            graphCreateInvoiceByServiceOrderPost.Filter.Current.UpToDate = fSServiceOrder.OrderDate;
                            graphCreateInvoiceByServiceOrderPost.Filter.Current.InvoiceDate = fSServiceOrder.OrderDate;
                        }
                        graphCreateInvoiceByServiceOrderPost.Filter.Insert(graphCreateInvoiceByServiceOrderPost.Filter.Current);
                        ServiceOrderToPost serviceOrderToPostRow = graphCreateInvoiceByServiceOrderPost.PostLines.Current =
                                    graphCreateInvoiceByServiceOrderPost.PostLines.Search<ServiceOrderToPost.refNbr>(fSServiceOrder.RefNbr, fSServiceOrder.SrvOrdType);
                        rows = new List<ServiceOrderToPost>
                        {
                            serviceOrderToPostRow
                        };

                        var jobExecutor = new JobExecutor<InvoicingProcessStepGroupShared>(processorCount: 1);
                        Guid currentProcessID = CreateInvoiceByServiceOrderPost.CreateInvoices(graphCreateInvoiceByServiceOrderPost, rows, graphCreateInvoiceByServiceOrderPost.Filter.Current, null, jobExecutor, adapter.QuickProcessFlow);

                        if (graphCreateInvoiceByServiceOrderPost.Filter.Current.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE)
                        { 
                            foreach (PXResult<FSPostBatch> result in SharedFunctions.GetPostBachByProcessID(this, currentProcessID))
                            {
                                FSPostBatch fSPostBatchRow = (FSPostBatch)result;

                                CreateInvoiceByAppointmentPost.ApplyPrepayments(fSPostBatchRow);
                            }
                        }

                        if (!adapter.MassProcess)
                        {
                            using (new PXTimeStampScope(null))
                            {
                                ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();
                                graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords.Search<FSServiceOrder.refNbr>(fSServiceOrder.RefNbr, fSServiceOrder.SrvOrdType);
                                graphServiceOrderEntry.ServiceOrderPostedIn.Current = graphServiceOrderEntry.ServiceOrderPostedIn.Select();
                                
                                graphServiceOrderEntry.openPostingDocument();
                            }
                        }
                    });
                }
            }

            return list;
        }
        #endregion
        #region ReopenOrder
        public PXAction<FSServiceOrder> reopenOrder;
        [PXUIField(DisplayName = "Reopen Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ReopenOrder()
        {
            ServiceOrderCore.ChangeStatus_Handler(this, ServiceOrderRecords.Current, BillingCycleRelated.Current, ID.Status_ServiceOrder.OPEN);
        }
        #endregion

        #region ViewDirectionOnMap
        public PXAction<FSServiceOrder> viewDirectionOnMap;
        [PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ViewDirectionOnMap()
        {
            var address = ServiceOrder_Address.SelectSingle();
            if (address != null)
            {
                CR.BAccountUtility.ViewOnMap<FSAddress, FSAddress.countryID>(address);
            }
        }
        #endregion

        #region Validate Address
        public PXAction<FSServiceOrder> validateAddress;
        [PXUIField(DisplayName = "Validate Address", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
        [PXButton]
        public virtual IEnumerable ValidateAddress(PXAdapter adapter)
        {
            foreach (FSServiceOrder current in adapter.Get<FSServiceOrder>())
            {
                if (current != null)
                {
                    FSAddress address = this.ServiceOrder_Address.Select();
                    if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
                    {
                        PXAddressValidator.Validate<FSAddress>(this, address, true);
                    }
                }
                yield return current;
            }
        }
        #endregion

        #region OpenEmployeeBoard
        public PXAction<FSServiceOrder> openEmployeeBoard;
        [PXUIField(DisplayName = TX.CalendarBoardAccess.MULTI_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenEmployeeBoard()
        {
            if (!SharedFunctions.isThisAProspect(this, ServiceOrderRecords.Current.CustomerID))
            {
                ServiceOrderCore.OpenEmployeeBoard_Handler(this, ServiceOrderRecords);
            }
        }
        #endregion
        #region OpenRoomBoard
        public PXAction<FSServiceOrder> openRoomBoard;
        [PXUIField(DisplayName = TX.CalendarBoardAccess.ROOM_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenRoomBoard()
        {
            throw new PXRedirectToBoardRequiredException(
                    Paths.ScreenPaths.MULTI_ROOM_DISPATCH,
                    ServiceOrderCore.GetServiceOrderUrlArguments(ServiceOrderRecords.Current));
        }
        #endregion
        #region OpenUserCalendar
        public PXAction<FSServiceOrder> openUserCalendar;
        [PXUIField(DisplayName = TX.CalendarBoardAccess.SINGLE_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenUserCalendar()
        {
            if (!SharedFunctions.isThisAProspect(this, ServiceOrderRecords.Current.CustomerID))
            {
                throw new PXRedirectToBoardRequiredException(
                    Paths.ScreenPaths.SINGLE_EMPLOYEE_DISPATCH,
                    ServiceOrderCore.GetServiceOrderUrlArguments(ServiceOrderRecords.Current));
            }
        }
        #endregion

        #region CreateNewCustomer
        public PXAction<FSServiceOrder> createNewCustomer;
        [PXUIField(DisplayName = "Create New Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void CreateNewCustomer()
        {
            var graph = PXGraph.CreateInstance<CustomerMaint>();
            Customer customer = new Customer();
            graph.CurrentCustomer.Insert(customer);
            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #region ConvertToServiceOrder
        public PXAction<FSServiceOrder> convertToServiceOrder;
        [PXUIField(DisplayName = "Copy to Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void ConvertToServiceOrder()
        {
            if (ServiceOrderTypeSelector.AskExt() == WebDialogResult.OK)
            {
                if (ServiceOrderTypeSelector.Current.SrvOrdType != null)
                {
                    ServiceOrderEntry newServiceOrderGraph = PXGraph.CreateInstance<ServiceOrderEntry>();
                    FSServiceOrder sourceServiceOrderRow = this.CurrentServiceOrder.Current;
                    FSServiceOrder newServiceOrderRow = ServiceOrderCore.CreateServiceOrderCleanCopy(sourceServiceOrderRow);

                    newServiceOrderRow.Quote = false;
                    newServiceOrderRow.SrvOrdType = ServiceOrderTypeSelector.Current.SrvOrdType;
                    newServiceOrderRow.SourceType = ID.SourceType_ServiceOrder.QUOTE;
                    newServiceOrderRow.SourceDocType = sourceServiceOrderRow.SrvOrdType;
                    newServiceOrderRow.SourceRefNbr = sourceServiceOrderRow.RefNbr;

                    // These fields depend on the Service Order Type and must be nulled
                    newServiceOrderRow.WFStageID = null;
                    newServiceOrderRow.ProblemID = null;

                    newServiceOrderRow = newServiceOrderGraph.ServiceOrderRecords.Insert(newServiceOrderRow);
                    newServiceOrderGraph.CurrentServiceOrder.Current = newServiceOrderRow;

                    PXCache newServiceOrderCache = newServiceOrderGraph.CurrentServiceOrder.Cache;

                    newServiceOrderCache.SetValueExt<FSServiceOrder.branchID>(newServiceOrderRow, sourceServiceOrderRow.BranchID);
                    newServiceOrderCache.SetValueExt<FSServiceOrder.branchLocationID>(newServiceOrderRow, sourceServiceOrderRow.BranchLocationID);
                    newServiceOrderCache.SetValueExt<FSServiceOrder.customerID>(newServiceOrderRow, sourceServiceOrderRow.CustomerID);
                    newServiceOrderCache.SetValueExt<FSServiceOrder.locationID>(newServiceOrderRow, sourceServiceOrderRow.LocationID);
                    newServiceOrderCache.SetValueExt<FSServiceOrder.contactID>(newServiceOrderRow, sourceServiceOrderRow.ContactID);

                    newServiceOrderCache.SetValueExt<FSServiceOrder.billCustomerID>(newServiceOrderRow, sourceServiceOrderRow.BillCustomerID);
                    newServiceOrderCache.SetValueExt<FSServiceOrder.billLocationID>(newServiceOrderRow, sourceServiceOrderRow.BillLocationID);

                    newServiceOrderCache.SetValueExt<FSServiceOrder.curyID>(newServiceOrderRow, sourceServiceOrderRow.CuryID);

                    foreach (FSSODetService sourceRow in this.ServiceOrderDetServices.Select())
                    {
                        var newRow = new FSSODetService();

                        newRow = AppointmentEntry.InsertServicePartLine<FSSODetService, FSSODetService>(
                                                                    newServiceOrderGraph.ServiceOrderDetServices.Cache,
                                                                    newRow,
                                                                    this.ServiceOrderDetServices.Cache,
                                                                    sourceRow,
                                                                    noteID: null,
                                                                    soDetID: null,
                                                                    copyTranDate: false,
                                                                    tranDate: sourceRow.TranDate,
                                                                    SetValuesAfterAssigningSODetID: true,
                                                                    copyingFromQuote: true);

                        PXNoteAttribute.CopyNoteAndFiles(
                                                        this.ServiceOrderDetServices.Cache,
                                                        sourceRow,
                                                        newServiceOrderGraph.ServiceOrderDetServices.Cache,
                                                        newRow,
                                                        copyNotes: true,
                                                        copyFiles: false);
                    }

                    foreach (FSSODetPart sourceRow in this.ServiceOrderDetParts.Select())
                    {
                        var newRow = new FSSODetPart();

                        newRow = AppointmentEntry.InsertServicePartLine<FSSODetPart, FSSODetPart>(
                                                                    newServiceOrderGraph.ServiceOrderDetParts.Cache,
                                                                    newRow,
                                                                    this.ServiceOrderDetParts.Cache,
                                                                    sourceRow,
                                                                    noteID: null,
                                                                    soDetID: null,
                                                                    copyTranDate: false,
                                                                    tranDate: sourceRow.TranDate,
                                                                    SetValuesAfterAssigningSODetID: true,
                                                                    copyingFromQuote: true);

                        PXNoteAttribute.CopyNoteAndFiles(
                                                        this.ServiceOrderDetParts.Cache,
                                                        sourceRow,
                                                        newServiceOrderGraph.ServiceOrderDetParts.Cache,
                                                        newRow,
                                                        copyNotes: true,
                                                        copyFiles: false);
                    }

                    throw new PXRedirectRequiredException(newServiceOrderGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
        }
        #endregion

        #region OpenPostingDocument
        public PXAction<FSServiceOrder> OpenPostingDocument;
        [PXButton]
        [PXUIField(DisplayName = "Open Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openPostingDocument()
        {
            FSPostDet fsPostDetRow = ServiceOrderPostedIn.Select();

            if (fsPostDetRow == null)
            {
                return;
            }

            if (fsPostDetRow.SOPosted == true)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
                    graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(fsPostDetRow.SOOrderNbr, fsPostDetRow.SOOrderType);
                    throw new PXRedirectRequiredException(graphSOOrderEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            else if (fsPostDetRow.ARPosted == true)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.ARRefNbr, fsPostDetRow.ARDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.SOInvPosted == true)
            {
                SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.SOInvRefNbr, fsPostDetRow.SOInvDocType);
                throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.APPosted == true)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(fsPostDetRow.APRefNbr, fsPostDetRow.APDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenInvoiceDocument
        public PXAction<FSServiceOrder> OpenInvoiceDocument;
        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openInvoiceDocument()
        {
            FSPostDet fsPostDetRow = ServiceOrderPostedIn.Select();

            if (fsPostDetRow == null || fsPostDetRow.InvoiceRefNbr == null)
            {
                return;
            }

            if (fsPostDetRow.SOPosted == true)
            {
                ARInvoice aRInvoiceRow = PXSelect<ARInvoice, 
                                                Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, 
                                                    And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>,
                                                    And<ARInvoice.released, Equal<False>>>>>
                                           .Select(this, fsPostDetRow.InvoiceDocType, fsPostDetRow.InvoiceRefNbr);

                if (aRInvoiceRow != null)
                {
                    SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                    graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<SOInvoice.refNbr>(fsPostDetRow.InvoiceRefNbr, fsPostDetRow.InvoiceDocType);
                    throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }

                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.InvoiceRefNbr, fsPostDetRow.InvoiceDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.SOInvPosted == true)
            {
                SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<SOInvoice.refNbr>(fsPostDetRow.SOInvRefNbr, fsPostDetRow.SOInvDocType);
                throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.APPosted == true)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(fsPostDetRow.APRefNbr, fsPostDetRow.APDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.ARPosted == true)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.ARRefNbr, fsPostDetRow.ARDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region Service Order Reports

        public PXAction<FSServiceOrder> printServiceOrder;
        [PXUIField(DisplayName = "Print Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void PrintServiceOrder()
        {
            if (this.IsDirty)
            {
                Save.Press();
            }

            if (ServiceOrderRecords.Current != null)
            {
                Dictionary<string, string> parameters = this.GetServiceOrderParameters(ServiceOrderRecords.Current, false);

                if (parameters.Count == 0)
                {
                    return;
                }

                throw new PXReportRequiredException(parameters, ID.ReportID.SERVICE_ORDER, PXBaseRedirectException.WindowMode.NewWindow, string.Empty);
            }
        }

        public PXAction<FSServiceOrder> printServiceTimeActivityReport;
        [PXUIField(DisplayName = "Print Service Time Activity", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void PrintServiceTimeActivityReport()
        {
            if (this.IsDirty)
            {
                Save.Press();
            }

            if (ServiceOrderRecords.Current != null)
            {
                Dictionary<string, string> parameters = this.GetServiceOrderParameters(ServiceOrderRecords.Current, true);

                if (parameters.Count == 0)
                {
                    return;
                }

                throw new PXReportRequiredException(parameters, ID.ReportID.SERVICE_TIME_ACTIVITY, PXBaseRedirectException.WindowMode.NewWindow, string.Empty);
            }
        }
        #endregion

        #region Appointments in Service Order Report
        public PXAction<FSServiceOrder> serviceOrderAppointmentsReport;
        [PXUIField(DisplayName = "Print Appointments in Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void ServiceOrderAppointmentsReport()
        {
            if (this.IsDirty)
            {
                Save.Press();
            }

            if (ServiceOrderRecords.Current != null)
            {
                Dictionary<string, string> parameters = this.GetServiceOrderParameters(ServiceOrderRecords.Current, false);

                if (parameters.Count == 0)
                {
                    return;
                }

                throw new PXReportRequiredException(parameters, ID.ReportID.APP_IN_SERVICE_ORDER, PXBaseRedirectException.WindowMode.NewWindow, string.Empty);
            }
        }
        #endregion

        #region AllowInvoice
        public PXAction<FSServiceOrder> allowInvoice;
        [PXUIField(DisplayName = "Allow Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void AllowInvoice()
        {
            if (ServiceOrderRecords.Cache.AllowUpdate == true)
            {
                Save.Press();
            }

            FSServiceOrder fsServiceOrderRow = ServiceOrderRecords.Current;

            if (fsServiceOrderRow != null)
            {
                ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.allowInvoice>(fsServiceOrderRow, true);
                ServiceOrderRecords.Update(ServiceOrderRecords.Current);
                this.Save.Press();
            }
        }
        #endregion
        #region ViewPayment
        public PXAction<FSServiceOrder> viewPayment;
        [PXUIField(DisplayName = "View Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ViewPayment()
        {
            if (ServiceOrderRecords.Current != null && Adjustments.Current != null)
            {
                ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();
                pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(Adjustments.Current.RefNbr, Adjustments.Current.DocType);

                throw new PXRedirectRequiredException(pe, true, "Payment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #region CreatePrepayment
        public PXAction<FSServiceOrder> createPrepayment;
        [PXUIField(DisplayName = "Create Prepayment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        protected virtual void CreatePrepayment()
        {
            if (ServiceOrderRecords.Current != null)
            {
                Save.Press();

                PXGraph target;

                ServiceOrderCore.CreatePrepayment(ServiceOrderRecords.Current, null, out target, ARPaymentType.Prepayment);

                throw new PXPopupRedirectException(target, "New Payment", true);
            }
        }
        #endregion

        #region OpenScheduleScreen
        public PXAction<FSServiceOrder> OpenScheduleScreen;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        protected virtual void openScheduleScreen()
        {
            if (ServiceOrderRecords.Current != null)
            {
                var graphServiceContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();

                graphServiceContractScheduleEntry.ContractScheduleRecords.Current = graphServiceContractScheduleEntry
                                                                                        .ContractScheduleRecords.Search<FSContractSchedule.scheduleID>(
                                                                                            ServiceOrderRecords.Current.ScheduleID,
                                                                                            ServiceOrderRecords.Current.ServiceContractID,
                                                                                            ServiceOrderRecords.Current.CustomerID);

                throw new PXRedirectRequiredException(graphServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #endregion

        #region Public Methods
        public virtual void DeallocateUnusedParts(FSServiceOrder serviceOrder)
        {
            if (serviceOrder.BillServiceContractID != null)
            {
                return;
            }

            PXGraph dummyGraph = new PXGraph();

            FSBillingCycle fsBillingCycleRow = PXSelectJoin<FSBillingCycle,
                                InnerJoin<FSCustomerBillingSetup,
                                    On<FSCustomerBillingSetup.billingCycleID, Equal<FSBillingCycle.billingCycleID>>>,
                                Where<
                                    FSCustomerBillingSetup.cBID, Equal<Required<FSCustomerBillingSetup.cBID>>>>
                                .Select(dummyGraph, serviceOrder.CBID);
            if (fsBillingCycleRow == null
                    || fsBillingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER)
            {
                return;
            }

            var resultSet = PXSelectJoin<FSSODet,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.srvOrdType, Equal<FSSODet.srvOrdType>,
                                    And<FSServiceOrder.refNbr, Equal<FSSODet.refNbr>>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSSODetSplit,
                                On<FSSODetSplit.srvOrdType, Equal<FSSODet.srvOrdType>,
                                    And<FSSODetSplit.refNbr, Equal<FSSODet.refNbr>,
                                    And<FSSODetSplit.lineNbr, Equal<FSSODet.lineNbr>>>>,
                            LeftJoin<INItemPlan,
                                On<INItemPlan.planID, Equal<FSSODetSplit.planID>>>>>>,
                        Where2<
                            Where<
                                FSSODet.lineType, Equal<FSSODet.lineType.Inventory_Item>,
                                Or<FSSODet.lineType, Equal<FSSODet.lineType.Service>,
                                Or<FSSODet.lineType, Equal<FSSODet.lineType.NonStockItem>>>>,
                            And<
                                FSSODet.isPrepaid, Equal<False>,
                                And<FSSODetSplit.completed, Equal<False>,
                                And<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>>>,
                        OrderBy<
                            Asc<FSServiceOrder.orderDate,
                            Asc<FSSODet.sOID,
                            Asc<FSSODet.sODetID>>>>>
                        .Select(dummyGraph, serviceOrder.SOID);

            var docLines = new List<DocLineExt>();

            foreach (PXResult<FSSODet, FSServiceOrder, FSSrvOrdType, FSSODetSplit, INItemPlan> row in resultSet)
            {
                FSSODet fsSODetRow = (FSSODet)row;

                FSSODetSplit fsSODetSplitRow = (FSSODetSplit)row;
                var newRow = row;

                if (fsSODetRow.LineType == ID.LineType_All.INVENTORY_ITEM)
                {
                    FSAppointmentDet splitQtyInUse = PXSelectJoinGroupBy<FSAppointmentDet,
                                                                InnerJoin<FSAppointment,
                                                                    On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
                                                                Where<
                                                                    FSAppointmentDet.lineType, Equal<FSAppointmentDet.lineType.Inventory_Item>,
                                                                    And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                                    And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                                                                    And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                                                    And<
                                                                        Where<
                                                                            FSAppointmentDet.lotSerialNbr, Equal<Required<FSAppointmentDet.lotSerialNbr>>,
                                                                            Or<
                                                                                Where<
                                                                                    FSAppointmentDet.lotSerialNbr, IsNull,
                                                                                    And<Required<FSAppointmentDet.lotSerialNbr>, IsNull>>>>>>>>>,
                                                                Aggregate<GroupBy<FSAppointmentDet.sODetID, Sum<FSAppointmentDet.baseBillableQty>>>>
                                                                .Select(dummyGraph, fsSODetRow.SODetID, fsSODetSplitRow.LotSerialNbr, fsSODetSplitRow.LotSerialNbr);
                    fsSODetRow = PXCache<FSSODet>.CreateCopy(fsSODetRow);

                    fsSODetRow.LotSerialNbr = fsSODetSplitRow.LotSerialNbr;
                    decimal? deductQty = splitQtyInUse == null ? 0m : splitQtyInUse.BaseBillableQty;

                    fsSODetRow.BaseBillableQty = fsSODetSplitRow.BaseQty - deductQty;
                    fsSODetRow.BillableQty = INUnitAttribute.ConvertFromBase(ServiceOrderDetParts.Cache, fsSODetRow.InventoryID, fsSODetRow.UOM, (decimal)fsSODetRow.BaseBillableQty, INPrecision.QUANTITY);

                    FSServiceOrder fsServiceOrderRow = (FSServiceOrder)row;
                    FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)row;
                    INItemPlan inItemPlanRow = (INItemPlan)row;

                    newRow = new PXResult<FSSODet, FSServiceOrder, FSSrvOrdType, FSSODetSplit, INItemPlan>(fsSODetRow, fsServiceOrderRow, fsSrvOrdTypeRow, fsSODetSplitRow, inItemPlanRow);
                }

                if (fsSODetRow.BaseBillableQty > 0)
                {
                    docLines.Add(new DocLineExt(newRow));
                }
            }

            CreateInvoiceBase<CreateInvoiceByServiceOrderPost, ServiceOrderToPost>.DeallocateServiceOrders(this, docLines, ProcessRepeatedLines: true);
        }
        #endregion

        #region Private Methods

        #region EnableDisable

        [Obsolete("EnableDisable_SODetLine is deprecated, please use the generic methods X_RowSelected and X_SetPersistingCheck instead.")]
        private void EnableDisable_SODetLine(PXGraph graph, PXCache cache, FSSODet fsSODetRow, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow)
        {
            // TODO:
            // Move all these SetEnabled and SetPersistingCheck calls to the new generic method X_RowSelected.
            // Verify if each field is handled by the generic method before moving it.
            // If the generic method already handles a field, check if the conditions to enable/disable
            // and PersistingCheck are the same.
            // DELETE this method when all fields are moved.

            bool enableLineType = true;
            bool enableInventoryID = true;
            bool requireInventoryID = true;
            bool enableBillingRule = false;
            bool enableIsBillable = false;
            bool enableEstimatedDuration = true;
            bool enableEstimatedQty = true;
            bool enableProjectTaskID = true;
            bool enableAcctID = false;
            bool enableSubID = false;
            bool enableTranDesc = true;
            bool requireTranDesc = true;
            bool showStandardContractColumns = false;

            if (fsSODetRow.IsPrepaid == true)
            {
                enableLineType = false;
                enableInventoryID = false;
                requireInventoryID = false;
                enableBillingRule = false;
                enableIsBillable = false;
                enableEstimatedQty = false;
                enableProjectTaskID = false;
                enableAcctID = false;
                enableSubID = false;
                enableTranDesc = false;
                requireTranDesc = false;
            }
            else
            {
                switch (fsSODetRow.LineType)
                {
                    case ID.LineType_ServiceTemplate.SERVICE:
                    case ID.LineType_ServiceTemplate.NONSTOCKITEM:
                        enableInventoryID = true && fsServiceOrderRow.AllowInvoice != true;
                        requireInventoryID = true;
                        enableBillingRule = true;
                        enableIsBillable = true;
                        enableEstimatedDuration = true;
                        enableEstimatedQty = true;
                        enableProjectTaskID = true;

                        break;

                    case ID.LineType_ServiceTemplate.INVENTORY_ITEM:
                        enableInventoryID = true;
                        requireInventoryID = true;
                        enableIsBillable = true;
                        enableEstimatedDuration = false;
                        enableEstimatedQty = true;
                        enableProjectTaskID = true;

                        break;

                    case ID.LineType_ServiceTemplate.INSTRUCTION_SERVICE:
                    case ID.LineType_ServiceTemplate.INSTRUCTION_PART:
                    case ID.LineType_ServiceTemplate.COMMENT_SERVICE:
                    case ID.LineType_ServiceTemplate.COMMENT_PART:
                        enableInventoryID = false;
                        requireInventoryID = false;
                        enableIsBillable = false;
                        enableEstimatedDuration = false;
                        enableEstimatedQty = false;
                        enableProjectTaskID = false;

                        break;
                }
            }

            // IF the line is already saved prevent the user to modify its LineType
            if (fsSODetRow.SODetID > 0)
            {
                enableLineType = false;
            }

            enableBillingRule = enableBillingRule && fsSODetRow.LineType == ID.LineType_ServiceTemplate.SERVICE && fsSODetRow.Mem_LastReferencedBy == null;
            enableInventoryID = enableInventoryID && fsSODetRow.Mem_LastReferencedBy == null;
            bool equipmentOrRouteModuleEnabled = PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>()
                                                        || PXAccess.FeatureInstalled<FeaturesSet.routeManagementModule>();
            showStandardContractColumns = fsServiceOrderRow.BillServiceContractID != null
                                            && equipmentOrRouteModuleEnabled;

            PXUIFieldAttribute.SetEnabled<FSSODet.lineType>(cache, fsSODetRow, enableLineType);

            PXUIFieldAttribute.SetEnabled<FSSODet.inventoryID>(cache, fsSODetRow, enableInventoryID);
            PXDefaultAttribute.SetPersistingCheck<FSSODet.inventoryID>(cache, fsSODetRow, requireInventoryID ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSSODet.billingRule>(cache, fsSODetRow, enableBillingRule);
            PXUIFieldAttribute.SetEnabled<FSSODet.isBillable>(cache, fsSODetRow, enableIsBillable);
            PXUIFieldAttribute.SetEnabled<FSSODet.estimatedDuration>(cache, fsSODetRow, enableEstimatedDuration);
            PXUIFieldAttribute.SetEnabled<FSSODet.estimatedQty>(cache, fsSODetRow, enableEstimatedQty);
            PXUIFieldAttribute.SetEnabled<FSSODet.projectTaskID>(cache, fsSODetRow, enableProjectTaskID);
            PXUIFieldAttribute.SetEnabled<FSSODet.acctID>(cache, fsSODetRow, enableAcctID);
            PXUIFieldAttribute.SetEnabled<FSSODet.subID>(cache, fsSODetRow, enableSubID);

            PXUIFieldAttribute.SetEnabled<FSSODet.tranDesc>(cache, fsSODetRow, enableTranDesc);
            PXDefaultAttribute.SetPersistingCheck<FSSODet.tranDesc>(cache, fsSODetRow, requireTranDesc ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetVisible<FSSODet.contractRelated>(cache, null, showStandardContractColumns);
            PXUIFieldAttribute.SetVisible<FSSODet.coveredQty>(cache, null, showStandardContractColumns);
            PXUIFieldAttribute.SetVisible<FSSODet.extraUsageQty>(cache, null, showStandardContractColumns);
            PXUIFieldAttribute.SetVisible<FSSODet.curyExtraUsageUnitPrice>(cache, null, showStandardContractColumns);

            PXUIFieldAttribute.SetVisibility<FSSODet.contractRelated>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetVisibility<FSSODet.coveredQty>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetVisibility<FSSODet.extraUsageQty>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetVisibility<FSSODet.curyExtraUsageUnitPrice>(cache, null, equipmentOrRouteModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

            if (fsSODetRow.IsPrepaid != true)
            {
                ServiceOrderCore.EnableDisable_Acct_Sub(cache, fsSODetRow, fsSrvOrdTypeRow, fsServiceOrderRow);
            }
        }

        private void EnableDisable_ActionButtons(FSServiceOrder fsServiceOrderRow, PXCache cache)
        {
            ServiceOrderCore.EnableDisable_ActionButtons(
                    this,
                    cache,
                    fsServiceOrderRow,
                    BAccount?.Current,
                    ServiceOrderTypeSelected.Current,
                    BillingCycleRelated.Current,
                    completeOrder,
                    cancelOrder,
                    reopenOrder,
                    closeOrder,
                    invoiceOrder,
                    uncloseOrder,
                    openAppointmentScreen,
                    openUserCalendar,
                    openEmployeeBoard,
                    openRoomBoard,
                    openServiceSelector,
                    openStaffSelectorFromServiceTab,
                    openStaffSelectorFromStaffTab,
                    viewDirectionOnMap,
                    validateAddress,
                    convertToServiceOrder,
                    createPurchaseOrder,
                    allowInvoice);
        }

        /// <summary>
        /// Check the ManageRooms value on Setup to check/hide the Rooms Values options.
        /// </summary>
        private void HideRooms(FSServiceOrder fsServiceOrderRow)
        {
            bool isRoomManagementActive = ServiceManagementSetup.IsRoomManagementActive(this, SetupRecord.Current);
            fsServiceOrderRow.Mem_ShowAttendees = ServiceManagementSetup.IsAttendeesManagementActive(this, SetupRecord.Current);

            PXUIFieldAttribute.SetVisible<FSServiceOrder.roomID>(this.CurrentServiceOrder.Cache, fsServiceOrderRow, isRoomManagementActive);
            openRoomBoard.SetVisible(isRoomManagementActive);
        }

        /// <summary>
        /// Enable/Disable the Tabs if the selected Service Order Type post to AR (Parts Tab) and if the customerID is null or not (Services and Parts).
        /// </summary>
        private void EnableDisableServiceDetTabs(FSServiceOrder fsServiceOrderRow)
        {
            bool enablePartsTab = fsServiceOrderRow.CustomerID != null && ServiceOrderTypeSelected.Current.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
            bool enableServicesTab = fsServiceOrderRow.CustomerID != null;

            ServiceOrderDetParts.Cache.AllowInsert = enablePartsTab;
            ServiceOrderDetParts.Cache.AllowUpdate = enablePartsTab;
            ServiceOrderDetParts.Cache.AllowDelete = enablePartsTab;
            ServiceOrderDetServices.Cache.AllowInsert = enableServicesTab;
            ServiceOrderDetServices.Cache.AllowUpdate = enableServicesTab;
            ServiceOrderDetServices.Cache.AllowDelete = enableServicesTab;
        }

        private void EnableDisable_StaffID(PXCache cache, FSSODetService fsSODetServiceRow)
        {
            bool enableStaffID = fsSODetServiceRow.EnableStaffID == true
                                      && fsSODetServiceRow.LineType == ID.LineType_All.SERVICE;

            PXUIFieldAttribute.SetEnabled<FSSODetService.staffID>(
                                                                    cache,
                                                                    fsSODetServiceRow,
                                                                    enableStaffID);
        }

        #endregion

        //This function is unused, we need to evalute if whe should re-use it or remove it 
        private bool isTheLineValid(PXCache cache, FSSODet fsSODetRow, PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            bool lineOK = true;

            if (fsSODetRow == null)
            {
                return lineOK;
            }

            if ((fsSODetRow.LineType == ID.LineType_ServiceTemplate.COMMENT_SERVICE
                        || fsSODetRow.LineType == ID.LineType_ServiceTemplate.INSTRUCTION_SERVICE
                        || fsSODetRow.LineType == ID.LineType_ServiceTemplate.COMMENT_PART
                        || fsSODetRow.LineType == ID.LineType_ServiceTemplate.INSTRUCTION_PART)
                    && fsSODetRow.InventoryID != null)
            {
                PXUIFieldAttribute.SetEnabled<FSSODet.inventoryID>(cache, fsSODetRow, true);
                cache.RaiseExceptionHandling<FSSODet.inventoryID>(fsSODetRow, null, new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                lineOK = false;
            }

            if ((fsSODetRow.LineType == ID.LineType_ServiceTemplate.SERVICE
                        || fsSODetRow.LineType == ID.LineType_ServiceTemplate.NONSTOCKITEM
                        || fsSODetRow.LineType == ID.LineType_ServiceTemplate.INVENTORY_ITEM)
                    && fsSODetRow.InventoryID == null)
            {
                PXUIFieldAttribute.SetEnabled<FSSODet.inventoryID>(cache, fsSODetRow, true);
                cache.RaiseExceptionHandling<FSSODet.inventoryID>(fsSODetRow, null, new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                lineOK = false;
            }

            if ((fsSODetRow.LineType == ID.LineType_ServiceTemplate.COMMENT_SERVICE
                    || fsSODetRow.LineType == ID.LineType_ServiceTemplate.INSTRUCTION_SERVICE
                    || fsSODetRow.LineType == ID.LineType_ServiceTemplate.COMMENT_PART
                    || fsSODetRow.LineType == ID.LineType_ServiceTemplate.INSTRUCTION_PART)
                        && string.IsNullOrEmpty(fsSODetRow.TranDesc))
            {
                cache.RaiseExceptionHandling<FSSODet.tranDesc>(fsSODetRow, null, new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                lineOK = false;
            }

            if ((fsSODetRow.LineType != ID.LineType_ServiceTemplate.SERVICE
                    && fsSODetRow.LineType != ID.LineType_ServiceTemplate.NONSTOCKITEM)
                        && fsSODetRow.EstimatedQty == null)
            {
                PXUIFieldAttribute.SetEnabled<FSSODet.estimatedQty>(cache, fsSODetRow, true);
                cache.RaiseExceptionHandling<FSSODet.estimatedQty>(fsSODetRow, null, new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                lineOK = false;
            }

            if (fsSODetRow.LineType == ID.LineType_ServiceTemplate.INVENTORY_ITEM
                && ServiceOrderTypeSelected.Current != null
                && ServiceOrderTypeSelected.Current.PostTo == ID.SourceType_ServiceOrder.SALES_ORDER
                && fsSODetRow.LastModifiedByScreenID != ID.ScreenID.GENERATE_SERVICE_CONTRACT_APPOINTMENT
                && fsSODetRow.SiteID == null)
            {
                cache.RaiseExceptionHandling<FSSODet.siteID>(fsSODetRow, null, new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                lineOK = false;
            }

            return lineOK;
        }

        private void ClearOpportunity(FSServiceOrder fsServiceOrderRow)
        {
            OpportunityMaint graphOpportunityMaint = PXGraph.CreateInstance<OpportunityMaint>();
            graphOpportunityMaint.Opportunity.Current = graphOpportunityMaint.Opportunity.Search<CROpportunity.opportunityID>(fsServiceOrderRow.SourceRefNbr);
            CROpportunity crOpportunityRow = graphOpportunityMaint.Opportunity.Current;

            if (crOpportunityRow != null)
            {
                ClearCRActivities(graphOpportunityMaint.Activities.Select());
                FSxCROpportunity fsxCROpportunityRow = graphOpportunityMaint.Opportunity.Cache.GetExtension<FSxCROpportunity>(crOpportunityRow);
                graphOpportunityMaint.Opportunity.Cache.SetValueExt<FSxCROpportunity.sDEnabled>(crOpportunityRow, false);
                fsxCROpportunityRow.SOID = null;
                graphOpportunityMaint.Opportunity.Update(crOpportunityRow);
                graphOpportunityMaint.Save.Press();
            }
        }

        private void ClearCase(FSServiceOrder fsServiceOrderRow)
        {
            CRCaseMaint graphCRCaseMaint = PXGraph.CreateInstance<CRCaseMaint>();
            graphCRCaseMaint.Case.Current = graphCRCaseMaint.Case.Search<CRCase.caseID>(fsServiceOrderRow.SourceID);
            CRCase crCaseRow = graphCRCaseMaint.Case.Current;

            if (crCaseRow != null)
            {
                ClearCRActivities(graphCRCaseMaint.Activities.Select());
                FSxCRCase fsxCRCaseRow = graphCRCaseMaint.Case.Cache.GetExtension<FSxCRCase>(crCaseRow);
                graphCRCaseMaint.Case.Cache.SetValueExt<FSxCRCase.sDEnabled>(crCaseRow, false);
                fsxCRCaseRow.SOID = null;
                graphCRCaseMaint.Case.Update(crCaseRow);
                graphCRCaseMaint.Save.Press();
            }
        }

        private void ClearCRActivities(PXResultset<CRPMTimeActivity> activities)
        {
            CRTaskMaint graphCRTaskMaint = PXGraph.CreateInstance<CRTaskMaint>();
            PMTimeActivity pmTimeActivityRow = null;
            FSxPMTimeActivity fsxPMTimeActivityRow = null;

            foreach (CRActivity crActivity in activities)
            {
                pmTimeActivityRow =
                    PXSelect<PMTimeActivity,
                    Where<
                        PMTimeActivity.refNoteID, Equal<Required<PMTimeActivity.refNoteID>>>>
                    .Select(graphCRTaskMaint, crActivity.NoteID);

                if (pmTimeActivityRow != null)
                {
                    fsxPMTimeActivityRow = PXCache<PMTimeActivity>.GetExtension<FSxPMTimeActivity>(pmTimeActivityRow);

                    if (fsxPMTimeActivityRow != null)
                    {
                        fsxPMTimeActivityRow.ServiceID = null;
                        graphCRTaskMaint.TimeActivity.Cache.Update(pmTimeActivityRow);
                    }
                }
            }

            if (graphCRTaskMaint.IsDirty)
            {
                graphCRTaskMaint.Save.Press();
            }
        }

        private void ClearSalesOrder(FSServiceOrder fsServiceOrderRow)
        {
            // SD-3875
            var graphSOOrder = PXGraph.CreateInstance<SOOrderEntry>();

            graphSOOrder.Document.Current = graphSOOrder.Document.Search<SOOrder.orderNbr>(fsServiceOrderRow.SourceRefNbr, fsServiceOrderRow.SourceDocType);

            if (graphSOOrder.Document.Current != null)
            {
                SOOrder soOrderRow_Copy = PXCache<SOOrder>.CreateCopy(graphSOOrder.Document.Current);
                FSxSOOrder fsxSOOrderRow = graphSOOrder.CurrentDocument.Cache.GetExtension<FSxSOOrder>(soOrderRow_Copy);
                SOLine soLineRow_Copy;
                FSxSOLine fsxSOLineRow;

                fsxSOOrderRow.SDEnabled = false;
                fsxSOOrderRow.SORefNbr = null;
                fsxSOOrderRow.SrvOrdType = null;

                graphSOOrder.CurrentDocument.Update(soOrderRow_Copy);

                foreach (SOLine sOLineRow in graphSOOrder.Transactions.Select())
                {
                    soLineRow_Copy = PXCache<SOLine>.CreateCopy(sOLineRow);
                    fsxSOLineRow = graphSOOrder.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow_Copy);

                    if (fsxSOLineRow.SDSelected != false)
                    {
                        fsxSOLineRow.SDSelected = false;
                        graphSOOrder.Transactions.Update(soLineRow_Copy);
                    }
                }

                graphSOOrder.Save.Press();
            }
        }

        private void ClearPrepayment(FSServiceOrder fsServiceOrderRow)
        {
            ARPaymentEntry graphARPaymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
            SM_ARPaymentEntry graphSM_ARPaymentEntry = graphARPaymentEntry.GetExtension<SM_ARPaymentEntry>();

            foreach (FSAdjust fsAdjustRow in PXSelect<FSAdjust, 
                                        Where<FSAdjust.adjdOrderType, Equal<Required<FSAdjust.adjdOrderType>>,
                                            And<FSAdjust.adjdOrderNbr, Equal<Required<FSAdjust.adjdOrderNbr>>>
                                        >>.Select(graphARPaymentEntry, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr))
            {
                graphARPaymentEntry.Document.Current = graphARPaymentEntry.Document.Search<ARPayment.refNbr>(fsAdjustRow.AdjgRefNbr, fsAdjustRow.AdjgDocType);

                if (graphARPaymentEntry.Document.Current != null)
                { 
                    graphSM_ARPaymentEntry.FSAdjustments.Delete(fsAdjustRow);
                    graphARPaymentEntry.Save.Press();
                }
            }
        }

        private void SetSODetServiceLastReferencedBy(FSSODet fsSODetRow)
        {
            FSAppointment fsAppointmentRow = PXSelectJoinOrderBy<FSAppointment,
                                             InnerJoin<FSAppointmentDet,
                                                On<
                                                    FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>,
                                                    And<FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                                    And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>>>>>,
                                             OrderBy<Desc<FSAppointmentDet.createdDateTime>>>
                                             .SelectWindowed(this, 0, 1, fsSODetRow.SOID, fsSODetRow.SODetID);

            if (fsAppointmentRow != null)
            {
                fsSODetRow.Mem_LastReferencedBy = fsAppointmentRow.RefNbr;
            }
            else
            {
                fsSODetRow.Mem_LastReferencedBy = null;
            }
        }

        /// <summary>
        /// Update the assigned Employee for the Service Order in Sales Order customization if conditions apply.
        /// </summary>
        /// <param name="fsServiceOrderRow">FSServiceOrder row.</param>
        private void UpdateAssignedEmpIDinSalesOrder(FSServiceOrder fsServiceOrderRow)
        {
            if (updateSOCstmAssigneeEmpID == true &&
                fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER)
            {
                SOOrder soOrderRow = PXSelect<SOOrder,
                                        Where<
                                            SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                                        And<
                                            SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                     .Select(this, fsServiceOrderRow.SourceDocType, fsServiceOrderRow.SourceRefNbr);

                if (soOrderRow == null)
                {
                    return;
                }

                SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
                graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(soOrderRow.OrderNbr, soOrderRow.OrderType);

                FSxSOOrder fsxSOOrderRow = graphSOOrderEntry.Document.Cache.GetExtension<FSxSOOrder>(graphSOOrderEntry.Document.Current);
                fsxSOOrderRow.AssignedEmpID = fsServiceOrderRow.AssignedEmpID;
                graphSOOrderEntry.Document.Cache.SetStatus(graphSOOrderEntry.Document.Current, PXEntryStatus.Updated);
                graphSOOrderEntry.Save.Press();
                updateSOCstmAssigneeEmpID = false;
            }
        }

        /// <summary>
        /// Check if the given Service Order detail line is related with any Sales Order details.
        /// </summary>
        /// <param name="fsServiceOrderRow">Service Order row.</param>
        /// <param name="fsSODetRow">Service Order detail line.</param>
        /// <returns>Returns true if the Service Order detail is related with at least one Sales Order detail.</returns>
        private bool IsThisLineRelatedToAsoLine(FSServiceOrder fsServiceOrderRow, FSSODet fsSODetRow)
        {
            if (fsSODetRow.IsPrepaid == true
                   && fsSODetRow.SourceLineNbr != null)
            {
                SOLine soLineRelated =
                    PXSelect<SOLine,
                    Where<
                        SOLine.orderType, Equal<Required<SOLine.orderType>>,
                        And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                        And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>
                .SelectWindowed(
                        this,
                        0,
                        1,
                        fsServiceOrderRow.SourceDocType,
                        fsServiceOrderRow.SourceRefNbr,
                        fsSODetRow.SourceLineNbr);

                return soLineRelated != null;
            }

            return false;
        }

        /// <summary>
        /// Hides or Shows Appointments, Staff, Resources Equipment, Related Service Orders, Attendees, Post info Tabs.
        /// </summary>
        /// <param name="fsServiceOrderRow">Service Order row.</param>
        private void HideOrShowTabs(FSServiceOrder fsServiceOrderRow)
        {
            bool isQuote = (bool)fsServiceOrderRow.Quote;

            this.ServiceOrderAppointments.AllowSelect = !isQuote;
            this.ServiceOrderEmployees.AllowSelect = !isQuote;
            this.ServiceOrderEquipment.AllowSelect = !isQuote;
            this.RelatedServiceOrders.AllowSelect = isQuote;

            this.ServiceOrderAttendees.AllowSelect = fsServiceOrderRow.Mem_ShowAttendees == true;

            bool isBillingBySO = BillingCycleRelated.Current != null && BillingCycleRelated.Current.BillingBy == ID.Billing_By.SERVICE_ORDER ? true : false;

            PXUIFieldAttribute.SetVisible<FSServiceOrder.allowInvoice>(ServiceOrderRecords.Cache, fsServiceOrderRow, isBillingBySO);
            PXUIFieldAttribute.SetVisible<FSServiceOrder.mem_Invoiced>(ServiceOrderRecords.Cache, fsServiceOrderRow, isBillingBySO);

            this.ServiceOrderPostedIn.AllowSelect = isBillingBySO;
        }

        private void SetDefaultVendorSettingsForItem(PXCache cache, ServiceOrderEntry serviceOrderEntry, FSSODetPart fsSODetPartRow)
        {
            if (fsSODetPartRow != null && fsSODetPartRow.EnablePO == true)
            {
                InventoryItem inventoryItemRow =
                    PXSelect<InventoryItem,
                    Where<
                        InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
                        And<InventoryItem.preferredVendorID, IsNotNull>>>
                .Select(this, fsSODetPartRow.InventoryID);

                if (inventoryItemRow != null)
                {
                    cache.SetValueExt<FSSODetPart.poVendorID>(fsSODetPartRow, inventoryItemRow.PreferredVendorID);
                    //fsSODetPartRow.POVendorID = inventoryItemRow.PreferredVendorID;
                    //cache.SetValueExt<FSSODetPart.poVendorLocationID>(fsSODetPartRow, inventoryItemRow.PreferredVendorLocationID);
                    //fsSODetPartRow.POVendorLocationID = inventoryItemRow.PreferredVendorLocationID;
                }
            }
        }

        private Dictionary<string, string> GetServiceOrderParameters(FSServiceOrder fsServiceOrderRow, bool isServiceTimeActivityReport = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (fsServiceOrderRow == null)
            {
                return parameters;
            }

            string srvOrdTypeFieldName = SharedFunctions.GetFieldName<FSServiceOrder.srvOrdType>();
            string refNbrFieldName = SharedFunctions.GetFieldName<FSServiceOrder.refNbr>();

            parameters[srvOrdTypeFieldName] = fsServiceOrderRow.SrvOrdType;
            parameters[refNbrFieldName] = fsServiceOrderRow.RefNbr;

            if (isServiceTimeActivityReport)
            {
                //This parameters are for the Report ServiceTimeActivity
                string SORefNbrFieldName = SharedFunctions.GetFieldName<FSAppointment.soRefNbr>();
                string DateFromFieldName = "DateFrom";
                string DateToFieldName = "DateTo";

                parameters[SORefNbrFieldName] = fsServiceOrderRow.RefNbr;
                parameters[refNbrFieldName] = null;
                parameters[DateFromFieldName] = null;
                parameters[DateToFieldName] = null;
            }

            return parameters;
        }

        private void UpdateAttribute(FSServiceOrder fsServiceOrderRow)
        {
            bool updateAnswers = false;

            if (AttributeListRecords.Current == null
                    && fsServiceOrderRow != null
                        && fsServiceOrderRow.SOID < 0)
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
                updateAnswers = true;
            }
            else if (fsServiceOrderRow.SOID > 0
                        && AttributeListRecords.Current == null)
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
                updateAnswers = true;
            }
            else if (AttributeListRecords.Current != null
                    && (AttributeListRecords.Current.SOID != fsServiceOrderRow.SOID
                        || AttributeListRecords.Current.SrvOrdType != fsServiceOrderRow.SrvOrdType))
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
                updateAnswers = true;
            }

            if (updateAnswers == true && fsServiceOrderRow.CreatedByScreenID != ID.ScreenID.SERVICE_ORDER)
            {
                Answers.Select();
            }
        }

        private void EnableDisableDocumentByWorkflowStage(PXCache cache, FSWFStage fsWStageRow)
        {
            List<PXCache> caches = new List<PXCache>();
            Dictionary<string, PXAction> headerActions = new Dictionary<string, PXAction>();
            List<PXAction> detailsActions = new List<PXAction>();
            List<PXView> views = new List<PXView>();

            caches.Add(cache);

            headerActions.Add(typeof(FSWFStage.allowComplete).Name, this.completeOrder);
            headerActions.Add(typeof(FSWFStage.allowClose).Name, this.closeOrder);
            headerActions.Add(typeof(FSWFStage.allowCancel).Name, this.cancelOrder);
            headerActions.Add(typeof(FSWFStage.allowReopen).Name, this.reopenOrder);

            detailsActions.Add(openAppointmentScreen);
            detailsActions.Add(openServiceSelector);
            detailsActions.Add(openStaffSelectorFromServiceTab);
            detailsActions.Add(openStaffSelectorFromStaffTab);
            detailsActions.Add(createPrepayment);

            views.Add(ServiceOrderDetServices.View);
            views.Add(ServiceOrderDetParts.View);
            views.Add(ServiceOrderEmployees.View);
            views.Add(ServiceOrderEquipment.View);
            views.Add(Adjustments.View);
            views.Add(Taxes.View);

            TreeWFStageHelper.EnableDisableDocumentByWorkflowStage(fsWStageRow, caches, headerActions, detailsActions, views);
        }
        #endregion

        #region Events
        #region FSServiceOrderEvents

        protected virtual void FSServiceOrder_SalesPersonID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            FSSrvOrdType fsSrvOrdTypeRow = ServiceOrderTypeSelected.Current;

            if (fsSrvOrdTypeRow != null)
            {
                e.NewValue = fsSrvOrdTypeRow.SalesPersonID;
            }
        }

        protected virtual void FSServiceOrder_Status_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            if (fsServiceOrderRow.SrvOrdType == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType,
                                           Where<
                                                FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                                           .Select(this, fsServiceOrderRow.SrvOrdType);

            e.NewValue = fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.QUOTE ? ID.Status_ServiceOrder.QUOTE : ID.Status_ServiceOrder.OPEN;
        }

        protected virtual void FSServiceOrder_Quote_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            if (fsServiceOrderRow.SrvOrdType == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType,
                                           Where<
                                                FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                                           .Select(this, fsServiceOrderRow.SrvOrdType);

            e.NewValue = fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.QUOTE;
        }

        protected virtual void FSServiceOrder_ProjectID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            ServiceOrderCore.FSServiceOrder_ProjectID_FieldUpdated_PartialHandler(
                                                                                fsServiceOrderRow,
                                                                                ServiceOrderDetServices,
                                                                                ServiceOrderDetParts);

            ContractRelatedToProject.Current = ContractRelatedToProject.Select(fsServiceOrderRow.ProjectID);

        }

        protected virtual void FSServiceOrder_BranchID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderCore.FSServiceOrder_BranchID_FieldUpdated_PartialHandler(
                                                                                (FSServiceOrder)e.Row,
                                                                                ServiceOrderDetServices,
                                                                                ServiceOrderDetParts);
        }

        protected virtual void FSServiceOrder_WFStageID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            WFStageRelated.Current = WFStageRelated.Select(fsServiceOrderRow.WFStageID);
        }

        protected virtual void FSServiceOrder_OrderDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            cache.SetDefaultExt<FSServiceOrder.billContractPeriodID>(fsServiceOrderRow);

            ServiceOrderCore.RefreshSalesPricesInTheWholeDocument(ServiceOrderDetServices, ServiceOrderDetParts);
            foreach (FSSODetService fsSODetServiceRow in ServiceOrderDetServices.Select())
            {
                ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetServiceRow, ServiceOrderRecords.Current.OrderDate);
                ServiceOrderDetServices.Update(fsSODetServiceRow);
            }

            foreach (FSSODetPart fsSODetPartRow in ServiceOrderDetParts.Select())
            {
                ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetPartRow, ServiceOrderRecords.Current.OrderDate);
                ServiceOrderDetParts.Update(fsSODetPartRow);
            }
        }

        protected virtual void FSServiceOrder_BillServiceContractID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            cache.SetDefaultExt<FSServiceOrder.billContractPeriodID>(fsServiceOrderRow);
        }

        protected virtual void FSServiceOrder_BillContractPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            if ((int?)e.OldValue == fsServiceOrderRow.BillContractPeriodID)
            {
                return;
            }

            StandardContractPeriod.Current = StandardContractPeriod.Select();
            StandardContractPeriodDetail.Current = StandardContractPeriodDetail.Select();

            foreach (FSSODetService fsSODetServiceRow in ServiceOrderDetServices.Select())
            {
                ServiceOrderDetServices.Cache.SetDefaultExt<FSSODetService.contractRelated>(fsSODetServiceRow);
                ServiceOrderDetServices.Cache.Update(fsSODetServiceRow);
            }

            foreach (FSSODetPart fsSODetPartRow in ServiceOrderDetParts.Select())
            {
                ServiceOrderDetParts.Cache.SetDefaultExt<FSSODetPart.contractRelated>(fsSODetPartRow);
                ServiceOrderDetParts.Cache.Update(fsSODetPartRow);
            }

            cache.SetValueExtIfDifferent<FSServiceOrder.hold>(fsServiceOrderRow, fsServiceOrderRow.BillServiceContractID != null
                                                                        && fsServiceOrderRow.BillContractPeriodID == null);
        }

        protected virtual void FSServiceOrder_PromisedDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            ServiceOrderCore.CheckPromiseDate(cache, fsServiceOrderRow);
        }

        protected virtual void FSServiceOrder_AllowInvoice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            updateContractPeriod = true;
        }

        protected virtual void FSServiceOrder_SLAETA_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            ServiceOrderCore.CheckSLAETA(cache, fsServiceOrderRow);
        }

        protected virtual void FSServiceOrder_BillCustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderCore.FSServiceOrder_BillCustomerID_FieldUpdated_Handler(cache, e);

            foreach (FSSODetService fsSODetServiceRow in ServiceOrderDetServices.Select())
            {
                fsSODetServiceRow.BillCustomerID = ServiceOrderRecords.Current.CustomerID;
                ServiceOrderDetServices.Update(fsSODetServiceRow);
            }

            foreach (FSSODetPart fsSODetPartRow in ServiceOrderDetParts.Select())
            {
                fsSODetPartRow.BillCustomerID = ServiceOrderRecords.Current.CustomerID;
                ServiceOrderDetParts.Update(fsSODetPartRow);
            }

            BillingCycleRelated.Current = BillingCycleRelated.Select();
        }

        protected virtual void FSServiceOrder_Hold_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            string newStatus;
            bool? newHoldValue = Convert.ToBoolean(e.NewValue);

            if (newHoldValue == true)
            {
                newStatus = ID.Status_ServiceOrder.ON_HOLD;
            }
            else
            {
                newStatus = ServiceOrderTypeSelected.Current != null && ServiceOrderTypeSelected.Current.Behavior == ID.Behavior_SrvOrderType.QUOTE ? ID.Status_ServiceOrder.QUOTE : ID.Status_ServiceOrder.OPEN;
            }

            FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType,
               Where<
                     FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
               .Select(cache.Graph, fsServiceOrderRow.SrvOrdType);

            if (ServiceOrderCore.CheckServiceOrderStatusTransition(fsSrvOrdTypeRow.Behavior, fsServiceOrderRow, (bool)newHoldValue, newStatus) == true)
            {
                fsServiceOrderRow.Status = newStatus;
            }
            else
            {
                throw new PXSetPropertyException(TX.Error.INVALID_SO_STATUS_TRANSITION);
            }
        }

        protected virtual void FSServiceOrder_ContactID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderCore.FSServiceOrder_ContactID_FieldUpdated_Handler(this, e, ServiceOrderTypeSelected.Current);
        }

        protected virtual void FSServiceOrder_LocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderCore.FSServiceOrder_LocationID_FieldUpdated_Handler(cache, e, ServiceOrderTypeSelected.Current);
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            if (fsServiceOrderRow.LocationID == null)
            {
                cache.RaiseFieldUpdated<FSServiceOrder.customerID>(fsServiceOrderRow, fsServiceOrderRow.CustomerID);
            }
        }

        protected virtual void FSServiceOrder_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            DateTime? orderDate = null;
            FSServiceOrder FSServiceOrderRow = (FSServiceOrder)e.Row;

            if (FSServiceOrderRow != null)
            {
                orderDate  = FSServiceOrderRow.OrderDate;
            }

            ServiceOrderCore.FSServiceOrder_CustomerID_FieldUpdated_Handler(
                                                                            cache,
                                                                            e,
                                                                            ServiceOrderTypeSelected.Current,
                                                                            ServiceOrderDetServices,
                                                                            ServiceOrderDetParts,
                                                                            null,
                                                                            null,
                                                                            null,
                                                                            ServiceOrderAppointments.Select(),
                                                                            orderDate,
                                                                            allowCustomerChange,
                                                                            BillCustomer.Current);

            ServiceOrderCore.ValidateCustomerBillingCycle(cache, SetupRecord.Current, ServiceOrderTypeSelected.Current, (FSServiceOrder)e.Row);
        }

        protected virtual void FSServiceOrder_BranchLocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderCore.FSServiceOrder_BranchLocationID_FieldUpdated_Handler(
                                                                                this,
                                                                                e,
                                                                                ServiceOrderTypeSelected.Current,
                                                                                CurrentServiceOrder);
        }

        protected virtual void FSServiceOrder_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SharedFunctions.InitializeNote(cache, e);

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (fsServiceOrderRow.SOID < 0)
            {
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, fsServiceOrderRow, BillingCycleRelated.Current, null, null, DisableServiceOrderUnboundFieldCalc);
            }
        }

        protected virtual void FSServiceOrder_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            
        }

        protected virtual void FSServiceOrder_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (ServiceOrderTypeSelected.Current != null && ServiceOrderTypeSelected.Current.Behavior == ID.Behavior_SrvOrderType.QUOTE
                        && RelatedServiceOrders.Select().Count > 0)
            {
                throw new PXException(TX.Error.QUOTE_HAS_SERVICE_ORDERS);
            }

            if (ServiceOrderCore.ServiceOrderHasAppointment(this, fsServiceOrderRow) == true)
            {
                throw new PXException(TX.Error.SERVICE_ORDER_HAS_APPOINTMENTS);
            }
        }

        protected virtual void FSServiceOrder_AssignedEmpID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            // This update only applies if the assignee employee is edited from the Service Order screen.
            updateSOCstmAssigneeEmpID = e.ExternalCall;
        }

        protected virtual void FSServiceOrder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (fsServiceOrderRow == null)
                return;

            // SrvOrdType is key field
            if (string.IsNullOrWhiteSpace(((FSServiceOrder)e.Row).SrvOrdType))
            {
                GraphHelper.RaiseRowPersistingException<FSServiceOrder.srvOrdType>(cache, e.Row);
            }

            ServiceOrderCore.FSServiceOrder_RowPersisting_Handler(
                                                                (ServiceOrderEntry)cache.Graph,
                                                                cache,
                                                                e,
                                                                ServiceOrderTypeSelected.Current,
                                                                ServiceOrderDetServices,
                                                                ServiceOrderDetParts,
                                                                ServiceOrderAppointments,
                                                                ServiceOrderAttendees.Cache,
                                                                null,
                                                                ref replicateAttendees,
                                                                ServiceOrderAttendees.Select().Count,
                                                                GraphAppointmentEntryCaller,
                                                                ForceAppointmentCheckings);

            ServiceOrderCore.ValidateCustomerBillingCycle(cache, SetupRecord.Current, ServiceOrderTypeSelected.Current, (FSServiceOrder)e.Row);

            bool isEnabledCustomerID = ServiceOrderCore.AllowEnableCustomerID(fsServiceOrderRow);

            PXDefaultAttribute.SetPersistingCheck<FSServiceOrder.customerID>(cache, fsServiceOrderRow, fsServiceOrderRow.BAccountRequired != false && isEnabledCustomerID ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<FSServiceOrder.locationID>(cache, fsServiceOrderRow, fsServiceOrderRow.BAccountRequired != false ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void FSServiceOrder_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (e.TranStatus == PXTranStatus.Completed)
            {
                switch(e.Operation)
                {
                    case PXDBOperation.Delete:

                        switch (fsServiceOrderRow.SourceType)
                        {
                            case ID.SourceType_ServiceOrder.CASE:
                                ClearCase(fsServiceOrderRow);
                                break;
                            case ID.SourceType_ServiceOrder.OPPORTUNITY:
                                ClearOpportunity(fsServiceOrderRow);
                                break;
                            case ID.SourceType_ServiceOrder.SALES_ORDER:
                                ClearSalesOrder(fsServiceOrderRow);
                                break;
                        }

                        ClearPrepayment(fsServiceOrderRow);
                        break;

                    case PXDBOperation.Insert:

                        if (fsServiceOrderRow.SourceRefNbr != null)
                        {
                            if(fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.OPPORTUNITY)
                            {
                                if (PXSelect<CROpportunity,
                                    Where<CROpportunity.opportunityID,
                                        Equal<Required<CROpportunity.opportunityID>>>>.Select(this, fsServiceOrderRow.SourceRefNbr).Count > 0)
                                {
                                    PXUpdate<
                                        Set<FSxCROpportunity.sOID, Required<FSxCROpportunity.sOID>,
                                        Set<FSxCROpportunity.srvOrdType, Required<FSxCROpportunity.srvOrdType>,
                                        Set<FSxCROpportunity.branchLocationID, Required<FSxCROpportunity.branchLocationID>,
                                        Set<FSxCROpportunity.sDEnabled, True>>>>,
                                        CROpportunity,
                                        Where<
                                            CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>.Update(this, fsServiceOrderRow.SOID, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.BranchLocationID, fsServiceOrderRow.SourceRefNbr);
                                }
                            }
                            else if (fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.CASE)
                            {
                                if (PXSelect<CRCase,
                                    Where<CRCase.caseID, Equal<Required<CRCase.caseID>>,
                                        And<CRCase.caseCD, Equal<Required<CRCase.caseCD>>>>>.Select(this, fsServiceOrderRow.SourceID, fsServiceOrderRow.SourceRefNbr).Count > 0)
                                {
                                    PXUpdate<
                                        Set<FSxCRCase.sDEnabled, True,
                                        Set<FSxCRCase.branchLocationID, Required<FSxCRCase.branchLocationID>,
                                        Set<FSxCRCase.srvOrdType, Required<FSxCRCase.srvOrdType>,
                                        Set<FSxCRCase.sOID, Required<FSxCRCase.sOID>,
                                        Set<FSxCRCase.assignedEmpID, Required<FSxCRCase.assignedEmpID>,
                                        Set<FSxCRCase.problemID, Required<FSxCRCase.problemID>>>>>>>,
                                        CRCase,
                                        Where<
                                            CRCase.caseID, Equal<Required<CRCase.caseID>>,
                                            And<CRCase.caseCD, Equal<Required<CRCase.caseCD>>>>>.Update(this,
                                                                                                        fsServiceOrderRow.BranchLocationID,
                                                                                                        fsServiceOrderRow.SrvOrdType,
                                                                                                        fsServiceOrderRow.SOID,
                                                                                                        fsServiceOrderRow.AssignedEmpID,
                                                                                                        fsServiceOrderRow.ProblemID,
                                                                                                        fsServiceOrderRow.SourceID,
                                                                                                        fsServiceOrderRow.SourceRefNbr);
                                }
                            }
                            else if (fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER)
                            {
                                if (fsServiceOrderRow.RefNbr != null
                                      && fsServiceOrderRow.SourceDocType != null
                                        && fsServiceOrderRow.SourceRefNbr != null)
                                {
                                    PXUpdate<
                                            Set<FSxSOOrder.sDEnabled, True,
                                            Set<FSxSOOrder.srvOrdType, Required<FSxSOOrder.srvOrdType>,
                                            Set<FSxSOOrder.soRefNbr, Required<FSxSOOrder.soRefNbr>>>>,
                                            SOOrder,
                                            Where<
                                                SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                                                And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                    .Update(this, fsServiceOrderRow.SrvOrdType,
                                                  fsServiceOrderRow.RefNbr, 
                                                  fsServiceOrderRow.SourceDocType, 
                                                  fsServiceOrderRow.SourceRefNbr);
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
            }

            if (e.TranStatus == PXTranStatus.Completed
                    && (e.Operation == PXDBOperation.Insert
                        || e.Operation == PXDBOperation.Update))
            {
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, fsServiceOrderRow, BillingCycleRelated.Current, null, null, DisableServiceOrderUnboundFieldCalc);
            }

            Helper.Current = GetFsSelectorHelperInstance;
            ServiceOrderCore.FSServiceOrder_RowPersisted_PartialHandler(
                                                                        this,
                                                                        cache,
                                                                        e,
                                                                        ServiceOrderAttendees.Select(),
                                                                        null,
                                                                        null,
                                                                        ref replicateAttendees,
                                                                        Helper.Current);

            if (e.TranStatus == PXTranStatus.Open)
            {
                if (updateContractPeriod == true && fsServiceOrderRow.BillContractPeriodID != null)
                {
                    int multSign = fsServiceOrderRow.AllowInvoice == true ? 1 : -1;
                    FSServiceContract currentStandardContract = StandardContractRelated.Current;
                    if (currentStandardContract != null && currentStandardContract.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
                    {
                        ServiceContractEntry graphServiceContract = PXGraph.CreateInstance<ServiceContractEntry>();
                        graphServiceContract.ServiceContractRecords.Current = graphServiceContract.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceOrderRow.BillServiceContractID, fsServiceOrderRow.BillCustomerID);
                        graphServiceContract.ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.contractPeriodID>(graphServiceContract.ContractPeriodFilter.Current, fsServiceOrderRow.BillContractPeriodID);

                        FSContractPeriodDet fsContractPeriodDetRow;
                        decimal? usedQty = 0;
                        int? usedTime = 0;

                        foreach (FSSODetService fsSODetServiceRow in ServiceOrderDetServices.Select().Where(x => ((FSSODetService)x).ContractRelated == true
                                                                                                                     && ((FSSODetService)x).Status != ID.Status_AppointmentDet.CANCELED))
                        {
                            fsContractPeriodDetRow = graphServiceContract.ContractPeriodDetRecords
                                                        .Search<FSContractPeriodDet.inventoryID,
                                                            FSContractPeriodDet.SMequipmentID,
                                                            FSContractPeriodDet.billingRule>(
                                                            fsSODetServiceRow.InventoryID,
                                                            fsSODetServiceRow.SMEquipmentID,
                                                            fsSODetServiceRow.BillingRule).FirstOrDefault();

                            StandardContractPeriodDetail.Cache.Clear();
                            StandardContractPeriodDetail.Cache.ClearQueryCache();
                            StandardContractPeriodDetail.View.Clear();
                            StandardContractPeriodDetail.Select();

                            if (fsContractPeriodDetRow != null)
                            {
                                usedQty = fsContractPeriodDetRow.UsedQty + (multSign * fsSODetServiceRow.CoveredQty)
                                            + (multSign * fsSODetServiceRow.ExtraUsageQty);

                                usedTime = fsContractPeriodDetRow.UsedTime + (int?)(multSign * fsSODetServiceRow.CoveredQty * 60)
                                            + (int?)(multSign * fsSODetServiceRow.ExtraUsageQty * 60);

                                fsContractPeriodDetRow.UsedQty = fsContractPeriodDetRow.BillingRule == ID.BillingRule.FLAT_RATE ? usedQty : 0;
                                fsContractPeriodDetRow.UsedTime = fsContractPeriodDetRow.BillingRule == ID.BillingRule.TIME ? usedTime : 0;
                            }

                            graphServiceContract.ContractPeriodDetRecords.Update(fsContractPeriodDetRow);
                        }

                        graphServiceContract.Save.PressButton();
                    }
                    else if (currentStandardContract != null && currentStandardContract.RecordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
                    {
                        RouteServiceContractEntry graphRouteServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();
                        graphRouteServiceContractEntry.ServiceContractRecords.Current = graphRouteServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceOrderRow.BillServiceContractID, fsServiceOrderRow.BillCustomerID);
                        graphRouteServiceContractEntry.ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.contractPeriodID>(graphRouteServiceContractEntry.ContractPeriodFilter.Current, fsServiceOrderRow.BillContractPeriodID);

                        FSContractPeriodDet fsContractPeriodDetRow;
                        decimal? usedQty = 0;
                        int? usedTime = 0;

                        foreach (FSSODetService fsSODetServiceRow in ServiceOrderDetServices.Select().Where(x => ((FSSODetService)x).ContractRelated == true
                                                                                                                    && ((FSSODetService)x).Status != ID.Status_AppointmentDet.CANCELED))
                        {
                            fsContractPeriodDetRow = graphRouteServiceContractEntry.ContractPeriodDetRecords
                                                        .Search<FSContractPeriodDet.inventoryID,
                                                            FSContractPeriodDet.SMequipmentID,
                                                            FSContractPeriodDet.billingRule>(
                                                            fsSODetServiceRow.InventoryID,
                                                            fsSODetServiceRow.SMEquipmentID,
                                                            fsSODetServiceRow.BillingRule).FirstOrDefault();

                            StandardContractPeriodDetail.Cache.Clear();
                            StandardContractPeriodDetail.Cache.ClearQueryCache();
                            StandardContractPeriodDetail.View.Clear();
                            StandardContractPeriodDetail.Select();

                            if (fsContractPeriodDetRow != null)
                            {
                                usedQty = fsContractPeriodDetRow.UsedQty + (multSign * fsSODetServiceRow.CoveredQty)
                                            + (multSign * fsSODetServiceRow.ExtraUsageQty);

                                usedTime = fsContractPeriodDetRow.UsedTime + (int?)(multSign * fsSODetServiceRow.CoveredQty * 60)
                                            + (int?)(multSign * fsSODetServiceRow.ExtraUsageQty * 60);

                                fsContractPeriodDetRow.UsedQty = fsContractPeriodDetRow.BillingRule == ID.BillingRule.FLAT_RATE ? usedQty : 0;
                                fsContractPeriodDetRow.UsedTime = fsContractPeriodDetRow.BillingRule == ID.BillingRule.TIME ? usedTime : 0;
                            }

                            graphRouteServiceContractEntry.ContractPeriodDetRecords.Update(fsContractPeriodDetRow);
                        }

                        graphRouteServiceContractEntry.Save.PressButton();
                    }

                    ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();

                    var serviceOrdersRelatedToSameBillingPeriod = PXSelect<FSServiceOrder,
                                                                            Where<
                                                                                FSServiceOrder.billCustomerID, Equal<Required<FSServiceOrder.billCustomerID>>,
                                                                                And<FSServiceOrder.billServiceContractID, Equal<Required<FSServiceOrder.billServiceContractID>>,
                                                                                And<FSServiceOrder.billContractPeriodID, Equal<Required<FSServiceOrder.billContractPeriodID>>,
                                                                                And<FSServiceOrder.status, NotEqual<FSServiceOrder.status.Hold>,
                                                                                And<FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                                                                                And<FSServiceOrder.allowInvoice, Equal<False>,
                                                                                And<FSServiceOrder.sOID, NotEqual<Required<FSServiceOrder.sOID>>>>>>>>>>
                                                                    .Select(graphServiceOrderEntry,
                                                                            fsServiceOrderRow.BillCustomerID,
                                                                            fsServiceOrderRow.BillServiceContractID,
                                                                            fsServiceOrderRow.BillContractPeriodID,
                                                                            fsServiceOrderRow.SOID);

                    foreach (FSServiceOrder fsRelatedServiceOrderRow in serviceOrdersRelatedToSameBillingPeriod)
                    {
                        graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsRelatedServiceOrderRow.RefNbr, fsRelatedServiceOrderRow.SrvOrdType);
                        graphServiceOrderEntry.ServiceOrderRecords.Cache.SetDefaultExt<FSServiceOrder.billContractPeriodID>(fsRelatedServiceOrderRow);
                        graphServiceOrderEntry.Save.PressButton();
                    }
                }

                if (fsServiceOrderRow.UpdateAppWaitingForParts == true)
                {
                    if (fsServiceOrderRow.WaitingForParts == false)
                    {
                        PXUpdate<
                            Set<FSAppointment.waitingForParts, Required<FSAppointment.waitingForParts>>,
                        FSAppointment,
                        Where<
                            FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>
                        .Update(this, 0, fsServiceOrderRow.SOID);

                        foreach (FSAppointment fsAppointmentRow in ServiceOrderAppointments.Select())
                        {
                            fsAppointmentRow.WaitingForParts = false;
                        }
                    }
                    else
                    {
                        List<FSSODet> soDetList = new List<FSSODet>();

						foreach(FSSODet temp in ServiceOrderDetParts.Select().Where(y => (((FSSODet)y).EnablePO == true && ((FSSODet)y).POCompleted != true)))
                        {
                            soDetList.Add(temp);
                        }
						
	                    foreach (FSSODet temp in ServiceOrderDetServices.Select().Where(y => (((FSSODet)y).EnablePO == true && ((FSSODet)y).POCompleted != true)))
						{
                            soDetList.Add(temp);
                        }

                        foreach (FSAppointment fsAppointmentRow in ServiceOrderAppointments.Select())
                        {
                            bool found = false;

                            foreach (FSAppointmentDet a in PXSelect<FSAppointmentDet,
                                                    Where<
                                                        FSAppointmentDet.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>>>
                                                    .Select(this, fsAppointmentRow.AppointmentID))
                            {
                                if (soDetList.Where(x => x.SODetID == a.SODetID).Count() > 0)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (fsAppointmentRow.WaitingForParts == true
                                && found == false)
                            {
                                PXUpdate<
                                    Set<FSAppointment.waitingForParts, Required<FSAppointment.waitingForParts>>,
                                FSAppointment,
                                Where<
                                    FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                .Update(this, 0, fsAppointmentRow.AppointmentID);

                                fsAppointmentRow.WaitingForParts = false;
                            }

                            if (fsAppointmentRow.WaitingForParts == false
                                && found == true)
                            {
                                PXUpdate<
                                    Set<FSAppointment.waitingForParts, Required<FSAppointment.waitingForParts>>,
                                FSAppointment,
                                Where<
                                    FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                .Update(this, 1, fsAppointmentRow.AppointmentID);

                                fsAppointmentRow.WaitingForParts = true;
                            }
                        }
                    }

                    fsServiceOrderRow.UpdateAppWaitingForParts = false;
                }
            }

            // Asignee can only be updated from the Service Order & Sales Order screens
            // that's why it's not included in the ServiceCore RowPersisted handler
            UpdateAssignedEmpIDinSalesOrder((FSServiceOrder)e.Row);
        }

        protected virtual void FSServiceOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (string.IsNullOrEmpty(fsServiceOrderRow.SrvOrdType))
            {
                return;
            }

            if (fsServiceOrderRow.ProjectID != null
                && fsServiceOrderRow.ProjectID >= 0
                && ContractRelatedToProject?.Current == null)
            {
                ContractRelatedToProject.Current = ContractRelatedToProject.Select(fsServiceOrderRow.ProjectID);
            }

            if (fsServiceOrderRow.WFStageID != null
                && fsServiceOrderRow.WFStageID > 0
                && WFStageRelated?.Current == null)
            {
                WFStageRelated.Current = WFStageRelated.Select(fsServiceOrderRow.WFStageID);
            }
            
            EnableDisable_ActionButtons(fsServiceOrderRow, cache);
            HideRooms(fsServiceOrderRow);
            HideOrShowTabs(fsServiceOrderRow);

            ServiceOrderCore.HidePrepayments(Adjustments.View, cache, fsServiceOrderRow, null, ServiceOrderTypeSelected.Current);
            createPrepayment.SetEnabled(fsServiceOrderRow.IsPrepaymentEnable == true 
                                        && cache.GetStatus(e.Row) != PXEntryStatus.Inserted
                                        && ServiceOrderTypeSelected.Current?.Active == true);

            ServiceOrderCore.FSServiceOrder_RowSelected_PartialHandler(
                                                                    this,
                                                                    cache,
                                                                    fsServiceOrderRow,
                                                                    null,
                                                                    ServiceOrderTypeSelected.Current,
                                                                    BillingCycleRelated.Current,
                                                                    ContractRelatedToProject?.Current,
                                                                    ServiceOrderAppointments.Select().Count,
                                                                    ServiceOrderDetServices.Select().Count,
                                                                    ServiceOrderDetParts.Select().Count,
                                                                    ServiceOrderDetServices.Cache,
                                                                    ServiceOrderDetParts.Cache,
                                                                    ServiceOrderAppointments.Cache,
                                                                    ServiceOrderAttendees.Cache,
                                                                    ServiceOrderEquipment.Cache,
                                                                    ServiceOrderEmployees.Cache,
                                                                    ServiceOrder_Contact.Cache,
                                                                    ServiceOrder_Address.Cache,
                                                                    ServiceOrderRecords.Current.IsCalledFromQuickProcess,
                                                                    allowCustomerChange);

            bool showPOFields = ServiceOrderTypeSelected.Current?.PostTo == ID.Batch_PostTo.SO || ServiceOrderTypeSelected.Current?.PostTo == ID.Batch_PostTo.SI;
            ServiceOrderAppointmentHandlers.SetVisiblePODetFields(ServiceOrderDetServices.Cache, showPOFields);
            ServiceOrderAppointmentHandlers.SetVisiblePODetFields(ServiceOrderDetParts.Cache, showPOFields);

            PXUIFieldAttribute.SetVisible<FSSODetPart.enablePO>(ServiceOrderDetParts.Cache, null, AllocationSOOrderTypeSelected.Current?.RequireShipping ?? false);
            PXUIFieldAttribute.SetVisible<FSSODetPart.pOCreate>(ServiceOrderDetParts.Cache, null, AllocationSOOrderTypeSelected.Current?.RequireShipping ?? false);
            PXUIFieldAttribute.SetVisible<FSSODetPartSplit.pOCreate>(partSplits.Cache, null, AllocationSOOrderTypeSelected.Current?.RequireShipping ?? false);
            UpdateAttribute(fsServiceOrderRow);

            Caches[typeof(FSContact)].AllowUpdate = fsServiceOrderRow.AllowOverrideContactAddress == true && Caches[typeof(FSContact)].AllowUpdate == true;
            Caches[typeof(FSAddress)].AllowUpdate = fsServiceOrderRow.AllowOverrideContactAddress == true && Caches[typeof(FSContact)].AllowUpdate == true;

            PXUIFieldAttribute.SetEnabled<FSManufacturer.allowOverrideContactAddress>(cache, fsServiceOrderRow, !(fsServiceOrderRow.CustomerID == null && fsServiceOrderRow.ContactID == null));

            EnableDisableDocumentByWorkflowStage(cache, WFStageRelated?.Current);
        }

        protected virtual void FSServiceOrder_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            using (new PXConnectionScope())
            {
                PXResultset<FSAppointment> fsAppointmentSet = PXSelect<FSAppointment,
                                    Where2<
                                        Where<
                                            FSAppointment.status, Equal<ListField_Status_Appointment.Closed>,
                                            Or<FSAppointment.status, Equal<ListField_Status_Appointment.Completed>>>,
                                        And<FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>>
                                    .Select(this, fsServiceOrderRow.SOID);

                fsServiceOrderRow.AppointmentsCompletedCntr = fsAppointmentSet.Where(x => ((FSAppointment)x).Status == ID.Status_Appointment.COMPLETED).Count();
                fsServiceOrderRow.AppointmentsCompletedOrClosedCntr = fsAppointmentSet.Count();
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, fsServiceOrderRow, BillingCycleRelated.Current, null, null, DisableServiceOrderUnboundFieldCalc);
            }
        }

        #endregion
        #region FSSODet
        protected virtual void FSSODetService_EstimatedQty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSSODetService.curyUnitPrice>(cache, e);
        }

        protected virtual void FSSODetService_EstimatedDuration_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
            ServiceOrderAppointmentHandlers.X_Duration_FieldUpdated<FSSODetService, FSSODetService.estimatedQty>(cache, e, fsSODetServiceRow.EstimatedDuration);
        }

        protected virtual void FSSODetService_SMEquipmentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetServiceRow, ServiceOrderRecords.Current.OrderDate);
        }

        protected virtual void FSSODetService_ComponentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetServiceRow, ServiceOrderRecords.Current.OrderDate);
        }

        protected virtual void FSSODetService_EquipmentLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetServiceRow, ServiceOrderRecords.Current.OrderDate);

            if (fsSODetServiceRow.ComponentID == null)
            {
                fsSODetServiceRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, fsSODetServiceRow.SMEquipmentID, fsSODetServiceRow.EquipmentLineRef);
            }
        }

        protected virtual void FSSODetService_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
            FSServiceOrder fsServiceOrderRow = CurrentServiceOrder.Current;

            ServiceOrderAppointmentHandlers.X_InventoryID_FieldUpdated<FSSODetService, FSSODetService.subItemID,
                                                FSSODetService.siteID, FSSODetService.siteLocationID, FSSODetService.uOM,
                                                FSSODetService.estimatedDuration, FSSODetService.estimatedQty, FSSODetService.billingRule,
                                                ServiceOrderAppointmentHandlers.fakeField, ServiceOrderAppointmentHandlers.fakeField>(
                                                        cache,
                                                        e,
                                                        fsServiceOrderRow.BranchLocationID,
                                                        BillCustomer.Current,
                                                        useActualFields: false);

            cache.SetDefaultExt<FSSODetService.curyUnitCost>(fsSODetServiceRow);
            cache.SetDefaultExt<FSSODetService.enablePO>(fsSODetServiceRow);

            if (e.ExternalCall == false)
            {
                //In case ExternalCall == false (any change not coming from Service Order's UI), ShowPopupMessage = false to avoid
                //showing the Popup Message for the InventoryID field. By default it's set to true.

                foreach (PXSelectorAttribute s in cache.GetAttributes(typeof(FSSODetService.inventoryID).Name).OfType<PXSelectorAttribute>())
                {
                    s.ShowPopupMessage = false;
                }
            }
        }

        protected virtual void FSSODetService_StaffID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;

            if (fsSODetServiceRow.StaffID != null && e.OldValue != null)
            {
                FSSOEmployee fsSOEmployeeRow = PXSelect<FSSOEmployee,
                                                Where<
                                                    FSSOEmployee.serviceLineRef, Equal<Required<FSSOEmployee.serviceLineRef>>,
                                                    And<FSSOEmployee.employeeID, Equal<Required<FSSOEmployee.employeeID>>,
                                                    And<FSSOEmployee.sOID, Equal<Current<FSServiceOrder.sOID>>>>>>.Select(this, fsSODetServiceRow.LineRef, e.OldValue);

                fsSOEmployeeRow.EmployeeID = fsSODetServiceRow.StaffID;
                ServiceOrderEmployees.Update(fsSOEmployeeRow);
            }
            else if (fsSODetServiceRow.StaffID != null && e.OldValue == null)
            {
				if(this.IsContractBasedAPI 
                        && string.IsNullOrEmpty(fsSODetServiceRow.LineRef))
                {
                    cache.RaiseRowInserting(fsSODetServiceRow);
                }

                FSSOEmployee fsSOEmployeeRow = new FSSOEmployee()
                {
                    EmployeeID = fsSODetServiceRow.StaffID
                };

                fsSOEmployeeRow = ServiceOrderEmployees.Insert(fsSOEmployeeRow);
                fsSOEmployeeRow.ServiceLineRef = fsSODetServiceRow.LineRef;
            }
            else
            {
                FSSOEmployee fsSOEmployeeRow = PXSelect<FSSOEmployee,
                                                Where<
                                                    FSSOEmployee.serviceLineRef, Equal<Required<FSSOEmployee.serviceLineRef>>,
                                                    And<FSSOEmployee.employeeID, Equal<Required<FSSOEmployee.employeeID>>,
                                                    And<FSSOEmployee.sOID, Equal<Current<FSServiceOrder.sOID>>>>>>
                                                .Select(this, fsSODetServiceRow.LineRef, e.OldValue);

                ServiceOrderEmployees.Delete(fsSOEmployeeRow);
            }
        }

        protected virtual void FSSODetService_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_LineType_FieldUpdated<FSSODetService>(cache, e);
        }

        protected virtual void FSSODetService_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            ServiceOrderHandlers.FSSODet_RowInserting(cache, e);

            if (e.Row != null && ((FSSODet)e.Row).EnablePO == true)
            {
                CurrentServiceOrder.Current.UpdateAppWaitingForParts = true;
            }
        }

        protected virtual void FSSODetService_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
            if (fsSODetServiceRow.LineType == null)
            {
                fsSODetServiceRow.LineType = ID.LineType_ServiceTemplate.SERVICE;
            }

            ServiceOrderAppointmentHandlers.X_SetPersistingCheck<FSSODetService>(cache, e, CurrentServiceOrder.Current, ServiceOrderTypeSelected.Current);
        }

        protected virtual void FSSODetService_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;

            EnableDisable_SODetLine(this, cache, fsSODetServiceRow, ServiceOrderTypeSelected.Current, CurrentServiceOrder.Current);
            EnableDisable_StaffID(cache, fsSODetServiceRow);

            // Move the old code of SetEnabled and SetPersistingCheck in previous methods to this new generic method
            // keeping the generic convention.
            ServiceOrderAppointmentHandlers.X_RowSelected<FSSODetService>(
                                            cache,
                                            e,
                                            CurrentServiceOrder.Current,
                                            ServiceOrderTypeSelected.Current,
                                            disableSODetReferenceFields: false,
                                            docAllowsActualFieldEdition: false);
        }
		protected virtual void FSSODetService_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			ServiceOrderHandlers.FSSODet_RowSelecting(cache, e);

			FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
			PXResultset<FSSOEmployee> fsSOEmployeeRows = null;

			using (new PXConnectionScope())
			{
				fsSOEmployeeRows = PXSelect<FSSOEmployee,
									Where<
										FSSOEmployee.sOID, Equal<Required<FSServiceOrder.sOID>>,
										And<FSSOEmployee.serviceLineRef, Equal<Required<FSSOEmployee.serviceLineRef>>>>>
									.Select(cache.Graph, fsSODetServiceRow.SOID, fsSODetServiceRow.LineRef);

                SetSODetServiceLastReferencedBy(fsSODetServiceRow);

                if (fsSOEmployeeRows != null)
                {
                    fsSODetServiceRow.EnableStaffID = fsSOEmployeeRows.Count <= 1;

                    if (fsSOEmployeeRows.Count == 1)
                    {
                        fsSODetServiceRow.StaffID = ((FSSOEmployee)fsSOEmployeeRows).EmployeeID;
                    }
                    else
                    {
                        fsSODetServiceRow.StaffID = null;
                    }
                }
                else
                {
                    fsSODetServiceRow.EnableStaffID = fsSODetServiceRow.SOID < 0;
                    fsSODetServiceRow.StaffID = null;
                }
            }
        }

        protected virtual void FSSODetService_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            MarkHeaderAsUpdated(cache, e.Row);

            TaxSourceLines.Insert(CreateTaxRow((FSSODet)e.Row));
        }

        protected virtual void FSSODetPart_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            MarkHeaderAsUpdated(cache, e.Row);

            TaxSourceLines.Insert(CreateTaxRow((FSSODet)e.Row));
        }

        protected virtual void FSSODetService_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            MarkHeaderAsUpdated(cache, e.Row);
            ServiceOrderAppointmentHandlers.CheckIfManualPrice<FSSODetService, FSSODetService.estimatedQty>(cache, e);
            ServiceOrderAppointmentHandlers.CheckSOIfManualCost(cache, e);

            TaxSourceLines.Update(CreateTaxRow((FSSODet)e.Row));

            if (!cache.ObjectsEqual<FSSODet.enablePO>(e.Row, e.OldRow))
            {
                CurrentServiceOrder.Current.UpdateAppWaitingForParts = true;
            }
        }

        protected virtual void FSSODetService_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            MarkHeaderAsUpdated(cache, e.Row);

            TaxSourceLines.Delete(CreateTaxRow((FSSODet)e.Row));
            ClearTaxes(CurrentServiceOrder.Current);

            if (((FSSODet)e.Row).EnablePO == true)
            {
                CurrentServiceOrder.Current.UpdateAppWaitingForParts = true;
            }
        }

        protected virtual void FSSODetPart_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            MarkHeaderAsUpdated(cache, e.Row);

            TaxSourceLines.Delete(CreateTaxRow((FSSODet)e.Row));
            ClearTaxes(CurrentServiceOrder.Current);
        }

        protected virtual void FSSODetService_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;

            if (ServiceOrderCore.FSSODetLinkedToAppointments(this, fsSODetServiceRow) == true)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.FSSODET_LINKED_TO_APPOINTMENTS, "Service"), PXErrorLevel.Error);
            }

            if (this.ServiceOrderRecords.Current != null
                    && this.ServiceOrderRecords.Current.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER
                        && e.ExternalCall == true)
            {
                if (IsThisLineRelatedToAsoLine(this.ServiceOrderRecords.Current, fsSODetServiceRow))
                {
                    throw new PXException(TX.Error.FSSODET_LINE_IS_RELATED_TO_A_SOLINE, fsSODetServiceRow.SourceLineNbr);
                }
            }

            foreach (FSSOEmployee fsSOEmployeeRow in ServiceOrderEmployees.Select().Where(y => ((FSSOEmployee)y).ServiceLineRef == fsSODetServiceRow.LineRef))
            {
                ServiceOrderEmployees.Delete(fsSOEmployeeRow);
            }
        }

        protected virtual void FSSODetService_AcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_AcctID_FieldDefaulting<FSSODetService>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    CurrentServiceOrder.Current);
        }

        protected virtual void FSSODetService_ContractRelated_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || ServiceOrderTypeSelected.Current == null || CurrentServiceOrder.Current == null)
            {
                return;
            }

            if (CurrentServiceOrder.Current.BillServiceContractID == null ||
                CurrentServiceOrder.Current.BillContractPeriodID == null ||
                BillingCycleRelated.Current == null ||
                BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER)
            {
                e.NewValue = false;
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;
            FSSODetService fsSODetDuplicatedByContract = ServiceOrderDetServices.Search<FSSODet.inventoryID, FSSODet.SMequipmentID, FSSODet.billingRule, FSSODet.contractRelated>
                (fsSODetServiceRow.InventoryID, fsSODetServiceRow.SMEquipmentID, fsSODetServiceRow.BillingRule, true);
            bool duplicatedContractLine = fsSODetDuplicatedByContract != null && fsSODetDuplicatedByContract.LineNbr != fsSODetServiceRow.LineNbr;

            e.NewValue = duplicatedContractLine == false
                            && StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetServiceRow.InventoryID
                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetServiceRow.SMEquipmentID
                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                && ((FSContractPeriodDet)x).BillingRule == fsSODetServiceRow.BillingRule).Count() == 1;
        }

        protected virtual void FSSODetService_CoveredQty_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || BillingCycleRelated.Current == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;

            if (BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER ||
                fsSODetServiceRow.ContractRelated == false)
            {
                e.NewValue = 0m;
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetServiceRow.InventoryID
                                                                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetServiceRow.SMEquipmentID
                                                                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                                                                && ((FSContractPeriodDet)x).BillingRule == fsSODetServiceRow.BillingRule).FirstOrDefault();

            if (fsContractPeriodDetRow != null)
            {
                if (fsSODetServiceRow.BillingRule == ID.BillingRule.TIME)
                {
                    e.NewValue = fsContractPeriodDetRow.RemainingTime - fsSODetServiceRow.EstimatedDuration >= 0 ? fsSODetServiceRow.EstimatedQty : fsContractPeriodDetRow.RemainingTime / 60;
                }
                else
                {
                    e.NewValue = fsContractPeriodDetRow.RemainingQty - fsSODetServiceRow.EstimatedQty >= 0 ? fsSODetServiceRow.EstimatedQty : fsContractPeriodDetRow.RemainingQty;
                }
            }
            else
            {
                e.NewValue = 0m;
            }
        }

        protected virtual void FSSODetService_CuryExtraUsageUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || BillingCycleRelated.Current == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;

            if (BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER ||
               fsSODetServiceRow.ContractRelated == false)
            {
                e.NewValue = 0m;
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetServiceRow.InventoryID
                                                                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetServiceRow.SMEquipmentID
                                                                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                                                                && ((FSContractPeriodDet)x).BillingRule == fsSODetServiceRow.BillingRule).FirstOrDefault();

            if (fsContractPeriodDetRow != null)
            {
                e.NewValue = fsContractPeriodDetRow.OverageItemPrice;
            }
            else
            {
                e.NewValue = 0m;
            }
        }

        protected virtual void FSSODetService_SubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SubID_FieldDefaulting<FSSODetService>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    CurrentServiceOrder.Current);
        }

        protected virtual void FSSODetService_EnablePO_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            FSSODetService row = e.Row as FSSODetService;
            if (row == null)
            {
                return;
            }

            bool? newVal = (bool?)e.NewValue;
            POCreateVerifyValue(sender, row, newVal);
        }

        protected virtual void FSSODetService_POVendorID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;

            if (fsSODetServiceRow.EnablePO == false || fsSODetServiceRow.InventoryID == null)
            {
                e.Cancel = true;
            }
        }

        protected virtual void FSSODetService_POVendorLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServiceRow = (FSSODetService)e.Row;

            if (fsSODetServiceRow.EnablePO == false || fsSODetServiceRow.InventoryID == null || fsSODetServiceRow.POVendorID == null)
            {
                e.Cancel = true;
            }
        }

        public virtual void POCreateVerifyValue(PXCache sender, FSSODetService row, bool? value)
        {
            if (row == null)
            {
                return;
            }

            if (row.InventoryID != null && value == true)
            {
                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
                if (item != null && item.StkItem != true)
                {
                    if (item.KitItem == true)
                    {
                        throw new PXSetPropertyException(PX.Objects.SO.Messages.SOPOLinkNotForNonStockKit);
                    }
                    else if (item.NonStockShip != true || item.NonStockReceipt != true)
                    {
                        sender.RaiseExceptionHandling<FSSODetService.enablePO>(row, value, new PXSetPropertyException(PX.Objects.SO.Messages.NonStockShipReceiptIsOff, PXErrorLevel.Warning));
                    }
                }
            }
        }

        protected virtual void FSSODetPart_AcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_AcctID_FieldDefaulting<FSSODetPart>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    CurrentServiceOrder.Current);
        }

        protected virtual void FSSODetPart_SubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SubID_FieldDefaulting<FSSODetPart>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    CurrentServiceOrder.Current);
        }

        protected virtual void FSSODetPart_POVendorID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;

            if (fsSODetPartRow.EnablePO == false || fsSODetPartRow.InventoryID == null)
            {
                e.Cancel = true;
            }
        }

        protected virtual void FSSODetPart_POVendorLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;

            if (fsSODetPartRow.EnablePO == false || fsSODetPartRow.InventoryID == null || fsSODetPartRow.POVendorID == null)
            {
                e.Cancel = true;
            }
        }

        protected virtual void FSSODetPart_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderHandlers.FSSODet_RowSelecting(cache, e);
        }

        protected virtual void FSSODetPart_IsPrepaid_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_IsPrepaid_FieldUpdated<FSSODetPart,
                                FSSODetPart.manualPrice, FSSODetPart.isBillable,
                                FSSODetPart.estimatedDuration, ServiceOrderAppointmentHandlers.fakeField>(
                                            cache,
                                            e,
                                            useActualField: false);
        }

        protected virtual void FSSODetService_IsPrepaid_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_IsPrepaid_FieldUpdated<FSSODetService,
                                FSSODetService.manualPrice, FSSODetService.isBillable,
                                FSSODetService.estimatedDuration, ServiceOrderAppointmentHandlers.fakeField>(
                                            cache,
                                            e,
                                            useActualField: false);
        }

        protected virtual void FSSODetService_ManualPrice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_ManualPrice_FieldUpdated<FSSODetService, FSSODetService.curyUnitPrice>(cache, e);
        }

        protected virtual void FSSODetPart_ManualPrice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_ManualPrice_FieldUpdated<FSSODetPart, FSSODetPart.curyUnitPrice>(cache, e);
        }

        protected virtual void FSSODetService_BillingRule_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_BillingRule_FieldVerifying<FSSODetService>(cache, e);
        }

        protected virtual void FSSODetService_BillingRule_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_BillingRule_FieldUpdated<FSSODetService,
                                    FSSODetService.estimatedDuration, ServiceOrderAppointmentHandlers.fakeField,
                                    FSSODetService.curyUnitPrice>(
                                                cache,
                                                e,
                                                useActualField: false);
        }

        protected virtual void FSSODetService_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldUpdated<FSSODetService.curyUnitPrice>(cache, e);
            cache.SetDefaultExt<FSSODetService.curyUnitCost>(e.Row);
        }

        protected virtual void FSSODetPart_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldUpdated<FSSODetPart.curyUnitPrice>(cache, e);
            cache.SetDefaultExt<FSSODetPart.curyUnitCost>(e.Row);
        }

        protected virtual void FSSODetService_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SiteID_FieldUpdated<FSSODetService.curyUnitPrice, FSSODetService.acctID, FSSODetService.subID>(cache, e);
            cache.SetDefaultExt<FSSODetService.curyUnitCost>(e.Row);
        }

        protected virtual void FSSODetService_SiteLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            FSSODetService row = (FSSODetService)e.Row;
            if (row != null && row.RequireLocation != true)
            {
                e.NewValue = null;
                e.Cancel = true;
            }
        }

        protected virtual void FSSODetServiceSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            FSSODetServiceSplit row = (FSSODetServiceSplit)e.Row;
            if (row != null && row.RequireLocation != true)
            {
                e.NewValue = null;
                e.Cancel = true;
            }
        }

        protected virtual void FSSODetPart_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SiteID_FieldUpdated<FSSODetPart.curyUnitPrice, FSSODetPart.acctID, FSSODetPart.subID>(cache, e);
            cache.SetDefaultExt<FSSODetPart.curyUnitCost>(e.Row);
        }

        protected virtual void FSSODetPart_SiteLocationID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            FSSODetPart row = (FSSODetPart)e.Row;
            if (row != null && row.RequireLocation != true)
            {
                e.NewValue = null;
                e.Cancel = true;
            }
        }

        protected virtual void FSSODetPartSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            FSSODetPartSplit row = (FSSODetPartSplit)e.Row;
            if (row != null && row.RequireLocation != true)
            {
                e.NewValue = null;
                e.Cancel = true;
            }
        }

        protected virtual void FSSODetPart_ContractRelated_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || ServiceOrderTypeSelected.Current == null || CurrentServiceOrder.Current == null || StandardContractPeriodDetail == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;

            if (BillingCycleRelated.Current == null ||
                BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER)
            {
                e.NewValue = false;
                return;
            }

            e.NewValue = StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetPartRow.InventoryID
                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetPartRow.SMEquipmentID
                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                && ((FSContractPeriodDet)x).BillingRule == fsSODetPartRow.BillingRule).Count() == 1;
        }

        protected virtual void FSSODetService_CuryUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (FSSODetService)e.Row;
            FSServiceOrder fsServiceOrderRow = CurrentServiceOrder.Current;

            var currencyInfo = ExtensionHelper.SelectCurrencyInfo(currencyInfoView, fsServiceOrderRow.CuryInfoID);

            ServiceOrderAppointmentHandlers.X_CuryUnitPrice_FieldDefaulting<FSSODetService, FSSODetService.curyUnitPrice>(
                                                    cache,
                                                    e,
                                                    row.EstimatedQty,
                                                    fsServiceOrderRow.OrderDate,
                                                    fsServiceOrderRow,
                                                    null,
                                                    currencyInfo);
        }

        protected virtual void FSSODetPart_CuryUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (FSSODetPart)e.Row;
            FSServiceOrder fsServiceOrderRow = CurrentServiceOrder.Current;

            var currencyInfo = ExtensionHelper.SelectCurrencyInfo(currencyInfoView, fsServiceOrderRow.CuryInfoID);

            ServiceOrderAppointmentHandlers.X_CuryUnitPrice_FieldDefaulting<FSSODetPart, FSSODetPart.curyUnitPrice>(
                                                    cache,
                                                    e,
                                                    row.EstimatedQty,
                                                    fsServiceOrderRow.OrderDate,
                                                    fsServiceOrderRow,
                                                    null,
                                                    currencyInfo);
        }

        protected virtual void FSSODetService_CuryUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetService fsSODetServicetRow = (FSSODetService)e.Row;

            if (string.IsNullOrEmpty(fsSODetServicetRow.UOM) == false && fsSODetServicetRow.EnablePO == true && fsSODetServicetRow.InventoryID != null)
            {
                object unitcost;
                cache.RaiseFieldDefaulting<FSSODetService.unitCost>(e.Row, out unitcost);

                if (unitcost != null && (decimal)unitcost != 0m)
                {
                    decimal newval = INUnitAttribute.ConvertToBase<FSSODetService.inventoryID, FSSODetService.uOM>(cache, fsSODetServicetRow, (decimal)unitcost, INPrecision.NOROUND);

                    IPXCurrencyHelper currencyHelper = this.FindImplementation<IPXCurrencyHelper>();

                    if (currencyHelper != null)
                    {
                        currencyHelper.CuryConvCury((decimal)unitcost, out newval);
                    }
                    else
                    {
                        CM.PXDBCurrencyAttribute.CuryConvCury(cache, fsSODetServicetRow, newval, out newval, true);
                    }

                    e.NewValue = Math.Round(newval, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
                    e.Cancel = true;
                }
            }
        }

        protected virtual void FSSODetPart_CuryUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;

            if (string.IsNullOrEmpty(fsSODetPartRow.UOM) == false && fsSODetPartRow.EnablePO == true && fsSODetPartRow.InventoryID != null)
            {
                object unitcost;
                cache.RaiseFieldDefaulting<FSSODetPart.unitCost>(e.Row, out unitcost);

                if (unitcost != null && (decimal)unitcost != 0m)
                {
                    decimal newval = INUnitAttribute.ConvertToBase<FSSODetPart.inventoryID, FSSODetPart.uOM>(cache, fsSODetPartRow, (decimal)unitcost, INPrecision.NOROUND);

                    IPXCurrencyHelper currencyHelper = this.FindImplementation<IPXCurrencyHelper>();

                    if (currencyHelper != null)
                    {
                        currencyHelper.CuryConvCury((decimal)unitcost, out newval);
                    }
                    else
                    {
                        CM.PXDBCurrencyAttribute.CuryConvCury(cache, fsSODetPartRow, newval, out newval, true);
                    }

                    e.NewValue = Math.Round(newval, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
                    e.Cancel = true;
                }
            }
        }

        protected virtual void FSSODetPart_EstimatedQty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSSODetPart.curyUnitPrice>(cache, e);
        }

        protected virtual void FSSODetPart_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;
            FSServiceOrder fsServiceOrderRow = CurrentServiceOrder.Current;

            ServiceOrderAppointmentHandlers.X_InventoryID_FieldUpdated<FSSODetPart, FSSODetPart.subItemID,
                                                FSSODetPart.siteID, FSSODetPart.siteLocationID, FSSODetPart.uOM,
                                                FSSODetPart.estimatedDuration, FSSODetPart.estimatedQty, FSSODetPart.billingRule,
                                                ServiceOrderAppointmentHandlers.fakeField, ServiceOrderAppointmentHandlers.fakeField>(
                                                        cache,
                                                        e,
                                                        fsServiceOrderRow.BranchLocationID,
                                                        BillCustomer.Current,
                                                        useActualFields: false);

            SharedFunctions.UpdateEquipmentFields(this, cache, fsSODetPartRow, fsSODetPartRow.InventoryID);

            cache.SetDefaultExt<FSSODetPart.curyUnitCost>(fsSODetPartRow);
            cache.SetDefaultExt<FSSODetPart.enablePO>(fsSODetPartRow);

            if (e.ExternalCall == false)
            {
                //In case ExternalCall == false (any change not coming from Service Order's UI), ShowPopupMessage = false to avoid
                //showing the Popup Message for the InventoryID field. By default it's set to true.

                foreach (PXSelectorAttribute s in cache.GetAttributes(typeof(FSSODetPart.inventoryID).Name).OfType<PXSelectorAttribute>())
                {
                    s.ShowPopupMessage = false;
                }
            }
        }

        protected virtual void FSSODetPart_SMEquipmentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetPartRow, ServiceOrderRecords.Current.OrderDate);
        }

        protected virtual void FSSODetPart_ComponentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetPartRow, ServiceOrderRecords.Current.OrderDate);
        }

        protected virtual void FSSODetPart_EquipmentLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetPartRow, ServiceOrderRecords.Current.OrderDate);

            if (fsSODetPartRow.ComponentID == null)
            {
                fsSODetPartRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, fsSODetPartRow.SMEquipmentID, fsSODetPartRow.EquipmentLineRef);
            }
        }

        protected virtual void FSSODetPart_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_LineType_FieldUpdated<FSSODetPart>(cache, e);
        }

        protected virtual void FSSODetPart_EquipmentAction_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;

            if (fsSODetPartRow.CreatedByScreenID == ID.ScreenID.SERVICE_ORDER)
            {
                SharedFunctions.ResetEquipmentFields(cache, fsSODetPartRow);
            }
        }

        protected virtual void FSSODetPart_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            ServiceOrderHandlers.FSSODet_RowInserting(cache, e);

            if (e.Row != null && ((FSSODet)e.Row).EnablePO == true)
            {
                CurrentServiceOrder.Current.UpdateAppWaitingForParts = true;
            }
        }

        protected virtual void FSSODetPart_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;
            if (fsSODetPartRow.LineType == null)
            {
                fsSODetPartRow.LineType = ID.LineType_ServiceTemplate.INVENTORY_ITEM;
            }

            string errorMessage = string.Empty;

            if (e.Operation != PXDBOperation.Delete
                    && !SharedFunctions.AreEquipmentFieldsValid(cache, fsSODetPartRow.InventoryID, fsSODetPartRow.SMEquipmentID, fsSODetPartRow.NewTargetEquipmentLineNbr, fsSODetPartRow.EquipmentAction, ref errorMessage))
            {
                cache.RaiseExceptionHandling<FSSODetPart.equipmentAction>(fsSODetPartRow, fsSODetPartRow.EquipmentAction, new PXSetPropertyException(errorMessage));
            }

            if (EquipmentHelper.CheckReplaceComponentLines<FSSODetPart, FSSODetPart.equipmentLineRef>(cache, ServiceOrderDetParts.Select(), (FSSODetPart)e.Row) == false)
            {
                return;
            }

            ServiceOrderAppointmentHandlers.X_SetPersistingCheck<FSSODetPart>(cache, e, CurrentServiceOrder.Current, ServiceOrderTypeSelected.Current);
        }

        protected virtual void FSSODetPart_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;

            if (fsSODetPartRow.InventoryID != null)
            {
                fsSODetPartRow.LineType = ID.LineType_ServiceTemplate.INVENTORY_ITEM;
            }

            EnableDisable_SODetLine(this, cache, fsSODetPartRow, ServiceOrderTypeSelected.Current, CurrentServiceOrder.Current);

            // Move the old code of SetEnabled and SetPersistingCheck in previous methods to this new generic method
            // keeping the generic convention.
            ServiceOrderAppointmentHandlers.X_RowSelected<FSSODetPart>(
                                            cache,
                                            e,
                                            CurrentServiceOrder.Current,
                                            ServiceOrderTypeSelected.Current,
                                            disableSODetReferenceFields: false,
                                            docAllowsActualFieldEdition: false);
        }

        protected virtual void FSSODetPart_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODetPart fsSODetPartRow = (FSSODetPart)e.Row;

            if (ServiceOrderCore.FSSODetLinkedToAppointments(this, fsSODetPartRow) == true)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.FSSODET_LINKED_TO_APPOINTMENTS, "Inventory Item"), PXErrorLevel.Error);
            }

            if (this.ServiceOrderRecords.Current != null
                    && this.ServiceOrderRecords.Current.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER
                        && e.ExternalCall == true)
            {
                if (IsThisLineRelatedToAsoLine(this.ServiceOrderRecords.Current, fsSODetPartRow))
                {
                    throw new PXException(TX.Error.FSSODET_LINE_IS_RELATED_TO_A_SOLINE, fsSODetPartRow.SourceLineNbr);
                }
            }

            if (fsSODetPartRow.EnablePO == true)
            {
                ServiceOrderRecords.Current.UpdateAppWaitingForParts = true;
            }
        }

        protected virtual void FSSODetPart_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EquipmentHelper.CheckReplaceComponentLines<FSSODetPart, FSSODetPart.equipmentLineRef>(cache, ServiceOrderDetParts.Select(), (FSSODetPart)e.NewRow);
        }

        protected virtual void FSSODetPart_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            MarkHeaderAsUpdated(cache, e.Row);

            ServiceOrderAppointmentHandlers.CheckIfManualPrice<FSSODetPart, FSSODetPart.estimatedQty>(cache, e);
            ServiceOrderAppointmentHandlers.CheckSOIfManualCost(cache, e);

            if (!cache.ObjectsEqual<FSSODet.enablePO>(e.Row, e.OldRow))
            {
                CurrentServiceOrder.Current.UpdateAppWaitingForParts = true;
            }

            TaxSourceLines.Update(CreateTaxRow((FSSODet)e.Row));
        }

        protected virtual void FSSODetService_UOM_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldDefaulting<FSSODetService>(cache, e);
        }
        protected virtual void FSSODetPart_UOM_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldDefaulting<FSSODetPart>(cache, e);
        }
        protected virtual void FSSODetUNION_UOM_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldDefaulting<FSSODetUNION>(cache, e);
        }
        #endregion
        #region FSSOAttendeesEvents
        protected virtual void FSSOAttendee_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOAttendee fsSOAttendeeRow = (FSSOAttendee)e.Row;

            Helper.Current = GetFsSelectorHelperInstance;
            Helper.Current.Mem_int = fsSOAttendeeRow.CustomerID;

            ServiceOrderCore.AttendeeInfo attendeeInfo = ServiceOrderCore.GetAttendeeInfo(this, fsSOAttendeeRow.CustomerID, fsSOAttendeeRow.ContactID);
            fsSOAttendeeRow.Mem_CustomerContactName = attendeeInfo.CustomerContactName;
            fsSOAttendeeRow.Mem_EMail = attendeeInfo.EMail;
            fsSOAttendeeRow.Mem_Phone1 = attendeeInfo.Phone1;
        }

        protected virtual void FSSOAttendee_ContactID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOAttendee fsSOAttendeeRow = (FSSOAttendee)e.Row;

            ServiceOrderCore.AttendeeInfo attendeeInfo = ServiceOrderCore.GetAttendeeInfo(this, fsSOAttendeeRow.CustomerID, fsSOAttendeeRow.ContactID);
            fsSOAttendeeRow.Mem_CustomerContactName = attendeeInfo.CustomerContactName;
            fsSOAttendeeRow.Mem_EMail = attendeeInfo.EMail;
            fsSOAttendeeRow.Mem_Phone1 = attendeeInfo.Phone1;
        }

        protected virtual void FSSOAttendee_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            fsSOAttendee_Inserting = (FSSOAttendee)e.Row;
        }

        protected virtual void FSSOAttendee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOAttendee fsSOAttendeeRow = (FSSOAttendee)e.Row;

            bool enableFields = fsSOAttendee_Inserting == fsSOAttendeeRow;

            PXUIFieldAttribute.SetEnabled<FSSOAttendee.customerID>(cache, fsSOAttendeeRow, enableFields);
            PXUIFieldAttribute.SetEnabled<FSSOAttendee.contactID>(cache, fsSOAttendeeRow, enableFields);

            ServiceOrderCore.AttendeeInfo attendeeInfo = ServiceOrderCore.GetAttendeeInfo(this, fsSOAttendeeRow.CustomerID, fsSOAttendeeRow.ContactID);
            fsSOAttendeeRow.Mem_CustomerContactName = attendeeInfo.CustomerContactName;
            fsSOAttendeeRow.Mem_EMail = attendeeInfo.EMail;
            fsSOAttendeeRow.Mem_Phone1 = attendeeInfo.Phone1;
        }
        #endregion 
        #region FSSOEmployeeEvents

        protected virtual void FSSOEmployee_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOEmployee fsSOEmployeeRow = (FSSOEmployee)e.Row;
            fsSOEmployeeRow.Type = SharedFunctions.GetBAccountType(this, fsSOEmployeeRow.EmployeeID);
        }

        protected virtual void FSSOEmployee_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOEmployee fsSOEmployeeRow = (FSSOEmployee)e.Row;

            if (fsSOEmployeeRow.LineRef == null)
            {
                fsSOEmployeeRow.LineRef = fsSOEmployeeRow.LineNbr.Value.ToString("000");
            }
        }

        protected virtual void FSSOEmployee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOEmployee fsSOEmployeeRow = (FSSOEmployee)e.Row;

            PXUIFieldAttribute.SetEnabled<FSSOEmployee.employeeID>(cache, fsSOEmployeeRow, fsSOEmployeeRow.EmployeeID == null);
            PXUIFieldAttribute.SetEnabled<FSSOEmployee.comment>(cache, fsSOEmployeeRow, fsSOEmployeeRow.EmployeeID != null);
        }

        protected virtual void FSSOEmployee_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            FSSOEmployee fsSOEmployeeRow = (FSSOEmployee)e.Row;

            if (e.Row == null)
            {
                return;
            }
        }

        #endregion
        #region FSSOResourceEvents
        protected virtual void FSSOResource_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOResource fsSOResourceRow = (FSSOResource)e.Row;

            PXUIFieldAttribute.SetEnabled<FSSOResource.SMequipmentID>(cache, fsSOResourceRow, fsSOResourceRow.SMEquipmentID == null);
            PXUIFieldAttribute.SetEnabled<FSSOResource.qty>(cache, fsSOResourceRow, fsSOResourceRow.SMEquipmentID != null);
            PXUIFieldAttribute.SetEnabled<FSSOResource.comment>(cache, fsSOResourceRow, fsSOResourceRow.SMEquipmentID != null);
        }
        #endregion
        #region FSPostDetEvents
        protected virtual void FSPostDet_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            FSPostDet fsPostDetRow = e.Row as FSPostDet;

            if (fsPostDetRow.SOPosted == true)
            {
                using (new PXConnectionScope())
                {
                    var soOrderShipment = (SOOrderShipment)PXSelectReadonly<SOOrderShipment,
                                          Where<
                                              SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
                                          And<
                                              SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>>>>
                                    .Select(cache.Graph, fsPostDetRow.SOOrderNbr, fsPostDetRow.SOOrderType);

                    fsPostDetRow.InvoiceRefNbr = soOrderShipment?.InvoiceNbr;
                    fsPostDetRow.InvoiceDocType = soOrderShipment?.InvoiceType;
                }
            }
            else if (fsPostDetRow.ARPosted == true || fsPostDetRow.SOInvPosted == true)
            {
                fsPostDetRow.InvoiceRefNbr = fsPostDetRow.Mem_DocNbr;
                fsPostDetRow.InvoiceDocType = fsPostDetRow.ARDocType;
            }
        }
        #endregion
        #region ARPaymentEvents
        protected virtual void ARPayment_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ARPayment arPaymentRow = (ARPayment)e.Row;

            using (new PXConnectionScope())
            {
                ServiceOrderCore.RecalcSOApplAmounts(this, arPaymentRow);
            }
        }
        #endregion
        #endregion

        protected virtual void MarkHeaderAsUpdated(PXCache cache, object row)
        {
            if (row == null)
            {
                return;
            }

            if (CurrentServiceOrder.Cache.GetStatus(CurrentServiceOrder.Current) == PXEntryStatus.Notchanged)
            {
                CurrentServiceOrder.Cache.SetStatus(CurrentServiceOrder.Current, PXEntryStatus.Updated);
            }
        }

        public static decimal GetCuryDocTotal(decimal? curyLineTotal, decimal? curyDiscTotal,
                                                decimal? curyTaxTotal, decimal? curyInclTaxTotal)
        {
            return (curyLineTotal ?? 0) - (curyDiscTotal ?? 0) + (curyTaxTotal ?? 0) - (curyInclTaxTotal ?? 0);
        }

        #region Selector Methods

        #region Staff Selector
        [PXCopyPasteHiddenView]
        public PXFilter<StaffSelectionFilter> StaffSelectorFilter;

        #region CacheAttached
        #region ServiceLineRef
        [PXDefault]
        [PXString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Line Ref.")]
        [FSSelectorServiceOrderSODetID]
        protected virtual void StaffSelectionFilter_ServiceLineRef_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        [PXCopyPasteHiddenView]
        public StaffSelectionHelper.SkillRecords_View SkillGridFilter;
        [PXCopyPasteHiddenView]
        public StaffSelectionHelper.LicenseTypeRecords_View LicenseTypeGridFilter;
        [PXCopyPasteHiddenView]
        public StaffSelectionHelper.StaffRecords_View StaffRecords;

        public IEnumerable skillGridFilter()
        {
            return StaffSelectionHelper.SkillFilterDelegate(this, ServiceOrderDetServices, StaffSelectorFilter, SkillGridFilter);
        }

        public IEnumerable licenseTypeGridFilter()
        {
            return StaffSelectionHelper.LicenseTypeFilterDelegate(this, ServiceOrderDetServices, StaffSelectorFilter, LicenseTypeGridFilter);
        }

        protected virtual IEnumerable staffRecords()
        {
            return StaffSelectionHelper.StaffRecordsDelegate(
                                                                (object)ServiceOrderEmployees,
                                                                SkillGridFilter,
                                                                LicenseTypeGridFilter,
                                                                StaffSelectorFilter);
        }

        protected virtual void StaffSelectionFilter_ServiceLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SkillGridFilter.Cache.Clear();
            LicenseTypeGridFilter.Cache.Clear();
            StaffRecords.Cache.Clear();
        }

        protected virtual void BAccountStaffMember_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            BAccountStaffMember bAccountStaffMemberRow = (BAccountStaffMember)e.Row;

            if (StaffSelectorFilter.Current != null)
            {
                if (bAccountStaffMemberRow.Selected == true)
                {
                    FSSOEmployee fsSOEmployeeRow = new FSSOEmployee()
                    {
                        EmployeeID = bAccountStaffMemberRow.BAccountID,
                        ServiceLineRef = StaffSelectorFilter.Current.ServiceLineRef
                    };
                    ServiceOrderEmployees.Insert(fsSOEmployeeRow);
                }
                else
                {
                    FSSOEmployee fsSOEmployeeRow = PXSelect<FSSOEmployee,
                                                    Where2<
                                                        Where<
                                                            FSSOEmployee.serviceLineRef, Equal<Required<FSSOEmployee.serviceLineRef>>,
                                                            Or<
                                                                Where<
                                                                    FSSOEmployee.serviceLineRef, IsNull,
                                                                    And<Required<FSSOEmployee.serviceLineRef>, IsNull>>>>,
                                                        And<
                                                            Where<
                                                                FSSOEmployee.sOID, Equal<Current<FSServiceOrder.sOID>>,
                                                                And<FSSOEmployee.employeeID, Equal<Required<FSSOEmployee.employeeID>>>>>>>
                                                    .Select(cache.Graph, StaffSelectorFilter.Current.ServiceLineRef, StaffSelectorFilter.Current.ServiceLineRef, bAccountStaffMemberRow.BAccountID);

                    fsSOEmployeeRow = (FSSOEmployee)ServiceOrderEmployees.Cache.Locate(fsSOEmployeeRow);
                    if (fsSOEmployeeRow != null)
                    {
                        ServiceOrderEmployees.Delete(fsSOEmployeeRow);
                    }
                }
            }

            StaffRecords.View.RequestRefresh();
        }

        #region OpenStaffSelectorFromServiceTab
        public PXAction<FSServiceOrder> openStaffSelectorFromServiceTab;
        [PXButton]
        [PXUIField(DisplayName = "Staff Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenStaffSelectorFromServiceTab()
        {
            ServiceOrder_Contact.Current = ServiceOrder_Contact.SelectSingle();
            ServiceOrder_Address.Current = ServiceOrder_Address.SelectSingle();
            
            if (ServiceOrder_Address.Current != null)
            {
                StaffSelectorFilter.Current.PostalCode = ServiceOrder_Address.Current.PostalCode;
                StaffSelectorFilter.Current.ProjectID = CurrentServiceOrder.Current.ProjectID;

                StaffSelectorFilter.Current.ScheduledDateTimeBegin = CurrentServiceOrder.Current.PromisedDate;
            }

            FSSODetService serviceRow = ServiceOrderDetServices.Current;

            if (serviceRow != null && serviceRow.LineType == ID.LineType_All.SERVICE
                && serviceRow.SODetID >= 0)
            {
                StaffSelectorFilter.Current.ServiceLineRef = ServiceOrderDetServices.Current.LineRef;
            }
            else
            {
                StaffSelectorFilter.Current.ServiceLineRef = null;
            }

            SkillGridFilter.Cache.Clear();
            LicenseTypeGridFilter.Cache.Clear();
            StaffRecords.Cache.Clear();

            StaffSelectionHelper srvOrdStaffSelector = new StaffSelectionHelper();
            srvOrdStaffSelector.LaunchStaffSelector(this, StaffSelectorFilter);
        }
        #endregion

        #region OpenStaffSelectorFromStaffTab
        public PXAction<FSServiceOrder> openStaffSelectorFromStaffTab;
        [PXButton]
        [PXUIField(DisplayName = "Staff Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenStaffSelectorFromStaffTab()
        {
            ServiceOrder_Contact.Current = ServiceOrder_Contact.SelectSingle();
            ServiceOrder_Address.Current = ServiceOrder_Address.SelectSingle();

            if (ServiceOrder_Address.Current != null)
            {
                StaffSelectorFilter.Current.PostalCode = ServiceOrder_Address.Current.PostalCode;
                StaffSelectorFilter.Current.ScheduledDateTimeBegin = CurrentServiceOrder.Current.PromisedDate;
                StaffSelectorFilter.Current.ProjectID = CurrentServiceOrder.Current.ProjectID;
            }

            StaffSelectorFilter.Current.ServiceLineRef = null;

            SkillGridFilter.Cache.Clear();
            LicenseTypeGridFilter.Cache.Clear();
            StaffRecords.Cache.Clear();

            StaffSelectionHelper srvOrdStaffSelector = new StaffSelectionHelper();
            srvOrdStaffSelector.LaunchStaffSelector(this, StaffSelectorFilter);
        }
        #endregion
        #endregion

        #region Service Selector

        [PXCopyPasteHiddenView]
        public PXFilter<ServiceSelectionFilter> ServiceSelectorFilter;
        [PXCopyPasteHiddenView]
        public PXSelect<EmployeeGridFilter> EmployeeGridFilter;
        [PXCopyPasteHiddenView]
        public ServiceSelectionHelper.ServiceRecords_View ServiceRecords;

        protected virtual IEnumerable employeeGridFilter()
        {
            return ServiceSelectionHelper.EmployeeRecordsDelegate((object)ServiceOrderEmployees, EmployeeGridFilter);
        }

        protected virtual IEnumerable serviceRecords()
        {
            return ServiceSelectionHelper.ServiceRecordsDelegate(
                                                                ServiceOrderDetServices,
                                                                EmployeeGridFilter,
                                                                ServiceSelectorFilter);
        }

        #region OpenServicesSelector
        public PXAction<FSServiceOrder> openServiceSelector;
        [PXButton]
        [PXUIField(DisplayName = "Service Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenServiceSelector()
        {
            ServiceSelectionHelper.OpenServiceSelector<FSServiceOrder.orderDate>(
                                                    CurrentServiceOrder.Cache,
                                                    ServiceSelectorFilter,
                                                    EmployeeGridFilter);
        }
        #endregion

        #region SelectCurrentService
        public PXAction<FSServiceOrder> selectCurrentService;
        [PXUIField(DisplayName = "Select Current Service", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void SelectCurrentService()
        {
            ServiceSelectionHelper.InsertCurrentService<FSSODetService>(ServiceRecords, ServiceOrderDetServices);
        }
        #endregion

        #endregion

        #endregion

        #region Extensions
        #region QuickProcess
        public PXQuickProcess.Action<FSServiceOrder>.ConfiguredBy<FSSrvOrdQuickProcessParams> quickProcess;

        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Quick Process")]
        protected virtual IEnumerable QuickProcess(PXAdapter adapter) => ServiceOrderQuickProcessExt.ButtonHandler(adapter);

        public ServiceOrderQuickProcess ServiceOrderQuickProcessExt => GetExtension<ServiceOrderQuickProcess>();

        public class ServiceOrderQuickProcess : PXGraphExtension<ServiceOrderEntry>
        {
            public PXSelect<SOOrderTypeQuickProcess, Where<SOOrderTypeQuickProcess.orderType, Equal<Current<FSSrvOrdType.postOrderType>>>> currentSOOrderType;
            public static bool isSOInvoice;

            public virtual IEnumerable<FSServiceOrder> ButtonHandler(PXAdapter adapter)
            {
                QuickProcessParameters.AskExt(InitQuickProcessPanel);

                if (Base.ServiceOrderRecords.AllowUpdate == true)
                {
                    Base.Save.Press();
                }

                PXQuickProcess.Start(Base, Base.ServiceOrderRecords.Current, QuickProcessParameters.Current);

                return new[] { Base.ServiceOrderRecords.Current };
            }

            public PXAction<FSServiceOrder> quickProcessOk;
            [PXButton, PXUIField(DisplayName = "OK")]
            public virtual IEnumerable QuickProcessOk(PXAdapter adapter)
            {
                Base.ServiceOrderRecords.Current.IsCalledFromQuickProcess = true;
                return adapter.Get();
            }

            public PXFilter<FSSrvOrdQuickProcessParams> QuickProcessParameters;

            protected virtual void FSServiceOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
                bool enableQuickProcess = (Base.ServiceOrderTypeSelected.Current == null ? false : Base.ServiceOrderTypeSelected.Current.AllowQuickProcess.Value)
                                            && fsServiceOrderRow.PendingAPARSOPost.Value
                                                && Base.BillingCycleRelated.Current?.BillingBy == ID.Billing_By.SERVICE_ORDER
                                                    && fsServiceOrderRow.Hold == false
                                                        && fsServiceOrderRow.Status != ID.Status_ServiceOrder.CANCELED;

                if (currentSOOrderType.Current == null)
                {
                    currentSOOrderType.Current = currentSOOrderType.Select();
                }

                isSOInvoice = Base.ServiceOrderTypeSelected.Current?.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;

                Base.quickProcess.SetEnabled(enableQuickProcess);
                Base.quickProcess.SetVisible(enableQuickProcess);
            }

            protected virtual void FSSrvOrdQuickProcessParams_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                quickProcessOk.SetEnabled(true);

                FSSrvOrdQuickProcessParams fsQuickProcessParametersRow = (FSSrvOrdQuickProcessParams)e.Row;
                SetQuickProcessSettingsVisibility(cache, Base.ServiceOrderTypeSelected.Current, Base.ServiceOrderRecords.Current, fsQuickProcessParametersRow);

                if (fsQuickProcessParametersRow.GenerateInvoiceFromServiceOrder == true && isSOInvoice)
                {
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdQuickProcessParams.releaseInvoice>(cache, fsQuickProcessParametersRow, true);
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdQuickProcessParams.emailInvoice>(cache, fsQuickProcessParametersRow, true);
                }
                else if (fsQuickProcessParametersRow.GenerateInvoiceFromServiceOrder == false && isSOInvoice)
                {
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdQuickProcessParams.generateInvoiceFromServiceOrder>(cache, fsQuickProcessParametersRow, true);
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdQuickProcessParams.releaseInvoice>(cache, fsQuickProcessParametersRow, false);
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdQuickProcessParams.emailInvoice>(cache, fsQuickProcessParametersRow, false);
                }
            }

            protected virtual void FSSrvOrdQuickProcessParams_GenerateInvoiceFromAppointment_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsRow = (FSSrvOrdQuickProcessParams)e.Row;

                if (isSOInvoice
                        && fsSrvOrdQuickProcessParamsRow.GenerateInvoiceFromServiceOrder == true
                            && fsSrvOrdQuickProcessParamsRow.GenerateInvoiceFromServiceOrder != (bool)e.OldValue)
                {
                    fsSrvOrdQuickProcessParamsRow.PrepareInvoice = true;
                }
            }

            private void SetQuickProcessSettingsVisibility(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow, FSSrvOrdQuickProcessParams fsQuickProcessParametersRow)
            {
                if (fsSrvOrdTypeRow == null
                        ||  fsServiceOrderRow == null)
                {
                    return;
                }

                    bool isInvoiceBehavior = false;
                    bool orderTypeQuickProcessIsEnabled = false;
                    bool postToSO = fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE;
                    bool postToSOInvoice = fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;

                    bool enableSOQuickProcess = postToSO;

                    if (postToSO && currentSOOrderType.Current?.AllowQuickProcess != null)
                    {
                        isInvoiceBehavior = currentSOOrderType.Current.Behavior == SOBehavior.IN;
                        orderTypeQuickProcessIsEnabled = (bool)currentSOOrderType.Current.AllowQuickProcess;
                    }
                    else if (postToSOInvoice)
                    {
                        isInvoiceBehavior = true;
                    }

                    enableSOQuickProcess = orderTypeQuickProcessIsEnabled
                                                && fsQuickProcessParametersRow.GenerateInvoiceFromServiceOrder == true
                                                    && (fsQuickProcessParametersRow.PrepareInvoice == false || fsQuickProcessParametersRow.SOQuickProcess == true);

                    PXUIFieldAttribute.SetVisible<FSSrvOrdQuickProcessParams.sOQuickProcess>(cache, fsQuickProcessParametersRow, postToSO && orderTypeQuickProcessIsEnabled);
                    PXUIFieldAttribute.SetVisible<FSSrvOrdQuickProcessParams.emailSalesOrder>(cache, fsQuickProcessParametersRow, postToSO);
                    PXUIFieldAttribute.SetVisible<FSSrvOrdQuickProcessParams.prepareInvoice>(cache, fsQuickProcessParametersRow, postToSO && isInvoiceBehavior && fsQuickProcessParametersRow.SOQuickProcess == false);
                    PXUIFieldAttribute.SetVisible<FSSrvOrdQuickProcessParams.releaseInvoice>(cache, fsQuickProcessParametersRow, (postToSO || postToSOInvoice) && isInvoiceBehavior && fsQuickProcessParametersRow.SOQuickProcess == false);
                    PXUIFieldAttribute.SetVisible<FSSrvOrdQuickProcessParams.emailInvoice>(cache, fsQuickProcessParametersRow, (postToSO || postToSOInvoice) && isInvoiceBehavior && fsQuickProcessParametersRow.SOQuickProcess == false);
                    PXUIFieldAttribute.SetVisible<FSSrvOrdQuickProcessParams.allowInvoiceServiceOrder>(cache, fsQuickProcessParametersRow, postToSO && fsServiceOrderRow.AllowInvoice == false);

                    PXUIFieldAttribute.SetEnabled<FSSrvOrdQuickProcessParams.sOQuickProcess>(cache, fsQuickProcessParametersRow, enableSOQuickProcess);

                    if (fsQuickProcessParametersRow.ReleaseInvoice == false
                        && fsQuickProcessParametersRow.EmailInvoice == false
                            && fsQuickProcessParametersRow.SOQuickProcess == false
                                && fsQuickProcessParametersRow.GenerateInvoiceFromServiceOrder == true)
                    {
                        PXUIFieldAttribute.SetEnabled<FSSrvOrdQuickProcessParams.prepareInvoice>(cache, fsQuickProcessParametersRow, true);
                    }
                }

            private static string[] GetExcludeFiels()
            {
                string[] excludeFields = {
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.allowInvoiceServiceOrder>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.completeServiceOrder>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.closeServiceOrder>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.generateInvoiceFromServiceOrder>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.sOQuickProcess>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.emailSalesOrder>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.srvOrdType>()
                };

                return excludeFields;
            }

            private static void SetQuickProcessOptions(PXGraph graph, PXCache targetCache, FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsRow, bool ignoreUpdateSOQuickProcess)
            {
                var ext = ((ServiceOrderEntry)graph).ServiceOrderQuickProcessExt;

                if (string.IsNullOrEmpty(ext.QuickProcessParameters.Current.OrderType))
                {
                    ext.QuickProcessParameters.Cache.Clear();
                    ResetSalesOrderQuickProcessValues(ext.QuickProcessParameters.Current);
                }

                if (fsSrvOrdQuickProcessParamsRow != null)
                {
                    ResetSalesOrderQuickProcessValues(fsSrvOrdQuickProcessParamsRow);
                }

                FSQuickProcessParameters fsQuickProcessParametersRow =
                    PXSelectReadonly<
                        FSQuickProcessParameters,
                    Where<
                        FSQuickProcessParameters.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>>
                    .Select(ext.Base);

                bool isSOQuickProcess = (fsSrvOrdQuickProcessParamsRow != null && fsSrvOrdQuickProcessParamsRow.SOQuickProcess == true)
                                            || (fsSrvOrdQuickProcessParamsRow == null && fsQuickProcessParametersRow?.SOQuickProcess == true);

                var cache = targetCache ?? ext.QuickProcessParameters.Cache;
                var row = fsSrvOrdQuickProcessParamsRow ?? ext.QuickProcessParameters.Current;

                if (ext.currentSOOrderType.Current?.AllowQuickProcess == true && isSOQuickProcess)
                {
                    var cacheFSSrvOrdQuickProcessParams = new PXCache<FSSrvOrdQuickProcessParams>(ext.Base);

                    FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsFromDB =
                        PXSelectReadonly<FSSrvOrdQuickProcessParams,
                        Where<
                            FSSrvOrdQuickProcessParams.orderType, Equal<Current<FSSrvOrdType.postOrderType>>>>
                        .Select(ext.Base);

                    SharedFunctions.CopyCommonFields(cache,
                                                     row,
                                                     cacheFSSrvOrdQuickProcessParams,
                                                     fsSrvOrdQuickProcessParamsFromDB,
                                                     GetExcludeFiels());
                }
                else
                {
                    SetCommonValues(row, fsQuickProcessParametersRow);
                }

                if (ignoreUpdateSOQuickProcess == false)
                {
                    SetServiceOrderTypeValues(row, fsQuickProcessParametersRow);
                }
            }

            private static void InitQuickProcessPanel(PXGraph graph, string viewName)
            {
                SetQuickProcessOptions(graph, null, null, false);
            }

            protected virtual void FSSrvOrdQuickProcessParams_SOQuickProcess_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsRow = (FSSrvOrdQuickProcessParams)e.Row;

                if (fsSrvOrdQuickProcessParamsRow.SOQuickProcess != (bool)e.OldValue)
                {
                    SetQuickProcessOptions(Base, cache, fsSrvOrdQuickProcessParamsRow, true);
                }
            }

            private static void ResetSalesOrderQuickProcessValues(FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsRow)
            {
                fsSrvOrdQuickProcessParamsRow.CreateShipment = false;
                fsSrvOrdQuickProcessParamsRow.ConfirmShipment = false;
                fsSrvOrdQuickProcessParamsRow.UpdateIN = false;
                fsSrvOrdQuickProcessParamsRow.PrepareInvoiceFromShipment = false;
                fsSrvOrdQuickProcessParamsRow.PrepareInvoice = false;
                fsSrvOrdQuickProcessParamsRow.EmailInvoice = false;
                fsSrvOrdQuickProcessParamsRow.ReleaseInvoice = false;
                fsSrvOrdQuickProcessParamsRow.AutoRedirect = false;
                fsSrvOrdQuickProcessParamsRow.AutoDownloadReports = false;
            }

            private static void SetCommonValues(FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsRowTarget, FSQuickProcessParameters FSQuickProcessParametersRowSource)
            {
                if (isSOInvoice && fsSrvOrdQuickProcessParamsRowTarget.GenerateInvoiceFromServiceOrder == true)
                {
                    fsSrvOrdQuickProcessParamsRowTarget.PrepareInvoice = true;
                }
                else
                {
                    fsSrvOrdQuickProcessParamsRowTarget.PrepareInvoice = FSQuickProcessParametersRowSource.GenerateInvoiceFromAppointment.Value ? FSQuickProcessParametersRowSource.PrepareInvoice : false;
                }

                fsSrvOrdQuickProcessParamsRowTarget.ReleaseInvoice = FSQuickProcessParametersRowSource.GenerateInvoiceFromServiceOrder.Value ? FSQuickProcessParametersRowSource.ReleaseInvoice : false;
                fsSrvOrdQuickProcessParamsRowTarget.EmailInvoice = FSQuickProcessParametersRowSource.GenerateInvoiceFromServiceOrder.Value ? FSQuickProcessParametersRowSource.EmailInvoice : false;
            }

            private static void SetServiceOrderTypeValues(FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsRowTarget, FSQuickProcessParameters FSQuickProcessParametersRowSource)
            {
                fsSrvOrdQuickProcessParamsRowTarget.AllowInvoiceServiceOrder = FSQuickProcessParametersRowSource.AllowInvoiceServiceOrder;
                fsSrvOrdQuickProcessParamsRowTarget.CompleteServiceOrder = FSQuickProcessParametersRowSource.CompleteServiceOrder;
                fsSrvOrdQuickProcessParamsRowTarget.CloseServiceOrder = FSQuickProcessParametersRowSource.CloseServiceOrder;
                fsSrvOrdQuickProcessParamsRowTarget.GenerateInvoiceFromServiceOrder = FSQuickProcessParametersRowSource.GenerateInvoiceFromServiceOrder;
                fsSrvOrdQuickProcessParamsRowTarget.SrvOrdType = FSQuickProcessParametersRowSource.SrvOrdType;
                fsSrvOrdQuickProcessParamsRowTarget.SOQuickProcess = FSQuickProcessParametersRowSource.SOQuickProcess;
                fsSrvOrdQuickProcessParamsRowTarget.EmailSalesOrder = FSQuickProcessParametersRowSource.GenerateInvoiceFromServiceOrder.Value ? FSQuickProcessParametersRowSource.EmailSalesOrder : false;
            }
        }
        #endregion

        #region MultiCurrency
        public class MultiCurrency : SMMultiCurrencyGraph<ServiceOrderEntry, FSServiceOrder>
        {
			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.ServiceOrderRecords,
					Base.ServiceOrderDetServices,
					Base.ServiceOrderDetParts
				};
			}

			protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(FSServiceOrder))
                {
                    BAccountID = typeof(FSServiceOrder.billCustomerID),
                    DocumentDate = typeof(FSServiceOrder.orderDate)
                };
            }
        }
        #endregion

        #region Sales Taxes
        public class SalesTax : TaxGraph<ServiceOrderEntry, FSServiceOrder>
        {
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(FSServiceOrder))
                {
                    DocumentDate = typeof(FSServiceOrder.orderDate),
                    CuryDocBal = typeof(FSServiceOrder.curyDocTotal),
                    CuryDiscountLineTotal = typeof(FSServiceOrder.curyDiscTot),
                    CuryDiscTot = typeof(FSServiceOrder.curyDiscTot),
                    BranchID = typeof(FSServiceOrder.branchID),
                    FinPeriodID = typeof(FSServiceOrder.finPeriodID),
                    TaxZoneID = typeof(FSServiceOrder.taxZoneID),
                    CuryLinetotal = typeof(FSServiceOrder.curyBillableOrderTotal),
                    CuryTaxTotal = typeof(FSServiceOrder.curyTaxTotal)
                };
            }

            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(FSSODetUNION))
                {
                    CuryTranAmt = typeof(FSSODetUNION.curyBillableTranAmt),
                    TaxCategoryID = typeof(FSSODetUNION.taxCategoryID),
                    DocumentDiscountRate = typeof(FSSODetUNION.documentDiscountRate),
                    GroupDiscountRate = typeof(FSSODetUNION.groupDiscountRate)
                };
            }

            protected override void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
            {
                TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
                if (taxCalc == TaxCalc.Calc || taxCalc == TaxCalc.ManualLineCalc)
                {
                    if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
                    {
                        if(Base.ServiceOrderDetServices.SelectSingle() != null 
                            || Base.ServiceOrderDetParts.SelectSingle() != null)
                        {
                            CalcTaxes(null);
                        }
                    }
                }
            }

            protected override TaxDetailMapping GetTaxDetailMapping()
            {
                return new TaxDetailMapping(typeof(FSServiceOrderTax), typeof(FSServiceOrderTax.taxID));
            }

            protected override TaxTotalMapping GetTaxTotalMapping()
            {
                return new TaxTotalMapping(typeof(FSServiceOrderTaxTran), typeof(FSServiceOrderTaxTran.taxID));
            }

            protected virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                if(e.Row == null)
                {
                    return;
                }

                var row = sender.GetExtension<Extensions.SalesTax.Document>(e.Row);
                if (row.TaxCalc == null)
                    row.TaxCalc = TaxCalc.Calc;
            }

            protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
            {
                base.CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
                FSServiceOrder doc = (FSServiceOrder)this.Documents.Cache.GetMain<Extensions.SalesTax.Document>(this.Documents.Current);

                decimal CuryLineTotal = (decimal)(ParentGetValue<FSServiceOrder.curyBillableOrderTotal>() ?? 0m);
                //decimal CuryDiscAmtTotal = (decimal)(ParentGetValue<FSServiceOrder.curyLineDiscountTotal>() ?? 0m);
                decimal CuryDiscTotal = (decimal)(ParentGetValue<FSServiceOrder.curyDiscTot>() ?? 0m);

                decimal CuryDocTotal = GetCuryDocTotal(CuryLineTotal, CuryDiscTotal,
                                                CuryTaxTotal, CuryInclTaxTotal);

                if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<FSServiceOrder.curyDocTotal>() ?? 0m)) == false)
                {
                    ParentSetValue<FSServiceOrder.curyDocTotal>(CuryDocTotal);
                }
            }

            protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
            {
                Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
                var currents = new[]
                {
                    row != null && row is Extensions.SalesTax.Detail ? Details.Cache.GetMain((Extensions.SalesTax.Detail)row):null,
                    ((ServiceOrderEntry)graph).CurrentServiceOrder.Current
                };

                foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                    LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
                        And<TaxRev.outdated, Equal<False>,
                            And<TaxRev.taxType, Equal<TaxType.sales>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                            And<Tax.taxType, NotEqual<CSTaxType.use>,
                            And<Tax.reverseTax, Equal<False>,
                            And<Current<FSServiceOrder.orderDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
                    Where>
                    .SelectMultiBound(graph, currents, parameters))
                {
                    tail[((Tax)record).TaxID] = record;
                }
                List<object> ret = new List<object>();
                switch (taxchk)
                {
                    case PXTaxCheck.Line:
                        foreach (FSServiceOrderTax record in PXSelect<FSServiceOrderTax,
                            Where<FSServiceOrderTax.entityID, Equal<Current<FSServiceOrder.sOID>>,
                                And<FSServiceOrderTax.lineNbr, Equal<Current<FSSODetUNION.lineNbr>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            PXResult<Tax, TaxRev> line;
                            if (tail.TryGetValue(record.TaxID, out line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && String.Compare(((Tax)(PXResult<FSServiceOrderTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                    idx--) ;
                                ret.Insert(idx, new PXResult<FSServiceOrderTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcLine:
                        foreach (FSServiceOrderTax record in PXSelect<FSServiceOrderTax,
                            Where<FSServiceOrderTax.entityID, Equal<Current<FSServiceOrder.sOID>>,
                                And<FSServiceOrderTax.lineNbr, Less<intMax>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            PXResult<Tax, TaxRev> line;
                            if (tail.TryGetValue(record.TaxID, out line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && ((FSServiceOrderTax)(PXResult<FSServiceOrderTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
                                    && String.Compare(((Tax)(PXResult<FSServiceOrderTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                    idx--) ;
                                ret.Insert(idx, new PXResult<FSServiceOrderTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcTotals:
                        foreach (FSServiceOrderTaxTran record in PXSelect<FSServiceOrderTaxTran,
                            Where<FSServiceOrderTaxTran.entityID, Equal<Current<FSServiceOrder.sOID>>>,
                            OrderBy<Asc<FSServiceOrderTaxTran.entityID, Asc<FSServiceOrderTaxTran.lineNbr, Asc<FSServiceOrderTaxTran.taxID>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            PXResult<Tax, TaxRev> line;
                            if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && String.Compare(((Tax)(PXResult<FSServiceOrderTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                    idx--) ;
                                ret.Insert(idx, new PXResult<FSServiceOrderTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                            }
                        }
                        return ret;
                    default:
                        return ret;
                }
            }

            #region FSServiceOrderTaxTran
            protected virtual void FSServiceOrderTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                    return;

                PXUIFieldAttribute.SetEnabled<FSServiceOrderTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
            }
            #endregion

            #region FSServiceOrderTax
            protected virtual void FSServiceOrderTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
            {
                FSServiceOrderTaxTran row = e.Row as FSServiceOrderTaxTran;
                if (row == null) return;

                if (e.Operation == PXDBOperation.Delete)
                {
                    FSServiceOrderTax tax = (FSServiceOrderTax)Base.TaxLines.Cache.Locate(FindFSServiceOrderTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Deleted ||
                         Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.InsertedDeleted)
                        e.Cancel = true;
                }
                if (e.Operation == PXDBOperation.Update)
                {
                    FSServiceOrderTax tax = (FSServiceOrderTax)Base.TaxLines.Cache.Locate(FindFSServiceOrderTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Updated)
                        e.Cancel = true;
                }
            }

            internal static FSServiceOrderTax FindFSServiceOrderTax(FSServiceOrderTaxTran tran)
            {
                var list = PXSelect<FSServiceOrderTax,
                    Where<FSServiceOrderTax.entityID, Equal<Required<FSServiceOrderTax.entityID>>,
                        And<FSServiceOrderTax.lineNbr, Equal<Required<FSServiceOrderTax.lineNbr>>,
                        And<FSServiceOrderTax.taxID, Equal<Required<FSServiceOrderTax.taxID>>>>>>
                        .SelectSingleBound(new PXGraph(), new object[] { }).RowCast<FSServiceOrderTax>();
                return list.FirstOrDefault();
            }
            #endregion

            #region FSServiceOrder
            protected virtual void FSServiceOrder_taxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
            {
                var row = e.Row as FSServiceOrder;
                if (row == null) return;

                var customerLocation = (Location)PXSelect<Location,
                    Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                        And<Location.locationID, Equal<Required<Location.locationID>>>>>.
                    Select(sender.Graph, row.BillCustomerID, row.BillLocationID);
                if (customerLocation != null)
                {
                    if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
                    {
                        e.NewValue = customerLocation.CTaxZoneID;
                    }
                    else
                    {
                        var address = (Address)PXSelect<Address,
                            Where<Address.addressID, Equal<Required<Address.addressID>>>>.
                            Select(sender.Graph, customerLocation.DefAddressID);
                        if (address != null && !string.IsNullOrEmpty(address.PostalCode))
                        {
                            e.NewValue = TaxBuilderEngine.GetTaxZoneByZip(sender.Graph, address.PostalCode);
                        }
                    }
                }
                if (e.NewValue == null)
                {
                    var branchLocationResult = (PXResult<Branch, BAccount, Location>)PXSelectJoin<Branch,
                            InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Branch.bAccountID>>,
                            InnerJoin<Location, On<Location.locationID, Equal<BAccount.defLocationID>>>>,
                        Where<
                            Branch.branchID, Equal<Required<Branch.branchID>>>>
                        .Select(sender.Graph, row.BranchID);

                    var branchLocation = (Location)branchLocationResult;

                    if (branchLocation != null && branchLocation.VTaxZoneID != null)
                        e.NewValue = branchLocation.VTaxZoneID;
                    else
                        e.NewValue = row.TaxZoneID;
                }
            }
            #endregion
        }
        #endregion

        #region Contact/Address
        public class ContactAddress : SrvOrdContactAddressGraph<ServiceOrderEntry>
        {
        }
        #endregion

        #region Service Registration
        public class ServiceRegistration : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.ActivateOnApplicationStart<ExtensionSorting>();
            }

            private class ExtensionSorting
            {
                private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
                {
                    {typeof(ContactAddress), 1},
                    {typeof(MultiCurrency), 2},
                    /*{typeof(SalesPrice), 3},
                    {typeof(Discount), 4},*/
                    {typeof(SalesTax), 5},
                };

                public ExtensionSorting()
                {
                    PXBuildManager.SortExtensions += StableSort;
                }

                private static void StableSort(List<Type> list)
                    {
					PXBuildManager.PartialSort(list, _order);
                }
            }
        }
        #endregion
        #endregion
    }
}