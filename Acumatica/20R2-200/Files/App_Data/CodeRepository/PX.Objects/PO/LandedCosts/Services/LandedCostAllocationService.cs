using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.TX;

namespace PX.Objects.PO.LandedCosts
{
	public class LandedCostAllocationService : AllocationServiceBase
	{
		private static readonly Lazy<LandedCostAllocationService> _landedCostAllocationService = new Lazy<LandedCostAllocationService>(PXGraph.CreateInstance<LandedCostAllocationService>);

		public static LandedCostAllocationService Instance => _landedCostAllocationService.Value;

		public virtual POLandedCostSplit[] GetLandedCostSplits(POLandedCostDoc doc, POLandedCostReceiptLineAdjustment[] adjustments)
		{
			var result = adjustments.Where(t=>t.LandedCostReceiptLine != null && t.LandedCostDetail != null).GroupBy(t => new
				{
					LandedCostReceiptLineLineNbr = t.LandedCostReceiptLine.LineNbr,
					LandedCostDetailLineNbr = t.LandedCostDetail.LineNbr
				})
				.Select(t => new POLandedCostSplit
				{
					DocType = doc.DocType,
					RefNbr = doc.RefNbr,
					ReceiptLineNbr = t.Key.LandedCostReceiptLineLineNbr,
					DetailLineNbr = t.Key.LandedCostDetailLineNbr,
					LineAmt = t.Sum(m => m.AllocatedAmt)
				});

			return result.ToArray();
		}

		public virtual POLandedCostReceiptLineAdjustment[] Allocate(PXGraph graph, POLandedCostDoc doc,
			IEnumerable<POLandedCostReceiptLine> landedCostReceiptLines, IEnumerable<POLandedCostDetail> details, IEnumerable<PXResult<POLandedCostTax, Tax>> taxes)
		{
			var result = new List<POLandedCostReceiptLineAdjustment>();

			if (!landedCostReceiptLines.Any() || !details.Any())
				return result.ToArray();

			var receiptLines = GetReceiptLines(graph, landedCostReceiptLines);
			var receipts = GetReceipts(graph, landedCostReceiptLines);
			var landedCostCodes = GetLandedCostCodes(graph, details);

			foreach (var poLandedCostDetail in details)
			{
				var detailAdjustments = AllocateLCOverRCTLines(graph, poLandedCostDetail, landedCostReceiptLines, taxes, receiptLines, landedCostCodes, receipts);

				result.AddRange(detailAdjustments);
			}

			return result.ToArray();
		}

		public bool HasApplicableTransfers { get; set; }

		public LandedCostAllocationService()
		{ 
			HasApplicableTransfers = false;
		}

		protected virtual POReceiptLine[] GetReceiptLines(PXGraph graph, IEnumerable<POLandedCostReceiptLine> landedCostReceiptLines)
		{
			var receiptLines = new List<POReceiptLine>();

			foreach (var receiptLineGroup in landedCostReceiptLines.GroupBy(t => new { t.POReceiptType, t.POReceiptNbr }))
			{
				var groupReceiptLines =
					PXSelect<POReceiptLine, Where<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
							And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>>>>
						.Select(graph, receiptLineGroup.Key.POReceiptType, receiptLineGroup.Key.POReceiptNbr).RowCast<POReceiptLine>()
						.ToList();

				var lineIds = receiptLineGroup.Select(t => t.POReceiptLineNbr);

				groupReceiptLines = groupReceiptLines.Where(t => lineIds.Contains(t.LineNbr)).ToList();

				receiptLines.AddRange(groupReceiptLines);
			}

			return receiptLines.ToArray();
		}

		protected virtual LandedCostCode[] GetLandedCostCodes(PXGraph graph, IEnumerable<POLandedCostDetail> landedCostDetails)
		{
			var landedCostCodeIds = landedCostDetails.Select(t => t.LandedCostCodeID).Distinct().ToArray();
			var landedCostCodes = PXSelectReadonly<LandedCostCode, Where<LandedCostCode.landedCostCodeID, In<Required<LandedCostCode.landedCostCodeID>>>>
				.Select(graph, (object)landedCostCodeIds).RowCast<LandedCostCode>()
				.ToArray();

			return landedCostCodes;
		}

