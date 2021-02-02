using PX.Data;
using PX.Objects.CN.Common.Utilities;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.Subcontracts.CR.CacheExtensions
{
    public sealed class CrActivityExt : PXCacheExtension<CRActivity>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXFormula(typeof(RelatedEntityDescription<CRActivity.refNoteID>))]
        public string Source
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
    }
}
