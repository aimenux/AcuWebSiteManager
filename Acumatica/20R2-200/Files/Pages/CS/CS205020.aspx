<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" CodeFile="CS205020.aspx.cs" Inherits="Page_CS205020" Title="Report Maintenance" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" UDFTypeField="GoodNews" Width="100%" runat="server" PrimaryView="ScreenSettings" TypeName="PX.CS.CSAttributeMaint2" Visible="True">
		<ClientEvents Initialize="initDS" />
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" Visible="false"/>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" Visible="false" />
            <px:PXDSCallbackCommand Name="SaveClose" Visible="false" />
            <px:PXDSCallbackCommand Name="AddAttrib" CommitChanges="True" RepaintControlsIDs="atPanel" />
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
		function initDS(ds) {
			if (ds) {
				var tab = __px_all(ds)['ctl00_phF_form'];
				if (tab) {
					tab.events.addEventHandler('afterRepaint', function () {
						if (tab.items[1]) {
							tab.items[1].setVisible(tab.items[0].elemC.querySelector('table'));
						}
					});
				}
			}
		}
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
        #ctl00_phF_form_content > div > div:first-child, #ctl00_phF_form_t0 > div:first-child {display:none}
        #ctl00_phF_AddNewAttr_PXFormView2_content {border-top:none}
        #ctl00_phF_form_atPanel .fld-lc >.fld-l, #ctl00_phF_form_t0_atPanel .fld-lc >.fld-l {width:200px}
        #ctl00_phF_form_atPanel .fld-c, #ctl00_phF_form_t0_atPanel .fld-c {width:200px; margin-left:200px}
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
    <px:PXTab ID="form" runat="server" Style="z-index: 100" Width="100%" 
        DataMember="ScreenSettings" Caption="" NoteIndicator="True" ControlsCreating="FormCreating"
        FilesIndicator="True" AllowCollapse="False" TemplateContainer="" 
		TabIndex="28300">
		<Items>
		<px:PXTabItem Text="User Defined Fields">
			<Template>
				<px:PXTextEdit ID="scrn" runat="server" DataField="ScreenId" />
           
			</Template>
		</px:PXTabItem>
		<px:PXTabItem Text="Visibility" LoadOnDemand="true" RepaintOnDemand="false">
			<Template>
				<px:PXPanel runat="server" ID="pnlVisibility" SkinID="Transparent">
					<px:PXLayoutRule runat="server" StartColumn="True"/>
					<px:PXDropDown ID="newDD" runat="server" size="XXL" Hidden="true" CommitChanges="true"/>
					<px:PXSelector ID="newSel" runat="server" size="XXL"  Hidden="true" CommitChanges="true" AutoGenerateColumns="true" />
				</px:PXPanel>
				<px:PXGrid ID="visibility" runat="server"  SkinID="Details" Width="100%" SyncPosition="true" FeedbackMode="ForceDataEntry" NoteIndicator="false" FilesIndicator="false">
					<AutoSize Enabled="true" />
					<Mode AllowDelete="false" AllowAddNew="false" />
					<ActionBar>
						<Actions>
							<AddNew Enabled="False" />
							<Delete Enabled="False" />
						</Actions>
					</ActionBar>
					<Levels>
						<px:PXGridLevel>
							<Columns >
								<px:PXGridColumn DataField="AttributeID"  Width="400px" AllowFilter="false" AllowSort="false" Visible="false" > <Header Text="User-Defined Attribute" /></px:PXGridColumn>
								<px:PXGridColumn DataField="Name" Width="400px" AllowUpdate="false" AllowFilter="false" AllowSort="false" ><Header Text="User-Defined Attribute" /></px:PXGridColumn>
								<px:PXGridColumn DataField="Required" Width="100px" Type="CheckBox" CommitChanges="true"  AllowFilter="false" AllowSort="false" ><Header Text="Required" /></px:PXGridColumn>
								<px:PXGridColumn DataField="Hidden" Width="100px" Type="CheckBox" CommitChanges="true"  AllowFilter="false" AllowSort="false" ><Header Text="Hidden" /></px:PXGridColumn>
							</Columns>
							</px:PXGridLevel>
					</Levels>
				</px:PXGrid>
			</Template>
		</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXTab>
	<px:PXSmartPanel runat="server" ID="AddNewAttr" Key="newAttrib" CaptionVisible="true" Caption="User-Defined Field Parameters"
		LoadOnDemand="true" AutoReload="true" ClientEvents-BeforeHide="beforeHidePopup">
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
