using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Web.UI;

public partial class Frames_PasswordRemind : Page
{
	protected void Page_Init(object sender, EventArgs e)
	{
		string[] companies = PXDatabase.Companies;
		if (companies.Length == 0 || PXDatabase.SecureCompanyID)
		{
			this.cmbCompany.Visible = false;
		}
		else if (this.cmbCompany.Items.Count == 0)
		{
			for (int i = 0; i < companies.Length; i++) this.cmbCompany.Items.Add(companies[i]);
			string company = Request.QueryString.Get("Company");
			if (company != null) this.cmbCompany.SelectedIndex = Int32.Parse(company);
		}
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (GetPostBackControl(this.Page) == btnSubmit && !String.IsNullOrEmpty(txtDummyCpny.Value))
			cmbCompany.SelectedValue = txtDummyCpny.Value;

		InitialiseRemindLink();

		edLogin.Attributes["placeholder"] = PXMessages.LocalizeNoPrefix(Msg.ReminderPageUserName);
		btnSubmit.Text = PXMessages.LocalizeNoPrefix(Msg.ReminderPageSubmitButton);
		lnkForgotLogin.Text = PXMessages.LocalizeNoPrefix(Msg.ReminderPageForgotCredentials);
	}

	protected void btnSubmit_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(edLogin.Text))
		{
			this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.InvalidLogin);
			return;
		}

		if (Request.QueryString.GetValues("ReturnUrl") == null)
		{
			this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.InvalidQueryString);
			return;
		}

		string link = PX.Common.PXUrl.SiteUrlWithPath().TrimEnd('/');
		link += "/Frames/Login.aspx";
		link += "?ReturnUrl=" + HttpUtility.UrlEncode(Request.QueryString.GetValues("ReturnUrl")[0]);

		string[] companies = PXDatabase.Companies;
		bool anySuccess = false;
		string errorMsg = null;
		if (companies.Length > 0)
		{
			if (!PXDatabase.SecureCompanyID)
				companies = new string[] { cmbCompany.SelectedItem.Value };
			
			foreach (string companyID in companies)
			{
				try
				{
					PXDatabase.ResetCredentials();
					PXLogin.SendUserPassword(
						edLogin.Text + "@" + companyID, link + "&cid=" + HttpUtility.UrlEncode(companyID), "gk");
					anySuccess = true;
				}
				catch (Exception ex)
				{
					errorMsg = ex.Message;
				}
			}
		}
		else
		{
			try
			{
				PXLogin.SendUserPassword(edLogin.Text, link, "gk");
				anySuccess = true;
			}
			catch (Exception ex)
			{
				errorMsg = ex.Message;
			}
		}
		if (anySuccess)
		{
			//lblMsg.ForeColor = System.Drawing.Color.Black;
			this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.PasswordSent);
			LiteralControl metaRefresh = new LiteralControl("<meta http-equiv=\"Refresh\" content=\"4;URL=" + link + "\" />");
			Page.Header.Controls.Add(metaRefresh);
		}
		else if (!String.IsNullOrEmpty(errorMsg))
		{
			this.Master.Message = errorMsg;
		}
	}

	protected void cmbCompany_OnSelectedIndexChanged(object sender, EventArgs e)
	{
		InitialiseRemindLink();
	}

	private void InitialiseRemindLink()
	{
		var requestValues = HttpUtility.ParseQueryString(Request.QueryString.ToString());
		if (Request.QueryString.Get("Company") != null)
			requestValues.Remove("Company");
		if (this.cmbCompany.SelectedIndex > 0)
			requestValues.Add("Company", this.cmbCompany.SelectedIndex.ToString());

		lnkForgotLogin.NavigateUrl = "~/Frames/LoginRemind.aspx?" + requestValues.ToString();
	}

	/// <summary>
	/// 
	/// </summary>
	private static Control GetPostBackControl(Page page)
	{
		Control control = null;
		string ctrlname = page.Request.Params.Get("__EVENTTARGET");
		if (ctrlname != null && ctrlname != string.Empty)
		{
			control = page.FindControl(ctrlname);
		}
		else
		{
			foreach (string ctl in page.Request.Form)
			{
				Control c = page.FindControl(ctl);
				if (c is System.Web.UI.WebControls.Button) { control = c; break; }
			}
		}
		return control;
	}
}
