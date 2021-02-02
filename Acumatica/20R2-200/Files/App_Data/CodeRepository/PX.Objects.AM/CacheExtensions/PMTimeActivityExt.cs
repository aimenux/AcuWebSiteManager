using PX.Data;
using PX.Objects.CR;
using System;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class PMTimeActivityExt : PXCacheExtension<PMTimeActivity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMIsProd
        /// <summary>
        /// Indicates a production related time activity
        /// </summary>
        public abstract class aMIsProd : PX.Data.BQL.BqlBool.Field<aMIsProd> { }

        /// <summary>
        /// Indicates a production related time activity
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]        
        public Boolean? AMIsProd { get; set; }
        #endregion
    }
}
