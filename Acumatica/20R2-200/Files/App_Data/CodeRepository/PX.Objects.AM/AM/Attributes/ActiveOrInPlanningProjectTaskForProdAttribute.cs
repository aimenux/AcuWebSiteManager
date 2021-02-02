using PX.Data;
using PX.Objects.PM;
using System;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM.Attributes
{
    [PXRestrictor(typeof(Where<PMTask.isCancelled, Equal<False>>), PX.Objects.PM.Messages.ProjectTaskIsCanceled, typeof(PMTask.taskCD))]
    [PXRestrictor(typeof(Where<PMTask.isCompleted, Equal<False>>), PX.Objects.PM.Messages.ProjectTaskIsCompleted, typeof(PMTask.taskCD))]
    [PXRestrictor(typeof(Where<PMTaskExt.visibleInPROD, Equal<True>>), PX.Objects.PM.Messages.ProjectTaskAttributeNotSupport, typeof(PMTask.taskCD))]
    public class ActiveOrInPlanningProjectTaskForProdAttribute : ActiveOrInPlanningProjectTaskAttribute
    {
        public ActiveOrInPlanningProjectTaskForProdAttribute(Type projectID) : base(projectID)
        {
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var graph = sender.Graph;
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            Visible = ProjectHelper.IsPMVisible(sender.Graph);
        }
    }
}
