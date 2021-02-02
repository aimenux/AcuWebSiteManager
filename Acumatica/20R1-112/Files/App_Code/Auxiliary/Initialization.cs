using PX.Objects.CS;
using PX.Objects.SM;
using PX.Data.RelativeDates;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.OAuthClient;

public static class Initialization
{
	public static void ProcessApplication()
	{
		InitTimeServices();
		InitReports();
	    InitCalendar();
		InitVersion();
		InitActivityService();
		InitNotificationService();
		InitMailProcessing();
		InitDashboards();
		InitSpellCheck();
		InitPageTitle();
		DITAConversionType();
        InitCommon();
		InitSanitizer();
	}

	private static void InitSanitizer()
	{
		PX.Common.HtmlSanitizerService.Register(new PX.Web.Controls.HtmlSanitizerService());
	}
    private static void InitCommon()
    {
        PX.Common.Localizer.Localize = PXLocalizer.Localize;
	}

    [System.Obsolete("This object is obsolete and will be removed. Rewrite your code without this object or contact your partner for assistance.")]
	public static void ProcessRequest()
	{
	}

    [System.Obsolete("This object is obsolete and will be removed. Rewrite your code without this object or contact your partner for assistance.")]
	public static void ProcessHandler()
	{
	    PX.Common.PXExecutionContext.Current.Request = new PX.Common.PXExecutionContext.RequestInfo(System.Web.HttpContext.Current.Request);
	}

	private static void InitTimeServices()
	{
		PX.Common.TimeRegionService.Register(new CustomTimeRegionProvider());
	}

	private static void InitReports()
	{
		PX.Reports.ReportFileManager.ReportsDir = "~/ReportsDefault";
		PX.Reports.ReportFileManager.CustomReportsDir = "~/ReportsCustomized";
		PX.Reports.CertificateProvider.Register(new PX.Data.Reports.PXCertificateProvider());
		PX.Reports.LocalizationProvider.Register(new PX.Data.Reports.PXReportLocalizationProvider());
		PX.Reports.DbImagesProvider.Register(new PX.Data.Reports.PXDbImagesProvider());
		PX.Reports.SettingsProvider.Register(new PX.Data.Reports.PXSettingProvider());
		PX.Export.DefaultStyleProvider.Register(new PX.Web.UI.PXDBDefaultStyleProvider());
		


		System.Type reportFunctionsType = System.Web.Compilation.PXBuildManager.GetType(PX.Data.Reports.ReportLauncherHelper._REPORTFUNCTIONS_TYPE, false);
		if (reportFunctionsType != null)
			PX.Common.Parser.ExpressionContext.RegisterExternalObject("Payments", System.Activator.CreateInstance(reportFunctionsType));

		System.Type commonReportFunctionsType = System.Web.Compilation.PXBuildManager.GetType(PX.Data.Reports.ReportLauncherHelper._COMMONREPORTFUNCTIONS_TYPE, false);
		if (commonReportFunctionsType != null)
			PX.Common.Parser.ExpressionContext.RegisterExternalObject("CustomFunc", System.Activator.CreateInstance(commonReportFunctionsType));
	}

	private static void InitCalendar()
    {
        PX.Export.Imc.VCalendarProcessor.Register((PX.Export.Imc.IVCalendarProcessor)new PX.Objects.EP.EPEventVCalendarProcessor());
    }
	private static void InitVersion()
	{
		PX.Common.Service.VersionService.Register(new PX.Data.PXVersionInfo.VersionService());
	}

	private static void InitActivityService()
	{
		PX.Data.EP.ActivityService.Register(new PX.Objects.EP.ActivityService());
	}

	private static void InitNotificationService()
	{
		PX.SM.NotificationService.Register(new PX.Objects.SM.NotificationService());
	}

	private static void InitMailProcessing()
	{
		PX.Data.EP.MailSendProvider.Register(new PX.Objects.EP.CommonMailSendProvider());
		
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.ConversationEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.ConfirmReceiptEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.DefaultEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.ExchangeEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.CR.CaseCommonEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.RouterEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.CR.NewCaseEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.CR.ContactBAccountEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.NotificationEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.UnassignedEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.CleanerEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.AssignmentEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.CR.NewLeadEmailProcessor());
		PX.Objects.EP.EmailProcessorManager.Register(new PX.Objects.EP.ImageExtractorEmailProcessor());
		PX.Objects.EP.CommonMailReceiveProvider receiver = new PX.Objects.EP.CommonMailReceiveProvider();
		PX.Data.EP.EMailMessageReceiver.RegisterProvider(receiver);
		PX.Data.EP.EMailMessageReceiver.RegisterProcessor(receiver);
		PX.Data.EP.EMailMessageReceiver.RegisterOriginalMailProvider(receiver);

		PX.Data.Wiki.Parser.ISettings wikiSettings = new PXWikiSettings(null).Absolute.Settings;
		PX.Data.Reports.PXReportTools.WikiSettings = wikiSettings;
	}

	private static void InitDashboards()
	{
		PX.Web.Controls.CategoriesService.Register(new PX.Web.Objects.EP.CategoriesService());
	}

	private static void InitSpellCheck()
	{
		PX.Web.Controls.PXRichTextEdit.SpellCheckService = new PX.Web.Objects.SM.SpellCheckService();
	}

	private static void InitPageTitle()
	{
		PX.Web.UI.TitleModuleService.Register(new PX.Web.Controls.TitleModules.AutomationDebugTitleModule());
		PX.Web.UI.TitleModuleService.Register(new PX.Web.Controls.TitleModules.ReminderTitleModule());
		PX.Web.UI.TitleModuleService.Register(new DashboardPageTitleModule());
        //PX.Web.UI.TitleModuleService.Register(new PX.Web.Controls.TitleModules.BranchTitleModule());
    }

	private static void DITAConversionType()
	{
		PX.Data.Wiki.WikiExportCollection.RegisterWikiExport("ConversionType1", typeof(PX.Data.Wiki.ConversionType1));
		PX.Data.Wiki.WikiExportCollection.RegisterWikiExport("ConversionType2", typeof(PX.Data.Wiki.ConversionType2));
		PX.Data.Wiki.WikiExportCollection.RegisterWikiExport("ConversionType3", typeof(PX.Data.Wiki.ConversionType3));
	}
}
