using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Project Management Preferences - Weather Service Integration Settings")]
    [PXPrimaryGraph(typeof(ProjectManagementSetupMaint))]
	public class WeatherIntegrationSetup : BaseCache, IBqlTable
    {
        [PXDBBool]
        [PXUIField(DisplayName = "Enable Weather Service Integration for Daily Field Reports")]
        [PXDefault(false)]
        public bool? IsConfigurationEnabled
        {
            get;
            set;
        }

        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Weather API Service", Enabled = false)]
        [WeatherApiService.List]
        public string WeatherApiService
        {
            get;
            set;
        }

        [PXRSACryptString(255)]
        [PXUIField(DisplayName = "Weather API Key", Enabled = false)]
        public string WeatherApiKey
        {
            get;
            set;
        }

        [PXDBString(25, IsUnicode = true)]
        [PXUIField(DisplayName = "Request By", Enabled = false)]
        [WeatherRequestParameters.List]
        [PXUIEnabled(typeof(weatherApiService.IsNotNull))]
        public string RequestParametersType
        {
            get;
            set;
        }

        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "UOM Format", Enabled = false)]
        [PXDefault(Descriptor.Attributes.UnitOfMeasureFormat.Imperial)]
        [UnitOfMeasureFormat.List]
        public string UnitOfMeasureFormat
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Enable Weather Processing Log")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? IsWeatherProcessingLogEnabled
        {
            get;
            set;
        }

        [PXDBInt(MinValue = 1)]
        [PXUIField(DisplayName = "Weather Processing Log Term (Days)")]
        [PXUIEnabled(typeof(isWeatherProcessingLogEnabled.IsEqual<True>))]
        public int? WeatherProcessingLogTerm
        {
            get;
            set;
        }

        public abstract class isConfigurationEnabled : BqlBool.Field<isConfigurationEnabled>
        {
        }

        public abstract class weatherApiService : BqlString.Field<weatherApiService>
        {
        }

        public abstract class weatherApiKey : BqlString.Field<weatherApiKey>
        {
        }

        public abstract class requestParametersType : BqlString.Field<requestParametersType>
        {
        }

        public abstract class unitOfMeasureFormat : BqlString.Field<unitOfMeasureFormat>
        {
        }

        public abstract class isWeatherProcessingLogEnabled : BqlBool.Field<isWeatherProcessingLogEnabled>
        {
        }

        public abstract class weatherProcessingLogTerm : BqlInt.Field<weatherProcessingLogTerm>
        {
        }

        public abstract class tstamp : BqlByteArray.Field<tstamp>
        {
        }

        public abstract class createdById : BqlGuid.Field<createdById>
        {
        }

        public abstract class createdByScreenId : BqlString.Field<createdByScreenId>
        {
        }

        public abstract class createdDateTime : BqlDateTime.Field<createdDateTime>
        {
        }

        public abstract class lastModifiedById : BqlGuid.Field<lastModifiedById>
        {
        }

        public abstract class lastModifiedByScreenId : BqlString.Field<lastModifiedByScreenId>
        {
        }

        public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }
    }
}
