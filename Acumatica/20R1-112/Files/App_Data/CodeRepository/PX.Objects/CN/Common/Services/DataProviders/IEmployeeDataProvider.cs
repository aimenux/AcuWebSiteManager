using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public interface IEmployeeDataProvider
	{
		EPEmployee GetEmployee(PXGraph graph, Guid? userId);
	}
}