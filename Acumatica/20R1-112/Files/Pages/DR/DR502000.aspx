<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR502000.aspx.cs"
    Inherits="Page_DR502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.DR.DRProjection">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Filter" DataMember="Filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector runat="server" DataField="DeferredCode" ID="edDeferredCode" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Deferrred Schedules">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSelector ID="edScheduleID" runat="server" DataField="ScheduleID" Enabled="False" />
                    <px:PXSelector ID="edComponentID" runat="server" DataField="ComponentID" Enabled="False" />
                    <px:PXNumberEdit ID="edTotalAmt" runat="server" DataField="TotalAmt" Enabled="False" />
                    <px:PXNumberEdit ID="edDefAmt" runat="server" DataField="DefAmt" Enabled="False" />
                    <px:PXTextEdit ID="edDefCode" runat="server" DataField="DefCode" Enabled="False" />
                    <px:PXSegmentMask ID="edDefAcctID" runat="server" DataField="DefAcctID" Enabled="False" />
                    <px:PXSegmentMask ID="edDefSubID" runat="server" DataField="DefSubID" Enabled="False" />
                    <px:PXTextEdit ID="edDocType" runat="server" DataField="DocType" Enabled="False" />
                    <px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ScheduleID" TextAlign="Right" />
                    <px:PXGridColumn DataField="ComponentID" TextAlign="Right" />
                    <px:PXGridColumn DataField="TotalAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="DefAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="DefCode" />
                    <px:PXGridColumn DataField="DefAcctID" />
                    <px:PXGridColumn DataField="DefSubID" />
                    <px:PXGridColumn DataField="DocType" />
                    <px:PXGridColumn DataField="RefNbr" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
