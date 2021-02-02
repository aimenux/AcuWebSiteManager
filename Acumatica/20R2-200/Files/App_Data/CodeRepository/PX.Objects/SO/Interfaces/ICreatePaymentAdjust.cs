using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.Interfaces
{
	public interface ICreatePaymentAdjust
	{
		string AdjgRefNbr { get; }
		string AdjgDocType { get; }
		decimal? CuryAdjdAmt { get; }
		bool? IsCCPayment { get; }
		bool? IsCCAuthorized { get; }
		bool? IsCCCaptured { get; }
		bool? Voided { get; }
		bool? Released { get; }
	}
}
