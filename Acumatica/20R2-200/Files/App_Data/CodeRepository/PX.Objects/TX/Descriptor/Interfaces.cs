using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.TX
{
	public interface ITaxableDetail
	{
		string TaxID { get; }
		decimal? GroupDiscountRate { get; }
		decimal? DocumentDiscountRate { get; }
		decimal? CuryTranAmt { get; }
		decimal? TranAmt { get; }
		decimal? CuryRetainageAmt { get; }
		decimal? RetainageAmt { get; }
	}

	public interface ITaxDetailWithAmounts
	{
		string TaxID { get; }
		decimal? CuryTaxableAmt { get; }
		decimal? TaxableAmt { get; }
		decimal? CuryTaxAmt { get; }
		decimal? TaxAmt { get; }
		decimal? CuryRetainedTaxableAmt { get; }
		decimal? RetainedTaxableAmt { get; }
	}
}
