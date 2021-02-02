using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Scheduling method parameters for controlling of scheduling start/end dates
    /// </summary>
    public class ScheduleMethod
    {
        /// <summary>
        /// Finish On = "F"
        ///     Backward schedule of an order
        /// </summary>
        public const string FinishOn = "F";
        /// <summary>
        /// Start On = "S"
        ///     Forward schedule of an order
        /// </summary>
        public const string StartOn = "S"; 
        /// <summary>
        /// User Dates = "U"
        ///     Allows users to select specific start/end dates
        /// </summary>
        public const string UserDates = "U";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string FinishOn => Messages.GetLocal(Messages.FinishOn);
            public static string StartOn => Messages.GetLocal(Messages.StartOn);
            public static string UserDates => Messages.GetLocal(Messages.UserDates);
        }

        public class finishOn : PX.Data.BQL.BqlString.Constant<finishOn>
        {
            public finishOn() : base(FinishOn) { }
        }
        public class startOn : PX.Data.BQL.BqlString.Constant<startOn>
        {
            public startOn() : base(StartOn) { }
        }
        public class userDates : PX.Data.BQL.BqlString.Constant<userDates>
        {
            public userDates() : base(UserDates) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { 
                        StartOn, 
                        FinishOn, 
                        UserDates },
                    new string[] { 
                        Messages.StartOn,
                        Messages.FinishOn,
                        Messages.UserDates }) { }
        }
    }
}