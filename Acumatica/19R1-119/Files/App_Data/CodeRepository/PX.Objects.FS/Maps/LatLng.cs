using System.Globalization;
using System.Xml;

namespace PX.Objects.FS
{
  /// <summary>
  /// Class representing a latitude/longitude pair.
  /// </summary>
  public class LatLng
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LatLng"/> class.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    public LatLng(double latitude, double longitude)
    {
      this.latitude = latitude;
      this.longitude = longitude;
    }

    internal LatLng(XmlElement locationElement, XmlNamespaceManager nameSpace)
    {
        this.latitude = double.Parse(locationElement.SelectSingleNode(
            string.Format(".//{0}Latitude",ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText, CultureInfo.InvariantCulture);
        this.longitude = double.Parse(locationElement.SelectSingleNode(
            string.Format(".//{0}Longitude", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText, CultureInfo.InvariantCulture);
    }

    private double latitude;

    /// <summary>
    /// Gets the latitude.
    /// </summary>
    public double Latitude
    {
      get
      {
          return this.latitude;
      }
    }

    private double longitude;

    /// <summary>
    /// Gets the longitude.
    /// </summary>
    public double Longitude
    {
      get
      {
          return this.longitude;
      }
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return this.latitude.ToString() + ", " + this.longitude.ToString();
    }
  }
}
