using PX.Data;
using PX.Data.BQL;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Weather Filter")]
    public class DailyFieldReportWeatherFilter : IBqlTable
    {
        [Project(typeof(Where<PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>>),
            DisplayName = "Project")]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }
    }
}
