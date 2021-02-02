using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [PXTable(IsOptional = true)]
    public class FSxPMTimeActivity : PXCacheExtension<PMTimeActivity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>();
        }

        #region ServiceID
        public abstract class serviceID : PX.Data.BQL.BqlInt.Field<serviceID> { }

        [Service]
        [PXUIField(DisplayName = "Service")]
        public virtual int? ServiceID { get; set; }
        #endregion
        #region AppointmentCustomerID
        public abstract class appointmentCustomerID : PX.Data.BQL.BqlInt.Field<appointmentCustomerID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorCustomer]
        public virtual int? AppointmentCustomerID { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        [PXSelector(typeof(Search<FSAppointment.appointmentID>),
                           SubstituteKey = typeof(FSAppointment.refNbr))]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region LogLineNbr
        public abstract class logLineNbr : PX.Data.BQL.BqlInt.Field<logLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Log Line Ref.", Enabled = false)]
        [PXSelector(
                    typeof(Search<FSAppointmentLog.lineNbr,
                                Where<FSAppointmentLog.docID, Equal<Current<FSxPMTimeActivity.appointmentID>>>>),
                    SubstituteKey = typeof(FSAppointmentLog.lineRef))]
        public virtual int? LogLineNbr { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(Search<FSServiceOrder.sOID>),
                           SubstituteKey = typeof(FSServiceOrder.refNbr))]
        public virtual int? SOID { get; set; }
        #endregion

        #region MemoryHelper
        #region LastBillable
        public abstract class lastBillable : PX.Data.BQL.BqlBool.Field<lastBillable> { }

        [PXBool]
        [PXUIField(Visible = false)]
        public virtual bool? LastBillable { get; set; }
        #endregion
        #endregion
    }
}
