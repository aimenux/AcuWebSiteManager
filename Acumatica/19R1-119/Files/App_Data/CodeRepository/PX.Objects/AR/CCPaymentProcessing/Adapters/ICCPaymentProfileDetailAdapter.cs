using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public interface ICCPaymentProfileDetailAdapter
	{
		ICCPaymentProfileDetail Current { get; }
		PXCache Cache { get; }
		IEnumerable<Tuple<ICCPaymentProfileDetail,ICCPaymentMethodDetail>> Select(params object[] arguments);
	}
}
