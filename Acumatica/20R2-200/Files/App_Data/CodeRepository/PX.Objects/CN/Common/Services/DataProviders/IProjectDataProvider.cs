using System.Collections.Generic;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public interface IProjectDataProvider
	{
		PMProject GetProject(PXGraph graph, int? projectId);

		IEnumerable<PMProject> GetProjects(PXGraph graph);

		bool IsActiveProject(PXGraph graph, int? projectId);
	}
}