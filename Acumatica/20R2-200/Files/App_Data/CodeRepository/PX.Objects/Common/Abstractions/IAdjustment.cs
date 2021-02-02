using System;

namespace PX.Objects
{
	public interface IAdjustment
	{
		long? AdjdCuryInfoID
		{
			get;
			set;
		}
		long? AdjdOrigCuryInfoID
		{
			get;
			set;
		}
		long? AdjgCuryInfoID
		{
			get;
			set;
		}
		DateTime? AdjgDocDate
		{
			get;
			set;
		}
		DateTime? AdjdDocDate
		{
			get;
			set;
		}
		decimal? CuryAdjgAmt
		{
			get;
			set;
		}
		decimal? CuryAdjgDiscAmt
		{
			get;
			set;
		}
		decimal? CuryAdjdAmt
		{
			get;
			set;
		}
		decimal? CuryAdjdDiscAmt
		{
			get;
			set;
		}
		decimal? AdjAmt
		{
			get;
			set;
		}
		decimal? AdjDiscAmt
		{
			get;
			set;
		}
		decimal? RGOLAmt
		{
			get;
			set;
		}
		bool? Released
		{
			get;
			set;
		}
		bool? Voided
		{
			get;
			set;
		}
		bool? ReverseGainLoss
		{
			get;
			set;
		}
		decimal? CuryDocBal
		{
			get;
			set;
		}
		decimal? DocBal
		{
			get;
			set;
		}
		decimal? CuryDiscBal
		{
			get;
			set;
		}
		decimal? DiscBal
		{
			get;
			set;
		}
		/// <summary>
		/// Represents third adjusting amount that is used in <see cref="PaymentEntry.CalcBalances<TInvoice, TAdjustment>"/> method.
		/// In case of APAdjust it is <see cref="APAdjust.CuryAdjgWhTaxAmt"/>. It's amount of withholding tax calculated for the adjusting document, if applicable.
		/// In case of ARAdjust it is <see cref="ARAdjust.CuryAdjgWOAmt"/> 
		/// </summary>
		decimal? CuryAdjgWhTaxAmt
		{
			get;
			set;
		}
		/// <summary>
		/// Represents third adjusted amount that is used in <see cref="PaymentEntry.CalcBalances<TInvoice, TAdjustment>"/> method.
		/// In case of APAdjust it is <see cref="APAdjust.CuryAdjdWhTaxAmt"/>. It's amount of tax withheld from the payments to the adjusted document.
		/// In case of ARAdjust it is <see cref="ARAdjust.CuryAdjdWOAmt"/>
		/// </summary>
		decimal? CuryAdjdWhTaxAmt
		{
			get;
			set;
		}
		/// <summary>
		/// Represents third amount that is used in <see cref="PaymentEntry.CalcBalances<TInvoice, TAdjustment>"/> method.
		/// In case of APAdjust it is <see cref="APAdjust.AdjWhTaxAmt"/>. It's amount of tax withheld from the payments to the adjusted document.
		/// In case of ARAdjust it is <see cref="ARAdjust.AdjWOAmt"/>
		/// </summary>
		decimal? AdjWhTaxAmt
		{
			get;
			set;
		}
		/// <summary>
		/// Represents third currency balance that is calculated in <see cref="PaymentEntry.CalcBalances<TInvoice, TAdjustment>"/> method.
		/// In case of APAdjust it is <see cref="APAdjust.CuryWhTaxBal"/>. It's difference between the amount of the tax to be withheld and the actual withheld amount (if any withholding taxes are applicable).
		/// In case of ARAdjust it is <see cref="ARAdjust.CuryWOBal"/>
		/// </summary>
		decimal? CuryWhTaxBal
		{
			get;
			set;
		}
		/// <summary>
		/// Represents third balance that is calculated in <see cref="PaymentEntry.CalcBalances<TInvoice, TAdjustment>"/> method.
		/// In case of APAdjust it is <see cref="APAdjust.WhTaxBal"/>. It's difference between the amount of the tax to be withheld and the actual withheld amount (if any withholding taxes are applicable).
		/// In case of ARAdjust it is <see cref="ARAdjust.WOBal"/>.
		/// </summary>
		decimal? WhTaxBal
		{
			get;
			set;
		}
	}
}
