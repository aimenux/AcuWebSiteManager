using System.Linq;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public class DrawingLogRelationStatusAttribute : PXStringListAttribute
    {
        public DrawingLogRelationStatusAttribute()
        {
            var requestForInformationStatusAttribute = new RequestForInformationStatusAttribute();
            var projectIssueStatusAttribute = new ProjectIssueStatusAttribute();
            _AllowedLabels = GetLabels(requestForInformationStatusAttribute, projectIssueStatusAttribute);
            _AllowedValues = GetValues(requestForInformationStatusAttribute, projectIssueStatusAttribute);
        }

        private static string[] GetLabels(PXStringListAttribute requestForInformationStatusAttribute,
            PXStringListAttribute projectIssueStatusAttribute)
        {
            var labels = requestForInformationStatusAttribute.ValueLabelDic.Select(x => x.Value).ToList();
            labels.AddRange(projectIssueStatusAttribute.ValueLabelDic.Select(x => x.Value));
            return labels.Distinct().ToArray();
        }

        private static string[] GetValues(PXStringListAttribute requestForInformationStatusAttribute,
            PXStringListAttribute projectIssueStatusAttribute)
        {
            var values = requestForInformationStatusAttribute.ValueLabelDic.Select(x => x.Key).ToList();
            values.AddRange(projectIssueStatusAttribute.ValueLabelDic.Select(x => x.Key));
            return values.Distinct().ToArray();
        }
    }
}
