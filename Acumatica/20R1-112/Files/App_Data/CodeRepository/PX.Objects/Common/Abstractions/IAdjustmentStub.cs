using System;

namespace PX.Objects.Common
{
	public interface IAdjustmentStub
	{
		string StubNbr { get; set; }
		int? CashAccountID { get; set; }
		string PaymentMethodID { get; set; }
		bool Persistent { get; }
		decimal? CuryAdjgAmt { get; }
		decimal? CuryAdjgDiscAmt { get; }
		string AdjdDocType { get; }
		decimal? CuryOutstandingBalance { get; }
		DateTime? OutstandingBalanceDate { get; }
		bool? IsRequest { get; }
	}
}
