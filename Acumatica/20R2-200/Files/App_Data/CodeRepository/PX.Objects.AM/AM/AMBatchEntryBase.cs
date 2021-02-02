using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Base graph with split/LS members
    /// </summary>
#pragma warning disable PX1018 // The graph with the specified primary DAC type does not contain a view of this type
    public abstract class AMBatchEntryBase : AMBatchSimpleEntryBase
#pragma warning restore PX1018
    {
        public abstract LSAMMTran LSSelectDataMember { get; }
        public abstract PXSelectBase<AMMTranSplit> AMMTranSplitDataMember { get; }
    }

    /// <summary>
    /// Base graph without split/LS members
    /// (Implementation copy of INRegisterEntryBase)
    /// </summary>
#pragma warning disable PX1093 // Graph declaration should contain graph type as first type paramenter
#pragma warning disable PX1018 // The graph with the specified primary DAC type does not contain a view of this type
    public abstract class AMBatchSimpleEntryBase : PXGraph<PXGraph, AMBatch>
#pragma warning restore PX1093
#pragma warning restore PX1018
    {
        public PXSetup<AMPSetup> ampsetup;

        public abstract PXSelectBase<AMBatch> AMBatchDataMember { get; }
        public abstract PXSelectBase<AMMTran> AMMTranDataMember { get; }
    }
}
