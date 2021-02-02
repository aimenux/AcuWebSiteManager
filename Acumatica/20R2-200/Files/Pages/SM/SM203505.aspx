<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM203505.aspx.cs" Inherits="Page_SM200578" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
		PrimaryView="Setup" TypeName="PX.SM.InstallationSetup">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="reloadParameters" Visible="False"  /> 
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Setup" 
		EmailingGraph="" Caption="General Settings" TemplateContainer="" TabIndex="500">
<Activity Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" 
				ControlSize="L" />
			<px:PXLayoutRule runat="server" GroupCaption="Update Settings" 
				StartGroup="True">
			</px:PXLayoutRule>
			<px:PXCheckBox CommitChanges="True" ID="chkUpdateEnabled" runat="server" DataField="UpdateEnabled" />
			<px:PXTextEdit ID="edUpdateServer" runat="server" DataField="UpdateServer"  />
			<px:PXCheckBox ID="chkUpdateNotification" runat="server" DataField="UpdateNotification" />
			<px:PXLayoutRule runat="server" GroupCaption="Storage Settings" StartGroup="True" />
			<px:PXSelector CommitChanges="True" ID="edStorageProvider" runat="server" DataField="StorageProvider"  />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" MatrixMode="true"
		Width="100%" Height="150px" SkinID="Details" Caption="Storage Provider Parameters"
		OnRowDataBound="grid_RowDataBound">
		<Levels>
			<px:PXGridLevel  DataMember="StorageSettings" >
				<Columns>
					<px:PXGridColumn AllowUpdate="False" DataField="Name" Label="Name" Width="150px" />
					<px:PXGridColumn DataField="Value" Width="200px" Label="Value" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<Actions>
				<AddNew Enabled="False" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton CommandName="reloadParameters" CommandSourceID="ds" Text="Reload Parameters">
				    <AutoCallBack Command="reloadParameters" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
