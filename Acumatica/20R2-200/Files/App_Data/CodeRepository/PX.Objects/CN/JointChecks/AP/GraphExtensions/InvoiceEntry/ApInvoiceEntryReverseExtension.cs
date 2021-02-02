using System;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry
{
    public class ApInvoiceEntryReverseExtension : PXGraphExtension<PX.Objects.CN.Compliance.AP.GraphExtensions.ApInvoiceEntryExt,
        APInvoiceEntry>
    {
        [PXOverride]
        public virtual IEnumerable ReverseInvoice(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            var lienWaiverVoidService = new LienWaiverVoidService();
            var complianceDocuments = Base1.ComplianceDocuments.SelectMain()
                .Where(cd => cd.IsCreatedAutomatically == true).ToList();
            if (complianceDocuments.Any() && lienWaiverVoidService.IsVoidOfAutomaticallyGeneratedLienWaiverConfirmed(
                Base, LienWaiverReferencedDocument.ApBill))
            {
                lienWaiverVoidService.VoidAutomaticallyCreatedLienWaivers(
                    Base1.ComplianceDocuments.Cache, complianceDocuments);
                Base1.ComplianceDocuments.Cache.Persist(PXDBOperation.Update);
            }
            return baseHandler(adapter);
        }

        public static bool IsActive()
        {
	        return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
	               !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }
	}
}
