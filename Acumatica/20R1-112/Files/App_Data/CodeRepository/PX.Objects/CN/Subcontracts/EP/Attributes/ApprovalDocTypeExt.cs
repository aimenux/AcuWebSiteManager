using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CN.Subcontracts.EP.Descriptor;
using PX.Objects.CN.Subcontracts.PO.Extensions;
using PX.Objects.EP;

namespace PX.Objects.CN.Subcontracts.EP.Attributes
{
    public class ApprovalDocTypeExt : ApprovalDocType<EPApprovalProcess.EPOwned.entityType, EPApproval.sourceItemType>
    {
        public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
        {
            var epOwned = item as EPApprovalProcess.EPOwned;
            return epOwned.GetSubcontractEntity(cache.Graph) != null
                ? Constants.SubcontractDocumentType
                : base.Evaluate(cache, item, pars);
        }
    }
}