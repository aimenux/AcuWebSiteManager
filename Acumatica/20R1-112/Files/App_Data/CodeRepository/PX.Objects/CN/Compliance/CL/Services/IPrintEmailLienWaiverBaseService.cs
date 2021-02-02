using System.Collections.Generic;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Services
{
    public interface IPrintEmailLienWaiverBaseService
    {
        void Process(List<ComplianceDocument> complianceDocuments);

        bool IsLienWaiverValid(ComplianceDocument complianceDocument);
    }
}