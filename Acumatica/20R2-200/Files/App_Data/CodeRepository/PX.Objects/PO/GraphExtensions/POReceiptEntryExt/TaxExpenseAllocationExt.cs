using System;
using System.Linq;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.Common;
using PX.Objects.PO.Services.AmountDistribution;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class TaxExpenseAllocationExt : PXGraphExtension<POReceiptEntry>
	{
		#region Types

		protected class Split : IAmountItem
		{
			public POAccrualSplit POAccrualSplit { get; set; }

			#region IAmountItem members
			public decimal Weight => (POAccrualSplit.AccruedQty ?? POAccrualSplit.BaseAccruedQty) ?? 0m;
			public decimal? Amount
			{
				get => POAccrualSplit.TaxAccruedCost;
				set => POAccrualSplit.TaxAccruedCost = value;
			}
			public decimal? CuryAmount
			{
				get;
				set;
			}
			#endregion
		}

		#endregion

		#region Variables, Properties

		[InjectDependency]
		public AmountDistributionFactory AmountDistributionFactory { get; set; }

		#endregion

		#region Ovverides

		[PXOverride]
		public virtual void InsertPOAccrualSplits(POReceiptLine rctLine, POAccrualStatus poAccrual,
			string accruedUom, decimal? accruedQty, decimal? baseAccruedQty, decimal? accruedCost,
			Action<POReceiptLine, POAccrualStatus, string, decimal?, decimal?, decimal?> baseMethod)
		{
			List<POAccrualSplit> pOAccrualSplits = CollectPOAccrualSplits(rctLine, poAccrual);
			baseMethod(rctLine, poAccrual, accruedUom, accruedQty, baseAccruedQty, accruedCost);
			ApplyTaxAmount(rctLine, poAccrual, accruedQty, baseAccruedQty, pOAccrualSplits);
		}

		#endregion

		#region Implementation

		protected virtual List<POAccrualSplit> CollectPOAccrualSplits(POReceiptLine rctLine, POAccrualStatus poAccrual)
		{
			return Base.apTranUpdate.View.SelectMultiBound(new object[] { poAccrual }).AsEnumerable().RowCast<APTranReceiptUpdate>()
				.Select(tran => new POAccrualSplit()
				{
					RefNoteID = poAccrual.RefNoteID,
					LineNbr = poAccrual.LineNbr,
					Type = poAccrual.Type,
					APDocType = tran.TranType,
					APRefNbr = tran.RefNbr,
					APLineNbr = tran.LineNbr,
					POReceiptNbr = rctLine.ReceiptNbr,
					POReceiptLineNbr = rctLine.LineNbr
				}).ToList();
		}

		protected virtual void ApplyTaxAmount(POReceiptLine rctLine, POAccrualStatus poAccrual, decimal? accruedQty, decimal? baseAccruedQty, List<POAccrualSplit> pOAccrualSplits)
		{
			decimal taxAmountToDistribute = CalculateTaxAmountToDistribute(poAccrual, accruedQty, baseAccruedQty);
			poAccrual = ApplyTaxAmountToPOAccrual(rctLine, poAccrual, taxAmountToDistribute);
			DistributeTaxAmountToSplits(rctLine, pOAccrualSplits, taxAmountToDistribute);
		}

		protected virtual decimal CalculateTaxAmountToDistribute(POAccrualStatus poAccrual, decimal? accruedQty, decimal? baseAccruedQty)
		{
			bool uomCoincide = (accruedQty != null);

			decimal qtyToDistribute;
			decimal unreceivedQty;

			if (uomCoincide)
			{
				qtyToDistribute = accruedQty ?? 0m;
				unreceivedQty = poAccrual.BilledQty - poAccrual.ReceivedQty ?? 0m;
			}
			else
			{
				qtyToDistribute = baseAccruedQty ?? 0m;
				unreceivedQty = poAccrual.BaseBilledQty - poAccrual.BaseReceivedQty ?? 0m;
			}

			decimal unreceivedTaxAmount = poAccrual.BilledTaxAdjCost - poAccrual.ReceivedTaxAdjCost ?? 0m;
			decimal taxAmountToDistribute = (unreceivedTaxAmount * qtyToDistribute) / unreceivedQty;

			return taxAmountToDistribute;
		}

		protected virtual POAccrualStatus ApplyTaxAmountToPOAccrual(POReceiptLine rctLine, POAccrualStatus poAccrual, decimal taxAmountToDistribute)
		{
			poAccrual.ReceivedTaxAdjCost += taxAmountToDistribute;
			poAccrual.ReceivedCost -= taxAmountToDistribute;
			poAccrual = Base.poAccrualUpdate.Update(poAccrual);

			rctLine.TranCostFinal += taxAmountToDistribute;

			return poAccrual;
		}

		protected virtual void DistributeTaxAmountToSplits(POReceiptLine rctLine, List<POAccrualSplit> pOAccrualSplits, decimal taxAmountToDistribute)
		{
			var splits = pOAccrualSplits
				.Select(s => new Split { POAccrualSplit = (POAccrualSplit)Base.poAccrualSplitUpdate.Cache.Locate(s) })
				.Where(s => s.POAccrualSplit != null);

			AmountDistributionFactory.CreateService(DistributionMethod.RemainderToBiggestLine, new DistributionParameter<Split>()
			{
				Items = splits,
				Amount = taxAmountToDistribute,
				CuryAmount = taxAmountToDistribute,
				CuryRow = rctLine,
				CacheOfCuryRow = Base.transactions.Cache,
				OnValueCalculated = (item, amount, curyAmount) => { item.POAccrualSplit = Base.poAccrualSplitUpdate.Update(item.POAccrualSplit); return item; },
				OnRoundingDifferenceApplied = (item, newAmount, curyNewAmount, oldAmount, curyOldAmount) => item.POAccrualSplit = Base.poAccrualSplitUpdate.Update(item.POAccrualSplit),
			}).Distribute();
		}

		#endregion
	}
}
