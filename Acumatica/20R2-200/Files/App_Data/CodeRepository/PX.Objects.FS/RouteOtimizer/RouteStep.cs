using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class RouteStep
    {
        //A zero-based counter indicating the step number in this vehicle's route.
        public int stepNumber { get; set; }

        //Identification of the waypoint associated with this step.
        //If this step represents the origin or the destination depot this field assumes particularvalues.
        public OutputWaypoint waypoint { get; set; }

        //Latitude coordinate specified using the WGS 84 reference frame(e.g.: 45.397204).
        public double latitude { get; set; }

        //Longitude coordinate specified using the WGS 84 reference frame(e.g.: 9.251765).
        public double longitude { get; set; }

        //Delivery time window constraint for this route step (i.e.: the visited Waypoint’s matching time window).
        //Set to the Vehicle’s working time window for origins and empty for the end destination.
        public TimeWindow timeWindow { get; set; }

        //Arrival time in seconds since midnight.
        public int arrivalTimeSec { get; set; }

        //This field is > 0 if the arrival time is earlier than the
        //time window start time(in which case the vehicle has to wait until the time window start time).
        public int idleTimeSec { get; set; }

        //Time when the goods are delivered, in seconds since midnight.
        public int serviceStartTimeSec { get; set; }

        //Departure time in seconds since midnight.
        public int departureTimeSec { get; set; }

        //Time in seconds required to reach the next route step.
        //Always set to 0 for the last step.
        public int nextStepDriveTimeSec { get; set; }

        //Meters between the current and the next route step.
        //Always set to 0 for the last step
        public int nextStepDistanceMt { get; set; }

        //This field is > 0 if the driving time between the previous step and this step is interrupted by one or
        //more Vehicle-defined Breaks(e.g.: lunch break) and amounts to the sum of the durationSec of all involved Breaks.
        //Note that arrivalTimeSec and all related times already take Vehicle Breaks into account.
        //This field is omitted if no Vehicle Breaks are defined in the input request.
        public int driveBreakTimeSec { get; set; }

        //Lists all breaks occurring during the driving time between the previous step and this step.
        //This field is omitted if no Vehicle Breaks are defined in the input request.
        public List<RouteStepBreak> driveBreaks { get; set; }

        //This field is > 0 if the service time at this step is interrupted by one or more Vehicle-defined Breaks
        //(e.g.: lunch break) and amounts to the sum of the durationSec of all involved Breaks.
        //Note that departureTimeSec already take Vehicle Breaks into account.
        //Note that the optimization engine will try to avoid
        //placing Breaks during a service time if at all possible.
        //This field is omitted if no Vehicle Breaks are defined in the input request.
        public int serviceBreakTimeSec { get; set; }

        //Lists all breaks interrupting the service time at this tep.
        ///This field is omitted if no Vehicle Breaks are defined in the input request.
        public List<RouteStepBreak> serviceBreaks { get; set; }

        //Vehicle capacities consumed up to this step.
        public RouteStats cumulativeRouteStats { get; set; }

        //Cost and time statistics cumulated from route start to this step.
        public Capacity cumulativeCapacityMap { get; set; }
    }
}
