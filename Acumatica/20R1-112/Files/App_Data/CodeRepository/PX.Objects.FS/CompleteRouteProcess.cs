using PX.Data;
using System;

namespace PX.Objects.FS
{
    public class CompleteRouteProcess : PXGraph<CompleteRouteProcess>
    {
        public CompleteRouteProcess()
        {
            RouteDocumentMaint graphRouteDocumentMaint = CreateInstance<RouteDocumentMaint>();

            RouteDocs.SetProcessDelegate(
                delegate(CompleteRouteProcess processor, FSRouteDocument fsRouteDocumentRow)
                {
                    processor.Clear();
                    graphRouteDocumentMaint.Clear();

                    processor.CompleteRoute(graphRouteDocumentMaint, fsRouteDocumentRow);
                });
        }

        #region DACFilter
        [Serializable]
        public partial class RouteFilter : IBqlTable
        {
            #region Date
            public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

            [PXDBDate]
            [PXUIField(DisplayName = "Date")]
            public virtual DateTime? Date { get; set; }
            #endregion
            #region ShowCompletedRoutes
            public abstract class showCompletedRoutes : PX.Data.BQL.BqlBool.Field<showCompletedRoutes> { }

            [PXBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Show Completed Routes")]
            public virtual bool? ShowCompletedRoutes { get; set; }
            #endregion
        }
        #endregion

        #region Select
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;
        
        [PXHidden]
        public PXFilter<RouteFilter> Filter;
        public PXCancel<RouteFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessingJoin<FSRouteDocument, RouteFilter,
               InnerJoin<FSRoute,
               On<
                   FSRoute.routeID, Equal<FSRouteDocument.routeID>>>,
               Where2<
                   Where<
                       CurrentValue<RouteFilter.date>, IsNull,
                       Or<FSRouteDocument.date, Equal<CurrentValue<RouteFilter.date>>>>,
                   And<
                       Where2<
                           Where<
                               FSRouteDocument.status, Equal<FSRouteDocument.status.Open>,
                               Or<FSRouteDocument.status, Equal<FSRouteDocument.status.InProcess>>>,
                           Or<
                               Where<
                                   FSRouteDocument.status, Equal<FSRouteDocument.status.Completed>,
                                   And<CurrentValue<RouteFilter.showCompletedRoutes>, Equal<True>>>>>>>,
               OrderBy<
                   Asc<FSRouteDocument.refNbr>>> RouteDocs;
        #endregion

        #region Methods
        /// <summary>
        /// Try to complete a set of routes.
        /// </summary>
        /// <param name="graphRouteDocumentMaint"> Route Document graph.</param>
        /// <param name="fsRouteDocumentRow">FSRouteDocument row to be processed.</param>
        public virtual void CompleteRoute(RouteDocumentMaint graphRouteDocumentMaint, FSRouteDocument fsRouteDocumentRow)
        {
            if (fsRouteDocumentRow.Status != ID.Status_Route.COMPLETED)
            {
                graphRouteDocumentMaint.RouteRecords.Current = graphRouteDocumentMaint.RouteRecords.Search<FSRouteDocument.refNbr>(fsRouteDocumentRow.RefNbr);
                graphRouteDocumentMaint.completeRoute.PressButton();
            }
        }
        #endregion

        #region Events
        protected virtual void _(Events.RowSelected<FSRouteDocument> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (fsRouteDocumentRow.Status == ID.Status_Route.COMPLETED)
            {
                PXUIFieldAttribute.SetEnabled<FSRouteDocument.selected>(e.Cache, fsRouteDocumentRow, false);  
            }
        }
        #endregion
    }
}
