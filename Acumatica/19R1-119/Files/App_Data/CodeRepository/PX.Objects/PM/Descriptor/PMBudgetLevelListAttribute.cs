using PX.Data;

namespace PX.Objects.PM
{
	public class PMBudgetLevelListAttribute : PXStringListAttribute
	{
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			_AllowedValues = CostCodeAttribute.UseCostCode() ? new string[]
			{
				BudgetLevels.Task,
				BudgetLevels.CostCode,
				BudgetLevels.Item
			} : new string[]
			{
				BudgetLevels.Task,
				BudgetLevels.Item
			};

			_AllowedLabels = CostCodeAttribute.UseCostCode() ? new string[]
			{
				PXMessages.LocalizeNoPrefix(Messages.Task),
				PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_CostCode),
				PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_Item)
			} : new string[]
			{
				PXMessages.LocalizeNoPrefix(Messages.Task),
				PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_Item)
			};

			base.FieldSelecting(sender, e);
		}
	}
}