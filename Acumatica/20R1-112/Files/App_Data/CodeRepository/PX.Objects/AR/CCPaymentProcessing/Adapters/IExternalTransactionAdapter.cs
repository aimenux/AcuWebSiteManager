using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public interface IExternalTransactionAdapter
	{
		PXCache Cache { get; }
		IEnumerable<IExternalTransaction> Select(params object[] arguments);
	}
}
