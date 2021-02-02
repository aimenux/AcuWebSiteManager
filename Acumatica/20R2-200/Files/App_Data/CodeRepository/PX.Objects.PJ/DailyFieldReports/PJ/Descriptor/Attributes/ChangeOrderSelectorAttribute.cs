using System.Collections;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public sealed class ChangeOrderSelectorAttribute : RelatedEntitiesBaseSelectorAttribute
    {
        public ChangeOrderSelectorAttribute()
            : base(typeof(PMChangeOrder.refNbr),
                typeof(PMChangeOrder.refNbr),
                typeof(PMChangeOrder.status),
                typeof(PMChangeOrder.projectNbr),
                typeof(PMChangeOrder.date),
                typeof(PMChangeOrder.completionDate),
                typeof(PMChangeOrder.description))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedChangeOrderNumbers = _Graph.GetExtension<DailyFieldReportEntryChangeOrderExtension>()
                .ChangeOrders.SelectMain().Select(co => co.ChangeOrderId);
            return GetRelatedEntities<PMChangeOrder, PMChangeOrder.projectID>()
                .Where(co => co.RefNbr.IsNotIn(linkedChangeOrderNumbers));
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            RelatedEntitiesIds = GetRelatedEntities<PMChangeOrder>().Select(co => co.RefNbr);
            base.FieldVerifying(cache, args);
        }
    }
}
