using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Customization;

public partial class Api_OpenItem : PX.Web.UI.PXPage
{
	//public override void ProcessRequest(HttpContext context)
	//{

	//    var proc = new System.Diagnostics.Process();
	//    proc.StartInfo.Domain = "ProjectX";
	//    proc.StartInfo.UserName = "aignatine";
	//    var psw = new SecureString();
	//    foreach (char c in "")
	//    {
	//        psw.AppendChar(c);
	//    }

	//    proc.StartInfo.Password = psw;

	//    proc.StartInfo.UseShellExecute = false;
	//    proc.StartInfo.FileName = @"";
	//    proc.StartInfo.Arguments = EXEC_ARGS;
	//    proc.StartInfo.RedirectStandardOutput = true;
	//    proc.Start();

	//    //Action a = () =>
	//                {
	//                    PX.Automation.VSNavigator.OpenItem2(@"D:\_VSS_BASE\WebSites\Pure\Site\Pages\RQ\RQ505000.aspx.cs",
	//                                                        null);
	//                };
	//    //a.BeginInvoke(null, null);
	//}

	public override void ProcessRequest(HttpContext context)
	{
		if(context.Request.QueryString["action"] == "assistant")
		{
			context.Response.ContentType = "application/executable";
			context.Response.AppendHeader("Content-Disposition", "attachment; filename=AcumaticaAssistant.exe");
			context.Response.WriteFile(HostingEnvironment.MapPath("~/App_Data/Assistant/AcumaticaAssistant.exe"), true);
			context.Response.End();
			return;
		}
		base.ProcessRequest(context);
	}
    protected void Page_Load(object sender, EventArgs e)
    {  
		var id = Request.QueryString["id"];

    	var href = Request.QueryString["page"];

		if (href == null)
			return;

    	href = Regex.Replace(href, @"\(W\(\d+\)\)", "");
		
  
		var u = new Uri(href);
		var path = u.PathAndQuery.Split('?').First();
		//var filePath = HostingEnvironment.MapPath(path);
    	
    	var baseHref = "http://127.0.0.1:8081/?path=";
    	var cmd = Request.QueryString["cmd"];
    	var dataField = Request.QueryString["dataField"];

		var n = new PXCodeNavigator(path, id, cmd, dataField);
		
    	foreach (CodeLine codeLine in n.Result)
    	{

			if (codeLine.FilePath == null)
			{
				continue;

			}

			//this.MenuContainer.Controls.Add(new LiteralControl(codeLine.Description + " "));
    		var a = new HtmlAnchor
    		        	{
    		        		Target = "MenuAction",
							InnerText = codeLine.Description,
							Title = codeLine.Hint,
							HRef = baseHref + HttpUtility.UrlPathEncode(codeLine.FilePath)

    		        	};
			a.Attributes.Add("onclick", "window.parent.ClosePageInfo();");
			a.Attributes.Add("class", "PageInfoMenuItem");

			if(codeLine.Line.HasValue)
				a.HRef += "&line=" + codeLine.Line.Value;


			MenuContainer.Controls.Add(a);

			//this.MenuContainer.Controls.Add(new LiteralControl("<br/>"));
    	}



		
			

    }
}