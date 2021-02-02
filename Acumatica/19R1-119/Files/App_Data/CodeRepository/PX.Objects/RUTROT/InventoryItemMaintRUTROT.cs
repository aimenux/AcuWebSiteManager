using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.AR;

namespace PX.Objects.RUTROT
{
    /// <summary>
    /// Classes <see cref="HiddenInventoryItem"/> and <see cref="HiddenInventoryItemRUTROT"/> will be removed in Acumatica 7.0.
    /// The class <see cref="HiddenInventoryItem"/> will be replaced with <see cref="InventoryItem"/>.
    /// The class <see cref="HiddenInventoryItemRUTROT"/> will be replaced with <see cref="InventoryItemRUTROT"/>.
    /// </summary>
    public class InventoryItemMaintRUTROT : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
        }

        #region Events
        protected virtual void HiddenInventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var item = (HiddenInventoryItem)e.Row;

            if (item == null)
            {
                return;
            }

            HiddenInventoryItemRUTROT itemRR = PXCache<HiddenInventoryItem>.GetExtension<HiddenInventoryItemRUTROT>(item);
            Branch branch = PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>>.Select(Base, PXAccess.GetBranchID());
            BranchRUTROT branchRR = RUTROTHelper.GetExtensionNullable<Branch, BranchRUTROT>(branch);

            bool allowRUTROT = branchRR?.AllowsRUTROT == true;
            bool isOtherCostOrNull = itemRR.RUTROTItemType == null || itemRR.RUTROTItemType == RUTROTItemTypes.OtherCost;

            PXUIFieldAttribute.SetVisible<HiddenInventoryItemRUTROT.rUTROTItemType>(Base.Item.Cache, item, allowRUTROT);
            PXUIFieldAttribute.SetVisible<HiddenInventoryItemRUTROT.rUTROTType>(Base.Item.Cache, item, allowRUTROT);
            PXUIFieldAttribute.SetVisible<HiddenInventoryItemRUTROT.rUTROTWorkTypeID>(Base.Item.Cache, item, allowRUTROT);

            PXUIFieldAttribute.SetEnabled<HiddenInventoryItemRUTROT.rUTROTWorkTypeID>(sender, item, !isOtherCostOrNull);
            PXUIFieldAttribute.SetEnabled<HiddenInventoryItemRUTROT.rUTROTItemType>(sender, item, true);
            PXUIFieldAttribute.SetEnabled<HiddenInventoryItemRUTROT.rUTROTType>(sender, item, true);

            if (!isOtherCostOrNull && !RUTROTHelper.IsUpToDateWorkType(itemRR.RUTROTWorkTypeID, this.Base.Accessinfo.BusinessDate ?? DateTime.Now, this.Base))
            {
                sender.RaiseExceptionHandling<HiddenInventoryItemRUTROT.rUTROTWorkTypeID>(item, itemRR.RUTROTWorkTypeID, new PXSetPropertyException(RUTROTMessages.ObsoleteWorkTypeWarning, PXErrorLevel.Warning));
            }
            else
            {
                sender.RaiseExceptionHandling<HiddenInventoryItemRUTROT.rUTROTWorkTypeID>(item, itemRR.RUTROTWorkTypeID, null);
            }
        }

        /// <summary>
        /// This event handler is a workaround for Kensium tests.
        /// This event handler will be removed in Acumatica 7.0.
        /// </summary>
        protected virtual void HiddenInventoryItem_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var item = (HiddenInventoryItem)e.Row;

            if (item == null)
            {
                return;
            }

            HiddenInventoryItemRUTROT itemRR = PXCache<HiddenInventoryItem>.GetExtension<HiddenInventoryItemRUTROT>(item);

            Base.Item.SetValueExt<InventoryItemRUTROT.rUTROTType>(Base.Item.Current, itemRR.RUTROTType);
            Base.Item.SetValueExt<InventoryItemRUTROT.rUTROTItemType>(Base.Item.Current, itemRR.RUTROTItemType);
            Base.Item.SetValueExt<InventoryItemRUTROT.rUTROTWorkTypeID>(Base.Item.Current, itemRR.RUTROTWorkTypeID);

            Base.Item.Update(Base.Item.Current);
        }

        protected virtual void HiddenInventoryItem_RUTROTItemType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (HiddenInventoryItem)e.Row;
            HiddenInventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<HiddenInventoryItem, HiddenInventoryItemRUTROT>(row);
            if (row != null && itemRR?.RUTROTItemType == RUTROTItemTypes.OtherCost)
            {
                sender.SetValueExt<HiddenInventoryItemRUTROT.rUTROTWorkTypeID>(row, null);
            }
        }

        protected virtual void HiddenInventoryItem_RUTROTType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (HiddenInventoryItem)e.Row;
            HiddenInventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<HiddenInventoryItem, HiddenInventoryItemRUTROT>(row);
            if (row != null && itemRR?.RUTROTType != (string)e.OldValue)
            {
                sender.SetValueExt<HiddenInventoryItemRUTROT.rUTROTWorkTypeID>(row, null);
            }
        }
        #endregion

        #region CacheAttached
        /// <summary>
        /// The cache attached field of the <see cref="HiddenInventoryItemRUTROT.RUTROTItemType"/> field.
        /// This cache attached field will be replaced with the cache attached field of the <see cref="InventoryItemRUTROT.RUTROTItemType"/> field in Acumatica 7.0.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRemoveBaseAttribute(typeof(RUTROTItemTypes.ListAttribute))]
        [RUTROTItemTypes.CostsList]
        public virtual void HiddenInventoryItem_RUTROTItemType_CacheAttached(PXCache sender)
        {
        }
        #endregion
    }
}