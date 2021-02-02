<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ReportLauncher.aspx.cs"
	Inherits="Pages_ReportLauncher" EnableViewState="false" EnableEventValidation="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Reports test</title>
</head>
<body style="margin: 0px; min-width:300px" runat="server">
	<%= ClientSideAppsHelper.RenderScriptConfiguration() %>
	<form id="form1" runat="server" autocomplete="off">
		<px_pt:PageTitle ID="usrCaption" runat="server" EnableTheming="true" />
		<div>
			<px:PXSoapDataSource ID="ds" runat="server">
			</px:PXSoapDataSource>
		</div>
		<div class="reportToolbar">
			<px:PXToolBar runat="server" ID="tlbReport" SkinID="Navigation">
			</px:PXToolBar>
		</div>
		<div class="phF">
			<px:PXReportViewer ID="viewer" runat="server" Height="150px" Width="100%" DataSourceID="ds" ToolBarID="tlbReport"
				EnableTheming="True" OnReportCreated="viewer_ReportCreated" OnReportLoaded="viewer_ReportLoaded" OnPreRender="viewer_ReportPreRender" >
				<AutoSize Container="Window" Enabled="True" />
			</px:PXReportViewer>
		</div>
	</form>
</body>
</html>
