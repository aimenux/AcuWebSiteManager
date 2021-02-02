namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TermsInstallments)]
	public partial class TermsInstallments : PX.Data.IBqlTable
	{
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Terms.termsID))]
		[PXUIField(DisplayName = "TermsID", Visible = false)]
		[PXParent(typeof(Select<Terms, Where<Terms.termsID, Equal<Current<TermsInstallments.termsID>>>>))]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
		#endregion
		#region InstallmentNbr
		public abstract class installmentNbr : PX.Data.BQL.BqlShort.Field<installmentNbr> { }
		protected Int16? _InstallmentNbr;
		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Inst.#", Visible = false, Enabled=false )]
		public virtual Int16? InstallmentNbr
		{
			get
			{
				return this._InstallmentNbr;
			}
			set
			{
				this._InstallmentNbr = value;
			}
		}
		#endregion
		#region InstDays
		public abstract class instDays : PX.Data.BQL.BqlShort.Field<instDays> { }
		protected Int16? _InstDays;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Days")]
		public virtual Int16? InstDays
		{
			get
			{
				return this._InstDays;
			}
			set
			{
				this._InstDays = value;
			}
		}
		#endregion
		#region InstPercent
		public abstract class instPercent : PX.Data.BQL.BqlDecimal.Field<instPercent> { }
		protected Decimal? _InstPercent;
		[PXDBDecimal(2, MinValue = 0.00, MaxValue = 100.00)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Percent")]
		public virtual Decimal? InstPercent
		{
			get
			{
				return this._InstPercent;
			}
			set
			{
				this._InstPercent = value;
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
	}
}
