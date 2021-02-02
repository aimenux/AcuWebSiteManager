using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using static PX.Objects.PO.POCreate;

namespace PX.Objects.FS
{
    public class FSxPOCreateFilter : PXCacheExtension<POCreateFilter>
	{
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.IBqlField
        {
        }

        [PXString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorSrvOrdType]
        [PX.Data.EP.PXFieldDescription]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region ServiceOrderRefNbr
        public abstract class serviceOrderRefNbr : PX.Data.IBqlField
        {
        }

        [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Service Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search2<FSServiceOrder.refNbr,
                               LeftJoin<BAccountSelectorBase,
                                    On<BAccountSelectorBase.bAccountID, Equal<FSServiceOrder.customerID>>,
                               LeftJoin<Location,
                                    On<Location.locationID, Equal<FSServiceOrder.locationID>>>>,
                               Where<
                                    FSServiceOrder.srvOrdType, Equal<Current<FSxPOCreateFilter.srvOrdType>>>,
                               OrderBy<
                                    Desc<FSServiceOrder.refNbr>>>))]
        public virtual string ServiceOrderRefNbr { get; set; }
        #endregion
    }
}
