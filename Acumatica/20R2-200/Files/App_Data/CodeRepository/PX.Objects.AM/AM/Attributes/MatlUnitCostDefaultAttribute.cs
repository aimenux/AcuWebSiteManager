using PX.Data;
using PX.Objects.IN;
using PX.Objects.PO;
using System;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Unit Cost Default value for better control on using warehouse from parent and last cost when tran cost could be zero.
    /// </summary>
    public class MatlUnitCostDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
    {
        protected PXGraph _Graph;
        protected Type _InventoryIdType;
        protected Type _SiteIdType;
        protected Type _UomType;
        protected Type _ParentType;
        protected Type _ParentSiteIdType;

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            _Graph = sender.Graph;
        }

        public MatlUnitCostDefaultAttribute(Type inventoryIdField, Type siteIdField, Type uomField)
        {
            _InventoryIdType = inventoryIdField;
            _SiteIdType = siteIdField;
            _UomType = uomField;
        }

        public MatlUnitCostDefaultAttribute(Type inventoryIdField, Type siteIdField, Type uomField, Type parent, Type parentSiteIdField) 
            : this(inventoryIdField, siteIdField, uomField)
        {
            _ParentType = parent;
            _ParentSiteIdType = parentSiteIdField;
        }

        public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = 0m;

            var inventoryId = (int?)sender.GetValue(e.Row, _InventoryIdType.Name);
            var siteId = (int?)sender.GetValue(e.Row, _SiteIdType.Name);
            var uom = (string) sender.GetValue(e.Row, _UomType.Name);

            if (e.Row == null || inventoryId == null || string.IsNullOrWhiteSpace(uom))
            {
                return;
            }

            var matlSiteId = siteId;
            if (matlSiteId == null && _ParentType != null && _ParentSiteIdType != null)
            {
                var parentRow = GetParentRow(e.Row);
                if (parentRow != null && parentRow.GetType() == _ParentType)
                {
                    matlSiteId = (int?)_Graph.Caches[_ParentType].GetValue(parentRow, _ParentSiteIdType.Name);
                }
            }

            if (matlSiteId != null)
            {
                var itemSiteUnitCost = GetINItemSiteUnitCost(inventoryId, matlSiteId, uom);
                if (itemSiteUnitCost.GetValueOrDefault() != 0)
                {
                    e.NewValue = itemSiteUnitCost;
                    return;
                }
            }

            e.NewValue = GetINItemCostUnitCost(inventoryId, uom).GetValueOrDefault();
        }

        protected virtual object GetParentRow(object childRow)
        {
            if (_ParentType == null)
            {
                return null;
            }
            return PXParentAttribute.SelectParent(_Graph.Caches[_BqlTable], childRow, _ParentType);
        }

        protected virtual decimal? GetINItemSiteUnitCost(int? inventoryID, int? siteId, string uom)
        {
            if (_Graph == null || inventoryID == null || siteId == null || string.IsNullOrWhiteSpace(uom))
            {
                return null;
            }

            var result = (PXResult<INItemSite, InventoryItem>)
                PXSelectJoin<INItemSite,
                        InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemSite.inventoryID>>>,
                        Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                            And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>
                    .Select(_Graph, inventoryID, siteId);
            var item = (InventoryItem)result;
            var itemSite = (INItemSite)result;
            if (item?.InventoryID != null && itemSite?.InventoryID != null)
            {
                var unitCostValue = itemSite.TranUnitCost.GetValueOrDefault() == 0
                    ? itemSite.LastCost.GetValueOrDefault()
                    : itemSite.TranUnitCost.GetValueOrDefault();

                return POItemCostManager.ConvertUOM(_Graph, item, item.BaseUnit, unitCostValue, uom);
            }

            return null;
        }

        protected virtual decimal? GetINItemCostUnitCost(int? inventoryID, string uom)
        {
            if (_Graph == null || inventoryID == null || string.IsNullOrWhiteSpace(uom))
            {
                return null;
            }

            var result = (PXResult<INItemCost, InventoryItem>)
                PXSelectJoin<INItemCost,
                        InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemCost.inventoryID>>>,
                        Where<INItemCost.inventoryID, Equal<Required<INItemCost.inventoryID>>>>
                    .Select(_Graph, inventoryID);
            var item = (InventoryItem)result;
            var itemCost = (INItemCost)result;
            if (item?.InventoryID != null && itemCost?.InventoryID != null)
            {
                var unitCostValue = itemCost.TranUnitCost.GetValueOrDefault() == 0
                    ? itemCost.LastCost.GetValueOrDefault()
                    : itemCost.TranUnitCost.GetValueOrDefault();

                return POItemCostManager.ConvertUOM(_Graph, item, item.BaseUnit, unitCostValue, uom);
            }

            return null;
        }
    }
}