using System;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CS;
using PX.Objects.TX;


namespace PX.Objects.PO
{
	/// <summary>
	/// Specialized for <see cref="FeaturesSet.Retainage"/> feature version of the POTaxAttribute. <br/>
	/// Provides Tax calculation for <see cref="POLine.CuryRetainageAmt"/> amount in the line, by default is attached 
	/// to <see cref="POLine"/> (details) and <see cref="POOrder"/> (header). <br/>
	/// Normally, should be placed on the <see cref="POLine.TaxCategoryID"/> field. <br/>
	/// As a result of this attribute work a set of <see cref="POTax"/> tran related to each line and to their parent will be created <br/>
	/// </summary>
	public class PORetainedTaxAttribute : POTaxAttribute
	{
		protected Type CuryRetainageTotal = typeof(POOrder.curyRetainageTotal);
		protected Type CuryOrderTotal = typeof(POOrder.curyOrderTotal);

		protected override short SortOrder
		{
			get
			{
				return 2;
			}
		}

		public PORetainedTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			Init();
		}

		private readonly HashSet<string> allowedParentFields = new HashSet<string>();

		private void Init()
		{
			#region Line fields

			CuryTranAmt = typeof(POLine.curyRetainageAmt);
			GroupDiscountRate = typeof(POLine.groupDiscountRate);

			#endregion

			#region Parent fields

			CuryLineTotal = typeof(POOrder.curyLineRetainageTotal);
			CuryTaxTotal = typeof(POOrder.curyRetainedTaxTotal);
			CuryDiscTot = typeof(POOrder.curyRetainedDiscTotal);
			CuryDocBal = typeof(POOrder.curyRetainageTotal);

			allowedParentFields.Add(_CuryLineTotal);
			allowedParentFields.Add(_CuryTaxTotal);
			allowedParentFields.Add(_CuryDocBal);

			#endregion

			_CuryOrigTaxableAmt = string.Empty;
			_CuryTaxableAmt = typeof(POTax.curyRetainedTaxableAmt).Name;
			_CuryTaxAmt = typeof(POTax.curyRetainedTaxAmt).Name;

			_Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(POLine.curyRetainageAmt), typeof(SumCalc<POOrder.curyLineRetainageTotal>)));
		}

		protected override List<object> SelectTaxes<WhereType>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return
				IsRetainedTaxes(graph)
					? base.SelectTaxes<WhereType>(graph, row, taxchk, parameters)
					: new List<object>();
		}

		protected override void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			if (IsRetainedTaxes(sender.Graph))
			{
				base.DefaultTaxes(sender, row, DefaultExisting);
			}
		}

		protected override bool IsTaxRowAmountUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			// This method should be overriden
			// to switch on a tax calculation on 
			// Tax_RowUpdated event in the case 
			// when retainage amount have been changed.
			//
			return
				base.IsTaxRowAmountUpdated(sender, e) ||
				!sender.ObjectsEqual<POTax.curyRetainedTaxAmt>(e.Row, e.OldRow);
		}

		protected override bool IsDeductibleVATTax(Tax tax)
		{
			// We shouldn't affect retainage tax amount
			// in this case, because it will be separated
			// further in the child retainage Bill.
			//
			return false;
		}

		protected override void ParentSetValue(PXGraph graph, string fieldname, object value)
		{
			// Retained taxes should affect
			// only parent fields described in the
			// current implementation of the
			// Init() method.
			//
			if (allowedParentFields.Contains(fieldname))
			{
				base.ParentSetValue(graph, fieldname, value);
			}
		}

		protected override void ReDefaultTaxes(PXCache cache, List<object> details) { }

		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true) { }

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType) { }

		protected override void SetTaxableAmt(PXCache sender, object row, decimal? value) { }

		protected override void SetTaxAmt(PXCache sender, object row, decimal? value) { }

		protected override decimal? GetTaxableAmt(PXCache sender, object row)
		{
			return 0m;
		}

		protected override decimal? GetTaxAmt(PXCache sender, object row)
		{
			return 0m;
		}

		protected override void _CalcDocTotals(PXCache sender, object row, decimal curyTaxTotal, decimal curyInclTaxTotal, decimal curyWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			base._CalcDocTotals(sender, row, curyTaxTotal, curyInclTaxTotal, curyWhTaxTotal, CuryTaxDiscountTotal);

			// the base attribute POTaxAttribute calculates Order Total without Retainage
			decimal curyRetainageTotal = (decimal)(ParentGetValue(sender.Graph, CuryRetainageTotal.Name) ?? 0m);
			if (curyRetainageTotal != 0m)
			{
				decimal docCuryDocBal = (decimal)(ParentGetValue(sender.Graph, CuryOrderTotal.Name) ?? 0m);
				base.ParentSetValue(sender.Graph, CuryOrderTotal.Name, docCuryDocBal + curyRetainageTotal);
			}
		}

		/// <summary>
		/// Fill tax details for line for per unit taxes. Do nothing for retained tax.
		/// </summary>
		protected override void TaxSetLineDefaultForPerUnitTaxes(PXCache rowCache, object row, Tax tax, TaxRev taxRevision, TaxDetail taxDetail)
		{
		}

		/// <summary>
		/// Fill aggregated tax detail for per unit tax. Do nothing for retained tax.
		/// </summary>
		protected override TaxDetail FillAggregatedTaxDetailForPerUnitTax(PXCache rowCache, object row, Tax tax, TaxRev taxRevision,
																		  TaxDetail aggrTaxDetail, List<object> taxItems)
		{
			return aggrTaxDetail;
		}
	}
}
