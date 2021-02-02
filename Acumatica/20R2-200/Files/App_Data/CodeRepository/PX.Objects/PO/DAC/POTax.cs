namespace PX.Objects.PO
{
	using System;
	using PX.Data;
	using PX.Objects.TX;
	using PX.Objects.CM;
    using PX.Objects.CS;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.POTax)]
	public partial class POTax : TaxDetail, PX.Data.IBqlTable, ITaxDetailWithAmounts
	{
		#region Keys
		public class PK : PrimaryKeyOf<POTax>.By<orderType, orderNbr, lineNbr, taxID>
		{
			public static POTax Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr, string taxID)
				=> FindBy(graph, orderType, orderNbr, lineNbr, taxID);
		}
		public static class FK
		{
			public class Line : POLine.PK.ForeignKeyOf<POTax>.By<orderType, orderNbr, lineNbr> { }
			public class LineR : POLineR.PK.ForeignKeyOf<POTax>.By<orderType, orderNbr, lineNbr> { }
			public class LineUOpen : POLineUOpen.PK.ForeignKeyOf<POTax>.By<orderType, orderNbr, lineNbr> { }

		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POOrder.orderType), DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		[PXDBDefault(typeof(POOrder.orderNbr),DefaultForUpdate=false)]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.Line))]
		[PXParent(typeof(FK.LineR))]
		[PXParent(typeof(FK.LineUOpen))]
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
		public  abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		
		[PXDBLong()]
		[CurrencyInfo(typeof(POOrder.curyInfoID))]
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
		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.taxableAmt))]
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
		#region CuryUnbilledTaxableAmt
		public abstract class curyUnbilledTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxableAmt> { }
		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.unbilledTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnbilledTaxableAmt
		{
			get;
			set;
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
		#region UnbilledTaxableAmt
		public abstract class unbilledTaxableAmt : PX.Data.BQL.BqlDecimal.Field<unbilledTaxableAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxableAmt
		{
			get;
			set;
		}
		#endregion

		#region Per Unit/Specific Tax Feature
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
		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.taxAmt))]
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
		#region CuryUnbilledTaxAmt
		public abstract class curyUnbilledTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxAmt> { }
		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.unbilledTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnbilledTaxAmt
		{
			get;
			set;
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
		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.expenseAmt))]
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
		#region UnbilledTaxAmt
		public abstract class unbilledTaxAmt : PX.Data.BQL.BqlDecimal.Field<unbilledTaxAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxAmt
		{
			get;
			set;
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

		#region CuryRetainedTaxableAmt
		public abstract class curyRetainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxableAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.retainedTaxableAmt))]
		[PXUIField(DisplayName = "Retained Taxable", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryRetainedTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxableAmt
		public abstract class retainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxableAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Taxable", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? RetainedTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedTaxAmt
		public abstract class curyRetainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(POTax.curyInfoID), typeof(POTax.retainedTaxAmt))]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryRetainedTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxAmt
		public abstract class retainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? RetainedTaxAmt
		{
			get;
			set;
		}
		#endregion
	}
}
