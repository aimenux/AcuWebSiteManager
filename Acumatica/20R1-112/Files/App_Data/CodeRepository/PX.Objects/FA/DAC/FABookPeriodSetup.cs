﻿using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FABookPeriodSetup)]
	public partial class FABookPeriodSetup: IBqlTable, IPeriodSetup
	{
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected int? _BookID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(FABookYearSetup.bookID))]
		[PXParent(typeof(Select<FABookYearSetup, Where<FABookYearSetup.bookID, Equal<Current<FABookPeriodSetup.bookID>>>>))]
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
		#region PeriodNbr
		public abstract class periodNbr : PX.Data.BQL.BqlString.Field<periodNbr> { }
		protected String _PeriodNbr;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
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
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "End Date", Enabled = false)]
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
		[PXUIField(DisplayName = "Description")]
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
                return (_EndDate.HasValue && _StartDate.HasValue && _StartDate == _EndDate) ? _StartDate.Value.AddDays(-1) : _StartDate;
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
