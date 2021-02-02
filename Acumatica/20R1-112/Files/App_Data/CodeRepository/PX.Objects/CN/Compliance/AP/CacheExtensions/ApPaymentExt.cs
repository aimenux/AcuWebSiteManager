using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.CacheExtensions
{
    public sealed class ApPaymentExt : PXCacheExtension<APPayment>
    {
        [PXString]
        public string ClDisplayName
        {
            get
            {
                switch (Base.DocType)
                {
                    case APDocType.Check:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.Check, Base.RefNbr);
                    case APDocType.DebitAdj:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.DebitAdj, Base.RefNbr);
                    case APDocType.Prepayment:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.Prepayment, Base.RefNbr);
                    case APDocType.Refund:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.Refund, Base.RefNbr);
                    case APDocType.VoidRefund:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.VoidRefund, Base.RefNbr);
                    case APDocType.VoidCheck:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.VoidCheck, Base.RefNbr);
                }

                return string.Format("{0}, {1}", Base.DocType, Base.RefNbr);
            }
            set { }
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class clDisplayName : IBqlField
        {
        }
    }
}