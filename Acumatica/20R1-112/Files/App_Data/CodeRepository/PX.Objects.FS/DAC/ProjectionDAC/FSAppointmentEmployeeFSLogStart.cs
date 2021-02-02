using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXProjection(typeof(Select5<FSAppointmentEmployee,
                         LeftJoin<Vendor,
                         On<
                             Vendor.bAccountID, Equal<FSAppointmentEmployee.employeeID>>,
                         LeftJoin<EPEmployee,
                         On<
                             EPEmployee.bAccountID, Equal<FSAppointmentEmployee.employeeID>>>>,
                         Aggregate <
                             GroupBy<FSAppointmentEmployee.srvOrdType,
                             GroupBy<FSAppointmentEmployee.refNbr,
                             GroupBy<FSAppointmentEmployee.employeeID>>>>>))]
    public class FSAppointmentEmployeeFSLogStart : IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true, BqlField = typeof(FSAppointmentEmployee.srvOrdType))]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "", BqlField = typeof(FSAppointmentEmployee.refNbr))]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSAppointment,
                            Where<FSAppointment.srvOrdType, Equal<Current<FSAppointmentEmployee.srvOrdType>>,
                                And<FSAppointment.refNbr, Equal<Current<FSAppointmentEmployee.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region DocID
        public abstract class docID : PX.Data.BQL.BqlInt.Field<docID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentEmployee.appointmentID))]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.", Visible = false, Enabled = false)]
        public virtual int? DocID { get; set; }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentEmployee.employeeID), IsKey = true)]
        [PXUIField(DisplayName = "Staff Member", Enabled = false)]
        [PXUIVisible(typeof(Where<
                                Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                                And<Current<FSLogActionFilter.type>, NotEqual<FSLogTypeAction.StaffAssignment>,
                                And<Current<FSLogActionFilter.type>, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>,
                                And<Current<FSLogActionFilter.me>, Equal<False>>>>>))]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        public virtual int? BAccountID { get; set; }
        #endregion
        #region UserID
        public abstract class userID : Data.BQL.BqlGuid.Field<userID> { }
        [PXDBGuid(BqlField = typeof(EPEmployee.userID))]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual Guid? UserID { get; set; }
        #endregion
        #region Selected
        public abstract class selected : Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        [PXFormula(typeof(Switch<Case<Where<userID, Equal<Current<AccessInfo.userID>>>, True>, False>))]
        [PXUIVisible(typeof(Where<
                                Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                                And<Current<FSLogActionFilter.type>, NotEqual<FSLogTypeAction.StaffAssignment>,
                                And<Current<FSLogActionFilter.type>, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>,
                                And<Current<FSLogActionFilter.me>, Equal<False>>>>>))]
        public virtual bool? Selected { get; set; }
        #endregion
    }
}
