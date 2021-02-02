using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class PMTaskExt : PXCacheExtension<PMTask>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region VisibleInPROD
        public abstract class visibleInPROD : PX.Data.BQL.BqlBool.Field<visibleInPROD> { }

        [PXDBBool]
        [PXDefault(typeof(Search<PMSetupExt.visibleInPROD>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "PROD")]
        public Boolean? VisibleInPROD { get; set; }
        #endregion
    }
}
