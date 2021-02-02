using Customization;
using PX.Data;
using System;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class UpgradeBomMatlLineCntrRef : UpgradeProcessVersionBase
    {
        public UpgradeBomMatlLineCntrRef(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public UpgradeBomMatlLineCntrRef(UpgradeProcess upgradeGraph, CustomizationPlugin plugin)
            : base(upgradeGraph, plugin)
        {
        }

        public static bool Process(UpgradeProcess upgradeGraph, CustomizationPlugin plugin, int upgradeFrom)
        {
            var upgrade = new UpgradeBomMatlLineCntrRef(upgradeGraph, plugin);

            if (upgradeFrom < upgrade.Version)
            {
                upgrade.Process();

                return true;
            }
            return false;
        }

        public override int Version => UpgradeVersions.Version2018R2Ver21;
        public override void ProcessTables()
        {
            UpdateLineCounter();
        }

        private void UpdateLineCounter()
        {
            PXUpdateJoin<
                Set<AMBomMatl.lineCntrRef, AMBomRefMaxLineCntr.lineID>,
                AMBomMatl,
                InnerJoin<AMBomRefMaxLineCntr, On<AMBomMatl.bOMID, Equal<AMBomRefMaxLineCntr.bOMID>,
                And<AMBomMatl.revisionID, Equal<AMBomRefMaxLineCntr.revisionID>,
                And<AMBomMatl.operationID, Equal<AMBomRefMaxLineCntr.operationID>,
                And<AMBomMatl.lineID, Equal<AMBomRefMaxLineCntr.matlLineID>>>>>>>.Update(_upgradeGraph);
        }
    }

    [Serializable]
    [PXHidden]
    [PXProjection(typeof(Select4<AMBomRef,
            Aggregate<
                GroupBy<AMBomRef.bOMID,
                    GroupBy<AMBomRef.revisionID,
                         GroupBy<AMBomRef.operationID,
                             GroupBy<AMBomRef.matlLineID>>>>>>), Persistent = false)]
    public class AMBomRefMaxLineCntr : AMBomRef
    {
        public new abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        public new abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
        public new abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }
        public new abstract class matlLineID : PX.Data.BQL.BqlInt.Field<matlLineID> { }
        public new abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }
    }
}