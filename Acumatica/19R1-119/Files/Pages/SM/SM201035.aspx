<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM201035.aspx.cs" Inherits="Page_SM205010"
	Title="User Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.UserAccess"
		PrimaryView="User">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveUser" />
			<px:PXDSCallbackCommand Name="FirstUser" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevUser" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextUser" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastUser" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="User" DataMember="User" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
			<px:PXSelector ID="edUsername" runat="server" DataField="Username"
				DataSourceID="ds">
				<AutoCallBack Command="CancelUser" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
			<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
			<px:PXTextEdit ID="edComment" runat="server" DataField="Comment" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" Caption="Restriction Groups"
		AllowSearch="True" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Included" TextAlign="Center" Type="CheckBox"
						Width="40px" RenderEditorText="True" AllowCheckAll="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="GroupName" Width="150px" />
					<px:PXGridColumn AllowUpdate="False" DataField="Description" Width="300px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Active" TextAlign="Center"
						Type="CheckBox" Width="40px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="GroupType" Label="Visible To Entities"
						RenderEditorText="True" Width="171px" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included"
						AlignLeft="True" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True"
						DataField="Active" AlignLeft="True" />
					<px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" DataField="GroupType"
						Enabled="False" />
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
