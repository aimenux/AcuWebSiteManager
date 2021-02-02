using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class ProjectDataProvider : IProjectDataProvider
	{
		public PMProject GetProject(PXGraph graph, int? projectId)
		{
			return graph.Select<PMProject>().SingleOrDefault(p => p.ContractID == projectId);
		}

		public IEnumerable<PMProject> GetProjects(PXGraph graph)
		{
			return SelectFrom<PMProject>
				.Where<PMProject.nonProject.IsEqual<False>
					.And<PMProject.baseType.IsEqual<CTPRType.project>>>.View.Select(graph).FirstTableItems;
		}

		public bool IsActiveProject(PXGraph graph, int? projectId)
		{
			var project = GetProject(graph, projectId);
			return project != null && project.Status
				       .IsIn(ProjectStatus.Active, ProjectStatus.Planned);
		}
	}
}