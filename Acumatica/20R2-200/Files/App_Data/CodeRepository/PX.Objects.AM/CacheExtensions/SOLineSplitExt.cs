using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class SOLineSplitExt : PXCacheExtension<SOLineSplit>
    {
        // Developer note: new fields added here should also be added to SOLineSplitMfgOnly
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMProdCreate
        public abstract class aMProdCreate : PX.Data.BQL.BqlBool.Field<aMProdCreate> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
#if DEBUG
        // Using AMProdStatus as a flag for records created from Move transactions
#endif
        [PXFormula(typeof(Switch<Case<Where<SOLineSplit.isAllocated, Equal<False>, And<SOLineSplitExt.aMProdStatusID, IsNull>>, Current<SOLineExt.aMProdCreate>>, False>))]
        [PXUIField(DisplayName = "Mark for Production", Enabled = false)]
        public Boolean? AMProdCreate { get; set; }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXUIField(DisplayName = "Prod. Order Type", Enabled = false)]
        public string AMOrderType { get; set; }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        [ProductionNbr]
        public string AMProdOrdID { get; set; }
        #endregion
        #region AMProdQtyComplete
        public abstract class aMProdQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdQtyComplete> { }
        [PXDBQuantity]
        [PXUIField(DisplayName = "Production Qty Complete", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMProdQtyComplete { get; set; }
        #endregion
        #region AMProdBaseQtyComplete
        public abstract class aMProdBaseQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdBaseQtyComplete> { }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Production Base Qty Complete", Enabled = false, Visible = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMProdBaseQtyComplete { get; set; }
        #endregion
        #region AMProdStatusID
        public abstract class aMProdStatusID : PX.Data.BQL.BqlString.Field<aMProdStatusID> { }

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Production Status", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        [ProductionOrderStatus.List]
        public String AMProdStatusID { get; set; }
        #endregion
    }
}