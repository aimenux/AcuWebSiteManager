using PX.Objects.CN.JointChecks.AP.Models;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public interface ILienWaiverTypeDeterminator
    {
        bool IsLienWaiverFinal(LienWaiverGenerationKey generationKey, bool isConditional);
    }
}