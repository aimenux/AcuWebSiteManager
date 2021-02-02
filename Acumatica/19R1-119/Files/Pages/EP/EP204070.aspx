<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP204070.aspx.cs" Inherits="Page_EP204070" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Setup" TypeName="PX.Objects.EP.EPEventSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Caption="Event Settings"
		Style="z-index: 100" Width="100%" DataMember="Setup" TemplateContainer="" >
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" GroupCaption="Attendee Notification" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOnlyEventCard" runat="server" Checked="True" DataField="SendOnlyEventCard" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkSimpleNotification" runat="server" Checked="True" DataField="IsSimpleNotification" />
            <px:PXCheckBox SuppressLabel="True" ID="chkAddContactInformation" runat="server" Checked="True" DataField="AddContactInformation" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
	        <px:PXSelector CommitChanges="True" ID="edInvitationTemplateID" runat="server" DataField="InvitationTemplateID" DisplayMode="Text" AllowEdit="True" edit="1" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
	        <px:PXSelector CommitChanges="True" ID="edRescheduleTemplateID" runat="server" DataField="RescheduleTemplateID" DisplayMode="Text" AllowEdit="True" edit="1" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
	        <px:PXSelector CommitChanges="True" ID="edCancelInvitationTemplateID" runat="server" DataField="CancelInvitationTemplateID" DisplayMode="Text" AllowEdit="True" 
                edit="1" />
            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartGroup="True" GroupCaption="Schedule" />
            <px:PXCheckBox SuppressLabel="True" ID="chkSearchOnlyInWorkingTime" runat="server" Checked="True" DataField="SearchOnlyInWorkingTime" />

        </Template>
	    
	</px:PXFormView>
</asp:Content>
