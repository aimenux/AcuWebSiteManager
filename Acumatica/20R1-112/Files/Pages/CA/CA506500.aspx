<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA506500.aspx.cs" Inherits="Page_CA506500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="filter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.CA.PaymentReclassifyProcess" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Selection" DataMember="filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" ControlSize="XXL" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="EntryTypeID" ID="edEntryTypeID" Size="m" />
            <px:PXCheckBox CommitChanges="True" runat="server" SuppressLabel="true" DataField="ShowReclassified" ID="chkShowReclassified" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="AccountID" ID="edAccountID" AutoRefresh="True" Size="m" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="CuryID" Enabled="False" ID="edCuryID" Size="s" />
            <px:PXCheckBox CommitChanges="True" runat="server" Checked="True" DataField="IncludeUnreleased" ID="chkIncludeUnreleased" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="StartDate" ID="edStartDate" Size="s" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="EndDate" ID="edEndDate" Size="s" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" ActionsPosition="Top" Caption="Transactions" Style="z-index: 100; left: 0px; top: 0px; height: 236px;" Width="100%"
        SkinID="PrimaryInquire" FastFilterFields="ExtRefNbr, AdjRefNbr" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Adjustments">
                <RowTemplate>
                    <px:PXSelector ID="RefIDSelector" runat="server" AutoRefresh="true" DataField="ReferenceID">
                    </px:PXSelector>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true" AllowCheckAll="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CashAccountID" DisplayFormat="&gt;######" Label="Cash Account" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ExtRefNbr" Label="Document Ref." />
                    <px:PXGridColumn AllowUpdate="False" DataField="TranDate" Label="Tran. Date" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AdjRefNbr" Label="Ref. Nbr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryID" DisplayFormat="&gt;LLLLL" Label="Currency" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DrCr" Label="Disb. / Receipt" RenderEditorText="True" AllowNull="False" Visible="False" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" Label="Fin. Period" Visible="False" DisplayFormat="##-####" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ReclassCashAccountID" Label="Offset Account" Visible="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AccountID" DisplayFormat="&gt;######" Label="Offset Account" Width="92px" Visible="False">
                        <Header Text="Offset Account"></Header>
                    </px:PXGridColumn>
                    <px:PXGridColumn AllowUpdate="False" DataField="SubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" Label="Offset Subaccount" Width="108px" Visible="False">
                        <Header Text="Offset Subaccount"></Header>
                    </px:PXGridColumn>
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryTranAmt" Label="Amount" Width="81px" AllowNull="False" TextAlign="Right">
                        <Header Text="Amount"></Header>
                    </px:PXGridColumn>
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TranAmt" Label="Tran. Amount" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Cleared" Label="Cleared" TextAlign="Center" Type="CheckBox" Visible="False" />
                    <px:PXGridColumn AllowNull="False" DataField="OrigModule" Label="Module" AutoCallBack="True" RenderEditorText="True" />
                    <px:PXGridColumn AllowShowHide="Server" AllowUpdate="False" DataField="ChildOrigTranType" Type="DropDownList" Label="Tran. Type"  RenderEditorText="True" MatrixMode="true"/>
                    <px:PXGridColumn AllowShowHide="Server" AllowUpdate="False" DataField="ChildOrigRefNbr" Label="ChildOrig. Doc. Number" />
                    <px:PXGridColumn AutoCallBack="True" DataField="ReferenceID" Label="Business Account" />
                    <px:PXGridColumn DataField="ReferenceID_BAccountR_AcctName" />
                    <px:PXGridColumn AutoCallBack="True" DataField="LocationID" Label="Location ID" DisplayFormat="&gt;AAAAAA" />
                    <px:PXGridColumn DataField="PaymentMethodID" DisplayFormat="&gt;aaaaaaaaaa" Label="Payment Method ID" AutoCallBack="true" />
                    <px:PXGridColumn DataField="PMInstanceID" Label="Payment Method Instance" AutoCallBack="True" TextAlign="Right" TextField="PMInstanceID_CustomerPaymentMethod_Descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TranDesc" Label="Description" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
