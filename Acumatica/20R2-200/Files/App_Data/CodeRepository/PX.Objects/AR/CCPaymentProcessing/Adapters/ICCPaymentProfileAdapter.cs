using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public interface ICCPaymentProfileAdapter
	{
		ICCPaymentProfile Current { get; }
		PXCache Cache { get; }
	}
}
