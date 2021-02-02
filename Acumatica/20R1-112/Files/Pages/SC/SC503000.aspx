<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="SC503000.aspx.cs" Inherits="Page_SC503000" Title="Print/Email Subcontracts" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
                     TypeName="PX.Objects.CN.Subcontracts.SC.Graphs.PrintSubcontract"
                     PrimaryView="Filter"
                     PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewSubcontractDetails" Visible="False" CommitChanges="True" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
                   DefaultControlID="edAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM"></px:PXLayoutRule>
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action"></px:PXDropDown>
            <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
            <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID"></px:PXSelector>
            <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner"></px:PXCheckBox>
            <px:PXLayoutRule runat="server" Merge="False"></px:PXLayoutRule>
            <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID"></px:PXSelector>
            <px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup"></px:PXCheckBox>
            <px:PXLayoutRule runat="server" Merge="False"></px:PXLayoutRule>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Orders"
               SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="OrderNbr,EmployeeID,EmployeeID_EPEmployee_acctName,OrderDesc,Vendor__AcctCD,Vendor__AcctName">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"></px:PXLayoutRule>
                    <px:PXDropDown ID="edOrderType" runat="server" DataField="OrderType" Enabled="False"></px:PXDropDown>
                    <px:PXSelector ID="edOrderNbr" runat="server" AllowEdit="True" DataField="OrderNbr" Enabled="False"></px:PXSelector>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="OrderDate" CommitChanges="True"/>
                    <px:PXGridColumn DataField="OrderNbr" LinkCommand="ViewSubcontractDetails"/>
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" RenderEditorText="True"/>
                    <px:PXGridColumn AllowUpdate="False" DataField="EmployeeID" DisplayFormat="CCCCCCCCCC"></px:PXGridColumn>
                    <px:PXGridColumn AllowUpdate="False" DataField="EPEmployee__acctName"></px:PXGridColumn>
                    <px:PXGridColumn DataField="OrderDesc"/>
                    <px:PXGridColumn DataField="OwnerID" DisplayMode="Text"></px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL"></px:PXGridColumn>
                    <px:PXGridColumn AllowNull="False" DataField="CuryControlTotal" TextAlign="Right"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Vendor__AcctCD" Label="Vendor"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Vendor__AcctName" Label="Vendor Name"></px:PXGridColumn>
                    <px:PXGridColumn DataField="Vendor__VendorClassID" Label="Vendor Class"></px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="Details"></ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
    </px:PXGrid>
</asp:Content>
