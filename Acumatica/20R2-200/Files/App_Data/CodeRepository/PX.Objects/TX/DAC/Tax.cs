using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.TX.Descriptor;

namespace PX.Objects.TX
{

	public static class CSTaxType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Sales, Use, VAT, Withholding, PerUnit },
				new string[] { Messages.Sales, Messages.Use, Messages.VAT, Messages.Withholding, Messages.PerUnit }) { }
		}

		[Obsolete("The ListSimpleAttribute class is obsolete and will be removed in future versions of Acumatica")]
		public class ListSimpleAttribute : PXStringListAttribute
		{
			public ListSimpleAttribute()
				: base(
				new string[] { Sales, Use, Withholding },
				new string[] { Messages.Sales, Messages.Use, Messages.Withholding }) { }
		}

		public static IEnumerable<(string TaxType, string Label)> GetTaxTypesWithLabels(bool includeVAT, bool includePerUnit)
		{
			yield return (Sales, Messages.Sales);
			yield return (Use, Messages.Use);

			if (includeVAT)
				yield return (VAT, Messages.VAT);

			yield return (Withholding, Messages.Withholding);

			if (includePerUnit)
				yield return (PerUnit, Messages.PerUnit);
		}

		public const string Sales = CSTaxBucketType.Sales;
		public const string Use = CSTaxBucketType.Purchase;
		public const string VAT = "V";
		public const string Withholding = "W";

		/// <summary>
		/// A per unit tax type for Per Unit/Specific taxes which calculation is based on quantity of items.
		/// </summary>
		public const string PerUnit = "Q";

		public class sales : PX.Data.BQL.BqlString.Constant<sales>
		{
			public sales() : base(Sales) { ;}
		}

		public class use : PX.Data.BQL.BqlString.Constant<use>
		{
			public use() : base(Use) { ;}
		}

		public class vat : PX.Data.BQL.BqlString.Constant<vat>
		{
			public vat() : base(VAT) { ;}
		}

		public class withholding : PX.Data.BQL.BqlString.Constant<withholding>
		{
			public withholding() : base(Withholding) { ;}
		}

		/// <summary>
		/// A per unit tax type for Per Unit/Specific taxes which calculation is based on quantity of items.
		/// </summary>
		public class perUnit : PX.Data.BQL.BqlString.Constant<perUnit>
		{
			public perUnit() : base(PerUnit) { }
		}
	}
	
	public static class CSTaxCalcType
	{
		public const string Item = "I";
		public const string Doc = "D";

		public class item : PX.Data.BQL.BqlString.Constant<item>
		{
			public item() : base(Item) { }
		}

		public class doc : PX.Data.BQL.BqlString.Constant<doc>
		{
			public doc() : base(Doc) { }
		}
	}

	public static class CSTaxCalcLevel
	{
		public const string Inclusive = "0";
		public const string CalcOnItemAmt = "1";
		public const string CalcOnItemAmtPlusTaxAmt = "2";

		public const string CalcOnItemQtyInclusively = "0";
		public const string CalcOnItemQtyExclusively = "1";

		public class inclusive : PX.Data.BQL.BqlString.Constant<inclusive>
		{
			public inclusive() : base(Inclusive) { ;}
		}

		public class calcOnItemAmt : PX.Data.BQL.BqlString.Constant<calcOnItemAmt>
		{
			public calcOnItemAmt() : base(CalcOnItemAmt) { ;}
		}

		public class calcOnItemAmtPlusTaxAmt : PX.Data.BQL.BqlString.Constant<calcOnItemAmtPlusTaxAmt>
		{
			public calcOnItemAmtPlusTaxAmt() : base(CalcOnItemAmtPlusTaxAmt) { ;}
		}

		public class calcOnItemQtyInclusively : PX.Data.BQL.BqlString.Constant<calcOnItemQtyInclusively>
		{
			public calcOnItemQtyInclusively() : base(CalcOnItemQtyInclusively) {; }
		}

		public class calcOnItemQtyExclusively : PX.Data.BQL.BqlString.Constant<calcOnItemQtyExclusively>
		{
			public calcOnItemQtyExclusively() : base(CalcOnItemQtyExclusively) {; }
		}
	}

	public static class CSTaxTermsDiscount
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				:base(
					new string[] { ToTaxableAmount, ToPromtPayment, NoAdjust },
					new string[] { Messages.DiscountToTaxableAmount, Messages.DiscountToPromtPayment, Messages.DiscountToTotalAmount }) {}
		}

		public const string ToTaxableAmount = "X";
		public const string ToPromtPayment = "P";
		public const string ToTaxAmount = "T";
		public const string AdjustTax = "A";
		public const string NoAdjust = "N";

		public class toPromtPayment : PX.Data.BQL.BqlString.Constant<toPromtPayment>
		{
			public toPromtPayment() : base(ToPromtPayment) {}
		}
	}

	#region One
	public sealed class One : Constant<int>
	{
		public One() : base(1) { }
	}
	#endregion

	/// <summary>
	/// Represents a tax.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(SalesTaxMaint))]
	[PXCacheName(Messages.Tax)]
	public partial class Tax : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<Tax>.By<taxID>
		{
			public static Tax Find(PXGraph graph, string taxID) => FindBy(graph, taxID);
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID>
		{
			/// <summary>
			/// 30
			/// </summary>
			public const int Length = 30;
		}
		protected String _TaxID;

		/// <summary>
		/// The tax ID. This is the key field, which can be specified by the user.
		/// </summary>
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), CacheGlobal = true)]
		[PX.Data.EP.PXFieldDescription]
		[PXReferentialIntegrityCheck]
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
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The description of the tax, which can be specified by the user.
		/// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region TaxType
		public abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }
		protected String _TaxType;

		/// <summary>
		/// The type of the tax.
		/// </summary>
		/// <value>
		/// The field can have the following values:
		/// <c>"S"</c>: Sales tax.
		/// <c>"P"</c>: Use (purchase) tax.
		/// <c>"V"</c>: Value-added tax (VAT).
		/// <c>"W"</c>: Withholding.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CSTaxType.Sales)]
		[CSTaxType.List()]
		[PXUIField(DisplayName = "Tax Type", Visibility=PXUIVisibility.Visible)]
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
		#region TaxCalcType
		public abstract class taxCalcType : PX.Data.BQL.BqlString.Field<taxCalcType> { }
        protected String _TaxCalcType;
		/// <summary>
		/// The basis of the tax calculation.
		/// </summary>
		/// <value>
		/// The field can have the following values:
		/// <c>"D"</c>: A tax is calculated on a document basis.
		/// <c>"I"</c>: A tax is calculated on a per-line basis.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CSTaxCalcType.Doc)]
		[PXStringList(new string[] { CSTaxCalcType.Doc, CSTaxCalcType.Item }, new string[] { "Document", "Item" })]
		[PXFormula(typeof(Substring<taxCalcRule, Zero, One>))]
        public virtual String TaxCalcType
        {
            get
            {
                return this._TaxCalcType;
            }
            set
            {
                this._TaxCalcType = value;
            }
        }
		#endregion
		#region TaxCalcLevel
		public abstract class taxCalcLevel : PX.Data.BQL.BqlString.Field<taxCalcLevel> { }
        protected String _TaxCalcLevel;
		/// <summary>
		/// The tax level.
		/// </summary>
		/// <value>
		/// The field can have the following values:
		/// <c>"0"</c>: An inclusive tax.
		/// <c>"1"</c>: A first-level exclusive tax.
		/// <c>"2"</c>: A second-level exclusive tax.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CSTaxCalcLevel.CalcOnItemAmt)]
		[PXStringList(new string[] { CSTaxCalcLevel.Inclusive, CSTaxCalcLevel.CalcOnItemAmt, CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt },
					new string[] { "Inclusive", "Calc. On Item Amount", "Calc. On Item Amt + Tax Amt" })]
		[PXFormula(typeof(Substring<taxCalcRule, One, One>))]
        public virtual String TaxCalcLevel
        {
            get
            {
                return this._TaxCalcLevel;
            }
            set
            {
                this._TaxCalcLevel = value;
            }
        }
		#endregion
		#region TaxCalcRule
		public abstract class taxCalcRule : PX.Data.BQL.BqlString.Field<taxCalcRule> { }
		protected String _TaxCalcRule;

		/// <summary>
		/// The aggregated rule of tax calculation based on <see cref="TaxCalcType"/> and <see cref="TaxCalcLevel"/>.
		/// </summary>
		/// <value>
		/// The field can have the following values:
		/// <c>"I0"</c>: The tax amount is included in item amounts and should be extracted.
		/// <c>"I1"</c>: The tax is a first-level tax calculated on item amounts.
		/// <c>"I2"</c>: The tax is a second-level tax calculated on item amounts.
		/// <c>"D1"</c>: The tax is a first-level tax calculated on the document amount.
		/// <c>"D2"</c>: The tax is a second-level tax calculated on the document amount.
		/// </value>
		[PXString(2, IsFixed = true)]
		[PXStringList(new string[] 
			{ 
				CSTaxCalcType.Item + CSTaxCalcLevel.Inclusive, 
				CSTaxCalcType.Item + CSTaxCalcLevel.CalcOnItemAmt, 
				CSTaxCalcType.Item + CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt, 
				CSTaxCalcType.Doc + CSTaxCalcLevel.Inclusive,
				CSTaxCalcType.Doc + CSTaxCalcLevel.CalcOnItemAmt, 
				CSTaxCalcType.Doc + CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt
			}, new string[] 
			{ 
				Messages.CalcRuleExtract,
				Messages.CalcRuleItemAmt,
				Messages.CalcRuleItemAmtPlusTaxAmt,
				Messages.CalcRuleDocInclusive,
				Messages.CalcRuleDocAmt,
				Messages.CalcRuleDocAmtPlusTaxAmt
			})]
		[PXDBCalced(typeof(Add<taxCalcType, taxCalcLevel>), typeof(string))]
		[PXFormula(typeof(IsNull<Add<Current2<TX.Tax.taxCalcType>, Current2<TX.Tax.taxCalcLevel>>, TX.Tax.taxCalcRule>))]
		[PXUIField(DisplayName = "Calculation Rule")]
        public virtual String TaxCalcRule
        {
            get
            {
                return _TaxCalcRule;
            }
            set
            {
                _TaxCalcRule = value;
            }
        }
		#endregion
		#region TaxCalcLevel2Exclude
		public abstract class taxCalcLevel2Exclude : PX.Data.BQL.BqlBool.Field<taxCalcLevel2Exclude> { }
		protected Boolean? _TaxCalcLevel2Exclude;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the system should exclude the first-level/per unit tax amount from the tax base
		/// that is used for calculation of the second-level tax amount in case of first-level taxes or all other taxes in case of per unit taxes.
		/// The flag is applicable to only first-level and per unit taxes.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Exclude from Tax-on-Tax Calculation")]
		public virtual Boolean? TaxCalcLevel2Exclude
		{
			get
			{
				return this._TaxCalcLevel2Exclude;
			}
			set
			{
				this._TaxCalcLevel2Exclude = value;
			}
		}
		#endregion
		#region TaxApplyTermsDisc
		public abstract class taxApplyTermsDisc : PX.Data.BQL.BqlString.Field<taxApplyTermsDisc> { }
		protected String _TaxApplyTermsDisc;

		/// <summary>
		/// The method of calculating the tax base amount if a cash discount is applied.
		/// </summary>
		/// <value>
		/// The field can have the following values:
		/// <c>"X"</c>: Reduce the taxable amount.
		/// <c>"P"</c>: Reduce taxable amount on early payment.
		/// <c>"N"</c>: Do not affect taxable amount.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CSTaxTermsDiscount.NoAdjust)]
		[CSTaxTermsDiscount.List()]
		[PXUIField(DisplayName = "Cash Discount")]
		public virtual String TaxApplyTermsDisc
		{
			get
			{
				return this._TaxApplyTermsDisc;
			}
			set
			{
				this._TaxApplyTermsDisc = value;
			}
		}
		#endregion
		#region PendingTax
		public abstract class pendingTax : PX.Data.BQL.BqlBool.Field<pendingTax> { }
		protected Boolean? _PendingTax;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax is a pending VAT. 
		/// The pending VAT should be calculated in documents, but should not be recorded in the tax report.
		/// Later the VAT of the pending type can be converted into the general VAT.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Pending VAT")]
		public virtual Boolean? PendingTax
		{
			get
			{
				return this._PendingTax;
			}
			set
			{
				this._PendingTax = value;
			}
		}
		#endregion
		#region ReverseTax
		public abstract class reverseTax : PX.Data.BQL.BqlBool.Field<reverseTax> { }
		protected Boolean? _ReverseTax;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax is a reverse VAT. When the reverse VAT is applied to a company that supplies goods or service to other EU countries,
		/// the liability of reporting VAT is reversed and goes to the customer rather than to the vendor.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Reverse VAT")]
		public virtual Boolean? ReverseTax
		{
			get
			{
				return this._ReverseTax;
			}
			set
			{
				this._ReverseTax = value;
			}
		}
		#endregion
		#region IncludeInTaxable
		public abstract class includeInTaxable : PX.Data.BQL.BqlBool.Field<includeInTaxable> { }
		protected Boolean? _IncludeInTaxable;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the VAT taxable amount should be displayed in the VAT Taxable Total box in the documents, 
		/// such as bills and invoices. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Include in VAT Taxable Total")]
		public virtual Boolean? IncludeInTaxable
		{
			get
			{
				return this._IncludeInTaxable;
			}
			set
			{
				this._IncludeInTaxable = value;
			}
		}
		#endregion
		#region ExemptTax
		public abstract class exemptTax : PX.Data.BQL.BqlBool.Field<exemptTax> { }
        protected Boolean? _ExemptTax;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the calculated amount should be displayed in the VAT Exempt Total box in the documents, such as bills and invoices.
		/// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include in VAT Exempt Total")]
        public virtual Boolean? ExemptTax
        {
            get
            {
                return this._ExemptTax;
            }
            set
            {
                this._ExemptTax = value;
            }
        }
        #endregion

        #region StatisticalTax
        public abstract class statisticalTax : PX.Data.BQL.BqlBool.Field<statisticalTax> { }
        protected Boolean? _StatisticalTax;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax is a statistical VAT. 
		/// The statistical VAT is calculated for statistical purposes; the VAT is reported but not paid.
		/// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Statistical VAT")]
        public virtual Boolean? StatisticalTax
        {
            get
            {
                return this._StatisticalTax;
            }
            set
            {
                this._StatisticalTax = value;
            }
        }
        #endregion
		#region DirectTax
		public abstract class directTax : PX.Data.BQL.BqlBool.Field<directTax> { }
		protected Boolean? _DirectTax;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax can be entered only by the documents from the Tax Bills and Adjustments form (TX303000).
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enter from Tax Bill")]
		public virtual Boolean? DirectTax
		{
			get
			{
				return this._DirectTax;
			}
			set
			{
				this._DirectTax = value;
			}
		}
		#endregion

		#region DeductibleVAT
		public abstract class deductibleVAT : PX.Data.BQL.BqlBool.Field<deductibleVAT> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax is a deductible VAT, 
		/// which means that a company is allowed to deduct some part of the tax paid to a vendor from its own VAT liability to the government.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Partially Deductible VAT")]
		public virtual Boolean? DeductibleVAT { get; set; }
		#endregion

		#region ReportExpenseToSingleAccount
		public abstract class reportExpenseToSingleAccount : PX.Data.BQL.BqlBool.Field<reportExpenseToSingleAccount> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the non-deductible part should be posted to the Tax Expense account.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Tax Expense Account", Visible = false)]
		public virtual bool? ReportExpenseToSingleAccount
		{
			get; set;
		}
		#endregion

		#region Specific and Per Unit Taxes
		#region TaxUOM
		/// <summary>
		/// The unit of measure used by tax. Specific/Per Unit taxes are calculated on quantities in this UOM.
		/// </summary>
		public abstract class taxUOM : PX.Data.BQL.BqlString.Field<taxUOM> { }

		/// <summary>
		/// The unit of measure used by tax. Specific/Per Unit taxes are calculated on quantities in this UOM
		/// </summary>
		[IN.INUnit(DisplayName = "Tax UOM", FieldClass = nameof(CS.FeaturesSet.PerUnitTaxSupport))]
		public virtual string TaxUOM
		{
			get;
			set;
		}
		#endregion

		#region PerUnitTaxPostMode
		/// <summary>
		/// An per-unit tax post mode for posting during the document release. 
		/// Possible options are - post tax amounts on tax account or post to document line's account.
		/// </summary>
		public abstract class perUnitTaxPostMode : PX.Data.BQL.BqlBool.Field<perUnitTaxPostMode> { }

		/// <summary>
		/// An per-unit tax post mode for posting during the document release. 
		/// Possible options are - post tax amounts on tax account or post to document line's account.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[Extensions.PerUnitTax.PerUnitTaxPostOptions.List]
		[PXDefault(Extensions.PerUnitTax.PerUnitTaxPostOptions.LineAccount)]
		[PXUIField(DisplayName = "Post To")]
		public virtual string PerUnitTaxPostMode { get; set; }
		#endregion

		#region SalesTaxAcctIDOverride Override
		public abstract class salesTaxAcctIDOverride : PX.Data.BQL.BqlInt.Field<salesTaxAcctIDOverride> { }

		/// <summary>
		/// The override of <see cref="Tax.SalesTaxAcctID"/> to display different labels for per-unit Tax.
		/// </summary>
		[Account(DisplayName = "Account to Use on Sale", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX, IsDBField = false)]
		public virtual Int32? SalesTaxAcctIDOverride
		{
			[PXDependsOnFields(typeof(salesTaxAcctID))]
			get => SalesTaxAcctID;
			set => SalesTaxAcctID = value;
		}
		#endregion

		#region SalesTaxAcctSubIDOverride
		public abstract class salesTaxSubIDOverride : PX.Data.BQL.BqlInt.Field<salesTaxSubIDOverride> { }

		/// <summary>
		/// The override of <see cref="Tax.SalesTaxSubID"/> to display different labels for per-unit Tax.
		/// </summary>
		[SubAccount(typeof(Tax.salesTaxAcctID), DisplayName = "Subaccount to Use on Sale", DescriptionField = typeof(Sub.description), IsDBField = false)]
		public virtual Int32? SalesTaxSubIDOverride
		{
			[PXDependsOnFields(typeof(salesTaxSubID))]
			get => SalesTaxSubID;
			set => SalesTaxSubID = value;
		}
		#endregion

		#region PurchTaxAcctIDOverride
		public abstract class purchTaxAcctIDOverride : PX.Data.BQL.BqlInt.Field<purchTaxAcctIDOverride> { }

		/// <summary>
		/// The override of <see cref="Tax.PurchTaxAcctID"/> to display different labels for per-unit Tax.
		/// </summary>
		[Account(DisplayName = "Account to Use on Purchase", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX, IsDBField = false)]
		public virtual Int32? PurchTaxAcctIDOverride
		{
			[PXDependsOnFields(typeof(purchTaxAcctID))]
			get => PurchTaxAcctID;
			set => PurchTaxAcctID = value;
		}
		#endregion

		#region PurchTaxSubIDOverride
		public abstract class purchTaxSubIDOverride : PX.Data.BQL.BqlInt.Field<purchTaxSubIDOverride> { }

		/// <summary>
		/// The override of <see cref="Tax.PurchTaxSubID"/> to display different labels for per-unit Tax.
		/// </summary>
		[SubAccount(typeof(Tax.purchTaxAcctID), DisplayName = "Subaccount to Use on Purchase", DescriptionField = typeof(Sub.description), IsDBField = false)]
		public virtual Int32? PurchTaxSubIDOverride
		{
			[PXDependsOnFields(typeof(purchTaxSubID))]
			get => PurchTaxSubID;
			set => PurchTaxSubID = value;
		}
		#endregion
		#endregion

		#region Tax Printing Parameters
		#region ShortPrintingLabel
		/// <summary>
		/// The Short Printing Label value is used to print tax in the invoice lines.
		/// </summary>
		public abstract class shortPrintingLabel : PX.Data.BQL.BqlString.Field<shortPrintingLabel> { }

		/// <summary>
		/// The Short Printing Label value is used to print tax in the invoice lines.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Short Printing Label")]
		public virtual string ShortPrintingLabel
		{
			get;
			set;
		}
		#endregion

		#region LongPrintingLabel
		/// <summary>
		/// The Long Printing Label of a tax is used to print total tax amount in the invoice totals section.
		/// </summary>
		public abstract class longPrintingLabel : PX.Data.BQL.BqlString.Field<longPrintingLabel> { }

		/// <summary>
		/// The Long Printing Label of a tax is used to print total tax amount in the invoice totals section.
		/// </summary>
		[PXDBLocalizableString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Long Printing Label")]
		public virtual string LongPrintingLabel
		{
			get;
			set;
		}
		#endregion

		#region PrintingSequence
		/// <summary>
		/// The Printing Sequence field value is used to define order in which taxes are printed on the documents.
		/// </summary>
		public abstract class printingSequence : PX.Data.BQL.BqlString.Field<printingSequence> { }

		/// <summary>
		/// The Printing Sequence field value is used to define order in which taxes are printed on the documents.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Printing Sequence")]
		public virtual int? PrintingSequence
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region TaxVendorID
		public abstract class taxVendorID : PX.Data.BQL.BqlInt.Field<taxVendorID> { }
		protected Int32? _TaxVendorID;

		/// <summary>
		/// The foreign key to <see cref="PX.Objects.AP.Vendor"/>, which specifies the tax agency to which the tax belongs.
		/// The key can be NULL.
		/// </summary>
		[TaxAgencyActive]
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
		#region SalesTaxAcctID
		public abstract class salesTaxAcctID : PX.Data.BQL.BqlInt.Field<salesTaxAcctID> { }
		protected Int32? _SalesTaxAcctID;

		/// <summary>
		/// The foreign key to <see cref="Account"/>, which specifies the liability account that accumulates the tax amounts to be paid to a tax agency for the tax reporting period.
		/// </summary>
		[PXDefault(typeof(Search<Vendor.salesTaxAcctID, Where<Vendor.bAccountID, Equal<Current<Tax.taxVendorID>>>>))]
		[Account(DisplayName = "Tax Payable Account", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX)]
		[PXForeignReference(typeof(Field<Tax.salesTaxAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? SalesTaxAcctID
		{
			get
			{
				return this._SalesTaxAcctID;
			}
			set
			{
				this._SalesTaxAcctID = value;
			}
		}
		#endregion
		#region SalesTaxSubID
		public abstract class salesTaxSubID : PX.Data.BQL.BqlInt.Field<salesTaxSubID> { }
		protected Int32? _SalesTaxSubID;

		/// <summary>
		/// The foreign key to <see cref="Sub"/>, which specifies the corresponding tax payable subaccount.
		/// </summary>
		[PXDefault(typeof(Search<Vendor.salesTaxSubID, Where<Vendor.bAccountID, Equal<Current<Tax.taxVendorID>>>>))]
		[SubAccount(typeof(Tax.salesTaxAcctID), DisplayName = "Tax Payable Subaccount", DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<Tax.salesTaxSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? SalesTaxSubID
		{
			get
			{
				return this._SalesTaxSubID;
			}
			set
			{
				this._SalesTaxSubID = value;
			}
		}
		#endregion
		#region PurchTaxAcctID
		public abstract class purchTaxAcctID : PX.Data.BQL.BqlInt.Field<purchTaxAcctID> { }
		protected Int32? _PurchTaxAcctID;

		/// <summary>
		/// The foreign key to <see cref="Account"/>, which specifies the account that accumulates the tax amounts to be claimed from the tax agency for the tax reporting period.
		/// </summary>
		[PXDefault(typeof(Search<Vendor.purchTaxAcctID, Where<Vendor.bAccountID, Equal<Current<Tax.taxVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Tax Claimable Account", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX)]
		[PXForeignReference(typeof(Field<Tax.purchTaxAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? PurchTaxAcctID
		{
			get
			{
				return this._PurchTaxAcctID;
			}
			set
			{
				this._PurchTaxAcctID = value;
			}
		}
		#endregion
		#region PurchTaxSubID
		public abstract class purchTaxSubID : PX.Data.BQL.BqlInt.Field<purchTaxSubID> { }
		protected Int32? _PurchTaxSubID;

		/// <summary>
		/// The foreign key to <see cref="Sub"/>, which specifies the corresponding tax claimable subaccount.
		/// </summary>
		[PXDefault(typeof(Search<Vendor.purchTaxSubID, Where<Vendor.bAccountID, Equal<Current<Tax.taxVendorID>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Tax.purchTaxAcctID), DisplayName = "Tax Claimable Subaccount", DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<Tax.purchTaxSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? PurchTaxSubID
		{
			get
			{
				return this._PurchTaxSubID;
			}
			set
			{
				this._PurchTaxSubID = value;
			}
		}
		#endregion
		#region PendingSalesTaxAcctID
		public abstract class pendingSalesTaxAcctID : PX.Data.BQL.BqlInt.Field<pendingSalesTaxAcctID> { }
		protected Int32? _PendingSalesTaxAcctID;

		/// <summary>
		/// The foreign key to <see cref="Account"/>, which specifies the liability account that accumulates the amount of taxes to be paid to a tax agency for the pending tax.
		/// </summary>
		[Account(DisplayName = "Pending Tax Payable Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<Tax.pendingSalesTaxAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? PendingSalesTaxAcctID
		{
			get
			{
				return this._PendingSalesTaxAcctID;
			}
			set
			{
				this._PendingSalesTaxAcctID = value;
			}
		}
		#endregion
		#region PendingSalesTaxSubID
		public abstract class pendingSalesTaxSubID : PX.Data.BQL.BqlInt.Field<pendingSalesTaxSubID> { }
		protected Int32? _PendingSalesTaxSubID;

		/// <summary>
		/// The foreign key to <see cref="Sub"/>, which specifies the corresponding tax payable pending subaccount.
		/// </summary>
		[SubAccount(typeof(Tax.pendingSalesTaxAcctID), DisplayName = "Pending Tax Payable Subaccount", DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<Tax.pendingSalesTaxSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? PendingSalesTaxSubID
		{
			get
			{
				return this._PendingSalesTaxSubID;
			}
			set
			{
				this._PendingSalesTaxSubID = value;
			}
		}
		#endregion
		#region PendingPurchTaxAcctID
		public abstract class pendingPurchTaxAcctID : PX.Data.BQL.BqlInt.Field<pendingPurchTaxAcctID> { }
		protected Int32? _PendingPurchTaxAcctID;

		/// <summary>
		/// The foreign key to <see cref="Account"/>, which specifies the account that accumulates the tax amounts to be claimed from the tax agency for the pending tax.
		/// </summary>
		[Account(DisplayName = "Pending Tax Claimable Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<Tax.pendingPurchTaxAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? PendingPurchTaxAcctID
		{
			get
			{
				return this._PendingPurchTaxAcctID;
			}
			set
			{
				this._PendingPurchTaxAcctID = value;
			}
		}
		#endregion
		#region PendingPurchTaxSubID
		public abstract class pendingPurchTaxSubID : PX.Data.BQL.BqlInt.Field<pendingPurchTaxSubID> { }
		protected Int32? _PendingPurchTaxSubID;

		/// <summary>
		/// The foreign key to <see cref="Sub"/>, which specifies the corresponding tax claimable pending subaccount.
		/// </summary>
		[SubAccount(typeof(Tax.pendingPurchTaxAcctID), DisplayName = "Pending Tax Claimable Subaccount", DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<Tax.pendingPurchTaxSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? PendingPurchTaxSubID
		{
			get
			{
				return this._PendingPurchTaxSubID;
			}
			set
			{
				this._PendingPurchTaxSubID = value;
			}
		}
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.BQL.BqlInt.Field<expenseAccountID> { }
		protected Int32? _ExpenseAccountID;

		/// <summary>
		/// The foreign key to <see cref="Account"/>, which specifies the expense account that is used to record either
		/// the tax amounts of use taxes or the non-deductible tax amounts of deductible value-added taxes.
		/// </summary>
		[PXDefault(typeof(Search<Vendor.taxExpenseAcctID, Where<Vendor.bAccountID, Equal<Current<Tax.taxVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Tax Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<Tax.expenseAccountID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? ExpenseAccountID
		{
			get
			{
				return this._ExpenseAccountID;
			}
			set
			{
				this._ExpenseAccountID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;

		/// <summary>
		/// The foreign key to <see cref="Sub"/>, which specifies the corresponding expense subaccount.
		/// </summary>
		[PXDefault(typeof(Search<Vendor.taxExpenseSubID, Where<Vendor.bAccountID, Equal<Current<Tax.taxVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Tax.expenseAccountID), DisplayName = "Tax Expense Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<Tax.expenseSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region Outdated
		public abstract class outdated : PX.Data.BQL.BqlBool.Field<outdated> { }
		protected Boolean? _Outdated;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that <see cref="OutDate"/> was specified.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Blocked", Enabled=false, Visibility=PXUIVisibility.SelectorVisible, Visible=false)]
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
		#region OutDate
		public abstract class outDate : PX.Data.BQL.BqlDateTime.Field<outDate> { }
		protected DateTime? _OutDate;

		/// <summary>
		///	The date after which the tax is not effective.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Not Valid After", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? OutDate
		{
			get
			{
				return this._OutDate;
			}
			set
			{
				this._OutDate = value;
			}
		}
		#endregion
		#region IsImported
		public abstract class isImported : PX.Data.BQL.BqlBool.Field<isImported> { }
		protected Boolean? _IsImported;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax configuration was imported from Avalara files.
		/// The field is obsolete.
		/// </summary>
		[Obsolete("This property is obsolete and will be removed in Acumatica 8.0")]
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
		#region IsExternal
		public abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }
		protected Boolean? _IsExternal;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the tax zone is used for the external tax provider.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsExternal
		{
			get
			{
				return this._IsExternal;
			}
			set
			{
				this._IsExternal = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(Tax.taxID),
			Selector = typeof(Search<Tax.taxID>))]
		public virtual Guid? NoteID { get; set; }
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
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region RetainageTaxPayableAcctID
		public abstract class retainageTaxPayableAcctID : PX.Data.BQL.BqlInt.Field<retainageTaxPayableAcctID> { }

		[Account(DisplayName = "Retainage Tax Payable Account", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX)]
		public virtual int? RetainageTaxPayableAcctID
		{
			get;
			set;
		}
		#endregion
		#region RetainageTaxPayableSubID
		public abstract class retainageTaxPayableSubID : PX.Data.BQL.BqlInt.Field<retainageTaxPayableSubID> { }

		[SubAccount(typeof(Tax.retainageTaxPayableAcctID), DisplayName = "Retainage Tax Payable Subaccount", DescriptionField = typeof(Sub.description))]
		public virtual int? RetainageTaxPayableSubID
		{
			get;
			set;
		}
		#endregion
		#region RetainageTaxClaimableAcctID
		public abstract class retainageTaxClaimableAcctID : PX.Data.BQL.BqlInt.Field<retainageTaxClaimableAcctID> { }

		[Account(DisplayName = "Retainage Tax Claimable Account", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX)]
		public virtual int? RetainageTaxClaimableAcctID
		{
			get;
			set;
		}
		#endregion
		#region RetainageTaxClaimableSubID
		public abstract class retainageTaxClaimableSubID : PX.Data.BQL.BqlInt.Field<retainageTaxClaimableSubID> { }

		[SubAccount(typeof(Tax.retainageTaxClaimableAcctID), DisplayName = "Retainage Tax Claimable Subaccount", DescriptionField = typeof(Sub.description))]
		public virtual int? RetainageTaxClaimableSubID
		{
			get;
			set;
		}
		#endregion
	}
}
