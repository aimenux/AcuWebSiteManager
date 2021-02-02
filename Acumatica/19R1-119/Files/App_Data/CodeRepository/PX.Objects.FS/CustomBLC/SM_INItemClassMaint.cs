using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class SM_INItemClassMaint : PXGraphExtension<INItemClassMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();
        }

        #region Selects
        public PXSelect<FSModelTemplateComponent,
               Where<
                   FSModelTemplateComponent.modelTemplateID, Equal<Current<INItemClass.itemClassID>>>>
               ModelTemplateComponentRecords;
        #endregion

        #region Private Functions

        /// <summary>
        /// Enables/Disables the Item Type field depending of there is at least one service related to.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="itemClassRow">The current INItemClass object row.</param>
        /// <param name="fsxServiceClassRow">The current <c>FSxServiceClass</c> object row.</param>
        private void EnableDisable_ItemType(PXCache cache, INItemClass itemClassRow, FSxServiceClass fsxServiceClassRow)
        {
            bool enableItemType = true;
            PXSetPropertyException exception = null;

            if (itemClassRow.ItemType == INItemTypes.ServiceItem && cache.GetStatus(itemClassRow) != PXEntryStatus.Inserted)
            {
                int rowCount = PXSelectJoin<InventoryItem,
                               InnerJoin<INItemClass,
                                   On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>,
                               Where<
                                   InventoryItem.itemClassID, Equal<Required<InventoryItem.itemClassID>>, 
                                   And<InventoryItem.itemType, Equal<INItemTypes.serviceItem>>>>
                               .SelectWindowed(cache.Graph, 0, 1, itemClassRow.ItemClassID).Count;

                enableItemType = rowCount == 0;
            }

            if (enableItemType == false)
            {
                exception = new PXSetPropertyException(
                        PXMessages.LocalizeFormatNoPrefix(TX.Warning.CANNOT_MODIFY_FIELD, "Services", "Item Class"),
                        PXErrorLevel.Warning);
            }

            cache.RaiseExceptionHandling<INItemClass.itemType>(
                itemClassRow,
                itemClassRow.ItemType,
                exception
                );

            cache.RaiseExceptionHandling<FSxServiceClass.requireRoute>(
                itemClassRow,
                fsxServiceClassRow.RequireRoute,
                exception
                );

            PXUIFieldAttribute.SetEnabled<INItemClass.itemType>(cache, itemClassRow, enableItemType);
            PXUIFieldAttribute.SetEnabled<FSxServiceClass.requireRoute>(cache, itemClassRow, enableItemType);
        }

        /// <summary>
        /// Enables or disables fields.
        /// </summary>
        private void EnableDisable(PXCache cache, INItemClass itemClassRow)
        {
            bool isEnabledAsModelTemplate = false;
            bool isStkItem = itemClassRow.StkItem == true;

            FSxServiceClass fsxServiceClassRow = cache.GetExtension<FSxServiceClass>(itemClassRow);

            PXUIFieldAttribute.SetEnabled<FSxServiceClass.dfltBillingRule>(cache, itemClassRow, itemClassRow.ItemType == INItemTypes.ServiceItem);

            EnableDisable_ItemType(cache, itemClassRow, fsxServiceClassRow);

            ModelTemplateComponentRecords.AllowSelect = false;

            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
            {
                FSxEquipmentModelTemplate fsxEquipmentModelTemplateRow = cache.GetExtension<FSxEquipmentModelTemplate>(itemClassRow);

                isEnabledAsModelTemplate = fsxEquipmentModelTemplateRow.EQEnabled == true
                    && fsxEquipmentModelTemplateRow.EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT;

                fsxEquipmentModelTemplateRow.Mem_ShowComponent = isEnabledAsModelTemplate;
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModelTemplate.equipmentItemClass>(cache, itemClassRow, isStkItem);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModelTemplate.eQEnabled>(cache, itemClassRow, isStkItem);
                PXUIFieldAttribute.SetEnabled<FSxEquipmentModelTemplate.dfltModelType>(cache, itemClassRow, isEnabledAsModelTemplate);
                PXDefaultAttribute.SetPersistingCheck<FSxEquipmentModelTemplate.dfltModelType>(
                                                                                               cache,
                                                                                               itemClassRow,
                                                                                               isEnabledAsModelTemplate == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

                ModelTemplateComponentRecords.AllowSelect = fsxEquipmentModelTemplateRow.Mem_ShowComponent == true;
            }

            ModelTemplateComponentRecords.Cache.AllowInsert = isEnabledAsModelTemplate;
            ModelTemplateComponentRecords.Cache.AllowUpdate = isEnabledAsModelTemplate;
            ModelTemplateComponentRecords.Cache.AllowDelete = isEnabledAsModelTemplate;

            if (isEnabledAsModelTemplate == false)
            {
                ModelTemplateComponentRecords.Cache.Clear();
            }
        }

        private void CheckComponentsClassID(FSxEquipmentModelTemplate fsxEquipmentModelTemplateRow)
        {
            if (fsxEquipmentModelTemplateRow == null
                    && fsxEquipmentModelTemplateRow.EquipmentItemClass != ID.Equipment_Item_Class.MODEL_EQUIPMENT)
            {
                return;
            }

            foreach (FSModelTemplateComponent fsModelTemplateComponentRow in ModelTemplateComponentRecords.Select())
            {
                if (ModelTemplateComponentRecords.Cache.GetStatus(fsModelTemplateComponentRow) == PXEntryStatus.Notchanged
                        && fsModelTemplateComponentRow.ClassID == null)
                {
                    FSModelTemplateComponent aux = fsModelTemplateComponentRow;
                    ModelTemplateComponentRecords.Cache.SetStatus(fsModelTemplateComponentRow, PXEntryStatus.Updated);
                }
            }
        }
        #endregion

        #region Event Handlers
        protected virtual void FSModelTemplateComponent_Qty_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if((int)e.NewValue < 1)
                throw new PXSetPropertyException(TX.Error.ZERO_OR_NEGATIVE_QTY);

        }

        protected void INItemClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            INItemClass inItemClassRow = (INItemClass)e.Row;
            EnableDisable(cache, inItemClassRow);
        }

        protected virtual void INItemClass_ItemType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }
            
            INItemClass itemClassRow = (INItemClass)e.Row;
            FSxServiceClass fsxServiceClassRow = cache.GetExtension<FSxServiceClass>(itemClassRow);

            if (itemClassRow.ItemType != INItemTypes.ServiceItem)
            {
                fsxServiceClassRow.RequireRoute = false;
            }            
        }

        protected virtual void INItemClass_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            INItemClass itemClassRow = (INItemClass)e.Row;
            FSxServiceClass fsxServiceClassRow = cache.GetExtension<FSxServiceClass>(itemClassRow);

            if (string.IsNullOrEmpty(fsxServiceClassRow.DfltBillingRule))
            {
                cache.RaiseExceptionHandling<FSxServiceClass.dfltBillingRule>(e.Row, fsxServiceClassRow.DfltBillingRule, new PXException(PX.Objects.FS.TX.Error.FIELD_EMPTY));
                e.Cancel = true;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
            {
            	FSxEquipmentModelTemplate fsxEquipmentModelTemplateRow = cache.GetExtension<FSxEquipmentModelTemplate>(itemClassRow);
            	CheckComponentsClassID(fsxEquipmentModelTemplateRow);
            }

        }

        protected virtual void FSModelTemplateComponent_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSModelTemplateComponent fsModelTemplateComponentRow = (FSModelTemplateComponent)e.Row;
            FSModelTemplateComponent fsModelTemplateComponentRow_InDB = PXSelect<FSModelTemplateComponent,
                                                                        Where<
                                                                            FSModelTemplateComponent.componentCD, Equal<Required<FSModelTemplateComponent.componentCD>>,
                                                                        And<
                                                                            FSModelTemplateComponent.modelTemplateID, Equal<Current<FSModelTemplateComponent.modelTemplateID>>>>>
                                                                        .SelectWindowed(Base, 0, 1, fsModelTemplateComponentRow.ComponentCD);

            if (fsModelTemplateComponentRow_InDB != null)
            {
                cache.RaiseExceptionHandling<FSModelTemplateComponent.componentCD>
                    (e.Row, fsModelTemplateComponentRow.ComponentCD, new PXException(TX.Error.ID_ALREADY_USED));
            }
        }
        #endregion
    }
}
