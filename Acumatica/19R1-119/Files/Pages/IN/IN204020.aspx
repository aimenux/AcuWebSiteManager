<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN204020.aspx.cs"
    Inherits="Page_IN204020" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="PopulateSavedValues" PrimaryView="Filter" TypeName="PX.Objects.IN.WarehouseProductivityEnq"/>
 </asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify"
        DataMember="Filter" DefaultControlID="edStartDate" TabIndex="5500">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XL" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" CommitChanges="True" DataField="StartDate" DisplayFormat="d"/>
            <px:PXDateTimeEdit ID="edEndDate" runat="server" CommitChanges="True" DataField="EndDate" DisplayFormat="d"/>
            <px:PXCheckBox CommitChanges="True" ID="chkByUser" runat="server" DataField="ExpandByUser" />
            <px:PXSelector CommitChanges="True" ID="edUser" runat="server" DataField="UserID" />
            <px:PXCheckBox CommitChanges="True" ID="chkByShipment" runat="server" DataField="ExpandByShipment" />
            <px:PXSelector CommitChanges="True" ID="edShipment" runat="server" DataField="ShipmentNbr" />
            <px:PXCheckBox CommitChanges="True" ID="chkByDay" runat="server" DataField="ExpandByDay" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" AdjustPageSize="Auto"
        AllowPaging="True" AllowSearch="True" BatchUpdate="True" Caption="Efficiency" SkinID="PrimaryInquire" RestrictFields="True"
        SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Efficiency">
                <Columns>
                    <px:PXGridColumn DataField="Day" />
                    <px:PXGridColumn DataField="StartDate" />
                    <px:PXGridColumn DataField="EndDate" />
                    <px:PXGridColumn DataField="UserID" />
                    <px:PXGridColumn DataField="ShipmentNbr" />
                    <px:PXGridColumn DataField="AmountOfShipments" />
                    <px:PXGridColumn DataField="AmountOfLines" />
                    <px:PXGridColumn DataField="AmountOfPackages" />
                    <px:PXGridColumn DataField="AmountOfInventories" />
                    <px:PXGridColumn DataField="TotalQty" />
                    <px:PXGridColumn DataField="QtyOfUsefulOperations" />
                    <px:PXGridColumn DataField="TotalTime" />
                    <px:PXGridColumn DataField="Efficiency" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>