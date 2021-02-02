<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM309000.aspx.cs"
    Inherits="Page_PM309000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PM.ProjectBalanceMaint" PrimaryView="Items" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewTask" Visible="False" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="ViewProject" Visible="False" CommitChanges="True"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" SyncPosition="true" FastFilterFields="ProjectID, ProjectTaskID, AccountGroupID, InventoryID, Description">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                     <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True" />
                     <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" />
                     <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True" />
                     <px:PXSelector ID="edAccountGroupID" runat="server" DataField="AccountGroupID" AutoRefresh="True" />
                     <px:PXSegmentMask ID="edCostCodeCost" runat="server" DataField="CostCodeID" AllowAddNew="true" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="ProjectID" Width="108px" CommitChanges="true" LinkCommand="ViewProject"/>
                    <px:PXGridColumn DataField="ProjectTaskID" Width="108px" CommitChanges="true" LinkCommand="ViewTask"/>
                    <px:PXGridColumn DataField="CostCodeID" Width="108px" CommitChanges="true" />
                    <px:PXGridColumn DataField="AccountGroupID" Width="108px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="InventoryID" Width="108px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="Description" Width="108px" />
                    <px:PXGridColumn DataField="UOM" Width="80px" CommitChanges="true"/>
					<px:PXGridColumn DataField="PMProject__CuryID" Width="50px" />
                    <px:PXGridColumn DataField="Qty" Width="80px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="CuryUnitRate" Width="80px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="CuryAmount" Width="80px" CommitChanges="true" />
                    <px:PXGridColumn DataField="RevisedQty" Width="80px" CommitChanges="true"/>
                    <px:PXGridColumn DataField="CuryRevisedAmount" Width="80px" CommitChanges="true" />
                    <px:PXGridColumn DataField="CommittedQty" Width="80px" />
                    <px:PXGridColumn DataField="CuryCommittedAmount" Width="80px" />
                    <px:PXGridColumn DataField="CommittedOpenQty" Width="80px" />
                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" Width="80px" />
                    <px:PXGridColumn DataField="CommittedReceivedQty" Width="80px" />
                    <px:PXGridColumn DataField="CommittedInvoicedQty" Width="80px" />
                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" Width="80px" />
                    <px:PXGridColumn DataField="ActualQty" Width="80px" />
                    <px:PXGridColumn DataField="CuryActualAmount" Width="80px" />
                    <px:PXGridColumn DataField="Type" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowUpload="true" />
    </px:PXGrid>
</asp:Content>
