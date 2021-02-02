using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class CostRollHistory : PXGraph<CostRollHistory>
    {
        [PXFilterable]
        public PXSelectJoin<AMBomCostHistory,
            LeftJoin<AMBomItem, On<AMBomCostHistory.bOMID, Equal<AMBomItem.bOMID>,
                And<AMBomCostHistory.revisionID, Equal<AMBomItem.revisionID>>>>> CostRollHistoryRecords;
        public PXSetup<AMBSetup> ambsetup;
        public PXSelect<AMBomItem> BomItemRecs;

        public CostRollHistory()
        {
            CostRollHistoryRecords.AllowDelete = false;
            CostRollHistoryRecords.AllowInsert = false;
            CostRollHistoryRecords.AllowUpdate = false;
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Current Status", Enabled = false)]
        [AMBomStatus.List]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        protected virtual void AMBomItem_Status_CacheAttached(PXCache sender)
        {
        }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Date")]
        [PXDateAndTime]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        protected virtual void AMBomCostHistory_CreatedDateTime_CacheAttached(PXCache sender)
        {
        }

        public PXAction<AMBomCostHistory> ViewBOM;
        [PXUIField(DisplayName = "View BOM")]
        [PXButton]
        protected virtual void viewBOM()
        {
            BOMMaint.Redirect(CostRollHistoryRecords?.Current?.BOMID, CostRollHistoryRecords?.Current?.RevisionID);
        }

    }
}