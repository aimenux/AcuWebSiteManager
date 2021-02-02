using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class OccupationalGeoZoneHelper : IBqlTable
    {
        #region GeoZoneID
        public abstract class geoZoneID : PX.Data.BQL.BqlInt.Field<geoZoneID> { }

        [PXInt(IsKey = true)]
        [PXUIField]
        public virtual int? GeoZoneID { get; set; }
        #endregion

        #region GeoZoneCD
        public abstract class geoZoneCD : PX.Data.BQL.BqlString.Field<geoZoneCD> { }

        [PXString]
        [PXUIField(DisplayName = "Service Area ID")]
        public virtual string GeoZoneCD { get; set; }
        #endregion

        #region ScheduledHours
        public abstract class scheduledHours : PX.Data.BQL.BqlDecimal.Field<scheduledHours> { }

        [PXDecimal(2)]
        [PXUIField(DisplayName = "Scheduled Hours", Enabled = false)]
        public virtual decimal? ScheduledHours { get; set; }
        #endregion
        #region AppointmentHours
        public abstract class appointmentHours : PX.Data.BQL.BqlDecimal.Field<appointmentHours> { }

        [PXDecimal(2)]
        [PXUIField(DisplayName = "Appointment Hours", Enabled = false)]
        public virtual decimal? AppointmentHours { get; set; }
        #endregion
        #region IdleRate
        public abstract class idleRate : PX.Data.BQL.BqlDecimal.Field<idleRate> { }

        [PXDecimal(2)]
        [PXUIField(DisplayName = "Idle Rate (%)", Enabled = false)]
        public virtual decimal? IdleRate { get; set; }
        #endregion
        #region OccupationalRate
        public abstract class occupationalRate : PX.Data.BQL.BqlDecimal.Field<occupationalRate> { }

        [PXDecimal(2)]
        [PXUIField(DisplayName = "Occupational Rate (%)", Enabled = false)]
        public virtual decimal? OccupationalRate { get; set; }
        #endregion
    }    
}