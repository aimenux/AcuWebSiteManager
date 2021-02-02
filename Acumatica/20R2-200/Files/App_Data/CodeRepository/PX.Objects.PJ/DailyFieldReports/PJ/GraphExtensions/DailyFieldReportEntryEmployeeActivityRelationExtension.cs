using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryEmployeeActivityRelationExtension :
        DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportEmployeeActivity>
            .Where<DailyFieldReportEmployeeActivity.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View DailyFieldReportEmployeeActivity;

        [PXHidden]
        public SelectFrom<EPActivityApprove>.View EpActivityApprove;

        public PXAction<DailyFieldReport> ViewTimeCard;

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.EmployeeTimeActivity, ViewNames.EmployeeActivities);

        protected override Type RelationPrimaryCacheType => typeof(EPActivityApprove);

        public override void Initialize()
        {
            Relations = new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportEmployeeActivity);
        }

        [PXDefault(typeof(DailyFieldReport.projectId.FromCurrent))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_ProjectID_CacheAttached(PXCache cache)
        {
        }

        [PXButton]
        [PXUIField]
        public virtual void viewTimeCard()
        {
            var timeCardCd = (Base.Caches<EPActivityApprove>().Current as EPActivityApprove)?.TimeCardCD;
            if (timeCardCd != null)
            {
                var timeCardMaint = PXGraph.CreateInstance<TimeCardMaint>();
                timeCardMaint.Document.Current = timeCardMaint.Document.Search<EPTimeCard.timeCardCD>(timeCardCd);
                timeCardMaint.RedirectToEntity(timeCardMaint.Document.Current, PXRedirectHelper.WindowMode.NewWindow);
            }
        }

        public virtual void _(Events.RowPersisting<EPActivityApprove> args)
        {
            if (args.Operation == PXDBOperation.Insert)
            {
                args.Cache.SetValueExt<EPActivityApprove.hold>(args.Row, false);
            }
        }

        public virtual void _(Events.RowPersisted<EPActivityApprove> args)
        {
            if (args.Row != null && args.Cache.GetStatus(args.Row) == PXEntryStatus.Inserted)
            {
                if (args.TranStatus == PXTranStatus.Open)
                {
                    InsertNewDailyFieldReportEmployeeActivity(args.Row);
                }
                if (args.TranStatus == PXTranStatus.Aborted)
                {
                    args.Cache.SetValueExt<EPActivityApprove.hold>(args.Row, true);
                }
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportEmployeeActivity))
            {
                RelationNoteId = typeof(DailyFieldReportEmployeeActivity.employeeActivityId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportEmployeeActivity);
        }

        private void InsertNewDailyFieldReportEmployeeActivity(EPActivityApprove employeeActivity)
        {
            if (!IsEmployeeActivityInserted(employeeActivity.NoteID))
            {
                var dailyFieldReportEmployeeActivity = new DailyFieldReportEmployeeActivity
                {
                    DailyFieldReportId = Base.DailyFieldReport.Current.DailyFieldReportId,
                    EmployeeActivityId = employeeActivity.NoteID
                };
                DailyFieldReportEmployeeActivity.Insert(dailyFieldReportEmployeeActivity);
            }
        }

        private bool IsEmployeeActivityInserted(Guid? employeeActivityId)
        {
            var insertedEmployeeActivities =
                (IEnumerable<DailyFieldReportEmployeeActivity>) DailyFieldReportEmployeeActivity.Cache.Inserted;
            return insertedEmployeeActivities.Any(a => a.EmployeeActivityId == employeeActivityId);
        }
    }
}