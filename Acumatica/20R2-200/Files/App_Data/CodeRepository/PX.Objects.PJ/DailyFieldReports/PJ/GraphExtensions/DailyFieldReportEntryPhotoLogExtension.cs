using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryPhotoLogExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.PhotoLogs)]
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportPhotoLog>
            .LeftJoin<PhotoLog>
                .On<DailyFieldReportPhotoLog.photoLogId.IsEqual<PhotoLog.photoLogId>>
            .Where<DailyFieldReportPhotoLog.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View PhotoLogs;

        [PXHidden]
        [PXCopyPasteHiddenView]
        public SelectFrom<Photo>
            .InnerJoin<PhotoLog>
                .On<Photo.photoLogId.IsEqual<PhotoLog.photoLogId>>
            .Where<Photo.photoLogId.IsEqual<DailyFieldReportPhotoLog.photoLogId.FromCurrent>
                .And<Photo.isMainPhoto.IsEqual<True>>>.View MainPhoto;

        public PXAction<DailyFieldReport> CreatePhotoLog;

        public PXAction<DailyFieldReport> ViewPhotoLog;

        [InjectDependency]
        public IPhotoLogDataProvider PhotoLogDataProvider
        {
            get;
            set;
        }

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.PhotoLog, ViewNames.PhotoLogs);

        [PXButton]
        [PXUIField]
        public virtual void viewPhotoLog()
        {
            var graph = PXGraph.CreateInstance<PhotoLogEntry>();
            graph.PhotoLog.Current = graph.PhotoLog.Search<PhotoLog.photoLogId>(PhotoLogs.Current.PhotoLogId);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField(DisplayName = "Create New Photo Log")]
        public virtual void createPhotoLog()
        {
            Base.Actions.PressSave();
            var graph = PXGraph.CreateInstance<PhotoLogEntry>();
            InsertPhotoLog(graph);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        public override void _(Events.RowSelected<DailyFieldReport> args)
        {
            base._(args);
            if (args.Row is DailyFieldReport dailyFieldReport)
            {
                var isActionAvailable = IsCreationActionAvailable(dailyFieldReport);
                CreatePhotoLog.SetEnabled(isActionAvailable);
            }
        }

        public virtual void _(Events.FieldUpdated<DailyFieldReportPhotoLog,
            DailyFieldReportPhotoLog.photoLogId> args)
        {
            var photoLogIsDuplicated = Base.IsMobile || args.OldValue != null
                ? DoesViewHaveAtLeastTwoSamePhotoLogs(args.Row)
                : DoesViewHaveTheSamePhotoLogs(args.Row);
            if (photoLogIsDuplicated)
            {
                RaiseDailyFieldReportException(args.Cache, args.Row);
            }
        }

        public virtual void _(Events.RowPersisting<DailyFieldReportPhotoLog> args)
        {
            if (DoesViewHaveAtLeastTwoSamePhotoLogs(args.Row))
            {
                RaiseDailyFieldReportException(args.Cache, args.Row);
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportPhotoLog))
            {
                RelationId = typeof(DailyFieldReportPhotoLog.photoLogId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(PhotoLogs);
        }

        private void RaiseDailyFieldReportException(PXCache cache, DailyFieldReportPhotoLog dailyFieldReportPhotoLog)
        {
            var photoLog = PhotoLogDataProvider.GetPhotoLog(dailyFieldReportPhotoLog.PhotoLogId);
            if (photoLog == null)
            {
                return;
            }
            var message = string.Format(DailyFieldReportMessages.EntityCannotBeSelectedTwice, Name.Entity);
            cache.RaiseException<DailyFieldReportPhotoLog.photoLogId>(dailyFieldReportPhotoLog,
                message, photoLog.PhotoLogCd);
        }

        private bool DoesViewHaveAtLeastTwoSamePhotoLogs(DailyFieldReportPhotoLog dailyFieldReportPhotoLog)
        {
            return PhotoLogs.Cache.Cached.Cast<DailyFieldReportPhotoLog>()
                .Where(pl => pl.PhotoLogId == dailyFieldReportPhotoLog.PhotoLogId
                    && pl.DailyFieldReportId == dailyFieldReportPhotoLog.DailyFieldReportId).HasAtLeastTwoItems();
        }

        private bool DoesViewHaveTheSamePhotoLogs(DailyFieldReportPhotoLog dailyFieldReportPhotoLog)
        {
            return PhotoLogs.SelectMain().Any(pl => pl.PhotoLogId == dailyFieldReportPhotoLog.PhotoLogId
                && pl.DailyFieldReportPhotoLogId != dailyFieldReportPhotoLog.DailyFieldReportPhotoLogId);
        }

        private void InsertPhotoLog(PhotoLogEntry graph)
        {
            var photoLog = graph.PhotoLog.Insert();
            var dailyFieldReport = Base.DailyFieldReport.Current;
            photoLog.ProjectId = dailyFieldReport.ProjectId;
            photoLog.Date = dailyFieldReport.Date;
            graph.PhotoLog.Cache.SetValueExt<PhotoLog.dailyFieldReportId>(photoLog,
                dailyFieldReport.DailyFieldReportId);
        }
    }
}