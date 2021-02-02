<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM505000.aspx.cs"
    Inherits="Page_PM505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ReverseUnbilledProcess" PrimaryView="Filter"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="BillingID" ID="edBillingID" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="CustomerClassID" ID="edCustomerClassID" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="CustomerID"
                ID="edCustomerID" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="ResourceID" ID="edResourceID" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="ProjectID" ID="edProjectID" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="ProjectTaskID"
                ID="edProjectTaskID" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="InventoryID"
                ID="edInventoryID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="s" />
            
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="DateFrom" ID="edDateFrom" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="DateTo" ID="edDateTo" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" AdjustPageSize="Auto"
        AllowPaging="True" Caption="Transactions" SyncPosition="True" FastFilterFields="RefNbr,ProjectID,TaskID,Description">
        <Levels>
            <px:PXGridLevel DataKeyNames="TranID" DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowShowHide="False" />
                    <px:PXGridColumn DataField="RefNbr" Label="Ref Number" LinkCommand="ViewDocument" />
                    <px:PXGridColumn DataField="Date" />
                    <px:PXGridColumn DataField="FinPeriodID"/>
                    <px:PXGridColumn DataField="ProjectID"/>
                    <px:PXGridColumn DataField="TaskID"/>
                    <px:PXGridColumn DataField="Customer__CustomerClassID" />
                    <px:PXGridColumn DataField="PMProject__CustomerID" />
                    <px:PXGridColumn DataField="ResourceID"/>
                    <px:PXGridColumn DataField="InventoryID"/>
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="UOM" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                    <px:PXGridColumn DataField="Billable" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="BillableQty" TextAlign="Right" />
                    <px:PXGridColumn DataField="UnitRate" TextAlign="Right" />
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" />
                    <px:PXGridColumn DataField="StartDate" />
                    <px:PXGridColumn DataField="EndDate" />
                    <px:PXGridColumn DataField="PMTask__BillingID"/>
                    <px:PXGridColumn DataField="BranchID"/>
                    <px:PXGridColumn DataField="EarningType" />
                    <px:PXGridColumn DataField="OvertimeMultiplier" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar DefaultAction="ViewDocument" PagerVisible="False"/>
    </px:PXGrid>
</asp:Content>
