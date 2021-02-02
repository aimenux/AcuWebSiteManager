using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class RouteAppointmentAssignmentFilter : IBqlTable
    {
        #region RouteDate
        public abstract class routeDate : PX.Data.BQL.BqlDateTime.Field<routeDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXDefault(typeof(FSRouteDocument.date))]
        [PXUIField(DisplayName = "Route Date", Visible = true)]
        public virtual DateTime? RouteDate { get; set; }
        #endregion
        #region RouteID
        public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Route")]
        [PXSelector(typeof(FSRoute.routeID), SubstituteKey = typeof(FSRoute.routeCD))]
        public virtual int? RouteID { get; set; }
        #endregion
        #region Mem_RouteShort
        public abstract class mem_RouteShort : PX.Data.BQL.BqlString.Field<mem_RouteShort> { }

        [PXString(10)]
        [PXUIField(DisplayName = "Route Short")]
        [PXSelector(typeof(FSRoute.routeShort))]
        public virtual string Mem_RouteShort { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXString]
        public virtual string RefNbr { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXInt]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXInt]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region RouteDocumentID
        public abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

        [PXInt]
        public virtual int? RouteDocumentID { get; set; }
        #endregion
        #region UnassignAppointment
        public abstract class unassignAppointment : PX.Data.BQL.BqlString.Field<unassignAppointment> { }

        [PXString]
        [PXDefault(ID.AppointmentAssignment_Status.UNASSIGN_APPOINTMENT_ONLY)]
        public virtual string UnassignAppointment { get; set; }
        #endregion

        #region SrvOrdType
        public abstract class appSrvOrdType : PX.Data.BQL.BqlString.Field<appSrvOrdType> { }

        [PXDBString(4, IsFixed = true, IsKey = true)]
        [PXDefault(typeof(Search<FSAppointment.srvOrdType, Where<FSAppointment.appointmentID, Equal<Current<FSAppointment.appointmentID>>>>))]
        [PXUIField(DisplayName = "Service Order Type", Enabled = false, IsReadOnly = true)]
        public virtual string AppSrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class appRefNbr : PX.Data.BQL.BqlString.Field<appRefNbr> { }

        [PXDefault(typeof(Search<FSAppointment.refNbr, Where<FSAppointment.appointmentID, Equal<Current<FSAppointment.appointmentID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSAppointment.appointmentID>))]
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = true, Enabled = false, IsReadOnly = true)]
        public virtual string AppRefNbr { get; set; }
        #endregion
        #region EmpEstimatedDurationTotal
        public abstract class estimatedDurationTotal : PX.Data.BQL.BqlInt.Field<estimatedDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Estimated Duration Total (by employee performance)", Enabled = false, IsReadOnly = true)]
        [PXDefault(typeof(FSAppointment.estimatedDurationTotal), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? EstimatedDurationTotal { get; set; }
        #endregion
        #region ScheduledDateTimeBegin
        public abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Start Time")]
        [PXDefault(typeof(FSAppointment.scheduledDateTimeBegin))]
        [PXUIField(DisplayName = "Scheduled Date", Enabled = false, IsReadOnly = true)]
        public virtual DateTime? ScheduledDateTimeBegin { get; set; }
        #endregion
        #region AddressLine1
        public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

        [PXDefault(typeof(Search2<FSAddress.addressLine1,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                            Where<FSServiceOrder.sOID, Equal<Current<FSAppointment.sOID>>>>))]
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 1", Enabled = false, IsReadOnly = true)]
        public virtual string AddressLine1 { get; set; }
        #endregion
        #region AddressLine2
        public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

        [PXDBString(50, IsUnicode = true)]
        [PXDefault(typeof(Search2<FSAddress.addressLine2,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                            Where<FSServiceOrder.sOID, Equal<Current<FSAppointment.sOID>>>>))]
        [PXUIField(DisplayName = "Address Line 2", Enabled = false, IsReadOnly = true)]
        public virtual string AddressLine2 { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt]
        [PXDefault(typeof(Search<FSServiceOrder.customerID, Where<FSServiceOrder.sOID, Equal<Current<FSAppointment.sOID>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Customer ID", Enabled = false, IsReadOnly = true)]
        [FSSelectorCustomer]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region State
        public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

        [PXDBString(50, IsUnicode = true)]
        [PXDefault(typeof(Search2<FSAddress.state,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                            Where<FSServiceOrder.sOID, Equal<Current<FSAppointment.sOID>>>>))]
        [PXUIField(DisplayName = "State", Enabled = false, IsReadOnly = true)]
        public virtual string State { get; set; }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        [PXDefault(typeof(Search<FSServiceOrder.locationID, Where<FSServiceOrder.sOID, Equal<Current<FSAppointment.sOID>>>>))]
        [PXUIField(DisplayName = "Location ID", Enabled = false, IsReadOnly = true)]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSServiceOrder.customerID>>>), 
                    DescriptionField = typeof(Location.descr), DirtyRead = true)]
        public virtual int? LocationID { get; set; }
        #endregion
        #region City
        public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

        [PXDBString(50, IsUnicode = true)]
        [PXDefault(typeof(Search2<FSAddress.city,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                            Where<FSServiceOrder.sOID, Equal<Current<FSAppointment.sOID>>>>))]
        [PXUIField(DisplayName = "City", Enabled = false, IsReadOnly = true)]
        public virtual string City { get; set; }
        #endregion
    }
}
