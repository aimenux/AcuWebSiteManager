using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class Break
    {
        //Minimum allowed break start time in seconds since midnight (00:00).
        //The allowed range can span a maximum of a 7-days period[0 – 604800].
        public int startTimeSec { get; set; }

        //Maximum allowed break stop time in seconds since midnight(00:00).
        //Must be >= startTimeSec.
        //The allowed range can span a maximum of a 7-days period[0 – 604800].
        public int stopTimeSec { get; set; }

        //Break duration in seconds.
        //The break can be scheduled to start at any time between startTimeSec and stopTimeSec, and lasts for durationSec.
        //Example: startTimeSec = 43200 (12:00), stopTimeSec = 45000 (12:30) 
        //and durationSec = 3600(1 hour) means: schedule a 1-hour break starting at any time between 12:00 and 12:30.
        public int durationSec { get; set; }
    }
}
