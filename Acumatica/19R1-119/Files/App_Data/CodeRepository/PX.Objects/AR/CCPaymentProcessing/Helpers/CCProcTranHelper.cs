using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.SO;
namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	public static class CCProcTranHelper
	{
		public class CCProcTranOrderComparer : IComparer<CCProcTran>,IComparer<ICCPaymentTransaction>
		{
			private bool _descending;

			public CCProcTranOrderComparer(bool aDescending)
			{
				this._descending = aDescending;
			}
			public CCProcTranOrderComparer()
			{
				this._descending = false;
			}

			#region IComparer<CCProcTran> Members

			public virtual int Compare(CCProcTran x, CCProcTran y)
			{
				int order = x.TranNbr.Value.CompareTo(y.TranNbr.Value);
				return (this._descending ? -order : order);
			}

			public virtual int Compare(ICCPaymentTransaction pt1, ICCPaymentTransaction pt2)
			{
				int order = pt1.TranNbr.Value.CompareTo(pt2.TranNbr.Value);
				return this._descending ? -order : order;
			}
			#endregion
		}

		public static string FormatCCPaymentState(CCPaymentState aState)
		{
			Dictionary<CCPaymentState, string> stateDict = new Dictionary<CCPaymentState, string>();
			stateDict[CCPaymentState.None] = PXMessages.LocalizeNoPrefix(Messages.CCNone);
			stateDict[CCPaymentState.PreAuthorized] = PXMessages.LocalizeNoPrefix(Messages.CCPreAuthorized);
			stateDict[CCPaymentState.PreAuthorizationFailed] = PXMessages.LocalizeNoPrefix(Messages.CCPreAuthorizationFailed);
			stateDict[CCPaymentState.PreAuthorizationExpired] = PXMessages.LocalizeNoPrefix(Messages.CCPreAuthorizationExpired);
			stateDict[CCPaymentState.Captured] = PXMessages.LocalizeNoPrefix(Messages.CCCaptured);
			stateDict[CCPaymentState.CaptureFailed] = PXMessages.LocalizeNoPrefix(Messages.CCCaptureFailed);
			stateDict[CCPaymentState.Voided] = PXMessages.LocalizeNoPrefix(Messages.CCVoided);
			stateDict[CCPaymentState.VoidFailed] = PXMessages.LocalizeNoPrefix(Messages.CCVoidFailed);
			stateDict[CCPaymentState.Refunded] = PXMessages.LocalizeNoPrefix(Messages.CCRefunded);
			stateDict[CCPaymentState.RefundFailed] = PXMessages.LocalizeNoPrefix(Messages.CCRefundFailed);
			stateDict[CCPaymentState.AuthorizedHoldingReview] = PXMessages.LocalizeNoPrefix(Messages.CCAuthorizedHoldingReview);
			stateDict[CCPaymentState.CapturedHoldingReview] = PXMessages.LocalizeNoPrefix(Messages.CCCapturedHoldingReview);
			StringBuilder result = new StringBuilder();
			foreach (KeyValuePair<CCPaymentState, string> it in stateDict)
			{
				if ((aState & it.Key) != 0)
				{
					if (result.Length > 0)
						result.Append(",");
					result.Append(it.Value);
				}
			}
			return result.ToString();
		}

	    public static IEnumerable<ICCPaymentTransaction> GetSOInvoiceCCProcTrans(PXGraph graph, ARInvoice currentInvoice)
	    {
	        Dictionary<int, CCProcTran> existsingTran = new Dictionary<int, CCProcTran>();
	        foreach (CCProcTran iTran in PXSelectReadonly<CCProcTran,
	            Where<CCProcTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
	                And<CCProcTran.docType, Equal<Current<ARInvoice.docType>>>>,
	            OrderBy<Desc<CCProcTran.tranNbr>>>.SelectMultiBound(graph, new object[] {currentInvoice}))
	        {
	            if (existsingTran.ContainsKey(iTran.TranNbr.Value)) continue;
	            existsingTran[iTran.TranNbr.Value] = iTran;
	            yield return iTran;
	        }

	        foreach (CCProcTran iTran1 in PXSelectReadonly2<CCProcTran,
	            InnerJoin<SOOrderShipment, On<SOOrderShipment.orderNbr, Equal<CCProcTran.origRefNbr>,
	                And<SOOrderShipment.orderType, Equal<CCProcTran.origDocType>>>>,
	            Where<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>,
	                And<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>,
	                    And<CCProcTran.refNbr, IsNull>>>,
	            OrderBy<Desc<CCProcTran.tranNbr>>>.SelectMultiBound(graph, new object[] { currentInvoice }))
	        {
	            if (existsingTran.ContainsKey(iTran1.TranNbr.Value)) continue;
	            existsingTran[iTran1.TranNbr.Value] = iTran1;
	            yield return iTran1;
	        }
        }

		public static CCPaymentState ResolveCCPaymentState(IEnumerable<ICCPaymentTransaction> ccProcTrans)
		{
			ICCPaymentTransaction lastTran;
			return ResolveCCPaymentState(ccProcTrans, out lastTran);
		}
		public static CCPaymentState ResolveCCPaymentState(IEnumerable<ICCPaymentTransaction> ccProcTrans, out ICCPaymentTransaction aLastTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTrans);
			return ResolveCCPaymentStateInt(filtered, out aLastTran);
		}

		public static IEnumerable<ICCPaymentTransaction> FilterDeclinedTrans(IEnumerable<ICCPaymentTransaction> ccProcTrans)
		{
			bool declineFound = false;
			string declinedPCTranNbr = null;
			foreach (ICCPaymentTransaction tran in ccProcTrans)
			{
				if (!declineFound && tran.TranStatus == CCTranStatusCode.Declined 
					&& ccProcTrans.Any(i => i.PCTranNumber == tran.PCTranNumber && i.TranStatus == CCTranStatusCode.HeldForReview))
				{
					declineFound = true;
					declinedPCTranNbr = tran.PCTranNumber;
				}
				else if (declineFound && tran.PCTranNumber == declinedPCTranNbr)
				{
					if (tran.TranStatus == CCTranStatusCode.HeldForReview)
					{
						declineFound = false;
					}
				}
				else
				{
					yield return tran;
				}
			}
		}

		private static CCPaymentState ResolveCCPaymentStateInt(IEnumerable<ICCPaymentTransaction> ccProcTrans, out ICCPaymentTransaction aLastTran)
		{
			CCPaymentState result = CCPaymentState.None;
			ICCPaymentTransaction lastTran = null;
			ICCPaymentTransaction lastSSTran = null; //Last successful tran
			ICCPaymentTransaction preLastSSTran = null;
	        CCProcTranOrderComparer ascComparer = new CCProcTranOrderComparer();
	        aLastTran = null;
			foreach (ICCPaymentTransaction iTran in ccProcTrans)
	        {
	            if (iTran.ProcStatus != CCProcStatus.Finalized && iTran.ProcStatus != CCProcStatus.Error)
	                continue;
	            if (lastTran == null)
	            {
	                lastTran = iTran;
	            }
	            else
	            {
	                if (ascComparer.Compare(iTran, lastTran) > 0)
	                {
	                    lastTran = iTran;
	                }
	            }

				if (iTran.TranStatus == CCTranStatusCode.Approved || iTran.TranStatus == CCTranStatusCode.HeldForReview)
				{
					if (lastSSTran == null)
					{
						lastSSTran = iTran;
					}
					else if (ascComparer.Compare(iTran, lastSSTran) > 0)
					{
						lastSSTran = iTran;
					}

	                if (lastSSTran != null && (ascComparer.Compare(iTran, lastSSTran) < 0))
	                {
	                    if (preLastSSTran == null)
	                    {
	                        preLastSSTran = iTran;
	                    }
	                    else if (ascComparer.Compare(iTran, preLastSSTran) > 0)
	                    {
	                        preLastSSTran = iTran;
	                    }
	                }
	            }
	        }

			if (lastTran != null)
			{
				if (lastSSTran != null)
				{
					switch (lastSSTran.TranType)
					{
						case CCTranTypeCode.Authorize:
							if (!IsExpired(lastSSTran))
								result = CCPaymentState.PreAuthorized;
							else
								result = CCPaymentState.PreAuthorizationExpired;
							break;
						case CCTranTypeCode.AuthorizeAndCapture:
						case CCTranTypeCode.PriorAuthorizedCapture:
						case CCTranTypeCode.CaptureOnly:
							result = CCPaymentState.Captured;
							break;
						case CCTranTypeCode.VoidTran:
							if (preLastSSTran != null)
							{
								result = (preLastSSTran.TranType == CCTranTypeCode.Authorize) ?
											result = CCPaymentState.None : result = CCPaymentState.Voided; //Voidin of credit currenly is not allowed								
							}
							break;
						case CCTranTypeCode.Credit:
							result = CCPaymentState.Refunded;
							break;
					}

					if (lastTran.TranStatus == CCTranStatusCode.HeldForReview && lastTran.TranType == CCTranTypeCode.Authorize)
					{
						result = CCPaymentState.AuthorizedHoldingReview;
					}

					if (lastTran.TranStatus == CCTranStatusCode.HeldForReview && lastTran.TranType == CCTranTypeCode.AuthorizeAndCapture)
					{
						result = CCPaymentState.CapturedHoldingReview;
					}
				}

				if (lastSSTran == null || lastSSTran.TranNbr != lastTran.TranNbr) //this means that lastOp failed
				{
					switch (lastTran.TranType)
					{
						case CCTranTypeCode.Authorize:
							result |= CCPaymentState.PreAuthorizationFailed;
							break;
						case CCTranTypeCode.AuthorizeAndCapture:
						case CCTranTypeCode.PriorAuthorizedCapture:
						case CCTranTypeCode.CaptureOnly:
							result |= CCPaymentState.CaptureFailed;
							break;
						case CCTranTypeCode.VoidTran:
							result |= CCPaymentState.VoidFailed;
							break;
						case CCTranTypeCode.Credit:
							result |= CCPaymentState.RefundFailed;
							break;
					}
				}
			}
			aLastTran = lastTran;
			return result;
		}

	    public static bool HasSuccessfulCCTrans(PXSelectBase<CCProcTran> ccProcTran)
		{
			CCProcTran lastSTran = FindCCLastSuccessfulTran(ccProcTran);
			if (lastSTran != null && !IsExpired(lastSTran))
			{
				return true;
			}
			return false;
		}

		public static bool HasUnfinishedCCTrans(PXGraph aGraph, CustomerPaymentMethod aCPM)
		{
			if (aCPM.PMInstanceID < 0)
			{
				return false;
			}

			Dictionary<string, List<PXResult<CCProcTran>>> TranDictionary = new Dictionary<string, List<PXResult<CCProcTran>>>();
			PXResultset<CCProcTran> ccTrans = PXSelect<CCProcTran, Where<CCProcTran.pMInstanceID, Equal<Required<CCProcTran.pMInstanceID>>,
					And<CCProcTran.pCTranNumber, IsNotNull>>, OrderBy<Asc<CCProcTran.pCTranNumber>>>.Select(aGraph, aCPM.PMInstanceID);

			foreach (var row in ccTrans)
			{
				CCProcTran tran = (CCProcTran)row;
				if (tran.PCTranNumber != "0")
				{
					if (!TranDictionary.ContainsKey(tran.PCTranNumber))
					{
						TranDictionary[tran.PCTranNumber] = new List<PXResult<CCProcTran>>();
					}
					TranDictionary[tran.PCTranNumber].Add(row);
				}
			}

			bool hasUnfinishedTrans = false;
			foreach (var kvp in TranDictionary)
			{
				List<PXResult<CCProcTran>> tranList = kvp.Value;
				ICCPaymentTransaction lastTran;
				IEnumerable<ICCPaymentTransaction> trans = tranList.RowCast<CCProcTran>().Cast<ICCPaymentTransaction>();
				CCPaymentState ccPaymentState = ResolveCCPaymentState(trans, out lastTran);
				bool isCCPreAuthorized = (ccPaymentState & CCPaymentState.PreAuthorized) != 0;
				if (isCCPreAuthorized && lastTran != null && (lastTran.ExpirationDate == null || lastTran.ExpirationDate > DateTime.Now))
				{
					hasUnfinishedTrans = true;
					break;
				}
			}
			return hasUnfinishedTrans;
		}

		public static bool UpdateCapturedState<T>(T doc, IEnumerable<PXResult<CCProcTran>> ccProcTrans)
			where T : class, IBqlTable, ICCCapturePayment
		{
			bool needUpdate = false;
			PaymentState state = new PaymentState(ccProcTrans);
			ICCPaymentTransaction lastTran = state.lastTran;
			if (doc.IsCCCaptured != state.isCCCaptured)
			{
				doc.IsCCCaptured = state.isCCCaptured;
				needUpdate = true;
			}

			if (lastTran != null
				&& (lastTran.TranType == CCTranTypeCode.PriorAuthorizedCapture
					|| lastTran.TranType == CCTranTypeCode.AuthorizeAndCapture
					|| lastTran.TranType == CCTranTypeCode.CaptureOnly))
			{
				if (state.isCCCaptured)
				{
					doc.CuryCCCapturedAmt = lastTran.Amount;
					doc.IsCCCaptureFailed = false;
					needUpdate = true;
				}
				else
				{
					doc.IsCCCaptureFailed = true;
					needUpdate = true;
				}
			}

			if (doc.IsCCCaptured == false && (doc.CuryCCCapturedAmt != Decimal.Zero))
			{
				doc.CuryCCCapturedAmt = Decimal.Zero;
				needUpdate = true;
			}

			return needUpdate;
		}

		public struct CCTransState
		{
			public bool NeedUpdate;
			public PaymentState PaymentState;
		}

		public static CCTransState UpdateCCPaymentState<T>(T doc, IEnumerable<PXResult<CCProcTran>> ccProcTrans)
			where T : class, ICCAuthorizePayment, ICCCapturePayment
		{
			PaymentState ps = new PaymentState(ccProcTrans);
			ICCPaymentTransaction lastTran = ps.lastTran;
			bool needUpdate = false;

			if (doc.IsCCAuthorized != ps.isCCPreAuthorized || doc.IsCCCaptured != ps.isCCCaptured)
			{
				if (!(ps.isCCVoidingAttempted || ps.isRefundAttempted))
				{
					doc.IsCCAuthorized = ps.isCCPreAuthorized;
					doc.IsCCCaptured = ps.isCCCaptured;
					needUpdate = true;
				}
				else
				{
					doc.IsCCAuthorized = false;
					doc.IsCCCaptured = false;
					needUpdate = false;
				}
			}

			if (lastTran != null && ps.isCCPreAuthorized && lastTran.TranType == CCTranTypeCode.Authorize)
			{
				doc.CCAuthExpirationDate = lastTran.ExpirationDate;
				doc.CuryCCPreAuthAmount = lastTran.Amount;
				needUpdate = true;
			}

			if (doc.IsCCAuthorized == false && (doc.CCAuthExpirationDate != null || doc.CuryCCPreAuthAmount > Decimal.Zero))
			{
				doc.CCAuthExpirationDate = null;
				doc.CuryCCPreAuthAmount = Decimal.Zero;

				needUpdate = true;
			}

			if (lastTran != null
				&& (lastTran.TranType == CCTranTypeCode.PriorAuthorizedCapture
					|| lastTran.TranType == CCTranTypeCode.AuthorizeAndCapture
					|| lastTran.TranType == CCTranTypeCode.CaptureOnly))
			{
				if (ps.isCCCaptured)
				{
					doc.CuryCCCapturedAmt = lastTran.Amount;
					doc.IsCCCaptureFailed = false;
					needUpdate = true;
				}
				else
				{
					doc.IsCCCaptureFailed = true;
					needUpdate = true;
				}
			}

			if (doc.IsCCCaptured == false && (doc.CuryCCCapturedAmt != Decimal.Zero))
			{
				doc.CuryCCCapturedAmt = Decimal.Zero;
				needUpdate = true;
			}

			return new CCTransState { NeedUpdate = needUpdate, PaymentState = ps };
		}

		public static bool IsExpired(ICCPaymentTransaction tran)
		{
			return tran.ExpirationDate.HasValue && tran.ExpirationDate.Value < PXTimeZoneInfo.Now;
		}

		public static bool HasOpenCCTran(IEnumerable<ICCPaymentTransaction> paymentTransaction)
		{
			return paymentTransaction.Any(i=>i.ProcStatus == CCProcStatus.Opened && !IsExpired(i));
		}

		public static bool HasCCTransactions(PXSelectBase<CCProcTran> ccProcTran)
		{
			return (ccProcTran.Any());
		}

		public static ICCPaymentTransaction FindCCPreAuthorizing(IEnumerable<ICCPaymentTransaction> ccProcTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTran);
			List<ICCPaymentTransaction> authTrans = new List<ICCPaymentTransaction>(1);
			List<ICCPaymentTransaction> result = new List<ICCPaymentTransaction>(1);
			foreach (ICCPaymentTransaction iTran in filtered)
			{
				if (iTran.ProcStatus != CCProcStatus.Finalized)
					continue;
				if (iTran.TranType == CCTranTypeCode.Authorize && (iTran.TranStatus == CCTranStatusCode.Approved || iTran.TranStatus == CCTranStatusCode.HeldForReview))
				{
					if(authTrans.Where(i => i.PCTranNumber == iTran.PCTranNumber).Count() == 0)
						authTrans.Add(iTran);
				}
			}

			foreach (ICCPaymentTransaction it in authTrans)
			{
				bool cancelled = false;
				foreach (ICCPaymentTransaction iTran in ccProcTran)
				{
					if (iTran.ProcStatus != CCProcStatus.Finalized)
						continue;
					if (iTran.RefTranNbr == it.TranNbr && iTran.TranStatus == CCTranStatusCode.Approved || iTran.TranStatus == CCTranStatusCode.HeldForReview)
					{
						if (iTran.TranType.Trim() == CCTranTypeCode.PriorAuthorizedCapture
								|| iTran.TranType.Trim() == CCTranTypeCode.VoidTran)
						{
							cancelled = true;
							break;
						}
					}
				}
				if (!cancelled)
				{
					result.Add(it);
				}
			}

			if (result.Count > 0)
			{
				result.Sort(new CCProcTranOrderComparer(true)
						/*new Comparison<CCProcTran>(delegate(CCProcTran a, CCProcTran b)
						{
							return a.EndTime.Value.CompareTo(b.EndTime.Value);
						}) */
						);
				return result[0];
			}
			return null;
		}

		public static ICCPaymentTransaction FindCCLastSuccessfulTran(IEnumerable<ICCPaymentTransaction> ccProcTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTran);
			ICCPaymentTransaction lastTran = null;
			CCProcTranOrderComparer ascComparer = new CCProcTranOrderComparer();
			foreach (ICCPaymentTransaction iTran in filtered)
			{
				if (iTran.ProcStatus != CCProcStatus.Finalized)
					continue;
				if (iTran.TranStatus == CCTranStatusCode.Approved || iTran.TranStatus == CCTranStatusCode.HeldForReview)
				{
					if (lastTran == null || (ascComparer.Compare(iTran, lastTran) > 0))
					{
						lastTran = iTran;
					}
				}
			}
			return lastTran;
		}

		public static CCProcTran FindCCLastSuccessfulTran(PXSelectBase<CCProcTran> ccProcTran)
		{
			CCProcTran lastTran = null;
			IEnumerable<ICCPaymentTransaction> trans = ccProcTran.Select().RowCast<CCProcTran>().Cast<ICCPaymentTransaction>();
			ICCPaymentTransaction lastTranImpl = FindCCLastSuccessfulTran(trans);
			lastTran = (CCProcTran)lastTranImpl;
			return lastTran;
		}

		public static ICCPaymentTransaction FindOpenForReviewTran(IEnumerable<ICCPaymentTransaction> ccProcTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTran);
			ICCPaymentTransaction openTran = filtered.Where(i => i.TranStatus == CCTranStatusCode.HeldForReview)
				.Where(i => !ccProcTran.Where(ii => ii.PCTranNumber == i.PCTranNumber && ii.TranNbr != i.TranNbr).Any())
				.FirstOrDefault();
			return openTran;
		}

		public static IEnumerable<ICCPaymentTransaction> FindAuthCaptureActiveTrans(IEnumerable<ICCPaymentTransaction> ccProcTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTran).OrderBy(i => i.TranNbr);
			List<ICCPaymentTransaction> activeTrans = new List<ICCPaymentTransaction>();
			foreach (ICCPaymentTransaction item in filtered)
			{
				if (item.ProcStatus == CCProcStatus.Error)
					continue;

				if (item.TranType == CCTranTypeCode.AuthorizeAndCapture || item.TranType == CCTranTypeCode.Authorize || 
					item.TranType == CCTranTypeCode.CaptureOnly || item.TranType == CCTranTypeCode.PriorAuthorizedCapture)
				{
					if (item.RefTranNbr != null)
					{
						var refTran = activeTrans.Where(i => i.TranNbr == item.RefTranNbr).FirstOrDefault();
						if (refTran != null)
						{
							activeTrans.Remove(refTran);
						}
					}

					activeTrans.Add(item);
				}

				if (item.TranType == CCTranTypeCode.VoidTran)
				{
					var refTran = activeTrans.Where(i => i.PCTranNumber == item.PCTranNumber).FirstOrDefault();
					if (refTran != null)
					{
						activeTrans.Remove(refTran);
					}
				}

				if (item.TranType == CCTranTypeCode.Credit)
				{
					var refTran = activeTrans.Where(i => i.TranNbr == item.RefTranNbr).FirstOrDefault();
					if (refTran != null)
					{
						activeTrans.Remove(refTran);
					}
				}
			}
			return activeTrans;
		}

		public static bool IsCCPreAuthorizedSOInvoice(PXGraph graph, ARInvoice doc)
		{
			PaymentState ps = new PaymentState(GetSOInvoiceCCProcTrans(graph, doc));
			return ps.isCCPreAuthorized;
		}
		
		public static bool IsOrderSelfCaptured(PXGraph graph, SOOrder doc)
		{
			return PXSelectReadonly<CCProcTran,
				Where<CCProcTran.origDocType, Equal<Required<SOOrder.orderType>>, And<CCProcTran.origRefNbr, Equal<Required<SOOrder.orderNbr>>,
				And<CCProcTran.origDocType, NotEqual<CCProcTran.docType>, And<CCProcTran.origRefNbr, NotEqual<CCProcTran.refNbr>>>>>>
				       .SelectWindowed(graph, 0, 1, doc.OrderType, doc.OrderNbr).Count == 0;
		}
	}
}
