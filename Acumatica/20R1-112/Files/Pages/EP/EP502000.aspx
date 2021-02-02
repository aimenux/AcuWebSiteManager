<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP502000.aspx.cs"
    Inherits="Page_EP502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.EP.EPCustomerBilling"/>
        
    
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edInvoiceDate" runat="server" DataField="InvoiceDate" />
            <px:PXSelector CommitChanges="True" ID="edInvFinPeriodID" runat="server" DataField="InvFinPeriodID" DataSourceID="ds" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        Caption="Customers" SkinID="PrimaryInquire" TabIndex="20900" SyncPosition="true" FastFilterFields="CustomerID,CustomerID_BAccountR_acctName">
        <Levels>
            <px:PXGridLevel DataMember="Customers">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
                    <px:PXGridColumn DataField="CustomerClassID" />
                    <px:PXGridColumn DataField="CustomerID" />
                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
                    <px:PXGridColumn DataField="LocationID" />
                    <px:PXGridColumn DataField="LocationID_Location_descr" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
