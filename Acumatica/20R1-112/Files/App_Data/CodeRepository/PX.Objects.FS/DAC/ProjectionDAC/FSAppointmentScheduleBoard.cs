using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXProjection(typeof(
            Select2<FSAppointment,
                        InnerJoin<FSServiceOrder,
                            On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                        LeftJoin<Customer,
                            On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
                        LeftJoin<Location,
                            On<Location.locationID, Equal<FSServiceOrder.locationID>>,
                        InnerJoin<FSContact,
                            On<FSContact.contactID, Equal<FSServiceOrder.serviceOrderContactID>>,
                        LeftJoin<FSWFStage,
                            On<FSWFStage.wFStageID, Equal<FSAppointment.wFStageID>>,
                        LeftJoin<FSBranchLocation,
                            On<FSBranchLocation.branchLocationID, Equal<FSServiceOrder.branchLocationID>>>>>>>>>))]
    #endregion
    public class FSAppointmentScheduleBoard : IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, IsKey = true, IsUnicode = true, BqlField = typeof(FSServiceOrder.srvOrdType))]
        [PXUIField(DisplayName = "Service Order Type")]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBIdentity(BqlField = typeof(FSAppointment.appointmentID))]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.sOID))]
        public virtual int? SOID { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC", BqlField = typeof(FSAppointment.refNbr))]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        public virtual string RefNbr { get; set; }
        #endregion
        #region SORefNbr
        public abstract class soRefNbr : PX.Data.BQL.BqlString.Field<soRefNbr> { }

        [PXDBString(15, IsUnicode = true, BqlField = typeof(FSServiceOrder.refNbr))]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        public virtual string SORefNbr { get; set; }
        #endregion

        #region Status
        public abstract class status : ListField_Status_Appointment
        {
        }

        [PXDBString(1, IsFixed = true, BqlField = typeof(FSAppointment.status))]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string Status { get; set; }
        #endregion
        #region Confirmed
        public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }

        [PXDBBool(BqlField = typeof(FSAppointment.confirmed))]
        [PXUIField(DisplayName = "Confirmed")]
        public virtual bool? Confirmed { get; set; }
        #endregion
        #region ValidatedByDispatcher
        public abstract class validatedByDispatcher : PX.Data.BQL.BqlBool.Field<validatedByDispatcher> { }

        [PXDBBool(BqlField = typeof(FSAppointment.validatedByDispatcher))]
        [PXUIField(DisplayName = "Confirmed")]
        public virtual bool? ValidatedByDispatcher { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.branchID))]
        [PXUIField(DisplayName = "Branch ID")]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.branchLocationID))]
        [PXUIField(DisplayName = "Branch Location ID")]
        public virtual int? BranchLocationID { get; set; }
        #endregion

        #region BranchLocationDesc
        public abstract class branchLocationDesc : PX.Data.BQL.BqlString.Field<branchLocationDesc> { }

        [PXDBLocalizableString(255, BqlField = typeof(FSBranchLocation.descr), IsProjection = true)]
        [PXUIField(DisplayName = "Branch Location Desc")]
        public virtual string BranchLocationDesc { get; set; }
        #endregion

        #region ScheduledDateTimeBegin
        public abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Start Time", BqlField = typeof(FSAppointment.scheduledDateTimeBegin))]
        [PXUIField(DisplayName = "Scheduled Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ScheduledDateTimeBegin { get; set; }
        #endregion
        #region ScheduledDateTimeEnd
        public abstract class scheduledDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEnd> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "End Time", BqlField = typeof(FSAppointment.scheduledDateTimeEnd))]
        [PXUIField(DisplayName = "Scheduled Date End", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ScheduledDateTimeEnd { get; set; }
        #endregion

        #region ContactName
        public abstract class contactName : PX.Data.BQL.BqlString.Field<contactName> { }

        private string _ContactName;
        [PXDBString(50, IsFixed = true, BqlField = typeof(FSContact.attention))]
        [PXUIField(DisplayName = "Contact Name")]
        public virtual string ContactName
        {
            get
            {
                return _ContactName?.Trim();
            }
            set
            {
                _ContactName = value;
            }
        }
        #endregion
        #region ContactPhone
        public abstract class contactPhone : PX.Data.BQL.BqlString.Field<contactPhone> { }

        private string _ContactPhone;
        [PXDBString(50, IsFixed = true, BqlField = typeof(FSContact.phone1))]
        [PXUIField(DisplayName = "Contact Phone")]
        public virtual string ContactPhone
        {
            get
            {
                return _ContactPhone?.Trim();
            }
            set
            {
                _ContactPhone = value;
            }
        }
        #endregion
        #region ContactEmail
        public abstract class contactEmail : PX.Data.BQL.BqlString.Field<contactEmail> { }

        [PXDBString(50, IsFixed = true, BqlField = typeof(FSContact.email))]
        [PXUIField(DisplayName = "Contact Email")]
        public virtual string ContactEmail { get; set; }
        #endregion

        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt(BqlField = typeof(Customer.bAccountID))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CustomerName
        public abstract class customerName : PX.Data.BQL.BqlString.Field<customerName> { }

        [PXDBString(60, BqlField = typeof(Customer.acctName))]
        [PXUIField(DisplayName = "Customer Name")]
        public virtual string CustomerName { get; set; }
        #endregion
        #region CustomerLocation
        public abstract class customerLocation : PX.Data.BQL.BqlInt.Field<customerLocation> { }

        [PXDBInt(BqlField = typeof(Customer.defLocationID))]
        [PXUIField(DisplayName = "Customer Location")]
        public virtual int? CustomerLocation { get; set; }
        #endregion
        
        #region WFStageCD
        public abstract class wFStageCD : PX.Data.BQL.BqlString.Field<wFStageCD> { }

        [PXDBString(1, IsFixed = true, BqlField = typeof(FSWFStage.wFStageCD))]
        [PXUIField(DisplayName = "WFStageCD")]
        public virtual string WFStageCD { get; set; }
        #endregion
        #region LocationDesc
        public abstract class locationDesc : PX.Data.BQL.BqlString.Field<locationDesc> { }

        [PXDBString(1, IsFixed = true, BqlField = typeof(Location.descr))]
        [PXUIField(DisplayName = "LocationDesc")]
        public virtual string LocationDesc { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }

        [PXDBString(1, IsFixed = true, BqlField = typeof(FSServiceOrder.roomID))]
        [PXUIField(DisplayName = "Room")]
        public virtual string RoomID { get; set; }
        #endregion
        #region RoomDesc
        public abstract class roomDesc : PX.Data.BQL.BqlString.Field<roomDesc> { }

        [PXString]
        public virtual string RoomDesc { get; set; }
        #endregion
        #region FirstServiceDesc
        public abstract class firstServiceDesc : PX.Data.BQL.BqlString.Field<firstServiceDesc> { }

        [PXString]
        public virtual string FirstServiceDesc { get; set; }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

        [PXDBString(255, BqlField = typeof(FSAppointment.docDesc))]
        [PXUIField(DisplayName = "DocDesc")]
        public virtual string DocDesc { get; set; }
        #endregion

        #region AddressID
        public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }

        [PXDBInt(BqlField = typeof(Location.defAddressID))]
        public virtual int? AddressID { get; set; }
        #endregion

        #region DispatchBoardHelper
        #region CustomID
        public abstract class customID : PX.Data.BQL.BqlString.Field<customID> { }

        [PXString]
        public virtual string CustomID { get; set; }
        #endregion
        #region CustomDateID
        public abstract class customDateID : PX.Data.BQL.BqlString.Field<customID> { }

        [PXString]
        public virtual string CustomDateID { get; set; }
        #endregion
        #region CustomRoomID
        public abstract class customRoomID : PX.Data.BQL.BqlString.Field<customRoomID> { }

        [PXString]
        public virtual string CustomRoomID
        {
            get
            {
                if (this.BranchLocationID != null && string.IsNullOrEmpty(this.RoomID) == false)
                {
                    return this.BranchLocationID.ToString() + "-" + this.RoomID;
                }

                return string.Empty;
            }
        }
        #endregion
        #region AppointmentCustomID
        public abstract class appointmentCustomID : PX.Data.BQL.BqlString.Field<appointmentCustomID> { }

        [PXInt]
        public virtual string AppointmentCustomID { get; set; }
        #endregion

        #region CustomDateTimeStart
        public abstract class customDateTimeStart : PX.Data.BQL.BqlString.Field<customDateTimeStart> { }

        [PXString]
        public virtual string CustomDateTimeStart
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.ScheduledDateTimeBegin != null)
                {
                    return this.ScheduledDateTimeBegin.ToString();
                }

                return string.Empty;
            }
        }
        #endregion
        #region CustomDateTimeEnd
        public abstract class customDateTimeEnd : PX.Data.BQL.BqlString.Field<customDateTimeEnd> { }

        [PXString]
        public virtual string CustomDateTimeEnd
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.ScheduledDateTimeEnd != null)
                {
                    return this.ScheduledDateTimeEnd.ToString();
                }

                return string.Empty;
            }
        }
        #endregion

        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXInt]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region OldEmployeeID
        public abstract class oldEmployeeID : PX.Data.BQL.BqlInt.Field<oldEmployeeID> { }

        [PXInt]
        public virtual int? OldEmployeeID { get; set; }
        #endregion
        #region EmployeeList
        public abstract class employeeList : PX.Data.IBqlField { }

        [PXStringList]
        public virtual List<string> EmployeeList { get; set; }
        #endregion
        #region EmployeeCount
        public abstract class employeeCount : PX.Data.BQL.BqlInt.Field<employeeCount> { }

        [PXInt]
        public virtual int? EmployeeCount { get; set; }
        #endregion
       
        #region ServiceCount
        public abstract class serviceCount : PX.Data.BQL.BqlInt.Field<serviceCount> { }

        [PXInt]
        public virtual int? ServiceCount { get; set; }
        #endregion
        #region ServiceList
        public abstract class serviceList : PX.Data.IBqlField { }

        [PXStringList]
        public virtual List<string> ServiceList { get; set; }
        #endregion

        #region SMEquipmentID
        public abstract class smEquipmentID : PX.Data.BQL.BqlInt.Field<smEquipmentID> { }

        [PXDBInt]
        public virtual int? SMEquipmentID { get; set; }
        #endregion

        #region CanDeleteAppointment
        [PXDefault(false)]
        public virtual bool? CanDeleteAppointment { get; set; }
        #endregion
        #region MemRefNbr
        public abstract class memRefNbr : PX.Data.BQL.BqlString.Field<memRefNbr>
		{
        }

        [PXString(22, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCCCC")]
        public virtual string MemRefNbr { get; set; }
        #endregion
        #region MemAcctName
        public abstract class memAcctName : PX.Data.BQL.BqlString.Field<memAcctName>
		{
        }
        [PXString(62, IsUnicode = true)]
        public virtual string MemAcctName { get; set; }
        #endregion
        #region OpenAppointmentScreenOnError
        [PXBool]
        [PXDefault(false)]
        public virtual bool? OpenAppointmentScreenOnError { get; set; }
        #endregion

        #endregion  
    }
}
