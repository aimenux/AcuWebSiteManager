using Autofac;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services;
using PX.Objects.PJ.DailyFieldReports.SM.Services;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.PJ.DailyFieldReports.PJ.Services;

namespace PX.Objects.PJ.Common.DependencyInjection
{
    public class ServiceRegistration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<DrawingLogDataProvider>().As<IDrawingLogDataProvider>();
            builder.RegisterType<ProjectManagementClassUsageService>().As<IProjectManagementClassUsageService>();
            builder.RegisterType<ProjectManagementClassDataProvider>().As<IProjectManagementClassDataProvider>();
            builder.RegisterType<ProjectManagementImpactService>().As<IProjectManagementImpactService>();
            builder.RegisterType<NumberingSequenceUsage>().As<INumberingSequenceUsage>();
            builder.RegisterType<BusinessAccountDataProvider>().As<IBusinessAccountDataProvider>();
            builder.RegisterType<ProjectDataProvider>().As<IProjectDataProvider>();
            builder.RegisterType<PhotoLogDataProvider>().As<IPhotoLogDataProvider>();
            builder.RegisterType<PhotoConfirmationService>().As<IPhotoConfirmationService>();
            builder.RegisterType<FilesDataProvider>().As<IFilesDataProvider>();
            builder.RegisterType<WeatherIntegrationService>().As<IWeatherIntegrationService>();
            builder.RegisterType<WeatherIntegrationUnitOfMeasureService>()
                .As<IWeatherIntegrationUnitOfMeasureService>();
        }
    }
}