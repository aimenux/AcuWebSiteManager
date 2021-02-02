using System;
using PX.Objects.CM;

namespace PX.Objects.AR
{
	/// <exclude/>
	public class ReverseInvoiceArgs
	{
		public enum CopyOption
		{
			SetOriginal,
			SetDefault,
			Override,
		}

		public bool ApplyToOriginalDocument { get; set; }
		public bool PreserveOriginalDocumentSign { get; set; }
		public bool? OverrideDocumentHold { get; set; }
		public string OverrideDocumentDescr { get; set; }

		public CopyOption DateOption { get; set; } = CopyOption.SetOriginal;
		public DateTime? DocumentDate { get; set; }
		public string DocumentFinPeriodID { get; set; }

		public CopyOption CurrencyRateOption { get; set; } = CopyOption.SetOriginal;
		public CurrencyInfo CurrencyRate { get; set; }
	}
}
