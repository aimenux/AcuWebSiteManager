using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FADisposalMethod)]
	public partial class FADisposalMethod : IBqlTable
	{
		#region DisposalMethodID
		public abstract class disposalMethodID : PX.Data.BQL.BqlInt.Field<disposalMethodID> { }
		protected Int32? _DisposalMethodID;
		[PXDBIdentity]
        [PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false)]
        public virtual Int32? DisposalMethodID
		{
			get
			{
				return _DisposalMethodID;
			}
			set
			{
				_DisposalMethodID = value;
			}
		}
		#endregion
		#region DisposalMethodCD
		public abstract class disposalMethodCD : PX.Data.BQL.BqlString.Field<disposalMethodCD> { }
		protected String _DisposalMethodCD;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
		[PXUIField(DisplayName = "Disposal Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DisposalMethodCD
		{
			get
			{
				return _DisposalMethodCD;
			}
			set
			{
				_DisposalMethodCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return _Description;
			}
			set
			{
				_Description = value;
			}
		}
		#endregion
		#region ProceedsAcctID
		public abstract class proceedsAcctID : PX.Data.BQL.BqlInt.Field<proceedsAcctID> { }
		protected Int32? _ProceedsAcctID;
		[Account(null,
			DisplayName = "Proceeds Account",
			DescriptionField = typeof(Account.description))]
        [PXDefault(typeof(FASetup.proceedsAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ProceedsAcctID
		{
			get
			{
				return this._ProceedsAcctID;
			}
			set
			{
				this._ProceedsAcctID = value;
			}
		}
		#endregion
		#region ProceedsSubID
		public abstract class proceedsSubID : PX.Data.BQL.BqlInt.Field<proceedsSubID> { }
		protected Int32? _ProceedsSubID;
		[SubAccount(typeof(proceedsAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Proceeds Subaccount")]
        [PXDefault(typeof(FASetup.proceedsSubID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? ProceedsSubID
		{
			get
			{
				return this._ProceedsSubID;
			}
			set
			{
				this._ProceedsSubID = value;
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
				return _tstamp;
			}
			set
			{
				_tstamp = value;
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
				return _CreatedByID;
			}
			set
			{
				_CreatedByID = value;
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
				return _CreatedByScreenID;
			}
			set
			{
				_CreatedByScreenID = value;
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
				return _CreatedDateTime;
			}
			set
			{
				_CreatedDateTime = value;
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
				return _LastModifiedByID;
			}
			set
			{
				_LastModifiedByID = value;
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
				return _LastModifiedByScreenID;
			}
			set
			{
				_LastModifiedByScreenID = value;
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
				return _LastModifiedDateTime;
			}
			set
			{
				_LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
