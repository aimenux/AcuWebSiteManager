using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    [PXTable(IsOptional = true)]
    public class FSxAPTran : PXCacheExtension<APTran>, IPostDocLineExtension
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Source
        public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(Enabled = false)]
        public virtual string Source { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<FSServiceOrder.sOID>), SubstituteKey = typeof(FSServiceOrder.refNbr))]
        [PXDBInt]
        public virtual int? SOID { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXUIField(DisplayName = "Appointment Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<FSAppointment.appointmentID>), SubstituteKey = typeof(FSAppointment.refNbr))]
        [PXDBInt]        
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region AppDetID
        public abstract class appDetID : PX.Data.BQL.BqlInt.Field<appDetID> { }

        [PXDBInt]
        public virtual int? AppDetID { get; set; }
        #endregion
        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt]
        public virtual int? SODetID { get; set; }
        #endregion
        #region AppointmentDate
        public abstract class appointmentDate : PX.Data.BQL.BqlDateTime.Field<appointmentDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Service Appointment Date", Enabled = false)]
        public virtual DateTime? AppointmentDate { get; set; }
        #endregion
        #region ServiceOrderDate
        public abstract class serviceOrderDate : PX.Data.BQL.BqlDateTime.Field<serviceOrderDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Service Order Date", Enabled = false)]
        public virtual DateTime? ServiceOrderDate { get; set; }
        #endregion

        // TODO: Rename this to CustomerID
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorCustomer]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSxAPTran.billCustomerID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Location ID", Enabled = false, DirtyRead = true)]
        public virtual int? CustomerLocationID { get; set; }
        #endregion

        #region Mem_PreviousPostID
        public abstract class mem_PreviousPostID : PX.Data.BQL.BqlInt.Field<mem_PreviousPostID> { }

        [PXInt]
        public virtual int? Mem_PreviousPostID { get; set; }
        #endregion
        #region Mem_TableSource
        public abstract class mem_TableSource : PX.Data.BQL.BqlString.Field<mem_TableSource> { }

        [PXString]
        public virtual string Mem_TableSource { get; set; }
        #endregion
    }
}
