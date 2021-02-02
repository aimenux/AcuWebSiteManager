using PX.Web.UI;
using System;
using System.Web.UI;

public partial class Page_AP301100 : PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (IsCallback)
		{
			return;
		}

		var form = (PXFormView)PXSplitContainer1.FindControl("edDocument");

		RegisterControlClientIdVariable(form, "AllowFiles", "allowFilesId");
		RegisterControlClientIdVariable(form, "AllowFilesMsg", "allowFilesMsgId");
		RegisterControlClientIdVariable(form, "AllowUploadFile", "allowUploadFileId");
		RegisterControlClientIdVariable(form, "FileID", "fileIDControlID");
		RegisterControlClientIdVariable(form, "RecognizedDataJson", "recognizedDataId");
		RegisterControlClientIdVariable(form, "VendorTermJson", "vendorTermId");
		RegisterControlClientIdVariable(edFeedback, "edFieldBound", "fieldBoundFeedbackId");
		RegisterControlClientIdVariable(edFeedback, "edTableRelated", "tableRelatedFeedbackId");
	}

	private void RegisterControlClientIdVariable(Control parent, string id, string variable)
	{
		var control = parent.FindControl(id);
		var script = string.Format("let {0} = '{1}';", variable, control.ClientID);

		Page.ClientScript.RegisterClientScriptBlock(GetType(), variable, script, true);
	}
}
