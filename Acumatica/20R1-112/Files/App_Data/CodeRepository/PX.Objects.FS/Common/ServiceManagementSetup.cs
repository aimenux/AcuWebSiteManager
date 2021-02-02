using PX.Data;

namespace PX.Objects.FS
{
    public class ServiceManagementSetup
    {
        /// <summary>
        /// Returns FSSetup record.
        /// </summary>
        public static FSSetup GetServiceManagementSetup(PXGraph graph)
        {
            return PXSelect<FSSetup>.Select(graph);
        }

        /// <summary>
        /// Returns FSRouteSetup record.
        /// </summary>
        public static FSRouteSetup GetServiceManagementRouteSetup(PXGraph graph)
        {
            return PXSelect<FSRouteSetup>.Select(graph);
        }

        /// <summary>
        /// Return if ManageRooms is active or inactive on the Service Management.
        /// </summary>
        public static bool IsRoomManagementActive(PXGraph graph, FSSetup fsSetupRow = null)
        {
            if (fsSetupRow == null)
            {
                fsSetupRow = GetServiceManagementSetup(graph);
            }

            if (fsSetupRow != null) 
            {
                return (bool)fsSetupRow.ManageRooms;
            }

            return false;
        }
    }
}
