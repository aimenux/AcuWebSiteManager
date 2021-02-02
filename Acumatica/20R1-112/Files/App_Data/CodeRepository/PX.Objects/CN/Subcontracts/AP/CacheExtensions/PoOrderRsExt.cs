using PX.Data;
using PX.Data.BQL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class PoOrderRsExt : PXCacheExtension<POOrderRS>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		public abstract class subcontractNbr : BqlString.Field<subcontractNbr>
	    {
	    }

		[PXDBString(BqlField = typeof(POOrder.orderNbr))]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractNumber)]
        public string SubcontractNbr
        {
            get;
            set;
        }

		public abstract class subcontractTotal : BqlDecimal.Field<subcontractTotal>
		{
		}

		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.orderTotal), BqlField = typeof(POOrder.curyOrderTotal))]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractTotal, Enabled = false)]
        public decimal? SubcontractTotal
        {
            get;
            set;
        }

        public abstract class projectCD : BqlString.Field<subcontractNbr>
        {
        }

		[PXString]
        [PXUIField(DisplayName = Messages.Subcontract.Project)]
        public string ProjectCD
        {
            get;
            set;
        }
    }
}