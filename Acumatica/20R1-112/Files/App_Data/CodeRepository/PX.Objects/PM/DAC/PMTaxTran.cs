using System;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.PM
{
	[PXCacheName(Messages.PMTaxTran)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMTaxTran : TaxDetail, IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
		[PXDBDefault(typeof(PMProforma.refNbr))]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false, Visible = false)]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault(int.MaxValue)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMTaxTran.refNbr>>>>))]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), DirtyRead = true)]
		public override String TaxID
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
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID>
		{
		}
		protected Int32? _RecordID;
		[PXDBIdentity]
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
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(PMProforma.curyInfoID))]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		protected decimal? _CuryTaxableAmt;
		[PXDBCurrency(typeof(PMTaxTran.curyInfoID), typeof(PMTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		protected Decimal? _TaxableAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxableAmt
		{
			get
			{
				return this._TaxableAmt;
			}
			set
			{
				this._TaxableAmt = value;
			}
		}
		#endregion
		#region CuryExemptedAmt
		public abstract class curyExemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(PMTaxTran.curyInfoID), typeof(PMTaxTran.exemptedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public decimal? CuryExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region ExemptedAmt
		public abstract class exemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public decimal? ExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		protected decimal? _CuryTaxAmt;
		[PXDBCurrency(typeof(PMTaxTran.curyInfoID), typeof(PMTaxTran.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}
			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		protected Decimal? _TaxAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxAmt
		{
			get
			{
				return this._TaxAmt;
			}
			set
			{
				this._TaxAmt = value;
			}
		}
		#endregion
		#region NonDeductibleTaxRate
		public abstract class nonDeductibleTaxRate : PX.Data.BQL.BqlDecimal.Field<nonDeductibleTaxRate> { }

		/// <summary>
		/// The deductible tax rate from the related <see cref="TaxRev"/> record.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? NonDeductibleTaxRate { get; set; }
		#endregion
		#region ExpenseAmt
		public abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }

		/// <summary>
		/// The expense amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? ExpenseAmt { get; set; }
		#endregion
		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }

		/// <summary>
		/// The expense amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(PMTaxTran.curyInfoID), typeof(PMTaxTran.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryExpenseAmt { get; set; }
		#endregion
		#region CuryRetainedTaxableAmt
		public abstract class curyRetainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxableAmt> { }
		protected decimal? _CuryRetainedTaxableAmt;
				
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(PMTaxTran.curyInfoID), typeof(PMTaxTran.retainedTaxableAmt))]
		[PXUIField(DisplayName = "Retained Taxable", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainedTaxableAmt
		{
			get
			{
				return _CuryRetainedTaxableAmt;
			}
			set
			{
				_CuryRetainedTaxableAmt = value;
			}
		}
		#endregion
		#region RetainedTaxableAmt
		public abstract class retainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxableAmt> { }
		protected decimal? _RetainedTaxableAmt;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Taxable", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? RetainedTaxableAmt
		{
			get
			{
				return _RetainedTaxableAmt;
			}
			set
			{
				_RetainedTaxableAmt = value;
			}
		}
		#endregion
		#region CuryRetainedTaxAmt
		public abstract class curyRetainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmt> { }
		protected decimal? _CuryRetainedTaxAmt;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(PMTaxTran.curyInfoID), typeof(PMTaxTran.retainedTaxAmt))]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainedTaxAmt
		{
			get
			{
				return _CuryRetainedTaxAmt;
			}
			set
			{
				_CuryRetainedTaxAmt = value;
			}
		}
		#endregion
		#region RetainedTaxAmt
		public abstract class retainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmt> { }
		protected decimal? _RetainedTaxAmt;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainedTaxAmt
		{
			get
			{
				return _RetainedTaxAmt;
			}
			set
			{
				_RetainedTaxAmt = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual string TaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region JurisType
		public abstract class jurisType : PX.Data.BQL.BqlString.Field<jurisType> { }
		protected String _JurisType;
		[PXDBString(9, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Jurisdiction Type")]
		public virtual String JurisType
		{
			get
			{
				return this._JurisType;
			}
			set
			{
				this._JurisType = value;
			}
		}
		#endregion
		#region JurisName
		public abstract class jurisName : PX.Data.BQL.BqlString.Field<jurisName> { }
		protected String _JurisName;
		[PXDBString(200, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Jurisdiction Name")]
		public virtual String JurisName
		{
			get
			{
				return this._JurisName;
			}
			set
			{
				this._JurisName = value;
			}
		}
		#endregion
	}
}
