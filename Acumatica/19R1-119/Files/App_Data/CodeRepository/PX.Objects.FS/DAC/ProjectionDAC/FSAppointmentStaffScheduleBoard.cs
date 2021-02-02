using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXProjection(typeof(
            Select5<FSAppointmentEmployee,
                        InnerJoin<FSAppointment,
                            On<FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>,
                        InnerJoin<FSServiceOrder,
                            On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                        LeftJoin<Customer,
                            On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
                        LeftJoin<Location,
                            On<Location.locationID, Equal<FSServiceOrder.locationID>>,
                        InnerJoin<FSContact,
                            On<FSContact.contactID, Equal<FSServiceOrder.serviceOrderContactID>>,
                        LeftJoin<FSWFStage,
                            On<FSWFStage.wFStageID, Equal<FSAppointment.wFStageID>>>>>>>>,
                        Aggregate<
                            GroupBy<FSAppointmentEmployee.appointmentID,
                            GroupBy<FSAppointmentEmployee.employeeID,
                            GroupBy<FSAppointment.validatedByDispatcher,
                            GroupBy<FSAppointment.confirmed>>>>>>))]
    #endregion
    public class FSAppointmentStaffScheduleBoard : FSAppointmentScheduleBoard
    {
        public virtual FSAppointmentStaffScheduleBoard Clone()
        {
            return (FSAppointmentStaffScheduleBoard)MemberwiseClone();
        }

        #region EmployeeID
        public new abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentEmployee.employeeID))]
        public override int? EmployeeID { get; set; }
        #endregion
    }
}
