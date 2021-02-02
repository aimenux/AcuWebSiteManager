namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using System.Globalization;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxRev)]
	public partial class TaxRev : PX.Data.IBqlTable
	{
		public const string DefaultStartDate = "01/01/1900";
		public const string DefaultEndDate = "06/06/9999";
		public DateTime GetDefaultEndDate()
		{
			return DateTime.Parse(DefaultEndDate, CultureInfo.InvariantCulture, DateTimeStyles.None);
		}

		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		protected String _TaxID;
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Tax.taxID))]
		[PXUIField(DisplayName = "Tax ID",Visible=false)]
		[PXParent(typeof(Select<Tax, Where<Tax.taxID, Equal<Current<TaxRev.taxID>>>>))]

		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region TaxVendorID
		public abstract class taxVendorID : PX.Data.BQL.BqlInt.Field<taxVendorID> { }
		protected Int32? _TaxVendorID;
		[PXDBInt]
		[PXDBDefault(typeof(Tax.taxVendorID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? TaxVendorID
		{
			get
			{
				return this._TaxVendorID;
			}
			set
			{
				this._TaxVendorID = value;
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "RevisionID", Visible = false)]
		public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, DefaultStartDate)]
		[PXUIField(DisplayName = "Start Date")]
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
		[PXDefault(TypeCode.DateTime, DefaultEndDate)]
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
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
		protected Decimal? _TaxRate;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Tax Rate")]
		public virtual Decimal? TaxRate
		{
			get
			{
				return this._TaxRate;
			}
			set
			{
				this._TaxRate = value;
			}
		}
		#endregion
		#region NonDeductibleTaxRate
		public abstract class nonDeductibleTaxRate : PX.Data.BQL.BqlDecimal.Field<nonDeductibleTaxRate> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? NonDeductibleTaxRate { get; set; }
		#endregion
		#region TaxableMin
		public abstract class taxableMin : PX.Data.BQL.BqlDecimal.Field<taxableMin> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Min. Taxable Amount")]
		public virtual Decimal? TaxableMin
		{
			get;
			set;
		}
		#endregion
		#region TaxableMax
		public abstract class taxableMax : PX.Data.BQL.BqlDecimal.Field<taxableMax> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Max. Taxable Amount")]
		public virtual Decimal? TaxableMax
		{
			get;
			set;
		}
		#endregion

		#region Specific and Per Unit Taxes
		#region TaxableMaxQty
		/// <summary>
		/// The maximum taxable quantity for Specific/Per Unit taxes.
		/// </summary>
		public abstract class taxableMaxQty : PX.Data.BQL.BqlDecimal.Field<taxableMaxQty> { }

		/// <summary>
		/// The maximum taxable quantity for Specific/Per Unit taxes.
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Max. Taxable Quantity", FieldClass = nameof(CS.FeaturesSet.PerUnitTaxSupport))]
		public virtual decimal? TaxableMaxQty
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region Outdated
		public abstract class outdated : PX.Data.BQL.BqlBool.Field<outdated> { }
		protected Boolean? _Outdated;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Not Valid", Visibility=PXUIVisibility.Visible, Enabled=false)]
		public virtual Boolean? Outdated
		{
			get
			{
				return this._Outdated;
			}
			set
			{
				this._Outdated = value;
			}
		}
		#endregion
		#region TaxType
		public abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }
		protected String _TaxType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("S")]
		[PXUIField(DisplayName = "Group Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[] { "S", "P" }, new string[] { "Output", "Input" })]
		public virtual String TaxType
		{
			get
			{
				return this._TaxType;
			}
			set
			{
				this._TaxType = value;
			}
		}
		#endregion
		#region TaxBucketID
		public abstract class taxBucketID : PX.Data.BQL.BqlInt.Field<taxBucketID> { }
		protected Int32? _TaxBucketID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Reporting Group", Visibility = PXUIVisibility.Visible)]
		[PXIntList(new int[] { 0 }, new string[] { "undefined" })]
		[PXDefault()]
		public virtual Int32? TaxBucketID
		{
			get
			{
				return this._TaxBucketID;
			}
			set
			{
				this._TaxBucketID = value;
			}
		}
		#endregion
		#region IsImported
		public abstract class isImported : PX.Data.BQL.BqlBool.Field<isImported> { }
		protected Boolean? _IsImported;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsImported
		{
			get
			{
				return this._IsImported;
			}
			set
			{
				this._IsImported = value;
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
