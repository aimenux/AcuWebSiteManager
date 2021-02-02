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
	public abstract class AllocationServiceBase : PXGraph<AllocationServiceBase>
	{
		public virtual decimal AllocateOverRCTLine(
			PXGraph graph,
			List<POReceiptLineAdjustment> result,
			AllocationItem allocationItem,
			decimal toDistribute,
			Int32? branchID)
		{
			// the artificial object is used for allocating the landed cost amount between splits (by quantity)
			var nAllocationItem = new AllocationItem()
			{
				LandedCostCode = new LandedCostCode()
				{
					AllocationMethod = LandedCostAllocationMethod.ByQuantity
				},
				ReceiptLine = allocationItem.ReceiptLine,
			};

			InventoryItem ii = InventoryItem.PK.Find(graph, allocationItem.ReceiptLine.InventoryID.Value);
			bool requireLotSerial = (ii.ValMethod == INValMethod.Specific);

			List<Type> bql = new List<Type>(16)
			{
				typeof(Select4<,,>),
				 typeof(POReceiptLineSplit),
				 typeof(Where<POReceiptLineSplit.receiptType, Equal<Required<POReceiptLine.receiptType>>, And<POReceiptLineSplit.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>, And<POReceiptLineSplit.lineNbr, Equal<Required<POReceiptLine.lineNbr>>>>>),
				 typeof(Aggregate<>),
				 typeof(GroupBy<,>),
				 typeof(POReceiptLineSplit.locationID),
				 typeof(GroupBy<,>),
				 typeof(POReceiptLineSplit.subItemID),
				 typeof(Sum<>),
				 typeof(POReceiptLineSplit.baseQty),
			};
			if (requireLotSerial)
			{
				bql.Insert(bql.Count - 2, typeof(GroupBy<,>));
				bql.Insert(bql.Count - 2, typeof(POReceiptLineSplit.lotSerialNbr));
			}

			PXView splitsView = new PXView(graph, false, BqlCommand.CreateInstance(bql.ToArray()));
			var splits = splitsView.SelectMulti(allocationItem.ReceiptLine.ReceiptType, allocationItem.ReceiptLine.ReceiptNbr,
				allocationItem.ReceiptLine.LineNbr);

			bool hasSplits = false;
			decimal baseTotal = GetBaseValue(nAllocationItem);
			decimal allocatedBase = 0m;
			decimal allocatingBase = 0m;
			decimal allocatedAmt = 0m;
			POReceiptLineSplit maxSplit = null;
			foreach (POReceiptLineSplit split in splits)
			{
				hasSplits = true;
				allocatingBase += split.BaseQty ?? 0m;
				decimal handledSplitsAmt = (baseTotal == 0m) ? 0m : (allocatedBase + allocatingBase) * toDistribute / baseTotal;
				handledSplitsAmt = PXDBCurrencyAttribute.BaseRound(graph, handledSplitsAmt);
				decimal shareAmt = handledSplitsAmt - allocatedAmt;

				if (maxSplit == null || maxSplit.BaseQty < split.BaseQty)
				{
					maxSplit = split;
				}

				if (shareAmt != 0m)
				{
					POReceiptLine newPOReceiptLine = PXCache<POReceiptLine>.CreateCopy(allocationItem.ReceiptLine);
					newPOReceiptLine.LocationID = maxSplit.LocationID;
					newPOReceiptLine.SiteID = maxSplit.SiteID;
					newPOReceiptLine.SubItemID = maxSplit.SubItemID;
					newPOReceiptLine.LotSerialNbr = requireLotSerial ? maxSplit.LotSerialNbr : null;

					var adj = CreateReceiptLineAdjustment(allocationItem, newPOReceiptLine, shareAmt, branchID);

					result.Add(adj);
					allocatedAmt += shareAmt;
					allocatedBase += allocatingBase;
					allocatingBase = 0m;
					maxSplit = null;
				}
			}
			if (!hasSplits)
			{
				decimal shareAmt = toDistribute;
				shareAmt = PXDBCurrencyAttribute.BaseRound(graph, shareAmt);
				if (shareAmt != 0m)
				{
					var adj = CreateReceiptLineAdjustment(allocationItem, null, shareAmt, branchID);

					result.Add(adj);
				}

				allocatedAmt = shareAmt;
			}
			return allocatedAmt;
		}

		public virtual void AllocateRestOverRCTLines(IList<POReceiptLineAdjustment> aLines, decimal rest)
		{
			if (rest != Decimal.Zero)
			{
				if (aLines.Count == 0)
				{
					var adj = CreateReceiptLineAdjustment(null, null, rest, null);
					aLines.Add(adj);
				}
				else
				{
					decimal maxAllocatedAmt = -1m;
					int indexOfMax = 0;
					for (int i = 0; i < aLines.Count; i++)
					{
						decimal absAllocatedAmt = Math.Abs(aLines[i].AllocatedAmt);
						if (maxAllocatedAmt < absAllocatedAmt)
						{
							maxAllocatedAmt = absAllocatedAmt;
							indexOfMax = i;
						}
					}
					aLines[indexOfMax].AllocatedAmt += rest;
				}
			}
		}

		public virtual INTran GetOriginalInTran(PXGraph graph, string receiptNbr, int? lineNbr)
		{
			if (receiptNbr == null || lineNbr == null)
				return null;

			return
					PXSelectReadonly<INTran, Where<INTran.docType, NotEqual<INDocType.adjustment>,
						And<INTran.docType, NotEqual<INDocType.transfer>,
						And<INTran.pOReceiptNbr, Equal<Required<INTran.pOReceiptNbr>>,
						And<INTran.pOReceiptLineNbr, Equal<Required<INTran.pOReceiptLineNbr>>>>>>>.SelectWindowed(graph, 0, 1, receiptNbr, lineNbr);
		}

		public virtual bool NeedsApplicabilityChecking(LandedCostCode aCode)
		{
			return (aCode.AllocationMethod != LandedCostAllocationMethod.None);
		}

		public virtual Decimal GetBaseValue(AllocationItem allocationItem)
		{
			Decimal value = Decimal.Zero;
			switch (allocationItem.LandedCostCode.AllocationMethod)
			{
				case LandedCostAllocationMethod.ByCost: value = allocationItem.TranCostFinal ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.ByQuantity: value = allocationItem.BaseQty ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.ByWeight: value = allocationItem.Weight ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.ByVolume: value = allocationItem.Volume ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.None: value = Decimal.One; break; //Line Count
				default:
					throw new PXException(Messages.UnknownLCAllocationMethod, allocationItem.LandedCostCode.LandedCostCodeID);
			}
			return value;
		}



		public virtual Decimal CalcAllocationValue(PXGraph graph, AllocationItem allocationItem, Decimal aBaseTotal, Decimal aAllocationTotal)
		{
			return aAllocationTotal;
		}

		public virtual Decimal CalcAllocationValue(PXGraph graph, AllocationItem allocationItem, POReceiptLineSplit aSplit, Decimal aBaseTotal, Decimal aAllocationTotal)
		{
			var result = CalcAllocationValue(graph, allocationItem, aBaseTotal, aAllocationTotal);

			result *= ((aSplit.BaseQty / (allocationItem.BaseQty == 0 ? (decimal?)null : allocationItem.BaseQty)) ?? 1);

			return result;
		}

		public virtual POReceiptLineAdjustment CreateReceiptLineAdjustment(AllocationItem allocationItem, POReceiptLine receiptLine,
			decimal qtyToAssign, int? branchID)
		{
			if ((allocationItem == null || allocationItem.ReceiptLine == null) && receiptLine == null)
			{
				var emptyReceiptLine = new POReceiptLine();

				return new POReceiptLineAdjustment(emptyReceiptLine, qtyToAssign, branchID);
			}

			var result = new POReceiptLineAdjustment(receiptLine ?? allocationItem.ReceiptLine, qtyToAssign, branchID);

			return result;
		}

		public class AllocationItem
		{
			public LandedCostCode LandedCostCode { get; set; }

			public POReceiptLine ReceiptLine { get; set; }

			public virtual Decimal? TranCostFinal => ReceiptLine?.TranCostFinal;

			public virtual Decimal? BaseQty => ReceiptLine?.BaseQty;

			public virtual Decimal? Weight => ReceiptLine?.ExtWeight;

			public virtual Decimal? Volume => ReceiptLine?.ExtVolume;
		}

		public class POReceiptLineAdjustment
		{
			public POReceiptLine ReceiptLine { get; protected set; }

			public decimal AllocatedAmt { get; protected internal set; }

			public Int32? BranchID { get; protected set; }

			public POReceiptLineAdjustment(POReceiptLine receiptLine, decimal allocatedAmt, int? branchID)
			{
				ReceiptLine = receiptLine;
				AllocatedAmt = allocatedAmt;
				BranchID = branchID;
			}
		}
	}
}
