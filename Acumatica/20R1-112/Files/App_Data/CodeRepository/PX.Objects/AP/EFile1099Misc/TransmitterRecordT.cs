namespace PX.Objects.AP
{
	/// <summary>
	/// Transmitter Record (T) 
	/// File format is based on IRS publication 1220 (http://www.irs.gov/pub/irs-pdf/p1220.pdf)
	/// </summary>
	public class TransmitterTRecord
	{
		[FixedLength(StartPosition = 1, FieldLength = 1)]
		public string RecordType { get; set; }

		[FixedLength(StartPosition = 2, FieldLength = 4)]
		public string PaymentYear { get; set; }

		[FixedLength(StartPosition = 6, FieldLength = 1)]
		public string PriorYearDataIndicator { get; set; }

		[FixedLength(StartPosition = 7, FieldLength = 9, RegexReplacePattern = @"[^0-9]")]
		public string TransmitterTIN { get; set; }

		[FixedLength(StartPosition = 16, FieldLength = 5)]
		public string TransmitterControlCode { get; set; }

		[FixedLength(StartPosition = 21, FieldLength = 7)]
		public string Blank1 { get; set; }

		[FixedLength(StartPosition = 28, FieldLength = 1)]
		public string TestFileIndicator { get; set; }

		[FixedLength(StartPosition = 29, FieldLength = 1)]
		public string ForeignEntityIndicator { get; set; }

		[FixedLength(StartPosition = 30, FieldLength = 80, PaddingStyle = PaddingEnum.Right)]
		public string TransmitterName { get; set; }

		[FixedLength(StartPosition = 110, FieldLength = 80, PaddingStyle = PaddingEnum.Right)]
		public string CompanyName { get; set; }

		[FixedLength(StartPosition = 190, FieldLength = 40, PaddingStyle = PaddingEnum.Right)]
		public string CompanyMailingAddress { get; set; }

		[FixedLength(StartPosition = 230, FieldLength = 40, PaddingStyle = PaddingEnum.Right)]
		public string CompanyCity { get; set; }

		[FixedLength(StartPosition = 270, FieldLength = 2, PaddingStyle = PaddingEnum.Right)]
		public string CompanyState { get; set; }

		[FixedLength(StartPosition = 272, FieldLength = 9, PaddingStyle = PaddingEnum.Right, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string CompanyZipCode { get; set; }

		[FixedLength(StartPosition = 281, FieldLength = 15, PaddingStyle = PaddingEnum.Right)]
		public string Blank2 { get; set; }

		[FixedLength(StartPosition = 296, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public string TotalNumberofPayees { get; set; }

		[FixedLength(StartPosition = 304, FieldLength = 40)]
		public string ContactName { get; set; }

		[FixedLength(StartPosition = 344, FieldLength = 15, PaddingStyle = PaddingEnum.Right, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string ContactTelephoneAndExt { get; set; }

		[FixedLength(StartPosition = 359, FieldLength = 50, PaddingStyle = PaddingEnum.Right, AlphaCharacterCaseStyle = AlphaCharacterCaseEnum.None)]
		public string ContactEmailAddress { get; set; }

		[FixedLength(StartPosition = 409, FieldLength = 91, PaddingStyle = PaddingEnum.Right)]
		public string Blank3 { get; set; }

		[FixedLength(StartPosition = 500, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public string RecordSequenceNumber { get; set; }

		[FixedLength(StartPosition = 508, FieldLength = 10, PaddingStyle = PaddingEnum.Right)]
		public string Blank4 { get; set; }

		[FixedLength(StartPosition = 518, FieldLength = 1)]
		public string VendorIndicator { get; set; }

		[FixedLength(StartPosition = 519, FieldLength = 40)]
		public string VendorName { get; set; }

		[FixedLength(StartPosition = 559, FieldLength = 40)]
		public string VendorMailingAddress { get; set; }

		[FixedLength(StartPosition = 599, FieldLength = 40)]
		public string VendorCity { get; set; }

		[FixedLength(StartPosition = 639, FieldLength = 2)]
		public string VendorState { get; set; }

		[FixedLength(StartPosition = 641, FieldLength = 9, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string VendorZipCode { get; set; }

		[FixedLength(StartPosition = 650, FieldLength = 40)]
		public string VendorContactName { get; set; }

		[FixedLength(StartPosition = 690, FieldLength = 15, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string VendorContactTelephoneAndExt { get; set; }

		[FixedLength(StartPosition = 705, FieldLength = 35)]
		public string Blank5 { get; set; }

		[FixedLength(StartPosition = 740, FieldLength = 1)]
		public string VendorForeignEntityIndicator { get; set; }

		[FixedLength(StartPosition = 741, FieldLength = 8)]
		public string Blank6 { get; set; }

		[FixedLength(StartPosition = 749, FieldLength = 2)]
		public string Blank7 { get; set; }

	}
}
