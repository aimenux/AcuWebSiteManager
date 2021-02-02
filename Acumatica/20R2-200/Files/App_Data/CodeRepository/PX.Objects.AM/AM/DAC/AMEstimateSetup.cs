using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateSetup)]
    [PXPrimaryGraph(typeof(EstimateSetup))]
	public class AMEstimateSetup : IBqlTable
	{
        #region Estimate Numbering ID
        public abstract class estimateNumberingID : PX.Data.BQL.BqlString.Field<estimateNumberingID> { }

        protected String _EstimateNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "Estimate Number Sequence")]
        public virtual String EstimateNumberingID
        {
            get
            {
                return this._EstimateNumberingID;
            }
            set
            {
                this._EstimateNumberingID = value;
            }
        }
        #endregion
        #region Default Revision ID
        public abstract class defaultRevisionID : PX.Data.BQL.BqlString.Field<defaultRevisionID> { }

        protected String _DefaultRevisionID;
        [RevisionIDField(DisplayName = "Default Revision")]
        public virtual String DefaultRevisionID
        {
            get
            {
                return this._DefaultRevisionID;
            }
            set
            {
                this._DefaultRevisionID = value;
            }
        }
        #endregion
        #region AutoNumber Revision ID
        public abstract class autoNumberRevisionID : PX.Data.BQL.BqlBool.Field<autoNumberRevisionID> { }

        protected Boolean? _AutoNumberRevisionID;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Auto Number Revisions", Enabled = false, Visible = false)]
        public virtual Boolean? AutoNumberRevisionID
        {
            get
            {
                return this._AutoNumberRevisionID;
            }
            set
            {
                this._AutoNumberRevisionID = value;
            }
        }
        #endregion
        #region Default Estimate Class ID
        public abstract class defaultEstimateClassID : PX.Data.BQL.BqlString.Field<defaultEstimateClassID> { }

        protected String _DefaultEstimateClassID;
	    [PXDBString(20, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Default Estimate Class")]
        [PXSelector(typeof(Search<AMEstimateClass.estimateClassID>))]
        public virtual String DefaultEstimateClassID
        {
            get
            {
                return this._DefaultEstimateClassID;
            }
            set
            {
                this._DefaultEstimateClassID = value;
            }
        }
        #endregion
        #region Default Work Center ID
        public abstract class defaultWorkCenterID : PX.Data.BQL.BqlString.Field<defaultWorkCenterID> { }

        protected String _DefaultWorkCenterID;
        [WorkCenterIDField(DisplayName = "Default Work Center")]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        [PXForeignReference(typeof(Field<AMEstimateSetup.defaultWorkCenterID>.IsRelatedTo<AMWC.wcID>))]
        public virtual String DefaultWorkCenterID
        {
            get
            {
                return this._DefaultWorkCenterID;
            }
            set
            {
                this._DefaultWorkCenterID = value;
            }
        }
        #endregion
        #region DefaultOrderType
        public abstract class defaultOrderType : PX.Data.BQL.BqlString.Field<defaultOrderType> { }

        protected String _DefaultOrderType;
        [AMOrderTypeField(DisplayName = "Default Prod. Order Type")]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        [PXDefault(typeof(AMPSetup.defaultOrderType), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String DefaultOrderType
        {
            get
            {
                return this._DefaultOrderType;
            }
            set
            {
                this._DefaultOrderType = value;
            }
        }
        #endregion
	    #region NewRevisionIsPrimary
        /// <summary>
        /// During new revision of an estimate, should the new revision automatically be marked as the primary revision
        /// </summary>
	    public abstract class newRevisionIsPrimary : PX.Data.BQL.BqlBool.Field<newRevisionIsPrimary> { }

	    protected Boolean? _NewRevisionIsPrimary;
	    /// <summary>
	    /// During new revision of an estimate, should the new revision automatically be marked as the primary revision
	    /// </summary>
	    [PXDBBool]
	    [PXDefault(true)]
	    [PXUIField(DisplayName = "New Revision Is Primary")]
	    public virtual Boolean? NewRevisionIsPrimary
        {
	        get
	        {
	            return this._NewRevisionIsPrimary;
	        }
	        set
	        {
	            this._NewRevisionIsPrimary = value;
	        }
	    }
	    #endregion
        #region Copy Estimate Notes
        public abstract class copyEstimateNotes : PX.Data.BQL.BqlBool.Field<copyEstimateNotes> { }

        protected Boolean? _CopyEstimateNotes;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Copy Estimate Notes")]
        public virtual Boolean? CopyEstimateNotes
        {
            get
            {
                return this._CopyEstimateNotes;
            }
            set
            {
                this._CopyEstimateNotes = value;
            }
        }
        #endregion
        #region Copy Estimate Files
        public abstract class copyEstimateFiles : PX.Data.BQL.BqlBool.Field<copyEstimateFiles> { }

        protected Boolean? _CopyEstimateFiles;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Copy Estimate Files")]
        public virtual Boolean? CopyEstimateFiles
        {
            get
            {
                return this._CopyEstimateFiles;
            }
            set
            {
                this._CopyEstimateFiles = value;
            }
        }
        #endregion
        #region Copy Operation Notes
        public abstract class copyOperationNotes : PX.Data.BQL.BqlBool.Field<copyOperationNotes> { }

        protected Boolean? _CopyOperationNotes;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Operation Notes")]
        public virtual Boolean? CopyOperationNotes
        {
            get
            {
                return this._CopyOperationNotes;
            }
            set
            {
                this._CopyOperationNotes = value;
            }
        }
        #endregion
        #region Copy Operation Files
        public abstract class copyOperationFiles : PX.Data.BQL.BqlBool.Field<copyOperationFiles> { }

        protected Boolean? _CopyOperationFiles;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copy Operation Files")]
        public virtual Boolean? CopyOperationFiles
        {
            get
            {
                return this._CopyOperationFiles;
            }
            set
            {
                this._CopyOperationFiles = value;
            }
        }
        #endregion
        #region Inventory ID Override
        public abstract class inventoryIDOverride : PX.Data.BQL.BqlBool.Field<inventoryIDOverride> { }

        protected Boolean? _InventoryIDOverride;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override Inventory ID")]
        public virtual Boolean? InventoryIDOverride
        {
            get
            {
                return this._InventoryIDOverride;
            }
            set
            {
                this._InventoryIDOverride = value;
            }
        }
        #endregion
        #region Update All Revisions
        public abstract class updateAllRevisions : PX.Data.BQL.BqlBool.Field<updateAllRevisions> { }

        protected Boolean? _UpdateAllRevisions;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Update All Revisions")]
        public virtual Boolean? UpdateAllRevisions
        {
            get
            {
                return this._UpdateAllRevisions;
            }
            set
            {
                this._UpdateAllRevisions = value;
            }
        }
        #endregion
        #region Update Price Info
        public abstract class updatePriceInfo : PX.Data.BQL.BqlBool.Field<updatePriceInfo> { }

        protected Boolean? _UpdatePriceInfo;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Update Price Info")]
        public virtual Boolean? UpdatePriceInfo
        {
            get
            {
                return this._UpdatePriceInfo;
            }
            set
            {
                this._UpdatePriceInfo = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
    }
}
