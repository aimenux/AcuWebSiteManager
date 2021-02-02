<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU220000.aspx.cs" Inherits="Page_AU220000" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<label class="projectLink transparent border-box">Mobile Application</label>

	<pxa:AUDataSource ID="ds" runat="server" Width="100%" TypeName="PX.SM.ProjectMobileSiteMapMaintenance" PrimaryView="Items" Visible="true">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="actionCustomizeMenu" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="actionCustomizeExistingScreen" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="actionRemoveExistingScreen" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="actionAddNewScreen" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="actionClearCurrentCompany" RepaintControls="None" RepaintControlsIDs="ds" SelectControlsIDs="grid" />
            <px:PXDSCallbackCommand Name="actionClearAllCompanies" RepaintControls="None" RepaintControlsIDs="ds" SelectControlsIDs="grid" />
		</CallbackCommands>
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" SkinID="Primary" 
		AutoAdjustColumns="True" SyncPosition="True" FilesIndicator="False" NoteIndicator="False">
		<AutoSize Enabled="true" Container="Window" />
		<Mode AllowAddNew="False" />
		<ActionBar Position="Top" ActionsVisible="false">
			<Actions>
				<AddNew MenuVisible="False" ToolBarVisible="False"/>
				<NoteShow MenuVisible="False" ToolBarVisible="False" />
				<ExportExcel MenuVisible="False" ToolBarVisible="False" />
				<AdjustColumns ToolBarVisible="False"/>
			</Actions>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit SuppressLabel="True" Height="100%" runat="server" ID="edSource" TextMode="MultiLine"
						DataField="Content" Font-Size="10pt" Font-Names="Courier New" Wrap="False" SelectOnFocus="False">
						<AutoSize Enabled="True" />
					</px:PXTextEdit>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Operation" Width="108px" />
					<px:PXGridColumn DataField="ScreenID" Width="108px" />
					<px:PXGridColumn DataField="Title" Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username" Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>

	<px:PXSmartPanel runat="server" ID="PanelSelectAdded"
		CaptionVisible="True" Caption="Update Existing Screen"
		Key="FilterSelectAddedMobileSiteMap" AllowResize="false"
		AutoRepaint="True" >
		<px:PXFormView runat="server" ID="PXFormView1" DataMember="FilterSelectAddedMobileSiteMap" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM"/>
				<px:PXSelector runat="server" ID="edScreenID" DataField="ScreenID" CommitChanges="True" AutoComplete="false" AutoRefresh="true" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK" />
					<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>

    <px:PXSmartPanel runat="server" ID="PanelRemove"
		CaptionVisible="True" Caption="Remove Existing Screen"
		Key="FilterSelectAddedMobileSiteMapForRemove" AllowResize="false"
		AutoRepaint="True" >
		<px:PXFormView runat="server" ID="PXFormView3" DataMember="FilterSelectAddedMobileSiteMap" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM"/>
				<px:PXSelector runat="server" ID="edScreenID" DataField="ScreenID" CommitChanges="True" AutoComplete="false" AutoRefresh="true" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK" />
					<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>

    <px:PXSmartPanel runat="server" ID="PanelAddNew"
		CaptionVisible="True" Caption="Add New Screen"
		Key="FilterSelectExistingScreenExceptAdded" AllowResize="false"
		AutoRepaint="True" >
		<px:PXFormView runat="server" ID="PXFormView2" DataMember="FilterSelectExistingScreenExceptAdded" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM"/>
				<px:PXSelector runat="server" ID="edScreenID" DataField="ScreenID" CommitChanges="True" AutoRefresh="true" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK" />
					<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
</asp:Content>
