using System.Collections;
using PX.Data;

namespace PX.Objects.CR
{
	public class MergeProcess : PXGraph<MergeProcess>
	{
		#region Selects

		[PXViewName(Messages.MergeItems)] 
		public PXProcessing<CRMerge>
			Items;

		#endregion

		#region Ctors

		public MergeProcess()
		{
			Items.SetProcessCaption(Messages.Process);
			Items.SetProcessAllCaption(Messages.ProcessAll);
			Items.SetSelected<CRMerge.selected>();

			PXProcessingStep[] targets = PXAutomation.GetProcessingSteps(this);
			if (targets.Length > 0)
			{
				Items.SetProcessTarget(targets[0].GraphName,
					targets.Length > 1 ? null : targets[0].Name,
					targets[0].Actions[0].Name,
					targets[0].Actions[0].Menus[0],
					null, null);
			}
			else
			{
				throw new PXScreenMisconfigurationException(SO.Messages.MissingMassProcessWorkFlow);
			}

			Actions.Move("Process", "Cancel");
		}

		#endregion

		#region Actions

		public PXCancel<CRMerge> Cancel;

		public PXAction<CRMerge> viewDetails;

		[PXUIField(DisplayName = Messages.ViewDetails)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			var row = Items.Current;
			if (row != null)
			{
				var graph = PXGraph.CreateInstance<MergeMaint>();
				graph.Document.Current = graph.Document.Search<CRMerge.mergeCD>(row.MergeCD);
				throw new PXRedirectRequiredException(graph, true, Messages.ViewDetails)
				{
					Mode = PXBaseRedirectException.WindowMode.NewWindow
				};
			}
			return adapter.Get();
		}

		#endregion
	}
}
