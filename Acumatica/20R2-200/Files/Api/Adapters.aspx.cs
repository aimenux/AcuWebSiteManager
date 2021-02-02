using System;
using PX.Api;

public partial class Api_Adapters : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if(IsPostBack)
			return;

		//EditDocument.Text = DescriptionStorage.Xml;
	}
	protected void SaveButton_Click(object sender, EventArgs e)
	{
		//DescriptionStorage.Xml = EditDocument.Text;
	}
	protected void ImportButton_Click(object sender, EventArgs e)
	{
		//DescriptionStorage.ImportTrace();
		//EditDocument.Text = DescriptionStorage.Xml;



	}
	protected void ButtonTestExport_Click(object sender, EventArgs e)
	{
		//TestDataset.Export(@"d:\tmp\_export.xml");
	}
	protected void ButtonTestImport_Click(object sender, EventArgs e)
	{
		//TestDataset.Import(@"d:\tmp\_export.xml", @"d:\tmp\_import_log.xml");

	}
}
