using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.AR;
using PX.Data;

namespace PX.Objects.AR
{
	public class RefTranExtNbrAttribute: PXCustomSelectorAttribute
	{
		public const string PaymentMethod = "PaymentMethodID";
		public const string CustomerIdField = "CustomerID";

		public RefTranExtNbrAttribute() : base(typeof(Search3<ExternalTransaction.tranNumber, 
			LeftJoin<CustomerPaymentMethod, On<ExternalTransaction.pMInstanceID,Equal<CustomerPaymentMethod.pMInstanceID>>>, 
			OrderBy<Desc<ExternalTransaction.tranNumber>>>), typeof(ExternalTransaction.docType),
			typeof(ExternalTransaction.refNbr), typeof(ExternalTransaction.amount), typeof(ExternalTransaction.tranNumber), typeof(CustomerPaymentMethod.descr))
		{
			ValidateValue = true;
		}

		public static ExternalTransaction GetStoredTran(string tranNbr, PXGraph graph, PXCache cache)
		{
			if (string.IsNullOrEmpty(tranNbr) || graph == null || cache == null)
			{
				return null;
			}

			int? customerID = cache.GetValue(cache.Current, CustomerIdField) as int?;
			string paymentMethod = cache.GetValue(cache.Current, PaymentMethod) as string;
			var query = new PXSelectJoin<ExternalTransaction,
				InnerJoin<ARPayment, On<ExternalTransaction.docType, Equal<ARPayment.docType>, And<ExternalTransaction.refNbr, Equal<ARPayment.refNbr>>>>,
				Where<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>,
					And<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>,
					And<ARPayment.customerID, Equal<Required<ARPayment.customerID>>,
					And<ARPayment.paymentMethodID, Equal<Required<ARPayment.paymentMethodID>>>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
			ExternalTransaction extTran = query.SelectSingle(tranNbr, customerID, paymentMethod);
			return extTran;
		}

		protected virtual IEnumerable GetRecords()
		{
			var graph = this._Graph;
			PXCache cache = null;

			if (graph != null)
			{
				cache = _Graph.GetPrimaryCache();
			}

			if (cache == null)
			{
				yield break;
			}

			int? customerID = cache.GetValue(cache.Current, CustomerIdField) as int?;
			string paymentMethod = cache.GetValue(cache.Current, PaymentMethod) as string;
			var result = new PXSelectJoin<ExternalTransaction,
				InnerJoin<ARPayment, On<ExternalTransaction.docType, Equal<ARPayment.docType>, And<ExternalTransaction.refNbr, Equal<ARPayment.refNbr>>>,
				LeftJoin<CustomerPaymentMethod, On<ExternalTransaction.pMInstanceID, Equal<CustomerPaymentMethod.pMInstanceID>>>>,
				Where<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>,
					And<ARPayment.customerID, Equal<Required<ARPayment.customerID>>,
					And<ARPayment.paymentMethodID, Equal<Required<ARPayment.paymentMethodID>>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);

			foreach (PXResult<ExternalTransaction, ARPayment, CustomerPaymentMethod> item in result.SelectWithViewContext(customerID, paymentMethod))
			{
				yield return item;
			}
		}
	}
}
