using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Inventory item field/selector with no PX restrictions for item status.
    /// There is a need to have the InventoryItemAttribute without the item status restriction.
    /// </summary>
    [PXDBInt]
    [PXUIField(DisplayName = "Inventory ID")]
    public class InventoryItemNoRestrictAttribute : AcctSubAttribute
    {
        public const string DimensionName = "INVENTORY";

        public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
        {
            public dimensionName() : base(DimensionName) {; }
        }

        public InventoryItemNoRestrictAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
        }

        public InventoryItemNoRestrictAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base()
		{
            PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, SubstituteKey);
            attr.CacheGlobal = true;
            attr.DescriptionField = DescriptionField;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }
    }

    /// <summary>
    /// Stock item field/selector with no PX restrictions for item status.
    /// There is a need to have the InventoryItemAttribute without the item status restriction.
    /// </summary>
    [PXDBInt]
    [PXRestrictor(typeof(Where<InventoryItem.stkItem, Equal<boolTrue>>), PX.Objects.IN.Messages.InventoryItemIsNotAStock)]
    [PXUIField(DisplayName = "Inventory ID")]
    public class StockItemNoRestrictAttribute : InventoryItemNoRestrictAttribute
    {

    }
}
