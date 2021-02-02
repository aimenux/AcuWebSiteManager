using System;

public partial class Api_Bind : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		//var action = Request["action"];
		//if(action == "trace")
		//{
		
		//    PX.Api.TraceStorage.BeginProfile(Request["s"], Request["g"]);
		//    Response.Redirect("~/Api/Trace.aspx");
		//}

		//var info = PX.Api.TraceStorage.LastRequest;
		//if(info == null)
		//{
		//    LastRequest.InnerText = "Waiting request";
		//    LinkTrace.Visible = false;
		//    return;
		//}

		//LastRequest.InnerText = info.Url + "\n" + info.GraphName;
		//LinkTrace.NavigateUrl = String.Format("~/Api/Bind.aspx?action=trace&s={0}&g={1}", info.AspSessionId, info.GraphName);



	}
}
