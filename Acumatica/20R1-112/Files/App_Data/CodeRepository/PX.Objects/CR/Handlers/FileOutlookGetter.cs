using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using PX.Data;

namespace PX.Objects.CR.Handlers
{
    public class FileOutlookGetter : IHttpHandler, IRequiresSessionState
    {
        private const string _resourceAddinManifest = "PX.Objects.CR.Handlers.AddinManifest.xml";
        private const string _outputFileName = "OutlookAddinManifest.xml";

        public bool IsReusable
        {
            get { return true; }
        }

        private static bool IsSecureConnection()
        {
            return
               HttpContext.Current.Request.IsSecureConnection 
               || string.Equals(HttpContext.Current.Request.Headers["X-Forwarded-Proto"],
                    "https",
                    StringComparison.InvariantCultureIgnoreCase);
        }
        public void ProcessRequest(HttpContext context)
        { 
           if (!IsSecureConnection())
                throw new PXSetPropertyException(Messages.OutlookPluginHttps);
			if (!PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.outlookIntegration>())
				throw new PXSetPropertyException(Messages.OutlookFeatureNotInstalled);
			context.Response.Clear();
            context.Response.Cache.SetCacheability(HttpCacheability.Private);
            context.Response.Cache.SetValidUntilExpires(true);
            context.Response.AddHeader("Connection", "Keep-Alive");
            context.Response.BufferOutput = false;
            foreach (string ck in context.Request.Cookies.AllKeys)
                context.Response.Cookies.Add(context.Request.Cookies[ck]);
            context.Response.AddHeader("content-type", "application/octet-stream");
            context.Response.AddHeader("Accept-Ranges", "bytes");

            string text = null;
            using (var fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(_resourceAddinManifest))
            {
                using (var sR = new StreamReader(fs))
                {
                    string address = GetSiteBaseUrl(context.Request);
                    text = sR.ReadToEnd().Replace("{domain}", address.Replace("http://", "https://"));
                }

            }
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + _outputFileName);
            context.Response.ContentType = "application/xml";
            context.Response.BinaryWrite(buffer);
        }

        private static string GetSiteBaseUrl(HttpRequest request)
        {
            var host = request.Headers["X-Host"] ?? request.Headers["X-Forwarded-Host"] ?? request.Headers["Host"];
            var scheme = request.Headers["X-Scheme"] ?? request.Headers["X-Forwarded-Proto"] ?? request.Url.Scheme;
            var path = string.Join("", request.Url.Segments.Take(request.Url.Segments.Length - 1)).Trim('/') + "/";
            var url = $"{scheme}://{host}/{path}";
            return url;
        }
    }
}
