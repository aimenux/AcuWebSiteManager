using System;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.SM;
using PX.Objects.PM;

namespace PX.Objects.PJ.PhotoLogs.PJ.Attributes
{
    public class PhotoLogSearchableAttribute : PXSearchableAttribute
    {
        private const string TitlePrefix = "Photo Log: {0}";
        private const string FirstLineFormat = "{0}{1:d}{2}{3}";
        private const string SecondLineFormat = "{0}{1}";

        private static readonly Type[] TitleFields =
        {
            typeof(PhotoLog.photoLogCd)
        };

        private static readonly Type[] FieldsForTheFirstLine =
        {
            typeof(PhotoLog.statusId),
            typeof(PhotoLog.date),
            typeof(PhotoLog.description),
            typeof(PhotoLog.createdById)
        };

        private static readonly Type[] FieldsForTheSecondLine =
        {
            typeof(PhotoLog.projectId),
            typeof(PhotoLog.projectTaskId)
        };

        private static readonly Type[] IndexedFields =
       {
            typeof(PhotoLog.description),
            typeof(PhotoLog.projectId),
            typeof(PMProject.contractCD),
            typeof(PMProject.description)
        };

        public PhotoLogSearchableAttribute()
            : base(SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
        {
            NumberFields = TitleFields;
            Line1Format = FirstLineFormat;
            Line1Fields = FieldsForTheFirstLine;
            Line2Format = SecondLineFormat;
            Line2Fields = FieldsForTheSecondLine;
            SelectForFastIndexing = typeof(Select2<PhotoLog, InnerJoin<PMProject, On<PhotoLog.projectId, Equal<PMProject.contractID>>>>);
        }
    }
}
