<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PO101000.aspx.cs" Inherits="Page_PO101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Setup"
		TypeName="PX.Objects.PO.POSetupMaint" BorderStyle="NotSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="487px" Style="z-index: 100"
		Width="100%" DataMember="Setup" Caption="General Settings" 
		DefaultControlID="edStandardPONumberingID">
		<autosize container="Window" enabled="True" minheight="200" />
<Activity Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="General Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Purchase Order Numbering Settings" />

					<px:PXSelector ID="edStandardPONumberingID" runat="server" AllowNull="False" DataField="StandardPONumberingID" Text="POORDERSTD" AllowEdit="True" />
					<px:PXSelector ID="edRegularPONumberingID" runat="server" AllowNull="False" DataField="RegularPONumberingID" Text="POORDERREG" AllowEdit="True" />
					<px:PXSelector ID="edReceiptNumberingID" runat="server" AllowNull="False" DataField="ReceiptNumberingID" Text="PORECEIPT" AllowEdit="True" />
					<px:PXSelector ID="edLandedCostDocNumberingID" runat="server" AllowNull="False" DataField="LandedCostDocNumberingID" Text="POLANDCOST" AllowEdit="True" />

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Validate  Total  on Entry" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireReceiptControlTotal" runat="server" DataField="RequireReceiptControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireOrderControlTotal" runat="server" DataField="RequireOrderControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireBlanketControlTotal" runat="server" DataField="RequireBlanketControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireDropShipControlTotal" runat="server" DataField="RequireDropShipControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireLandedCostsControlTotal" runat="server" DataField="RequireLandedCostsControlTotal" />

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Purchase Price Variance Allocation" />
					<px:PXDropDown ID="edPPVAllocationMode" runat="server" AllowNull="False" DataField="PPVAllocationMode"  CommitChanges="true" />
					<px:PXSelector ID="edPPVReasonCodeID" runat="server" DataField="PPVReasonCodeID" AllowEdit="True" />
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Other" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoCreateInvoiceOnReceipt" runat="server" DataField="AutoCreateInvoiceOnReceipt" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoCreateLCAP" runat="server" DataField="AutoCreateLCAP" />
					<px:PXSegmentMask CommitChanges="True" ID="edFreightExpenseAcctID" runat="server" DataField="FreightExpenseAcctID" />
					<px:PXSegmentMask ID="edFreightExpenseSubID" runat="server" DataField="FreightExpenseSubID" AutoRefresh="True" />
					<px:PXSelector ID="edRCReturnReasonCodeID" runat="server" DataField="RCReturnReasonCodeID" AllowEdit="True" />
					<px:PXSelector ID="edTaxReasonCodeID" runat="server" DataField="TaxReasonCodeID" AllowEdit="True" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkAutoReleaseIN" runat="server" DataField="AutoReleaseIN" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoReleaseLCIN" runat="server" DataField="AutoReleaseLCIN" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoReleaseAP" runat="server" DataField="AutoReleaseAP" />
					<px:PXCheckBox SuppressLabel="True" ID="chkHoldReceipts" runat="server" Checked="True" DataField="HoldReceipts" />
					<px:PXCheckBox SuppressLabel="True" ID="chkHoldLandedCosts" runat="server" Checked="True" DataField="HoldLandedCosts" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAddServicesFromNormalPO" runat="server" DataField="AddServicesFromNormalPOtoPR" CommitChanges="true" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAddServicesFromDSPO" runat="server" DataField="AddServicesFromDSPOtoPR" CommitChanges="true" />
					<px:PXCheckBox SuppressLabel="True" ID="chkUpdateSubOnOwnerChange" runat="server" DataField="UpdateSubOnOwnerChange" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkCopyLineDescrSO" runat="server" DataField="CopyLineDescrSO" />
					<px:PXCheckBox ID="chkCopyLineNoteSO" runat="server" DataField="CopyLineNoteSO" />
					<px:PXCheckBox ID="chkCopyLineNotesFromServiceOrder" runat="server" DataField="CopyLineNotesFromServiceOrder" />
					<px:PXCheckBox ID="chkCopyLineAttachmentsFromServiceOrder" runat="server" DataField="CopyLineAttachmentsFromServiceOrder" />
					<px:PXCheckBox ID="chkAutoAddLineReceiptBarcode" runat="server" DataField="AutoAddLineReceiptBarcode" />
					<px:PXCheckBox ID="chkReceiptByOneBarcodeReceiptBarcode" runat="server" DataField="ReceiptByOneBarcodeReceiptBarcode" />
					<px:PXCheckBox ID="chkReturnOrigCost" runat="server" DataField="ReturnOrigCost" />
					<px:PXCheckBox ID="chkChangeCuryRateOnReceipt" runat="server" DataField="ChangeCuryRateOnReceipt" />
					<px:PXCheckBox ID="chkCopyLineNotesToReceipt" runat="server" DataField="CopyLineNotesToReceipt" />
					<px:PXCheckBox ID="chkCopyLineFilesToReceipt" runat="server" DataField="CopyLineFilesToReceipt" />
					<px:PXSelector ID="edDefaultReceiptAssignmentMapID" runat="server" AllowEdit="True" DataField="DefaultReceiptAssignmentMapID" TextField="Name" />
					<px:PXDropDown ID="edShipDestType" runat="server" AllowNull="False" DataField="ShipDestType"  />
					<px:PXDropDown ID="edDefaultReceiptQty" runat="server" AllowNull="False" DataField="DefaultReceiptQty"  />
					</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Approval">				
				<Template>
					<px:PXPanel runat="server">
						<px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM" />
						<px:PXCheckBox ID="chkOrderRequestApproval" runat="server" AlignLeft="true" Checked="True" DataField="OrderRequestApproval" />				        
					</px:PXPanel>
					<px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" >
						<AutoSize Enabled="true" Container="Parent" MinHeight="200"/>
						<Levels>
							<px:PXGridLevel DataMember="SetupApproval" DataKeyNames="ApprovalID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

									<px:PXDropDown ID="edOrderType" runat="server" DataField="OrderType"  />
									<px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" AllowEdit="True" />
									<px:PXSelector ID="edAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" AllowEdit="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="OrderType" RenderEditorText="True" Width="100px" />
									<px:PXGridColumn DataField="AssignmentMapID" Width="250px" RenderEditorText="True" TextField="AssignmentMapID_EPAssignmentMap_Name" />
									<px:PXGridColumn DataField="AssignmentNotificationID" Width="250px" RenderEditorText="True"  />
								</Columns>
							</px:PXGridLevel>
						</Levels>                        
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Reporting Settings">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px">
						<AutoSize Enabled="true" />
						<Template1>
							<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Default Sources"
								AdjustPageSize="Auto" AllowPaging="True">
								<AutoCallBack Target="gridNR" Command="Refresh" />
								<Levels>
									<px:PXGridLevel DataMember="Notifications" DataKeyNames="Module,SourceCD,NotificationCD,NBranchID">
										<RowTemplate>
											<px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
											<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
											<px:PXSelector ID="edNBranchID" runat="server" DataField="NBranchID" />
											<px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
											<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
											<px:PXSelector ID="edDefPrinterID" runat="server" DataField="DefaultPrinterID" />
											<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
											<px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="NotificationCD" Width="120px" />
											<px:PXGridColumn DataField="NBranchID" Width="120px" />
											<px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
											<px:PXGridColumn DataField="DefaultPrinterID" Width="120px" />
											<px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" Width="150px" AutoCallBack="True" />
											<px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="True" />
											<px:PXGridColumn AllowNull="False" DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
											<px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" Caption="Default Recipients" AdjustPageSize="Auto"
								AllowPaging="True" Style="left: 0px; top: 0px">
								<Parameters>
									<px:PXSyncGridParam ControlID="gridNS" />
								</Parameters>
								<CallbackCommands>
									<Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds" />
									<FetchRow RepaintControls="None" />
								</CallbackCommands>
								<Levels>
									<px:PXGridLevel DataMember="Recipients" DataKeyNames="RecipientID">
										<Columns>
											<px:PXGridColumn DataField="ContactType" RenderEditorText="True" Width="100px" AutoCallBack="True" />
											<px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
											<px:PXGridColumn DataField="ContactID" Width="120px">
												<NavigateParams>
													<px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
												</NavigateParams>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
											<px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
										</Columns>
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName" AllowEdit="True">
												<Parameters>
													<px:PXSyncGridParam ControlID="gridNR" />
												</Parameters>
											</px:PXSelector>
										</RowTemplate>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" MinHeight="150" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Warehouse Management">
				<Template>
					<px:PXFormView ID="formScanSetup" runat="server" DataSourceID="ds" Width="100%" DataMember="ReceivePutAwaySetup" DefaultControlID="edShowReceivingTab" RenderStyle="Simple">
						<Template>
							<px:PXLabel ID="lblScanSetup" runat="server" Height="30px">These settings are specific to the current branch.</px:PXLabel>
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Receiving Workflow" />
							<px:PXCheckBox ID="edShowReceivingTab" runat="server" DataField="ShowReceivingTab" CommitChanges="true" />
							<px:PXCheckBox ID="edShowPutAwayTab" runat="server" DataField="ShowPutAwayTab" CommitChanges="true" />
							<px:PXCheckBox ID="edShowScanLogTab" runat="server" DataField="ShowScanLogTab" />

							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Receiving Settings"  SuppressLabel="True"/>
							<px:PXCheckBox ID="edUseDefaultQty" runat="server" DataField="UseDefaultQty" CommitChanges="true" />
							<px:PXCheckBox ID="edExplicitLineConfirmation" runat="server" DataField="ExplicitLineConfirmation" />
							<px:PXCheckBox ID="edUseCartsForPutAway" runat="server" DataField="UseCartsForPutAway" />

							<px:PXCheckBox ID="edDefaultLotSerialNumber" runat="server" DataField="DefaultLotSerialNumber" CommitChanges="true" />
							<px:PXCheckBox ID="edDefaultExpireDate" runat="server" DataField="DefaultExpireDate" CommitChanges="true" />
							<px:PXCheckBox ID="edSingleLocation" runat="server" DataField="SingleLocation" CommitChanges="true" />
							<px:PXCheckBox ID="edDefaultReceivingLocation" runat="server" DataField="DefaultReceivingLocation" CommitChanges="true" />
							<px:PXCheckBox ID="edRequestLocationForEachItemInReceive" runat="server" DataField="RequestLocationForEachItemInReceive" />
							<px:PXCheckBox ID="edRequestLocationForEachItemInPutAway" runat="server" DataField="RequestLocationForEachItemInPutAway" />

							<px:PXCheckBox SuppressLabel="True" ID="edPrintInventoryLabelsAutomatically" runat="server" DataField="PrintInventoryLabelsAutomatically" CommitChanges="true" />
							<px:PXSelector ID="edInventoryLabelsReportID" runat="server" DataField="InventoryLabelsReportID" ValueField="ScreenID" />
							<px:PXCheckBox ID="edPrintPurchaseReceiptAutomatically" runat="server" DataField="PrintPurchaseReceiptAutomatically" CommitChanges="true" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
</asp:Content>