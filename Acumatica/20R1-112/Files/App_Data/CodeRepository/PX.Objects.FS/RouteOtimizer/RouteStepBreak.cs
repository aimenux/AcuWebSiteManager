using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class RouteStepBreak
    {
        //Integer Break start time in seconds since midnight.
        public int breakTimeSec { get; set; }

        //Integer Break duration in seconds.
        public int durationSec { get; set; }
    }
}
