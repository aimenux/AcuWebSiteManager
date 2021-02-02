using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using Constants = PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Constants;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    public class ProjectManagementClassPriorityService
    {
        private readonly PXGraph graph;

        public ProjectManagementClassPriorityService(PXGraph graph)
        {
            this.graph = graph;
        }

        public void ValidateSystemPriority(ProjectManagementClassPriority priority)
        {
            if (priority.IsSystemPriority == true)
            {
                throw new Exception(ProjectManagementMessages.SystemRecordCannotBeDeleted);
            }
        }

        public void ValidateUsedPriority(ProjectManagementClassPriority priority)
        {
            if (DoesAnyDocumentExist<RequestForInformation, RequestForInformation.priorityId>(priority.PriorityId)
                || DoesAnyDocumentExist<ProjectIssue, ProjectIssue.priorityId>(priority.PriorityId))
            {
                throw new Exception(ProjectManagementMessages.UsedValueCannotBeDeleted);
            }
        }

        public void ValidateDefaultPrioritySelected(PXCache cache, List<ProjectManagementClassPriority> priorities)
        {
            if (priorities.Any() && priorities.All(p => p.IsDefault != true))
            {
                cache.RaiseException<ProjectManagementClassPriority.isDefault>(priorities.First(),
                    ProjectManagementMessages.NoDefaultPrioritySpecified, null, PXErrorLevel.RowError);
            }
        }

        public void ValidateDefaultPriority(PXCache cache, ProjectManagementClassPriority priority)
        {
            if (priority.IsActive == false && priority.IsDefault == true)
            {
                cache.RaiseException<ProjectManagementClassPriority.isDefault>(priority,
                    ProjectManagementMessages.InactivePriorityCannotBeDefault, null, PXErrorLevel.RowError);
            }
        }

        public void ValidateActivePriority(ProjectManagementClassPriority priority)
        {
            if (priority.IsActive == true)
            {
                throw new Exception(ProjectManagementMessages.ActivePriorityCannotBeDeleted);
            }
        }

        public List<ProjectManagementClassPriority> GetDefaultProjectManagementClassPriorities(string classId)
        {
            return new List<ProjectManagementClassPriority>
            {
                CreateProjectManagementClassPriority(Constants.LowPriority, 1, classId),
                CreateProjectManagementClassPriority(Constants.MediumPriority, 2, classId, true),
                CreateProjectManagementClassPriority(Constants.HighPriority, 3, classId, false, true)
            };
        }

        public bool DoesAnyPriorityExistForClass(string classId)
        {
            return new PXSelect<ProjectManagementClassPriority,
                   Where<ProjectManagementClassPriority.classId,
                       Equal<Required<ProjectManagementClassPriority.classId>>>>(graph)
               .SelectSingle(classId) != null;
        }

        public static ProjectManagementClassPriority GetProjectManagementClassPriority(PXGraph graph, int? priorityId)
        {
            return new PXSelect<ProjectManagementClassPriority,
                   Where<ProjectManagementClassPriority.priorityId,
                       Equal<Required<ProjectManagementClassPriority.priorityId>>>>(graph)
                .SelectSingle(priorityId);
        }

        public static int? GetDefaultProjectManagementClassPriorityId(PXGraph graph, string classId)
        {
            return new PXSelect<ProjectManagementClassPriority,
                    Where<ProjectManagementClassPriority.classId,
                        Equal<Required<ProjectManagementClassPriority.classId>>,
                        And<ProjectManagementClassPriority.isActive, Equal<True>,
                        And<ProjectManagementClassPriority.isDefault, Equal<True>>>>>(graph)
                .SelectSingle(classId)?.PriorityId;
        }

        private static ProjectManagementClassPriority CreateProjectManagementClassPriority(string priorityName,
            int sortOrder, string classId, bool isDefault = false, bool isHighestPriority = false)
        {
            return new ProjectManagementClassPriority
            {
                ClassId = classId,
                IsSystemPriority = true,
                PriorityName = priorityName,
                IsActive = true,
                SortOrder = sortOrder,
                IsDefault = isDefault,
                IsHighestPriority = isHighestPriority
            };
        }

        private bool DoesAnyDocumentExist<TCache, TPriorityId>(int? priorityId)
            where TCache : class, IBqlTable, new()
            where TPriorityId : IBqlField
        {
            return new PXSelect<TCache,
                Where<TPriorityId, Equal<Required<TPriorityId>>>>(graph).Select(priorityId).FirstTableItems.Any();
        }
    }
}