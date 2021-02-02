using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System.Collections;
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

        public PXAction<POCreateFilter> viewServiceOrderDocument;

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
            LeftJoin<FSServiceOrder, On<FSServiceOrder.noteID, Equal<POFixedDemand.refNoteID>>,
            LeftJoin<FSSODetSplit, On<FSSODetSplit.planID, Equal<POFixedDemand.planID>>,
            LeftJoin<FSSODet, On<FSSODet.srvOrdType, Equal<FSSODetSplit.srvOrdType>, And<FSSODet.refNbr, Equal<FSSODetSplit.refNbr>, And<FSSODet.lineNbr, Equal<FSSODetSplit.lineNbr>>>>,
            LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>>>>>>>>>>>,
            Where2<Where<POFixedDemand.vendorID, Equal<Current<POCreateFilter.vendorID>>, Or<Current<POCreateFilter.vendorID>, IsNull>>,
            And2<Where<POFixedDemand.inventoryID, Equal<Current<POCreateFilter.inventoryID>>, Or<Current<POCreateFilter.inventoryID>, IsNull>>,
            And2<Where<POFixedDemand.siteID, Equal<Current<POCreateFilter.siteID>>, Or<Current<POCreateFilter.siteID>, IsNull>>,
            And2<
                Where2<
                    Where<SOOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull>>,
                    Or<FSServiceOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull>>>,
            And2<Where<SOOrder.orderType, Equal<Current<POCreateFilter.orderType>>, Or<Current<POCreateFilter.orderType>, IsNull>>,
            And2<Where<SOOrder.orderNbr, Equal<Current<POCreateFilter.orderNbr>>, Or<Current<POCreateFilter.orderNbr>, IsNull>>,
            And2<Where<FSServiceOrder.srvOrdType, Equal<Current<FSxPOCreateFilter.srvOrdType>>, Or<Current<FSxPOCreateFilter.srvOrdType>, IsNull>>,
            And2<Where<FSServiceOrder.refNbr, Equal<Current<FSxPOCreateFilter.serviceOrderRefNbr>>, Or<Current<FSxPOCreateFilter.serviceOrderRefNbr>, IsNull>>,
            And2<Where<POFixedDemand.planDate, LessEqual<Current<POCreateFilter.requestedOnDate>>, Or<Current<POCreateFilter.requestedOnDate>, IsNull>>,
            And<Where<INItemClass.itemClassCD, Like<Current<POCreateFilter.itemClassCDWildcard>>, Or<Current<POCreateFilter.itemClassCDWildcard>, IsNull>>>>>>>>>>>>> FixedDemand;

        protected IEnumerable fixedDemand()
        {
            PXResultset<POFixedDemand> fixedDemands =
                PXSelectJoin<POFixedDemand,
                InnerJoin<InventoryItem,
                    On<InventoryItem.inventoryID, Equal<POFixedDemand.inventoryID>>,
                LeftJoin<Vendor,
                    On<Vendor.bAccountID, Equal<POFixedDemand.vendorID>>,
                LeftJoin<POVendorInventory,
                      On<POVendorInventory.recordID, Equal<POFixedDemand.recordID>>,
                LeftJoin<CRLocation,
                    On<CRLocation.bAccountID, Equal<POFixedDemand.vendorID>,
                    And<CRLocation.locationID, Equal<POFixedDemand.vendorLocationID>>>,
                LeftJoin<SOOrder,
                    On<SOOrder.noteID, Equal<POFixedDemand.refNoteID>>,
                LeftJoin<SOLineSplit,
                    On<SOLineSplit.planID, Equal<POFixedDemand.planID>>,
                LeftJoin<SOLine,
                    On<SOLine.orderType, Equal<SOLineSplit.orderType>,
                    And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>,
                    And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
                LeftJoin<FSServiceOrder, 
                    On<FSServiceOrder.noteID, Equal<POFixedDemand.refNoteID>>,
                LeftJoin<FSSODetSplit, 
                    On<FSSODetSplit.planID, Equal<POFixedDemand.planID>>,
                LeftJoin<FSSODet, 
                    On<FSSODet.srvOrdType, Equal<FSSODetSplit.srvOrdType>, 
                    And<FSSODet.refNbr, Equal<FSSODetSplit.refNbr>, 
                    And<FSSODet.lineNbr, Equal<FSSODetSplit.lineNbr>>>>,
                LeftJoin<INItemClass,
                    On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>>>>>>>>>>>,
                Where2<
                    Where<POFixedDemand.vendorID, Equal<Current<POCreateFilter.vendorID>>,
                        Or<Current<POCreateFilter.vendorID>, IsNull>>,
                    And2<
                        Where<POFixedDemand.inventoryID, Equal<Current<POCreateFilter.inventoryID>>,
                            Or<Current<POCreateFilter.inventoryID>, IsNull>>,
                        And2<
                            Where<POFixedDemand.siteID, Equal<Current<POCreateFilter.siteID>>,
                                Or<Current<POCreateFilter.siteID>, IsNull>>,
                            And2<
                                Where2<
                                    Where<SOOrder.customerID, Equal<Current<POCreateFilter.customerID>>, 
                                        Or<Current<POCreateFilter.customerID>, IsNull>>,
                                    Or<FSServiceOrder.customerID, Equal<Current<POCreateFilter.customerID>>, 
                                            Or<Current<POCreateFilter.customerID>, IsNull>>>,
                                And2<
                                    Where<SOOrder.orderType, Equal<Current<POCreateFilter.orderType>>,
                                        Or<Current<POCreateFilter.orderType>, IsNull>>,
                                    And2<
                                        Where<SOOrder.orderNbr, Equal<Current<POCreateFilter.orderNbr>>,
                                            Or<Current<POCreateFilter.orderNbr>, IsNull>>,
                                    And2<
                                            Where<FSServiceOrder.srvOrdType, Equal<Current<FSxPOCreateFilter.srvOrdType>>, 
                                                Or<Current<FSxPOCreateFilter.srvOrdType>, IsNull>>,
                                        And2<
                                            Where<FSServiceOrder.refNbr, Equal<Current<FSxPOCreateFilter.serviceOrderRefNbr>>,
                                                Or<Current<FSxPOCreateFilter.serviceOrderRefNbr>, IsNull>>,
                                        And2<
                                            Where<POFixedDemand.planDate, LessEqual<Current<POCreateFilter.requestedOnDate>>,
                                                Or<Current<POCreateFilter.requestedOnDate>, IsNull>>,
                                            And<Where<INItemClass.itemClassCD, Like<Current<POCreateFilter.itemClassCDWildcard>>,
                                                Or<Current<POCreateFilter.itemClassCDWildcard>, IsNull>>>>>>>>>>>>>
                    .Select(Base);

            foreach(PXResult<POFixedDemand, InventoryItem, Vendor, POVendorInventory, CRLocation, SOOrder, SOLineSplit, SOLine, FSServiceOrder, FSSODetSplit, FSSODet, INItemClass> resultRow in fixedDemands)
            {
                POFixedDemand poFixedDemandRow = (POFixedDemand)resultRow;
                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)resultRow;
                SOOrder soOrderRow = (SOOrder)resultRow;

                FSxPOFixedDemand fSxPOFixedDemandRow = Base.FixedDemand.Cache.GetExtension<FSxPOFixedDemand>(poFixedDemandRow);

                if (fsServiceOrderRow.CustomerID != null)
                {
                    soOrderRow.CustomerID = fsServiceOrderRow.CustomerID;
                }

                fSxPOFixedDemandRow.SrvOrdType = fsServiceOrderRow.SrvOrdType;
                fSxPOFixedDemandRow.FSRefNbr = fsServiceOrderRow.RefNbr;
            }

            return Base.EnumerateAndPrepareFixedDemands(fixedDemands);
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewServiceOrderDocument(PXAdapter adapter)
        {
            POFixedDemand line = FixedDemand.Current;
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