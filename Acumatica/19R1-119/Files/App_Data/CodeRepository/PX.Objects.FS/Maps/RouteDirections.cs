using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace PX.Objects.FS
{
  /// <summary>
  /// Static class providing methods to retrieve directions between locations.
  /// </summary>
  public static class RouteDirections
  {
    /// <summary>
    /// Gets a route from the Google Maps Directions web service.
    /// </summary>
    /// <param name="optimize">If set to <c>true</c> optimize the route by re-ordering the locations to minimize the
    /// time to complete the route.</param>
    public static Route GetRoute(string optimize , string apiKey, params GLocation[] locations)
    {
        if (locations.Length < 2)
        {
            throw new ArgumentException(TX.Error.MAPS_MISSING_REQUIRED_PARAMETERS);
        }

        /*Points inside a route are identified as: wp.n and vwp.n where wp demarks start and ed location and vwp demarks
            intermidle points inside route  
        */
        string reqStr = null;

        for (int i = 0; i < locations.Length; i++)
        {
            reqStr += "wp." + i.ToString() + "=";
            reqStr += locations[i].ToString() + "&";
        }

        if (!string.IsNullOrEmpty(optimize))
            reqStr += "optimize=" + optimize + "&"; // Options for optimize route are: distance, time [default], timeWithTraffic, timeAvoidClosure

        reqStr = RemoveSpecialCharacters(reqStr);

        string response = null;
        int attempts = 0;
        string status = null;
        try
        {
            while (attempts < 3)
            {
                response = HttpWebService.MakeRequest(
                    TX.MapsWebService.URL_PREFIX + reqStr + "key=" + apiKey);
                status = GetStatus(response);

                if ( ( !string.IsNullOrEmpty(status) ) && ( !string.Equals(status, ID.MapsStatusCodes.TOO_MANY_REQUESTS) ) )
                {
                    break;
                }
            
                attempts++;
                status = null;
                Thread.Sleep(2000);
            }
        }catch
        {
            return null;
        }

        return ParseResponse(response);
    }

    private static string RemoveSpecialCharacters(string reqStr)
    {
        return reqStr.Replace("#", string.Empty);
    }
 
    private static Route ParseResponse(string response)
    {
        XmlDocument xmlDoc = new XmlDocument() { XmlResolver = null };
        xmlDoc.LoadXml(response);
        //The XML document comes with a schema. So, was needed load the schema used by the site
        XmlNamespaceManager nameSpace = new XmlNamespaceManager(xmlDoc.NameTable);
        SharedFunctions.GenerateXmlNameSpace(ref nameSpace);
        string status = xmlDoc.SelectSingleNode(
            string.Format(".//{0}StatusDescription",ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText;

        if (status != ID.MapsStatusCodes.OK)
        {
            throw new RoutingException(GetStatusMessage(status));
        }

        return new Route(xmlDoc, nameSpace);
    }
    
    /// <summary>
    /// Returns the status code from a google map response.
    /// </summary>
    /// <param name="response">The google map response.</param>
    private static string GetStatus(string response)
    {
        XmlDocument xmlDoc = new XmlDocument() { XmlResolver = null };
        xmlDoc.LoadXml(response);
        //The XML document comes with a schema. So, was needed load the schema used by the site
        XmlNamespaceManager nameSpace = new XmlNamespaceManager(xmlDoc.NameTable);
        SharedFunctions.GenerateXmlNameSpace(ref nameSpace);
        XmlElement result = (XmlElement)xmlDoc.SelectSingleNode(
        string.Format(".//{0}StatusDescription", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace);
        string status = result.InnerText;
        return status;
    }

    private static string GetStatusMessage(string status)
    {
      switch (status)
      {
        case ID.MapsStatusCodes.OK:
            return TX.Error.MAPS_STATUS_CODE_OK;
        case ID.MapsStatusCodes.CREATED:
            return TX.Error.MAPS_STATUS_CODE_CREATED;
        case ID.MapsStatusCodes.ACCEPTED:
            return TX.Error.MAPS_STATUS_CODE_ACCEPTED;
        case ID.MapsStatusCodes.BAD_REQUEST:
            return TX.Error.MAPS_STATUS_CODE_BAD_REQUEST;
        case ID.MapsStatusCodes.UNAUTHORIZED:
            return TX.Error.MAPS_STATUS_CODE_UNAUTHORIZED;
        case ID.MapsStatusCodes.FORBIDDEN:
            return TX.Error.MAPS_STATUS_CODE_FORBIDDEN;
        case ID.MapsStatusCodes.NOT_FOUND:
            return TX.Error.MAPS_STATUS_CODE_NOT_FOUND;
        case ID.MapsStatusCodes.TOO_MANY_REQUESTS:
            return TX.Error.MAPS_STATUS_CODE_TOO_MANY_REQUESTS;
        case ID.MapsStatusCodes.INTERNAL_SERVER_ERROR:
            return TX.Error.MAPS_STATUS_CODE_INTERNAL_SERVER_ERROR;
        default:
            return TX.Error.MAPS_STATUS_CODE_SERVICE_UNAVAILABLE; //"UNKNOWN_ERROR"              
      }      
    }
  }
}
