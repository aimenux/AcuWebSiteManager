<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="RMLauncher.aspx.cs" Inherits="Reports_RMLauncher"
	EnableViewState="false" EnableEventValidation="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>ARm Reports Launcher</title>
</head>
<body runat="server">
	<%= ClientSideAppsHelper.RenderScriptConfiguration() %>
	<form id="form1" runat="server" autocomplete="off">
		<px_pt:PageTitle ID="usrCaption" runat="server" EnableTheming="true" />
		<div>
			<pxa:ARmDataSource ID="ds" runat="server"  TypeName="PX.CS.RMReportReader">
			</pxa:ARmDataSource>
		</div>
		<div style="padding: 5px;">
			<px:PXToolBar runat="server" ID="tlbReport" SkinID="Navigation">
			</px:PXToolBar>
		</div>
		<div>
			<px:PXReportViewer ID="viewer" runat="server" Height="150px" Width="100%" DataSourceID="ds" ToolBarID="tlbReport"
				ArmEditPage="~/pages/cs/cs206000.aspx" OnReportLoaded="viewer_ReportLoaded">
				<AutoSize Container="Window" Enabled="True" />
			</px:PXReportViewer>
		</div>
	</form>
</body>
</html>
