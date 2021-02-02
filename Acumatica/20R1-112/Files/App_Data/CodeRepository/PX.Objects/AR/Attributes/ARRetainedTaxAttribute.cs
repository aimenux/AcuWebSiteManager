using System;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CS;
using PX.Objects.TX;


namespace PX.Objects.AR
{
	/// <summary>
	/// Specialized for <see cref="FeaturesSet.Retainage"/> feature version of the ARTaxAttribute. <br/>
	/// Provides Tax calculation for <see cref="ARTran.CuryRetainageAmt"/> amount in the line, by default is attached 
	/// to <see cref="ARTran"/> (details) and <see cref="ARInvoice"/> (header). <br/>
	/// Normally, should be placed on the <see cref="ARTran.TaxCategoryID"/> field. <br/>
	/// Internally, it uses <see cref="ARInvoiceEntry"/> graph, otherwise taxes will not be calculated 
	/// (<see cref="Tax.TaxCalcLevel"/> is set to <see cref="TaxCalc.NoCalc"/>).<br/>
	/// As a result of this attribute work a set of <see cref="ARTax"/> tran related to each line and to their parent will be created <br/>
	/// <example>
	/// [ARRetainedTaxAttribute(typeof(ARRegister), typeof(ARTax), typeof(ARTaxTran))]
	/// </example>
	/// </summary>
	public class ARRetainedTaxAttribute : ARTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 1;
			}
		}

		public ARRetainedTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			Init();
		}

		private readonly HashSet<string> allowedParentFields = new HashSet<string>();

		private void Init()
		{
			#region Line fields

			CuryTranAmt = typeof(ARTran.curyRetainageAmt);
			GroupDiscountRate = typeof(ARTran.groupDiscountRate);

			#endregion

			#region Parent fields

			CuryLineTotal = typeof(ARInvoice.curyLineRetainageTotal);
			CuryTaxTotal = typeof(ARInvoice.curyRetainedTaxTotal);
			CuryDiscTot = typeof(ARInvoice.curyRetainedDiscTotal);
			CuryDocBal = typeof(ARInvoice.curyRetainageTotal);

			allowedParentFields.Add(_CuryLineTotal);
			allowedParentFields.Add(_CuryTaxTotal);
			allowedParentFields.Add(_CuryDiscTot);
			allowedParentFields.Add(_CuryDocBal);

			#endregion

			_CuryOrigTaxableAmt = string.Empty;
			_CuryTaxableAmt = typeof(ARTax.curyRetainedTaxableAmt).Name;
			_CuryTaxAmt = typeof(ARTax.curyRetainedTaxAmt).Name;
			_CuryTaxAmtSumm = typeof(TaxTran.curyRetainedTaxAmtSumm).Name;

			_Attributes.Clear();
			_Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>>, ARTran.curyRetainageAmt>, decimal0>),
				typeof(SumCalc<ARInvoice.curyLineRetainageTotal>)));
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
				!sender.ObjectsEqual<ARTax.curyRetainedTaxAmt>(e.Row, e.OldRow);
		}

		protected override bool IsDeductibleVATTax(Tax tax)
		{
			// We shouldn't affect retainage tax amount
			// in this case, because it will be split
			// further in the child retainage Invoice.
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

		protected override bool IsRoundingNeeded(PXGraph graph)
		{
			// We shouldn't affect CuryRoundDiff value
			// by the ARetainedTax attribute.
			//
			return false;
		}

		protected override void ResetRoundingDiff(PXGraph graph) { }

		protected override void ReDefaultTaxes(PXCache cache, List<object> details) { }

		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true) { }

		protected override void SetExtCostExt(PXCache sender, object child, decimal? value) { }

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

		#region Per Unit Taxes Override Stubs
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
		#endregion
	}
}