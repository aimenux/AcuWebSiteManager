using PX.Objects.PJ.Common.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Objects.PJ.Submittals.PJ.Graphs;
using System.Collections;

namespace PX.Objects.PJ.Submittals.PJ.Graphs
{
	public class SubmittalActivityListWithAttach<TProjectManagementEntity> : ProjectManagementActivityList<TProjectManagementEntity>
		where TProjectManagementEntity : class, IBqlTable, new()
	{
		public SubmittalActivityListWithAttach(PXGraph graph)
			: base(graph)
		{
		}

		public override IEnumerable ViewActivity(PXAdapter adapter)
		{
			try
			{
				base.ViewActivity(adapter);
			}
			catch (PXRedirectRequiredException e)
			{
				throw e;
			}

			return adapter.Get();
		}

		public override IEnumerable NewMailActivity(PXAdapter adapter)
		{
			try
			{
				base.NewMailActivity(adapter);
			}
			catch (PXRedirectRequiredException e)
			{

				e.RepaintControls = true;
				throw e;
			}

			return adapter.Get();
		}
	}
}
