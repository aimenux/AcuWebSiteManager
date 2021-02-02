using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class RouteLocation
    {
        //Latitude coordinate specified using the WGS 84 reference frame(e.g.: 45.397204).
        public double latitude { get; set; }

        //Longitude coordinate specified using the WGS 84 reference frame(e.g.: 9.251765).
        public double longitude { get; set; }
    }
}
