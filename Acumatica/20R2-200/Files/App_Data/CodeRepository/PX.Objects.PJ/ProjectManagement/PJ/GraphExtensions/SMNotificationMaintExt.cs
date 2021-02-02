using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.ProjectManagement.PJ.GraphExtensions
{
    public class SmNotificationMaintExt : PXGraphExtension<SMNotificationMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public void _(Events.RowPersisting<Notification> args)
        {
            var notificationTemplate = args.Row;
            if (notificationTemplate != null && args.Operation == PXDBOperation.Delete)
            {
                UpdateProjectManagementSetupIfNeeded(notificationTemplate.NotificationID);
            }
        }

        private void UpdateProjectManagementSetupIfNeeded(int? notificationId)
        {
            var projectManagementSetup = new PXSelect<ProjectManagementSetup>(Base).SelectSingle();
            if (projectManagementSetup.DefaultEmailNotification == notificationId)
            {
                UpdateProjectManagementSetup(projectManagementSetup);
            }
        }

        private void UpdateProjectManagementSetup(ProjectManagementSetup projectManagementSetup)
        {
            projectManagementSetup.DefaultEmailNotification = null;
            Base.Caches<ProjectManagementSetup>().Update(projectManagementSetup);
            Base.Caches<ProjectManagementSetup>().PersistUpdated(projectManagementSetup);
        }
    }
}