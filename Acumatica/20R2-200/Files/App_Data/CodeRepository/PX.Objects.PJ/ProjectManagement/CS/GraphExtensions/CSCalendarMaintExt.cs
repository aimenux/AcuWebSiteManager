using System;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.PJ.ProjectManagement.CS.GraphExtensions
{
    public class CsCalendarMaintExt : PXGraphExtension<CSCalendarMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected virtual void _(Events.RowDeleting<CSCalendar> args)
        {
            var calendar = args.Row;
            if (IsCalendarUsed(calendar.CalendarID))
            {
                throw new Exception(ProjectManagementMessages.WorkCalendarCannotBeDeleted);
            }
        }

        private bool IsCalendarUsed(string calendarId)
        {
            return new PXSelect<ProjectManagementSetup,
                    Where<ProjectManagementSetup.calendarId,
                        Equal<Required<ProjectManagementSetup.calendarId>>>>(Base)
                .SelectSingle(calendarId) != null;
        }
    }
}