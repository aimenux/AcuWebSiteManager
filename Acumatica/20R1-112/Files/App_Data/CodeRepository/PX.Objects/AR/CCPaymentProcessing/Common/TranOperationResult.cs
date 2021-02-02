using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public class TranOperationResult
	{
		public bool Success { get; set; }
		public int? TransactionId { get; set; }
	}
}
