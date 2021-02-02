<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP204040.aspx.cs"
    Inherits="Page_EP204040" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Categories" TypeName="PX.Objects.EP.EventTaskCategoryMaint" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" ActionsPosition="Top" AllowSearch="true"
        DataSourceID="ds" SkinID="Primary">
        <Levels>
            <px:PXGridLevel DataMember="Categories">
                <Columns>
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="Color" TextAlign="Left" />
                </Columns>
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                    <px:PXDropDown ID="edColor" runat="server" DataField="Color" />
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
