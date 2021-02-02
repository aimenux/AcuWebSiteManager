using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class OutputWaypoint
    {
        //The name of the referenced waypoint in the "waypoints" array.
        //In route steps that are not waypoints like origin and
        //destination it is set to "origin" and "destination" respectively
        public string name { get; set; }

        //The value represents the zero-based index of the referenced waypoint in the "waypoints" array.
        //In route steps that are not waypoints like origin and destination it is set to -1 and -2 respectively.
        public int number { get; set; }
    }
}
