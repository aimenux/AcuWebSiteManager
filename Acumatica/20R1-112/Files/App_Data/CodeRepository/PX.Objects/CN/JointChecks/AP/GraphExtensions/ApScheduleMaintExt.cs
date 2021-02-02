using PX.Data;
using PX.Objects.AP;
using PX.Objects.AP.BQL;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions
{
    public class ApScheduleMaintExt : PXGraphExtension<APScheduleMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
        [PXSelector(typeof(Search2<APRegister.refNbr,
                LeftJoin<APInvoice, On<APRegister.docType, Equal<APInvoice.docType>,
                    And<APRegister.refNbr, Equal<APInvoice.refNbr>>>>,
                Where<APRegister.docType, Equal<Optional<APRegister.docType>>,
                    And<APInvoiceJCExt.isJointPayees, NotEqual<True>,
                    And<IsSchedulable<APRegister>>>>>),
            typeof(APRegister.finPeriodID),
            typeof(APRegister.refNbr),
            typeof(APRegister.vendorID),
            typeof(APRegister.vendorLocationID),
            typeof(APRegister.status),
            typeof(APRegister.curyID),
            typeof(APRegister.curyOrigDocAmt))]
        protected virtual void DocumentSelection_RefNbr_CacheAttached(PXCache cache)
        {
        }
    }
}
