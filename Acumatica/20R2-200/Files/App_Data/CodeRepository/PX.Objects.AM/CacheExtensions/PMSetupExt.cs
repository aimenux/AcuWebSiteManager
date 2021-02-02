using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class PMSetupExt : PXCacheExtension<PMSetup>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region VisibleInPROD
        public abstract class visibleInPROD : PX.Data.BQL.BqlBool.Field<visibleInPROD> { }

        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "PROD")]
        public Boolean? VisibleInPROD { get; set; }
        #endregion
    }
}
