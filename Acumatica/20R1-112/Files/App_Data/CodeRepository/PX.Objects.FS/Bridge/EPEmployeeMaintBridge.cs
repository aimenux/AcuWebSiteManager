using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    public class EPEmployeeMaintBridge : PXGraph<EPEmployeeMaintBridge, EPEmployeeFSRouteEmployee>
    {
        #region Selects
        public PXSelect<EPEmployeeFSRouteEmployee> EPEmployeeFSRouteEmployeeRecords;
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<EPEmployeeFSRouteEmployee> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployeeFSRouteEmployee epEmployeeFSRouteEmployeeRow = e.Row;

            var graphEmployeeMaint = PXGraph.CreateInstance<EmployeeMaint>();

            if (epEmployeeFSRouteEmployeeRow.BAccountID != null)
            {
                graphEmployeeMaint.CurrentEmployee.Current = PXSelect<EPEmployee, 
                                                             Where<
                                                                EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
                                                             .Select(e.Cache.Graph, epEmployeeFSRouteEmployeeRow.BAccountID);
            }

            throw new PXRedirectRequiredException(graphEmployeeMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion
    }
}
