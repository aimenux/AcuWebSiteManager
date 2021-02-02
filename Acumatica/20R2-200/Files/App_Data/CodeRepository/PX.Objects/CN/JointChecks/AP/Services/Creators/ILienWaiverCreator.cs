using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.Models;

namespace PX.Objects.CN.JointChecks.AP.Services.Creators
{
    public interface ILienWaiverCreator
    {
        void CreateLienWaiver(LienWaiverGenerationKey lienWaiverGenerationKey, APPayment payment,
            ComplianceAttribute complianceAttribute);
    }
}