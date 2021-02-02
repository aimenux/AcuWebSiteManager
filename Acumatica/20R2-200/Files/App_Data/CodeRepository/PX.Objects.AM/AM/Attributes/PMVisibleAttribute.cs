using PX.Data;
using PX.Objects.GL;
using System;

namespace PX.Objects.AM
{
    [PXUIField(Visibility = PXUIVisibility.Visible)]
    public class PMVisibleAttribute : AcctSubAttribute
    {
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            PXGraph graph = sender.Graph;
            if (graph == null)
                throw new ArgumentNullException("graph");
            Visible = ProjectHelper.IsPMVisible(sender.Graph);
        }
    }
}
