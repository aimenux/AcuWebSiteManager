using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class ServiceSelectionFilter : IBqlTable
    {
        #region ServiceClassID
        public abstract class serviceClassID : PX.Data.BQL.BqlInt.Field<serviceClassID> { }

        [PXInt]
        [PXUIField(DisplayName = "Service Class ID")]
        [PXSelector(typeof(Search<INItemClass.itemClassID,
                        Where<INItemClass.itemType, Equal<INItemTypes.serviceItem>>>),
            SubstituteKey = typeof(INItemClass.itemClassCD))]
        public virtual int? ServiceClassID { get; set; }
        #endregion
        #region ScheduledDateTimeBegin
        public abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Scheduled Date")]
        public virtual DateTime? ScheduledDateTimeBegin { get; set; }
        #endregion
    }

    [System.SerializableAttribute]
    public class EmployeeGridFilter : PX.Data.IBqlTable
    {
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXInt(IsKey = true)]
        [FSSelector_StaffMember_All]
        [PXUIField(DisplayName = "Staff Member ID", Enabled = false)]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region Mem_Selected
        public abstract class mem_Selected : PX.Data.BQL.BqlBool.Field<mem_Selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Mem_Selected { get; set; }
        #endregion

        public override int GetHashCode()
        {
            if (EmployeeID != null)
            {
                return (int)EmployeeID;
            }

            return -1;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EmployeeGridFilter);
        }

        public bool Equals(EmployeeGridFilter p)
        {
            return EmployeeID == p.EmployeeID;
        }
    }
}
