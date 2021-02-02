using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.PO;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public sealed class PurchaseReceiptSelectorAttribute : PXCustomSelectorAttribute
    {
        public PurchaseReceiptSelectorAttribute()
            : base(typeof(POReceipt.receiptNbr),
                typeof(POReceipt.receiptNbr),
                typeof(POReceipt.status),
                typeof(POReceipt.receiptType),
                typeof(POReceipt.vendorID),
                typeof(POReceipt.vendorLocationID),
                typeof(POReceipt.receiptDate),
                typeof(POReceipt.orderQty))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedPurchaseReceiptNumbers = _Graph.GetExtension<DailyFieldReportEntryPurchaseReceiptExtension>()
                .PurchaseReceipts.SelectMain().Select(pr => pr.PurchaseReceiptId);
            return GetPurchaseReceipts().Where(pr => pr.ReceiptNbr.IsNotIn(linkedPurchaseReceiptNumbers));
        }

        private IEnumerable<POReceipt> GetPurchaseReceipts()
        {
            return SelectFrom<POReceipt>
                .LeftJoin<POReceiptLine>
                    .On<POReceipt.receiptNbr.IsEqual<POReceiptLine.receiptNbr>>
                .Where<APSetup.requireSingleProjectPerDocument.FromCurrent.IsEqual<True>
                        .And<POReceipt.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>>
                    .Or<APSetup.requireSingleProjectPerDocument.FromCurrent.IsEqual<False>>
                        .And<POReceiptLine.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>>>
                .AggregateTo<GroupBy<POReceipt.receiptNbr>>
                .View.Select(_Graph).FirstTableItems;
        }
    }
}
