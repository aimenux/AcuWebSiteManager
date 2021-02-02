<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP101000.aspx.cs"
	Inherits="Page_AP101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APSetupMaint" PrimaryView="Setup">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand DependOnGrid="gridApproval" Name="ViewAssignmentMap" Visible="False"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="540px" Style="z-index: 100" Width="100%" DataMember="Setup" DefaultControlID="edBatchNumberingID" TabIndex="100">
		<Items>
			<px:PXTabItem Text="General Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" />
					<px:PXSelector ID="edBatchNumberingID" runat="server" DataField="BatchNumberingID" AllowEdit="True" />
					<px:PXSelector ID="edInvoiceNumberingID" runat="server" DataField="InvoiceNumberingID" AllowEdit="True" />
					<px:PXSelector ID="edDebitAdjNumberingID" runat="server" DataField="DebitAdjNumberingID" AllowEdit="True" />
					<px:PXSelector ID="edCreditAdjNumberingID" runat="server" DataField="CreditAdjNumberingID" AllowEdit="True" />
					<px:PXSelector ID="edCheckNumberingID" runat="server" DataField="CheckNumberingID" AllowEdit="True" />
					<px:PXSelector ID="edPriceWSNumberingID" runat="server" DataField="PriceWSNumberingID" AllowEdit="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Posting Settings" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoPost" runat="server" DataField="AutoPost" AlignLeft="True" Size="Empty" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSummaryPost" runat="server" DataField="SummaryPost" AlignLeft="True" Size="Empty" />
					<px:PXCheckBox SuppressLabel="True" ID="chkMigrationMode" runat="server" DataField="MigrationMode" AlignLeft="True" Size="Empty" CommitChanges="true" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Aging Settings" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit Size="xxs" ID="edPastDue00" runat="server" DataField="PastDue00" />
					<px:PXLabel Size="xs" ID="lblDays1" runat="server">Days</px:PXLabel>
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit Size="xxs" ID="edPastDue01" runat="server" DataField="PastDue01" />
					<px:PXLabel Size="xs" ID="lblDays2" runat="server">Days</px:PXLabel>
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit Size="xxs" ID="edPastDue02" runat="server" DataField="PastDue02" />
					<px:PXLabel Size="xs" ID="lblDays3" runat="server">Days</px:PXLabel>
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat='server' ControlSize="M" LabelsWidth="M" StartColumn="True" />
					<px:PXLayoutRule runat="server" GroupCaption="Data Entry Settings" StartGroup="True" />
					<px:PXSelector ID="edDfltVendorClassID" runat="server" DataField="DfltVendorClassID" AllowEdit="True" CommitChanges="true" />
					<px:PXSegmentMask ID="edExpenseSubMask" runat="server" DataField="ExpenseSubMask" />
					<px:PXDropDown ID="edInvoiceRounding" runat="server" DataField="InvoiceRounding" CommitChanges="true" />
					<px:PXDropDown ID="edInvoicePrecision" runat="server" DataField="InvoicePrecision" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit Size="xxs" ID="edPaymentLeadTime" runat="server" AllowNull="False" DataField="PaymentLeadTime" />
					<px:PXLabel Size="xs" ID="lblDays4" runat="server">Days</px:PXLabel>
					<px:PXLayoutRule runat="server" />
					<px:PXCheckBox ID="chkHoldEntry" runat="server" DataField="HoldEntry" CommitChanges="true" />
					<px:PXCheckBox ID="chkAutoPreapprovePayments" runat="server" DataField="RequireApprovePayments" />
					<px:PXCheckBox ID="chkEarlyChecks" runat="server" DataField="EarlyChecks" />
					<px:PXCheckBox ID="chkRequireControlTotal" runat="server" DataField="RequireControlTotal" />
					<px:PXCheckBox ID="chkRequireControlTaxTotal" runat="server" DataField="RequireControlTaxTotal" />
                    <px:PXCheckBox ID="chkSuggestPaymentAmount" runat="server" DataField="SuggestPaymentAmount" />
					<px:PXCheckBox ID="chkRequireVendorRef" runat="server" DataField="RequireVendorRef" />
					<px:PXCheckBox ID="chkRaiseErrorOnDoubleInvoiceNbr" runat="server" DataField="RaiseErrorOnDoubleInvoiceNbr" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Retainage Settings" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRetainTaxes" runat="server" DataField="RetainTaxes" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRetainageBillsAutoRelease" runat="server" DataField="RetainageBillsAutoRelease" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireSingleProjectPerDocument" runat="server" DataField="RequireSingleProjectPerDocument" />
					<px:PXLayoutRule runat="server" GroupCaption="VAT Recalculation Settings" StartGroup="True" />
					<px:PXTextEdit ID="txtPPDDebitAdjustmentDescr" runat="server" DataField="PPDDebitAdjustmentDescr" />

				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Price/Discount Settings">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Price Maintenance" StartGroup="True" />
                    <px:PXDropDown ID="edVendorPriceUpdate" runat="server" AllowNull="False" DataField="VendorPriceUpdate"  />
					<px:PXCheckBox ID="chkLoadVendorsPricesUsingAlternateID" runat="server" DataField="LoadVendorsPricesUsingAlternateID" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Price Retention" StartGroup="True" />
					<px:PXDropDown ID="edRetentionType" runat="server" DataField="RetentionType" CommitChanges="true" />
					<px:PXNumberEdit ID="edNumberOfMonths" runat="server" AllowNull="False" DataField="NumberOfMonths" CommitChanges="true" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Discount Application" StartGroup="True" />
					<px:PXDropDown ID="edApplyQuantityDiscountBy" runat="server" AllowNull="False" DataField="ApplyQuantityDiscountBy" />
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Approval">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" SyncPosition="true" FeedbackMode="ForceDataEntry">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="SetupApproval" >
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" />
                                    <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" CommitChanges="True" />
                                    <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" AutoRefresh="True" AllowEdit="true"/>
                                    <px:PXSelector ID="edAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" AutoRefresh="True" AllowEdit="true" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="DocType" CommitChanges="True" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  TextField="AssignmentMapID_EPAssignmentMap_Name" AutoCallBack="True" LinkCommand="ViewAssignmentMap"/>
                                    <px:PXGridColumn DataField="AssignmentNotificationID" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>                         
                    </px:PXGrid>
                    </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="1099 Settings">
				<Template>
					<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowSearch="true" DataSourceID="ds"
						SkinID="DetailsInTab" TabIndex="200">
						<Levels>
							<px:PXGridLevel DataMember="Boxes1099">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXNumberEdit ID="edBoxNbr" runat="server" DataField="BoxNbr" />
									<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
									<px:PXNumberEdit ID="edMinReportAmt" runat="server" DataField="MinReportAmt" />
									<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" AllowEdit="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BoxNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="Descr" />
									<px:PXGridColumn DataField="MinReportAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="AccountID" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Reporting Settings">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px">
						<AutoSize Enabled="true" />
						<Template1>
							<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="200px" Caption="Default Sources" AdjustPageSize="Auto" AllowPaging="True" TabIndex="300">
								<AutoCallBack Target="gridNR" Command="Refresh" />
								<Levels>
									<px:PXGridLevel DataMember="Notifications">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
											<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXSelector ID="edNBranchID" runat="server" DataField="NBranchID" />
											<px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
											<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                            <px:PXSelector ID="edDefPrinterID" runat="server" DataField="DefaultPrinterID" />
											<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="NotificationCD" />
                                            <px:PXGridColumn DataField="NBranchID" />
											<px:PXGridColumn DataField="EMailAccountID" Label="Default Email Account" DisplayMode="Text" />
                                            <px:PXGridColumn DataField="DefaultPrinterID" />
											<px:PXGridColumn DataField="ReportID" AutoCallBack="true" />
											<px:PXGridColumn DataField="NotificationID" AutoCallBack="true" />
											<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="true" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="true" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" Caption="Default Recipients" AdjustPageSize="Auto" AllowPaging="True" TabIndex="400">
								<Parameters>
									<px:PXSyncGridParam ControlID="gridNS" />
								</Parameters>
								<CallbackCommands>
									<Save CommitChanges="true" CommitChangesIDs="gridNR" RepaintControls="None" />
									<FetchRow RepaintControls="None" />
								</CallbackCommands>
								<Levels>
									<px:PXGridLevel DataMember="Recipients">
										<Columns>
											<px:PXGridColumn DataField="ContactType" RenderEditorText="True" AutoCallBack="true" />
											<px:PXGridColumn DataField="OriginalContactID" Visible="false" AllowShowHide="false" />
											<px:PXGridColumn DataField="ContactID" Width="100px">
												<NavigateParams>
													<px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
												</NavigateParams>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="true" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
											<px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center" Type="CheckBox" />
										</Columns>
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="true" ValueField="DisplayName" AllowEdit="True">
												<Parameters>
													<px:PXSyncGridParam ControlID="gridNR" />
												</Parameters>
											</px:PXSelector>
										</RowTemplate>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="true" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="300" MinWidth="100" />
	</px:PXTab>
</asp:Content>
