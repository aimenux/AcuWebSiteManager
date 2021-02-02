using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;

using PX.Objects.Common;

namespace PX.Objects.AR.Repositories
{
	public class ARStatementRepository : RepositoryBase<ARStatement>
	{
		public ARStatementRepository(PXGraph graph)
			: base(graph)
		{ }

		/// <summary>
		/// Finds the last statement in the specified statement cycle. 
		/// </summary>
		public ARStatement FindLastStatement(string statementCycleId, DateTime? priorToDate = null, bool includeOnDemand = false)
			=> SelectSingle<
				Where<
					ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
					And2<Where<
						ARStatement.onDemand, Equal<False>,
						Or<ARStatement.onDemand, Equal<Required<ARStatement.onDemand>>>>,
					And<Where<
						ARStatement.statementDate, Less<Required<ARStatement.statementDate>>,
						Or<Required<ARStatement.statementDate>, IsNull>>>>>,
				OrderBy<
					Desc<ARStatement.statementDate>>>(statementCycleId, includeOnDemand, priorToDate, priorToDate);

		public ARStatement FindLastStatement(Customer customer, DateTime? priorToDate = null, bool includeOnDemand = false)
			=> SelectSingle<
				Where<
					ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
					And<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And2<Where<
						ARStatement.onDemand, Equal<False>,
						Or<ARStatement.onDemand, Equal<Required<ARStatement.onDemand>>>>,
					And<Where<
						ARStatement.statementDate, Less<Required<ARStatement.statementDate>>,
						Or<Required<ARStatement.statementDate>, IsNull>>>>>>,
				OrderBy<
					Desc<ARStatement.statementDate>>>(
				customer.StatementCycleId,
				customer.StatementCustomerID,
				includeOnDemand,
				priorToDate, 
				priorToDate);

		public ARStatement FindFirstStatement(string statementCycleId, DateTime? afterDate = null, bool includeOnDemand = false)
			=> SelectSingle<
				Where<
					ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
					And2<Where<
						ARStatement.onDemand, Equal<False>,
						Or<ARStatement.onDemand, Equal<Required<ARStatement.onDemand>>>>,
					And<Where<
						ARStatement.statementDate, Greater<Required<ARStatement.statementDate>>,
						Or<Required<ARStatement.statementDate>, IsNull>>>>>,
				OrderBy<
					Asc<ARStatement.statementDate>>>(statementCycleId, includeOnDemand, afterDate, afterDate);
	}
}
