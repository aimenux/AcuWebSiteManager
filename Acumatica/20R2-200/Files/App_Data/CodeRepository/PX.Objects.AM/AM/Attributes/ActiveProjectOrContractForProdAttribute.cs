using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.PM;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing active project attribute for visible in production projects
    /// </summary>
    [PXDBInt]
    [PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
    [PXRestrictor(typeof(Where<PMProject.isCompleted, Equal<False>>), PX.Objects.PM.Messages.CompleteContract, typeof(PMProject.contractCD))]
    [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PX.Objects.PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
    [PXRestrictor(typeof(Where<IsNull<PMProjectExt.visibleInPROD, True>, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
    public class ActiveProjectOrContractForProdAttribute : ActiveProjectOrContractBaseAttribute
    {
        public ActiveProjectOrContractForProdAttribute() : base(null)
        {
            Filterable = true;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Visible = ProjectHelper.IsPMVisible(sender.Graph);
        }
    }
}
