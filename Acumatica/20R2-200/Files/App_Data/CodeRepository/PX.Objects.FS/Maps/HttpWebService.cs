using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace PX.Objects.FS
{
    internal static class HttpWebService
    {
        /// <summary>
        /// Invokes the maps WebService.
        /// </summary>
        /// <param name="url">WebService URL.</param>
        internal static string MakeRequest(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            using (WebResponse resp = req.GetResponse())
            using (Stream respStream = resp.GetResponseStream())
            using (StreamReader reader = new StreamReader(respStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}