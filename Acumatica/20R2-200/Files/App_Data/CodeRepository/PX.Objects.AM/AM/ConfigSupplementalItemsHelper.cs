using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.AM
{
    public static class ConfigSupplementalItemsHelper
    {

        public static void AddSupplementalLineItems(SOLine parentConfigSOLine, SOOrderEntry soOrderEntryGraph, AMConfigurationResults configResult)
        {
            AddSupplementalLineItems(parentConfigSOLine, soOrderEntryGraph, GetSupplementalOptions(soOrderEntryGraph, configResult));
        }

        public static void AddSupplementalLineItems(SOLine parentConfigSOLine, SOOrderEntry soOrderEntryGraph, PXResultset<AMConfigResultsOption> configResultOptions)
        {
            if (parentConfigSOLine == null)
            {
                throw new PXArgumentException(nameof(parentConfigSOLine));
            }

            int sortOrder = parentConfigSOLine.SortOrder.GetValueOrDefault();
            foreach (PXResult<AMConfigResultsOption, AMConfigurationOption> result in configResultOptions)
            {
                var amConfigResultsOption = (AMConfigResultsOption)result;
                var amConfigurationOption = (AMConfigurationOption)result;

                if (amConfigResultsOption == null || amConfigurationOption == null)
                {
                    continue;
                }

                SOLine newLine = soOrderEntryGraph.Transactions.Insert(new SOLine());

                newLine.SortOrder = ++sortOrder;
                newLine.CustomerID = parentConfigSOLine.CustomerID;
                newLine.InventoryID = amConfigResultsOption.InventoryID;
                newLine.UOM = amConfigResultsOption.UOM;
                newLine.OrderQty = amConfigResultsOption.Qty.GetValueOrDefault() * parentConfigSOLine.OrderQty.GetValueOrDefault();
                newLine.TranDesc = amConfigurationOption.Descr;
                newLine.RequestDate = parentConfigSOLine.RequestDate;
                newLine.ShipComplete = parentConfigSOLine.ShipComplete;
                newLine.SiteID = amConfigurationOption.SiteID ?? parentConfigSOLine.SiteID;
                newLine.BranchID = parentConfigSOLine.BranchID;
                newLine.ProjectID = parentConfigSOLine.ProjectID;
                newLine.TaskID = parentConfigSOLine.TaskID;
                newLine.ShipComplete = parentConfigSOLine.ShipComplete;

                var newExt = PXCache<SOLine>.GetExtension<SOLineExt>(newLine);
                newExt.AMParentLineNbr = parentConfigSOLine.LineNbr;
                newExt.AMIsSupplemental = true;
                newExt.AMOrigParentLineNbr = null; //For some reason this is getting a value when it should be null; force null for sups

                soOrderEntryGraph.Transactions.Update(newLine);
            }

            ResetSalesOrderLinesSortOrder(parentConfigSOLine, soOrderEntryGraph, sortOrder);
        }

        public static void AddSupplementalProductLineItems(CROpportunityProducts parentConfigProducts, PXGraph graph, AMConfigurationResults configResult)
        {
            AddSupplementalProductLineItems(parentConfigProducts, graph, GetSupplementalOptions(graph, configResult));
        }

        public static void AddSupplementalProductLineItems(CROpportunityProducts parentConfigProducts, PXGraph graph, PXResultset<AMConfigResultsOption> configResultOptions)
        {
            if (parentConfigProducts == null)
            {
                throw new PXArgumentException(nameof(parentConfigProducts));
            }

            if (graph == null)
            {
                return;
            }

            foreach (PXResult<AMConfigResultsOption, AMConfigurationOption> result in configResultOptions)
            {
                var amConfigResultsOption = (AMConfigResultsOption)result;
                var amConfigurationOption = (AMConfigurationOption)result;

                if (amConfigResultsOption == null || amConfigurationOption == null)
                {
                    continue;
                }

                var newLine = (CROpportunityProducts)graph.Caches<CROpportunityProducts>().Insert(new CROpportunityProducts { InventoryID = amConfigResultsOption.InventoryID });
                newLine.UOM = amConfigResultsOption.UOM;
                newLine.Quantity = amConfigResultsOption.Qty.GetValueOrDefault() * parentConfigProducts.Quantity.GetValueOrDefault();
                newLine.Descr = amConfigurationOption.Descr;
                newLine.SiteID = amConfigurationOption.SiteID ?? parentConfigProducts.SiteID;

                var newExt = newLine.GetExtension<CROpportunityProductsExt>();
                newExt.AMParentLineNbr = parentConfigProducts.LineNbr;
                newExt.AMIsSupplemental = true;

                graph.Caches<CROpportunityProducts>().Update(newLine);
            }
        }

        public static PXResultset<AMConfigResultsOption> GetSupplementalOptions(PXGraph graph, AMConfigurationResults configResult)
        {
            return PXSelectJoin<AMConfigResultsOption,
                InnerJoin<AMConfigurationOption, On<AMConfigResultsOption.configurationID, Equal<AMConfigurationOption.configurationID>,
                    And<AMConfigResultsOption.revision, Equal<AMConfigurationOption.revision>,
                        And<AMConfigResultsOption.featureLineNbr, Equal<AMConfigurationOption.configFeatureLineNbr>,
                            And<AMConfigResultsOption.optionLineNbr, Equal<AMConfigurationOption.lineNbr>>>>>>,
                Where<AMConfigResultsOption.configurationID, Equal<Required<AMConfigResultsOption.configurationID>>,
                    And<AMConfigResultsOption.configResultsID, Equal<Required<AMConfigResultsOption.configResultsID>>,
                        And<AMConfigurationOption.materialType, Equal<AMMaterialType.supplemental>,
                            And<AMConfigResultsOption.included, Equal<True>>>
                    >>>.Select(graph, configResult?.ConfigurationID, configResult?.ConfigResultsID);
        }

        public static List<SOLine> GetInsertedSupplementalLinesByParent(SOOrderEntry graph, int? parentLineNbr)
        {
            var list = new List<SOLine>();
            if (parentLineNbr.GetValueOrDefault() == 0)
            {
                return list;
            }
            foreach (SOLine cachedLine in graph.Transactions.Cache.Inserted)
            {
                var cachedLineExt = cachedLine.GetExtension<SOLineExt>();
                if (cachedLineExt == null)
                {
                    continue;
                }
                if(cachedLineExt.AMIsSupplemental.GetValueOrDefault() && cachedLineExt.AMParentLineNbr.GetValueOrDefault() == parentLineNbr)
                {
                    list.Add(cachedLine);
                }
            }
            return list;
        }

        /// <summary>
        /// Reset the sales lines sort order to fit in the new supplemental lines after the parent
        /// </summary>
        private static void ResetSalesOrderLinesSortOrder(SOLine parentConfigSOLine, SOOrderEntry soOrderEntryGraph, int currentSortOrder)
        {
            int sortOrder = currentSortOrder;
            foreach (SOLine soLine in soOrderEntryGraph.Transactions.Select())
            {
                if (soLine.SortOrder.GetValueOrDefault() <= parentConfigSOLine.SortOrder.GetValueOrDefault()
                    || IsChildLine(parentConfigSOLine, soLine))
                {
                    continue;
                }

                if (soLine.SortOrder.GetValueOrDefault() <= sortOrder)
                {
                    soLine.SortOrder = ++sortOrder;
                    soOrderEntryGraph.Transactions.Update(soLine);
                }
            }
        }

        public static bool IsChildLine(SOLine parentLine, SOLine childLine)
        {
            var childExt = childLine.GetExtension<SOLineExt>();
            if (childExt?.AMParentLineNbr == null || parentLine == null)
            {
                return false;
            }

            return parentLine.LineNbr.GetValueOrDefault() == childExt.AMParentLineNbr.GetValueOrDefault()
                   && parentLine.LineNbr.GetValueOrDefault() != childLine.LineNbr.GetValueOrDefault();
        }

        public static bool RemoveSupplementalLineItems(PXGraph graph, AMConfigurationResults configResult)
        {
            if (configResult == null)
            {
                return false;
            }

            if (configResult.IsSalesReferenced.GetValueOrDefault())
            {
                return graph is SOOrderEntry ?
                    RemoveSOSupplementalLineItems((SOOrderEntry)graph, configResult) :
                    RemoveSOSupplementalLineItems(configResult) ;
            }

            if (configResult.IsOpportunityReferenced.GetValueOrDefault())
            {
                RemoveProductSupplementalLineItems(graph, configResult); 
            }

            return false;
        }

        /// <summary>
        /// Remove sales order related supplemental line items specific to given configuration result
        /// </summary>
        /// <param name="configResult">Configuration to remove related supplemental sales lines</param>
        /// <returns>True if updates made</returns>
        public static bool RemoveSOSupplementalLineItems(AMConfigurationResults configResult)
        {
            if (configResult == null)
            {
                return false;
            }

            var soOrderEntryGraph = PXGraph.CreateInstance<SOOrderEntry>();
            soOrderEntryGraph.Document.Current = soOrderEntryGraph.Document.Search<SOOrder.orderNbr>(configResult.OrdNbrRef, configResult.OrdTypeRef);

            var containsChanges = RemoveSOSupplementalLineItems(soOrderEntryGraph, configResult);

            if (containsChanges)
            {
                soOrderEntryGraph.Actions.PressSave();
            }

            return containsChanges;
        }

        /// <summary>
        /// Remove sales order related supplemental line items specific to given configuration result
        /// </summary>
        /// <param name="soOrderEntryGraph">Sales Order Entry Graph</param>
        /// <param name="configResult">Configuration to remove related supplemental sales lines</param>
        /// <returns>True if updates made</returns>
        public static bool RemoveSOSupplementalLineItems(SOOrderEntry soOrderEntryGraph, AMConfigurationResults configResult)
        {
            bool containsChanges = false;
            if (configResult == null || soOrderEntryGraph?.Document?.Current == null)
            {
                return containsChanges;
            }

            foreach (SOLine supplementalLine in PXSelect<SOLine,
                Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                    And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                        And<SOLineExt.aMParentLineNbr, Equal<Required<SOLineExt.aMParentLineNbr>>,
                            And<SOLineExt.aMIsSupplemental, Equal<True>>
                        >>>>.Select(soOrderEntryGraph, configResult.OrdTypeRef, configResult.OrdNbrRef, configResult.OrdLineRef))
            {
                var newExt = PXCache<SOLine>.GetExtension<SOLineExt>(supplementalLine);
                newExt.AMParentLineNbr = supplementalLine.LineNbr;
                soOrderEntryGraph.Transactions.Update(supplementalLine);
                soOrderEntryGraph.Transactions.Delete(supplementalLine);
                containsChanges = true;
            }

            return containsChanges;
        }

        /// <summary>
        /// Remove Opportunity related supplemental line items
        /// </summary>
        /// <param name="configResult"></param>
        /// <returns>True if updates made</returns>
        public static bool RemoveProductSupplementalLineItems(AMConfigurationResults configResult)
        {
            if (configResult == null)
            {
                return false;
            }

            //var opportunityGraph = PXGraph.CreateInstance<OpportunityMaint>();
            //opportunityGraph.Opportunity.Current = opportunityGraph.Opportunity.Search<CROpportunity.opportunityID>(configResult.OpportunityID);

            //var containsChanges = RemoveProductSupplementalLineItems(opportunityGraph, configResult);
            //if (containsChanges)
            //{
            //    opportunityGraph.Actions.PressSave();
            //}

            return true;
        }

        /// <summary>
        /// Remove Opportunity related supplemental line items
        /// </summary>
        /// <param name="configResult"></param>
        /// <returns>True if updates made</returns>
        public static bool RemoveProductSupplementalLineItems(PXGraph graph, AMConfigurationResults configResult)
        {
            var containsChanges = false;
            if (configResult == null)
            {
                return containsChanges;
            }

            foreach (CROpportunityProducts supplementalLine in PXSelect<
                CROpportunityProducts,
                Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunityProducts.quoteID>>,
                    And<CROpportunityProductsExt.aMParentLineNbr, Equal<Required<CROpportunityProductsExt.aMParentLineNbr>>,
                    And<CROpportunityProductsExt.aMIsSupplemental, Equal<True>>>>>
                .Select(graph, configResult.OpportunityQuoteID, configResult.OpportunityLineNbr))
            {
                var newExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(supplementalLine);
                newExt.AMParentLineNbr = supplementalLine.LineNbr;
                graph.Caches<CROpportunityProducts>().Update(supplementalLine);
                graph.Caches<CROpportunityProducts>().Delete(supplementalLine);

                containsChanges = true;
            }

            return containsChanges;
        }

        /// <summary>
        /// Does the given opportunity or Quote graph contain new/inserted Supplemental lines?
        /// </summary>
        /// <param name="graph">Opportunity graph or Quote Graph</param>
        /// <returns>True if inserted Supplemental lines found</returns>
        public static bool ContainsNewSupplementalLines(PXGraph graph)
        {
            return graph.Caches<CROpportunityProducts>().Inserted.Cast<CROpportunityProducts>().Any(row => row?.GetExtension<CROpportunityProductsExt>()?.AMIsSupplemental == true);
        }

        /// <summary>
        /// Does the given sales order graph contain new/inserted Supplemental lines?
        /// </summary>
        /// <param name="graph">Sales Order graph</param>
        /// <returns>True if inserted Supplemental lines found</returns>
        public static bool ContainsNewSupplementalLines(SOOrderEntry graph)
        {
            return graph.Transactions.Cache.Inserted.Cast<SOLine>().Any(row => row?.GetExtension<SOLineExt>()?.AMIsSupplemental == true);
        }

        public static SOLine FindParentConfigLineByOrigParentLineNbr(PXCache soLineCache, SOLine suppLine)
        {
            var suppLineExt = suppLine.GetExtension<SOLineExt>();
            if (suppLineExt == null)
            {
                return null;
            }
            foreach (SOLine soLine in soLineCache.Inserted)
            {
                var soLineExt = soLine.GetExtension<SOLineExt>();
                if (soLineExt == null)
                {
                    continue;
                }
                if (soLineExt.AMOrigParentLineNbr == suppLineExt.AMOrigParentLineNbr
                    && !soLineExt.AMIsSupplemental.GetValueOrDefault())
                {
                    return soLine;
                }
            }
            return null;
        }
    }
}