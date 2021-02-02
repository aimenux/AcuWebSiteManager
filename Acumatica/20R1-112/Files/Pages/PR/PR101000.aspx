<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR101000.aspx.cs" Inherits="Page_PR101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRSetupMaint" PrimaryView="Setup">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" DataMember="Setup">
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Numbering Settings" />
					<px:PXSelector ID="edBatchNumberingID" runat="server" AllowEdit="True" DataField="BatchNumberingID" />
					<px:PXSelector ID="edTranNumberingCD" runat="server" AllowEdit="True" DataField="TranNumberingCD" />
					<px:PXSelector ID="edBatchNumberingCD" runat="server" AllowEdit="True" DataField="BatchNumberingCD" />

					<px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Miscellaneous Settings" />
					<px:PXNumberEdit ID="edPayRateDecimalPlaces" runat="server" DataField="PayRateDecimalPlaces" />
					<px:PXCheckBox ID="edPayPeriodDateChangeAllowed" runat="server" DataField="PayPeriodDateChangeAllowed" />
                    <px:PXSelector ID="edRegularHoursType" runat="server" DataField="RegularHoursType" CommitChanges="True" />
                    <px:PXSelector ID="edHolidaysType" runat="server" DataField="HolidaysType" CommitChanges="True" />
                    <px:PXSelector ID="edCommissionType" runat="server" DataField="CommissionType" />
                    <PX:PXCheckBox ID="edEnablePieceworkEarningType" runat="server" DataField="EnablePieceworkEarningType" CommitChanges="True" />
                    <PX:PXCheckBox ID="edHoldEntry" runat="server" DataField="HoldEntry" />

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Posting and Retention Settings" StartColumn="true" ControlSize="M" LabelsWidth="M" />
					<px:PXDropDown ID="edEarningsAcctDefault" runat="server" AllowNull="False" DataField="EarningsAcctDefault" CommitChanges="true" />
					<px:PXSegmentMask ID="edEarningsSubMask" runat="server" DataField="EarningsSubMask" CommitChanges="true" />
					<px:PXDropDown ID="edDeductLiabilityAcctDefault" runat="server" AllowNull="False" DataField="DeductLiabilityAcctDefault" />
					<px:PXSegmentMask ID="edDeductLiabilitySubMask" runat="server" DataField="DeductLiabilitySubMask" />
					<px:PXDropDown ID="edBenefitExpenseAcctDefault" runat="server" AllowNull="False" DataField="BenefitExpenseAcctDefault" CommitChanges="true" />
					<px:PXSegmentMask ID="edBenefitExpenseSubMask" runat="server" DataField="BenefitExpenseSubMask" CommitChanges="true" />
					<px:PXDropDown ID="edBenefitLiabilityAcctDefault" runat="server" AllowNull="False" DataField="BenefitLiabilityAcctDefault" />
					<px:PXSegmentMask ID="edBenefitLiabilitySubMask" runat="server" DataField="BenefitLiabilitySubMask" />
					<px:PXDropDown ID="edTaxExpenseAcctDefault" runat="server" AllowNull="False" DataField="TaxExpenseAcctDefault" CommitChanges="true" />
					<px:PXSegmentMask ID="edTaxExpenseSubMask" runat="server" DataField="TaxExpenseSubMask" CommitChanges="true" />
					<px:PXDropDown ID="edTaxLiabilityAcctDefault" runat="server" AllowNull="False" DataField="TaxLiabilityAcctDefault" />
					<px:PXSegmentMask ID="edTaxLiabilitySubMask" runat="server" DataField="TaxLiabilitySubMask" />
					<px:PXDropDown ID="edProjectCostAssignment" runat="server" AllowNull="False" DataField="ProjectCostAssignment" CommitChanges="true" />
					<px:PXCheckBox SuppressLabel="True" ID="chkUpdateGL" runat="server" DataField="UpdateGL" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSummPost" runat="server" DataField="SummPost" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoPost" runat="server" DataField="AutoPost" />
					<px:PXCheckBox ID="edDisableGLWarnings" runat="server" DataField="DisableGLWarnings" />
					<px:PXCheckBox ID="edHideEmployeeInfo" runat="server" DataField="HideEmployeeInfo" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Attributes">
				<Template>
					<px:PXFormView ID="filterForm" runat="server" DataMember="AttributeFilter">
						<Template>
							<px:PXCheckBox ID="chkFilterStates" runat="server" DataField="FilterStates" CommitChanges="True" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="AttributeGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" MatrixMode="True" Style="margin-top: 10px">
						<Levels>
							<px:PXGridLevel DataMember="TaxAttributes">
								<Columns>
									<px:PXGridColumn DataField="Description" Width="400px" />
									<px:PXGridColumn DataField="State" Width="120px" />
									<px:PXGridColumn DataField="AllowOverride" TextAlign="Center" Type="CheckBox" Width="150px" />
									<px:PXGridColumn DataField="Value" Width="200px" />
									<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="80px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Transaction Date Exceptions">
				<Template>
					<px:PXFormView ID="noTransDateOnWeekendForm" runat="server" DataMember="Setup">
						<Template>
							<px:PXCheckBox ID="chkNoTransDateOnWeekendForm" runat="server" DataField="NoWeekendTransactionDate" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="transactionDateExceptionGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" Style="margin-top: 10px">
						<Levels>
							<px:PXGridLevel DataMember="TransactionDateExceptions">
								<Columns>
									<px:PXGridColumn DataField="Date" Width="200px" CommitChanges="true" />
									<px:PXGridColumn DataField="DayOfWeek" Width="200px" />
									<px:PXGridColumn DataField="Description" Width="500px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
</asp:Content>
