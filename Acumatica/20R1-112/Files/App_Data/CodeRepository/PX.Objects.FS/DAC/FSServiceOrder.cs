using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using System;

using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.SERVICE_ORDER)]
    [PXPrimaryGraph(typeof(ServiceOrderEntry))]
    public class FSServiceOrder : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FSServiceOrder>.By<srvOrdType, refNbr>
		{
			public static FSServiceOrder Find(PXGraph graph, string srvOrdType, string refNbr) => FindBy(graph, srvOrdType, refNbr);
		}
		#endregion

		#region SrvOrdType
		public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Coalesce<
            Search<FSxUserPreferences.dfltSrvOrdType,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>,
            Search<FSSetup.dfltSrvOrdType>>))]
        [FSSelectorSrvOrdType]
        [PXUIVerify(typeof(Where<Current<FSSrvOrdType.active>, Equal<True>>), 
                    PXErrorLevel.Warning, TX.Error.SRVORDTYPE_INACTIVE, CheckOnRowSelected = true)]
        [PX.Data.EP.PXFieldDescription]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Service Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorSORefNbr]
        [AutoNumber(typeof(Search<FSSrvOrdType.srvOrdNumberingID,
                        Where<FSSrvOrdType.srvOrdType, Equal<Optional<FSServiceOrder.srvOrdType>>>>),
                    typeof(AccessInfo.businessDate))]
        [PX.Data.EP.PXFieldDescription]
        public virtual string RefNbr { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBIdentity]
        public virtual int? SOID { get; set; }
        #endregion
        #region Attributes
        /// <summary>
        /// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
        /// added attributes</see> defined at the <see cref="FSSrvOrdType">customer 
        /// class</see> level to function correctly.
        /// </summary>
        [CRAttributesField(typeof(FSServiceOrder.srvOrdType), typeof(FSServiceOrder.noteID))]
        public virtual string[] Attributes { get; set; }
        #endregion

        #region ServiceOrderAddressID
        public abstract class serviceOrderAddressID : PX.Data.IBqlField
        {
        }
        protected Int32? _SrvOrdAddressID;
        [PXDBInt]
        [FSSrvOrdAddressAttribute(typeof(Select<Address,
             Where<True, Equal<False>>>))]
        public virtual Int32? ServiceOrderAddressID
        {
            get
            {
                return this._SrvOrdAddressID;
            }
            set
            {
                this._SrvOrdAddressID = value;
            }
        }
        #endregion
        #region ServiceOrderContactID
        public abstract class serviceOrderContactID : PX.Data.IBqlField
        {
        }
        protected Int32? _SrvOrdContactID;
        [PXDBInt]
        [FSSrvOrdContactAttribute(typeof(Select<Contact,
             Where<True, Equal<False>>>))]
        public virtual Int32? ServiceOrderContactID
        {
            get
            {

                return this._SrvOrdContactID;
            }
            set
            {
                this._SrvOrdContactID = value;
            }
        }
        #endregion  
        #region AllowOverrideContactAddress
        public abstract class allowOverrideContactAddress : PX.Data.IBqlField
        {
        }
        protected Boolean? _AllowOverrideContactAddress;

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AllowOverrideContactAddress
        {
            get
            {
                return this._AllowOverrideContactAddress;
            }
            set
            {
                this._AllowOverrideContactAddress = value;
            }
        }
        #endregion

        #region AllowInvoice
        public abstract class allowInvoice : PX.Data.BQL.BqlBool.Field<allowInvoice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allow Billing", Enabled = false)]
        public virtual bool? AllowInvoice { get; set; }
        #endregion
        #region AssignedEmpID
        public abstract class assignedEmpID : PX.Data.BQL.BqlInt.Field<assignedEmpID> { }

        [PXDBInt]
        [FSSelector_StaffMember_All]
        [PXUIField(DisplayName = "Supervisor", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? AssignedEmpID { get; set; }
        #endregion
        #region AutoDocDesc
        public abstract class autoDocDesc : PX.Data.BQL.BqlString.Field<autoDocDesc> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Service description", Visible = true, Enabled = false)]
        public virtual string AutoDocDesc { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<BAccountSelectorBase.status, IsNull,
                Or<BAccountSelectorBase.status, Equal<BAccount.status.active>,
                Or<BAccountSelectorBase.status, Equal<BAccount.status.oneTime>>>>), 
                PX.Objects.AR.Messages.CustomerIsInStatus, typeof(BAccountSelectorBase.status))]
        [FSSelectorBusinessAccount_CU_PR_VC]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSServiceOrder.customerID>>,
                            And<MatchWithBranch<Location.cBranchID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Location", DirtyRead = true)]
        [PXRestrictor(typeof(Where<Location.isActive, Equal<True>>), IN.Messages.InactiveLocation, typeof(Location.locationCD))]
        [PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
            InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<FSServiceOrder.customerID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.cBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<FSServiceOrder.customerID>>,
            And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>>))]
        public virtual int? LocationID { get; set; }
        #endregion
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Billing Customer")]
        [FSCustomer]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>,
                            And<MatchWithBranch<Location.cBranchID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Billing Location", DirtyRead = true)]
        [PXRestrictor(typeof(Where<Location.isActive, Equal<True>>), IN.Messages.InactiveLocation, typeof(Location.locationCD))]
        public virtual int? BillLocationID { get; set; }
        #endregion

        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Contact")]
        [FSSelectorContact]
        public virtual int? ContactID { get; set; }
        #endregion
        #region ContractID
        public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contract", Enabled = false)]
        [FSSelectorContract]
        [PXRestrictor(typeof(Where<Contract.status, Equal<FSServiceContract.status.Active>>), "Restrictor 1")]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), "Restrictor 2")]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), "Restrictor 3", typeof(Contract.startDate))]
        public virtual int? ContractID { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXDefault(typeof(
            Search<FSxUserPreferences.dfltBranchLocationID, 
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<PX.SM.UserPreferences.defBranchID, Equal<Current<FSServiceOrder.branchID>>>>>))]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID, 
            Where<
                FSBranchLocation.branchID, Equal<Current<FSServiceOrder.branchID>>>>), 
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<FSServiceOrder.branchID>))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Room")]
        [PXSelector(typeof(
            Search<FSRoom.roomID, 
            Where<
                FSRoom.branchLocationID, Equal<Current<FSServiceOrder.branchLocationID>>>>), 
            SubstituteKey = typeof(FSRoom.roomID), DescriptionField = typeof(FSRoom.descr))]
        public virtual string RoomID { get; set; }
        #endregion
        #region OrderDate
        public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }

        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? OrderDate { get; set; }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXDefault(typeof(Search<Company.baseCuryID>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Currency.curyID))]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String CuryID { get; set; }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [ProjectDefault]
        [PXFormula(typeof(Default<billCustomerID>))]
        [ProjectBase(typeof(FSServiceOrder.billCustomerID))]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region DfltProjectTaskID
        public abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Default Project Task", Visibility = PXUIVisibility.Visible, FieldClass = ProjectAttribute.DimensionName)]
        [PXFormula(typeof(Default<projectID>))]
        [PXDefault(typeof(Search<PMTask.taskID,
                        Where<PMTask.projectID, Equal<Current<projectID>>,
                        And<PMTask.isDefault, Equal<True>,
                        And<Where<PMTask.status,
                            Equal<ProjectTaskStatus.active>, Or<PMTask.status, Equal<ProjectTaskStatus.planned>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        [PXForeignReference(typeof(Field<dfltProjectTaskID>.IsRelatedTo<PMTask.taskID>))]
        public virtual int? DfltProjectTaskID { get; set; }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string DocDesc { get; set; }
        #endregion

        #region EstimatedDurationTotal
        public abstract class estimatedDurationTotal : PX.Data.BQL.BqlInt.Field<estimatedDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Estimated Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? EstimatedDurationTotal { get; set; }
        #endregion

        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Hold")]
        public virtual bool? Hold { get; set; }
        #endregion
        #region LongDescr
        public abstract class longDescr : PX.Data.BQL.BqlString.Field<longDescr> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string LongDescr { get; set; }
        #endregion

        #region EstimatedOrderTotal
        public abstract class estimatedOrderTotal : PX.Data.BQL.BqlDecimal.Field<estimatedOrderTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Total", Enabled = false)]
        public virtual Decimal? EstimatedOrderTotal { get; set; }
        #endregion
        #region CuryEstimatedOrderTotal
        public abstract class curyEstimatedOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyEstimatedOrderTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedOrderTotal))]
        [PXUIField(DisplayName = "Estimated Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryEstimatedOrderTotal { get; set; }
        #endregion
        #region BillableOrderTotal
        public abstract class billableOrderTotal : PX.Data.BQL.BqlDecimal.Field<billableOrderTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Total", Enabled = false)]
        public virtual Decimal? BillableOrderTotal { get; set; }
        #endregion
        #region CuryBillableOrderTotal
        public abstract class curyBillableOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyBillableOrderTotal> { }
        private Decimal? _CuryBillableOrderTotal;

        [PXDBCurrency(typeof(curyInfoID), typeof(billableOrderTotal))]
        [PXUIField(DisplayName = "Line Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryBillableOrderTotal
        {
            get
            {
                return _CuryBillableOrderTotal;
            }
            set
            {
                _CuryBillableOrderTotal = value;
            }
        }

        #endregion

        #region Priority
        public abstract class priority : ListField_Priority_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Priority_ServiceOrder.MEDIUM)]
        [PXUIField(DisplayName = "Priority", Visibility = PXUIVisibility.SelectorVisible)]
        [priority.ListAtrribute]
        public virtual string Priority { get; set; }
        #endregion
        #region ProblemID
        public abstract class problemID : PX.Data.BQL.BqlInt.Field<problemID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Problem")]
        [PXSelector(typeof(Search2<FSProblem.problemID,
                            InnerJoin<FSSrvOrdTypeProblem, On<FSProblem.problemID, Equal<FSSrvOrdTypeProblem.problemID>>,
                            InnerJoin<FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSSrvOrdTypeProblem.srvOrdType>>>>,
                            Where<FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>>),
                            SubstituteKey = typeof(FSProblem.problemCD), DescriptionField = typeof(FSProblem.descr))]
        public virtual int? ProblemID { get; set; }
        #endregion
        #region Severity
        public abstract class severity : ListField_Severity_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Severity_ServiceOrder.MEDIUM)]
        [PXUIField(DisplayName = "Severity", Visibility = PXUIVisibility.SelectorVisible)]
        [severity.ListAtrribute]
        public virtual string Severity { get; set; }
        #endregion
        #region SLAETA
        public abstract class sLAETA : PX.Data.BQL.BqlDateTime.Field<sLAETA> { }

        protected DateTime? _SLAETA;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "SLA")]
        [PXUIField(DisplayName = "SLA", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? SLAETA
        {
            get
            {
                return this._SLAETA;
            }

            set
            {
                this.SLAETAUTC = value;
                this._SLAETA = value;
            }
        }
        #endregion
        #region SourceDocType
        public abstract class sourceDocType : PX.Data.BQL.BqlString.Field<sourceDocType> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Source Document Type", Enabled = false)]
        public virtual string SourceDocType { get; set; }
        #endregion
        #region SourceID
        public abstract class sourceID : PX.Data.BQL.BqlInt.Field<sourceID> { }

        [PXDBInt]
        public virtual int? SourceID { get; set; }
        #endregion
        #region SourceRefNbr
        public abstract class sourceRefNbr : PX.Data.BQL.BqlString.Field<sourceRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Source Ref. Nbr.", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string SourceRefNbr { get; set; }
        #endregion
        #region SourceType
        public abstract class sourceType : ListField_SourceType_ServiceOrder
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.SourceType_ServiceOrder.SERVICE_DISPATCH)]
        [PXUIField(DisplayName = "Document Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [sourceType.ListAtrribute]
        public virtual string SourceType { get; set; }
        #endregion

        #region Status
        public abstract class status : ListField_Status_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Status_ServiceOrder.OPEN)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [status.ListAtrribute]
        public virtual string Status { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Workflow Stage")]
        [FSSelectorWorkflowStage(typeof(FSServiceOrder.srvOrdType))]
        [PXDefault(typeof(Search2<FSWFStage.wFStageID,
                    InnerJoin<FSSrvOrdType,
                        On<
                            FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>,
                    Where<
                        FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>,
                    OrderBy<
                        Asc<FSWFStage.parentWFStageID,
                        Asc<FSWFStage.sortOrder>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WFStageID { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0], ShowInReferenceSelector=true)]
        [PXSearchable(SM.SearchCategory.FS, "SM {0}: {1} - {3}", new Type[] { typeof(FSServiceOrder.srvOrdType), typeof(FSServiceOrder.refNbr), typeof(FSServiceOrder.customerID), typeof(Customer.acctName) },
           new Type[] { typeof(Customer.acctCD), typeof(FSServiceOrder.srvOrdType), typeof(FSServiceOrder.custWorkOrderRefNbr), typeof(FSServiceOrder.docDesc) },
           NumberFields = new Type[] { typeof(FSServiceOrder.refNbr) },
           Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(FSServiceOrder.orderDate), typeof(FSServiceOrder.status), typeof(FSServiceOrder.custWorkOrderRefNbr) },
           Line2Format = "{0}", Line2Fields = new Type[] { typeof(FSServiceOrder.docDesc) },
           MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>),
           SelectForFastIndexing = typeof(Select2<FSServiceOrder, InnerJoin<Customer, On<FSServiceOrder.customerID, Equal<Customer.bAccountID>>>>)
        )]
        
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr { get; set; }
        #endregion
        #region SplitLineCntr
        public abstract class splitLineCntr : PX.Data.BQL.BqlInt.Field<splitLineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? SplitLineCntr { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "Created By Screen ID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "Last Modified By Screen ID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region BAccountRequired
        public abstract class bAccountRequired : PX.Data.BQL.BqlBool.Field<bAccountRequired> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Customer Required", Enabled = false)]
        public virtual bool? BAccountRequired { get; set; }
        #endregion
        #region Quote
        public abstract class quote : PX.Data.BQL.BqlBool.Field<quote> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Quote", Enabled = false)]
        public virtual bool? Quote { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                           Where<
                                FSServiceContract.customerID, Equal<Current<FSServiceOrder.customerID>>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Source Service Contract ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<FSSchedule.scheduleID,
                           Where<
                                FSSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                                And<FSSchedule.entityID, Equal<Current<FSServiceOrder.serviceContractID>>>>>),
                           SubstituteKey = typeof(FSSchedule.refNbr))]
        [PXUIField(DisplayName = "Source Schedule ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        [PXDBString(6, IsFixed = true)]
        [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string FinPeriodID { get; set; }
        #endregion
        #region GenerationID
        public abstract class generationID : PX.Data.BQL.BqlInt.Field<generationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Generation ID")]
        public virtual int? GenerationID { get; set; }
        #endregion
        #region CustWorkOrderRefNbr
        public abstract class custWorkOrderRefNbr : PX.Data.BQL.BqlString.Field<custWorkOrderRefNbr> { }

        [PXDBString(40, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "External Reference")]
        public virtual string CustWorkOrderRefNbr { get; set; }
        #endregion
        #region CustPORefNbr
        public abstract class custPORefNbr : PX.Data.BQL.BqlString.Field<custPORefNbr> { }

        [PXDBString(40, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Customer Order")]
        public virtual string CustPORefNbr { get; set; }
        #endregion
        #region ServiceCount
        public abstract class serviceCount : PX.Data.BQL.BqlInt.Field<serviceCount> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Service Count", Enabled = false)]
        public virtual int? ServiceCount { get; set; }
        #endregion
        #region ScheduledServiceCount
        public abstract class scheduledServiceCount : PX.Data.BQL.BqlInt.Field<scheduledServiceCount> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Scheduled Service Count", Enabled = false)]
        public virtual int? ScheduledServiceCount { get; set; }
        #endregion
        #region CompleteServiceCount
        public abstract class completeServiceCount : PX.Data.BQL.BqlInt.Field<completeServiceCount> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Complete Service Count", Enabled = false)]
        public virtual int? CompleteServiceCount { get; set; }
        #endregion
        #region PostedBy
        public abstract class postedBy : ListField_Billing_By
        {
        }

        [PXDBString(2, IsFixed = true)]
        public virtual string PostedBy { get; set; }
        #endregion
        #region PendingAPARSOPost
        public abstract class pendingAPARSOPost : PX.Data.BQL.BqlBool.Field<pendingAPARSOPost> { }

        [PXDBBool]
        [PXDefault(false)]
        public virtual bool? PendingAPARSOPost { get; set; }
        #endregion
        #region PendingINPost
        public abstract class pendingINPost : PX.Data.BQL.BqlBool.Field<pendingINPost> { }

        [PXDBBool]
        [PXDefault(false)]
        public virtual bool? PendingINPost { get; set; }
        #endregion
        #region CBID
        public abstract class cBID : PX.Data.BQL.BqlInt.Field<cBID> { }

        [PXDBInt]
        public virtual int? CBID { get; set; }
        #endregion

        #region SalesPersonID
        public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }

        [SalesPerson(DisplayName = "Salesperson")]
        [PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, 
                          Where<CustDefSalesPeople.bAccountID, Equal<Current<FSServiceOrder.customerID>>, 
                          And<CustDefSalesPeople.locationID, Equal<Current<FSServiceOrder.locationID>>, 
                          And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSServiceOrder.customerID>))]
        [PXFormula(typeof(Default<FSServiceOrder.locationID>))]
        public virtual int? SalesPersonID { get; set; }
        #endregion
        #region Commissionable
        public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }

        [PXDBBool]
        [PXDefault(typeof(
            Search<FSSrvOrdType.commissionable,
                Where<FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Commissionable")]
        public virtual bool? Commissionable { get; set; }
        #endregion

        #region CutOffDate
        public abstract class cutOffDate : PX.Data.BQL.BqlDateTime.Field<cutOffDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Cut-Off Date")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? CutOffDate { get; set; }
        #endregion

        #region ApptDurationTotal
        public abstract class apptDurationTotal : PX.Data.BQL.BqlInt.Field<apptDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Appointment Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? ApptDurationTotal { get; set; }
        #endregion
        #region ApptOrderTotal
        public abstract class apptOrderTotal : PX.Data.BQL.BqlDecimal.Field<apptOrderTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Appointment Amount", Enabled = false)]
        public virtual Decimal? ApptOrderTotal { get; set; }
        #endregion
        #region CuryApptOrderTotal
        public abstract class curyApptOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyApptOrderTotal> { }

        [PXDBBaseCury]
        [PXUIField(DisplayName = "Appointment Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryApptOrderTotal { get; set; }
        #endregion

        #region BillServiceContractID
        public abstract class billServiceContractID : PX.Data.BQL.BqlInt.Field<billServiceContractID> { }

        [PXDBInt]
        [PXDefault(typeof(Null), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID>))]
        [FSSelectorPrepaidServiceContract(typeof(FSServiceOrder.customerID), typeof(FSServiceOrder.billCustomerID))]
        [PXUIField(DisplayName = "Service Contract", FieldClass = "FSCONTRACT")]
        public virtual int? BillServiceContractID { get; set; }
        #endregion
        #region BillContractPeriodID 
        public abstract class billContractPeriodID : PX.Data.BQL.BqlInt.Field<billContractPeriodID> { }

        [PXDBInt]
        [FSSelectorContractBillingPeriod]
        [PXDefault(typeof(Search<FSContractPeriod.contractPeriodID,
                            Where2<
                                Where<
                                    FSContractPeriod.startPeriodDate, LessEqual<Current<FSServiceOrder.orderDate>>,
                                        And<FSContractPeriod.endPeriodDate, GreaterEqual<Current<FSServiceOrder.orderDate>>>>,
                                And<
                                    FSContractPeriod.serviceContractID, Equal<Current<FSServiceOrder.billServiceContractID>>,
                                    And2<
                                         Where<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>,
                                             Or<FSContractPeriod.status, Equal<FSContractPeriod.status.Pending>>>,
                                    And<Current<FSBillingCycle.billingBy>, Equal<FSBillingCycle.billingBy.ServiceOrder>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID, FSServiceOrder.orderDate>))]
        [PXUIField(DisplayName = "Contract Period", Enabled = false)]
        public virtual int? BillContractPeriodID { get; set; }
        #endregion

        #region CuryCostTotal
        public abstract class curyCostTotal : PX.Data.BQL.BqlDecimal.Field<curyCostTotal> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(costTotal))]
        [PXUIField(DisplayName = "Cost Total")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryCostTotal { get; set; }
        #endregion
        #region CostTotal
        public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CostTotal { get; set; }
		#endregion

        #region SOCuryCompletedBillableTotal
        public abstract class sOCuryCompletedBillableTotal : PX.Data.BQL.BqlDecimal.Field<sOCuryCompletedBillableTotal> { }

        [PXCurrency(typeof(curyInfoID), typeof(sOCompletedBillableTotal))]
        [PXUIField(DisplayName = "Billable Total", Enabled = false)]
        public virtual Decimal? SOCuryCompletedBillableTotal { get; set; }
        #endregion
        #region SOCompletedBillableTotal
        public abstract class sOCompletedBillableTotal : PX.Data.BQL.BqlDecimal.Field<sOCompletedBillableTotal> { }

        [PXDecimal]
        public virtual Decimal? SOCompletedBillableTotal { get; set; }
        #endregion

        #region SOCuryUnpaidBalanace
        public abstract class sOCuryUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOCuryUnpaidBalanace> { }

        [PXCurrency(typeof(curyInfoID), typeof(sOUnpaidBalanace))]
        [PXUIField(DisplayName = "Service Order Unpaid Balance", Enabled = false)]
        public virtual Decimal? SOCuryUnpaidBalanace { get; set; }
        #endregion
        #region SOUnpaidBalanace
        public abstract class sOUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOUnpaidBalanace> { }

        [PXDecimal]
        public virtual Decimal? SOUnpaidBalanace { get; set; }
        #endregion

        #region SOCuryBillableUnpaidBalanace
        public abstract class sOCuryBillableUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOCuryBillableUnpaidBalanace> { }

        [PXCurrency(typeof(curyInfoID), typeof(sOBillableUnpaidBalanace))]
        [PXUIField(DisplayName = "Service Order Billable Unpaid Balance", Enabled = false)]
        public virtual Decimal? SOCuryBillableUnpaidBalanace { get; set; }
        #endregion
        #region SOBillableUnpaidBalanace
        public abstract class sOBillableUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOBillableUnpaidBalanace> { }

        [PXDecimal]
        public virtual Decimal? SOBillableUnpaidBalanace { get; set; }
        #endregion


        #region SOPrepaymentReceived
        public abstract class sOPrepaymentReceived : PX.Data.BQL.BqlDecimal.Field<sOPrepaymentReceived> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Prepayment Received", Enabled = false)]
        public virtual Decimal? SOPrepaymentReceived { get; set; }
        #endregion
        #region SOPrepaymentRemaining
        public abstract class sOPrepaymentRemaining : PX.Data.BQL.BqlDecimal.Field<sOPrepaymentRemaining> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Prepayment Remaining", Enabled = false)]
        public virtual Decimal? SOPrepaymentRemaining { get; set; }
        #endregion
        #region SOPrepaymentApplied
        public abstract class sOPrepaymentApplied : PX.Data.BQL.BqlDecimal.Field<sOPrepaymentApplied> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Prepayment Applied", Enabled = false)]
        public virtual Decimal? SOPrepaymentApplied { get; set; }
        #endregion

        #region POLineCntr
        public abstract class pOLineCntr : PX.Data.BQL.BqlInt.Field<pOLineCntr> { }

        [PXInt()]
        public virtual int? POLineCntr { get; set; }
        #endregion
		#region WaitingForParts
        public abstract class waitingForParts : PX.Data.BQL.BqlBool.Field<waitingForParts> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Invoice>,
                               Or<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Module>,
                               Or<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>>>>))]
        [PXUIField(DisplayName = "Waiting for Purchased Items", Enabled = false, FieldClass = "DISTINV")]
        public virtual bool? WaitingForParts { get; set; }
        #endregion
        #region AppointmentsNeeded
        public abstract class appointmentsNeeded : PX.Data.BQL.BqlBool.Field<appointmentsNeeded> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Appointments Needed", Enabled = false)]
        public virtual bool? AppointmentsNeeded { get; set; }
        #endregion

        #region MemoryHelper
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion

        #region ReportLocationID
        public abstract class reportLocationID : PX.Data.BQL.BqlInt.Field<reportLocationID> { }

        [PXInt]
        [PXSelector(typeof(Search<Location.locationID,
                           Where<
                                Location.bAccountID, Equal<Optional<FSServiceOrder.customerID>>>>),
                           SubstituteKey = typeof(Location.locationCD), 
                           DescriptionField = typeof(Location.descr))]
        public virtual int? ReportLocationID { get; set; }
        #endregion
                
        #region Mem_ReturnValueID
        [PXInt]
        public virtual int? Mem_ReturnValueID { get; set; }
        #endregion

        #region Mem_Invoiced
        public abstract class mem_Invoiced : PX.Data.BQL.BqlBool.Field<mem_Invoiced> { }

        [PXBool]
        [PXUIField(DisplayName = "Billed", Enabled = false)]
        public virtual bool? Mem_Invoiced
        {
            get
            {
                return this.PostedBy != null && this.PostedBy == ID.Billing_By.SERVICE_ORDER;
            }
        }
        #endregion

        #region AppointmentsCompleted
        [PXInt]
        public virtual int? AppointmentsCompletedCntr { get; set; }
        #endregion
        #region AppointmentsCompletedOrClosed
        [PXInt]
        public virtual int? AppointmentsCompletedOrClosedCntr { get; set; }
        #endregion
        #region MemRefNbr
        public abstract class memRefNbr : PX.Data.BQL.BqlString.Field<memRefNbr>
		{
        }

        [PXString(17, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCC")]
        public virtual string MemRefNbr { get; set; }
        #endregion
        #region MemAcctName
        public abstract class memAcctName : PX.Data.BQL.BqlString.Field<memAcctName>
		{
        }
        [PXString(62, IsUnicode = true)]
        public virtual string MemAcctName { get; set; }
        #endregion

        #region IsPrepaymentEnable
        public abstract class isPrepaymentEnable : PX.Data.BQL.BqlBool.Field<isPrepaymentEnable> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual bool? IsPrepaymentEnable { get; set; }
        #endregion

		#region UpdateAppWaitingForParts
        public abstract class updateAppWaitingForParts : PX.Data.BQL.BqlBool.Field<updateAppWaitingForParts> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UpdateAppWaitingForParts { get; set; }
        #endregion

        #region SourceReferenceNbr
        public abstract class sourceReferenceNbr : PX.Data.IBqlField { }
        [PXString]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string SourceReferenceNbr
        {
            get
            {
                return (SourceDocType != null ? SourceDocType.Trim() + ", " : "") +
                       (SourceRefNbr != null ? SourceRefNbr.Trim() : "");
            }
        }
        #endregion

        #region TaskCD
        public abstract class taskCD : PX.Data.BQL.BqlBool.Field<taskCD> { }

        [PXString]
        [PXUIField(DisplayName = "Task ID", FieldClass = ProjectAttribute.DimensionName)]
        public virtual string TaskCD { get; set; }
        #endregion
        #region ProjectDescr
        public abstract class projectDescr : PX.Data.BQL.BqlBool.Field<projectDescr> { }

        [PXString]
        [PXUIField(DisplayName = "Project Description", FieldClass = ProjectAttribute.DimensionName)]
        public virtual string ProjectDescr { get; set; }
        #endregion
        #region ProjectTaskDescr
        public abstract class projectTaskDescr : PX.Data.BQL.BqlBool.Field<projectTaskDescr> { }

        [PXString]
        [PXUIField(DisplayName = "Task Description", FieldClass = ProjectAttribute.DimensionName)]
        public virtual string ProjectTaskDescr { get; set; }
        #endregion

        #endregion
        #region DispatchBoardHelper
        #region SLARemaning
        public abstract class sLARemaning : PX.Data.BQL.BqlString.Field<sLARemaning> { }

        [PXString]
        public virtual string SLARemaning { get; set; }
        #endregion
        #region CustomerDisplayName
        public abstract class customerDisplayName : PX.Data.BQL.BqlString.Field<customerDisplayName> { }

        [PXString]
        public virtual string CustomerDisplayName { get; set; }
        #endregion
        #region ContactName
        public abstract class contactName : PX.Data.BQL.BqlString.Field<contactName> { }

        [PXString]
        public virtual string ContactName { get; set; }
        #endregion
        #region ContactPhone
        public abstract class contactPhone : PX.Data.BQL.BqlString.Field<contactPhone> { }

        [PXString]
        public virtual string ContactPhone { get; set; }
        #endregion
        #region ContactEmail
        public abstract class contactEmail : PX.Data.BQL.BqlString.Field<contactEmail> { }

        [PXString]
        public virtual string ContactEmail { get; set; }
        #endregion
        #region AssignedEmployeeDisplayName
        public abstract class assignedEmployeeDisplayName : PX.Data.BQL.BqlString.Field<assignedEmployeeDisplayName> { }

        [PXString]
        public virtual string AssignedEmployeeDisplayName { get; set; }
        #endregion
        #region ServicesRemaning
        public abstract class servicesRemaning : PX.Data.BQL.BqlInt.Field<servicesRemaning> { }

        [PXInt]
        public virtual int? ServicesRemaning { get; set; }
        #endregion
        #region ServicesCount
        public abstract class servicesCount : PX.Data.BQL.BqlInt.Field<servicesCount> { }

        [PXInt]
        public virtual int? ServicesCount { get; set; }
        #endregion
        #region ServiceClassIDs
        public abstract class serviceClassIDs : PX.Data.IBqlField { }

        [PXInt]
        public virtual Array ServiceClassIDs { get; set; }
        #endregion
        #region BranchLocationDesc
        public abstract class branchLocationDesc : PX.Data.BQL.BqlString.Field<branchLocationDesc> { }

        [PXString]
        public virtual string BranchLocationDesc { get; set; }
        #endregion
        #region ServiceOrderTreeHelper
        #region TreeID
        public abstract class treeID : PX.Data.BQL.BqlInt.Field<treeID> { }

        [PXInt]
        public virtual int? TreeID { get; set; }
        #endregion
        #region Text
        public abstract class text : PX.Data.BQL.BqlString.Field<text> { }

        [PXString]
        public virtual string Text { get; set; }
        #endregion
        #region Leaf
        public abstract class leaf : PX.Data.BQL.BqlBool.Field<leaf> { }

        [PXBool]
        public virtual bool? Leaf { get; set; }
        #endregion
        #region Rows
        public abstract class rows : PX.Data.IBqlField { }

        public virtual object Rows { get; set; }
        #endregion
        #endregion
        #region CustomOrderDate
        public abstract class customOrderDate : PX.Data.IBqlField
        {
        }

        [PXString]
        public virtual string CustomOrderDate
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.OrderDate != null)
                {
                    return this.OrderDate.ToString();
                }

                return string.Empty;
            }
        }
        #endregion
        #endregion

        #region UTC Fields
        #region SLAETAUTC
        public abstract class sLAETAUTC : PX.Data.BQL.BqlDateTime.Field<sLAETAUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Deadline - SLA Date", DisplayNameTime = "Deadline - SLA Time")]
        [PXUIField(DisplayName = "Deadline - SLA", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? SLAETAUTC { get; set; }
        #endregion
        #endregion

        #region Tax Fields
        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.vATReporting>))]
        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.vatExemptTotal))]
        [PXUIField(DisplayName = "VAT Exempt Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatExemptTotal { get; set; }
        #endregion
        #region VatExemptTotal
        public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatExemptTotal { get; set; }
        #endregion
        #region CuryVatTaxableTotal
        public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.vATReporting>))]
        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.vatTaxableTotal))]
        [PXUIField(DisplayName = "VAT Taxable Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatTaxableTotal { get; set; }
        #endregion
        #region VatTaxableTotal
        public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatTaxableTotal { get; set; }
        #endregion

        #region TaxZoneID
        public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Tax Zone")]
        [PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
        [PXFormula(typeof(Default<FSServiceOrder.branchID>))]
        [PXFormula(typeof(Default<FSServiceOrder.billLocationID>))]
        public virtual String TaxZoneID { get; set; }
        #endregion

        #region TaxCalcMode
        public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(TaxCalculationMode.TaxSetting,
            typeof(Search<Location.cTaxCalcMode,
                Where<Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>,
                    And<Location.locationID, Equal<Current<FSServiceOrder.billLocationID>>>>>))]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID>))]
        [PXFormula(typeof(Default<FSServiceOrder.billLocationID>))]
        [TaxCalculationMode.List]
        [PXUIField(DisplayName = "Tax Calculation Mode")]
        public virtual string TaxCalcMode { get; set; }
        #endregion

        #region TaxTotal
        public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxTotal { get; set; }
        #endregion
        #region CuryTaxTotal
        public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.taxTotal))]
        [PXUIField(DisplayName = "Tax Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryTaxTotal { get; set; }
        #endregion

        /*Discounts*/
        #region DiscTot
        public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }
        protected Decimal? _DiscTot;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total")]
        public virtual Decimal? DiscTot
        {
            get
            {
                return this._DiscTot;
            }
            set
            {
                this._DiscTot = value;
            }
        }
        #endregion
        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
        protected Decimal? _CuryDiscTot;
        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total")]
        public virtual Decimal? CuryDiscTot
        {
            get
            {
                return this._CuryDiscTot;
            }
            set
            {
                this._CuryDiscTot = value;
            }
        }
        #endregion
        #region DocDisc
        public abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
        protected Decimal? _DocDisc;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? DocDisc
        {
            get
            {
                return this._DocDisc;
            }
            set
            {
                this._DocDisc = value;
            }
        }
        #endregion
        #region CuryDocDisc
        public abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
        protected Decimal? _CuryDocDisc;
        [PXCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.docDisc))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Document Discount", Enabled = true)]
        public virtual Decimal? CuryDocDisc
        {
            get
            {
                return this._CuryDocDisc;
            }
            set
            {
                this._CuryDocDisc = value;
            }
        }
        #endregion
        /*End Discounts*/

        #region DocTotal
        public abstract class docTotal : PX.Data.BQL.BqlDecimal.Field<docTotal> { }
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Base Order Total", Enabled = false)]
        public virtual Decimal? DocTotal { get; set; }
        #endregion
        #region CuryDocTotal
        public abstract class curyDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDocTotal> { }
        [PXDependsOnFields(typeof(curyBillableOrderTotal), typeof(curyDiscTot), typeof(curyTaxTotal))]
        [PXDBCurrency(typeof(curyInfoID), typeof(docTotal))]
        [PXUIField(DisplayName = "Service Order Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDocTotal { get; set; }
        #endregion
        #region CuryLineDocDiscountTotal
        public abstract class curyLineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDocDiscountTotal> { }
        //AC-162992 -> Refactor for adding missing fields required in SalesTax extension 
        [PXCurrency(typeof(curyInfoID))]
        [PXUIField(Enabled = false)]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryLineDocDiscountTotal { get; set; }
        #endregion

        #region IsTaxValid
        public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
        public virtual Boolean? IsTaxValid { get; set; }
        #endregion
        #endregion

        #region IsCalledFromQuickProcess
        public abstract class isCalledFromQuickProcess : PX.Data.BQL.BqlBool.Field<isCalledFromQuickProcess> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsCalledFromQuickProcess { get; set; }
        #endregion
    }
}
