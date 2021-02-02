using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing BOM ID PX Selector Attribute for all non archived BOMs
    /// </summary>
    public class BOMIDSelectorAttribute : PXSelectorAttribute
    {
        /// <summary>
        /// Display full list of unique BOM IDS
        /// </summary>
        public BOMIDSelectorAttribute()
            : base(typeof(Search2<AMBomItemActive.bOMID,
                LeftJoin<InventoryItem, On<AMBomItemActive.inventoryID, Equal<InventoryItem.inventoryID>>>>), ColumnList)
        {
            _DescriptionField = typeof(AMBomItemActive.descr);
        }

        /// <summary>
        /// BOMID attribute with a search type and predefined column list
        /// </summary>
        /// <param name="searchType"></param>
        public BOMIDSelectorAttribute(Type searchType)
            : base(searchType, ColumnList)
        {
            _DescriptionField = typeof(AMBomItemActive.descr);
        }

        private static Type[] ColumnList => new Type[]
        {
            typeof(AMBomItemActive.bOMID), typeof(AMBomItemActive.revisionID), typeof(AMBomItemActive.inventoryID),
            typeof(AMBomItemActive.subItemID), typeof(AMBomItemActive.siteID), typeof(AMBomItemActive.descr),
            typeof(InventoryItem.itemClassID), typeof(InventoryItem.descr)
        };
    }

    /// <summary>
    /// Manufacturing BOM ID PX Selector Attribute for all Active BOMs
    /// </summary>
    public class BOMIDActiveSelectorAttribute : PXSelectorAttribute
    {
        /// <summary>
        /// Display full list of unique BOM IDS
        /// </summary>
        public BOMIDActiveSelectorAttribute()
            : base(typeof(Search2<AMBomItemActive2.bOMID,
                LeftJoin<InventoryItem, On<AMBomItemActive2.inventoryID, Equal<InventoryItem.inventoryID>>>>), ColumnList)
        {
            _DescriptionField = typeof(AMBomItemActive2.descr);
        }

        /// <summary>
        /// BOMID attribute with a search type and predefined column list
        /// </summary>
        /// <param name="searchType"></param>
        public BOMIDActiveSelectorAttribute(Type searchType)
            : base(searchType, ColumnList)
        {
            _DescriptionField = typeof(AMBomItemActive2.descr);
        }

        private static Type[] ColumnList => new Type[]
        {
            typeof(AMBomItemActive2.bOMID), typeof(AMBomItemActive2.revisionID), typeof(AMBomItemActive2.inventoryID),
            typeof(AMBomItemActive2.subItemID), typeof(AMBomItemActive2.siteID), typeof(AMBomItemActive2.descr),
            typeof(InventoryItem.itemClassID), typeof(InventoryItem.descr)
        };
    }
}
