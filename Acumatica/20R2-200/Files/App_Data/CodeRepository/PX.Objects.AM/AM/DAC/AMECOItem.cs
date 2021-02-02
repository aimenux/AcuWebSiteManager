using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// ECR Item (Master ECR Header Record)
    /// </summary>
    [PXEMailSource]
    [Serializable]
    [PXCacheName(Messages.ECOItem)]
    [PXPrimaryGraph(typeof(ECOMaint))]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMECOItem : IBqlTable, PX.Data.EP.IAssign
    {
        internal string DebuggerDisplay => $"ECOID = {ECOID}, BOMID = {BOMID}, RevisionID = {RevisionID}, InventoryID = {InventoryID}, SiteID = {SiteID}";

        #region ECOID
        public abstract class eCOID : PX.Data.BQL.BqlString.Field<eCOID> { }
        protected string _ECOID;
        [ECOID(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [AutoNumber(typeof(AMBSetup.eCONumberingID), typeof(AMECOItem.requestDate))]
        [PXSelector(typeof(Search<AMECOItem.eCOID>))]
        [PXDefault]
        public virtual string ECOID
        {
            get
            {
                return this._ECOID;
            }
            set
            {
                this._ECOID = value;
            }
        }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
        protected string _RevisionID;
        [PXDefault]
        [RevisionIDField(Enabled = false, Visible = false)]
        public virtual string RevisionID
        {
            get
            {
                return this._RevisionID;
            }
            set
            {
                this._RevisionID = value;
            }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected string _BOMID;
        [BomID(Required = true, Visibility = PXUIVisibility.SelectorVisible, Enabled =false)]
        [PXDefault]
        public virtual string BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
        #region BOMRevisionID
        public abstract class bOMRevisionID : PX.Data.BQL.BqlString.Field<bOMRevisionID> { }
        protected string _BOMRevisionID;
        [PXDefault]
        [RevisionIDField(DisplayName = "BOM Revision", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMECOItem.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.status)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
        [PXForeignReference(typeof(CompositeKey<Field<AMECOItem.bOMID>.IsRelatedTo<AMBomItem.bOMID>, Field<AMECOItem.bOMRevisionID>.IsRelatedTo<AMBomItem.revisionID>>))]
        public virtual string BOMRevisionID
        {
            get
            {
                return this._BOMRevisionID;
            }
            set
            {
                this._BOMRevisionID = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual String Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion        
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [StockItem(Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [SubItem(typeof(AMBomItem.inventoryID), Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXSearchable(PX.Objects.SM.SearchCategory.IN, Messages.BOMSearchableTitleDocument, new[] { typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID) },
            new Type[] { typeof(AMBomItem.descr) },
            NumberFields = new Type[] { typeof(AMBomItem.bOMID) },
            Line1Format = "{1}{2:d}", Line1Fields = new Type[] { typeof(AMBomItem.inventoryID), typeof(InventoryItem.inventoryCD), typeof(AMBomItem.effStartDate) },
            Line2Format = "{0}", Line2Fields = new Type[] { typeof(AMBomItem.descr) }
        )]
        [PXNote(DescriptionField = typeof(bOMID), Selector = typeof(bOMID))]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        [Site(Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }
        protected Int32? _LineCntrAttribute;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrAttribute
        {
            get
            {
                return this._LineCntrAttribute;
            }
            set
            {
                this._LineCntrAttribute = value;
            }
        }
        #endregion
        #region LineCntrOperation
        public abstract class lineCntrOperation : PX.Data.BQL.BqlInt.Field<lineCntrOperation> { }
        protected int? _LineCntrOperation;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Operation Line Cntr", Enabled = false, Visible = false)]
        public virtual int? LineCntrOperation
        {
            get
            {
                return this._LineCntrOperation;
            }
            set
            {
                this._LineCntrOperation = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByte.Field<Tstamp> { }
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
        #region Status
        public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }
        protected int? _Status;
        [PXDBInt]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [AMECRStatus.List]
        [PXDefault(AMECRStatus.Hold)]
        public virtual int? Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region OwnerID
        public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
        protected int? _OwnerID;
        //[PXDefault(typeof(Coalesce<
        //    Search<CREmployee.userID, Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>, And<CREmployee.status, NotEqual<BAccount.status.inactive>>>>,
        //    Search<BAccount.ownerID, Where<BAccount.bAccountID, Equal<Current<SOOrder.customerID>>>>>),
        //    PersistingCheck = PXPersistingCheck.Nothing)]
        [PX.TM.Owner]
        public virtual int? OwnerID
        {
            get
            {
                return this._OwnerID;
            }
            set
            {
                this._OwnerID = value;
            }
        }
        #endregion
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
        protected int? _WorkgroupID;
        [PXDBInt]
        //[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PX.TM.PXCompanyTreeSelector]
        [PXUIField(DisplayName = "Workgroup", Enabled = false)]
        public virtual int? WorkgroupID
        {
            get
            {
                return this._WorkgroupID;
            }
            set
            {
                this._WorkgroupID = value;
            }
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Hold")]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region Approved
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        protected Boolean? _Approved;
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Boolean? Approved
        {
            get
            {
                return this._Approved;
            }
            set
            {
                this._Approved = value;
            }
        }
        #endregion
        #region Rejected
        public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
        protected bool? _Rejected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Rejected
        {
            get
            {
                return _Rejected;
            }
            set
            {
                _Rejected = value;
            }
        }
        #endregion
        #region Requestor
        public abstract class requestor : PX.Data.BQL.BqlInt.Field<requestor> { }
        protected Int32? _Requestor;

        [PXDBInt]
        [ProductionEmployeeSelector]
        [PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
        [PXUIField(DisplayName = "Requestor")]
        public virtual Int32? Requestor
        {
            get
            {
                return this._Requestor;
            }
            set
            {
                this._Requestor = value;
            }
        }
        #endregion
        #region Priority
        public abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }
        protected int? _Priority;
        [PXDBInt]
        [PXUIField(DisplayName = "Priority", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault(0)]
        public virtual int? Priority
        {
            get
            {
                return this._Priority;
            }
            set
            {
                this._Priority = value;
            }
        }
        #endregion
        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
        protected DateTime? _EffectiveDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Effective Date")]
        public virtual DateTime? EffectiveDate
        {
            get
            {
                return this._EffectiveDate;
            }
            set
            {
                this._EffectiveDate = value;
            }
        }
        #endregion
        #region RequestDate
        public abstract class requestDate : PX.Data.BQL.BqlDateTime.Field<requestDate> { }
        protected DateTime? _RequestDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Request Date")]
        public virtual DateTime? RequestDate
        {
            get
            {
                return this._RequestDate;
            }
            set
            {
                this._RequestDate = value;
            }
        }
        #endregion

        /// <summary>
        /// Constant Revision ID Value for all ECO Item records. A unique value to prevent any key violations.
        /// </summary>
        public const string ECORev = "-ECO";

        /// <summary>
        /// BQL Constant Revision ID Value for all ECO Item records. A unique value to prevent any key violations.
        /// </summary>
        public sealed class eCORev : PX.Data.BQL.BqlString.Constant<eCORev>
        {
            public eCORev() : base(ECORev) { }
        }
    }
}