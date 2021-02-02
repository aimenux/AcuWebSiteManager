using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing
{
	class GenericExternalTransactionAdapter<T> : IExternalTransactionAdapter
		where T : class, IBqlTable, IExternalTransaction, new()
	{
		PXSelectBase<T> externalTransaction;

		public PXCache Cache => externalTransaction.Cache;

		public GenericExternalTransactionAdapter(PXSelectBase<T> externalTransaction)
		{
			this.externalTransaction = externalTransaction;
		}

		public IEnumerable<IExternalTransaction> Select(params object[] arguments)
		{
			foreach (T item in externalTransaction.Select(arguments))
			{
				yield return item;
			}
		}
	}
}
