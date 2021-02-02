using System;
using System.Data;
using System.Linq;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.Data;
using PX.Data.Reports;
using PX.SM;
using PX.Web.UI;
using System.Text;
using PX.Data.Wiki.Parser;
using PX.Common.Parser;
using PX.Reports.Parser;
using Roles = System.Web.Security.Roles;
using SiteMap = System.Web.SiteMap;
using PX.Reports.Controls;

public partial class Pages_ReportLauncher : PX.Web.UI.PXPage
{
	public string ReportID;
	private string _screenID;

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

	static Pages_ReportLauncher()
	{
		_sendEmailMaint = System.Web.Compilation.PXBuildManager.GetType(ReportLauncherHelper._SENDEMAILMAINT_TYPE, false);
		_sendEmailMethod = null;
		MemberInfo[] search = null;
		if (_sendEmailMaint != null)
		{
			_sendEmailMethod = _sendEmailMaint.GetMethod(ReportLauncherHelper._SENDEMAIL_METHOD, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public);
			search = _sendEmailMaint.GetMember(ReportLauncherHelper._SENDEMAILPARAMS_TYPE);
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
			_activitySourceMethod != null && _parentSourceMethod != null && _templateIDMethod != null && _attachmentsMethod != null && !PXSiteMap.IsPortal;
	}

	protected override void OnPreRender(EventArgs e)
	{
		if (ControlHelper.IsRtlCulture())
			this.Page.Form.Style[HtmlTextWriterStyle.Direction] = "rtl";
		base.OnPreRender(e);
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		// remove unum parameter
		PropertyInfo isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
		// make collection editable
		isreadonly.SetValue(this.Request.QueryString, false, null);
		this.Request.QueryString.Remove(PXUrl.UNum);
		isreadonly.SetValue(this.Request.QueryString, true, null);

		this.usrCaption.CustomizationAvailable = false;

		var date = PXContext.GetBusinessDate();
		PX.Common.PXContext.SetBusinessDate((DateTime?)date);

		if ((this.viewer.SchemaUrl = this.Request.QueryString["ID"]) == null)
		{
			this.viewer.SchemaUrl = ReportID;
		}

		if (SiteMap.CurrentNode != null)
		{
			this.Title = PXSiteMap.CurrentNode.Title;
			_screenID = PXSiteMap.CurrentNode.ScreenID;
		}
		else
		{
			string url;
			if (Request.ApplicationPath != "/")
			{
				url = Request.Path.Replace(Request.ApplicationPath, "~") + "?ID=" + this.viewer.SchemaUrl;
			}
			else if (Request.Path.StartsWith("/"))
			{
				url = "~" + Request.Path;
			}
			else
			{
				url = Request.Path;
			}
			PXSiteMapNode node = SiteMap.Provider.FindSiteMapNode(url) as PXSiteMapNode;
			if (node != null)
			{
				this.Title = node.Title;
				this.usrCaption.ScreenTitle = node.Title;
				this.usrCaption.ScreenID = PX.Common.Mask.Format(">CC.CC.CC.CC", node.ScreenID);
				_screenID = node.ScreenID;
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
						_screenID = record.GetString(0);
						if (!String.IsNullOrEmpty(_screenID) && !PXAccess.VerifyRights(_screenID))
						{
							throw new PXSetPropertyException(ErrorMessages.NotEnoughRights, this.viewer.SchemaUrl);
						}
					}
				}
			}
		}
		if (String.IsNullOrEmpty(PX.Common.PXContext.GetScreenID()))
		{
			if (String.IsNullOrEmpty(_screenID) && !String.IsNullOrEmpty(this.viewer.SchemaUrl))
			{
				string schema = this.viewer.SchemaUrl;
				if (schema.EndsWith(".rpx", StringComparison.OrdinalIgnoreCase))
				{
					schema = schema.Substring(0, schema.Length - 4);
				}
				if (schema.Length == 8)
				{
					_screenID = schema;
				}
			}
			if (!String.IsNullOrEmpty(_screenID))
			{
				PX.Common.PXContext.SetScreenID(PX.Common.Mask.Format(">CC.CC.CC.CC", _screenID));
			}
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

	protected void Page_Load(object sender, EventArgs e)
	{
		Response.AddHeader("cache-control", "no-store, private");
	}

	protected void viewer_Reload(object sender, EventArgs e)
	{
	}
	protected void viewer_ReportCreated(object sender, EventArgs e)
	{
        object passed = PXContext.Session.PageInfo[VirtualPathUtility.ToAbsolute(this.Page.Request.Path)];
		if (passed == null)
		{
			object passedByRawUrl = PXContext.Session.PageInfo[VirtualPathUtility.ToAbsolute(this.Page.Request.RawUrl)];
			var pars = passedByRawUrl as Dictionary<string, string>;
			if (pars != null)
			{
				string autoexport;
				if (pars.TryGetValue(ReportLauncherHelper._AUTO_EXPORT_PDF, out autoexport) && 
					autoexport == "true")
				{
					viewer.AutoExportPDF = true;
					pars.Remove(ReportLauncherHelper._AUTO_EXPORT_PDF);
				}
				passed = new Dictionary<string, string>(pars);
			}
			else
				passed = passedByRawUrl;
		}
		PXReportsRedirectList reports = passed as PXReportsRedirectList;
		sessionReportParams.Add(reports == null ? passed : reports[0].Value);

		PXSiteMapProvider provider = SiteMap.Provider as PXSiteMapProvider;
		if (provider != null && reports != null)
		{
			if (reports.SeparateWindows)
			{
				if (reports.Count > 1)
				{
					KeyValuePair<String, Object> pair;
					do
					{
						reports.RemoveAt(0);
						pair = reports.First();
					} while (reports.Count > 1 && String.IsNullOrEmpty(pair.Key));

					string reportID = pair.Key;
					if (String.IsNullOrEmpty(reportID)) return;
					string url = PXBaseDataSource.getScreenUrl(reportID);
					if (!String.IsNullOrEmpty(url))
					{
						url = PXUrl.ToAbsoluteUrl(url) + "&preserveSession=true";

						NextReport = new KeyValuePair<String, Object>(url, reports);
						viewer.NextReportUrl = url;
					}
				}
			}
			else
			{
				foreach (KeyValuePair<string, object> t in reports)
				{
					string reportID = t.Key;
					if (string.IsNullOrEmpty(reportID)) continue;
					PXSiteMapNode reportNode = provider.FindSiteMapNodeByScreenID(reportID);
					string reportName;
					if (reportNode != null && !string.IsNullOrEmpty(reportName = PXUrl.GetParameter(reportNode.Url, "ID")))
					{
						Report report = new Report();
						report.ReportName = reportName;
						viewer.Report.SiblingReports.Add(report);
						sessionReportParams.Add(t.Value);
					}
				}
			}
		}
	}

	private KeyValuePair<String, Object>? NextReport;
	private readonly List<object> sessionReportParams = new List<object>();

	protected void viewer_ReportLoaded(object sender, EventArgs e)
	{
		bool renderName = (Request.QueryString["RenderNames"] != null);
		int systemParamsCount = renderName ? 2 : 1;
		if ((Request.QueryString[PXUrl.HideScriptParameter] != null)) systemParamsCount++;
        if ((Request.QueryString[PXUrl.PopupParameter] != null)) systemParamsCount++;
		if ((Request.QueryString[PXPageCache.TimeStamp] != null)) systemParamsCount++;
		if ((Request.QueryString[PXUrl.UNum] != null)) systemParamsCount++;
		if ((Request.QueryString[PXUrl.CompanyID] != null)) systemParamsCount++;
		if ((Request.QueryString["_" + PXUrl.CompanyID] != null)) systemParamsCount++;

		if (Request.QueryString[PX.Reports.Messages.Max] != null)
			viewer.Report.TopCount = Convert.ToInt32(Request.QueryString[PX.Reports.Messages.Max].ToString());

		object passed = sessionReportParams.Count > 0 ? sessionReportParams[0] : null;
		if (passed == null && PXSiteMap.IsPortal && this.Request.QueryString["PortalAccessAllowed"] == null)
		{
			throw new PXException(ErrorMessages.NotEnoughRights, viewer.Report.ReportName);
		}

	    var pars = PXReportRedirectParameters.UnwrapParameters(passed);
	    passed = PXReportRedirectParameters.UnwrapSet(passed);

        if (pars == null && Request.QueryString.Count > systemParamsCount && !(Request.AppRelativeCurrentExecutionFilePath ?? "").StartsWith("~/rest/"))
		{
			foreach (string key in Request.QueryString.AllKeys)
			{
				if (String.Compare(key, "ID", StringComparison.OrdinalIgnoreCase) == 0 ||
					String.Compare(key, "RenderNames", StringComparison.OrdinalIgnoreCase) == 0 ||
					String.Compare(key, PXUrl.CompanyID, StringComparison.OrdinalIgnoreCase) == 0 ||
					String.Compare(key, "_CompanyID", StringComparison.OrdinalIgnoreCase) == 0 ||	PXUrl.SystemParams.Contains(key))
				{
					continue;
				}
				if (pars == null)
				{
					pars = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				}
				pars[key] = Request.QueryString[key];
			}
		}

		viewer.Report.RenderNames = renderName;
		viewer.Report.Title = this.Title;
		// reportid from params
		string reportIdParamValue = pars != null && pars.ContainsKey(ReportLauncherHelper._REPORTID_PARAM_KEY) ? pars[ReportLauncherHelper._REPORTID_PARAM_KEY] : string.Empty;
		if (string.IsNullOrEmpty(reportIdParamValue))
		{
		    var id = Page.Request.Params["id"];
		    reportIdParamValue = id != null ? id.Replace(".rpx", string.Empty) : null;
		}
	    // true if params are intended for the current report
		bool isCurrReportParams = passed == null || 
			String.Equals(_screenID, reportIdParamValue, StringComparison.OrdinalIgnoreCase);
		// clear params if they are not for the current report
		if (!isCurrReportParams && pars != null)
		{
			pars = null;
			PXContext.Session.PageInfo[VirtualPathUtility.ToAbsolute(this.Page.Request.Path)] = null;
		}

		if (!viewer.HideParameters)
			viewer.Report.RequestParams = !(passed is IPXResultset) && (!isCurrReportParams || pars == null);
		if (passed is IPXResultset && viewer.Report.RequestContext)
		{
			this.usrCaption.ScreenUrl = string.Empty;			
		}
		
		if (isCurrReportParams)
		{
			for (int i = 0; i < sessionReportParams.Count; i++)
			{
				try
				{
					if (i == 0)
					{						
						ReportLauncherHelper.LoadParameters(viewer.Report, pars, (SoapNavigator)((PXReportViewer)sender).GetNavigator());
						// Clear report data from session after report has been loaded when there is only one report
						if (sessionReportParams.Count == 1 && (pars == null || pars.Count == 1 && pars.ContainsKey(ReportLauncherHelper._REPORTID_PARAM_KEY)))
							PXContext.Session.PageInfo[VirtualPathUtility.ToAbsolute(this.Page.Request.Path)] = null;
					}
					else
                        ReportLauncherHelper.LoadParameters(viewer.Report.SiblingReports[i - 1], sessionReportParams[i], (SoapNavigator)((PXReportViewer)sender).GetNavigator());
				}
				catch
				{
					PXContext.Session.PageInfo[VirtualPathUtility.ToAbsolute(this.Page.Request.Path)] = null;
					throw;
				}
			}
		}
	}

	protected void viewer_ReportPreRender(object sender, EventArgs e)
	{
		//this.Session.Remove(VirtualPathUtility.ToAbsolute(this.Page.Request.Path));// because crear when refresh(F5)
		if (NextReport != null)
		{
			String url = NextReport.Value.Key;
			if (url.IndexOf('?') > -1) url = url.Substring(0, url.IndexOf('?'));

			PXContext.Session.PageInfo[url] = NextReport.Value.Value;
		}
	}
}
