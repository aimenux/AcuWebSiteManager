using System;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.SM;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes
{
    public class ProjectIssueSearchableAttribute : PXSearchableAttribute
    {
        private const string TitlePrefix = "Project Issue: {0}";
        private const string FirstLineFormat = "{0:d}{1}{2}";
        private const string SecondLineFormat = "{0}{1}{2}";

        private static readonly Type[] FieldsForTheFirstLine =
        {
            typeof(ProjectIssue.creationDate),
            typeof(ProjectIssue.status),
            typeof(ProjectIssue.projectId),
        };

        private static readonly Type[] FieldsForTheSecondLine =
        {
            typeof(ProjectIssue.projectId),
            typeof(ProjectIssue.priorityId),
            typeof(ProjectIssue.summary)
        };

        private static readonly Type[] TitleFields =
        {
            typeof(ProjectIssue.projectIssueCd)
        };

        private static readonly Type[] IndexedFields =
        {
            typeof(ProjectIssue.summary),
            typeof(ProjectIssue.projectId),
            typeof(PMProject.contractCD),
            typeof(PMProject.description)
        };

        public ProjectIssueSearchableAttribute()
            : base(SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
        {
            NumberFields = TitleFields;
            Line1Format = FirstLineFormat;
            Line1Fields = FieldsForTheFirstLine;
            Line2Format = SecondLineFormat;
            Line2Fields = FieldsForTheSecondLine;
            SelectForFastIndexing = typeof(Select2<ProjectIssue, InnerJoin<PMProject, On<ProjectIssue.projectId, Equal<PMProject.contractID>>>>);
        }
    }
}
