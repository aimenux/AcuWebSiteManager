using System;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class ReleaseOrd : PXGraph<ReleaseOrd>
    {
        public PXCancel<AMBatch> Cancel;
        [PXFilterable]
        public PXProcessing<AMProdItem,
            Where<AMProdItem.statusID, Equal<ProductionOrderStatus.planned>,
                And<AMProdItem.hold, Equal<False>,
                And<Where<AMProdItem.function, Equal<OrderTypeFunction.regular>,
                    Or<AMProdItem.function, Equal<OrderTypeFunction.disassemble>>>>>>> PlannedOrds;

        public PXSetup<AMPSetup> ampsetup;

        public ReleaseOrd()
        {
            PlannedOrds.SetProcessDelegate(delegate (List<AMProdItem> list)
            {
                ReleaseDoc(list, true);
            });
        }

        public static void ReleaseDoc(List<AMProdItem> list, bool isMassProcess)
        {
            var releaseOrdGraph = CreateInstance<ReleaseOrd>();
            releaseOrdGraph.Clear();
            releaseOrdGraph.ReleaseOrder(list, isMassProcess);
        }

        protected virtual void ReleaseOrder(List<AMProdItem> list, bool isMassProcess)
        {
            ProductionStatus.SetStatus(list, ProductionOrderStatus.Released, new FinancialPeriod { FinancialPeriodID = string.Empty }, isMassProcess, false);
        }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2021R1 + " Use ReleaseOrder(List<AMProdItem>, bool)")]
        protected virtual void ReleaseOrder(AMProdItem doc)
        {
            ProductionStatus.SetStatus(new List<AMProdItem> { doc }, ProductionOrderStatus.Released);
        }
    }
}