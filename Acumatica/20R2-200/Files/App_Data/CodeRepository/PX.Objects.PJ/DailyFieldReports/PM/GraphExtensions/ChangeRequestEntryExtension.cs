using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

namespace PX.Objects.PJ.DailyFieldReports.PM.GraphExtensions
{
    public class ChangeRequestEntryExtension : CreatedFromDailyFieldReportExtension<ChangeRequestEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportChangeRequest>
            .Where<DailyFieldReportChangeRequest.changeRequestId.IsEqual<PMChangeRequest.refNbr.FromCurrent>>
            .View DailyFieldReportChangeRequests;

        protected override string EntityName => DailyFieldReportEntityNames.ChangeRequest;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected override DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping()
        {
            return new DailyFieldReportRelatedDocumentMapping(typeof(PMChangeRequest))
            {
                ReferenceNumber = typeof(PMChangeRequest.refNbr)
            };
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportChangeRequest))
            {
                RelationNumber = typeof(DailyFieldReportChangeRequest.changeRequestId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportChangeRequests);
        }
    }
}