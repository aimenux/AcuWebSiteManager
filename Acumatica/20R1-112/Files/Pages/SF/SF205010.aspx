<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SF205010.aspx.cs" Inherits="Page_SF205000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Records" TypeName="PX.Salesforce.SFSyncRecordMaint">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Records" TabIndex="900">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="M"/>
		    <px:PXSelector ID="edSyncRecordID" runat="server" CommitChanges="True" DataField="SyncRecordID">
            </px:PXSelector>
		    <px:PXDropDown ID="edEntityType" runat="server" DataField="EntityType">
            </px:PXDropDown>
            <px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Local info" StartGroup="True">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edLocalID" runat="server" AlreadyLocalized="False" DataField="LocalID">
            </px:PXTextEdit>
            <px:PXDateTimeEdit ID="edLocalTS" runat="server" AlreadyLocalized="False" DataField="LocalTS" Size="M" DefaultLocale="">
            </px:PXDateTimeEdit>
            <px:PXTextEdit ID="edLocalGuid" runat="server" AlreadyLocalized="False" DataField="LocalGuid" DefaultLocale="">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" EndGroup="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Remote info" StartGroup="True">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edRemoteID" runat="server" AlreadyLocalized="False" DataField="RemoteID" DefaultLocale="">
            </px:PXTextEdit>
            <px:PXDateTimeEdit ID="edRemoteTS" runat="server" AlreadyLocalized="False" DataField="RemoteTS" Size="M" DefaultLocale="">
            </px:PXDateTimeEdit>
            <px:PXLinkEdit ID="edSalesforceUrl" runat="server" AlreadyLocalized="False" DataField="SalesforceUrl" Enabled="False" Size="XL">
            </px:PXLinkEdit>
            <px:PXLayoutRule runat="server" EndGroup="True">
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Sync info" StartGroup="True">
            </px:PXLayoutRule>
            <px:PXCheckBox ID="edPendingSync" runat="server" AlreadyLocalized="False" DataField="PendingSync" Text="Pending Sync">
            </px:PXCheckBox>
            <px:PXNumberEdit ID="edAttemptCount" runat="server" AlreadyLocalized="False" DataField="AttemptCount" Size="M" DefaultLocale="">
            </px:PXNumberEdit>
            <px:PXDropDown ID="edLastOperation" runat="server" DataField="LastOperation">
            </px:PXDropDown>
            <px:PXTextEdit ID="edLastErrorMessage" runat="server" AlreadyLocalized="False" DataField="LastErrorMessage" Height="200px" Size="XXL" TextMode="MultiLine" DefaultLocale="">
            </px:PXTextEdit>
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
