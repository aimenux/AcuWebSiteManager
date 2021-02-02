using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class PMProjectExt : PXCacheExtension<PMProject>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region VisibleInPROD
        public abstract class visibleInPROD : PX.Data.BQL.BqlBool.Field<visibleInPROD> { }
        protected Boolean? _VisibleInPROD;

        [PXDBBool]
        [PXDefault(typeof(Search<PMSetupExt.visibleInPROD>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "PROD")]
        public Boolean? VisibleInPROD
        {
            get => this._VisibleInPROD ?? true;
            set => this._VisibleInPROD = value;
        }
        #endregion
    }
}