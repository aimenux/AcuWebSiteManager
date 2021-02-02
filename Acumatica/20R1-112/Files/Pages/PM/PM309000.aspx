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
                    <px:PXGridColumn DataField="ProjectID" CommitChanges="true" LinkCommand="ViewProject"/>
                    <px:PXGridColumn DataField="ProjectTaskID" CommitChanges="true" LinkCommand="ViewTask"/>
                    <px:PXGridColumn DataField="CostCodeID" CommitChanges="true" />
                    <px:PXGridColumn DataField="AccountGroupID" CommitChanges="true"/>
                    <px:PXGridColumn DataField="InventoryID" CommitChanges="true"/>
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="UOM" CommitChanges="true"/>
					<px:PXGridColumn DataField="PMProject__CuryID" />
                    <px:PXGridColumn DataField="Qty" CommitChanges="true"/>
                    <px:PXGridColumn DataField="CuryUnitRate" CommitChanges="true"/>
                    <px:PXGridColumn DataField="CuryAmount" CommitChanges="true" />
                    <px:PXGridColumn DataField="RevisedQty" CommitChanges="true"/>
                    <px:PXGridColumn DataField="CuryRevisedAmount" CommitChanges="true" />
                    <px:PXGridColumn DataField="CommittedQty" />
                    <px:PXGridColumn DataField="CuryCommittedAmount" />
                    <px:PXGridColumn DataField="CommittedOpenQty" />
                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" />
                    <px:PXGridColumn DataField="CommittedReceivedQty" />
                    <px:PXGridColumn DataField="CommittedInvoicedQty" />
                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" />
                    <px:PXGridColumn DataField="ActualQty" />
                    <px:PXGridColumn DataField="CuryActualAmount" />
                    <px:PXGridColumn DataField="Type" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowUpload="true" />
    </px:PXGrid>
</asp:Content>
