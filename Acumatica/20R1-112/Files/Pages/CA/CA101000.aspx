<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA101000.aspx.cs" Inherits="Page_CA101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="CASetupRecord" TypeName="PX.Objects.CA.CASetupMaint">
		<CallbackCommands>		
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="669px" Width="100%" DataMember="CASetupRecord"
		DefaultControlID="edBatchNumberingID" TabIndex="100" DataSourceID="ds">
		<AutoSize Container="Window" Enabled="True" />
		<Items>
			<px:PXTabItem Text="General Settings">
		
			<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" ControlSize="m" />
			<px:PXLayoutRule runat="server" GroupCaption="Numbering Settings" StartGroup="True" />
			<px:PXSelector ID="edBatchNumberingID" runat="server" DataField="BatchNumberingID" AllowEdit="True" DataSourceID="ds" />
			<px:PXSelector ID="edRegisterNumberingID" runat="server" DataField="RegisterNumberingID" AllowEdit="True" DataSourceID="ds" />
			<px:PXSelector ID="edTransferNumberingID" runat="server" DataField="TransferNumberingID" AllowEdit="True" DataSourceID="ds" />
			<px:PXSelector ID="edCABatchNumberingID" runat="server" AllowEdit="True" AllowNull="False" DataField="CABatchNumberingID" DataSourceID="ds" />
			<px:PXSelector ID="edCAStatementNumberingID" runat="server" AllowNull="False" AllowEdit="True" DataField="CAStatementNumberingID" DataSourceID="ds" />
			<px:PXSelector ID="edCAImportPaymentsNumberingID" runat="server" AllowEdit="True" DataField="CAImportPaymentsNumberingID" DataSourceID="ds" />
			<px:PXSelector runat="server" ID="CstPXSelector1" DataField="CorpCardNumberingID" AllowEdit="True" />
			<px:PXLayoutRule runat="server" GroupCaption="Reconciliation Settings" StartGroup="True" />
			<px:PXSelector ID="edUnknownPaymentEntryTypeID" runat="server" DataField="UnknownPaymentEntryTypeID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edTransitAcctId" runat="server" DataField="TransitAcctId" DataSourceID="ds" />
			<px:PXSegmentMask ID="edTransitSubID" runat="server" DataField="TransitSubID" DataSourceID="ds" />
            <px:PXCheckBox ID="chkSkipVoided" SuppressLabel="True" runat="server" DataField="SkipVoided" />
			<px:PXLayoutRule runat="server" GroupCaption="Receipts to Add to Available Balances" />
			<px:PXCheckBox ID="chkCalcBalDebitUnclearedUnreleased" runat="server" DataField="CalcBalDebitUnclearedUnreleased" />
			<px:PXCheckBox ID="chkCalcBalDebitClearedUnreleased" runat="server" DataField="CalcBalDebitClearedUnreleased" />
			<px:PXCheckBox ID="chkCalcBalDebitUnclearedReleased" runat="server" DataField="CalcBalDebitUnclearedReleased" />
			<px:PXLayoutRule runat="server" GroupCaption="Disbursements to Deduct From Available Balances" />
			<px:PXCheckBox ID="chkCalcBalCreditUnclearedUnreleased" runat="server" DataField="CalcBalCreditUnclearedUnreleased" />
			<px:PXCheckBox ID="chkCalcBalCreditClearedUnreleased" runat="server" DataField="CalcBalCreditClearedUnreleased" />
			<px:PXCheckBox ID="chkCalcBalCreditUnclearedReleased" runat="server" DataField="CalcBalCreditUnclearedReleased" />
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" StartColumn="True" StartGroup="True" />
			<px:PXLayoutRule runat="server" GroupCaption="Posting and Release Settings" />
			<px:PXCheckBox ID="chkAutoPostOption" runat="server" DataField="AutoPostOption" />
			<px:PXCheckBox ID="chkReleaseAP" runat="server" Checked="True" DataField="ReleaseAP" />
			<px:PXCheckBox ID="chkReleaseAR" runat="server" Checked="True" DataField="ReleaseAR" />

			<px:PXLayoutRule runat="server" GroupCaption="Data Entry Settings" StartGroup="True" />
			<px:PXCheckBox ID="chkHoldEntry" runat="server" Checked="True" DataField="HoldEntry" />
			<px:PXCheckBox ID="chkRequireControlTotal" runat="server" DataField="RequireControlTotal" />
            <px:PXCheckBox ID="edRequireControlTaxTotal" runat="server" DataField="RequireControlTaxTotal" />
            <px:PXCheckBox ID="chkRequireExtRefNbr" runat="server" DataField="RequireExtRefNbr" />
            <px:PXCheckBox ID="chkValidateDataConsistencyOnRelease" runat="server" DataField="ValidateDataConsistencyOnRelease" />
			<px:PXDropDown ID="edDateRangeDefault" runat="server" DataField="DateRangeDefault" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Cash Transactions Approval Settings" />
			<px:PXCheckBox SuppressLabel="True" ID="chkRequestApproval" runat="server" Checked="True" DataField="RequestApproval" CommitChanges="True" />
			<px:PXGrid ID="gridApproval" runat="server" Width="450px" Height="200px" TabIndex="11900" DataSourceID="ds" FeedbackMode="ForceDataEntry">
				<Levels>
					<px:PXGridLevel DataMember="Approval">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="l" />
							<px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" AllowEdit="True" />
                            <px:PXSelector ID="edAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" AllowEdit="True" />
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="AssignmentMapID" RenderEditorText="True" TextField="AssignmentMapID_EPAssignmentMap_Name" />
                            <px:PXGridColumn DataField="AssignmentNotificationID" RenderEditorText="True" />
						</Columns>
						<Layout FormViewHeight="" />
					</px:PXGridLevel>
				</Levels>
			</px:PXGrid>
		</Template>
		</px:PXTabItem>
		<px:PXTabItem Text="Bank Statement Settings">
			<Template>
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="XM" StartGroup="True" GroupCaption="Disbursement Matching"/>
				<px:PXNumberEdit ID="edDisbursementTranDaysBefore" DataField="DisbursementTranDaysBefore"  runat="server" />
				<px:PXNumberEdit ID="edDisbursementTranDaysAfter" DataField="DisbursementTranDaysAfter" runat="server" />
				<px:PXCheckBox ID="chkAllowMatchingCreditMemo" SuppressLabel="True" LabelsWidth="XXS" ControlSize="M" runat="server" DataField="AllowMatchingCreditMemo" />
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="XM" StartGroup="True" GroupCaption="Receipt Matching"/>
				<px:PXNumberEdit ID="edReceiptTranDaysBefore" DataField="ReceiptTranDaysBefore"  runat="server" />
				<px:PXNumberEdit ID="edReceiptTranDaysAfter" DataField="ReceiptTranDaysAfter"  runat="server" />

				<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Weights for Relevance Calculation"/>
				<px:PXPanel runat="server" Border="none"   RenderStyle="Simple" 
					ID="pnlRelativeWeights" RenderSimple="True"> 
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="xs" StartColumn="True"/>
				<px:PXNumberEdit ID="edRefNbrCompareWeight" DataField="RefNbrCompareWeight" CommitChanges="True" runat="server" />
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="xs" ColumnSpan="2"/>
				<px:PXCheckBox ID="chkEmptyRefNbrMatching" runat="server" DataField="EmptyRefNbrMatching" CommitChanges="True"/>
				<px:PXNumberEdit ID="edDateCompareWeight" DataField="DateCompareWeight" CommitChanges="True" runat="server" />
				<px:PXNumberEdit ID="edPayeeCompareWeight" DataField="PayeeCompareWeight" CommitChanges="True" runat="server" />			
				</px:PXPanel>
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="XM" StartGroup="True" GroupCaption="Date Range for Relevance Calculation"/>
				<px:PXNumberEdit ID="edDateMeanOffset" DataField="DateMeanOffset"  runat="server" />
				<px:PXNumberEdit ID="edDateSigma" DataField="DateSigma"  runat="server" />              
			    <px:PXLayoutRule  runat="server" ID ="PXLayoutRule25" StartGroup="True" ControlSize="XS" LabelsWidth="L" GroupCaption="Expense Receipt Matching" />
			    <px:PXNumberEdit  runat="server" ID="edCuryDiffThreshold" DataField="CuryDiffThreshold" />
			    <px:PXNumberEdit  runat="server" ID="edAmountWeight" DataField="AmountWeight" />                      
				<px:PXLayoutRule runat="server" LabelsWidth="XXS" ControlSize="M" StartGroup="True" GroupCaption="Transaction Matching Settings" SuppressLabel="true"/>
				<px:PXCheckBox ID="chkAllowMatchingToUnreleasedBatch" SuppressLabel="True" LabelsWidth="XXS" ControlSize="M" runat="server" DataField="AllowMatchingToUnreleasedBatch" />
                <px:PXCheckBox ID="chkSkipReconciled" SuppressLabel="True" runat="server" DataField="SkipReconciled" />
				<px:PXLayoutRule runat="server" LabelsWidth="XXS" ControlSize="M" StartGroup="True" GroupCaption="Import Settings" SuppressLabel="true"/>
				<px:PXCheckBox ID="chkIgnoreCuryCheckOnImport" SuppressLabel="True" runat="server" DataField="IgnoreCuryCheckOnImport" />
				<px:PXCheckBox ID="chkAllowEmptyFITID" SuppressLabel="True" runat="server" DataField="AllowEmptyFITID" />
				<px:PXCheckBox ID="chkImportToSingleAccount" SuppressLabel="True" runat="server" DataField="ImportToSingleAccount" />
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="L" StartGroup="True"/>
				<px:PXSelector ID="edStatementImportTypeName" LabelsWidth="L" runat="server" DataField="StatementImportTypeName"/>
			</Template>
		</px:PXTabItem>
			<px:PXTabItem Text="Incoming Payments Settings">
			<Template>
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="XM" StartGroup="True" GroupCaption="Disbursement Matching"/>
				<px:PXNumberEdit ID="edDisbursementTranDaysBeforeIncPayments" DataField="DisbursementTranDaysBeforeIncPayments" runat="server" />
				<px:PXNumberEdit ID="edDisbursementTranDaysAfterIncPayments" DataField="DisbursementTranDaysAfterIncPayments" runat="server" />
				<px:PXCheckBox ID="chkAllowMatchingCreditMemoIncPayments" SuppressLabel="True" LabelsWidth="XXS" ControlSize="M" runat="server" DataField="AllowMatchingCreditMemoIncPayments" />
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="XM" StartGroup="True" GroupCaption="Receipt Matching"/>
				<px:PXNumberEdit ID="edReceiptTranDaysBeforeIncPayments" DataField="ReceiptTranDaysBeforeIncPayments" runat="server" />
				<px:PXNumberEdit ID="edReceiptTranDaysAfterIncPayments" DataField="ReceiptTranDaysAfterIncPayments" runat="server" />

				<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Weights for Relevance Calculation"/>
				<px:PXPanel runat="server" Border="none"   RenderStyle="Simple" 
					ID="pnlRelativeWeights" RenderSimple="True"> 
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="xs" StartColumn="True" />
				<px:PXNumberEdit ID="edRefNbrCompareWeightIncPayments" DataField="RefNbrCompareWeightIncPayments" CommitChanges="True" runat="server" />
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="xs" ColumnSpan="2"/>
				<px:PXCheckBox ID="chkEmptyRefNbrMatchingIncPayments" runat="server" DataField="EmptyRefNbrMatchingIncPayments" CommitChanges="True"/>
				<px:PXNumberEdit ID="edDateCompareWeightIncPayments" DataField="DateCompareWeightIncPayments" CommitChanges="True" runat="server" />
				<px:PXNumberEdit ID="edPayeeCompareWeightIncPayments" DataField="PayeeCompareWeightIncPayments" CommitChanges="True" runat="server" />
				<px:PXLayoutRule runat="server" LabelsWidth="XXS" ControlSize="XS" StartColumn="True"/>
				<px:PXNumberEdit ID="edRefNbrComparePercentIncPayments" DataField="RefNbrComparePercentIncPayments"  runat="server"  Enabled="False" Size="xs"/>
				<px:PXNumberEdit ID="edDateComparePercentIncPayments" DataField="DateComparePercentIncPayments"  runat="server" Enabled="False" Size="xs" />
				<px:PXNumberEdit ID="edPayeeComparePercentIncPayments" DataField="PayeeComparePercentIncPayments"  runat="server" Enabled="False" Size="xs"/>
				</px:PXPanel>
				<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="XM" StartGroup="True" GroupCaption="Date Range for Relevance Calculation"/>
				<px:PXNumberEdit ID="edDateMeanOffsetIncPayments" DataField="DateMeanOffsetIncPayments"  runat="server" />
				<px:PXNumberEdit ID="edDateSigmaIncPayments" DataField="DateSigmaIncPayments"  runat="server" />
			</Template>
		</px:PXTabItem>
	</Items>
	</px:PXTab>
</asp:Content>
