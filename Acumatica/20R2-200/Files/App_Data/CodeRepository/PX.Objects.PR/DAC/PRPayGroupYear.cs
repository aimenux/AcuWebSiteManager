using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRPayGroupYear)]
	public partial class PRPayGroupYear : IBqlTable, IYear
	{
		#region PayGroupID
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		protected string _PayGroupID;
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(Search<PRPayGroup.payGroupID, Where<PRPayGroup.isDefault, Equal<boolTrue>>>))]
		[PXSelector(typeof(Search<PRPayGroup.payGroupID>), DescriptionField = typeof(PRPayGroup.description))]
		[PXUIField(DisplayName = "Pay Group")]
		[PXParent(typeof(Select<PRPayGroup, Where<PRPayGroup.payGroupID, Equal<Current<PRPayGroupYear.payGroupID>>>>))]
		public virtual string PayGroupID
		{
			get
			{
				return _PayGroupID;
			}
			set
			{
				_PayGroupID = value;
			}
		}
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year>
		{
		}
		protected String _Year;
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PRPayGroupYear.year, Where<PRPayGroupYear.payGroupID, Equal<Optional<PRPayGroupYear.payGroupID>>>, OrderBy<Desc<PRPayGroupYear.year>>>))]
		public virtual String Year
		{
			get
			{
				return this._Year;
			}
			set
			{
				this._Year = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate>
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
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
		#region FinPeriods
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods>
		{
		}
		protected Int16? _FinPeriods;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Number of Periods", Enabled = false)]
		public virtual Int16? FinPeriods
		{
			get
			{
				return this._FinPeriods;
			}
			set
			{
				this._FinPeriods = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate>
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "EndDate", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
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
		#region UseTransactionDateExceptions
		public abstract class useTransactionDateExceptions : PX.Data.BQL.BqlBool.Field<useTransactionDateExceptions> { }
		[PXDBBool]
		[PXUIField(Visible = false)]
		[PXDefault(false)]
		public virtual bool? UseTransactionDateExceptions { get; set; }
		#endregion
		#region TransactionDateExceptionBehavior
		public abstract class transactionDateExceptionBehavior : PX.Data.BQL.BqlBool.Field<transactionDateExceptionBehavior> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(Visible = false)]
		[TransactionDateExceptionBehavior.List]
		public virtual string TransactionDateExceptionBehavior { get; set; }
		#endregion
		#region PeriodsFullyCreated
		public abstract class periodsFullyCreated : PX.Data.BQL.BqlBool.Field<periodsFullyCreated> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual bool? PeriodsFullyCreated { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
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
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
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
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
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
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
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
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
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
