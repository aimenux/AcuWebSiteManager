using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Photo Log")]
    public class DailyFieldReportPhotoLog : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public int? DailyFieldReportPhotoLogId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBInt]
        [PXSelector(typeof(SearchFor<PhotoLog.photoLogId>
                .Where<PhotoLog.photoLogId
                    .IsNotInSubselect<SearchFor<photoLogId>
                        .Where<dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>>
                    .And<PhotoLog.projectId.IsEqual<DailyFieldReport.projectId.FromCurrent>>
                    .Or<PhotoLog.dailyFieldReportId.FromCurrent.IsNotNull>>),
            SubstituteKey = typeof(PhotoLog.photoLogCd))]
        [PXParent(typeof(SelectFrom<PhotoLog>
            .Where<PhotoLog.photoLogId.IsEqual<photoLogId>>))]
        [PXUIField(DisplayName = "Photo Log ID", Required = true)]
        public int? PhotoLogId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportPhotoLogId : BqlInt.Field<dailyFieldReportPhotoLogId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class photoLogId : BqlInt.Field<photoLogId>
        {
        }
    }
}
