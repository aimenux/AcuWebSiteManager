using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.PO.LandedCosts;
using PX.Objects.PO.Services.AmountDistribution;
using PX.Objects.TX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amount = PX.Objects.AR.ARReleaseProcess.Amount;

namespace PX.Objects.PO.GraphExtensions.POLandedCostDocEntryExt
{
	public class TaxExpenseAllocationExt : PXGraphExtension<POLandedCostDocEntry>
	{
		#region Types
		protected interface ISplit
		{
			int LineNbr { get; }
			decimal? LineAmt { get; set; }
			decimal? CuryLineAmt { get; set; }
		}

		protected class POLandedCostTax_Tax_Split<TSplit> : IAmountItem
			where TSplit : ISplit, IAmountItem
		{
			public POLandedCostTax_Tax_Split(POLandedCostTax pOLandedCostTax, Tax tax, TSplit split)
			{
				POLandedCostTax = pOLandedCostTax;
				Tax = tax;
				Split = split;
			}

			public POLandedCostTax POLandedCostTax { get; private set; }
			public Tax Tax { get; private set; }
			public TSplit Split { get; private set; }


			public decimal Weight => Split.Weight;

			public decimal? Amount
			{
				get => Split.Amount;
				set => Split.Amount = value;
			}
			public decimal? CuryAmount
			{
				get => Split.CuryAmount;
				set => Split.CuryAmount = value;
			}
		}

		protected class POLCSplit : IAmountItem, ISplit
		{
			public POLCSplit(POLandedCostSplit adj)
			{
				POLandedCostSplit = adj;
			}

			POLandedCostSplit POLandedCostSplit;

			public decimal Weight => POLandedCostSplit.LineAmt ?? 0m;

			public decimal? Amount
			{
				get;
				set;
			}
			public decimal? CuryAmount
			{
				get;
				set;
			}

			public int LineNbr => POLandedCostSplit.DetailLineNbr ?? 0;

			public decimal? LineAmt
			{
				get => POLandedCostSplit.LineAmt;
				set => POLandedCostSplit.LineAmt = value;
			}
			public decimal? CuryLineAmt
			{
				get => POLandedCostSplit.CuryLineAmt;
				set => POLandedCostSplit.CuryLineAmt = value;
			}
		}

		protected class LineAdjustments : IAmountItem, ISplit
		{
			public LineAdjustments(LandedCostAllocationService.POLandedCostReceiptLineAdjustment adj)
			{
				POLandedCostReceiptLineAdjustment = adj;
			}

			LandedCostAllocationService.POLandedCostReceiptLineAdjustment POLandedCostReceiptLineAdjustment;

			public decimal Weight => POLandedCostReceiptLineAdjustment.AllocatedAmt;

			public decimal? Amount
			{
				get;
				set;
			}
			public decimal? CuryAmount
			{
				get;
				set;
			}

			public int LineNbr => POLandedCostReceiptLineAdjustment.LandedCostDetail?.LineNbr ?? 0;

			public decimal? LineAmt
			{
				get => POLandedCostReceiptLineAdjustment.AllocatedAmt;
				set => POLandedCostReceiptLineAdjustment.AllocatedAmt = value ?? 0m;
			}
			public decimal? CuryLineAmt
			{
				get;
				set;
			}
		}
		#endregion

		#region Properties

		[InjectDependency]
		public AmountDistributionFactory AmountDistributionFactory { get; set; }

		#endregion

		#region Overrides

		[PXOverride]
		public virtual void TrackLandedCostSplits(IEnumerable<POLandedCostSplit> landedCostSplits, Action<IEnumerable<POLandedCostSplit>> baseMethod)
		{
			baseMethod(landedCostSplits);
			CalculateTaxes(landedCostSplits.Select(s => new POLCSplit(s)));
		}

		[PXOverride]
		public virtual List<INRegister> CreateLandedCostAdjustment(POLandedCostDoc doc,
			IEnumerable<LandedCostAllocationService.POLandedCostReceiptLineAdjustment> adjustments,
			Func<POLandedCostDoc, IEnumerable<LandedCostAllocationService.POLandedCostReceiptLineAdjustment>, List<INRegister>> baseMethod)
		{
			CalculateTaxes(adjustments.Select(s => new LineAdjustments(s)));
			return baseMethod(doc, adjustments);
		}

		#endregion

		#region Implementation

