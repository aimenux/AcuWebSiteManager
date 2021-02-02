using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Data.Reports;
using PX.SM;
using PX.Web.UI;
using PX.Data.Wiki.Parser;
using Roles = System.Web.Security.Roles;
using SiteMap = System.Web.SiteMap;

public partial class Reports_RMLauncher : PX.Web.UI.PXPage
{
	private const string _SENDEMAILPARAMS_TYPE = "SendEmailParams";
	private const string _SENDEMAILMAINT_TYPE = "PX.Objects.CR.CREmailActivityMaint";
	private const string _SENDEMAIL_METHOD = "SendEmail";

	private static readonly ConstructorInfo _sendEmailParamsCtor;
	private static readonly PropertyInfo _fromMethod;
	private static readonly PropertyInfo _toMethod;
	private static readonly PropertyInfo _ccMethod;
	private static readonly PropertyInfo _bccMethod;
	private static readonly PropertyInfo _subjectMethod;
	private static readonly PropertyInfo _bodyMethod;
	private static readonly PropertyInfo _activitySourceMethod;
	private static readonly PropertyInfo _parentSourceMethod;
	private static readonly PropertyInfo _templateIDMethod;
	private static readonly PropertyInfo _attachmentsMethod;
	private static readonly Type _sendEmailMaint;
	private static readonly MethodInfo _sendEmailMethod;
	private static readonly bool _canSendEmail;

	static Reports_RMLauncher()
	{
		_sendEmailMaint = System.Web.Compilation.PXBuildManager.GetType(_SENDEMAILMAINT_TYPE, false);
		_sendEmailMethod = null;
		MemberInfo[] search = null;
		if (_sendEmailMaint != null)
		{
			_sendEmailMethod = _sendEmailMaint.GetMethod(_SENDEMAIL_METHOD, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public);
			search = _sendEmailMaint.GetMember(_SENDEMAILPARAMS_TYPE);
		}
		Type sendEmailParams = search != null && search.Length > 0 && search[0] is Type ? (Type)search[0] : null;
		if (sendEmailParams != null)
		{
			_sendEmailParamsCtor = sendEmailParams.GetConstructor(new Type[0]);
			_fromMethod = sendEmailParams.GetProperty("From");
			_toMethod = sendEmailParams.GetProperty("To");
			_ccMethod = sendEmailParams.GetProperty("Cc");
			_bccMethod = sendEmailParams.GetProperty("Bcc");
			_subjectMethod = sendEmailParams.GetProperty("Subject");
			_bodyMethod = sendEmailParams.GetProperty("Body");
			_activitySourceMethod = sendEmailParams.GetProperty("Source");
			_parentSourceMethod = sendEmailParams.GetProperty("ParentSource");
			_templateIDMethod = sendEmailParams.GetProperty("TemplateID");
			_attachmentsMethod = sendEmailParams.GetProperty("Attachments");
		}

		_canSendEmail = _sendEmailParamsCtor != null && _sendEmailMaint != null && _sendEmailMethod != null &&
			_fromMethod != null && _toMethod != null && _ccMethod != null && _bccMethod != null && 
			_subjectMethod != null && _bodyMethod != null &&
			_activitySourceMethod != null && _parentSourceMethod != null && _templateIDMethod != null && _attachmentsMethod != null;
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		this.usrCaption.CustomizationAvailable = false;
		if (!PXGraph.ProxyIsActive)
		{
			this.ds.ReportCode = this.Request.QueryString["ID"];
		}
		if (!String.IsNullOrEmpty(this.ds.ReportCode) && this.ds.ReportCode.Contains(".rpx"))
		{
			this.ds.ReportCode = this.ds.ReportCode.Replace(".rpx", "");
		}
		string screenID = null;
		object date = PX.Common.PXContext.GetBusinessDate();
		PX.Common.PXContext.SetBusinessDate((DateTime?)date);
		if (SiteMap.CurrentNode != null)
		{
			this.Title = PXSiteMap.CurrentNode.Title;
			screenID = PXSiteMap.CurrentNode.ScreenID;
		}
		else
		{
			string url = (Request.ApplicationPath != "/" ? Request.Path.Replace(Request.ApplicationPath, "~") : "~" + Request.Path) + "?id=" + this.ds.ReportCode + ".rpx";
			PXSiteMapNode node = SiteMap.Provider.FindSiteMapNode(url) as PXSiteMapNode;
			if (node != null)
			{
				this.Title = node.Title;
				this.usrCaption.ScreenTitle = node.Title;
				this.usrCaption.ScreenID = PX.Common.Mask.Format("CC.CC.CC.CC", node.ScreenID);
				screenID = node.ScreenID;
			}
			else
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<PX.SM.SiteMap>(
					new PXDataField("ScreenID"),
					new PXDataFieldValue("Url", PXDbType.VarChar, 512, url)
				))
				{
					if (record != null)
					{
						screenID = record.GetString(0);
						if (!String.IsNullOrEmpty(screenID) && !PXAccess.VerifyRights(screenID))
						{
							throw new PXSetPropertyException(ErrorMessages.NotEnoughRights, this.ds.ReportCode);
						}
					}
				}
			}
		}
		if (String.IsNullOrEmpty(PX.Common.PXContext.GetScreenID()) && !String.IsNullOrEmpty(screenID))
		{
			PX.Common.PXContext.SetScreenID(PX.Common.Mask.Format(">CC.CC.CC.CC", screenID));
		}
		if (_canSendEmail) viewer.EmailSend += new PXReportViewer.EmailSendHandler(viewer_EmailSend);
		else viewer.AllowSendEmails = false;
	}

	void viewer_EmailSend(PX.Reports.Mail.GroupMessage message, IList<FileInfo> files)
	{
		try
		{
			object emailParams = _sendEmailParamsCtor.Invoke(new object[0]);
			_fromMethod.SetValue(emailParams, message.From, null);
			_toMethod.SetValue(emailParams, message.Addressee.To, null);
			_ccMethod.SetValue(emailParams, message.Addressee.Cc, null);
			_bccMethod.SetValue(emailParams, message.Addressee.Bcc, null);
			_subjectMethod.SetValue(emailParams, message.Content.Subject, null);
			_bodyMethod.SetValue(emailParams, message.Content.Body, null);
			_activitySourceMethod.SetValue(emailParams, message.Relationship.ActivitySource, null);
			_parentSourceMethod.SetValue(emailParams, message.Relationship.ParentSource, null);
			_templateIDMethod.SetValue(emailParams, message.TemplateID, null);
			/*_reportNameMethod.SetValue(emailParams, message.ReportName, null);
			_reportFormatMethod.SetValue(emailParams, message.ReportFormat, null);
			_reportDataMethod.SetValue(emailParams, message.ReportData, null);*/
			IList<FileInfo> attachments = (IList<FileInfo>)_attachmentsMethod.GetValue(emailParams, null);
			foreach (FileInfo file in files)
				attachments.Add(file);
			_sendEmailMethod.Invoke(null, new object[] { emailParams });
		}
		catch (TargetInvocationException ex)
		{
			if (ex.InnerException != null && ex.InnerException is PXPopupRedirectException)
				new PXDataSource.RedirectHelper(ds).TryRedirect((PXPopupRedirectException)ex.InnerException);
		}
	}

	protected void viewer_ReportLoaded(object sender, EventArgs e)
	{
		if (viewer.Report != null) viewer.Report.Title = this.Title;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Response.AddHeader("cache-control", "no-store, private");
	}
}
