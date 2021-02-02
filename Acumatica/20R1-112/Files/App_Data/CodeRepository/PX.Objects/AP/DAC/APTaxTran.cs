using System;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	/// <summary>
	/// Represents a tax detail of an Accounts Payable document. This is a 
	/// projection DAC over <see cref="TaxTran"/> restricted by <see 
	/// cref="BatchModule.moduleAP">the Accounts Payable module</see>. 
	/// The entities of this type are edited on the Bills and Adjustments
	/// (AP301000) and Quick Checks (AP304000) forms, which correspond to 
	/// the <see cref="APInvoiceEntry"/> and <see cref="APQuickCheckEntry"/>
	/// graphs, respectively.
	/// </summary>
	/// <remarks>
	/// Tax details are aggregates combined by <see cref="TaxBaseAttribute"/> 
	/// descendants from the line-level <see cref="APTax"/> records.
	/// </remarks>
	[PXProjection(typeof(Select<TaxTran,Where<TaxTran.module,Equal<BatchModule.moduleAP>>>), Persistent=true)]
	[PXCacheName(Messages.APTaxTran)]
    [Serializable]
	public partial class APTaxTran : TaxTran
	{
        #region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
        [PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault((string)BatchModule.AP)]
		[PXUIField(DisplayName = "Module", Enabled = false, Visible = false)]
		public override String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region TranType
		public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        [PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(APRegister.docType))]
		[PXParent(typeof(Select<APRegister,Where<APRegister.docType,Equal<Current<TaxTran.tranType>>,And<APRegister.refNbr,Equal<Current<TaxTran.refNbr>>>>>))]
		[PXUIField(DisplayName = "Tran. Type", Enabled = false, Visible = false)]
		public override String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(APRegister.refNbr))]
		[PXUIField(DisplayName = "Ref. Nbr.", Enabled = false, Visible = false)]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region OrigTranType
		public new abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType> { }
		#endregion
		#region OrigRefNbr
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(typeof(APRegister.branchID), Enabled = false)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion
		#region TaxPeriodID
		public new abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		[GL.FinPeriodID()]
		public override String TaxPeriodID
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
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
	    [FinPeriodID(branchSourceType: typeof(APTaxTran.branchID), 
	        headerMasterFinPeriodIDType:typeof(APRegister.tranPeriodID))]
        [PXDefault]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        [PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
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
		public new abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID>
		{
		}

		/// <summary>
		/// This is an auto-numbered field, which is a part of the primary key.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public override Int32? RecordID { get; set; }
		#endregion
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[PXDBInt()]
		[PXDefault(typeof(Search<Tax.taxVendorID, Where<Tax.taxID,Equal<Current<APTaxTran.taxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? VendorID
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
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt]
		[PXDefault(typeof(Parent<APRegister.vendorID>))]
		public override Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault()]
		public override String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(ControlAccountForModule = ControlAccountModule.TX)]
		[PXDefault(typeof(Search<PurchaseTax.histTaxAcctID, Where<PurchaseTax.taxID, Equal<Current<APTaxTran.taxID>>>>))]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount()]
		[PXDefault(typeof(Search<PurchaseTax.histTaxSubID, Where<PurchaseTax.taxID, Equal<Current<APTaxTran.taxID>>>>))]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate()]
		[PXDBDefault(typeof(APRegister.docDate))]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region TaxType
		public new abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<PurchaseTax.tranTaxType, Where<PurchaseTax.taxID, Equal<Current<APTaxTran.taxID>>>>))]
		public override String TaxType
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
		public new abstract class taxBucketID : PX.Data.BQL.BqlInt.Field<taxBucketID> { }
		[PXDBInt()]
		[PXDefault(typeof(Search<TaxRev.taxBucketID, 
			Where<TaxRev.taxID, Equal<Current<APTaxTran.taxID>>, 
				And<Current<APTaxTran.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>, 
				And2<
					Where<TaxRev.taxType, Equal<Current<APTaxTran.taxType>>, 
						Or<TaxRev.taxType, Equal<TaxType.sales>, 
						And<Current<APTaxTran.taxType>,  Equal<TaxType.pendingSales>, 
						Or<TaxRev.taxType, Equal<TaxType.purchase>, 
						And<Current<APTaxTran.taxType>, Equal<TaxType.pendingPurchase>>>>>>, 
					And<TaxRev.outdated, Equal<False>>>>>>))]
		public override Int32? TaxBucketID
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
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(APRegister.curyInfoID))]
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
		public new abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<WhereExempt<APTaxTran.taxID>, APTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<APInvoice.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<WhereTaxable<APTaxTran.taxID>, APTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<APInvoice.curyVatTaxableTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<WhereExempt<APTaxTran.taxID>, APTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<AP.Standalone.APQuickCheck.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<WhereTaxable<APTaxTran.taxID>, APTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<AP.Standalone.APQuickCheck.curyVatTaxableTotal>))]
		public override Decimal? CuryTaxableAmt
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
		public new abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		#endregion
		#region CuryTaxAmt
		public new abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryTaxAmt
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
		public new abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		#endregion
		#region CuryTaxAmtSumm
		public new abstract class curyTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyTaxAmtSumm> { }
		[PXDBCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.taxAmtSumm))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? CuryTaxAmtSumm { get; set; }
		#endregion
		#region TaxAmtSumm
		public new abstract class taxAmtSumm : PX.Data.BQL.BqlDecimal.Field<taxAmtSumm> { }
		#endregion
		#region NonDeductibleTaxRate
		public new abstract class nonDeductibleTaxRate : PX.Data.BQL.BqlDecimal.Field<nonDeductibleTaxRate> { }
		#endregion
		#region ExpenseAmt
		public new abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }
		#endregion
		#region CuryExpenseAmt
		public new abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }
		[PXDBCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryExpenseAmt
		{
			get; set;
		}
		#endregion

		#region CuryTaxableDiscountAmt
		public abstract class curyTaxableDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableDiscountAmt> { }
		[PXCurrency(typeof(curyInfoID), typeof(taxableDiscountAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryTaxableDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableDiscountAmt
		public abstract class taxableDiscountAmt : PX.Data.BQL.BqlDecimal.Field<taxableDiscountAmt> { }
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? TaxableDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxDiscountAmt
		public abstract class curyTaxDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxDiscountAmt> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(taxDiscountAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryTaxDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableDiscountAmt
		public abstract class taxDiscountAmt : PX.Data.BQL.BqlDecimal.Field<taxDiscountAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? TaxDiscountAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainedTaxableAmt
		public new abstract class curyRetainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxableAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.retainedTaxableAmt))]
		[PXUIField(DisplayName = "Retained Taxable Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public override decimal? CuryRetainedTaxableAmt
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
		public new abstract class retainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxableAmt> { }
		#endregion
		#region CuryRetainedTaxAmt
		public new abstract class curyRetainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.retainedTaxAmt))]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public override decimal? CuryRetainedTaxAmt
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
		public new abstract class retainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmt> { }
		#endregion
		#region CuryRetainedTaxAmtSumm
		public new abstract class curyRetainedTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmtSumm> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.retainedTaxAmtSumm))]
		public override decimal? CuryRetainedTaxAmtSumm { get; set; }
		#endregion
		#region RetainedTaxAmtSumm
		public new abstract class retainedTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmtSumm> { }
		#endregion

		#region VAT Recalculation section
		#region CuryDiscountedTaxableAmt
		public abstract class curyDiscountedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountedTaxableAmt> { }


		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.discountedTaxableAmt))]
		[PXUIField(DisplayName = Messages.DiscountedTaxableAmount, Visible = false, Enabled = false)]
		public virtual decimal? CuryDiscountedTaxableAmt
		{
			get; set;
		}
		#endregion
		#region DiscountedTaxableAmt
		public abstract class discountedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<discountedTaxableAmt> { }
		protected decimal? _DiscountedTaxableAmt;

	
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? DiscountedTaxableAmt
		{
			get
			{
				return _DiscountedTaxableAmt;
			}
			set
			{
				_DiscountedTaxableAmt = value;
			}
		}
		#endregion
		#region CuryDiscountedPrice
		public abstract class curyDiscountedPrice : PX.Data.BQL.BqlDecimal.Field<curyDiscountedPrice> { }
		protected decimal? _CuryDiscountedPrice;


		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(APTaxTran.curyInfoID), typeof(APTaxTran.discountedPrice))]
		[PXUIField(DisplayName = Messages.TaxOnDiscountedPrice, Visible = false, Enabled = false)]
		public virtual decimal? CuryDiscountedPrice
		{
			get
			{
				return _CuryDiscountedPrice;
			}
			set
			{
				_CuryDiscountedPrice = value;
			}
		}
		#endregion
		#region DiscountedPrice
		public abstract class discountedPrice : PX.Data.BQL.BqlDecimal.Field<discountedPrice> { }
		protected decimal? _DiscountedPrice;

	
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? DiscountedPrice
		{
			get
			{
				return _DiscountedPrice;
			}
			set
			{
				_DiscountedPrice = value;
			}
		}
		#endregion
		#endregion
	}

	[Serializable]
    [PXHidden]
	public partial class PurchaseTax : Tax
	{
		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		#endregion
		#region TaxType
		public new abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }
		#endregion
		#region TaxVendorID
		public new abstract class taxVendorID : PX.Data.BQL.BqlInt.Field<taxVendorID> { }
		#endregion
		#region SalesTaxAcctID
		public new abstract class salesTaxAcctID : PX.Data.BQL.BqlInt.Field<salesTaxAcctID> { }
		#endregion
		#region SalesTaxSubID
		public new abstract class salesTaxSubID : PX.Data.BQL.BqlInt.Field<salesTaxSubID> { }
		#endregion
		#region PurchTaxAcctID
		public new abstract class purchTaxAcctID : PX.Data.BQL.BqlInt.Field<purchTaxAcctID> { }
		#endregion
		#region PurchTaxSubID
		public new abstract class purchTaxSubID : PX.Data.BQL.BqlInt.Field<purchTaxSubID> { }
		#endregion
		#region PendingTax
		public new abstract class pendingTax : PX.Data.BQL.BqlBool.Field<pendingTax> { }
		#endregion
		#region PendingSalesTaxAcctID
		public new abstract class pendingSalesTaxAcctID : PX.Data.BQL.BqlInt.Field<pendingSalesTaxAcctID> { }
		#endregion
		#region PendingSalesTaxSubID
		public new abstract class pendingSalesTaxSubID : PX.Data.BQL.BqlInt.Field<pendingSalesTaxSubID> { }
		#endregion
		#region PendingPurchTaxAcctID
		public new abstract class pendingPurchTaxAcctID : PX.Data.BQL.BqlInt.Field<pendingPurchTaxAcctID> { }
		#endregion
		#region PendingPurchTaxSubID
		public new abstract class pendingPurchTaxSubID : PX.Data.BQL.BqlInt.Field<pendingPurchTaxSubID> { }
		#endregion
		#region HistTaxAcctID
		public abstract class histTaxAcctID : PX.Data.BQL.BqlInt.Field<histTaxAcctID> { }
		protected Int32? _HistTaxAcctID;
		[PXDBCalcedAttribute(typeof(Switch<
			Case<Where<PurchaseTax.pendingTax, Equal<True>, And<PurchaseTax.reverseTax, Equal<False>>>, PurchaseTax.pendingPurchTaxAcctID, 
			Case<Where<PurchaseTax.pendingTax, Equal<True>, And<PurchaseTax.reverseTax, Equal<True>>>, PurchaseTax.pendingSalesTaxAcctID, 
			Case<Where<
				PurchaseTax.taxType, Equal<CSTaxType.use>, 
				Or<PurchaseTax.taxType, Equal<CSTaxType.withholding>,
				Or<PurchaseTax.taxType, Equal<CSTaxType.sales>,
				Or<PurchaseTax.reverseTax, Equal<True>>>>>, PurchaseTax.salesTaxAcctID>>>, 
			PurchaseTax.purchTaxAcctID>), typeof(Int32))]
		public virtual Int32? HistTaxAcctID
		{
			get
			{
				return this._HistTaxAcctID;
			}
			set
			{
				this._HistTaxAcctID = value;
			}
		}
		#endregion
		#region HistTaxSubID
		public abstract class histTaxSubID : PX.Data.BQL.BqlInt.Field<histTaxSubID> { }
		protected Int32? _HistTaxSubID;
        [PXDBCalcedAttribute(typeof(Switch<
			Case<Where<PurchaseTax.pendingTax, Equal<True>, And<PurchaseTax.reverseTax, Equal<False>>>, PurchaseTax.pendingPurchTaxSubID,
			Case<Where<PurchaseTax.pendingTax, Equal<True>, And<PurchaseTax.reverseTax, Equal<True>>>, PurchaseTax.pendingSalesTaxSubID,
			Case<Where<
				PurchaseTax.taxType, Equal<CSTaxType.use>, 
				Or<PurchaseTax.taxType, Equal<CSTaxType.withholding>,
				Or<PurchaseTax.taxType, Equal<CSTaxType.perUnit>,
				Or<PurchaseTax.taxType, Equal<CSTaxType.sales>,
				Or<PurchaseTax.reverseTax, Equal<True>>>>>>, PurchaseTax.salesTaxSubID>>>, 
			PurchaseTax.purchTaxSubID>), typeof(Int32))]
		public virtual Int32? HistTaxSubID
		{
			get
			{
				return this._HistTaxSubID;
			}
			set
			{
				this._HistTaxSubID = value;
			}
		}
		#endregion
		#region TranTaxType
		public abstract class tranTaxType : PX.Data.BQL.BqlString.Field<tranTaxType> { }
		protected string _TranTaxType;
		[PXDBCalced(typeof(
			Switch<
				Case<Where<PurchaseTax.pendingTax, Equal<True>, And<PurchaseTax.reverseTax, Equal<False>>>, 
					TaxType.pendingPurchase,
				Case<Where<PurchaseTax.pendingTax, Equal<True>, And<PurchaseTax.reverseTax, Equal<True>>>, 
					TaxType.pendingSales,
				Case<Where<PurchaseTax.taxType, Equal<CSTaxType.use>,
						Or<PurchaseTax.taxType, Equal<CSTaxType.withholding>, 
						Or<PurchaseTax.taxType, Equal<CSTaxType.perUnit>, 
						Or<PurchaseTax.reverseTax, Equal<True>>>>>, 
					TaxType.sales>>>,
			TaxType.purchase>), typeof(string))]
		public virtual String TranTaxType
		{
			get
			{
				return this._TranTaxType;
			}
			set
			{
				this._TranTaxType = value;
			}
		}
		#endregion
	}
}
