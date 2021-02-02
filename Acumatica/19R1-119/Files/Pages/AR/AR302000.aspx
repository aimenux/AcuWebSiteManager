<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR302000.aspx.cs" Inherits="Page_AR302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARPaymentEntry" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Release" PopupVisible="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewBatch" />
            <px:PXDSCallbackCommand Name="Action" CommitChanges="True" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Inquiry" RepaintControls="All" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Name="NewCustomer" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" Name="EditCustomer" />
            <px:PXDSCallbackCommand Visible="false" Name="CustomerDocuments" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="LoadInvoices" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="LoadOrders" />
            <px:PXDSCallbackCommand Visible="false" Name="ReverseApplication" CommitChanges="True" DependOnGrid="detgrid2" />
            <px:PXDSCallbackCommand Name="ViewDocumentToApply" DependOnGrid="detgrid" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewSODocumentToApply" DependOnGrid="detgrid3" />
            <px:PXDSCallbackCommand Name="ViewFSDocumentToApply" DependOnGrid="detgridFS" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewFSAppointmentSource" DependOnGrid="detgridFS" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewApplicationDocument" DependOnGrid="detgrid2" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewCurrentBatch" DependOnGrid="detgrid2" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewVoucherBatch" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewWorkBook" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument" />
            <px:PXDSCallbackCommand Name="Action@AuthorizeCCPayment" CommitChanges="true" PopupCommand="SyncPaymentTransaction" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Name="Action@CaptureCCPayment" CommitChanges="true" PopupCommand="SyncPaymentTransaction" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Name="CaptureCCPayment" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AuthorizeCCPayment" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="SyncPaymentTransaction" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="VoidCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="CreditCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="RecordCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="CaptureOnlyCCPayment" />
			<px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="ValidateCCPayment" />
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXSmartPanel ID="pnlRecordCCPayment" runat="server" Caption="Record CC Payment" CaptionVisible="True" Key="Document" LoadOnDemand="True" CommandSourceID="ds" ShowAfterLoad="True"
        Style="z-index: 108;" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmCCPaymentInfo" CloseAfterAction="true">
        <px:PXFormView ID="frmCCPaymentInfo" runat="server" Caption="CC Payment Data" DataMember="ccPaymentInfo" Style="z-index: 100; border: none" CaptionVisible="False" SkinID="Transparent" DefaultControlID="edPCTranNumber">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit ID="edPCTranNumber" runat="server" DataField="PCTranNumber" />
                <px:PXTextEdit ID="edAuthNumber" runat="server" DataField="AuthNumber" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="OK" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="Cancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>

    <px:PXSmartPanel ID="pnlCaptureCCOnly" runat="server" Caption="CC Payment with External Authorization" CaptionVisible="True" Key="ccPaymentInfo"
        LoadOnDemand="True" CommandSourceID="ds" ShowAfterLoad="True"
        Style="z-index: 108;" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmCCPaymentInfo1" CloseAfterAction="true">
        <px:PXFormView ID="frmCCPaymentInfo1" runat="server" Caption="CC Payment Data" DataMember="ccPaymentInfo" Style="z-index: 100; border: none" CaptionVisible="false" SkinID="Transparent" DefaultControlID="edAuthNumber">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit ID="edAuthNumber" runat="server" DataField="AuthNumber" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%"
        DataMember="Document" Caption="Payment Summary" NoteIndicator="True"
        FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edDocType"
        TabIndex="100" DataSourceID="ds" MarkRequired="Dynamic">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" SelectedIndex="-1">
            </px:PXDropDown>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr"
                AutoRefresh="True" DataSourceID="ds">
                <GridProperties FastFilterFields="ARPayment__ExtRefNbr, CustomerID, CustomerID_Customer_acctName" />
            </px:PXSelector>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXCheckBox Size="s" CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edAdjDate" runat="server" DataField="AdjDate" />
            <px:PXSelector CommitChanges="True" ID="edAdjFinPeriodID" runat="server"
                DataField="AdjFinPeriodID" DataSourceID="ds" AutoRefresh="True"/>
            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server"
                DataField="CustomerID" AllowAddNew="True" AllowEdit="True" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server"
                AutoRefresh="True" DataField="CustomerLocationID" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server"
                DataField="PaymentMethodID" AutoRefresh="True" AllowAddNew="True"
                DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" ID="edPMInstanceID" runat="server"
                DataField="PMInstanceID" TextField="Descr" AutoRefresh="True"
                AutoGenerateColumns="True" DataSourceID="ds" />
            <px:PXSelector ID="edProcessingCenterID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="ProcessingCenterID">
			</px:PXSelector>
			<px:PXCheckBox ID="chkNewCard" runat="server" CommitChanges="true" DataField="NewCard"></px:PXCheckBox>
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server"
                DataField="CashAccountID" AutoRefresh="True" DataSourceID="ds" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server"
                RateTypeView="_ARPayment_CurrencyInfo_" DataMember="_Currency_"
                DataSourceID="ds"></pxa:PXCurrencyRate>
            <px:PXDateTimeEdit ID="edDepositAfter" runat="server" DataField="DepositAfter" CommitChanges="True" />
			<px:PXSelector CommitChanges="True" ID="edServiceContractID" runat="server" AutoRefresh="True"
                DataField="ServiceContractID" DataSourceID="ds" />
            <px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
            <px:PXTextEdit ID="edCCPaymentStateDescr" runat="server" DataField="CCPaymentStateDescr" Enabled="False" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
            <px:PXNumberEdit ID="edCuryApplAmt" runat="server" DataField="CuryApplAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCurySOApplAmt" runat="server" DataField="CurySOApplAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCuryUnappliedBal" runat="server" DataField="CuryUnappliedBal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryInitDocBal" runat="server" DataField="CuryInitDocBal" CommitChanges="True" />
            <px:PXNumberEdit ID="edCuryWOAmt" runat="server" DataField="CuryWOAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCuryChargeAmt" runat="server" DataField="CuryChargeAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCuryConsolidateChargeTotal" runat="server" DataField="CuryConsolidateChargeTotal" Enabled="False" />
            <px:PXCheckBox SuppressLabel="True" ID="chkIsCCPayment" runat="server" DataField="IsCCPayment" />
            <px:PXSelector ID="edRefTranExtNbr" runat="server" DataField="RefTranExtNbr"
                ValueField="PCTranNumber" DataSourceID="ds">
                <Parameters>
                    <px:PXControlParam ControlID="form" Name="ARPayment.pMInstanceID" PropertyName="DataControls[&quot;edPMInstanceID&quot;].Value" />
                </Parameters>
            </px:PXSelector>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" ControlSize="M" />
            <px:PXCheckBox runat="server" DataField="IsRUTROTPayment" CommitChanges="True" Size="m" ID="chkIsRUTROT" AlignLeft="True" />
        </Template>
    </px:PXFormView>
    	<style type="text/css">
		.leftDocTemplateCol
		{
			width: 50%; float:left; min-width: 90px;
		}
		.rightDocTemplateCol
		{
			margin-left: 51%; min-width: 90px;
		}
	</style>
	<px:PXGrid ID="docsTemplate" runat="server" Visible="false">
		<Levels>
			<px:PXGridLevel>
				<Columns>
					<px:PXGridColumn Key="Template">
						<CellTemplate>
							<div id="ParentDiv1" class="leftDocTemplateCol">
                                <div id="div11" class="Field0"><%# ((PXGridCellContainer)Container).Text("refNbr") %></div>								
								<div id="div12" class="Field1"><%# ((PXGridCellContainer)Container).Text("docDate") %></div>
							</div>
							<div id="ParentDiv2" class="rightDocTemplateCol">
								<span id="span21" class="Field1"><%# ((PXGridCellContainer)Container).Text("curyOrigDocAmt") %></span>                                
								<span id="span22" class="Field1"><%# ((PXGridCellContainer)Container).Text("curyID") %></span>
                                <div id="div21" class="Field1"><%# ((PXGridCellContainer)Container).Text("status") %></div>
							</div>
							<div id="div3" class="Field1"><%# ((PXGridCellContainer)Container).Text("customerID_Customer_acctName") %></div>
						</CellTemplate>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" TabIndex="200">
        <Items>
            <px:PXTabItem Text="Documents to Apply" RepaintOnDemand="false" >
                <Template>
                    <px:PXGrid ID="detgrid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SkinID="DetailsInTab" 
                        AdjustPageSize="Auto" AllowPaging="True" >
                        <Levels>
                            <px:PXGridLevel DataMember="Adjustments" >
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXDropDown ID="edAdjdDocType" runat="server" AllowNull="False" DataField="AdjdDocType" />
                                    <px:PXSelector CommitChanges="True" ID="edAdjdRefNbr" runat="server" DataField="AdjdRefNbr" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="form" Name="ARPayment.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
                                            <px:PXControlParam ControlID="detgrid" Name="ARAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edCuryAdjgAmt" runat="server" AllowNull="False" DataField="CuryAdjgAmt" />
                                    <px:PXNumberEdit ID="edCuryAdjgPPDAmt" runat="server" AllowNull="False" DataField="CuryAdjgPPDAmt" />
                                    <px:PXNumberEdit ID="edCuryAdjgWOAmt" runat="server" AllowNull="False" DataField="CuryAdjgWOAmt" />
                                    <px:PXDateTimeEdit ID="edAdjdDocDate" runat="server" DataField="AdjdDocDate" Enabled="False" />
                                    <px:PXDateTimeEdit ID="edARRegisterAlias__DueDate" runat="server" DataField="ARRegisterAlias__DueDate" />
                                    <px:PXDateTimeEdit ID="edARInvoice__DiscDate" runat="server" DataField="ARInvoice__DiscDate" />
                                    <px:PXLayoutRule runat="server" ColumnSpan="2" />
                                    <px:PXNumberEdit ID="edAdjdCuryRate" runat="server" CommitChanges="True" DataField="AdjdCuryRate" />
                                    <px:PXTextEdit ID="edARRegisterAlias__DocDesc1" runat="server" DataField="ARRegisterAlias__DocDesc" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXNumberEdit ID="edCuryDocBal" runat="server" AllowNull="False" DataField="CuryDocBal" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryDiscBal" runat="server" DataField="CuryDiscBal" Enabled="False" />
                                    <px:PXTextEdit ID="edAdjdCuryID" runat="server" DataField="AdjdCuryID" />
                                    <px:PXSelector ID="PXSelector1" runat="server" DataField="WriteOffReasonCode" AutoRefresh="True" />
                                    <px:PXTextEdit ID="edARInvoice__InvoiceNbr" runat="server" DataField="ARInvoice__InvoiceNbr" />
                                    <px:PXMaskEdit ID="edAdjdFinPeriodID" runat="server" DataField="AdjdFinPeriodID" />
                                </RowTemplate>
                                <Columns>
									<px:PXGridColumn DataField="AdjdBranchID" Width="81px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjdDocType" Width="117px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjdRefNbr" Width="108px" AutoCallBack="True" LinkCommand="ViewDocumentToApply" />
                                    <px:PXGridColumn DataField="AdjdCustomerID" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgAmt" AutoCallBack="True" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgPPDAmt" AutoCallBack="True" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgWOAmt" AutoCallBack="True" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="WriteOffReasonCode" Width="99px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdDocDate" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARRegisterAlias__DueDate" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__DiscDate" Width="90px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AdjdCuryRate" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDiscBal" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARRegisterAlias__DocDesc" Width="180px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdCuryID" Width="54px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdFinPeriodID" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__InvoiceNbr" Width="90px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Load Documents" Tooltip="Load Documents">
                                    <AutoCallBack Command="LoadInvoices" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Application History" RepaintOnDemand="false" >
                <Template>
                    <px:PXGrid ID="detgrid2" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SkinID="DetailsInTab" 
                        AdjustPageSize="Auto" AllowPaging="True" >
                        <Levels>
                            <px:PXGridLevel DataMember="Adjustments_History">
                                <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowFormEdit="False" />
                                <Columns>
									<px:PXGridColumn DataField="AdjdBranchID" Width="81px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjBatchNbr" Width="90px" AllowUpdate="False" LinkCommand="ViewCurrentBatch" />
                                    <px:PXGridColumn DataField="DisplayDocType" Width="117px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DisplayRefNbr" Width="108px" LinkCommand="ViewApplicationDocument" />
                                    <px:PXGridColumn DataField="DisplayCustomerID" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="DisplayCuryAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="DisplayCuryPPDAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="DisplayCuryWOAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjgFinPeriodID" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="DisplayDocDate" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__DueDate" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__DiscDate" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDiscBal" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="DisplayDocDesc" Width="180px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="DisplayCuryID" Width="54px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdFinPeriodID" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__InvoiceNbr" Width="90px" />
                                    <px:PXGridColumn DataField="PendingPPD" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="PPDCrMemoRefNbr" LinkCommand="ViewPPDCrMemo" Width="108px" />
                                    <px:PXGridColumn DataField="TaxInvoiceNbr" Width="100px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False" />
                                <EditRecord Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Text="Reverse Application" Tooltip="Reverse Application">
                                    <AutoCallBack Command="ReverseApplication" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Orders to Apply" RepaintOnDemand="false" >
                <Template>
                    <px:PXGrid ID="detgrid3" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 382px;" Width="100%"
                        BorderWidth="0px" SkinID="Details" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="SOAdjustments">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edSOAdjdOrderType" runat="server" DataField="AdjdOrderType" AutoRefresh="true" CommitChanges="true" />
                                    <px:PXSelector CommitChanges="True" ID="edSOAdjdOrderNbr" runat="server" DataField="AdjdOrderNbr" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="detgrid3" Name="SOAdjust.adjdOrderType" PropertyName="DataValues[&quot;AdjdOrderType&quot;]" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edSOSOOrder__Status" runat="server" DataField="SOOrder__Status" Enabled="False" />
                                    <px:PXNumberEdit ID="edSOCuryAdjgAmt" runat="server" DataField="CuryAdjgAmt" />
                                    <px:PXNumberEdit ID="edSOCuryAdjgBilledAmt" runat="server" DataField="CuryAdjgBilledAmt" />
                                    <px:PXDateTimeEdit ID="edSOAdjdOrderDate" runat="server" DataField="AdjdOrderDate" Enabled="False" />
                                    <px:PXDateTimeEdit ID="edSOSOOrder__DueDate" runat="server" DataField="SOOrder__DueDate" />
                                    <px:PXDateTimeEdit ID="edSOSOOrder__DiscDate" runat="server" DataField="SOOrder__DiscDate" />
                                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
                                    <px:PXTextEdit ID="edSOSOOrder__OrderDesc" runat="server" DataField="SOOrder__OrderDesc" />
                                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edSOCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
                                    <px:PXNumberEdit ID="edSOOrder__CuryOrderTotal" runat="server" DataField="SOOrder__CuryOrderTotal" Enabled="False" />
                                    <px:PXTextEdit ID="edSOOrder__CuryID" runat="server" DataField="SOOrder__CuryID" />
                                    <px:PXMaskEdit ID="edSOSOOrder__InvoiceNbr" runat="server" DataField="SOOrder__InvoiceNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
                                    <px:PXDateTimeEdit ID="edSOSOOrder__InvoiceDate" runat="server" DataField="SOOrder__InvoiceDate" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AdjdOrderType" Label="Order Type" RenderEditorText="True"  AutoCallBack="True" />
                                    <px:PXGridColumn DataField="AdjdOrderNbr" Label="Order Nbr." AutoCallBack="True" LinkCommand="ViewSODocumentToApply" />
                                    <px:PXGridColumn DataField="SOOrder__Status" Label="Status" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgAmt" Label="Applied To Order" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgBilledAmt" Label="Transferred to Invoice" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdOrderDate" Label="Date" Width="90px" />
                                    <px:PXGridColumn DataField="SOOrder__DueDate" Label="Due Date" Width="90px" />
                                    <px:PXGridColumn DataField="SOOrder__DiscDate" Label="Cash Discount Date" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" Label="Balance" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="SOOrder__OrderDesc" Label="Sales Order-Description" Width="200px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="SOOrder__CuryOrderTotal" Label="Amount" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="SOOrder__CuryID" Label="Currency" />
                                    <px:PXGridColumn DataField="SOOrder__InvoiceNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="Invoice Nbr." />
                                    <px:PXGridColumn DataField="SOOrder__InvoiceDate" Label="Invoice Date" Width="90px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar DefaultAction="ViewSODocumentToApply">
                            <CustomItems>
                                <px:PXToolBarButton Text="Load Documents" Tooltip="Load Documents">
                                    <AutoCallBack Command="LoadOrders" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Service Orders to Apply" RepaintOnDemand="false" VisibleExp="DataControls[&quot;chkServiceManagement&quot;].Value == 1" BindingContext="form">
                <Template>
                    <px:PXGrid ID="detgridFS" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 382px;" Width="100%"
                        BorderWidth="0px" SkinID="Details" AdjustPageSize="Auto" AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataMember="FSAdjustments">
                                <Columns>
                                    <px:PXGridColumn DataField="AdjdOrderType" Label="Service Order Type" Width="70px"/>
                                    <px:PXGridColumn DataField="AdjdOrderNbr" Label="Service Order Nbr." Width="140px" LinkCommand="ViewFSDocumentToApply"/>
                                    <px:PXGridColumn DataField="FSServiceOrder__Status" Label="Status" Width="90px"/>
                                    <px:PXGridColumn DataField="AdjdAppRefNbr" Label="Source Appointment Nbr." Width="130px" LinkCommand="ViewFSAppointmentSource"/>
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdOrderDate" Label="Date" Width="90px" />
                                    <px:PXGridColumn DataField="FSServiceOrder__DocDesc" Label="Description" Width="200px" />
                                    <px:PXGridColumn DataField="FSServiceOrder__CuryDocTotal" Label="Service Order Total" Width="160px"/>
                                    <px:PXGridColumn DataField="SOCuryCompletedBillableTotal" Label="Service Order Billable Total" Width="130px"/>
                                    <px:PXGridColumn DataField="FSServiceOrder__CuryID" Label="Currency" Width="90px"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXFormView ID="form2" runat="server" Style="z-index: 100" Width="100%" DataMember="CurrentDocument" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Link to GL"></px:PXLayoutRule>
                            <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
                            <px:PXNumberEdit ID="edDisplayCuryInitDocBal" runat="server" DataField="DisplayCuryInitDocBal" Enabled="False"/>
                            <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                            <px:PXSegmentMask CommitChanges="True" ID="edARAccountID" runat="server" DataField="ARAccountID" AutoGenerateColumns="true" />
                            <px:PXSegmentMask ID="edARSubID" runat="server" DataField="ARSubID" AutoRefresh="true" AutoGenerateColumns="true">
                                <Parameters>
                                    <px:PXControlParam ControlID="form2" Name="ARRegister.aRAccountID" PropertyName="DataControls[&quot;edARAccountID&quot;].Value" />
                                </Parameters>
                            </px:PXSegmentMask>
                            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoGenerateColumns="true" />
                            <px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID" AutoGenerateColumns="true" />
							<px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True">
								<LinkCommand Target="ds" Command="ViewOriginalDocument"/>
							</px:PXTextEdit>

                            <px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Payment Information" LabelsWidth="SM" ControlSize="M" StartColumn="true"></px:PXLayoutRule>
                            <px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
                            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
                            <px:PXCheckBox CommitChanges="True" ID="edCleared" runat="server" DataField="Cleared" />
                            <px:PXDateTimeEdit CommitChanges="True" Size="s" ID="edClearDate" runat="server" DataField="ClearDate" />
                            <px:PXCheckBox CommitChanges="True" ID="chkDepositAsBatch" runat="server" DataField="DepositAsBatch" />
                            <px:PXCheckBox ID="chkDeposited" runat="server" DataField="Deposited" />
                            <px:PXDateTimeEdit ID="edDepositDate" runat="server" DataField="DepositDate" Enabled="False" />
                            <px:PXTextEdit ID="edDepositNbr" runat="server" DataField="DepositNbr" />
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" GroupCaption="Voucher Details" />
                            <px:PXFormView ID="VoucherDetails" runat="server" RenderStyle="Simple"
                                DataMember="Voucher" DataSourceID="ds" TabIndex="1100">
                                <Template>
                                    <px:PXTextEdit ID="linkGLVoucherBatch" runat="server" DataField="VoucherBatchNbr" Enabled="false">
                                        <LinkCommand Target="ds" Command="ViewVoucherBatch"></LinkCommand>
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="linkGLWorkBook" runat="server" DataField="WorkBookID" Enabled="false">
                                        <LinkCommand Target="ds" Command="ViewWorkBook"></LinkCommand>
                                    </px:PXTextEdit>
                                </Template>
                            </px:PXFormView>
                        </Template>
                    </px:PXFormView>

                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details" BindingContext="form" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" Style="left: 0px; top: 0px;">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="Approval">
                                <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" Width="160px" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="WorkgroupID" Width="150px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" Width="100px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="ApproveDate" Width="90px" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" Width="160px" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false" Width="160px"/>
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" Width="160px" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" Width="160px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" Width="100px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Credit Card Processing Info" BindingContext="form" VisibleExp="DataControls[&quot;chkIsCCPayment&quot;].Value = 1" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="grdCCProcTran" runat="server" Height="120px" Width="100%" BorderWidth="0px" Style="left: 0px; top: 0px;" SkinID="DetailsInTab">
                        <ActionBar>
                            <Actions>
                                <Save Enabled="False" />
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="ccProcTran">
                                <Mode AllowAddNew="True" AllowDelete="True" AllowUpdate="True" />
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXNumberEdit ID="edTranNbr" runat="server" DataField="TranNbr" />
                                    <px:PXDropDown ID="edProcStatus" runat="server" AllowNull="False" DataField="ProcStatus" />
                                    <px:PXTextEdit ID="edProcessingCenterID" runat="server" AllowNull="False" DataField="ProcessingCenterID" />
                                    <px:PXDropDown ID="edCVVVerificationStatus" runat="server" DataField="CVVVerificationStatus" />
                                    <px:PXDropDown ID="edTranType" runat="server" DataField="TranType" />
                                    <px:PXDropDown ID="edTranStatus" runat="server" AllowNull="False" DataField="TranStatus" />
                                    <px:PXNumberEdit ID="edAmount" runat="server" AllowNull="False" DataField="Amount" />
                                    <px:PXNumberEdit ID="edRefTranNbr" runat="server" DataField="RefTranNbr" />
                                    <px:PXTextEdit ID="edPCTranNumber" runat="server" DataField="PCTranNumber" />
                                    <px:PXTextEdit ID="edAuthNumber" runat="server" DataField="AuthNumber" />
                                    <px:PXTextEdit ID="edPCResponseReasonText" runat="server" DataField="PCResponseReasonText" />
                                    <px:PXDateTimeEdit ID="edStartTime" runat="server" DataField="StartTime" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TranNbr" TextAlign="Right" Width="54px" />
                                    <px:PXGridColumn AllowNull="False" DataField="ProcessingCenterID" Width="85px" />
                                    <px:PXGridColumn DataField="TranType" RenderEditorText="True" Width="140px" />
                                    <px:PXGridColumn AllowNull="False" DataField="TranStatus" RenderEditorText="True" Width="75px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Amount" TextAlign="Right" Width="80px" />
                                    <px:PXGridColumn DataField="RefTranNbr" TextAlign="Right" Width="70px" />
                                    <px:PXGridColumn DataField="PCTranNumber" Width="90px" />
                                    <px:PXGridColumn DataField="AuthNumber" Width="75px" />
                                    <px:PXGridColumn DataField="PCResponseReasonText" Width="240px" />
                                    <px:PXGridColumn DataField="StartTime" />
                                    <px:PXGridColumn AllowNull="False" DataField="ProcStatus" RenderEditorText="True" Width="72px" />
                                    <px:PXGridColumn DataField="CVVVerificationStatus" RenderEditorText="True" Width="171px" />
                                    <px:PXGridColumn DataField="ErrorSource" Visible="False" />
                                    <px:PXGridColumn DataField="ErrorText" Visible="False" Width="200px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="50" MinWidth="50" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Finance Charges" RepaintOnDemand="false" >
                <Template>
                    <px:PXGrid ID="detgrid3" runat="server" Height="300px" SkinID="DetailsInTab" Style="z-index: 100;" TabIndex="30500" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="PaymentCharges" DataKeyNames="DocType,RefNbr,LineNbr">
                                <RowTemplate>
                                    <px:PXSelector ID="edEntryTypeID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="EntryTypeID" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" Enabled="False" AllowEdit="False" />
                                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" Enabled="False" AllowEdit="False" />
                                    <px:PXNumberEdit ID="edCuryTranAmt" runat="server" CommitChanges="true" DataField="CuryTranAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="EntryTypeID" Width="100px" />
                                    <px:PXGridColumn DataField="TranDesc" Width="160px" />
                                    <px:PXGridColumn DataField="AccountID" Width="115px" />
                                    <px:PXGridColumn DataField="SubID" Width="130px" />
                                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" AutoCallBack="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <px:PXSmartPanel ID="pnlLoadOpts" runat="server" Style="z-index: 108;" Caption="Load Options" CaptionVisible="True" Key="loadOpts" AutoReload="true" LoadOnDemand="true">
        <px:PXFormView ID="loform" runat="server" Style="z-index: 100;" DataMember="loadOpts" CaptionVisible="False" DefaultControlID="edFromDate" SkinID="Transparent">
            <ContentStyle BorderWidth="0px">
            </ContentStyle>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="180px" ColumnWidth="270" ControlSize="S" />
				<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="true" />
				<px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" />
                <px:PXSelector ID="edStartRefNbr" runat="server" DataField="StartRefNbr" />
                <px:PXSelector ID="edStartOrderNbr" runat="server" DataField="StartOrderNbr" />
                <px:PXNumberEdit ID="edMaxDocs" runat="server" AllowNull="False" DataField="MaxDocs" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="160px" ColumnWidth="322" ControlSize="S" />
				<px:PXDateTimeEdit ID="edTillDate" runat="server" DataField="TillDate" />
                <px:PXSelector ID="edEndRefNbr" runat="server" DataField="EndRefNbr" AutoRefresh="true" />
                <px:PXSelector ID="edEndOrderNbr" runat="server" DataField="EndOrderNbr" AutoRefresh="true" />
                <px:PXDropDown ID="edLoadChildDocuments" runat="server" DataField="LoadChildDocuments" Size="SM" />
                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="M" LabelsWidth="S" StartRow="True" />
                <px:PXGroupBox RenderStyle="Fieldset" ID="gbOrderBy" runat="server" Caption="Order By" DataField="OrderBy">
                    <Template>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                        <px:PXRadioButton ID="rbDueDateRefNbr" runat="server" Text="Due Date, Reference Nbr." Value="DUE" GroupName="gbOrderBy" />
                        <px:PXRadioButton ID="rbDocDateRefNbr" runat="server" Text="Doc. Date, Reference Nbr." Value="DOC" GroupName="gbOrderBy" />
                        <px:PXRadioButton ID="rbRefNbr" runat="server" Text="Reference Nbr." Value="REF" GroupName="gbOrderBy" />
                    </Template>
                </px:PXGroupBox>
                <px:PXGroupBox RenderStyle="Fieldset" ID="gbSOOrderBy" runat="server" Caption="Order By" DataField="SOOrderBy">
                    <Template>
                        <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                        <px:PXRadioButton ID="rbOrderDateOrderNbr" runat="server" Text="Order Date, Order Nbr." Value="DAT" GroupName="gbSOOrderBy" />
                        <px:PXRadioButton ID="rbOrderNbr" runat="server" Text="Order Nbr." Value="ORD" GroupName="gbSOOrderBy" />
                    </Template>
                </px:PXGroupBox>

                <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Load" />
                    <px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" />
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
	<!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
