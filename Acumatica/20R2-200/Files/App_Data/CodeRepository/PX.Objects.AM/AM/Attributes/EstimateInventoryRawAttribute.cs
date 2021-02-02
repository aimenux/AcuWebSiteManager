using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
#if DEBUG
    // Copy of InventoryRawAttribute as of 6.10.0269 - InventoryRawAttribute is sealed
    //  + Remove substitute key from PXDimensionSelectorAttribute
    //  + set PXSelectorMode.MaskAutocomplete;
    //  + Always Skip Value validation (we are either creating a new or selecting an existing)
    //  + Updated related inventory item fields 
#endif
    /// <summary>
    /// Estimate inventory CD attribute for Estimate Inventory records.
    /// Allows the user of the CD value to support both inventory (found in Inventory Item) and non-inventory (items not yet existing)
    /// while supporting inventory segments
    /// </summary>
    [PXDBString(InputMask = "", IsUnicode = true)]
    [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
    [PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
                                And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>), PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
    public class EstimateInventoryRawAttribute : AcctSubAttribute, IPXFieldUpdatedSubscriber
    {
        public const string DimensionName = "INVENTORY";

        private Type _whereType;

 
        public EstimateInventoryRawAttribute()
			: base()
		{
            Type SearchType = typeof(Search<InventoryItem.inventoryCD, Where<Match<Current<AccessInfo.userName>>>>);
            var attr = new EstimateDimensionSelectorAttribute(DimensionName, SearchType);
            attr.CacheGlobal = true;
            attr.SelectorMode = PXSelectorMode.MaskAutocomplete;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public EstimateInventoryRawAttribute(Type WhereType)
			: this()
		{
            if (WhereType != null)
            {
                _whereType = WhereType;

                Type SearchType = BqlCommand.Compose(
                    typeof(Search<,>),
                    typeof(InventoryItem.inventoryCD),
                    typeof(Where2<,>),
                    typeof(Match<>),
                    typeof(Current<AccessInfo.userName>),
                    typeof(And<>),
                    _whereType);
                var attr = new EstimateDimensionSelectorAttribute(DimensionName, SearchType);
                attr.CacheGlobal = true;
                attr.SelectorMode = PXSelectorMode.MaskAutocomplete;
                _Attributes[_SelAttrIndex] = attr;
            }
        }

        /// <summary>
        /// Validate the entered dimension
        /// </summary>
        protected void DimensionAttributeFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            GetDimensionAttribute(sender, e.Row, nameof(IEstimateInventory.InventoryCD))?.FieldVerifying(sender, e);
        }

        /// <summary>
        /// Get the PXDimensionAttribute instance on the given field name
        /// </summary>
        private static PXDimensionAttribute GetDimensionAttribute(PXCache cache, object data, string fieldName)
        {
            return Common.Cache.GetFirstAttributeOfType<PXDimensionAttribute>(cache, data, fieldName);
        }

        public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var estimateInventory = e.Row as IEstimateInventory;
            if (estimateInventory == null)
            {
                return;
            }

            var inventoryItem = FindByInventoryCD(sender.Graph, estimateInventory.InventoryCD);
            sender.SetValueExt(e.Row, nameof(IEstimateInventory.IsNonInventory), inventoryItem == null);
            sender.SetValueExt(e.Row, nameof(IEstimateInventory.InventoryID), inventoryItem?.InventoryID);

            if (inventoryItem != null)
            {
                sender.SetValueExt(e.Row, nameof(IEstimateInventory.ItemDesc), inventoryItem.Descr);
                sender.SetValueExt(e.Row, nameof(IEstimateInventory.UOM), inventoryItem.BaseUnit);
                sender.SetValueExt(e.Row, nameof(IEstimateInventory.ItemClassID), inventoryItem.ItemClassID);
                SetReferenceTaxCategory(sender.Graph, estimateInventory, inventoryItem);
            }
        }

        /// <summary>
        /// Estimate tax category is on the reference record - set from the given Inventory Item record as needed
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="estimateInventory"></param>
        /// <param name="inventoryItem"></param>
        protected virtual void SetReferenceTaxCategory(PXGraph graph, IEstimateInventory estimateInventory, InventoryItem inventoryItem)
        {
            if (estimateInventory is AMEstimateItem && graph is EstimateMaint
                && ((EstimateMaint)graph).EstimateReferenceRecord.Current != null && !string.IsNullOrWhiteSpace(inventoryItem.TaxCategoryID))
            {
                ((EstimateMaint)graph).EstimateReferenceRecord.Cache.SetValue<AMEstimateReference.taxCategoryID>(((EstimateMaint)graph).EstimateReferenceRecord.Current, inventoryItem.TaxCategoryID);
            }
        }

        public static int InventoryDimensionLength()
        {
            return PXDimensionAttribute.GetLength(DimensionName);
        }

        public static string GetDisplayMaskValue<TField>(PXCache cache, object row, string value) where TField : IBqlField
        {
            var displayValue = cache.GetStateExt<TField>(row) as PXSegmentedState;

            if (displayValue == null)
            {
                return null;
            }

            var maskNoSeperator = RemoveMaskSeparators(displayValue.Segments, displayValue.InputMask);
            return PX.Common.Mask.Parse(maskNoSeperator, value);
        }

        /// <summary>
        /// Remove the mask separators to get the value without the display added separators
        /// </summary>
        /// <param name="segments">Segments related to the INVENTORY dimension</param>
        /// <param name="mask">Field mask</param>
        /// <returns>mask less separators</returns>
        protected static string RemoveMaskSeparators(PXSegment[] segments, string mask)
        {
            if (segments.Length <= 1)
            {
                return mask;
            }

            var sb = new System.Text.StringBuilder('>');
            int position = 1;
            for (int i = 0; i < segments.Length; i++)
            {
                sb.Append(mask.Substring(position, segments[i].Length));
                position += segments[i].Length + (i < segments.Length ? 1 : 0);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Does the given CD value currently exist in Inventory
        /// </summary>
        public static bool IsExistingInventoryCD(PXGraph graph, string inventoryCD)
        {
            return FindByInventoryCD(graph, inventoryCD) != null;
        }

        protected InventoryItem FindByInventoryCDSelector(PXCache cache, object row)
        {
            return (InventoryItem)PXSelectorAttribute.Select(cache, row, nameof(IEstimateInventory.InventoryCD));
        }

        /// <summary>
        /// Find/Search for the inventory record by CD value
        /// </summary>
        public static InventoryItem FindByInventoryCD(PXGraph graph, string inventoryCD)
        {
            return PXSelect<InventoryItem, 
                Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>.Select(graph, inventoryCD);
        }

        /// <summary>
        /// Is the given dimension valid
        /// </summary>
        public static bool IsDimensionValid<TField>(PXCache sender, String value)
        {
            return PXDimensionAttribute.MatchMask<TField>(sender, value);
        }

        /// <summary>
        /// PXDimensionSelectorAttribute with the selector validate value as false
        /// </summary>
        public class EstimateDimensionSelectorAttribute : PXDimensionSelectorAttribute
        {
            public EstimateDimensionSelectorAttribute(string dimension, Type type) : base(dimension, type)
            {
                for (int i = _Attributes.Count - 1; i >= 0; i--)
                {
                    if (_Attributes[i] is PXSelectorAttribute)
                    {
                        ((PXSelectorAttribute)_Attributes[i]).ValidateValue = false;
                        return;
                    }
                }
            }
        }
    }
}