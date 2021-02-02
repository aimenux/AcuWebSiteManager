using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class Vehicle
    {
        //Unique vehicle identifier.
        //Note: The following substrings/characters are NOT
        //allowed: '..', '/', '\', '&', '?', '#', '='
        public string name { get; set; }

        //List of maximum capacities for the vehicle.
        //Each capacity is identified by a user-defined name (e.g.: "weight", "pounds", "seats", "cm3","crates", etc) 
        //and an integer value representing the maximum allowed capacity.
        public Capacity maxCapacityMap { get; set; }

        //The location of the start point for this vehicle.
        public RouteLocation origin { get; set; }

        //If provided, defines the vehicle availability time range.
        //If not specified, the vehicle will always be considered as available.
        //The vehicle's time window will take into account the service time at the last visited waypoint and, 
        //if a destination is specified, the driving time to get there(that is, this API will never schedule a route so that
        //the scheduled time of arrival at the last waypoint + the service time at that waypoint + the driving time to
        //reach the destination exceeds the vehicle's time window).
        //Note: both start time and stop time of this time window must be in the single day interval [0s, 86400s].
        public TimeWindow timeWindow { get; set; }

        //If provided, defines the location of the end-destination for this vehicle.
        //If not specified, the route calculated for this vehicle terminates with the last visited waypoint.
        public RouteLocation destination { get; set; }

        //List of textual tags associated to this vehicle.
        //Used together with waypoint's tagsExclude, tagsIncludeAnd and tagsIncludeOr fields, tags enable
        //configuring complex vehicle-towaypoint association constraints.
        public List<string> tags { get; set; }

        //If true breaks can be scheduled during waypoints' service time. 
        //If false breaks are only allowed during driving time.
        public bool allowBreakService { get; set; }

        //List of breaks defined for this vehicle.Defaults to an empty array. 
        //If multiple Breaks are specified, they must be non-overlapping. 
        //If a Time Window is specified for this  vehicle then all defined breaks must be fully contained in the Time
        //Window(i.e.: break's startTimeSec > timewindow's startTimeSec AND
        //break's stopTimeSec + durationSec < timewindow's stopTimeSec).
        //This parameter can’t be set if the “dynamicBreaks” parameter is defined too.
        public List<Break> breaks { get; set; }
    }
}
