<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN208500.aspx.cs"
    Inherits="Page_IN208500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INABCCodeMaint" PrimaryView="ABCCodes" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" Visible="False" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Previous" Visible="False" />
            <px:PXDSCallbackCommand Name="Next" Visible="False" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ABCTotals" Caption="ABC Code Summary">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXNumberEdit ID="edTotalABCPct" runat="server" AllowNull="False" DataField="TotalABCPct" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        AllowSearch="True" SkinID="Details">
        <Mode InitNewRow="True" />
        <Levels>
            <px:PXGridLevel DataMember="ABCCodes">
                <Columns>
                    <px:PXGridColumn DataField="ABCCodeID" Required="True" TextCase="Upper" />
                    <px:PXGridColumn DataField="Descr" Width="200px" />
                    <px:PXGridColumn DataField="CountsPerYear" DataType="Int16" TextAlign="Right" AutoCallBack="True" />
                    <px:PXGridColumn AllowNull="False" DataField="ABCPct" DataType="Decimal" DefValueText="0.0" TextAlign="Right" Width="100px"
                        AutoCallBack="True" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
