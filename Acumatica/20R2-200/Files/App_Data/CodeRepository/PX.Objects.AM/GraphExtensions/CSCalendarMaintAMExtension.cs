using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Manufacturing extension on standard Acumatica Work Calendars
    /// </summary>
    public class CSCalendarMaintAMExtension : PXGraphExtension<CSCalendarMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        /// <summary>
        /// Manufacturing break time based on time of day. Supports both APS and non APS implementations
        /// </summary>
        public PXSelect<AMCalendarBreakTime, Where<AMCalendarBreakTime.calendarID, Equal<Current<CSCalendar.calendarID>>>> AMCalendarBreakTimes;

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXReferentialIntegrityCheck2]
        protected virtual void CSCalendar_CalendarID_CacheAttached(PXCache sender)
        {
            // Add support for Referential Integrity check to prevent users from Deleting calendars that are used in Manufacturing
        }
    }
}