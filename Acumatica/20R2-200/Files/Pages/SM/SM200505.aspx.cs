using System;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;
using PX.Data;
using PX.SM;

public partial class Page_SM200505 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		form.InvalidateDataControls();
	}


	protected void Page_Load(object sender, EventArgs e)
	{
		FillColors((PXDropDown)this.form.DataControls["edBorderColor"]);
		FillColors((PXDropDown)this.form.DataControls["edHeaderFontColor"]);
		FillColors((PXDropDown)this.form.DataControls["edHeaderFillColor"]);
		FillColors((PXDropDown)this.form.DataControls["edBodyFontColor"]);
		FillColors((PXDropDown)this.form.DataControls["edBodyFillColor"]);
		FillFonts((PXDropDown)this.form.DataControls["edBodyFont"]);
		FillFonts((PXDropDown)this.form.DataControls["edHeaderFont"]);
		FillFontSizes((PXDropDown)this.form.DataControls["edBodyFontSize"]);
		FillFontSizes((PXDropDown)this.form.DataControls["edHeaderFontSize"]);
	}
	
	private void FillColors(PXDropDown dd)
	{
		string[] colors = PXSpecialResources.ColorNames;
		dd.Items.Clear();
		foreach (string n in colors)
		{
			string nLocalized = n;
			PXLocalizerRepository.SpecialLocalizer.LocalizeColorName(ref nLocalized);

			dd.Items.Add(new PXListItem(nLocalized, n));
		}
	}

	private void FillFonts(PXDropDown dd)
	{
		dd.Items.Clear();
		string[] fonts = PXSpecialResources.FontNames;
		foreach (string ff in fonts)
		{
			string ffLocalized = ff;
			PXLocalizerRepository.SpecialLocalizer.LocalizeFontName(ref ffLocalized);

			dd.Items.Add(new PXListItem(ffLocalized, ff));
		}
	}

	private void FillFontSizes(PXDropDown dd)
	{
		int[] size = new int[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
		dd.Items.Clear();
		foreach (int s in size) dd.Items.Add(new PXListItem(string.Empty, s.ToString()));
	}

	protected void form_DataBound(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(FrameHelper.GetHelpApiHref(false)) || string.IsNullOrEmpty(FrameHelper.GetHelpApiKey()))
		{
			var chk = this.form.DataControls["chkUseMLSearch"] as PXCheckBox;
			if (chk != null) chk.Hidden = true;
		}
	}
}
