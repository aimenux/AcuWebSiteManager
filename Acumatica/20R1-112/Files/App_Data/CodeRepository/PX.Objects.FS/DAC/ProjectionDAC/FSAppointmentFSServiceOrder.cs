using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(AppointmentEntry))]
    [PXProjection(typeof(
        Select2<FSAppointment,
                InnerJoin<FSServiceOrder,
                    On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                InnerJoin<FSAddress,
                    On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                InnerJoin<FSContact,
                    On<FSContact.contactID, Equal<FSServiceOrder.serviceOrderContactID>>>>>>))]
    public partial class FSAppointmentFSServiceOrder : FSAppointment
    {
        #region Appointment Overrides
         
        #region SOID
        public new abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt(BqlField = typeof(FSAppointment.sOID))]
        public override int? SOID { get; set; }
        #endregion
        #region AppointmentID
        public new abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt(BqlField = typeof(FSAppointment.appointmentID))]
        public override int? AppointmentID { get; set; }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC", BqlField = typeof(FSAppointment.refNbr))]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        [PXSelector(typeof(
            Search3<FSAppointment.refNbr,
            LeftJoin<FSServiceOrder,
                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
            LeftJoin<Customer,
                On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
            LeftJoin<Location,
                On<Location.locationID, Equal<FSServiceOrder.locationID>>>>>,
            OrderBy<
                Desc<FSAppointment.refNbr,
                Asc<FSAppointment.srvOrdType>>>>),
                    new Type[] {
                                typeof(FSAppointment.refNbr),
                                typeof(FSAppointment.srvOrdType),
                                typeof(Customer.acctCD),
                                typeof(Customer.acctName),
                                typeof(Location.locationCD),
                                typeof(FSAppointment.docDesc),
                                typeof(FSAppointment.status),
                                typeof(FSAppointment.scheduledDateTimeBegin)
                    })]
        public override string RefNbr { get; set; }
        #endregion
        #region ServiceContractID
        public new abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt(BqlField = typeof(FSAppointment.serviceContractID))]
        [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                           Where<
                                FSServiceContract.customerID, Equal<Current<FSAppointmentFSServiceOrder.customerID>>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Source Service Contract ID", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public override int? ServiceContractID { get; set; }
        #endregion
        #region ScheduleID
        public new abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt(BqlField = typeof(FSAppointment.scheduleID))]
        [PXSelector(typeof(Search<FSSchedule.scheduleID,
                           Where<
                                FSSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                                And<FSSchedule.entityID, Equal<Current<FSServiceOrder.serviceContractID>>>>>),
                           SubstituteKey = typeof(FSSchedule.refNbr))]
        [PXUIField(DisplayName = "Schedule ID", Enabled = false)]
        public override int? ScheduleID { get; set; }
        #endregion
        #region BillServiceContractID
        public new abstract class billServiceContractID : PX.Data.BQL.BqlInt.Field<billServiceContractID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                           Where<
                                FSServiceContract.customerID, Equal<Current<FSAppointmentFSServiceOrder.customerID>>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Service Contract ID", Enabled = false, Visible = false)]
        public override int? BillServiceContractID { get; set; }
        #endregion
        #region ScheduledDateTimeBegin
        public new abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Scheduled Date", DisplayNameTime = "Scheduled Start Time", BqlField = typeof(FSAppointment.scheduledDateTimeBegin))]
        [PXUIField(DisplayName = "Scheduled Date", Visibility = PXUIVisibility.SelectorVisible)]
        public override DateTime? ScheduledDateTimeBegin { get; set; }
        #endregion
        #region ScheduledDateTimeEnd
        public new abstract class scheduledDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEnd> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Scheduled End Date", DisplayNameTime = "Scheduled End Time", BqlField = typeof(FSAppointment.scheduledDateTimeEnd))]
        [PXUIField(DisplayName = "Scheduled End Date", Visibility = PXUIVisibility.Invisible)]
        public override DateTime? ScheduledDateTimeEnd { get; set; }
        #endregion
        #region SrvOrdType
        public new abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true, BqlField = typeof(FSAppointment.srvOrdType))]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorSrvOrdTypeNOTQuote]
        public override string SrvOrdType { get; set; }
        #endregion
        #region EstimatedDurationTotal
        public new abstract class estimatedDurationTotal : PX.Data.BQL.BqlInt.Field<estimatedDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(FSAppointment.estimatedDurationTotal))]
        [PXUIField(DisplayName = "Estimated Duration Total", Enabled = false, Visible = false)]
        public override int? EstimatedDurationTotal { get; set; }
        #endregion
        #region ActualDurationTotal
        public new abstract class actualDurationTotal : PX.Data.BQL.BqlInt.Field<actualDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(FSAppointment.actualDurationTotal))]
        [PXUIField(DisplayName = "Actual Duration Total", Enabled = false, Visible = false)]
        public override int? ActualDurationTotal { get; set; }
        #endregion
        #region WaitingForParts
        public new abstract class waitingForParts : PX.Data.BQL.BqlBool.Field<waitingForParts> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Waiting for Purchased Items", Visible = false, Enabled = false)]
        public override bool? WaitingForParts { get; set; }
        #endregion

        #endregion

        #region Service Order Fields 

        #region AddressLine1
        public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.addressLine1))]
        [PXUIField(DisplayName = "Address Line 1")]
        public virtual string AddressLine1 { get; set; }
        #endregion
        #region AddressLine2
        public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.addressLine2))]
        [PXUIField(DisplayName = "Address Line 2")]
        public virtual string AddressLine2 { get; set; }
        #endregion
        #region AddressLine3
        public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.addressLine3))]
        [PXUIField(DisplayName = "Address Line 3", Visible = false, Enabled = false)]
        public virtual string AddressLine3 { get; set; }
        #endregion
        #region AddressValidated
        public abstract class addressValidated : PX.Data.BQL.BqlBool.Field<addressValidated> { }

        [PXDBBool(BqlField = typeof(FSAddress.isValidated))]
        [PXUIField(DisplayName = "Address Validated", Enabled = true)]
        public virtual bool? AddressValidated { get; set; }
        #endregion
        #region AssignedEmpID
        public abstract class assignedEmpID : PX.Data.BQL.BqlInt.Field<assignedEmpID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.assignedEmpID))]
        [FSSelector_StaffMember_All]
        [PXUIField(DisplayName = "Assigned To", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? AssignedEmpID { get; set; }
        #endregion
        #region BillCustomerID
        public new abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.billCustomerID))]
        [PXUIField(DisplayName = "Billing Customer", Visible = false)]
        [FSSelectorCustomer]
        public override int? BillCustomerID { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>>), 
                    DescriptionField = typeof(Location.descr),
                    BqlField = typeof(FSServiceOrder.billLocationID), 
                    DisplayName = "Billing Location", Visible = false, DirtyRead = true)]
        public virtual int? BillLocationID { get; set; }
        #endregion
        #region City
        public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.city))]
        [PXUIField(DisplayName = "City", Visible = false)]
        public virtual string City { get; set; }
        #endregion
        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.contactID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Contact ID")]
        [FSSelectorContact]
        public virtual int? ContactID { get; set; }
        #endregion
        #region ContractID
        public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.contractID))]
        [PXUIField(DisplayName = "Contract", Enabled = false)]
        [FSSelectorContract]
        public virtual int? ContractID { get; set; }
        #endregion
        #region CountryID
        public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

        [PXDBString(2, IsUnicode = true, BqlField = typeof(FSAddress.countryID))]
        [PXUIField(DisplayName = "Country")]
        [Country]
        public virtual string CountryID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.branchLocationID))]
        [PXUIField(DisplayName = "Branch Location ID")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<FSServiceOrder.branchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(FSServiceOrder.roomID))]
        [PXUIField(DisplayName = "Room", Visible = false)]
        [PXSelector(typeof(
            Search<FSRoom.roomID,
            Where<
                FSRoom.branchLocationID, Equal<Current<FSServiceOrder.branchLocationID>>>>),
            SubstituteKey = typeof(FSRoom.roomID), DescriptionField = typeof(FSRoom.descr))]
        public virtual string RoomID { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.customerID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorCustomer]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region DocDesc
        public abstract class soDocDesc : PX.Data.BQL.BqlString.Field<soDocDesc> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(FSServiceOrder.docDesc))]
        [PXUIField(DisplayName = "Description")]
        public virtual string SODocDesc { get; set; }
        #endregion
        #region EMail
        public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }

        [PXDBEmail(BqlField = typeof(FSContact.email))]
        [PXUIField(DisplayName = "Email")]
        public virtual string EMail { get; set; }
        #endregion
        #region Fax
        public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }

        [PXDBString(50, BqlField = typeof(FSContact.fax))]
        [PXUIField(DisplayName = "Fax")]
        public virtual string Fax { get; set; }
        #endregion
        #region Hold
        public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        [PXDBBool(BqlField = typeof(FSServiceOrder.hold))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Hold")]
        public override bool? Hold { get; set; }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSServiceOrder.customerID>>>), 
                    DescriptionField = typeof(Location.descr),
                    BqlField = typeof(FSServiceOrder.locationID), DisplayName = "Location", DirtyRead = true)]
        public virtual int? LocationID { get; set; }
        #endregion
        #region Phone1
        public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

        [PXDBString(50, BqlField = typeof(FSContact.phone1))]
        [PXUIField(DisplayName = "Phone 1")]
        public virtual string Phone1 { get; set; }
        #endregion
        #region Phone2
        public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

        [PXDBString(50, BqlField = typeof(FSContact.phone2))]
        [PXUIField(DisplayName = "Phone 2")]
        public virtual string Phone2 { get; set; }
        #endregion
        #region Phone3
        public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }

        [PXDBString(50, BqlField = typeof(FSContact.phone3))]
        [PXUIField(DisplayName = "Phone 3", Visible = false, Enabled = false)]
        public virtual string Phone3 { get; set; }
        #endregion
        #region PostalCode
        public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

        [PXDBString(20, BqlField = typeof(FSAddress.postalCode))]
        [PXUIField(DisplayName = "Postal Code")]
        public virtual string PostalCode { get; set; }
        #endregion
        #region CuryEstimatedOrderTotal
        public abstract class curyEstimatedOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyEstimatedOrderTotal> { }

        [PXDBBaseCury(BqlField = typeof(FSServiceOrder.curyEstimatedOrderTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Estimated Order Total", Enabled = false)]
        public virtual decimal? CuryEstimatedOrderTotal { get; set; }
        #endregion
        #region Priority
        public abstract class priority : ListField_Priority_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true, BqlField = typeof(FSServiceOrder.priority))]
        [PXUIField(DisplayName = "Priority", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        [priority.ListAtrribute]
        public virtual string Priority { get; set; }
        #endregion
        #region ProblemID
        public abstract class problemID : PX.Data.BQL.BqlInt.Field<problemID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.problemID))]
        [PXUIField(DisplayName = "Problem ID")]
        [PXSelector(typeof(Search2<FSProblem.problemID,
                            InnerJoin<FSSrvOrdTypeProblem, On<FSProblem.problemID, Equal<FSSrvOrdTypeProblem.problemID>>,
                            InnerJoin<FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSSrvOrdTypeProblem.srvOrdType>>>>,
                            Where<FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>>),
                            SubstituteKey = typeof(FSProblem.problemCD), DescriptionField = typeof(FSProblem.descr))]
        public virtual int? ProblemID { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[ProjectBase(BqlField = typeof(FSServiceOrder.projectID))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region Severity
        public abstract class severity : ListField_Severity_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true, BqlField = typeof(FSServiceOrder.severity))]
        [PXDefault(ID.Severity_ServiceOrder.MEDIUM)]
        [PXUIField(DisplayName = "Severity", Visibility = PXUIVisibility.SelectorVisible)]
        [severity.ListAtrribute]
        public virtual string Severity { get; set; }
        #endregion
        #region SLAETA
        public abstract class sLAETA : PX.Data.BQL.BqlDateTime.Field<sLAETA> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Deadline - SLA Date", DisplayNameTime = "Deadline - SLA Time", BqlField = typeof(FSServiceOrder.sLAETA))]
        [PXUIField(DisplayName = "Deadline - SLA", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? SLAETA { get; set; }
        #endregion
        #region SourceDocType
        public abstract class sourceDocType : PX.Data.BQL.BqlString.Field<sourceDocType> { }

        [PXDBString(4, IsFixed = true, BqlField = typeof(FSServiceOrder.sourceDocType))]
        [PXUIField(DisplayName = "Source document type", Enabled = false)]
        public virtual string SourceDocType { get; set; }
        #endregion
        #region SourceID
        public abstract class sourceID : PX.Data.BQL.BqlInt.Field<sourceID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.sourceID))]
        public virtual int? SourceID { get; set; }
        #endregion
        #region SourceRefNbr
        public abstract class sourceRefNbr : PX.Data.BQL.BqlString.Field<sourceRefNbr> { }

        [PXDBString(15, IsUnicode = true, BqlField = typeof(FSServiceOrder.sourceRefNbr))]
        [PXUIField(DisplayName = "Source Ref. Nbr.", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string SourceRefNbr { get; set; }
        #endregion
        #region SourceType
        public abstract class sourceType : ListField_SourceType_ServiceOrder
        {
        }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSServiceOrder.sourceType))]
        [PXUIField(DisplayName = "Source Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [sourceType.ListAtrribute]
        public virtual string SourceType { get; set; }
        #endregion
        #region State
        public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

        [PXDBString(50, IsUnicode = true, BqlField = typeof(FSAddress.state))]
        [PXUIField(DisplayName = "State", Visible = false)]
        public virtual string State { get; set; }
        #endregion
        #region BAccountRequired
        public abstract class bAccountRequired : PX.Data.BQL.BqlBool.Field<bAccountRequired> { }

        [PXDBBool(BqlField = typeof(FSServiceOrder.bAccountRequired))]
        [PXUIField(DisplayName = "Customer Required", Enabled = false)]
        public virtual bool? BAccountRequired { get; set; }
        #endregion
        #region Quote
        public abstract class quote : PX.Data.BQL.BqlBool.Field<quote> { }

        [PXDBBool(BqlField = typeof(FSServiceOrder.quote))]
        [PXUIField(DisplayName = "Quote", Enabled = false)]
        public virtual bool? Quote { get; set; }
        #endregion
        #region CustWorkOrderRefNbr
        public abstract class custWorkOrderRefNbr : PX.Data.BQL.BqlString.Field<custWorkOrderRefNbr> { }

        [PXDBString(40, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", BqlField = typeof(FSServiceOrder.custWorkOrderRefNbr))]
        [PXUIField(DisplayName = "Customer Work Order Ref. Nbr.")]
        public virtual string CustWorkOrderRefNbr { get; set; }
        #endregion
        /*#region CustPORefNbr
        public abstract class custPORefNbr : PX.Data.BQL.BqlString.Field<custPORefNbr> { }

        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(FSServiceOrder.custPORefNbr))]
        [PXUIField(DisplayName = "Customer Purchase Order Ref. Nbr.")]
        public virtual string CustPORefNbr { get; set; }
        #endregion*/
        #region PostedBy
        public abstract class postedBy : PX.Data.BQL.BqlString.Field<postedBy> { }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSServiceOrder.postedBy))]
        public virtual string PostedBy { get; set; }
        #endregion
        #region CBID
        public abstract class cBID : PX.Data.BQL.BqlInt.Field<cBID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.cBID))]
        public virtual int? CBID { get; set; }
        #endregion
        #region UTC Fields
        #region SLAETAUTC
        public abstract class sLAETAUTC : PX.Data.BQL.BqlDateTime.Field<sLAETAUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Deadline - SLA Date", DisplayNameTime = "Deadline - SLA Time", BqlField = typeof(FSServiceOrder.sLAETAUTC))]
        [PXUIField(DisplayName = "Deadline - SLA", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? SLAETAUTC { get; set; }
        #endregion
        #endregion

        #endregion

        #region ScheduledDuration
        public abstract class scheduledDuration : PX.Data.BQL.BqlInt.Field<scheduledDuration> { }

        [PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Scheduled Duration", Enabled = false)]
        public virtual int? ScheduledDuration
        {
            [PXDependsOnFields(typeof(scheduledDateTimeBegin), typeof(scheduledDateTimeEnd))]
            get
            {
                if (ScheduledDateTimeBegin != null && ScheduledDateTimeEnd != null)
                    return Convert.ToInt32(SharedFunctions.GetTimeSpanDiff(ScheduledDateTimeBegin.Value, ScheduledDateTimeEnd.Value).TotalMinutes);

                return 0;
            }
        }
        #endregion
    }
}