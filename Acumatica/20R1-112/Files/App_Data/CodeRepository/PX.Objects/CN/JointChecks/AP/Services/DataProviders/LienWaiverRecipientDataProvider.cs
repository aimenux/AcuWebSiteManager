using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Compliance.PM.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
    public class LienWaiverRecipientDataProvider
    {
        public static LienWaiverRecipient GetLienWaiverRecipient(PXGraph graph, string vendorClassId, int? projectId)
        {
            return SelectFrom<LienWaiverRecipient>
                .Where<LienWaiverRecipient.vendorClassId.IsEqual<P.AsString>
                    .And<LienWaiverRecipient.projectId.IsEqual<P.AsInt>>>.View
                .Select(graph, vendorClassId, projectId);
        }
    }
}