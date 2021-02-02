<%@ WebHandler Language="C#" Class="drilldown" %>

using System;
using System.Web;
using System.IO;
using PX.Common;
using PX.Web.UI;
using System.Runtime.Serialization.Formatters.Binary;

public class drilldown : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        var filters = PXContext.SessionTyped<PXSessionStateWebUI>().RedirectFilters[context.Request.Url.PathAndQuery];
        string filterLine = null;
        string displayName = string.Empty;

        if (filters != null && filters.Length > 0)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, filters);
                filterLine = HttpUtility.UrlEncode(Convert.ToBase64String(stream.ToArray()));
            }

            displayName = filters[0].Name;
        }
        var response = new
        {
            Redirect = new
            {
                ScreenId = context.Request.QueryString["screenId"],
                Filters = filterLine,
                DisplayName = displayName
            }
        };

        using (var sw = new StreamWriter(context.Response.OutputStream))
        {
            (new Newtonsoft.Json.JsonSerializer()).Serialize(sw, response);
        }
    }

    public bool IsReusable
{
    get
    {
        return false;
    }
}
}