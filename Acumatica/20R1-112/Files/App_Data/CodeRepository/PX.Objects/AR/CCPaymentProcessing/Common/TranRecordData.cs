using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public class TranRecordData
	{
		public string ExternalTranId { get; set; }

		public string RefExternalTranId { get; set; }

		public string ProcessingCenterId { get; set; }

		public string AuthCode { get; set; }

		public string TranStatus { get; set; }

		public decimal? Amount { get; set; }

		public string ResponseText { get; set; }

		public DateTime? ExpirationDate { get; set; }

		public string CvvVerificationCode { get; set; }

		/// <summary>The <see cref="ExternalTransaction.TransactionID" /> identifier after recording operation.</summary>
		public int? InnerTranId { get; set; }

		public bool ValidateDoc { get; set; } = true;
	}
}
