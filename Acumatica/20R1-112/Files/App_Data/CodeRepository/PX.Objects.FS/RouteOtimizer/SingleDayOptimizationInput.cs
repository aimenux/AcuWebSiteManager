using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class SingleDayOptimizationInput
    {
        //Set to true to balance route duration, set to false to disable 
        //route duration balancing and minimize the overall duration of all routes.
        public bool balanced { get; set; }

        //Array of objects defining vehicle properties like
        //costs, working time window and capacity
        public List<Vehicle> vehicles { get; set; }

        //Array of objects defining waypoint properties like
        //latitude, longitude and delivery time window.
        public List<Waypoint> waypoints { get; set; }
    }
}
