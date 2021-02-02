using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class INRegisterExt : PXCacheExtension<INRegister>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMDocType

        public abstract class aMDocType : PX.Data.BQL.BqlString.Field<aMDocType> { }

        [PXDBString(1, IsFixed = true)]
        [AM.Attributes.AMDocType.List]
        [PXUIField(DisplayName = "MFG Document Type", Visible = true, Enabled = false)]
        public String AMDocType { get; set; }
        #endregion
        #region AMBatNbr
        public abstract class aMBatNbr : PX.Data.BQL.BqlString.Field<aMBatNbr> { }

        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "MFG Batch Nbr", Visible = true, Enabled = false)]
        [PXSelector(typeof(Search<AM.AMBatch.batNbr, Where<AM.AMBatch.docType, Equal<Current<INRegisterExt.aMDocType>>>>), ValidateValue = false)]
        public String AMBatNbr { get; set; }
        #endregion
    }
}
