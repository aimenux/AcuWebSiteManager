using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryChangeRequestExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.ChangeRequests)]
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportChangeRequest>
            .LeftJoin<PMChangeRequest>
                .On<DailyFieldReportChangeRequest.changeRequestId.IsEqual<PMChangeRequest.refNbr>>
            .Where<DailyFieldReportChangeRequest.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View ChangeRequests;

        public PXAction<DailyFieldReport> CreateChangeRequest;

        public PXAction<DailyFieldReport> ViewChangeRequest;

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.ChangeRequest, ViewNames.ChangeRequests);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.changeRequest>();
        }

        [PXButton]
        [PXUIField]
        public virtual void viewChangeRequest()
        {
            var changeRequestEntry = PXGraph.CreateInstance<ChangeRequestEntry>();
            changeRequestEntry.Document.Current =
                changeRequestEntry.Document.Search<PMChangeRequest.refNbr>(ChangeRequests.Current.ChangeRequestId);
            PXRedirectHelper.TryRedirect(changeRequestEntry, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField(DisplayName = "Create New Change Request")]
        public virtual void createChangeRequest()
        {
            Base.Actions.PressSave();
            var graph = PXGraph.CreateInstance<ChangeRequestEntry>();
            var changeRequest = graph.Document.Insert();
            var dailyFieldReport = Base.DailyFieldReport.Current;
            changeRequest.ProjectID = dailyFieldReport.ProjectId;
            changeRequest.CustomerID = ProjectDataProvider.GetProject(Base, dailyFieldReport.ProjectId).CustomerID;
            changeRequest.Date = dailyFieldReport.Date;
            graph.Document.Cache.SetValueExt<PmChangeRequestExt.dailyFieldReportId>(changeRequest,
                dailyFieldReport.DailyFieldReportId);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        public override void _(Events.RowSelected<DailyFieldReport> args)
        {
            base._(args);
            if (args.Row is DailyFieldReport dailyFieldReport)
            {
                var isActionAvailable = IsCreationActionAvailable(dailyFieldReport);
                CreateChangeRequest.SetEnabled(isActionAvailable);
            }
            SetDisplayName(Base.Caches<PMChangeRequest>());
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
            return new PXSelectExtension<DailyFieldReportRelation>(ChangeRequests);
        }

        private static void SetDisplayName(PXCache cache)
        {
            PXUIFieldAttribute.SetDisplayName<PMChangeRequest.lastModifiedDateTime>(
                cache, ProjectManagementLabels.LastModificationDate);
        }
    }
}
