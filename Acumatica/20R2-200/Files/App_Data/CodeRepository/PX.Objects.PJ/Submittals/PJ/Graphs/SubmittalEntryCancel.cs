using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.PJ.Common.Actions;
using PX.Data;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.Common.Extensions;

namespace PX.Objects.PJ.Submittals.PJ.Graphs
{
	public partial class SubmittalEntry
	{
		public class CancelAction<TNode> : PXCancel<TNode>
			where TNode : class, IBqlTable, new()
		{
			public CancelAction(PXGraph graph, string name) : base(graph, name)
			{
			}

			public CancelAction(PXGraph graph, Delegate handler) : base(graph, handler)
			{
			}

			[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
			[PXCancelButton]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				string submittalIDToSelect = (string)adapter.Searches.GetSearchValueByPosition(0);

				PJSubmittal previousSubmittal = (PJSubmittal)Graph.GetPrimaryCache().Current;

				if (previousSubmittal?.SubmittalID != submittalIDToSelect)
				{
					adapter.SortColumns = new string[] {adapter.SortColumns[0]};
					adapter.Searches = new object[] {adapter.Searches[0]};
					adapter.Descendings = new[] {adapter.Descendings[0]};
				}

				return base.Handler(adapter);
			}

			protected override void Insert(PXAdapter adapter)
			{
				PXActionHelper.InsertNoKeysFromUI(adapter);
			}
		}
	}
}
