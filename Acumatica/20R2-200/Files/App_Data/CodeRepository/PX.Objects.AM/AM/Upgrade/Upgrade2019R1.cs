using Customization;

namespace PX.Objects.AM.Upgrade
{
    internal sealed class Upgrade2019R1 : UpgradeProcessVersionBase
    {
        public Upgrade2019R1(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : base(upgradeGraph, plugin)
        {
        }

        public Upgrade2019R1(UpgradeProcess upgradeGraph) : base(upgradeGraph)
        {
        }

        public override int Version => UpgradeVersions.Version2019R1Ver02;

        public override void ProcessTables()
        {
            // 18R2 Trim Updates
            ProcessRTrimUpdate<AMBomRef, AMBomRef.descr>();
            ProcessRTrimUpdate<AMConfigurationResults, AMConfigurationResults.prodOrderNbr>();
            ProcessRTrimUpdate<AMProdItem, AMProdItem.ordNbr>();
            ProcessRTrimUpdate<AMProdOper, AMProdOper.phtmBOMID>();
            ProcessRTrimUpdate<AMProdOper, AMProdOper.phtmMatlBOMID>();
            ProcessRTrimUpdate<AMProdOvhd, AMProdOvhd.phtmBOMID>();
            ProcessRTrimUpdate<AMProdOvhd, AMProdOvhd.phtmMatlBOMID>();
            ProcessRTrimUpdate<AMProdStep, AMProdStep.phtmBOMID>();
            ProcessRTrimUpdate<AMProdStep, AMProdStep.phtmMatlBOMID>();
            ProcessRTrimUpdate<AMProdTool, AMProdTool.phtmBOMID>();
            ProcessRTrimUpdate<AMProdTool, AMProdTool.phtmMatlBOMID>();
            ProcessRTrimUpdate<AMWCSchdDetail, AMWCSchdDetail.shiftID>();

            //  19R1 Fixed Fields to Var
            ProcessRTrimUpdate<AMConfigurationFeature, AMConfigurationFeature.featureID>();
            ProcessRTrimUpdate<AMEstimateOper, AMEstimateOper.description>();
            ProcessRTrimUpdate<AMEstimateOvhd, AMEstimateOvhd.description>();
            ProcessRTrimUpdate<AMEstimateTool, AMEstimateTool.description>();
            ProcessRTrimUpdate<AMEstimateTool, AMEstimateTool.toolID>();
            ProcessRTrimUpdate<AMFeature, AMFeature.featureID>();
            ProcessRTrimUpdate<AMFeatureAttribute, AMFeatureAttribute.featureID>();
            ProcessRTrimUpdate<AMFeatureOption, AMFeatureOption.featureID>();
            ProcessRTrimUpdate<AMLaborCode, AMLaborCode.descr>();
            ProcessRTrimUpdate<AMMach, AMMach.assetID>();
            ProcessRTrimUpdate<AMMach, AMMach.descr>();
            ProcessRTrimUpdate<AMMPSType, AMMPSType.descr>();
            ProcessRTrimUpdate<AMMRPBucket, AMMRPBucket.descr>();
            ProcessRTrimUpdate<AMOrderType, AMOrderType.descr>();
            ProcessRTrimUpdate<AMProdOper, AMProdOper.descr>();
            ProcessRTrimUpdate<AMProdStep, AMProdStep.descr>();
            ProcessRTrimUpdate<AMProdTool, AMProdTool.descr>();
            ProcessRTrimUpdate<AMProdTool, AMProdTool.toolID>();
        }
    }
}