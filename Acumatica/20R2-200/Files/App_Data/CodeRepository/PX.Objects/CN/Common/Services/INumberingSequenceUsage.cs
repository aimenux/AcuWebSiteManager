using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CN.Common.Services
{
	public interface INumberingSequenceUsage
	{
		void CheckForNumberingUsage<TSetup, TNumberingId>(Numbering numbering,
			PXGraph graph, string message)
			where TSetup : class, IBqlTable, new();
	}
}