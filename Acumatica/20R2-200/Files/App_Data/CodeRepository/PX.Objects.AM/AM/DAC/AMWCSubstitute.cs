using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Work Center Substitute Record
    /// </summary>
	[Serializable]
    [PXCacheName(Messages.WorkCenterSubstitute)]
    [PXPrimaryGraph(typeof(WCMaint))]
    public class AMWCSubstitute : IBqlTable
    {
        #region WcID
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMWC.wcID))]
        [PXParent(typeof(Select<AMWC, Where<AMWC.wcID, Equal<Current<AMWCSubstitute.wcID>>>>))]
        public virtual String WcID
        {
            get
            {
                return this._WcID;
            }
            set
            {
                this._WcID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;

        [Site(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXRestrictor(typeof(Where<INSite.siteID, NotEqual<Current<AMWC.siteID>>>), Messages.WarehouseIsCurrentWorkCenterWarehouse)]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
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
        #region SubstituteWcID
        public abstract class substituteWcID : PX.Data.BQL.BqlString.Field<substituteWcID> { }

        protected String _SubstituteWcID;

        [WorkCenterIDField(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Substitute Work Center")]
        [PXDefault]
        [PXRestrictor(typeof(Where<AMWC.wcID, NotEqual<Current<AMWC.wcID>>>), Messages.InvalidWorkCenterSubstitute)]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        public virtual String SubstituteWcID
        {
            get
            {
                return this._SubstituteWcID;
            }
            set
            {
                this._SubstituteWcID = value;
            }
        }
        #endregion
        #region UpdateOperDesc
        public abstract class updateOperDesc : PX.Data.BQL.BqlBool.Field<updateOperDesc> { }

        protected Boolean? _UpdateOperDesc;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Update Operation Description")]
        public virtual Boolean? UpdateOperDesc
        {
            get
            {
                return this._UpdateOperDesc;
            }
            set
            {
                this._UpdateOperDesc = value;
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
    }
}
