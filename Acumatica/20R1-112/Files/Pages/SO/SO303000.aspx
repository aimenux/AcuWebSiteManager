<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO303000.aspx.cs"
	Inherits="Page_SO303000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOInvoiceEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Visible="false" Name="Release" />
			<px:PXDSCallbackCommand Visible="False" Name="ReverseInvoice" />
			<px:PXDSCallbackCommand Visible="False" Name="PayInvoice" />
			<px:PXDSCallbackCommand Visible="False" Name="CreateSchedule" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewSchedule" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
			<px:PXDSCallbackCommand Visible="False" Name="NewCustomer" />
			<px:PXDSCallbackCommand Visible="False" Name="SendARInvoiceMemo" />
			<px:PXDSCallbackCommand Visible="False" Name="EditCustomer" />
			<px:PXDSCallbackCommand Visible="False" Name="CustomerDocuments" />
			<px:PXDSCallbackCommand Visible="False" Name="SOInvoice" />
            <px:PXDSCallbackCommand Visible="false" Name="CustomerRefund" />
			<px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="SelectShipment" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="AddShipment" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="AddShipmentCancel" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Action" Visible="True" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Report" Visible="True" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" Visible="True" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Hold" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="CreditHold" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Flow" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AutoApply" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="LoadDocuments" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewPayment" DependOnGrid="detgrid" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewInvoice" DependOnGrid="detgrid2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewInvoice2" DependOnGrid="detgrid3" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewItem" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="False" CommitChanges="true" Name="CaptureCCPayment" />
			<px:PXDSCallbackCommand Visible="False" CommitChanges="true" Name="AuthorizeCCPayment" />
			<px:PXDSCallbackCommand Visible="False" CommitChanges="true" Name="VoidCCPayment" />
			<px:PXDSCallbackCommand Visible="False" CommitChanges="true" Name="CreditCCPayment" />
			<px:PXDSCallbackCommand Visible="False" CommitChanges="true" Name="ValidateCCPayment" />
            <px:PXDSCallbackCommand Visible="False" CommitChanges="true" Name="ViewExternalTransaction" DependOnGrid="grdCCProcTran" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
			<px:PXDSCallbackCommand Name="WriteOff" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="false" Name="ReclassifyBatch" />
            <px:PXDSCallbackCommand Name="LSARTran_generateLotSerial" PopupCommand="" PopupCommandTarget="" Visible="False" />
            <px:PXDSCallbackCommand Name="LSARTran_binLotSerial" PopupCommand="" PopupCommandTarget="" Visible="False" />
            <px:PXDSCallbackCommand Name="SelectSOLine" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddSOLine" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="SelectARTran" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddARTran" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$PurchaseOrder$Link" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$Subcontract$Link" Visible="false" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$InvoiceID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$BillID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ApCheckID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ArPaymentID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" />
			<px:PXDSCallbackCommand Name="ComplianceDocument$ProjectTransactionID$Link" Visible="false" DependOnGrid="grdComplianceDocuments" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewCorrectionDocument" />
			<px:PXDSCallbackCommand Visible="False" Name="CancelInvoice" />
			<px:PXDSCallbackCommand Visible="False" Name="CorrectInvoice" />
			<px:PXDSCallbackCommand Name="PutOnCreditHold" Visible="false" CommitChanges="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Invoice Summary"
		NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects"
		ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edDocType" TabIndex="100">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" SelectedIndex="-1" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
                <GridProperties FastFilterFields="ARInvoice__InvoiceNbr, CustomerID, CustomerID_Customer_AcctName" />
            </px:PXSelector>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox ID="chkHold" runat="server" DataField="Hold">
				<AutoCallBack Command="Hold" Target="ds">
					<Behavior CommitChanges="True" />
				</AutoCallBack>
			</px:PXCheckBox>
			<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" AutoRefresh="True" />
			<px:PXTextEdit ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowAddNew="True" AllowEdit="True"
				DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" AutoRefresh="True" DataField="CustomerLocationID"
				DataSourceID="ds" />
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_ARInvoice_CurrencyInfo_"
				DataMember="_Currency_" />
			<px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" DataSourceID="ds" />
			<px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" />
			<px:PXDateTimeEdit ID="edDiscDate" runat="server" DataField="DiscDate" />
			<px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" CommitChanges="True" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
			<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot" CommitChanges="true" />
			<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryTaxTotal2" runat="server" DataField="CuryTaxTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryBalanceWOTotal" runat="server" DataField="CuryBalanceWOTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDiscAmt" runat="server" DataField="CuryOrigDiscAmt" />
			<px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRUTROT" AlignLeft="true" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="363px" Style="z-index: 100;" Width="100%" TabIndex="23" DataMember="CurrentDocument">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 385px;" Width="100%"
						BorderWidth="0px" SkinID="Details" SyncPosition="True" Height="385px" TabIndex="7700">
						<Levels>
							<px:PXGridLevel DataMember="Transactions" DataKeyNames="RefNbr,LineNbr">
								<Columns>
									<px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" RenderEditorText="True" />
									<px:PXGridColumn DataField="TranType" />
									<px:PXGridColumn DataField="RefNbr" />
									<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="LineType" />
									<px:PXGridColumn AllowUpdate="False" DataField="SOShipmentNbr">
										<NavigateParams>
											<px:PXSyncGridParam ControlID="grid" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="SOOrderType" />
									<px:PXGridColumn AllowUpdate="False" DataField="SOOrderNbr" />
									<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" RenderEditorText="True"
										LinkCommand="ViewItem" AllowDragDrop="true" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="SubItemID" DisplayFormat="#" NullText="<SPLIT>" />
									<px:PXGridColumn DataField="AppointmentDate" />
									<px:PXGridColumn DataField="SuspendedSMEquipmentID" />
									<px:PXGridColumn DataField="EquipmentAction" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Comment" />
                                    <px:PXGridColumn DataField="SMEquipmentID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="NewTargetEquipmentLineNbr" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ComponentID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="EquipmentLineRef" CommitChanges="True" />
									<px:PXGridColumn DataField="CustomerLocationID" />
									<px:PXGridColumn DataField="AppointmentID" />
									<px:PXGridColumn DataField="SOID" />
									<px:PXGridColumn DataField="TranDesc" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" CommitChanges="True" NullText="<SPLIT>" />
									<px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" CommitChanges="True" AllowDragDrop="true" />
									<px:PXGridColumn AllowNull="False" DataField="BaseQty" TextAlign="Right" />
									<px:PXGridColumn DataField="UOM" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="LotSerialNbr" CommitChanges="True" NullText="<SPLIT>" />
                                    <px:PXGridColumn DataField="ExpireDate" />
									<px:PXGridColumn AllowNull="False" DataField="CuryUnitPrice" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="ManualPrice" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryExtPrice" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn AllowNull="False" DataField="DiscPct" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="CuryDiscAmt" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="ManualDisc" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="DiscountID" RenderEditorText="True" TextAlign="Left" AutoCallBack="True" />
									<px:PXGridColumn DataField="DiscountSequenceID" TextAlign="Left" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTranAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="AccountID" DisplayFormat="&gt;#####" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID_Account_description" />
									<px:PXGridColumn DataField="SubID" DisplayFormat="&gt;AA-AA-AA-AA-AA-AA" />
									<px:PXGridColumn DataField="ExpenseAccrualAccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="ExpenseAccrualAccountID_Account_description" />
									<px:PXGridColumn DataField="ExpenseAccrualSubID" />
									<px:PXGridColumn DataField="ExpenseAccountID" AutoCallBack="True" />
									<px:PXGridColumn DataField="ExpenseAccountID_Account_description" />
									<px:PXGridColumn DataField="ExpenseSubID" />
									<px:PXGridColumn DataField="CostBasis" />
									<px:PXGridColumn DataField="CuryAccruedCost" />
									<px:PXGridColumn DataField="TaskID" DisplayFormat="&gt;AAAAAAAAAA" Label="Task" />
									<px:PXGridColumn DataField="CostCodeID" Label="Task" />
									<px:PXGridColumn DataField="SalesPersonID" DisplayFormat="&gt;AAAAAAAAAA" RenderEditorText="True" AutoCallBack="True" />
									<px:PXGridColumn DataField="DeferredCode" DisplayFormat="&gt;aaaaaaaaaa" Label="Deferral Code" CommitChanges="true" />
									<px:PXGridColumn DataField="DRTermStartDate" CommitChanges="true" />
									<px:PXGridColumn DataField="DRTermEndDate" CommitChanges="true" />
									<px:PXGridColumn DataField="DefScheduleID" Label="Deferral Schedule" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxCategoryID" />
									<px:PXGridColumn DataField="AvalaraCustomerUsageType" />
									<px:PXGridColumn AllowNull="False" DataField="Commissionable" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="SOOrderLineNbr" />
									<px:PXGridColumn DataField="IsRUTROTDeductible" Type="Checkbox" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="RUTROTItemType" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="RUTROTWorkTypeID" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryRUTROTAvailableAmt" />
                                    <px:PXGridColumn DataField="OrigInvoiceType" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="OrigInvoiceNbr" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="OrigInvoiceLineNbr" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="InvtDocType" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="InvtRefNbr" AllowUpdate="False" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
									<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" Size="XM" />
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" Enabled="False" AutoRefresh="true" />
									<px:PXSelector runat="server" ID="edSuspendedSMEquipmentID" DataField="SuspendedSMEquipmentID" AllowEdit="True" />
									<px:PXSelector runat="server" ID="edAppointmentID" DataField="AppointmentID" AllowEdit="True" />
									<px:PXSelector runat="server" ID="edSOID" DataField="SOID" AllowEdit="True" />
									<px:PXDropDown runat="server" ID="edEquipmentAction" DataField="EquipmentAction" CommitChanges="True" />
									<px:PXTextEdit runat="server" ID="edSMComment" DataField="Comment" />
									<px:PXSelector runat="server" ID="edSMEquipmentID" DataField="SMEquipmentID" CommitChanges="True" AutoRefresh="True" AllowEdit="True"/>
									<px:PXSelector runat="server" ID="edNewTargetEquipmentLineNbr" DataField="NewTargetEquipmentLineNbr" CommitChanges="True" AutoRefresh="True" />
									<px:PXSelector runat="server" ID="edSMComponentID" DataField="ComponentID" CommitChanges="True" AutoRefresh="True" />
									<px:PXSelector runat="server" ID="edEquipmentLineRef" DataField="EquipmentLineRef" CommitChanges="True" AutoRefresh="True" />

									<px:PXSelector ID="edSOShipmentNbr" runat="server" DataField="SOShipmentNbr" Enabled="False" AllowEdit="True" />
									<px:PXTextEdit ID="edSOOrderType1" runat="server" DataField="SOOrderType" Enabled="False" />
									<px:PXSelector ID="edSOOrderNbr1" runat="server" DataField="SOOrderNbr" Enabled="False" AllowEdit="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True" Size="XM">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="ARTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="ARTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" CommitChanges="True" Size="XM">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="ARTran.siteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="ARTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="ARTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
									<px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM">
										<Parameters>
											<px:PXControlParam ControlID="grid" Name="ARTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
										</Parameters>
									</px:PXSelector>
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                    <px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" AutoRefresh="True" CommitChanges="True" Size="XM">
                                        <Parameters>
                                            <px:PXControlParam ControlID="grid" Name="ARTran.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="ARTran.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
                                            <px:PXControlParam ControlID="grid" Name="ARTran.locationID" PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" DisplayFormat="d" />
									<px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" CommitChanges="true" />
									<px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
									<px:PXSelector ID="edDiscountCode" runat="server" DataField="DiscountID" AllowEdit="True" edit="1" />
									<px:PXNumberEdit ID="edDiscPct" runat="server" DataField="DiscPct" />
									<px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" />
									<px:PXCheckBox ID="chkManualDisc" runat="server" DataField="ManualDisc" CommitChanges="true" />
									<px:PXLayoutRule runat="server" ColumnSpan="2" />
									<px:PXTextEdit ID="edTranDesc1" runat="server" DataField="TranDesc" />

									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" />
									<px:PXNumberEdit ID="edCuryExtPrice" runat="server" DataField="CuryExtPrice" CommitChanges="true" />
									<px:PXNumberEdit ID="edCuryTranAmt1" runat="server" DataField="CuryTranAmt" Enabled="False" />
									<px:PXSegmentMask ID="edAccountID1" runat="server" DataField="AccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSubID1" runat="server" DataField="SubID" AutoRefresh="True">
										<Parameters>
											<px:PXControlParam ControlID="grid" Name="ARTran.accountID" PropertyName="DataValues[&quot;AccountID&quot;]" Type="String" />
										</Parameters>
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edSalesPersonID1" runat="server" DataField="SalesPersonID" />
									<px:PXSelector ID="edTaxCategoryID1" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
									<px:PXDropDown ID="edAvalaraCustomerUsageTypeID1" runat="server" DataField="AvalaraCustomerUsageType" />
									<px:PXCheckBox CommitChanges="True" ID="chkCommissionable" runat="server" Checked="True" DataField="Commissionable" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" CommitChanges="true" />
									<px:PXDateTimeEdit ID="edDRTermStartDate" runat="server" DataField="DRTermStartDate" CommitChanges="true" />
									<px:PXDateTimeEdit ID="edDRTermEndDate" runat="server" DataField="DRTermEndDate" CommitChanges="true" />
									<px:PXSelector ID="edDefScheduleID" runat="server" DataField="DefScheduleID" />
									<px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRRDeductibleTran" />
									<px:PXDropDown runat="server" DataField="RUTROTItemType" CommitChanges="True" ID="cmbRRItemType" />
									<px:PXSelector runat="server" DataField="RUTROTWorkTypeID" CommitChanges="True" ID="cmbRRWorkType" AutoRefresh="true" />
									<px:PXNumberEdit runat="server" DataField="CuryRUTROTAvailableAmt" ID="edRRAvailable" />
                                    <px:PXSelector runat="server" DataField="InvtRefNbr" ID="edInvtRefNbr" AutoRefresh="True" AllowEdit="True" />

									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edExpenseAccrualAccountID" runat="server" DataField="ExpenseAccrualAccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edExpenseAccrualSubID" runat="server" DataField="ExpenseAccrualSubID" AutoRefresh="True" />
									<px:PXSegmentMask CommitChanges="True" ID="edExpenseAccountID" runat="server" DataField="ExpenseAccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
									<px:PXDropDown runat="server" DataField="CostBasis" CommitChanges="True" ID="cmbCostBasis" />
									<px:PXNumberEdit ID="edCuryAccruedCost" runat="server" DataField="CuryAccruedCost" CommitChanges="true" />
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" AllowDragRows="true" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Add Order" Key="cmdShipmentList">
									<AutoCallBack Command="SelectShipment" Target="ds">
										<Behavior PostData="Page" CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add SO Line" Key="cmdSOLineList">
									<AutoCallBack Target="ds" Command="SelectSOLine">
										<Behavior CommitChanges="True" PostData="Page" />
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdARTranList" Text="Add Return Line">
									<AutoCallBack Command="SelectARTran" Target="ds">
										<Behavior PostData="Page" CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="View Schedule" Key="cmdViewSchedule">
									<AutoCallBack Command="ViewSchedule" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="View Item" Key="ViewItem">
									<AutoCallBack Command="ViewItem" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Reset Order" DependOnGrid="grid" Key="cmdResetOrder">
									<AutoCallBack Command="ResetOrder" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Insert Row" SyncText="false" ImageSet="main" ImageKey="AddNew">
									<AutoCallBack Target="grid" Command="AddNew" Argument="1"></AutoCallBack>
									<ActionBar ToolBarVisible="External" MenuVisible="true" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Cut Row" SyncText="false" ImageSet="main" ImageKey="Copy">
									<AutoCallBack Target="grid" Command="Copy"></AutoCallBack>
									<ActionBar ToolBarVisible="External" MenuVisible="true" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Insert Cut Row" SyncText="false" ImageSet="main" ImageKey="Paste">
									<AutoCallBack Target="grid" Command="Paste"></AutoCallBack>
									<ActionBar ToolBarVisible="External" MenuVisible="true" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<CallbackCommands PasteCommand="PasteLine">
							<Save PostData="Container" />
						</CallbackCommands>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details"
						BorderWidth="0px">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" CommitChanges="true" AutoRefresh="true"/>
									<px:PXNumberEdit SuppressLabel="True" ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
									<px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
									<px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
									<px:PXNumberEdit ID="edCuryDiscountedTaxableAmt" runat="server" DataField="CuryDiscountedTaxableAmt" Enabled="False" />
									<px:PXNumberEdit ID="edCuryDiscountedPrice" runat="server" DataField="CuryDiscountedPrice" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" AllowUpdate="False" CommitChanges="true"/>
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="CuryExemptedAmt" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="TaxUOM" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxableQty" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="CuryDiscountedTaxableAmt" />
									<px:PXGridColumn DataField="CuryDiscountedPrice" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Commissions" RepaintOnDemand="false">
				<Template>
					<px:PXFormView ID="Commission" runat="server" DataMember="CurrentDocument" RenderStyle="Simple" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXSegmentMask CommitChanges="True" ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" Enabled="False" />
							<px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" Enabled="False" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridSalesPerTran" runat="server" Height="200px" Width="100%" DataSourceID="ds" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="SalesPerTrans">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXNumberEdit ID="edCommnPct" runat="server" DataField="CommnPct" AllowNull="True" />
									<px:PXNumberEdit ID="edCommnAmt" runat="server" DataField="CommnAmt" />
									<px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" />
									<px:PXNumberEdit ID="edCommnblAmt" runat="server" DataField="CommnblAmt" />
									<px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" AllowNull="True" />
									<px:PXSegmentMask ID="edSalesPersonID_1" runat="server" DataField="SalespersonID" AutoRefresh="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SalespersonID" AutoCallBack="True" />
									<px:PXGridColumn DataField="CommnPct" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryCommnAmt" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="CuryCommnblAmt" TextAlign="Right" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Freight Details">
				<Template>
					<px:PXFormView ID="formG" runat="server" DataSourceID="ds" Width="100%" DataMember="CurrentDocument" CaptionVisible="False" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXNumberEdit ID="edCuryFreightAmt" runat="server" DataField="CuryFreightAmt" />
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXNumberEdit ID="edCuryPremiumFreightAmt" runat="server" DataField="CuryPremiumFreightAmt"/>
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridFreightDetails" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 265px;"
						Width="100%" BorderWidth="0px" SkinID="Details" Height="265px">
						<Levels>
							<px:PXGridLevel DataMember="FreightDetails">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" />
									<px:PXSelector ID="edOrderNbr" runat="server" DataField="OrderNbr" AllowEdit="True" />
									<px:PXSelector ID="edShipmentNbr" runat="server" DataField="ShipmentNbr" AllowEdit="true" />
									<px:PXDropDown ID="edShipmentType" runat="server" DataField="ShipmentType" />
									<px:PXSelector ID="edShipTermsID" runat="server" DataField="ShipTermsID" />
									<px:PXSelector ID="edShipZoneID" runat="server" DataField="ShipZoneID" />
									<px:PXSelector ID="edShipVia" runat="server" DataField="ShipVia" />
									<px:PXNumberEdit ID="edWeight" runat="server" DataField="Weight" />
									<px:PXNumberEdit ID="edVolume" runat="server" DataField="Volume" />
									<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" />
									<px:PXNumberEdit ID="edCuryFreightAmt" runat="server" DataField="CuryFreightAmt" />
									<px:PXNumberEdit ID="edCuryPremiumFreightAmt" runat="server" DataField="CuryPremiumFreightAmt" />
									<px:PXNumberEdit ID="edCuryTotalFreightAmt" runat="server" DataField="CuryTotalFreightAmt" />
									<px:PXSegmentMask CommitChanges="True" ID="edAccountID2" runat="server" DataField="AccountID" />
									<px:PXSegmentMask ID="edSubID2" runat="server" DataField="SubID" />
									<px:PXSelector ID="edTaxCategoryID2" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="OrderType" />
									<px:PXGridColumn DataField="OrderNbr" />
									<px:PXGridColumn AllowUpdate="False" DataField="ShipmentNbr" />
									<px:PXGridColumn DataField="ShipmentType" />
									<px:PXGridColumn DataField="ShipTermsID" DisplayFormat="&gt;aaaaaaaaaa" />
									<px:PXGridColumn DataField="ShipZoneID" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
									<px:PXGridColumn DataField="ShipVia" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Weight" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Volume" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryLineTotal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryFreightCost" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryFreightAmt" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryPremiumFreightAmt" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryTotalFreightAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="AccountID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID_Account_description" />
									<px:PXGridColumn DataField="SubID" DisplayFormat="&gt;AAAA.AA.AA.AAAA" />
									<px:PXGridColumn DataField="TaskID" AutoCallBack="True" />
									<px:PXGridColumn DataField="TaxCategoryID" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Link to GL" />
                    <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" DataSourceID="ds" />
                    <px:PXSegmentMask ID="edARAccountID" runat="server" DataField="ARAccountID" DataSourceID="ds" CommitChanges="true" />
	                <px:PXSegmentMask ID="edARSubID" runat="server" DataField="ARSubID" AutoRefresh="True" DataSourceID="ds" />    
					<px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True">
						<LinkCommand Target="ds" Command="ViewOriginalDocument" />
					</px:PXTextEdit>
					<px:PXTextEdit ID="edCorrectionRefNbr" runat="server" DataField="CorrectionRefNbr" Enabled="False" AllowEdit="True">
						<LinkCommand Target="ds" Command="ViewCorrectionDocument" />
					</px:PXTextEdit>
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="Assigned To" />
                    <px:PXTreeSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_"
                        TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False">
                        <DataBindings>
                            <px:PXTreeItemBinding TextField="Description" ValueField="Description" />
                        </DataBindings>
                    </px:PXTreeSelector>
                    <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" AutoRefresh="True" DataField="OwnerID" DataSourceID="ds" />
                    <px:PXLayoutRule runat="server" GroupCaption="Print and Email Options" StartGroup="True" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkDontPrint" runat="server" Checked="True" DataField="DontPrint" Size="SM" AlignLeft="true" />
                    <px:PXCheckBox ID="chkPrinted" runat="server" DataField="Printed" Enabled="False" Size="SM" AlignLeft="true" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkDontEmail" runat="server" Checked="True" DataField="DontEmail" Size="SM" AlignLeft="true" />
                    <px:PXCheckBox ID="chkEmailed" runat="server" DataField="Emailed" Enabled="False" Size="SM" AlignLeft="true" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Tax Info" StartGroup="True" />
                    <px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" Text="ZONE1" DataSourceID="ds" />
                    <px:PXDropDown CommitChanges="True" ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" />
                    <px:PXDropDown ID="edAvalaraCustomerUsageTypeID" runat="server" CommitChanges="True" DataField="AvalaraCustomerUsageType" />
                    <px:PXLayoutRule runat="server" StartGroup="True" LabelsWidth="SM" ControlSize="M" GroupCaption="Cash Discount Info" />
                    <px:PXNumberEdit runat="server" DataField="CuryDiscountedDocTotal" ID="edCuryDiscountedDocTotal" Enabled="false" />
                    <px:PXNumberEdit runat="server" DataField="CuryDiscountedTaxableTotal" ID="edCuryDiscountedTaxableTotal" Enabled="false" />
                    <px:PXNumberEdit runat="server" DataField="CuryDiscountedPrice" ID="edCuryDiscountedPrice" Enabled="false" />
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Payment Information" RepaintOnDemand = "false">
				<Template>
					<px:PXFormView ID="formP" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="SODocument" CaptionVisible="False"
						SkinID="Transparent">
						<Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
							<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector CommitChanges="True" Size="xm" ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr"
								AutoRefresh="True" AutoGenerateColumns="True" DataSourceID="ds" />
							<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" DataSourceID="ds" />
							<px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXDateTimeEdit CommitChanges="True" Size="s" ID="edClearDate" runat="server" DataField="ClearDate" />
							<px:PXCheckBox CommitChanges="True" ID="edCleared" runat="server" DataField="Cleared" />
							<px:PXLayoutRule runat="server" />
							<px:PXTextEdit ID="edCCPaymentStateDescr" runat="server" DataField="CCPaymentStateDescr" Enabled="False" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XXS" ControlSize="M" SuppressLabel="True" />
							<px:PXButton ID="pbAutorizeCCPayment" runat="server" Text="Authorize Payment" CommandName="AuthorizeCCPayment" CommandSourceID="ds" Width="200px" />
							<px:PXButton ID="pbCaptureCCPayment" runat="server" Text="Capture Payment" CommandName="CaptureCCPayment" CommandSourceID="ds" Width="200px" />
							<px:PXButton ID="pbVoidCCPayment" runat="server" Text="Void Authorization" CommandName="VoidCCPayment" CommandSourceID="ds" Width="200px" />
							<px:PXButton ID="pbCreditCCPayment" runat="server" Text="Refund Payment" CommandName="CreditCCPayment" CommandSourceID="ds" Width="200px" />
                            <px:PXButton ID="pbValidateCCPayment" runat="server" Text="Validate Payment" CommandName="ValidateCCPayment" CommandSourceID="ds" Width="200px"/>
                            <px:PXLayoutRule runat="server"  LabelsWidth="SM" ControlSize="XM" StartColumn="true" />
                            <px:PXPanel ID="formPpanel1" runat="server" DataMember="CurrentDocument" RenderStyle="Simple">
                                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                                <px:PXNumberEdit ID="edCuryPaymentTotal" runat="server" DataField="CuryPaymentTotal" Enabled="False" />
                            </px:PXPanel>
                            	<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXPanel ID="formPpanel2" runat="server"  RenderStyle="Simple" >
                                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit ID="edCuryAmtToCapture" runat="server" DataField="CuryAmtToCapture" Enabled="False" />
							<px:PXNumberEdit ID="edCuryCCCapturedAmt" runat="server" DataField="CuryCCCapturedAmt" Enabled="False" />
							<px:PXSelector ID="edRefTranExtNbr" runat="server" DataField="RefTranExtNbr" DataSourceID="ds" AutoRefresh="True" CommitChanges="True" />
                                </px:PXPanel>
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="grdCCProcTran" runat="server" DataSourceID="ds" Height="100%" Width="100%" BorderWidth="0px" Style="left: 0px; top: 0px;"
						SkinID="DetailsInTab" Caption="Credit Card Processing Info">
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
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXNumberEdit ID="edTranNbr2" runat="server" DataField="TranNbr" />
									<px:PXDropDown ID="edProcStatus" runat="server" AllowNull="False" DataField="ProcStatus" />
									<px:PXTextEdit ID="edProcessingCenterID" runat="server" AllowNull="False" DataField="ProcessingCenterID" />
									<px:PXDropDown ID="edCVVVerificationStatus" runat="server" DataField="CVVVerificationStatus" />
									<px:PXDropDown ID="edCCTranType" runat="server" DataField="TranType" />
									<px:PXDropDown ID="edTranStatus" runat="server" AllowNull="False" DataField="TranStatus" />
									<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
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
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" MinWidth="50" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Address Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
					<px:PXFormView ID="formC" runat="server" Caption="Bill-To Contact" DataMember="Billing_Contact" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
							<px:PXCheckBox ID="chkOverrideContact" runat="server" CommitChanges="True" DataField="OverrideContact" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" />
						</Template>
						<ContentStyle BackColor="Transparent" BorderStyle="None" />
					</px:PXFormView>
					<px:PXFormView ID="formA" runat="server" Caption="Bill-To Address" DataMember="Billing_Address" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
							<px:PXCheckBox ID="chkOverrideAddress" runat="server" CommitChanges="True" DataField="OverrideAddress" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AutoRefresh="True" DataField="CountryID" DataSourceID="ds" CommitChanges="true" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" DataSourceID="ds" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
						</Template>
						<ContentStyle BackColor="Transparent" BorderStyle="None" />
					</px:PXFormView>
					<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
					<px:PXFormView ID="Shipping_Contact" runat="server" Caption="Ship-To Contact" DataMember="Shipping_Contact" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
							<px:PXCheckBox ID="chkOverrideContact" runat="server" CommitChanges="True" DataField="OverrideContact" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" />
						</Template>
						<ContentStyle BackColor="Transparent" BorderStyle="None" />
					</px:PXFormView>
					<px:PXFormView ID="Shipping_Address" runat="server" Caption="Ship-To Address" DataMember="Shipping_Address" DataSourceID="ds" AllowCollapse="false" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
							<px:PXCheckBox ID="chkMultiShipAddress" runat="server" CommitChanges="True" DataField="CurrentDocument.MultiShipAddress" />
							<px:PXCheckBox ID="chkOverrideAddress" runat="server" CommitChanges="True" DataField="OverrideAddress" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AutoRefresh="True" DataField="CountryID" DataSourceID="ds" CommitChanges="true" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" DataSourceID="ds" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" CommitChanges="True" DataField="PostalCode" />
						</Template>
						<ContentStyle BackColor="Transparent" BorderStyle="None" />
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Discount Details" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="discountDetailGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="Details" BorderWidth="0px" SyncPosition="true"
						Style="left: 0px; top: 0px;">
						<Levels>
							<px:PXGridLevel DataMember="ARDiscountDetails">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkSkipDiscount" runat="server" DataField="SkipDiscount" />
                                    <px:PXTextEdit ID="edSOOrderType" runat="server" DataField="OrderType" Enabled="False" />
                                    <px:PXSelector ID="edSOOrderNbr" runat="server" DataField="OrderNbr" Enabled="False" AllowEdit="True" />
									<px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID" AllowEdit="true" />
									<px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" AllowEdit="true" />
									<px:PXMaskEdit ID="edType" runat="server" DataField="Type" InputMask="&gt;a" />
									<px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
									<px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
									<px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
									<px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" CommitChanges="true" />
									<px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" CommitChanges="true" />
									<px:PXSegmentMask ID="edFreeItemID" runat="server" DataField="FreeItemID" AllowEdit="True" />
									<px:PXNumberEdit ID="edFreeItemQty" runat="server" DataField="FreeItemQty" />
                                    <px:PXTextEdit ID="edExtDiscCode" runat="server" DataField="ExtDiscCode" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SkipDiscount" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="LineNbr" />
									<px:PXGridColumn DataField="OrderType" />
									<px:PXGridColumn DataField="OrderNbr" />
									<px:PXGridColumn DataField="DiscountID" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscountSequenceID" CommitChanges="true" />
									<px:PXGridColumn DataField="Type" DisplayFormat="&gt;a" />
									<px:PXGridColumn DataField="IsManual" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscountPct" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="FreeItemID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
									<px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ExtDiscCode" />
                                    <px:PXGridColumn DataField="Description" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Applications" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="detgrid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 332px;" Width="100%"
								BorderWidth="0px" SkinID="Details" Height="332px" FilesIndicator="True" NoteIndicator="True" SyncPosition="True" Caption="Application Invoice" CaptionVisible="false"
								AllowPaging="true" PageSize="100">
						<Levels>
							<px:PXGridLevel DataMember="Adjustments">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXDropDown ID="edAdjgDocType" runat="server" DataField="AdjgDocType" Enabled="False" />
									<px:PXSelector CommitChanges="True" ID="edAdjgRefNbr" runat="server" DataField="AdjgRefNbr" Enabled="False" AllowEdit="true">
										<Parameters>
											<px:PXControlParam ControlID="form" Name="ARInvoice.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
											<px:PXControlParam ControlID="detgrid" Name="ARAdjust.adjgDocType" PropertyName="DataValues[&quot;AdjgDocType&quot;]" />
										</Parameters>
									</px:PXSelector>
									<px:PXNumberEdit CommitChanges="True" ID="edCuryAdjdAmt" runat="server" DataField="CuryAdjdAmt" />
									<px:PXDateTimeEdit ID="edARPayment__DocDate" runat="server" DataField="ARPayment__DocDate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
									<px:PXSelector ID="edARPayment__CuryID" runat="server" DataField="ARPayment__CuryID" />
									<px:PXSelector ID="edARPayment__FinPeriodID" runat="server" DataField="ARPayment__FinPeriodID" Enabled="False" />
									<px:PXTextEdit ID="edARPayment__ExtRefNbr" runat="server" DataField="ARPayment__ExtRefNbr" />
									<px:PXDropDown ID="edARPayment__Status" runat="server" AllowNull="False" DataField="ARPayment__Status" Enabled="False" />
									<px:PXTextEdit ID="edARPayment__DocDesc" runat="server" DataField="ARPayment__DocDesc" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="True" AllowFilter="True" AllowMove="False" AutoCallBack="True" />
									<px:PXGridColumn DataField="AdjgDocType" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjgRefNbr" AutoCallBack="True" LinkCommand="ViewPayment"/>
									<px:PXGridColumn AllowNull="False" DataField="CuryAdjdAmt" AutoCallBack="True" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryAdjgDiscAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryAdjdWOAmt" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="ARPayment__DocDate" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARPayment__DocDesc" />
									<px:PXGridColumn AllowUpdate="False" DataField="ARPayment__CuryID" DisplayFormat="&gt;LLLLL" />
									<px:PXGridColumn AllowUpdate="False" DataField="ARPayment__FinPeriodID" DisplayFormat="##-####" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ARPayment__ExtRefNbr" />
									<px:PXGridColumn DataField="CustomerID" TextAlign="Right" />
									<px:PXGridColumn DataField="AdjdDocType" />
									<px:PXGridColumn DataField="AdjdRefNbr" />
									<px:PXGridColumn DataField="AdjNbr" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="ARPayment__Status" RenderEditorText="True" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar DefaultAction="ViewPayment">
							<CustomItems>
								<px:PXToolBarButton Text="Load Documents">
									<AutoCallBack Command="LoadDocuments" Target="ds">
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
						<Mode AllowFormEdit="False" AllowAddNew="False" />
					</px:PXGrid>
                    <px:PXGrid ID="detgrid2" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" FilesIndicator="True" NoteIndicator="True" SyncPosition="True" Caption="Application Credit Memo"  CaptionVisible="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Adjustments_1">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXDropDown ID="edAdjdDocType3" runat="server" DataField="AdjgDocType" Enabled="False" />
                                    <px:PXSelector CommitChanges="True" ID="edAdjdRefNbr3" runat="server" DataField="AdjdRefNbr" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="form" Name="ARInvoice.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
                                            <px:PXControlParam ControlID="detgrid2" Name="ARAdjust.adjdDocType" PropertyName="DataValues[&quot;AdjdDocType&quot;]" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit CommitChanges="True" ID="edCuryAdjgAmt3" runat="server" DataField="CuryAdjgAmt" />
                                    <px:PXDateTimeEdit ID="edARInvoice__DocDate3" runat="server" DataField="ARInvoice__DocDate" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryDocBal3" runat="server" DataField="CuryDocBal" Enabled="False" />
                                    <px:PXSelector ID="edARInvoice__CuryID3" runat="server" DataField="ARInvoice__CuryID" />
                                    <px:PXSelector ID="edARInvoice__FinPeriodID3" runat="server" DataField="ARInvoice__FinPeriodID" Enabled="False" />
                                    <px:PXTextEdit ID="edARInvoice__InvoiceNbr3" runat="server" DataField="ARInvoice__InvoiceNbr" />
                                    <px:PXDropDown ID="edARInvoice__Status3" runat="server" DataField="ARInvoice__Status" Enabled="False" />
                                    <px:PXTextEdit ID="edARInvoice__DocDesc3" runat="server" DataField="ARInvoice__DocDesc" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AdjdDocType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="AdjdRefNbr" AutoCallBack="True" LinkCommand="ViewInvoice" />
                                    <px:PXGridColumn DataField="AdjdCustomerID" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryAdjgAmt" AutoCallBack="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ARInvoice__DocDate" />
                                    <px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ARInvoice__DocDesc" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryID" />
                                    <px:PXGridColumn DataField="ARInvoice__FinPeriodID" />
                                    <px:PXGridColumn DataField="ARInvoice__InvoiceNbr" />
                                    <px:PXGridColumn DataField="ARInvoice__Status" Label="Status" Type="DropDownList" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowFormEdit="False" />
                    </px:PXGrid>
                    <px:PXGrid ID="detgrid3" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" FilesIndicator="True" NoteIndicator="True" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Adjustments_2">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXDropDown ID="edAdjdDocType2" runat="server" DataField="DisplayDocType" Enabled="False" />
                                    <px:PXSelector CommitChanges="True" ID="edAdjdRefNbr2" runat="server" DataField="DisplayRefNbr" AutoRefresh="true">
                                        <Parameters>
                                            <px:PXControlParam ControlID="form" Name="ARInvoice.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
                                            <px:PXControlParam ControlID="detgrid3" Name="ARAdjust.adjdDocType" PropertyName="DataValues[&quot;DisplayDocType&quot;]" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXNumberEdit CommitChanges="True" ID="edCuryAdjgAmt2" runat="server" DataField="DisplayCuryAmt" />
                                    <px:PXDateTimeEdit ID="edARInvoice__DocDate" runat="server" DataField="DisplayDocDate" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryDocBal2" runat="server" DataField="CuryDocBal" Enabled="False" />
                                    <px:PXSelector ID="edARInvoice__CuryID" runat="server" DataField="DisplayCuryID" />
                                    <px:PXSelector ID="edARInvoice__FinPeriodID" runat="server" DataField="DisplayFinPeriodID" Enabled="False" />
                                    <px:PXTextEdit ID="edARInvoice__InvoiceNbr" runat="server" DataField="ARInvoice__InvoiceNbr" />
                                    <px:PXDropDown ID="edARInvoice__Status" runat="server" DataField="DisplayStatus" Enabled="False" />
                                    <px:PXTextEdit ID="edARInvoice__DocDesc" runat="server" DataField="DisplayDocDesc" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DisplayDocType" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DisplayRefNbr" AutoCallBack="True" LinkCommand="ViewInvoice2" />
                                    <px:PXGridColumn DataField="DisplayCustomerID" TextAlign="Right" />
                                    <px:PXGridColumn DataField="DisplayCuryAmt" AutoCallBack="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="DisplayDocDate" />
                                    <px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="DisplayDocDesc" />
                                    <px:PXGridColumn DataField="DisplayCuryID" />
                                    <px:PXGridColumn DataField="DisplayFinPeriodID" />
                                    <px:PXGridColumn DataField="ARInvoice__InvoiceNbr" />
                                    <px:PXGridColumn DataField="DisplayStatus" Label="Status" Type="DropDownList" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar DefaultAction="ViewInvoice2" />
						<Mode AllowFormEdit="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="ROT/RUT Details" VisibleExp="DataControls[&quot;chkRUTROT&quot;].Value == 1" BindingContext="form" RepaintOnDemand="false">
				<Template>
					<px:PXFormView runat="server" SkinID="Transparent" ID="RUTROTForm" DataSourceID="ds" DataMember="Rutrots">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" GroupCaption="RUT and ROT Settings" />
							<px:PXCheckBox runat="server" DataField="AutoDistribution" CommitChanges="True" ID="chkRRAutoDistribution" AlignLeft="true" />
							<px:PXGroupBox runat="server" DataField="RUTROTType" CommitChanges="True" RenderStyle="Simple" ID="gbRRType">
								<ContentLayout Layout="Stack" Orientation="Horizontal" />
								<Template>
									<px:PXRadioButton runat="server" Value="O" ID="gbRRType_opO" GroupName="gbRRType" Text="ROT" />
									<px:PXRadioButton runat="server" Value="U" ID="gbRRType_opU" GroupName="gbRRType" Text="RUT" />
								</Template>
							</px:PXGroupBox>
							<px:PXTextEdit runat="server" DataField="ROTAppartment" ID="edRAppartment" />
							<px:PXTextEdit runat="server" DataField="ROTEstate" ID="edRREstate" />
							<px:PXTextEdit runat="server" DataField="ROTOrganizationNbr" ID="edRROrganizationNbr" CommitChanges="true" />
							<px:PXLayoutRule runat="server" GroupCaption="RUT and ROT Distribution" />
							<px:PXGrid runat="server" DataSourceID="ds" Width="350px" AllowFilter="false" AllowSearch="false" Height="200px" SkinID="DetailsInTab" ID="gridDistribution"
								Caption="RUT and ROT Distribution" CaptionVisible="false">
								<Mode InitNewRow="true" />
								<Levels>
									<px:PXGridLevel DataMember="RRDistribution">
										<RowTemplate>
											<px:PXTextEdit runat="server" DataField="PersonalID" ID="edPersonalID" />
											<px:PXNumberEdit runat="server" DataField="CuryAmount" ID="edAmountRR" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="PersonalID" />
											<px:PXGridColumn DataField="CuryAmount" TextAlign="Right" />
											<px:PXGridColumn DataField="Extra" Type="CheckBox" />
										</Columns>

									</px:PXGridLevel>
								</Levels>
								<ActionBar>
									<Actions>
										<ExportExcel Enabled="false" />
										<AddNew Enabled="true" />
										<Delete Enabled="true" />
										<AdjustColumns Enabled="true" />
									</Actions>
								</ActionBar>
							</px:PXGrid>
							<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" GroupCaption="RUT and ROT Totals" />
							<px:PXNumberEdit runat="server" DataField="DeductionPct" CommitChanges="True" ID="edRRDeduction" />
							<px:PXNumberEdit runat="server" DataField="CuryTotalAmt" ID="edRRTotalAmt" Enabled="false" />
							<px:PXNumberEdit runat="server" DataField="CuryOtherCost" ID="edRUTROTOtherCost" Enabled="false" />
							<px:PXNumberEdit runat="server" DataField="CuryMaterialCost" ID="edRUTROTMaterialCost" Enabled="false" />
							<px:PXNumberEdit runat="server" DataField="CuryWorkPrice" ID="edRUTROTWorkPrice" Enabled="false" />
							<px:PXNumberEdit runat="server" DataField="CuryDistributedAmt" ID="edRRAvailAmt" Enabled="false" />
							<px:PXNumberEdit runat="server" DataField="CuryUndistributedAmt" ID="edRRUndsitributedAmt" Enabled="false" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" RepaintControlsIDs="ds" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>

	<px:PXSmartPanel ID="PanelAddShipment" runat="server" Height="396px" Style="z-index: 108; left: 216px; position: absolute; top: 171px"
		Width="873px" CommandName="AddShipment" CommandSourceID="ds" Caption="Add Order" CaptionVisible="True" LoadOnDemand="true"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
		AutoCallBack-Target="grid4" Key="ShipmentList">
		<px:PXGrid ID="grid4" runat="server" Height="240px" Width="100%" DataSourceID="ds" BatchUpdate="true" Style="border-width: 1px 0px"
			AutoAdjustColumns="true" SkinID="Inquire" FilesIndicator="false" NoteIndicator="false">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="shipmentlist">
					<Columns>
						<px:PXGridColumn AllowNull="False" DataField="Selected" DataType="Boolean" DefValueText="False" TextAlign="Center" Type="CheckBox"
							AllowCheckAll="True" AllowSort="False" AllowMove="False" />
						<px:PXGridColumn AllowUpdate="False" DataField="OrderType" />
						<px:PXGridColumn AllowUpdate="False" DataField="OrderNbr" />
						<px:PXGridColumn AllowUpdate="False" DataField="ShipmentNbr" />
						<px:PXGridColumn AllowUpdate="False" DataField="CustomerID" DisplayFormat="AAAAAAAAAA" />
						<px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID" DisplayFormat="&gt;AAAAAAA" />
						<px:PXGridColumn AllowUpdate="False" DataField="ShipDate" DataType="DateTime" />
						<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ShipmentQty" DataType="Decimal" Decimals="2" DefValueText="0.0"
							TextAlign="Right" />
					</Columns>
					<Layout ColumnsMenu="False" />
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" CommandName="AddShipment" CommandSourceID="ds" Text="Add" SyncVisible="false" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="Add &amp; Close" />
			<px:PXButton ID="PXButton3" runat="server" CommandName="AddShipmentCancel" CommandSourceID="ds" DialogResult="Cancel" Text="Cancel" SyncVisible="false" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<%-- Recalculate Prices and Discounts --%>
	<px:PXSmartPanel ID="PanelRecalcDiscounts" runat="server" Caption="Recalculate Prices" CaptionVisible="true" LoadOnDemand="true" Key="recalcdiscountsfilter"
		AutoCallBack-Enabled="true" AutoCallBack-Target="formRecalcDiscounts" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
			<px:PXFormView ID="formRecalcDiscounts" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="recalcdiscountsfilter">
				<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
				<ContentStyle BackColor="Transparent" BorderStyle="None" />
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
					<px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" />
					<px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server" DataField="RecalcUnitPrices" />
					<px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server" DataField="OverrideManualPrices" Style="margin-left: 25px" />
					<px:PXCheckBox CommitChanges="True" ID="chkRecalcDiscounts" runat="server" DataField="RecalcDiscounts" />
					<px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" Style="margin-left: 25px" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" Style="margin-left: 25px" />
				</Template>
			</px:PXFormView>
		<px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton10" runat="server" DialogResult="OK" Text="OK" CommandName="RecalcOk" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" Height="396px" Width="900px" ID="PanelAddSOLine" LoadOnDemand="true" CaptionVisible="True" Caption="Add SO Line" Key="SOLineList" CommandSourceID="ds"
		CommandName="AddSOLine" AutoCallBack-Enabled="True" AutoCallBack-Target="gridSOLineList" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
		<px:PXGrid runat="server" Height="240px" SkinID="Inquire" Width="100%" ID="gridSOLineList" AutoAdjustColumns="true" BatchUpdate="true" DataSourceID="ds" NoteIndicator="false" FilesIndicator="false">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="soLineList">
					<Layout ColumnsMenu="False" />
					<Columns>
						<px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="Selected" DefValueText="False" AllowNull="False" AllowMove="False" AllowSort="False" AllowCheckAll="True" />
						<px:PXGridColumn DataField="OrderType" AllowUpdate="False" />
						<px:PXGridColumn DataField="OrderNbr" AllowUpdate="False" />
						<px:PXGridColumn DataField="CustomerID" AllowUpdate="False" />
						<px:PXGridColumn DataField="Operation" AllowUpdate="False" />
						<px:PXGridColumn DataField="ShipDate" AllowUpdate="False" />
						<px:PXGridColumn DataField="InventoryID" AllowUpdate="False" />
						<px:PXGridColumn TextAlign="Right" DataField="OrderQty" AllowNull="False" AllowUpdate="False" />
						<px:PXGridColumn TextAlign="Right" DataField="ShippedQty" AllowNull="False" AllowUpdate="False" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel runat="server" SkinID="Buttons" ID="PXPanel2">
			<px:PXButton runat="server" Text="Add" SyncVisible="false" CommandSourceID="ds" CommandName="AddSOLine" ID="PXButton4" />
			<px:PXButton runat="server" Text="Add &amp; Close" DialogResult="OK" ID="PXButton5" />
			<px:PXButton runat="server" Text="Cancel" DialogResult="Cancel" SyncVisible="false" CommandSourceID="ds" ID="PXButton6" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="PanelAddARTran" Height="396px" Width="910px" LoadOnDemand="true" CaptionVisible="True" Caption="Add Return Line" Key="ARTranList" CommandSourceID="ds" CommandName="AddARTran"
		AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" AutoCallBack-Target="gridARTranList" CallBackMode-PostData="Page" CallBackMode-CommitChanges="True">
		<px:PXGrid runat="server" Height="240px" SkinID="Inquire" Width="100%" ID="gridARTranList" AutoAdjustColumns="true" BatchUpdate="true" DataSourceID="ds" NoteIndicator="false" FilesIndicator="false">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="arTranList">
					<Layout ColumnsMenu="False" />
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" TextAlign="Center" DefValueText="False" Width="20px" AllowNull="False" AllowMove="False" AllowSort="False" AllowCheckAll="True" />
						<px:PXGridColumn DataField="TranType" AllowUpdate="False" />
						<px:PXGridColumn DataField="RefNbr" AllowUpdate="False" />
						<px:PXGridColumn DataField="LineNbr" AllowUpdate="False" />
						<px:PXGridColumn DataField="CustomerID" AllowUpdate="False" />
						<px:PXGridColumn DataField="TranDate" AllowUpdate="False" />
						<px:PXGridColumn DataField="InventoryID" AllowUpdate="False" />
						<px:PXGridColumn DataField="UOM" AllowUpdate="False" />
						<px:PXGridColumn DataField="Qty" TextAlign="Right" AllowUpdate="False" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel runat="server" ID="PXPanel20" SkinID="Buttons">
			<px:PXButton runat="server" ID="PXButton40" Text="Add" SyncVisible="false" CommandSourceID="ds" CommandName="AddARTran" />
			<px:PXButton runat="server" ID="PXButton50" Text="Add &amp; Close" DialogResult="OK" />
			<px:PXButton runat="server" ID="PXButton60" Text="Cancel" DialogResult="Cancel" SyncVisible="false" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
