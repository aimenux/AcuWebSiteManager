using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CA
{
	/// <summary>
	/// Contains totals of unprocessed Bank Transactions grouped by Cash Account
	/// </summary>
	[PXProjection(typeof(Select4<CABankTranSplitDebitCredit,
		Aggregate<
			GroupBy<CABankTranSplitDebitCredit.cashAccountID,
			Sum<CABankTranSplitDebitCredit.curyDebitAmount,
			Sum<CABankTranSplitDebitCredit.curyCreditAmount,
			Sum<CABankTranSplitDebitCredit.debitNumber,
			Sum<CABankTranSplitDebitCredit.creditNumber>>>>>>>))]
	[PXCacheName(Messages.BankTranByCashAccount)]
	public class CABankTranByCashAccount : IBqlTable
	{
		#region CashAccountID

		public abstract class cashAccountID : BqlInt.Field<cashAccountID> { }
		[PXDBInt(BqlField = typeof(CABankTranSplitDebitCredit.cashAccountID))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion

		#region CuryDebitAmount
		public abstract class curyDebitAmount : IBqlField { }
		[PXDBDecimal(BqlField = typeof(CABankTranSplitDebitCredit.curyDebitAmount))]
		[PXUIField(DisplayName = "Unprocessed Receipts")]
		public virtual decimal? CuryDebitAmount
		{
			get;
			set;
		}
		#endregion

		#region CuryCreditAmount
		public abstract class curyCreditAmount : IBqlField { }
		[PXDBDecimal(BqlField = typeof(CABankTranSplitDebitCredit.curyCreditAmount))]
		[PXUIField(DisplayName = "Unprocessed Disb.")]
		public virtual decimal? CuryCreditAmount
		{
			get;
			set;
		}
		#endregion

		#region DebitNumber
		public abstract class debitNumber : IBqlField { }

		[PXDBInt(BqlField = typeof(CABankTranSplitDebitCredit.debitNumber))]
		[PXUIField(DisplayName = "Receipt Count")]
		public virtual int? DebitNumber
		{
			get;
			set;
		}
		#endregion

		#region CreditNumber
		public abstract class creditNumber : IBqlField { }

		[PXDBInt(BqlField = typeof(CABankTranSplitDebitCredit.creditNumber))]
		[PXUIField(DisplayName = "Disbursement Count")]
		public virtual int? CreditNumber
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// Contains unprocessed Bank Transactions with the Amount split into Debit and Credit depending on DrCr value
	/// </summary>
	[PXProjection(typeof(Select<CABankTran,
		Where<CABankTran.processed, Equal<boolFalse>, And<CABankTran.tranType, Equal<CABankTranType.statement>>>>))]
	[PXHidden]
	public class CABankTranSplitDebitCredit : IBqlTable
	{
		#region CashAccountID
		public abstract class cashAccountID : IBqlField { }

		/// <summary>
		/// The cash account specified on the bank statement for which you want to upload bank transactions.
		/// This field is a part of the compound key of the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(CABankTran.cashAccountID))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion

		#region CuryDebitAmount
		public abstract class curyDebitAmount : IBqlField { }

		[PXDecimal]
		[PXDBCalced(typeof(IIf<Where<CABankTran.drCr, Equal<CADrCr.cADebit>>, CABankTran.curyTranAmt, decimal0>), typeof(decimal))]
		public virtual decimal? CuryDebitAmount
		{
			get;
			set;
		}
		#endregion

		#region CuryCreditAmount
		public abstract class curyCreditAmount : IBqlField { }

		[PXDecimal]
		[PXDBCalced(typeof(IIf<Where<CABankTran.drCr, Equal<CADrCr.cACredit>>, PX.Data.BQL.Minus<CABankTran.curyTranAmt>, decimal0>), typeof(decimal))]
		public virtual decimal? CuryCreditAmount
		{
			get;
			set;
		}
		#endregion

		#region DebitNumber
		public abstract class debitNumber : IBqlField { }

		[PXInt]
		[PXDBCalced(typeof(IIf<Where<CABankTran.drCr, Equal<CADrCr.cADebit>>, int1, int0>), typeof(int))]
		public virtual int? DebitNumber
		{
			get;
			set;
		}
		#endregion

		#region CreditNumber
		public abstract class creditNumber : IBqlField { }

		[PXInt]
		[PXDBCalced(typeof(IIf<Where<CABankTran.drCr, Equal<CADrCr.cACredit>>, int1, int0>), typeof(int))]
		public virtual int? CreditNumber
		{
			get;
			set;
		}
		#endregion
	}
}
