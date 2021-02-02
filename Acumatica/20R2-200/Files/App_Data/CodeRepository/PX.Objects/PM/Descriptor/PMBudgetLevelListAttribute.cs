using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.PM
{
	public class PMBudgetLevelListAttribute : PXStringListAttribute
	{
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();

			allowedValues.Add(BudgetLevels.Task);
			allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.Task));

			if (CostCodeAttribute.UseCostCode())
			{
				allowedValues.Add(BudgetLevels.CostCode);
				allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_CostCode));
			}

			allowedValues.Add(BudgetLevels.Item);
			allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_Item));

			if (CostCodeAttribute.UseCostCode() && PXAccess.FeatureInstalled<CS.FeaturesSet.construction>())
			{
				allowedValues.Add(BudgetLevels.Detail);
				allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_Detail));
			}

			_AllowedValues = allowedValues.ToArray();

			_AllowedLabels = allowedLabels.ToArray();

			base.FieldSelecting(sender, e);
		}
	}
}