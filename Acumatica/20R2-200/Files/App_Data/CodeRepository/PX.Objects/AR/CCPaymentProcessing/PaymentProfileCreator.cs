using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.CA;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using System.Text.RegularExpressions;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class PaymentProfileCreator
	{
		CCPaymentHelperGraph graph;
		string paymentMethod;
		string processingCenter;
		int? bAccountId;

		public PaymentProfileCreator(CCPaymentHelperGraph graph, string paymentMethod, string procCenter, int? bAccountId)
		{
			this.graph = graph;
			this.paymentMethod = paymentMethod;
			this.processingCenter = procCenter;
			this.bAccountId = bAccountId;

			graph.CustomerPaymentMethodDetails.View = new PXView(graph, false, new Select<CustomerPaymentMethodDetail>(), (PXSelectDelegate)GetCpmd);
			graph.PaymentMethodDet.View = new PXView(graph, false, new Select<PaymentMethodDetail>(), (PXSelectDelegate)GetPaymentMethodDet);
			graph.CustomerPaymentMethods.View = new PXView(graph, false, new Select<CustomerPaymentMethod>(), (PXSelectDelegate)GetCpm);
		}

		public CustomerPaymentMethod PrepeareCpmRecord()
		{
			CustomerPaymentMethod cpm = graph.CustomerPaymentMethods.Current as CustomerPaymentMethod;
			if (cpm == null)
			{
				cpm = new CustomerPaymentMethod();
				cpm.PaymentMethodID = paymentMethod;
				cpm.CCProcessingCenterID = processingCenter;
				cpm.BAccountID = bAccountId;
				cpm = graph.CustomerPaymentMethods.Insert(cpm);
			}
			return cpm;
		}

		public int? CreatePaymentProfile(TranProfile input)
		{
			var cpmSelect = graph.CustomerPaymentMethods;
			var cpmdSelect = graph.CustomerPaymentMethodDetails;
			var pmdSelect = graph.PaymentMethodDet;

			CustomerPaymentMethod cpm = cpmSelect.Cache.Current as CustomerPaymentMethod;
			cpm.CustomerCCPID = input.CustomerProfileId;

			foreach (PaymentMethodDetail item in GetPaymentMethodDetails())
			{
				CustomerPaymentMethodDetail cpmd = new CustomerPaymentMethodDetail();
				cpmd.PaymentMethodID = paymentMethod;
				cpmd.DetailID = item.DetailID;
				if (item.IsCCProcessingID == true)
				{
					cpmd.Value = input.PaymentProfileId;
				}
				cpmdSelect.Insert(cpmd);
			}

			CCCustomerInformationManagerGraph infoManagerGraph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
			GenericCCPaymentProfileAdapter<CustomerPaymentMethod> cpmAdapter = new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(cpmSelect);
			var cpmdAdapter = new GenericCCPaymentProfileDetailAdapter<CustomerPaymentMethodDetail, PaymentMethodDetail>(cpmdSelect, pmdSelect);
			infoManagerGraph.GetPaymentProfile(graph, cpmAdapter, cpmdAdapter);

			using (PXTransactionScope scope = new PXTransactionScope())
			{
				cpmSelect.Cache.Persist(PXDBOperation.Insert);
				cpmdSelect.Cache.Persist(PXDBOperation.Insert);
				scope.Complete();
			}
			return cpm.PMInstanceID;
		}

		public void CreateCustomerProcessingCenterRecord(TranProfile input)
		{
			PXCache customerProcessingCenterCache = graph.Caches[typeof(CustomerProcessingCenterID)];
			customerProcessingCenterCache.ClearQueryCacheObsolete();
			PXSelectBase<CustomerProcessingCenterID> checkRecordExist = new PXSelectReadonly<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CustomerProcessingCenterID.cCProcessingCenterID>>,
				And<CustomerProcessingCenterID.bAccountID, Equal<Required<CustomerProcessingCenterID.bAccountID>>,
				And<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerProcessingCenterID.customerCCPID>>>>>>(graph);

			CustomerProcessingCenterID cProcessingCenter = checkRecordExist.SelectSingle(processingCenter, bAccountId, input.CustomerProfileId);

			if (cProcessingCenter == null)
			{
				cProcessingCenter = customerProcessingCenterCache.CreateInstance() as CustomerProcessingCenterID;
				cProcessingCenter.BAccountID = bAccountId;
				cProcessingCenter.CCProcessingCenterID = processingCenter;
				cProcessingCenter.CustomerCCPID = input.CustomerProfileId;
				customerProcessingCenterCache.Insert(cProcessingCenter);
				customerProcessingCenterCache.Persist(PXDBOperation.Insert);
			}
		}

		public IEnumerable<PaymentMethodDetail> GetPaymentMethodDetails()
		{
			PXSelectBase<PaymentMethodDetail> query = new PXSelect<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>>>(graph);
			var result = query.Select(paymentMethod).RowCast<PaymentMethodDetail>();
			return result;
		}

		public void ClearCaches()
		{
			graph.CustomerPaymentMethods.Cache.Clear();
			graph.CustomerPaymentMethodDetails.Cache.Clear();
		}

		private IEnumerable GetCpmd()
		{
			PXCache cache = graph.Caches[typeof(CustomerPaymentMethodDetail)];
			foreach (object item in cache.Cached)
			{
				CustomerPaymentMethodDetail cpm = item as CustomerPaymentMethodDetail;
				if (cache.GetStatus(item) == PXEntryStatus.Inserted)
				{
					yield return item;
				}
			}
			yield break;
		}

		private IEnumerable GetPaymentMethodDet()
		{
			PXSelectBase<PaymentMethodDetail> query = new PXSelect<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>>>(graph);
			var result = query.Select(paymentMethod);
			return result;
		}

		private IEnumerable GetCpm()
		{
			PXCache cache = graph.Caches[typeof(CustomerPaymentMethod)];
			foreach (object item in cache.Cached)
			{
				CustomerPaymentMethod cpm = item as CustomerPaymentMethod;
				if (cache.GetStatus(item) == PXEntryStatus.Inserted && cpm.PMInstanceID < 0)
				{
					yield return item;
				}
			}
			yield break;
		}
	}
}
