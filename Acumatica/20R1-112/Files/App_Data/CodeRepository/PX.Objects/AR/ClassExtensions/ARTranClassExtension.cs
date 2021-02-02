
namespace PX.Objects.AR
{
	public static class ARTranClassExtension
	{
		public static void ClearInvoiceDetailsBalance(this ARTran tran)
		{
			tran.CuryCashDiscBal = 0m;
			tran.CashDiscBal = 0m;

			tran.CuryRetainedTaxableAmt = 0m;
			tran.RetainedTaxableAmt = 0m;
			tran.CuryRetainedTaxAmt = 0m;
			tran.RetainedTaxAmt = 0m;

			tran.CuryRetainageBal = 0m;
			tran.RetainageBal = 0m;
			tran.CuryOrigRetainageAmt = 0m;
			tran.OrigRetainageAmt = 0m;

			tran.CuryOrigTranAmt = 0m;
			tran.OrigTranAmt = 0m;
			tran.CuryTranBal = 0m;
			tran.TranBal = 0m;

			tran.CuryOrigTaxableAmt = 0m;
			tran.OrigTaxableAmt = 0m;
			tran.CuryOrigTaxAmt = 0m;
			tran.OrigTaxAmt = 0m;
		}

		public static void RecoverInvoiceDetailsBalance(this ARTran tran)
		{
			tran.CuryRetainageBal = tran.CuryOrigRetainageAmt;
			tran.RetainageBal = tran.OrigRetainageAmt;

			tran.CuryTranBal = tran.CuryOrigTranAmt;
			tran.TranBal = tran.OrigTranAmt;
		}
	}
}