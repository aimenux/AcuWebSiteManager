using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Production Material lot/serial number attribute
    /// </summary>
    public class AMProdMatlLotSerialNbrAttribute : INLotSerialNbrAttribute
    {
        public AMProdMatlLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType) : base(InventoryType, SubItemType, LocationType)
        {
        }

        public AMProdMatlLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType) : base(InventoryType, SubItemType, LocationType, ParentLotSerialNbrType)
        {
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            AMProdMatlSplit split = e.Row as AMProdMatlSplit;

            if (split != null)
            {
                bool isAllocated = split.IsAllocated == true;

                PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
                ((PXUIFieldAttribute)_Attributes[_UIAttrIndex]).Enabled = isAllocated;
            }
        }
    }
}

