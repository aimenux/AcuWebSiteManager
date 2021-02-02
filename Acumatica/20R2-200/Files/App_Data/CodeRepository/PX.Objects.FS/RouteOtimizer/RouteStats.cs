using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class RouteStats
    {
        //Working time.
        //Computed as arrival time to destination - departure time.
        public int workTimeSec { get; set; }

        //Time in seconds elapsed during breaks
        public int breakTimeSec { get; set; }

        //Driving time in seconds.Includes clusterEnterTimeSec,
        //clusterExitTimeSec and perStopSetupTimeSec
        public int driveTimeSec { get; set; }

        //Idle time in seconds(i.e.: sum of all idle times). Idle time happens when, as per schedule, the Vehicle 
        //arrives early at a waypoint and must wait, idle, for the waypoint's time window to open.
        //Idle time is computed according to this formula: idleTimeSec = workTimeSec - drivingTimeSec - serviceTimeSec - breakTimeSec
        public int idleTimeSec { get; set; }

        //Service time in seconds(i.e.: sum of all waypoint service times). Includes clusterSetupTimeSec.
        public int serviceTimeSec { get; set; }

        //Mileage in meters
        public int distanceMt { get; set; }

        //Time in seconds elapsed during vehicle stops. 
        public int perStopSetupTimeSec { get; set; }

        //Cost paid related to vehicle stop
        public double perStopSetupCost { get; set; }
    }
}
