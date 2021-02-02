using PX.Data;
using PX.Objects.AP;
using PX.Objects.DR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.TX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public class APReleaseProcessExt : PXGraphExtension<APReleaseProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
        }

        [PXOverride]
        public virtual void ReleaseInvoiceTransactionPostProcessing(JournalEntry je, APInvoice apdoc, PXResult<APTran, APTax, Tax, DRDeferredCode, LandedCostCode, InventoryItem, APTaxTran> r, GLTran tran)
        {
            APTran n = (APTran)r;

            if (CopyProjectFromAPTran(apdoc, n))
            {
                tran.ProjectID = n.ProjectID;
                tran.TaskID = n.TaskID;
                tran.CostCodeID = n.CostCodeID;
            }            
        }

        protected virtual bool CopyProjectFromAPTran(APInvoice doc, APTran tran)
        {
            if (doc.IsChildRetainageDocument()) return false;
            if (tran.AccrueCost == true) return false;
            Account account = Account.PK.Find(Base, tran.AccountID);
            if (account != null && account.AccountGroupID == null) return false;

            return true;
        }
    }
}
