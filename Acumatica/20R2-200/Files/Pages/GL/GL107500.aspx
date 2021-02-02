<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL107500.aspx.cs" Inherits="Page_GL107500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.GL.GLWorkBookMaint" Visible="True"
        PrimaryView="WorkBooks">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
            <px:PXTreeDataMember TreeView="PrimaryScreenTree" TreeKeys="NodeID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="WorkBooks" Caption="GL Work Books"
        AllowCollapse="False" TabIndex="100" DataSourceID="ds" Height="243px">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" LabelsWidth="M" StartColumn="True" GroupCaption="General Settings" />
            <px:PXSelector ID="edWorkBookID" runat="server" DataField="WorkBookID" AutoRefresh="true" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXDropDown ID="edModule" runat="server" DataField="Module" CommitChanges="true" />
            <px:PXDropDown ID="edDocType" runat="server" AutoRefresh="True" CommitChanges="True" DataField="DocType" />
            <px:PXSelector ID="edReversingWorkBookID" runat="server" DataField="ReversingWorkBookID" AutoRefresh="True" />
            <px:PXSelector ID="edVoucherEditScreen" runat="server" DataField="VoucherEditScreen" AutoRefresh="True" DisplayMode="Text" FilterByAllFields="true" TextField="Title" />

            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="true" />
            <px:PXCheckBox ID="edSingleOpenVoucherBatch" runat="server" DataField="SingleOpenVoucherBatch" AlignLeft="true" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="M" LabelsWidth="M" GroupCaption="Numbering Sequences" />
            <px:PXSelector ID="edVoucherBatchNumberingID" runat="server" DataField="VoucherBatchNumberingID">
            </px:PXSelector>
            <px:PXSelector ID="edVoucherNumberingID" runat="server" DataField="VoucherNumberingID" CommitChanges="true" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" ControlSize="M" LabelsWidth="M" GroupCaption="Voucher Default Values" />
            <px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="DefaultCashAccountID" />
            <px:PXSelector ID="edEntryTypeID" runat="server" DataField="DefaultEntryTypeID" CommitChanges="true" AutoRefresh="true" />
            <px:PXSelector ID="edDefCustomer" runat="server" DataField="DefaultCustomerID"
                CommitChanges="true" AutoRefresh="True" AllowEdit="true" />
            <px:PXSelector ID="edDefVendor" runat="server" DataField="DefaultVendorID"
                CommitChanges="true" AutoRefresh="True" AllowEdit="true" />
            <px:PXSegmentMask ID="edDefaultLocationID" runat="server" DataField="DefaultLocationID"
                CommitChanges="true" AutoRefresh="True" />
            <px:PXTextEdit runat="server" DataField="DefaultDescription" ID="edDefaultDescription" />
        </Template>
        <AutoSize Enabled="true" Container="Window" />
    </px:PXFormView>
</asp:Content>
