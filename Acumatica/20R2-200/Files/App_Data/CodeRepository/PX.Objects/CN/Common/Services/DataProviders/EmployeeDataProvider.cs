using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class EmployeeDataProvider : IEmployeeDataProvider
	{
		public EPEmployee GetEmployee(PXGraph graph, int? contactID)
		{
			return SelectFrom<EPEmployee>
				.Where<EPEmployee.defContactID.IsEqual<P.AsInt>>.View.Select(graph, contactID);
		}
	}
}