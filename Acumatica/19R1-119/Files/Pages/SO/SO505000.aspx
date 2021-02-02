<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO505000.aspx.cs"
    Inherits="Page_SO505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.SO.SOReleaseInvoice"
                     PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
        DefaultControlID="edAction" AllowCollapse="false">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" DataField="Action" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowFailedCCCapture" runat="server" DataField="ShowFailedCCCapture" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" AlignLeft="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" AlignLeft="true" />
            <px:PXSelector CommitChanges="True" ID="edPrinterName" runat="server" DataField="PrinterName" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" 
        AllowSearch="true" DataSourceID="ds" BatchUpdate="True" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True" FastFilterFields="RefNbr,CustomerID,CustomerID_BAccountR_acctName,InvoiceNbr,DocDesc">
        <Levels>
            <px:PXGridLevel DataMember="SOInvoiceList">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector SuppressLabel="True" ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask SuppressLabel="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
                    <px:PXDateTimeEdit SuppressLabel="True" ID="edDocDate" runat="server" DataField="DocDate" Enabled="False" />
                    <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" />
                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
                    <px:PXSelector SuppressLabel="True" ID="edCuryID" runat="server" DataField="CuryID" />
                    <px:PXTextEdit SuppressLabel="True" ID="edDocDesc" runat="server" DataField="DocDesc" /></RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False"
                        AllowMove="False" Width="30px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocType" Type="DropDownList" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RefNbr" Width="90px" LinkCommand="viewDocument" />
                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="117px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID" DisplayFormat="&gt;AAAAAAA" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID_Location_descr" Width="117px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InvoiceNbr" Width="108px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" Type="DropDownList" Width="126px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" DisplayFormat="##-####" Width="63px" />
                    <px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" Width="54px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="DocDesc" Width="180px" AllowUpdate="False" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TermsID" DisplayFormat="&gt;aaaaaaaaaa" Label="Terms" />
                    <%--<px:PXGridColumn AllowNull="False" DataField="SOInvoice__CCCapturedAmt" TextAlign="Right" Width="81px" AllowUpdate="False" />--%>
                    <%--<px:PXGridColumn AllowNull="False" DataField="SOInvoice__PaymentTotal" TextAlign="Right" Width="81px" AllowUpdate="False" />--%>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar DefaultAction="viewDocument"/>
    </px:PXGrid>
</asp:Content>
