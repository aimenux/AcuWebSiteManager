using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.Data.Maintenance;
using PX.Data.Update;
using PX.Data;

public partial class Frames_Update : Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		this.Response.AppendHeader("Refresh", "10");

		this.lblMessage.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.SiteUnderMaintenanceCannotAccess);
		this.lblPersentCaption.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.PersentCaption) + " ";
		this.lblActionCaption.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.ActionCaption) + " ";
		this.lblAction.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.ActionDatabaseUpdate);
		this.lblDatabaseNameCaption.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.DatabaseNameCaption) + " ";
		this.lblDatabaseVersionCaption.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.DatabaseVersionCaption) + " ";
		this.lblSiteVersionCaption.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.SiteVersionCaption) + " ";
		this.lblQuestion.Text = PXMessages.LocalizeNoPrefix(PX.Data.Update.Messages.Question) + " ";

		if (!PX.Data.Update.PXUpdateHelper.CheckUpdateLock() && PX.Data.Maintenance.PXSiteLockout.GetStatus() != PXSiteLockout.Status.Locked)
		{
			this.lblMessage.Text = PX.Data.PXMessages.Localize(PX.Data.InfoMessages.SiteMaintetanceComplite);
			PX.Data.Redirector.RedirectPage(this.Context, PXUrl.MainPagePath);
		}

		if (PXSiteLockout.GetStatus() == PXSiteLockout.Status.Locked)
		{
			this.lblMessage.Text += "<br/>" + string.Format(
				PXMessages.Localize(PX.Data.Update.Messages.LockoutReason), PXSiteLockout.Message);
		}

		PXUpdateStatusResult result = PXUpdateHelper.GetUpdateStatus();
		if ((result.ProcessStatus == PX.Data.PXLongRunStatus.InProcess))
		{
			if (result.UpdateStatus != null)
			{
				this.statusTable.Visible = true;
				this.lblPersent.Text = result.UpdateStatus.Peresent.ToString() + "%";
				this.lblAction.Text = PXMessages.LocalizeNoPrefix(result.UpdateStatus.Message);
			}
			if (result.UpdateQuetion != null)
			{
				this.questionTable.Visible = true;
				this.lblQuestion.Text = result.UpdateQuetion.Question;
				this.lblDatabaseVersion.Text = result.UpdateQuetion.DatabaseVersion;
				this.lblSiteVersion.Text = result.UpdateQuetion.AssemblyVersion;

				if (!String.IsNullOrEmpty(result.UpdateQuetion.DatabaseName))
				{
					this.applicationTable.Visible = true;
					this.lblDatabaseName.Text = result.UpdateQuetion.DatabaseName;
				}
			}
		}
		else
		{
			this.statusTable.Visible = false;
			this.questionTable.Visible = false;
			this.applicationTable.Visible = false;
		}
	}

	protected void btnYes_OnClick(object sender, EventArgs e)
	{
		PX.Data.Update.PXUpdateHelper.SetUpdateAnswer(WebDialogResult.Yes);
		this.statusTable.Visible = false;
		this.questionTable.Visible = false;
	}
	protected void btnNo_OnClick(object sender, EventArgs e)
	{
		PX.Data.Update.PXUpdateHelper.SetUpdateAnswer(WebDialogResult.No);
		this.statusTable.Visible = false;
		this.questionTable.Visible = false;
	}
}
