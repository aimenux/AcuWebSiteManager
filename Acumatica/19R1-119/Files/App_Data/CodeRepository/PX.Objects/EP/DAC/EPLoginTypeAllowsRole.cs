using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.EP
{
    [Serializable]
	[PXCacheName(Messages.LoginTypeAllowsRole)]
	public partial class EPLoginTypeAllowsRole : IBqlTable
    {
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Default")]
		[PXDefault(false)]
		public bool? IsDefault { get; set; }
		#endregion
		#region LoginType
        public abstract class loginTypeID : PX.Data.BQL.BqlInt.Field<loginTypeID> { }
        [PXDBInt(IsKey = true)]
        [PXDBChildIdentity(typeof(EPLoginType.loginTypeID))]
        [PXParent(typeof(Select<EPLoginType, Where<EPLoginType.loginTypeID, Equal<EPLoginTypeAllowsRole.loginTypeID>>>))]
        [PXDefault(typeof(EPLoginType.loginTypeID))]
        [PXUIField(DisplayName = "User Type")]
        public virtual int? LoginTypeID { get; set; }
        #endregion
        #region Rolename
        public abstract class rolename : PX.Data.BQL.BqlString.Field<rolename> { }
        [PXDBString(64, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Role Name", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<Roles.rolename, Where<Current<EPLoginType.isExternal>, Equal<Roles.guest>, Or<Roles.guest, Equal<True>>>>), DescriptionField = typeof(Roles.descr))]
        public virtual String Rolename { get; set; }
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
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

        [PXDBLastModifiedDateTimeUtc]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
