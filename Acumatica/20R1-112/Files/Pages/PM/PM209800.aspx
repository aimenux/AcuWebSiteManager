<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM209800.aspx.cs"
    Inherits="Page_PM209800" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PM.WorkCodeMaint" PrimaryView="Items" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" SyncPosition="true" FastFilterFields="Description">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn DataField="IsActive" Type="CheckBox" />
                    <px:PXGridColumn DataField="WorkCodeID" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="CostCodeFrom" />
                    <px:PXGridColumn DataField="CostCodeTo" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowUpload="true" />
    </px:PXGrid>
</asp:Content>
