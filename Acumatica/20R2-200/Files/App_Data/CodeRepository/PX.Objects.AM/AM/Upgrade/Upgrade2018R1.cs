using Customization;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// Upgrade process for changes related to 2018R1 release
    /// </summary>
    internal sealed class Upgrade2018R1 : UpgradeProcessVersionBase
    {
        public Upgrade2018R1(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public Upgrade2018R1(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new Upgrade2018R1(upgradeGraph, plugin);

            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();
                return true;
            }
            return false;
        }

        public override int Version => UpgradeVersions.Version2018R1Ver00;
        public override void ProcessTables()
        {
            //ProcessAMConfigurationResults();
            ProcessAMEstimateReference();
            ProcessAMEstimatePrimary();
            ProcessAMEstimateSetup();
        }

        private void ProcessAMEstimateSetup()
        {
            TryUpdate<AMEstimateSetup>(
                new PXDataFieldAssign<AMEstimateSetup.copyEstimateFiles>(PXDbType.Bit, 1),
                new PXDataFieldAssign<AMEstimateSetup.copyEstimateNotes>(PXDbType.Bit, 1),
                new PXDataFieldAssign<AMEstimateSetup.newRevisionIsPrimary>(PXDbType.Bit, 1));
        }

        private void ProcessAMEstimatePrimary()
        {
            TryUpdate<Standalone.AMEstimatePrimary>(
                new PXDataFieldAssign<Standalone.AMEstimatePrimary.estimateStatus>(PXDbType.Int, EstimateStatus.Closed),
                new PXDataFieldRestrict<Standalone.AMEstimatePrimary.estimateStatus>(PXDbType.Int, 4, EstimateStatus.Completed, PXComp.EQ));
        }

        // Removing from upgrade for 2018R2 - 2018R2 upgrade script will auto set default value of 0 before upgrading values to R2
        //private void ProcessAMConfigurationResults()
        //{
        //    /*
        //     *  RevisionID is a new key for opportunities in 2018R1 - make sure if the configuration is linked to an opp that the revision is set to the default zero
        //     *
        //     *  UPDATE AMConfigurationResults 
        //     *  SET [opportunityRevisionID] = 0
        //     *  WHERE ([opportunityID] IS NOT NULL)
        //     *      AND CompanyID = 2;
        //     */

        //    TryUpdate<AMConfigurationResults>(
        //        new PXDataFieldAssign<AMConfigurationResults.opportunityRevisionID>(PXDbType.Int, 0),
        //        new PXDataFieldRestrict<AMConfigurationResults.opportunityID>(PXDbType.NVarChar, 10, null, PXComp.ISNOTNULL));
        //}

        private void ProcessAMEstimateReference()
        {
            /*  We added the OpportunityID field to the estimate reference in 2018R1. Previous version stored the opp number in the QuoteNbr field.
             *  We need to move the values and set revisionid defaults for those estimates related to opportunities
             *
             *  UPDATE AMEstimateReference 
             *  SET [opportunityID] = LEFT([quoteNbr], 10), 
             *     [opportunityRevisionID] = 0 
             *  WHERE ([source] = 2 AND [quoteNbr] IS NOT NULL) 
             *     AND CompanyID = 2
             */

            TryUpdate<AMEstimateReference>(
                new PXDataFieldAssign<AMEstimateReference.opportunityID>(PXDbType.DirectExpression, LeftCharOfFieldDirectSqlExpression(typeof(AMEstimateReference.quoteNbr), 10)),
                // Removing from upgrade for 2018R2 - 2018R2 upgrade script will auto set default value of 0 before upgrading values to R2
                //new PXDataFieldAssign<AMEstimateReference.opportunityRevisionID>(PXDbType.Int, 0),
#pragma warning disable CS0618 // Type or member is obsolete
                new PXDataFieldRestrict<AMEstimateReference.source>(PXDbType.Int, EstimateSource.Opportunity),
#pragma warning restore CS0618 // Type or member is obsolete
                new PXDataFieldRestrict<AMEstimateReference.quoteNbr>(PXDbType.NVarChar, 15, null, PXComp.ISNOTNULL));

            /*
             *  UPDATE AMEstimateReference
             *  SET [quoteNbr] = NULL
             *  WHERE ([source] = 2 AND [quoteNbr] IS NOT NULL)
             *      AND CompanyID = 2
             */

            TryUpdate<AMEstimateReference>(
                new PXDataFieldAssign<AMEstimateReference.quoteNbr>(PXDbType.NVarChar, 15, null),
#pragma warning disable CS0618 // Type or member is obsolete
                new PXDataFieldRestrict<AMEstimateReference.source>(PXDbType.Int, EstimateSource.Opportunity),
#pragma warning restore CS0618 // Type or member is obsolete
                new PXDataFieldRestrict<AMEstimateReference.quoteNbr>(PXDbType.NVarChar, 15, null, PXComp.ISNOTNULL));
        }
    }
}