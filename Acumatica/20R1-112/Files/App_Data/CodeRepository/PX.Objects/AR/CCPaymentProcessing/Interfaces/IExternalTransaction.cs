using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR.CCPaymentProcessing.Interfaces
{
	public interface IExternalTransaction
	{
		int? TransactionID { get; set; }
		string DocType { get; set; }
		string RefNbr { get; set; }
		string OrigDocType { get; set; }
		string OrigRefNbr { get; set; }
		string TranNumber { get; set; }
		string AuthNumber { get; set; }
		decimal? Amount { get; set; }
		string ProcessingStatus { get; set; }
		DateTime? LastActivityDate { get; set; }
		string Direction { get; set; }
		bool? Active { get; set; }
		bool? Completed { get; set; }
		int? ParentTranID { get; set; }
		DateTime? ExpirationDate { get; set; }
		string CVVVerification { get; set; }
	}
}
