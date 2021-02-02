using PX.Data;
using System;

namespace PX.Objects.FS
{
    public class RoutePendingInq : PXGraph<RoutePendingInq>
    {
        public RoutePendingInq()
            : base()
        {
            Routes.AllowUpdate = false;
        }
  
        #region DACFilter
        [Serializable]
        public partial class RouteWrkSheetFilter : IBqlTable
        {
            #region Date
		    public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		    [PXDBDate]
		    [PXUIField(DisplayName = "Date")]
            [PXDefault]
		    public virtual DateTime? Date { get; set; }
		    #endregion
        }
        #endregion

        #region Filter+Select
        public PXFilter<RouteWrkSheetFilter> Filter;

        [PXFilterable]
        public PXSelectJoin<FSRouteDocument,
                InnerJoin<FSRoute,
                    On<FSRouteDocument.routeID, Equal<FSRoute.routeID>>>,
                Where2<
                    Where<
                        CurrentValue<RouteWrkSheetFilter.date>, IsNull,     
                    Or<
                        FSRouteDocument.date, Equal<CurrentValue<RouteWrkSheetFilter.date>>>>,
                    And<
                        Where<
                            FSRouteDocument.status, Equal<FSRouteDocument.status.Completed>,
                        Or<
                            FSRouteDocument.status, Equal<FSRouteDocument.status.Closed>>>>>,
                OrderBy<
                        Asc<FSRouteDocument.refNbr>>>
                Routes;
        #endregion

        #region Actions
        #region OpenRouteClosing
        public PXAction<RouteWrkSheetFilter> openRouteClosing;
        [PXUIField(DisplayName = "Open Route Closing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenRouteClosing()
        {
            if (Routes.Current != null)
            {
                RouteClosingMaint graphRouteClosingMaint = PXGraph.CreateInstance<RouteClosingMaint>();

                graphRouteClosingMaint.RouteRecords.Current = PXSelect<FSRouteDocument,
                                                              Where<
                                                                FSRouteDocument.refNbr, Equal<Required<FSRouteDocument.refNbr>>>>
                                                              .Select(this, Routes.Current.RefNbr);

                throw new PXRedirectRequiredException(graphRouteClosingMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #endregion
    }
}