using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM.Standalone
{
    // New for 2018R1
    /// <summary>
    /// Primary Estimate record
    /// </summary>
    [PXHidden]
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("EstimateID = {EstimateID}; PrimaryRevisionID = {PrimaryRevisionID}")]
    public class AMEstimatePrimary : IBqlTable
    {
#if DEBUG
        //Developer note: 
        //      adding new fields: review logic in EstimateItemProjectionAttribute
#endif
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimatePrimary>.By<estimateID>
        {
            public static AMEstimatePrimary Find(PXGraph graph, string estimateID)
                => FindBy(graph, estimateID);
            public static AMEstimatePrimary FindDirty(PXGraph graph, string estimateID)
                => PXSelect<AMEstimatePrimary,
                        Where<estimateID, Equal<Required<estimateID>>>>
                        .SelectWindowed(graph, 0, 1, estimateID);
        }

        #endregion

        #region EstimateID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        [EstimateID(IsKey = true)]
        [PXDefault]
        public virtual String EstimateID { get; set; }
        #endregion
        #region PrimaryRevisionID
        public abstract class primaryRevisionID : PX.Data.BQL.BqlString.Field<primaryRevisionID> { }

        [RevisionIDField(DisplayName = "Primary Revision")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual String PrimaryRevisionID { get; set; }
        #endregion
        #region QuoteSource
        public abstract class quoteSource : PX.Data.BQL.BqlInt.Field<quoteSource> { }

        [PXDBInt]
        [PXDefault(EstimateSource.Estimate)]
        [PXUIField(DisplayName = "Quote Source", Enabled = false)]
        [EstimateSource.List]
        public virtual int? QuoteSource { get; set; }
        #endregion
        #region EstimateStatus
        public abstract class estimateStatus : PX.Data.BQL.BqlInt.Field<estimateStatus> { }

        [PXDBInt]
        [PXDefault(Attributes.EstimateStatus.NewStatus)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        [Attributes.EstimateStatus.List]
        public virtual int? EstimateStatus { get; set; }
        #endregion
        #region IsLockedByQuote
        /// <summary>
        /// When the estimate is linked to specific quote orders, the quote order will drive some fields such as mark as primary which should prevent the user from making changes on the estimate directly
        /// </summary>
        public abstract class isLockedByQuote : PX.Data.BQL.BqlBool.Field<isLockedByQuote> { }

        /// <summary>
        /// When the estimate is linked to specific quote orders, the quote order will drive some fields such as mark as primary which should prevent the user from making changes on the estimate directly
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Locked by Quote", Enabled = false, Visible = false)]
        public virtual Boolean? IsLockedByQuote { get; set; }
        #endregion
        #region LineCntrHistory
        public abstract class lineCntrHistory : PX.Data.BQL.BqlInt.Field<lineCntrHistory> { }

        protected int? _LineCntrHistory;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "History Line Cntr", Enabled = false, Visible = false)]
        public virtual int? LineCntrHistory
        {
            get
            {
                return this._LineCntrHistory;
            }
            set
            {
                this._LineCntrHistory = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
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
        [PXDBCreatedByScreenID()]
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
        [PXDBCreatedDateTime()]
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
        [PXDBLastModifiedByID()]
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
        [PXDBLastModifiedByScreenID()]
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
        [PXDBLastModifiedDateTime()]
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