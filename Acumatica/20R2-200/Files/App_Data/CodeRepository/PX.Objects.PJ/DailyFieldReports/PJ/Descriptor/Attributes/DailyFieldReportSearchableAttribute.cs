using System;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.SM;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class DailyFieldReportSearchableAttribute : PXSearchableAttribute
    {
        private const string TitlePrefix = "Daily Field Report: {0}";
        private const string FirstLineFormat = "{0:d}{1}{2}";
        private const string SecondLineFormat = "{0}{2}";

        private static readonly Type[] FieldsForTheFirstLine =
        {
            typeof(DailyFieldReport.date),
            typeof(DailyFieldReport.status),
            typeof(DailyFieldReport.projectId)
        };

        private static readonly Type[] FieldsForTheSecondLine =
        {
            typeof(DailyFieldReport.projectId),
            typeof(DailyFieldReport.projectManagerId),
            typeof(TM.OwnerAttribute.Owner.acctCD)
        };

        private static readonly Type[] TitleFields =
        {
            typeof(DailyFieldReport.dailyFieldReportCd)
        };

        private static readonly Type[] IndexedFields =
        {
            typeof(DailyFieldReport.projectId),
            typeof(PMProject.contractCD),
            typeof(PMProject.description)
        };

        public DailyFieldReportSearchableAttribute()
            : base(SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
        {
            NumberFields = TitleFields;
            Line1Format = FirstLineFormat;
            Line1Fields = FieldsForTheFirstLine;
            Line2Format = SecondLineFormat;
            Line2Fields = FieldsForTheSecondLine;
            SelectForFastIndexing = typeof(Select2<DailyFieldReport, InnerJoin<PMProject, On<DailyFieldReport.projectId, Equal<PMProject.contractID>>>>);
        }

        protected override string OverrideDisplayName(Type field, string displayName)
        {
            if (field == typeof(TM.OwnerAttribute.Owner.acctCD))
            {
                return PX.Objects.PJ.Common.Descriptor.CacheNames.ProjectManager;
            }

            return displayName;
        }
    }
}
