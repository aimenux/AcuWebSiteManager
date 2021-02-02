using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            Base.FixedDemand.Join<LeftJoin<FSSODetFSSODetSplit,
                On<FSSODetFSSODetSplit.planID, Equal<POFixedDemand.planID>,
                And<FSSODetFSSODetSplit.srvOrdType, Equal<FSServiceOrder.srvOrdType>,
                And<FSSODetFSSODetSplit.refNbr, Equal<FSServiceOrder.refNbr>>>>>>();

            Base.FixedDemand.WhereAnd<Where<
                Where2<Where<FSServiceOrder.customerID, Equal<Current<POCreateFilter.customerID>>, Or<Current<POCreateFilter.customerID>, IsNull, Or<FSServiceOrder.refNbr, IsNull>>>,
                And2<Where<FSServiceOrder.srvOrdType, Equal<Current<FSxPOCreateFilter.srvOrdType>>, Or<Current<FSxPOCreateFilter.srvOrdType>, IsNull>>,
                And<Where<FSServiceOrder.refNbr, Equal<Current<FSxPOCreateFilter.serviceOrderRefNbr>>, Or<Current<FSxPOCreateFilter.serviceOrderRefNbr>, IsNull>>>>>>>();
        }

        public PXAction<POCreateFilter> viewDocument;

        protected virtual IEnumerable fixedDemand()
        {
            PXView select = new PXView(Base, false, Base.FixedDemand.View.BqlSelect);

            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters, PXView.Searches,
                PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;

            List<long> planIDList = GetServicePlanIDList();

            foreach (PXResult<POFixedDemand, InventoryItem, Vendor, POVendorInventory, CRLocation, SOOrder, SOLineSplit, SOLine, INItemClass, FSServiceOrder, FSSODetFSSODetSplit> rec in result)
            {
                var demand = (POFixedDemand)rec;
                var item = (InventoryItem)rec;
                var vendor = (Vendor)rec;
                var price = (POVendorInventory)rec;
                Base.EnumerateAndPrepareFixedDemandRow(demand, item, vendor, price);

                PrepareFixedDemandFieldServiceRow(demand, rec, rec, rec, rec, planIDList);
            }

            return result;
        }

        public virtual void PrepareFixedDemandFieldServiceRow(POFixedDemand demand, FSServiceOrder fsServiceOrderRow, SOOrder soOrderRow, SOLine soLineRow, FSSODetFSSODetSplit fsSODetRow, List<long> planIDList)
        {
            if (fsServiceOrderRow != null
                && string.IsNullOrEmpty(fsServiceOrderRow.RefNbr) == false)
            {
                soOrderRow.CustomerID = fsServiceOrderRow.CustomerID;

                if (fsSODetRow != null
                    && string.IsNullOrEmpty(fsSODetRow.RefNbr) == false)
                {
                    soLineRow.UnitPrice = fsSODetRow.UnitPrice;
                    soLineRow.UOM = fsSODetRow.UOM;
                }

                FSxPOFixedDemand fSxPOFixedDemandRow = Base.FixedDemand.Cache.GetExtension<FSxPOFixedDemand>(demand);
                fSxPOFixedDemandRow.FSRefNbr = fsServiceOrderRow.RefNbr;

                if (planIDList != null
                    && planIDList.Count > 0
                    && planIDList.Contains((long)demand.PlanID) == true)
                {
                    demand.Selected = true;
                }
            }
        }

        public List<long> GetServicePlanIDList()
        {
            if (Base.Filter.Current != null)
            {
                FSxPOCreateFilter filterExt = Base.Filter.Cache.GetExtension<FSxPOCreateFilter>(Base.Filter.Current);

                if (string.IsNullOrEmpty(filterExt.AppointmentRefNbr) == false
                    && string.IsNullOrEmpty(filterExt.SrvOrdType) == false)
                {
                    return SelectFrom<FSSODetSplit>
                                    .InnerJoin<FSAppointmentDet>
                                        .On<FSAppointmentDet.srvOrdType.IsEqual<FSSODetSplit.srvOrdType>
                                            .And<FSAppointmentDet.origSrvOrdNbr.IsEqual<FSSODetSplit.refNbr>>
                                            .And<FSAppointmentDet.origLineNbr.IsEqual<FSSODetSplit.lineNbr>>>
                            .Where<FSAppointmentDet.srvOrdType.IsEqual<@P.AsString>
                                .And<FSAppointmentDet.refNbr.IsEqual<@P.AsString>>>
                            .View.Select(Base, filterExt.SrvOrdType, filterExt.AppointmentRefNbr).RowCast<FSSODetSplit>().Select(x => (long)x.PlanID).ToList();
                }
            }

            return null;
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
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
                return adapter.Get();
            }
            else
            {
                return Base.viewDocument.Press(adapter);
            }
        }
    }
}