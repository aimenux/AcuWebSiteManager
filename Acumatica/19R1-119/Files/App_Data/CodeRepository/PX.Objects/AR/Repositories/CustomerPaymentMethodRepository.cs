using PX.Data;
using System;

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
	}
}
