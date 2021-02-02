using System;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions
{
    [PXHidden]
    public class DailyFieldReportRelation : PXMappedCacheExtension
    {
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        public virtual int? RelationId
        {
            get;
            set;
        }

        public virtual string RelationNumber
        {
            get;
            set;
        }

        public virtual Guid? RelationNoteId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class relationId : BqlInt.Field<relationId>
        {
        }

        public abstract class relationNumber : BqlString.Field<relationNumber>
        {
        }

        public abstract class relationNoteId : BqlGuid.Field<relationNoteId>
        {
        }
    }
}
