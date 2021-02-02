using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public class PaymentState
	{
		public bool isCCVoided = false;
		public bool isCCCaptured = false;
		public bool isCCPreAuthorized = false;
		public bool isCCRefunded = false;
		public bool isCCVoidingAttempted = false; //Special flag for VoidPayment Release logic
		public bool isRefundAttempted = false;
		public bool isNone = false;
		public bool isOpenForReview = false;
		public string Description;
		public ICCPaymentTransaction lastTran;
		/// <summary>
		/// Flag that indicates that credit card transaction was submitted for settlement. 
		/// </summary>
		public bool IsSettlementDue => isCCCaptured || isCCRefunded;

		public PaymentState() : this(Enumerable.Empty<ICCPaymentTransaction>())
		{

		}

		public PaymentState(IEnumerable<PXResult<CCProcTran>> ccProcTrans)
		{
			IEnumerable<ICCPaymentTransaction> trans = ccProcTrans.RowCast<CCProcTran>().Cast<ICCPaymentTransaction>();
			CCPaymentState ccPaymentState = CCProcTranHelper.ResolveCCPaymentState(trans, out lastTran);
			SetFields(ccPaymentState, trans);
		}

		public PaymentState(IEnumerable<ICCPaymentTransaction> trans)
		{
			CCPaymentState ccPaymentState = CCProcTranHelper.ResolveCCPaymentState(trans, out lastTran);
			SetFields(ccPaymentState, trans);
		}

		private void SetFields(CCPaymentState ccPaymentState, IEnumerable<ICCPaymentTransaction> trans)
		{
			isCCVoided = ccPaymentState.HasFlag(CCPaymentState.Voided);
			isCCCaptured = ccPaymentState.HasFlag(CCPaymentState.Captured) || 
				ccPaymentState.HasFlag(CCPaymentState.CapturedHoldingReview);
			isCCPreAuthorized = ccPaymentState.HasFlag(CCPaymentState.PreAuthorized) ||
				ccPaymentState.HasFlag(CCPaymentState.AuthorizedHoldingReview);
			isCCRefunded = ccPaymentState.HasFlag(CCPaymentState.Refunded);
			isCCVoidingAttempted = ccPaymentState.HasFlag(CCPaymentState.VoidFailed);
			isRefundAttempted = ccPaymentState.HasFlag(CCPaymentState.RefundFailed);
			isNone = ccPaymentState == CCPaymentState.None;
			Description = CCProcTranHelper.FormatCCPaymentState(ccPaymentState);
			isOpenForReview = CCProcTranHelper.FindOpenForReviewTran(trans) != null;
		}
	}
}
