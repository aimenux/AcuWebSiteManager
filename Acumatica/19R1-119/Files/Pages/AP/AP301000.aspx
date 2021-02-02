<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP301000.aspx.cs" Inherits="Page_AP301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APInvoiceEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Prebook" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="VoidInvoice" CommitChanges="true" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand Visible="false" Name="ReverseInvoice" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ReclassifyBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="VendorRefund" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="VoidDocument" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="PayInvoice" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewSchedule" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="CreateSchedule" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewBatch" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument"/>
			<px:PXDSCallbackCommand Visible="False" Name="ViewRetainageDocument"/>
			<px:PXDSCallbackCommand Visible="false" Name="ViewVoucherBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewWorkBook" />
			<px:PXDSCallbackCommand Visible="false" Name="NewVendor" />
			<px:PXDSCallbackCommand Visible="false" Name="EditVendor" />
			<px:PXDSCallbackCommand Visible="false" Name="VendorDocuments" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddReceiptLine2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOOrder2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOOrderLine2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddReceiptLine" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOOrder" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOOrderLine" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddLandedCost" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddLandedCost2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="LinkLine" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewPODocument" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="AutoApply" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewItem" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="ViewPayment" DependOnGrid="detgrid" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPostLandedCostTran" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
			<px:PXDSCallbackCommand Visible="false" Name="ReleaseRetainage" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%"
		DataMember="Document" Caption="Document Summary" NoteIndicator="True"
		FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
		LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edDocType"
		TabIndex="100" DataSourceID="ds" MarkRequired="Dynamic">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
				<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds">
				<GridProperties FastFilterFields="APInvoice__InvoiceNbr, VendorID, VendorID_Vendor_acctName" />
			</px:PXSelector>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
				<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" AutoRefresh="True" DataSourceID="ds" />
			<px:PXTextEdit CommitChanges="True" ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />

			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />

			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
				<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID"
					AllowAddNew="True" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server"
				AutoRefresh="True" DataField="VendorLocationID" DataSourceID="ds" />
				<pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" RateTypeView="_APInvoice_CurrencyInfo_"
					DataMember="_Currency_" DataSourceID="ds"/>
				<px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" DataSourceID="ds" AutoRefresh ="true" />
			<px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" AllowEdit="True" CommitChanges="true" />
			
			<px:PXLayoutRule runat="server" Merge="True" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edDueDate" runat="server" DataField="DueDate" />
				<px:PXCheckBox runat="server" ID="chkIsRetainageDocument" DataField="IsRetainageDocument" AlignLeft="true" Enabled="false" />
				<px:PXCheckBox runat="server" ID="chkRetainageApply" DataField="RetainageApply" CommitChanges="true" AlignLeft="true" />
			<px:PXLayoutRule runat="server" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDiscDate" runat="server" DataField="DiscDate" />
			
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
				<px:PXLayoutRule runat="server" StartColumn="True" />
				<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot" />
				<px:PXNumberEdit ID="CuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
				<px:PXNumberEdit ID="CuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" Size="s" />
				<px:PXNumberEdit ID="edCuryOrigWhTaxAmt" runat="server" DataField="CuryOrigWhTaxAmt" Enabled="False" />
				<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
				<px:PXNumberEdit ID="edCuryInitDocBal" runat="server" DataField="CuryInitDocBal" CommitChanges="True" />
				<px:PXNumberEdit ID="edCuryRoundDiff" runat="server" CommitChanges="True" DataField="CuryRoundDiff" Enabled="False" />
				<px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" CommitChanges="True" DataField="CuryOrigDocAmt" />
				<px:PXNumberEdit ID="edCuryTaxAmt" runat="server" CommitChanges="True" DataField="CuryTaxAmt" />
				<px:PXNumberEdit ID="edCuryOrigDiscAmt" runat="server" CommitChanges="True" DataField="CuryOrigDiscAmt" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>

	<style type="text/css">
		.leftDocTemplateCol {
			width: 50%;
			float: left;
			min-width: 90px;
		}

		.rightDocTemplateCol {
			min-width: 90px;
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
							<div id="div3" class="Field1"><%# ((PXGridCellContainer)Container).Text("vendorID_Vendor_acctName") %></div>
						</CellTemplate>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%">
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SyncPosition="True" SkinID="DetailsInTab" DataSourceID="ds" TabIndex="18300" KeepPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="Transactions">
								<Columns>
									<px:PXGridColumn DataField="BranchID" Width="100px" AutoCallBack="True"/>
									<px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="50px" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="InventoryID" Width="100px" AutoCallBack="True" LinkCommand="ViewItem" />
									<px:PXGridColumn DataField="AppointmentDate" Width="90" />
									<px:PXGridColumn DataField="CustomerLocationID" Width="90" />
									<px:PXGridColumn DataField="AppointmentID" Width="80" />
									<px:PXGridColumn DataField="SOID" Width="70" />
									<px:PXGridColumn DataField="POReceiptLine__SubItemID" Label="Subitem" />
									<px:PXGridColumn DataField="TranDesc" Width="200px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" AutoCallBack="True" />
									<px:PXGridColumn DataField="BaseQty" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="UOM" Width="50px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" Width="81px" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="ManualPrice" TextAlign="Center" AllowNull="False" Type="CheckBox" CommitChanges="True"/>
									<px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" Width="81px" AutoCallBack="True" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscPct" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryDiscCost" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="ManualDisc" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="DiscountID" RenderEditorText="True" TextAlign="Left" Width="90px" AutoCallBack="True" />
									<px:PXGridColumn DataField="DiscountSequenceID" TextAlign="Left" Width="90px" />
									<px:PXGridColumn DataField="RetainagePct" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryCashDiscBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryRetainageAmt" TextAlign="Right" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryRetainageBal" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="CuryTranBal" TextAlign="Right" />
									<px:PXGridColumn DataField="AccountID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID_Account_description" Width="100px" SyncVisibility="false" />
									<px:PXGridColumn DataField="SubID" Width="200px" AutoCallBack="True" />
									<px:PXGridColumn DataField="ProjectID" Label="Project" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="TaskID" Label="Task" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CostCodeID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="NonBillable" Label="Non Billable" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Box1099" Width="200px"/>
									<px:PXGridColumn DataField="DeferredCode" Width="50px" AutoCallBack="True" />
									<px:PXGridColumn DataField="DefScheduleID" TextAlign="Right" Width="100px"/>
									<px:PXGridColumn DataField="TaxCategoryID" Width="50px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Date" />
									<px:PXGridColumn DataField="POOrderType" />
									<px:PXGridColumn DataField="PONbr" />
									<px:PXGridColumn DataField="POLineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="LCDocType" />
									<px:PXGridColumn DataField="LCRefNbr" />
									<px:PXGridColumn DataField="LCLineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="ReceiptNbr" />
									<px:PXGridColumn DataField="ReceiptLineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="PPVDocType" />
									<px:PXGridColumn DataField="PPVRefNbr" TextAlign="Right" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AllowAddNew="True" AutoRefresh="True" />
									<px:PXSelector runat="server" ID="edAppointmentID" DataField="AppointmentID" AllowEdit="True" />
									<px:PXSelector runat="server" ID="edSOID" DataField="SOID" AllowEdit="True" />
									<px:PXSegmentMask ID="edPOReceiptLine__SubItemID" runat="server" DataField="POReceiptLine__SubItemID" Enabled="False" />
									<px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
									<px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" CommitChanges="true" />
									<px:PXCheckBox ID="chkManualPrice" runat="server" DataField="ManualPrice" CommitChanges="True" />
									<px:PXNumberEdit ID="edCuryLineAmt" runat="server" DataField="CuryLineAmt" CommitChanges="true" />
									<px:PXSelector ID="edDiscountCode" runat="server" DataField="DiscountID" CommitChanges="True" AllowEdit="True" />
									<px:PXNumberEdit ID="edDiscPct" runat="server" DataField="DiscPct" />
									<px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" />
									<px:PXCheckBox ID="chkManualDisc" runat="server" DataField="ManualDisc" CommitChanges="true" />

									<px:PXNumberEdit ID="edRetainagePct" runat="server" DataField="RetainagePct" />

									<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" Enabled="False" />
									<px:PXNumberEdit ID="edCuryTranBal" runat="server" DataField="CuryTranBal" Enabled="False" />
									<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" />
									<px:PXDropDown ID="edBox1099" runat="server" DataField="Box1099" />

									<px:PXLayoutRule runat="server" ColumnSpan="2" />
									<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />

									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" />
									<px:PXSelector ID="edDefScheduleID" runat="server" DataField="DefScheduleID" AutoRefresh="True" AllowEdit="True"/>
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" CommitChanges="True" AutoRefresh="True" />
									<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
									<px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edCostCode" runat="server" DataField="CostCodeID" AutoRefresh="True" AllowAddNew="true" />
									<px:PXDropDown ID="edPOOrderType" runat="server" DataField="POOrderType" Enabled="False" />
									<px:PXSelector ID="edPONbr" runat="server" DataField="PONbr" Enabled="False" AllowEdit="True" />
									<px:PXSelector ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" Enabled="False" AllowEdit="True" />
									<px:PXDropDown ID="edPPVDocType" runat="server" DataField="PPVDocType" Enabled="False" AllowEdit="True" />
									<px:PXSelector ID="edPPVRefNbr" runat="server" DataField="PPVRefNbr" Enabled="False" AllowEdit="True" />
									<px:PXDropDown ID="edLCDocType" runat="server" DataField="LCDocType" Enabled="False" AllowEdit="True" />
									<px:PXSelector ID="edLCRefNbr" runat="server" DataField="LCRefNbr" Enabled="False" AllowEdit="True" />
									<px:PXNumberEdit ID="edLCLineNbr" runat="server" DataField="LCLineNbr" Enabled="False" AllowEdit="True" />
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" AutoInsertField="CuryLineAmt" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="View Schedule" Key="cmdViewSchedule">
									<AutoCallBack Command="ViewSchedule" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add PO Receipt" Key="cmdRT">
									<AutoCallBack Command="AddPOReceipt" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add PO Receipt Line" Key="cmdRTL">
									<AutoCallBack Command="AddReceiptLine" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add PO Order" Key="cmdPO">
									<AutoCallBack Command="AddPOOrder" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add PO Line" Key="cmdPOLine">
									<AutoCallBack Command="AddPOOrderLine" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add LC" Key="cmdLC">
									<AutoCallBack Command="AddLandedCost" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton>
									<AutoCallBack Command="LinkLine" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="View Item" Key="ViewItem">
									<AutoCallBack Command="ViewItem" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details" LoadOnDemand="false">
				<Template>
					<px:PXFormView ID="form2" runat="server" Style="z-index: 100" Width="100%" DataMember="CurrentDocument" CaptionVisible="False" SkinID="Transparent" DataSourceID="ds" MarkRequired="Dynamic">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Link to GL" StartGroup="True" />
							<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" DataSourceID="ds" />
							<px:PXSelector ID="edPrebookBatchNbr" runat="server" DataField="PrebookBatchNbr" Enabled="False" AllowEdit="true" DataSourceID="ds" />
							<px:PXSelector ID="edVoidBatchNbr" runat="server" DataField="VoidBatchNbr" Enabled="False" AllowEdit="true" DataSourceID="ds" />
							<px:PXNumberEdit ID="edDisplayCuryInitDocBal" runat="server" DataField="DisplayCuryInitDocBal" Enabled="False" />
							<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" DataSourceID="ds" />
							<px:PXSegmentMask ID="edAPAccountID" runat="server" DataField="APAccountID" CommitChanges="True" DataSourceID="ds" />
							<px:PXSegmentMask ID="edAPSubID" runat="server" DataField="APSubID" CommitChanges="True" AutoRefresh="True" DataSourceID="ds" />
							<px:PXSegmentMask ID="edPrebookAcctID" runat="server" DataField="PrebookAcctID" CommitChanges="True" DataSourceID="ds" />
							<px:PXSegmentMask ID="edPrebookSubID" runat="server" DataField="PrebookSubID" CommitChanges="True" AutoRefresh="True" DataSourceID="ds" />
							<px:PXSegmentMask ID="edRetainageAcctID" runat="server" DataField="RetainageAcctID" DataSourceID="ds" />
							<px:PXSegmentMask ID="edRetainageSubID" runat="server" DataField="RetainageSubID" DataSourceID="ds" />
								<px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True">
									<LinkCommand Target="ds" Command="ViewOriginalDocument"/>
								</px:PXTextEdit>

							<px:PXLayoutRule runat="server" GroupCaption="Default Payment Info" StartGroup="True" />
							<px:PXCheckBox ID="chkSeparateCheck" runat="server" DataField="SeparateCheck" />
							<px:PXCheckBox CommitChanges="True" ID="chkPaySel" runat="server" DataField="PaySel" />
							<px:PXCheckBox CommitChanges="True" ID="chkPaymentsByLinesAllowed" runat="server" DataField="PaymentsByLinesAllowed" />
							<px:PXDateTimeEdit ID="edPayDate" runat="server" DataField="PayDate" />
							<px:PXSegmentMask CommitChanges="True" ID="edPayLocationID" runat="server" AutoRefresh="True" DataField="PayLocationID" DataSourceID="ds" />
							<px:PXSelector CommitChanges="True" ID="edPayTypeID" runat="server" DataField="PayTypeID" DataSourceID="ds" />
							<px:PXSegmentMask CommitChanges="True" ID="edPayAccountID" runat="server" DataField="PayAccountID" DataSourceID="ds" AutoRefresh="true"/>

							<px:PXLayoutRule runat="server" ControlSize="XM" GroupCaption="Tax" LabelsWidth="SM" StartColumn="True" StartGroup="True" />
							<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" DataSourceID="ds" />
							<px:PXCheckBox ID="chkUsesManualVAT" runat="server" DataField="UsesManualVAT" Enabled="false" />
							<px:PXDropDown runat="server" ID="edTaxCalcMode" DataField="TaxCalcMode" CommitChanges="true" />

							<px:PXLayoutRule runat="server" ControlSize="XM" GroupCaption="Assigned To" LabelsWidth="SM" StartGroup="True" />
							<px:PXSelector ID="edEmployeeWorkgroupID" CommitChanges="true" runat="server" AutoRefresh="True" DataField="EmployeeWorkgroupID" DataSourceID="ds" />
							<px:PXSelector ID="edEmployeeID" CommitChanges="true" runat="server" AutoRefresh="True" DataField="EmployeeID" DataSourceID="ds" />

							<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" GroupCaption="Voucher Details" />
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
							<px:PXLayoutRule runat="server" ControlSize="XM" GroupCaption="Receipt Info" LabelsWidth="SM" StartGroup="True" />
							<px:PXSegmentMask CommitChanges="True" ID="edSuppliedByVendorID" runat="server" DataField="SuppliedByVendorID" AllowEdit="True" AutoRefresh="True"/>
							<px:PXSegmentMask CommitChanges="True" ID="edSuppliedByVendorLocationID" runat="server" DataField="SuppliedByVendorLocationID" AutoRefresh="True" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" GroupCaption="Cash Discount Info" StartGroup="True" />
							<px:PXNumberEdit runat="server" DataField="CuryDiscountedDocTotal" ID="edCuryDiscountedDocTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" DataField="CuryDiscountedTaxableTotal" ID="edCuryDiscountedTaxableTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" DataField="CuryDiscountedPrice" ID="edCuryDiscountedPrice" Enabled="false" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" Style="z-index: 100;" Height="300px"
						Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" DataSourceID="ds"
						TabIndex="3900">
						<AutoSize Enabled="True" MinHeight="150" />
						<LevelStyles>
							<RowForm Width="300px">
							</RowForm>
						</LevelStyles>
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
								<Save Enabled="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="100px" />
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryRetainedTaxableAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryRetainedTaxAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryExpenseAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="Tax__TaxType"/>
									<px:PXGridColumn DataField="Tax__PendingTax" TextAlign="Center" Type="CheckBox" Width="60px"/>
									<px:PXGridColumn DataField="Tax__ReverseTax" TextAlign="Center" Type="CheckBox" Width="60px"/>
									<px:PXGridColumn DataField="Tax__ExemptTax" TextAlign="Center" Type="CheckBox" Width="60px"/>
									<px:PXGridColumn DataField="Tax__StatisticalTax" TextAlign="Center" Type="CheckBox" Width="60px"/>
									<px:PXGridColumn DataField="CuryDiscountedTaxableAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryDiscountedPrice" TextAlign="Right" Width="100px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Approval Details" BindingContext="form" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" Style="left: 0; top: 0;">
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
			<px:PXTabItem Text="Discount Details">
				<Template>
					<px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None">
						<Levels>
							<px:PXGridLevel DataMember="DiscountDetails" DataKeyNames="DocType,RefNbr,DiscountID,DiscountSequenceID,Type">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkSkipDiscount" runat="server" DataField="SkipDiscount" />
									<px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID"
										AllowEdit="True" edit="1" />
									<px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
									<px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
									<px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" AutoRefresh="true" AllowEdit="true" />
									<px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
									<px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
									<px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" CommitChanges="true" />
									<px:PXNumberEdit ID="edCuryRetainedDiscountAmt" runat="server" DataField="CuryRetainedDiscountAmt" />
									<px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" CommitChanges="true" />
									<px:PXTextEdit ID="edExtDiscCode" runat="server" DataField="ExtDiscCode" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SkipDiscount" Width="75px" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="DiscountID" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="DiscountSequenceID" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="Type" RenderEditorText="True" Width="90px" />
									<px:PXGridColumn DataField="IsManual" Width="75px" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" Width="81px" CommitChanges="true" />
									<px:PXGridColumn DataField="CuryRetainedDiscountAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="DiscountPct" TextAlign="Right" Width="81px" CommitChanges="true" />
<%--									<px:PXGridColumn DataField="OrderNbr" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="ReceiptNbr" TextAlign="Right" Width="81px" />--%>
									<px:PXGridColumn DataField="ExtDiscCode" Width="110px"/>
									<px:PXGridColumn DataField="Description" Width="250px"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			
			<px:PXTabItem Text="Retainage" RepaintOnDemand="false" VisibleExp="DataControls[&quot;chkRetainageApply&quot;].Value = 1" BindingContext="form">
				<Template>
					<px:PXFormView ID="formRetainage" runat="server" Style="z-index: 100" Width="100%" DataMember="CurrentDocument" SkinID="Transparent" DataSourceID="ds" >
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edDefRetainagePct" DataField="DefRetainagePct" />
							<px:PXNumberEdit runat="server" ID="edCuryOrigDocAmtWithRetainageTotal" DataField="CuryOrigDocAmtWithRetainageTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainageTotal" DataField="CuryRetainageTotal" Enabled="false" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainageUnreleasedAmt" DataField="CuryRetainageUnreleasedAmt" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainageUnpaidTotal" DataField="CuryRetainageUnpaidTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainagePaidTotal" DataField="CuryRetainagePaidTotal" Enabled="false" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainedTaxTotal" DataField="CuryRetainedTaxTotal" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCuryRetainedDiscTotal" DataField="CuryRetainedDiscTotal" Enabled="false" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="detgrid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SkinID="DetailsInTab" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="RetainageDocuments">
								<Columns>
									<px:PXGridColumn DataField="DocType" Width="100px" Type="DropDownList" />
									<px:PXGridColumn DataField="RefNbr" Width="100px" AutoCallBack="True" LinkCommand="ViewRetainageDocument" />
									<px:PXGridColumn DataField="DocDate" Width="90px" />
									<px:PXGridColumn DataField="FinPeriodID" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="APInvoice__PayTypeID" />
									<px:PXGridColumn DataField="APInvoice__InvoiceNbr" Width="90px" />
									<px:PXGridColumn DataField="DocDesc" Width="250px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Applications" RepaintOnDemand="false" >
				<Template>
					<px:PXGrid ID="detgrid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SkinID="DetailsInTab">
						<ActionBar DefaultAction="ViewPayment">
							<CustomItems>
								<px:PXToolBarButton Text="Auto Apply" Tooltip="Auto Apply">
									<AutoCallBack Command="AutoApply" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Adjustments">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXNumberEdit CommitChanges="True" ID="edCuryAdjdAmt" runat="server" DataField="CuryAdjdAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjgBranchID" Width="100px" RenderEditorText="True" />
									<px:PXGridColumn DataField="DisplayDocType" Width="100px" Type="DropDownList" />
									<px:PXGridColumn DataField="DisplayRefNbr" Width="100px" AutoCallBack="True" LinkCommand="ViewPayment" />
									<px:PXGridColumn DataField="CuryAdjdAmt" AutoCallBack="True" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryAdjdPPDAmt" AutoCallBack="True" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="DisplayDocDate" Width="90px" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="DisplayDocDesc" Width="250px" />
									<px:PXGridColumn DataField="DisplayCuryID" Width="50px" />
									<px:PXGridColumn DataField="DisplayFinPeriodID" />
									<px:PXGridColumn DataField="APPayment__ExtRefNbr" Width="90px" />
									<px:PXGridColumn DataField="AdjdDocType" Width="18px" />
									<px:PXGridColumn DataField="AdjdRefNbr" Width="90px" />
									<px:PXGridColumn DataField="DisplayStatus" Label="Status" />
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
	<px:PXSmartPanel ID="spRetainageOptions" runat="server" Style="z-index: 108;" Caption="Release Retainage" CaptionVisible="True" Key="ReleaseRetainageOptions" AutoReload="true" LoadOnDemand="true">
		<px:PXFormView ID="frmRetainageOptions" runat="server" Style="z-index: 100;" DataMember="ReleaseRetainageOptions" CaptionVisible="False" SkinID="Transparent">
			<ContentStyle BorderWidth="0px">
			</ContentStyle>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
				<px:PXNumberEdit CommitChanges="True" ID="edRetainagePct" runat="server" AllowNull="False" DataField="RetainagePct" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryRetainageAmt" runat="server" AllowNull="False" DataField="CuryRetainageAmt" />
				<px:PXNumberEdit CommitChanges="True" ID="edCuryRetainageUnreleasedAmt" runat="server" AllowNull="False" DataField="CuryRetainageUnreleasedAmt" />
				<px:PXTextEdit CommitChanges="True" ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel9" runat="server" SkinID="Buttons">
					<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
					<px:PXButton ID="btnRelease" runat="server" DialogResult="OK" Text="Release" />
				</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPOReceipt" runat="server" Style="z-index: 108;" Width="900px" Height="400px"
		Key="poreceiptslist" AutoCallBack-Command="Refresh" AutoCallBack-Target="grid4" CommandSourceID="ds"
		Caption="Add PO Receipt" CaptionVisible="True" LoadOnDemand="True" ContentLayout-OuterSpacing="None" AutoReload="True">
		<px:PXFormView ID="frmPOrderFilter" runat="server" DataMember="filter" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="M"/>
				<px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="True" />
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="grid4" runat="server" Width="100%" Height="200px" BatchUpdate="True" SkinID="Inquire"
			DataSourceID="ds" FastFilterFields="ReceiptNbr" TabIndex="16900">
			<Levels>
				<px:PXGridLevel DataMember="poreceiptslist" DataKeyNames="ReceiptType,ReceiptNbr">
					<Columns>
						<px:PXGridColumn AllowCheckAll="True" AllowMove="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="26px" />
						<px:PXGridColumn DataField="ReceiptNbr" Width="120px" />
						<px:PXGridColumn DataField="ReceiptType" />
						<px:PXGridColumn DataField="VendorID" Width="100px" />
						<px:PXGridColumn DataField="VendorLocationID" Width="80px" />
						<px:PXGridColumn DataField="ReceiptDate" Width="90px" />
						<px:PXGridColumn DataField="OrderQty" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="UnbilledQty" TextAlign="Right" Width="100px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Add & Close" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPOReceiptLine" runat="server" Caption="Add Receipt Line" CaptionVisible="True"
		Height="400px" Width="900px" Key="poReceiptLinesSelection" LoadOnDemand="True" AutoCallBack-Command="Refresh" AutoCallBack-Target="gridOL" AutoReload="True">
		<px:PXFormView ID="frmPOFilter" runat="server" DataMember="filter" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent" >
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="M" />
				<px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="True" />
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="gridOL" runat="server" Height="200px" Width="100%" SkinID="Inquire" DataSourceID="ds"
			FastFilterFields="InvenotryID,TranDesc" TabIndex="17300">
			<Levels>
				<px:PXGridLevel DataMember="poReceiptLinesSelection">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" TextAlign="Center" Width="26px" AutoCallBack="True" CommitChanges="True" AllowResize="False" AllowShowHide="False" />
						<px:PXGridColumn DataField="PONbr" Width="100px" />
						<px:PXGridColumn DataField="POType" Width="70px" />
						<px:PXGridColumn DataField="ReceiptNbr" Width="100px" />
						<px:PXGridColumn DataField="POReceipt__InvoiceNbr" Width="80px" />
						<px:PXGridColumn DataField="InventoryID" Width="100px" />
						<px:PXGridColumn DataField="SubItemID" Width="70px" />
						<px:PXGridColumn DataField="SiteID" Width="100px" />
						<px:PXGridColumn DataField="UOM" />
						<px:PXGridColumn DataField="LineNbr" Width="70px" />
						<px:PXGridColumn DataField="CuryID" Width="70px" />
						<px:PXGridColumn DataField="ReceiptQty" Width="50px" />
						<px:PXGridColumn DataField="CuryExtCost" Width="95px" />
						<px:PXGridColumn DataField="UnbilledQty" Width="50px" />
						<px:PXGridColumn DataField="TranDesc" Width="200px" />
						<px:PXGridColumn DataField="VendorID" Width="100px"/>
						<px:PXGridColumn DataField="POReceipt__VendorLocationID" Width="100px"/>
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton3" runat="server" Text="Add" SyncVisible="false">
				<AutoCallBack Command="AddReceiptLine2" Target="ds" />
			</px:PXButton>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="OK" Text="Add & Close" />
			<px:PXButton ID="PXButton5" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPOOrder" runat="server" Width="800px" Height="400px" Key="poorderslist" CommandSourceID="ds"
		Caption="Add PO Order" CaptionVisible="True" LoadOnDemand="True" AutoCallBack-Command="Refresh" AutoCallBack-Target="PXGrid1">
		<px:PXGrid ID="PXGrid1" runat="server" Height="200px" Width="100%" BatchUpdate="True" SkinID="Inquire"
			DataSourceID="ds" FastFilterFields="OrderNbr" TabIndex="17500">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="poorderslist">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" TextAlign="Center" AllowResize="False" Width="26px" />
						<px:PXGridColumn DataField="OrderNbr" Width="100px" />
						<px:PXGridColumn DataField="OrderType" />
						<px:PXGridColumn DataField="VendorID" Width="100px" />
						<px:PXGridColumn DataField="VendorLocationID" Width="80px" />
						<px:PXGridColumn DataField="OrderDate" Width="90px" />
						<px:PXGridColumn DataField="CuryID" Width="60px" />
						<px:PXGridColumn DataField="CuryOrderTotal" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="UnbilledOrderQty" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="CuryUnbilledOrderTotal" TextAlign="Right" Width="81px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton6" runat="server" Text="Add" SyncVisible="false">
				<AutoCallBack Command="AddPOOrder2" Target="ds" />
			</px:PXButton>
			<px:PXButton ID="PXButton7" runat="server" DialogResult="OK" Text="Add & Close" />
			<px:PXButton ID="PXButton8" runat="server" DialogResult="No" Text="Cancel" SyncVisible="false" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPOOrderLine" runat="server" Width="1000px" Height="400px" Key="poorderlineslist" CommandSourceID="ds"
					 Caption="Add PO Line" CaptionVisible="True" LoadOnDemand="True" AutoCallBack-Command="Refresh" AutoCallBack-Target="POOrderLinesGrid" AutoReload="True">
		<px:PXFormView ID="PXFormView1" runat="server" DataMember="orderfilter" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent" >
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="M" />
				<px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="True" />
				<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
				<px:PXCheckBox CommitChanges="True" ID="chkShowBilledLines" runat="server" DataField="ShowBilledLines" />
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="POOrderLinesGrid" runat="server" Height="200px" Width="100%" BatchUpdate="True" SkinID="Inquire"
				   DataSourceID="ds" FastFilterFields="OrderNbr" TabIndex="17500">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="poorderlineslist">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" TextAlign="Center" AllowResize="False" Width="26px" />
						<px:PXGridColumn DataField="OrderNbr" Width="100px" />
						<px:PXGridColumn DataField="OrderType" />
						<px:PXGridColumn DataField="VendorID" Width="100px" />
						<px:PXGridColumn DataField="VendorLocationID" Width="80px" />
						<px:PXGridColumn DataField="OrderDate" Width="90px" />
						<px:PXGridColumn DataField="InventoryID" Width="100px" />
						<px:PXGridColumn DataField="SubItemID" Width="70px" />
						<px:PXGridColumn DataField="SiteID" Width="100px" />
						<px:PXGridColumn DataField="CuryID" Width="60px" />
						<px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="UnbilledQty" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="CuryUnbilledAmt" TextAlign="Right" Width="81px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton9" runat="server" Text="Add" SyncVisible="false">
				<AutoCallBack Command="AddPOOrderLine2" Target="ds" />
			</px:PXButton>
			<px:PXButton ID="PXButton10" runat="server" DialogResult="OK" Text="Add & Close" />
			<px:PXButton ID="PXButton11" runat="server" DialogResult="No" Text="Cancel" SyncVisible="false" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="panelDuplicate" runat="server" Caption="Duplicate Reference Nbr." CaptionVisible="true" LoadOnDemand="true" Key="duplicatefilter"
		AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
		<div style="padding: 5px">
			<px:PXFormView ID="formCopyTo" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="duplicatefilter">
				<ContentStyle BackColor="Transparent" BorderStyle="None" />
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
					<px:PXLabel Size="xl" ID="lblMessage" runat="server">Record already exists. Please enter new Reference Nbr.</px:PXLabel>
					<px:PXMaskEdit CommitChanges="True" ID="edRefNbr" runat="server" DataField="RefNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
				</Template>
			</px:PXFormView>
		</div>
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton14" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds" />
			<px:PXButton ID="PXButton15" runat="server" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddLandedCost" runat="server" Width="800px" Height="400px" Key="LandedCostDetailsAdd" CommandSourceID="ds"
					 Caption="Add LC" CaptionVisible="True" LoadOnDemand="True" AutoCallBack-Command="Refresh" AutoCallBack-Target="grdLC">
		<px:PXFormView ID="frmLCFilter" runat="server" DataMember="landedCostFilter" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent" >
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="M" />
				<px:PXSelector CommitChanges="True" ID="edLandedCostDocRefNbr" runat="server" DataField="LandedCostDocRefNbr" AutoRefresh="True" />
				<px:PXSelector CommitChanges="True" ID="edLandedCostCodeID" runat="server" DataField="LandedCostCodeID" AutoRefresh="True" />
				<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
				<px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AutoRefresh="True" />
				<px:PXDropDown CommitChanges="True" ID="edPOOrderType" runat="server" DataField="OrderType" />
				<px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="True" />
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="grdLC" runat="server" Height="200px" Width="100%" BatchUpdate="True" SkinID="Details" DataSourceID="ds" FastFilterFields="RefNbr" TabIndex="17500">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="LandedCostDetailsAdd">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" TextAlign="Center" AllowResize="False" Width="36px" />
						<px:PXGridColumn DataField="DocType" Width="100px" />
						<px:PXGridColumn DataField="RefNbr" Width="120px" />
						<px:PXGridColumn DataField="VendorRefNbr" Width="120px" />
						<px:PXGridColumn DataField="LandedCostCodeID" Width="100px" />
						<px:PXGridColumn DataField="Descr" />
						<px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="CuryID" Width="60px" />
						<px:PXGridColumn DataField="TaxCategoryID" Width="100px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel ID="PXPanel8" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton16" runat="server" Text="Add" SyncVisible="false">
				<AutoCallBack Command="AddLandedCost2" Target="ds" />
			</px:PXButton>
			<px:PXButton ID="PXButton17" runat="server" DialogResult="OK" Text="Add & Close" />
			<px:PXButton ID="PXButton18" runat="server" DialogResult="No" Text="Cancel" SyncVisible="false" />
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
					<px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" CommitChanges="true" />
					<px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server" DataField="RecalcUnitPrices" />
					<px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server" DataField="OverrideManualPrices" Style="margin-left: 25px" />
					<px:PXCheckBox CommitChanges="True" ID="chkRecalcDiscounts" runat="server" DataField="RecalcDiscounts" />
					<px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" Style="margin-left: 25px" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" Style="margin-left: 25px" />
				</Template>
			</px:PXFormView>
		<px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton13" runat="server" DialogResult="OK" Text="OK" CommandName="RecalcOk" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelLinkLine" runat="server" Width="1100px" Height="500px" Key="linkLineFilter" CommandSourceID="ds" Caption="Link Line" CaptionVisible="True" LoadOnDemand="False" AutoCallBack-Command="Refresh" AutoCallBack-Target="LinkLineFilterForm">
		<px:PXFormView runat="server" ID="LinkLineFilterForm" DataMember="linkLineFilter" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule02" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSelector runat="server" ID="edPOOrderNbr" DataField="POOrderNbr" CommitChanges="True" AutoRefresh="True" />
				<px:PXSelector runat="server" ID="edSiteID" DataField="SiteID" CommitChanges="True" AutoRefresh="True" />
				<px:PXSelector runat="server" ID="edInventoryID" DataField="InventoryID" />
				<px:PXSelector runat="server" ID="edUOM" DataField="UOM" />
				<px:PXLayoutRule ID="PXLayoutRule01" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
				<px:PXGroupBox CommitChanges="True" RenderStyle="RoundBorder" ID="gpMode" runat="server" Caption="Selected Mode" DataField="SelectedMode">
					<Template>
						<px:PXRadioButton runat="server" ID="rOrder" Value="O" />
						<px:PXRadioButton runat="server" ID="rReceipt" Value="R" />
						<px:PXRadioButton runat="server" ID="rLandedCost" Value="L" />
					</Template>
				</px:PXGroupBox>
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="LinkLineGrid" runat="server" Height="200px" Width="100%" BatchUpdate="False" SkinID="Inquire" DataSourceID="ds" FastFilterFields="PONbr,ReceiptNbr,POReceipt__InvoiceNbr,TranDesc,SiteID" TabIndex="17500" FilesIndicator="False" NoteIndicator="False"
			Caption="Receipt" CaptionVisible="false" AdjustPageSize="Auto">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="linkLineReceiptTran">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="False" AllowSort="False" AllowMove="False" TextAlign="Center" Width="20px" AutoCallBack="True" CommitChanges="True" AllowResize="False" AllowShowHide="False" />
						<px:PXGridColumn DataField="PONbr" Width="100px" />
						<px:PXGridColumn DataField="POType" Width="70px" />
						<px:PXGridColumn DataField="ReceiptNbr" Width="100px" />
						<px:PXGridColumn DataField="POReceipt__InvoiceNbr" Width="80px" />
						<px:PXGridColumn DataField="SubItemID" Width="70px" />
						<px:PXGridColumn DataField="SiteID" Width="100px" />
						<px:PXGridColumn DataField="LineNbr" Width="70px" />
						<px:PXGridColumn DataField="CuryID" Width="70px" />
						<px:PXGridColumn DataField="ReceiptQty" Width="50px" />
						<px:PXGridColumn DataField="CuryExtCost" Width="95px" />
						<px:PXGridColumn DataField="UnbilledQty" Width="50px" />
						<px:PXGridColumn DataField="TranDesc" Width="200px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowAddNew="False" AllowDelete="False" />
		</px:PXGrid>
		<px:PXGrid ID="LinkLineOrderGrid" runat="server" Height="200px" Width="100%" BatchUpdate="False" SkinID="Inquire" DataSourceID="ds" FastFilterFields="POOrder__OrderNbr,POOrder__VendorRefNbr,TranDesc,SiteID" TabIndex="17500" FilesIndicator="False" NoteIndicator="False"
			Caption="Order" CaptionVisible="false" AdjustPageSize="Auto">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="linkLineOrderTran">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="False" AllowSort="False" AllowMove="False" TextAlign="Center" Width="20px" AutoCallBack="True" CommitChanges="True" AllowResize="False" AllowShowHide="False" />
						<px:PXGridColumn DataField="POOrder__OrderNbr" Width="100px" />
						<px:PXGridColumn DataField="POOrder__OrderType" Width="100px" />
						<px:PXGridColumn DataField="LineNbr" Width="100px" />
						<px:PXGridColumn DataField="POOrder__VendorRefNbr" Width="80px" />
						<px:PXGridColumn DataField="SubItemID" Width="70px" />
						<px:PXGridColumn DataField="SiteID" Width="100px" />
						<px:PXGridColumn DataField="POOrder__CuryID" Width="70px" />
						<px:PXGridColumn DataField="OrderQty" Width="50px" />
						<px:PXGridColumn DataField="curyLineAmt" Width="95px" />
						<px:PXGridColumn DataField="UnbilledQty" Width="50px" />
						<px:PXGridColumn DataField="CuryUnbilledAmt" Width="95px" />
						<px:PXGridColumn DataField="TranDesc" Width="200px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowAddNew="False" AllowDelete="False" />
		</px:PXGrid>
		<px:PXGrid ID="LinkLineLandedCostGrid" runat="server" Height="200px" Width="100%" BatchUpdate="False" SkinID="Inquire" DataSourceID="ds" FastFilterFields="POLandedCostDoc__RefNbr,POLandedCostDoc__VendorRefNbr,Descr" TabIndex="17500" FilesIndicator="False" NoteIndicator="False"
			Caption="Landed Cost" CaptionVisible="false" AdjustPageSize="Auto">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="LinkLineLandedCostDetail">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="False" AllowSort="False" AllowMove="False" TextAlign="Center" Width="24px" AutoCallBack="True" CommitChanges="True" AllowResize="False" AllowShowHide="False" />
						<px:PXGridColumn DataField="DocType" Width="100px" />
						<px:PXGridColumn DataField="RefNbr" Width="100px" />
						<px:PXGridColumn DataField="LineNbr" Width="40px" />
						<px:PXGridColumn DataField="VendorRefNbr" Width="80px" />
						<px:PXGridColumn DataField="CuryLineAmt" Width="95px" />
						<px:PXGridColumn DataField="Descr" Width="200px" />
						<px:PXGridColumn DataField="INDocType" Width="200px" />
						<px:PXGridColumn DataField="INRefNbr" Width="200px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowAddNew="False" AllowDelete="False" />
		</px:PXGrid>
		<px:PXPanel ID="LinkLineButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="LinkLineSave" runat="server" DialogResult="Yes" Text="Save" />
			<px:PXButton ID="LinkLineCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
