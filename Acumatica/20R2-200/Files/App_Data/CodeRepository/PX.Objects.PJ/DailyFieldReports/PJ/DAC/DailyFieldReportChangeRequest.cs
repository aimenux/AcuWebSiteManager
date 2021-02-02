using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Change Request")]
    public class DailyFieldReportChangeRequest : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportChangeRequestId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBString(10, IsUnicode = true)]
        [ChangeRequestSelector]
        [PXParent(typeof(SelectFrom<PMChangeRequest>
            .Where<PMChangeRequest.refNbr.IsEqual<changeRequestId>>))]
        [PXUIField(DisplayName = "Reference Nbr.", Required = true)]
        public virtual string ChangeRequestId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportChangeRequestId : BqlInt.Field<dailyFieldReportChangeRequestId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class changeRequestId : BqlString.Field<changeRequestId>
        {
        }
    }
}