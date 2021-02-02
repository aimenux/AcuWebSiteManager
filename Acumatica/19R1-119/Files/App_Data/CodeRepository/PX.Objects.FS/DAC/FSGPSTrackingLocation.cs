using System;
using PX.Data;
using PX.FS;
using PX.SM;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSGPSTrackingLocation : FSGPSTrackingRequest
    {
        #region WeekDay
        public abstract class weekDay : PX.Data.IBqlField
        {
        }

        [PXInt]
        [EP.DayOfWeek]
        [PXDefault((int)System.DayOfWeek.Sunday)]
        [PXUIField(DisplayName = PX.Objects.AR.Messages.DayOfWeek)]
        public virtual int? WeekDay { get; set; }
        #endregion
        #region UserName
        public new abstract class userName : PX.Data.IBqlField
        {
        }

        [PXDBString(128, IsUnicode = true)]
        [PXUIField(DisplayName = "User Name")]
        [PXDBLiteDefault(typeof(Users.username))]
        [PXParent(typeof(Select<Users, Where<Users.username, Equal<Current<FSGPSTrackingLocation.userName>>>>))]
        public override string UserName { get; set; }
        #endregion
        #region TimeZoneID
        public new abstract class timeZoneID : PX.Data.IBqlField
        {
        }
        
        [PXDBString(32)]
        [PXDefault(typeof(Search<UserPreferences.timeZone, Where<UserPreferences.userID, Equal<Current<Users.pKID>>>>))]
        [PXUIField(DisplayName = "Time Zone")]
        public override string TimeZoneID { get; set; }
        #endregion
        #region WeeklyOnDay1
        public new abstract class weeklyOnDay1 : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Sunday")]
        public override bool? WeeklyOnDay1 { get; set; }
        #endregion
        #region WeeklyOnDay2
        public new abstract class weeklyOnDay2 : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Monday")]
        public override bool? WeeklyOnDay2 { get; set; }
        #endregion
        #region WeeklyOnDay3
        public new abstract class weeklyOnDay3 : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tuesday")]
        public override bool? WeeklyOnDay3 { get; set; }
        #endregion
        #region WeeklyOnDay4
        public new abstract class weeklyOnDay4 : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Wednesday")]
        public override bool? WeeklyOnDay4 { get; set; }
        #endregion
        #region WeeklyOnDay5
        public new abstract class weeklyOnDay5 : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Thursday")]
        public override bool? WeeklyOnDay5 { get; set; }
        #endregion
        #region WeeklyOnDay6
        public new abstract class weeklyOnDay6 : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Friday")]
        public override bool? WeeklyOnDay6 { get; set; }
        #endregion
        #region WeeklyOnDay7
        public new abstract class weeklyOnDay7 : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Saturday")]
        public override bool? WeeklyOnDay7 { get; set; }
        #endregion
        #region StartTime
        public new abstract class startTime : PX.Data.IBqlField
        {
        }

        [PX.SM.AUSchedule.AUSeparateTime(typeof(startDate), UseTimeZone = false, DisplayMask = "t")]
        [PXUIField(DisplayName = "Start Time")]
        [PXUIVerify(typeof(Where<startTime, IsNull, Or<endTime, IsNull, Or<startTime, Less<endTime>>>>), PXErrorLevel.Error, TX.Error.START_TIME_GREATER_THAN_END_TIME)]
        public override DateTime? StartTime { get; set; }
        #endregion
        #region EndTime
        public new abstract class endTime : PX.Data.IBqlField
        {
        }

        [PX.SM.AUSchedule.AUSeparateTime(typeof(endDate), UseTimeZone = false, DisplayMask = "t")]
        [PXUIField(DisplayName = "End Time")]
        [PXUIVerify(typeof(Where<startTime, IsNull, Or<endTime, IsNull, Or<endTime, Greater<startTime>>>>), PXErrorLevel.Error, TX.Error.END_TIME_LESSER_THAN_START_TIME)]
        public override DateTime? EndTime { get; set; }
        #endregion
    }
}



