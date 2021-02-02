using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.AM.Attributes
{
    [PXDBInt]
    [PXUIField(DisplayName = "Cost Code", FieldClass = COSTCODE)]
    public class CostCodeForProdAttribute : CostCodeAttribute
    {
        public CostCodeForProdAttribute(Type account, Type task, string budgetType) : base(account, task, budgetType, null) { }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var graph = sender.Graph;
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }
            Visible = ProjectHelper.IsPMVisible(sender.Graph);
        }
    }
}
