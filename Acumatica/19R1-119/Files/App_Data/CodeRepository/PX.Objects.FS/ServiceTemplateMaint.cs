using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class ServiceTemplateMaint : PXGraph<ServiceTemplateMaint, FSServiceTemplate>
    {
        #region Selects
        public PXSelect<FSServiceTemplate>
               ServiceTemplateRecords;

        public PXSelect<FSServiceTemplateDetService,
               Where<
                    FSServiceTemplateDet.serviceTemplateID, Equal<Current<FSServiceTemplate.serviceTemplateID>>,
                    And<
                       Where<
                            FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Service>,
                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Comment_Service>,
                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.Instruction_Service>,
                            Or<FSServiceTemplateDet.lineType, Equal<ListField_LineType_Service_ServiceTemplate.NonStockItem>>>>>>>>
               ServiceTemplateDetServices;

        public PXSelect<FSServiceTemplateDetPart,
               Where<
                    FSServiceTemplateDetPart.serviceTemplateID, Equal<Current<FSServiceTemplate.serviceTemplateID>>,
                    And<
                       Where<
                            FSServiceTemplateDetPart.lineType, Equal<ListField_LineType_Part_ALL.Inventory_Item>,
                            Or<FSServiceTemplateDetPart.lineType, Equal<ListField_LineType_Part_ALL.Comment_Part>,
                            Or<FSServiceTemplateDetPart.lineType, Equal<ListField_LineType_Part_ALL.Instruction_Part>>>>>>>
               ServiceTemplateDetParts;

        public PXSetup<FSSrvOrdType>.Where<
                    Where<
                        FSSrvOrdType.srvOrdType, Equal<Current<FSServiceTemplate.srvOrdType>>>> ServiceOrderTypeSelected;

        #endregion

        #region Events

        protected virtual void FSServiceTemplate_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplate fsServiceTemplateRow = (FSServiceTemplate)e.Row;

            bool enable = false;
            bool enableDisableSrvOrdType = true;

            enable = ServiceOrderTypeSelected.Current != null && ServiceOrderTypeSelected.Current.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;

            if (enable && ServiceTemplateDetParts.Select().Count > 0)
            {
                enableDisableSrvOrdType = false;
            }

            PXUIFieldAttribute.SetEnabled<FSServiceTemplate.srvOrdType>(cache, fsServiceTemplateRow, enableDisableSrvOrdType);

            ServiceTemplateDetParts.Cache.AllowInsert = enable;
            ServiceTemplateDetParts.Cache.AllowUpdate = enable;
            ServiceTemplateDetParts.Cache.AllowDelete = enable;
        }
        
        protected virtual void FSServiceTemplateDetService_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDetService fsServiceTemplateDetServiceRow = (FSServiceTemplateDetService)e.Row;
            LineTypeBlankFields(fsServiceTemplateDetServiceRow);
        }

        protected virtual void FSServiceTemplateDetService_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDetService fsServiceTemplateDetServiceRow = (FSServiceTemplateDetService)e.Row;

            if (fsServiceTemplateDetServiceRow.LineType == null)
            {
                //We just run the field defaulting because this is the first field when you try to insert a new line.
                object lineTypeValue;
                ServiceTemplateDetServices.Cache.RaiseFieldDefaulting<FSSODetService.lineType>(ServiceTemplateDetServices.Current, out lineTypeValue);
                fsServiceTemplateDetServiceRow.LineType = (string)lineTypeValue;
            }
        }

        protected virtual void FSServiceTemplateDetService_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDetService fsServiceTemplateDetServiceRow = (FSServiceTemplateDetService)e.Row;
            LineTypeBlankFields(fsServiceTemplateDetServiceRow);
            LineTypeEnableDisable(cache, fsServiceTemplateDetServiceRow);
        }

        protected virtual void FSServiceTemplateDetService_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            FSServiceTemplateDetService fsServiceTemplateDetServiceRow = (FSServiceTemplateDetService)e.Row;
            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                LineTypeValidateLine(cache, fsServiceTemplateDetServiceRow, PXErrorLevel.Error);
            }
        }

        protected virtual void FSServiceTemplateDetPart_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDetPart fsServiceTemplateDetPartRow = (FSServiceTemplateDetPart)e.Row;
            LineTypeBlankFields(fsServiceTemplateDetPartRow);
        }

        protected virtual void FSServiceTemplateDetPart_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDetPart fsServiceTemplateDetPartRow = (FSServiceTemplateDetPart)e.Row;

            if (fsServiceTemplateDetPartRow.LineType == null)
            {
                //We just run the field defaulting because this is the first field when you try to insert a new line.
                object lineTypeValue;
                ServiceTemplateDetServices.Cache.RaiseFieldDefaulting<FSSODetService.lineType>(ServiceTemplateDetServices.Current, out lineTypeValue);
                fsServiceTemplateDetPartRow.LineType = (string)lineTypeValue;
            }
        }

        protected virtual void FSServiceTemplateDetPart_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceTemplateDetPart fsServiceTemplateDetPartRow = (FSServiceTemplateDetPart)e.Row;
            LineTypeBlankFields(fsServiceTemplateDetPartRow);
            LineTypeEnableDisable(cache, fsServiceTemplateDetPartRow);
        }

        protected virtual void FSServiceTemplateDetPart_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            FSServiceTemplateDetPart fsServiceTemplateDetPartRow = (FSServiceTemplateDetPart)e.Row;
            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                LineTypeValidateLine(cache, fsServiceTemplateDetPartRow, PXErrorLevel.Error);
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// This method enables or disables the fields on the <c>FSserviceTemplateDet</c> grid depending on the <c>FSServiceTemplateDet.LineType</c> field.
        /// </summary>
        private void LineTypeEnableDisable(PXCache cache, FSServiceTemplateDet fsServiceTemplateDetRow)
        {
            switch (fsServiceTemplateDetRow.LineType)
            {
                case ID.LineType_ServiceTemplate.COMMENT_SERVICE:
                case ID.LineType_ServiceTemplate.INSTRUCTION_SERVICE:
                case ID.LineType_ServiceTemplate.COMMENT_PART:
                case ID.LineType_ServiceTemplate.INSTRUCTION_PART:
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
        private void LineTypeBlankFields(FSServiceTemplateDet fsServiceTemplateDetRow)
        {
            switch (fsServiceTemplateDetRow.LineType)
            {
                case ID.LineType_ServiceTemplate.COMMENT_SERVICE:
                case ID.LineType_ServiceTemplate.INSTRUCTION_SERVICE:
                case ID.LineType_ServiceTemplate.COMMENT_PART:
                case ID.LineType_ServiceTemplate.INSTRUCTION_PART:
                    fsServiceTemplateDetRow.InventoryID = null;
                    fsServiceTemplateDetRow.Qty = 0;
                    break;
            }
        }

        /// <summary>
        /// This method validates if necessary fields are not null and launch the corresponding exception and error message.
        /// </summary>
        private void LineTypeValidateLine(
                                        PXCache cache,
                                        FSServiceTemplateDet fsServiceTemplateDetRow,
                                        PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            switch (fsServiceTemplateDetRow.LineType)
            {
                case ID.LineType_ServiceTemplate.INVENTORY_ITEM:
                    if (fsServiceTemplateDetRow.InventoryID == null)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.inventoryID>(
                                                    fsServiceTemplateDetRow,
                                                    null,
                                                    new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsServiceTemplateDetRow.Qty < 0)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.qty>(
                                                                            fsServiceTemplateDetRow,
                                                                            null,
                                                                            new PXSetPropertyException(TX.Error.NEGATIVE_QTY, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceTemplate.COMMENT_SERVICE:
                case ID.LineType_ServiceTemplate.INSTRUCTION_SERVICE:
                case ID.LineType_ServiceTemplate.COMMENT_PART:
                case ID.LineType_ServiceTemplate.INSTRUCTION_PART:
                    if (string.IsNullOrEmpty(fsServiceTemplateDetRow.TranDesc))
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.tranDesc>(
                                                    fsServiceTemplateDetRow,
                                                    null,
                                                    new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceTemplate.SERVICE:
                case ID.LineType_ServiceTemplate.NONSTOCKITEM:
                    if (fsServiceTemplateDetRow.InventoryID == null)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.inventoryID>(
                                                    fsServiceTemplateDetRow,
                                                    null,
                                                    new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsServiceTemplateDetRow.Qty < 0)
                    {
                        cache.RaiseExceptionHandling<FSServiceTemplateDet.qty>(
                                                    fsServiceTemplateDetRow,
                                                    null,
                                                    new PXSetPropertyException(TX.Error.NEGATIVE_QTY, errorLevel));
                    }

                    break;
            }
        }
        #endregion
    }
}
