using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class OuputVehicle
    {
        //The value represents the zero-based index of the referenced vehicles in the "vehicles" array.
        public int number { get; set; }

        //The name of the referenced vehicle in the "vehicles" array.
        public string name { get; set; }

        //Always zero(0) for single-day optimizations.
        public int day { get; set; }
    }
}
