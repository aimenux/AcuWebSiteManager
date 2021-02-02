<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR523000.aspx.cs" Inherits="Page_AR523000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARCustomerCreditHoldProcess">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" SelectedIndex="-1" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowAll" runat="server" DataField="ShowAll" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        Width="100%" Height="150px" Caption="Customers" SkinID="PrimaryInquire" FastFilterFields="CustomerId, CustomerId_description">
        <Levels>
            <px:PXGridLevel DataKeyNames="CustomerId" DataMember="Details">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AutoCallBack="True" />
                    <px:PXGridColumn DataField="CustomerId" Label="Customer" LinkCommand ="ViewDocument" />
                    <px:PXGridColumn DataField="CustomerId_description" Label="Customer Name" />
                    <px:PXGridColumn AllowNull="False" DataField="DunningLetterDate" Label="Dunning Letter Date" />
                    <px:PXGridColumn AllowNull="False" DataField="DocBal" Label="Overdue Balance" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="InvBal" Label="Inv Balance" TextAlign="Right" />
                    <px:PXGridColumn DataField="Status" Label="Status" TextAlign="Center" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
