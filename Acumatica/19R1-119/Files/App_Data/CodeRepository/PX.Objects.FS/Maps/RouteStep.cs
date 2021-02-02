using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PX.Objects.FS
{
  /// <summary>
  /// Class representing a step within a leg of a route.
  /// </summary>
  public class RouteStep
  {
    internal RouteStep(XmlElement step, LatLng startLocation,XmlNamespaceManager nameSpace)
    {
        this.duration           = int.Parse(step.SelectSingleNode(
            string.Format(".//{0}TravelDuration", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText);
        this.durationDescr      = SharedFunctions.parseSecsDurationToString(duration);
        this.distance           = Math.Round(double.Parse(step.SelectSingleNode(
            string.Format(".//{0}TravelDistance", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText), 2);
        this.distanceDescr      = string.Format("{0} mi", distance);
        this.startLocation      = startLocation;
        this.endLocation        = new LatLng((XmlElement)step.SelectSingleNode(
            string.Format(".//{0}ManeuverPoint", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace), nameSpace);
        this.htmlInstructions   = step.SelectSingleNode(
            string.Format(".//{0}Instruction", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText;
         
        if (step.SelectSingleNode(string.Format(".//{0}TravelMode", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace) != null)
        {
            this.travelMode = step.SelectSingleNode(
                string.Format(".//{0}TravelMode", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText;
        }

        if (!string.IsNullOrEmpty(
            step.SelectSingleNode(
                string.Format(".//{0}Instruction", ID.MapsConsts.XML_SCHEMA_TAG),nameSpace).Attributes["maneuverType"].Value))
        {
            this.maneuver = step.SelectSingleNode(
                string.Format(".//{0}Instruction", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).Attributes["maneuverType"].Value;
        }
    }

    private int duration;
    private string durationDescr;

    /// <summary>
    /// Gets the duration of this step in seconds.
    /// </summary>
    public int Duration
    {
      get
      {
          return this.duration;
      }
    }

    /// <summary>
    /// Gets the duration of this step in an user-friendly format (e.g. minutes, hours, days, etc.).
    /// </summary>
    public string DurationDescr
    {
        get
        {
            return this.durationDescr;
        }
    }

    private double distance;
    private string distanceDescr;

    /// <summary>
    /// Gets the distance of this step in meters.
    /// </summary>
    public double Distance
    {
      get
      {
          return this.distance;
      }
    }

    /// <summary>
    /// Gets the distance of this step in an user-friendly format (e.g. Km, miles, feet, etc.).
    /// </summary>
    public string DistanceDescr
    {
        get
        {
            return this.distanceDescr;
        }
    }

    private LatLng startLocation;

    /// <summary>
    /// Gets the start location for this step.
    /// </summary>
    public LatLng StartLocation
    {
      get
      {
          return this.startLocation;
      }
    }

    private LatLng endLocation;

    /// <summary>
    /// Gets the end location of this step.
    /// </summary>
    public LatLng EndLocation
    {
      get
      {
          return this.endLocation;
      }
    }

    private string htmlInstructions;

    /// <summary>
    /// Gets the instructions for this step with HTML formatting.
    /// </summary>
    public string HtmlInstructions
    {
      get
      {
          return this.htmlInstructions;
      }
    }

    private string maneuver;

    /// <summary>
    /// Gets the instructions for this step.
    /// </summary>
    public string Maneuver
    {
        get
        {
            return this.maneuver;
        }
    }

    private string travelMode;

    /// <summary>
    /// Gets the travel mode for this step.
    /// </summary>
    public string TravelMode
    {
        get
        {
            return this.travelMode;
        }
    }
  }
}
