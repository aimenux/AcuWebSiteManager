using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.TaxProvider;

namespace PX.Objects.TX
{
	public abstract class ExternalTaxBase<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public static bool IsExternalTax(PXGraph graph, string taxZoneID)
		{
			if (string.IsNullOrEmpty(taxZoneID) || !PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>())
				return false;

			TX.TaxZone tz = PXSelect<TX.TaxZone, Where<TX.TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(graph, taxZoneID);
			if (tz != null)
				return tz.IsExternal.GetValueOrDefault(false);
			else
				return false;
		}

		public static bool IsEmptyAddress(IAddressBase address)
		{
			return string.IsNullOrEmpty(address?.PostalCode);
		}

		public static string CompanyCodeFromBranch(PXGraph graph, string taxZoneID, int? branchID)
		{
			TaxPluginMapping m = PXSelectJoin<TaxPluginMapping,
				InnerJoin<TaxZone, On<TaxPluginMapping.taxPluginID, Equal<TaxZone.taxPluginID>>>,
				Where<TaxPluginMapping.branchID, Equal<Required<TaxPluginMapping.branchID>>,
					And<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>>
				.Select(graph, branchID, taxZoneID);
			if (m == null)
			{
				TaxPlugin taxPlugin = PXSelectJoin<TaxPlugin,
					InnerJoin<TaxZone, On<TaxPlugin.taxPluginID, Equal<TaxZone.taxPluginID>>>,
					Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>
					.Select(graph, taxZoneID);
				if (taxPlugin == null)
				{
					throw new PXSetPropertyException(Messages.ExternalTaxProviderNotConfigured);
				}

				throw new PXException(Messages.ExternalTaxProviderBranchToCompanyCodeMappingIsMissing);
			}

			return m.CompanyCode;
		}

		public static Func<PXGraph, string, ITaxProvider> TaxProviderFactory = (PXGraph graph, string taxZoneID) =>
		{
			TX.TaxZone tz =
				PXSelect<TX.TaxZone, Where<TX.TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(graph, taxZoneID);

			if (tz.IsExternal == true && PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>() &&
			    !String.IsNullOrEmpty(tz.TaxPluginID))
			{
				var provider = TaxPluginMaint.CreateTaxProvider(graph, tz.TaxPluginID);

				return provider;
			}

			return null;
		};

		public static string GetTaxID(PX.TaxProvider.TaxDetail taxDetail)
		{
			return string.Format("{0} {1:G6}", taxDetail.TaxName.ToUpperInvariant(), taxDetail.Rate * 100);
		}
	}

	public abstract class ExternalTaxBase<TGraph, TPrimary> : ExternalTaxBase<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		protected bool skipExternalTaxCalcOnSave = false;

		[PXOverride]
		public virtual bool IsExternalTax(string taxZoneID)
        {
	        return IsExternalTax(Base, taxZoneID);
        }

		public virtual bool IsNonTaxable(IAddressBase address)
		{
			PXSelectBase<State> select = new PXSelect
				<State, Where<State.countryID, Equal<Required<State.countryID>>, And<State.stateID, Equal<Required<State.stateID>>>>>(Base);

			var state = select.SelectSingle(address.CountryID, address.State);
			return state?.NonTaxable == true;
		}

		public virtual string CompanyCodeFromBranch(string taxZoneID, int? branchID)
		{
			return CompanyCodeFromBranch(Base, taxZoneID, branchID);
		}

		[PXOverride]
		public abstract TPrimary CalculateExternalTax(TPrimary document);

		protected virtual void LogMessages(ResultBase result)
		{
			foreach (var msg in result.Messages)
			{
				PXTrace.WriteError(msg);
			}
		}

		public abstract void SkipTaxCalcAndSave();

		protected virtual decimal? GetDocDiscount()
		{
			return null;
		}

		protected virtual string GetExternalTaxProviderLocationCode(TPrimary order)
		{
			return null;
		}

		protected string GetExternalTaxProviderLocationCode<TLine, TLineDocFK, TLineSiteID>(TPrimary document)
			where TLine : class, IBqlTable, new()
			where TLineDocFK : IParameterizedForeignKeyBetween<TLine, TPrimary>, new()
			where TLineSiteID : IBqlField
		{
			TLine lineWithSite = PXSelect<TLine, Where2<TLineDocFK, And<TLineSiteID, IsNotNull>>>.SelectSingleBound(Base, new[] { document });

			if (lineWithSite == null)
				return null;

			var site = PX.Objects.IN.INSite.PK.Find(Base, (int?)Base.Caches<TLine>().GetValue<TLineSiteID>(lineWithSite));
			return site?.SiteCD;
		}
		
        protected virtual void CreateTax(TGraph graph, TaxZone taxZone, AP.Vendor taxAgency, TaxProvider.TaxDetail taxDetail, string taxID)
        {
            Tax tx = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(graph, taxID);
            if (tx == null)
            {
                tx = new Tax
                {
                    TaxID = taxID,
                    Descr = PXMessages.LocalizeFormatNoPrefixNLA(TX.Messages.ExternalTaxProviderTaxFor, taxDetail.JurisType, taxDetail.JurisName),
                    TaxType = CSTaxType.Sales,
                    TaxCalcType = CSTaxCalcType.Doc,
                    TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt,
                    TaxApplyTermsDisc = CSTaxTermsDiscount.ToTaxableAmount,
                    SalesTaxAcctID = taxAgency.SalesTaxAcctID,
                    SalesTaxSubID = taxAgency.SalesTaxSubID,
                    ExpenseAccountID = taxAgency.TaxExpenseAcctID,
                    ExpenseSubID = taxAgency.TaxExpenseSubID,
                    TaxVendorID = taxZone.TaxVendorID,
                    IsExternal = true
                };
                PXDBLocalizableStringAttribute.SetTranslationsFromMessageFormatNLA<Tax.descr>(graph.Caches[typeof(Tax)], tx, TX.Messages.ExternalTaxProviderTaxFor, new string[] { taxDetail.JurisType, taxDetail.JurisName });
                graph.Caches[typeof(Tax)].Insert(tx);
            }
        }
	}

	public abstract class ExternalTax<TGraph, TPrimary> : ExternalTaxBase<TGraph, TPrimary>
		where TGraph : PXGraph<TGraph, TPrimary>
		where TPrimary : class, IBqlTable, new()
	{
		public override void SkipTaxCalcAndSave()
		{
			try
			{
				skipExternalTaxCalcOnSave = true;
				Base.Save.Press();
			}
			finally
			{
				skipExternalTaxCalcOnSave = false;
			}
		}
	}

	sealed class ExternalTax : ExternalTaxBase<PXGraph>
	{

	}
}
