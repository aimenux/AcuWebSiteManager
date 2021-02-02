<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR401000.aspx.cs"
    Inherits="Page_DR401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="Filter"
                     TypeName="PX.Objects.DR.SchedulesInq"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDropDown CommitChanges="True" ID="edAccountType" runat="server" DataField="AccountType"/>
			<px:PXSelector CommitChanges="True" ID="edDeferredCode" runat="server" DataField="DeferredCode" AutoRefresh="true"/>
            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" InitialExpandLevel="0" />
			<px:PXSegmentMask CommitChanges="True" ID="edComponentID" runat="server" DataField="ComponentID" AllowEdit="true" />
            <px:PXSelector CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXNumberEdit ID="edTotalScheduled" runat="server" DataField="TotalScheduled" />
            <px:PXNumberEdit ID="edTotalDeferred" runat="server" DataField="TotalDeferred" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Deferral Schedules"
        AllowPaging="True" AdjustPageSize="Auto" RestrictFields="True" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSelector ID="edScheduleID" runat="server" DataField="ScheduleID" Enabled="False" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                    <px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr" LinkCommand="ViewDocument" />
                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
                    <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" />
                    <px:PXNumberEdit ID="edTotalAmt" runat="server" DataField="TotalAmt" />
                    <px:PXNumberEdit ID="edDefAmt" runat="server" DataField="DefAmt" />
                    <px:PXTextEdit ID="edDefCode" runat="server" DataField="DefCode" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSegmentMask ID="edDefAcctID" runat="server" DataField="DefAcctID" />
                    <px:PXSegmentMask ID="edDefSubID" runat="server" DataField="DefSubID" />
                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="ScheduleID" TextAlign="Right" LinkCommand ="ViewSchedule" />
                    <px:PXGridColumn DataField="DocumentType" />
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="BranchID" />
                    <px:PXGridColumn DataField="ComponentCD" />
                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="Status" RenderEditorText="True" />
                    <px:PXGridColumn DataField="SignTotalAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="SignDefAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="DefCode" />
                    <px:PXGridColumn DataField="DefAcctID" />
                    <px:PXGridColumn DataField="DefSubID" />
                    <px:PXGridColumn DataField="AccountID" />
                    <px:PXGridColumn DataField="SubID" />
                    <px:PXGridColumn DataField="BAccountID" />
                    <px:PXGridColumn DataField="BAccountID_BAccountR_acctName" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
</asp:Content>
