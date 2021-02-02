<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM201065.aspx.cs" Inherits="Page_SM201060" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Identities" TypeName="PX.SM.PreferencesIdentityProviderMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AdjustPageSize="Auto" AutoAdjustColumns="True" AllowSearch="True" SkinID="Primary" > 
		<Levels>
			<px:PXGridLevel DataMember="Identities">
				<Columns>
					<px:PXGridColumn DataField="ProviderName" Width="108px" />
					<px:PXGridColumn DataField="Active" Width="90px" TextAlign="Center" Type="CheckBox"  />
					<px:PXGridColumn DataField="Realm" Width="250px" />
					<px:PXGridColumn DataField="ApplicationID" Width="150px" />
					<px:PXGridColumn DataField="ApplicationSecret" Width="150px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Enabled="True" Container="Window" MinHeight="100" />
		<Mode AllowFormEdit="True" AllowAddNew="False" AllowDelete="False" />
		<ActionBar>
			<Actions>
				<EditRecord Enabled="False" />
				<NoteShow Enabled="False" />
				<PageNext Enabled="False" />
				<PagePrev Enabled="False" />
				<PageFirst Enabled="False" />
				<PageLast Enabled="False" />
				<AddNew Enabled="False" MenuVisible="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
