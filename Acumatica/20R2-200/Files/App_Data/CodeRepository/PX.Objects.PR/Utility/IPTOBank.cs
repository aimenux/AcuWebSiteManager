using System;

namespace PX.Objects.PR
{
	public interface IPTOBank
	{
		string BankID { get; set; }

		decimal? AccrualRate { get; set; }

		decimal? AccrualLimit { get; set; }

		bool? IsActive { get; set; }

		DateTime? StartDate { get; set; }

		string CarryoverType { get; set; }

		decimal? CarryoverAmount { get; set; }

		decimal? FrontLoadingAmount { get; set; }

		bool? AllowNegativeBalance { get; set; }

		int? CarryoverPayMonthLimit { get; set; }

		bool? DisburseFromCarryover { get; set; }
	}
}
