using PX.Data;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.PO.CacheExtensions
{
    public sealed class PoOrderExt : PXCacheExtension<POOrder>
    {
        [PXString]
        public string ClDisplayName
        {
            get
            {
                switch (Base.OrderType)
                {
                    case POOrderType.RegularOrder:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.RegularOrder, Base.OrderNbr);
                    case POOrderType.RegularSubcontract:
                        return Base.OrderNbr;
                    case POOrderType.DropShip:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.DropShip, Base.OrderNbr);
                    case POOrderType.Blanket:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.Blanket, Base.OrderNbr);
                    case POOrderType.StandardBlanket:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.StandardBlanket, Base.OrderNbr);

                }

                return string.Format("{0}, {1}", Base.OrderType, Base.OrderNbr);
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