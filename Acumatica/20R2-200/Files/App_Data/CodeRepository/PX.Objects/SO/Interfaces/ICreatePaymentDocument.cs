using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.Interfaces
{
	public interface ICreatePaymentDocument
	{
		int? CustomerID { get; }
		int? CustomerLocationID { get; }
		string PaymentMethodID { get; }
		int? PMInstanceID { get; }
		int? CashAccountID { get; }
		string CuryID { get; }
		decimal? CuryUnpaidBalance { get; }
		decimal? UnpaidBalance { get; }
	}
}
