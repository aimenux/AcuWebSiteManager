using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PX.Objects.FS
{
  /// <summary>
  /// Class representing the leg of a route.
  /// </summary>
  public class RouteLeg
  {
    internal RouteLeg(XmlElement leg, XmlNamespaceManager nameSpace)
    {
        this.startAddress   = leg.SelectSingleNode(
            string.Format(".//{0}StartLocation//{0}Address//{0}FormattedAddress", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText;
        this.endAddress     = leg.SelectSingleNode(
            string.Format(".//{0}EndLocation//{0}Address//{0}FormattedAddress", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText;
        this.startLocation  = new LatLng((XmlElement)leg.SelectSingleNode(
            string.Format(".//{0}ActualStart", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace), nameSpace);
        this.endLocation    = new LatLng((XmlElement)leg.SelectSingleNode(
            string.Format(".//{0}ActualEnd", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace), nameSpace);
        this.distance       = Math.Round(double.Parse(leg.SelectSingleNode(
            string.Format(".//{0}TravelDistance", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText), 2);
        this.distanceDescr  = string.Format("{0} mi", distance);
        this.duration       = int.Parse(leg.SelectSingleNode(
            string.Format(".//{0}TravelDuration", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText);
        this.durationDescr = SharedFunctions.parseSecsDurationToString(duration);
        XmlNodeList stepsXml      = leg.SelectNodes(
            string.Format(".//{0}ItineraryItem", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace);
        List<RouteStep> stepsList = new List<RouteStep>();
        foreach (XmlElement step in stepsXml)
        {
            if(stepsList.Count == 0)
                stepsList.Add(new RouteStep(step, this.startLocation,nameSpace));
            else
                stepsList.Add(new RouteStep(step, stepsList[stepsList.Count-1].EndLocation, nameSpace));
            }

        this.steps = stepsList.ToArray();
    }

    private string startAddress;

    /// <summary>
    /// Gets the start address for this leg.
    /// </summary>
    public string StartAddress
    {
      get
      {
          return this.startAddress;
      }
    }

    private string endAddress;

    /// <summary>
    /// Gets the end address for this leg.
    /// </summary>
    public string EndAddress
    {
      get
      {
          return this.endAddress;
      }
    }

    private LatLng startLocation;

    /// <summary>
    /// Gets the start location coordinates of this leg of the route.
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
    /// Gets the end location coordinates of this leg of the route.
    /// </summary>
    public LatLng EndLocation
    {
      get
      {
          return this.endLocation;
      }
    }

    private double distance;
    private string distanceDescr;

    /// <summary>
    /// Gets the distance of this leg in miles.
    /// </summary>
    public double Distance
    {
      get
      {
          return this.distance;
      }
    }

    /// <summary>
    /// Gets the distance of this leg in an user-friendly format (e.g. Km, miles, feet, etc.).
    /// </summary>
    public string DistanceDescr
    {
        get
        {
            return this.distanceDescr;
        }
    }

    private int duration;
    private string durationDescr;

    /// <summary>
    /// Gets the duration of this leg in seconds.
    /// </summary>
    public int Duration
    {
      get
      {
          return this.duration;
      }
    }

    /// <summary>
    /// Gets the duration of this leg in an user-friendly format (e.g. minutes, hours, days, etc.).
    /// </summary>
    public string DurationDescr
    {
        get
        {
            return this.durationDescr;
        }
    }

    private RouteStep[] steps;

    /// <summary>
    /// Gets the steps for this leg of the route.
    /// </summary>
    public RouteStep[] Steps
    {
      get
      {
          return this.steps;
      }
    }
  }
}
