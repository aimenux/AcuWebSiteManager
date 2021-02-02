using System;
using PX.Objects.PJ.DailyFieldReports.PJ.CacheExtensions.PJ;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Project Issue")]
    public class DailyFieldReportProjectIssue : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportProjectIssueId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBInt]
        [PXSelector(typeof(SearchFor<ProjectIssue.projectIssueId>
            .Where<ProjectIssue.projectIssueId
                .IsNotInSubselect<SearchFor<projectIssueId>
                    .Where<dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>>
                .And<ProjectIssue.projectId.IsEqual<DailyFieldReport.projectId.FromCurrent>>
                .Or<ProjectIssueExt.dailyFieldReportId.FromCurrent.IsNotNull>>),
            SubstituteKey = typeof(ProjectIssue.projectIssueCd))]
        [PXParent(typeof(SelectFrom<ProjectIssue>
            .Where<ProjectIssue.projectIssueId.IsEqual<projectIssueId>>))]
        [PXUIField(DisplayName = "Project Issue ID", Required = true)]
        public virtual int? ProjectIssueId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportProjectIssueId : BqlInt.Field<dailyFieldReportProjectIssueId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class projectIssueId : BqlInt.Field<projectIssueId>
        {
        }
    }
}