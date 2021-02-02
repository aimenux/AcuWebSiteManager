using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class ServiceContractsByRouteFilter : IBqlTable
    {        
        #region RouteID
        public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

        [PXInt]
        [PXDefault]
        [PXUIField(DisplayName = "Route")]
        [FSSelectorRouteID]
        public virtual int? RouteID { get; set; }
        #endregion
        #region WeekDay
        public abstract class weekDay : ListField_WeekDays
        {
        }

        [PXString(2, IsFixed = true)]
        [PXDefault(ID.WeekDays.ANYDAY)]
        [PXUIField(DisplayName = "Weekday")]
        [weekDay.ListAtrribute]
        public virtual string WeekDay { get; set; }
        #endregion
        #region ServiceContractFlag
        public abstract class serviceContractFlag : PX.Data.BQL.BqlBool.Field<serviceContractFlag> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Display active Service Contracts only")]
        public virtual bool? ServiceContractFlag { get; set; }
        #endregion
        #region ScheduleFlag
        public abstract class scheduleFlag : PX.Data.BQL.BqlBool.Field<scheduleFlag> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Display active Schedules only")]
        public virtual bool? ScheduleFlag { get; set; }
        #endregion
    }
}
