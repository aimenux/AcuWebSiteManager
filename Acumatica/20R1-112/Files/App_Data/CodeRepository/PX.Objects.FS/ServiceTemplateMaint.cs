using PX.Data;

namespace PX.Objects.FS
{
    public class ServiceTemplateMaint : PXGraph<ServiceTemplateMaint, FSServiceTemplate>
    {
        #region Selects
        public PXSelect<FSServiceTemplate> ServiceTemplateRecords;

        [PXFilterable]
        public PXSelect<FSServiceTemplateDet, 
               Where<
                   FSServiceTemplateDet.serviceTemplateID, Equal<Current<FSServiceTemplate.serviceTemplateID>>>>
               ServiceTemplateDetails;

        public PXSetup<FSSrvOrdType>.Where<
               Where<
                   FSSrvOrdType.srvOrdType, Equal<Current<FSServiceTemplate.srvOrdType>>>> ServiceOrderTypeSelected;

        #endregion

        #region Events

        #region FSServiceTemplate

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSServiceTemplate> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSServiceTemplate> e)
        {
        }

        #endregion

        #region FSServiceTemplateDet

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSServiceTemplateDet, FSServiceTemplateDet.lineType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDet fsServiceTemplateDetRow = (FSServiceTemplateDet)e.Row;
            LineTypeBlankFields(fsServiceTemplateDetRow);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceTemplateDet, FSServiceTemplateDet.inventoryID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDet fsServiceTemplateDetRow = (FSServiceTemplateDet)e.Row;

            if (fsServiceTemplateDetRow.LineType == null)
            {
                //We just run the field defaulting because this is the first field when you try to insert a new line.
                object lineTypeValue;
                ServiceTemplateDetails.Cache.RaiseFieldDefaulting<FSServiceTemplateDet.lineType>(ServiceTemplateDetails.Current, out lineTypeValue);
                fsServiceTemplateDetRow.LineType = (string)lineTypeValue;
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSServiceTemplateDet> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSServiceTemplateDet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDet fsServiceTemplateDetRow = (FSServiceTemplateDet)e.Row;
            LineTypeBlankFields(fsServiceTemplateDetRow);
            LineTypeEnableDisable(e.Cache, fsServiceTemplateDetRow);
        }

