using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    public class EPEmployeeMaintBridge : PXGraph<EPEmployeeMaintBridge, EPEmployeeFSRouteEmployee>
    {
        #region Selects
        public PXSelect<EPEmployeeFSRouteEmployee> EPEmployeeFSRouteEmployeeRecords;
        #endregion

        #region Event Handlers
        protected virtual void EPEmployeeFSRouteEmployee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployeeFSRouteEmployee epEmployeeFSRouteEmployeeRow = (EPEmployeeFSRouteEmployee)e.Row;
            var graphEmployeeMaint = PXGraph.CreateInstance<EmployeeMaint>();

            if (epEmployeeFSRouteEmployeeRow.BAccountID != null)
            {
                graphEmployeeMaint.CurrentEmployee.Current = PXSelect<EPEmployee, 
                                                             Where<
                                                                EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
                                                             .Select(cache.Graph, epEmployeeFSRouteEmployeeRow.BAccountID);
            }

            throw new PXRedirectRequiredException(graphEmployeeMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion
    }
}
