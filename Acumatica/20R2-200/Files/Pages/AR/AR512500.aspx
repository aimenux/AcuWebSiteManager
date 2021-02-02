<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR512500.aspx.cs" Inherits="Page_AR512500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.AR.ARExpiredCardsProcess" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds"
        Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edBeginDate">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Expiration Period" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate" />
            <px:PXNumberEdit CommitChanges="True" ID="edExpireXDays" runat="server" AllowNull="False" DataField="ExpireXDays" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLabel runat="server"></px:PXLabel>
            <px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefaultOnly" runat="server" DataField="DefaultOnly" />
            <px:PXCheckBox CommitChanges="True" ID="chkNotificationSendOnly" runat="server" DataField="NotificationSendOnly" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" Caption="Card List" AllowSearch="True" 
        AllowPaging="True" SkinID="PrimaryInquire" FastFilterFields="BAccountID, Customer__AcctName, Customer__CustomerClassID, PaymentMethodID" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Cards">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
                    <px:PXGridColumn DataField="BAccountID" DisplayFormat="&gt;AAAAAAAAAA" LinkCommand="ViewCustomer" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Customer__AcctName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Customer__CustomerClassID" DisplayFormat="&gt;aaaaaaaaaa" />
                    <px:PXGridColumn DataField="PaymentMethodID" DisplayFormat="&gt;aaaaaaaaaa" LinkCommand="ViewPaymentMethod" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Descr" />
                    <px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ExpirationDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="LastNotificationDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Contact__EMail" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Contact__Phone1" DisplayFormat="CCCCCCCCCCCCCCCCCCCC" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Contact__Fax" DisplayFormat="CCCCCCCCCCCCCCCCCCCC" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Contact__WebSite" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
