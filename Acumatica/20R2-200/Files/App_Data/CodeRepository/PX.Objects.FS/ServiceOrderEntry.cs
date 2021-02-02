using Autofac;
using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.FS.ParallelProcessing;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.PM;
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
        public bool SkipTaxCalcTotals = false;

        public bool ForceAppointmentCheckings = false;
        public bool DisableServiceOrderUnboundFieldCalc = false;
        public List<PXResult<FSAppointmentDet>> LastRefAppointmentDetails = null;

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

        public void SaveBeforeApplyAction(PXCache cache, FSServiceOrder row)
        {
            PXEntryStatus rowEntrySTatus = cache.GetStatus(row);
            bool isInserted = cache.AllowInsert == true && rowEntrySTatus == PXEntryStatus.Inserted;
            bool isUpdated = cache.AllowUpdate == true && rowEntrySTatus == PXEntryStatus.Updated;

            if (isInserted || isUpdated)
            {
                this.SkipTaxCalcAndSave();
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
            actionsMenu.AddMenuAction(allowBilling);
            actionsMenu.AddMenuAction(scheduleAppointment);
            actionsMenu.AddMenuAction(validateAddress);
            actionsMenu.AddMenuAction(openEmployeeBoard);
            actionsMenu.AddMenuAction(openUserCalendar);

            if (fsSetupRow != null && fsSetupRow.ManageRooms == true)
            {
                actionsMenu.AddMenuAction(OpenRoomBoard);
            }

            report.AddMenuAction(printServiceOrder);
            report.AddMenuAction(serviceOrderAppointmentsReport);
            report.AddMenuAction(printServiceTimeActivityReport);

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

        public virtual void ClearTaxes(FSServiceOrder serviceOrderRow)
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

            ServiceOrderDetails.Cache.Clear();
            ServiceOrderDetails.View.Clear();
            ServiceOrderDetails.View.RequestRefresh();
        }
        #endregion

        #region Private Members
        
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

        private ExpenseClaimDetailEntry _expenseClaimDetailGraph;

        protected ExpenseClaimDetailEntry GetExpenseClaimDetailGraph(bool clearGraph)
        {
            if (_expenseClaimDetailGraph == null)
            {
                _expenseClaimDetailGraph = PXGraph.CreateInstance<ExpenseClaimDetailEntry>();
            }
            else if (clearGraph == true)
            {
                _expenseClaimDetailGraph.Clear();
            }

            return _expenseClaimDetailGraph;
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
        [PXUIField(DisplayName = "Account Name", Enabled = false)]
        protected virtual void BAccount_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PlanID
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [FSSODetSplitPlanID(typeof(FSServiceOrder.noteID), typeof(FSServiceOrder.hold), typeof(FSServiceOrder.orderDate))]
        protected virtual void FSSODetSplit_PlanID_CacheAttached(PXCache sender)
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
        #region FSSODet CacheAttached
        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSSODet_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointmentDet CacheAttached
        [PXDBDate]
        protected virtual void _(Events.CacheAttached<FSAppointmentDet.tranDate> e){ }
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

        [PXViewName(CR.Messages.Answers)]
        public FSAttributeList<FSServiceOrder> Answers;

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
               Where<
                   SOOrderType.orderType, Equal<Current<FSSrvOrdType.postOrderType>>>> postSOOrderTypeSelected;

        public PXSetup<SOOrderType>.Where<
               Where<
                   SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>> AllocationSOOrderTypeSelected;

        [PXHidden]
        public PXSetup<Customer>.Where<
               Where<
                   Customer.bAccountID, Equal<Optional<FSServiceOrder.billCustomerID>>>> BillCustomer;

        [PXHidden]
        public PXSelect<CurrencyInfo,
               Where<
                   CurrencyInfo.curyInfoID, Equal<Current<FSServiceOrder.curyInfoID>>>> currencyInfoView;

        [PXFilterable]
        [PXImport(typeof(FSServiceOrder))]
        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_DETAILS)]
        public ServiceOrderCore.ServiceOrderDetailsOrdered ServiceOrderDetails;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_APPOINTMENTS)]
        [PXCopyPasteHiddenView]
        public ServiceOrderCore.ServiceOrderAppointments_View ServiceOrderAppointments;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_EMPLOYEES)]
        public ServiceOrderCore.ServiceOrderEmployees_View ServiceOrderEmployees;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_EQUIPMENT)]
        public ServiceOrderCore.ServiceOrderEquipment_View ServiceOrderEquipment;

        public PXSelect<INSiteStatus> sitestatusview_dummy;
        public PXSelect<SiteStatus> sitestatusview;
        public PXSelect<INItemSite> initemsite;

        [PXCopyPasteHiddenView()]
        public PlanningHelper<FSSODetSplit> Planning;

        [PXCopyPasteHiddenView()]
        [PXFilterable]
        public PXSelect<FSSODetSplit,
               Where<
                   FSSODetSplit.srvOrdType, Equal<Current<FSSODet.srvOrdType>>, 
                   And<FSSODetSplit.refNbr, Equal<Current<FSSODet.refNbr>>, 
                   And<FSSODetSplit.lineNbr, Equal<Current<FSSODet.lineNbr>>>>>> Splits;

        public LSFSSODetLine lsFSSODetSelect;

        public PXSelect<INTranSplit> intransplit;

        [PXFilterable]
        public PXFilter<SrvOrderTypeAux> ServiceOrderTypeSelector;

        public ServiceOrderCore.RelatedServiceOrders_View RelatedServiceOrders;

        [PXViewName(TX.FriendlyViewName.ServiceOrder.SERVICEORDER_POSTED_IN)]
        [PXCopyPasteHiddenView]
        public PXSelectJoinGroupBy<FSPostDet,
               InnerJoin<FSSODet,
               On<
                   FSSODet.postID, Equal<FSPostDet.postID>>,
               InnerJoin<FSPostBatch,
               On<
                   FSPostBatch.batchID, Equal<FSPostDet.batchID>>>>,
               Where<
                   FSSODet.sOID, Equal<Current<FSServiceOrder.sOID>>>,
               Aggregate<
                   GroupBy<FSPostDet.batchID,
                   GroupBy<FSPostDet.aRPosted,
                   GroupBy<FSPostDet.aPPosted,
                   GroupBy<FSPostDet.sOPosted,
                   GroupBy<FSPostDet.pMPosted,
                   GroupBy<FSPostDet.sOInvPosted>>>>>>>> ServiceOrderPostedIn;

        [PXCopyPasteHiddenView]
        public PXSelect<FSSchedule,
               Where<
                   FSSchedule.scheduleID, Equal<Current<FSServiceOrder.scheduleID>>>> ScheduleRecord;

        [PXCopyPasteHiddenView]
        public PXSetup<FSBillingCycle,
               InnerJoin<FSCustomerBillingSetup,
               On<
                   FSBillingCycle.billingCycleID, Equal<FSCustomerBillingSetup.billingCycleID>>>,
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
               Where<
                   FSContractPeriodDet.contractPeriodID, Equal<Current<FSContractPeriod.contractPeriodID>>,
                   And<FSContractPeriodDet.serviceContractID, Equal<Current<FSContractPeriod.serviceContractID>>>>> StandardContractPeriodDetail;

        public PXSelect<FSAppointment> appointmentView;
        public PXSelect<FSAppointmentDet> apptDetView;

        public PXFilter<FSSiteStatusFilter> sitestatusfilter;
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public FSSiteStatusLookup<FSSiteStatusSelected, FSSiteStatusFilter> sitestatus;

        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }
        
        [PXCopyPasteHiddenView()]
        public PXSelectReadonly2<ARPayment,
               InnerJoin<FSAdjust,
               On<
                   ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                   And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
               Where<
                   FSAdjust.adjdOrderType, Equal<Current<FSServiceOrder.srvOrdType>>,
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
        [PXCopyPasteHiddenView]
        public PXSelect<FSServiceOrderTax,
               Where<
                   FSServiceOrderTax.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                   And<FSServiceOrderTax.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>,
               OrderBy<
                   Asc<FSServiceOrderTax.taxID>>> TaxLines;

        [PXViewName(TX.Messages.ServiceOrderTax)]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSServiceOrderTaxTran,
               InnerJoin<Tax,
               On<
                   Tax.taxID, Equal<FSServiceOrderTaxTran.taxID>>>,
               Where<
                   FSServiceOrderTaxTran.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                   And<FSServiceOrderTaxTran.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>,
               OrderBy<
                   Asc<FSServiceOrderTaxTran.taxID,
                   Asc<FSServiceOrderTaxTran.recordID>>>> Taxes;
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

        public PXAction<FSServiceOrder> report;
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports")]
        public virtual IEnumerable Report(PXAdapter adapter,
            [PXString(8, InputMask = "CC.CC.CC.CC")]
            string reportID
            )
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            if (!String.IsNullOrEmpty(reportID))
            {
                Save.Press();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                string actualReportID = null;

                PXReportRequiredException ex = null;
                Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

                foreach (FSServiceOrder so in list)
                {
                    parameters = new Dictionary<string, string>();
                    parameters["FSServiceOrder.SrvOrdType"] = so.SrvOrdType;
                    parameters["FSServiceOrder.RefNbr"] = so.RefNbr;

                    object customer = PXSelectorAttribute.Select<FSServiceOrder.customerID>(ServiceOrderRecords.Cache, so);
                    actualReportID = new NotificationUtility(this).SearchReport(SONotificationSource.Customer, customer, reportID, so.BranchID);
                    ex = PXReportRequiredException.CombineReport(ex, actualReportID, parameters);

                    reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parameters, adapter, new NotificationUtility(this).SearchPrinter, SONotificationSource.Customer, reportID, actualReportID, so.BranchID);
                }

                if (ex != null)
                {
                    PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint);

                    throw ex;
                }
            }
            return adapter.Get();
        }

        #endregion

        #region Non-Shared Actions
        #region ScheduleScreen
        public PXAction<FSServiceOrder> scheduleAppointment;
        [PXUIField(DisplayName = "Schedule Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual IEnumerable ScheduleAppointment(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            foreach (FSServiceOrder fsServiceOrderRow in list)
            {
                SaveBeforeApplyAction(ServiceOrderRecords.Cache, fsServiceOrderRow);

                if (SharedFunctions.isThisAProspect(this, fsServiceOrderRow.CustomerID))
                {
                    throw new PXException("Error");
                }

                var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                FSAppointment fsAppointmentRow = new FSAppointment()
                {
                    SrvOrdType = ServiceOrderRecords.Current.SrvOrdType,
                    SOID = ServiceOrderRecords.Current.SOID
                };

                fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Insert(fsAppointmentRow);

                graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.soRefNbr>(graphAppointmentEntry.AppointmentRecords.Current, fsServiceOrderRow.RefNbr);

                throw new PXRedirectRequiredException(graphAppointmentEntry, null);
            }

            return list;
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

                        CRCase crCase = PXSelect<CRCase,
                                        Where<
                                            CRCase.caseCD, Equal<Required<CRCase.caseCD>>>>
                                        .Select(graphCRCase, fsServiceOrderRow.SourceRefNbr);

                        if (crCase != null)
                        {
                            graphCRCase.Case.Current = crCase;
                            throw new PXRedirectRequiredException(graphCRCase, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }

                        break;

                    case ID.SourceType_ServiceOrder.OPPORTUNITY:
                        var graphOpportunityMaint = PXGraph.CreateInstance<OpportunityMaint>();
                        CROpportunity crOpportunityRow = PXSelect<CROpportunity,
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
                        SOOrder soOrder = PXSelect<SOOrder,
                                          Where<
                                              SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                                              And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                          .Select(graphSOOrder, fsServiceOrderRow.SourceDocType, fsServiceOrderRow.SourceRefNbr);

                        if (soOrder != null)
                        {
                            graphSOOrder.Document.Current = soOrder;
                            throw new PXRedirectRequiredException(graphSOOrder, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }

                        break;

                    case ID.SourceType_ServiceOrder.SERVICE_DISPATCH:

                        var graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

                        graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                                                        .Search<FSServiceOrder.refNbr>
                                                                        (fsServiceOrderRow.SourceRefNbr, fsServiceOrderRow.SourceDocType);

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

                    graphPostBatchEntry.BatchRecords.Current = graphPostBatchEntry.BatchRecords.Search<FSPostBatch.batchID>(fsPostDetRow.BatchID);

                    throw new PXRedirectRequiredException(graphPostBatchEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }

            return adapter.Get();
        }
        #endregion

        #region OpenServiceOrderScreen
        public PXAction<FSServiceOrder> openServiceOrderScreen;
        [PXUIField(DisplayName = "Open Service Order Screen", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenServiceOrderScreen()
        {
            if (RelatedServiceOrders.Current != null)
            {
                var graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

                graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                                                .Search<FSServiceOrder.refNbr>
                                                                (RelatedServiceOrders.Current.RefNbr, RelatedServiceOrders.Current.SrvOrdType);

                throw new PXRedirectRequiredException(graphServiceOrder, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #region CreatePurchaseOrder
        public PXAction<FSServiceOrder> createPurchaseOrder;
        [PXUIField(DisplayName = "Create Purchase Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = "DISTINV")]
        [PXButton]
        public virtual IEnumerable CreatePurchaseOrder(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            if (!adapter.MassProcess)
            {
                SaveBeforeApplyAction(ServiceOrderRecords.Cache, list[0]);

                POCreate graphPOCreate = PXGraph.CreateInstance<POCreate>();
                FSxPOCreateFilter fSxPOCreateFilterRow = graphPOCreate.Filter.Cache.GetExtension<FSxPOCreateFilter>(graphPOCreate.Filter.Current);
                fSxPOCreateFilterRow.SrvOrdType = list[0].SrvOrdType;
                fSxPOCreateFilterRow.ServiceOrderRefNbr = list[0].RefNbr;

                throw new PXRedirectRequiredException(graphPOCreate, null);
            }

            return list;
        }
        #endregion
        #endregion

        #region Actions

        #region CompleteOrder
        public PXAction<FSServiceOrder> completeOrder;
        [PXUIField(DisplayName = "Complete Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CompleteOrder(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            foreach (FSServiceOrder fsServiceOrderRow in list)
            {
                ChangeStatus_Handler(this, fsServiceOrderRow, BillingCycleRelated.Current, ID.Status_ServiceOrder.COMPLETED);
            }

            return list;
        }
        #endregion
        #region CancelOrder
        public PXAction<FSServiceOrder> cancelOrder;
        [PXUIField(DisplayName = "Cancel Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CancelOrder(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            foreach (FSServiceOrder fsServiceOrderRow in list)
            {
                ChangeStatus_Handler(this, fsServiceOrderRow, BillingCycleRelated.Current, ID.Status_ServiceOrder.CANCELED);
            }

            return list;
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
                ChangeStatus_Handler(this, ServiceOrderRecords.Current, BillingCycleRelated.Current, ID.Status_ServiceOrder.CLOSED);

                DeallocateUnusedItems(ServiceOrderRecords.Current);

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
        [PXUIField(DisplayName = "Run Service Order Billing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable InvoiceOrder(PXAdapter adapter)
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

                        if (ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
                        {
                            UpdateUnitCostsAndUnitPrices(this,
                                                         ServiceOrderDetails.Cache,
                                                         ServiceOrderDetails.Select().RowCast<FSSODet>(),
                                                         ServiceOrderRecords.Current.SOID);

                            if (ServiceOrderDetails.Cache.IsDirty == true)
                            {
                                this.SelectTimeStamp();
                                Save.Press();
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
            ChangeStatus_Handler(this, ServiceOrderRecords.Current, BillingCycleRelated.Current, ID.Status_ServiceOrder.OPEN);
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
        public PXAction<FSServiceOrder> OpenRoomBoard;
        [PXUIField(DisplayName = TX.CalendarBoardAccess.ROOM_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void openRoomBoard()
        {
            throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.MULTI_ROOM_DISPATCH,
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
                throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.SINGLE_EMPLOYEE_DISPATCH,
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
                    newServiceOrderRow.SourceType = ID.SourceType_ServiceOrder.SERVICE_DISPATCH;
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

                    newServiceOrderCache.SetValueExt<FSServiceOrder.projectID>(newServiceOrderRow, sourceServiceOrderRow.ProjectID);
                    newServiceOrderCache.SetValueExt<FSServiceOrder.dfltProjectTaskID>(newServiceOrderRow, sourceServiceOrderRow.DfltProjectTaskID);

                    newServiceOrderCache.SetValueExt<FSServiceOrder.contactID>(newServiceOrderRow, sourceServiceOrderRow.ContactID);

                    newServiceOrderCache.SetValueExt<FSServiceOrder.billCustomerID>(newServiceOrderRow, sourceServiceOrderRow.BillCustomerID);
                    newServiceOrderCache.SetValueExt<FSServiceOrder.billLocationID>(newServiceOrderRow, sourceServiceOrderRow.BillLocationID);

                    newServiceOrderCache.SetValueExt<FSServiceOrder.curyID>(newServiceOrderRow, sourceServiceOrderRow.CuryID);

                    foreach (FSSODet sourceRow in this.ServiceOrderDetails.Select())
                    {
                        var newRow = new FSSODet();

                        newRow = AppointmentEntry.InsertDetailLine<FSSODet, FSSODet>(
                                                                    newServiceOrderGraph.ServiceOrderDetails.Cache,
                                                                    newRow,
                                                                    this.ServiceOrderDetails.Cache,
                                                                    sourceRow,
                                                                    noteID: null,
                                                                    soDetID: null,
                                                                    copyTranDate: false,
                                                                    tranDate: sourceRow.TranDate,
                                                                    SetValuesAfterAssigningSODetID: true,
                                                                    copyingFromQuote: true);

                        PXNoteAttribute.CopyNoteAndFiles(this.ServiceOrderDetails.Cache,
                                                         sourceRow,
                                                         newServiceOrderGraph.ServiceOrderDetails.Cache,
                                                         newRow,
                                                         copyNotes: true,
                                                         copyFiles: false);
                    }

                    newServiceOrderGraph.Answers.CopyAllAttributes(newServiceOrderGraph.CurrentServiceOrder.Current, CurrentServiceOrder.Current);

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
            else if (fsPostDetRow.PMPosted == true)
            {
                RegisterEntry graphRegisterEntry = PXGraph.CreateInstance<RegisterEntry>();
                graphRegisterEntry.Document.Current = graphRegisterEntry.Document.Search<PMRegister.refNbr>(fsPostDetRow.PMRefNbr, fsPostDetRow.PMDocType);
                throw new PXRedirectRequiredException(graphRegisterEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenINPostingDocument
        public PXAction<FSServiceOrder> OpenINPostingDocument;
        [PXButton]
        [PXUIField(DisplayName = "Open IN Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openINPostingDocument()
        {
            FSPostDet fsPostDetRow = ServiceOrderPostedIn.Select();

            if (fsPostDetRow == null)
            {
                return;
            }

            if (fsPostDetRow.INPosted == true)
            {
                if (fsPostDetRow.INDocType.Trim() == INDocType.Receipt)
                {
                    INReceiptEntry graphINReceiptEntry = PXGraph.CreateInstance<INReceiptEntry>();
                    graphINReceiptEntry.receipt.Current = graphINReceiptEntry.receipt.Search<INRegister.refNbr>(fsPostDetRow.INRefNbr);
                    throw new PXRedirectRequiredException(graphINReceiptEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else
                {
                    INIssueEntry graphINIssueEntry = PXGraph.CreateInstance<INIssueEntry>();
                    graphINIssueEntry.issue.Current = graphINIssueEntry.issue.Search<INRegister.refNbr>(fsPostDetRow.INRefNbr);
                    throw new PXRedirectRequiredException(graphINIssueEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
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
                                         Where<
                                             ARInvoice.docType, Equal<Required<ARInvoice.docType>>, 
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
            else if (fsPostDetRow.PMPosted == true)
            {
                RegisterEntry graphRegisterEntry = PXGraph.CreateInstance<RegisterEntry>();
                graphRegisterEntry.Document.Current = graphRegisterEntry.Document.Search<PMRegister.refNbr>(fsPostDetRow.PMRefNbr, fsPostDetRow.PMDocType);
                throw new PXRedirectRequiredException(graphRegisterEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region Service Order Reports

        public PXAction<FSServiceOrder> printServiceOrder;
        [PXUIField(DisplayName = "Print Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable PrintServiceOrder(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            if (!adapter.MassProcess)
            {
                SaveBeforeApplyAction(ServiceOrderRecords.Cache, list[0]);

                Dictionary<string, string> parameters = this.GetServiceOrderParameters(list[0], false);

                if (parameters.Count > 0)
                {
                    throw new PXReportRequiredException(parameters, ID.ReportID.SERVICE_ORDER, string.Empty);
                }
            }

            return list;
        }

        public PXAction<FSServiceOrder> printServiceTimeActivityReport;
        [PXUIField(DisplayName = "Print Service Time Activity", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable PrintServiceTimeActivityReport(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            if (!adapter.MassProcess)
            {
                SaveBeforeApplyAction(ServiceOrderRecords.Cache, list[0]);

                Dictionary<string, string> parameters = this.GetServiceOrderParameters(list[0], true);

                if (parameters.Count > 0)
                {
                    throw new PXReportRequiredException(parameters, ID.ReportID.SERVICE_TIME_ACTIVITY, string.Empty);
                }
            }

            return list;
        }
        #endregion

        #region Appointments in Service Order Report
        public PXAction<FSServiceOrder> serviceOrderAppointmentsReport;
        [PXUIField(DisplayName = "Print Appointments in Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable ServiceOrderAppointmentsReport(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            if (!adapter.MassProcess)
            {
                SaveBeforeApplyAction(ServiceOrderRecords.Cache, list[0]);

                Dictionary<string, string> parameters = this.GetServiceOrderParameters(list[0], false);

                if (parameters.Count > 0)
                {
                    throw new PXReportRequiredException(parameters, ID.ReportID.APP_IN_SERVICE_ORDER, string.Empty);
                }
            }

            return list;
        }
        #endregion

        #region AllowInvoice
        public PXAction<FSServiceOrder> allowBilling;
        [PXUIField(DisplayName = "Allow Billing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable AllowBilling(PXAdapter adapter)
        {
            List<FSServiceOrder> list = adapter.Get<FSServiceOrder>().ToList();

            foreach (FSServiceOrder fsServiceOrderRow in list)
            {
                SaveBeforeApplyAction(ServiceOrderRecords.Cache, fsServiceOrderRow);

                if (fsServiceOrderRow != null)
                {
                    ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.allowInvoice>(fsServiceOrderRow, true);
                    ServiceOrderRecords.Update(ServiceOrderRecords.Current);
                    this.Save.Press();
                }
            }

            return list;
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
                                                                                    .ContractScheduleRecords.Search<FSContractSchedule.scheduleID>
                                                                                    (ServiceOrderRecords.Current.ScheduleID,
                                                                                     ServiceOrderRecords.Current.ServiceContractID,
                                                                                     ServiceOrderRecords.Current.CustomerID);

                throw new PXRedirectRequiredException(graphServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region AddInvBySite
        public PXAction<FSServiceOrder> addInvBySite;
        [PXUIField(DisplayName = "Add Items", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable AddInvBySite(PXAdapter adapter)
        {
            sitestatusfilter.Cache.Clear();
            if (sitestatus.AskExt() == WebDialogResult.OK)
            {
                return AddInvSelBySite(adapter);
            }
            sitestatusfilter.Cache.Clear();
            sitestatus.Cache.Clear();
            return adapter.Get();
        }
        #endregion

        #region AddInvSelBySite
        public PXAction<FSServiceOrder> addInvSelBySite;
        [PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
        {
            ServiceOrderDetails.Cache.ForceExceptionHandling = true;

            foreach (FSSiteStatusSelected line in sitestatus.Cache.Cached)
            {
                if (line.Selected == true
                    && (line.QtySelected > 0 || line.DurationSelected > 0))
                {
                    InventoryItem inventoryItem  =
                        PXSelect<InventoryItem,
                        Where<
                            InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .Select(this, line.InventoryID);

                    FSSODet newline = PXCache<FSSODet>.CreateCopy(ServiceOrderDetails.Insert(new FSSODet()));
                    if (inventoryItem.StkItem == true)
                    {
                        newline.LineType = ID.LineType_ALL.INVENTORY_ITEM;
                    }
                    else
                    {
                        newline.LineType = inventoryItem.ItemType == INItemTypes.ServiceItem ? ID.LineType_ALL.SERVICE : ID.LineType_ALL.NONSTOCKITEM;
                    }

                    newline.SiteID = line.SiteID;
                    newline.InventoryID = line.InventoryID;
                    newline.SubItemID = line.SubItemID;
                    newline.UOM = line.SalesUnit;
                    newline = PXCache<FSSODet>.CreateCopy(ServiceOrderDetails.Update(newline));
                    if (newline.RequireLocation != true)
                        newline.LocationID = null;
                    newline = PXCache<FSSODet>.CreateCopy(ServiceOrderDetails.Update(newline));

                    if (line.BillingRule == ID.BillingRule.TIME)
                    {
                        newline.EstimatedDuration = line.DurationSelected;
                    }
                    else
                    {
                        newline.Qty = line.QtySelected;
                    }

                    ServiceOrderDetails.Update(newline);
                }
            }

            sitestatus.Cache.Clear();
            return adapter.Get();
        }
        #endregion
        #region ViewLinkedDoc
        public ViewLinkedDoc<FSServiceOrder, FSSODet> viewLinkedDoc;
        #endregion
        #region AddReceipt
        public PXAction<FSServiceOrder> addReceipt;
        [PXUIField(DisplayName = "Create Expense Receipt", MapEnableRights = PXCacheRights.Select)]
        [PXButton()]
        protected virtual IEnumerable AddReceipt(PXAdapter adapter)
        {
            FSServiceOrder fsServiceOrderRow = ServiceOrderRecords.Current;
            FSSrvOrdType fsSrvOrdTypeRow = ServiceOrderTypeSelected.Current;

            if (fsServiceOrderRow != null)
            {
                Save.Press();

                ExpenseClaimDetailEntry graph = GetExpenseClaimDetailGraph(true);
                EPExpenseClaimDetails claimDetails = (EPExpenseClaimDetails)graph.ClaimDetails.Cache.CreateInstance();

                claimDetails = graph.ClaimDetails.Insert(claimDetails);

                claimDetails.ExpenseDate = fsServiceOrderRow.OrderDate;
                claimDetails.BranchID = fsServiceOrderRow.BranchID;
                claimDetails.CustomerID = fsServiceOrderRow.BillCustomerID;
                claimDetails.CustomerLocationID = fsServiceOrderRow.BillLocationID;
                claimDetails.ContractID = fsServiceOrderRow.ProjectID;
                claimDetails.TaskID = fsServiceOrderRow.DfltProjectTaskID;

                if (fsSrvOrdTypeRow != null
                   && !ProjectDefaultAttribute.IsNonProject(fsServiceOrderRow.ProjectID)
                   && PXAccess.FeatureInstalled<FeaturesSet.costCodes>())
                {
                    claimDetails.CostCodeID = fsSrvOrdTypeRow.DfltCostCodeID;
                }

                claimDetails = graph.ClaimDetails.Update(claimDetails);

                FSxEPExpenseClaimDetails row = graph.ClaimDetails.Cache.GetExtension<FSxEPExpenseClaimDetails>(claimDetails);

                graph.ClaimDetails.Cache.SetValueExt<FSxEPExpenseClaimDetails.fsEntityTypeUI>(claimDetails, ID.FSEntityType.ServiceOrder);
                graph.ClaimDetails.Cache.SetValueExt<FSxEPExpenseClaimDetails.fsEntityNoteID>(claimDetails, fsServiceOrderRow.NoteID);

                claimDetails = graph.ClaimDetails.Update(claimDetails);

                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
            }


            return adapter.Get();
        }
        #endregion
        #endregion

        #region Public Methods
        public virtual void DeallocateUnusedItems(FSServiceOrder serviceOrder)
        {
            if (serviceOrder.BillServiceContractID != null)
            {
                return;
            }

            FSBillingCycle fsBillingCycleRow = PXSelectJoin<FSBillingCycle,
                                               InnerJoin<FSCustomerBillingSetup,
                                               On<
                                                   FSCustomerBillingSetup.billingCycleID, Equal<FSBillingCycle.billingCycleID>>>,
                                               Where<
                                                   FSCustomerBillingSetup.cBID, Equal<Required<FSCustomerBillingSetup.cBID>>>>
                                               .Select(this, serviceOrder.CBID);

            if (fsBillingCycleRow == null
                    || fsBillingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER)
            {
                return;
            }

            var apptLineView = new PXSelect<FSAppointmentDet>(this);
            if (!this.Views.Caches.Contains(typeof(FSAppointmentDet)))
                this.Views.Caches.Add(typeof(FSAppointmentDet));

            var apptLineSplitView = new PXSelect<FSApptLineSplit>(this);
            if (!this.Views.Caches.Contains(typeof(FSApptLineSplit)))
                this.Views.Caches.Add(typeof(FSApptLineSplit));

            List<FSSODetSplit> soSplits = new List<FSSODetSplit>();

            foreach(FSSODetSplit soSplit in PXSelect<FSSODetSplit,
                                        Where<FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                            And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                            And<FSSODetSplit.completed, Equal<False>,
                                            And<FSSODetSplit.pOCreate, Equal<False>,
                                            And<FSSODetSplit.inventoryID, IsNotNull>>>>>,
                                        OrderBy<Asc<FSSODetSplit.lineNbr,
                                                Asc<FSSODetSplit.splitLineNbr>>>>
                                        .Select(this, serviceOrder.SrvOrdType, serviceOrder.RefNbr))
            {
                soSplits.Add((FSSODetSplit)this.Splits.Cache.CreateCopy(soSplit));
            }

            int? lastSOLineLineNbr = null;
            FSSODet soLine = null;
            
            List<FSAppointmentDet> apptLines = new List<FSAppointmentDet>();
            List<FSApptLineSplit> apptSplits = new List<FSApptLineSplit>();
            bool isLotSerialRequired = false;
            List<FSSODetSplit> splitsToDeallocate = new List<FSSODetSplit>();
            decimal? baseUsedNotLotSerialQty = 0m;

            foreach (FSSODetSplit soSplit in soSplits)
            {
                if (lastSOLineLineNbr == null || lastSOLineLineNbr != soSplit.LineNbr)
                {
                    baseUsedNotLotSerialQty = 0m;

                    isLotSerialRequired = SharedFunctions.IsLotSerialRequired(this.ServiceOrderDetails.Cache, soSplit.InventoryID);
                    lastSOLineLineNbr = soSplit.LineNbr;
                    
                    soLine = (FSSODet)PXParentAttribute.SelectParent(this.Splits.Cache, soSplit, typeof(FSSODet));
                    if (soLine == null)
                    {
                        throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                    }

                    apptLines.Clear();
                    apptSplits.Clear();
                    foreach (FSAppointmentDet apptLine in AppointmentEntry.GetRelatedApptLines(this, soLine.SODetID, excludeSpecificApptLine: false, apptDetID: null, onlyMarkForPOLines: false, sortResult: false))
                    {
                        apptLines.Add((FSAppointmentDet)apptLineView.Cache.CreateCopy(apptLine));
                    }

                    if (isLotSerialRequired == true)
                    {
                        foreach (FSAppointmentDet apptLine in apptLines)
                        {
                            foreach (FSApptLineSplit split in PXParentAttribute.SelectChildren(apptLineSplitView.Cache, apptLine, typeof(FSAppointmentDet)))
                            {
                                apptSplits.Add((FSApptLineSplit)apptLineSplitView.Cache.CreateCopy(split));
                            }
                        }
                    }
                    else
                    {
                        foreach (FSAppointmentDet apptLine in apptLines.Where(x => x.BaseEffTranQty > 0m))
                        {
                            baseUsedNotLotSerialQty += apptLine.BaseEffTranQty;
                            apptLine.BaseEffTranQty = 0;
                        }
                    }
                }

                if (isLotSerialRequired == true)
                {
                    decimal? baseLotSerialUsedQty = 0m;
                    foreach (FSApptLineSplit apptSplit in apptSplits.Where(x => string.IsNullOrEmpty(x.LotSerialNbr) == false
                                                                                    && x.LotSerialNbr == soSplit.LotSerialNbr))
                    {
                        baseLotSerialUsedQty += apptSplit.BaseQty;
                        apptSplit.BaseQty = 0;
                    }

                    if (baseLotSerialUsedQty <= soSplit.BaseQty)
                    {
                        soSplit.BaseQty = baseLotSerialUsedQty;
                    }

                    splitsToDeallocate.Add(soSplit);
                }
                else 
                {
                    if (baseUsedNotLotSerialQty <= 0)
                    {
                        soSplit.BaseQty = 0;
                    }
                    else
                    {
                        if (soSplit.BaseQty <= baseUsedNotLotSerialQty)
                        {
                            baseUsedNotLotSerialQty -= soSplit.BaseQty;
                        }else
                        {
                            soSplit.BaseQty = baseUsedNotLotSerialQty;
                        }
                    }

                    splitsToDeallocate.Add(soSplit);
                }
            }

            FSDeallocateProcess.DeallocateServiceOrders(this, splitsToDeallocate);
        }

        public virtual void UpdateUnitCostsAndUnitPrices(PXGraph graph, PXCache srvOrdDetailsCache, IEnumerable<FSSODet> soDetails, int? sOID)
        {
            FSSODet fsSODetRow;
            PMTran pmTranRow;
            int? postID;    // This is needed to avoid the bug related to UpdatePendingPostFlags() method called during the RowPersisting

            if (sOID != null)
            {
                var postedRows = PXSelectJoin<PMTran,
                                 InnerJoin<FSPostDet,
                                 On<
                                     FSPostDet.pMDocType, Equal<PMTran.tranType>,
                                     And<FSPostDet.pMRefNbr, Equal<PMTran.refNbr>,
                                     And<FSPostDet.pMTranID, Equal<PMTran.tranID>>>>,
                                 InnerJoin<FSSODet,
                                 On<
                                     FSSODet.postID, Equal<FSPostDet.postID>>>>,
                                 Where<
                                     FSSODet.sOID, Equal<Required<FSSODet.sOID>>>>
                                 .Select(graph, sOID);

                foreach (PXResult<PMTran, FSPostDet, FSSODet> postedRow in postedRows)
                {
                    pmTranRow = (PMTran)postedRow;
                    fsSODetRow = (FSSODet)postedRow;
                    postID = fsSODetRow.PostID;

                    fsSODetRow = soDetails.Where(x => x.SODetID == fsSODetRow.SODetID).FirstOrDefault();

                    if (fsSODetRow != null)
                    {
                        FSSODet appDetRowCopy = (FSSODet)srvOrdDetailsCache.CreateCopy(fsSODetRow);
                        appDetRowCopy.CuryUnitCost = pmTranRow.TranCuryUnitRate;
                        appDetRowCopy.CuryUnitPrice = pmTranRow.TranCuryUnitRate;
                        appDetRowCopy.PostID = postID;
                        srvOrdDetailsCache.Update(appDetRowCopy);
                    }
                }
            }
        }
        #endregion

        #region Virtual Methods

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

                    case ID.LineType_AppSrvOrd.COMMENT:
                    case ID.LineType_AppSrvOrd.INSTRUCTION:
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
            PXUIFieldAttribute.SetEnabled<FSSODet.isFree>(cache, fsSODetRow, enableIsBillable);
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

        public virtual void EnableDisable_ActionButtons(FSServiceOrder fsServiceOrderRow, PXCache cache)
        {
            ServiceOrderCore.EnableDisable_ActionButtons(this,
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
                                                         scheduleAppointment,
                                                         openUserCalendar,
                                                         openEmployeeBoard,
                                                         OpenRoomBoard,
                                                         addInvBySite,
                                                         addInvSelBySite,
                                                         openStaffSelectorFromServiceTab,
                                                         openStaffSelectorFromStaffTab,
                                                         viewDirectionOnMap,
                                                         validateAddress,
                                                         convertToServiceOrder,
                                                         createPurchaseOrder,
                                                         allowBilling,
                                                         addReceipt);
        }

        /// <summary>
        /// Check the ManageRooms value on Setup to check/hide the Rooms Values options.
        /// </summary>
        public virtual void HideRooms(FSServiceOrder fsServiceOrderRow)
        {
            bool isRoomManagementActive = ServiceManagementSetup.IsRoomManagementActive(this, SetupRecord.Current);

            PXUIFieldAttribute.SetVisible<FSServiceOrder.roomID>(this.CurrentServiceOrder.Cache, fsServiceOrderRow, isRoomManagementActive);
            OpenRoomBoard.SetVisible(isRoomManagementActive);
        }

        /// <summary>
        /// Enable/Disable the Tabs if the selected Service Order Type post to AR (Parts Tab) and if the customerID is null or not (Services and Parts).
        /// </summary>
        [Obsolete("Remove in major update 2021R1")]
        public virtual void EnableDisable_StaffID(PXCache cache, FSSODet fsSODetRow)
        {
            bool enableStaffID = fsSODetRow.EnableStaffID == true
                                      && fsSODetRow.LineType == ID.LineType_ALL.SERVICE;

            PXUIFieldAttribute.SetEnabled<FSSODet.staffID>(cache, fsSODetRow, enableStaffID);
        }

        public virtual void DisableAllDACFields(PXCache cache, object row, List<Type> fieldsToIgnore)
        {
            SharedFunctions.DisableAllDACFields(cache, row, fieldsToIgnore);
        }
        #endregion

        //This function is unused, we need to evalute if whe should re-use it or remove it 
        public virtual bool isTheLineValid(PXCache cache, FSSODet fsSODetRow, PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            bool lineOK = true;

            if (fsSODetRow == null)
            {
                return lineOK;
            }

            if ((fsSODetRow.LineType == ID.LineType_ServiceTemplate.COMMENT
                        || fsSODetRow.LineType == ID.LineType_ServiceTemplate.INSTRUCTION)
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

            if ((fsSODetRow.LineType == ID.LineType_AppSrvOrd.COMMENT
                    || fsSODetRow.LineType == ID.LineType_AppSrvOrd.INSTRUCTION)
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

        public virtual void ClearOpportunity(FSServiceOrder fsServiceOrderRow)
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

        public virtual void ClearCase(FSServiceOrder fsServiceOrderRow)
        {
            CRCaseMaint graphCRCaseMaint = PXGraph.CreateInstance<CRCaseMaint>();
            graphCRCaseMaint.Case.Current = graphCRCaseMaint.Case.Search<CRCase.caseCD>(fsServiceOrderRow.SourceRefNbr);
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

        public virtual void ClearCRActivities(PXResultset<CRPMTimeActivity> activities)
        {
            CRTaskMaint graphCRTaskMaint = PXGraph.CreateInstance<CRTaskMaint>();
            PMTimeActivity pmTimeActivityRow = null;
            FSxPMTimeActivity fsxPMTimeActivityRow = null;

            foreach (CRActivity crActivity in activities)
            {
                pmTimeActivityRow = PXSelect<PMTimeActivity,
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

        public virtual void ClearSalesOrder(FSServiceOrder fsServiceOrderRow)
        {
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

        public virtual void ClearPrepayment(FSServiceOrder fsServiceOrderRow)
        {
            ARPaymentEntry graphARPaymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
            SM_ARPaymentEntry graphSM_ARPaymentEntry = graphARPaymentEntry.GetExtension<SM_ARPaymentEntry>();

            var adjustments = PXSelect<FSAdjust,
                              Where<
                                  FSAdjust.adjdOrderType, Equal<Required<FSAdjust.adjdOrderType>>,
                                  And<FSAdjust.adjdOrderNbr, Equal<Required<FSAdjust.adjdOrderNbr>>>>>
                              .Select(graphARPaymentEntry, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr);

            foreach (FSAdjust fsAdjustRow in adjustments)
            {
                graphARPaymentEntry.Document.Current = graphARPaymentEntry.Document.Search<ARPayment.refNbr>(fsAdjustRow.AdjgRefNbr, fsAdjustRow.AdjgDocType);

                if (graphARPaymentEntry.Document.Current != null)
                { 
                    graphSM_ARPaymentEntry.FSAdjustments.Delete(fsAdjustRow);
                    graphARPaymentEntry.Save.Press();
                }
            }
        }

        public virtual void ClearFSDocExpenseReceipts(Guid? noteID) 
        {
            if (noteID == null)
            {
                return;
            }

            ExpenseClaimDetailEntry graph = GetExpenseClaimDetailGraph(true);

            PXResultset<EPExpenseClaimDetails> list = PXSelect<EPExpenseClaimDetails,
                                  Where<
                                      FSxEPExpenseClaimDetails.fsEntityNoteID, Equal<Required<FSxEPExpenseClaimDetails.fsEntityNoteID>>>>
                                  .Select(graph, noteID);

            ClearFSDocExpenseReceipts(graph, list);
        }

        public virtual void ClearFSDocExpenseReceipts(string DocRefNbr)
        {
            if (string.IsNullOrEmpty(DocRefNbr) == true)
            {
                return;
            }

            ExpenseClaimDetailEntry graph = GetExpenseClaimDetailGraph(true);

            PXResultset<EPExpenseClaimDetails> list = PXSelect<EPExpenseClaimDetails,
                                      Where<
                                          EPExpenseClaimDetails.claimDetailCD, Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>
                                      .Select(graph, DocRefNbr);

            ClearFSDocExpenseReceipts(graph, list);
        }

        public virtual void ClearFSDocExpenseReceipts(ExpenseClaimDetailEntry graph, PXResultset<EPExpenseClaimDetails> list)
        {
            if (graph == null 
                || list == null 
                || list.Count == 0)
            {
                return;
            }

            foreach (EPExpenseClaimDetails row in list)
            {
                FSxEPExpenseClaimDetails extRow = PXCache<EPExpenseClaimDetails>.GetExtension<FSxEPExpenseClaimDetails>(row);

                if (extRow != null)
                {
                    extRow.FSEntityTypeUI = null;
                    extRow.FSEntityNoteID = null;
                    extRow.FSBillable = false;
                    graph.ClaimDetails.Cache.Update(row);
                }
            }

            if (graph.IsDirty)
            {
                graph.Save.Press();
            }
        }

        public virtual void SetSODetServiceLastReferencedBy(FSSODet fsSODetRow)
        {
            if (fsSODetRow == null)
                return;

            if (LastRefAppointmentDetails == null)
            {
                LastRefAppointmentDetails = PXSelectJoinGroupBy<
                                            FSAppointmentDet,
                                            InnerJoin<
                                                FSAppointment,
                                                On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
                                            Where<
                                                FSAppointment.sOID, Equal<Current<FSServiceOrder.sOID>>,
                                                And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>>>>,
                                            Aggregate<
                                                GroupBy<FSAppointmentDet.sODetID>>>
                                            .Select(this)
                                            .ToList();
            }

            if (LastRefAppointmentDetails != null)
            {
                FSAppointmentDet fsAppointmentDetRow = null;
                fsAppointmentDetRow = LastRefAppointmentDetails.Where(x => ((FSAppointmentDet)x).SODetID == fsSODetRow.SODetID).FirstOrDefault();

                if (fsAppointmentDetRow != null)
                {
                    fsSODetRow.Mem_LastReferencedBy = fsAppointmentDetRow.RefNbr;
                }
                else
                {
                    fsSODetRow.Mem_LastReferencedBy = null;
                }
            }
        }

        /// <summary>
        /// Update the assigned Employee for the Service Order in Sales Order customization if conditions apply.
        /// </summary>
        /// <param name="fsServiceOrderRow">FSServiceOrder row.</param>
        public virtual void UpdateAssignedEmpIDinSalesOrder(FSServiceOrder fsServiceOrderRow)
        {
            if (updateSOCstmAssigneeEmpID == true
                    && fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER)
            {
                SOOrder soOrderRow = PXSelect<SOOrder,
                                     Where<
                                         SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                                         And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
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
        public virtual bool IsThisLineRelatedToAsoLine(FSServiceOrder fsServiceOrderRow, FSSODet fsSODetRow)
        {
            if (fsSODetRow.IsPrepaid == true
                   && fsSODetRow.SourceLineNbr != null)
            {
                SOLine soLineRelated = PXSelect<SOLine,
                                       Where<
                                           SOLine.orderType, Equal<Required<SOLine.orderType>>,
                                           And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                                           And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>
                                       .SelectWindowed(this, 0, 1, fsServiceOrderRow.SourceDocType, fsServiceOrderRow.SourceRefNbr, fsSODetRow.SourceLineNbr);

                return soLineRelated != null;
            }

            return false;
        }

        /// <summary>
        /// Hides or Shows Appointments, Staff, Resources Equipment, Related Service Orders, Post info Tabs.
        /// </summary>
        /// <param name="fsServiceOrderRow">Service Order row.</param>
        public virtual void HideOrShowTabs(FSServiceOrder fsServiceOrderRow)
        {
            bool isQuote = (bool)fsServiceOrderRow.Quote;

            this.ServiceOrderAppointments.AllowSelect = !isQuote;
            this.ServiceOrderEmployees.AllowSelect = !isQuote && (SetupRecord.Current?.EnableDfltStaffOnServiceOrder ?? false);
            this.ServiceOrderEquipment.AllowSelect = !isQuote && (SetupRecord.Current?.EnableDfltResEquipOnServiceOrder ?? false);
            this.RelatedServiceOrders.AllowSelect = isQuote;
            
            bool isBillingBySO = BillingCycleRelated.Current != null && BillingCycleRelated.Current.BillingBy == ID.Billing_By.SERVICE_ORDER ? true : false;

            PXUIFieldAttribute.SetVisible<FSServiceOrder.allowInvoice>(ServiceOrderRecords.Cache, fsServiceOrderRow, isBillingBySO);
            PXUIFieldAttribute.SetVisible<FSServiceOrder.mem_Invoiced>(ServiceOrderRecords.Cache, fsServiceOrderRow, isBillingBySO);
            
            PXUIFieldAttribute.SetVisible<FSSODet.staffID>(ServiceOrderDetails.Cache,
                                                           ServiceOrderDetails.Current,
                                                           (SetupRecord.Current?.EnableDfltStaffOnServiceOrder ?? false));
            openStaffSelectorFromServiceTab.SetVisible(SetupRecord.Current?.EnableDfltStaffOnServiceOrder ?? false);

            this.ServiceOrderPostedIn.AllowSelect = isBillingBySO;
        }

        public virtual Dictionary<string, string> GetServiceOrderParameters(FSServiceOrder fsServiceOrderRow, bool isServiceTimeActivityReport = false)
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

        public virtual void EnableDisableDocumentByWorkflowStage(PXCache cache, FSWFStage fsWStageRow)
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

            detailsActions.Add(scheduleAppointment);
            detailsActions.Add(addInvBySite);
            detailsActions.Add(addInvSelBySite);
            detailsActions.Add(openStaffSelectorFromServiceTab);
            detailsActions.Add(openStaffSelectorFromStaffTab);
            detailsActions.Add(createPrepayment);

            views.Add(ServiceOrderDetails.View);
            views.Add(ServiceOrderEmployees.View);
            views.Add(ServiceOrderEquipment.View);
            views.Add(Adjustments.View);
            views.Add(Taxes.View);

            TreeWFStageHelper.EnableDisableDocumentByWorkflowStage(fsWStageRow, caches, headerActions, detailsActions, views);
        }

        public virtual void ChangeStatus_Handler(ServiceOrderEntry graph,
                                                FSServiceOrder fsServiceOrderRow,
                                                FSBillingCycle fsBillingCycleRow,
                                                string newStatus)
        {
            FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType,
                                           Where<
                                                FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                                           .Select(graph, fsServiceOrderRow.SrvOrdType);

            if (ServiceOrderCore.CheckServiceOrderStatusTransition(fsSrvOrdTypeRow.Behavior, fsServiceOrderRow, (bool)fsServiceOrderRow.Hold, newStatus) == false)
            {
                throw new PXException(TX.Error.INVALID_SO_STATUS_TRANSITION);
            }

            graph.SaveBeforeApplyAction(graph.ServiceOrderRecords.Cache, fsServiceOrderRow);

            WebDialogResult wdr;
            ShowAsk(graph, graph.ServiceOrderRecords.View, newStatus, out wdr);

            bool changeServiceOrderStatus = true;

            switch (newStatus)
            {
                case ID.Status_ServiceOrder.COMPLETED:
                    CompleteAppointmentsInServiceOrder(graph, fsServiceOrderRow);

                    IEnumerable<FSSODet> rowsToComplete = ServiceOrderDetails.Select().RowCast<FSSODet>()
                                                                             .Where(x => x.Status == ID.Status_SODet.SCHEDULED || x.Status == ID.Status_SODet.SCHEDULED_NEEDED);

                    ChangeDetailsLinesStatus(rowsToComplete, ID.Status_SODet.COMPLETED);

                    break;
                case ID.Status_ServiceOrder.CLOSED:
                    changeServiceOrderStatus = CloseAppointmentsInServiceOrder(graph, fsServiceOrderRow, wdr);
                    break;
                case ID.Status_ServiceOrder.CANCELED:

                    IEnumerable<FSSODet> rowsToCancel = ServiceOrderDetails.Select().RowCast<FSSODet>()
                                                                           .Where(x => x.Status == ID.Status_SODet.SCHEDULED || x.Status == ID.Status_SODet.SCHEDULED_NEEDED);

                    int hasCompletedLines = rowsToCancel.Where(_ => _.Status == ID.Status_SODet.COMPLETED).Count();

                    if(hasCompletedLines > 0)
                    {
                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.CANNOT_CANCEL_SERVICE_ORDER_LINE_IN_STATUS_COMPLETE));
                    }

                    CancelAppointmentsInServiceOrder(graph, fsServiceOrderRow);
                    ChangeDetailsLinesStatus(rowsToCancel, ID.Status_SODet.CANCELED);

                    break;

                case ID.Status_ServiceOrder.OPEN:

                    if (fsServiceOrderRow.Mem_Invoiced == false)
                    {
                        IEnumerable<FSSODet> rowsToOpen = ServiceOrderDetails.Select().RowCast<FSSODet>();
                        ChangeDetailsLinesStatus(rowsToOpen, ID.Status_SODet.SCHEDULED_NEEDED);
                        fsServiceOrderRow = graph.ServiceOrderRecords.Current;
                    }

                    break;
            }

            if (changeServiceOrderStatus)
            {
                graph.SelectTimeStamp();
                graph.ServiceOrderRecords.Cache.AllowUpdate = true;
                string oldStatus = fsServiceOrderRow.Status;

                fsServiceOrderRow = (FSServiceOrder)graph.ServiceOrderRecords.Cache.CreateCopy(fsServiceOrderRow);
                fsServiceOrderRow.Status = newStatus;
                graph.ServiceOrderRecords.Current = fsServiceOrderRow = graph.ServiceOrderRecords.Update(fsServiceOrderRow);

                if (newStatus == ID.Status_ServiceOrder.OPEN
                    && fsServiceOrderRow.Mem_Invoiced == false
                        && fsServiceOrderRow.AllowInvoice == true)
                {
                    graph.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.allowInvoice>(fsServiceOrderRow, false);
                }

                if (newStatus == ID.Status_ServiceOrder.CLOSED
                        && fsServiceOrderRow.Mem_Invoiced == false
                            && fsServiceOrderRow.AllowInvoice == false
                                && fsBillingCycleRow != null
                                    && fsBillingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER)
                {
                    fsServiceOrderRow.AllowInvoice = true;
                }

                graph.ServiceOrderRecords.Update(fsServiceOrderRow);
                graph.SkipTaxCalcAndSave();
            }

            graph.RecalculateExternalTaxes();
        }

        public virtual void ChangeDetailsLinesStatus(IEnumerable<FSSODet> rows, string newStatus)
        {
            foreach (FSSODet row in rows)
            {
                FSSODet fsSODetRow = (FSSODet)ServiceOrderDetails.Cache.CreateCopy(row);
                fsSODetRow.Status = newStatus;
                ServiceOrderDetails.Update(fsSODetRow);
            }
        }

        /// <summary>
        /// Completes all appointments belonging to <c>fsServiceOrderRow</c>, in case an error occurs with any appointment,
        /// the service order will not be completed and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        public virtual void CompleteAppointmentsInServiceOrder(ServiceOrderEntry graph, FSServiceOrder fsServiceOrderRow)
        {
            PXResultset<FSAppointmentInRoute> bqlResultSet = PXSelect<FSAppointmentInRoute,
                                                             Where<
                                                                 FSAppointmentInRoute.sOID, Equal<Required<FSAppointmentInRoute.sOID>>,
                                                             And<
                                                                 FSAppointmentInRoute.status, NotEqual<FSAppointmentInRoute.status.Closed>,
                                                             And<
                                                                 FSAppointmentInRoute.status, NotEqual<FSAppointmentInRoute.status.Canceled>,
                                                             And<
                                                                 FSAppointmentInRoute.status, NotEqual<FSAppointmentInRoute.status.Completed>>>>>>
                                                             .Select(graph, fsServiceOrderRow.SOID);

            if (bqlResultSet.Count > 0)
            {
                Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CompleteAppointments(graph, bqlResultSet);

                if (appWithErrors.Count > 0)
                {
                    List<FSAppointment> myApptList = graph.ServiceOrderAppointments.Select().RowCast<FSAppointment>().ToList();

                    foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                    {
                        FSAppointment appt = myApptList.Where(r => r.RefNbr == kvp.Key.RefNbr).FirstOrDefault();
                        if (appt != null)
                        {
                            graph.ServiceOrderAppointments.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(appt,
                                                                                                              appt.RefNbr,
                                                                                                              new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                        }
                    }

                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.SERVICE_ORDER_CANT_BE_COMPLETED_APPOINTMENTS_HAVE_ISSUES));
                }
            }
        }

        /// <summary>
        /// Closes all appointments belonging to <c>fsServiceOrderRow</c>, in case an error occurs with any appointment,
        /// the service order will not be closed and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        public virtual bool CloseAppointmentsInServiceOrder(ServiceOrderEntry graph, FSServiceOrder fsServiceOrderRow, WebDialogResult wdr)
        {
            bool closingActionStatus = true;

            PXResultset<FSAppointment> bqlResultSet =
                                        PXSelect<
                                            FSAppointment,
                                        Where<
                                            FSAppointment.sOID, Equal<Required<FSServiceOrder.sOID>>,
                                        And<
                                            FSAppointment.status, NotEqual<FSAppointment.status.Closed>,
                                        And<
                                            FSAppointment.status, NotEqual<FSAppointment.status.Canceled>>>>>
                                        .Select(graph, fsServiceOrderRow.SOID);

            if (bqlResultSet.Count > 0)
            {
                if (wdr != WebDialogResult.No)
                {
                    Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CloseAppointments(bqlResultSet);

                    if (appWithErrors.Count > 0)
                    {
                        foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                        {
                            graph.ServiceOrderAppointments.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(kvp.Key,
                                                                                                              kvp.Key.RefNbr,
                                                                                                              new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                        }

                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.SERVICE_ORDER_CANT_BE_CLOSED_APPOINTMENTS_HAVE_ISSUES));
                    }
                }
                else
                {
                    closingActionStatus = false;
                }
            }

            return closingActionStatus;
        }

        /// <summary>
        /// Cancels all appointments belonging to <c>fsServiceOrderRow</c>, in case an error occurs with any appointment,
        /// the service order will not be canceled and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        public virtual void CancelAppointmentsInServiceOrder(ServiceOrderEntry graph, FSServiceOrder fsServiceOrderRow)
        {
            PXResultset<FSAppointment> bqlResultSet = PXSelect<FSAppointment,
                                                      Where<
                                                          FSAppointment.sOID, Equal<Required<FSServiceOrder.sOID>>,
                                                      And<
                                                          FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                      And<
                                                          FSAppointment.status, NotEqual<FSAppointment.status.Closed>,
                                                      And <
                                                          FSAppointment.status, NotEqual < FSAppointment.status.Completed>>>>>>
                                                      .Select(graph, fsServiceOrderRow.SOID);

            if (bqlResultSet.Count > 0)
            {
                Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CancelAppointments(graph, bqlResultSet);

                if (appWithErrors.Count > 0)
                {
                    foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                    {
                        graph.ServiceOrderAppointments.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(kvp.Key,
                                                                                                          kvp.Key.RefNbr,
                                                                                                          new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                    }

                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.SERVICE_ORDER_CANT_BE_CANCELED_APPOINTMENTS_HAVE_ISSUES));
                }
            }
        }

        public virtual void ShowAsk(ServiceOrderEntry graph, PXView view, string newStatus, out WebDialogResult wdr)
        {
            wdr = WebDialogResult.None;

            switch (newStatus)
            {
                case ID.Status_ServiceOrder.COMPLETED:
                    break;
                case ID.Status_ServiceOrder.CLOSED:

                    bool displayAlert = graph.SetupRecord.Current.AlertBeforeCloseServiceOrder == true
                                            && graph.ServiceOrderRecords.Current.IsCalledFromQuickProcess != true;

                    if (displayAlert == true
                        && graph.Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SERVICE_ORDER)
                            && graph.ServiceOrderRecords.Current.AppointmentsCompletedCntr > 0)
                    {
                        wdr = view.Ask(TX.WebDialogTitles.CONFIRM_SERVICE_ORDER_CLOSING,
                                       TX.Messages.ASK_CONFIRM_SERVICE_ORDER_CLOSING,
                                       MessageButtons.YesNo);
                    }

                    break;
                case ID.Status_ServiceOrder.CANCELED:
                    break;
            }
        }

        public virtual bool IsManualPriceFlagNeeded(PXCache sender, IFSSODetBase row)
        {
            if (row != null 
                && row.ManualPrice != true 
                && (sender.Graph.IsImportFromExcel 
                    || sender.Graph.IsContractBasedAPI))
            {
                decimal price;

                object curyUnitPrice = sender.GetValuePending<FSSODet.curyUnitPrice>(row);
                object curyExtPrice = sender.GetValuePending<FSSODet.curyExtPrice>(row);
                object manualPrice = sender.GetValuePending<FSSODet.manualPrice>(row);

                if (((curyUnitPrice != PXCache.NotSetValue && curyUnitPrice != null && Decimal.TryParse(curyUnitPrice.ToString(), out price))
                    || (curyExtPrice != PXCache.NotSetValue && curyExtPrice != null && Decimal.TryParse(curyExtPrice.ToString(), out price)))
                    && (manualPrice == PXCache.NotSetValue || manualPrice == null))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool ShouldShowMarkForPOFields()
        {
            return ShouldShowMarkForPOFieldsInt(AllocationSOOrderTypeSelected.Current);
        }

        public virtual void UpdatePOOptionsInAppointmentRelatedLines(FSSODet soDet)
        {
            if (!Views.Caches.Contains(typeof(FSAppointment)))
                Views.Caches.Add(typeof(FSAppointment));

            if (!Views.Caches.Contains(typeof(FSAppointmentDet)))
                Views.Caches.Add(typeof(FSAppointmentDet));

            int? lastApptID = null;
            var apptLines = AppointmentEntry.GetRelatedApptLines(this, soDet.SODetID,
                        excludeSpecificApptLine: false, apptDetID: null, onlyMarkForPOLines: false, sortResult: false);

            RelatedApptSummary summ = CalculateRelatedApptSummary(this, soDet.SODetID, soDet, null, recalculateValues: false);

            foreach (FSAppointmentDet apptLine in apptLines.Where(x => x.Status == FSAppointmentDet.status.NOT_STARTED
                                                                    || x.Status == FSAppointmentDet.status.WaitingForPO
                                                                    || x.Status == FSAppointmentDet.status.RequestForPO)
                                                           .OrderBy(x => x.AppointmentID))
            {
                if (lastApptID == null || lastApptID != apptLine.AppointmentID)
                {
                    lastApptID = apptLine.AppointmentID;
                    appointmentView.Current = (FSAppointment)PXParentAttribute.SelectParent(apptDetView.Cache, apptLine, typeof(FSAppointment));
                }

                FSAppointmentDet apptLineCopy = (FSAppointmentDet)apptDetView.Cache.CreateCopy(apptLine);

                AccumulateRelatedApptLine(summ, apptLineCopy, -1);

                apptLineCopy.EnablePO = soDet.EnablePO;
                apptLineCopy.POSource = soDet.POSource;
                apptLineCopy.POVendorID = soDet.POVendorID;
                apptLineCopy.POVendorLocationID= soDet.POVendorLocationID;

                apptLineCopy.Status = soDet.EnablePO == true ? FSAppointmentDet.status.WaitingForPO : FSAppointmentDet.status.NOT_STARTED;

                string oldStatusValue = (string)apptDetView.Cache.GetValueOriginal<FSAppointmentDet.status>(apptLineCopy);

                AppointmentEntry.UpdateCanceledNotPerformed(apptDetView.Cache, apptLineCopy, appointmentView.Current, oldStatusValue);

                apptLineCopy = apptDetView.Update(apptLineCopy);

                AccumulateRelatedApptLine(summ, apptLineCopy, 1);

                //TODO: AC-169443 if you dont persist here only latest appointment changes are persisted
                apptDetView.Cache.Persist(apptLineCopy, PXDBOperation.Update);
            }

            UpdateRelatedApptSummaryFields(this.ServiceOrderDetails.Cache, soDet, summ);
        }

        public virtual RelatedApptSummary CalculateRelatedApptSummary(PXGraph graph, int? soDetID, FSSODet soDetRow, int? apptDetIDToIgnore, bool recalculateValues)
        {
            if (soDetRow != null && soDetRow.SODetID != soDetID)
            {
                return null;
            }

            if (soDetID == null || soDetID < 0)
            {
                return new RelatedApptSummary(soDetID);
            }

            RelatedApptSummary summ = null;

            if (soDetRow == null || recalculateValues == true)
            {
                summ = new RelatedApptSummary(soDetID);

                var relatedApptLines = PXSelect<FSAppointmentDet,
                                Where<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                    And2<Where<Required<FSAppointmentDet.appDetID>, IsNull,
                                            Or<FSAppointmentDet.appDetID, NotEqual<Required<FSAppointmentDet.appDetID>>>>,
                                    And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                                    And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.NotPerformed>>>>>>.
                            Select(graph, soDetID, apptDetIDToIgnore, apptDetIDToIgnore);

                foreach (FSAppointmentDet apptLine in relatedApptLines)
                {
                    AccumulateRelatedApptLine(summ, apptLine, 1);
                }
            }
            else
            {
                summ = InitializeRelatedApptSummary(soDetRow);
            }

            return summ;
        }

        public virtual RelatedApptSummary InitializeRelatedApptSummary(FSSODet soDetRow)
        {
            RelatedApptSummary summ = new RelatedApptSummary(soDetRow.SODetID);

            summ.ApptCntr = (int)soDetRow.ApptCntr;
            summ.ApptCntrIncludingRequestPO = summ.ApptCntr;

            summ.ApptEstimatedDuration = (int)soDetRow.ApptEstimatedDuration;
            summ.ApptActualDuration = (int)soDetRow.ApptDuration;

            summ.ApptEffTranQty = (decimal)soDetRow.ApptQty;
            //summ.BaseApptEffTranQty = (decimal)soDetRow.BaseApptQty;

            summ.CuryApptEffTranAmt = (decimal)soDetRow.CuryApptTranAmt;
            summ.ApptEffTranAmt = (decimal)soDetRow.ApptTranAmt;

            return summ;
        }

        public virtual FSSODet UpdateRelatedApptSummaryFields(PXCache cache, FSSODet soDetRow, RelatedApptSummary summ)
        {
            if (soDetRow.SODetID != summ.SODetID)
            {
                return soDetRow;
            }

            soDetRow = (FSSODet)cache.CreateCopy(soDetRow);

            soDetRow.ApptCntr = summ.ApptCntr;

            soDetRow.ApptEstimatedDuration = summ.ApptEstimatedDuration;
            soDetRow.ApptDuration = summ.ApptActualDuration;

            soDetRow.ApptQty = summ.ApptEffTranQty;
            //soDetRow.BaseApptQty = summ.BaseApptEffTranQty;

            soDetRow.CuryApptTranAmt = summ.CuryApptEffTranAmt;
            soDetRow.ApptTranAmt = summ.ApptEffTranAmt;

            return (FSSODet)cache.Update(soDetRow);
        }

        public virtual void AccumulateRelatedApptLine(RelatedApptSummary summ, FSAppointmentDet apptLine, int invtMult)
        {
            if (apptLine.LineType != ID.LineType_ALL.SERVICE
                    && apptLine.LineType != ID.LineType_ALL.NONSTOCKITEM
                        && apptLine.LineType != ID.LineType_ALL.INVENTORY_ITEM)
            {
                return;
            }

            if (apptLine.Status == FSAppointmentDet.status.CANCELED
                    || apptLine.Status == FSAppointmentDet.status.NOT_PERFORMED)
            {
                return;
            }

            if (apptLine.Status != FSAppointmentDet.status.RequestForPO)
            {
                summ.ApptCntr += 1 * invtMult;

                summ.ApptEstimatedDuration += (int)apptLine.EstimatedDuration * invtMult;
                summ.ApptEstimatedQty += (decimal)apptLine.EstimatedQty * invtMult;

                summ.ApptActualDuration += (int)apptLine.ActualDuration * invtMult;
                summ.ApptActualQty += (decimal)apptLine.ActualQty * invtMult;

                if (apptLine.Status == FSAppointmentDet.status.NOT_STARTED)
                {
                    summ.ApptEffTranDuration += (int)apptLine.EstimatedDuration * invtMult;
                }
                else
                {
                    summ.ApptEffTranDuration += (int)apptLine.ActualDuration * invtMult;
                }

                summ.ApptEffTranQty += (decimal)apptLine.BillableQty * invtMult;
                summ.BaseApptEffTranQty += (decimal)apptLine.BaseBillableQty * invtMult;

                summ.CuryApptEffTranAmt += (decimal)apptLine.CuryBillableTranAmt * invtMult;
                summ.ApptEffTranAmt += (decimal)apptLine.BillableTranAmt * invtMult;
            }

            summ.ApptCntrIncludingRequestPO += 1 * invtMult;
        }

        public virtual void UpdateRelatedApptSummaryFields(PXCache AppointmentDetCache, FSAppointmentDet apptLine, PXCache SODetCache, FSSODet soLine)
        {
            PXEntryStatus status = AppointmentDetCache.GetStatus(apptLine);

            RelatedApptSummary summ = CalculateRelatedApptSummary(this, soLine.SODetID, soLine, apptLine.AppDetID, recalculateValues: false);

            if (status == PXEntryStatus.Inserted)
            {
                AccumulateRelatedApptLine(summ, apptLine, 1);
            }
            else if (status == PXEntryStatus.Updated)
            {
                FSAppointmentDet oldApptLine = (FSAppointmentDet)AppointmentDetCache.GetOriginal(apptLine);
                AccumulateRelatedApptLine(summ, oldApptLine, -1);

                AccumulateRelatedApptLine(summ, apptLine, 1);
            }
            else if (status == PXEntryStatus.Deleted)
            {
                AccumulateRelatedApptLine(summ, apptLine, -1);
            }

            soLine = UpdateRelatedApptSummaryFields(SODetCache, soLine, summ);

            if (apptLine.Status != ID.Status_AppointmentDet.NOT_FINISHED)
            {
                SODetCache.SetDefaultExt<FSSODet.status>(soLine);
            }
        }
        public virtual string GetLineDisplayHint(PXGraph graph, string lineRefNbr, string lineDescr, int? inventoryID)
        {
            return MessageHelper.GetLineDisplayHint(graph, lineRefNbr, lineDescr, inventoryID);
        }

        public virtual void ValidateCustomerBillingCycle(PXCache cache, Object row, int? customerID, FSSrvOrdType fsSrvOrdTypeRow, FSSetup setupRecordRow)
        {
            ServiceOrderCore.ValidateCustomerBillingCycle<FSServiceOrder.customerID>(cache, row, customerID, fsSrvOrdTypeRow, setupRecordRow);
        }
        #endregion

        #region Static Methods
        public static bool ShouldShowMarkForPOFieldsInt(SOOrderType currentSOOrderType)
        {
            return currentSOOrderType?.RequireShipping ?? false;
        }
        #endregion

        #region Events
        #region FSServiceOrder

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<FSServiceOrder, FSServiceOrder.salesPersonID> e)
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

        protected virtual void _(Events.FieldDefaulting<FSServiceOrder, FSServiceOrder.status> e)
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

        protected virtual void _(Events.FieldDefaulting<FSServiceOrder, FSServiceOrder.quote> e)
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
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying

        protected virtual void _(Events.FieldVerifying<FSServiceOrder, FSServiceOrder.hold> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

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

        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.projectID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            ServiceOrderCore.FSServiceOrder_ProjectID_FieldUpdated_PartialHandler(fsServiceOrderRow, ServiceOrderDetails);

            ContractRelatedToProject.Current = ContractRelatedToProject.Select(fsServiceOrderRow.ProjectID);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.branchID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderCore.FSServiceOrder_BranchID_FieldUpdated_PartialHandler((FSServiceOrder)e.Row, ServiceOrderDetails);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.wFStageID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            WFStageRelated.Current = WFStageRelated.Select(fsServiceOrderRow.WFStageID);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.orderDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

            cache.SetDefaultExt<FSServiceOrder.billContractPeriodID>(fsServiceOrderRow);

            if (SharedFunctions.TryParseHandlingDateTime(cache, e.OldValue) != fsServiceOrderRow.OrderDate)
            {
                ServiceOrderCore.RefreshSalesPricesInTheWholeDocument(ServiceOrderDetails);

                foreach (FSSODet fsSODetRow in ServiceOrderDetails.Select())
                {
                    ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetRow, ServiceOrderRecords.Current.OrderDate);
                    ServiceOrderDetails.Update(fsSODetRow);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.billServiceContractID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

            cache.SetDefaultExt<FSServiceOrder.billContractPeriodID>(fsServiceOrderRow);

            if (StandardContractRelated.Current != null
                && StandardContractRelated.Current.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
            {
                cache.SetValueExt<FSServiceOrder.projectID>(fsServiceOrderRow, StandardContractRelated.Current.ProjectID);
                cache.SetValueExt<FSServiceOrder.dfltProjectTaskID>(fsServiceOrderRow, StandardContractRelated.Current.DfltProjectTaskID);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.billContractPeriodID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

            if ((int?)e.OldValue == fsServiceOrderRow.BillContractPeriodID)
            {
                return;
            }

            StandardContractPeriod.Current = StandardContractPeriod.Select();
            StandardContractPeriodDetail.Current = StandardContractPeriodDetail.Select();

            foreach (FSSODet fsSODetRow in ServiceOrderDetails.Select())
            {
                ServiceOrderDetails.Cache.SetDefaultExt<FSSODet.contractRelated>(fsSODetRow);
                ServiceOrderDetails.Cache.Update(fsSODetRow);
            }

            cache.SetValueExtIfDifferent<FSServiceOrder.hold>(fsServiceOrderRow, fsServiceOrderRow.BillServiceContractID != null
                                                                        && fsServiceOrderRow.BillContractPeriodID == null);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.allowInvoice> e)
        {
            if (e.Row == null)
            {
                return;
            }

            updateContractPeriod = true;
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.billCustomerID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

            ServiceOrderCore.FSServiceOrder_BillCustomerID_FieldUpdated_Handler(cache, e.Args);

            foreach (FSSODet fsSODetRow in ServiceOrderDetails.Select())
            {
                fsSODetRow.BillCustomerID = ServiceOrderRecords.Current.CustomerID;
                ServiceOrderDetails.Update(fsSODetRow);
            }

            BillingCycleRelated.Current = BillingCycleRelated.Select();
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.contactID> e)
        {
            ServiceOrderCore.FSServiceOrder_ContactID_FieldUpdated_Handler(this, e.Args, ServiceOrderTypeSelected.Current);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.locationID> e)
        {
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

            ServiceOrderCore.FSServiceOrder_LocationID_FieldUpdated_Handler(cache, e.Args);

            if (fsServiceOrderRow.LocationID == null)
            {
                cache.RaiseFieldUpdated<FSServiceOrder.customerID>(fsServiceOrderRow, fsServiceOrderRow.CustomerID);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.customerID> e)
        {
            DateTime? orderDate = null;
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

            if (fsServiceOrderRow != null)
            {
                orderDate = fsServiceOrderRow.OrderDate;
            }

            ServiceOrderCore.FSServiceOrder_CustomerID_FieldUpdated_Handler(cache,
                                                                            fsServiceOrderRow,
                                                                            ServiceOrderTypeSelected.Current,
                                                                            ServiceOrderDetails,
                                                                            null,
                                                                            ServiceOrderAppointments.Select(),
                                                                            (int?)e.Args.OldValue,
                                                                            orderDate,
                                                                            allowCustomerChange,
                                                                            BillCustomer.Current);

            ValidateCustomerBillingCycle(cache, e.Row, e.Row.CustomerID, ServiceOrderTypeSelected.Current, SetupRecord.Current);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.branchLocationID> e)
        {
            ServiceOrderCore.FSServiceOrder_BranchLocationID_FieldUpdated_Handler(this,
                                                                                  e.Args,
                                                                                  ServiceOrderTypeSelected.Current,
                                                                                  CurrentServiceOrder);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.assignedEmpID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            // This update only applies if the assignee employee is edited from the Service Order screen.
            updateSOCstmAssigneeEmpID = e.ExternalCall;
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.curyBillableOrderTotal> e)
        {
            if (e.Row != null && ServiceOrderTypeSelected.Current != null && ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
                e.Row.CuryDocTotal = e.Row.CuryBillableOrderTotal - e.Row.CuryDiscTot;
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.curyDiscTot> e)
        {
            if (e.Row != null && ServiceOrderTypeSelected.Current != null && ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
                e.Row.CuryDocTotal = e.Row.CuryBillableOrderTotal - e.Row.CuryDiscTot;
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSServiceOrder> e)
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

        protected virtual void _(Events.RowSelected<FSServiceOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

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
                                        && fsServiceOrderRow.AllowInvoice == false
                                        && cache.GetStatus(e.Row) != PXEntryStatus.Inserted
                                        && ServiceOrderTypeSelected.Current?.Active == true);

            ServiceOrderCore.FSServiceOrder_RowSelected_PartialHandler(this,
                                                                       cache,
                                                                       fsServiceOrderRow,
                                                                       null,
                                                                       ServiceOrderTypeSelected.Current,
                                                                       BillingCycleRelated.Current,
                                                                       ContractRelatedToProject?.Current,
                                                                       ServiceOrderAppointments.Select().Count,
                                                                       fsServiceOrderRow.LineCntr == null ? 0 : (int)fsServiceOrderRow.LineCntr,
                                                                       ServiceOrderDetails.Cache,
                                                                       ServiceOrderAppointments.Cache,
                                                                       ServiceOrderEquipment.Cache,
                                                                       ServiceOrderEmployees.Cache,
                                                                       ServiceOrder_Contact.Cache,
                                                                       ServiceOrder_Address.Cache,
                                                                       ServiceOrderRecords.Current.IsCalledFromQuickProcess,
                                                                       allowCustomerChange);

            PXUIFieldAttribute.SetVisible<FSSODet.equipmentAction>(ServiceOrderDetails.Cache, null, ServiceOrderTypeSelected.Current?.PostToSOSIPM == true);

            bool showPOFields = ServiceOrderTypeSelected.Current?.PostToSOSIPM == true;
            ServiceOrderAppointmentHandlers.SetVisiblePODetFields(ServiceOrderDetails.Cache, showPOFields);

            bool showMarkForPO = ShouldShowMarkForPOFields();
            PXUIFieldAttribute.SetVisible<FSSODet.enablePO>(ServiceOrderDetails.Cache, null, showMarkForPO);
            PXUIFieldAttribute.SetVisible<FSSODet.pOCreate>(ServiceOrderDetails.Cache, null, showMarkForPO);
            PXUIFieldAttribute.SetVisible<FSSODetSplit.pOCreate>(Splits.Cache, null, showMarkForPO);

            Caches[typeof(FSContact)].AllowUpdate = fsServiceOrderRow.AllowOverrideContactAddress == true && Caches[typeof(FSContact)].AllowUpdate == true;
            Caches[typeof(FSAddress)].AllowUpdate = fsServiceOrderRow.AllowOverrideContactAddress == true && Caches[typeof(FSContact)].AllowUpdate == true;

            PXUIFieldAttribute.SetEnabled<FSManufacturer.allowOverrideContactAddress>(cache, fsServiceOrderRow, !(fsServiceOrderRow.CustomerID == null && fsServiceOrderRow.ContactID == null));

            if (StandardContractRelated.Current != null
                && StandardContractRelated.Current.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
            {
                PXUIFieldAttribute.SetEnabled<FSServiceOrder.projectID>(cache, fsServiceOrderRow, false);
            }

            PXUIFieldAttribute.SetVisible<FSPostDet.invoiceReferenceNbr>(ServiceOrderPostedIn.Cache, null, ServiceOrderTypeSelected.Current?.PostTo != ID.SrvOrdType_PostTo.PROJECTS);
            PXUIFieldAttribute.SetVisible<FSPostDet.iNPostDocReferenceNbr>(ServiceOrderPostedIn.Cache, null, ServiceOrderTypeSelected.Current?.PostTo == ID.SrvOrdType_PostTo.PROJECTS);

            bool inventoryAndEquipmentModuleEnabled = PXAccess.FeatureInstalled<FeaturesSet.inventory>()
                                                            && PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();

            PXUIFieldAttribute.SetVisibility<FSSODet.comment>(ServiceOrderDetails.Cache, null, inventoryAndEquipmentModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetVisibility<FSSODet.equipmentAction>(ServiceOrderDetails.Cache, null, inventoryAndEquipmentModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetVisibility<FSSODet.newTargetEquipmentLineNbr>(ServiceOrderDetails.Cache, null, inventoryAndEquipmentModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

            EnableDisableDocumentByWorkflowStage(cache, WFStageRelated?.Current);
        }

        protected virtual void _(Events.RowInserting<FSServiceOrder> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSServiceOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;

            SharedFunctions.InitializeNote(cache, e.Args);

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (fsServiceOrderRow.SOID < 0)
            {
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, fsServiceOrderRow, BillingCycleRelated.Current, null, null, DisableServiceOrderUnboundFieldCalc);
            }
        }

        protected virtual void _(Events.RowUpdating<FSServiceOrder> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSServiceOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
        }

        protected virtual void _(Events.RowDeleting<FSServiceOrder> e)
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

        protected virtual void _(Events.RowDeleted<FSServiceOrder> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSServiceOrder> e)
        {
            if (e.Row == null)
                return;

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            PXCache cache = e.Cache;

            // SrvOrdType is key field
            if (string.IsNullOrWhiteSpace(((FSServiceOrder)e.Row).SrvOrdType))
            {
                GraphHelper.RaiseRowPersistingException<FSServiceOrder.srvOrdType>(cache, fsServiceOrderRow);
            }

            ServiceOrderCore.FSServiceOrder_RowPersisting_Handler((ServiceOrderEntry)cache.Graph,
                                                                  cache,
                                                                  e.Args,
                                                                  ServiceOrderTypeSelected.Current,
                                                                  ServiceOrderDetails,
                                                                  ServiceOrderAppointments,
                                                                  GraphAppointmentEntryCaller,
                                                                  ForceAppointmentCheckings);

            ValidateCustomerBillingCycle(cache, e.Row, e.Row.CustomerID, ServiceOrderTypeSelected.Current, SetupRecord.Current);

            bool isEnabledCustomerID = ServiceOrderCore.AllowEnableCustomerID(fsServiceOrderRow);

            PXDefaultAttribute.SetPersistingCheck<FSServiceOrder.customerID>(cache, fsServiceOrderRow, fsServiceOrderRow.BAccountRequired != false && isEnabledCustomerID ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<FSServiceOrder.locationID>(cache, fsServiceOrderRow, fsServiceOrderRow.BAccountRequired != false ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            if (e.Row != null && e.Operation != PXDBOperation.Delete)
            {
                SharedFunctions.ValidateDuplicateLineNbr(ServiceOrderDetails, null);
            }
        }

        protected virtual void _(Events.RowPersisted<FSServiceOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (e.TranStatus == PXTranStatus.Completed)
            {
                switch (e.Operation)
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

                        ClearFSDocExpenseReceipts(fsServiceOrderRow.NoteID);
                        ClearPrepayment(fsServiceOrderRow);
                        break;

                    case PXDBOperation.Insert:

                        if (fsServiceOrderRow.SourceRefNbr != null)
                        {
                            if (fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.OPPORTUNITY)
                            {
                                if (PXSelect<CROpportunity,
                                    Where<
                                        CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>
                                    .Select(this, fsServiceOrderRow.SourceRefNbr).Count > 0)
                                {
                                    PXUpdate<
                                        Set<FSxCROpportunity.sOID, Required<FSxCROpportunity.sOID>,
                                        Set<FSxCROpportunity.srvOrdType, Required<FSxCROpportunity.srvOrdType>,
                                        Set<FSxCROpportunity.branchLocationID, Required<FSxCROpportunity.branchLocationID>,
                                        Set<FSxCROpportunity.sDEnabled, True>>>>,
                                    CROpportunity,
                                    Where<
                                        CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>
                                    .Update(this, fsServiceOrderRow.SOID, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.BranchLocationID, fsServiceOrderRow.SourceRefNbr);
                                }
                            }
                            else if (fsServiceOrderRow.SourceType == ID.SourceType_ServiceOrder.CASE)
                            {
                                if (PXSelect<CRCase,
                                    Where<
                                        CRCase.caseCD, Equal<Required<CRCase.caseCD>>>>
                                    .Select(this, fsServiceOrderRow.SourceRefNbr).Count > 0)
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
                                            CRCase.caseCD, Equal<Required<CRCase.caseCD>>>>
                                    .Update(this,
                                            fsServiceOrderRow.BranchLocationID,
                                            fsServiceOrderRow.SrvOrdType,
                                            fsServiceOrderRow.SOID,
                                            fsServiceOrderRow.AssignedEmpID,
                                            fsServiceOrderRow.ProblemID,
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
                                    .Update(this,
                                            fsServiceOrderRow.SrvOrdType,
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

                        foreach (FSSODet fsSODetRow in ServiceOrderDetails.Select().RowCast<FSSODet>().Where(x => x.IsService
                                                                                                                     && x.ContractRelated == true
                                                                                                                     && x.Status != ID.Status_AppointmentDet.CANCELED))
                        {
                            fsContractPeriodDetRow = graphServiceContract.ContractPeriodDetRecords
                                                     .Search<FSContractPeriodDet.inventoryID,
                                                             FSContractPeriodDet.SMequipmentID,
                                                             FSContractPeriodDet.billingRule>
                                                             (fsSODetRow.InventoryID,
                                                              fsSODetRow.SMEquipmentID,
                                                              fsSODetRow.BillingRule)
                                                     .FirstOrDefault();

                            StandardContractPeriodDetail.Cache.Clear();
                            StandardContractPeriodDetail.Cache.ClearQueryCacheObsolete();
                            StandardContractPeriodDetail.View.Clear();
                            StandardContractPeriodDetail.Select();

                            if (fsContractPeriodDetRow != null)
                            {
                                usedQty = fsContractPeriodDetRow.UsedQty + (multSign * fsSODetRow.CoveredQty)
                                            + (multSign * fsSODetRow.ExtraUsageQty);

                                usedTime = fsContractPeriodDetRow.UsedTime + (int?)(multSign * fsSODetRow.CoveredQty * 60)
                                            + (int?)(multSign * fsSODetRow.ExtraUsageQty * 60);

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

                        foreach (FSSODet fsSODetRow in ServiceOrderDetails.Select().RowCast<FSSODet>().Where(x => x.IsService
                                                                                                                    && x.ContractRelated == true
                                                                                                                    && x.Status != ID.Status_AppointmentDet.CANCELED))
                        {
                            fsContractPeriodDetRow = graphRouteServiceContractEntry.ContractPeriodDetRecords
                                                     .Search<FSContractPeriodDet.inventoryID,
                                                             FSContractPeriodDet.SMequipmentID,
                                                             FSContractPeriodDet.billingRule>
                                                             (fsSODetRow.InventoryID,
                                                              fsSODetRow.SMEquipmentID,
                                                              fsSODetRow.BillingRule)
                                                     .FirstOrDefault();

                            StandardContractPeriodDetail.Cache.Clear();
                            StandardContractPeriodDetail.Cache.ClearQueryCacheObsolete();
                            StandardContractPeriodDetail.View.Clear();
                            StandardContractPeriodDetail.Select();

                            if (fsContractPeriodDetRow != null)
                            {
                                usedQty = fsContractPeriodDetRow.UsedQty + (multSign * fsSODetRow.CoveredQty)
                                            + (multSign * fsSODetRow.ExtraUsageQty);

                                usedTime = fsContractPeriodDetRow.UsedTime + (int?)(multSign * fsSODetRow.CoveredQty * 60)
                                            + (int?)(multSign * fsSODetRow.ExtraUsageQty * 60);

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
            }

            // Asignee can only be updated from the Service Order & Sales Order screens
            // that's why it's not included in the ServiceCore RowPersisted handler
            UpdateAssignedEmpIDinSalesOrder((FSServiceOrder)e.Row);
        }

        #endregion
        #region FSSODet

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.acctID> e)
        {
            PXCache cache = e.Cache;
            ServiceOrderAppointmentHandlers.X_AcctID_FieldDefaulting<FSSODet>(cache, 
                                                                              e.Args,
                                                                              ServiceOrderTypeSelected.Current,
                                                                              CurrentServiceOrder.Current);
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.contractRelated> e)
        {
            FSSODet fsSODetRow = (FSSODet)e.Row;
            PXCache cache = e.Cache;
            bool duplicatedContractLine = false;

            if (e.Row == null || ServiceOrderTypeSelected.Current == null || CurrentServiceOrder.Current == null || StandardContractPeriodDetail == null)
            {
                return;
            }

            if (fsSODetRow.IsService)
            {
                if (CurrentServiceOrder.Current.BillServiceContractID == null ||
                    CurrentServiceOrder.Current.BillContractPeriodID == null ||
                    BillingCycleRelated.Current == null ||
                    BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER)
                {
                    e.NewValue = false;
                    return;
                }

                FSSODet fsSODetDuplicatedByContract = ServiceOrderDetails.Search<FSSODet.inventoryID, FSSODet.SMequipmentID, FSSODet.billingRule, FSSODet.contractRelated>
                                                                         (fsSODetRow.InventoryID, fsSODetRow.SMEquipmentID, fsSODetRow.BillingRule, true);

                duplicatedContractLine = fsSODetDuplicatedByContract != null && fsSODetDuplicatedByContract.LineNbr != fsSODetRow.LineNbr;
            }

            if (fsSODetRow.IsInventoryItem)
            {
                if (BillingCycleRelated.Current == null ||
                    BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER)
                {
                    e.NewValue = false;
                    return;
                }
            }

            e.NewValue = duplicatedContractLine == false
                                && StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetRow.InventoryID
                                                                                    && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetRow.SMEquipmentID
                                                                                            || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                    && ((FSContractPeriodDet)x).BillingRule == fsSODetRow.BillingRule).Count() == 1;
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.coveredQty> e)
        {
            if (e.Row == null || BillingCycleRelated.Current == null)
            {
                return;
            }

            FSSODet fsSODetRow = (FSSODet)e.Row;
            PXCache cache = e.Cache;

            if (fsSODetRow.IsService == true)
            {

                if (BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER ||
                    fsSODetRow.ContractRelated == false)
                {
                    e.NewValue = 0m;
                    return;
                }

                FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetRow.InventoryID
                                                                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetRow.SMEquipmentID
                                                                                                                                            || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                                                                    && ((FSContractPeriodDet)x).BillingRule == fsSODetRow.BillingRule).FirstOrDefault();

                if (fsContractPeriodDetRow != null)
                {
                    if (fsSODetRow.BillingRule == ID.BillingRule.TIME)
                    {
                        e.NewValue = fsContractPeriodDetRow.RemainingTime - fsSODetRow.EstimatedDuration >= 0 ? fsSODetRow.EstimatedQty : fsContractPeriodDetRow.RemainingTime / 60;
                    }
                    else
                    {
                        e.NewValue = fsContractPeriodDetRow.RemainingQty - fsSODetRow.EstimatedQty >= 0 ? fsSODetRow.EstimatedQty : fsContractPeriodDetRow.RemainingQty;
                    }
                }
                else
                {
                    e.NewValue = 0m;
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.curyExtraUsageUnitPrice> e)
        {
            if (e.Row == null || BillingCycleRelated.Current == null)
            {
                return;
            }

            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.IsService == true)
            {
                if (BillingCycleRelated.Current.BillingBy != ID.Billing_By.SERVICE_ORDER
                    || fsSODetRow.ContractRelated == false)
                {
                    e.NewValue = 0m;
                    return;
                }

                FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetRow.InventoryID
                                                                                                                        && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetRow.SMEquipmentID
                                                                                                                                || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                                                        && ((FSContractPeriodDet)x).BillingRule == fsSODetRow.BillingRule).FirstOrDefault();

                if (fsContractPeriodDetRow != null)
                {
                    e.NewValue = fsContractPeriodDetRow.OverageItemPrice;
                }
                else
                {
                    e.NewValue = 0m;
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.subID> e)
        {
            PXCache cache = e.Cache;

            ServiceOrderAppointmentHandlers.X_SubID_FieldDefaulting<FSSODet>(cache,
                                                                             e.Args,
                                                                             ServiceOrderTypeSelected.Current,
                                                                             CurrentServiceOrder.Current);
        }
        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.enablePO> e)
        {
            if (e.Row == null 
                || ServiceOrderTypeSelected.Current == null 
                || ServiceOrderTypeSelected.Current.PostTo != ID.SrvOrdType_PostTo.SALES_ORDER_MODULE)
            {
                return;
            }

            SOOrderType soOrderType = postSOOrderTypeSelected.Current;

            if (soOrderType != null && !(soOrderType.RequireShipping == true && soOrderType.RequireLocation == false && soOrderType.RequireAllocation == false))
            {
                e.NewValue = false;
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.poVendorID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.EnablePO == false || fsSODetRow.InventoryID == null)
            {
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.poVendorLocationID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.EnablePO == false || fsSODetRow.InventoryID == null || fsSODetRow.POVendorID == null)
            {
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.curyUnitPrice> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            var row = (FSSODet)e.Row;
            FSServiceOrder fsServiceOrderRow = CurrentServiceOrder.Current;

            bool unitPriceEqualsToUnitCost = ServiceOrderTypeSelected.Current?.PostTo == ID.SrvOrdType_PostTo.PROJECTS
                                                    && ServiceOrderTypeSelected.Current?.BillingType == ID.SrvOrdType_BillingType.COST_AS_COST;

            if (row.SkipUnitPriceCalc == true)
            {
                e.NewValue = row.AlreadyCalculatedUnitPrice;
            }
            else if (unitPriceEqualsToUnitCost == false)
            {
                var currencyInfo = ExtensionHelper.SelectCurrencyInfo(currencyInfoView, fsServiceOrderRow.CuryInfoID);

                ServiceOrderAppointmentHandlers.X_CuryUnitPrice_FieldDefaulting<FSSODet,
                                                                                FSSODet.curyUnitPrice>
                                                                                (cache,
                                                                                 e.Args,
                                                                                 row.EstimatedQty,
                                                                                 fsServiceOrderRow.OrderDate,
                                                                                 fsServiceOrderRow,
                                                                                 null,
                                                                                 currencyInfo);
            }
            else if (unitPriceEqualsToUnitCost == true)
            {
                e.NewValue = row.CuryUnitCost ?? 0m;
                e.Cancel = row.CuryUnitCost != null;
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.curyUnitCost> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (string.IsNullOrEmpty(fsSODetRow.UOM) == false && fsSODetRow.InventoryID != null)
            {
                object unitcost;
                cache.RaiseFieldDefaulting<FSSODet.unitCost>(e.Row, out unitcost);

                if (unitcost != null && (decimal)unitcost != 0m)
                {
                    decimal newval = INUnitAttribute.ConvertToBase<FSSODet.inventoryID, FSSODet.uOM>(cache, fsSODetRow, (decimal)unitcost, INPrecision.NOROUND);
                    CM.PXDBCurrencyAttribute.CuryConvCury(e.Cache, fsSODetRow, newval, out newval, CommonSetupDecPl.PrcCst);
                    e.NewValue = newval;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.uOM> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODet fsSODetRow = e.Row;

            if (fsSODetRow.IsService || fsSODetRow.IsInventoryItem)
            {
                ServiceOrderAppointmentHandlers.X_UOM_FieldDefaulting<FSSODet>(e.Cache, e.Args);
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.costCodeID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            ServiceOrderCore.SetCostCodeDefault(fsSODetRow, CurrentServiceOrder.Current?.ProjectID, ServiceOrderTypeSelected.Current, e.Args);
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.unitCost> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.Row.IsInventoryItem)
            {
                INItemSite inItemSiteRow = PXSelect<INItemSite,
                                           Where<
                                               INItemSite.inventoryID, Equal<Required<FSSODet.inventoryID>>,
                                               And<
                                                   INItemSite.siteID, Equal<Required<FSSODet.siteID>>>>>
                                           .Select(this, e.Row.InventoryID, e.Row.SiteID);

                e.NewValue = inItemSiteRow?.TranUnitCost;
            }
            else
            {
                InventoryItem inventoryItemRow = InventoryItem.PK.Find(this, e.Row.InventoryID);
                if (inventoryItemRow != null)
                {
                    if (inventoryItemRow.StdCostDate <= ServiceOrderRecords.Current?.OrderDate)
                        e.NewValue = inventoryItemRow.StdCost;
                    else
                        e.NewValue = inventoryItemRow.LastStdCost;
                }   
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.siteID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (ServiceOrderAppointmentHandlers.IsInventoryLine(fsSODetRow.LineType) == false
                || fsSODetRow.InventoryID == null)
            {
                e.NewValue = null;
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSODet, FSSODet.status> e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = ServiceOrderCore.GetSODetStatus(e.Row);
        }

        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying

        protected virtual void _(Events.FieldVerifying<FSSODet, FSSODet.billingRule> e)
        {
            PXCache cache = e.Cache;

            ServiceOrderAppointmentHandlers.X_BillingRule_FieldVerifying<FSSODet>(cache, e.Args);
        }

        protected virtual void _(Events.FieldVerifying<FSSODet, FSSODet.curyUnitPrice> e)
        {
            if (e.Row == null)
                return;

            if (e.Row.BillingRule == ID.BillingRule.NONE
                    && e.Row.InventoryID != null)
            {
                e.Row.ManualPrice = false;
                e.NewValue = 0m;
                return;
            }

            if (IsManualPriceFlagNeeded(e.Cache, e.Row))
                e.Row.ManualPrice = true;
        }

        protected virtual void _(Events.FieldVerifying<FSSODet, FSSODet.curyExtPrice> e)
        {
            if (e.Row == null)
                return;

            if (IsManualPriceFlagNeeded(e.Cache, e.Row))
                e.Row.ManualPrice = true;
        }

        protected virtual void _(Events.FieldVerifying<FSSODet, FSSODet.enablePO> e)
        {
            if ((bool)e.NewValue == false
                    && (e.Row.POType != null || e.Row.PONbr != null))
            {
                throw new PXSetPropertyException(TX.Error.CannotUnselectMarkForPOBecausePOIsAlreadyCreated, PXErrorLevel.Error);
            }

            if (e.Row.ApptCntr > 1 && GraphAppointmentEntryCaller != null)
            {
                throw new PXSetPropertyException(TX.Error.CannotChangeMarkForPOInSrvOrdLine, PXErrorLevel.Error);
            }

            POCreateVerifyValue(e.Cache, e.Row, (bool?)e.NewValue);
        }

        protected virtual void _(Events.FieldVerifying<FSSODet, FSSODet.curyBillableExtPrice> e)
        {
            if (e.Row == null)
                return;

            if (e.Row.BillingRule == ID.BillingRule.NONE)
                e.NewValue = 0m;
        }

        protected virtual void _(Events.FieldVerifying<FSSODet, FSSODet.discPct> e)
        {
            if (e.Row == null)
                return;

            if (e.Row.BillingRule == ID.BillingRule.NONE)
                e.NewValue = 0m;
        }

        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.estimatedQty> e)
        {
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSSODet.curyUnitPrice>(e.Cache, e.Args);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.estimatedDuration> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.IsService == true)
            {
                ServiceOrderAppointmentHandlers.X_Duration_FieldUpdated<FSSODet, FSSODet.estimatedQty>(e.Cache, e.Args, fsSODetRow.EstimatedDuration);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.SMequipmentID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetRow, ServiceOrderRecords.Current.OrderDate);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.componentID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetRow, ServiceOrderRecords.Current.OrderDate);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.equipmentLineRef> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            ServiceOrderCore.UpdateWarrantyFlag(cache, fsSODetRow, ServiceOrderRecords.Current.OrderDate);

            if (fsSODetRow.ComponentID == null)
            {
                fsSODetRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, fsSODetRow.SMEquipmentID, fsSODetRow.EquipmentLineRef);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.inventoryID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;
            FSServiceOrder fsServiceOrderRow = CurrentServiceOrder.Current;

            ServiceOrderAppointmentHandlers.X_InventoryID_FieldUpdated<FSSODet,
                                                                       FSSODet.acctID,
                                                                       FSSODet.subItemID,
                                                                       FSSODet.siteID,
                                                                       FSSODet.siteLocationID,
                                                                       FSSODet.uOM,
                                                                       FSSODet.estimatedDuration,
                                                                       FSSODet.estimatedQty,
                                                                       FSSODet.billingRule,
                                                                       ServiceOrderAppointmentHandlers.fakeField,
                                                                       ServiceOrderAppointmentHandlers.fakeField>
                                                                       (cache,
                                                                        e.Args,
                                                                        fsServiceOrderRow.BranchLocationID,
                                                                        BillCustomer.Current,
                                                                        useActualFields: false);

            if (fsSODetRow.IsInventoryItem == true)
            {
                SharedFunctions.UpdateEquipmentFields(this, cache, fsSODetRow, fsSODetRow.InventoryID);
            }

            cache.SetDefaultExt<FSSODet.curyUnitCost>(fsSODetRow);
            cache.SetDefaultExt<FSSODet.enablePO>(fsSODetRow);

            if (fsSODetRow.IsService && fsSODetRow.ContractRelated == true)
            {
                FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)StandardContractPeriodDetail.Select().Where(x => ((FSContractPeriodDet)x).InventoryID == fsSODetRow.InventoryID
                                                                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsSODetRow.SMEquipmentID
                                                                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                                                                && ((FSContractPeriodDet)x).BillingRule == fsSODetRow.BillingRule).FirstOrDefault();

                cache.SetValueExt<FSSODet.projectTaskID>(fsSODetRow, fsContractPeriodDetRow.ProjectTaskID);
                cache.SetValueExt<FSSODet.costCodeID>(fsSODetRow, fsContractPeriodDetRow.CostCodeID);
            }

            if ((e.ExternalCall == false
                    && fsSODetRow.InventoryID != null) || GraphAppointmentEntryCaller != null)
            {
                //In case ExternalCall == false (any change not coming from Service Order's UI), ShowPopupMessage = false to avoid
                //showing the Popup Message for the InventoryID field. By default it's set to true.

                foreach (PXSelectorAttribute s in cache.GetAttributes(typeof(FSSODet.inventoryID).Name).OfType<PXSelectorAttribute>())
                {
                    s.ShowPopupMessage = false;
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.staffID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.IsService == false)
            {
                return;
            }

            if (fsSODetRow.StaffID != null)
            {
                if (e.OldValue != null)
                {
                    FSSOEmployee fsSOEmployeeRow = PXSelect<FSSOEmployee,
                                                   Where<
                                                        FSSOEmployee.serviceLineRef, Equal<Required<FSSOEmployee.serviceLineRef>>,
                                                        And<FSSOEmployee.employeeID, Equal<Required<FSSOEmployee.employeeID>>,
                                                        And<FSSOEmployee.sOID, Equal<Current<FSServiceOrder.sOID>>>>>>
                                                   .Select(this, fsSODetRow.LineRef, e.OldValue);

                    fsSOEmployeeRow.EmployeeID = fsSODetRow.StaffID;
                    ServiceOrderEmployees.Update(fsSOEmployeeRow);
                }
                else
                {
                    if (this.IsContractBasedAPI
                            && string.IsNullOrEmpty(fsSODetRow.LineRef))
                    {
                        cache.RaiseRowInserting(fsSODetRow);
                    }

                    FSSOEmployee fsSOEmployeeRow = new FSSOEmployee()
                    {
                        EmployeeID = fsSODetRow.StaffID
                    };

                    fsSOEmployeeRow = ServiceOrderEmployees.Insert(fsSOEmployeeRow);
                    fsSOEmployeeRow.ServiceLineRef = fsSODetRow.LineRef;
                }
            }
            else
            {
                FSSOEmployee fsSOEmployeeRow = PXSelect<FSSOEmployee,
                                               Where<
                                                    FSSOEmployee.serviceLineRef, Equal<Required<FSSOEmployee.serviceLineRef>>,
                                                    And<FSSOEmployee.employeeID, Equal<Required<FSSOEmployee.employeeID>>,
                                                    And<FSSOEmployee.sOID, Equal<Current<FSServiceOrder.sOID>>>>>>
                                               .Select(this, fsSODetRow.LineRef, e.OldValue);

                ServiceOrderEmployees.Delete(fsSOEmployeeRow);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.lineType> e)
        {
            PXCache cache = e.Cache;

            ServiceOrderAppointmentHandlers.X_LineType_FieldUpdated<FSSODet>(cache, e.Args);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.isPrepaid> e)
        {
            PXCache cache = e.Cache;

            ServiceOrderAppointmentHandlers.X_IsPrepaid_FieldUpdated<FSSODet,
                                                                     FSSODet.manualPrice,
                                                                     FSSODet.isFree,
                                                                     FSSODet.estimatedDuration,
                                                                     ServiceOrderAppointmentHandlers.fakeField>
                                                                     (cache,
                                                                      e.Args,
                                                                      useActualField: false);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.manualPrice> e)
        {
            ServiceOrderAppointmentHandlers.X_ManualPrice_FieldUpdated<FSSODet, FSSODet.curyUnitPrice, FSSODet.curyBillableExtPrice>(e.Cache, e.Args);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.billingRule> e)
        {
            PXCache cache = e.Cache;

            ServiceOrderAppointmentHandlers.X_BillingRule_FieldUpdated<FSSODet,
                                                                       FSSODet.estimatedDuration,
                                                                       ServiceOrderAppointmentHandlers.fakeField,
                                                                       FSSODet.curyUnitPrice,
                                                                       FSSODet.isFree>
                                                                       (cache,
                                                                        e.Args,
                                                                        useActualField: false);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.uOM> e)
        {
            PXCache cache = e.Cache;

            ServiceOrderAppointmentHandlers.X_UOM_FieldUpdated<FSSODet.curyUnitPrice>(cache, e.Args);
            cache.SetDefaultExt<FSSODet.curyUnitCost>(e.Row);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.siteID> e)
        {
            ServiceOrderAppointmentHandlers.X_SiteID_FieldUpdated<FSSODet.curyUnitPrice, FSSODet.acctID, FSSODet.subID>(e.Cache, e.Args);

            e.Cache.SetDefaultExt<FSSODet.curyUnitCost>(e.Row);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.equipmentAction> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.IsInventoryItem == true && fsSODetRow.CreatedByScreenID == ID.ScreenID.SERVICE_ORDER)
            {
                SharedFunctions.ResetEquipmentFields(cache, fsSODetRow);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.billableQty> e)
        {
            PXCache cache = e.Cache;
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSSODet.curyUnitPrice>(cache, e.Args);
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.contractRelated> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if ((bool?)e.OldValue != ((FSSODet)e.Row).ContractRelated)
            {
                e.Cache.SetDefaultExt<FSSODet.curyUnitPrice>(e.Row);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.tranDesc> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODet fsSODetRow = e.Row;

            if (!String.IsNullOrEmpty(fsSODetRow.TranDesc) && fsSODetRow.LineType == null)
            {
                e.Cache.SetValueExt<FSSODet.lineType>(fsSODetRow, ID.LineType_ALL.INSTRUCTION);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.isFree> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.Row.IsFree == true)
            {
                e.Cache.SetValueExt<FSSODet.curyUnitPrice>(e.Row, 0m);
                e.Cache.SetValueExt<FSSODet.curyBillableExtPrice>(e.Row, 0m);
                e.Cache.SetValueExt<FSSODet.discPct>(e.Row, 0m);
                e.Cache.SetValueExt<FSSODet.curyDiscAmt>(e.Row, 0m);
                if (e.ExternalCall)
                {
                    e.Cache.SetValueExt<FSSODet.manualDisc>(e.Row, true);
                }
            }
            else
            {
                e.Cache.SetDefaultExt<FSSODet.curyUnitPrice>(e.Row);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.isBillable> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.Row.IsBillable == false || e.Row.BillingRule == ID.BillingRule.NONE)
            {
                if (e.Row.Status == ID.Status_SODet.CANCELED)
                {
                    e.Cache.SetValueExt<FSSODet.curyBillableExtPrice>(e.Row, 0m);
                    e.Cache.SetValueExt<FSSODet.discPct>(e.Row, 0m);
                    e.Cache.SetValueExt<FSSODet.curyDiscAmt>(e.Row, 0m);
                }
                else
                {
                    e.Cache.SetValueExt<FSSODet.isFree>(e.Row, true);
                }

                e.Cache.SetValueExt<FSSODet.contractRelated>(e.Row, false);
            }
            else
            {
                e.Cache.SetValueExt<FSSODet.isFree>(e.Row, false);
                e.Cache.SetValueExt<FSSODet.manualPrice>(e.Row, e.Row.IsExpenseReceiptItem == true);

                if (e.Row.IsExpenseReceiptItem == true)
                {
                    e.Cache.SetValueExt<FSSODet.curyUnitPrice>(e.Row, e.Row.CuryUnitCost);
                    e.Cache.SetValueExt<FSSODet.curyBillableExtPrice>(e.Row, e.Row.CuryExtCost);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSODet, FSSODet.status> e)
        {
            if (e.Row == null)
            {
                return;
            }

            string oldStatus = (string)e.OldValue;
            string newStatus = e.Row.Status;

            if (oldStatus != newStatus)
            {
                if (oldStatus == ID.Status_SODet.CANCELED
                        || newStatus == ID.Status_SODet.CANCELED)
                {
                    e.Cache.SetDefaultExt<FSSODet.isBillable>(e.Row);
                }
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSSODet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (GraphAppointmentEntryCaller == null)
            {
                using (new PXConnectionScope())
                {
                    if (fsSODetRow.IsService == true)
                    {
                        PXResultset<FSSOEmployee> fsSOEmployeeRows = null;


                        fsSOEmployeeRows = PXSelect<FSSOEmployee,
                                           Where<
                                                FSSOEmployee.sOID, Equal<Required<FSServiceOrder.sOID>>,
                                                And<FSSOEmployee.serviceLineRef, Equal<Required<FSSOEmployee.serviceLineRef>>>>>
                                           .Select(cache.Graph, fsSODetRow.SOID, fsSODetRow.LineRef);

                        if (fsSOEmployeeRows != null)
                        {
                            fsSODetRow.EnableStaffID = fsSOEmployeeRows.Count <= 1;

                            if (fsSOEmployeeRows.Count == 1)
                            {
                                fsSODetRow.StaffID = ((FSSOEmployee)fsSOEmployeeRows).EmployeeID;
                            }
                            else
                            {
                                fsSODetRow.StaffID = null;
                            }
                        }
                        else
                        {
                            fsSODetRow.EnableStaffID = fsSODetRow.SOID < 0;
                            fsSODetRow.StaffID = null;
                        }
                    }

                    SetSODetServiceLastReferencedBy(fsSODetRow);
                }
            }
        }

        protected virtual void _(Events.RowSelected<FSSODet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;
            FSServiceOrder serviceOrderRow = CurrentServiceOrder.Current;

            EnableDisable_SODetLine(this, cache, fsSODetRow, ServiceOrderTypeSelected.Current, serviceOrderRow);

            // Move the old code of SetEnabled and SetPersistingCheck in previous methods to this new generic method
            // keeping the generic convention.
            ServiceOrderAppointmentHandlers.X_RowSelected<FSSODet>(cache,
                                                                   e.Args,
                                                                   serviceOrderRow,
                                                                   ServiceOrderTypeSelected.Current,
                                                                   disableSODetReferenceFields: false,
                                                                   docAllowsActualFieldEdition: false);

            POCreateVerifyValue(cache, fsSODetRow, fsSODetRow.EnablePO);

            bool includeIN = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
                                && PXAccess.FeatureInstalled<FeaturesSet.inventory>()
                                    && ServiceOrderTypeSelected.Current.AllowInventoryItems;

            FSLineType.SetLineTypeList<FSSODet.lineType>(ServiceOrderDetails.Cache, null, includeIN, false, false, true, false);

            if (ServiceOrderTypeSelected.Current != null 
                    && (ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS 
                        || serviceOrderRow.AllowInvoice == true))
            {
                PXUIFieldAttribute.SetEnabled<FSSODet.equipmentAction>(cache, null, false);
            }

            bool callDisableField = false;
            List<Type> ignoreFieldList = new List<Type>();

            if (fsSODetRow.Status == ID.Status_SODet.CANCELED)
            {
                ignoreFieldList.Add(typeof(FSSODet.status));
                callDisableField = true;
            }

            if (fsSODetRow.IsExpenseReceiptItem == true) 
            {
                if (ProjectDefaultAttribute.IsNonProject(serviceOrderRow.ProjectID))
                {
                    ignoreFieldList.Add(typeof(FSSODet.isBillable));
                }

                if (fsSODetRow.IsBillable == true)
                {
                    ignoreFieldList.Add(typeof(FSSODet.curyUnitPrice));
                    ignoreFieldList.Add(typeof(FSSODet.manualPrice));
                    ignoreFieldList.Add(typeof(FSSODet.curyBillableExtPrice));
                    ignoreFieldList.Add(typeof(FSSODet.curyExtCost));
                    ignoreFieldList.Add(typeof(FSSODet.discPct));
                    ignoreFieldList.Add(typeof(FSSODet.curyDiscAmt));
                    ignoreFieldList.Add(typeof(FSSODet.isFree));
                    ignoreFieldList.Add(typeof(FSSODet.taxCategoryID));
                    ignoreFieldList.Add(typeof(FSSODet.acctID));
                    ignoreFieldList.Add(typeof(FSSODet.subID));
                }
                callDisableField = true;
            }

            if (callDisableField)
            {
                this.DisableAllDACFields(e.Cache, fsSODetRow, ignoreFieldList);
            }
        }

        protected virtual void _(Events.RowInserting<FSSODet> e)
        {
            PXCache cache = e.Cache;
            ServiceOrderHandlers.FSSODet_RowInserting(cache, e.Args);
        }

        protected virtual void _(Events.RowInserted<FSSODet> e)
        {
            PXCache cache = e.Cache;
            MarkHeaderAsUpdated(cache, e.Row);
        }

        protected virtual void _(Events.RowUpdating<FSSODet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.IsInventoryItem)
            {
                EquipmentHelper.CheckReplaceComponentLines<FSSODet, FSSODet.equipmentLineRef>(cache, ServiceOrderDetails.Select(), (FSSODet)e.NewRow);
            }
        }

        protected virtual void _(Events.RowUpdated<FSSODet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            MarkHeaderAsUpdated(cache, e.Row);

            ServiceOrderAppointmentHandlers.CheckIfManualPrice<FSSODet, FSSODet.estimatedQty>(cache, e.Args);
            ServiceOrderAppointmentHandlers.CheckSOIfManualCost(cache, e.Args);
        }

        protected virtual void _(Events.RowDeleting<FSSODet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (ServiceOrderCore.FSSODetLinkedToAppointments(this, fsSODetRow) == true)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.FSSODET_LINKED_TO_APPOINTMENTS, SharedFunctions.GetLineType(fsSODetRow.LineType)), PXErrorLevel.Error);
            }

            if (this.ServiceOrderRecords.Current != null
                    && this.ServiceOrderRecords.Current.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER
                        && e.ExternalCall == true)
            {
                if (IsThisLineRelatedToAsoLine(this.ServiceOrderRecords.Current, fsSODetRow))
                {
                    throw new PXException(TX.Error.FSSODET_LINE_IS_RELATED_TO_A_SOLINE, fsSODetRow.SourceLineNbr);
                }
            }

            if (fsSODetRow.IsService == true)
            {
                foreach (FSSOEmployee fsSOEmployeeRow in ServiceOrderEmployees.Select().RowCast<FSSOEmployee>().Where(y => y.ServiceLineRef == fsSODetRow.LineRef))
                {
                    ServiceOrderEmployees.Delete(fsSOEmployeeRow);
                }
            }
        }

        protected virtual void _(Events.RowDeleted<FSSODet> e)
        {
            PXCache cache = e.Cache;
            FSSODet fsSODetRow = e.Row as FSSODet;

            MarkHeaderAsUpdated(cache, e.Row);

            ClearTaxes(CurrentServiceOrder.Current);
        }

        protected virtual void _(Events.RowPersisting<FSSODet> e)
        {
            PXCache cache = e.Cache;
            FSSODet fsSODetRow = (FSSODet)e.Row;

            if (fsSODetRow.IsInventoryItem == true)
            {
                string errorMessage = string.Empty;

                if (e.Operation != PXDBOperation.Delete
                        && !SharedFunctions.AreEquipmentFieldsValid(cache, fsSODetRow.InventoryID, fsSODetRow.SMEquipmentID, fsSODetRow.NewTargetEquipmentLineNbr, fsSODetRow.EquipmentAction, ref errorMessage))
                {
                    cache.RaiseExceptionHandling<FSSODet.equipmentAction>(fsSODetRow, fsSODetRow.EquipmentAction, new PXSetPropertyException(errorMessage));
                }


                if (EquipmentHelper.CheckReplaceComponentLines<FSSODet, FSSODet.equipmentLineRef>(cache, ServiceOrderDetails.Select(), e.Row) == false)
                {
                    return;
                }
            }
            
            string oldStatus = (string)cache.GetValueOriginal<FSSODet.status>(fsSODetRow);

            if ((fsSODetRow.Status == ID.Status_SODet.CANCELED
                    || fsSODetRow.Status == ID.Status_SODet.COMPLETED)
                && fsSODetRow.Status != oldStatus
                && GraphAppointmentEntryCaller == null)
            {
                int countAppCancelComplete = -1;

                if (fsSODetRow.Status == ID.Status_SODet.CANCELED)
                {
                    countAppCancelComplete = PXSelect<FSAppointmentDet,
                                                Where<
                                                    FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                                    And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>>>>
                                            .Select(this, fsSODetRow.SODetID).Count();

                    if (countAppCancelComplete != 0)
                    {
                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.CANNOT_CANCEL_FSSODET_LINE, GetLineDisplayHint(this, fsSODetRow.LineRef, fsSODetRow.TranDesc, fsSODetRow.InventoryID)));
                    }
                }

                if (fsSODetRow.Status == ID.Status_SODet.COMPLETED)
                {
                    countAppCancelComplete = PXSelect<FSAppointmentDet,
                                                    Where<
                                                        FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                                        And<
                                                            Where<
                                                                FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.NotPerformed>,
                                                                And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.NotFinished>,
                                                                And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Completed>,
                                                                And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                                                                And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.requestForPO>>>>>>>>>
                                                .Select(this, fsSODetRow.SODetID).Count();

                    if (countAppCancelComplete != 0)
                    {
                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.CANNOT_COMPLETE_FSSODET_LINE, GetLineDisplayHint(this, fsSODetRow.LineRef, fsSODetRow.TranDesc, fsSODetRow.InventoryID)));
                    }
                }
            }

            ServiceOrderAppointmentHandlers.X_SetPersistingCheck<FSSODet>(cache, e.Args, CurrentServiceOrder.Current, ServiceOrderTypeSelected.Current);

            if (e.Operation == PXDBOperation.Update)
            {
                bool? oldEnablePO = (bool?)e.Cache.GetValueOriginal<FSSODet.enablePO>(e.Row);

                if (e.Row.EnablePO != oldEnablePO
                    && GraphAppointmentEntryCaller == null)
                {
                    UpdatePOOptionsInAppointmentRelatedLines(e.Row);
                }
            }
        }

        protected virtual void _(Events.RowPersisted<FSSODet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSODet fsSODetRow = e.Row;

            if (Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.ExpenseReceipt)
                && e.TranStatus == PXTranStatus.Completed
                && e.Operation == PXDBOperation.Delete
                && this.ServiceOrderRecords.Cache.GetStatus(CurrentServiceOrder.Current) != PXEntryStatus.Deleted
                && fsSODetRow.IsExpenseReceiptItem == true)
            {
                ClearFSDocExpenseReceipts(fsSODetRow.LinkedDocRefNbr);
            }
        }

        #endregion

        #region FSSOEmployee

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<FSSOEmployee, FSSOEmployee.employeeID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOEmployee fsSOEmployeeRow = (FSSOEmployee)e.Row;
            fsSOEmployeeRow.Type = SharedFunctions.GetBAccountType(this, fsSOEmployeeRow.EmployeeID);
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSSOEmployee> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSSOEmployee> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOEmployee fsSOEmployeeRow = (FSSOEmployee)e.Row;
            PXCache cache = e.Cache;

            PXUIFieldAttribute.SetEnabled<FSSOEmployee.employeeID>(cache, fsSOEmployeeRow, fsSOEmployeeRow.EmployeeID == null);
            PXUIFieldAttribute.SetEnabled<FSSOEmployee.comment>(cache, fsSOEmployeeRow, fsSOEmployeeRow.EmployeeID != null);
        }

        protected virtual void _(Events.RowInserting<FSSOEmployee> e)
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

        protected virtual void _(Events.RowInserted<FSSOEmployee> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSSOEmployee> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSSOEmployee> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSSOEmployee> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSSOEmployee> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSSOEmployee> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSSOEmployee> e)
        {
        }

        #endregion
        #region FSSOResource

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

        protected virtual void _(Events.RowSelecting<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSSOResource> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSOResource fsSOResourceRow = (FSSOResource)e.Row;
            PXCache cache = e.Cache;

            PXUIFieldAttribute.SetEnabled<FSSOResource.SMequipmentID>(cache, fsSOResourceRow, fsSOResourceRow.SMEquipmentID == null);
            PXUIFieldAttribute.SetEnabled<FSSOResource.qty>(cache, fsSOResourceRow, fsSOResourceRow.SMEquipmentID != null);
            PXUIFieldAttribute.SetEnabled<FSSOResource.comment>(cache, fsSOResourceRow, fsSOResourceRow.SMEquipmentID != null);
        }

        protected virtual void _(Events.RowInserting<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSSOResource> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSSOResource> e)
        {
        }

        #endregion
        #region FSPostDet

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

        protected virtual void _(Events.RowSelecting<FSPostDet> e)
        {
            FSPostDet fsPostDetRow = e.Row as FSPostDet;
            PXCache cache = e.Cache;

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

        protected virtual void _(Events.RowSelected<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSPostDet> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSPostDet> e)
        {
        }
        #endregion
        #region ARPayment

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

        protected virtual void _(Events.RowSelecting<ARPayment> e)
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

        protected virtual void _(Events.RowSelected<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowInserting<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowInserted<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowUpdating<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowUpdated<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowDeleting<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowDeleted<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowPersisting<ARPayment> e)
        {
        }

        protected virtual void _(Events.RowPersisted<ARPayment> e)
        {
        }

        #endregion

        #region FSSiteStatusFilter
        protected virtual void _(Events.RowSelected<FSSiteStatusFilter> e)
        {
            FSSiteStatusFilter row = (FSSiteStatusFilter)e.Row;
            bool includeIN = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
                                && PXAccess.FeatureInstalled<FeaturesSet.inventory>()
                                    && ServiceOrderTypeSelected.Current?.AllowInventoryItems == true;
            row.IncludeIN = includeIN;

            if (row != null && !includeIN)
            {
                row.OnlyAvailable = false;
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.onlyAvailable>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.inventory_Wildcard>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.mode>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.barCode>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.barCodeWildcard>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.siteID>(e.Cache, row, includeIN);
            }

            FSLineType.SetLineTypeList<FSSiteStatusFilter.lineType>(e.Cache,
                                                                  row,
                                                                  includeIN,
                                                                  false,
                                                                  false,
                                                                  false,
                                                                  true);
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

        public static decimal GetCuryDocTotal(decimal? curyLineTotal, decimal? curyDiscTotal, decimal? curyTaxTotal, decimal? curyInclTaxTotal)
        {
            return (curyLineTotal ?? 0) - (curyDiscTotal ?? 0) + (curyTaxTotal ?? 0) - (curyInclTaxTotal ?? 0);
        }

        public virtual void POCreateVerifyValue(PXCache sender, FSSODet row, bool? value)
        {
            POCreateVerifyValueInt<FSSODet.enablePO>(sender, row, row.InventoryID, value);
        }

        public static void POCreateVerifyValueInt<POCreateField>(PXCache sender, object row, int? inventoryID, bool? value)
            where POCreateField : IBqlField
        {
            if (row == null)
            {
                return;
            }

            if (inventoryID != null && value == true)
            {
                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, inventoryID);

                if (item != null && item.StkItem != true)
                {
                    if (item.KitItem == true)
                    {
                        throw new PXSetPropertyException(PX.Objects.SO.Messages.SOPOLinkNotForNonStockKit);
                    }
                    else if (item.NonStockShip != true || item.NonStockReceipt != true)
                    {
                        sender.RaiseExceptionHandling<POCreateField>(row, value, new PXSetPropertyException(PX.Objects.SO.Messages.NonStockShipReceiptIsOff, PXErrorLevel.Warning));
                    }
                }
            }
        }

        #region Selector Methods

        #region Staff Selector
        [PXCopyPasteHiddenView]
        public PXFilter<StaffSelectionFilter> StaffSelectorFilter;

        #region CacheAttached
        #region ServiceLineRef
        [PXString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Ref. Nbr.")]
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

        public virtual IEnumerable skillGridFilter()
        {
            return StaffSelectionHelper.SkillFilterDelegate(this, ServiceOrderDetails, StaffSelectorFilter, SkillGridFilter);
        }

        public virtual IEnumerable licenseTypeGridFilter()
        {
            return StaffSelectionHelper.LicenseTypeFilterDelegate(this, ServiceOrderDetails, StaffSelectorFilter, LicenseTypeGridFilter);
        }

        protected virtual IEnumerable staffRecords()
        {
            return StaffSelectionHelper.StaffRecordsDelegate(ServiceOrderEmployees,
                                                             SkillGridFilter,
                                                             LicenseTypeGridFilter,
                                                             StaffSelectorFilter);
        }


        protected virtual void _(Events.FieldUpdated<StaffSelectionFilter, StaffSelectionFilter.serviceLineRef> e)
        {
            if (e.Row == null)
            {
                return;
            }

            SkillGridFilter.Cache.Clear();
            LicenseTypeGridFilter.Cache.Clear();
            StaffRecords.Cache.Clear();
        }

        protected virtual void _(Events.RowUpdated<BAccountStaffMember> e)
        {
            BAccountStaffMember bAccountStaffMemberRow = (BAccountStaffMember)e.Row;

            if (StaffSelectorFilter.Current != null)
            {
                if (bAccountStaffMemberRow.Selected == true)
                {
                    if (ServiceOrderDetails.Current != null)
                    {
                        if (ServiceOrderDetails.Current.LineRef != StaffSelectorFilter.Current.ServiceLineRef)
                        {
                            ServiceOrderDetails.Current = ServiceOrderDetails.Search<FSSODet.lineRef>(StaffSelectorFilter.Current.ServiceLineRef);
                        }

                        if (ServiceOrderEmployees.Select().RowCast<FSSOEmployee>().Where(_ => _.ServiceLineRef == StaffSelectorFilter.Current.ServiceLineRef).Any() == false
                                && ServiceOrderDetails.Current.IsService == true)
                        {
                            ServiceOrderDetails.Current.StaffID = bAccountStaffMemberRow.BAccountID;
                        }
                        else
                        {
                            ServiceOrderDetails.Current.StaffID = null;
                        }
                    }

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
                                                    .Select(e.Cache.Graph, StaffSelectorFilter.Current.ServiceLineRef, StaffSelectorFilter.Current.ServiceLineRef, bAccountStaffMemberRow.BAccountID);

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
        [PXUIField(DisplayName = "Add Staff", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenStaffSelectorFromServiceTab()
        {
            ServiceOrder_Contact.Current = ServiceOrder_Contact.SelectSingle();
            ServiceOrder_Address.Current = ServiceOrder_Address.SelectSingle();
            
            if (ServiceOrder_Address.Current != null)
            {
                StaffSelectorFilter.Current.PostalCode = ServiceOrder_Address.Current.PostalCode;
                StaffSelectorFilter.Current.ProjectID = CurrentServiceOrder.Current.ProjectID;
                StaffSelectorFilter.Current.ScheduledDateTimeBegin = CurrentServiceOrder.Current.SLAETA;
            }

            FSSODet fsSODetRow = ServiceOrderDetails.Current;

            if (fsSODetRow != null && fsSODetRow.IsService)
            {
                StaffSelectorFilter.Current.ServiceLineRef = fsSODetRow.LineRef;
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
        [PXUIField(DisplayName = "Add Staff", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenStaffSelectorFromStaffTab()
        {
            ServiceOrder_Contact.Current = ServiceOrder_Contact.SelectSingle();
            ServiceOrder_Address.Current = ServiceOrder_Address.SelectSingle();

            if (ServiceOrder_Address.Current != null)
            {
                StaffSelectorFilter.Current.PostalCode = ServiceOrder_Address.Current.PostalCode;
                StaffSelectorFilter.Current.ProjectID = CurrentServiceOrder.Current.ProjectID;
                StaffSelectorFilter.Current.ScheduledDateTimeBegin = CurrentServiceOrder.Current.SLAETA;
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

            protected virtual void _(Events.RowSelected<FSServiceOrder> e)
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
                                                        && fsServiceOrderRow.BillServiceContractID == null
                                                            && fsServiceOrderRow.Status != ID.Status_ServiceOrder.CANCELED;

                if (currentSOOrderType.Current == null)
                {
                    currentSOOrderType.Current = currentSOOrderType.Select();
                }

                isSOInvoice = Base.ServiceOrderTypeSelected.Current?.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;

                Base.quickProcess.SetEnabled(enableQuickProcess);
                Base.quickProcess.SetVisible(enableQuickProcess);
            }

            protected virtual void _(Events.RowSelected<FSSrvOrdQuickProcessParams> e)
            {
                if (e.Row == null)
                {
                    return;
                }

                PXCache cache = e.Cache;
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

            protected virtual void _(Events.FieldUpdated<FSSrvOrdQuickProcessParams, FSSrvOrdQuickProcessParams.generateInvoiceFromServiceOrder> e)
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

            protected virtual void _(Events.FieldUpdated<FSSrvOrdQuickProcessParams, FSSrvOrdQuickProcessParams.sOQuickProcess> e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSSrvOrdQuickProcessParams fsSrvOrdQuickProcessParamsRow = (FSSrvOrdQuickProcessParams)e.Row;

                if (fsSrvOrdQuickProcessParamsRow.SOQuickProcess != (bool)e.OldValue)
                {
                    SetQuickProcessOptions(Base, e.Cache, fsSrvOrdQuickProcessParamsRow, true);
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
					Base.ServiceOrderDetails
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
            protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }

            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(FSServiceOrder))
                {
                    DocumentDate = typeof(FSServiceOrder.orderDate),
                    CuryDocBal = typeof(FSServiceOrder.curyDocTotal),
                    CuryDiscountLineTotal = typeof(FSServiceOrder.curyLineDocDiscountTotal),
                    CuryDiscTot = typeof(FSServiceOrder.curyDiscTot),
                    BranchID = typeof(FSServiceOrder.branchID),
                    FinPeriodID = typeof(FSServiceOrder.finPeriodID),
                    TaxZoneID = typeof(FSServiceOrder.taxZoneID),
                    CuryLinetotal = typeof(FSServiceOrder.curyBillableOrderTotal),
                    CuryTaxTotal = typeof(FSServiceOrder.curyTaxTotal),
                    TaxCalcMode = typeof(FSServiceOrder.taxCalcMode)
                };
            }

            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(FSSODet))
                {
                    CuryTranAmt = typeof(FSSODet.curyBillableTranAmt),
                    TaxCategoryID = typeof(FSSODet.taxCategoryID),
                    DocumentDiscountRate = typeof(FSSODet.documentDiscountRate),
                    GroupDiscountRate = typeof(FSSODet.groupDiscountRate),
                    CuryTranDiscount = typeof(FSSODet.curyDiscAmt),
                    CuryTranExtPrice = typeof(FSSODet.curyBillableExtPrice)
                };
            }

            protected override void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
            {
                TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
                if (taxCalc == TaxCalc.Calc || taxCalc == TaxCalc.ManualLineCalc)
                {
                    if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
                    {
                        if (Base.ServiceOrderDetails.SelectSingle() != null)
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

                if (Base.ServiceOrderTypeSelected.Current != null && Base.ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
                    row.TaxCalc = TaxCalc.NoCalc;
            }

            public void CalcTaxes()
            {
                CalcTaxes(null);
            }

            protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
            {
                if (Base.SkipTaxCalcTotals == false)
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
            }

            protected override string GetExtCostLabel(PXCache sender, object row)
            {
                return ((PXDecimalState)sender.GetValueExt<FSSODet.curyBillableExtPrice>(row)).DisplayName;
            }

            protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
            {
                var row = child as PX.Data.PXResult<PX.Objects.Extensions.SalesTax.Detail>;
                if (row != null)
                {
                    var det = PXResult.Unwrap<PX.Objects.Extensions.SalesTax.Detail>(row);
                    var line = (FSSODet)det.Base;
                    line.CuryBillableExtPrice = value;
                    sender.Update(row);
                }
            }

            protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
            {
				IComparer<Tax> taxComparer = GetTaxByCalculationLevelComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

				Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
                var currents = new[]
                {
                    row != null && row is Extensions.SalesTax.Detail ? Details.Cache.GetMain((Extensions.SalesTax.Detail)row):null,
                    ((ServiceOrderEntry)graph).CurrentServiceOrder.Current
                };

                foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                                                         LeftJoin<TaxRev,
                                                         On<
                                                             TaxRev.taxID, Equal<Tax.taxID>,
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
                                                             Where<
                                                                 FSServiceOrderTax.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                                                 And<FSServiceOrderTax.refNbr, Equal<Current<FSServiceOrder.refNbr>>,
                                                                 And<FSServiceOrderTax.lineNbr, Equal<Current<FSSODet.lineNbr>>>>>>
                                                             .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0) && taxComparer.Compare((PXResult<FSServiceOrderTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<FSServiceOrderTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcLine:
                        foreach (FSServiceOrderTax record in PXSelect<FSServiceOrderTax,
                                                             Where<
                                                                 FSServiceOrderTax.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                                                 And<FSServiceOrderTax.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
                                                             .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && ((FSServiceOrderTax)(PXResult<FSServiceOrderTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
                                    && taxComparer.Compare((PXResult<FSServiceOrderTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<FSServiceOrderTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcTotals:
                        foreach (FSServiceOrderTaxTran record in PXSelect<FSServiceOrderTaxTran,
                                                                 Where<
                                                                     FSServiceOrderTaxTran.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                                                        And<FSServiceOrderTaxTran.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>,
                                                                 OrderBy <
                                                                     Asc<FSServiceOrderTaxTran.srvOrdType, Asc<FSServiceOrderTaxTran.refNbr, Asc<FSServiceOrderTaxTran.taxID>>>>>
                                                                 .SelectMultiBound(graph, currents))
                        {
                            if (record.TaxID != null && tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0) && taxComparer.Compare((PXResult<FSServiceOrderTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<FSServiceOrderTaxTran, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    default:
                        return ret;
                }
            }

            protected override List<Object> SelectDocumentLines(PXGraph graph, object row)
            {
                var res = PXSelect<FSSODet,
                            Where<FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                And<FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
                            .SelectMultiBound(graph, new object[] { row })
                            .RowCast<FSSODet>()
                            .Select(_ => (object)_)
                            .ToList();
                return res;
            }

            #region FSServiceOrderTaxTran
            protected virtual void _(Events.RowSelected<FSServiceOrderTaxTran> e)
            {
                if (e.Row == null)
                    return;

                PXUIFieldAttribute.SetEnabled<FSServiceOrderTaxTran.taxID>(e.Cache, e.Row, e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
            }
            
            protected virtual void _(Events.RowPersisting<FSServiceOrderTaxTran> e)
            {
                FSServiceOrderTaxTran row = e.Row as FSServiceOrderTaxTran;

                if (row == null) return;

                if (e.Operation == PXDBOperation.Delete)
                {
                    FSServiceOrderTax tax = (FSServiceOrderTax)Base.TaxLines.Cache.Locate(FindFSServiceOrderTax(row));

                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Deleted
                        || Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.InsertedDeleted)
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
                           Where<
                               FSServiceOrderTax.srvOrdType, Equal<Required<FSServiceOrderTax.srvOrdType>>,
                               And<FSServiceOrderTax.refNbr, Equal<Required<FSServiceOrderTax.refNbr>>,
                               And<FSServiceOrderTax.lineNbr, Equal<Required<FSServiceOrderTax.lineNbr>>,
                               And<FSServiceOrderTax.taxID, Equal<Required<FSServiceOrderTax.taxID>>>>>>>
                           .SelectSingleBound(new PXGraph(), new object[] { }).RowCast<FSServiceOrderTax>();

                return list.FirstOrDefault();
            }
            #endregion

            #region FSServiceOrder
            protected virtual void _(Events.FieldDefaulting<FSServiceOrder, FSServiceOrder.taxZoneID> e)
            {
                if (e.Row == null) return;
                FSServiceOrder row = e.Row;
                PXCache cache = e.Cache;

                var customerLocation = (Location)PXSelect<Location,
                                                 Where<
                                                     Location.bAccountID, Equal<Required<Location.bAccountID>>,
                                                     And<Location.locationID, Equal<Required<Location.locationID>>>>>
                                                 .Select(cache.Graph, row.BillCustomerID, row.BillLocationID);

                if (customerLocation != null)
                {
                    if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
                    {
                        e.NewValue = customerLocation.CTaxZoneID;
                    }
                    else
                    {
                        var address = (Address)PXSelect<Address,
                                               Where<
                                                   Address.addressID, Equal<Required<Address.addressID>>>>
                                               .Select(cache.Graph, customerLocation.DefAddressID);

                        if (address != null && !string.IsNullOrEmpty(address.PostalCode))
                        {
                            e.NewValue = TaxBuilderEngine.GetTaxZoneByZip(cache.Graph, address.PostalCode);
                        }
                    }
                }
                if (e.NewValue == null)
                {
                    var branchLocationResult = (PXResult<Branch, BAccount, Location>)
                                               PXSelectJoin<Branch,
                                               InnerJoin<BAccount,
                                               On<
                                                   BAccount.bAccountID, Equal<Branch.bAccountID>>,
                                               InnerJoin<Location,
                                               On<
                                                   Location.locationID, Equal<BAccount.defLocationID>>>>,
                                               Where<
                                                   Branch.branchID, Equal<Required<Branch.branchID>>>>
                                               .Select(cache.Graph, row.BranchID);

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
        public class ExtensionSorting : Module
        {
            protected override void Load(ContainerBuilder builder) => builder.RunOnApplicationStart(() =>
                PXBuildManager.SortExtensions += list => PXBuildManager.PartialSort(list, _order)
                );

            private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
            {
                {typeof(ContactAddress), 1},
                {typeof(MultiCurrency), 2},
                /*{typeof(SalesPrice), 3},
                    {typeof(Discount), 4},*/
                {typeof(SalesTax), 5},
            };
        }
        #endregion

        #region Address Lookup Extension
        /// <exclude/>
        public class ServiceOrderEntryAddressLookupExtension : CR.Extensions.AddressLookupExtension<ServiceOrderEntry, FSServiceOrder, FSAddress>
        {
            protected override string AddressView => nameof(Base.ServiceOrder_Address);
            protected override string ViewOnMap => nameof(Base.viewDirectionOnMap);
        }
        #endregion
        #endregion
    }
}