<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP201000.aspx.cs"
    Inherits="Page_EX201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.EP.PositionMaint" PrimaryView="EPPosition" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AllowSearch="true" DataSourceID="ds" AdjustPageSize="Auto" SkinID="Primary">
        <Levels>
            <px:PXGridLevel DataMember="EPPosition">
                <Columns>
                    <px:PXGridColumn DataField="PositionID" DisplayFormat="&gt;aaaaaaaaaa" />
					<px:PXGridColumn DataField="SDEnabled" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn AllowNull="False" DataField="Description" />
                </Columns>
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXMaskEdit ID="edPositionID" runat="server" DataField="PositionID" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox runat="server" ID="edSDEnabled" DataField="SDEnabled" />
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="200" Container="Window" />
    </px:PXGrid>
</asp:Content>
