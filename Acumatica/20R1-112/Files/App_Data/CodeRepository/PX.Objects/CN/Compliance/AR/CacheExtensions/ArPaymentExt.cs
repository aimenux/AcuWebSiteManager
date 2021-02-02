using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AR.CacheExtensions
{
    public sealed class ArPaymentExt : PXCacheExtension<ARPayment>
    {
        [PXString]
        public string ClDisplayName
        {
            get
            {
                switch (Base.DocType)
                {
                    case ARDocType.Payment:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.Payment, Base.RefNbr);
                    case ARDocType.CreditMemo:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.CreditMemo, Base.RefNbr);
                    case ARDocType.Prepayment:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.Prepayment, Base.RefNbr);
                    case ARDocType.Refund:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.Refund, Base.RefNbr);
                    case ARDocType.VoidRefund:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.VoidRefund, Base.RefNbr);
                    case ARDocType.VoidPayment:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.VoidPayment, Base.RefNbr);
                    case ARDocType.SmallBalanceWO:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.SmallBalanceWO, Base.RefNbr);
                    case ARDocType.CashSale:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.CashSale, Base.RefNbr);
                    case ARDocType.CashReturn:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.CashReturn, Base.RefNbr);
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