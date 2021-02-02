using System;
using PX.Objects.PM;

namespace PX.Objects.CN.Mobile.EP.Descriptor.Attributes
{
    public class MobileProjectCostCodeDimensionSelectorAttribute : CostCodeDimensionSelectorAttribute
    {
        public MobileProjectCostCodeDimensionSelectorAttribute(Type task, string budgetType)
            : base(null, task, budgetType, null, false)
        {
            _Attributes[_Attributes.Count - 1] = new MobileProjectCostCodeSelectorAttribute
            {
                TaskID = task,
                BudgetType = budgetType,
                DescriptionField = typeof(PMCostCode.description)
            };
        }
    }
}