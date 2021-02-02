using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSReasonFilter : PX.Data.IBqlTable
    {
        #region ReasonType
        public abstract class reasonType : ListField_ReasonType
        {
        }

        [PXString(4, IsFixed = true)]
        [reasonType.ListAtrribute]
        [PXDefault(ID.ReasonType.CANCEL_APPOINTMENT)]
        [PXUIField(DisplayName = "Reason Type")]
        public virtual string ReasonType { get; set; }
        #endregion    
        #region WFID
        public abstract class wFID : PX.Data.BQL.BqlInt.Field<wFID> { }

        [PXInt]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorWorkflow]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WFID { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXInt]
        [PXUIField(DisplayName = "Workflow Stage")]
        [FSSelectorWorkflowStageInReason]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WFStageID { get; set; }
        #endregion
    }
}
