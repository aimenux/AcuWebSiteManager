<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" CodeFile="CS206000.aspx.cs" Inherits="Page_CS206000" Title="Report Maintenance" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script language="javascript" type="text/javascript">
		function commandResult(ds, context)
		{
			if (context.command == "Save" || context.command == "Delete")
			{
				var ds = px_all[context.id];
				var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
				if (isSitemapAltered) __refreshMainMenu();
			}
		}
	</script>
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Report" TypeName="PX.CS.RMReportMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="CopyReport" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Preview" CommitChanges="true" />

		</CallbackCommands>
		<ClientEvents CommandPerformed="commandResult" />
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlCopyReport" runat="server" Style="z-index: 108;" Caption="New Report Code" CaptionVisible="True" Key="Parameter" CreateOnDemand="False" AutoCallBack-Target="formCopyReport" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="btnOK">
		<px:PXFormView ID="formCopyReport" runat="server" CaptionVisible="False" DataMember="Parameter" DataSourceID="ds" Style="z-index: 100" TabIndex="29900" Width="100%">
			<ContentStyle BackColor="Transparent" BorderStyle="None">
			</ContentStyle>
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="S" StartColumn="True" />
				<px:PXMaskEdit ID="edNewReportCode" runat="server" DataField="NewReportCode">
				</px:PXMaskEdit>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="Copy">
				<AutoCallBack Command="Save" Target="formCopyReport" />
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" 
        DataMember="Report" Caption="Report Summary" NoteIndicator="True" 
        FilesIndicator="True" AllowCollapse="False" TemplateContainer=""
		TabIndex="28300" OnDataBound="form_DataBound" >
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="m" GroupCaption="Report Definition" StartGroup="True" />
			<px:PXSelector ID="edReportCode" runat="server" DataField="ReportCode" AutoGenerateColumns="True" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXDropDown CommitChanges="True" ID="edType" runat="server" AllowNull="False" DataField="Type" />
			<px:PXSelector ID="edRowSetCode" runat="server" DataField="RowSetCode" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
			<px:PXSelector ID="edColumnSetCode" runat="server" DataField="ColumnSetCode" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
			<px:PXSelector ID="edUnitSetCode" runat="server" DataField="UnitSetCode" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" CommitChanges="True" />
			<px:PXSelector ID="edStartUnitCode" runat="server" DataField="StartUnitCode" AutoRefresh="True" />                      
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" GroupCaption="Site Map" LabelsWidth="S" StartColumn="True" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" />
			<px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" CommitChanges="true" />
            <px:PXCheckBox ID="chkVisible" runat="server" DataField="Visible" CommitChanges="true" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" />
		    <px:PXSelector runat="server" DataField="WorkspaceID" ID="edWorkspaceID" DisplayMode="Text"/>
		    <px:PXSelector runat="server" DataField="SubcategoryID" ID="edSubcategoryID" DisplayMode="Text"/>
			<px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Page Settings" LabelsWidth="S" />
			<px:PXDropDown ID="edPaperKind" runat="server" DataField="PaperKind" />
			<px:PXCheckBox ID="chkLandscape" runat="server" DataField="Landscape" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Margins" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
			<px:PXNumberEdit ID="edMarginTop" runat="server" DataField="MarginTop" Size="XS" />
			<px:PXDropDown ID="edMarginTopType" runat="server" AllowNull="False" DataField="MarginTopType" SuppressLabel="True" Width="110px" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
			<px:PXNumberEdit ID="edMarginBottom" runat="server" DataField="MarginBottom" Size="XS" />
			<px:PXDropDown ID="edMarginBottomType" runat="server" AllowNull="False" DataField="MarginBottomType" SuppressLabel="True" Width="110px" />
			<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit ID="edMarginLeft" runat="server" DataField="MarginLeft" Size="XS" />
			<px:PXDropDown ID="edMarginLeftType" runat="server" AllowNull="False" DataField="MarginLeftType" SuppressLabel="True" Width="110px" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit ID="edMarginRight" runat="server" DataField="MarginRight" Size="XS" />
			<px:PXDropDown ID="edMarginRightType" runat="server" AllowNull="False" DataField="MarginRightType" SuppressLabel="True" Width="110px" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" GroupCaption="Print Area" StartGroup="True" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit ID="edWidth" runat="server" DataField="Width" Size="XS" />
			<px:PXDropDown ID="edWidthType" runat="server" AllowNull="False" DataField="WidthType" SuppressLabel="True" Width="110px" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit ID="edHeight" runat="server" DataField="Height" Size="XS" />
			<px:PXDropDown ID="edHeightType" runat="server" AllowNull="False" DataField="HeightType" SuppressLabel="True" Width="110px" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Font Style" />
			<px:PXDropDown ID="edStyleFontName" runat="server" DataField="StyleFontName" AllowEdit="True" />
			<px:PXDropDown ID="edStyleTextAlign" runat="server" AllowNull="False" DataField="StyleTextAlign" />
			<px:PXDropDown ID="edStyleFontStyle" runat="server" AllowNull="False" DataField="StyleFontStyle" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXDropDown Size="xs" ID="edStyleFontSize" runat="server" DataField="StyleFontSize" AllowEdit="True" />
			<px:PXDropDown ID="edStyleFontSizeType" runat="server" AllowNull="False" DataField="StyleFontSizeType" SuppressLabel="True" Width="110px" />
			<px:PXLayoutRule runat="server" />
			<px:PXDropDown ID="edStyleColor" runat="server" DataField="StyleColor" AllowEdit="True" />
			<px:PXDropDown ID="edStyleBackColor" runat="server" DataField="StyleBackColor" AllowEdit="True" />
		</Template>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXFormView>
</asp:Content>
