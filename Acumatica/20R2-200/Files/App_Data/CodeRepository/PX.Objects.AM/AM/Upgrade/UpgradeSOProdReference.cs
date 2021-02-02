using Customization;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.SO;

namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// To correct exiting data related to bug 2390 we need to update AMSOLineSplitAMExtension
    /// </summary>
    internal sealed class UpgradeSOProdReference : UpgradeProcessVersionBase
    {
        public UpgradeSOProdReference(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public UpgradeSOProdReference(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public override int Version => UpgradeVersions.Version2019R1Ver01;

        protected override bool ProcessCompletedInPreviousVersion(int upgradeFrom)
        {
            return upgradeFrom.BetweenInclusive(UpgradeVersions.Version2018R1Ver1024, UpgradeVersions.MaxVersionNumbers.Version2018R1) 
                   || upgradeFrom.BetweenInclusive(UpgradeVersions.Version2018R2Ver43, UpgradeVersions.MaxVersionNumbers.Version2018R2);
        }

        public override void ProcessTables()
        {
            PXUpdateJoin<
                Set<SOLineSplitExt.aMOrderType, SOLineExt.aMOrderType,
                    Set<SOLineSplitExt.aMProdOrdID, SOLineExt.aMProdOrdID>>,
                SOLineSplit,
                InnerJoin<SOLine, 
                On<SOLineSplit.orderType, Equal<SOLine.orderType>,
                    And<SOLineSplit.orderNbr, Equal<SOLine.orderNbr>,
                    And<SOLineSplit.lineNbr, Equal<SOLine.lineNbr>>>>>,
                Where<SOLineSplitExt.aMProdCreate, Equal<True>,
                    And<SOLineSplitExt.aMProdOrdID, IsNull,
                    And<SOLineExt.aMProdOrdID, IsNotNull>>>
            >.Update(_upgradeGraph);
        }
    }
}