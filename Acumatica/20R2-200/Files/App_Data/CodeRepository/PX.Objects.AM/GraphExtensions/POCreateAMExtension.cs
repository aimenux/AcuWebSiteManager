using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using CRLocation = PX.Objects.CR.Standalone.Location;
using System;
using System.Collections.Generic;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.FS;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// MFG Extension to Create Purchase Orders screen
    /// </summary>
    public class POCreateAMExtension : PXGraphExtension<POCreate>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public override void Initialize()
        {
            base.Initialize();

            Base.FixedDemand.Join<LeftJoin<AMProdMatlSplitPlan, On<AMProdMatlSplitPlan.planID, Equal<POFixedDemand.planID>>>>();
            Base.FixedDemand.WhereAnd<Where<
                Where2<Where<AMProdMatlSplitPlan.orderType, Equal<Current<POCreateFilterExt.aMOrderType>>, Or<Current<POCreateFilterExt.aMOrderType>, IsNull>>,
                And<Where<AMProdMatlSplitPlan.prodOrdID, Equal<Current<POCreateFilterExt.prodOrdID>>, Or<Current<POCreateFilterExt.prodOrdID>, IsNull>>>>>>();
        }

        protected virtual IEnumerable fixedDemand()
        {
            PXView select = new PXView(Base, true, Base.FixedDemand.View.BqlSelect);
            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters, PXView.Searches,
                PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;

            return EnumerateAndPrepareResults(result);
        }

        /// <summary>
        /// Enumerate and Prepare Fixed Demands
        /// (Code is a copy from POCreate EnumerateAndPrepareFixedDemands)
        /// </summary>
        /// <param name="fixedDemands">result set of FixedDemand to process</param>
        /// <returns></returns>
        public virtual IEnumerable EnumerateAndPrepareResults(List<object> fixedDemands)
        {
            foreach (PXResult<POFixedDemand, InventoryItem, Vendor, POVendorInventory> rec in fixedDemands)
            {
                var demand = (POFixedDemand)rec;
                var item = (InventoryItem)rec;
                var vendor = (Vendor)rec;
                var price = (POVendorInventory)rec;
                Base.EnumerateAndPrepareFixedDemandRow(demand, item, vendor, price);

                yield return rec;
            }
        }

        public static void POCreatePOOrders(List<POFixedDemand> list, DateTime? purchaseDate)
        {
            var poredirect = POCreate.CreatePOOrders(list, purchaseDate, false);
            PXLongOperationHelper.TraceProcessingMessages<POFixedDemand>();
            if (poredirect != null)
            {
                throw poredirect;
            }

            throw new PXException(ErrorMessages.SeveralItemsFailed);
        }

        /// <summary>
        /// Create a PO using manual numbering
        /// </summary>
        public static void POCreatePOOrders(List<POFixedDemand> list, DateTime? purchaseDate, string manualOrdNbr)
        {
            PXGraph.InstanceCreated.AddHandler<POOrderEntry>(graph =>
            {
                graph.RowInserting.AddHandler<POOrder>((cache, e) =>
                {
                    var row = (POOrder)e.Row;
                    row.OrderNbr = manualOrdNbr;
                });
            });

            POCreatePOOrders(list, purchaseDate);
        }

        public PXAction<POCreate.POCreateFilter> viewProdDocument;
        [PXUIField(DisplayName = "viewProdDocument", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
        protected virtual System.Collections.IEnumerable ViewProdDocument(PXAdapter adapter)
        {
            var graph = GetProductionGraph(Base.FixedDemand?.Current);
            if (graph != null)
            {
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            }

            return adapter.Get();
        }

        protected virtual PXGraph GetProductionGraph(POFixedDemand fixedDemand)
        {
            if (fixedDemand?.PlanID == null)
            {
                return null;
            }

            var prodMatlSplit = (AMProdMatlSplitPlan)PXSelect<AMProdMatlSplitPlan,
                    Where<AMProdMatlSplitPlan.planID, Equal<Required<AMProdMatlSplitPlan.planID>>>>
                .Select(Base, fixedDemand.PlanID);

            if (prodMatlSplit?.ProdOrdID == null)
            {
                return null;
            }

            var graph = PXGraph.CreateInstance<ProdMaint>();
            graph.ProdMaintRecords.Current = graph.ProdMaintRecords.Search<AMProdItem.prodOrdID>(prodMatlSplit.ProdOrdID, prodMatlSplit.OrderType);
            if (graph.ProdMaintRecords.Current == null)
            {
                return null;
            }

            return graph;
        }
    }

    /// <summary>
    /// MFG Extension to Create Purchase Orders screen
    /// (enabled when field service is enabled)
    /// </summary>
    public class FSPOCreateAMExtension : PXGraphExtension<POCreateAMExtension, SM_POCreate, POCreate>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>() && PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        protected virtual IEnumerable fixedDemand()
        {
            PXView select = new PXView(Base, true, Base.FixedDemand.View.BqlSelect);

            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters, PXView.Searches,
                PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;

            List<long> planIDList = Base1.GetServicePlanIDList();

            // This is the only change from SM_POCreate as we need to include AMProdMatlSplitPlan into this cast
            foreach (PXResult<POFixedDemand, InventoryItem, Vendor, POVendorInventory, CRLocation, SOOrder, SOLineSplit, SOLine, INItemClass, AMProdMatlSplitPlan, FSServiceOrder, FSSODetFSSODetSplit> rec in result)
            {
                var demand = (POFixedDemand)rec;
                var item = (InventoryItem)rec;
                var vendor = (Vendor)rec;
                var price = (POVendorInventory)rec;
                Base.EnumerateAndPrepareFixedDemandRow(demand, item, vendor, price);

                Base1.PrepareFixedDemandFieldServiceRow(demand, rec, rec, rec, rec, planIDList);
            }

            return result;
        }
    }
}