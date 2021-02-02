using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.CacheExtensions
{
    public sealed class ApAdjustExt : PXCacheExtension<APAdjust>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [AdjustmentReferenceNumberSelector]
        public string AdjdRefNbr
        {
            get;
            set;
        }

        [ChangeBillLineNumberHeaders(typeof(APInvoiceType.AdjdLineNbrAttribute))]
        public int? AdjdLineNbr
        {
            get;
            set;
        }

        [PXDecimal]
        public decimal? AmountToPayPerLine
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class adjdRefNbr : BqlString.Field<adjdRefNbr>
        {
        }

        public abstract class adjdLineNbr : BqlInt.Field<adjdLineNbr>
        {
        }

        public abstract class amountToPayPerLine : BqlDecimal.Field<amountToPayPerLine>
        {
        }
    }
}