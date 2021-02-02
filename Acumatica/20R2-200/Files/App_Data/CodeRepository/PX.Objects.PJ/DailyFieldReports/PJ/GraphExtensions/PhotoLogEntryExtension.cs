using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class PhotoLogEntryExtension : CreatedFromDailyFieldReportExtension<PhotoLogEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportPhotoLog>
            .Where<DailyFieldReportPhotoLog.photoLogId.IsEqual<PhotoLog.photoLogId.FromCurrent>>
            .View DailyFieldReportPhotoLogs;

        protected override string EntityName => DailyFieldReportEntityNames.PhotoLog;

        protected override DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping()
        {
            return new DailyFieldReportRelatedDocumentMapping(typeof(PhotoLog))
            {
                ReferenceId = typeof(PhotoLog.photoLogId)
            };
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportPhotoLog))
            {
                RelationId = typeof(DailyFieldReportPhotoLog.photoLogId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportPhotoLogs);
        }

        protected override DailyFieldReportRelation CreateDailyFieldReportRelation(int? dailyFieldReportId)
        {
            return new DailyFieldReportRelation
            {
                RelationId = Documents.Current.ReferenceId,
                DailyFieldReportId = dailyFieldReportId
            };
        }
    }
}
