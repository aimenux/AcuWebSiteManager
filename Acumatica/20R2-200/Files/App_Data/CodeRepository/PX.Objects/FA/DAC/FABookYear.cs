using System;
using System.Diagnostics;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FABookYear)]
	[DebuggerDisplay("{GetType()}: BookID = {BookID}, OrganizationID = {OrganizationID}, Year = {Year}, tstamp = {PX.Data.PXDBTimestampAttribute.ToString(tstamp)}}")]
	public partial class FABookYear : IBqlTable, IYear
	{
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected int? _BookID;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXSelector(typeof(Search<FABook.bookID>),
					SubstituteKey = typeof(FABook.bookCode),
					DescriptionField = typeof(FABook.description))]
		[PXUIField(DisplayName = "Book")]
		[PXParent(typeof(Select<FABook, Where<FABook.bookID, Equal<Current<FABookYear.bookID>>>>))]
		public virtual int? BookID
		{
			get
			{
				return _BookID;
			}
			set
			{
				_BookID = value;
			}
		}
		#endregion
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault(FinPeriod.organizationID.MasterValue)]
		public virtual int? OrganizationID { get; set; }
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		protected String _Year;
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Financial Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<
			FABookYear.year, 
			Where<FABookYear.organizationID, Equal<Current<FABookYear.organizationID>>>, 
			OrderBy<
				Desc<FABookYear.year>>>))]
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
		#region StartMasterFinPeriodID
		public abstract class startMasterFinPeriodID : PX.Data.BQL.BqlString.Field<startMasterFinPeriodID> { }

		[PXDBString(6, IsFixed = true)]
		[FinPeriodIDFormatting]
		[PXUIField(Visibility = PXUIVisibility.Visible, Enabled = false, DisplayName = "Start Master Period ID")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string StartMasterFinPeriodID { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
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
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods> { }
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
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
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
