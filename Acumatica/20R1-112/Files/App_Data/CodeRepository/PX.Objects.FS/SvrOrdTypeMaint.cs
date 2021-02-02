using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.SM;

namespace PX.Objects.FS
{
    public class SvrOrdTypeMaint : PXGraph<SvrOrdTypeMaint, FSSrvOrdType>
    {
        #region Ctor
        public SvrOrdTypeMaint()
        {
            FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });
        }
        #endregion

        #region Selects
        [PXImport(typeof(FSSrvOrdType))]
        public PXSelect<FSSrvOrdType> SvrOrdTypeRecords;

        [PXHidden]
        public PXSelectJoin<FSAppointmentDet,
               InnerJoin<FSAppointment,
               On<
                   FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>,
               Where<
                   FSAppointment.srvOrdType, Equal<Current<FSSrvOrdType.srvOrdType>>,
                   And<FSAppointmentDet.postID, IsNotNull>>> SrvOrdAppointmentsPosted;

        [PXViewName(CR.Messages.Attributes)]
        public FSAttributeGroupList<FSSrvOrdType, FSAppointment, FSServiceOrder, FSSchedule> Mapping;

        public PXSelect<FSSrvOrdType, 
               Where<
                   FSSrvOrdType.srvOrdType, Equal<Current<FSSrvOrdType.srvOrdType>>>> CurrentSrvOrdTypeRecord;

        public PXSelectJoin<FSSrvOrdTypeProblem,
               InnerJoin<FSProblem, 
               On<
                   FSSrvOrdTypeProblem.problemID, Equal<FSProblem.problemID>>>,
               Where<
                   FSSrvOrdTypeProblem.srvOrdType, Equal<Current<FSSrvOrdType.srvOrdType>>>> SrvOrdTypeProblemRecords;

        public CRClassNotificationSourceList<NotificationSource, FSSrvOrdType.srvOrdType, FSNotificationSource.appointment> NotificationSources;

        public PXSelect<NotificationRecipient,
               Where<
                   NotificationRecipient.refNoteID, IsNull,
                   And<NotificationRecipient.sourceID, Equal<Optional<NotificationSource.sourceID>>>>> NotificationRecipients;

        public PXSelect<FSQuickProcessParameters, 
               Where<
                   FSQuickProcessParameters.srvOrdType, Equal<Current<FSSrvOrdType.srvOrdType>>>> QuickProcessSettings;

        public PXSetup<FSSetup> SetupRecord;
        #endregion

        #region CacheAttached
        #region NotificationSource_SetupID
        [PXDBGuid(IsKey = true)]
        [PXSelector(
            typeof(
                Search<NotificationSetup.setupID,
                Where<
                    NotificationSetup.sourceCD, Equal<FSNotificationSource.appointment>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID")]
        protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NotificationSource_ClassID
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(FSSrvOrdType.srvOrdType))]
        [PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.Invisible)]
        [PXParent(
            typeof(
                Select2<FSSrvOrdType,
                InnerJoin<NotificationSetup, 
                    On<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>,
                Where<
                    FSSrvOrdType.srvOrdType, Equal<Current<NotificationSource.classID>>>>))]
        protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NotificationSource_ReportID
        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName = "Report")]
        [PXDefault(
            typeof(
                Search<NotificationSetup.reportID,
                Where<
                    NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(
            typeof(
                Search<SiteMap.screenID,
                Where<
                    SiteMap.url, Like<urlReports>,
                    And<Where<SiteMap.screenID, Like<FSModule.fs_>>>>,
                OrderBy<Asc<SiteMap.screenID>>>), 
            typeof(SiteMap.screenID), 
            typeof(SiteMap.title),
        Headers = new string[] { PX.Objects.CA.Messages.ReportID, PX.Objects.CA.Messages.ReportName },
        DescriptionField = typeof(SiteMap.title))]
        [PXFormula(typeof(Default<NotificationSource.setupID>))]
        protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NotificationRecipient_SourceID_
        [PXDBInt]
        [PXDBLiteDefault(typeof(NotificationSource.sourceID))]
        protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NotificationRecipient_ContactType
        [PXDBString(10)]
        [PXDefault]
        [ApptContactType.ClassList]
        [PXUIField(DisplayName = "Contact Type")]
        [PXCheckUnique(typeof(NotificationRecipient.contactID),
            Where = typeof(Where<NotificationRecipient.refNoteID, IsNull, And<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>>>))]
        protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NotificationRecipient_ContactID
        [PXDBInt]
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationRecipient.contactType))]
        protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region EntityClassID
        [PXDBString(4, IsUnicode = true, IsKey = true, IsFixed = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Entity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Virtual Methods

        /// <summary>
        /// Enables/Disables the Employee Time Card Integration fields.
        /// </summary>
        public virtual void EnableDisableEmployeeTimeCardIntegrationFields(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow)
        {
            FSSetup fsSetupRow = SetupRecord.Current;

            bool enable = fsSetupRow != null
                                && SetupRecord.Current.EnableEmpTimeCardIntegration == true
                                    && fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.QUOTE;

            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireTimeApprovalToInvoice>(cache, fsSrvOrdTypeRow, enable);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.createTimeActivitiesFromAppointment>(cache, fsSrvOrdTypeRow, enable);

            PXUIFieldAttribute.SetVisible<FSSrvOrdType.requireTimeApprovalToInvoice>(cache, fsSrvOrdTypeRow, enable);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.createTimeActivitiesFromAppointment>(cache, fsSrvOrdTypeRow, enable);

            bool activateEarningType = enable && fsSrvOrdTypeRow.CreateTimeActivitiesFromAppointment == true;

            bool requireEarningType = activateEarningType
                                            && fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.QUOTE;

            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.dfltEarningType>(cache, fsSrvOrdTypeRow, activateEarningType);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.dfltEarningType>(cache, fsSrvOrdTypeRow, activateEarningType);
            PXUIFieldAttribute.SetRequired<FSSrvOrdType.dfltEarningType>(cache, requireEarningType);
            PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.dfltEarningType>(cache, fsSrvOrdTypeRow, requireEarningType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);           
        }

        public virtual void CheckAppointmenAddressContactOptions(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow) 
        {
            //if 'Customer Contact' option is selected as source for the 'Appointment Address' or the 'Appointment Contact' the 'Contact Required' field is set to 'true' and disabled.
            if (fsSrvOrdTypeRow.AppAddressSource.Equals(ID.Source_Info.CUSTOMER_CONTACT) || fsSrvOrdTypeRow.AppContactInfoSource.Equals(ID.Source_Info.CUSTOMER_CONTACT))
            {
                fsSrvOrdTypeRow.RequireContact = true;
                PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireContact>(cache, fsSrvOrdTypeRow, false);
            }
            else
            {
                PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireContact>(cache, fsSrvOrdTypeRow, true);
            }
        }

        public virtual void SetQuickProcessSettingsVisibility(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow, FSQuickProcessParameters fsQuickProcessParametersRow)
        {
            if (fsSrvOrdTypeRow != null)
            {
                bool isQuickProcessChecked = false;
                bool isGeneratingInvoice = fsQuickProcessParametersRow.GenerateInvoiceFromServiceOrder.Value || fsQuickProcessParametersRow.GenerateInvoiceFromAppointment.Value;
                bool postToSO = fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE;
                bool postToSOInvoice = fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;
                bool isInvoiceBehavior = false || postToSOInvoice == true;

                if (postToSO == true)
                {
                    SOOrderType soOrderTypeRow = PXSelect<SOOrderType, 
                                                 Where<
                                                    SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>
                                                 .Select(this, fsSrvOrdTypeRow.PostOrderType);

                    if (soOrderTypeRow != null)
                    {
                        isInvoiceBehavior = soOrderTypeRow.Behavior == SOBehavior.IN;
                        isQuickProcessChecked = (bool)soOrderTypeRow.AllowQuickProcess;
                    }
                }

                bool enableInvoiceActions = isInvoiceBehavior
                                                && ((postToSO
                                                        && fsQuickProcessParametersRow.SOQuickProcess == false
                                                            && fsQuickProcessParametersRow.PrepareInvoice == true)
                                                    || postToSOInvoice) && isGeneratingInvoice;

                PXUIFieldAttribute.SetVisible<FSQuickProcessParameters.sOQuickProcess>(cache, fsQuickProcessParametersRow, postToSO && isQuickProcessChecked);
                PXUIFieldAttribute.SetVisible<FSQuickProcessParameters.prepareInvoice>(cache, fsQuickProcessParametersRow, postToSO && isInvoiceBehavior && fsQuickProcessParametersRow.SOQuickProcess == false);                
                PXUIFieldAttribute.SetVisible<FSQuickProcessParameters.emailSalesOrder>(cache, fsQuickProcessParametersRow, postToSO);

                PXUIFieldAttribute.SetVisible<FSQuickProcessParameters.releaseInvoice>(cache, fsQuickProcessParametersRow, enableInvoiceActions);
                PXUIFieldAttribute.SetVisible<FSQuickProcessParameters.emailInvoice>(cache, fsQuickProcessParametersRow, enableInvoiceActions);
                
                PXUIFieldAttribute.SetVisible<FSQuickProcessParameters.releaseBill>(cache, fsQuickProcessParametersRow, false);
                PXUIFieldAttribute.SetVisible<FSQuickProcessParameters.payBill>(cache, fsQuickProcessParametersRow, false);

                PXUIFieldAttribute.SetEnabled<FSQuickProcessParameters.sOQuickProcess>(cache, fsQuickProcessParametersRow, fsQuickProcessParametersRow.PrepareInvoice.Value == false && isGeneratingInvoice);
                PXUIFieldAttribute.SetEnabled<FSQuickProcessParameters.releaseInvoice>(cache, fsQuickProcessParametersRow, enableInvoiceActions);
                PXUIFieldAttribute.SetEnabled<FSQuickProcessParameters.emailInvoice>(cache, fsQuickProcessParametersRow, enableInvoiceActions);
            }
        }

        /// <summary>
        /// Hides/Shows invoicing related fields, depending on the <c>fsSrvOrdTypeRow.Behavior</c>.
        /// </summary>
        public virtual void SetPostingSettingVisibility(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow, bool isVisible)
        {
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.postTo>(cache, fsSrvOrdTypeRow, isVisible);

            bool isDistributionModuleInstalled = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
            isVisible = isVisible && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.NONE;
            
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.dfltTermIDARSO>(cache, fsSrvOrdTypeRow, isVisible && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.dfltTermIDAP>(cache, fsSrvOrdTypeRow, isVisible && fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.salesAcctSource>(cache, fsSrvOrdTypeRow, isVisible && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.combineSubFrom>(cache, fsSrvOrdTypeRow, isVisible && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.subID>(cache, fsSrvOrdTypeRow, isVisible && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.allowInvoiceOnlyClosedAppointment>(cache, fsSrvOrdTypeRow, isVisible || fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.postNegBalanceToAP>(cache, fsSrvOrdTypeRow, isVisible && fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.enableINPosting>(cache, fsSrvOrdTypeRow, isVisible && fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.postOrderType>(cache, fsSrvOrdTypeRow, isVisible && isDistributionModuleInstalled && fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.postOrderTypeNegativeBalance>(cache, fsSrvOrdTypeRow, isVisible && isDistributionModuleInstalled && fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.allowQuickProcess>(cache, fsSrvOrdTypeRow, fsSrvOrdTypeRow.PostToSOSIPM == true && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS);

            bool allocationOrderTypeRequired = PXAccess.FeatureInstalled<FeaturesSet.inventory>()
                                                    && isVisible && isDistributionModuleInstalled
                                                    && (fsSrvOrdTypeRow.PostToSOSIPM == true);

            PXUIFieldAttribute.SetVisible<FSSrvOrdType.allocationOrderType>(cache, fsSrvOrdTypeRow, allocationOrderTypeRequired);
            PXUIFieldAttribute.SetRequired<FSSrvOrdType.allocationOrderType>(cache, allocationOrderTypeRequired);
            PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.allocationOrderType>(cache, fsSrvOrdTypeRow, allocationOrderTypeRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            if (!isVisible || fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.NONE || fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
            {
                PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.dfltTermIDAP>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.dfltTermIDARSO>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.combineSubFrom>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.postOrderType>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.postOrderTypeNegativeBalance>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
            }
            else
            {
                if (fsSrvOrdTypeRow.PostNegBalanceToAP == true)
                {
                    PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.dfltTermIDAP>(cache, fsSrvOrdTypeRow, PXPersistingCheck.NullOrBlank);
                }
                else
                {
                    PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.dfltTermIDAP>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
                }

                PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.dfltTermIDARSO>(cache, fsSrvOrdTypeRow, PXPersistingCheck.NullOrBlank);
                PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.combineSubFrom>(cache, fsSrvOrdTypeRow, PXPersistingCheck.NullOrBlank);

                if (fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE)
                {
                    PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.postOrderType>(cache, fsSrvOrdTypeRow, PXPersistingCheck.NullOrBlank);
                    PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.postOrderTypeNegativeBalance>(cache, fsSrvOrdTypeRow, PXPersistingCheck.NullOrBlank);
                }
                else
                {
                    PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.postOrderType>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
                    PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.postOrderTypeNegativeBalance>(cache, fsSrvOrdTypeRow, PXPersistingCheck.Nothing);
                }
            }
        }

        public virtual void EnableDisable_Behavior(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow) 
        {
            bool enableBehavior = true;

            if (cache.GetStatus(fsSrvOrdTypeRow) != PXEntryStatus.Inserted)
            {
                int rowCount = PXSelectJoin<FSSrvOrdType,
                                LeftJoin<FSServiceOrder,
                                    On<FSServiceOrder.srvOrdType, Equal<FSSrvOrdType.srvOrdType>>,
                                LeftJoin<FSSchedule,
                                    On<FSSchedule.srvOrdType, Equal<FSSrvOrdType.srvOrdType>>>>,
                                Where<
                                     FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>,
                                     And<
                                         Where<FSServiceOrder.sOID, IsNotNull,
                                                Or<FSSchedule.scheduleID, IsNotNull>>>>>
                                .SelectWindowed(cache.Graph, 0, 1, fsSrvOrdTypeRow.SrvOrdType).Count;

                enableBehavior = rowCount == 0;
            }

            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.behavior>(cache, fsSrvOrdTypeRow, enableBehavior);
        }

        public virtual void EnableDisableFields(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow)
        {
            bool enabledPostToOption = SrvOrdAppointmentsPosted.Current != null && SrvOrdAppointmentsPosted.Current.AppointmentID != null;
            bool postToIsNone = fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.NONE;
            fsSrvOrdTypeRow.BAccountRequired = fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT;
            fsSrvOrdTypeRow.RequireRoute = fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT;

            if (fsSrvOrdTypeRow.BAccountRequired == false)
            {
                fsSrvOrdTypeRow.RequireContact = false;
            }

            bool isRoomManagementActive = ServiceManagementSetup.IsRoomManagementActive(cache.Graph, SetupRecord?.Current);

            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireRoom>(cache, fsSrvOrdTypeRow, isRoomManagementActive);
            PXUIFieldAttribute.SetVisible<FSSrvOrdType.requireRoom>(cache, fsSrvOrdTypeRow, isRoomManagementActive);

            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.salesAcctSource>(cache, fsSrvOrdTypeRow, postToIsNone);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.combineSubFrom>(cache, fsSrvOrdTypeRow, postToIsNone);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.dfltTermIDARSO>(cache, fsSrvOrdTypeRow, postToIsNone);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.postOrderType>(cache, fsSrvOrdTypeRow, postToIsNone);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.dfltTermIDAP>(cache, fsSrvOrdTypeRow, postToIsNone && fsSrvOrdTypeRow.PostNegBalanceToAP == true);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.postOrderTypeNegativeBalance>(cache, fsSrvOrdTypeRow, postToIsNone);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.subID>(cache, fsSrvOrdTypeRow, postToIsNone);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.allowInvoiceOnlyClosedAppointment>(cache, fsSrvOrdTypeRow, postToIsNone);

            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.postNegBalanceToAP>(cache, fsSrvOrdTypeRow, fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.enableINPosting>(
                cache,
                fsSrvOrdTypeRow,
                postToIsNone
                && PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
                && fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireContact>(cache, fsSrvOrdTypeRow, fsSrvOrdTypeRow.BAccountRequired == true);
            PXUIFieldAttribute.SetEnabled<FSSrvOrdType.postTo>(cache, fsSrvOrdTypeRow, !enabledPostToOption);

            EnableDisableEmployeeTimeCardIntegrationFields(cache, fsSrvOrdTypeRow);

            switch (fsSrvOrdTypeRow.Behavior)
            {
                case ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT:
                    fsSrvOrdTypeRow.AppAddressSource = ID.Source_Info.BRANCH_LOCATION;
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdType.appAddressSource>(cache, fsSrvOrdTypeRow, false);
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireCustomerSignature>(cache, fsSrvOrdTypeRow, true);
                    SetPostingSettingVisibility(cache, fsSrvOrdTypeRow, false);
                    break;
                case ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT:
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdType.appAddressSource>(cache, fsSrvOrdTypeRow, true);
                    PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireCustomerSignature>(cache, fsSrvOrdTypeRow, true);
                    SetPostingSettingVisibility(cache, fsSrvOrdTypeRow, true);
                    break;
                case ID.Behavior_SrvOrderType.QUOTE:
                default:

                    if (fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.REGULAR_APPOINTMENT)
                    {
                        PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireCustomerSignature>(cache, fsSrvOrdTypeRow, true);
                    }
                    else
                    {
                        PXUIFieldAttribute.SetEnabled<FSSrvOrdType.requireCustomerSignature>(cache, fsSrvOrdTypeRow, false);
                    }

                    PXUIFieldAttribute.SetEnabled<FSSrvOrdType.appAddressSource>(cache, fsSrvOrdTypeRow, true);

                    if (fsSrvOrdTypeRow.RequireContact == false
                            && fsSrvOrdTypeRow.AppAddressSource == ID.Source_Info.CUSTOMER_CONTACT)
                    {
                        fsSrvOrdTypeRow.AppAddressSource = ID.Source_Info.BUSINESS_ACCOUNT;
                    }

                    SetPostingSettingVisibility(cache, fsSrvOrdTypeRow, fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.QUOTE && fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT);
                    break;
            }
        }

        public virtual void TurnOffInvoiceOptions(PXCache cache, FSQuickProcessParameters fsQuickProcessParametersRow)
        {
            cache.SetValueExt<FSQuickProcessParameters.prepareInvoice>(fsQuickProcessParametersRow, false);
            cache.SetValueExt<FSQuickProcessParameters.emailSalesOrder>(fsQuickProcessParametersRow, false);
            cache.SetValueExt<FSQuickProcessParameters.releaseInvoice>(fsQuickProcessParametersRow, false);
            cache.SetValueExt<FSQuickProcessParameters.emailInvoice>(fsQuickProcessParametersRow, false);
            cache.SetValueExt<FSQuickProcessParameters.sOQuickProcess>(fsQuickProcessParametersRow, false);
        }

        public virtual void ValidatePostToByFeatures(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if(fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
                return;
            }

            SharedFunctions.ValidatePostToByFeatures<FSSrvOrdType.postTo>(cache, fsSrvOrdTypeRow, fsSrvOrdTypeRow.PostTo);
        }
        #endregion

        #region Events

        #region FSSrvOrdType

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSSrvOrdType, FSSrvOrdType.appAddressSource> e)
        {
            //The default value for this field cannot be handled in the DAC with the PXDefault attribute 
            //because the information is retrieved from FSSetup and it fails when the setup hasn't been configured
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = SetupRecord.Select();

            e.NewValue = ID.Source_Info.BUSINESS_ACCOUNT;
        }

        protected virtual void _(Events.FieldDefaulting<FSSrvOrdType, FSSrvOrdType.postTo> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
            {
                e.NewValue = ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;
            }
            else
            {
                e.NewValue = ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
            }
        }

        protected virtual void _(Events.FieldDefaulting<FSSrvOrdType, FSSrvOrdType.appContactInfoSource> e)
        {
            //The default value for this field cannot be handled in the DAC with the PXDefault attribute 
            //because the information is retrieved from FSSetup and it fails when the setup hasn't been configured
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = SetupRecord.Select();

            if (fsSetupRow != null && fsSetupRow.DfltAppContactInfoSource != null)
            {
                e.NewValue = fsSetupRow.DfltAppContactInfoSource;
            }
            else
            {
                e.NewValue = ID.Source_Info.BUSINESS_ACCOUNT;
            }
        }

        #endregion
        #region FieldUpdating

        protected virtual void _(Events.FieldUpdating<FSSrvOrdType, FSSrvOrdType.appAddressSource> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;

            if (fsSrvOrdTypeRow.RequireContact == false && (string)e.NewValue == ID.Source_Info.CUSTOMER_CONTACT)
            {
                e.Cache.RaiseExceptionHandling<FSSrvOrdType.appAddressSource>(fsSrvOrdTypeRow,
                                                                              ID.Source_Info.CUSTOMER_CONTACT,
                                                                              new PXSetPropertyException(TX.Error.CUSTOMER_CONTACT_ADDRESS_OPTION_NOT_AVAILABLE, PXErrorLevel.Error));
            }
        }

        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        
        protected virtual void _(Events.FieldUpdated<FSSrvOrdType, FSSrvOrdType.postTo> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;

            if (fsSrvOrdTypeRow.SalesAcctSource == null)
            {
                fsSrvOrdTypeRow.SalesAcctSource = ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION;
            }

            if (fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.NONE)
            {
                PXDefaultAttribute.SetDefault<FSSrvOrdType.combineSubFrom>(e.Cache, fsSrvOrdTypeRow);
            }

            ValidatePostToByFeatures(e.Cache, fsSrvOrdTypeRow);
        }

        protected virtual void _(Events.FieldUpdated<FSSrvOrdType, FSSrvOrdType.behavior> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;

            fsSrvOrdTypeRow.BAccountRequired = fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT;
            fsSrvOrdTypeRow.RequireRoute = fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT;

            if (fsSrvOrdTypeRow.BAccountRequired == false)
            {
                fsSrvOrdTypeRow.RequireContact = false;
            }

            if (fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.QUOTE)
            {
                fsSrvOrdTypeRow.RequireCustomerSignature = false;
                fsSrvOrdTypeRow.PostTo = ID.SrvOrdType_PostTo.NONE;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSrvOrdType, FSSrvOrdType.postOrderType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;

            SOOrderType soOrderTypeRow = PXSelect<SOOrderType,
                                         Where<
                                             SOOrderType.orderType, Equal<Required<FSSrvOrdType.postOrderType>>>>
                                         .Select(this, fsSrvOrdTypeRow.PostOrderType);

            if (soOrderTypeRow == null)
            {
                return;
            }

            if (soOrderTypeRow.Behavior != SOBehavior.IN)
            {
                e.Cache.RaiseExceptionHandling<FSSrvOrdType.postOrderType>(
                    fsSrvOrdTypeRow,
                    fsSrvOrdTypeRow.PostOrderType,
                    new PXSetPropertyException(
                        TX.Warning.SALES_ORDER_NOT_INVOICE,
                        PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSrvOrdType, FSSrvOrdType.allowQuickProcess> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;

            if (fsSrvOrdTypeRow.AllowQuickProcess == true
                    && QuickProcessSettings.Current == null)
            {
                QuickProcessSettings.Insert();

                if (QuickProcessSettings.Current != null)
                {
                    QuickProcessSettings.Current.GenerateInvoiceFromServiceOrder = true;
                    QuickProcessSettings.Current.GenerateInvoiceFromAppointment = true;
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSrvOrdType, FSSrvOrdType.billingType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = e.Row;

            if ((string)e.NewValue == ID.SrvOrdType_BillingType.COST_AS_COST && fsSrvOrdTypeRow.ReleaseIssueOnInvoice == false)
            {
                fsSrvOrdTypeRow.ReleaseIssueOnInvoice = true;
            }
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSSrvOrdType> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSSrvOrdType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;
            PXCache cache = e.Cache;

            EnableDisableFields(cache, fsSrvOrdTypeRow);
            EnableDisable_Behavior(cache, fsSrvOrdTypeRow);

            FSPostTo.SetLineTypeList<FSSrvOrdType.postTo>(e.Cache, e.Row, true, true, fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.REGULAR_APPOINTMENT);
        }

        protected virtual void _(Events.RowInserting<FSSrvOrdType> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSSrvOrdType> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSSrvOrdType> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSSrvOrdType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;

            CheckAppointmenAddressContactOptions(e.Cache, fsSrvOrdTypeRow);
        }

        protected virtual void _(Events.RowDeleting<FSSrvOrdType> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSSrvOrdType> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSSrvOrdType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)e.Row;
            PXCache cache = e.Cache;

            if (e.Operation == PXDBOperation.Delete)
            {
                int serviceOrderCount = (int)PXSelectGroupBy<FSServiceOrder,
                                             Where<
                                                 FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>>,
                                             Aggregate<Count>>
                                             .Select(this, fsSrvOrdTypeRow.SrvOrdType).RowCount;

                if (serviceOrderCount > 0)
                {
                    throw new PXException(TX.Error.SRV_ORD_TYPE_ERROR_DELETING_SO_USING_IT, fsSrvOrdTypeRow);
                }
                else
                {
                    int scheduleCount = (int)PXSelectGroupBy<FSSchedule,
                                             Where<
                                                 FSSchedule.srvOrdType, Equal<Required<FSSchedule.srvOrdType>>>,
                                             Aggregate<Count>>
                                             .Select(this, fsSrvOrdTypeRow.SrvOrdType).RowCount;

                    if (scheduleCount > 0)
                    {
                        throw new PXException(TX.Error.SRV_ORD_TYPE_ERROR_DELETING_CONTRACT_USING_IT, fsSrvOrdTypeRow);
                    }
                }
            }

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                ValidatePostToByFeatures(cache, fsSrvOrdTypeRow);
            }
        }

        protected virtual void _(Events.RowPersisted<FSSrvOrdType> e)
        {
        }

        //not sure where to put this event.
        protected virtual void FSSrvOrdType_CombineSubFrom_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
        {
            if (e.Exception != null)
            {
                if (CurrentSrvOrdTypeRecord.Current.PostTo == ID.SrvOrdType_PostTo.NONE)
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion

        #region FSSrvOrdTypeProblem

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

        protected virtual void _(Events.RowSelecting<FSSrvOrdTypeProblem> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSSrvOrdTypeProblem> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdTypeProblem fsSrvOrdTypeProblemRow = (FSSrvOrdTypeProblem)e.Row;

            bool enableDisableFields = fsSrvOrdTypeProblemRow.ProblemID == null;

            PXUIFieldAttribute.SetEnabled<FSSrvOrdTypeProblem.problemID>(e.Cache, fsSrvOrdTypeProblemRow, enableDisableFields);
        }

        protected virtual void _(Events.RowInserting<FSSrvOrdTypeProblem> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSrvOrdTypeProblem fsSrvOrdTypeProblemRow = (FSSrvOrdTypeProblem)e.Row;
            int? problemID = fsSrvOrdTypeProblemRow.ProblemID;

            FSSrvOrdTypeProblem fsSrvOrdTypeProblemRow_InDB = PXSelect<FSSrvOrdTypeProblem,
                                                              Where<
                                                                  FSSrvOrdTypeProblem.problemID, Equal<Required<FSSrvOrdTypeProblem.problemID>>,
                                                              And<
                                                                  FSSrvOrdTypeProblem.srvOrdType, Equal<Current<FSSrvOrdType.srvOrdType>>>>>
                                                              .SelectWindowed(this, 0, 1, problemID);
            if (fsSrvOrdTypeProblemRow_InDB != null)
            {
                e.Cache.RaiseExceptionHandling<FSSrvOrdTypeProblem.problemID>(fsSrvOrdTypeProblemRow, problemID, new PXSetPropertyException(TX.Error.ID_ALREADY_USED, PXErrorLevel.RowError));
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.RowInserted<FSSrvOrdTypeProblem> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSSrvOrdTypeProblem> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSSrvOrdTypeProblem> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSSrvOrdTypeProblem> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSSrvOrdTypeProblem> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSSrvOrdTypeProblem> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSSrvOrdTypeProblem> e)
        {
        }

        #endregion

        #region FSQuickProcessParameters

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSQuickProcessParameters, FSQuickProcessParameters.generateInvoiceFromServiceOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSQuickProcessParameters fsQuickProcessParametersRow = (FSQuickProcessParameters)e.Row;

            if ((bool)e.OldValue != fsQuickProcessParametersRow.GenerateInvoiceFromServiceOrder
                && fsQuickProcessParametersRow.GenerateInvoiceFromAppointment == false)
            {
                TurnOffInvoiceOptions(e.Cache, fsQuickProcessParametersRow);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSQuickProcessParameters, FSQuickProcessParameters.generateInvoiceFromAppointment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSQuickProcessParameters fsQuickProcessParametersRow = (FSQuickProcessParameters)e.Row;

            if ((bool)e.OldValue != fsQuickProcessParametersRow.GenerateInvoiceFromAppointment
                && fsQuickProcessParametersRow.GenerateInvoiceFromServiceOrder == false)
            {
                TurnOffInvoiceOptions(e.Cache, fsQuickProcessParametersRow);
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSQuickProcessParameters> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSQuickProcessParameters fsQuickProcessParametersRow = (FSQuickProcessParameters)e.Row;
            SetQuickProcessSettingsVisibility(e.Cache, CurrentSrvOrdTypeRecord.Current, fsQuickProcessParametersRow);
        }

        protected virtual void _(Events.RowInserting<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSQuickProcessParameters> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSQuickProcessParameters> e)
        {
        }

        #endregion
        
        #endregion
    }
}