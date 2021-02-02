using System;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.Common.Mappings
{
    public class DailyFieldReportRelationMapping : IBqlMapping
    {
        public Type DailyFieldReportId = typeof(DailyFieldReportRelation.dailyFieldReportId);

        public Type RelationId = typeof(DailyFieldReportRelation.relationId);

        public Type RelationNumber = typeof(DailyFieldReportRelation.relationNumber);

        public Type RelationNoteId = typeof(DailyFieldReportRelation.relationNoteId);

        public DailyFieldReportRelationMapping(Type table)
        {
            Table = table;
        }

        public Type Table
        {
            get;
            set;
        }

        public Type Extension => typeof(DailyFieldReportRelation);
    }
}
