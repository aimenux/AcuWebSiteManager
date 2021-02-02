<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR202500.aspx.cs" Inherits="Page_CR202500" 
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CampaignClass"
		TypeName="PX.Objects.CR.CRCampaignClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ShowDetails" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Campaign Class Summary" DataMember="CampaignClass" FilesIndicator="True"
		NoteIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edTypeID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="XL" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True"/>
			<px:PXSelector ID="edTypeID" runat="server" DataField="TypeID"
				FilterByAllFields="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server"/>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Size="XL" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds"  SkinID="Details" Height="150px"
		Style="z-index: 100" Width="100%" ActionsPosition="Top" Caption="Attributes" MatrixMode="True">
		<Levels>
			<px:PXGridLevel DataMember="Mapping">
				<Columns>
					<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="AttributeID" DisplayFormat="&gt;aaaaaaaaaa"
						AutoCallBack="true" LinkCommand="ShowDetails" />
					<px:PXGridColumn AllowNull="False" DataField="Description" />
					<px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
