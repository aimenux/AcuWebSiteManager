<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM202550.aspx.cs" Inherits="Page_SM202550"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Prefs"
		TypeName="PX.SM.UploadAllowedFileTypesMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Upload Settings"
		Width="100%" DataMember="Prefs">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
			<px:PXNumberEdit ID="edMaxUploadSize" runat="server" DataField="MaxUploadSize" Size="M" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		Caption="Allowed File Types" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto"
		DataSourceID="ds" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="PrefsDetail">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edFileExt" runat="server" DataField="FileExt" />
					<px:PXTextEdit ID="edIconUrl" runat="server" DataField="IconUrl" />
					<px:PXCheckBox ID="chkForbidden" runat="server" DataField="Forbidden" />
					<px:PXCheckBox ID="chkIsImage" runat="server" DataField="IsImage" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="FileExt" Width="108px" />
					<px:PXGridColumn DataField="IconUrl" Width="250px" />
					<px:PXGridColumn DataField="Forbidden" TextAlign="Center" Type="CheckBox" Width="90px" />
					<px:PXGridColumn DataField="IsImage" TextAlign="Center" Type="CheckBox" Width="90px" />
                    <px:PXGridColumn DataField="DefApplication" Width="300px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Enabled="True" Container="Window" MinHeight="100" />
		<ActionBar>
			<Actions>
				<EditRecord Enabled="False" />
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
