<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA302000.aspx.cs" Inherits="Page_CA302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CAReconRecords" TypeName="PX.Objects.CA.CAReconEntry">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Visible="False" CommitChanges="True" Name="createAdjustment" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="False" CommitChanges="True" Name="ToggleReconciled" />
            <px:PXDSCallbackCommand Visible="False" CommitChanges="True" Name="ToggleCleared" />
            <px:PXDSCallbackCommand Visible="False" CommitChanges="True" Name="ReconcileProcessed" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="release" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="voided" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDoc" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">

    <px:PXSmartPanel ID="AddFilterPanel" Key="AddFilter" runat="server" Caption="Quick Transaction" CaptionVisible="True" AutoCallBack-Target="AddFilter" AutoCallBack-Command="Refresh" Overflow="Hidden">
        <px:PXFormView ID="AddFilter" runat="server" DataMember="AddFilter" Caption="Quick Transaction" SkinID="Transparent" MarkRequired="Dynamic">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
                <px:PXSelector CommitChanges="True" ID="edEntryTypeId" runat="server" DataField="EntryTypeID" AutoRefresh="True" />
                <px:PXDateTimeEdit ID="edTranDate" runat="server" CommitChanges="True" DataField="TranDate" />
                <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Size="S" CommitChanges="true" />
                <pxa:PXCurrencyRate ID="edCury" runat="server" DataField="CuryID" DataMember="_Currency_" RateTypeView="_AddTrxFilter_CurrencyInfo_" />
                <px:PXTextEdit ID="edExtRefNbr" runat="server" CommitChanges="True" DataField="ExtRefNbr" />
                <px:PXSelector CommitChanges="True" ID="edReferenceID" runat="server" DataField="ReferenceID" AutoRefresh="True" />
                <px:PXSelector CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" />
                <px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" />
                <px:PXSelector ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr" AutoRefresh="True" AutoGenerateColumns="True" />
                <px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" />
                <px:PXLayoutRule ID="PXLayoutRule7" runat="server" ColumnSpan="2" />
                <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                <px:PXLayoutRule ID="PXLayoutRule8" runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
                <px:PXDropDown ID="edDrCr" runat="server" CommitChanges="True" DataField="DrCr" Enabled="False" />
                <px:PXTextEdit CommitChanges="True" ID="PXTextEdit1" runat="server" DataField="OrigModule" Enabled="False" />
                <px:PXDropDown Size="s" ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
                <px:PXCheckBox ID="chkCleared" runat="server" DataField="Cleared" />
                <px:PXSegmentMask ID="edAccountID" runat="server" CommitChanges="True" DataField="AccountID" />
                <px:PXSegmentMask ID="edSubID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="SubID" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="OK" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="Cancel" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>


    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Reconciliation Summary" DataMember="CAReconRecords" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True"
        ActivityIndicator="True" ActivityField="NoteActivity" TabIndex="100">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXSegmentMask ID="edCashAccountID" runat="server" DataField="CashAccountID" AutoRefresh="True" DataSourceID="ds" Size="XM" />
            <px:PXSelector DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" Enabled="False" />
            <px:PXSelector ID="edReconNbr" runat="server" DataField="ReconNbr" AutoRefresh="True" ValueField="ReconNbr" DataSourceID="ds" >
                 <GridProperties>
                     <Columns>
                           <px:PXGridColumn DataField="CashAccountID" />
                           <px:PXGridColumn DataField="ReconNbr" />
                           <px:PXGridColumn DataField="ReconDate"  />
                           <px:PXGridColumn DataField="Status"  />
                           <px:PXGridColumn DataField="CashAccountID_Description" />
                     </Columns>
                 </GridProperties>
            </px:PXSelector>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" Checked="True" DataField="Hold" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
            <px:PXDateTimeEdit ID="edLastReconDate" runat="server" DataField="LastReconDate" Enabled="False" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edReconDate" runat="server" DataField="ReconDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edLoadDocumentsTill" runat="server" DataField="LoadDocumentsTill" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" ControlSize="S" LabelsWidth="SM" StartColumn="True" />
            <px:PXNumberEdit ID="edCuryBegBalance" runat="server" DataField="CuryBegBalance" Enabled="False" />
            <px:PXNumberEdit ID="edCuryReconciledDebits" runat="server" DataField="CuryReconciledDebits" Enabled="False" />
            <px:PXNumberEdit ID="edCuryReconciledCredits" runat="server" DataField="CuryReconciledCredits" Enabled="False" />
            <px:PXNumberEdit ID="edCuryReconciledBalance" runat="server" DataField="CuryReconciledBalance" Enabled="False" />
            <px:PXNumberEdit ID="edCuryBalance" runat="server" CommitChanges="True" DataField="CuryBalance" />
            <px:PXNumberEdit ID="edCuryDiffBalance" runat="server" DataField="CuryDiffBalance" Enabled="False" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" ControlSize="S" StartColumn="True" SuppressLabel="True" />
            <px:PXLabel ID="PXLabel1" runat="server">Document Count:</px:PXLabel>
            <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="CountDebit" Enabled="False" />
            <px:PXNumberEdit ID="PXNumberEdit2" runat="server" DataField="CountCredit" Enabled="False" />
            <px:PXCheckBox ID="chkSkipVoided" runat="server" DataField="SkipVoided" Enabled="False" />
            <px:PXCheckBox ID="chkShowBatches" runat="server" DataField="ShowBatchPayments" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100;" Width="100%" Caption="Reconciliation Details" SkinID="Details" TabIndex="2900" FilesIndicator="False" NoteIndicator="False">
        <Levels>
            <px:PXGridLevel DataMember="CAReconTranRecords">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXDateTimeEdit ID="edClearDate" runat="server" DataField="ClearDate" />
                    <px:PXCheckBox ID="chkCleared" runat="server" DataField="Cleared" />
                    <px:PXNumberEdit ID="edCuryEffDebitAmt" runat="server" DataField="CuryEffDebitAmt" />
                    <px:PXNumberEdit ID="edCuryEffCreditAmt" runat="server" DataField="CuryEffCreditAmt" />
                    <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
                    <px:PXTextEdit ID="edOrigModule" runat="server" DataField="OrigModule" />
                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                    <px:PXDropDown ID="edOrigTranType" runat="server" DataField="OrigTranType" />
                    <px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" />
                    <px:PXDropDown ID="edStatus1" runat="server" DataField="Status" Enabled="False" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" />
                    <px:PXSelector ID="edReferenceID" runat="server" DataField="ReferenceID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Reconciled" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
                    <px:PXGridColumn DataField="Cleared" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ClearDate" />
                    <px:PXGridColumn DataField="CuryEffDebitAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryEffCreditAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="ExtRefNbr" />
                    <px:PXGridColumn DataField="OrigModule" />
                    <px:PXGridColumn DataField="OrigTranType" Type="DropDownList" MatrixMode="true" />
                    <px:PXGridColumn DataField="OrigRefNbr" LinkCommand="ViewDoc" />
                    <px:PXGridColumn DataField="Status" AllowUpdate="False" Type="DropDownList" />
                    <px:PXGridColumn DataField="TranDate" />
                    <px:PXGridColumn DataField="ReferenceID" />
                    <px:PXGridColumn DataField="ReferenceID_description" />
                    <px:PXGridColumn DataField="TranDesc" />
                </Columns>
                <Layout FormViewHeight=""></Layout>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode InitNewRow="True" AllowFormEdit="True" />
        <ActionBar DefaultAction="ViewDoc">
            <Actions>
                <AddNew Enabled="False" />
                <Delete Enabled="False" />
            </Actions>
            <CustomItems>
                <px:PXToolBarButton Text="Toggle Reconciled" Key="cmdToggleReconciled">
                    <AutoCallBack Command="ToggleReconciled" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Toggle Cleared" Key="cmdToggleCleared">
                    <AutoCallBack Command="ToggleCleared" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="cmdReconcileProcessed">
                    <AutoCallBack Command="ReconcileProcessed" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Cash Adjustment" Key="cmdCreateAdjustment">
                    <AutoCallBack Command="CreateAdjustment" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
