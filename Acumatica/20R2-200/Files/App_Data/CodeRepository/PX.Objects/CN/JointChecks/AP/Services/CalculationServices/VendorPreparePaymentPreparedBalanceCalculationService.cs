using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
	public class VendorPreparePaymentPreparedBalanceCalculationService : VendorPreparedBalanceCalculationService
	{
		public VendorPreparePaymentPreparedBalanceCalculationService(PXGraph graph)
			: base(graph)
		{
		}

		protected override decimal? GetTotalVendorPaymentAmount(APAdjust adjustment, string referenceNumber,
			int? origLineNbr)
		{
			return AdjustmentDataProvider
				.GetAdjustmentsForInvoiceGroup(Graph, referenceNumber, origLineNbr)
				.Where(adj => adj.AdjdRefNbr == adjustment.AdjdRefNbr && adj.AdjdLineNbr == adjustment.AdjdLineNbr)
				.Sum(adj => GetVendorPaymentAmount(adj).GetValueOrDefault());
		}
	}
}