using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Generic list of selectable time unit types
    /// </summary>
    public class TimeUnits
    {
        /// <summary>
        /// Units in Days
        /// </summary>
        public const int Days = 0;
        /// <summary>
        /// Units in Hours
        /// </summary>
        public const int Hours = 1;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Days => Messages.GetLocal(Messages.Days);
            public static string Hours => Messages.GetLocal(Messages.Hours);
        }

        /// <summary>
        /// Time units for Lead Time
        /// </summary>
        public class LeadTimeListAttribute : PXIntListAttribute
        {
            public LeadTimeListAttribute()
                : base(new int[]
            {
                Days,
                Hours
            }, new string[]
            {
                Messages.Days,
                Messages.Hours
            })
                {
                }
        }

        public class days : PX.Data.BQL.BqlInt.Constant<days>
        {
            public days()
                : base(Days) { }
        }

        public class hours : PX.Data.BQL.BqlInt.Constant<hours>
        {
            public hours()
                : base(Hours) { }
        }

    }
}