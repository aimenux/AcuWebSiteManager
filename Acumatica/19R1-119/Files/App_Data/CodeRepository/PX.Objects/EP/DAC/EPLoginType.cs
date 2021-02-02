using System;
using PX.Data;

namespace PX.Objects.EP
{
    [Serializable]
	[PXPrimaryGraph(typeof(EPLoginTypeMaint))]
	[PXCacheName(Messages.LoginType)]
	public partial class EPLoginType : IBqlTable
    {

        #region LoginTypeID

        public abstract class loginTypeID : PX.Data.BQL.BqlInt.Field<loginTypeID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? LoginTypeID { get; set; }

        #endregion

        #region LoginTypeName

        public abstract class loginTypeName : PX.Data.BQL.BqlString.Field<loginTypeName> { }

        [PXDBString(50, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "User Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(EPLoginType.loginTypeName))]
        public virtual String LoginTypeName { get; set; }

        #endregion
        #region Entity

        public abstract class entity : PX.Data.BQL.BqlString.Field<entity>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { Contact, Employee },
						new string[] { Messages.ContactType, Messages.EmployeeType }
						) { }
			}

			public const string Contact = "C";
			public const string Employee = "E";

			public class contact : PX.Data.BQL.BqlString.Constant<contact>
			{
				public contact() : base(Contact) { }
			}
			public class employee : PX.Data.BQL.BqlString.Constant<employee>
			{
				public employee() : base(Employee) { }
			}
		}

        [PXDBString(1, IsFixed = true)]
        [PXDefault(entity.Employee)]
		[PXUIField(DisplayName = "Linked Entity", Visibility = PXUIVisibility.SelectorVisible)]
        [entity.List]
        public virtual String Entity { get; set; }
        #endregion
        #region Description

        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        [PXDBString(255, IsUnicode = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Description")]
        public virtual String Description { get; set; }

        #endregion
        #region IsExternal

        public abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }

        [PXBool]
		[PXFormula(typeof(Switch<Case<Where<entity, Equal<entity.contact>>, True>, False>))]
        public virtual bool? IsExternal { get; set; }
        #endregion
        #region EmailAsLogin

        public abstract class emailAsLogin : PX.Data.BQL.BqlBool.Field<emailAsLogin> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Email as Login")]
		[PXUIEnabled(typeof(Where<EPLoginType.entity, Equal<EPLoginType.entity.contact>>))]
		[PXFormula(typeof(Switch<Case<Where<EPLoginType.entity, Equal<EPLoginType.entity.employee>>, False>, EPLoginType.emailAsLogin>))]
        public virtual bool? EmailAsLogin { get; set; }
        #endregion
        #region ResetPasswordOnLogin

        public abstract class resetPasswordOnLogin : PX.Data.BQL.BqlBool.Field<resetPasswordOnLogin> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Reset Password on First Login")]
        public virtual bool? ResetPasswordOnLogin { get; set; }
        #endregion
        #region RequireLoginActivation

        public abstract class requireLoginActivation : PX.Data.BQL.BqlBool.Field<requireLoginActivation> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Login Activation")]
        public virtual bool? RequireLoginActivation { get; set; }
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

