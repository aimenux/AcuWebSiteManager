using System;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	/// <summary>
	/// Represents a tax detail of an Expense Claim document. 
	/// The entities of this type are edited on the Expense Claim
	/// (EP301000) form, which correspond to 
	/// the <see cref="ExpenseClaimEntry"/> graph.
	/// </summary>
	[Serializable]
	public partial class EPTaxAggregate : TaxDetail, IBqlTable
	{
        
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(EPExpenseClaim.refNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Ref. Nbr.", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<EPExpenseClaim,
			Where<EPExpenseClaim.refNbr, Equal<Current<EPTaxAggregate.refNbr>>>>))]
		public string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
		public override string TaxID
		{
			get
			{
				return _TaxID;
			}
			set
			{
				_TaxID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXLong]
		public override long? CuryInfoID
		{
			get
			{
				return _CuryInfoID;
			}
			set
			{
				_CuryInfoID = value;
			}
		}
		#endregion
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }

		/// <summary>
		/// The tax rate of the relevant <see cref="Tax"/> record.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override decimal? TaxRate
		{
			get
			{
				return _TaxRate;
			}
			set
			{
				_TaxRate = value;
			}
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]

		public decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		[PXDBBaseCury]
		public decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public decimal? CuryTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		[PXDBBaseCury]
		public decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion
		#region NonDeductibleTaxRate
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "100.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override decimal? NonDeductibleTaxRate
		{
			get;
			set;
		}
		#endregion

		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override decimal? CuryExpenseAmt
		{
			get;
			set;
		}
		#endregion
		#region ExpenseAmt
		public abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }
		[PXBaseCury]
		public override decimal? ExpenseAmt
		{
			get;
			set;
		}
		#endregion
	}
}