		protected virtual void CalculateTaxes<TSplit>(IEnumerable<TSplit> landedCostSplits)
			where TSplit : ISplit, IAmountItem
		{
			var doc = Base.Document.Current;

			var taxTrans = Base.Taxes.View.SelectMultiBound(new object[] { doc })
				.AsEnumerable()
				.Cast<PXResult<POLandedCostTaxTran, Tax>>()
				.Select(t => new { POLandedCostTaxTran = t.GetItem<POLandedCostTaxTran>(), Tax = t.GetItem<Tax>() })
				.Where(t => IsItemCostTax(t.Tax));

			if (taxTrans.Any()) // Posting Tax to Inventory through Landed Costs is not supported now
				throw new PXException(Messages.PostingTaxToInventoryThroughLandedCostsIsNotSupported, taxTrans.First().Tax.TaxID);

			var taxesGroupedByTaxId = GetLCTaxesGrouppedByTaxId(landedCostSplits, doc);

			foreach (var taxTran in taxTrans)
			{
				if (!taxesGroupedByTaxId.ContainsKey(taxTran.Tax.TaxID))
					throw new PXArgumentException(nameof(taxTran.Tax.TaxID));

				List<POLandedCostTax_Tax_Split<TSplit>> lcTaxes = taxesGroupedByTaxId[taxTran.Tax.TaxID];

				if (lcTaxes.Count == 0)
					throw new PXArgumentException(nameof(lcTaxes));

				int taxMult = 1;

				Amount taxAmount = (taxTran.Tax.TaxType != CSTaxType.VAT || taxTran.Tax.DeductibleVAT != true) ?
					new Amount(taxTran.POLandedCostTaxTran.CuryTaxAmt, taxTran.POLandedCostTaxTran.TaxAmt) * taxMult :
					new Amount(taxTran.POLandedCostTaxTran.CuryExpenseAmt, taxTran.POLandedCostTaxTran.ExpenseAmt) * taxMult;

				Func<POLandedCostTax_Tax_Split<TSplit>, decimal?, decimal?, POLandedCostTax_Tax_Split<TSplit>> addAmount =
					(item, amount, curyAmount) =>
					{
						item.Split.LineAmt += amount;
						item.Split.CuryLineAmt += curyAmount;
						return item;
					};

				AmountDistributionFactory.CreateService(DistributionMethod.RemainderToBiggestLine, new DistributionParameter<POLandedCostTax_Tax_Split<TSplit>>()
				{
					Items = lcTaxes,
					Amount = taxAmount.Base,
					CuryAmount = taxAmount.Cury,
					CuryRow = doc,
					CacheOfCuryRow = Base.Document.Cache,
					OnValueCalculated = addAmount,
					OnRoundingDifferenceApplied = (item, newAmount, curyNewAmount, oldAmount, curyOldAmount) => addAmount(item, newAmount - oldAmount, curyNewAmount - curyOldAmount)
				}).Distribute();
			}
		}

		protected virtual Dictionary<string, List<POLandedCostTax_Tax_Split<TSplit>>> GetLCTaxesGrouppedByTaxId<TSplit>(IEnumerable<TSplit> landedCostSplits, POLandedCostDoc doc)
			where TSplit : ISplit, IAmountItem
		{
			var taxesQuery = new PXSelectJoin<POLandedCostTax, InnerJoin<Tax, On<POLandedCostTax.taxID, Equal<Tax.taxID>>>,
				Where<POLandedCostTax.docType, Equal<Required<POLandedCostTax.docType>>, And<POLandedCostTax.refNbr, Equal<Required<POLandedCostTax.refNbr>>>>>(Base);

			var taxesGroupedByTaxId = taxesQuery.Select(doc.DocType, doc.RefNbr)
				.AsEnumerable()
				.Join(landedCostSplits, t => ((POLandedCostTax)t).LineNbr, s => s.LineNbr,
					(t, s) => new POLandedCostTax_Tax_Split<TSplit>(t.GetItem<POLandedCostTax>(), t.GetItem<Tax>(), s))
				.Where(t => !string.IsNullOrEmpty(t.Tax.TaxID))
				.GroupBy(t => t.Tax.TaxID)
				.ToDictionary(r => r.Key, r => r.ToList());

			return taxesGroupedByTaxId;
		}

		protected virtual bool IsItemCostTax(Tax tax)
		{
			return tax.ReportExpenseToSingleAccount != true &&
				(tax.TaxType.IsIn(CSTaxType.Sales, CSTaxType.Use) || (tax.TaxType == CSTaxType.VAT && tax.DeductibleVAT == true));
		}

		#endregion
	}
}
