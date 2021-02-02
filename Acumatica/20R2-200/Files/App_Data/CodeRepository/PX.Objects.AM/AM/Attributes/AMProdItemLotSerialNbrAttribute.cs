using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Production Item lot/serial number attribute
    /// </summary>
    public class AMProdItemLotSerialNbrAttribute : INLotSerialNbrAttribute
    {
        public AMProdItemLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType) : base(InventoryType, SubItemType, LocationType)
        {
        }

        public AMProdItemLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType) : base(InventoryType, SubItemType, LocationType, ParentLotSerialNbrType)
        {
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row != null)
            {
                PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
                ((PXUIFieldAttribute)_Attributes[_UIAttrIndex]).Enabled = false;
            }
        }
    }
}
