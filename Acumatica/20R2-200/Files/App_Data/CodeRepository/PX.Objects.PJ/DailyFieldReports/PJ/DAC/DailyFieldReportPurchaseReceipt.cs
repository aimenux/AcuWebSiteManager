using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PO;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXHidden]
    [PXCacheName("Daily Field Report Purchase Receipt")]
    public class DailyFieldReportPurchaseReceipt : IBqlTable
    {
        [PXDBIdentity]
        public virtual int? DailyFieldReportPurchaseReceiptId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBString(15, IsKey = true, IsUnicode = true)]
        [PurchaseReceiptSelector]
        [PXParent(typeof(SelectFrom<POReceipt>
            .Where<POReceipt.receiptNbr.IsEqual<purchaseReceiptId>>))]
        [PXUIField(DisplayName = "Reference Nbr.", Required = true)]
        public virtual string PurchaseReceiptId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportPurchaseReceiptId : BqlInt.Field<dailyFieldReportPurchaseReceiptId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class purchaseReceiptId : BqlString.Field<purchaseReceiptId>
        {
        }
    }
}