using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	public class SyncCardCustomerSelectorAttribute : PXCustomSelectorAttribute
	{
		public SyncCardCustomerSelectorAttribute(params Type[] fields) : base(typeof(Customer.bAccountID), fields)
		{
			SubstituteKey = typeof(Customer.acctCD);
		}

		public IEnumerable GetRecords()
		{
			CCSynchronizeCards graph = _Graph as CCSynchronizeCards;

			if (graph == null)
			{ 
				return GetAllCustomers();
			}

			CCSynchronizeCard syncCards = graph.Caches[typeof(CCSynchronizeCard)].Current as CCSynchronizeCard;

			if (syncCards == null || syncCards.CCProcessingCenterID == null || syncCards.CustomerCCPID == null)
			{
				return GetAllCustomers();
			}
			else
			{
				string customerId = syncCards.CustomerCCPID;
				string processingCenterID = syncCards.CCProcessingCenterID;
				IEnumerable<Customer> customers = GetCustomersWithSameCCPID(processingCenterID,customerId);

				if (!customers.Any())
				{
					return GetAllCustomers();
				}
				return customers;
			}		
		}

		private IEnumerable<Customer> GetCustomersWithSameCCPID(string processingCenterID,string customerCCPID)
		{
			PXSelectBase<Customer> query = new PXSelectReadonly2<Customer,
				InnerJoin<CustomerProcessingCenterID,On<Customer.bAccountID,Equal<CustomerProcessingCenterID.bAccountID>>>,
				Where<CustomerProcessingCenterID.cCProcessingCenterID,Equal<Required<CustomerProcessingCenterID.cCProcessingCenterID>>,
					And<CustomerProcessingCenterID.customerCCPID,Equal<Required<CustomerProcessingCenterID.customerCCPID>>>>>(_Graph);
			PXResultset<Customer> customers = query.Select(processingCenterID,customerCCPID);
			return customers.RowCast<Customer>();
		}

		private IEnumerable GetAllCustomers()
		{
			PXSelectBase<Customer> query = new PXSelectReadonly<Customer>(_Graph);
			return query.Select();
		}
	}
}
