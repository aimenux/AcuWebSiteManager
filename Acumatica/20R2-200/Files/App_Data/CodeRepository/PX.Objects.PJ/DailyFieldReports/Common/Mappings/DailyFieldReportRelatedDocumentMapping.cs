using System;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.Common.Mappings
{
    public class DailyFieldReportRelatedDocumentMapping : IBqlMapping
    {
        public Type DailyFieldReportId = typeof(DailyFieldReportRelatedDocument.dailyFieldReportId);

        public Type ReferenceNumber = typeof(DailyFieldReportRelatedDocument.referenceNumber);

        public Type ReferenceId = typeof(DailyFieldReportRelatedDocument.referenceId);

        public Type ProjectId = typeof(DailyFieldReportRelatedDocument.projectId);

        public DailyFieldReportRelatedDocumentMapping(Type table)
        {
            Table = table;
        }

        public Type Table
        {
            get;
            set;
        }

        public Type Extension => typeof(DailyFieldReportRelatedDocument);
    }
}
