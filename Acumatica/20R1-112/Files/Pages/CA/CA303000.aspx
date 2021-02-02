<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA303000.aspx.cs" Inherits="Page_CA303000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.CA.CATranEnq">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="clearence" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="release" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDoc" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewBatch" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewRecon" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="DoubleClick" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="AddDet" Visible="false" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="AddFilterPanel" Key="AddFilter" runat="server" Caption="Quick Transaction" CaptionVisible="True" AutoCallBack-Target="AddFilter" AutoCallBack-Command="Refresh"
        Overflow="Hidden">
        <px:PXFormView ID="AddFilter" runat="server" DataMember="AddFilter" Caption="Quick Transaction" SkinID="Transparent" MarkRequired="Dynamic">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
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
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
                <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                <px:PXLayoutRule ID="PXLayoutRule3" runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
                <px:PXDropDown ID="edDrCr" runat="server" CommitChanges="True" DataField="DrCr" Enabled="False" />
                <px:PXTextEdit CommitChanges="True" ID="PXTextEdit1" runat="server" DataField="OrigModule" Enabled="False" />
                <px:PXDropDown Size="s" ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
                <px:PXCheckBox ID="chkCleared" runat="server" DataField="Cleared" />
                <px:PXSegmentMask ID="edAccountID" runat="server" CommitChanges="True" DataField="AccountID" AutoRefresh="True" />
                <px:PXSegmentMask ID="edSubID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="SubID" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="OK" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="Cancel" runat="server" DialogResult="No" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Selection" DataMember="Filter">
        <ContentLayout Orientation="Horizontal" />
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="AccountID" DataSourceID="ds" Size="L" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowSummary" runat="server" DataField="ShowSummary" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXCheckBox CommitChanges="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" />
            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
            <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" DataSourceID="ds" />
            <px:PXLayoutRule ID="PXLayoutRule9" runat="server" ControlSize="S" LabelsWidth="SM" StartColumn="True" GroupCaption="All Transactions" StartGroup="True" />
            <px:PXNumberEdit ID="edBegBal" runat="server" DataField="BegBal" Enabled="False" />
            <px:PXNumberEdit ID="edDebitTotal" runat="server" DataField="DebitTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCreditTotal" runat="server" DataField="CreditTotal" Enabled="False" />
            <px:PXNumberEdit ID="edEndBal" runat="server" DataField="EndBal" Enabled="False" />
            <px:PXLayoutRule ID="PXLayoutRule10" runat="server" ControlSize="S" StartColumn="True" LabelsWidth="S" SuppressLabel="True" GroupCaption="Cleared Only" StartGroup="True" />
            <px:PXNumberEdit ID="edBegClearedBal" runat="server" DataField="BegClearedBal" Enabled="False" />
            <px:PXNumberEdit ID="edDebitClearedTotal" runat="server" DataField="DebitClearedTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCreditClearedTotal" runat="server" DataField="CreditClearedTotal" Enabled="False" />
            <px:PXNumberEdit ID="edEndClearedBal" runat="server" DataField="EndClearedBal" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" Caption="Cash Transactions" AllowPaging="true" AdjustPageSize="Auto" SkinID="Details">
        <CallbackCommands>
            <Refresh RepaintControlsIDs="form" />
        </CallbackCommands>
        <ActionBar>
            <Actions>
                <AddNew ToolBarVisible="False" />
                <EditRecord ToolBarVisible="False" />
            </Actions>
        </ActionBar>
        <Levels>
            <px:PXGridLevel DataMember="CATranListRecords">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" />
                    <px:PXTextEdit ID="edFinPeriod" runat="server" DataField="FinPeriodID" />
                    <px:PXTextEdit ID="edOrigModule" runat="server" DataField="OrigModule" />
                    <px:PXDropDown ID="edOrigTranType" runat="server" DataField="OrigTranType" />
                    <px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" />
                    <px:PXTextEdit ID="edBatchNbr" runat="server" DataField="BatchNbr" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                    <px:PXSelector ID="edReferenceID" runat="server" DataField="ReferenceID" />
                    <px:PXTextEdit ID="edReferenceName" runat="server" DataField="ReferenceID_Description" />
                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                    <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
                    <px:PXNumberEdit ID="edCuryDebitAmt" runat="server" DataField="CuryDebitAmt" />
                    <px:PXNumberEdit ID="edCuryClearedCreditAmt" runat="server" DataField="CuryClearedCreditAmt" />
                    <px:PXNumberEdit ID="edEndBal" runat="server" DataField="EndBal" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" RenderEditorText="True" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                    <px:PXGridColumn DataField="TranDate" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="DayDesc" />
                    <px:PXGridColumn DataField="OrigModule" />
                    <px:PXGridColumn DataField="OrigRefNbr" LinkCommand="ViewDoc"/>
                    <px:PXGridColumn DataField="ExtRefNbr" />
                    <px:PXGridColumn DataField="OrigTranType" Type="DropDownList" MatrixMode="true" />
                    <px:PXGridColumn DataField="BatchNbr" LinkCommand="ViewBatch" />
                    <px:PXGridColumn DataField="Status" Type="DropDownList" />
                    <px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="EndBal" TextAlign="Right" />
                    <px:PXGridColumn DataField="Cleared" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ClearDate" />
                    <px:PXGridColumn DataField="Reconciled" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="DepositNbr" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranDesc" />
                    <px:PXGridColumn DataField="ReferenceID" />
                    <px:PXGridColumn DataField="BAccountR__AcctName" />
                    <px:PXGridColumn DataField="ReconNbr" LinkCommand="ViewRecon" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar DefaultAction="cmdDoubleClick">
            <CustomItems>
                <px:PXToolBarButton Text="Create Transaction" Key="cmdAddDet">
                    <AutoCallBack Command="AddDet" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="View Document" Key="cmdViewDocument">
                    <AutoCallBack Command="ViewDoc" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Double Click" Visible="False" Key="cmdDoubleClick">
                    <AutoCallBack Command="DoubleClick" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Mode AllowFormEdit="True" />
    </px:PXGrid>
</asp:Content>
