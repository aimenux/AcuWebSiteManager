using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class ProjectAccountingService : PXGraph<ProjectAccountingService>
	{
		public virtual void NavigateToProjectScreen(int? projectID, PXRedirectHelper.WindowMode mode)
		{
			var graph = CreateInstance<ProjectEntry>();
			graph.Project.Current = graph.Project.Search<PMProject.contractID>(projectID);
			PXRedirectHelper.TryRedirect(graph, mode);
		}
	}
}
