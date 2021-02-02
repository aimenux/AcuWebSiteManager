using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CR;

namespace PX.Objects.CA
{
	public interface IStatementReader
	{
		void Read(byte[] aInput);
		bool IsValidInput(byte[] aInput);
		bool AllowsMultipleAccounts();
		void ExportToNew<T>(PX.SM.FileInfo aFileInfo, T current, out List<CABankTranHeader> aExported)
			where T : CABankTransactionsImport, new();
	}
}

namespace PX.Objects.CA.BankStatementHelpers
{
	public interface IBankMatchRelevance
	{
		Decimal? MatchRelevance { get; set; }
	}
	
	[Serializable]
	public partial class CATranExt : CATran, IBankMatchRelevance
	{
		#region TranID
		public new abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }
		#endregion
		#region IsMatched
		public abstract class isMatched : PX.Data.BQL.BqlBool.Field<isMatched> { }
		protected Boolean? _IsMatched;
		[PXBool()]
		[PXUIField(DisplayName = "Matched")]
		public virtual Boolean? IsMatched
		{
			get
			{
				return this._IsMatched;
			}
			set
			{
				this._IsMatched = value;
			}
		}
		#endregion
		#region MatchRelevance
		public abstract class matchRelevance : PX.Data.BQL.BqlDecimal.Field<matchRelevance> { }
		protected Decimal? _MatchRelevance;
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDecimal(3)]
		[PXUIField(DisplayName = "Match Relevance", Enabled = false)]
		public virtual Decimal? MatchRelevance
		{
			get
			{
				return this._MatchRelevance;
			}
			set
			{
				this._MatchRelevance = value;
			}
		}
		#endregion
		#region MatchRelevancePercent
		public abstract class matchRelevancePercent : PX.Data.BQL.BqlDecimal.Field<matchRelevancePercent> { }
		
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDecimal(3)]
		[PXUIField(DisplayName = "Match Relevance, %", Enabled = false)]
		[PXFormula(typeof(Mult<CATranExt.matchRelevance, decimal100>))]
		public virtual Decimal? MatchRelevancePercent
		{
			get;
			set;
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		[PXDefault(false)]
		public override bool? Released
			{
			get;
			set;
		}
		#endregion
		#region CuryTranAbsAmt
		public abstract class curyTranAbsAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAbsAmt> { }
		[PXCurrency(typeof(CATran.curyInfoID), typeof(CATran.tranAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryTranAbsAmt
		{
			get
			{
				return Math.Abs(this.CuryTranAmt ?? 0m);
			}
			set
			{
			}
		}
		#endregion
		#region TranAbsAmt
		public abstract class tranAbsAmt : PX.Data.BQL.BqlDecimal.Field<tranAbsAmt> { }
		[PXDecimal(4)]
		[PXUIField(DisplayName = "Tran. Amount")]
		public virtual decimal? TranAbsAmt
		{
			get
			{
				return Math.Abs(this.TranAmt ?? 0m);
			}
			set
			{
			}
		}
		#endregion
        #region IsBestMatch
        public abstract class isBestMatch : PX.Data.BQL.BqlBool.Field<isBestMatch> { }
        protected Boolean? _IsBestMatch;
        [PXBool()]
        [PXUIField(DisplayName = "Best Match")]
        public virtual Boolean? IsBestMatch
        {
            get
            {
                return this._IsBestMatch;
            }
            set
            {
                this._IsBestMatch = value;
            }
        }
        #endregion
	}

	[Serializable]
	[PXHidden]
	public partial class CATran2 : CATran
	{
		#region TranID
		public new abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }
		#endregion
		#region CashAccountID
		public new abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		#endregion
		#region VoidedTranID
		public new abstract class voidedTranID : PX.Data.BQL.BqlLong.Field<voidedTranID> { }
		#endregion

		public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule>{}

		public new abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType>{}

		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>{}

		public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>{}
	}

	public interface IMatchSettings
	{
		int? DisbursementTranDaysBefore { get; set; }
		int? DisbursementTranDaysAfter { get; set; }

		int? ReceiptTranDaysBefore { get; set; }
		int? ReceiptTranDaysAfter { get; set; }

		Decimal? RefNbrCompareWeight { get; set; }
		Decimal? DateCompareWeight { get; set; }
		Decimal? PayeeCompareWeight { get; set; }

		Decimal? DateMeanOffset { get; set; }
		Decimal? DateSigma { get; set; }
		Boolean? SkipVoided { get; set; }

		Decimal? AmountWeight { get; set; }
		Decimal? CuryDiffThreshold { get; set; }

		bool? EmptyRefNbrMatching { get; set; }
		bool? AllowMatchingCreditMemo { get; set; }
	}

	[Serializable]
	public partial class MatchSettings : IBqlTable, IMatchSettings
	{
		#region ReceiptTranDaysBefore
		public abstract class receiptTranDaysBefore : PX.Data.BQL.BqlInt.Field<receiptTranDaysBefore> { }
		protected Int32? _ReceiptTranDaysBefore;
		[PXInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(5, typeof(CASetup.receiptTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Bank Transaction Date")]
		public virtual Int32? ReceiptTranDaysBefore
		{
			get
			{
				return this._ReceiptTranDaysBefore;
			}
			set
			{
				this._ReceiptTranDaysBefore = value;
			}
		}
		#endregion
		#region ReceiptTranDaysAfter
		public abstract class receiptTranDaysAfter : PX.Data.BQL.BqlInt.Field<receiptTranDaysAfter> { }
		protected Int32? _ReceiptTranDaysAfter;
		[PXInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(2, typeof(CASetup.receiptTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days After Bank Transaction Date")]
		public virtual Int32? ReceiptTranDaysAfter
		{
			get
			{
				return this._ReceiptTranDaysAfter;
			}
			set
			{
				this._ReceiptTranDaysAfter = value;
			}
		}
		#endregion
		#region DisbursementTranDaysBefore
		public abstract class disbursementTranDaysBefore : PX.Data.BQL.BqlInt.Field<disbursementTranDaysBefore> { }
		protected Int32? _DisbursementTranDaysBefore;
		[PXInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(5, typeof(CASetup.disbursementTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Bank Transaction Date")]
		public virtual Int32? DisbursementTranDaysBefore
		{
			get
			{
				return this._DisbursementTranDaysBefore;
			}
			set
			{
				this._DisbursementTranDaysBefore = value;
			}
		}
		#endregion
		#region DisbursementTranDaysAfter
		public abstract class disbursementTranDaysAfter : PX.Data.BQL.BqlInt.Field<disbursementTranDaysAfter> { }
		protected Int32? _DisbursementTranDaysAfter;
		[PXInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(2, typeof(CASetup.disbursementTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days After Bank Transaction Date")]
		public virtual Int32? DisbursementTranDaysAfter
		{
			get
			{
				return this._DisbursementTranDaysAfter;
			}
			set
			{
				this._DisbursementTranDaysAfter = value;
			}
		}
		#endregion
		#region AllowMatchingCreditMemo
		public abstract class allowMatchingCreditMemoIncPayments : PX.Data.BQL.BqlBool.Field<allowMatchingCreditMemoIncPayments> { }

		[PXBool]
		[PXDefault(false, typeof(CASetup.allowMatchingCreditMemoIncPayments), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Allow Matching to Credit Memo")]
		public virtual bool? AllowMatchingCreditMemo
		{
			get;
			set;
		}
		#endregion

		#region RefNbrCompareWeight
		public abstract class refNbrCompareWeight : PX.Data.BQL.BqlDecimal.Field<refNbrCompareWeight> { }
		protected Decimal? _RefNbrCompareWeight;
		[PXDecimal(MinValue = 0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "70.0", typeof(CASetup.refNbrCompareWeightIncPayments),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Ref. Nbr. Weight")]
		public virtual Decimal? RefNbrCompareWeight
		{
			get
			{
				return this._RefNbrCompareWeight;
			}
			set
			{
				this._RefNbrCompareWeight = value;
			}
		}
		#endregion
		#region DateCompareWeight
		public abstract class dateCompareWeight : PX.Data.BQL.BqlDecimal.Field<dateCompareWeight> { }
		protected Decimal? _DateCompareWeight;
		[PXDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "20.0",
			typeof(CASetup.dateCompareWeightIncPayments),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Date Weight")]
		public virtual Decimal? DateCompareWeight
		{
			get
			{
				return this._DateCompareWeight;
			}
			set
			{
				this._DateCompareWeight = value;
			}
		}
		#endregion
		#region PayeeCompareWeight
		public abstract class payeeCompareWeight : PX.Data.BQL.BqlDecimal.Field<payeeCompareWeight> { }
		protected Decimal? _PayeeCompareWeight;
		[PXDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "10.0",
				typeof(CASetup.payeeCompareWeightIncPayments),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Payee Weight")]
		public virtual Decimal? PayeeCompareWeight
		{
			get
			{
				return this._PayeeCompareWeight;
			}
			set
			{
				this._PayeeCompareWeight = value;
			}
		}
		#endregion
		protected Decimal TotalWeight
		{
			get
			{
				decimal total = (this._DateCompareWeight ?? Decimal.Zero)
								+ (this.RefNbrCompareWeight ?? Decimal.Zero)
								+ (this.PayeeCompareWeight ?? Decimal.Zero);
				return total;
			}

		}
		#region RefNbrComparePercent
		public abstract class refNbrComparePercent : PX.Data.BQL.BqlDecimal.Field<refNbrComparePercent> { }

		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? RefNbrComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.RefNbrCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
		#endregion
		#region EmptyRefNbrMatching
		public abstract class emptyRefNbrMatching : PX.Data.BQL.BqlBool.Field<emptyRefNbrMatching> { }

		[PXBool]
		[PXDefault(false, typeof(CASetup.emptyRefNbrMatching), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Consider Empty Ref. Nbr. as Matching", Visibility = PXUIVisibility.Visible)]
		public virtual bool? EmptyRefNbrMatching
		{
			get;
			set;
		}
		#endregion EmptyRefNbrMatching
		#region DateComparePercent
		public abstract class dateComparePercent : PX.Data.BQL.BqlDecimal.Field<dateComparePercent> { }
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? DateComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.DateCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
		#endregion
		#region PayeeComparePercent
		public abstract class payeeComparePercent : PX.Data.BQL.BqlDecimal.Field<payeeComparePercent> { }

		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? PayeeComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.PayeeCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
		#endregion
		#region DateMeanOffset
		public abstract class dateMeanOffset : PX.Data.BQL.BqlDecimal.Field<dateMeanOffset> { }
		protected Decimal? _DateMeanOffset;
		[PXDecimal(MinValue = -365, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "10.0",
			typeof(CASetup.dateMeanOffsetIncPayments),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Clearing Average Delay")]
		public virtual Decimal? DateMeanOffset
		{
			get
			{
				return this._DateMeanOffset;
			}
			set
			{
				this._DateMeanOffset = value;
			}
		}
		#endregion
		#region DateSigma
		public abstract class dateSigma : PX.Data.BQL.BqlDecimal.Field<dateSigma> { }
		protected Decimal? _DateSigma;
		[PXDecimal(MinValue = 0, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "5.0",
			typeof(CASetup.dateSigmaIncPayments),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Deviation (Days)")]
		public virtual Decimal? DateSigma
		{
			get
			{
				return this._DateSigma;
			}
			set
			{
				this._DateSigma = value;
			}
		}
		#endregion
		#region SkipVoided
		public abstract class skipVoided : PX.Data.BQL.BqlBool.Field<skipVoided> { }
		protected Boolean? _SkipVoided;
		[PXBool()]
		[PXDefault(false, typeof(CASetup.skipVoided),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Skip Voided Transactions During Matching")]
		public virtual Boolean? SkipVoided
		{
			get
			{
				return this._SkipVoided;
			}
			set
			{
				this._SkipVoided = value;
			}
		}
		#endregion
		public decimal? AmountWeight { get; set; }
		public decimal? CuryDiffThreshold { get; set; }
	}


	[PXHidden]
	public partial class GeneralInvoice : IBqlTable
	{
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        [Branch(Visibility = PXUIVisibility.SelectorVisible)]
        public int? BranchID
        {
            get;
            set;
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		public string RefNbr
		{
            get;
            set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }

		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[BatchModule.FullList]
		public virtual string OrigModule
		{
            get;
            set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual string DocType
		{
            get;
            set;
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		[PXDate]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
            get;
            set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[APOpenPeriod(typeof(APRegister.docDate))]
		[PXDefault]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string FinPeriodID
		{
            get;
            set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXDefault]
        [PXInt]
		[PXSelector(typeof(BAccountR.bAccountID),
					SubstituteKey = typeof(BAccountR.acctCD),
					DescriptionField = typeof(BAccountR.acctName))]

		[PXUIField(DisplayName = "Customer/Vendor", Enabled = false)]
		public virtual int? BAccountID
		{
            get;
            set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		[LocationID(
			typeof(Where<Location.bAccountID, Equal<Optional<GeneralInvoice.bAccountID>>,
				And<Location.isActive, Equal<boolTrue>,
				And<MatchWithBranch<Location.vBranchID>>>>),
			DescriptionField = typeof(Location.descr),
			Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? LocationID
		{
            get;
            set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
            get;
            set;
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXCury(typeof(GeneralInvoice.curyID))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryOrigDocAmt
		{
            get;
            set;
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }

		[PXCury(typeof(GeneralInvoice.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual decimal? CuryDocBal
		{
            get;
            set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual string Status
		{
            get;
            set;
        }
        #endregion
        #region DueDate
        public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }

		[PXDate]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DueDate
		{
            get;
            set;
		}
		#endregion
	}

	public class CurrencyInfoConditional : CurrencyInfoAttribute
	{
		Type _conditionField = null;
		public CurrencyInfoConditional(Type conditionField)
			: base()
		{
			this._conditionField = conditionField;
		}

		public override void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (this._conditionField != null)
			{
				Boolean? condition = sender.GetValue(e.Row, this._conditionField.Name) as Boolean?;
				if (condition ?? false)
					base.RowInserting(sender, e);
			}
			else
				base.RowInserting(sender, e);
		}
		public override void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (this._conditionField != null)
			{
				Boolean? newCondition = sender.GetValue(e.NewRow, this._conditionField.Name) as Boolean?;
				Boolean? condition = sender.GetValue(e.Row, this._conditionField.Name) as Boolean?;
				object value = sender.GetValue(e.Row, this._FieldName);
				if ((newCondition ?? false) || ((newCondition ?? false) && (condition ?? false) && value == null))
					base.RowUpdating(sender, e);
			}
			else
				base.RowUpdating(sender, e);
		}
	}
	
	public static class StatementMatching
	{
		private static decimal MatchEmptyStrings(string aStr1, string aStr2, bool matchEmpty = true)
		{
			return (String.IsNullOrEmpty(aStr1) && String.IsNullOrEmpty(aStr2) && matchEmpty) ? Decimal.One : Decimal.Zero;
		}

		public static decimal EvaluateMatching(string aStr1, string aStr2, bool aCaseSensitive, bool matchEmpty = true)
		{
			decimal result = Decimal.Zero;
			if (String.IsNullOrEmpty(aStr1) || String.IsNullOrEmpty(aStr2))
			{
				return MatchEmptyStrings(aStr1, aStr2, matchEmpty);
			}
			string str1 = aStr1.Trim();
			string str2 = aStr2.Trim();
			int length = str1.Length > str2.Length ? str1.Length : str2.Length;
			if (length == 0) return Decimal.One;
			Decimal charWeight = Decimal.One / (decimal)length;
			Decimal total = Decimal.Zero;
			for (int i = 0; i < length; i++)
			{
				if (i < str1.Length && i < str2.Length)
				{
					bool match = (aCaseSensitive) ? (str2[i].CompareTo(str1[i]) == 0) : (Char.ToLower(str2[i]).CompareTo(Char.ToLower(str1[i])) == 0);
					if (match)
						result += charWeight;
				}
				total += charWeight;
			}
			//Compencate rounding
			if (result > Decimal.Zero && total != Decimal.One)
			{
				result += (Decimal.One - total);
			}
			return result;
		}

		public static decimal EvaluateTideMatching(string aStr1, string aStr2, bool aCaseSensitive, bool matchEmpty = true)
		{
			decimal result = Decimal.One;
			const int maxDiffCount = 3;
			decimal[] distr = { Decimal.One, 0.5m, 0.25m, 0.05m };
			if (String.IsNullOrEmpty(aStr1) || String.IsNullOrEmpty(aStr2))
			{
				return MatchEmptyStrings(aStr1, aStr2, matchEmpty);
			}

			string str1 = aStr1.Trim();
			string str2 = aStr2.Trim();

			long strAsInt1, strAsInt2;
			if (Int64.TryParse(str1, out strAsInt1) && Int64.TryParse(str2, out strAsInt2))
			{
				return (strAsInt1 == strAsInt2 ? Decimal.One : Decimal.Zero);
			}

			int length = Math.Max(str1.Length, str2.Length);
			if (length == 0) return Decimal.One;
			int diff = Math.Abs(str1.Length - str2.Length);
			if (diff > maxDiffCount) return Decimal.Zero;
			int differentCount = 0;
			for (int i = 0; i < length; i++)
			{
				if (i < str1.Length && i < str2.Length)
				{
					bool match = (aCaseSensitive) ? (str2[i].CompareTo(str1[i]) == 0) : (Char.ToLower(str2[i]).CompareTo(Char.ToLower(str1[i])) == 0);
					if (!match)
						differentCount++;
				}
				else
				{
					differentCount++;
				}
				if (differentCount > maxDiffCount) return Decimal.Zero;
			}
			//Compencate rounding

			result = distr[differentCount];
			return result;
		}
	}

	public static class StatementApplicationBalances
	{

        public static void CalculateBalancesAR<TInvoice>(PXGraph graph, PXSelectBase<CurrencyInfo> curyInfoSelect, ARAdjust adj, TInvoice invoice, bool isCalcRGOL, bool DiscOnDiscDate)
            where TInvoice : IInvoice
        {
			Customer currentCustomer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Optional<CABankTran.payeeBAccountID>>>>.Select(graph);
			
            PaymentEntry.CalcBalances<TInvoice, ARAdjust>(curyInfoSelect, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, invoice, adj);
            if (DiscOnDiscDate)
            {
                PaymentEntry.CalcDiscount<TInvoice, ARAdjust>(adj.AdjgDocDate, invoice, adj);
            }
            PaymentEntry.WarnDiscount<TInvoice, ARAdjust>(graph, adj.AdjgDocDate, invoice, adj);

			CurrencyInfo pay_info = curyInfoSelect.Select(adj.AdjgCuryInfoID);
			CurrencyInfo vouch_info = curyInfoSelect.Select(adj.AdjdCuryInfoID);

			if (vouch_info != null && string.Equals(pay_info.CuryID, vouch_info.CuryID) == false)
			{
				adj.AdjdCuryRate = Math.Round((vouch_info.CuryMultDiv == "M" ? (decimal)vouch_info.CuryRate : 1 / (decimal)vouch_info.CuryRate) * (pay_info.CuryMultDiv == "M" ? 1 / (decimal)pay_info.CuryRate : (decimal)pay_info.CuryRate), 8, MidpointRounding.AwayFromZero);
			}
			else
			{
				adj.AdjdCuryRate = 1m;
			}

			if (currentCustomer != null && currentCustomer.SmallBalanceAllow == true && adj.AdjgDocType != ARDocType.Refund && adj.AdjdDocType != ARDocType.CreditMemo)
            {
                decimal payment_smallbalancelimit;
                CurrencyInfo payment_info = curyInfoSelect.Select(adj.AdjgCuryInfoID);
				PXDBCurrencyAttribute.CuryConvCury(curyInfoSelect.Cache, payment_info, currentCustomer.SmallBalanceLimit ?? 0m, out payment_smallbalancelimit);
                adj.CuryWOBal = payment_smallbalancelimit;
				adj.WOBal = currentCustomer.SmallBalanceLimit;
            }
            else
            {
                adj.CuryWOBal = 0m;
                adj.WOBal = 0m;
            }

            PaymentEntry.AdjustBalance<ARAdjust>(curyInfoSelect, adj);
            if (isCalcRGOL && (adj.Voided != true))
            {
                PaymentEntry.CalcRGOL<TInvoice, ARAdjust>(curyInfoSelect, invoice, adj);
                adj.RGOLAmt = (bool)adj.ReverseGainLoss ? -1.0m * adj.RGOLAmt : adj.RGOLAmt;
            }
        }
	}
}
