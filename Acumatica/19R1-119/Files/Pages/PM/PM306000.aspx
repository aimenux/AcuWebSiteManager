<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM306000.aspx.cs"
    Inherits="Page_PM306000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.CommitmentInquiry"
        PrimaryView="Filter" BorderStyle="NotSet" PageLoadBehavior="PopulateSavedValues" >
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand Visible="false" Name="PMCommitment$RefNoteID$Link" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewProject" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="true" Name="CreateCommitment" CommitChanges="True" PopupCommand="Refresh" PopupCommandTarget="grid" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewExternalCommitment" CommitChanges="True" PopupCommand="Refresh" PopupCommandTarget="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
        EmailingGraph="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountGroupID" runat="server" DataField="AccountGroupID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" DataSourceID="ds" AutoRefresh="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edCostCode" runat="server" DataField="CostCode" DataSourceID="ds" AutoRefresh="True" />
            <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" />
            <px:PXLayoutRule ID="LayoutRule_QTY" runat="server" ControlSize="S" LabelsWidth="S" StartColumn="True" />
            <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" Enabled="False" />
            <px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" Enabled="False" />
            <px:PXNumberEdit ID="edOpenQty" runat="server" DataField="OpenQty" Enabled="False" />
            <px:PXNumberEdit ID="edOpenAmount" runat="server" DataField="OpenAmount" Enabled="False" />
             <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="S" LabelsWidth="SM" StartColumn="True" />
            <px:PXNumberEdit ID="edReceivedQty" runat="server" DataField="ReceivedQty" Enabled="False" />
            <px:PXNumberEdit ID="edInvoicedQty" runat="server" DataField="InvoicedQty" Enabled="False" />
            <px:PXNumberEdit ID="edInvoicedAmount" runat="server" DataField="InvoicedAmount" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" SyncPosition="true" FastFilterFields="ExtRefNbr, RefNoteID">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="PMCommitment$RefNoteID$Link" Width="160px"/>
                    <px:PXGridColumn DataField="Type" Width="63px" />
                    <px:PXGridColumn DataField="ProjectID" Width="90px" LinkCommand="ViewProject"/>
                    <px:PXGridColumn DataField="AccountGroupID" Width="90px" />
                    <px:PXGridColumn DataField="ProjectTaskID" Width="90px" />
                    <px:PXGridColumn DataField="InventoryID" Width="120px" CommitChanges="true" />
                    <px:PXGridColumn DataField="CostCodeID" Width="90px" />
                    <px:PXGridColumn DataField="ExtRefNbr" Width="90px" LinkCommand="ViewExternalCommitment" />
                    <px:PXGridColumn DataField="UOM" Width="81px" />
                    <px:PXGridColumn DataField="ProjectCuryID" Width="81px" />
                    <px:PXGridColumn DataField="OrigQty" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="OrigAmount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="CommittedCOQty" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="CommittedCOAmount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="OpenQty" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="OpenAmount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="ReceivedQty" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="InvoicedQty" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="InvoicedAmount" TextAlign="Right" Width="81px" />
                </Columns>
                <RowTemplate>
                    <px:PXSegmentMask ID="edInventoryIDGrid" runat="server" DataField="InventoryID" />
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowUpdate="false" AllowAddNew="false" AllowDelete="false"  />
        <CallbackCommands>
            <Refresh RepaintControlsIDs="form"/>
        </CallbackCommands>
    </px:PXGrid>
</asp:Content>
