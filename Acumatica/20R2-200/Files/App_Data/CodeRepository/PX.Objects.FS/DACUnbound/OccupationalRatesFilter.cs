using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class OccupationalRatesFilter : IBqlTable
    {
        #region PeriodType
        public abstract class periodType : ListField_Period_Appointment
        {
        }

        [PXString(1, IsFixed = true)]
        [PXDefault(ID.PeriodType.WEEK)]
        [PXUIField(DisplayName = "Period")]
        [periodType.ListAtrribute]
        public virtual string PeriodType { get; set; }
        #endregion
        #region DateInRange
        public abstract class dateInRange : PX.Data.BQL.BqlDateTime.Field<dateInRange> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Date in Range")]
        public virtual DateTime? DateInRange { get; set; }
        #endregion
        #region DateBegin
        public abstract class dateBegin : PX.Data.BQL.BqlDateTime.Field<dateBegin> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Begin Date", Enabled = false, Visible = true)]
        public virtual DateTime? DateBegin { get; set; }
        #endregion
        #region DateEnd
        public abstract class dateEnd : PX.Data.BQL.BqlDateTime.Field<dateEnd> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "End Date", Enabled = false, Visible = true)]
        public virtual DateTime? DateEnd { get; set; }
        #endregion
    }
}
