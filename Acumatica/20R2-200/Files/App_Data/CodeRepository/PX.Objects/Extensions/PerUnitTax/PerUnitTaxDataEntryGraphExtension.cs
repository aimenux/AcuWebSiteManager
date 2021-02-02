using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;
using PX.Objects.CS;
using PX.Objects.TX;

namespace PX.Objects.Extensions.PerUnitTax
{
	/// <summary>
	/// A per-unit tax graph extension for data entry graphs which will forbid edit of per-unit taxes in UI.
	/// </summary>
	public abstract class PerUnitTaxDataEntryGraphExtension<TGraph, TaxDetailDAC> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	where TaxDetailDAC : TaxDetail, IBqlTable, new()
	{
		protected static bool IsActiveBase() => PXAccess.FeatureInstalled<FeaturesSet.perUnitTaxSupport>();

		protected virtual void _(Events.RowSelected<TaxDetailDAC> e)
		{
			if (e.Row == null)
				return;

			const string fieldName = nameof(TaxDetail.TaxID);
			var uiFieldAttribute = e.Cache.GetAttributesOfType<PXUIFieldAttribute>(e.Row, fieldName)
										  .FirstOrDefault();

			if (uiFieldAttribute == null || uiFieldAttribute.Enabled == false)
				return;

			Tax tax = GetTax(e.Row);
			ConfigureUI(e.Cache, e.Row, tax);		
		}

		protected virtual void TaxDetailDAC_RowPersisting(Events.RowPersisting<TaxDetailDAC> e)
		{
			if (!(e.Row is TaxTran))
				return;

			Tax tax = GetTax(e.Row);

			if (tax != null)
			{
				ConfigureChecks(e.Cache, e.Row, tax);
			}		
		}

		/// <summary>
		/// Gets a tax from <see cref="TaxDetail"/>.
		/// </summary>
		/// <param name="taxDetail">The taxDetail to act on.</param>
		/// <returns/>
		private Tax GetTax(TaxDetailDAC taxDetail)
		{
			if (taxDetail == null)
				return null;

			return PXSelect<Tax,
					  Where<Tax.taxID, Equal<Required<Tax.taxID>>>>
				  .SelectSingleBound(Base, currents: null, pars: taxDetail.TaxID);
		}

		protected virtual void ConfigureUI(PXCache cache, TaxDetailDAC taxDetail, Tax tax)
		{
			bool isPerUnitTax = tax?.TaxType == CSTaxType.PerUnit;
			PXUIFieldAttribute.SetEnabled(cache, taxDetail, !isPerUnitTax);
		}

		protected virtual void ConfigureChecks(PXCache cache, TaxDetailDAC taxDetail, Tax tax)
		{
			bool areAccountAndSubaccountRequired = tax.TaxType != CSTaxType.PerUnit || tax.PerUnitTaxPostMode == PerUnitTaxPostOptions.TaxAccount;
			PXPersistingCheck persistingCheck = areAccountAndSubaccountRequired
				? PXPersistingCheck.NullOrBlank
				: PXPersistingCheck.Nothing;

			cache.Adjust<PXDefaultAttribute>(taxDetail)
				 .For<TaxTran.accountID>(a => a.PersistingCheck = persistingCheck)
				 .SameFor<TaxTran.subID>();
		}
	}
}
