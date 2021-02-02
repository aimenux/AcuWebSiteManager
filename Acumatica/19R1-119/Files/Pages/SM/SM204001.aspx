<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204001.aspx.cs" Inherits="Page_SM204001"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:DynamicDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Prefs"
		TypeName="PX.SM.PreferencesEmailMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="EMailAccount_New" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="EMailAccount_View" Visible="False" DependOnGrid="gridEMail" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="EMailAccount_Delete" Visible="False" DependOnGrid="gridEMail" />
		</CallbackCommands>
	</pxa:DynamicDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="General Settings"
		Width="100%" DataMember="Prefs">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="M" />
			<px:PXSelector ID="edDefaultEMailAccount" runat="server" DataField="DefaultEMailAccountID"
				FilterByAllFields="True" DisplayMode="Text" />
            <px:PXTextEdit ID="edEmailTagPrefix" runat="server" DataField="EmailTagPrefix" />
            <px:PXTextEdit ID="edEmailTagSuffix" runat="server" DataField="EmailTagSuffix" />
		    <px:PXDropDown ID="edArchiveEmailsOlderThan" runat="server" DataField="ArchiveEmailsOlderThan" />
            <px:PXNumberEdit ID="edRepeatOnErrorSending" runat="server" DataField="RepeatOnErrorSending" />
            <px:PXCheckBox ID="lblSuspendEmailProcessing" runat="server" DataField="SuspendEmailProcessing" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="M" />
            <px:PXSelector ID="PXSelector1" runat="server" DataField="UserWelcomeNotificationId"
				FilterByAllFields="True" DisplayMode="Text" />
            <px:PXSelector ID="PXSelector4" runat="server" DataField="PasswordChangedNotificationId"
				FilterByAllFields="True" DisplayMode="Text" />
            <px:PXSelector ID="PXSelector2" runat="server" DataField="LoginRecoveryNotificationId"
				FilterByAllFields="True" DisplayMode="Text" />
            <px:PXSelector ID="PXSelector3" runat="server" DataField="PasswordRecoveryNotificationId"
				FilterByAllFields="True" DisplayMode="Text" />
            <px:PXSelector ID="PXSelector5" runat="server" DataField="TwoFactorNewDeviceNotificationId"
				FilterByAllFields="True" DisplayMode="Text" />
            <px:PXSelector ID="PXSelector6" runat="server" DataField="TwoFactorCodeByNotificationId"
				FilterByAllFields="True" DisplayMode="Text" />
		    <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="NotificationSiteUrl" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="gridEMail" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Height="150px" SkinID="Inquire" Caption="Email Accounts" FeedbackMode="DisableAll">
		<Levels>
			<px:PXGridLevel DataMember="EMailAccounts">
				<Columns>
					<px:PXGridColumn DataField="Description" Width="300px" LinkCommand="EMailAccount_View" />
					<px:PXGridColumn DataField="Address" Width="250px" />
					<px:PXGridColumn DataField="LoginName" Width="180px" />
					<px:PXGridColumn DataField="OutcomingHostName" Width="180px" />
					<px:PXGridColumn DataField="IncomingHostName" Width="180px" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXTextEdit ID="edAddress" runat="server" DataField="Address" />
					<px:PXTextEdit ID="edLoginName" runat="server" DataField="LoginName" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="View">
			<Actions>
				<Save Enabled="False" />
				<EditRecord Enabled="False" />
				<NoteShow Enabled="False" />
				<ExportExcel Enabled="False" />
				<PageNext Enabled="False" />
				<PagePrev Enabled="False" />
				<PageFirst Enabled="False" />
				<PageLast Enabled="False" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton Text="Insert" ImageKey="AddNew" DisplayStyle="Image">
					<AutoCallBack Command="EMailAccount_New" Target="ds" />
					<PopupCommand Command="Refresh" Target="gridEMail" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Delete" ImageKey="Remove" DisplayStyle="Image">
					<AutoCallBack Command="EMailAccount_Delete" Target="ds" />
					<PopupCommand Command="Refresh" Target="gridEMail" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="View" Key="View">
					<AutoCallBack Command="EMailAccount_View" Target="ds" />
					<PopupCommand Command="Refresh" Target="gridEMail" />	 
					<ActionBar GroupIndex="0" Order="0"/>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<LevelStyles>
			<RowForm Height="265px" Width="640px">
			</RowForm>
		</LevelStyles>
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowRowSelect="False" />
		<AutoSize Enabled="True" Container="Window" MinHeight="100" />
	</px:PXGrid>
</asp:Content>
