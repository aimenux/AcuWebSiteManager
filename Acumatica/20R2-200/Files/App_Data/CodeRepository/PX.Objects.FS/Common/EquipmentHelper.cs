using PX.Data;
using PX.Objects.EP;
using PX.Objects.FA;
using System.Linq;

namespace PX.Objects.FS
{
    public static class EquipmentHelper
    {
        /// <summary>
        /// Set default values in FSEquipment from the Fixed Asset specified.
        /// </summary>
        public static void SetDefaultValuesFromFixedAsset(PXCache cacheFSEquipment, FSEquipment fsEquipmentRow, int? fixedAssetID)
        {
            if (fixedAssetID == null)
            {
                return;
            }

            FADetails faDetailsRow = PXSelect<FADetails,
                                     Where<
                                         FADetails.assetID, Equal<Required<FADetails.assetID>>>>
                                     .Select(cacheFSEquipment.Graph, fixedAssetID);

            if (faDetailsRow != null)
            {
                cacheFSEquipment.SetValueExt<FSEquipment.purchDate>(fsEquipmentRow, faDetailsRow.ReceiptDate);
                cacheFSEquipment.SetValueExt<FSEquipment.registeredDate>(fsEquipmentRow, faDetailsRow.DepreciateFromDate);
                cacheFSEquipment.SetValueExt<FSEquipment.purchAmount>(fsEquipmentRow, faDetailsRow.AcquisitionCost);
                cacheFSEquipment.SetValueExt<FSEquipment.purchPONumber>(fsEquipmentRow, faDetailsRow.PONumber);
                cacheFSEquipment.SetValueExt<FSEquipment.propertyType>(fsEquipmentRow, faDetailsRow.PropertyType);
                cacheFSEquipment.SetValueExt<FSEquipment.serialNumber>(fsEquipmentRow, faDetailsRow.SerialNumber);
            }
        }

        /// <summary>
        /// Update a FSEquipment record with the values in the EPEquipment record.
        /// </summary>
        /// <param name="cacheFSEquipment">The cache of the FSEquipment record.</param>
        /// <param name="fsEquipmentRow">The FSEquipment record.</param>
        /// <param name="cacheEPEquipment">The cache of the EPEquipment record.</param>
        /// <param name="epEquipmentRow">The EPEquipment record.</param>
        /// <returns>Returns true if some value changes, otherwise it returns false.</returns>
        public static bool UpdateFSEquipmentWithEPEquipment(PXCache cacheFSEquipment, FSEquipment fsEquipmentRow, PXCache cacheEPEquipment, EPEquipment epEquipmentRow)
        {
            return CopyEPEquipmentFields(cacheFSEquipment, fsEquipmentRow, cacheEPEquipment, epEquipmentRow);
        }

        /// <summary>
        /// Update a EPEquipment record with the values in the FSEquipment record.
        /// </summary>
        /// <param name="cacheEPEquipment">The cache of the EPEquipment record.</param>
        /// <param name="epEquipmentRow">The EPEquipment record.</param>
        /// <param name="cacheFSEquipment">The cache of the FSEquipment record.</param>
        /// <param name="fsEquipmentRow">The FSEquipment record.</param>
        /// <returns>Returns true if some value changes, otherwise it returns false.</returns>
        public static bool UpdateEPEquipmentWithFSEquipment(PXCache cacheEPEquipment, EPEquipment epEquipmentRow, PXCache cacheFSEquipment, FSEquipment fsEquipmentRow)
        {
            return CopyEPEquipmentFields(cacheEPEquipment, epEquipmentRow, cacheFSEquipment, fsEquipmentRow);
        }

        /// <summary>
        /// Update a record with the values in another one.
        /// </summary>
        /// <param name="cacheTo">The cache of the record to be updated.</param>
        /// <param name="rowTo">The record to be updated.</param>
        /// <param name="cacheFrom">The cache of the record to be read.</param>
        /// <param name="rowFrom">The record to be read.</param>
        /// <returns>Returns true if some value changes, otherwise it returns false.</returns>
        private static bool CopyEPEquipmentFields(PXCache cacheTo, IBqlTable rowTo, PXCache cacheFrom, IBqlTable rowFrom)
        {
            string fieldTo;
            string fieldFrom;
            string tempSwap;

            bool someValueChanged = false;
            string stringValue;

            //// Copy Status
            fieldTo = typeof(FSEquipment.status).Name;
            fieldFrom = typeof(EPEquipment.status).Name;

            if (rowTo is EPEquipment)
            {
                tempSwap = fieldTo;
                fieldTo = fieldFrom;
                fieldFrom = tempSwap;
            }

            stringValue = (string)cacheFrom.GetValue(rowFrom, fieldFrom);

            if (string.Equals(stringValue, cacheTo.GetValue(rowTo, fieldTo)) == false)
            {
                cacheTo.SetValueExt(rowTo, fieldTo, stringValue);
                someValueChanged = true;
            }

            //// Copy Description
            fieldTo = typeof(FSEquipment.descr).Name;
            fieldFrom = typeof(EPEquipment.description).Name;

            if (rowTo is EPEquipment)
            {
                tempSwap = fieldTo;
                fieldTo = fieldFrom;
                fieldFrom = tempSwap;
            }

            stringValue = (string)cacheFrom.GetValue(rowFrom, fieldFrom);

            if (string.Equals(stringValue, cacheTo.GetValue(rowTo, fieldTo)) == false)
            {
                cacheTo.SetValueExt(rowTo, fieldTo, stringValue);
                someValueChanged = true;
            }

            return someValueChanged;
        }

        public static bool CheckReplaceComponentLines<TPartLine, TComponentLineRef>(PXCache cache, PXResultset<TPartLine> rows, IFSSODetBase currentRow)
            where TPartLine : class, IBqlTable, IFSSODetBase, new()
            where TComponentLineRef : IBqlField
        {
            if (currentRow == null)
            {
                return true;
            }

            if (currentRow.EquipmentAction != ID.Equipment_Action.REPLACING_COMPONENT
                || currentRow.SMEquipmentID == null
                || currentRow.EquipmentLineRef == null)
            {
                return true;
            }

            bool noErrors = true;

            foreach (TPartLine row in rows.RowCast<TPartLine>().Where(x => x.IsInventoryItem == true))
            {
                if (row.LineID != currentRow.LineID
                    && row.EquipmentAction == currentRow.EquipmentAction
                    && row.SMEquipmentID == currentRow.SMEquipmentID
                    && row.EquipmentLineRef == currentRow.EquipmentLineRef)
                {
                    string componentLineRef = (string)PXSelectorAttribute.GetField(cache,
                                                                                   currentRow,
                                                                                   typeof(TComponentLineRef).Name,
                                                                                   currentRow.EquipmentLineRef,
                                                                                   typeof(FSEquipmentComponent.lineRef).Name);

                    cache.RaiseExceptionHandling<TComponentLineRef>(currentRow,
                                                                    componentLineRef,
                                                                    new PXSetPropertyException(TX.Error.SELECTED_COMPONENT_HAS_ALREADY_BEEN_CHOSEN_FOR_REPLACEMENT, PXErrorLevel.Error));

                    noErrors = false;
                }
            }

            return noErrors;
        }
    }
}
