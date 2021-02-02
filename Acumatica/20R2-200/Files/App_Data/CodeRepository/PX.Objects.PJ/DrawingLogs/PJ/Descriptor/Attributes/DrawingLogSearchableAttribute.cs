using System;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.SM;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public class DrawingLogSearchableAttribute : PXSearchableAttribute
    {
        private const string TitlePrefix = "Drawing Log: {0}";
        private const string FirstLineFormat = "{0}{1:d}{2}{3}";
        private const string SecondLineFormat = "{0}{1}{2}{3}";

        private static readonly Type[] FieldsForTheFirstLine =
        {
            typeof(DrawingLog.statusId),
            typeof(DrawingLog.drawingDate),
            typeof(DrawingLog.projectId),
            typeof(DrawingLog.disciplineId)
        };

        private static readonly Type[] FieldsForTheSecondLine =
        {
            typeof(DrawingLog.number),
            typeof(DrawingLog.title),
            typeof(DrawingLog.description),
            typeof(DrawingLog.isCurrent)
        };

        private static readonly Type[] TitleFields =
        {
            typeof(DrawingLog.drawingLogCd)
        };

        private static readonly Type[] IndexedFields =
        {
            typeof(DrawingLog.description),
            typeof(DrawingLog.projectId),
            typeof(PMProject.contractCD),
            typeof(PMProject.description)
        };

        public DrawingLogSearchableAttribute()
            : base(SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
        {
            NumberFields = TitleFields;
            Line1Format = FirstLineFormat;
            Line1Fields = FieldsForTheFirstLine;
            Line2Format = SecondLineFormat;
            Line2Fields = FieldsForTheSecondLine;
            SelectForFastIndexing = typeof(Select2<DrawingLog, InnerJoin<PMProject, On<DrawingLog.projectId, Equal<PMProject.contractID>>>>);
        }
    }
}
