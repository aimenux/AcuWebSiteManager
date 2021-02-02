using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class RouteAppointmentAssignmentHelper
    {
        public class AppointmentEmployees
        {
            public FSAppointment fsAppointmentRow;
            public List<FSAppointmentEmployee> fsAppointmentEmployeeList;

            public AppointmentEmployees()
            {
            }

            public AppointmentEmployees(FSAppointment fsAppointmentRow)
            {
                this.fsAppointmentRow = fsAppointmentRow;
                this.fsAppointmentEmployeeList = new List<FSAppointmentEmployee>();
            }
        }

        #region Selects

        public class RouteRecords_View : PXSelectJoin<FSRouteDocument,
                                         InnerJoin<FSRoute,
                                             On<
                                                 FSRouteDocument.routeID, Equal<FSRoute.routeID>>>,
                                         Where<
                                             FSRouteDocument.routeDocumentID, NotEqual<Current<RouteAppointmentAssignmentFilter.routeDocumentID>>,  
                                         And<
                                             Where<
                                                 Current<RouteAppointmentAssignmentFilter.routeID>, IsNull,
                                             Or<
                                                 FSRouteDocument.routeID, Equal<Current<RouteAppointmentAssignmentFilter.routeID>>>>>>>
        {
            public RouteRecords_View(PXGraph graph) : base(graph)
            {
            }

            public RouteRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }
        #endregion

        public static IEnumerable RouteRecordsDelegate(PXFilter<RouteAppointmentAssignmentFilter> filter,
                                                       PXSelectBase<FSRouteDocument> cmd)
        {
            if (filter.Current == null)
            {
                yield break;
            }

            foreach (PXResult<FSRouteDocument, FSRoute> bqlResult in cmd.Select())
            {
                FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)bqlResult;
                FSRoute fsRouteRow = (FSRoute)bqlResult;

                if (filter.Current.RouteDate.HasValue == true)
                {
                    if (!fsRouteDocumentRow.TimeBegin.HasValue) 
                    {
                        fsRouteDocumentRow.TimeBegin = AppointmentCore.GetDateTimeEnd(fsRouteDocumentRow.Date, 0, 0, 0);
                    }

                    if (fsRouteDocumentRow.Date.Value.Date >= filter.Current.RouteDate.Value.Date
                           && fsRouteDocumentRow.Date.Value.Date <= AppointmentCore.GetDateTimeEnd(filter.Current.RouteDate.Value.Date, 23, 59, 59))
                    {
                        yield return bqlResult;
                    }
                }
                else
                {
                    yield return bqlResult;
                } 
            }
        }

        #region Static Functions

        /// <summary>
        /// Reassign the selected appointment <c>RefNbr</c> to the selected RouteDocumentID from the SmartPanel.
        /// </summary>
        /// <param name="fsRouteDocumentRow">New RouteDocumentID where the appointment is going to be assigned.</param>
        /// <param name="refNbr"><c>RefNbr</c> of the appointment to be assigned.</param>
        /// <param name="srvOrdType"><c>SrvOrdType</c> of the appointment to be assigned.</param>
        public static void ReassignAppointmentToRoute(FSRouteDocument fsRouteDocumentRow, string refNbr, string srvOrdType)
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                FSAppointment fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(refNbr, srvOrdType);

                int? originalRouteDocumentID = fsAppointmentRow.RouteDocumentID;
                int? originalAppointmentPosition = fsAppointmentRow.RoutePosition;

                fsAppointmentRow.RoutePosition = null;

                if (fsRouteDocumentRow != null)
                {
                    fsAppointmentRow.RouteID = fsRouteDocumentRow.RouteID;
                    fsAppointmentRow.RouteDocumentID = fsRouteDocumentRow.RouteDocumentID;
                    fsAppointmentRow.ScheduledDateTimeBegin = fsRouteDocumentRow.TimeBegin != null ? fsRouteDocumentRow.TimeBegin : fsRouteDocumentRow.Date;
                }
                else 
                {
                    fsAppointmentRow.RouteID = null;
                    fsAppointmentRow.RouteDocumentID = null;

                    //Clear vehicle and driver if exist
                    fsAppointmentRow.VehicleID = null;

                    FSAppointmentEmployee fsAppointmentEmployeeRow = PXSelect<FSAppointmentEmployee,
                                                                     Where<
                                                                         FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>,
                                                                     And<
                                                                         FSAppointmentEmployee.isDriver, Equal<True>>>>
                                                                     .Select(graphAppointmentEntry, fsAppointmentRow.AppointmentID);
                    if (fsAppointmentEmployeeRow != null)
                    {
                        graphAppointmentEntry.AppointmentServiceEmployees.Delete(fsAppointmentEmployeeRow);
                    }
                }

                fsAppointmentRow.IsReassigned = true;
                graphAppointmentEntry.AppointmentRecords.Update(fsAppointmentRow);
                graphAppointmentEntry.SelectTimeStamp();
                graphAppointmentEntry.Save.Press();

                ReassignAppointmentPositionsInRoute(originalRouteDocumentID, originalAppointmentPosition);
                ts.Complete();
            }
        }

        /// <summary>
        /// Deletes the selected appointment <c>RefNbr</c> from Database.
        /// </summary>
        /// <param name="refNbr"><c>RefNbr</c> of the appointment to be deleted.</param>
        /// <param name="srvOrdType"><c>SrvOrdType</c> of the appointment to be deleted.</param>
        public static void DeleteAppointmentRoute(string refNbr, string srvOrdType)
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(refNbr, srvOrdType);

                int? routeDocumentID = graphAppointmentEntry.AppointmentRecords.Current.RouteDocumentID;
                int? appointmentPosition = graphAppointmentEntry.AppointmentRecords.Current.RoutePosition;

                graphAppointmentEntry.AppointmentRecords.Delete(graphAppointmentEntry.AppointmentRecords.Current);
                graphAppointmentEntry.Save.Press();

                ReassignAppointmentPositionsInRoute(routeDocumentID, appointmentPosition);
                ts.Complete();
            }
        }

        /// <summary>
        /// Reassign the positions of the appointments in a route beginning from a given position.
        /// </summary>
        private static void ReassignAppointmentPositionsInRoute(int? routeDocumentID, int? initialPosition)
        {
            if (routeDocumentID == null || initialPosition <= 0)
            {
                return;
            }

            AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();

            var fsAppointmentSet = PXSelect<FSAppointment,
                                   Where<
                                       FSAppointment.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>,
                                   And<
                                       FSAppointment.routePosition, GreaterEqual<Required<FSAppointment.routePosition>>>>,
                                   OrderBy<
                                       Asc<FSAppointment.routePosition>>>
                                   .Select(appointmentEntryGraph, routeDocumentID, initialPosition);

            if (fsAppointmentSet != null)
            {
                foreach (FSAppointment fsAppointmentRowInRoute in fsAppointmentSet)
                {
                    fsAppointmentRowInRoute.RoutePosition = initialPosition;
                    appointmentEntryGraph.AppointmentRecords.Update(fsAppointmentRowInRoute);
                    initialPosition = initialPosition + 1;
                }

                appointmentEntryGraph.SkipServiceOrderUpdate = true;
                appointmentEntryGraph.Save.Press();
            }
        }
        #endregion
    }
}
