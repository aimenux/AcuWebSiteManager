using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Services.DataProviders
{
    public class ComplianceAttributeTypeDataProvider : IComplianceAttributeTypeDataProvider
    {
        public ComplianceAttributeType GetComplianceAttributeType(PXGraph graph, string documentType)
        {
            return SelectFrom<ComplianceAttributeType>
                .Where<ComplianceAttributeType.type.IsEqual<P.AsString>>.View.Select(graph, documentType);
        }
    }
}