using System;
using System.Collections.Generic;
using System.Diagnostics;
using PX.Data;
using PX.Objects.Common.Abstractions.Periods;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FABookPeriod)]
	[DebuggerDisplay("{GetType()}: BookID = {BookID}, OrganizationID = {OrganizationID}, FinPeriodID = {FinPeriodID}, tstamp = {PX.Data.PXDBTimestampAttribute.ToString(tstamp)}}")]
	public partial class FABookPeriod : IBqlTable, IPeriod
	{
	    public class Key : OrganizationDependedPeriodKey
	    {
	        public int? BookID { get; protected set; }
			public bool IsPostingBook { get; protected set; }
			public void SetBookID(FABook book)
			{
				BookID = book.BookID;
				IsPostingBook = book.UpdateGL == true;
			}

	        public override bool Defined => base.Defined && BookID != null;

            public override List<object> ToListOfObjects(bool skipPeriodID = false)
	        {
	            List<object> values = base.ToListOfObjects(skipPeriodID);

                values.Add(BookID);

	            return values;
	        }

	        public override bool IsNotPeriodPartsEqual(object otherKey)
	        {
	            return base.IsNotPeriodPartsEqual(otherKey)
	                   && ((Key) otherKey).BookID == BookID;
	        }

			public override bool IsMasterCalendar => base.IsMasterCalendar && IsPostingBook;
		}

        #region BookID
        public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> {}
		protected int? _BookID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(FABookYear.bookID))]
		[PXParent(typeof(Select<
			FABookYear, 
			Where<FABookYear.bookID, Equal<Current<FABookPeriod.bookID>>, 
				And<FABookYear.organizationID, Equal<Current<FABookPeriod.organizationID>>>>>))]
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
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID>
		{
			public class nonPostingBookValue : PX.Data.BQL.BqlInt.Constant<nonPostingBookValue>
			{
				public nonPostingBookValue() : base(NonPostingBookValue) { }
			}

			public const int NonPostingBookValue = 0;
		}

		[PXDefault(FinPeriod.organizationID.MasterValue)]
		[PXDBInt(IsKey = true)]
		public virtual int? OrganizationID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear>
		{
		}
		protected String _FinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault(typeof(FABookYear.year))]
		[PXUIField(DisplayName = "FinYear", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXParent(typeof(Select<
			FABookYear,
			Where<FABookYear.year, Equal<Current<FABookPeriod.finYear>>,
				And<FABookYear.bookID, Equal<Current<FABookPeriod.bookID>>,
				And<FABookYear.organizationID, Equal<Current<FABookPeriod.organizationID>>>>>>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FABookPeriodID(
			organizationSourceType: typeof(FABookPeriod.organizationID),
			bookSourceType: typeof(FABookPeriod.bookID),
			IsKey = true)]
		[PXDefault()]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false, DisplayName = "Financial Period ID")]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region MasterFinPeriodID
		public abstract class masterFinPeriodID : PX.Data.BQL.BqlString.Field<masterFinPeriodID> { }

		[FABookPeriodID]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false, DisplayName = "Master Calendar Period ID")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string MasterFinPeriodID { get; set; }
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
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
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
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Closed
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R1)]
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
		protected Boolean? _Closed;
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R1)]
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in GL", Enabled = false)]
		public virtual Boolean? Closed
		{
			get
			{
				return this._Closed;
			}
			set
			{
				this._Closed = value;
			}
		}
		#endregion
		#region DateLocked
		public abstract class dateLocked : PX.Data.BQL.BqlBool.Field<dateLocked> { }
		protected Boolean? _DateLocked;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Date Locked", Enabled = false, Visible = false)]
		public virtual Boolean? DateLocked
		{
			get
			{
				return this._DateLocked;
			}
			set
			{
				this._DateLocked = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region PeriodNbr
		public abstract class periodNbr : PX.Data.BQL.BqlString.Field<periodNbr> { }
		protected String _PeriodNbr;
		[PXDBString(2, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Period Nbr.", Enabled = false)]
		public virtual String PeriodNbr
		{
			get
			{
				return this._PeriodNbr;
			}
			set
			{
				this._PeriodNbr = value;
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
        #region StartDateUI
        public abstract class startDateUI : PX.Data.BQL.BqlDateTime.Field<startDateUI> { }

        /// <summary>
        /// The field used to display and edit the <see cref="StartDate"/> of the period (inclusive) in the UI.
        /// </summary>
        /// <value>
        /// Depends on and changes the value of the <see cref="StartDate"/> field, performing additional transformations.
        /// </value>
        [PXDate]
        [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? StartDateUI
        {
            [PXDependsOnFields(typeof(startDate), typeof(endDate))]
            get
            {
                return (_StartDate != null && _EndDate != null && _StartDate == _EndDate) ? _StartDate.Value.AddDays(-1) : _StartDate;
            }
            set
            {
                _StartDate = (value != null && _EndDate != null && value == EndDateUI) ? value.Value.AddDays(1) : value;
            }
        }
        #endregion
        #region EndDateUI
        public abstract class endDateUI : PX.Data.BQL.BqlDateTime.Field<endDateUI> { }

        /// <summary>
        /// The field used to display and edit the <see cref="EndDate"/> of the period (inclusive) in the UI.
        /// </summary>
        /// <value>
        /// Depends on and changes the value of the <see cref="EndDate"/> field, performing additional transformations.
        /// </value>
        [PXDate]
        [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? EndDateUI
        {
            [PXDependsOnFields(typeof(endDate))]
            get
            {
                return _EndDate?.AddDays(-1);
            }
            set
            {
                _EndDate = value?.AddDays(1);
            }
        }
        #endregion
        #region Custom
        public abstract class custom : PX.Data.BQL.BqlBool.Field<custom> { }
		protected Boolean? _Custom;
		public virtual Boolean? Custom
		{
			get
			{
				return this._Custom;
			}
			set
			{
				this._Custom = value;
			}
		}
		#endregion
	}
}
