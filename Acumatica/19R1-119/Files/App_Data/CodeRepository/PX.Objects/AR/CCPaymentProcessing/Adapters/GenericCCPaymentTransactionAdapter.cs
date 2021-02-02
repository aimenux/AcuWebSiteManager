using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing
{
	class GenericCCPaymentTransactionAdapter<T> : ICCPaymentTransactionAdapter
		where T:class, IBqlTable, ICCPaymentTransaction, new()
	{
		PXSelectBase<T> paymentTransaction;
		public ICCPaymentTransaction Current => throw new NotImplementedException();

		public PXCache Cache => throw new NotImplementedException();

		public GenericCCPaymentTransactionAdapter(PXSelectBase<T> paymentTransaction)
		{
			this.paymentTransaction = paymentTransaction;
		}

		public IEnumerable<ICCPaymentTransaction> Select(params object[] arguments)
		{
			foreach (T item in paymentTransaction.Select(arguments))
			{
				yield return item;
			}
		}
	}
}
