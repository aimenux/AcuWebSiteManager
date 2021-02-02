using System;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public interface ICCPayment
	{
		int? PMInstanceID
		{
			get; set;
		}
		decimal? CuryDocBal
		{
			get; set;
		}
		string CuryID
		{
			get; set;
		}
		string DocType
		{
			get; set;
		}
		string RefNbr
		{
			get; set;
		}
		string OrigDocType
		{
			get;
		}
		string OrigRefNbr
		{
			get;
		}

		string RefTranExtNbr
		{
			get;
		}

		bool? Released
		{
			get;
		}
	}

	public interface ICCCapturePayment : ICCPayment
	{
		bool? IsCCCaptured
		{
			get; set;
		}
		bool? IsCCCaptureFailed
		{
			get; set;
		}
		decimal? CuryCCCapturedAmt
		{
			get; set;
		}
	}

	public interface ICCAuthorizePayment : ICCPayment
	{
		bool? IsCCAuthorized
		{
			get; set;
		}
		decimal? CuryCCPreAuthAmount
		{
			get; set;
		}
		DateTime? CCAuthExpirationDate
		{
			get; set;
		}
	}
}
