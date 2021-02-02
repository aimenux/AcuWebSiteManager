using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    [PXTable(typeof(INItemClass.itemClassID), IsOptional = true)]
    public class FSxEquipmentModelTemplate : PXCacheExtension<INItemClass>
	{
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();
        }

        #region DfltModelType
        public abstract class dfltModelType : ListField_ModelType
		{
		}

		[PXDBString(2, IsFixed = true)]
        [ListField_ModelType.ListAtrribute]
        [PXDefault(ID.ModelType.EQUIPMENT, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Model Type")]
        public virtual string DfltModelType { get; set; }
		#endregion
		#region EQEnabled
		public abstract class eQEnabled : PX.Data.BQL.BqlBool.Field<eQEnabled> { }

		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Model Equipment Class", Visible = false)]
        public virtual bool? EQEnabled { get; set; }
        #endregion
        #region EquipmentItemClass
        public abstract class equipmentItemClass : ListField_EquipmentItemClass
        {
        }

        protected string _EquipmentItemClass;
        [PXDBString(2, IsFixed = true)]
        [equipmentItemClass.ListAtrribute]
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