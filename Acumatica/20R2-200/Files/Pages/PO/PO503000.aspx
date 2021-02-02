<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO503000.aspx.cs"
    Inherits="Page_PO503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PO.POPrintOrder" PrimaryView="Filter"
                     PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
        DefaultControlID="edAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" DataField="PrintWithDeviceHub" AlignLeft="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" DataField="DefinePrinterManually" AlignLeft="true" />
            <px:PXSelector CommitChanges="True" ID="edPrinterID" runat="server" DataField="PrinterID" />
			<px:PXTextEdit CommitChanges="true" ID="edNumberOfCopies" runat="server" DataField="NumberOfCopies" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Orders" 
        SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="OrderNbr,EmployeeID,EmployeeID_EPEmployee_acctName,OrderDesc,Vendor__AcctCD,Vendor__AcctName">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXDropDown ID="edOrderType" runat="server" DataField="OrderType" Enabled="False" />
                    <px:PXSelector ID="edOrderNbr" runat="server" AllowEdit="True" DataField="OrderNbr" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="OrderDate" />
                    <px:PXGridColumn DataField="OrderNbr" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" RenderEditorText="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="EmployeeID" DisplayFormat="CCCCCCCCCC" />
                    <px:PXGridColumn AllowUpdate="False" DataField="EPEmployee__acctName" />
                    <px:PXGridColumn DataField="OrderDesc" />
                    <px:PXGridColumn DataField="OwnerID" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                    <px:PXGridColumn AllowNull="False" DataField="CuryControlTotal" TextAlign="Right" />
                    <px:PXGridColumn DataField="Vendor__AcctCD" Label="Vendor" />
                    <px:PXGridColumn DataField="Vendor__AcctName" Label="Vendor Name" />
                    <px:PXGridColumn DataField="Vendor__VendorClassID" Label="Vendor Class" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="Details"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
