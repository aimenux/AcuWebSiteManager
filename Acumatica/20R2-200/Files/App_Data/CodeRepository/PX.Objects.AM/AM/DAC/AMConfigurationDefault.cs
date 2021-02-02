using System;
using PX.Data;

namespace PX.Objects.AM
{
#if DEBUG
    // 
    /// <summary>
    /// Manufacturing Configuration Defaults Not Implemented
    /// </summary>
#endif
    [Serializable]
    [PXHidden]
	public class AMConfigurationDefault : IBqlTable
	{
		#region ConfigurationID
		public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

		protected string _ConfigurationID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault(typeof(AMConfiguration.configurationID))]
        [PXParent(typeof(Select<AMConfiguration, Where<AMConfiguration.configurationID, Equal<Current<AMConfigurationDefault.configurationID>>,
            And<AMConfiguration.revision, Equal<Current<AMConfigurationDefault.revision>>>>>))]
        public virtual string ConfigurationID
		{
			get
			{
				return this._ConfigurationID;
			}
			set
			{
				this._ConfigurationID = value;
			}
		}
		#endregion
		#region Revision
		public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }

		protected string _Revision;
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault(typeof(AMConfiguratorSetup.dfltRevisionNbr))]
        public virtual string Revision
		{
			get
			{
				return this._Revision;
			}
			set
			{
				this._Revision = value;
			}
		}
		#endregion
		#region DefaultID
		public abstract class defaultID : PX.Data.BQL.BqlString.Field<defaultID> { }

		protected string _DefaultID;
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "DefaultID")]
		public virtual string DefaultID
		{
			get
			{
				return this._DefaultID;
			}
			set
			{
				this._DefaultID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		protected DateTime? _StartDate;
		[PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "StartDate")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		protected DateTime? _EndDate;
		[PXDBDate]
		[PXUIField(DisplayName = "EndDate")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
        #endregion

        #region System Fields
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
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID
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
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected byte[] _tstamp;
        [PXDBTimestamp]
        public virtual byte[] tstamp
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
        #endregion
    }
}