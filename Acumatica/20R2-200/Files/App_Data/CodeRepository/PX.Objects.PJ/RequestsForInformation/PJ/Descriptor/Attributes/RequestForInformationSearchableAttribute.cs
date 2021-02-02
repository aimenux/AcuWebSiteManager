using System;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.SM;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationSearchableAttribute : PXSearchableAttribute
    {
        private const string TitlePrefix = "Request for Information: {0}";
        private const string FirstLineFormat = "{0}{1:d}{2}";
        private const string SecondLineFormat = "{0:d}{1}{2}";

        private static readonly Type[] FieldsForTheFirstLine =
        {
            typeof(RequestForInformation.status),
            typeof(RequestForInformation.creationDate),
            typeof(RequestForInformation.projectId)
        };

        private static readonly Type[] FieldsForTheSecondLine =
        {
            typeof(RequestForInformation.dueResponseDate),
            typeof(RequestForInformation.incoming),
            typeof(RequestForInformation.summary)
        };

        private static readonly Type[] TitleFields =
        {
            typeof(RequestForInformation.requestForInformationCd)
        };

        private static readonly Type[] IndexedFields =
       {
            typeof(RequestForInformation.summary),
            typeof(RequestForInformation.projectId),
            typeof(PMProject.contractCD),
            typeof(PMProject.description)
        };

        public RequestForInformationSearchableAttribute()
            : base(SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
        {
            NumberFields = TitleFields;
            Line1Format = FirstLineFormat;
            Line1Fields = FieldsForTheFirstLine;
            Line2Format = SecondLineFormat;
            Line2Fields = FieldsForTheSecondLine;
            SelectForFastIndexing = typeof(Select2<RequestForInformation, InnerJoin<PMProject, On<RequestForInformation.projectId, Equal<PMProject.contractID>>>>);
        }
    }
}