        protected virtual void _(Events.RowInserting<FSServiceTemplateDet> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSServiceTemplateDet> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSServiceTemplateDet> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSServiceTemplateDet> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSServiceTemplateDet> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSServiceTemplateDet> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSServiceTemplateDet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDet fsServiceTemplateDetRow = (FSServiceTemplateDet)e.Row;

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                LineTypeValidateLine(e.Cache, fsServiceTemplateDetRow, PXErrorLevel.Error);
            }
        }

        protected virtual void _(Events.RowPersisted<FSServiceTemplateDet> e)
        {
        }
        #endregion

        #endregion

        #region Virtual methods

        /// <summary>
        /// This method enables or disables the fields on the <c>FSserviceTemplateDet</c> grid depending on the <c>FSServiceTemplateDet.LineType</c> field.
        /// </summary>
        public virtual void LineTypeEnableDisable(PXCache cache, FSServiceTemplateDet fsServiceTemplateDetRow)
        {
            switch (fsServiceTemplateDetRow.LineType)
            {
                case ID.LineType_ServiceTemplate.COMMENT:
                case ID.LineType_ServiceTemplate.INSTRUCTION:
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.serviceTemplateID>(cache, fsServiceTemplateDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.serviceTemplateID>(cache, fsServiceTemplateDetRow, PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.inventoryID>(cache, fsServiceTemplateDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.inventoryID>(cache, fsServiceTemplateDetRow, PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.qty>(cache, fsServiceTemplateDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.qty>(cache, fsServiceTemplateDetRow, PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.tranDesc>(cache, fsServiceTemplateDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.tranDesc>(cache, fsServiceTemplateDetRow, PXPersistingCheck.NullOrBlank);
                    break;

                case ID.LineType_ServiceTemplate.INVENTORY_ITEM:
                case ID.LineType_ServiceTemplate.SERVICE:
                case ID.LineType_ServiceTemplate.NONSTOCKITEM:
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.serviceTemplateID>(cache, fsServiceTemplateDetRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.serviceTemplateID>(cache, fsServiceTemplateDetRow, PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.inventoryID>(cache, fsServiceTemplateDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.inventoryID>(cache, fsServiceTemplateDetRow, PXPersistingCheck.NullOrBlank);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.qty>(cache, fsServiceTemplateDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.qty>(cache, fsServiceTemplateDetRow, PXPersistingCheck.NullOrBlank);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.tranDesc>(cache, fsServiceTemplateDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.tranDesc>(cache, fsServiceTemplateDetRow, PXPersistingCheck.NullOrBlank);
                    break;

                default:
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.inventoryID>(cache, fsServiceTemplateDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.inventoryID>(cache, fsServiceTemplateDetRow, PXPersistingCheck.NullOrBlank);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.qty>(cache, fsServiceTemplateDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.qty>(cache, fsServiceTemplateDetRow, PXPersistingCheck.NullOrBlank);
                    PXUIFieldAttribute.SetEnabled<FSServiceTemplateDet.tranDesc>(cache, fsServiceTemplateDetRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSServiceTemplateDet.tranDesc>(cache, fsServiceTemplateDetRow, PXPersistingCheck.NullOrBlank);
                    break;
            }
        }

        /// <summary>
        /// This method blanks the fields that aren't needed depending on the <c>FSServiceTemplateDet.LineType</c> field.
        /// </summary>
        public virtual void LineTypeBlankFields(FSServiceTemplateDet fsServiceTemplateDetRow)
        {
            switch (fsServiceTemplateDetRow.LineType)
            {
                case ID.LineType_ServiceTemplate.COMMENT:
                case ID.LineType_ServiceTemplate.INSTRUCTION:
                    fsServiceTemplateDetRow.InventoryID = null;
                    fsServiceTemplateDetRow.Qty = 0;
                    break;
            }
        }

        /// <summary>
        /// This method validates if necessary fields are not null and launch the corresponding exception and error message.
        /// </summary>
        public virtual void LineTypeValidateLine(PXCache cache,
                                                 FSServiceTemplateDet fsServiceTemplateDetRow,
                                                 PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            switch (fsServiceTemplateDetRow.LineType)
            {
                case ID.LineType_ServiceTemplate.INVENTORY_ITEM:
                    if (fsServiceTemplateDetRow.InventoryID == null)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.inventoryID>(fsServiceTemplateDetRow,
                                                                                       null,
                                                                                       new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsServiceTemplateDetRow.Qty < 0)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.qty>(fsServiceTemplateDetRow,
                                                                               null,
                                                                               new PXSetPropertyException(TX.Error.NEGATIVE_QTY, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceTemplate.COMMENT:
                case ID.LineType_ServiceTemplate.INSTRUCTION:
                    if (string.IsNullOrEmpty(fsServiceTemplateDetRow.TranDesc))
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.tranDesc>(fsServiceTemplateDetRow,
                                                                                    null,
                                                                                    new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceTemplate.SERVICE:
                case ID.LineType_ServiceTemplate.NONSTOCKITEM:
                    if (fsServiceTemplateDetRow.InventoryID == null)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.inventoryID>(fsServiceTemplateDetRow,
                                                                                       null,
                                                                                       new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsServiceTemplateDetRow.Qty < 0)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.qty>(fsServiceTemplateDetRow,
                                                                               null,
                                                                               new PXSetPropertyException(TX.Error.NEGATIVE_QTY, errorLevel));
                    }

                    break;
            }
        }
        #endregion
    }
}
