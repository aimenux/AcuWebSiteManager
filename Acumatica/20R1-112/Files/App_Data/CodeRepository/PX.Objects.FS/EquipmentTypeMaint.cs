using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class EquipmentTypeMaint : PXGraph<EquipmentTypeMaint, FSEquipmentType>
    {
        [PXImport(typeof(FSEquipmentType))]
        public PXSelect<FSEquipmentType> EquipmentTypeRecords;

        [PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<FSEquipmentType, FSEquipment> Mapping;

        public PXSelect<FSEquipmentType,
               Where<
                   FSEquipmentType.equipmentTypeID, Equal<Current<FSEquipmentType.equipmentTypeID>>>>
               CurrentEquipmentTypeRecord;

        #region CacheAttached
        #region EntityClassID
        [PXDBString(15, IsUnicode = true, IsKey = true, IsFixed = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Entity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }
}