		protected virtual POReceipt[] GetReceipts(PXGraph graph, IEnumerable<POLandedCostReceiptLine> landedCostReceiptLines)
		{
			var receipts = new List<POReceipt>();

			foreach (var receiptLineGroup in landedCostReceiptLines.GroupBy(t => new { t.POReceiptType }))
			{
				var receiptType = receiptLineGroup.Key.POReceiptType;
				var receiptNbrs = receiptLineGroup.Select(t => t.POReceiptNbr).Distinct().ToArray();

				var groupReceiptLines =
					PXSelect<POReceipt, Where<POReceipt.receiptType, Equal<Required<POReceipt.receiptType>>,
							And<POReceipt.receiptNbr, In<Required<POReceipt.receiptNbr>>>>>
						.Select(graph, receiptType, receiptNbrs).RowCast<POReceipt>()
						.ToList();

				receipts.AddRange(groupReceiptLines);
			}

			return receipts.ToArray();
		}

		public virtual bool IsApplicable(PXGraph graph, POLandedCostDetail aTran, LandedCostCode aCode, POReceiptLine aLine)
		{
			if (!IsApplicable(graph, aCode, aLine)) return false;
			
			if (aTran.InventoryID.HasValue)
					if (aTran.InventoryID != aLine.InventoryID) return false;

			return true;
		}

		public virtual bool IsApplicable(PXGraph graph, LandedCostCode aCode, POReceiptLine aLine)
		{
			bool transferinsidewarehouse = false;

			if (aLine.ReceiptType == POReceiptType.TransferReceipt)
			{
				INTran ortran = INTran.PK.Find(graph, aLine.OrigDocType, aLine.OrigRefNbr, aLine.OrigLineNbr);
				transferinsidewarehouse = (ortran == null || ortran.SiteID == aLine.SiteID);
			}

			if (transferinsidewarehouse == false)
				HasApplicableTransfers = true;

			//Memo - in this release, non-stock Items are not applicable for the landed cost. Review later.
			return !transferinsidewarehouse &&
				(aCode.AllocationMethod != LandedCostAllocationMethod.None && (aLine.LineType == POLineType.GoodsForInventory ||
				aLine.LineType == POLineType.GoodsForReplenishment ||
				aLine.LineType == POLineType.GoodsForSalesOrder ||
				aLine.LineType == POLineType.GoodsForDropShip ||
				aLine.LineType == POLineType.NonStock ||
				aLine.LineType == POLineType.NonStockForDropShip ||
				aLine.LineType == POLineType.NonStockForSalesOrder ||
				aLine.LineType == POLineType.GoodsForServiceOrder ||
				aLine.LineType == POLineType.NonStockForServiceOrder ||
				aLine.LineType == POLineType.GoodsForManufacturing ||
				aLine.LineType == POLineType.NonStockForManufacturing));
		}

		public override Decimal CalcAllocationValue(PXGraph graph, AllocationItem allocationItem, Decimal aBaseTotal, Decimal aAllocationTotal)
		{
			Decimal value = Decimal.Zero;

			if (IsApplicable(graph, allocationItem.LandedCostCode, allocationItem.ReceiptLine))
			{
				Decimal baseShare = GetBaseValue(allocationItem);
				value = aBaseTotal == 0 ? 0 : (baseShare * aAllocationTotal) / aBaseTotal;
			}

			return value;
		}

		public override POReceiptLineAdjustment CreateReceiptLineAdjustment(AllocationItem allocationItem, POReceiptLine receiptLine,
			decimal qtyToAssign, int? branchID)
		{
			if ((allocationItem == null || allocationItem.ReceiptLine == null) && receiptLine == null)
			{
				var emptyReceiptLine = new POReceiptLine();

				return new POLandedCostReceiptLineAdjustment(emptyReceiptLine, null, null, qtyToAssign, branchID);
			}

			if (!(allocationItem is LandedCostAllocationItem))
				return base.CreateReceiptLineAdjustment(allocationItem, receiptLine, qtyToAssign, branchID);

			var lcAllocationItemitem = (LandedCostAllocationItem) allocationItem;

			var result = new POLandedCostReceiptLineAdjustment(receiptLine ?? allocationItem.ReceiptLine, lcAllocationItemitem.LandedCostReceiptLine, lcAllocationItemitem.LandedCostDetail, qtyToAssign, branchID);

			return result;
		}

