<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM208020.aspx.cs"
    Inherits="Page_PM208020" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PM.TemplateGlobalTaskListMaint" PrimaryView="Tasks" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewTask" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary">
        <Levels>
            <px:PXGridLevel DataMember="Tasks">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSegmentMask ID="edTaskCD" runat="server" DataField="TaskCD" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                    <px:PXSelector ID="edBillingID" runat="server" DataField="BillingID" />
                    <px:PXDropDown ID="edBillingOption" runat="server" DataField="BillingOption" />
                    <px:PXSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" />
                    <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="true" />
                    <px:PXSegmentMask ID="edDefaultSubID" runat="server" DataField="DefaultSubID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="TaskCD" Label="Task ID" />
                    <px:PXGridColumn DataField="Description" Label="Description" />
                    <px:PXGridColumn DataField="BillingID" Label="Billing Rule" />
                    <px:PXGridColumn DataField="BillingOption" Label="Billing Option" RenderEditorText="True" />
                    <px:PXGridColumn DataField="WorkgroupID" Label="Workgroup ID" AutoCallBack="true" />
                    <px:PXGridColumn DataField="OwnerID" Label="Owner ID"  DisplayMode="Text"/>
                    <px:PXGridColumn DataField="DefaultSubID" Label="Default Sub." />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
