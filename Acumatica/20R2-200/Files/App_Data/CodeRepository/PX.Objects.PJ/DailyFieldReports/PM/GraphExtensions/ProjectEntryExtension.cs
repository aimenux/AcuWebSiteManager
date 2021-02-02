using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions;
using PX.Objects.PJ.DailyFieldReports.PM.Services;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PM.GraphExtensions
{
    public class ProjectEntryExtension : PXGraphExtension<ProjectEntry>
    {
        public SelectFrom<WeatherIntegrationSetup>.View WeatherIntegrationSetup;

        
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

       

        public virtual void _(Events.RowSelected<PMProject> args)
        {
            var weatherIntegrationSetup = WeatherIntegrationSetup.SelectSingle();
            if (weatherIntegrationSetup != null)
            {
                WeatherIntegrationSetup.Current = weatherIntegrationSetup;
            }
        }

        
    }
}