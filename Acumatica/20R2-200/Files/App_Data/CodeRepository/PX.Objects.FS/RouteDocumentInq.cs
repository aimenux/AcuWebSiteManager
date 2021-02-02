using PX.Data;
using PX.SM;
using System.Collections;

namespace PX.Objects.FS
{
    public class RouteDocumentInq : PXGraph<RouteDocumentInq>
    {
        #region Filter+Select
        [PXHidden]
        public PXSetup<FSRouteSetup> RouteSetupRecord;
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        public PXFilter<RouteDocumentFilter> Filter;
        public PXCancel<RouteDocumentFilter> Cancel;

        [PXFilterable]
        [PXViewDetailsButton(typeof(RouteDocumentFilter))]
        public PXSelectJoin<FSRouteDocument,
               InnerJoin<FSRoute,
                   On<FSRoute.routeID, Equal<FSRouteDocument.routeID>>>> RouteDocuments;
        #endregion

        #region Actions
        public PXAction<RouteDocumentFilter> openRouteDocument;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenRouteDocument()
        {
            if (RouteDocuments.Current.Status == ID.Status_Route.CLOSED)
            {
                RouteClosingMaint routeClosingMaintGraph = PXGraph.CreateInstance<RouteClosingMaint>();
                routeClosingMaintGraph.RouteRecords.Current = RouteDocuments.Current;
                throw new PXRedirectRequiredException(routeClosingMaintGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else
            {
                RouteDocumentMaint routeDocumentMaintGraph = PXGraph.CreateInstance<RouteDocumentMaint>();
                routeDocumentMaintGraph.RouteRecords.Current = RouteDocuments.Current;
                throw new PXRedirectRequiredException(routeDocumentMaintGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region Delegate

        public virtual System.Type SetWhereStatus(bool? statusOpen, bool? statusInProcess, bool? statusCanceled, bool? statusCompleted, bool? statusClosed, PXSelectBase<FSRouteDocument> commandFilter)
        {
            // Open
            if (statusOpen == true && statusInProcess == false && statusCanceled == false && statusCompleted == false && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.status, Equal<ListField_Status_Route.Open>>>();
            }

            // InProcess
            if (statusOpen == false && statusInProcess == true && statusCanceled == false && statusCompleted == false && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>>>();
            }

            // Canceled
            if (statusOpen == false && statusInProcess == false && statusCanceled == true && statusCompleted == false && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>>>();
            }

            // Completed
            if (statusOpen == false && statusInProcess == false && statusCanceled == false && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>();
            }

            // Closed
            if (statusOpen == false && statusInProcess == false && statusCanceled == false && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>();
            }

            // Open + InProcess
            if (statusOpen == true && statusInProcess == true && statusCanceled == false && statusCompleted == false && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>>>>();
            }

            // Open + Canceled
            if (statusOpen == true && statusInProcess == false && statusCanceled == true && statusCompleted == false && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>>>>();
            }

            // Open + Completed
            if (statusOpen == true && statusInProcess == false && statusCanceled == false && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>>();
            }

            // Open + Closed
            if (statusOpen == true && statusInProcess == false && statusCanceled == false && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>();
            }

            // InProcess + Canceled
            if (statusOpen == false && statusInProcess == true && statusCanceled == true && statusCompleted == false && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>>>>();
            }

            // InProcess + Completed
            if (statusOpen == false && statusInProcess == true && statusCanceled == false && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>>();
            }

            // InProcess + Closed
            if (statusOpen == false && statusInProcess == true && statusCanceled == false && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>();
            }

            // Canceled + Completed
            if (statusOpen == false && statusInProcess == false && statusCanceled == true && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>>();
            }

            // Canceled + Closed
            if (statusOpen == false && statusInProcess == false && statusCanceled == true && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>();
            }

            // Completed + Closed
            if (statusOpen == false && statusInProcess == false && statusCanceled == false && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>();
            }

            // Open + InProcess + Canceled
            if (statusOpen == true && statusInProcess == true && statusCanceled == true && statusCompleted == false && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>>>>>();
            }

            // Open + InProcess + Completed
            if (statusOpen == true && statusInProcess == true && statusCanceled == false && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>>>();
            }

            // Open + InProcess + Closed
            if (statusOpen == true && statusInProcess == true && statusCanceled == false && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>();
            }

            // Open + Canceled + Completed
            if (statusOpen == true && statusInProcess == false && statusCanceled == true && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>>>();
            }

            // Open + Canceled + Closed
            if (statusOpen == true && statusInProcess == false && statusCanceled == true && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>();
            }

            // Open + Completed + Closed
            if (statusOpen == true && statusInProcess == false && statusCanceled == false && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>();
            }

            // InProcess + Canceled + Completed
            if (statusOpen == false && statusInProcess == true && statusCanceled == true && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>>>();
            }

            // InProcess + Canceled + Closed
            if (statusOpen == false && statusInProcess == true && statusCanceled == true && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>();
            }

            // InProcess + Completed + Closed
            if (statusOpen == false && statusInProcess == true && statusCanceled == false && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>();
            }

            // Canceled + Completed + Closed
            if (statusOpen == false && statusInProcess == false && statusCanceled == true && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>();
            }

            // Open + InProcess + Canceled + Completed
            if (statusOpen == true && statusInProcess == true && statusCanceled == true && statusCompleted == true && statusClosed == false)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>>>>();
            }

            // Open + Canceled + Completed + Closed
            if (statusOpen == true && statusInProcess == false && statusCanceled == true && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>>();
            }

            // Open + InProcess + Completed + Closed
            if (statusOpen == true && statusInProcess == true && statusCanceled == false && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>>();
            }

            // Open + InProcess + Canceled + Closed
            if (statusOpen == true && statusInProcess == true && statusCanceled == true && statusCompleted == false && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>>();
            }

            // InProcess + Canceled + Completed + Closed
            if (statusOpen == false && statusInProcess == true && statusCanceled == true && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>>();
            }

            // Open + InProcess + Canceled + Completed + Closed
            if (statusOpen == true && statusInProcess == true && statusCanceled == true && statusCompleted == true && statusClosed == true)
            {
                commandFilter.WhereAnd<Where<
                                            FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Canceled>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>,
                                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>>>>>();
            }

            return null;
        }

        public virtual IEnumerable routeDocuments()
        {
            var commandFilter = new PXSelectJoin<FSRouteDocument,
                                    InnerJoin<FSRoute,
                                        On<FSRoute.routeID, Equal<FSRouteDocument.routeID>>>>(this);

            var filter = Filter.Current;

            //Filters
            if (filter.FromDate != null)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.date, GreaterEqual<CurrentValue<RouteDocumentFilter.fromDate>>>>();
            }

            if (filter.ToDate != null)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.date, LessEqual<CurrentValue<RouteDocumentFilter.toDate>>>>();
            }

            if (filter.RouteID != null)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.routeID, Equal<CurrentValue<RouteDocumentFilter.routeID>>>>();
            }

            if (filter.StatusOpen == true || filter.StatusInProcess == true || filter.StatusCanceled == true || filter.StatusCompleted == true || filter.StatusClosed == true)
            {
                SetWhereStatus(filter.StatusOpen, filter.StatusInProcess, filter.StatusCanceled, filter.StatusCompleted, filter.StatusClosed, commandFilter);
            }

            int startRow = PXView.StartRow;
            int totalRows = 0;
            var list = commandFilter.View.Select(PXView.Currents,
                                                 PXView.Parameters,
                                                 PXView.Searches,
                                                 PXView.SortColumns,
                                                 PXView.Descendings,
                                                 PXView.Filters,
                                                 ref startRow,
                                                 PXView.MaximumRows,
                                                 ref totalRows);

            PXView.StartRow = 0;

            return list;
        }

        #endregion
    }
}