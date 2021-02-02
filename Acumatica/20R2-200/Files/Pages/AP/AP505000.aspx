<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP505000.aspx.cs" Inherits="Page_AP505000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APPrintChecks" PrimaryView="Filter" PageLoadBehavior="GoLastRecord" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edPayTypeID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edPayTypeID" runat="server" DataField="PayTypeID" AutoRefresh="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edPayAccountID" runat="server" DataField="PayAccountID" AutoRefresh="True"/>
            <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
            <px:PXTextEdit CommitChanges="True" ID="edNextCheckNbr" runat="server" DataField="NextCheckNbr" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXNumberEdit ID="edGLBalance" runat="server" DataField="GLBalance" Enabled="False" />
            <px:PXNumberEdit ID="edCashBalance" runat="server" DataField="CashBalance" Enabled="False" />
            <px:PXNumberEdit ID="edCurySelTotal" runat="server" DataField="CurySelTotal" Enabled="False" />
            <px:PXNumberEdit ID="edSelCount" runat="server" DataField="SelCount" Enabled="False" />
           <%-- <px:PXSelector CommitChanges="True" ID="edPrinterName" runat="server" DataField="PrinterName" />--%>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100" Width="100%" Caption="Payments" AllowPaging="true" AdjustPageSize="Auto"
        SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="ExtRefNbr,RefNbr,VendorID,VendorID_Vendor_acctName">
        <Levels>
            <px:PXGridLevel DataMember="APPaymentList">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="ExtRefNbr" />
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="DocType" />
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="viewDocument" />
                    <px:PXGridColumn DataField="VendorID" />
                    <px:PXGridColumn DataField="VendorID_Vendor_acctName" />
                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
