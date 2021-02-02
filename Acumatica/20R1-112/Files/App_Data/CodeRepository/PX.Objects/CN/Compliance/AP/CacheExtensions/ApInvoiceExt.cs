using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.CacheExtensions
{
    public sealed class ApInvoiceExt : PXCacheExtension<APInvoice>
    {
        [PXString]
        public string ClDisplayName
        {
            get 
            {
                switch (Base.DocType)
                {
                    case APDocType.Invoice:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.Invoice, Base.RefNbr);
                    case APDocType.DebitAdj:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.DebitAdj, Base.RefNbr);
                    case APDocType.CreditAdj:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.CreditAdj, Base.RefNbr);
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