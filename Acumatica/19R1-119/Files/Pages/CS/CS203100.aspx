<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS203100.aspx.cs"
    Inherits="Page_CS203100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.IN.INUnitMaint" PrimaryView="Unit" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AllowSearch="true" DataSourceID="ds" SkinID="Primary" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="Unit">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXNumberEdit ID="edItemClassID" runat="server" DataField="ItemClassID" />
                    <px:PXNumberEdit ID="edInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXMaskEdit ID="edFromUnit" runat="server" DataField="FromUnit" InputMask=">aaaaaa" />
                    <px:PXMaskEdit ID="edToUnit" runat="server" DataField="ToUnit" InputMask=">aaaaaa" />
                    <px:PXNumberEdit ID="edUnitRate" runat="server" DataField="UnitRate" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="UnitType" Type="DropDownList" Width="99px" Visible="false" />
                    <px:PXGridColumn AllowNull="False" DataField="ItemClassID" Width="36px" Visible="false" />
                    <px:PXGridColumn AllowNull="False" DataField="InventoryID" Visible="false" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn DataField="FromUnit" Width="72px" />
                    <px:PXGridColumn DataField="ToUnit" Width="72px" />
                    <px:PXGridColumn AllowNull="False" DataField="UnitMultDiv" Type="DropDownList" Width="102px" />
                    <px:PXGridColumn AllowNull="False" DataField="UnitRate" TextAlign="Right" Width="138px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
