using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    public class SM_InventoryItemMaint : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Cache Attached
        #region InventoryItem_BillingRule
        [Obsolete("Remove in 2020R2")]
        protected virtual void InventoryItem_BillingRule_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Selects
        public PXSelect<FSModelComponent, 
               Where<
                   FSModelComponent.modelID, Equal<Current<InventoryItem.inventoryID>>>>
               ModelComponents;
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Manages the <c>SetEnabled</c> attribute for the <c>eQEnabled</c>, <c>manufacturerID</c>, <c>modelType</c> and <c>hasWarranty</c> fields of the <c>FSxEquipmentModel</c> DAC.
        /// </summary>
        public virtual void EnableDisable_InventoryItem(PXCache cache, InventoryItem inventoryItemRow)
        {
            bool enableGridActions = false;

            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == true)
            {
                FSxEquipmentModel fsxEquipmentModelRow = cache.GetExtension<FSxEquipmentModel>(inventoryItemRow);
                bool enableHeaderWarranties = fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT ||
                                                fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.COMPONENT;
                enableGridActions = fsxEquipmentModelRow.EQEnabled == true && fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT;

                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.eQEnabled>(cache, inventoryItemRow, false);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.manufacturerID>(cache, 
                                                                                inventoryItemRow, 
                                                                                fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT ||
                                                                                fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.COMPONENT);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.manufacturerModelID>(cache, 
                                                                                     inventoryItemRow, 
                                                                                     fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT ||
                                                                                     fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.COMPONENT);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.equipmentTypeID>(cache,
                                                                                 inventoryItemRow,
                                                                                 fsxEquipmentModelRow.EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT);

                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.cpnyWarrantyType>(cache, inventoryItemRow, enableHeaderWarranties);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.cpnyWarrantyValue>(cache, inventoryItemRow, enableHeaderWarranties);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.vendorWarrantyType>(cache, inventoryItemRow, enableHeaderWarranties);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.vendorWarrantyValue>(cache, inventoryItemRow, enableHeaderWarranties);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.modelType>(cache, inventoryItemRow, false);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModel.equipmentItemClass>(cache, inventoryItemRow, false);

                fsxEquipmentModelRow.Mem_ShowComponent = enableGridActions;
            }

            ModelComponents.Cache.AllowInsert = enableGridActions;
            ModelComponents.Cache.AllowUpdate = enableGridActions;
            ModelComponents.Cache.AllowDelete = enableGridActions;

            if (enableGridActions == false)
            {
                ModelComponents.Cache.Clear();
            }
        }

        /// <summary>
        /// Manages the <c>SetEnabled</c> attribute for the <c>componentCD</c>, <c>descr</c>, <c>vendorWarrantyDuration</c>, <c>vendorID</c> and <c>cpnyWarrantyDuration</c> fields of the <c>FSModelComponent</c> DAC.
        /// </summary>
        public virtual void EnableDisable_FSModelComponent(PXCache cache, FSModelComponent fsModelComponentRow)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
                return;

            if (fsModelComponentRow.ComponentID != null)
            {
                bool enableLineByActive = fsModelComponentRow.Active == true;

                PXUIFieldAttribute.SetEnabled<FSModelComponent.componentID>(cache, fsModelComponentRow, enableLineByActive);
                PXUIFieldAttribute.SetEnabled<FSModelComponent.descr>(cache, fsModelComponentRow, enableLineByActive);
                PXUIFieldAttribute.SetEnabled<FSModelComponent.classID>(cache, fsModelComponentRow, fsModelComponentRow.ClassID == null);
                PXUIFieldAttribute.SetEnabled<FSModelComponent.requireSerial>(cache, fsModelComponentRow, enableLineByActive);
            }
        }

        /// <summary>
        /// Reset the values on the 'Components' grid and loads the Component registers from the selected 'ItemClass' for the current 'InventoryItem' ('StockItem').
        /// </summary>
        public virtual void ResetValuesFromItemClass(PXCache cache, InventoryItem inventoryItemRow, int? itemClassID)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
                return;

            if (inventoryItemRow != null && inventoryItemRow.ItemClassID != null)
            {
                using (var ts = new PXTransactionScope())
                {
                    INItemClass inItemClassRow = PXSelect<INItemClass,
                                                 Where<
                                                     INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>>>
                                                 .Select(Base, itemClassID);

                    FSxEquipmentModelTemplate fsxEquipmentModelTemplateRow = Base.ItemClass.Cache.GetExtension<FSxEquipmentModelTemplate>(inItemClassRow);
                    FSxEquipmentModel fsxEquipmentModelRow = cache.GetExtension<FSxEquipmentModel>(inventoryItemRow);

                    var fsModelTemplateComponentSet = PXSelect<FSModelTemplateComponent,
                                                      Where<
                                                          FSModelTemplateComponent.modelTemplateID, Equal<Required<FSModelTemplateComponent.modelTemplateID>>,
                                                          And<FSModelTemplateComponent.active, Equal<True>>>>
                                                      .Select(Base, itemClassID);

                    fsxEquipmentModelRow.EQEnabled = fsxEquipmentModelTemplateRow.EQEnabled;
                    fsxEquipmentModelRow.EquipmentItemClass = fsxEquipmentModelTemplateRow.EquipmentItemClass;
                    fsxEquipmentModelRow.ModelType = fsxEquipmentModelTemplateRow.DfltModelType;

                    foreach (FSModelComponent fsModelComponentRow in ModelComponents.Select())
                    {
                        ModelComponents.Delete(fsModelComponentRow);
                    }

                    foreach (FSModelTemplateComponent fsModelTemplateComponentRow in fsModelTemplateComponentSet)
                    {
                        if (fsxEquipmentModelTemplateRow.EQEnabled == true)
                        {
                            FSModelComponent fsModelComponentRow = new FSModelComponent();
                            fsModelComponentRow.Active = fsModelTemplateComponentRow.Active;
                            fsModelComponentRow.ComponentID = fsModelTemplateComponentRow.ComponentID;
                            fsModelComponentRow.Descr = fsModelTemplateComponentRow.Descr;
                            fsModelComponentRow.ClassID = fsModelTemplateComponentRow.ClassID;
                            fsModelComponentRow.Optional = fsModelTemplateComponentRow.Optional;
                            fsModelComponentRow.Qty = fsModelTemplateComponentRow.Qty;

                            ModelComponents.Cache.Insert(fsModelComponentRow);
                        }
                    }

                    ts.Complete();
                }
            }
        }

        /// <summary>
        /// Show or Hide Model Fields.
        /// </summary>
        public virtual void ShowOrHideFields(PXCache cache, InventoryItem inventoryItemRow)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
                return;

            FSxEquipmentModel fsxEquipmemtModelRow = cache.GetExtension<FSxEquipmentModel>(inventoryItemRow);
            bool hideFields = fsxEquipmemtModelRow.EquipmentItemClass != ID.Equipment_Item_Class.PART_OTHER_INVENTORY
                                && fsxEquipmemtModelRow.EquipmentItemClass != ID.Equipment_Item_Class.CONSUMABLE;

            PXUIFieldAttribute.SetVisible<FSxEquipmentModel.manufacturerID>(cache, inventoryItemRow, hideFields);
            PXUIFieldAttribute.SetVisible<FSxEquipmentModel.manufacturerModelID>(cache, inventoryItemRow, hideFields);
            PXUIFieldAttribute.SetVisible<FSxEquipmentModel.equipmentTypeID>(cache, inventoryItemRow, hideFields);
            PXUIFieldAttribute.SetVisible<FSxEquipmentModel.cpnyWarrantyValue>(cache, inventoryItemRow, hideFields);
            PXUIFieldAttribute.SetVisible<FSxEquipmentModel.cpnyWarrantyType>(cache, inventoryItemRow, hideFields);
            PXUIFieldAttribute.SetVisible<FSxEquipmentModel.vendorWarrantyValue>(cache, inventoryItemRow, hideFields);
            PXUIFieldAttribute.SetVisible<FSxEquipmentModel.vendorWarrantyType>(cache, inventoryItemRow, hideFields);
        }

        /// <summary>
        /// Show or hide Components tab.
        /// </summary>
        /// <param name="cache">Cache of the Inventory Item.</param>
        /// <param name="inventoryItemRow">Inventory Item Row.</param>
        public virtual void ShowOrHideComponetsTab(PXCache cache, InventoryItem inventoryItemRow)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
                return;

            FSxEquipmentModel fsxEquipmemtModelRow = cache.GetExtension<FSxEquipmentModel>(inventoryItemRow);
            this.ModelComponents.AllowSelect = fsxEquipmemtModelRow.Mem_ShowComponent == true;
        }
        #endregion

        #region Event Handlers

        #region InventoryItem

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<InventoryItem, FSxService.billingRule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = ID.BillingRule.FLAT_RATE;
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            InventoryItem inventoryItemRow = (InventoryItem)e.Row;
            ResetValuesFromItemClass(e.Cache, inventoryItemRow, inventoryItemRow.ItemClassID);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<InventoryItem> e)
        {
        }

        protected virtual void _(Events.RowSelected<InventoryItem> e)
        {
            if (e.Row == null)
            {
                return;
            }

            InventoryItem inventoryItemRow = (InventoryItem)e.Row;
            PXCache cache = e.Cache;

            ShowOrHideFields(cache, inventoryItemRow);
            EnableDisable_InventoryItem(cache, inventoryItemRow);
            ShowOrHideComponetsTab(cache, inventoryItemRow);
        }

        protected virtual void _(Events.RowInserting<InventoryItem> e)
        {
        }

        protected virtual void _(Events.RowInserted<InventoryItem> e)
        {
            if (e.Row == null)
            {
                return;
            }

            InventoryItem inventoryItemRow = (InventoryItem)e.Row;
            ResetValuesFromItemClass(e.Cache, inventoryItemRow, inventoryItemRow.ItemClassID);
        }

        protected virtual void _(Events.RowUpdating<InventoryItem> e)
        {
        }

        protected virtual void _(Events.RowUpdated<InventoryItem> e)
        {
        }

        protected virtual void _(Events.RowDeleting<InventoryItem> e)
        {
        }

        protected virtual void _(Events.RowDeleted<InventoryItem> e)
        {
        }

        protected virtual void _(Events.RowPersisting<InventoryItem> e)
        {
        }

        protected virtual void _(Events.RowPersisted<InventoryItem> e)
        {
        }

        #endregion

        #region FSModelComponent

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSModelComponent, FSModelComponent.componentID> e)
        {
            if (e.Row == null || PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
            {
                return;
            }

            FSModelComponent fsModelComponentRow = (FSModelComponent)e.Row;

            if (fsModelComponentRow.ComponentID != null)
            {
                FSModelTemplateComponent fsModelTemplateComponentRow = PXSelect<FSModelTemplateComponent,
                                                                       Where<
                                                                           FSModelTemplateComponent.componentID, Equal<Required<FSModelTemplateComponent.componentID>>>>
                                                                       .Select(Base, fsModelComponentRow.ComponentID);

                fsModelComponentRow.Active = fsModelTemplateComponentRow.Active;
                fsModelComponentRow.ComponentID = fsModelTemplateComponentRow.ComponentID;
                fsModelComponentRow.Descr = fsModelTemplateComponentRow.Descr;
                fsModelComponentRow.ClassID = fsModelTemplateComponentRow.ClassID;
                fsModelComponentRow.Optional = fsModelTemplateComponentRow.Optional;
                fsModelComponentRow.Qty = fsModelTemplateComponentRow.Qty;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSModelComponent, FSModelComponent.inventoryID> e)
        {
            if (e.Row == null || PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
            {
                return;
            }

            FSModelComponent fsModelComponentRow = (FSModelComponent)e.Row;

            if (fsModelComponentRow.InventoryID != null)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(Base, fsModelComponentRow.InventoryID);

                FSxEquipmentModel fsxEquipmentModelTemplate = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);

                fsModelComponentRow.CpnyWarrantyValue = fsxEquipmentModelTemplate.CpnyWarrantyValue;
                fsModelComponentRow.CpnyWarrantyType = fsxEquipmentModelTemplate.CpnyWarrantyType;
                fsModelComponentRow.VendorWarrantyValue = fsxEquipmentModelTemplate.VendorWarrantyValue;
                fsModelComponentRow.VendorWarrantyType = fsxEquipmentModelTemplate.VendorWarrantyType;
                fsModelComponentRow.VendorID = inventoryItemRow.PreferredVendorID;
            }
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSModelComponent> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSModelComponent> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSModelComponent fsModelComponentRow = (FSModelComponent)e.Row;
            EnableDisable_FSModelComponent(e.Cache, fsModelComponentRow);
        }

        protected virtual void _(Events.RowInserting<FSModelComponent> e)
        {
            if (e.Row == null || PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
            {
                return;
            }

            FSModelComponent fsModelComponentRow = (FSModelComponent)e.Row;

            FSModelComponent fsModelComponentRow_InDB = PXSelect<FSModelComponent,
                                                        Where<
                                                            FSModelComponent.componentID, Equal<Required<FSModelComponent.componentID>>,
                                                        And<
                                                            FSModelComponent.modelID, Equal<Current<InventoryItem.inventoryID>>>>>
                                                        .SelectWindowed(Base, 0, 1, fsModelComponentRow.ComponentID);

            if (fsModelComponentRow_InDB != null)
            {
                e.Cache.RaiseExceptionHandling<FSModelComponent.componentID>
                    (e.Row, fsModelComponentRow.ComponentID, new PXException(TX.Error.ID_ALREADY_USED));
            }
        }

        protected virtual void _(Events.RowInserted<FSModelComponent> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSModelComponent> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSModelComponent> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSModelComponent> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSModelComponent> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSModelComponent> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSModelComponent> e)
        {
        }

        #endregion

        #endregion
    }
}