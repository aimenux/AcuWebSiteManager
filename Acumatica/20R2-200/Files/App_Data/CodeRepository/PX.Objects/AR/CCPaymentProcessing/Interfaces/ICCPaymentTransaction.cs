using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR.CCPaymentProcessing.Interfaces
{
	public interface ICCPaymentTransaction
	{
		int? TranNbr { get; set; }
		int? PMInstanceID { get; set; }
		string ProcessingCenterID { get; set; }
		string DocType { get; set; }
		string RefNbr { get; set; }
		int? RefTranNbr { get; set; }
		string OrigDocType { get; set; }
		string OrigRefNbr { get; set; }
		string PCTranNumber { get; set; }
		string AuthNumber { get; set; }
		string TranType { get; set; }
		string ProcStatus { get; set; }
		string TranStatus { get; set; }
		DateTime? ExpirationDate { get; set; }
		decimal? Amount { get; set; }
	}
}
