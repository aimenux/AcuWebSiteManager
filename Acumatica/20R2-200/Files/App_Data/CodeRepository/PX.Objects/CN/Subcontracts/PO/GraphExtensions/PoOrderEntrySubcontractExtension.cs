using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PO.GraphExtensions
{
    public class PoOrderEntrySubcontractExtension : PXGraphExtension<POOrderEntryExt, POOrderEntry>
    {
        public override void Initialize()
        {
            SetChangeOrdersAvailability();
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected virtual void _(Events.FieldVerifying<POOrder, POOrder.projectID> args, PXFieldVerifying baseMethod)
        {
            if (IsSubcontractScreen())
            {
                var projectId = (int?) args?.NewValue;
                if (NeedToVerifyProjectId(projectId))
                {
                    VerifyProjectLockCommitments(projectId);
                }
            }
            else
            {
                baseMethod(args.Cache, args.Args);
            }
        }

        private bool NeedToVerifyProjectId(int? projectId)
        {
            return projectId != null && PXAccess.FeatureInstalled<FeaturesSet.changeOrder>() &&
                !Base1.SkipProjectLockCommitmentsVerification;
        }

        private void VerifyProjectLockCommitments(int? projectId)
        {
            var project = GetProject(projectId);
            if (project?.LockCommitments == true)
            {
                throw new PXSetPropertyException(ScMessages.ProjectCommitmentsLocked)
                {
                    ErrorValue = project.ContractCD
                };
            }
        }

        private PMProject GetProject(int? projectId)
        {
            ProjectDefaultAttribute.IsProject(Base, projectId, out var project);
            return project;
        }

        private void SetChangeOrdersAvailability()
        {
            Base1.ChangeOrders.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.changeOrder>();
        }

        private bool IsSubcontractScreen()
        {
            return Base.GetType() == typeof(SubcontractEntry) || Base.GetType().BaseType == typeof(SubcontractEntry);
        }
    }
}
