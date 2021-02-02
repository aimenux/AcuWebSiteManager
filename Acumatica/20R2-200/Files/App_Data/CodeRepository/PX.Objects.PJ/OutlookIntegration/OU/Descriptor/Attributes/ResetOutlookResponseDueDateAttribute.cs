using System;
using PX.Objects.PJ.OutlookIntegration.OU.DAC;
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.OutlookIntegration.OU.Descriptor.Attributes
{
    /// <summary>
    /// Attribute used for resetting response due date on <see cref="RequestForInformationOutlook"/>
    /// and <see cref="ProjectIssueOutlook"/> classes.
    /// Referenced <see cref="ProjectManagementClass.ProjectManagementClassId"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ResetOutlookResponseDueDateAttribute : ResetResponseDueDateAttribute
    {
        public ResetOutlookResponseDueDateAttribute(Type dueDateFieldType)
            : base(dueDateFieldType)
        {
        }

        protected override int? GetDefaultResponseTimeFrame(object row, PXCache cache)
        {
            var projectManagementClass = GetProjectManagementClass(cache, row);
            if (projectManagementClass == null)
            {
                return null;
            }
            switch (row)
            {
                case RequestForInformationOutlook _:
                    return projectManagementClass.RequestForInformationResponseTimeFrame;
                case ProjectIssueOutlook _:
                    return projectManagementClass.ProjectIssueResponseTimeFrame;
                default:
                    return null;
            }
        }
    }
}
