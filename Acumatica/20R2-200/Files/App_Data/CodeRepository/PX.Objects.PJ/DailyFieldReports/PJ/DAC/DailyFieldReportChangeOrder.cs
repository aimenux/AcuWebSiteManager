using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Change Order")]
    public class DailyFieldReportChangeOrder : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportChangeOrderId
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
        [PXDBString(15, IsUnicode = true)]
        [ChangeOrderSelector]
        [PXParent(typeof(SelectFrom<PMChangeOrder>
            .Where<PMChangeOrder.refNbr.IsEqual<changeOrderId>>))]
        [PXUIField(DisplayName = "Reference Nbr.", Required = true)]
        public virtual string ChangeOrderId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportChangeOrderId : BqlInt.Field<dailyFieldReportChangeOrderId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class changeOrderId : BqlString.Field<changeOrderId>
        {
        }
    }
}