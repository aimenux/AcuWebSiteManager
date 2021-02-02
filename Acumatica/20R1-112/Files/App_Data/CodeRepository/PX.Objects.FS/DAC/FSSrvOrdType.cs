using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.ORDER_TYPE)]
    [PXPrimaryGraph(typeof(SvrOrdTypeMaint))]
	public class FSSrvOrdType : PX.Data.IBqlTable
	{
        #region SrvOrdTypeID
        public abstract class srvOrdTypeID : PX.Data.BQL.BqlInt.Field<srvOrdTypeID> { }

        [PXDBIdentity]
        [PXUIField(Enabled = false)]
        public virtual int? SrvOrdTypeID { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, InputMask = ">AAAA", IsFixed = true)]
		[PXUIField(DisplayName = "Service Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(FSSrvOrdType.srvOrdType))]
        [PXDefault]
        [NormalizeWhiteSpace]
        public virtual string SrvOrdType { get; set; }
		#endregion
        
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
        #endregion
        #region AllowPartialBilling
        public abstract class allowPartialBilling : PX.Data.BQL.BqlBool.Field<allowPartialBilling> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allow Partial Billing", Enabled = false)]
        public virtual bool? AllowPartialBilling { get; set; }
        #endregion
        #region AllowQuickProcess
        public abstract class allowQuickProcess : PX.Data.BQL.BqlBool.Field<allowQuickProcess> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allow Quick Process")]
        public virtual bool? AllowQuickProcess { get; set; }
        #endregion
        #region AppAddressSource
        public abstract class appAddressSource : ListField_SrvOrdType_AddressSource
        {
        }

        [PXDBString(2)]
        [appAddressSource.ListAtrribute]
        [PXDefault(ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT)]
        [PXUIField(DisplayName = "Take Address and Contact Information From")]
        public virtual string AppAddressSource { get; set; }
        #endregion
        #region AppContactInfoSource
        public abstract class appContactInfoSource : PX.Data.BQL.BqlString.Field<appContactInfoSource> { }

        [PXDBString(2)]
        [PXUIField(DisplayName = "Appointment Contact info source", Visible = false)]
        public virtual string AppContactInfoSource { get; set; }
        #endregion
        #region BAccountRequired
        public abstract class bAccountRequired : PX.Data.BQL.BqlBool.Field<bAccountRequired> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Business Account", Enabled = false)]
        public virtual bool? BAccountRequired { get; set; }
        #endregion
        #region Behavior
        public abstract class behavior : ListField_Behavior_SrvOrdType
        {
        }

        [PXDBString(2, IsFixed = true)]
        [behavior.ListAttribute]
        [PXDefault(ID.Behavior_SrvOrderType.REGULAR_APPOINTMENT)]
        [PXUIField(DisplayName = "Behavior")]
        public virtual string Behavior { get; set; }
        #endregion
        #region BillSeparately
        public abstract class billSeparately : PX.Data.BQL.BqlBool.Field<billSeparately> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Bill Separately", Enabled = false)]
        public virtual bool? BillSeparately { get; set; }
        #endregion
        #region CombineSubFrom

        public abstract class combineSubFrom : PX.Data.BQL.BqlString.Field<combineSubFrom> { }

        [PXDefault]
        [SubAccountMask(DisplayName = "Combine Sales Sub. From")]
        public virtual string CombineSubFrom { get; set; }
        #endregion
        #region CompleteSrvOrdWhenSrvDone
        public abstract class completeSrvOrdWhenSrvDone : PX.Data.BQL.BqlBool.Field<completeSrvOrdWhenSrvDone> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Complete Service Order When Its Appointments Are Completed")]
        public virtual bool? CompleteSrvOrdWhenSrvDone { get; set; }
        #endregion
        #region CloseSrvOrdWhenSrvDone
        public abstract class closeSrvOrdWhenSrvDone : PX.Data.BQL.BqlBool.Field<closeSrvOrdWhenSrvDone> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Close Service Orders When Its Appointments Are Closed")]
        public virtual bool? CloseSrvOrdWhenSrvDone { get; set; }
        #endregion
        #region DfltTermID_SO_AR
        public abstract class dfltTermIDARSO : PX.Data.BQL.BqlString.Field<dfltTermIDARSO> { }

        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Default Terms for AR and SO", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<Terms.termsID,
                            Where<
                                Terms.visibleTo, Equal<TermsVisibleTo.all>,
                                Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>),
                    DescriptionField = typeof(Terms.descr), Filterable = true)]
        public virtual string DfltTermIDARSO { get; set; }
        #endregion
        #region DfltTermIDAP
        public abstract class dfltTermIDAP : PX.Data.BQL.BqlString.Field<dfltTermIDAP> { }

        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Default Terms for AP", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<Terms.termsID,
                            Where<
                                Terms.visibleTo, Equal<TermsVisibleTo.all>,
                                Or<Terms.visibleTo, Equal<TermsVisibleTo.vendor>>>>),
                    DescriptionField = typeof(Terms.descr), Filterable = true)]
        public virtual string DfltTermIDAP { get; set; }
        #endregion
        #region DfltCostCodeID
        public abstract class dfltCostCodeID : PX.Data.BQL.BqlInt.Field<dfltCostCodeID> { }
        [CostCode(Filterable = false, SkipVerification = true)]
        public virtual int? DfltCostCodeID { get; set; }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBLocalizableString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
        #endregion
        #region EnableINPosting
        public abstract class enableINPosting : PX.Data.BQL.BqlBool.Field<enableINPosting> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXFormula(typeof(Default<FSSrvOrdType.postTo>))]
        [PXFormula(typeof(Default<FSSrvOrdType.behavior>))]
        [PXUIField(DisplayName = "Post Pickup/Delivery Items to Inventory")]
        public virtual bool? EnableINPosting { get; set; }
        #endregion
        #region GenerateInvoiceBy
        public abstract class generateInvoiceBy : ListField_SrvOrdType_GenerateInvoiceBy
        {
        }

        [PXDBString(4)]
        [generateInvoiceBy.ListAtrribute]
        [PXDefault(ID.SrvOrdType_GenerateInvoiceBy.SALES_ORDER)]
        [PXUIField(DisplayName = "Generate Invoice By")]
        public virtual string GenerateInvoiceBy { get; set; }
        #endregion
        #region InvoiceCompleteAppointment
        public abstract class allowInvoiceOnlyClosedAppointment : PX.Data.BQL.BqlBool.Field<allowInvoiceOnlyClosedAppointment> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Bill Only Closed Appointments")]
        public virtual bool? AllowInvoiceOnlyClosedAppointment { get; set; }
        #endregion
        #region PostNegBalanceToAP
        public abstract class postNegBalanceToAP : PX.Data.BQL.BqlBool.Field<postNegBalanceToAP> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXFormula(typeof(Default<FSSrvOrdType.postTo>))]
        [PXUIField(DisplayName = "Create AP Bills for Negative Balances")]
        public virtual bool? PostNegBalanceToAP { get; set; }
        #endregion
        #region PostOrderType
        public abstract class postOrderType : PX.Data.BQL.BqlString.Field<postOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXDefault]
        [PXUIField(DisplayName = "Order Type for Billing", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<SOOrderType.orderType,
                            Where<
                                SOOrderType.active, Equal<True>,
                                And<FSxSOOrderType.enableFSIntegration, Equal<True>>>>),
                    DescriptionField = typeof(SOOrderType.descr))]
        public virtual string PostOrderType { get; set; }
        #endregion
        #region PostOrderTypeNegativeBalance
        public abstract class postOrderTypeNegativeBalance : PX.Data.BQL.BqlString.Field<postOrderTypeNegativeBalance> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXDefault]
        [PXUIField(DisplayName = "Order Type for Negative Balance Billing", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<SOOrderType.orderType,
                            Where<
                                SOOrderType.active, Equal<True>,
                                And<FSxSOOrderType.enableFSIntegration, Equal<True>,
                                And<SOOrderType.aRDocType, Equal<ARInvoiceType.creditMemo>>>>>),
                    DescriptionField = typeof(SOOrderType.descr))]
        public virtual string PostOrderTypeNegativeBalance { get; set; }
        #endregion
        #region AllocationOrderType
        public abstract class allocationOrderType : PX.Data.BQL.BqlString.Field<allocationOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXDefault(typeof(Search<SOOrderType.orderType, 
                    Where<SOOrderType.active, Equal<True>,
                    And<SOOrderType.orderType, Equal<SOOrderTypeConstants.salesOrder>>>>))]
        [PXUIField(DisplayName = "Order Type for Allocation", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<SOOrderType.orderType, 
                    Where<SOOrderType.active, Equal<True>, 
                    And<SOOrderType.behavior, Equal<SOOrderTypeConstants.salesOrder>>>>), 
                    DescriptionField = typeof(SOOrderType.descr))]
        public virtual string AllocationOrderType { get; set; }
        #endregion
        #region PostTo
        public abstract class postTo : PX.Data.BQL.BqlString.Field<postTo> { }

        [PXDBString(2)]
        [PXDefault()]
        [FSPostTo.List]
        [PXUIField(DisplayName = "Generated Billing Documents")]
        public virtual string PostTo { get; set; }
        #endregion
        #region RequireAppConfirmation
        public abstract class requireAppConfirmation : PX.Data.BQL.BqlBool.Field<requireAppConfirmation> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Appointment Confirmation Required", Visible = false)]
        public virtual bool? RequireAppConfirmation { get; set; }
        #endregion        
        #region RequireAddressValidation
        public abstract class requireAddressValidation : PX.Data.BQL.BqlBool.Field<requireAddressValidation> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Address Validation")]
        public virtual bool? RequireAddressValidation { get; set; }
        #endregion
        #region RequireContact
        public abstract class requireContact : PX.Data.BQL.BqlBool.Field<requireContact> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Contact")]
        public virtual bool? RequireContact { get; set; }
        #endregion
        #region RequireCustomerSignature
        public abstract class requireCustomerSignature : PX.Data.BQL.BqlBool.Field<requireCustomerSignature> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Require Customer Signature on Mobile App")]
        public virtual bool? RequireCustomerSignature { get; set; }
        #endregion
        #region RequireRoom
        public abstract class requireRoom : PX.Data.BQL.BqlBool.Field<requireRoom> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Room")]
        public virtual bool? RequireRoom { get; set; }
        #endregion
        #region RequireRoute
        public abstract class requireRoute : PX.Data.BQL.BqlBool.Field<requireRoute> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Route", Enabled = false)]
        public virtual bool? RequireRoute { get; set; }
        #endregion
        #region SalesAcctSource
        public abstract class salesAcctSource : ListField_SrvOrdType_SalesAcctSource
        {
        }

        [PXDBString(2)]
        [salesAcctSource.ListAtrribute]
        [PXDefault(ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION)]
        [PXUIField(DisplayName = "Use Sales Account From")]
        public virtual string SalesAcctSource { get; set; }
        #endregion
        #region SrvOrdNumberingID
        public abstract class srvOrdNumberingID : PX.Data.BQL.BqlString.Field<srvOrdNumberingID> { }

        [PXDBString(10)]
        [PXDefault]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Numbering Sequence")]
        public virtual string SrvOrdNumberingID { get; set; }
        #endregion
        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [SubAccount]
        [PXUIField(DisplayName = "General Subaccount")]
        public virtual int? SubID { get; set; }
        #endregion
        #region RequireTimeApprovalToInvoice
        public abstract class requireTimeApprovalToInvoice : PX.Data.BQL.BqlBool.Field<requireTimeApprovalToInvoice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Time Approval to Close Appointments")]
        public virtual bool? RequireTimeApprovalToInvoice { get; set; }
        #endregion
        #region CreateTimeActivitiesFromAppointment
        public abstract class createTimeActivitiesFromAppointment : PX.Data.BQL.BqlBool.Field<createTimeActivitiesFromAppointment> { }

        [PXDBBool]
        [PXDefault(typeof(Search<FSSetup.enableEmpTimeCardIntegration>))]
        [PXUIField(DisplayName = "Automatically Create Time Activities from Appointments")]
        [PXUIVisible(typeof(IIf<FeatureInstalled<FeaturesSet.timeReportingModule>, True, False>))]
        public virtual bool? CreateTimeActivitiesFromAppointment { get; set; }
        #endregion
        #region DfltEarningType
        public abstract class dfltEarningType : PX.Data.BQL.BqlString.Field<dfltEarningType> { }

        [PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL")]
        [PXDefault(typeof(Search<EPSetup.regularHoursType>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(EPEarningType.typeCD))]
        [PXUIField(DisplayName = "Default Earning Type")]
        public virtual string DfltEarningType { get; set; }
        #endregion
        #region AccountGroupID
        public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
        [PXUIRequired(typeof(Where<postTo, Equal<FSPostTo.Projects>>))]
        [PXUIVisible(typeof(Where<postTo, Equal<FSPostTo.Projects>>))]
        [AccountGroup]
        [PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
        public virtual int? AccountGroupID { get; set; }
        #endregion
        #region ReasonCode
        public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }

        [PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIRequired(typeof(Where<postTo, Equal<FSPostTo.Projects>, And<FeatureInstalled<FeaturesSet.inventory>>>))]
        [PXUIVisible(typeof(Where<postTo, Equal<FSPostTo.Projects>>))]
        [PXUIField(DisplayName = "Reason Code", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "DISTINV")]
        [PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.issue>>>), DescriptionField = typeof(ReasonCode.descr))]
        [PXForeignReference(typeof(Field<reasonCode>.IsRelatedTo<ReasonCode.reasonCodeID>))]
        public virtual string ReasonCode { get; set; }
        #endregion
        #region ReleaseProjectTransactionOnInvoice
        public abstract class releaseProjectTransactionOnInvoice : PX.Data.BQL.BqlBool.Field<releaseProjectTransactionOnInvoice> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIVisible(typeof(Where<postTo, Equal<FSPostTo.Projects>>))]
        [PXUIField(DisplayName = "Automatically Release Project Transactions")]
        public virtual bool? ReleaseProjectTransactionOnInvoice { get; set; }
        #endregion
        #region ReleaseIssueOnInvoice
        public abstract class releaseIssueOnInvoice : PX.Data.BQL.BqlBool.Field<releaseIssueOnInvoice> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIVisible(typeof(Where<postTo, Equal<FSPostTo.Projects>>))]
        [PXUIEnabled(typeof(Where<billingType, NotEqual<billingType.CostAsCost>>))]
        [PXUIField(DisplayName = "Automatically Release Issues", FieldClass = "DISTINV")]
        public virtual bool? ReleaseIssueOnInvoice { get; set; }
        #endregion
        #region BillingType
        public abstract class billingType : ListField_SrvOrdType_BillingType
        {
        }

        [PXDBString(2, IsFixed = true)]
        [billingType.ListAtrribute]
        [PXDefault(ID.SrvOrdType_BillingType.COST_AS_COST)]
        [PXUIVisible(typeof(Where<postTo, Equal<FSPostTo.Projects>>))]
        [PXUIField(DisplayName = "Billing Type")]
        public virtual string BillingType { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created On")]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion

        #region OnStartApptSetStartTimeInHeader
        public abstract class onStartApptSetStartTimeInHeader : PX.Data.BQL.BqlBool.Field<onStartApptSetStartTimeInHeader> { }
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Set Start Time in Appointment")]
        public virtual bool? OnStartApptSetStartTimeInHeader { get; set; }
        #endregion

        #region OnStartApptSetNotStartItemInProcess
        public abstract class onStartApptSetNotStartItemInProcess : PX.Data.BQL.BqlBool.Field<onStartApptSetNotStartItemInProcess> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Set Not Started Items as In Process")]
        public virtual bool? OnStartApptSetNotStartItemInProcess { get; set; }
        #endregion

        #region OnStartApptStartUnassignedStaff
        public abstract class onStartApptStartUnassignedStaff : PX.Data.BQL.BqlBool.Field<onStartApptStartUnassignedStaff> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Start Logging for Unassigned Staff")]
        public virtual bool? OnStartApptStartUnassignedStaff { get; set; }
        #endregion

        #region OnStartApptStartServiceAndStaff
        public abstract class onStartApptStartServiceAndStaff : PX.Data.BQL.BqlBool.Field<onStartApptStartServiceAndStaff> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Start Logging for Services and Assigned Staff (If Any)")]
        public virtual bool? OnStartApptStartServiceAndStaff { get; set; }
        #endregion
        #region OnCompleteApptSetEndTimeInHeader
        public abstract class onCompleteApptSetEndTimeInHeader : PX.Data.BQL.BqlBool.Field<onCompleteApptSetEndTimeInHeader> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Set End Time in Appointment")]
        public virtual bool? OnCompleteApptSetEndTimeInHeader { get; set; }
        #endregion

        #region OnCompleteApptSetInProcessItemsAs
        public abstract class onCompleteApptSetInProcessItemsAs : PX.Data.BQL.BqlString.Field<onCompleteApptSetInProcessItemsAs>
        {
            public abstract class Values : ListField_OnCompleteApptSetInProcessItemsAs { }
        }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(onCompleteApptSetInProcessItemsAs.Values.Completed)]
        [PXUIField(DisplayName = "Status to Set for In Process Items")]
        [onCompleteApptSetInProcessItemsAs.Values.ListAtrribute]
        public virtual string OnCompleteApptSetInProcessItemsAs { get; set; }
        #endregion
        #region OnCompleteApptSetNotStartedItemsAs
        public abstract class onCompleteApptSetNotStartedItemsAs : PX.Data.BQL.BqlString.Field<onCompleteApptSetNotStartedItemsAs>
        {
            public abstract class Values : ListField_OnCompleteApptSetNotStartedItemsAs { }
        }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(onCompleteApptSetNotStartedItemsAs.Values.Completed)]
        [PXUIField(DisplayName = "Status to Set for Not Started Items")]
        [onCompleteApptSetNotStartedItemsAs.Values.ListAtrribute]
        public virtual string OnCompleteApptSetNotStartedItemsAs { get; set; }
        #endregion
        #region OnStartTimeChangeUpdateLogStartTime
        public abstract class onStartTimeChangeUpdateLogStartTime : PX.Data.BQL.BqlBool.Field<onStartTimeChangeUpdateLogStartTime> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Update Log Start Time When Appointment Start Time is Updated")]
        public virtual bool? OnStartTimeChangeUpdateLogStartTime { get; set; }
        #endregion
        #region OnEndTimeChangeUpdateLogEndTime
        public abstract class onEndTimeChangeUpdateLogEndTime : PX.Data.BQL.BqlBool.Field<onEndTimeChangeUpdateLogEndTime> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Update Log End Time When Appointment End Time is Updated")]
        public virtual bool? OnEndTimeChangeUpdateLogEndTime { get; set; }
        #endregion
        #region OnCompleteApptRequireLog
        public abstract class onCompleteApptRequireLog : PX.Data.BQL.BqlBool.Field<onCompleteApptRequireLog> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Service Logs on Appointment Completion")]
        public virtual bool? OnCompleteApptRequireLog { get; set; }
        #endregion

        #region OnTravelCompleteStartAppt
        public abstract class onTravelCompleteStartAppt : PX.Data.BQL.BqlBool.Field<onTravelCompleteStartAppt> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Start Appointment When Travel is Completed")]
        public virtual bool? OnTravelCompleteStartAppt { get; set; }
        #endregion
        #region DfltBillableTravelItem
        public abstract class dfltBillableTravelItem : PX.Data.BQL.BqlInt.Field<dfltBillableTravelItem> { }

        [Service(DisplayName = "Default Travel Item")]
        [PXRestrictor(typeof(Where<
                                FSxService.isTravelItem, Equal<True>>),
                            TX.Error.NON_TRAVEL_ITEM_CAN_BE_ASSIGNED)]
        public virtual int? DfltBillableTravelItem { get; set; }
        #endregion

        #region SetTimeInHeaderBasedOnLog
        public abstract class setTimeInHeaderBasedOnLog : PX.Data.BQL.BqlBool.Field<setTimeInHeaderBasedOnLog> { }

        [PXDBBool]
        [PXUIField(DisplayName = "Update Appointment Time Based on Logged Time")]
        [PXDefault(false)]
        [PXFormula(typeof(Default<onStartTimeChangeUpdateLogStartTime, onEndTimeChangeUpdateLogEndTime>))]
        [PXUIEnabled(typeof(Where<
                                Current<onStartTimeChangeUpdateLogStartTime>, Equal<False>,
                                And<
                                    Current<onEndTimeChangeUpdateLogEndTime>, Equal<False>>>))]
        public virtual bool? SetTimeInHeaderBasedOnLog { get; set; }
        #endregion
        #region AllowManualLogTimeEdition
        public abstract class allowManualLogTimeEdition : PX.Data.BQL.BqlBool.Field<allowManualLogTimeEdition> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manually Manage Time")]
        public virtual bool? AllowManualLogTimeEdition { get; set; }
        #endregion
        #region SalesPersonID
        public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }

        [PXUIVisible(typeof(Where<postTo, NotEqual<FSPostTo.Projects>>))]
        [SalesPerson(DisplayName = "Salesperson ID")]
        public virtual int? SalesPersonID { get; set; }
        #endregion
        #region Commissionable
        public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIVisible(typeof(Where<postTo, NotEqual<FSPostTo.Projects>>))]
        [PXUIField(DisplayName = "Commissionable")]
        public virtual bool? Commissionable { get; set; }
        #endregion

        #region Notes and Attachments
        #region CopyNotesFromCustomer
        public abstract class copyNotesFromCustomer : PX.Data.BQL.BqlBool.Field<copyNotesFromCustomer> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Notes From Customer")]
        public virtual bool? CopyNotesFromCustomer { get; set; }
        #endregion
        #region CopyAttachmentsFromCustomer
        public abstract class copyAttachmentsFromCustomer : PX.Data.BQL.BqlBool.Field<copyAttachmentsFromCustomer> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Attachments From Customer")]
        public virtual bool? CopyAttachmentsFromCustomer { get; set; }
        #endregion
        #region CopyNotesFromCustomerLocation
        public abstract class copyNotesFromCustomerLocation : PX.Data.BQL.BqlBool.Field<copyNotesFromCustomerLocation> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Notes From Customer Location")]
        public virtual bool? CopyNotesFromCustomerLocation { get; set; }
        #endregion
        #region CopyAttachmentsFromCustomerLocation
        public abstract class copyAttachmentsFromCustomerLocation : PX.Data.BQL.BqlBool.Field<copyAttachmentsFromCustomerLocation> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Attachments From Customer Location")]
        public virtual bool? CopyAttachmentsFromCustomerLocation { get; set; }
        #endregion
        #region CopyNotesToAppoinment
        public abstract class copyNotesToAppoinment : PX.Data.BQL.BqlBool.Field<copyNotesToAppoinment> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Notes To Appoinment")]
        public virtual bool? CopyNotesToAppoinment { get; set; }
        #endregion
        #region CopyAttachmentsToAppoinment
        public abstract class copyAttachmentsToAppoinment : PX.Data.BQL.BqlBool.Field<copyAttachmentsToAppoinment> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Attachments To Appoinment")]
        public virtual bool? CopyAttachmentsToAppoinment { get; set; }
        #endregion
        #region CopyLineNotesToInvoice
        public abstract class copyLineNotesToInvoice : PX.Data.BQL.BqlBool.Field<copyLineNotesToInvoice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Line Notes To Invoice")]
        public virtual bool? CopyLineNotesToInvoice { get; set; }
        #endregion
        #region CopyLineAttachmentsToInvoice
        public abstract class copyLineAttachmentsToInvoice : PX.Data.BQL.BqlBool.Field<copyLineAttachmentsToInvoice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Line Attachments To Invoice")]
        public virtual bool? CopyLineAttachmentsToInvoice { get; set; }
        #endregion
        #endregion

        #region ShowQuickProcessTab
        public abstract class showQuickProcessTab : PX.Data.BQL.BqlBool.Field<showQuickProcessTab> { }

        [PXBool]
        public virtual bool? ShowQuickProcessTab
        {
            get
            {
                return this.AllowQuickProcess == true 
                        && this.PostTo != ID.SrvOrdType_PostTo.NONE
                            && this.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
            }
        }
        #endregion
        #region PostToSOSIPM
        public abstract class postToSOSIPM : PX.Data.BQL.BqlBool.Field<postToSOSIPM> { }

        [PXBool]
        public virtual bool PostToSOSIPM
        {
            get
            {
                return (this.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE
                            || this.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE
                            || this.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
                        && (this.Behavior != ID.Behavior_SrvOrderType.QUOTE
                            && this.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT);
            }
        }
        #endregion
        #region AllowInventoryItems 
        public abstract class allowInventoryItems : PX.Data.BQL.BqlBool.Field<allowInventoryItems> { }

        [PXBool]
        public virtual bool AllowInventoryItems
        {
            get
            {
                return this.PostToSOSIPM == true
                            || this.Behavior == ID.Behavior_SrvOrderType.QUOTE;
            }
        }
        #endregion
    }
}
