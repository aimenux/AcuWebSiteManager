<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AR508000.aspx.cs" Inherits="Page_AR508000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARPrintInvoices"
        PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" />
            <px:PXDropDown Size="m" CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector Size="m" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" Size="m" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" />           
            <px:PXCheckBox CommitChanges="True" Size="m" ID="chkShowAll" runat="server" DataField="ShowAll" />
            <px:PXDateTimeEdit CommitChanges="True" Size="m" ID="BeginDate" runat="server" DataField="BeginDate" />
            <px:PXDateTimeEdit CommitChanges="True" Size="m" ID="EndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" AlignLeft="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" AlignLeft="true" />
            <px:PXSelector CommitChanges="True" ID="edPrinterName" runat="server" DataField="PrinterName" />
            <px:PXLayoutRule runat="server" Merge="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100" Width="100%" Caption="Documents" 
        AllowPaging="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" FastFilterFields="RefNbr, CustomerID, CustomerID_BAccountR_acctName" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="ARDocumentList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
                    <px:PXGridColumn DataField="DocType" Type="DropDownList" Width="81px" />
                    <px:PXGridColumn DataField="RefNbr" Width="108px" LinkCommand="EditDetail" />
                    <px:PXGridColumn DataField="Status" Type="DropDownList" Width="100px" />
                    <px:PXGridColumn DataField="DocDate" Width="90px" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="140px" />
                    <px:PXGridColumn AllowNull="False" DataField="DueDate" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrigDiscAmt" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" Width="80px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="InvoiceNbr" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="Printed" Width="90px"  Type="CheckBox" AllowUpdate="False"  TextAlign="Center"  />
                    <px:PXGridColumn AllowNull="False" DataField="DontPrint" Width="90px" Type="CheckBox" AllowUpdate="False"  TextAlign="Center"  />
                    <px:PXGridColumn AllowNull="False" DataField="Emailed" Width="90px" Type="CheckBox" AllowUpdate="False" TextAlign="Center"  />
                    <px:PXGridColumn AllowNull="False" DataField="DontEmail" Width="90px" Type="CheckBox" AllowUpdate="False"  TextAlign="Center"  />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
