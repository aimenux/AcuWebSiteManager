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
	/// A comparer of taxes by tax calculation level and by tax type for taxes with the same calculation level
	/// </summary>
	public class TaxByCalculationLevelAndTypeComparer : TaxByCalculationLevelComparer
	{
		public static new readonly TaxByCalculationLevelAndTypeComparer Instance = new TaxByCalculationLevelAndTypeComparer();

		protected TaxByCalculationLevelAndTypeComparer() : base() { }

		/// <summary>
		/// Tax comparison by tax type. Compares tax types via <see cref="string.Compare(string, string)"/>.
		/// </summary>
		/// <param name="taxX">The tax x.</param>
		/// <param name="taxY">The tax y.</param>
		/// <returns/>
		protected override int CompareTaxesByTaxTypes(Tax taxX, Tax taxY) => 
			string.Compare(taxX.TaxType, taxY.TaxType);
	}
}
