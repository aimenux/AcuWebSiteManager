namespace PX.Objects.AP
{
	/// <summary>
	/// Payee Record (B)
	/// File format is based on IRS publication 1220 (http://www.irs.gov/pub/irs-pdf/p1220.pdf)
	/// </summary>
	public class PayeeRecordB
	{
		[FixedLength(StartPosition = 1, FieldLength = 1)]
		public string RecordType { get; set; }

		[FixedLength(StartPosition = 2, FieldLength = 4)]
		public string PaymentYear { get; set; }

		[FixedLength(StartPosition = 6, FieldLength = 1)]
		public string CorrectedReturnIndicator { get; set; }

		[FixedLength(StartPosition = 7, FieldLength = 4, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string NameControl { get; set; }

		[FixedLength(StartPosition = 11, FieldLength = 1)]
		public string TypeOfTIN { get; set; }

		[FixedLength(StartPosition = 12, FieldLength = 9, RegexReplacePattern = @"[^0-9]")]
		public string PayerTaxpayerIdentificationNumberTIN { get; set; }

		[FixedLength(StartPosition = 21, FieldLength = 20)]
		public string PayerAccountNumberForPayee { get; set; }

		[FixedLength(StartPosition = 41, FieldLength = 4)]
		public string PayerOfficeCode { get; set; }

		[FixedLength(StartPosition = 45, FieldLength = 10)]
		public string Blank1 { get; set; }

		[FixedLength(StartPosition = 55, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount1 { get; set; }

		[FixedLength(StartPosition = 67, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount2 { get; set; }

		[FixedLength(StartPosition = 79, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount3 { get; set; }

		[FixedLength(StartPosition = 91, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount4 { get; set; }

		[FixedLength(StartPosition = 103, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount5 { get; set; }

		[FixedLength(StartPosition = 115, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount6 { get; set; }

		[FixedLength(StartPosition = 127, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount7 { get; set; }

		[FixedLength(StartPosition = 139, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount8 { get; set; }

		[FixedLength(StartPosition = 151, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmount9 { get; set; }

		[FixedLength(StartPosition = 163, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmountA { get; set; }

		[FixedLength(StartPosition = 175, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmountB { get; set; }

		[FixedLength(StartPosition = 187, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmountC { get; set; }

		[FixedLength(StartPosition = 199, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal Payment { get; set; }

		[FixedLength(StartPosition = 211, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmountE { get; set; }

		[FixedLength(StartPosition = 223, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmountF { get; set; }

		[FixedLength(StartPosition = 235, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public decimal PaymentAmountG { get; set; }

		[FixedLength(StartPosition = 247, FieldLength = 1)]
		public string ForeignCountryIndicator { get; set; }

		[FixedLength(StartPosition = 248, FieldLength = 80, RegexReplacePattern = @"[^0-9a-zA-Z-& ]")]
		public string PayeeNameLine { get; set; }

		[FixedLength(StartPosition = 328, FieldLength = 40)]
		public string Blank2 { get; set; }

		[FixedLength(StartPosition = 368, FieldLength = 40)]
		public string PayeeMailingAddress { get; set; }

		[FixedLength(StartPosition = 408, FieldLength = 40)]
		public string Blank3 { get; set; }

		[FixedLength(StartPosition = 448, FieldLength = 40)]
		public string PayeeCity { get; set; }

		[FixedLength(StartPosition = 488, FieldLength = 2)]
		public string PayeeState { get; set; }

		[FixedLength(StartPosition = 490, FieldLength = 9, RegexReplacePattern = @"[^0-9a-zA-Z]")]
		public string PayeeZipCode { get; set; }

		[FixedLength(StartPosition = 499, FieldLength = 1)]
		public string Blank4 { get; set; }

		[FixedLength(StartPosition = 500, FieldLength = 8, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public string RecordSequenceNumber { get; set; }

		[FixedLength(StartPosition = 508, FieldLength = 36)]
		public string Blank5 { get; set; }

		[FixedLength(StartPosition = 544, FieldLength = 1)]
		public string SecondTINNotice { get; set; }

		[FixedLength(StartPosition = 545, FieldLength = 2)]
		public string Blank6 { get; set; }

		[FixedLength(StartPosition = 547, FieldLength = 1)]
		public string DirectSalesIndicator { get; set; }

		[FixedLength(StartPosition = 548, FieldLength = 1)]
		public string FATCA { get; set; }

		[FixedLength(StartPosition = 549, FieldLength = 114)]
		public string Blank7 { get; set; }

		[FixedLength(StartPosition = 663, FieldLength = 60)]
		public string SpecialDataEntries { get; set; }

		[FixedLength(StartPosition = 723, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public string StateIncomeTaxWithheld { get; set; }

		[FixedLength(StartPosition = 735, FieldLength = 12, PaddingStyle = PaddingEnum.Left, PaddingChar = '0')]
		public string LocalIncomeTaxWithheld { get; set; }

		[FixedLength(StartPosition = 747, FieldLength = 2)]
		public string CombineFederalOrStateCode { get; set; }

		[FixedLength(StartPosition = 749, FieldLength = 2)]
		public string Blank8 { get; set; }
	}
}
