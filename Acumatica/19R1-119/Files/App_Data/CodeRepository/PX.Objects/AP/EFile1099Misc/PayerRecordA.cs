namespace PX.Objects.AP
{
	/// <summary>
	/// Payer Record (A)
	/// File format is based on IRS publication 1220 (http://www.irs.gov/pub/irs-pdf/p1220.pdf)
	/// </summary>
	public class PayerRecordA
	{
		[FixedLength(StartPosition = 1, FieldLength = 1)]
		public string RecordType { get; set; }

		[FixedLength(StartPosition = 2, FieldLength = 4)]
		public string PaymentYear { get; set; }

		[FixedLength(StartPosition = 6, FieldLength = 1)]
		public string CombinedFederalORStateFiler { get; set; }

		[FixedLength(StartPosition = 7, FieldLength = 5)]
		public string Blank1 { get; set; }

		[FixedLength(StartPosition = 12, FieldLength = 9, RegexReplacePattern = @"[^0-9]")]
		public string PayerTaxpayerIdentificationNumberTIN { get; set; }

		[FixedLength(StartPosition = 21, FieldLength = 4, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string PayerNameControl { get; set; }

		[FixedLength(StartPosition = 25, FieldLength = 1)]
		public string LastFilingIndicator { get; set; }

		[FixedLength(StartPosition = 26, FieldLength = 2)]
		public string TypeofReturn { get; set; }

		[FixedLength(StartPosition = 28, FieldLength = 16)]
		public string AmountCodes { get; set; }

		[FixedLength(StartPosition = 44, FieldLength = 8)]
		public string Blank2 { get; set; }

		[FixedLength(StartPosition = 52, FieldLength = 1)]
		public string ForeignEntityIndicator { get; set; }

		[FixedLength(StartPosition = 53, FieldLength = 40)]
		public string FirstPayerNameLine { get; set; }

		[FixedLength(StartPosition = 93, FieldLength = 40)]
		public string SecondPayerNameLine { get; set; }

		[FixedLength(StartPosition = 133, FieldLength = 1)]
		public string TransferAgentIndicator { get; set; }

		[FixedLength(StartPosition = 134, FieldLength = 40)]
		public string PayerShippingAddress { get; set; }

		[FixedLength(StartPosition = 174, FieldLength = 40)]
		public string PayerCity { get; set; }

		[FixedLength(StartPosition = 214, FieldLength = 2)]
		public string PayerState { get; set; }

		[FixedLength(StartPosition = 216, FieldLength = 9, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string PayerZipCode { get; set; }

		[FixedLength(StartPosition = 225, FieldLength = 15, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string PayerTelephoneAndExt { get; set; }

		[FixedLength(StartPosition = 240, FieldLength = 260)]
		public string Blank3 { get; set; }

		[FixedLength(StartPosition = 500, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public string RecordSequenceNumber { get; set; }

		[FixedLength(StartPosition = 508, FieldLength = 241)]
		public string Blank4 { get; set; }

		[FixedLength(StartPosition = 749, FieldLength = 2)]
		public string Blank5 { get; set; }
	}
}
