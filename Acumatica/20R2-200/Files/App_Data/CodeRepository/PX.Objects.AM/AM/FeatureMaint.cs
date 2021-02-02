using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System.Linq;

namespace PX.Objects.AM
{
    public class FeatureMaint : PXGraph<FeatureMaint, AMFeature>
    {
        #region Views

        public PXSelect<AMFeature> Features;
        [PXImport(typeof(AMFeature))]
        public PXSelect<AMFeatureOption, Where<AMFeatureOption.featureID, Equal<Current<AMFeature.featureID>>>> FeatureOptions;
        [PXImport(typeof(AMFeature))]
        public PXSelect<AMFeatureAttribute, Where<AMFeatureAttribute.featureID, Equal<Current<AMFeature.featureID>>>> FeatureAttributes;

        public PXSetup<AMConfiguratorSetup> ConfiguratorSetup;
        public PXSetup<AMPSetup> ProductionSetup;

        #endregion

        public FeatureMaint()
        {
            var setup = ConfiguratorSetup.Current;
            var prodSetup = ProductionSetup.Current;
            AMPSetup.CheckSetup(prodSetup);
        }

        #region Auxiliary methods
        // This function is used to get the Attributes' variable names in the Formula editor.
        public string[] GetAllAttributes()
        {
            return GetAttributeVariables();
        }

        public string[] GetAllButCurrentAttributes()
        {
            if (this.FeatureAttributes.Current == null)
                return GetAttributeVariables();
            else
                return GetAttributeVariables(this.FeatureAttributes.Current.Variable);
        }

        private string[] GetAttributeVariables(params string[] removeVal)
        {
            return this.FeatureAttributes.Select()
                                         .Select(attr => ((AMFeatureAttribute)attr).Variable)
                                         .Where(var => !string.IsNullOrEmpty(var) && !removeVal.Contains(var))
                                         .Select(var => string.Format("[{0}]", var))
                                         .OrderBy(var => var)
                                         .ToArray();
        }
        #endregion

        #region Handlers

        protected virtual void AMFeature_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMFeature)e.Row;
            if (row == null) return;

            var inventoryItemRequired = row.AllowNonInventoryOptions != true;

            PXUIFieldAttribute.SetRequired<AMFeatureOption.inventoryID>(FeatureOptions.Cache, inventoryItemRequired);

            foreach (AMFeatureOption option in FeatureOptions.Select())
            {
                PXDefaultAttribute.SetPersistingCheck<AMFeatureOption.inventoryID>(FeatureOptions.Cache, option, inventoryItemRequired
                                                                                ? PXPersistingCheck.NullOrBlank
                                                                                : PXPersistingCheck.Nothing);
                // Let the cache revalidate what was already saved.
                if (inventoryItemRequired && !option.InventoryID.HasValue)
                {
                    if (FeatureOptions.Cache.GetStatus(option) == PXEntryStatus.Notchanged)
                    {
                        FeatureOptions.Cache.SetStatus(option, PXEntryStatus.Updated);
                    }
                }
            }
        }

        protected virtual void AMFeatureOption_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMFeatureOption)e.Row;
            if (row == null
                || !sender.AllowUpdate)
            {
                return;
            }

            bool isInventoryRow = row.InventoryID != null;
            PXUIFieldAttribute.SetEnabled<AMFeatureOption.bFlush>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMFeatureOption.scrapFactor>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMFeatureOption.phantomRouting>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMFeatureOption.materialType>(sender, row, isInventoryRow);
            PXUIFieldAttribute.SetEnabled<AMFeatureOption.subcontractSource>(sender, row, isInventoryRow);
        }

        protected virtual void AMFeatureOption_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMFeatureOption)e.Row;
            if (row == null)
            {
                return;
            }

            var item = PXSelectorAttribute.Select<AMFeatureOption.inventoryID>(sender, row) as InventoryItem;
            if (item == null)
            {
                row.UOM = null;
                row.ScrapFactor = null;
                sender.SetDefaultExt<AMFeatureOption.bFlush>(row);
                sender.SetDefaultExt<AMFeatureOption.phantomRouting>(row);
                sender.SetDefaultExt<AMFeatureOption.materialType>(row);
                return;
            }
            if (string.IsNullOrWhiteSpace(row.Label))
            {
                row.Label = item.InventoryCD;
            }
            if(string.IsNullOrWhiteSpace(row.Descr))
            {
                row.Descr = item.Descr;
            }
            row.UOM = item.BaseUnit;
        }

        protected virtual void AMFeatureAttribute_AttributeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            // Set some default values.
            var row = (AMFeatureAttribute)e.Row;
            if (row == null)
            {
                return;
            }

            var item = PXSelectorAttribute.Select<AMFeatureAttribute.attributeID>(sender, row) as CSAttribute;
            if (string.IsNullOrWhiteSpace(row.Label))
            {
                row.Label = item.AttributeID;
            }
            if (string.IsNullOrWhiteSpace(row.Descr))
            {
                row.Descr = item.Description;
            }
        }

        protected virtual void AMFeature_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMFeature)e.Row;
            if (row == null) 
            {
                return;
            }

            AMConfigurationFeature configFeature = PXSelect<AMConfigurationFeature,
                Where<AMConfigurationFeature.featureID, Equal<Required<AMConfigurationFeature.featureID>>
                >>.SelectWindowed(this, 0, 1, row.FeatureID);

            if (configFeature != null)
            {
                sender.RaiseExceptionHandling<AMFeature.featureID>(
                    row,
                    row.FeatureID,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FeatureInUseCannotDelete,
                    configFeature.ConfigurationID, configFeature.Revision), PXErrorLevel.Error)
                    );
                e.Cancel = true;
            }
        }

        protected virtual void AMFeatureOption_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMFeatureOption)e.Row;
            if (row == null || Features.Current == null)
            {
                return;
            }

            if (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update)
            {
                return;
            }

            if (row.InventoryID == null && !Features.Current.AllowNonInventoryOptions.GetValueOrDefault())
            {
                sender.RaiseExceptionHandling<AMFeatureOption.inventoryID>(
                    row,
                    row.InventoryID,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FeatureDoesntAllowNonInventory),
                        Features.Current.FeatureID.TrimIfNotNullEmpty(),
                    PXErrorLevel.Error));
                e.Cancel = true;
            }

            // Require SUBITEMID when the item is a stock item
            if (InventoryHelper.SubItemFeatureEnabled && row.InventoryID != null && row.SubItemID == null)
            {
                var inventoryItem = (InventoryItem)PXSelectorAttribute.Select<AMConfigurationOption.inventoryID>(sender, row);
                if (inventoryItem?.StkItem != true)
                {
                    return;
                }

                sender.RaiseExceptionHandling<AMFeatureOption.subItemID>(
                        row,
                        row.SubItemID,
                        new PXSetPropertyException(Messages.SubItemIDRequiredForStockItem, PXErrorLevel.Error));
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.FieldUpdated<AMFeatureOption, AMFeatureOption.materialType> e)
        {
            if (e.Row?.MaterialType == null)
            {
                return;
            }

            if (e.Row.MaterialType == AMMaterialType.Regular)
            {
                e.Cache.SetValueExt<AMFeatureOption.subcontractSource>(e.Row, AMSubcontractSource.None);
                return;
            }

            e.Cache.SetValueExt<AMFeatureOption.subcontractSource>(e.Row, AMSubcontractSource.Purchase);
        }
        #endregion
    }
 }