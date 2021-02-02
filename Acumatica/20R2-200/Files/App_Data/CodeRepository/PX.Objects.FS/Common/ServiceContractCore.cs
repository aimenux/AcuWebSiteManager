using PX.Data;
using System;

namespace PX.Objects.FS
{
    public static class ServiceContractCore
    {
        #region ServiceContract
        public class ServiceContract_View : PXSelect<FSServiceContract,
                                            Where<
                                                FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
        {
            public ServiceContract_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceContract_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }
        #endregion
    }
}
