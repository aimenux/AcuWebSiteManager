<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="CT501000.aspx.cs" Inherits="Page_CT501000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CT.ContractBilling"
        PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewContract" Visible="False" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edInvoiceDate">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edInvoiceDate" runat="server" DataField="InvoiceDate" />
            <px:PXSelector CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID">
                <GridProperties FastFilterFields="Description"></GridProperties>
            </px:PXSelector>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
        Width="100%" Caption="Contracts" SkinID="PrimaryInquire" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px" AllowCheckAll="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ContractCD" Width="120px" LinkCommand="editDetail" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Description" Width="200px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerID" Width="120px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Customer__AcctName" Width="200px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ContractBillingSchedule__LastDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ContractBillingSchedule__NextDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ExpireDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TemplateID" Width="120px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
