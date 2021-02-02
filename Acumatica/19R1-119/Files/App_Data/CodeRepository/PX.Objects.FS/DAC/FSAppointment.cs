using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.APPOINTMENT)]
    [PXPrimaryGraph(typeof(AppointmentEntry))]
    public partial class FSAppointment : PX.Data.IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, IsKey = true, InputMask = ">AAAA")]
        [PXDefault(typeof(FSSetup.dfltSrvOrdType))]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorSrvOrdTypeNOTQuote]
        [PXUIVerify(typeof(Where<Current<FSSrvOrdType.active>, Equal<True>>),
                    PXErrorLevel.Warning, TX.Error.SRVORDTYPE_INACTIVE, CheckOnRowSelected = true)]
        [PX.Data.EP.PXFieldDescription]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        [PXSelector(typeof(
            Search2<FSAppointment.refNbr,
            LeftJoin<FSServiceOrder,
                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
            LeftJoin<Customer, 
                On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
            LeftJoin<Location, 
                On<Location.bAccountID, Equal<FSServiceOrder.customerID>,
                    And<Location.locationID, Equal<FSServiceOrder.locationID>>>>>>,
            Where2<
            Where<
                FSAppointment.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>>,
                And<Where<
                    Customer.bAccountID, IsNull,
                    Or<Match<Customer, Current<AccessInfo.userName>>>>>>,
            OrderBy<
                Desc<FSAppointment.refNbr>>>),
                    new Type[] {
                                typeof(FSAppointment.refNbr),
                                typeof(Customer.acctCD),
                                typeof(Customer.acctName),
                                typeof(Location.locationCD),
                                typeof(FSAppointment.docDesc),
                                typeof(FSAppointment.status),
                                typeof(FSAppointment.scheduledDateTimeBegin)
                    })]
        [AppointmentAutoNumber(typeof(
            Search<FSSrvOrdType.srvOrdNumberingID,
            Where<
                FSSrvOrdType.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>>>),
            typeof(AccessInfo.businessDate))]
		[PX.Data.EP.PXFieldDescription]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBIdentity]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region SORefNbr
        public abstract class soRefNbr : PX.Data.BQL.BqlString.Field<soRefNbr> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [FSSelectorSORefNbr_Appointment]
        public virtual string SORefNbr { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        public virtual int? SOID { get; set; }
        #endregion
        #region Attributes
        /// <summary>
        /// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
        /// added attributes</see> defined at the <see cref="FSSrvOrdType">customer 
        /// class</see> level to function correctly.
        /// </summary>
        [CRAttributesField(typeof(FSAppointment.srvOrdType), typeof(FSAppointment.noteID))]
        public virtual string[] Attributes { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Branch", Enabled = false, Visible = false)]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion

        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Billing Customer")]
        public virtual int? BillCustomerID { get; set; }
        #endregion

        #region ScheduledDateTimeBegin
        public abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }

        protected DateTime? _ScheduledDateTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Scheduled Date", DisplayNameTime = "Scheduled Start Time")]
        [PXDefault]
        [PXUIField(DisplayName = "Scheduled Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ScheduledDateTimeBegin
        {
            get
            {
                return this._ScheduledDateTimeBegin;
            }

            set
            {
                this.ScheduledDateTimeBeginUTC = value;
                this._ScheduledDateTimeBegin = value;
            }
        }
        #endregion
        #region ScheduledDateTimeEnd
        public abstract class scheduledDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEnd> { }

        protected DateTime? _ScheduledDateTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Scheduled End Date", DisplayNameTime = "Scheduled End Time")]
        [PXDefault]
        [PXUIField(DisplayName = "Scheduled Date End", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ScheduledDateTimeEnd
        {
            get
            {
                return this._ScheduledDateTimeEnd;
            }

            set
            {
                this.ScheduledDateTimeEndUTC = value;
                this._ScheduledDateTimeEnd = value;
            }
        }
        #endregion

        #region ExecutionDate
        public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }

        [PXDBDate]
        [PXDefault]
        [PXUIField(DisplayName = "Actual Date", Enabled = false)]
        public virtual DateTime? ExecutionDate { get; set; }
        #endregion
        #region ActualDateTimeBegin
        public abstract class actualDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBegin> { }

        protected DateTime? _ActualDateTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Actual Start Date", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ActualDateTimeBegin
        {
            get
            {
                return this._ActualDateTimeBegin;
            }

            set
            {
                this.ActualDateTimeBeginUTC = value;
                this._ActualDateTimeBegin = value;
            }
        }
        #endregion
        #region ActualDateTimeEnd
        public abstract class actualDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEnd> { }

        protected DateTime? _ActualDateTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Actual Date Time End", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual Date End", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ActualDateTimeEnd
        {
            get
            {
                return this._ActualDateTimeEnd;
            }

            set
            {
                this.ActualDateTimeEndUTC = value;
                this._ActualDateTimeEnd = value;
            }
        }
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

        #region AutoDocDesc
        public abstract class autoDocDesc : PX.Data.BQL.BqlString.Field<autoDocDesc> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Description", Visible = true, Enabled = false)]
        public virtual string AutoDocDesc { get; set; }
        #endregion        
        #region Confirmed
        public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Confirmed")]
        public virtual bool? Confirmed { get; set; }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", FieldName = "DocDesc")]
        public virtual string DocDesc { get; set; }
        #endregion
        #region DeliveryNotes
        public abstract class deliveryNotes : PX.Data.BQL.BqlString.Field<deliveryNotes> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Delivery Notes")]
        public virtual string DeliveryNotes { get; set; }
        #endregion
        #region DfltProjectTaskID
        public abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Default Project Task")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<FSServiceOrder.projectID>>>))]
        public virtual int? DfltProjectTaskID { get; set; }
        #endregion
        #region LongDescr
        public abstract class longDescr : PX.Data.BQL.BqlString.Field<longDescr> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string LongDescr { get; set; }
        #endregion

        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Hold")]
        public virtual bool? Hold { get; set; }
        #endregion
        #region Status
        public abstract class status : ListField_Status_Appointment
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Status_Appointment.MANUAL_SCHEDULED)]
        [status.ListAtrribute]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string Status { get; set; }
        #endregion

        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXDefault]
        [PXUIField(DisplayName = "NoteID")]
        [PXSearchable(SM.SearchCategory.FS, "SM {0}: {1}", new Type[] { typeof(FSAppointment.srvOrdType), typeof(FSAppointment.refNbr) },
           new Type[] { typeof(Customer.acctCD), typeof(FSAppointment.srvOrdType), typeof(FSAppointment.refNbr), typeof(FSAppointment.soRefNbr),  typeof(FSAppointment.docDesc) },
           NumberFields = new Type[] { typeof(FSAppointment.refNbr) },
           Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(FSAppointment.scheduledDateTimeBegin), typeof(FSAppointment.status), typeof(FSAppointment.soRefNbr) },
           Line2Format = "{0}", Line2Fields = new Type[] { typeof(FSAppointment.docDesc) },
           MatchWithJoin = typeof(InnerJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>, InnerJoin<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>),
           SelectForFastIndexing = typeof(Select2<FSAppointment, InnerJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>, InnerJoin<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>)
        )]
        [PXNote(new Type[0], ShowInReferenceSelector = true)]
        public virtual Guid? NoteID { get; set; }
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
        [PXUIField(DisplayName = "CreatedByScreenID")]
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
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
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
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }
        #endregion

        #region EstimatedDurationTotal
        // SetDisplayName in RouteDocumentMaint
        // SetDisplayName in RouteClosingMaint
        public abstract class estimatedDurationTotal : PX.Data.BQL.BqlInt.Field<estimatedDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Estimated Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? EstimatedDurationTotal { get; set; }
        #endregion
        #region ActualDurationTotal
        public abstract class actualDurationTotal : PX.Data.BQL.BqlInt.Field<actualDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Actual Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? ActualDurationTotal { get; set; }
        #endregion

        #region DriveTime
        public abstract class driveTime : PX.Data.BQL.BqlInt.Field<driveTime> { }

        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Driving Time")]
        public virtual int? DriveTime { get; set; }
        #endregion
        #region MapLatitude
        public abstract class mapLatitude : PX.Data.BQL.BqlDecimal.Field<mapLatitude> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Latitude", Enabled = false)]
        public virtual decimal? MapLatitude { get; set; }
        #endregion
        #region MapLongitude
        public abstract class mapLongitude : PX.Data.BQL.BqlDecimal.Field<mapLongitude> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Longitude", Enabled = false)]
        public virtual decimal? MapLongitude { get; set; }
        #endregion
        #region RoutePosition
        public abstract class routePosition : PX.Data.BQL.BqlInt.Field<routePosition> { }

        [PXDBInt(MinValue = 1)]
        [PXUIField(DisplayName = "Route Position")]
        public virtual int? RoutePosition { get; set; }
        #endregion

        #region TimeLocked
        public abstract class timeLocked : PX.Data.BQL.BqlBool.Field<timeLocked> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Time Locked")]
        public virtual bool? TimeLocked { get; set; }
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
                                And< FSSchedule.entityID, Equal<Current<FSServiceOrder.serviceContractID>>>>>),
                           SubstituteKey = typeof(FSSchedule.refNbr))]
        [PXUIField(DisplayName = "Source Schedule ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region OriginalAppointmentID
        public abstract class originalAppointmentID : PX.Data.BQL.BqlInt.Field<originalAppointmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Original Appointment ID", Enabled = false)]
        public virtual int? OriginalAppointmentID { get; set; }
        #endregion
        #region UnreachedCustomer
        public abstract class unreachedCustomer : PX.Data.BQL.BqlBool.Field<unreachedCustomer> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Unreached Customer")]
        public virtual bool? UnreachedCustomer { get; set; }
        #endregion
        #region Route ID
        public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Route ID", Enabled = true)]
        [FSSelectorRouteID]
        public virtual int? RouteID { get; set; }
        #endregion
        #region RouteDocumentID
        public abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<FSRouteDocument.routeDocumentID>), SubstituteKey = typeof(FSRouteDocument.refNbr))]
        [PXUIField(DisplayName = "Route Nbr.")]
        public virtual int? RouteDocumentID { get; set; }
        #endregion
        #region GeneratedBySystem
        public abstract class generatedBySystem : PX.Data.BQL.BqlBool.Field<generatedBySystem> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Generated by System", Enabled = false)]
        public virtual bool? GeneratedBySystem { get; set; }
        #endregion
        #region ValidatedByDispatcher
        public abstract class validatedByDispatcher : PX.Data.BQL.BqlBool.Field<validatedByDispatcher> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Validated by Dispatcher")]
        public virtual bool? ValidatedByDispatcher { get; set; }
        #endregion
        #region VehicleID
        public abstract class vehicleID : PX.Data.BQL.BqlInt.Field<vehicleID> { }

        [PXDBInt]
        [FSSelectorVehicle]
        [PXRestrictor(typeof(Where<FSVehicle.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                TX.Messages.VEHICLE_IS_INSTATUS, typeof(FSVehicle.status))]
        [PXUIField(DisplayName = "Vehicle ID", FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual int? VehicleID { get; set; }
        #endregion
        #region GenerationID
        public abstract class generationID : PX.Data.BQL.BqlInt.Field<generationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Generation ID")]
        public virtual int? GenerationID { get; set; }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        [PXDBString(6, IsFixed = true)]
        [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string FinPeriodID { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Workflow Stage")]
        [FSSelectorWorkflowStage(typeof(FSAppointment.srvOrdType))]
        [PXDefault(typeof(Search2<FSWFStage.wFStageID,
                    InnerJoin<FSSrvOrdType,
                        On<
                            FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>,
                    Where<
                        FSSrvOrdType.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>>,
                    OrderBy<
                        Asc<FSWFStage.parentWFStageID,
                        Asc<FSWFStage.sortOrder>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WFStageID { get; set; }
        #endregion
        #region TimeRegistered
        public abstract class timeRegistered : PX.Data.BQL.BqlBool.Field<timeRegistered> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Approved Staff Times", Enabled = false)]
        public virtual bool? TimeRegistered { get; set; }
        #endregion        
        #region CustomerSignaturePath
        public abstract class CustomerSignaturePath : PX.Data.BQL.BqlString.Field<CustomerSignaturePath> { }

        [PXDBString(IsUnicode = true, InputMask = "")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Customer Signature")]
        public virtual string customerSignaturePath { get; set; }
        #endregion
        #region customerSignedReport
        public abstract class customerSignedReport : PX.Data.BQL.BqlGuid.Field<customerSignedReport> { }

        [PXUIField(DisplayName = "Signed Report ID")]
        [PXDBGuid]
        public virtual Guid? CustomerSignedReport { get; set; }
        #endregion

        #region FullNameSignature
        public abstract class fullNameSignature : PX.Data.BQL.BqlString.Field<fullNameSignature> { }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Full Name")]
        public virtual string FullNameSignature { get; set; }
        #endregion
        #region AdditionalCommentsCustomer
        public abstract class additionalCommentsCustomer : PX.Data.BQL.BqlString.Field<additionalCommentsCustomer> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Additional Comments from Customer")]
        public virtual string AdditionalCommentsCustomer { get; set; }
        #endregion
        #region AdditionalCommentsCustomer
        public abstract class additionalCommentsStaff : PX.Data.BQL.BqlString.Field<additionalCommentsStaff> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Additional Comments from Staff")]
        public virtual string AdditionalCommentsStaff { get; set; }
        #endregion
        #region Agreement
        public abstract class agreementSignature : PX.Data.BQL.BqlBool.Field<agreementSignature> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "I, the Person Above, Agree That the Appointment Has Been Completed")]
        public virtual bool? AgreementSignature { get; set; }
        #endregion
        #region TotalAttendees
        public abstract class totalAttendees : PX.Data.BQL.BqlInt.Field<totalAttendees> { }

        [PXDBInt]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual int? TotalAttendees { get; set; }
        #endregion
        #region SalesPersonID
        public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }

        [SalesPerson(DisplayName = "Salesperson")]
        [PXDefault(typeof(
            Coalesce<
            Search<FSServiceOrder.salesPersonID,
                Where<FSServiceOrder.sOID, Equal<Current<FSServiceOrder.sOID>>>>,
            Search<FSSrvOrdType.salesPersonID,
            Where<
                FSSrvOrdType.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>>>>), 
            PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? SalesPersonID { get; set; }
        #endregion
        #region Commissionable
        public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }

        [PXDBBool]
        [PXDefault(typeof(
            Coalesce<
            Search<FSServiceOrder.commissionable,
                Where<FSServiceOrder.sOID, Equal<Current<FSServiceOrder.sOID>>>>,
            Search<FSSrvOrdType.commissionable,
                Where<FSSrvOrdType.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>>>>), 
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Commissionable")]
        public virtual bool? Commissionable { get; set; }
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
        #region PostingStatusAPARSO
        public abstract class postingStatusAPARSO : ListField_Status_Posting
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Status_Posting.NOTHING_TO_POST)]
        [PXUIField(Visible = false)]
        public virtual string PostingStatusAPARSO { get; set; }
        #endregion
        #region PostingStatusIN
        public abstract class postingStatusIN : ListField_Status_Posting
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Status_Posting.NOTHING_TO_POST)]
        [PXUIField(Visible = false)]
        public virtual string PostingStatusIN { get; set; }
        #endregion

        #region CutOffDate
        public abstract class cutOffDate : PX.Data.BQL.BqlDateTime.Field<cutOffDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Cut-Off Date")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? CutOffDate { get; set; }
        #endregion

        #region GPSLatitudeStart
        public abstract class gPSLatitudeStart : PX.Data.BQL.BqlDecimal.Field<gPSLatitudeStart> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Latitude", Enabled = false)]
        public virtual decimal? GPSLatitudeStart { get; set; }
        #endregion
        #region GPSLongitudeStart
        public abstract class gPSLongitudeStart : PX.Data.BQL.BqlDecimal.Field<gPSLongitudeStart> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Longitude", Enabled = false)]
        public virtual decimal? GPSLongitudeStart { get; set; }
        #endregion

        #region GPSLatitudeComplete
        public abstract class gPSLatitudeComplete : PX.Data.BQL.BqlDecimal.Field<gPSLatitudeComplete> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Latitude", Enabled = false)]
        public virtual decimal? GPSLatitudeComplete { get; set; }
        #endregion
        #region GPSLongitudeComplete
        public abstract class gPSLongitudeComplete : PX.Data.BQL.BqlDecimal.Field<gPSLongitudeComplete> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Longitude", Enabled = false)]
        public virtual decimal? GPSLongitudeComplete { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region EstimatedLineTotal
        public abstract class estimatedLineTotal : PX.Data.BQL.BqlDecimal.Field<estimatedLineTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Total", Enabled = false)]
        public virtual decimal? EstimatedLineTotal { get; set; }
        #endregion
        #region CuryEstimatedLineTotal
        public abstract class curyEstimatedLineTotal : PX.Data.BQL.BqlDecimal.Field<curyEstimatedLineTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedLineTotal))]
        [PXUIField(DisplayName = "Estimated Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryEstimatedLineTotal { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region LineTotal
        public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Actual Total", Enabled = false)]
        public virtual decimal? LineTotal { get; set; }
        #endregion
        #region CuryLineTotal
        public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(lineTotal))]
        [PXUIField(DisplayName = "Actual Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryLineTotal { get; set; }
        #endregion

        #region BillableLineTotal
        public abstract class billableLineTotal : PX.Data.BQL.BqlDecimal.Field<billableLineTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Total", Enabled = false)]
        public virtual Decimal? BillableLineTotal { get; set; }
        #endregion
        #region CuryBillableLineTotal
        public abstract class curyBillableLineTotal : PX.Data.BQL.BqlDecimal.Field<curyBillableLineTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(billableLineTotal))]
        [PXUIField(DisplayName = "Line Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryBillableLineTotal { get; set; }
        #endregion

        #region HandleManuallyScheduleTime
        public abstract class handleManuallyScheduleTime : PX.Data.BQL.BqlBool.Field<handleManuallyScheduleTime> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handle Manually")]
        public virtual bool? HandleManuallyScheduleTime { get; set; }
        #endregion
        #region HandleManuallyActualTime
        public abstract class handleManuallyActualTime : PX.Data.BQL.BqlBool.Field<handleManuallyActualTime> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handle Manually")]
        public virtual bool? HandleManuallyActualTime { get; set; }
        #endregion

        #region BillServiceContractID
        public abstract class billServiceContractID : PX.Data.BQL.BqlInt.Field<billServiceContractID> { }

        [PXDBInt]
        [FSSelectorPrepaidServiceContract(typeof(FSServiceOrder.customerID), typeof(FSServiceOrder.billCustomerID))]
        [PXUIField(DisplayName = "Service Contract", FieldClass = "FSCONTRACT")]
        public virtual int? BillServiceContractID { get; set; }
        #endregion
        #region BillContractPeriodID 
        public abstract class billContractPeriodID : PX.Data.BQL.BqlInt.Field<billContractPeriodID> { }

        [PXDBInt]
        [FSSelectorContractBillingPeriod]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID, FSAppointment.scheduledDateTimeBegin>))]
        [PXUIField(DisplayName = "Contract Period", Enabled = false)]
        public virtual int? BillContractPeriodID { get; set; }
        #endregion
        

        #region CuryCostTotal
        public abstract class curyCostTotal : PX.Data.BQL.BqlDecimal.Field<curyCostTotal> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(costTotal))]
        [PXUIField(DisplayName = "Cost Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryCostTotal { get; set; }
        #endregion
        #region CostTotal
        public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(Enabled = false)]
        public virtual Decimal? CostTotal { get; set; }
        #endregion
        #region ProfitPercent
        public abstract class profitPercent : PX.Data.BQL.BqlDecimal.Field<profitPercent> { }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Profit (%)", Enabled = false)]
        [PXFormula(typeof(
            Mult<
                Div<
                    Switch<
                        Case<
                            Where<curyCostTotal, Equal<SharedClasses.decimal_0>>, SharedClasses.decimal_0>,
                        Sub<curyBillableLineTotal, curyCostTotal>>,
                    Switch<
                        Case<
                            Where<curyCostTotal, Equal<SharedClasses.decimal_0>>, SharedClasses.decimal_1>,
                        curyCostTotal>>,
                SharedClasses.decimal_100>))]
        public virtual decimal? ProfitPercent { get; set; }
        #endregion

        #region Tax Fields
        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.vATReporting>))]
        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.vatExemptTotal))]
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
        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.vatTaxableTotal))]
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
        [PXFormula(typeof(Default<FSAppointment.branchID>))]
        [PXFormula(typeof(Default<FSServiceOrder.billLocationID>))]
        public virtual String TaxZoneID { get; set; }
        #endregion

        #region TaxTotal
        public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxTotal { get; set; }
        #endregion
        #region CuryTaxTotal
        public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.taxTotal))]
        [PXUIField(DisplayName = "Tax Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryTaxTotal { get; set; }
        #endregion

        #region DiscTot
        public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }

        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscTot { get; set; }
        #endregion
        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount")]
        public virtual Decimal? CuryDiscTot { get; set; }
        #endregion

        #region DocTotal
        public abstract class docTotal : PX.Data.BQL.BqlDecimal.Field<docTotal> { }
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Base Order Total", Enabled = false)]
        public virtual Decimal? DocTotal { get; set; }
        #endregion
        #region CuryDocTotal
        public abstract class curyDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDocTotal> { }
        [PXDependsOnFields(typeof(curyBillableLineTotal), typeof(curyDiscTot), typeof(curyTaxTotal))]
        [PXDBCurrency(typeof(curyInfoID), typeof(docTotal))]
        [PXUIField(DisplayName = "Appointment Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDocTotal { get; set; }
        #endregion

        #region IsTaxValid
        public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
        public virtual Boolean? IsTaxValid { get; set; }
        #endregion

        #endregion

        #region WaitingForParts
        public abstract class waitingForParts : PX.Data.BQL.BqlBool.Field<waitingForParts> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.postTo>, Equal<ListField_PostTo_SrvOrdType.Sales_Order_Invoice>,
                               Or<Current<FSSrvOrdType.postTo>, Equal<ListField_PostTo_SrvOrdType.Sales_Order_Module>>>))]
        [PXUIField(DisplayName = "Waiting for Purchased Items", Enabled = false, FieldClass = "DISTINV")]
        public virtual bool? WaitingForParts { get; set; }
        #endregion
        #region Finished
        public abstract class finished : PX.Data.BQL.BqlBool.Field<finished> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Finished")]
        public virtual bool? Finished { get; set; }
        #endregion

		#region AppCompletedBillableTotal
        public abstract class appCompletedBillableTotal : PX.Data.BQL.BqlDecimal.Field<appCompletedBillableTotal> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Appointment Billable Total", Enabled = false)]
        public virtual Decimal? AppCompletedBillableTotal { get; set; }
        #endregion
        
        #region MemoryHelper

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

            [PXBool]
            [PXUIField(DisplayName = "Selected")]
            public virtual bool? Selected { get; set; }
            #endregion

            #region Mem_InvoiceDate
        public abstract class mem_InvoiceDate : PX.Data.BQL.BqlDateTime.Field<mem_InvoiceDate> { }

            [PXDate]
            public virtual DateTime? Mem_InvoiceDate { get; set; }
            #endregion

            #region Mem_InvoiceDocType
            public abstract class mem_InvoiceDocType : PX.Data.BQL.BqlString.Field<mem_InvoiceDocType> { }

            [PXString]
            [PXUIField(DisplayName = "Invoice Doc Type", Enabled = false)]
            public virtual string Mem_InvoiceDocType { get; set; }
            #endregion

            #region Mem_BatchNbr
            public abstract class mem_BatchNbr : PX.Data.BQL.BqlString.Field<mem_BatchNbr> { }

            [PXString(15, IsFixed = true)]
            [PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
            public virtual string Mem_BatchNbr { get; set; }
            #endregion

            #region Mem_InvoiceRef    
            public abstract class mem_InvoiceRef : PX.Data.BQL.BqlString.Field<mem_InvoiceRef> { }

            [PXString(15)]
            [PXUIField(DisplayName = "Invoice Ref. Nbr.", Enabled = false)]
            public virtual string Mem_InvoiceRef { get; set; }
            #endregion

            #region Mem_ScheduledHours
            public abstract class mem_ScheduledHours : PX.Data.BQL.BqlDecimal.Field<mem_ScheduledHours> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Scheduled Hours", Enabled = false)]
            public virtual decimal? Mem_ScheduledHours { get; set; }
            #endregion

            #region Mem_AppointmentHours
            public abstract class mem_AppointmentHours : PX.Data.BQL.BqlDecimal.Field<mem_AppointmentHours> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Appointment Hours", Enabled = false)]
            public virtual decimal? Mem_AppointmentHours { get; set; }
            #endregion

            #region Mem_IdleRate
            public abstract class mem_IdleRate : PX.Data.BQL.BqlDecimal.Field<mem_IdleRate> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Idle Rate (%)", Enabled = false)]
            public virtual decimal? Mem_IdleRate { get; set; }
            #endregion

            #region Mem_OccupationalRate
            public abstract class mem_OccupationalRate : PX.Data.BQL.BqlDecimal.Field<mem_OccupationalRate> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Occupational Rate (%)", Enabled = false)]
            public virtual decimal? Mem_OccupationalRate { get; set; }
            #endregion

            #region Mem_isBeingCloned
            // Useful for skipping unwanted appointment logic during the cloning process    
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual bool? isBeingCloned { get; set; }
            #endregion

            #region Mem_ReturnValueID
            [PXInt]
            public virtual int? Mem_ReturnValueID { get; set; }
            #endregion

            #region Mem_LastRouteDocumentID
            [PXInt]
            public virtual int? Mem_LastRouteDocumentID { get; set; }
            #endregion

            #region Mem_BusinessDateTime
            public abstract class mem_BusinessDateTime : PX.Data.BQL.BqlDateTime.Field<mem_BusinessDateTime> { }

            [PXDateAndTime]
            public virtual DateTime? Mem_BusinessDateTime
            {
                get
                {
                    return PXTimeZoneInfo.Now;
                }
            }
            #endregion

            #region Mem_Duration
            public abstract class mem_Duration : PX.Data.BQL.BqlInt.Field<mem_Duration> { }

            [PXInt]
            [PXFormula(typeof(DateDiff<FSAppointment.scheduledDateTimeBegin, FSAppointment.scheduledDateTimeEnd, DateDiff.minute>))]
            public virtual int? Mem_Duration { get; set; }
            #endregion

            #region Mem_ActualDateTimeBegin_Time
            public abstract class mem_ActualDateTimeBegin_Time : PX.Data.BQL.BqlInt.Field<mem_ActualDateTimeBegin_Time> { }

            [PXTimeList(1, 1440)]
            [PXInt]
            [PXUIField(DisplayName = "Actual Start Time")]
            public virtual int? Mem_ActualDateTimeBegin_Time
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ActualDateTimeBegin != null && ActualDateTimeBegin.Value != null)
                    {
                        return (int?)ActualDateTimeBegin.Value.TimeOfDay.TotalMinutes;
                    }

                    return null;
                }
            }
            #endregion

            #region Mem_ActualDateTimeEnd_Time
            public abstract class mem_ActualDateTimeEnd_Time : PX.Data.BQL.BqlInt.Field<mem_ActualDateTimeEnd_Time> { }

            [PXTimeList(1, 1440)]
            [PXInt]
            [PXUIField(DisplayName = "Actual End Time")]
            public virtual int? Mem_ActualDateTimeEnd_Time
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ActualDateTimeEnd != null && ActualDateTimeEnd.Value != null)
                    {
                        return (int?)ActualDateTimeEnd.Value.TimeOfDay.TotalMinutes;
                    }

                    return null;
                }
            }
            #endregion

            #region WildCard_AssignedEmployeesList
            public abstract class wildCard_AssignedEmployeesList : PX.Data.BQL.BqlString.Field<wildCard_AssignedEmployeesList> { }

            [PXString]
            [PXUIField(DisplayName = "Assigned employees list", Enabled = false)]
            public virtual string WildCard_AssignedEmployeesList { get; set; }
            #endregion
        
            #region WildCard_AssignedEmployeesCellPhoneList
            public abstract class wildCard_AssignedEmployeesCellPhoneList : PX.Data.BQL.BqlString.Field<wildCard_AssignedEmployeesCellPhoneList> { }

            [PXString]
            [PXUIField(DisplayName = "Assigned employees cells list", Enabled = false)]
            public virtual string WildCard_AssignedEmployeesCellPhoneList { get; set; }
            #endregion
        
            #region WildCard_CustomerPrimaryContact
            /// <summary>
            /// This memory field is used to store the names from the contact(s) associated to a given customer.
            /// </summary>
            public abstract class wildCard_CustomerPrimaryContact : PX.Data.BQL.BqlString.Field<wildCard_CustomerPrimaryContact> { }

            [PXString]
            [PXUIField(DisplayName = "Customer primary contact", Enabled = false)]
            public virtual string WildCard_CustomerPrimaryContact { get; set; }
            #endregion

            #region WildCard_CustomerPrimaryContactCell
            /// <summary>
            /// This memory field is used to store the cellphones from the contact(s) associated to a given customer.
            /// </summary>
            public abstract class wildCard_CustomerPrimaryContactCell : PX.Data.BQL.BqlString.Field<wildCard_CustomerPrimaryContactCell> { }

            [PXString]
            [PXUIField(DisplayName = "Customer primary contact cell", Enabled = false)]
            public virtual string WildCard_CustomerPrimaryContactCell { get; set; }
            #endregion

            #region Mem_ScheduledTimeBegin
            public abstract class mem_ScheduledTimeBegin : PX.Data.BQL.BqlString.Field<mem_ScheduledTimeBegin> { }

            [PXString(15, IsUnicode = true)]
            public virtual string Mem_ScheduledTimeBegin
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    return SharedFunctions.GetTimeStringFromDate(this.ScheduledDateTimeBegin);
                }
            }
            #endregion

        #region ScheduledDateBegin
        public abstract class scheduledDateBegin : PX.Data.BQL.BqlString.Field<scheduledDateBegin> { }

        [PXString(15, IsUnicode = true)]
        public virtual string ScheduledDateBegin
        {
            get
            {
                return ScheduledDateTimeBegin?.Date.ToString("MM/dd/yyyy");
            }
        }
		#endregion

		#region Mem_CompanyLogo
		//public abstract class mem_CompanyLogo : PX.Data.BQL.BqlString.Field<mem_CompanyLogo>
		//{
		//}
		//[PXString]
		//[PXUIField(DisplayName = "Logo", Enabled = false)]
		//public virtual string Mem_CompanyLogo
		//{
		//    get
		//    {
		//        StringBuilder names = new StringBuilder();
		//        // SD-7259 what to do in this cases?
		//        names.Append("<img src='http://66.35.42.244/Hoveround_4_20/icons/logo.jpg'>");
		//        return names.ToString();
		//    }
		//}
		#endregion

		#region Mem_ActualDateTime_Month
		public abstract class mem_ActualDateTime_Month : ListField_Month
            {
            }

            [PXString]
            [mem_ActualDateTime_Month.ListAtrribute]
            [PXDefault(ID.Months.JANUARY)]
            [PXUIField(DisplayName = "Month")]
            public virtual string Mem_ActualDateTime_Month
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ScheduledDateTimeBegin != null && ScheduledDateTimeBegin.Value != null)
                    {
                        return SharedFunctions.GetMonthOfYearInStringByID(ScheduledDateTimeBegin.Value.Month);
                    }

                    return null;
                }
            }
            #endregion

            #region Mem_ActualDateTime_Year
            public abstract class mem_ActualDateTime_Year : PX.Data.BQL.BqlInt.Field<mem_ActualDateTime_Year> { }

            [PXInt]
            [PXUIField(DisplayName = "Year")]
            public virtual int? Mem_ActualDateTime_Year
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ScheduledDateTimeBegin != null && ScheduledDateTimeBegin.Value != null)
                    {
                        return (int?)ScheduledDateTimeBegin.Value.Year;
                    }

                    return null;
                }
            }
            #endregion

            #region IsRouteAppoinment
            public abstract class isRouteAppoinment : PX.Data.BQL.BqlBool.Field<isRouteAppoinment> { }

            [PXBool]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(Visible = false)]
            public virtual bool? IsRouteAppoinment { get; set; }
            #endregion

            #region IsPrepaymentEnable
            public abstract class isPrepaymentEnable : PX.Data.BQL.BqlBool.Field<isPrepaymentEnable> { }

            [PXBool]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(Visible = false)]
            public virtual bool? IsPrepaymentEnable { get; set; }
            #endregion

            #region IsReassigned
            public abstract class isReassigned : PX.Data.BQL.BqlBool.Field<isReassigned> { }

            [PXBool]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual bool? IsReassigned { get; set; }
            #endregion

            #region Mem_ShowAttendees
            //Used to manage whether the Attendees tab (Appointment screen) is visible or not.
            public abstract class mem_ShowAttendees : PX.Data.BQL.BqlBool.Field<mem_ShowAttendees> { }

            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(Visible = false)]
            public virtual bool? Mem_ShowAttendees { get; set; }
            #endregion

            #region Mem_SMequipmentID
            public abstract class mem_SMequipmentID : PX.Data.BQL.BqlInt.Field<mem_SMequipmentID> { }

            [PXInt]
            public virtual int? Mem_SMequipmentID { get; set; }
            #endregion

            #region ActualDurationTotalReport
            public abstract class actualDurationTotalReport : PX.Data.BQL.BqlInt.Field<actualDurationTotalReport> { }

            [PXInt]
            [PXFormula(typeof(FSAppointment.actualDurationTotal))]
            public virtual int? ActualDurationTotalReport { get; set; }
            #endregion

            #region AppointmentRefReport
            public abstract class appointmentRefReport : PX.Data.BQL.BqlInt.Field<appointmentRefReport> { }

            [PXInt]
            [PXSelector(typeof(Search<FSAppointment.refNbr,
                               Where<
                                    FSAppointment.soRefNbr, Equal<Optional<FSAppointment.soRefNbr>>>>),
                               SubstituteKey = typeof(FSAppointment.refNbr),
                               DescriptionField = typeof(FSAppointment.refNbr))]
            public virtual int? AppointmentRefReport { get; set; }
            #endregion

            #region Mem_GPSLatitudeLongitude    
            public abstract class mem_GPSLatitudeLongitude : PX.Data.BQL.BqlString.Field<mem_GPSLatitudeLongitude> { }

            [PXString(255)]
            [PXUIField(DisplayName = "GPS Latitude Longitude", Enabled = false)]
            public virtual string Mem_GPSLatitudeLongitude { get; set; }
            #endregion
        #endregion

        #region UTC Fields
        #region ActualDateTimeBeginUTC
        public abstract class actualDateTimeBeginUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBeginUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Actual Date Time Begin", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ActualDateTimeBeginUTC { get; set; }
        #endregion
        #region ActualDateTimeEndUTC
        public abstract class actualDateTimeEndUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEndUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Actual Date Time End", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual Date End", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ActualDateTimeEndUTC { get; set; }
        #endregion
        #region ScheduledDateTimeBeginUTC
        public abstract class scheduledDateTimeBeginUTC : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBeginUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Scheduled Date", DisplayNameTime = "Scheduled Start Time")]
        [PXUIField(DisplayName = "Scheduled Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ScheduledDateTimeBeginUTC { get; set; }
        #endregion
        #region ScheduledDateTimeEndUTC
        public abstract class scheduledDateTimeEndUTC : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEndUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Scheduled End Date", DisplayNameTime = "Scheduled End Time")]
        [PXUIField(DisplayName = "Scheduled Date End", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ScheduledDateTimeEndUTC { get; set; }
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
