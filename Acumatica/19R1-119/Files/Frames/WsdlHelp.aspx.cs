using System.Web;
using System.Web.UI;
using PX.Api.Soap.Screen;

public partial class Frames_WsdlHelp : Page
{
	public override void ProcessRequest(HttpContext context)
	{
		WsdlBuilder.ProcessHelpRequest(context);
	}
}