		protected virtual decimal GetAllocationAmount(POLandedCostDetail landedCostDetail, IEnumerable<PXResult<POLandedCostTax, Tax>> taxes)
		{
			var landedCostTax = taxes
				.Where(t => ((Tax)t).TaxCalcLevel == CSTaxCalcLevel.Inclusive && ((Tax)t).TaxType != CSTaxType.Withholding && ((Tax)t).ReverseTax != true)
				.RowCast<POLandedCostTax>()
				.FirstOrDefault(t => t.LineNbr == landedCostDetail.LineNbr);

			if (landedCostTax != null)
			{
				return landedCostTax.TaxableAmt ?? 0;
			}

			return landedCostDetail.LineAmt ?? 0;
		}

		public virtual List<POLandedCostReceiptLineAdjustment> AllocateLCOverRCTLines(
			PXGraph graph,
			POLandedCostDetail landedCostDetail,
			IEnumerable<POLandedCostReceiptLine> landedCostReceiptLines,
			IEnumerable<PXResult<POLandedCostTax, Tax>> taxes,
			IEnumerable<POReceiptLine> receiptLines,
			IEnumerable<LandedCostCode> landedCostCodes,
			IEnumerable<POReceipt> receipts)
		{
			var landedCostCode = landedCostCodes.Single(t => t.LandedCostCodeID == landedCostDetail.LandedCostCodeID);

			var rctLinesList = receiptLines.Where(rl => IsApplicable(graph, landedCostDetail, landedCostCode, rl)).ToList();
			var rctLinesAllocItems = new LandedCostAllocationItem[rctLinesList.Count];
			Decimal toDistribute = GetAllocationAmount(landedCostDetail, taxes);
			Decimal baseTotal = decimal.Zero;

			for (int i = 0; i < rctLinesList.Count; i++)
			{
				POReceiptLine receiptLine = rctLinesList[i];
				var landedCostReceiptLine = landedCostReceiptLines.Single(t
					=> t.POReceiptType == receiptLine.ReceiptType
					&& t.POReceiptNbr == receiptLine.ReceiptNbr
					&& t.POReceiptLineNbr == receiptLine.LineNbr);

				rctLinesAllocItems[i] = new LandedCostAllocationItem()
				{
					LandedCostCode = landedCostCode,
					ReceiptLine = receiptLine,
					LandedCostDetail = landedCostDetail,
					LandedCostReceiptLine = landedCostReceiptLine
				};

				baseTotal += GetBaseValue(rctLinesAllocItems[i]);
			}

			var result = new List<POReceiptLineAdjustment>();
			decimal allocatedAmt = 0m;
			decimal handledLinesAllocAmt = 0m;
			int? maxLineIndex = null;
			decimal maxLineAllocAmt = 0m;
			for (int i = 0; i < rctLinesList.Count; i++)
			{
				decimal allocAmt = CalcAllocationValue(graph, rctLinesAllocItems[i], baseTotal, toDistribute);
				handledLinesAllocAmt += allocAmt;

				if (maxLineIndex == null || Math.Abs(maxLineAllocAmt) < Math.Abs(allocAmt))
				{
					maxLineIndex = i;
					maxLineAllocAmt = allocAmt;
				}

				allocAmt = handledLinesAllocAmt - allocatedAmt;
				if (allocAmt != 0m)
				{
					POReceiptLine maxLine = rctLinesList[maxLineIndex.Value];
					var poreceipt = receipts.Single(r
						=> r.ReceiptType == maxLine.ReceiptType
						&& r.ReceiptNbr == maxLine.ReceiptNbr);

					decimal lineAllocatedAmt = AllocateOverRCTLine(graph, result, rctLinesAllocItems[maxLineIndex.Value], allocAmt, poreceipt.BranchID);
					if (lineAllocatedAmt != 0m)
					{
						maxLineIndex = null;
						maxLineAllocAmt = 0m;
						allocatedAmt += lineAllocatedAmt;
					}
				}
			}

			AllocateRestOverRCTLines(result, toDistribute - allocatedAmt);

			var castResult = result.Cast<POLandedCostReceiptLineAdjustment>().ToList();

			return castResult;
		}

