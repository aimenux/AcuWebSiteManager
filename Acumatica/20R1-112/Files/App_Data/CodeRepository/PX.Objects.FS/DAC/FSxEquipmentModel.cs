using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    [PXTable(typeof(InventoryItem.inventoryID), IsOptional = true)]
    public class FSxEquipmentModel : PXCacheExtension<InventoryItem>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();
        }

        #region EQEnabled
        public abstract class eQEnabled : PX.Data.BQL.BqlBool.Field<eQEnabled> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Model Equipment")]
        public virtual bool? EQEnabled { get; set; }
        #endregion
        #region ManufacturerID
        public abstract class manufacturerID : PX.Data.BQL.BqlInt.Field<manufacturerID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Manufacturer")]
        [PXSelector(typeof(FSManufacturer.manufacturerID),
                        SubstituteKey = typeof(FSManufacturer.manufacturerCD),
                        DescriptionField = typeof(FSManufacturer.descr))]
        public virtual int? ManufacturerID { get; set; }
        #endregion
        #region ManufacturerModelID
        public abstract class manufacturerModelID : PX.Data.BQL.BqlInt.Field<manufacturerModelID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Manufacturer Model", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(
                            Search<FSManufacturerModel.manufacturerModelID,
                            Where<
                                FSManufacturerModel.manufacturerID, Equal<Current<manufacturerID>>>>),
                           SubstituteKey = typeof(FSManufacturerModel.manufacturerModelCD),
                           DescriptionField = typeof(FSManufacturerModel.descr))]
        [PXFormula(typeof(Default<manufacturerID>))]
        public virtual int? ManufacturerModelID { get; set; }
        #endregion
        #region ModelType
        public abstract class modelType : ListField_ModelType
        {
        }

        [PXDBString(2, IsFixed = true)]
        [ListField_ModelType.ListAtrribute]
        [PXDefault(ID.ModelType.EQUIPMENT, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Model Type")]
        public virtual string ModelType { get; set; }
        #endregion
        #region EquipmentItemClass
        public abstract class equipmentItemClass : PX.Data.BQL.BqlString.Field<equipmentItemClass> { }

        protected string _EquipmentItemClass;
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Equipment_Item_Class.PART_OTHER_INVENTORY)]
        [PXUIField(DisplayName = "Equipment Class")]
        public virtual string EquipmentItemClass
        {
            get
            {
                return this._EquipmentItemClass;
            }

            set
            {
                this._EquipmentItemClass = value;
                if (this._EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT
                    || this._EquipmentItemClass == ID.Equipment_Item_Class.COMPONENT)
                {
                    EQEnabled = true;
                }
                else
                {
                    EQEnabled = false;
                }
            }
        }
        #endregion
        #region EquipmentTypeID
        public abstract class equipmentTypeID : PX.Data.BQL.BqlInt.Field<equipmentTypeID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Equipment Type")]
        [FSSelectorEquipmentType]
        public virtual int? EquipmentTypeID { get; set; }
        #endregion
        #region CpnyWarrantyValue
        public abstract class cpnyWarrantyValue : PX.Data.BQL.BqlInt.Field<cpnyWarrantyValue> { }

        [PXDBInt(MinValue = 0)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Company Warranty")]
        public virtual int? CpnyWarrantyValue { get; set; }
        #endregion
        #region CpnyWarrantyType
        public abstract class cpnyWarrantyType : ListField_WarrantyDurationType
        {
        }

        [PXDBString(1, IsFixed = true)]
        [cpnyWarrantyType.ListAtrribute]
        [PXDefault(ID.WarrantyDurationType.MONTH, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Company Warranty Type")]
        public virtual string CpnyWarrantyType { get; set; }
        #endregion
        #region VendorWarrantyValue
        public abstract class vendorWarrantyValue : PX.Data.BQL.BqlInt.Field<vendorWarrantyValue> { }

        [PXDBInt(MinValue = 0)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Vendor Warranty")]
        public virtual int? VendorWarrantyValue { get; set; }
        #endregion
        #region VendorWarrantyType
        public abstract class vendorWarrantyType : ListField_WarrantyDurationType
        {
        }

        [PXDBString(1, IsFixed = true)]
        [vendorWarrantyType.ListAtrribute]
        [PXDefault(ID.WarrantyDurationType.MONTH, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Vendor Warranty Type")]
        public virtual string VendorWarrantyType { get; set; }
        #endregion
        #region ChkEquipmentManagement
        public abstract class ChkEquipmentManagement : PX.Data.BQL.BqlBool.Field<ChkEquipmentManagement> { }

        [PXBool]
        [PXUIField(Visible = false)]
        public virtual bool? chkEquipmentManagement
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Memory Fields
        #region Mem_ShowComponent
        // This memory field exists to show the Component tab according to the values of the screen
        public abstract class mem_ShowComponent : PX.Data.BQL.BqlBool.Field<mem_ShowComponent> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual bool Mem_ShowComponent { get; set; }
        #endregion
        #endregion
	}
}
