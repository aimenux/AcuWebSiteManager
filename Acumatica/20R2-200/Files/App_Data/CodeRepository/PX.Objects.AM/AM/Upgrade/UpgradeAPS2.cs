using Customization;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class UpgradeAPS2 : UpgradeProcessVersionBase
    {
        public UpgradeAPS2(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public UpgradeAPS2(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public override int Version => UpgradeVersions.Version2019R1Ver00;

        protected override bool ProcessCompletedInPreviousVersion(int upgradeFrom)
        {
            return false;
            //return upgradeFrom.BetweenInclusive(UpgradeVersions.Version2018R2Ver43, UpgradeVersions.MaxVersionNumbers.Version2018R2);
        }

        public override void ProcessTables()
        {
            //New field schdEfficiencyTime added - to initialize we will set equal to schdTime
            CopyFieldValueWhereDecimalZero<AMWCSchd, AMWCSchd.schdEfficiencyTime, AMWCSchd.schdTime>();
            CopyFieldValueWhereDecimalZero<AMWCSchdDetail, AMWCSchdDetail.schdEfficiencyTime, AMWCSchdDetail.schdTime>();
            CopyFieldValueWhereDecimalZero<AMSchdOperDetail, AMSchdOperDetail.schdEfficiencyTime, AMSchdOperDetail.schdTime>();
        }
    }
}