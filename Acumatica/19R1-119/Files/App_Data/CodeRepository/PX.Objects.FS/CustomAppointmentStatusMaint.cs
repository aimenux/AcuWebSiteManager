using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.FS
{
    public class CustomAppointmentStatusMaint : PXGraph<CustomAppointmentStatusMaint, FSCustomAppointmentStatus>
    {
        [PXImport(typeof(FSCustomAppointmentStatus))]
        public PXSelect<FSCustomAppointmentStatus> CustomAppointmentStatusRecords;

        #region PrivateMethods
        public IEnumerable customAppointmentStatusRecords()
        {
            PXSelect<FSCustomAppointmentStatus>.Clear(this);
            return PXSelect<FSCustomAppointmentStatus>.Select(this);
        }
        #endregion
    }
}