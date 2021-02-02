using System.Collections.Generic;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services
{
    public class LienWaiverVoidService
    {
        public bool IsVoidOfAutomaticallyGeneratedLienWaiverConfirmed(PXGraph graph, string message)
        {
            message = string.Format(ComplianceMessages.LienWaiver.WouldYouLikeToVoidAutomaticallyCreatedLienWaiver, message);
            return graph.Views[Constants.ComplianceDocumentViewName].Ask(message, MessageButtons.YesNo).IsPositive();
        }

        public void VoidAutomaticallyCreatedLienWaivers(PXCache cache,
            IEnumerable<ComplianceDocument> complianceDocuments)
        {
            foreach (var complianceDocument in complianceDocuments)
            {
                complianceDocument.IsVoided = true;
                cache.Update(complianceDocument);
            }
        }
    }
}
