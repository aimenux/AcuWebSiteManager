<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" CodeFile="CS205020.aspx.cs" Inherits="Page_CS205020" Title="Report Maintenance" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="SelectedScreen" TypeName="PX.CS.CSAttributeMaint2" Visible="True">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" Visible="false"/>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" Visible="false" />
            <px:PXDSCallbackCommand Name="SaveClose" Visible="false" />
            <px:PXDSCallbackCommand Name="AddAttrib" CommitChanges="True" RepaintControlsIDs="panel" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="CopyReport" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Preview" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Del"  CommitChanges="True" Visible="false" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="Edit" CommitChanges="True" Visible="false" RepaintControls="All"/>
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
		</DataTrees>
		<ClientEvents CommandPerformed="commandResult" />
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<script type="text/javascript">
		function beforeHidePopup(src,ev)
		{
			var cm = __px_cm(src);
			if (cm && cm.dataSource && cm.dataSource && cm.dataSource.alert && src.dialogResult != 2)
			{
				ev.cancel = true;
			}
			src.dialogResult = 0;
		}
	</script>
    <style type="text/css">
        .wideAttrib td {padding-right: 50px}
		.wideAttrib input {caret-color: transparent; transition: border 1s 5s !important}
        #ctl00_phF_form_content > div > div:first-child {display:none}
        #ctl00_phF_AddNewAttr_PXFormView2_content {border-top:none}
        #ctl00_phF_form_panel .fld-lc >.fld-l {width:200px}
        #ctl00_phF_form_panel .fld-c {width:200px; margin-left:200px}
		[data-delFor], [data-editFor] {
			top: 3px;
			cursor: pointer;
			z-index: 100;
			position: absolute;
		}
    	[data-delFor] {
			transform: scale(0.7);
			right: 80px;
    	}
    	[data-editFor] {
			right: 110px;
    	}

    </style>
    <px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" 
        DataMember="SelectedScreen" Caption="" NoteIndicator="True" ControlsCreating="FormCreating"
        FilesIndicator="True" AllowCollapse="False" TemplateContainer="" 
		TabIndex="28300">
		<Template>
            <px:PXTextEdit ID="scrn" runat="server" DataField="ScreenId" />
           
		</Template>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXFormView>
    <px:PXSmartPanel runat="server" ID="AddNewAttr" Key="newAttrib" CaptionVisible="true" Caption="User-Defined Field Parameters" 
        LoadOnDemand="true" AutoReload="true" ClientEvents-BeforeHide="beforeHidePopup" >
        <px:PXFormView ID="PXFormView2" runat="server" Style="z-index: 100" Width="100%" 
        DataMember="NewAttrib" Caption="" NoteIndicator="True" 
        FilesIndicator="True" AllowCollapse="False" TemplateContainer=""
		TabIndex="28300">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector ID="attrib" runat="server" DataField="AttributeId" />
            <px:PXNumberEdit ID="column" runat="server" DataField="Column" />
            <px:PXNumberEdit ID="row" runat="server" DataField="Row" />
		</Template>
        </px:PXFormView>
     		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="btnCancel1" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
