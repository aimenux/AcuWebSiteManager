using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AR.CacheExtensions
{
    public sealed class ArInvoiceExt : PXCacheExtension<ARInvoice>
    {
        [PXString]
        public string ClDisplayName
        {
            get
            {
                switch (Base.DocType)
                {
                    case ARDocType.Invoice:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.Invoice, Base.RefNbr);
                    case ARDocType.DebitMemo:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.DebitMemo, Base.RefNbr);
                    case ARDocType.CreditMemo:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.CreditMemo, Base.RefNbr);
                    case ARDocType.FinCharge:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.FinCharge, Base.RefNbr);
                    case ARDocType.SmallCreditWO:
                        return string.Format("{0}, {1}", PX.Objects.AR.Messages.SmallCreditWO, Base.RefNbr);
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