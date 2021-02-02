using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryNoteExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXCopyPasteHiddenView]
        [PXViewName(ViewNames.Notes)]
        public SelectFrom<DailyFieldReportNote>
            .Where<DailyFieldReportNote.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View Notes;

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.Note, ViewNames.Notes);

        public virtual void _(Events.FieldDefaulting<DailyFieldReportNote.time> args)
        {
            args.NewValue = PXTimeZoneInfo.Now;
        }

        public virtual void _(Events.RowSelected<DailyFieldReportNote> args)
        {
            var note = args.Row;
            if (Base.IsMobile && note != null)
            {
                note.LastModifiedDateTime = note.LastModifiedDateTime.GetValueOrDefault().Date;
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportNote))
            {
                RelationId = typeof(DailyFieldReportNote.dailyFieldReportNoteId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(Notes);
        }
    }
}