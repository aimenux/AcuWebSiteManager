using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.ProjectAccounting.PM.Services;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntrySubcontractorActivityExtension :
        DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.Subcontractors)]
        [PXCopyPasteHiddenFields(typeof(DailyFieldReportSubcontractorActivity.totalWorkingTimeSpent),
            typeof(Note.noteText))]
        public SelectFrom<DailyFieldReportSubcontractorActivity>
            .LeftJoin<Vendor>
                .On<Vendor.bAccountID.IsEqual<DailyFieldReportSubcontractorActivity.vendorId>>
            .Where<DailyFieldReportSubcontractorActivity.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View Subcontractors;

        [InjectDependency]
        public IProjectTaskDataProvider ProjectTaskDataProvider
        {
            get;
            set;
        }

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.Subcontractor, ViewNames.Subcontractors);

        public virtual void _(Events.FieldVerifying<DailyFieldReportSubcontractorActivity,
            DailyFieldReportSubcontractorActivity.workingTimeSpent> args)
        {
            var defaultWorkingTimeSpent = args.Row.DefaultWorkingTimeSpent;
            var subcontractorActivity = args.Row;
            if ((int) args.NewValue > defaultWorkingTimeSpent
                && subcontractorActivity.TimeArrived < subcontractorActivity.TimeDeparted)
            {
                var message =
                    string.Format(DailyFieldReportMessages.WorkingHoursCannotExceedDefaultValue,
                        PXTimeListAttribute.GetString(defaultWorkingTimeSpent));
                throw new PXSetPropertyException<DailyFieldReportSubcontractorActivity.workingTimeSpent>(message);
            }
        }

        public virtual void _(Events.FieldUpdated<DailyFieldReportSubcontractorActivity,
            DailyFieldReportSubcontractorActivity.projectTaskID> args)
        {
            if (args.NewValue is int costTaskId && args.Row.Description == null)
            {
                var projectTask = ProjectTaskDataProvider.GetProjectTask(Base, costTaskId);
                args.Cache.SetValueExt<DailyFieldReportSubcontractorActivity.description>(args.Row,
                    projectTask.Description);
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReportSubcontractorActivity> args)
        {
            var subcontractorActivity = args.Row;
            if (Base.IsMobile && subcontractorActivity != null)
            {
                subcontractorActivity.LastModifiedDateTime =
                    subcontractorActivity.LastModifiedDateTime.GetValueOrDefault().Date;
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportSubcontractorActivity))
            {
                RelationId = typeof(DailyFieldReportSubcontractorActivity.subcontractorId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(Subcontractors);
        }
    }
}