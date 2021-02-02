using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions
{
    [PXHidden]
    public class DailyFieldReportRelatedDocument : PXMappedCacheExtension
    {
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        public virtual string ReferenceNumber
        {
            get;
            set;
        }

        public virtual int? ReferenceId
        {
            get;
            set;
        }

        public virtual int? ProjectId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class referenceNumber : BqlString.Field<referenceNumber>
        {
        }

        public abstract class referenceId : BqlInt.Field<referenceId>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }
    }
}
