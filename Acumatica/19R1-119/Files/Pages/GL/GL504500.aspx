<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL504500.aspx.cs" Inherits="Page_GL504500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
        TypeName="PX.Objects.GL.AllocationProcess" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Visible="False" Name="editDetail" />
            <px:PXDSCallbackCommand Name="viewBatch" Visible="False"></px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" Caption="Allocation Post Period"
        DataMember="Filter" DefaultControlID="edDateEntered">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDateEntered" runat="server" DataField="DateEntered" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100; left: 0px; top: 0px;"
        Width="100%" Caption="Active Allocations" AllowPaging="True" AllowSearch="true"
        SkinID="PrimaryInquire" AdjustPageSize="Auto" SyncPosition="true" FastFilterFields="Descr,GLAllocationID">
        <Levels>
            <px:PXGridLevel DataMember="Allocations">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True"
                        AllowSort="False" AllowMove="False" Width="30px" />
                    <px:PXGridColumn DataField="GLAllocationID" Width="100px" LinkCommand="EditDetail" />
                    <px:PXGridColumn DataField="Descr" Width="200px" />
                    <px:PXGridColumn DataField="AllocMethod" RenderEditorText="True" Width="150px" />
                    <px:PXGridColumn DataField="AllocLedgerID" Width="100px" />
                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="70px" />
                    <px:PXGridColumn DataField="BatchNbr" Width="70px" LinkCommand="viewBatch" />
                    <px:PXGridColumn DataField="BatchPeriod" Width="70px" />
                    <px:PXGridColumn DataField="ControlTotal" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="Status" RenderEditorText="True" Width="150px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode AllowSort="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
