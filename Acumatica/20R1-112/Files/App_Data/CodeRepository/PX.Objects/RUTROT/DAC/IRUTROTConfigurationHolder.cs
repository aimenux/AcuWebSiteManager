using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.RUTROT.DAC
{
	public interface IRUTROTConfigurationHolder
	{
		bool? AllowsRUTROT { get; set; }
		decimal? RUTDeductionPct { get; set; }
		decimal? RUTPersonalAllowanceLimit { get; set; }
		decimal? RUTExtraAllowanceLimit { get; set; }
		decimal? ROTDeductionPct { get; set; }
		decimal? ROTPersonalAllowanceLimit { get; set; }
		decimal? ROTExtraAllowanceLimit { get; set; }
		string RUTROTCuryID { get; set; }
		int? RUTROTClaimNextRefNbr { get; set; }
		string RUTROTOrgNbrValidRegEx { get; set; }
		string DefaultRUTROTType { get; set; }
		int? TaxAgencyAccountID { get; set; }
		string BalanceOnProcess { get; set; }
	}
}
