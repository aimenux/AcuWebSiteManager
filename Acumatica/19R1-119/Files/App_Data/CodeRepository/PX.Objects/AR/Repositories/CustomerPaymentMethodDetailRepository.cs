using PX.Data;
using System;

namespace PX.Objects.AR.Repositories
{
	public class CustomerPaymentMethodDetailRepository
	{
		protected readonly PXGraph _graph;
		public CustomerPaymentMethodDetailRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CustomerPaymentMethodDetail GetCustomerPaymentMethodDetail(int? pMInstanceId, string detailID)
		{
			return PXSelect<CustomerPaymentMethodDetail,
						Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Required<CustomerPaymentMethodDetail.pMInstanceID>>,
							And<CustomerPaymentMethodDetail.detailID, 
								Equal<Required<CustomerPaymentMethodDetail.detailID>>>>>.Select(_graph, pMInstanceId, detailID);
		}

		public void DeletePaymentMethodDetail(CustomerPaymentMethodDetail detail)
		{
			_graph.Caches[typeof(CustomerPaymentMethodDetail)].Delete(detail);
		}
	}
}
