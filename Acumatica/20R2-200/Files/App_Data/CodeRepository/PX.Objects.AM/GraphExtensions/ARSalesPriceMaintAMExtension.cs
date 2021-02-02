using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Manufacturing extension for sales price calculations
    /// </summary>
    public class ARSalesPriceMaintAMExtension : PXGraphExtension<ARSalesPriceMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        /// <summary>
        /// Control if the base call to CalculateSalesPrice should be used
        /// </summary>
        protected bool UseBaseCall;

        /// <summary>
        /// Force call to use base ARSalesPriceMaint.CalculateSalesPriceInt
        /// </summary>
        public static decimal? BaseCalculateSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, CurrencyInfo currencyinfo, string UOM, Decimal? quantity, DateTime date, Decimal? currentUnitPrice)
        {
            return BaseCalculateSalesPrice(sender, custPriceClass, customerID, inventoryID, null, currencyinfo, UOM, quantity, date, currentUnitPrice);
        }

        /// <summary>
        /// Force call to use base ARSalesPriceMaint.CalculateSalesPriceInt using this same signature
        /// </summary>
        public static decimal? BaseCalculateSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, string UOM, Decimal? quantity, DateTime date, Decimal? currentUnitPrice)
        {
            var graphBase = ARSalesPriceMaint.SingleARSalesPriceMaint;
            var graphExt = graphBase.GetExtension<ARSalesPriceMaintAMExtension>();
            try
            {
                graphExt.UseBaseCall = true;
                return graphBase.CalculateSalesPriceInt(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo, UOM, quantity, date, currentUnitPrice);
            }
            finally
            {
                graphExt.UseBaseCall = false;
            }
        }

        [PXOverride]
        public virtual ARSalesPriceMaint.SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? customerID,
            int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date,
            Func<PXCache, string, int?,
                int?, int?, string, string, decimal?, string, DateTime, ARSalesPriceMaint.SalesPriceItem> del)
        {
            // Acumatica changed sales order default price to use FindSalesPrice starting in 18.211
            if (!UseBaseCall && sender.Graph is SOOrderEntry && TryCalculateConfiguredPriceInt((SOOrderEntry)sender.Graph, sender, inventoryID, UOM,
                    ConfigCuryType.Document, out var configResult, out var configuredPrice))
            {
                return new ARSalesPriceMaint.SalesPriceItem(configResult?.UOM, configuredPrice ?? 0m, curyID);
            }

            return del?.Invoke(sender, custPriceClass, customerID, inventoryID, siteID, baseCuryID, curyID, quantity, UOM, date);
        }

        [PXOverride]
        public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, decimal? quantity, string UOM, DateTime date, bool alwaysFromBaseCurrency,
            Func<PXCache, string, int?, int?, int?, CurrencyInfo, decimal?, string, DateTime, bool, decimal?> del)
        {
            decimal? configPrice = null;
            if (!UseBaseCall && TryCalculateConfiguredPrice(sender, inventoryID, currencyinfo, UOM, out configPrice))
            {
                return configPrice;
            }

            return del?.Invoke(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo, quantity, UOM, date, alwaysFromBaseCurrency);
        }

        protected virtual bool TryCalculateConfiguredPrice(PXCache cache, int? inventoryID, CurrencyInfo currencyinfo, string uom, out decimal? configuredPrice)
        {
            configuredPrice = null;
            if (cache.Graph is SOOrderEntry)
            {
                return TryCalculateConfiguredPrice((SOOrderEntry)cache.Graph, cache, inventoryID, currencyinfo, uom, out configuredPrice);
            }
            if (cache.Graph is OpportunityMaint)
            {
                return TryCalculateConfiguredPrice((OpportunityMaint)cache.Graph, cache, inventoryID, currencyinfo, uom, out configuredPrice);
            }
            return false;
        }

        internal virtual bool TryCalculateConfiguredPriceInt(SOOrderEntry graph, PXCache cache, int? inventoryID, string uom, ConfigCuryType configType, out AMConfigurationResults configResult, out decimal? configuredPrice)
        {
            configResult = null;
            configuredPrice = null;
            if (inventoryID == null || graph == null)
            {
                return false;
            }

            var soLine = graph.Transactions.Current;
            if (soLine == null || inventoryID != soLine.InventoryID)
            {
                return false;
            }

            var graphExt = graph.GetExtension<SOOrderEntryAMExtension>();
            if (graphExt == null || !graphExt.AllowConfigurations)
            {
                return false;
            }

            var rowStatus = graph.Transactions.Cache.GetStatus(soLine);
            var soLineExt = soLine.GetExtension<SOLineExt>();
            if (soLineExt == null || !IsConfigurable(graph, rowStatus, soLineExt.AMConfigurationID, soLine.InventoryID, soLine.SiteID))
            {
                return false;
            }

            configResult = PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                        And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
            >.Select(graph, soLine.OrderType, soLine.OrderNbr, soLine.LineNbr);

            if (configResult == null)
            {
                if (rowStatus == PXEntryStatus.Inserted)
                {
                    configuredPrice = 0m;
                    return true;
                }

                return false;
            }

            configuredPrice = AMConfigurationPriceAttribute.GetPriceExt<AMConfigurationResults.displayPrice>(graphExt.ItemConfiguration.Cache, configResult, configType);

            return configuredPrice != null;
        }

        protected virtual bool TryCalculateConfiguredPrice(SOOrderEntry graph, PXCache cache, int? inventoryID,
            CurrencyInfo currencyinfo, string uom, out decimal? configuredPrice)
        {
            configuredPrice = 0m;
            var result = TryCalculateConfiguredPriceInt(graph, cache, inventoryID, uom, ConfigCuryType.Document,
                out var configResult, out var configBasePrice);

            configuredPrice = ConvertToCuryUomPrice(cache, inventoryID, currencyinfo, uom, configResult, configBasePrice);

            return result && configuredPrice != null;
        }

        protected virtual bool TryCalculateConfiguredPrice(OpportunityMaint graph, PXCache cache, int? inventoryID, CurrencyInfo currencyinfo, string uom, out decimal? configuredPrice)
        {
            configuredPrice = null;
            if (inventoryID == null || graph == null || currencyinfo == null)
            {
                return false;
            }

            var product = graph.Products.Current;
            if (product == null || inventoryID != product.InventoryID)
            {
                return false;
            }

            var rowStatus = graph.Products.Cache.GetStatus(product);
            var productExt = product.GetExtension<CROpportunityProductsExt>();
            if (productExt == null || !IsConfigurable(graph, rowStatus, productExt.AMConfigurationID, product.InventoryID, product.SiteID))
            {
                return false;
            }

            var graphExt = graph.GetExtension<OpportunityMaintAMExtension>();
            if (graphExt == null || !graphExt.AllowConfigurations)
            {
                return false;
            }

            AMConfigurationResults configResult = PXSelect<AMConfigurationResults,
            	Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>,
            		And<AMConfigurationResults.opportunityLineNbr, Equal<Required<AMConfigurationResults.opportunityLineNbr>>>>>
            	.Select(graph, product.QuoteID, product.LineNbr);

            if (configResult == null)
            {
                if (rowStatus == PXEntryStatus.Inserted)
                {
                    configuredPrice = 0m;
                    return true;
                }
                return false;
            }

            configuredPrice = ConvertToCuryUomPrice(cache, inventoryID, currencyinfo, uom, configResult,
                AMConfigurationPriceAttribute.GetPriceExt<AMConfigurationResults.displayPrice>(graphExt.ItemConfiguration.Cache, configResult, ConfigCuryType.Document));

            return configuredPrice != null;
        }

        protected virtual decimal? ConvertToCuryUomPrice(PXCache cache, int? inventoryID, CurrencyInfo currencyinfo,
            string uom, AMConfigurationResults configResult, decimal? configuredPrice)
        {
            if (configuredPrice == null || configResult == null)
            {
                return null;
            }
                                          
            var salesPrice = configuredPrice.GetValueOrDefault();

            if (currencyinfo != null && configResult.CuryID != currencyinfo.CuryID)
            {
                if (currencyinfo.CuryRate == null)
                {
                    throw new PXSetPropertyException(PX.Objects.CM.Messages.RateNotFound, PXErrorLevel.Warning);
                }
                PXCurrencyAttribute.CuryConvCury(cache, currencyinfo, configuredPrice.GetValueOrDefault(), out salesPrice);
            }

            if (uom == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(configResult.UOM) && configResult.UOM != uom)
            {
                try
                {
                    var salesPriceInBase = INUnitAttribute.ConvertFromBase(cache, inventoryID, configResult.UOM, salesPrice, INPrecision.UNITCOST);
                    salesPrice = INUnitAttribute.ConvertToBase(cache, inventoryID, uom, salesPriceInBase, INPrecision.UNITCOST);
                }
                catch (PXUnitConversionException)
                {
                    return salesPrice;
                }
            }
                                          
            return salesPrice;
        }

        protected static bool IsConfigurable(PXGraph graph, PXEntryStatus rowEntryStatus, string configurationID, int? inventoryID, int? siteID)
        {
            // The calls for price are getting called before the Configuration ID can get set so lets see if its null (not yet set)
            var cid = configurationID == null && rowEntryStatus == PXEntryStatus.Inserted && inventoryID != null
                ? ConfigurationIDManager.GetID(graph, inventoryID, siteID)
                : configurationID;

            return !string.IsNullOrWhiteSpace(cid);
        }
    }
}