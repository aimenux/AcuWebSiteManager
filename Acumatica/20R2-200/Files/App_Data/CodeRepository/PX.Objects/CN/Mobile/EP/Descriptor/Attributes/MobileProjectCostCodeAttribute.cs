using System;
using PX.Objects.PM;

namespace PX.Objects.CN.Mobile.EP.Descriptor.Attributes
{
    public class MobileProjectCostCodeAttribute : CostCodeAttribute
    {
        public MobileProjectCostCodeAttribute(Type task, string budgetType)
            : base(null, task, budgetType)
        {
            _Attributes[_Attributes.Count - 1] = new MobileProjectCostCodeDimensionSelectorAttribute(task, budgetType);
            _SelAttrIndex = _Attributes.Count - 1;
        }
    }
}