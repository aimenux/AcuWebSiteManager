using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Bql;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class FinancialPeriodDataProvider
	{
		public static OrganizationFinPeriod GetFinancialPeriod(PXGraph graph, string financialPeriodId)
		{
			return SelectFrom<OrganizationFinPeriod>
				.Where<OrganizationFinPeriod.finPeriodID.IsEqual<P.AsString>
					.And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, AccessInfo.branchID.FromCurrent>>>
				.View.Select(graph, financialPeriodId);
		}
	}
}