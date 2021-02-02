using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.PO;
using DailyFieldReport = PX.Objects.PJ.DailyFieldReports.PJ.DAC.DailyFieldReport;

namespace PX.Objects.PJ.DailyFieldReports.PO.GraphExtensions
{
    public class PoReceiptEntryExtension : CreatedFromDailyFieldReportExtension<POReceiptEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportPurchaseReceipt>
            .Where<DailyFieldReportPurchaseReceipt.purchaseReceiptId.IsEqual<POReceipt.receiptNbr>>
            .View DailyFieldReportPurchaseReceipts;

        protected override string EntityName => DailyFieldReportEntityNames.PurchaseReceipt;

        public static bool IsActive()
        {
            return false;
        }

        public virtual void _(Events.FieldDefaulting<POReceiptLine.projectID> args)
        {
            if (args.Row != null)
            {
                var dailyFieldReportId = Documents.Current.DailyFieldReportId;
                if (dailyFieldReportId != null)
                {
                    args.NewValue = GetDailyFieldReport(dailyFieldReportId)?.ProjectId;
                }
            }
        }

        protected override DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping()
        {
            return new DailyFieldReportRelatedDocumentMapping(typeof(POReceipt))
            {
                ReferenceNumber = typeof(POReceipt.receiptNbr)
            };
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportPurchaseReceipt))
            {
                RelationNumber = typeof(DailyFieldReportPurchaseReceipt.purchaseReceiptId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportPurchaseReceipts);
        }

        private DailyFieldReport GetDailyFieldReport(int? dailyFieldReportId)
        {
            return Base.Select<DailyFieldReport>().SingleOrDefault(dfr => dfr.DailyFieldReportId == dailyFieldReportId);
        }
    }
}