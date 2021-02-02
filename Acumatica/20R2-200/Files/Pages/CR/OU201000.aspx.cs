using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Web.UI;
using System;
using System.Collections.Generic;
using PX.Api;
using System.Linq;
using System.Web;
using System.Web.UI;
using Messages = PX.Objects.CR.Messages;

public partial class Page_OU201000 : PX.Web.UI.PXPage
{
	private const string _outlookEmail = "__ouEmail";
	private const string _outlookDisplayName = "__ouDisplayName";
	private const string _outlookFirstName = "__ouFirstName";
	private const string _outlookLastName = "__ouLastName";
	private const string _outlookIsIncome = "__ouIsIncome";
	
    private const string _outlookEwsUrl = "__ouEwsUrl";
    private const string _outlookAttachmentToken = "__ouAttachmentToken";
	private const string _outlookAttachmentsCount = "__ouAttachmentsCount";
	private const string _outlookAttachmentNames = "__ouAttachmentNames";

	private const string _primaryViewName = "filter";

	private string[] _messageFields =
	{
		"MessageId", "From", "To", "CC", "Subject", "ItemId", "EwsUrl", "Token"
	};

	const string oums = "__oums";

	public Page_OU201000() : base ()
	{
		this.SupressMainRedirect = true;
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (PX.Translation.ResourceCollectingManager.IsStringCollecting)
			return;

		AddFileControls();

		PX.Data.PXCacheRights rights;
		List<string> invisible = null;
		List<string> disabled = null;

		if (!PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.outlookIntegration>())
			this.RedirectToError(Messages.OutlookFeatureNotInstalled, true);

		var node = PX.Data.PXSiteMap.Provider.FindSiteMapNode(this.Request.RawUrl) as PX.Data.PXSiteMapNode;
		if (node == null)
			this.RedirectToError(Messages.AccessToAddInHasBeenDenied, true);

		PXSiteMapNode nodeForAddInGraph = PXSiteMap.Provider.FindSiteMapNodeByGraphType(CustomizedTypeManager.GetTypeNotCustomized(typeof(OUSearchMaint).FullName));
		if (nodeForAddInGraph == null)
			this.RedirectToError(Messages.AccessToAddInHasBeenDenied, true);

		Page.Header.Controls.Add(
				new System.Web.UI.LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ResolveUrl("~/Content/fabric.min.css") + "\" />"));
		Page.Header.Controls.Add(
				new System.Web.UI.LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ResolveUrl("~/Content/fabric.components.min.css") + "\" />"));

		this.Master.FindControl("usrCaption").Visible = false;	 
		this.ClientScript.RegisterClientScriptInclude(this.GetType(), "msd", VirtualPathUtility.ToAbsolute("~/Scripts/modernizr-2.8.3.js"));
		this.ClientScript.RegisterClientScriptInclude(this.GetType(), "ms", "//appsforoffice.microsoft.com/lib/1.1/hosted/office.js");		
		//this.ClientScript.RegisterClientScriptInclude(this.GetType(), "ms", VirtualPathUtility.ToAbsolute("~/Scripts/Office/1/office.debug.js"));
		this.ClientScript.RegisterHiddenField(_outlookEmail, null);
		this.ClientScript.RegisterHiddenField(_outlookDisplayName, null);
		this.ClientScript.RegisterHiddenField(_outlookFirstName, null);
		this.ClientScript.RegisterHiddenField(_outlookLastName, null);
		this.ClientScript.RegisterHiddenField(_outlookIsIncome, null);

		ClientScript.RegisterHiddenField(_outlookEwsUrl, null);
		ClientScript.RegisterHiddenField(_outlookAttachmentToken, null);
		ClientScript.RegisterHiddenField(_outlookAttachmentsCount, null);
		ClientScript.RegisterHiddenField(_outlookAttachmentNames, null);

		_messageFields.ForEach(f=> this.ClientScript.RegisterHiddenField(oums + f, null));
		this.ClientScript.RegisterHiddenField("suppressReloadPage", 1.ToString());
	}

	private void RedirectToError(string errorMessage, bool showLogoutButton = false)
	{
		PX.Data.Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}&showLogoutButton={2}", errorMessage, "error", showLogoutButton));
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (this.IsCallback)
		{
			OUSearchMaint graph = (OUSearchMaint)this.ds.DataGraph;

			if (this.Request.Form[oums + _messageFields[0]] != null)
			{

				_messageFields.ForEach(
					f =>
					graph.SourceMessage.Cache.SetValue(graph.SourceMessage.Current, f,
						this.Request.Form[oums + f])
				);
			}
			graph.SourceMessage.Current.IsIncome = true;
			if (this.Request.Form[_outlookIsIncome] != "1")
				graph.SourceMessage.Current.IsIncome = false;

			if (this.Request.Form[_outlookIsIncome] == "1")
				graph.Filter.Current.OutgoingEmail = this.Request.Form[_outlookEmail];
			graph.Filter.Current.DisplayName = this.Request.Form[_outlookDisplayName];
			graph.Filter.Current.NewContactFirstName = this.Request.Form[_outlookFirstName];
			graph.Filter.Current.NewContactLastName = this.Request.Form[_outlookLastName];
			graph.Filter.Current.AttachmentNames = Request.Form[_outlookAttachmentNames];

			int attachmentsCount;
			if (int.TryParse(Request.Form[_outlookAttachmentsCount], out attachmentsCount))
			{
				graph.Filter.Current.AttachmentsCount = attachmentsCount;
			}

			var token = Request.Form[_outlookAttachmentToken];
            if (!String.IsNullOrEmpty(graph.SourceMessage.Current.Token))
			{
                graph.CreateActivity.SetEnabled(true);
                graph.CreateCase.SetEnabled(true);
                graph.CreateContact.SetEnabled(true);
                graph.CreateLead.SetEnabled(true);
                graph.CreateOpportunity.SetEnabled(true);
            }
            else
            {
				graph.CreateActivity.SetEnabled(false);
                graph.CreateCase.SetEnabled(false);
                graph.CreateContact.SetEnabled(false);
                graph.CreateLead.SetEnabled(false);
                graph.CreateOpportunity.SetEnabled(false);
            }
		}
		else
		{
			RegisterControlClientIdVariable(form, "personTitle", "personTitleId");
			RegisterControlClientIdVariable(form, "edHelp", "helpId");
			RegisterControlClientIdVariable(form, "PXLabel1", "label1Id");
			RegisterControlClientIdVariable(form, "edFake", "label2Id");
			RegisterControlClientIdVariable(form, "edFake2", "label3Id");
			RegisterControlClientIdVariable(form, "edAttachmentsPanel", "attachmentsPanelId");
			RegisterControlClientIdVariable(form, "edPanelCaption", "panelCaptionId");
			RegisterControlClientIdVariable(form, "edRecognitionInProgress", "isRecognitionInProgressId");
			RegisterControlClientIdVariable(form, "edRefreshAPDoc", "refreshButtonId");
			RegisterControlClientIdVariable(form, "edRefreshText", "refreshTextId");
			RegisterControlClientIdVariable(form, "edViewAPDoc", "viewAPDocumentButtonId");
		}
	}

	protected void form_OnDataBound(object sender, EventArgs e)
	{
		try
		{
			OUSearchMaint graph = (OUSearchMaint) this.ds.DataGraph;

			if (graph.Filter.Current.ContactType == null)
			{
				((PXTextEdit) this.form.FindControl("edPosition")).Enabled = false;
				((PXTextEdit) this.form.FindControl("edCompany")).Enabled = false;
			}
			if (graph.Filter.Current.Operation == OUOperation.CreateAPDocument ||
				graph.Filter.Current.Operation == OUOperation.ViewAPDocument)
			{
				((PXLabel)form.FindControl("personTitle")).Hidden = true;
				((PXSelector)form.FindControl("edContact")).Hidden = true;
				((PXButton)form.FindControl("edHelp")).Hidden = true;
				((PXTextEdit)this.form.FindControl("edSEmail")).Hidden = true;
			}
		}
		catch
		{			
		}
	}

	private void RegisterControlClientIdVariable(Control parent, string id, string variable)
	{
		var control = parent.FindControl(id);
		var script = string.Format("let {0} = '{1}';", variable, control.ClientID);

		Page.ClientScript.RegisterClientScriptBlock(GetType(), variable, script, true);
	}

	protected void edAttachmentsPanel_LoadContent(object sender, EventArgs e)
	{
		AddFileControls((PXSmartPanel)sender);
	}

	private void AddFileControls(PXSmartPanel smartPanel = null)
	{
		if (smartPanel == null)
		{
			smartPanel = (PXSmartPanel)form.FindControl("edAttachmentsPanel");
		}

		var graph = (OUSearchMaint)ds.DataGraph;
		var attachments = graph.APBillAttachments.Select()
			.AsEnumerable()
			.Select(a => (OUAPBillAttachment)a)
			.ToArray();

		for (var i = 0; i < attachments.Length; i++)
		{
			var id = string.Format("edCheckboxFile{0}", i);
			var itemIdCaptured = attachments[i].ItemId;
			var idCaptured = attachments[i].Id;

			if (smartPanel.FindControl(id) != null)
			{
				continue;
			}

			var fieldName = string.Format("File{0}", i);

			if (!graph.Filter.Cache.Fields.Contains(fieldName))
			{
				graph.Filter.Cache.Fields.Add(fieldName);

				graph.FieldSelecting.AddHandler(_primaryViewName, fieldName, (s, e) =>
				{
					graph.OUAPBillAttachmentSelectFileFieldSelecting(s, e, itemIdCaptured, idCaptured);
				});

				graph.FieldUpdating.AddHandler(_primaryViewName, fieldName, (s, e) =>
				{
					graph.OUAPBillAttachmentSelectFileFieldUpdating(s, e, itemIdCaptured, idCaptured);
				});
			}

			var checkboxFile = new PXCheckBox
			{
				ID = id,
				CommitChanges = true,
				DataField = fieldName,
				IsClientControl = false	
			};

			checkboxFile.ClientEvents.ValueChanged = "onFileSelect";
			checkboxFile.ApplyStyleSheetSkin(this);
			smartPanel.Controls.Add(checkboxFile);

			var formDataProvider = form.DataProviders[_primaryViewName];
			formDataProvider.DataControls[id] = checkboxFile;
		}
	}
}
