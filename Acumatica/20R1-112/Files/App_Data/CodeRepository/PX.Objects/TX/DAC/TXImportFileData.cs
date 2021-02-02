namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public partial class TXImportFileData : PX.Data.IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "RecordID")]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region StateCode
		public abstract class stateCode : PX.Data.BQL.BqlString.Field<stateCode> { }
		protected String _StateCode;
		[PXDBString(25)]
		[PXUIField(DisplayName = "StateCode", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String StateCode
		{
			get
			{
				return this._StateCode;
			}
			set
			{
				this._StateCode = value;
			}
		}
		#endregion
		#region StateName
		public abstract class stateName : PX.Data.BQL.BqlString.Field<stateName> { }
		protected String _StateName;
		[PXDBString(25)]
		[PXUIField(DisplayName = "StateName", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String StateName
		{
			get
			{
				return this._StateName;
			}
			set
			{
				this._StateName = value;
			}
		}
		#endregion
		#region CityName
		public abstract class cityName : PX.Data.BQL.BqlString.Field<cityName> { }
		protected String _CityName;
		[PXDBString(25)]
		[PXUIField(DisplayName = "CityName")]
		public virtual String CityName
		{
			get
			{
				return this._CityName;
			}
			set
			{
				this._CityName = value;
			}
		}
		#endregion
		#region CountyName
		public abstract class countyName : PX.Data.BQL.BqlString.Field<countyName> { }
		protected String _CountyName;
		[PXDBString(25)]
		[PXUIField(DisplayName = "CountyName")]
		public virtual String CountyName
		{
			get
			{
				return this._CountyName;
			}
			set
			{
				this._CountyName = value;
			}
		}
		#endregion
		#region ZipCode
		public abstract class zipCode : PX.Data.BQL.BqlString.Field<zipCode> { }
		protected String _ZipCode;
		[PXDBString(25)]
		[PXUIField(DisplayName = "ZipCode")]
		public virtual String ZipCode
		{
			get
			{
				return this._ZipCode;
			}
			set
			{
				this._ZipCode = value;
			}
		}
		#endregion
		#region Origin
		public abstract class origin : PX.Data.BQL.BqlString.Field<origin> { }
		protected String _Origin;
		[PXDBString(25)]
		[PXUIField(DisplayName = "Origin")]
		public virtual String Origin
		{
			get
			{
				return this._Origin;
			}
			set
			{
				this._Origin = value;
			}
		}
		#endregion
		#region TaxFreight
		public abstract class taxFreight : PX.Data.BQL.BqlString.Field<taxFreight> { }
		protected String _TaxFreight;
		[PXDBString(25)]
		[PXUIField(DisplayName = "TaxFreight")]
		public virtual String TaxFreight
		{
			get
			{
				return this._TaxFreight;
			}
			set
			{
				this._TaxFreight = value;
			}
		}
		#endregion
		#region TaxServices
		public abstract class taxServices : PX.Data.BQL.BqlString.Field<taxServices> { }
		protected String _TaxServices;
		[PXDBString(25)]
		[PXUIField(DisplayName = "TaxServices")]
		public virtual String TaxServices
		{
			get
			{
				return this._TaxServices;
			}
			set
			{
				this._TaxServices = value;
			}
		}
		#endregion
		#region SignatureCode
		public abstract class signatureCode : PX.Data.BQL.BqlString.Field<signatureCode> { }
		protected String _SignatureCode;
		[PXDBString(25)]
		[PXDefault("")]
		[PXUIField(DisplayName = "SignatureCode")]
		public virtual String SignatureCode
		{
			get
			{
				return this._SignatureCode;
			}
			set
			{
				this._SignatureCode = value;
			}
		}
		#endregion
		#region StateSalesTaxRate
		public abstract class stateSalesTaxRate : PX.Data.BQL.BqlDecimal.Field<stateSalesTaxRate> { }
		protected Decimal? _StateSalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "StateSalesTaxRate")]
		public virtual Decimal? StateSalesTaxRate
		{
			get
			{
				return this._StateSalesTaxRate;
			}
			set
			{
				this._StateSalesTaxRate = value;
			}
		}
		#endregion
		#region StateSalesTaxRateEffectiveDate
		public abstract class stateSalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<stateSalesTaxRateEffectiveDate> { }
		protected DateTime? _StateSalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "StateSalesTaxRateEffectiveDate")]
		public virtual DateTime? StateSalesTaxRateEffectiveDate
		{
			get
			{
				return this._StateSalesTaxRateEffectiveDate;
			}
			set
			{
				this._StateSalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region StateSalesTaxPreviousRate
		public abstract class stateSalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<stateSalesTaxPreviousRate> { }
		protected Decimal? _StateSalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "StateSalesTaxPreviousRate")]
		public virtual Decimal? StateSalesTaxPreviousRate
		{
			get
			{
				return this._StateSalesTaxPreviousRate;
			}
			set
			{
				this._StateSalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region StateUseTaxRate
		public abstract class stateUseTaxRate : PX.Data.BQL.BqlDecimal.Field<stateUseTaxRate> { }
		protected Decimal? _StateUseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "StateUseTaxRate")]
		public virtual Decimal? StateUseTaxRate
		{
			get
			{
				return this._StateUseTaxRate;
			}
			set
			{
				this._StateUseTaxRate = value;
			}
		}
		#endregion
		#region StateUseTaxRateEffectiveDate
		public abstract class stateUseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<stateUseTaxRateEffectiveDate> { }
		protected DateTime? _StateUseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "StateUseTaxRateEffectiveDate")]
		public virtual DateTime? StateUseTaxRateEffectiveDate
		{
			get
			{
				return this._StateUseTaxRateEffectiveDate;
			}
			set
			{
				this._StateUseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region StateUseTaxPreviousRate
		public abstract class stateUseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<stateUseTaxPreviousRate> { }
		protected Decimal? _StateUseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "StateUseTaxPreviousRate")]
		public virtual Decimal? StateUseTaxPreviousRate
		{
			get
			{
				return this._StateUseTaxPreviousRate;
			}
			set
			{
				this._StateUseTaxPreviousRate = value;
			}
		}
		#endregion
		#region StateTaxableMaximum
		public abstract class stateTaxableMaximum : PX.Data.BQL.BqlDecimal.Field<stateTaxableMaximum> { }
		protected Decimal? _StateTaxableMaximum;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "StateTaxableMaximum")]
		public virtual Decimal? StateTaxableMaximum
		{
			get
			{
				return this._StateTaxableMaximum;
			}
			set
			{
				this._StateTaxableMaximum = value;
			}
		}
		#endregion
		#region StateTaxOverMaximumRate
		public abstract class stateTaxOverMaximumRate : PX.Data.BQL.BqlDecimal.Field<stateTaxOverMaximumRate> { }
		protected Decimal? _StateTaxOverMaximumRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "StateTaxOverMaximumRate")]
		public virtual Decimal? StateTaxOverMaximumRate
		{
			get
			{
				return this._StateTaxOverMaximumRate;
			}
			set
			{
				this._StateTaxOverMaximumRate = value;
			}
		}
		#endregion
		#region SignatureCodeCity
		public abstract class signatureCodeCity : PX.Data.BQL.BqlString.Field<signatureCodeCity> { }
		protected String _SignatureCodeCity;
		[PXDBString(25)]
		[PXUIField(DisplayName = "SignatureCodeCity")]
		public virtual String SignatureCodeCity
		{
			get
			{
				return this._SignatureCodeCity;
			}
			set
			{
				this._SignatureCodeCity = value;
			}
		}
		#endregion
		#region CityTaxCodeAssignedByState
		public abstract class cityTaxCodeAssignedByState : PX.Data.BQL.BqlString.Field<cityTaxCodeAssignedByState> { }
		protected String _CityTaxCodeAssignedByState;
		[PXDBString(25)]
		[PXUIField(DisplayName = "CityTaxCodeAssignedByState")]
		public virtual String CityTaxCodeAssignedByState
		{
			get
			{
				return this._CityTaxCodeAssignedByState;
			}
			set
			{
				this._CityTaxCodeAssignedByState = value;
			}
		}
		#endregion
		#region CityLocalRegister
		public abstract class cityLocalRegister : PX.Data.BQL.BqlString.Field<cityLocalRegister> { }
		protected String _CityLocalRegister;
		[PXDBString(25)]
		[PXUIField(DisplayName = "CityLocalRegister")]
		public virtual String CityLocalRegister
		{
			get
			{
				return this._CityLocalRegister;
			}
			set
			{
				this._CityLocalRegister = value;
			}
		}
		#endregion
		#region CitySalesTaxRate
		public abstract class citySalesTaxRate : PX.Data.BQL.BqlDecimal.Field<citySalesTaxRate> { }
		protected Decimal? _CitySalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CitySalesTaxRate")]
		public virtual Decimal? CitySalesTaxRate
		{
			get
			{
				return this._CitySalesTaxRate;
			}
			set
			{
				this._CitySalesTaxRate = value;
			}
		}
		#endregion
		#region CitySalesTaxRateEffectiveDate
		public abstract class citySalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<citySalesTaxRateEffectiveDate> { }
		protected DateTime? _CitySalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CitySalesTaxRateEffectiveDate")]
		public virtual DateTime? CitySalesTaxRateEffectiveDate
		{
			get
			{
				return this._CitySalesTaxRateEffectiveDate;
			}
			set
			{
				this._CitySalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region CitySalesTaxPreviousRate
		public abstract class citySalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<citySalesTaxPreviousRate> { }
		protected Decimal? _CitySalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CitySalesTaxPreviousRate")]
		public virtual Decimal? CitySalesTaxPreviousRate
		{
			get
			{
				return this._CitySalesTaxPreviousRate;
			}
			set
			{
				this._CitySalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region CityUseTaxRate
		public abstract class cityUseTaxRate : PX.Data.BQL.BqlDecimal.Field<cityUseTaxRate> { }
		protected Decimal? _CityUseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CityUseTaxRate")]
		public virtual Decimal? CityUseTaxRate
		{
			get
			{
				return this._CityUseTaxRate;
			}
			set
			{
				this._CityUseTaxRate = value;
			}
		}
		#endregion
		#region CityUseTaxRateEffectiveDate
		public abstract class cityUseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<cityUseTaxRateEffectiveDate> { }
		protected DateTime? _CityUseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CityUseTaxRateEffectiveDate")]
		public virtual DateTime? CityUseTaxRateEffectiveDate
		{
			get
			{
				return this._CityUseTaxRateEffectiveDate;
			}
			set
			{
				this._CityUseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region CityUseTaxPreviousRate
		public abstract class cityUseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<cityUseTaxPreviousRate> { }
		protected Decimal? _CityUseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CityUseTaxPreviousRate")]
		public virtual Decimal? CityUseTaxPreviousRate
		{
			get
			{
				return this._CityUseTaxPreviousRate;
			}
			set
			{
				this._CityUseTaxPreviousRate = value;
			}
		}
		#endregion
		#region CityTaxableMaximum
		public abstract class cityTaxableMaximum : PX.Data.BQL.BqlDecimal.Field<cityTaxableMaximum> { }
		protected Decimal? _CityTaxableMaximum;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CityTaxableMaximum")]
		public virtual Decimal? CityTaxableMaximum
		{
			get
			{
				return this._CityTaxableMaximum;
			}
			set
			{
				this._CityTaxableMaximum = value;
			}
		}
		#endregion
		#region CityTaxOverMaximumRate
		public abstract class cityTaxOverMaximumRate : PX.Data.BQL.BqlDecimal.Field<cityTaxOverMaximumRate> { }
		protected Decimal? _CityTaxOverMaximumRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CityTaxOverMaximumRate")]
		public virtual Decimal? CityTaxOverMaximumRate
		{
			get
			{
				return this._CityTaxOverMaximumRate;
			}
			set
			{
				this._CityTaxOverMaximumRate = value;
			}
		}
		#endregion
		#region SignatureCodeCounty
		public abstract class signatureCodeCounty : PX.Data.BQL.BqlString.Field<signatureCodeCounty> { }
		protected String _SignatureCodeCounty;
		[PXDBString(25)]
		[PXUIField(DisplayName = "SignatureCodeCounty")]
		public virtual String SignatureCodeCounty
		{
			get
			{
				return this._SignatureCodeCounty;
			}
			set
			{
				this._SignatureCodeCounty = value;
			}
		}
		#endregion
		#region CountyTaxCodeAssignedByState
		public abstract class countyTaxCodeAssignedByState : PX.Data.BQL.BqlString.Field<countyTaxCodeAssignedByState> { }
		protected String _CountyTaxCodeAssignedByState;
		[PXDBString(25)]
		[PXUIField(DisplayName = "CountyTaxCodeAssignedByState")]
		public virtual String CountyTaxCodeAssignedByState
		{
			get
			{
				return this._CountyTaxCodeAssignedByState;
			}
			set
			{
				this._CountyTaxCodeAssignedByState = value;
			}
		}
		#endregion
		#region CountyLocalRegister
		public abstract class countyLocalRegister : PX.Data.BQL.BqlString.Field<countyLocalRegister> { }
		protected String _CountyLocalRegister;
		[PXDBString(25)]
		[PXUIField(DisplayName = "CountyLocalRegister")]
		public virtual String CountyLocalRegister
		{
			get
			{
				return this._CountyLocalRegister;
			}
			set
			{
				this._CountyLocalRegister = value;
			}
		}
		#endregion
		#region CountySalesTaxRate
		public abstract class countySalesTaxRate : PX.Data.BQL.BqlDecimal.Field<countySalesTaxRate> { }
		protected Decimal? _CountySalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CountySalesTaxRate")]
		public virtual Decimal? CountySalesTaxRate
		{
			get
			{
				return this._CountySalesTaxRate;
			}
			set
			{
				this._CountySalesTaxRate = value;
			}
		}
		#endregion
		#region CountySalesTaxRateEffectiveDate
		public abstract class countySalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<countySalesTaxRateEffectiveDate> { }
		protected DateTime? _CountySalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CountySalesTaxRateEffectiveDate")]
		public virtual DateTime? CountySalesTaxRateEffectiveDate
		{
			get
			{
				return this._CountySalesTaxRateEffectiveDate;
			}
			set
			{
				this._CountySalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region CountySalesTaxPreviousRate
		public abstract class countySalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<countySalesTaxPreviousRate> { }
		protected Decimal? _CountySalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CountySalesTaxPreviousRate")]
		public virtual Decimal? CountySalesTaxPreviousRate
		{
			get
			{
				return this._CountySalesTaxPreviousRate;
			}
			set
			{
				this._CountySalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region CountyUseTaxRate
		public abstract class countyUseTaxRate : PX.Data.BQL.BqlDecimal.Field<countyUseTaxRate> { }
		protected Decimal? _CountyUseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CountyUseTaxRate")]
		public virtual Decimal? CountyUseTaxRate
		{
			get
			{
				return this._CountyUseTaxRate;
			}
			set
			{
				this._CountyUseTaxRate = value;
			}
		}
		#endregion
		#region CountyUseTaxRateEffectiveDate
		public abstract class countyUseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<countyUseTaxRateEffectiveDate> { }
		protected DateTime? _CountyUseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CountyUseTaxRateEffectiveDate")]
		public virtual DateTime? CountyUseTaxRateEffectiveDate
		{
			get
			{
				return this._CountyUseTaxRateEffectiveDate;
			}
			set
			{
				this._CountyUseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region CountyUseTaxPreviousRate
		public abstract class countyUseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<countyUseTaxPreviousRate> { }
		protected Decimal? _CountyUseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CountyUseTaxPreviousRate")]
		public virtual Decimal? CountyUseTaxPreviousRate
		{
			get
			{
				return this._CountyUseTaxPreviousRate;
			}
			set
			{
				this._CountyUseTaxPreviousRate = value;
			}
		}
		#endregion
		#region CountyTaxableMaximum
		public abstract class countyTaxableMaximum : PX.Data.BQL.BqlDecimal.Field<countyTaxableMaximum> { }
		protected Decimal? _CountyTaxableMaximum;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CountyTaxableMaximum")]
		public virtual Decimal? CountyTaxableMaximum
		{
			get
			{
				return this._CountyTaxableMaximum;
			}
			set
			{
				this._CountyTaxableMaximum = value;
			}
		}
		#endregion
		#region CountyTaxOverMaximumRate
		public abstract class countyTaxOverMaximumRate : PX.Data.BQL.BqlDecimal.Field<countyTaxOverMaximumRate> { }
		protected Decimal? _CountyTaxOverMaximumRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CountyTaxOverMaximumRate")]
		public virtual Decimal? CountyTaxOverMaximumRate
		{
			get
			{
				return this._CountyTaxOverMaximumRate;
			}
			set
			{
				this._CountyTaxOverMaximumRate = value;
			}
		}
		#endregion
		#region SignatureCodeTransit
		public abstract class signatureCodeTransit : PX.Data.BQL.BqlString.Field<signatureCodeTransit> { }
		protected String _SignatureCodeTransit;
		[PXDBString(25)]
		[PXUIField(DisplayName = "SignatureCodeTransit")]
		public virtual String SignatureCodeTransit
		{
			get
			{
				return this._SignatureCodeTransit;
			}
			set
			{
				this._SignatureCodeTransit = value;
			}
		}
		#endregion
		#region TransitTaxCodeAssignedByState
		public abstract class transitTaxCodeAssignedByState : PX.Data.BQL.BqlString.Field<transitTaxCodeAssignedByState> { }
		protected String _TransitTaxCodeAssignedByState;
		[PXDBString(25)]
		[PXUIField(DisplayName = "TransitTaxCodeAssignedByState")]
		public virtual String TransitTaxCodeAssignedByState
		{
			get
			{
				return this._TransitTaxCodeAssignedByState;
			}
			set
			{
				this._TransitTaxCodeAssignedByState = value;
			}
		}
		#endregion
		#region TransitSalesTaxRate
		public abstract class transitSalesTaxRate : PX.Data.BQL.BqlDecimal.Field<transitSalesTaxRate> { }
		protected Decimal? _TransitSalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "TransitSalesTaxRate")]
		public virtual Decimal? TransitSalesTaxRate
		{
			get
			{
				return this._TransitSalesTaxRate;
			}
			set
			{
				this._TransitSalesTaxRate = value;
			}
		}
		#endregion
		#region TransitSalesTaxRateEffectiveDate
		public abstract class transitSalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<transitSalesTaxRateEffectiveDate> { }
		protected DateTime? _TransitSalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "TransitSalesTaxRateEffectiveDate")]
		public virtual DateTime? TransitSalesTaxRateEffectiveDate
		{
			get
			{
				return this._TransitSalesTaxRateEffectiveDate;
			}
			set
			{
				this._TransitSalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region TransitSalesTaxPreviousRate
		public abstract class transitSalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<transitSalesTaxPreviousRate> { }
		protected Decimal? _TransitSalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "TransitSalesTaxPreviousRate")]
		public virtual Decimal? TransitSalesTaxPreviousRate
		{
			get
			{
				return this._TransitSalesTaxPreviousRate;
			}
			set
			{
				this._TransitSalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region TransitUseTaxRate
		public abstract class transitUseTaxRate : PX.Data.BQL.BqlDecimal.Field<transitUseTaxRate> { }
		protected Decimal? _TransitUseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "TransitUseTaxRate")]
		public virtual Decimal? TransitUseTaxRate
		{
			get
			{
				return this._TransitUseTaxRate;
			}
			set
			{
				this._TransitUseTaxRate = value;
			}
		}
		#endregion
		#region TransitUseTaxRateEffectiveDate
		public abstract class transitUseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<transitUseTaxRateEffectiveDate> { }
		protected DateTime? _TransitUseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "TransitUseTaxRateEffectiveDate")]
		public virtual DateTime? TransitUseTaxRateEffectiveDate
		{
			get
			{
				return this._TransitUseTaxRateEffectiveDate;
			}
			set
			{
				this._TransitUseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region TransitUseTaxPreviousRate
		public abstract class transitUseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<transitUseTaxPreviousRate> { }
		protected Decimal? _TransitUseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "TransitUseTaxPreviousRate")]
		public virtual Decimal? TransitUseTaxPreviousRate
		{
			get
			{
				return this._TransitUseTaxPreviousRate;
			}
			set
			{
				this._TransitUseTaxPreviousRate = value;
			}
		}
		#endregion
		#region TransitTaxIsCity
		public abstract class transitTaxIsCity : PX.Data.BQL.BqlString.Field<transitTaxIsCity> { }
		protected String _TransitTaxIsCity;
		[PXDBString(25)]
		[PXUIField(DisplayName = "TransitTaxIsCity")]
		public virtual String TransitTaxIsCity
		{
			get
			{
				return this._TransitTaxIsCity;
			}
			set
			{
				this._TransitTaxIsCity = value;
			}
		}
		#endregion
		#region SignatureCodeOther1
		public abstract class signatureCodeOther1 : PX.Data.BQL.BqlString.Field<signatureCodeOther1> { }
		protected String _SignatureCodeOther1;
		[PXDBString(25)]
		[PXUIField(DisplayName = "SignatureCodeOther1")]
		public virtual String SignatureCodeOther1
		{
			get
			{
				return this._SignatureCodeOther1;
			}
			set
			{
				this._SignatureCodeOther1 = value;
			}
		}
		#endregion
		#region OtherTaxCode1AssignedByState
		public abstract class otherTaxCode1AssignedByState : PX.Data.BQL.BqlString.Field<otherTaxCode1AssignedByState> { }
		protected String _OtherTaxCode1AssignedByState;
		[PXDBString(25)]
		[PXUIField(DisplayName = "OtherTaxCode1AssignedByState")]
		public virtual String OtherTaxCode1AssignedByState
		{
			get
			{
				return this._OtherTaxCode1AssignedByState;
			}
			set
			{
				this._OtherTaxCode1AssignedByState = value;
			}
		}
		#endregion
		#region Other1SalesTaxRate
		public abstract class other1SalesTaxRate : PX.Data.BQL.BqlDecimal.Field<other1SalesTaxRate> { }
		protected Decimal? _Other1SalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other1SalesTaxRate")]
		public virtual Decimal? Other1SalesTaxRate
		{
			get
			{
				return this._Other1SalesTaxRate;
			}
			set
			{
				this._Other1SalesTaxRate = value;
			}
		}
		#endregion
		#region Other1SalesTaxRateEffectiveDate
		public abstract class other1SalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other1SalesTaxRateEffectiveDate> { }
		protected DateTime? _Other1SalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other1SalesTaxRateEffectiveDate")]
		public virtual DateTime? Other1SalesTaxRateEffectiveDate
		{
			get
			{
				return this._Other1SalesTaxRateEffectiveDate;
			}
			set
			{
				this._Other1SalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other1SalesTaxPreviousRate
		public abstract class other1SalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other1SalesTaxPreviousRate> { }
		protected Decimal? _Other1SalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other1SalesTaxPreviousRate")]
		public virtual Decimal? Other1SalesTaxPreviousRate
		{
			get
			{
				return this._Other1SalesTaxPreviousRate;
			}
			set
			{
				this._Other1SalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other1UseTaxRate
		public abstract class other1UseTaxRate : PX.Data.BQL.BqlDecimal.Field<other1UseTaxRate> { }
		protected Decimal? _Other1UseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other1UseTaxRate")]
		public virtual Decimal? Other1UseTaxRate
		{
			get
			{
				return this._Other1UseTaxRate;
			}
			set
			{
				this._Other1UseTaxRate = value;
			}
		}
		#endregion
		#region Other1UseTaxRateEffectiveDate
		public abstract class other1UseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other1UseTaxRateEffectiveDate> { }
		protected DateTime? _Other1UseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other1UseTaxRateEffectiveDate")]
		public virtual DateTime? Other1UseTaxRateEffectiveDate
		{
			get
			{
				return this._Other1UseTaxRateEffectiveDate;
			}
			set
			{
				this._Other1UseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other1UseTaxPreviousRate
		public abstract class other1UseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other1UseTaxPreviousRate> { }
		protected Decimal? _Other1UseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other1UseTaxPreviousRate")]
		public virtual Decimal? Other1UseTaxPreviousRate
		{
			get
			{
				return this._Other1UseTaxPreviousRate;
			}
			set
			{
				this._Other1UseTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other1TaxIsCity
		public abstract class other1TaxIsCity : PX.Data.BQL.BqlString.Field<other1TaxIsCity> { }
		protected String _Other1TaxIsCity;
		[PXDBString(25)]
		[PXUIField(DisplayName = "Other1TaxIsCity")]
		public virtual String Other1TaxIsCity
		{
			get
			{
				return this._Other1TaxIsCity;
			}
			set
			{
				this._Other1TaxIsCity = value;
			}
		}
		#endregion
		#region SignatureCodeOther2
		public abstract class signatureCodeOther2 : PX.Data.BQL.BqlString.Field<signatureCodeOther2> { }
		protected String _SignatureCodeOther2;
		[PXDBString(25)]
		[PXUIField(DisplayName = "SignatureCodeOther2")]
		public virtual String SignatureCodeOther2
		{
			get
			{
				return this._SignatureCodeOther2;
			}
			set
			{
				this._SignatureCodeOther2 = value;
			}
		}
		#endregion
		#region OtherTaxCode2AssignedByState
		public abstract class otherTaxCode2AssignedByState : PX.Data.BQL.BqlString.Field<otherTaxCode2AssignedByState> { }
		protected String _OtherTaxCode2AssignedByState;
		[PXDBString(25)]
		[PXUIField(DisplayName = "OtherTaxCode2AssignedByState")]
		public virtual String OtherTaxCode2AssignedByState
		{
			get
			{
				return this._OtherTaxCode2AssignedByState;
			}
			set
			{
				this._OtherTaxCode2AssignedByState = value;
			}
		}
		#endregion
		#region Other2SalesTaxRate
		public abstract class other2SalesTaxRate : PX.Data.BQL.BqlDecimal.Field<other2SalesTaxRate> { }
		protected Decimal? _Other2SalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other2SalesTaxRate")]
		public virtual Decimal? Other2SalesTaxRate
		{
			get
			{
				return this._Other2SalesTaxRate;
			}
			set
			{
				this._Other2SalesTaxRate = value;
			}
		}
		#endregion
		#region Other2SalesTaxRateEffectiveDate
		public abstract class other2SalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other2SalesTaxRateEffectiveDate> { }
		protected DateTime? _Other2SalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other2SalesTaxRateEffectiveDate")]
		public virtual DateTime? Other2SalesTaxRateEffectiveDate
		{
			get
			{
				return this._Other2SalesTaxRateEffectiveDate;
			}
			set
			{
				this._Other2SalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other2SalesTaxPreviousRate
		public abstract class other2SalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other2SalesTaxPreviousRate> { }
		protected Decimal? _Other2SalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other2SalesTaxPreviousRate")]
		public virtual Decimal? Other2SalesTaxPreviousRate
		{
			get
			{
				return this._Other2SalesTaxPreviousRate;
			}
			set
			{
				this._Other2SalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other2UseTaxRate
		public abstract class other2UseTaxRate : PX.Data.BQL.BqlDecimal.Field<other2UseTaxRate> { }
		protected Decimal? _Other2UseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other2UseTaxRate")]
		public virtual Decimal? Other2UseTaxRate
		{
			get
			{
				return this._Other2UseTaxRate;
			}
			set
			{
				this._Other2UseTaxRate = value;
			}
		}
		#endregion
		#region Other2UseTaxRateEffectiveDate
		public abstract class other2UseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other2UseTaxRateEffectiveDate> { }
		protected DateTime? _Other2UseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other2UseTaxRateEffectiveDate")]
		public virtual DateTime? Other2UseTaxRateEffectiveDate
		{
			get
			{
				return this._Other2UseTaxRateEffectiveDate;
			}
			set
			{
				this._Other2UseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other2UseTaxPreviousRate
		public abstract class other2UseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other2UseTaxPreviousRate> { }
		protected Decimal? _Other2UseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other2UseTaxPreviousRate")]
		public virtual Decimal? Other2UseTaxPreviousRate
		{
			get
			{
				return this._Other2UseTaxPreviousRate;
			}
			set
			{
				this._Other2UseTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other2TaxIsCity
		public abstract class other2TaxIsCity : PX.Data.BQL.BqlString.Field<other2TaxIsCity> { }
		protected String _Other2TaxIsCity;
		[PXDBString(25)]
		[PXUIField(DisplayName = "Other2TaxIsCity")]
		public virtual String Other2TaxIsCity
		{
			get
			{
				return this._Other2TaxIsCity;
			}
			set
			{
				this._Other2TaxIsCity = value;
			}
		}
		#endregion
		#region SignatureCodeOther3
		public abstract class signatureCodeOther3 : PX.Data.BQL.BqlString.Field<signatureCodeOther3> { }
		protected String _SignatureCodeOther3;
		[PXDBString(25)]
		[PXUIField(DisplayName = "SignatureCodeOther3")]
		public virtual String SignatureCodeOther3
		{
			get
			{
				return this._SignatureCodeOther3;
			}
			set
			{
				this._SignatureCodeOther3 = value;
			}
		}
		#endregion
		#region OtherTaxCode3AssignedByState
		public abstract class otherTaxCode3AssignedByState : PX.Data.BQL.BqlString.Field<otherTaxCode3AssignedByState> { }
		protected String _OtherTaxCode3AssignedByState;
		[PXDBString(25)]
		[PXUIField(DisplayName = "OtherTaxCode3AssignedByState")]
		public virtual String OtherTaxCode3AssignedByState
		{
			get
			{
				return this._OtherTaxCode3AssignedByState;
			}
			set
			{
				this._OtherTaxCode3AssignedByState = value;
			}
		}
		#endregion
		#region Other3SalesTaxRate
		public abstract class other3SalesTaxRate : PX.Data.BQL.BqlDecimal.Field<other3SalesTaxRate> { }
		protected Decimal? _Other3SalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other3SalesTaxRate")]
		public virtual Decimal? Other3SalesTaxRate
		{
			get
			{
				return this._Other3SalesTaxRate;
			}
			set
			{
				this._Other3SalesTaxRate = value;
			}
		}
		#endregion
		#region Other3SalesTaxRateEffectiveDate
		public abstract class other3SalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other3SalesTaxRateEffectiveDate> { }
		protected DateTime? _Other3SalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other3SalesTaxRateEffectiveDate")]
		public virtual DateTime? Other3SalesTaxRateEffectiveDate
		{
			get
			{
				return this._Other3SalesTaxRateEffectiveDate;
			}
			set
			{
				this._Other3SalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other3SalesTaxPreviousRate
		public abstract class other3SalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other3SalesTaxPreviousRate> { }
		protected Decimal? _Other3SalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other3SalesTaxPreviousRate")]
		public virtual Decimal? Other3SalesTaxPreviousRate
		{
			get
			{
				return this._Other3SalesTaxPreviousRate;
			}
			set
			{
				this._Other3SalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other3UseTaxRate
		public abstract class other3UseTaxRate : PX.Data.BQL.BqlDecimal.Field<other3UseTaxRate> { }
		protected Decimal? _Other3UseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other3UseTaxRate")]
		public virtual Decimal? Other3UseTaxRate
		{
			get
			{
				return this._Other3UseTaxRate;
			}
			set
			{
				this._Other3UseTaxRate = value;
			}
		}
		#endregion
		#region Other3UseTaxRateEffectiveDate
		public abstract class other3UseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other3UseTaxRateEffectiveDate> { }
		protected DateTime? _Other3UseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other3UseTaxRateEffectiveDate")]
		public virtual DateTime? Other3UseTaxRateEffectiveDate
		{
			get
			{
				return this._Other3UseTaxRateEffectiveDate;
			}
			set
			{
				this._Other3UseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other3UseTaxPreviousRate
		public abstract class other3UseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other3UseTaxPreviousRate> { }
		protected Decimal? _Other3UseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other3UseTaxPreviousRate")]
		public virtual Decimal? Other3UseTaxPreviousRate
		{
			get
			{
				return this._Other3UseTaxPreviousRate;
			}
			set
			{
				this._Other3UseTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other3TaxIsCity
		public abstract class other3TaxIsCity : PX.Data.BQL.BqlString.Field<other3TaxIsCity> { }
		protected String _Other3TaxIsCity;
		[PXDBString(25)]
		[PXUIField(DisplayName = "Other3TaxIsCity")]
		public virtual String Other3TaxIsCity
		{
			get
			{
				return this._Other3TaxIsCity;
			}
			set
			{
				this._Other3TaxIsCity = value;
			}
		}
		#endregion
		#region SignatureCodeOther4
		public abstract class signatureCodeOther4 : PX.Data.BQL.BqlString.Field<signatureCodeOther4> { }
		protected String _SignatureCodeOther4;
		[PXDBString(25)]
		[PXUIField(DisplayName = "SignatureCodeOther4")]
		public virtual String SignatureCodeOther4
		{
			get
			{
				return this._SignatureCodeOther4;
			}
			set
			{
				this._SignatureCodeOther4 = value;
			}
		}
		#endregion
		#region OtherTaxCode4AssignedByState
		public abstract class otherTaxCode4AssignedByState : PX.Data.BQL.BqlString.Field<otherTaxCode4AssignedByState> { }
		protected String _OtherTaxCode4AssignedByState;
		[PXDBString(25)]
		[PXUIField(DisplayName = "OtherTaxCode4AssignedByState")]
		public virtual String OtherTaxCode4AssignedByState
		{
			get
			{
				return this._OtherTaxCode4AssignedByState;
			}
			set
			{
				this._OtherTaxCode4AssignedByState = value;
			}
		}
		#endregion
		#region Other4SalesTaxRate
		public abstract class other4SalesTaxRate : PX.Data.BQL.BqlDecimal.Field<other4SalesTaxRate> { }
		protected Decimal? _Other4SalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other4SalesTaxRate")]
		public virtual Decimal? Other4SalesTaxRate
		{
			get
			{
				return this._Other4SalesTaxRate;
			}
			set
			{
				this._Other4SalesTaxRate = value;
			}
		}
		#endregion
		#region Other4SalesTaxRateEffectiveDate
		public abstract class other4SalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other4SalesTaxRateEffectiveDate> { }
		protected DateTime? _Other4SalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other4SalesTaxRateEffectiveDate")]
		public virtual DateTime? Other4SalesTaxRateEffectiveDate
		{
			get
			{
				return this._Other4SalesTaxRateEffectiveDate;
			}
			set
			{
				this._Other4SalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other4SalesTaxPreviousRate
		public abstract class other4SalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other4SalesTaxPreviousRate> { }
		protected Decimal? _Other4SalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other4SalesTaxPreviousRate")]
		public virtual Decimal? Other4SalesTaxPreviousRate
		{
			get
			{
				return this._Other4SalesTaxPreviousRate;
			}
			set
			{
				this._Other4SalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other4UseTaxRate
		public abstract class other4UseTaxRate : PX.Data.BQL.BqlDecimal.Field<other4UseTaxRate> { }
		protected Decimal? _Other4UseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other4UseTaxRate")]
		public virtual Decimal? Other4UseTaxRate
		{
			get
			{
				return this._Other4UseTaxRate;
			}
			set
			{
				this._Other4UseTaxRate = value;
			}
		}
		#endregion
		#region Other4UseTaxRateEffectiveDate
		public abstract class other4UseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<other4UseTaxRateEffectiveDate> { }
		protected DateTime? _Other4UseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Other4UseTaxRateEffectiveDate")]
		public virtual DateTime? Other4UseTaxRateEffectiveDate
		{
			get
			{
				return this._Other4UseTaxRateEffectiveDate;
			}
			set
			{
				this._Other4UseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region Other4UseTaxPreviousRate
		public abstract class other4UseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<other4UseTaxPreviousRate> { }
		protected Decimal? _Other4UseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Other4UseTaxPreviousRate")]
		public virtual Decimal? Other4UseTaxPreviousRate
		{
			get
			{
				return this._Other4UseTaxPreviousRate;
			}
			set
			{
				this._Other4UseTaxPreviousRate = value;
			}
		}
		#endregion
		#region Other4TaxIsCity
		public abstract class other4TaxIsCity : PX.Data.BQL.BqlString.Field<other4TaxIsCity> { }
		protected String _Other4TaxIsCity;
		[PXDBString(25)]
		[PXUIField(DisplayName = "Other4TaxIsCity")]
		public virtual String Other4TaxIsCity
		{
			get
			{
				return this._Other4TaxIsCity;
			}
			set
			{
				this._Other4TaxIsCity = value;
			}
		}
		#endregion
		#region CombinedSalesTaxRate
		public abstract class combinedSalesTaxRate : PX.Data.BQL.BqlDecimal.Field<combinedSalesTaxRate> { }
		protected Decimal? _CombinedSalesTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CombinedSalesTaxRate")]
		public virtual Decimal? CombinedSalesTaxRate
		{
			get
			{
				return this._CombinedSalesTaxRate;
			}
			set
			{
				this._CombinedSalesTaxRate = value;
			}
		}
		#endregion
		#region CombinedSalesTaxRateEffectiveDate
		public abstract class combinedSalesTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<combinedSalesTaxRateEffectiveDate> { }
		protected DateTime? _CombinedSalesTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CombinedSalesTaxRateEffectiveDate")]
		public virtual DateTime? CombinedSalesTaxRateEffectiveDate
		{
			get
			{
				return this._CombinedSalesTaxRateEffectiveDate;
			}
			set
			{
				this._CombinedSalesTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region CombinedSalesTaxPreviousRate
		public abstract class combinedSalesTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<combinedSalesTaxPreviousRate> { }
		protected Decimal? _CombinedSalesTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CombinedSalesTaxPreviousRate")]
		public virtual Decimal? CombinedSalesTaxPreviousRate
		{
			get
			{
				return this._CombinedSalesTaxPreviousRate;
			}
			set
			{
				this._CombinedSalesTaxPreviousRate = value;
			}
		}
		#endregion
		#region CombinedUseTaxRate
		public abstract class combinedUseTaxRate : PX.Data.BQL.BqlDecimal.Field<combinedUseTaxRate> { }
		protected Decimal? _CombinedUseTaxRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CombinedUseTaxRate")]
		public virtual Decimal? CombinedUseTaxRate
		{
			get
			{
				return this._CombinedUseTaxRate;
			}
			set
			{
				this._CombinedUseTaxRate = value;
			}
		}
		#endregion
		#region CombinedUseTaxRateEffectiveDate
		public abstract class combinedUseTaxRateEffectiveDate : PX.Data.BQL.BqlDateTime.Field<combinedUseTaxRateEffectiveDate> { }
		protected DateTime? _CombinedUseTaxRateEffectiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CombinedUseTaxRateEffectiveDate")]
		public virtual DateTime? CombinedUseTaxRateEffectiveDate
		{
			get
			{
				return this._CombinedUseTaxRateEffectiveDate;
			}
			set
			{
				this._CombinedUseTaxRateEffectiveDate = value;
			}
		}
		#endregion
		#region CombinedUseTaxPreviousRate
		public abstract class combinedUseTaxPreviousRate : PX.Data.BQL.BqlDecimal.Field<combinedUseTaxPreviousRate> { }
		protected Decimal? _CombinedUseTaxPreviousRate;
		[PXDBDecimal(5)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "CombinedUseTaxPreviousRate")]
		public virtual Decimal? CombinedUseTaxPreviousRate
		{
			get
			{
				return this._CombinedUseTaxPreviousRate;
			}
			set
			{
				this._CombinedUseTaxPreviousRate = value;
			}
		}
		#endregion
		#region DateLastUpdated
		public abstract class dateLastUpdated : PX.Data.BQL.BqlDateTime.Field<dateLastUpdated> { }
		protected DateTime? _DateLastUpdated;
		[PXDBDate()]
		[PXUIField(DisplayName = "DateLastUpdated")]
		public virtual DateTime? DateLastUpdated
		{
			get
			{
				return this._DateLastUpdated;
			}
			set
			{
				this._DateLastUpdated = value;
			}
		}
		#endregion
		#region DeleteCode
		public abstract class deleteCode : PX.Data.BQL.BqlString.Field<deleteCode> { }
		protected String _DeleteCode;
		[PXDBString(25)]
		[PXUIField(DisplayName = "DeleteCode")]
		public virtual String DeleteCode
		{
			get
			{
				return this._DeleteCode;
			}
			set
			{
				this._DeleteCode = value;
			}
		}
		#endregion
	}
}
