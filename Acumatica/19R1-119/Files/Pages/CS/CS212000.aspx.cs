using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Export.Excel;
using System.IO;
//using PX.Web.Customization.API;

public partial class Pages_CS_CS212000 : PX.Web.UI.PXPage
{

	protected void Page_Load(object sender, EventArgs e)
	{
	//	this.UploadDialog.FileUploadFinished += (s, a) => ImportFile(a.UploadedFile);


	}

	//static void ImportFile(PX.SM.FileInfo fileInfo)
	//{
	//    using(var s = new MemoryStream(fileInfo.BinData))
	//    {
			
	//        Package p = new Package();
	//        var w = p.Read(s);
	//        ApiGraph.ImportWorkbook(w);

	//    }
		
	//}
}
