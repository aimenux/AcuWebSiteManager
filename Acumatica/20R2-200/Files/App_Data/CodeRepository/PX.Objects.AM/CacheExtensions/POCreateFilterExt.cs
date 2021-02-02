using System;
using PX.Data;
using PX.Objects.PO;
using PX.Objects.AM.Attributes;
using PX.Objects.CS;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing cache extension for <see cref="POCreate.POCreateFilter"/>
    /// </summary>
    [Serializable]
    public sealed class POCreateFilterExt : PXCacheExtension<POCreate.POCreateFilter>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
        }

        #region OrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        [PXSelector(typeof(Search<AMOrderType.orderType>))]
        [AMOrderTypeField(DisplayName = "Production Order Type")]
        public String AMOrderType { get; set; }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        [PXSelector(typeof(Search<AMProdItem.prodOrdID>))]
        [ProductionNbr]
        public String ProdOrdID { get; set; }
        #endregion
    }
}