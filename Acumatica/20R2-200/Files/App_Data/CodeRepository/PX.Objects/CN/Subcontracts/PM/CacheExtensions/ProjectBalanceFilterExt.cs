using PX.Data;
using PX.Objects.CN.Subcontracts.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using Messages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.CacheExtensions
{
    public sealed class ProjectBalanceFilterExt : PXCacheExtension<CommitmentInquiry.ProjectBalanceFilter>
    {
        [PXString]
        [PXUIField(DisplayName = Messages.PmCommitment.RelatedDocumentType)]
        [PXDefault(Descriptor.RelatedDocumentType.AllCommitmentsType)]
        [RelatedDocumentType.List]
        public string RelatedDocumentType
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class relatedDocumentType : IBqlField
        {
        }
    }
}