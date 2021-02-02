using PX.Data;
using System.Collections;
using PX.Objects.AM.Upgrade;

namespace PX.Objects.AM
{
    public class ProdSetup : PXGraph<ProdSetup>
    {
        public PXSelect<AMPSetup> ProdSetupRecord;
        public PXSave<AMPSetup> Save;
        public PXCancel<AMPSetup> Cancel;
        public PXSelect<AMScanSetup, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>> ScanSetup;

        protected virtual IEnumerable scanSetup()
        {
            AMScanSetup result = PXSelect<AMScanSetup, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
            return new AMScanSetup[] { result ?? ScanSetup.Insert() };
        }

        public PXSelect<AMScanUserSetup, Where<AMScanUserSetup.isOverridden, Equal<False>>> ScanUserSetups;

        protected virtual void _(Events.RowUpdated<AMScanSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            foreach (AMScanUserSetup userSetup in ScanUserSetups.Select())
            {
                userSetup.DefaultWarehouse = e.Row.DefaultWarehouse;
                userSetup.DefaultLotSerialNumber = e.Row.DefaultLotSerialNumber;
                userSetup.DefaultExpireDate = e.Row.DefaultExpireDate;
                ScanUserSetups.Update(userSetup);
            }
        }

        public override void Persist()
        {
            var prodSetup = ProdSetupRecord?.Current;
            if (prodSetup == null)
            {
                return;
            }

            var setupRowStatus = ProdSetupRecord.Cache.GetStatus(prodSetup);
            var oldSchdBlockSize = (int?)ProdSetupRecord.Cache.GetValueOriginal<AMPSetup.schdBlockSize>(prodSetup);
            var newBlockSize = prodSetup.SchdBlockSize;
            var blockSizeChanged = setupRowStatus == PXEntryStatus.Updated && newBlockSize != null && oldSchdBlockSize != null && newBlockSize != oldSchdBlockSize;

            if (blockSizeChanged)
            {
                //Postpone the change of block size until the SyncBlockSizeChange process completes (will setup table there)
                prodSetup.SchdBlockSize = oldSchdBlockSize;
                ProdSetupRecord.Update(prodSetup);
            }
            else if (setupRowStatus == PXEntryStatus.Inserted || setupRowStatus == PXEntryStatus.Updated)
            {
                var setup = (AMAPSMaintenanceSetup)PXSelect<AMAPSMaintenanceSetup>.Select(this);
                if (setup == null)
                {
                    Common.Cache.AddCacheView<AMAPSMaintenanceSetup>(this);
                    this.Caches<AMAPSMaintenanceSetup>().Insert();
                }
            }

            base.Persist();

            if (blockSizeChanged)
            {
                PXLongOperation.StartOperation(this, () =>
                {
                    SyncBlockSizeChange(newBlockSize, oldSchdBlockSize.GetValueOrDefault());
                });
            }
        }

        protected static void SyncBlockSizeChange(int? newBlockSize, int? oldBlockSize)
        {
            if (newBlockSize == null || oldBlockSize == null)
            {
                return;
            }

            APSMaintenanceProcess.UpdateBlockSizeProcess(newBlockSize.GetValueOrDefault(), oldBlockSize.GetValueOrDefault());
        }

        protected virtual void AMPSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (AMPSetup)e.Row;
            if (row == null)
            {
                return;
            }

            UpgradeButton.SetVisible(UpgradeProcess.NeedsUpgrade(row.UpgradeStatus));

            PXUIFieldAttribute.SetRequired<AMPSetup.fixMfgCalendarID>(cache, row.FMLTime.GetValueOrDefault());
            PXUIFieldAttribute.SetRequired<AMPSetup.fMLTimeUnits>(cache, false);
        }

        protected virtual void AMPSetup_SchdBlockSize_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var row = (AMPSetup) e.Row;
            if (row?.SchdBlockSize == null || IsImport || IsContractBasedAPI)
            {
                return;
            }

            var scheduleExists = (AMWCSchd)PXSelect<AMWCSchd>.SelectWindowed(this, 0, 1);

            if (scheduleExists == null)
            {
                return;
            }

            if (ProdSetupRecord.Ask(Messages.ChangingBlockSizeMsg, MessageButtons.YesNo) != WebDialogResult.Yes)
            {
                e.Cancel = true;
                e.NewValue = row.SchdBlockSize;
            }
        }

        protected virtual void AMPSetup_FMLTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var productionSetup = (AMPSetup)e.Row;
            if (productionSetup == null)
            {
                return;
            }

            if (productionSetup.FMLTime.GetValueOrDefault() && productionSetup.FMLTimeUnits == null)
            {
                //Default days
                productionSetup.FMLTimeUnits = Attributes.TimeUnits.Days;
            }
        }

        protected virtual void AMPSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var productionSetup = (AMPSetup)e.Row;
            if (productionSetup == null)
            {
                return;
            }

            if (productionSetup.FixMfgCalendarID == null && (productionSetup.FMLTime ?? false))
            {
                sender.RaiseExceptionHandling<AMPSetup.fixMfgCalendarID>(productionSetup, productionSetup.FixMfgCalendarID, new PXSetPropertyException(Messages.MissingFixMfgCalendar, PXErrorLevel.Error));
            }
        }

        public PXAction<AMPSetup> UpgradeButton;
        [PXUIField(DisplayName = Messages.Upgrade, MapEnableRights = PXCacheRights.Update, Visible = false)]
        [PXButton]
        protected System.Collections.IEnumerable upgradeButton(PXAdapter adapter)
        {

            if (this.ProdSetupRecord.Current != null)
            {
                if (this.IsDirty)
                {
                    this.Actions.PressSave();
                }

                UpgradeProcess.PerformUpgrade(this);
            }

            return adapter.Get();
        }
    }
}