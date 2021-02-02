using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CR;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryVisitorExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportVisitor>
            .Where<DailyFieldReportVisitor.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View Visitors;

        [InjectDependency]
        public IBusinessAccountDataProvider BusinessAccountDataProvider
        {
            get;
            set;
        }

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.Visitor, ViewNames.Visitors);

        public virtual void _(Events.FieldUpdated<DailyFieldReportVisitor.businessAccountId> args)
        {
            if (args.NewValue is int businessAccountId && args.Row is DailyFieldReportVisitor visitor)
            {
                var businessAccount = BusinessAccountDataProvider.GetBusinessAccountReceivable(Base, businessAccountId);
                if (businessAccount.Type == BAccountType.EmployeeType)
                {
                    businessAccount = BusinessAccountDataProvider
                        .GetBusinessAccountReceivable(Base, businessAccount.ParentBAccountID);
                }
                visitor.Company = businessAccount.AcctName;
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReportVisitor> args)
        {
            var visitor = args.Row;
            if (Base.IsMobile && visitor != null)
            {
                visitor.LastModifiedDateTime = visitor.LastModifiedDateTime.GetValueOrDefault().Date;
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportVisitor))
            {
                RelationId = typeof(DailyFieldReportVisitor.dailyFieldReportVisitorId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
           return new PXSelectExtension<DailyFieldReportRelation>(Visitors);
        }
    }
}