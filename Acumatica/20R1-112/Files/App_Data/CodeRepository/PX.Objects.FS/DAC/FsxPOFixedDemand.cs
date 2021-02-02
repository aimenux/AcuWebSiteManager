using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.FS
{
    public class FSxPOFixedDemand : PXCacheExtension<POFixedDemand>
	{
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        // Delete this in the next major upgrade
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.IBqlField
        {
        }

        [PXString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type", FieldClass = "SERVICEMANAGEMENT")]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region FSRefNbr
        public abstract class fsRefNbr : PX.Data.IBqlField
        {
        }

        [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Service Order Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        [PXSelector(typeof(Search<FSServiceOrder.refNbr>))]
        public virtual string FSRefNbr { get; set; }
        #endregion
    }
}
