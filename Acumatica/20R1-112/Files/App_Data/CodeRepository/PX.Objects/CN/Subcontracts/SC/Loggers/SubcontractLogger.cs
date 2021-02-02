using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common.Tools;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.SC.Loggers
{
    public class SubcontractLogger
    {
        private readonly POOrder subcontract;
        private readonly List<POLine> subcontractLines;

        public SubcontractLogger(POOrder subcontract, List<POLine> subcontractLines)
        {
            this.subcontract = subcontract;
            this.subcontractLines = subcontractLines;
        }

        public void TraceFullInformation()
        {
            TraceDocumentInformation();
            TraceTransactionsInformation();
        }

        private void TraceDocumentInformation()
        {
            PXTrace.WriteInformation(subcontract.Dump());
        }

        private void TraceTransactionsInformation()
        {
            foreach (var line in subcontractLines)
            {
                PXTrace.WriteInformation(line.Dump());
            }
        }
    }
}
