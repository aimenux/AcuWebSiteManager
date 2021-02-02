using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	/// <summary>
	/// A comparer of taxes by tax calculation level
	/// </summary>
	public class TaxByCalculationLevelComparer : IComparer<Tax>
	{
		public static readonly TaxByCalculationLevelComparer Instance = new TaxByCalculationLevelComparer();

		protected TaxByCalculationLevelComparer() { }

		public int Compare(Tax taxX, Tax taxY)
		{
			if (taxX == null && taxY == null)
				return 0;
			else if (taxX == null)
				return -1;
			else if (taxY == null)
				return 1;
			else
				return CompareTaxesWithPerUnitTaxSupport(taxX, taxY);
		}

		protected virtual int CompareTaxesWithPerUnitTaxSupport(Tax taxX, Tax taxY)
		{
			// First compare taxes for per unit tax support in the way that per unit taxes always placed before all taxes of other types regardless of calculation level.
			// Taxes with different types are considered equal
			int taxTypePerUnitCompareResult = CompareTaxesForPerUnitOrdering(taxX, taxY);	

			if (taxTypePerUnitCompareResult != 0)
			{
				return taxTypePerUnitCompareResult; //One of the taxes is a per unit tax. Comparison is finished.
			}

			int compareTaxesByCalculationLevelResult = CompareTaxesByCalculationLevels(taxX, taxY);	
			return compareTaxesByCalculationLevelResult != 0
				? compareTaxesByCalculationLevelResult
				: CompareTaxesWithTheSameCalculationLevel(taxX, taxY);	
		}

		/// <summary>
		/// Tax comparison for per unit taxes ordering. Places Per Unit taxes first, consider other tax types to be equal.
		/// </summary>
		/// <param name="taxX">The tax x.</param>
		/// <param name="taxY">The tax y.</param>
		/// <returns/>
		protected virtual int CompareTaxesForPerUnitOrdering(Tax taxX, Tax taxY)
		{
			bool isFirstTaxPerUnit = taxX.TaxType == CSTaxType.PerUnit;
			bool isSecondTaxPerUnit = taxY.TaxType == CSTaxType.PerUnit;

			if (isFirstTaxPerUnit && isSecondTaxPerUnit)
				return 0;
			else if (isFirstTaxPerUnit)
				return -1;                  //Per unit taxes should come first
			else if (isSecondTaxPerUnit)
				return 1;                   //Non per unit taxes should come after per unit taxes
			else
				return 0;                   //Non per unit taxes are considered equal
		}

		protected virtual int CompareTaxesByCalculationLevels(Tax taxX, Tax taxY) =>
			string.Compare(taxX.TaxCalcLevel, taxY.TaxCalcLevel);

		protected virtual int CompareTaxesWithTheSameCalculationLevel(Tax taxX, Tax taxY) =>
			CompareTaxesByTaxTypes(taxX, taxY);

		/// <summary>
		/// Tax comparison by tax type. Default implementation considers taxes of all tax types to be equal.
		/// </summary>
		/// <param name="taxX">The tax x.</param>
		/// <param name="taxY">The tax y.</param>
		/// <returns/>
		protected virtual int CompareTaxesByTaxTypes(Tax taxX, Tax taxY) => 0;
	}
}
