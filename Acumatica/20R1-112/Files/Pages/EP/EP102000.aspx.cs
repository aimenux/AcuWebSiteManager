using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PR;
using PX.Web.UI;
using System;

public partial class Page_EP102000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!PXAccess.FeatureInstalled<FeaturesSet.payrollModule>())
			return;

		PREarningTypeMaint graph = PXGraph.CreateInstance<PREarningTypeMaint>();
		string url = PXDataSource.getMainForm(graph.GetType());
		if (url == null)
			return;

		graph.Unload();
		PXContext.Session.RedirectGraphType[PXUrl.ToAbsoluteUrl(url)] = graph.GetType();
		Response.Redirect(this.ResolveUrl(url));
	}
}
