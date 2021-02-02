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
            <px:PXDSCallbackCommand Visible="false" CommitChanges="false" Name="AutoApply" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="false" Name="AdjustDocAmt" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="LoadOrders" />
            <px:PXDSCallbackCommand Visible="false" Name="ReverseApplication" CommitChanges="True" DependOnGrid="detgrid2" />
            <px:PXDSCallbackCommand Name="ViewDocumentToApply" DependOnGrid="detgrid" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewSODocumentToApply" DependOnGrid="detgrid3" />
            <px:PXDSCallbackCommand Name="ViewFSDocumentToApply" DependOnGrid="detgridFS" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewFSAppointmentSource" DependOnGrid="detgridFS" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewExternalTransaction" DependOnGrid="grdCCProcTran" Visible="False" />
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
            <px:PXDSCallbackCommand Name="ComplianceDocument$PurchaseOrder$Link" Visible="false" DependOnGrid="grid" CommitChanges="True" />
	        <px:PXDSCallbackCommand CommitChanges="True" Name="ComplianceDocument$Subcontract$Link" Visible="false" DependOnGrid="grid" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
	        <px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" CommitChanges="True" />
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
    <style type="text/css">
		button[id$=btnAdjustDocAmt]{ min-width: 25px; width:25px; background-color:transparent; border:0; margin-left:-10px;}
	</style>
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
            <px:PXTextEdit ID="edCCPaymentStateDescr" runat="server" DataField="CCPaymentStateDescr" Enabled="False" />
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server"
                DataField="CashAccountID" AutoRefresh="True" DataSourceID="ds" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server"
                RateTypeView="_ARPayment_CurrencyInfo_" DataMember="_Currency_"
                DataSourceID="ds"></pxa:PXCurrencyRate>
            <px:PXDateTimeEdit ID="edDepositAfter" runat="server" DataField="DepositAfter" CommitChanges="True" />
            <px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXLayoutRule runat="server" Merge="True" />
            <px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" CommitChanges="True"/>
            <px:PXButton ID="btnAdjustDocAmt" CommandName="AdjustDocAmt"  CommandSourceID="ds" runat="server"
                    DisplayStyle="Image"  ImageSet="main" ImageKey="Refresh" Tooltip="Set Payment Amount to Applied to Documents amount" Width="30px"                
                />
			<px:PXLayoutRule runat="server" />
            <px:PXNumberEdit ID="edCuryApplAmt" runat="server" DataField="CuryApplAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCurySOApplAmt" runat="server" DataField="CurySOApplAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCuryUnappliedBal" runat="server" DataField="CuryUnappliedBal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryInitDocBal" runat="server" DataField="CuryInitDocBal" CommitChanges="True" />
            <px:PXNumberEdit ID="edCuryWOAmt" runat="server" DataField="CuryWOAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCuryChargeAmt" runat="server" DataField="CuryChargeAmt" Enabled="False" />
            <px:PXNumberEdit ID="edCuryConsolidateChargeTotal" runat="server" DataField="CuryConsolidateChargeTotal" Enabled="False" />
            <px:PXCheckBox SuppressLabel="True" ID="chkIsCCPayment" runat="server" DataField="IsCCPayment" />
            <px:PXSelector ID="edRefTranExtNbr" runat="server" DataField="RefTranExtNbr"
                ValueField="TranNumber" DataSourceID="ds">
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
                        PageSize="100" AllowPaging="True" >
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
									<px:PXSelector ID="edAdjdLineNbr" runat="server" AutoRefresh="True" CommitChanges="True" DataField="AdjdLineNbr" >
										<Parameters>
											<px:PXControlParam ControlID="detgrid" Name="ARAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" Type="String" />
											<px:PXControlParam ControlID="detgrid" Name="ARAdjust.adjdRefNbr" PropertyName="DataValues[&quot;AdjdRefNbr&quot;]" Type="String" DefaultValue=" " />
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
									<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="True" AllowFilter="True" AllowMove="False" AutoCallBack="True" />
									<px:PXGridColumn DataField="AdjdBranchID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjdDocType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjdRefNbr" AutoCallBack="True" LinkCommand="ViewDocumentToApply" />
									<px:PXGridColumn AutoCallBack="True" DataField="AdjdLineNbr" />
									
									<px:PXGridColumn DataField="ARTran__InventoryID" />
									<px:PXGridColumn DataField="ARTran__ProjectID" />
									<px:PXGridColumn DataField="ARTran__TaskID" />
									<px:PXGridColumn DataField="ARTran__CostCodeID" />
									<px:PXGridColumn DataField="ARTran__AccountID" />

                                    <px:PXGridColumn DataField="AdjdCustomerID" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgAmt" AutoCallBack="True" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgPPDAmt" AutoCallBack="True" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgWOAmt" AutoCallBack="True" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="WriteOffReasonCode" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdDocDate" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARRegisterAlias__DueDate" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__DiscDate" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AdjdCuryRate" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDiscBal" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARRegisterAlias__DocDesc" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdCuryID" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdFinPeriodID" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__InvoiceNbr" />
                                    <px:PXGridColumn DataField="HasExpiredComplianceDocuments" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Load Documents">
                                    <AutoCallBack Command="LoadInvoices" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Auto Apply">
                                    <AutoCallBack Command="AutoApply" Target="ds">
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
									<px:PXGridColumn DataField="AdjdBranchID" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjBatchNbr" AllowUpdate="False" LinkCommand="ViewCurrentBatch" />
                                    <px:PXGridColumn DataField="DisplayDocType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DisplayRefNbr" LinkCommand="ViewApplicationDocument" />
									<px:PXGridColumn DataField="AdjdLineNbr" />

									<px:PXGridColumn DataField="ARTran__InventoryID" />
									<px:PXGridColumn DataField="ARTran__ProjectID" />
									<px:PXGridColumn DataField="ARTran__TaskID" />
									<px:PXGridColumn DataField="ARTran__CostCodeID" />
									<px:PXGridColumn DataField="ARTran__AccountID" />

                                    <px:PXGridColumn DataField="DisplayCustomerID" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="DisplayCuryAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="DisplayCuryPPDAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="DisplayCuryWOAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjgFinPeriodID" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="DisplayDocDate" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__DueDate" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__DiscDate" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDiscBal" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="DisplayDocDesc" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="DisplayCuryID" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdFinPeriodID" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARInvoice__InvoiceNbr" />
                                    <px:PXGridColumn DataField="PendingPPD" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="PPDCrMemoRefNbr" LinkCommand="ViewPPDCrMemo" />
                                    <px:PXGridColumn DataField="TaxInvoiceNbr" />
                                    <px:PXGridColumn DataField="HasExpiredComplianceDocuments" Type="CheckBox" />
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
                                    <px:PXGridColumn DataField="SOOrder__Status" Label="Status" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgAmt" Label="Applied To Order" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAdjgBilledAmt" Label="Transferred to Invoice" TextAlign="Right" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdOrderDate" Label="Date" />
                                    <px:PXGridColumn DataField="SOOrder__DueDate" Label="Due Date" />
                                    <px:PXGridColumn DataField="SOOrder__DiscDate" Label="Cash Discount Date" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" Label="Balance" TextAlign="Right" />
                                    <px:PXGridColumn DataField="SOOrder__OrderDesc" Label="Sales Order-Description" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="SOOrder__CuryOrderTotal" Label="Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="SOOrder__CuryID" Label="Currency" />
                                    <px:PXGridColumn DataField="SOOrder__InvoiceNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="Invoice Nbr." />
                                    <px:PXGridColumn DataField="SOOrder__InvoiceDate" Label="Invoice Date" />
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
                                    <px:PXGridColumn DataField="AdjdOrderType" Label="Service Order Type"/>
                                    <px:PXGridColumn DataField="AdjdOrderNbr" Label="Service Order Nbr." LinkCommand="ViewFSDocumentToApply"/>
                                    <px:PXGridColumn DataField="FSServiceOrder__Status" Label="Status"/>
                                    <px:PXGridColumn DataField="AdjdAppRefNbr" Label="Source Appointment Nbr." LinkCommand="ViewFSAppointmentSource"/>
                                    <px:PXGridColumn AllowUpdate="False" DataField="AdjdOrderDate" Label="Date" />
                                    <px:PXGridColumn DataField="FSServiceOrder__DocDesc" Label="Description" />
                                    <px:PXGridColumn DataField="FSServiceOrder__CuryDocTotal" Label="Service Order Total"/>
                                    <px:PXGridColumn DataField="SOCuryCompletedBillableTotal" Label="Service Order Billable Total"/>
                                    <px:PXGridColumn DataField="FSServiceOrder__CuryID" Label="Currency"/>
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
                            <px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID" AutoGenerateColumns="true" AutoRefresh="true" />
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
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                    <px:PXGridColumn DataField="ApproveDate" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false"/>
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
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
                                    <px:PXGridColumn DataField="TranNbr" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="ProcessingCenterID" />
                                    <px:PXGridColumn DataField="TranType" RenderEditorText="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="TranStatus" RenderEditorText="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RefTranNbr" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PCTranNumber" />
                                    <px:PXGridColumn DataField="AuthNumber" />
                                    <px:PXGridColumn DataField="PCResponseReasonText" />
                                    <px:PXGridColumn DataField="StartTime" />
                                    <px:PXGridColumn AllowNull="False" DataField="ProcStatus" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="CVVVerificationStatus" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="ErrorSource" Visible="False" />
                                    <px:PXGridColumn DataField="ErrorText" Visible="False" />
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
                                    <px:PXGridColumn AutoCallBack="True" DataField="EntryTypeID" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="AccountID" CommitChanges="true" />
                                    <px:PXGridColumn DataField="SubID" />
                                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" AutoCallBack="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" ID="grdComplianceDocuments" AutoGenerateColumns="Append" SyncPosition="True" KeepPosition="True" Height="300px" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" AllowPaging="True" PageSize="12">
                        <Levels>
                            <px:PXGridLevel DataMember="ComplianceDocuments">
                                <Columns>
                                    <px:PXGridColumn DataField="ExpirationDate" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CreationDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Required" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Received" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="ReceivedDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="IsProcessed" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsVoided" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsCreatedAutomatically" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="SentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True" LinkCommand="ComplianceDocuments_Project_ViewDetails" />
                                    <px:PXGridColumn DataField="CostTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RevenueTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CostCodeID" TextAlign="Left" LinkCommand="ComplianceDocuments_CostCode_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="VendorID" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="VendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="BillID" LinkCommand="ComplianceDocument$BillID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ApCheckID" LinkCommand="ComplianceDocument$ApCheckID$Link" DisplayMode="Text" CommitChanges="True" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CustomerID" LinkCommand="ComplianceDocuments_Customer_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn DataField="CustomerName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentID" LinkCommand="ComplianceDocument$ArPaymentID$Link" DisplayMode="Text" TextAlign="Left" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InvoiceID" LinkCommand="ComplianceDocument$InvoiceID$Link" CommitChanges="True" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="IsExpired" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsRequiredJointCheck" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="JointRelease" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointReleaseReceived" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="JointVendorInternalId" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="LastModifiedByID" />
                                    <px:PXGridColumn DataField="LienWaiverAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Limit" TextAlign="Right" />
                                    <px:PXGridColumn DataField="MethodSent" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentMethodID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ApPaymentMethodID" />
                                    <px:PXGridColumn DataField="Policy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ProjectTransactionID" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" CommitChanges="True" DisplayMode="Text" TextAlign="Left" />
                                    <px:PXGridColumn DataField="PurchaseOrder" TextAlign="Left" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ReceiptDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceivedBy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SecondaryVendorID" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SecondaryVendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SourceType" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SponsorOrganization" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask runat="server" DataField="CostCodeID" AutoRefresh="True" ID="edCostCode2" />
                                    <px:PXSelector runat="server" DataField="DocumentTypeValue" AutoRefresh="True" ID="edDocumentTypeValue" />
                                    <px:PXSelector runat="server" DataField="BillID" FilterByAllFields="True" AutoRefresh="True" ID="edBillID" />
                                    <px:PXSelector runat="server" DataField="InvoiceID" FilterByAllFields="True" AutoRefresh="True" ID="edInvoiceID" />
                                    <px:PXSelector runat="server" DataField="ApCheckID" FilterByAllFields="True" AutoRefresh="True" ID="edApCheckID" />
                                    <px:PXSelector runat="server" DataField="ArPaymentID" FilterByAllFields="True" AutoRefresh="True" ID="edArPaymentID" />
                                    <px:PXSelector runat="server" DataField="ProjectTransactionID" FilterByAllFields="True" AutoRefresh="True" ID="edProjectTransactionID" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrder" FilterByAllFields="True" AutoRefresh="True" ID="edPurchaseOrder" CommitChanges="True" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrderLineItem" AutoRefresh="True" ID="edPurchaseOrderLineItem" />
                                    <px:PXSelector runat="server" DataField="Subcontract" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edSubcontract" />
                                    <px:PXSelector runat="server" DataField="SubcontractLineItem" AutoRefresh="True" ID="edSubcontractLineItem" />
                                    <px:PXSelector runat="server" DataField="ChangeOrderNumber" AutoRefresh="True" ID="edChangeOrderNumber" />
                                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                                    <px:PXSelector runat="server" ID="edProjectID" DataField="ProjectID" FilterByAllFields="True" AutoRefresh="True" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" />
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
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="180px" ColumnWidth="380" ControlSize="S" />
					<px:PXBranchSelector ID="BranchSelector1" runat="server" CommitChanges="True" DataField="OrgBAccountID" InitialExpandLevel="0" TabIndex="1" Width="180px"/>
					<px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate"  TabIndex="2" />
					<px:PXDateTimeEdit ID="edTillDate" runat="server" DataField="TillDate" TabIndex="3" />
					<px:PXNumberEdit ID="edMaxDocs" runat="server" AllowNull="False" DataField="MaxDocs" TabIndex="4" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="180px" ColumnWidth="340" ControlSize="S" />
					<px:PXSelector ID="edStartRefNbr" runat="server" DataField="StartRefNbr"  TabIndex="5"   />
					<px:PXSelector ID="edEndRefNbr" runat="server" DataField="EndRefNbr" AutoRefresh="true"  TabIndex="6"  />
					<px:PXSelector ID="edStartOrderNbr" runat="server" DataField="StartOrderNbr"  TabIndex="7"  />
					<px:PXSelector ID="edEndOrderNbr" runat="server" DataField="EndOrderNbr" AutoRefresh="true" TabIndex="8" />
					<px:PXCheckBox ID="chApply" DataField="Apply" Size="SM" runat="server" Width="100%" TabIndex="9"  AlignLeft="True"></px:PXCheckBox>
					<px:PXDropDown ID="edLoadChildDocuments" runat="server" DataField="LoadChildDocuments" Size="SM" TabIndex="10"  />
                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="M" LabelsWidth="S" StartRow="True" ColumnWidth="320" />
                <px:PXGroupBox RenderStyle="Fieldset" ID="gbOrderBy" runat="server" Caption="Sort Order" DataField="OrderBy" >
                    <Template>
                        <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                        <px:PXRadioButton ID="rbDueDateRefNbr" runat="server" Text="Due Date, Reference Nbr." Value="DUE" GroupName="gbOrderBy" TabIndex="11" />
                        <px:PXRadioButton ID="rbDocDateRefNbr" runat="server" Text="Doc. Date, Reference Nbr." Value="DOC" GroupName="gbOrderBy" TabIndex="12" />
                        <px:PXRadioButton ID="rbRefNbr" runat="server" Text="Reference Nbr." Value="REF" GroupName="gbOrderBy" TabIndex="13" />
                    </Template>
                </px:PXGroupBox>
                <px:PXGroupBox RenderStyle="Fieldset" ID="gbSOOrderBy" runat="server" Caption="Order By" DataField="SOOrderBy">
                    <Template>
                        <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                        <px:PXRadioButton ID="rbOrderDateOrderNbr" runat="server" Text="Order Date, Order Nbr." Value="DAT" GroupName="gbSOOrderBy" TabIndex="14" />
                        <px:PXRadioButton ID="rbOrderNbr" runat="server" Text="Order Nbr." Value="ORD" GroupName="gbSOOrderBy"  TabIndex="15" />
                    </Template>
                </px:PXGroupBox>
                <px:PXLayoutRule runat="server" StartColumn="false" SuppressLabel="True" ControlSize="M" LabelsWidth="S" StartRow="True" />
                <px:PXPanel ID="PXButtons" runat="server" SkinID="Buttons">
                    <px:PXButton ID="btnLoad"   runat="server" DialogResult="OK" Text="Load" TabIndex="16"   />
                    <px:PXButton ID="btnReload" runat="server" DialogResult="Yes" Text="Reload" TabIndex="17" />
                    <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" TabIndex="18" />
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
	<!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
