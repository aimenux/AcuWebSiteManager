using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.TX;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public partial class ARTaxRUTROT : PXCacheExtension<ARTax>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region CuryRUTROTTaxAmt
		public abstract class curyRUTROTTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRUTROTTaxAmt> { }

		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="ARTran">lines</see> 
		/// in selected currency.
		/// </summary>
		/// <remarks>
		/// This field has no representation in a database.
		/// </remarks>
		[PXCurrency(typeof(ARTax.curyInfoID), typeof(rUTROTTaxAmt))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Selector<ARTax.taxID, Tax.taxCalcLevel>, NotEqual<CSTaxCalcLevel.inclusive>>, ARTax.curyTaxAmt>, CS.decimal0>),
				   typeof(SumCalc<ARTranRUTROT.curyRUTROTTaxAmountDeductible>), FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? CuryRUTROTTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region RUTROTTaxAmt
		public abstract class rUTROTTaxAmt : PX.Data.BQL.BqlDecimal.Field<rUTROTTaxAmt> { }
		
		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="ARTran">line</see> in the base currency.
		/// </summary>
		[PXBaseCury]
		public virtual decimal? RUTROTTaxAmt
		{
			get;
			set;
		}
		#endregion
	}
}
