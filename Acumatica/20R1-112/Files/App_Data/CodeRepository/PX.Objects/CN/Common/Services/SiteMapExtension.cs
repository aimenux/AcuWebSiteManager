using PX.Common;
using PX.Objects.CN.Common.Descriptor;

namespace PX.Objects.CN.Common.Services
{
	public static class SiteMapExtension
	{
		public static bool IsTaxBillsAndAdjustmentsScreenId()
		{
			return PXContext.GetScreenID() == Constants.ScreenIds.TaxBillsAndAdjustments ||
			       PXContext.GetScreenID() == Constants.ScreenIds.TaxBillsAndAdjustmentsGenericInquiry;
		}

		public static bool IsInvoicesScreenId()
		{
			return PXContext.GetScreenID() == Constants.ScreenIds.Invoices ||
			       PXContext.GetScreenID() == Constants.ScreenIds.InvoicesGenericInquiry;
		}

		public static bool IsChecksAndPaymentsScreenId()
		{
			return PXContext.GetScreenID() == Constants.ScreenIds.ChecksAndPayments;
		}

		public static bool IsPreparePaymentsScreenId()
		{
			return PXContext.GetScreenID() == Constants.ScreenIds.PreparePayments;
		}

		public static string WithoutSeparator(this string screenId)
		{
			return screenId.Replace(Constants.ScreenIds.Separator, string.Empty);
		}
	}
}