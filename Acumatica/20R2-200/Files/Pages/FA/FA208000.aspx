<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA208000.aspx.cs"
    Inherits="Page_FA208000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.BonusMaint" PrimaryView="Bonuses">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Bonus Summary" DataMember="Bonuses"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector runat="server" DataField="BonusCD" ID="edBonusCD" AutoRefresh="True" />
            <px:PXTextEdit runat="server" DataField="Description" ID="edDescription" Size="XL" />
            </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Details">
        <Levels>
            <px:PXGridLevel DataMember="Details">
                <Columns>
                    <px:PXGridColumn AutoCallBack="True" DataField="StartDate" Label="Start Date" />
                    <px:PXGridColumn AutoCallBack="True" DataField="EndDate" Label="End Date" />
                    <px:PXGridColumn DataField="BonusPercent" Label="Bonus, %" TextAlign="Right" />
                    <px:PXGridColumn DataField="BonusMax" Label="Max. Bonus" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
