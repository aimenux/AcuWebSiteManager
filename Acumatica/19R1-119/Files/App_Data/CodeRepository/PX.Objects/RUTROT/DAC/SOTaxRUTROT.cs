using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.SO;
using PX.Objects.TX;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public partial class SOTaxRUTROT : PXCacheExtension<SOTax>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region CuryRUTROTTaxAmt
		public abstract class curyRUTROTTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRUTROTTaxAmt> { }
		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="SOLine">lines</see> in the selected currency.
		/// </summary>
		[PXCurrency(typeof(SOTax.curyInfoID), typeof(rUTROTTaxAmt))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Selector<SOTax.taxID, Tax.taxCalcLevel>, NotEqual<CSTaxCalcLevel.inclusive>>, SOTax.curyTaxAmt>, CS.decimal0>),
				   typeof(SumCalc<SOLineRUTROT.curyRUTROTTaxAmountDeductible>), FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? CuryRUTROTTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region RUTROTTaxAmt
		public abstract class rUTROTTaxAmt : PX.Data.BQL.BqlDecimal.Field<rUTROTTaxAmt> { }
		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="SOLine">line</see> in the base currency.
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
