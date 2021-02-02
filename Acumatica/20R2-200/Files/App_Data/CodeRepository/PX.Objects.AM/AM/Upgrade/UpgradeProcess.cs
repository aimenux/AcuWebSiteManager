using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using Customization;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// Manufacturing graph used for upgrades
    /// </summary>
    public sealed class UpgradeProcess : PXGraph<UpgradeProcess>
    {
        public PXSelect<AMPSetup> ProductionSetup;
        public PXSelect<AMProdItem> ProdItem;
        public PXSelect<AMProdItemSplit, Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdItemSplits;
        public PXSelect<AMProdEvnt, Where<AMProdEvnt.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdEvnt.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdEventRecords;
        public PXSelect<AMProdTotal, Where<AMProdTotal.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdTotal.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdTotalRecs;
        public PXSelect<AMProdOper, Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdOperRecords;
        public PXSelect<AMProdMatl,
            Where<AMProdMatl.orderType, Equal<Current<AMProdOper.orderType>>, 
                And<AMProdMatl.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, 
                And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProdMatlRecords;

        public PXSelect<AMBatch> Document;
        public PXSelect<AMMTran, Where<AMMTran.docType, Equal<Current<AMBatch.docType>>, And<AMMTran.batNbr, Equal<Current<AMBatch.batNbr>>>>> Transactions;

        public PXSelect<AMBomItem> BomItem;
        public PXSelect<AMBomOper> BomOper;
        public PXSelect<AMBomMatl> BomMatl;
        public PXSelect<AMBomStep> BomStep;
        public PXSelect<AMBomTool> BomTool;
        public PXSelect<AMBomOvhd> BomOvhd;

        public override int Persist(Type cacheType, PXDBOperation operation)
        {
            try
            {
                return base.Persist(cacheType, operation);
            }
            catch (Exception e)
            {
                PXTrace.WriteInformation($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; Error: {e.Message}");
                throw;
            }
        }

        public void ClearAll()
        {
            Clear(PXClearOption.ClearAll);
            ProductionSetup.Current = ProductionSetup.Select();
        }

        #region CACHEATTACHED / EVENTS

        [AMOrderTypeField]
        public void AMMTran_OrderType_CacheAttached(PXCache sender)
        {
        }

        [ProductionNbr]
        public void AMMTran_ProdOrdID_CacheAttached(PXCache sender)
        {
        }

        [OperationIDField(DisplayName = "Tran Operation ID")]
        public void AMMTran_OperationID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Tran Project")]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public void AMMTran_ProjectID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID")]
        public void AMMTran_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMMTran_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Prod Item Project")]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public void AMProdItem_ProjectID_CacheAttached(PXCache sender)
        {
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public void AMProdItem_UpdateProject_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Inventory ID")]
        public void AMProdItem_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdItem_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdItem_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdItem_LocationID_CacheAttached(PXCache sender)
        {
        }

        [RevisionIDField(DisplayName = "BOM Revision")]
        public void AMProdItem_BOMRevisionID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Inventory ID")]
        public void AMProdItemSplit_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdItemSplit_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdItemSplit_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdItemSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Inventory ID")]
        public void AMProdMatl_InventoryID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdMatl_SubItemID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdMatl_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt]
        public void AMProdMatl_LocationID_CacheAttached(PXCache sender)
        {
        }

        // Allow blank due to old records potentially containing blanks. we don't want to limit upgrading other parts of an operation record due to blank work center. 
        // Another process can focus on fixing the blank work center
        [WorkCenterIDField]
        [PXDefault(typeof(Search<AMBSetup.wcID>), PersistingCheck = PXPersistingCheck.Null)]
        public void AMProdOper_WcID_CacheAttached(PXCache sender)
        {
            // Changing PersistingCheck from NullorBlank to just Null
        }

        #endregion

        /// <summary>
        /// Start to perform the update process from a graph
        /// </summary>
        /// <param name="callingGraph">Calling graph requesting the upgrade</param>
        public static void PerformUpgrade(PXGraph callingGraph)
        {
            PXLongOperation.StartOperation(callingGraph, () =>
            {
                PXLongOperationHelper.SetCustomInfoCompanyID();
                PXLongOperationHelper.CheckForProcessIsRunningByCompany(callingGraph);

                RunUpgrade(null);
            });
        }

        /// <summary>
        /// Perform the upgrade from a customization plugin
        /// </summary>
        /// <param name="plugin"></param>
        public static void PerformUpgrade(CustomizationPlugin plugin)
        {
            if (plugin == null)
            {
                throw new PXArgumentException(nameof(plugin));
            }

            var companyMsg = string.IsNullOrWhiteSpace(Common.Current.CompanyName) ? string.Empty : Messages.GetLocal(Messages.InCompany, Common.Current.CompanyName);
            UpgradeHelper.WriteCstInfoOnly(plugin, Messages.GetLocal(Messages.CheckingForMfgVersionUpdates, Common.AMVersionNumber, companyMsg));

            var updatesExecuted = RunUpgrade(plugin);

            if (!updatesExecuted)
            {
                UpgradeHelper.WriteCstInfoOnly(plugin, Messages.GetLocal(Messages.NoUpdatesForMfgVersionUpdates, Common.AMVersionNumber, companyMsg));
            }
        }

        private static bool RunUpgrade(CustomizationPlugin plugin)
        {
            var upgradeProcessGraph = PXGraph.CreateInstance<UpgradeProcess>();
            AMPSetup ampSetup = PXSelect<AMPSetup>.Select(upgradeProcessGraph);
            var updatesExecuted = false;
            if (ampSetup == null || !NeedsUpgrade(ampSetup.UpgradeStatus))
            {
                return updatesExecuted;
            }

            upgradeProcessGraph.ProductionSetup.Current = ampSetup;

            var upgradeFrom = ampSetup.UpgradeStatus.GetValueOrDefault();

            // START ...

            //
            //  START OF 2018R1 UPGRADES ...
            //
            updatesExecuted |= Upgrade2018R1.Process(upgradeProcessGraph, plugin, upgradeFrom);
            //Same as 2017R2Ver49
            updatesExecuted |= Upgrade2018R1Ver27.Process(upgradeProcessGraph, plugin, upgradeFrom);
            //Same as 2017R2Ver50
            updatesExecuted |= Upgrade2018R1Ver29.Process(upgradeProcessGraph, plugin, upgradeFrom);

            //
            //  START OF 2018R2 UPGRADES ...
            //
            updatesExecuted |= UpgradeProject.Process(upgradeProcessGraph, plugin, upgradeFrom);
            updatesExecuted |= UpgradeAPS.Process(upgradeProcessGraph, plugin, upgradeFrom);
            updatesExecuted |= Upgrade2018R2.Process(upgradeProcessGraph, plugin, upgradeFrom);
            updatesExecuted |= UpgradeOperationNote.Process(upgradeProcessGraph, plugin, upgradeFrom);
            updatesExecuted |= UpgradeBomMatlLineCntrRef.Process(upgradeProcessGraph, plugin, upgradeFrom);
            updatesExecuted |= UpgradeLaborTranTimeCardStatus.Process(upgradeProcessGraph, plugin, upgradeFrom);

            //
            //  START OF 2019R1 UPGRADES ...
            //
            updatesExecuted |= UpgradeProcessVersionBase.Process(new UpgradeAPS2(upgradeProcessGraph, plugin), upgradeFrom);
            updatesExecuted |= UpgradeProcessVersionBase.Process(new UpgradeSOProdReference(upgradeProcessGraph, plugin), upgradeFrom);
            updatesExecuted |= UpgradeProcessVersionBase.Process(new Upgrade2019R1(upgradeProcessGraph, plugin), upgradeFrom);
            updatesExecuted |= UpgradeProcessVersionBase.Process(new UpgradeBomNoteIDs(upgradeProcessGraph, plugin), upgradeFrom);
            updatesExecuted |= UpgradeProcessVersionBase.Process(new UpgradeClosedProductionAllocations(upgradeProcessGraph, plugin), upgradeFrom);

            //
            //  START OF 2019R2 UPGRADES ...
            //
            updatesExecuted |= UpgradeProcessVersionBase.Process(new Upgrade2019R2(upgradeProcessGraph, plugin), upgradeFrom);
            updatesExecuted |= UpgradeProcessVersionBase.Process(new UpgradeConfigResultsOption(upgradeProcessGraph, plugin), upgradeFrom);

            //
            //  START OF 2020R1 UPGRADES ...
            //
            updatesExecuted |= UpgradeProcessVersionBase.Process(new UpgradeSubcontractSource(upgradeProcessGraph, plugin), upgradeFrom);

            // FINISHED

            upgradeProcessGraph.ProductionSetup.Current = upgradeProcessGraph.ProductionSetup.Select();
            return updatesExecuted;
        }

        /// <summary>
        /// Indicates of an upgrade is required
        /// </summary>
        /// <param name="upgradeStatusVersion">Current upgrade status version found within a module setup table</param>
        /// <returns>True when upgrade is necessary</returns>
        public static bool NeedsUpgrade(int? upgradeStatusVersion)
        {
            return upgradeStatusVersion.GetValueOrDefault() < UpgradeVersions.Version2019R2Ver07;
        }
    }
}