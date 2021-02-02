using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.SM;
using PX.Web.UI;
using PX.Web.Controls;

public partial class Page_CR306015 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupWidth = 980;
		this.Master.PopupHeight = 650;
	}

	protected void Page_Load(object sender, EventArgs e)
	{

		bool allowCustomItems = GetAllowCustomItems();
		FillEmailSelector((PXMultiSelector)this.message.FindControl("edMailTo"), allowCustomItems);
		FillEmailSelector((PXMultiSelector)this.message.FindControl("edMailCc"), allowCustomItems);
		FillEmailSelector((PXMultiSelector)this.message.FindControl("edMailBcc"), allowCustomItems);

		/*Control edProject = tab.FindControl("edProject");
		if (edProject != null && ((PX.Web.UI.IFieldEditor)edProject).Hidden)
		{
			string top = Unit.Pixel(144).ToString();
			WebControl lblRefEntity = (WebControl)tab.FindControl("lblRefNoteID");
			if (lblRefEntity != null) lblRefEntity.Style[HtmlTextWriterStyle.Top] = top;
			WebControl edRefEntity = (WebControl)tab.FindControl("edRefNoteID");
			if (edRefEntity != null) edRefEntity.Style[HtmlTextWriterStyle.Top] = top;
		}*/

		bool isIncoming = GetIncoming();
		PXMultiSelector MailToSelector = (PXMultiSelector)message.FindControl("edMailTo");
		PXTextEdit MailToTextEdit = (PXTextEdit)message.FindControl("edMailToTe");
		PXMultiSelector MailCcSelector = (PXMultiSelector)message.FindControl("edMailCc");
		PXTextEdit MailCcTextEdit = (PXTextEdit)message.FindControl("edMailCcTe");
		PXMultiSelector MailBccSelector = (PXMultiSelector)message.FindControl("edMailBcc");
		PXTextEdit MailBccTextEdit = (PXTextEdit)message.FindControl("edMailBccTe");

		if (isIncoming)
		{
			if (MailToSelector != null) MailToSelector.DataField = "MailFake";
			if (MailCcSelector != null) MailCcSelector.DataField = "MailFake";
			if (MailBccSelector != null) MailBccSelector.DataField = "MailFake";

			if (MailToTextEdit != null) MailToTextEdit.DataField = "MailTo";
			if (MailCcTextEdit != null) MailCcTextEdit.DataField = "MailCc";
			if (MailBccTextEdit != null) MailBccTextEdit.DataField = "MailBcc";
		}
		else
		{
			if (MailToTextEdit != null) MailToTextEdit.DataField = "MailFake";
			if (MailCcTextEdit != null) MailCcTextEdit.DataField = "MailFake";
			if (MailBccTextEdit != null) MailBccTextEdit.DataField = "MailFake";

			if (MailToSelector != null) MailToSelector.DataField = "MailTo";
			if (MailCcSelector != null) MailCcSelector.DataField = "MailCc";
			if (MailBccSelector != null) MailBccSelector.DataField = "MailBcc";
		}
	}
	
   private bool GetIncoming()
	{
		try
		{
			CREmailActivityMaint graph = (CREmailActivityMaint)this.ds.DataGraph;
			return graph.Message.Current != null && graph.Message.Current.IsIncome == true;
		}
		catch { }
		return false;
	}

	private bool GetAllowCustomItems()
	{
		return true;
	}

	private static void FillEmailSelector(PXMultiSelector sel, bool allowCustomItems)
	{
		sel.AllowCustomItems = allowCustomItems;
		sel.AllowEdit = !allowCustomItems;
	}

	protected void on_data_bound(object sender, EventArgs e)
	{
		bool isIncoming = GetIncoming();
		PXSelector MailFromSelector = (PXSelector)message.FindControl("edMailFrom");
		PXTextEdit MailFromTextEdit = (PXTextEdit)message.FindControl("edMailFromTe");
		PXMultiSelector MailToSelector = (PXMultiSelector)message.FindControl("edMailTo");
		PXTextEdit MailToTextEdit = (PXTextEdit)message.FindControl("edMailToTe");
		PXMultiSelector MailCcSelector = (PXMultiSelector)message.FindControl("edMailCc");
		PXTextEdit MailCcTextEdit = (PXTextEdit)message.FindControl("edMailCcTe");
		PXMultiSelector MailBccSelector = (PXMultiSelector)message.FindControl("edMailBcc");
		PXTextEdit MailBccTextEdit = (PXTextEdit)message.FindControl("edMailBccTe");

		if (isIncoming)
		{
			if (MailFromSelector != null) MailFromSelector.Hidden = true;
			if (MailToSelector != null) MailToSelector.Hidden = true;
			if (MailCcSelector != null) MailCcSelector.Hidden = true;
			if (MailBccSelector != null) MailBccSelector.Hidden = true;

			if (MailFromTextEdit != null) MailFromTextEdit.Hidden = false;
			if (MailToTextEdit != null) MailToTextEdit.Hidden = false;
			if (MailCcTextEdit != null) MailCcTextEdit.Hidden = false;
			if (MailBccTextEdit != null) MailBccTextEdit.Hidden = false;
		}
		else
		{
			if (MailFromTextEdit != null) MailFromTextEdit.Hidden = true;
			if (MailToTextEdit != null) MailToTextEdit.Hidden = true;
			if (MailCcTextEdit != null) MailCcTextEdit.Hidden = true;
			if (MailBccTextEdit != null) MailBccTextEdit.Hidden = true;

			if (MailFromSelector != null) MailFromSelector.Hidden = false;
			if (MailToSelector != null) MailToSelector.Hidden = false;
			if (MailCcSelector != null) MailCcSelector.Hidden = false;
			if (MailBccSelector != null) MailBccSelector.Hidden = false;
		}		
	}

	protected void message_OnSelect(object sender, PXSelectEventArgs e)
	{
		if (this.Page.IsCallback && this.Page.Request.QueryString["Run"] != null)
		{
			this.ds.DataGraph.Views[this.ds.DataGraph.PrimaryView].Cache.Current = null;
			e.SelectArgumentsExt.ResetPendingCommand = this.Page.Request.QueryString["Run"];			
		}
	}
}
