using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    //List capacities for the vehicle.
    //Each capacity is identified by a user-defined name(e.g.: "weight","pounds", "seats", "cm3", "crates", etc) 
    //and an integer value representing the maximum allowed capacity.
    public class Capacity
    {
        public int pounds { get; set; }

        public int cm3 { get; set; }
    }
}
