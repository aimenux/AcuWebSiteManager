using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Pages_CS_CS212010 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{

	}
	protected void UploadDialog_FileUploadFinished(object sender, PX.Web.UI.PXFileUploadEventArgs e)
	{
		//PX.Web.Customization.API.ApiImportGraph.Upload(e.UploadedFile.Name, e.UploadedFile.Comment, e.UploadedFile.BinData);
	}
}
