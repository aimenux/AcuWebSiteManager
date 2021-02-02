using System;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Helpers;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
    /// <summary>
    /// Attribute used for resetting response due date on <see cref="RequestForInformation"/>
    /// and <see cref="ProjectIssue"/> classes.
    /// Referenced <see cref="ProjectManagementClass.ProjectManagementClassId"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ResetResponseDueDateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        private readonly Type dueDateFieldType;
        private PXGraph graph;

        public ResetResponseDueDateAttribute(Type dueDateFieldType)
        {
            this.dueDateFieldType = dueDateFieldType;
        }

        public override void CacheAttached(PXCache cache)
        {
            graph = cache.Graph;
        }

        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            var dueDateFieldName = cache.GetField(dueDateFieldType);
            var dueDate = cache.GetValue(args.Row, dueDateFieldName);
            if (dueDate == null)
            {
                var defaultResponseTimeFrame = GetDefaultResponseTimeFrame(args.Row, cache);
                if (defaultResponseTimeFrame.HasValue && graph.Accessinfo.BusinessDate.HasValue)
                {
                    var dueResponseDate = GetDueResponseDate(defaultResponseTimeFrame.Value,
                        graph.Accessinfo.BusinessDate.Value);
                    cache.SetValueExt(args.Row, dueDateFieldName, dueResponseDate);
                }
            }
        }

        protected virtual int? GetDefaultResponseTimeFrame(object row, PXCache cache)
        {
            var projectManagementClass = GetProjectManagementClass(cache, row);
            if (projectManagementClass == null)
            {
                return null;
            }
            switch (row)
            {
                case RequestForInformation _:
                    return projectManagementClass.RequestForInformationResponseTimeFrame;
                case ProjectIssue _:
                    return projectManagementClass.ProjectIssueResponseTimeFrame;
                default:
                    return null;
            }
        }

        protected ProjectManagementClass GetProjectManagementClass(PXCache cache, object row)
        {
            var projectManagementClassDataProvider = cache.Graph.GetService<IProjectManagementClassDataProvider>();
            var classId = (string) cache.GetValue(row, FieldName);
            return projectManagementClassDataProvider.GetProjectManagementClass(classId);
        }

        private DateTime? GetDueResponseDate(int defaultResponseTimeFrame, DateTime businessDate)
        {
            var setup = (ProjectManagementSetup) graph.Caches[typeof(ProjectManagementSetup)].Current;
            return setup.AnswerDaysCalculationType == AnswerDaysCalculationTypeAttribute.SequentialDays
                ? businessDate.AddDays(defaultResponseTimeFrame)
                : DateTimeHelper.CalculateBusinessDate(businessDate, defaultResponseTimeFrame, setup.CalendarId);
        }
    }
}
