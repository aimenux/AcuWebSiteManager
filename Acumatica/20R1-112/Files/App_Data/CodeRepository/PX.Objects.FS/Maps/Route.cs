using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PX.Objects.FS
{
  /// <summary>
  /// Class representing a Route containing directions between an origin and a final destination.
  /// </summary>
  public class Route
  {
    internal Route(XmlDocument route, XmlNamespaceManager nameSpace)
    {
      XmlNodeList legsXml = route.DocumentElement.SelectNodes(
          string.Format(".//{0}RouteLeg", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace);
      List<RouteLeg> legsList = new List<RouteLeg>();
      foreach (XmlElement leg in legsXml)
      {
        legsList.Add(new RouteLeg(leg, nameSpace));
      }

      this.legs = legsList.ToArray();
    }

    private RouteLeg[] legs;

    /// <summary>
    /// Gets the legs of this Route.
    /// </summary>
    public RouteLeg[] Legs
    {
      get
      {
          return this.legs;
      }
    }

    /// <summary>
    /// Gets the duration of the Route in seconds.
    /// </summary>
    public int Duration
    {
      get
      {
        int duration = 0;
        for (int i = 0; i < this.legs.Length; i++)
        {
            duration += this.legs[i].Duration;
        }

        return duration;
      }
    }

    /// <summary>
    /// Gets the distance of the Route in miles.
    /// </summary>
    public double Distance
    {
      get
      {
        double distance = 0;
        for (int i = 0; i < this.legs.Length; i++)
        {
            distance += this.legs[i].Distance;
        }

        return distance;
      }
    }
  }
}
