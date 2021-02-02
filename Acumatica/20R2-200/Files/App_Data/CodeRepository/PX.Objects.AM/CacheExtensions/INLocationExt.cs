using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class INLocationExt : PXCacheExtension<INLocation>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingMRP>();
        }

        #region AMMRPFlag
        public abstract class aMMRPFlag : PX.Data.BQL.BqlBool.Field<aMMRPFlag> { }
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "MRP")]
        public Boolean? AMMRPFlag { get; set; }
        #endregion
    }
}