using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PM.GraphExtensions
{
    public class ChangeOrderEntryExtension : CreatedFromDailyFieldReportExtension<ChangeOrderEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportChangeOrder>
            .Where<DailyFieldReportChangeOrder.changeOrderId.IsEqual<PMChangeOrder.refNbr.FromCurrent>>
            .View DailyFieldReportChangeOrders;

        protected override string EntityName => DailyFieldReportEntityNames.ChangeOrder;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected override DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping()
        {
            return new DailyFieldReportRelatedDocumentMapping(typeof(PMChangeOrder))
            {
                ReferenceNumber = typeof(PMChangeOrder.refNbr)
            };
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportChangeOrder))
            {
                RelationNumber = typeof(DailyFieldReportChangeOrder.changeOrderId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportChangeOrders);
        }
    }
}