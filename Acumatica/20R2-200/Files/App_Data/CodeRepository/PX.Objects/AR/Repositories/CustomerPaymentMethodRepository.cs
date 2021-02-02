using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using System.Collections.Generic;

namespace PX.Objects.AR.Repositories
{
	public class CustomerPaymentMethodRepository
	{
		protected readonly PXGraph _graph;

		public CustomerPaymentMethodRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CustomerPaymentMethod GetCustomerPaymentMethod(int? pMInstanceId)
		{
			return PXSelect<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.pMInstanceID,
					Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(_graph, pMInstanceId);
		}

		public CustomerPaymentMethod UpdateCustomerPaymentMethod(CustomerPaymentMethod paymentMethod)
		{
			return (CustomerPaymentMethod)_graph.Caches[typeof(CustomerPaymentMethod)].Update(paymentMethod);
		}

		public PXResult<CustomerPaymentMethod, Customer> FindCustomerAndPaymentMethod(int? pMInstanceID)
		{
			return (PXResult<CustomerPaymentMethod, Customer>)PXSelectJoin<CustomerPaymentMethod,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(_graph, pMInstanceID);
		}

		public Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(string procCenter, string customerProfile, string paymentProfile)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod,
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
					And<CustomerPaymentMethod.customerCCPID, Equal<Required<CustomerPaymentMethod.customerCCPID>>,
					And<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				OrderBy<Desc<CustomerPaymentMethod.pMInstanceID>>>(_graph);

			PXResultset<CustomerPaymentMethod> result = query.Select(procCenter, customerProfile);
			foreach (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail> item in result)
			{
				CustomerPaymentMethodDetail det = (CustomerPaymentMethodDetail)item;
				if (det.Value == paymentProfile)
				{
					return new Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail>(item, det);
				}
			}
			return null;
		}

		public Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(int? pmInstanceId)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod,
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>,
					And<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>(_graph);
			PXResultset<CustomerPaymentMethod> result = query.Select(pmInstanceId);
			foreach (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail> item in result)
			{
				CustomerPaymentMethodDetail det = (CustomerPaymentMethodDetail)item;
				return new Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail>(item, det);
			}
			return null;
		}
	}
}
