using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Services.DataProviders
{
    public interface IComplianceAttributeTypeDataProvider
    {
        ComplianceAttributeType GetComplianceAttributeType(PXGraph graph, string documentType);
    }
}