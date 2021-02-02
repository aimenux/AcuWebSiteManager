namespace PX.Objects.IN
{
	class PrinterParameters : PX.SM.IPrintable
	{
		public bool? PrintWithDeviceHub { get; set; }
		public bool? DefinePrinterManually { get; set; }
		public string PrinterName { get; set; }
	}
}
