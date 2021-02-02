using PX.Data;
using PX.Objects.EP;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class DriverSelectionHelper
    {
        public class DriverRecords_View : PXSelectReadonly2<EPEmployee,
                                          InnerJoin<FSRouteEmployee, 
                                          On<
                                              FSRouteEmployee.employeeID, Equal<EPEmployee.bAccountID>>>,
                                          Where<
                                              FSxEPEmployee.isDriver, Equal<True>,
                                          And<
                                              FSxEPEmployee.sDEnabled, Equal<True>>>,
                                          OrderBy<
                                              Asc<FSRouteEmployee.priorityPreference>>>
        {
            public DriverRecords_View(PXGraph graph) : base(graph)
            {
            }

            public DriverRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public static IEnumerable DriverRecordsDelegate(PXGraph graph, SharedClasses.RouteSelected_view routeSelected, PXFilter<DriverSelectionFilter> filter)
        {
            if (routeSelected.Current == null)
            {
                yield break;
            }

            List<object> args = new List<object>();

            PXSelectBase<EPEmployee> commandFilter = new PXSelectJoinGroupBy<EPEmployee,
                                                         InnerJoin<FSRouteEmployee,
                                                         On<
                                                             FSRouteEmployee.employeeID, Equal<EPEmployee.bAccountID>>,
                                                         LeftJoin<FSRouteDocument,
                                                         On<
                                                             FSRouteDocument.driverID, Equal<FSRouteEmployee.employeeID>,
                                                             And<FSRouteDocument.date, Equal<Required<FSRouteDocument.date>>>>>>,
                                                         Where<
                                                             FSRouteEmployee.routeID, Equal<Required<FSRouteEmployee.routeID>>,
                                                         And<
                                                             FSxEPEmployee.sDEnabled, Equal<True>,
                                                         And<
                                                             FSxEPEmployee.isDriver, Equal<True>>>>,
                                                         Aggregate<
                                                             GroupBy<EPEmployee.bAccountID>>,
                                                         OrderBy<
                                                             Asc<FSRouteEmployee.priorityPreference>>>(graph);

            args.Add(routeSelected.Current.Date);
            args.Add(routeSelected.Current.RouteID);

            if (filter.Current.ShowUnassignedDrivers == true)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.routeID, IsNull>>();
            }

            var bqlResultSet = commandFilter.Select(args.ToArray());

            foreach (PXResult<EPEmployee, FSRouteEmployee, FSRouteDocument> bqlResult in bqlResultSet)
            {
                EPEmployee epEmployeeRow = (EPEmployee)bqlResult;
                FSRouteEmployee fsRouteEmployeeRow = (FSRouteEmployee)bqlResult;
                FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)bqlResult;

                FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

                if (fsRouteDocumentRow != null && fsRouteDocumentRow.RouteID != null)
                {
                    fsxEPEmployeeRow.Mem_UnassignedDriver = true;
                }

                yield return bqlResult;
            }
        }
    }
}
