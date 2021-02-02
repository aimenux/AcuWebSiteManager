using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using static PX.Objects.PO.POCreate;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    public class SM_POCreate : PXGraphExtension<POCreate>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public override void Initialize()
        {
            base.Initialize();

            Base.FixedDemand.Join<LeftJoin<FSServiceOrder,
                On<FSServiceOrder.noteID, Equal<POFixedDemand.refNoteID>>>>();

            Base.FixedDemand.WhereAnd<Where<
                Where2<Where<FSServiceOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull, Or<FSServiceOrder.refNbr, IsNull>>>,
                And2<Where<FSServiceOrder.srvOrdType, Equal<Current<FSxPOCreateFilter.srvOrdType>>, Or<Current<FSxPOCreateFilter.srvOrdType>, IsNull>>,
                And<Where<FSServiceOrder.refNbr, Equal<Current<FSxPOCreateFilter.serviceOrderRefNbr>>, Or<Current<FSxPOCreateFilter.serviceOrderRefNbr>, IsNull>>>>>>>();
        }

        #region Views

        // TODO: Delete this view in 2020R2
        // We don't delete this view in the moment of these last changes to avoid breaking changes in 2020R1RTW
        // DON'T DELETE THE DELEGATE
        [PXFilterable]
        public PXFilteredProcessingJoin<POFixedDemand, POCreateFilter,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<POFixedDemand.inventoryID>>,
                LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POFixedDemand.vendorID>>,
                LeftJoin<POVendorInventory,
                        On<POVendorInventory.recordID, Equal<POFixedDemand.recordID>>,
                LeftJoin<CRLocation, On<CRLocation.bAccountID, Equal<POFixedDemand.vendorID>, And<CRLocation.locationID, Equal<POFixedDemand.vendorLocationID>>>,
                LeftJoin<SOOrder, On<SOOrder.noteID, Equal<POFixedDemand.refNoteID>>,
                LeftJoin<SOLineSplit, On<SOLineSplit.planID, Equal<POFixedDemand.planID>>,
                LeftJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
                LeftJoin<INItemClass,
                    On<InventoryItem.FK.ItemClass>>>>>>>>>,
                Where2<Where<POFixedDemand.vendorID, Equal<Current<POCreateFilter.vendorID>>, Or<Current<POCreateFilter.vendorID>, IsNull>>,
                And2<Where<POFixedDemand.inventoryID, Equal<Current<POCreateFilter.inventoryID>>, Or<Current<POCreateFilter.inventoryID>, IsNull>>,
                And2<Where<POFixedDemand.siteID, Equal<Current<POCreateFilter.siteID>>, Or<Current<POCreateFilter.siteID>, IsNull>>,
                And2<Where<SOOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull, Or<SOOrder.orderNbr, IsNull>>>,
                And2<Where<SOOrder.orderType, Equal<Current<POCreateFilter.orderType>>, Or<Current<POCreateFilter.orderType>, IsNull>>,
                And2<Where<SOOrder.orderNbr, Equal<Current<POCreateFilter.orderNbr>>, Or<Current<POCreateFilter.orderNbr>, IsNull>>,
                And2<Where<POFixedDemand.planDate, LessEqual<Current<POCreateFilter.requestedOnDate>>, Or<Current<POCreateFilter.requestedOnDate>, IsNull>>,
                And<Where<INItemClass.itemClassCD, Like<Current<POCreateFilter.itemClassCDWildcard>>, Or<Current<POCreateFilter.itemClassCDWildcard>, IsNull>>>>>>>>>>,
                OrderBy<Asc<POFixedDemand.inventoryID>>> FixedDemand;
        #endregion

        public PXAction<POCreateFilter> viewServiceOrderDocument;

        protected virtual IEnumerable fixedDemand()
        {
            var query = Base.FixedDemand.View.BqlSelect;
            object[] parameters = null;
            if (PXView.MaximumRows == 1
                && PXView.Searches?.Length == Base.FixedDemand.Cache.BqlKeys.Count
                && PXView.Searches[0] != null && PXView.Searches[1] != null)
            {
                var inventoryCD = (string)PXView.Searches[0];
                var planID = Convert.ToInt64(PXView.Searches[1]);
                query.WhereAnd<Where<
                    POFixedDemand.planID.IsEqual<@P.AsLong>
                    .And<InventoryItem.inventoryCD.IsEqual<@P.AsString>>>>();
                parameters = new object[] { planID, inventoryCD };
            }

            PXView view = new PXView(Base, false, query);

            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = view.Select(PXView.Currents, parameters, PXView.Searches,
                PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;

            foreach (PXResult<POFixedDemand, InventoryItem, Vendor, POVendorInventory, CRLocation, SOOrder, SOLineSplit, SOLine, INItemClass, FSServiceOrder> rec in result)
            {
                var demand = (POFixedDemand)rec;
                var item = (InventoryItem)rec;
                var vendor = (Vendor)rec;
                var price = (POVendorInventory)rec;
                Base.EnumerateAndPrepareFixedDemandRow(demand, item, vendor, price);

                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)rec;
                SOOrder soOrderRow = (SOOrder)rec;

                if (fsServiceOrderRow.CustomerID != null)
                {
                    soOrderRow.CustomerID = fsServiceOrderRow.CustomerID;
                }

                FSxPOFixedDemand fSxPOFixedDemandRow = Base.FixedDemand.Cache.GetExtension<FSxPOFixedDemand>(demand);
                fSxPOFixedDemandRow.FSRefNbr = fsServiceOrderRow.RefNbr;
            }

            return result;
        }


        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewServiceOrderDocument(PXAdapter adapter)
        {
            POFixedDemand line = Base.FixedDemand.Current;
            if (line == null || line.RefNoteID == null) return adapter.Get();

            FSServiceOrder doc = PXSelect<FSServiceOrder, Where<FSServiceOrder.noteID, Equal<Required<POFixedDemand.refNoteID>>>>.Select(Base, line.RefNoteID);

            if (doc != null)
            {
                ServiceOrderEntry graph = PXGraph.CreateInstance<ServiceOrderEntry>();
                graph.ServiceOrderRecords.Current = graph.ServiceOrderRecords.Search<FSServiceOrder.refNbr>
                                                                                                (doc.RefNbr, doc.SrvOrdType);
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            }
            return adapter.Get();
        }
    }
}