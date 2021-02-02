using System.Collections;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.GraphExtensions
{
    public class AccountByPeriodEnqAMExtension : PXGraphExtension<AccountByPeriodEnq>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public PXAction<AccountByPeriodFilter> ViewDocument;
        [PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable viewDocument(PXAdapter adapter)
        {
            GLTranR tran = Base.GLTranEnq.Current;
            if (tran != null && tran.OrigModule == Common.ModuleAM && tran.Module == BatchModule.GL)
            {
                // This is a manufactured created entry...
                AMDocType.DocTypeRedirectRequiredException(tran.TranType, tran.OrigBatchNbr, Base);
                return adapter.Get();
            }
            return Base.viewDocument(adapter);
        }
    }
}
