using System.Collections;
using System.Linq;
using PX.Objects.PM;

namespace PX.Objects.CN.Mobile.EP.Descriptor.Attributes
{
    public class MobileProjectCostCodeSelectorAttribute : CostCodeSelectorAttribute
    {
        protected override IEnumerable GetRecords()
        {
            return _Graph.IsMobile
                ? base.GetRecords().Cast<PMCostCode>().Where(c => IsProjectSpecific(c.CostCodeID.GetValueOrDefault()))
                : base.GetRecords();
        }
    }
}