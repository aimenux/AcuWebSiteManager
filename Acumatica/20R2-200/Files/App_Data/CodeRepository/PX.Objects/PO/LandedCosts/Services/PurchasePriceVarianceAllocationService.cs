using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;

namespace PX.Objects.PO.LandedCosts
{
	public class PurchasePriceVarianceAllocationService : AllocationServiceBase
	{
		private static readonly Lazy<PurchasePriceVarianceAllocationService> _purchasePriceVarianceAllocationService = new Lazy<PurchasePriceVarianceAllocationService>(PXGraph.CreateInstance<PurchasePriceVarianceAllocationService>);

		public static PurchasePriceVarianceAllocationService Instance => _purchasePriceVarianceAllocationService.Value;

		public virtual decimal AllocateOverRCTLine(PXGraph graph, List<POReceiptLineAdjustment> result, POReceiptLine aLine, decimal toDistribute, Int32? branchID)
		{
			var allocationItem = new AllocationItem
			{
				LandedCostCode = new LandedCostCode(),
				ReceiptLine = aLine
			};

			allocationItem.LandedCostCode.AllocationMethod = LandedCostAllocationMethod.ByQuantity;

			var rest = AllocateOverRCTLine(graph, result, allocationItem, toDistribute, branchID);

			return rest;
		}
	}
}
