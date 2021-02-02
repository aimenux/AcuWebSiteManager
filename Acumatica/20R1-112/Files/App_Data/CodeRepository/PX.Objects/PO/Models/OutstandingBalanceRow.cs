using PX.Objects.Common;
using System;


namespace PX.Objects.PO
{
	class OutstandingBalanceRow : IAdjustmentStub
	{
		public string StubNbr { get; set; }
		public int? CashAccountID { get; set; }
		public string PaymentMethodID { get; set; }
		public bool Persistent => false;
		public decimal? CuryOutstandingBalance { get; set; }
		public DateTime? OutstandingBalanceDate { get; set; }
		public decimal? CuryAdjgAmt => 0;
		public decimal? CuryAdjgDiscAmt => 0m;
		public string AdjdDocType => null;
		public bool? IsRequest => null;
	}
}
