namespace PX.Objects.PO
{
    using System;
    using PX.Data;
    using PX.Objects.TX;
    using PX.Objects.CM;
    using PX.Objects.CS;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.POTaxTran)]
    public partial class POTaxTran : TaxDetail, PX.Data.IBqlTable
    {
		#region Keys
		public class PK : PrimaryKeyOf<POTaxTran>.By<recordID, orderType, orderNbr, lineNbr, taxID>
		{
			public static POTaxTran Find(PXGraph graph, int? recordID, string orderType, string orderNbr, int? lineNbr, string taxID)
				=> Find(graph, recordID, orderType, orderNbr, lineNbr, taxID);
		}
		public static class FK
		{
			public class Order : POOrder.PK.ForeignKeyOf<POTaxTran>.By<orderType, orderNbr> { }
			public class Line : POLine.PK.ForeignKeyOf<POTaxTran>.By<orderType, orderNbr, lineNbr> { }
			public class Tax : POTax.PK.ForeignKeyOf<POTaxTran>.By<orderType, orderNbr, lineNbr, taxID> { }
		}
		#endregion
		
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        protected String _OrderType;
        [PXDBString(2, IsFixed = true, IsKey = true)]
        [PXDBDefault(typeof(POOrder.orderType), DefaultForUpdate = false)]
        [PXUIField(DisplayName = "Order Type", Enabled = false, Visible = false)]
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
        [PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
        [PXDBDefault(typeof(POOrder.orderNbr), DefaultForUpdate = false)]
        [PXUIField(DisplayName = "Order Nbr.", Enabled = false, Visible = false)]
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
        [PXDefault(LineNbrValue)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.Order))]
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
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), IsDirty = true)]
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
		[PXDBIdentity(IsKey = true)]
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
        #region TaxRate
        public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
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
        [PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.taxableAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<Where<WhereExempt<POTaxTran.taxID>>, POTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<POOrder.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<Where<WhereTaxable<POTaxTran.taxID>>, POTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<POOrder.curyVatTaxableTotal>))]
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
		[PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.unbilledTaxableAmt))]
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
        [PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.taxAmt))]
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
		[PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.unbilledTaxAmt))]
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
		[PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.expenseAmt))]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.retainedTaxableAmt))]
		[PXUIField(DisplayName = "Retained Taxable", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainedTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxableAmt
		public abstract class retainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxableAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Taxable", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainedTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedTaxAmt
		public abstract class curyRetainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(POTaxTran.curyInfoID), typeof(POTaxTran.retainedTaxAmt))]
		[PXUIField(DisplayName = "Retained Tax", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainedTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxAmt
		public abstract class retainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Tax", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainedTaxAmt
		{
			get;
			set;
		}
		#endregion

        public class lineNbrValue : PX.Data.BQL.BqlInt.Constant<lineNbrValue>
		{
            public lineNbrValue() : base(LineNbrValue) { ;}
        }

        public const Int32 LineNbrValue = int.MaxValue;
    }
}
