using System;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes
{
    public class ProjectTaskTypeValidationAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
    {
        private IProjectTaskDataProvider projectTaskDataProvider;

        public Type ProjectTaskIdField
        {
            get;
            set;
        }

        public string WrongProjectTaskType
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
        {
            if (cache.GetValue(args.Row, _FieldName) is int projectTaskId)
            {
                ValidateProjectTaskType(cache, args.Row, projectTaskId);
            }
        }

        private void ValidateProjectTaskType(PXCache cache, object row, int? projectTaskId)
        {
            projectTaskDataProvider = cache.Graph.GetService<IProjectTaskDataProvider>();
            var projectTask = projectTaskDataProvider.GetProjectTask(cache.Graph, projectTaskId);
            if (projectTask != null)
            {
                if (projectTask.Type == WrongProjectTaskType)
                {
                    cache.RaiseException(_FieldName, row, Message, projectTask.TaskCD);
                }
            }
        }
    }
}