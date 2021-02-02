using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateItem)]
    public class AMOrderEstimateItemFilter : AMEstimateItem
    {
        public new abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        //override to remove is key
        [EstimateIDSelectPrimary(typeof(Search<AMEstimateItem.estimateID, 
            Where<AMEstimateItem.revisionID, Equal<AMEstimateItem.primaryRevisionID>,
                And<AMEstimateItem.quoteSource, Equal<EstimateSource.estimate>,
                And<AMEstimateItem.estimateStatus, NotEqual<EstimateStatus.canceled>,
                    And<AMEstimateItem.estimateStatus, NotEqual<EstimateStatus.closed>>>>>>))]
        [EstimateID(Required = true)]
        public override String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }

        //Added Field for getting the Current Estimate in order to select Revision
        public abstract class currentEstimate : PX.Data.BQL.BqlString.Field<currentEstimate> { }

        protected String _CurrentEstimate;
        [PXString]
        [PXUIField(DisplayName = "Current Estimate")]
        public virtual String CurrentEstimate
        {
            get { return this._CurrentEstimate; }
            set { this._CurrentEstimate = value; }
        }

        public new abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        //override to remove is key
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXSelector(typeof(Search<AMEstimateItem.revisionID, Where<AMEstimateItem.estimateID, 
            Equal<Current<AMOrderEstimateItemFilter.currentEstimate>>>>),
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.revisionDate),
            typeof(AMEstimateItem.estimateStatus),
            typeof(AMEstimateItem.isPrimary), ValidateValue = false)]
        [PXUIField(DisplayName = "Revision", Required = true)]
        [PXFormula(typeof(Default<AMOrderEstimateItemFilter.estimateID>))]
        public override String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }

        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        protected int? _BranchID;
        [Branch(DisplayName = "Branch", IsDBField = false)]
        public virtual int? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }

        public abstract class addExisting : PX.Data.BQL.BqlBool.Field<addExisting> { }

        protected bool? _AddExisting;
        [PXUnboundDefault(false)]
        [PXBool]
        [PXUIField(DisplayName = "Add Existing")]
        public virtual bool? AddExisting
        {
            get
            {
                return this._AddExisting;
            }
            set
            {
                this._AddExisting = value;
            }
        }
    }
}