		public virtual bool ValidateLCTran(PXGraph graph, POLandedCostDoc doc, IEnumerable<POLandedCostReceiptLine> landedCostReceiptLines, POLandedCostDetail row, out string message)
		{
			decimal valueToCompare = decimal.Zero;
			int count = 0;
			if (row != null && !String.IsNullOrEmpty(row.LandedCostCodeID))
			{
				LandedCostCode code = LandedCostCode.PK.Find(graph, row.LandedCostCodeID);

				var receiptLines = GetReceiptLines(graph, landedCostReceiptLines);

				foreach (POReceiptLine it in receiptLines)
				{
					var landedCostReceiptLine = landedCostReceiptLines.Single(t =>
						t.POReceiptType == it.ReceiptType && t.POReceiptNbr == it.ReceiptNbr && t.POReceiptLineNbr == it.LineNbr);

					if (IsApplicable(graph, row, code, it))
					{
						var landedCostAllocationItem = new LandedCostAllocationItem()
						{
							LandedCostCode = code,
							ReceiptLine = it,
							LandedCostDetail = row,
							LandedCostReceiptLine = landedCostReceiptLine
						};

						valueToCompare += GetBaseValue(landedCostAllocationItem);
					}
					count++;
				}

				if (!HasApplicableTransfers)
				{
					message = Messages.LandedCostCannotBeDistributed;
					return false;
				}

				switch (code.AllocationMethod)
				{
					case LandedCostAllocationMethod.ByCost:
						message = Messages.LandedCostReceiptTotalCostIsZero;
						break;
					case LandedCostAllocationMethod.ByWeight:
						message = Messages.LandedCostReceiptTotalWeightIsZero;
						break;
					case LandedCostAllocationMethod.ByVolume:
						message = Messages.LandedCostReceiptTotalVolumeIsZero;
						break;
					case LandedCostAllocationMethod.ByQuantity:
						message = Messages.LandedCostReceiptTotalQuantityIsZero;
						break;
					case LandedCostAllocationMethod.None:
						message = Messages.LandedCostReceiptNoReceiptLines;
						valueToCompare = count;
						break;
					default:
						message = Messages.LandedCostUnknownAllocationType;
						break;
				}
				if (valueToCompare == Decimal.Zero)
				{
					return false;
				}
			}
			message = String.Empty;
			return true;
		}

		public class LandedCostAllocationItem : AllocationItem
		{
			public POLandedCostReceiptLine LandedCostReceiptLine { get; set; }

			public POLandedCostDetail LandedCostDetail { get; set; }

			public override Decimal? Weight => LandedCostReceiptLine?.ExtWeight ?? ReceiptLine?.ExtWeight;

			public override Decimal? Volume => LandedCostReceiptLine?.ExtVolume ?? ReceiptLine?.ExtVolume;
		}

		public class POLandedCostReceiptLineAdjustment : POReceiptLineAdjustment
		{
			public POLandedCostReceiptLineAdjustment(POReceiptLine receiptLine, POLandedCostReceiptLine landedCostReceiptLine,
				POLandedCostDetail landedCostDetail, decimal qtyToAssign, int? branchID)
				: base(receiptLine, qtyToAssign, branchID)
			{
				LandedCostReceiptLine = landedCostReceiptLine;
				LandedCostDetail = landedCostDetail;
			}

			public POLandedCostReceiptLine LandedCostReceiptLine { get; protected set; }

			public POLandedCostDetail LandedCostDetail { get; protected set; }
		}
	}
}
