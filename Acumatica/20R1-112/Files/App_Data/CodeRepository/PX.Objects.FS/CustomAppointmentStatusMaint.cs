using PX.Data;
using System.Collections;

namespace PX.Objects.FS
{
    public class CustomAppointmentStatusMaint : PXGraph<CustomAppointmentStatusMaint, FSCustomAppointmentStatus>
    {
        [PXImport(typeof(FSCustomAppointmentStatus))]
        public PXSelect<FSCustomAppointmentStatus> CustomAppointmentStatusRecords;

        #region PrivateMethods
        public virtual IEnumerable customAppointmentStatusRecords()
        {
            PXSelect<FSCustomAppointmentStatus>.Clear(this);
            return PXSelect<FSCustomAppointmentStatus>.Select(this);
        }
        #endregion
    }
}