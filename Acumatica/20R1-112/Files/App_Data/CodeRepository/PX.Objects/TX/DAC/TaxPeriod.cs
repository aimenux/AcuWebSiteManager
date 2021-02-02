using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.TX
{
	using System;
	using PX.Data;

	public class TaxPeriodStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Prepared, Open, Closed },
				new string[] { Messages.Prepared, Messages.Open, Messages.Closed }) { ; }
		}

		public const string Prepared = "P";
		public const string Open = "N";
		public const string Closed = "C";
		public const string Dummy = "D";

		public class prepared : PX.Data.BQL.BqlString.Constant<prepared>
		{
			public prepared() : base(Prepared) { ;}
		}

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { ;}
		}

		public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) { ;}
		}
	}

	/// <summary>
	/// Represent an agency tax period at the company level.
	/// The instance of DAC is created on the Prepare Tax Report(TX501000) page when the user selects a tax agency.
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxPeriod)]
	public partial class TaxPeriod : PX.Data.IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		/// <summary>
		/// The reference to the <see cref="Organization"/> record to which the record belongs.
		/// </summary>
		[Organization(IsKey = true)]
		public virtual Int32? OrganizationID { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

		/// <summary>
		/// <see cref="PX.Objects.AP.Vendor.BAccountID"/> of a tax agency to which the tax period belongs. 
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		protected String _TaxPeriodID;

		/// <summary>
		/// Identifier of the tax period. 
		/// </summary>
		[GL.FinPeriodID(IsKey=true)]
		[PXDefault()]
		[PXUIField(DisplayName = Messages.TaxPeriod, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
		#region TaxYear
		public abstract class taxYear : PX.Data.BQL.BqlString.Field<taxYear> { }
		protected String _TaxYear;

		/// <summary>
		/// Identifier of a <see cref="PX.Objects.TX.TaxYear"/> to which the tax period belongs.
		/// </summary>
		[PXDBString(4, IsFixed = true)]
		[PXDefault()]
		public virtual String TaxYear
		{
			get
			{
				return this._TaxYear;
			}
			set
			{
				this._TaxYear = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;

		/// <summary>
		/// The start date of the period.
		/// </summary>
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible)]
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

		/// <summary>
		/// The end date of the period (exclusive).
		/// </summary>
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "To", Visibility = PXUIVisibility.Visible)]
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
		/// The field that is used to display <see cref="EndDate"/> of the period (inclusive) in the UI.
		/// </summary>
		/// <value>
		/// Depends on and changes the value of the <see cref="EndDate"/> field by performing additional transformations.
		/// </value>
		[PXDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual DateTime? EndDateUI
		{
			[PXDependsOnFields(typeof(endDate))]
			get
			{
				return this._EndDate?.AddDays(-1);
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public const string DefaultStatus = TaxPeriodStatus.Open;
		}
		protected String _Status;

		/// <summary>
		/// The status of the tax period.
		/// </summary>
		/// <value>
		/// The field can have the following values:
		/// <c>"N"</c> - Open.
		/// <c>"P"</c> - Prepared.
		/// <c>"C"</c> - Closed.
		/// <c>"D"</c> - Dummy. This status is used to show periods without <see cref="TaxTran"/> on the Prepare Tax Report (TX501000) page.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(status.DefaultStatus)]
		[TaxPeriodStatus.List()]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion		
		#region Filed
		public abstract class filed : PX.Data.BQL.BqlBool.Field<filed> { }
		protected Boolean? _Filed;

		/// <summary>
		/// The field is not used.
		/// </summary>
		[Obsolete("This property is obsolete and will be removed in Acumatica 8.0")]
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Filed
		{
			get
			{
				return this._Filed;
			}
			set
			{
				this._Filed = value;
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
	}
}