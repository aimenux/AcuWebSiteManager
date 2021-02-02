using System;

namespace PX.Objects.Common.Interfaces
{
	/// <summary>
	/// The interface for the approvable documents that have descriptions of details.
	/// The short description is a string that the approver can see in a column of the grid with documents.
	/// </summary>
	public interface IApprovalDescription
	{
		int? CashAccountID
		{
			get;
		}

		String PaymentMethodID
		{
			get;
		}

		decimal? CuryChargeAmt
		{
			get;
		}

		Int64? CuryInfoID
		{
			get;
		}
	}
}
