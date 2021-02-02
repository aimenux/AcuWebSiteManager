using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class TimeWindow
    {
        //Time window start time in seconds since midnight(00:00).
        //The allowed range can span a maximum of a 7-days period[0 – 604800].
        public int startTimeSec { get; set; }

        //Time window stop time in seconds since midnight(00:00). Must be >= startTimeSec.
        //The allowed range can span a maximum of a 7-days period[0 – 604800].
        public int stopTimeSec { get; set; }
    }
}
