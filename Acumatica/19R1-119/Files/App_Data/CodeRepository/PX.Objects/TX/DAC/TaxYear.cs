using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.TX.Descriptor;

namespace PX.Objects.TX
{
	using System;
	using PX.Data;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxYear)]
	public partial class TaxYear : PX.Data.IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		/// <summary>
		/// The reference to the <see cref="Organization"/> record to which the record belongs.
		/// </summary>
		[Organization(IsKey = true)]
		[PXDefault]
		public virtual Int32? OrganizationID { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		/// <summary>
		/// The reference to the tax agency (<see cref="Vendor"/>) record to which the record belongs.
		/// </summary>
		[TaxAgencyActive(IsKey = true)]
		[PXDefault]
		public virtual Int32? VendorID { get; set; }

		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }

		[PXUIField(DisplayName = "Tax Year")]
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[PXSelector(typeof(Search<TaxYear.year,
							Where<TaxYear.organizationID, Equal<Optional<TaxYear.organizationID>>,
									And<TaxYear.vendorID, Equal<Optional<TaxYear.vendorID>>>>>),
					new[]{typeof(TaxYear.year)})]
		public virtual String Year { get; set; }

		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? StartDate { get; set; }

		#endregion
		#region Filed
		public abstract class filed : PX.Data.BQL.BqlBool.Field<filed> { }
		protected Boolean? _Filed;
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
		#region PeriodsCount 
		public abstract class periodsCount : PX.Data.BQL.BqlInt.Field<periodsCount> { }

		/// <summary>
		/// The actual count of periods of the year.
		/// </summary>
		[PXDBInt]
		public virtual int? PeriodsCount { get; set; }
		#endregion
		#region PlanPeriodsCount 
		public abstract class planPeriodsCount : PX.Data.BQL.BqlInt.Field<planPeriodsCount> { }

		/// <summary>
		/// The calculated (by <see cref="TaxCalendar<>"/>) count of periods of the year.
		/// </summary>
		[PXDBInt]
		public virtual int? PlanPeriodsCount { get; set; }
		#endregion
		#region TaxPeriodType
		public abstract class taxPeriodType : PX.Data.BQL.BqlString.Field<taxPeriodType> { }
		protected String _TaxPeriodType;

		/// <summary>
		/// The calendar type of the tax year.
		/// </summary>
		[PXDBString(1)]
		[PXDefault(VendorTaxPeriodType.Monthly)]
		[PXUIField(DisplayName = "Tax Period Type")]
		[VendorTaxPeriodType.List()]
		public virtual String TaxPeriodType
		{
			get
			{
				return this._TaxPeriodType;
			}
			set
			{
				this._TaxPeriodType = value;
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