using PX.Api;
using PX.CCProcessingBase;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.Extensions.PaymentTransaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARRegisterAlias = PX.Objects.AR.Standalone.ARRegisterAlias;
using ISessionStateItemCollection = System.Web.SessionState.ISessionStateItemCollection;
using SessionStateItemCollection = System.Web.SessionState.SessionStateItemCollection;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{

	[PXHidden]
	public class InputCCTransactionSO : InputCCTransaction
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXInt(IsKey = true)]
		[PXDBDefault(typeof(SOAdjust.recordID))]
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
	}

	public class CreatePaymentAPIExt : PXGraphExtension<CreatePaymentExt, SOOrderEntry>
	{
		public PXFilter<InputCCTransactionSO> apiInputCCTran;

		public virtual ARPaymentEntry CreatePaymentAPI(SOAdjust soAdjust, SOOrder order, string paymentType)
		{
			ARPaymentEntry paymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();

			List<InputCCTransactionSO> ccTransactions = new List<InputCCTransactionSO>();
			foreach (InputCCTransactionSO tran in apiInputCCTran.Cache.Inserted)
			{
				if (tran.RecordID == soAdjust.RecordID)
					ccTransactions.Add(tran);
			}

			if (ccTransactions.Count() != 0)
				paymentEntry.IsContractBasedAPI = true;
			else if (soAdjust.Capture == true || soAdjust.Authorize == true)
				paymentEntry.arsetup.Current.HoldEntry = false;

			var payment = paymentEntry.Document.Insert(new ARPayment() { DocType = paymentType });
			FillARPaymentAPI(paymentEntry, payment, soAdjust, order);
			payment = paymentEntry.Document.Update(payment);
			PXNoteAttribute.CopyNoteAndFiles(Base.Caches[typeof(SOAdjust)], soAdjust, paymentEntry.Caches[typeof(ARPayment)], payment);

			ARPaymentEntry.PaymentTransaction paymentExt = paymentEntry.GetExtension<ARPaymentEntry.PaymentTransaction>();
			foreach (InputCCTransaction tran in ccTransactions)
			{
				paymentExt.apiInputCCTran.Insert(tran);
			}

			SOAdjust adj = new SOAdjust()
			{
				AdjdOrderType = order.OrderType,
				AdjdOrderNbr = order.OrderNbr
			};

			if (soAdjust.CuryAdjdAmt != null)
				adj.CuryAdjgAmt = soAdjust.CuryAdjdAmt;

			var currentOrder = paymentEntry.SOOrder_CustomerID_OrderType_RefNbr.Select(soAdjust.CustomerID, soAdjust.AdjdOrderType, Base.Document.Current.OrderNbr);
			if (currentOrder.Count != 1)
				throw new PXException(Messages.SOOrderNotFound);

			adj = paymentEntry.SOAdjustments.Insert(adj);
			PXNoteAttribute.CopyNoteAndFiles(paymentEntry.Caches[typeof(ARPayment)], payment, paymentEntry.Caches[typeof(SOAdjust)], adj);

			return paymentEntry;
		}

		public virtual void FillARPaymentAPI(ARPaymentEntry paymentEntry, ARPayment arpayment, SOAdjust soAdjust, SOOrder order)
		{
			if (soAdjust.AdjgDocDate != null)
				arpayment.AdjDate = soAdjust.AdjgDocDate;
			arpayment.CustomerID = order.CustomerID;
			arpayment.CustomerLocationID = order.CustomerLocationID;
			if (soAdjust.PaymentMethodID != null)
				arpayment.PaymentMethodID = soAdjust.PaymentMethodID;
			if (soAdjust.PMInstanceID != null)
				paymentEntry.Document.Cache.SetValueExt<ARPayment.pMInstanceID>(arpayment, soAdjust.PMInstanceID);
			if (soAdjust.CashAccountID != null)
				paymentEntry.Document.Cache.SetValueExt<ARPayment.cashAccountID>(arpayment, soAdjust.CashAccountID);
			if (soAdjust.ProcessingCenterID != null)
				paymentEntry.Document.Cache.SetValueExt<ARPayment.processingCenterID>(arpayment, soAdjust.ProcessingCenterID);
			if (soAdjust.DocDesc != null)
				arpayment.DocDesc = soAdjust.DocDesc;
			if (soAdjust.ExtRefNbr != null)
				arpayment.ExtRefNbr = soAdjust.ExtRefNbr;
			if (soAdjust.SaveCard != null)
				arpayment.SaveCard = soAdjust.SaveCard;
			if (soAdjust.Hold != null && soAdjust.Capture != true && soAdjust.Authorize != true)
				arpayment.Hold = soAdjust.Hold;
			arpayment.CuryOrigDocAmt = soAdjust.CuryOrigDocAmt;
		}

		protected virtual void CreatePaymentAPI(SOAdjust soAdjust, string paymentType)
		{
			string ccPaymentConnectorUrl = System.Web.HttpContext.Current.GetPaymentConnectorUrl();

			PXContext.SetSlot<ISessionStateItemCollection>(new SessionStateItemCollection());
			PXContext.Session.SetString("CCPaymentConnectorUrl", ccPaymentConnectorUrl);

			ARPaymentEntry paymentEntry = CreatePaymentAPI(soAdjust, Base.Document.Current, paymentType);
			paymentEntry.Save.Press();
			Base.Adjustments.Cache.Remove(soAdjust);

			try
			{
				if (soAdjust.Capture == true)
				{
					Base1.PressButtonIfEnabled(paymentEntry, nameof(ARPaymentEntry.PaymentTransaction.captureCCPayment));
				}
				else if (soAdjust.Authorize == true)
				{
					Base1.PressButtonIfEnabled(paymentEntry, nameof(ARPaymentEntry.PaymentTransaction.authorizeCCPayment));
				}
			}
			catch (PXBaseRedirectException)
			{
				throw;
			}
			catch (Exception exception)
			{
				Base1.RedirectToNewGraph(paymentEntry, exception);
			}
			finally
			{
				PXContext.SetSlot<ISessionStateItemCollection>(null);
			}
		}

		protected virtual void _(Events.RowPersisting<SOAdjust> e)
		{
			SOAdjust row = e.Row as SOAdjust;
			if (row == null) return;

			if (e.Cache.Graph.IsContractBasedAPI && row.AdjgRefNbr == null)
			{
				e.Cancel = true;
			}
		}

		public delegate void PersistDelegate();
		[PXOverride]
		public virtual void Persist(PersistDelegate baseMethod)
		{
			if (Base.Adjustments.Cache.Graph.IsContractBasedAPI && Base.Adjustments.Cache.Inserted.Count() != 0)
			{
				Action createPayments = () => 
				{
					foreach (SOAdjust row in Base.Adjustments.Cache.Inserted)
					{
						if (row.AdjgRefNbr == null)
						{
							CreatePaymentAPI(row, row.AdjgDocType ?? ARDocType.Payment);
						}
					}
				};

				bool processAsync = ((IEnumerable<SOAdjust>)Base.Adjustments.Cache.Inserted).Any(a => a.Authorize == true || a.Capture == true);

				if (processAsync)
				{
					baseMethod();
					PXLongOperation.StartOperation(Base, () =>
					{
						createPayments();
					});
				}
				else
				{
					using (var ts = new PXTransactionScope())
					{
						baseMethod();
						createPayments();
						ts.Complete();
					}
				}
			}
			else
			{
				baseMethod();
			}
		}
	}
}
