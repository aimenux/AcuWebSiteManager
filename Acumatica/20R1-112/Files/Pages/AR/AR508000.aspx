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
            <px:PXSelector CommitChanges="True" ID="edPrinterID" runat="server" DataField="PrinterID" />
			<px:PXTextEdit CommitChanges="true" ID="edNumberOfCopies" runat="server" DataField="NumberOfCopies" />
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
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
                    <px:PXGridColumn DataField="DocType" Type="DropDownList" />
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="EditDetail" />
                    <px:PXGridColumn DataField="Status" Type="DropDownList" />
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
                    <px:PXGridColumn AllowNull="False" DataField="DueDate" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrigDocAmt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrigDiscAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="InvoiceNbr" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="Printed"  Type="CheckBox" AllowUpdate="False"  TextAlign="Center"  />
                    <px:PXGridColumn AllowNull="False" DataField="DontPrint" Type="CheckBox" AllowUpdate="False"  TextAlign="Center"  />
                    <px:PXGridColumn AllowNull="False" DataField="Emailed" Type="CheckBox" AllowUpdate="False" TextAlign="Center"  />
                    <px:PXGridColumn AllowNull="False" DataField="DontEmail" Type="CheckBox" AllowUpdate="False"  TextAlign="Center"  />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
