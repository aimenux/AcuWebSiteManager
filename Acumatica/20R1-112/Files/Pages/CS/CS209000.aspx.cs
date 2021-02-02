using System;
using PX.Data;
using PX.Web.UI;
using PX.Objects.CS;

public partial class Page_CS209000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!PXAccess.FeatureInstalled<FeaturesSet.payrollModule>())
			HideUnpaidTimeControls();
	}

	private void HideUnpaidTimeControls()
	{
		PXLabel unpaidTimeLabel = (PXLabel)tab.FindControl("PXUnpaidTimeLabel");
		if (unpaidTimeLabel != null)
			unpaidTimeLabel.Visible = false;

		PXLayoutRule layoutRule = (PXLayoutRule)tab.FindControl("PXUnpaidTimeLayoutRule");
		if (layoutRule != null)
			layoutRule.StartColumn = false;
	}
}
