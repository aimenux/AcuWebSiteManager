using System;
using PX.Api;

public partial class Api_Menu : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{

//		CheckPaused.Checked = TraceStorage.Paused;
	}
	protected void CheckPaused_CheckedChanged(object sender, EventArgs e)
	{
		//TraceStorage.Paused = !TraceStorage.Paused;
		//CheckPaused.Checked = TraceStorage.Paused;

	}
	protected void ButtonClearTrace_Click(object sender, EventArgs e)
	{
		//TraceStorage.ClearLog();
	}
	protected void ButtonClearCaches_Click(object sender, EventArgs e)
	{
		//TraceStorage.ClearCaches();
	}
}
