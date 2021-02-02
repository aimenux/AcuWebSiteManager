namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.TX;
	using PX.Objects.CS;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOTax)]
	public partial class SOTax : TaxDetail, PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOTax>.By<orderType, orderNbr, lineNbr, taxID>
		{
			public static SOTax Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr, string taxID)
				=> FindBy(graph, orderType, orderNbr, lineNbr, taxID);
		}
		public static class FK
		{
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOTax>.By<orderType> { }
			public class Order : SOOrder.PK.ForeignKeyOf<SOTax>.By<orderType, orderNbr> { }
			public class Line : SOLine.PK.ForeignKeyOf<SOTax>.By<orderType, orderNbr, lineNbr> { }
			public class Tax : TX.Tax.PK.ForeignKeyOf<SOTax>.By<taxID> { }
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrder.orderType))]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOOrder.orderNbr), DefaultForUpdate = false)]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.Line), LeaveChildren = true)]
		[PXParent(typeof(FK.Order))]
		[PXParent(typeof(Select<SOLine2, Where<SOLine2.orderType, Equal<Current<SOTax.orderType>>, And<SOLine2.orderNbr, Equal<Current<SOTax.orderNbr>>, And<SOLine2.lineNbr, Equal<Current<SOTax.lineNbr>>>>>>), LeaveChildren = true)]
		[PXParent(typeof(Select<SOLine4, Where<SOLine4.orderType, Equal<Current<SOTax.orderType>>, And<SOLine4.orderNbr, Equal<Current<SOTax.orderNbr>>, And<SOLine4.lineNbr, Equal<Current<SOTax.lineNbr>>>>>>), LeaveChildren = true)]
		[PXParent(typeof(Select<SOMiscLine2, Where<SOMiscLine2.orderType, Equal<Current<SOTax.orderType>>, And<SOMiscLine2.orderNbr, Equal<Current<SOTax.orderNbr>>, And<SOMiscLine2.lineNbr, Equal<Current<SOTax.lineNbr>>>>>>), LeaveChildren = true)]
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
		[PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
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
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
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
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.taxableAmt))]
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
		#region CuryExemptedAmt
		public abstract class curyExemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.exemptedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual decimal? CuryExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUnshippedTaxableAmt
		public abstract class curyUnshippedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyUnshippedTaxableAmt> { }
		protected Decimal? _CuryUnshippedTaxableAmt;
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.unshippedTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnshippedTaxableAmt
		{
			get
			{
				return this._CuryUnshippedTaxableAmt;
			}
			set
			{
				this._CuryUnshippedTaxableAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledTaxableAmt
		public abstract class curyUnbilledTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxableAmt> { }
		protected Decimal? _CuryUnbilledTaxableAmt;
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.unbilledTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnbilledTaxableAmt
		{
			get
			{
				return this._CuryUnbilledTaxableAmt;
			}
			set
			{
				this._CuryUnbilledTaxableAmt = value;
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
		#region ExemptedAmt
		public abstract class exemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual decimal? ExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region UnshippedTaxableAmt
		public abstract class unshippedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<unshippedTaxableAmt> { }
		protected Decimal? _UnshippedTaxableAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnshippedTaxableAmt
		{
			get
			{
				return this._UnshippedTaxableAmt;
			}
			set
			{
				this._UnshippedTaxableAmt = value;
			}
		}
		#endregion
		#region UnbilledTaxableAmt
		public abstract class unbilledTaxableAmt : PX.Data.BQL.BqlDecimal.Field<unbilledTaxableAmt> { }
		protected Decimal? _UnbilledTaxableAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxableAmt
		{
			get
			{
				return this._UnbilledTaxableAmt;
			}
			set
			{
				this._UnbilledTaxableAmt = value;
			}
		}
		#endregion

		#region Per Unit/Specific Tax Feature
		#region UnshippedTaxableQty
		/// <summary>
		///The unshipped taxable quantity for per unit taxes.
		/// </summary>
		public abstract class unshippedTaxableQty : PX.Data.BQL.BqlDecimal.Field<unshippedTaxableQty> { }

		/// <summary>
		///The unshipped taxable quantity for per unit taxes.
		/// </summary>
		[IN.PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Taxable Qty.", Enabled = false)]
		public virtual decimal? UnshippedTaxableQty
		{
			get;
			set;
		}
		#endregion

		#region UnbilledTaxableQty
		/// <summary>
		///The unbilled taxable quantity for per unit taxes.
		/// </summary>
		public abstract class unbilledTaxableQty : PX.Data.BQL.BqlDecimal.Field<unbilledTaxableQty> { }

		/// <summary>
		///The unbilled taxable quantity for per unit taxes.
		/// </summary>
		[IN.PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Taxable Qty.", Enabled = false)]
		public virtual decimal? UnbilledTaxableQty
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		protected decimal? _CuryTaxAmt;
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.taxAmt))]
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
		#region CuryUnshippedTaxAmt
		public abstract class curyUnshippedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyUnshippedTaxAmt> { }
		protected Decimal? _CuryUnshippedTaxAmt;
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.unshippedTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnshippedTaxAmt
		{
			get
			{
				return this._CuryUnshippedTaxAmt;
			}
			set
			{
				this._CuryUnshippedTaxAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledTaxAmt
		public abstract class curyUnbilledTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxAmt> { }
		protected Decimal? _CuryUnbilledTaxAmt;
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.unbilledTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnbilledTaxAmt
		{
			get
			{
				return this._CuryUnbilledTaxAmt;
			}
			set
			{
				this._CuryUnbilledTaxAmt = value;
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
		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }
		[PXDBCurrency(typeof(SOTax.curyInfoID), typeof(SOTax.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryExpenseAmt
		{
			get; set;
		}
		#endregion
		#region ExpenseAmt
		public abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }
		#endregion
		#region UnshippedTaxAmt
		public abstract class unshippedTaxAmt : PX.Data.BQL.BqlDecimal.Field<unshippedTaxAmt> { }
		protected Decimal? _UnshippedTaxAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnshippedTaxAmt
		{
			get
			{
				return this._UnshippedTaxAmt;
			}
			set
			{
				this._UnshippedTaxAmt = value;
			}
		}
		#endregion
		#region UnbilledTaxAmt
		public abstract class unbilledTaxAmt : PX.Data.BQL.BqlDecimal.Field<unbilledTaxAmt> { }
		protected Decimal? _UnbilledTaxAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxAmt
		{
			get
			{
				return this._UnbilledTaxAmt;
			}
			set
			{
				this._UnbilledTaxAmt = value;
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
    }
}
