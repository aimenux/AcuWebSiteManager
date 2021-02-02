<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO409000.aspx.cs" Inherits="Pages_CO409000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
		function refreshTasksAndEvents(sender, args)
		{
			var top = window.top;
			if (top != window && top.MainFrame != null) top.MainFrame.refreshEventsInfo();
		}
	</script>    
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Emails" TypeName="PX.Objects.CR.CRCommunicationInbox">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="ViewEMail" Visible="False"/>
    		<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="reply" Visible="False" RepaintControls="All"/>
			<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="replyAll" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="forward" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="create"/>
			<px:PXDSCallbackCommand Name="relate" Visible="False"/>
            <px:PXDSCallbackCommand Name="screenActions" RepaintControls="All"/>
			<px:PXDSCallbackCommand Name="markAsRead" Visible="False" RepaintControls="All" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="markAsUnread" Visible="False" RepaintControls="All"/>
			<px:PXDSCallbackCommand Name="archive" Visible="False" RepaintControls="All"/>
			<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="viewEntity" Visible="false" RepaintControls="All" /> 
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="gridInbox" runat="server" DataSourceID="ds" ActionsPosition="Top" SyncPosition="True" OnRowDataBound="grid_RowDataBound"
		AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SkinID="PrimaryInquire" NoteIndicator="false"
		Width="100%" MatrixMode="True" RestrictFields="False" FastFilterFields="Subject,MailFrom,EMailAccount__Description">
	    <ClientEvents AfterRefresh="refreshTasksAndEvents" /> 
		<Levels>
			<px:PXGridLevel DataMember="Emails">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="50px" AllowCheckAll="True" TextAlign="Center" Type="CheckBox"/>
					<px:PXGridColumn DataField="EPView__Read" Width="50px" TextAlign="Center" Type="CheckBox" AllowShowHide="True" Visible="false" SyncVisible="false"/>
					<px:PXGridColumn DataField="EPView__Status" Width="50px" AllowShowHide="True" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="MailCc" Width="250px" AllowShowHide="True" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="MailBcc" Width="250px" AllowShowHide="True" Visible="false" SyncVisible="false"/>
					<px:PXGridColumn DataField="Subject" Width="450px" LinkCommand="ViewEMail" />
					<px:PXGridColumn DataField="MailFrom" Width="250px" />
                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
					<px:PXGridColumn DataField="MPStatus" Width="70px" />
					<px:PXGridColumn DataField="EMailAccount__Description" Width="200px" />
					<px:PXGridColumn DataField="Source" Width="150px" LinkCommand="viewEntity" AllowSort="false" AllowFilter="false" />
					<px:PXGridColumn DataField="RefNoteID" Width="150px" Visible="false" SyncVisible="false"/>
					<px:PXGridColumn DataField="WorkgroupID" Width="90px" DisplayMode="Text" />
					<px:PXGridColumn DataField="OwnerID" Width="90px" DisplayMode="Text" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="90px"/>
                    <px:PXGridColumn DataField="IsArchived" Width="70px" Type="CheckBox" TextAlign="Center" AllowShowHide="True" Visible="false" SyncVisible="false" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="DoubleClick" PagerVisible="False">
		    <Actions>
		        <AddNew Enabled="False"/>
                <Delete GroupIndex="1"/>
		    </Actions>
		</ActionBar>
		<Mode AllowAddNew="False" AllowUpdate="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
    <px:PXSmartPanel ID="spRelateDlg" runat="server" DesignView="Content" Key="entityFilter" LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel"
        Caption="Related Entity" CaptionVisible="True" ShowAfterLoad="true" AutoCallBack-Enabled="true" AutoCallBack-Target="RelateFrm" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXFormView ID="RelateFrm" runat="server" DataSourceID="ds" DataMember="entityFilter" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown CommitChanges="True" ID="edEntityType" runat="server" DataField="Type" />
                <pxa:PXDynamicSelector CommitChanges="True" ID="edEntityNoteID" runat="server" DataField="RefNoteID" AutoRefresh="True" FilterByAllFields="True"/>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
