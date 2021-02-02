using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class PMForecastHistoryAccumAttribute : PXAccumulatorAttribute
	{
		public PMForecastHistoryAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			PMForecastHistory item = (PMForecastHistory)row;

			columns.Update<PMForecastHistory.actualQty>(item.ActualQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.curyActualAmount>(item.CuryActualAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.actualAmount>(item.ActualAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.changeOrderQty>(item.ChangeOrderQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.curyChangeOrderAmount>(item.CuryChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.draftChangeOrderQty>(item.DraftChangeOrderQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.curyDraftChangeOrderAmount>(item.CuryDraftChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}


	}
}
