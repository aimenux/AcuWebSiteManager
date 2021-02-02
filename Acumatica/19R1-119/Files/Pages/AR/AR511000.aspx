<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR511000.aspx.cs" Inherits="Page_AR511000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARChargeInvoices" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewDocument" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edPayDate">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edPayDate" runat="server" DataField="PayDate" Size="S" />
            <px:PXSelector CommitChanges="True" ID="edPayFinPeriodID" runat="server" DataField="PayFinPeriodID" Size="S" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_PayBillsFilter_CurrencyInfo_" DataMember="_Currency_"></pxa:PXCurrencyRate>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXCheckBox AlignLeft="true" Width="190px" CommitChanges="True" SuppressLabel="True" ID="chkOverDueFor" runat="server" DataField="ShowOverDueFor" />
            <px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edOverDueFor" runat="server" DataField="OverDueFor" SuppressLabel="True" />
            <px:PXLabel Size="s" ID="lblOverDueFor" runat="server" Height="18px">Days or more</px:PXLabel>
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXCheckBox AlignLeft="true" Width="190px" CommitChanges="True" SuppressLabel="True" ID="chkOverDueIn" runat="server" DataField="ShowDueInLessThan" />
            <px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edOverDueIn" runat="server" DataField="DueInLessThan" SuppressLabel="True" />
            <px:PXLabel Size="xs" ID="lblOverDueIn" runat="server" Height="18px">Days</px:PXLabel>
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXCheckBox AlignLeft="true" Width="190px" CommitChanges="True" SuppressLabel="True" ID="chkDiscountExparedWithinLast" runat="server" DataField="ShowDiscountExparedWithinLast" />
            <px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edDiscountExparedWithinLast" runat="server" DataField="DiscountExparedWithinLast" SuppressLabel="True" />
            <px:PXLabel Size="xs" ID="lblDiscountExparedWithinLast" runat="server" Height="18px">Days</px:PXLabel>
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXCheckBox AlignLeft="true" Width="190px" CommitChanges="True" SuppressLabel="True" ID="chkDiscountExpiresInLessThan" runat="server" DataField="ShowDiscountExpiresInLessThan" />
            <px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edDiscountExpiresInLessThan" runat="server" DataField="DiscountExpiresInLessThan" SuppressLabel="True" />
            <px:PXLabel Size="xs" ID="lblDiscountExpiresInLessThan" runat="server" Height="18px">Days</px:PXLabel>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" Caption="Payment Details" AllowPaging="true"
        AdjustPageSize="Auto" SkinID="PrimaryInquire" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="ARDocumentList">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
                    <px:PXLayoutRule runat="server" Merge="False" />
                    <px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False"
                        AllowUpdate="False" AutoCallBack="True" />
                    <px:PXGridColumn DataField="DocType" Type="DropDownList" Width="81px" />
                    <px:PXGridColumn DataField="RefNbr" Width="108px" />
                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="140px" />
                    <px:PXGridColumn AllowNull="False" DataField="DueDate" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="DiscDate" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="InvoiceNbr" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="CuryID" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" Width="90px" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="CuryDocBal" Width="90px" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="CashAccount__CashAccountCD" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="CashAccount__CuryID" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="CustomerPaymentMethod__PaymentMethodID" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn AllowNull="False" DataField="CustomerPaymentMethod__Descr" Width="150px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="TermsID" />
                    <px:PXGridColumn DataField="Customer__StatementCycleID" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode InitNewRow="True" AllowDelete="True" />
    </px:PXGrid>
</asp:Content>
