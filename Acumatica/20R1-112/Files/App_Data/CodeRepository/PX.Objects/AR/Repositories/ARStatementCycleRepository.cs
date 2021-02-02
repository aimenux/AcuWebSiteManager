using PX.Data;

using PX.Objects.Common;

namespace PX.Objects.AR.Repositories
{
	public class ARStatementCycleRepository : RepositoryBase<ARStatementCycle>
	{
		public ARStatementCycleRepository(PXGraph graph)
			: base(graph)
		{ }

		public ARStatementCycle FindByID(string statementCycleID) => SelectSingle<
			Where<
				ARStatementCycle.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>>(
			statementCycleID);
	}
}
