namespace PX.Objects.CN.Common.Descriptor
{
	public static class Constants
	{
		public const string Screen = "Screen";
		public const string ScreenId = "ScreenID";
		public const string PdfFormat = "P";
		public const string XmlFileExtension = "*.xml";
		public const string PdfFileExtension = ".pdf";
		public const string ExcelFileExtension = ".xlsx";
		public const string ComplianceAttributeClassId = "COMPLIANCE";
		public const string Report = "Report";

		public static class ScreenIds
		{
			public const string Default = "00.00.00.00";
			public const string TaxBillsAndAdjustments = "TX.30.30.00";
			public const string TaxBillsAndAdjustmentsGenericInquiry = "TX.30.30.PL";
			public const string Invoices = "SO.30.30.00";
			public const string InvoicesGenericInquiry = "SO.30.30.PL";
			public const string ChecksAndPayments = "AP.30.20.00";
			public const string PreparePayments = "AP.50.30.00";
			public const string ConditionalPartialReportId = "CL642001";
			public const string UnconditionalPartialReportId = "CL642002";
			public const string ConditionalFinalReportId = "CL642003";
			public const string UnconditionalFinalReportId = "CL642004";
			public const string BillsAndAdjustments = "AP.30.10.00";

			public const string Separator = ".";
		}

		public static class NumberingSequence
		{
			public const string NewSymbol = "<NEW>";
			public const string StartNumber = "{0}-000000";
			public const string LastNumber = "{0}-000000";
			public const string WarnNumber = "{0}-999899";
			public const string EndNumber = "{0}-999999";
			public const int DefaultYear = 1900;
			public const int DefaultMonth = 1;
			public const int DefaultDay = 1;
		}

		public static class HtmlParser
		{
			public const string HtmlBodyXpath = "//body";
			public const string HtmlSpace = "&nbsp;";
			public const string InnerSpace = " ";
		}

		public static class AttributeProperties
		{
			public const string DisplayName = "DisplayName";
			public const string Visible = "Visible";
			public const string DescriptionField = "DescriptionField";
			public const string IsKey = "IsKey";
			public const string Headers = "Headers";
			public const string Required = "Required";
		}

		public static class ActionNames
		{
			public const string ProcessAll = "ProcessAll";
			public const string Schedule = "Schedule";
		}

		public static class WarningMessageSymbols
		{
			public const string NewLine = "~";
		}
	}
}