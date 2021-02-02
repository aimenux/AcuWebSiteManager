using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Data;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public class ActiveOrPlanningProjectAttribute : ProjectAttribute
    {
        public ActiveOrPlanningProjectAttribute()
            : base(typeof(Where<PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>>))
        {
            DisplayName = DrawingLogLabels.Project;
            Visibility = PXUIVisibility.SelectorVisible;
            _Attributes.Add(new PXRestrictorAttribute(
                typeof(Where<PMProject.status.IsIn<ProjectStatus.active, ProjectStatusExtension.planning>>),
                DrawingLogMessages.OnlyProjectsInStatusesAreAllowed));
        }
    }
}
