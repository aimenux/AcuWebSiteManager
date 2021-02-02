using System;
﻿using PX.Data;

using PX.Objects.Common;
using PX.Objects.CR;

namespace PX.Objects.AR.Repositories
{
	public class CustomerRepository : RepositoryBase<Customer>
	{
		public CustomerRepository(PXGraph graph)
			: base(graph)
		{ }

		public Customer FindByID(int? accountID)
		{
			foreach (PXResult<BAccount, Customer> result in PXSelectJoin<
				BAccount,
					InnerJoinSingleTable<Customer,
						On<BAccount.bAccountID, Equal<Customer.bAccountID>>>,
				Where2<
					Where<
						BAccount.type, Equal<BAccountType.customerType>,
						Or<BAccount.type, Equal<BAccountType.combinedType>>>,
					And<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>>
				.Select(_graph, accountID))
			{
				BAccount businessAccount = result;
				Customer customer = result;

				PXCache<BAccount>.RestoreCopy(customer, businessAccount);

				return customer;
			}

			return null;
		}

		public Customer FindByCD(string accountCD)
		{
			foreach (PXResult<BAccount, Customer> result in PXSelectJoin<
				BAccount,
					InnerJoinSingleTable<Customer,
						On<BAccount.bAccountID, Equal<Customer.bAccountID>>>,
				Where2<
					Where<
						BAccount.type, Equal<BAccountType.customerType>,
						Or<BAccount.type, Equal<BAccountType.combinedType>>>,
					And<BAccount.acctCD, Equal<Required<Customer.acctCD>>>>>
				.Select(_graph, accountCD))
			{
				BAccount businessAccount = result;
				Customer customer = result;

				PXCache<BAccount>.RestoreCopy(customer, businessAccount);

				return customer;
			}

			return null;
		}

		public Customer GetByCD(string accountCD) => ForceNotNull(FindByCD(accountCD), accountCD);
	}
}
