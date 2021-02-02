using System;
using System.Collections.Generic;
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Data;
using PX.Web.UI;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Utilities
{
    public class PriorityIcon<TPriorityId> : BqlFormulaEvaluator<TPriorityId>
        where TPriorityId : IBqlField
    {
        public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> entityTypes)
        {
            var priorityId = (int?) entityTypes[typeof(TPriorityId)];
            var projectManagementClassPriority = ProjectManagementClassPriorityService
                .GetProjectManagementClassPriority(cache.Graph, priorityId);
            return projectManagementClassPriority?.IsHighestPriority == true
                ? Sprite.Control.GetFullUrl(Sprite.Control.PriorityHigh)
                : string.Empty;
        }
    }
}