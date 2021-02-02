using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class INTranExt : PXCacheExtension<INTran>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMDocType

        public abstract class aMDocType : PX.Data.BQL.BqlString.Field<aMDocType> { }

        [PXDBString(1, IsFixed = true)]
        [AM.Attributes.AMDocType.List]
        [PXUIField(DisplayName = "MFG Doc. Type", Visible = false, Enabled = false)]
        public String AMDocType { get; set; }
        #endregion
        #region AMBatNbr
        public abstract class aMBatNbr : PX.Data.BQL.BqlString.Field<aMBatNbr> { }

        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "MFG Batch Nbr", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<AM.AMBatch.batNbr, Where<AM.AMBatch.docType, Equal<Current<INTranExt.aMDocType>>>>), ValidateValue = false)]
        public String AMBatNbr { get; set; }
        #endregion
        #region AMLineNbr
        public abstract class aMLineNbr : PX.Data.BQL.BqlInt.Field<aMLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "MFG Line Nbr.", Visible = false, Enabled = false)]
        public Int32? AMLineNbr { get; set; }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXUIField(DisplayName = "Prod. Order Type", Enabled = false)]
        public string AMOrderType { get; set; }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        [ProductionNbr(Enabled = false)]
        [ProductionOrderSelector(typeof(INTranExt.aMOrderType), includeAll: true, ValidateValue = false)]
        public String AMProdOrdID { get; set; }
        #endregion
    }
}
