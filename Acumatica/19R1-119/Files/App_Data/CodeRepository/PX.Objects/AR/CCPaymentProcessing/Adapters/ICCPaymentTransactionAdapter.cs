using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public interface ICCPaymentTransactionAdapter
	{
		ICCPaymentTransaction Current { get; }
		PXCache Cache { get; }
		IEnumerable<ICCPaymentTransaction> Select(params object[] arguments);
	}
}
