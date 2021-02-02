<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
 ValidateRequest="false" CodeFile="FS400100.aspx.cs" Inherits="Page_FS400100" 
 Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%"  Visible="True" runat="server" TypeName="PX.Objects.FS.AppointmentInq"  PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
   <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter" NoteField="" AllowCollapse="True" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector runat="server" DataSourceID="ds" ID="edBranchID" DataField="BranchID" CommitChanges="True" />
            <px:PXSelector runat="server" DataSourceID="ds" ID="edBranchLocationID" DataField="BranchLocationID" CommitChanges="True" AutoRefresh="True" />
            <px:PXSegmentMask runat="server" DataSourceID="ds" ID="edCustomerID" DataField="CustomerID" CommitChanges="True" />
            <px:PXSegmentMask runat="server" DataSourceID="ds" ID="edCustomerLocationID" DataField="CustomerLocationID" CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector runat="server" DataSourceID="ds" ID="edSORefNbr" DataField="SORefNbr" CommitChanges="True" AutoRefresh="True" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector runat="server" DataSourceID="ds" ID="edServiceContractID" DataField="ServiceContractID" CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector runat="server" DataSourceID="ds" ID="edScheduleID" DataField="ScheduleID" CommitChanges="True" AutoRefresh="True" />
            <px:PXSegmentMask runat="server" DataSourceID="ds" ID="edStaffMemberID" DataField="StaffMemberID" CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector runat="server" DataSourceID="ds" ID="edSMEquipmentID" DataField="SMEquipmentID" CommitChanges="True" AutoRefresh="True" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDateTimeEdit runat="server" DataSourceID="ds" ID="edFromScheduledDate" DataField="FromScheduledDate_Date" CommitChanges="True" />
            <px:PXDateTimeEdit runat="server" DataSourceID="ds" ID="edToScheduledDate" DataField="ToScheduledDate_Date" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="PrimaryInquire" FastFilterFields="SrvOrdType,SORefNbr,RefNbr,DocDesc,CustomerID,LocationID" SyncPosition="True" AutoSize="True">
        <Levels>
            <px:PXGridLevel DataMember="Appointments">
                <RowTemplate>
                    <px:PXSelector runat="server" ID="edBranchID" DataField="BranchID" />
                    <px:PXSelector runat="server" ID="edBranchLocationID" DataField="BranchLocationID" />
                    <px:PXSelector runat="server" ID="edSrvOrdType" DataField="SrvOrdType" />
                    <px:PXSelector runat="server" ID="edSORefNbr" DataField="SORefNbr" AllowEdit="True" />
                    <px:PXSelector runat="server" ID="edRefNbr" DataField="RefNbr" />
                    <px:PXTextEdit runat="server" ID="edDocDesc" DataField="DocDesc" />
                    <px:PXSegmentMask runat="server" ID="edCustomerID" DataField="CustomerID" AllowEdit="True" />
                    <px:PXSegmentMask runat="server" ID="edLocationID" DataField="LocationID" />
                    <px:PXSegmentMask runat="server" ID="edBillCustomerID" DataField="BillCustomerID" AllowEdit="True" />
                    <px:PXSegmentMask runat="server" ID="edBillLocationID" DataField="BillLocationID" />
                    <px:PXDateTimeEdit runat="server" ID="edScheduledDateTimeBegin" DataField="ScheduledDateTimeBegin" />
                    <px:PXDateTimeEdit runat="server" ID="edScheduledDateTimeBeginTime" DataField="ScheduledDateTimeBegin_Time" TimeMode="True"/>
                    <px:PXDateTimeEdit runat="server" ID="edActualDateTimeBegin" DataField="ActualDateTimeBegin" />
                    <px:PXDropDown runat="server" ID="edStatus" DataField="Status" />
                    <px:PXCheckBox runat="server" ID="edFinished" DataField="Finished" />
                    <px:PXSelector runat="server" ID="edWFStageID" DataField="WFStageID" />
                    <px:PXCheckBox runat="server" ID="edConfirmed" DataField="Confirmed" />
                    <px:PXDropDown runat="server" ID="edPriority" DataField="Priority" />
                    <px:PXMaskEdit runat="server" ID="edEstimatedDurationTotal" DataField="EstimatedDurationTotal" />
                    <px:PXMaskEdit runat="server" ID="edActualDurationTotal" DataField="ActualDurationTotal" />
                    <px:PXSelector runat="server" ID="edBillingCycleID" DataField="FSCustomerBillingSetup__BillingCycleID" />
                    <px:PXSelector runat="server" ID="edBillServiceContractID" DataField="BillServiceContractID"/>
                    <px:PXSelector runat="server" ID="edServiceContractID2" DataField="ServiceContractID"/>
                    <px:PXTextEdit runat="server" ID="edState" DataField="State" />
                    <px:PXTextEdit runat="server" ID="edCity" DataField="City" />
                    <px:PXSelector runat="server" ID="edGeoZoneID" DataField="FSGeoZonePostalCode__GeoZoneID" />
                    <px:PXMaskEdit runat="server" ID="edTotalAttendees" DataField="TotalAttendees" />
                    <px:PXSelector runat="server" ID="edRoomID" DataField="RoomID" />
                    <px:PXSelector runat="server" ID="edRouteID" DataField="RouteID" AllowEdit="True" />
                    <px:PXSelector runat="server" ID="edRouteDocumentID" DataField="RouteDocumentID" AllowEdit="True" />
                    <px:PXCheckBox runat="server" ID="edWaitingForParts" DataField="WaitingForParts" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="BranchID" Width="90px" />
                    <px:PXGridColumn DataField="BranchLocationID" Width="90px" />
                    <px:PXGridColumn DataField="SrvOrdType" Width="90px" />
                    <px:PXGridColumn DataField="SORefNbr" Width="90px" />
                    <px:PXGridColumn DataField="RefNbr" Width="90px" LinkCommand="EditDetail" AutoCallBack="True" />
                    <px:PXGridColumn DataField="DocDesc" Width="300px" />
                    <px:PXGridColumn DataField="CustomerID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="LocationID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="BillCustomerID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="BillLocationID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="ScheduledDateTimeBegin" Width="100px" />
                    <px:PXGridColumn DataField="ScheduledDateTimeBegin_Time" Width="100px" />
                    <px:PXGridColumn DataField="ActualDateTimeBegin" Width="100px" />
                    <px:PXGridColumn DataField="Status" Width="100px" />
                    <px:PXGridColumn DataField="Finished" Width="80px" />
                    <px:PXGridColumn DataField="WFStageID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="Confirmed" Width="80px" />
					<px:PXGridColumn DataField="EstimatedLineTotal" Width="100px" />
                    <px:PXGridColumn DataField="LineTotal" Width="100px" />
                    <px:PXGridColumn DataField="Priority" Width="80px" />
                    <px:PXGridColumn DataField="EstimatedDurationTotal" Width="100px" />
                    <px:PXGridColumn DataField="ActualDurationTotal" Width="100px" />
                    <px:PXGridColumn DataField="FSCustomerBillingSetup__BillingCycleID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="ServiceContractID" Width="100px" />
                    <px:PXGridColumn DataField="BillServiceContractID" Width="100px" />
                    <px:PXGridColumn DataField="State" Width="120px" />
                    <px:PXGridColumn DataField="City" Width="120px" />
                    <px:PXGridColumn DataField="FSGeoZonePostalCode__GeoZoneID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="TotalAttendees" Width="100px" />
                    <px:PXGridColumn DataField="RoomID" Width="100px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="RouteID" Width="200px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="RouteDocumentID" Width="100px" />
                    <px:PXGridColumn DataField="WaitingForParts" Width="80px" />
                    <px:PXGridColumn DataField="ProjectID" Width="80px" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="DfltProjectTaskID" Width="80px" DisplayMode="Hint" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
        <ActionBar DefaultAction="EditDetail" PagerVisible="False"></ActionBar>
    </px:PXGrid>
</asp:Content>
