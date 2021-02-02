using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR.CCPaymentProcessing.Interfaces
{
	public interface ICCManualInputPaymentInfo
	{
		string PCTranNumber
		{
			get; set;
		}

		string AuthNumber
		{
			get; set;
		}
	}
}
