using System;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Weather Processing Log Filter")]
    public class WeatherProcessingLogFilter : IBqlTable
    {
        [Project(typeof(Where<PMProject.nonProject.IsEqual<False>
            .And<PMProject.baseType.IsEqual<CTPRType.project>>>), DisplayName = "Project")]
        public int? ProjectId
        {
            get;
            set;
        }

        [PXString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Weather API Service")]
        [WeatherApiService.List]
        public string WeatherApiService
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Request Date From")]
        public DateTime? RequestDateFrom
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Request Date To")]
        public DateTime? RequestDateTo
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Show Errors Only")]
        public bool? IsShowErrorsOnly
        {
            get;
            set;
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class weatherApiService : BqlString.Field<weatherApiService>
        {
        }

        public abstract class requestDateFrom : BqlDateTime.Field<requestDateFrom>
        {
        }

        public abstract class requestDateTo : BqlDateTime.Field<requestDateTo>
        {
        }

        public abstract class isShowErrorsOnly : BqlBool.Field<isShowErrorsOnly>
        {
        }
    }
}