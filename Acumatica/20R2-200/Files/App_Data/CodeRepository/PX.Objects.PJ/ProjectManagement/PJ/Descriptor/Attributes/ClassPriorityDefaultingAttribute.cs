using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Data;

namespace PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes
{
    public class ClassPriorityDefaultingAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        private readonly string priorityFieldName;

        public ClassPriorityDefaultingAttribute(string priorityFieldName)
        {
            this.priorityFieldName = priorityFieldName;
        }

        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row != null)
            {
                var classId = cache.GetValue(args.Row, _FieldName) as string;
                var defaultPriorityId = ProjectManagementClassPriorityService
                    .GetDefaultProjectManagementClassPriorityId(cache.Graph, classId);
                cache.SetValue(args.Row, priorityFieldName, defaultPriorityId);
            }
        }
    }
}