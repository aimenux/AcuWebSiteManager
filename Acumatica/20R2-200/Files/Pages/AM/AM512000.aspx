<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM512000.aspx.cs" Inherits="Page_AM512000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.APSMaintenanceProcess" PrimaryView="ProcessingRecords" BorderStyle="NotSet" >
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Process Filter" DefaultControlID="IsWorkCenterCalendarProcess" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXCheckBox ID="edIsWorkCenterCalendarProcess" runat="server"  DataField="IsWorkCenterCalendarProcess" AlignLeft="True" />
            <px:PXCheckBox ID="edIsHistoryCleanupProcess" runat="server"  DataField="IsHistoryCleanupProcess" AlignLeft="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="ProcessingRecords">
		<Items>
			<px:PXTabItem Text="Process History">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />

                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Work Center Schedule" />
                    <px:PXDateTimeEdit ID="edWorkCenterCalendarProcessLastRunDateTime" runat="server"  DataField="WorkCenterCalendarProcessLastRunDateTime" Width="200px"/>
                    <px:PXTextEdit ID="edWorkCenterCalendarProcessLastRunByID" runat="server" DataField="WorkCenterCalendarProcessLastRunByID" />
                    
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Block Size Change" />
                    <px:PXDateTimeEdit ID="edBlockSizeSyncProcessLastRunDateTime" runat="server"  DataField="BlockSizeSyncProcessLastRunDateTime" Width="200px"/>
                    <px:PXTextEdit ID="edBlockSizeSyncProcessLastRunByID" runat="server" DataField="BlockSizeSyncProcessLastRunByID" />
                    <px:PXDropDown ID="edLastBlockSize" runat="server" DataField="LastBlockSize" />
                    <px:PXDropDown ID="edCurrentBlockSize" runat="server" DataField="CurrentBlockSize" />
                    
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="History Cleanup" />
                    <px:PXDateTimeEdit ID="edHistoryCleanupProcessLastRunDateTime" runat="server"  DataField="HistoryCleanupProcessLastRunDateTime" Width="200px"/>
                    <px:PXTextEdit ID="edHistoryCleanupProcessLastRunByID" runat="server" DataField="HistoryCleanupProcessLastRunByID" />
                    
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Work Calendar" />
                    <px:PXDateTimeEdit ID="edWorkCalendarProcessLastRunDateTime" runat="server"  DataField="WorkCalendarProcessLastRunDateTime" Width="200px"/>
                    <px:PXTextEdit ID="edWorkCalendarProcessLastRunByID" runat="server" DataField="WorkCalendarProcessLastRunByID" />

                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
</asp:Content>
