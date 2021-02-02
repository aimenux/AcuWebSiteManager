using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.PJ.OutlookIntegration.OU.GraphExtensions
{
    public class OuSearchMaintHideProjectManagementViewsExtension :
        PXGraphExtension<OuSearchMaintExtensionBase, OUSearchMaint>
    {
        public static bool IsActive()
        {
            return !PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public override void Initialize()
        {
            PXUIFieldAttribute.SetVisible(Base1.RequestForInformationOutlook.Cache, null, false);
            PXUIFieldAttribute.SetVisible(Base1.ProjectIssueOutlook.Cache, null, false);
        }

        protected virtual void _(Events.RowSelected<OUSearchEntity> args, PXRowSelected baseHandler)
        {
            baseHandler(args.Cache, args.Args);
            HideViewEntityActionIfRequired();
        }

        private void HideViewEntityActionIfRequired()
        {
            if (Base.SourceMessage.Current.MessageId == null)
            {
                return;
            }
            var helper = new EntityHelper(Base);
            var email = Base.Message.SelectSingle();
            var entityType = helper.GetEntityRowType(email?.RefNoteID);
            if (entityType != null && entityType.IsIn(typeof(ProjectIssue), typeof(RequestForInformation)))
            {
                Base.ViewEntity.SetVisible(false);
            }
        }
    }
}
