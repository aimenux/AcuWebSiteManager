using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CN.Subcontracts.PM.CacheExtensions;
using PX.Objects.CN.Subcontracts.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PM.GraphExtensions
{
    public class CommitmentInquiryExt : PXGraphExtension<CommitmentInquiry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public IEnumerable items()
        {
            var helper = new EntityHelper(Base);
            var relatedDocumentType = GetRelatedDocumentType();
            foreach (var commitment in GetCommitments())
            {
                var documentTypeName = GetDocumentTypeName(commitment, helper);
                PXCache<PMCommitment>.GetExtension<PmCommitmentExt>(commitment).RelatedDocumentType =
                    documentTypeName;
                if (relatedDocumentType == RelatedDocumentType.AllCommitmentsType ||
                    relatedDocumentType == documentTypeName)
                {
                    yield return commitment;
                }
            }
        }

        private string GetRelatedDocumentType()
        {
            return PXCache<CommitmentInquiry.ProjectBalanceFilter>
                .GetExtension<ProjectBalanceFilterExt>(Base.Filter.Current).RelatedDocumentType;
        }

        private static string GetDocumentTypeName(PMCommitment commitment, EntityHelper helper)
        {
            var documentTypeName = helper.GetEntityRowType(commitment.RefNoteID)?.Name;
            if (documentTypeName == RelatedDocumentType.PurchaseOrderType)
            {
                POOrder purchaseOrder;
                using (new PXReadBranchRestrictedScope())
                {
                    purchaseOrder = (POOrder) helper.GetEntityRow(commitment.RefNoteID);
                }
                if (purchaseOrder.OrderType == POOrderType.RegularSubcontract)
                {
                    return RelatedDocumentType.SubcontractType;
                }
            }
            return documentTypeName;
        }

        private IEnumerable<PMCommitment> GetCommitments()
        {
            var view = new PXView(Base, true, Base.Items.View.BqlSelect);
            return view.SelectMulti().RowCast<PMCommitment>();
        }
    }
}