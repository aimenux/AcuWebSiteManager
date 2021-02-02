using System.Collections;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public sealed class ChangeRequestSelectorAttribute : RelatedEntitiesBaseSelectorAttribute
    {
        public ChangeRequestSelectorAttribute()
            : base(typeof(PMChangeRequest.refNbr),
                typeof(PMChangeRequest.refNbr),
                typeof(PMChangeRequest.date),
                typeof(PMChangeRequest.extRefNbr),
                typeof(PMChangeRequest.description),
                typeof(PMChangeRequest.costTotal),
                typeof(PMChangeRequest.lineTotal),
                typeof(PMChangeRequest.markupTotal),
                typeof(PMChangeRequest.priceTotal))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedChangeRequestNumbers = _Graph.GetExtension<DailyFieldReportEntryChangeRequestExtension>()
                .ChangeRequests.SelectMain().Select(cr => cr.ChangeRequestId);
            return GetRelatedEntities<PMChangeRequest, PMChangeRequest.projectID>()
                .Where(cr => cr.RefNbr.IsNotIn(linkedChangeRequestNumbers));
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            RelatedEntitiesIds = GetRelatedEntities<PMChangeRequest>().Select(cr => cr.RefNbr);
            base.FieldVerifying(cache, args);
        }
    }
}