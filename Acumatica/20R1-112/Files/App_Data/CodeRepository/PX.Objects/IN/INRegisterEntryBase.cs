using PX.Data;

namespace PX.Objects.IN
{
	public abstract class INRegisterEntryBase : PXGraph<PXGraph, INRegister>	// the first generic parameter is not used by the platform
	{
		public PXSetup<INSetup> insetup;
		public abstract PXSelectBase<INRegister> INRegisterDataMember { get; }
		public abstract PXSelectBase<INTran> INTranDataMember { get; }
		public abstract LSINTran LSSelectDataMember { get; }
		public abstract PXSelectBase<INTranSplit> INTranSplitDataMember { get; }
	}
}
