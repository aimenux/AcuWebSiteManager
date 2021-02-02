<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR302000.aspx.cs"
	Inherits="Page_PR302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function refreshFringeReducingRateGrids() {
			px_alls.grdFringeBenefitsReducingRate.refresh();
			px_alls.grdFringeEarningsReducingRate.refresh();
		}
	</script>

	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRPayChecksAndAdjustments" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewTaxSplits" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewTaxDetails" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewDeductionDetails" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewBenefitDetails" Visible="false" />
			<px:PXDSCallbackCommand Name="CopySelectedEarningDetailLine" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewOvertimeRules" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewExistingPayment" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewExistingPayrollBatch" Visible="false" />
            <px:PXDSCallbackCommand Name="viewDirectDepositSplits" Visible="false" />
            <px:PXDSCallbackCommand Name="RevertOvertimeCalculation" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="ViewProjectDeductionAndBenefitPackages" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Document">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="SM" StartColumn="True" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" CommitChanges="True" />
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="true" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
			<px:PXCheckBox ID="edHold" runat="server" DataField="Hold" CommitChanges="true" />
			<px:PXSelector ID="edPayGroupID" runat="server" DataField="PayGroupID" CommitChanges="True" />
			<px:PXSelector ID="edPayPeriodID" runat="server" DataField="PayPeriodID" CommitChanges="True" AutoRefresh="True" />
			<px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />

			<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="XM" StartColumn="True" />
			<px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" CommitChanges="True" AutoRefresh="True" />
			<px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" CommitChanges="True" />
			<px:PXSelector ID="edCashAccountID" runat="server" DataField="CashAccountID" CommitChanges="True" AutoRefresh="True" />
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
			<px:PXDateTimeEdit ID="edTransactionDate" runat="server" DataField="TransactionDate" />

			<px:PXLayoutRule runat="server" ColumnSpan="2"></px:PXLayoutRule>
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />

			<px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True" />
			<px:PXNumberEdit ID="edGrossAmount" runat="server" DataField="GrossAmount" />
			<px:PXNumberEdit ID="edDedAmount" runat="server" DataField="DedAmount" />
			<px:PXNumberEdit ID="edTaxAmount" runat="server" DataField="TaxAmount" />
			<px:PXNumberEdit ID="edNetAmount" runat="server" DataField="NetAmount" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataMember="CurrentDocument">
		<Items>
			<px:PXTabItem Text="Earning Details">
				<Template>
					<px:PXGrid ID="gridEarnings" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" Height="500px" KeepPosition="True" SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto">
						<Levels>
							<px:PXGridLevel DataMember="Earnings">
								<Mode InitNewRow="True" />
								<RowTemplate>
                                    <px:PXCheckBox runat="server" ID="edAllowCopy" DataField="AllowCopy" />
									<px:PXSelector runat="server" ID="edBranchID" DataField="BranchID" />
									<px:PXDateTimeEdit runat="server" ID="edDate" DataField="Date" />
									<px:PXSelector runat="server" ID="edTypeCD" DataField="TypeCD" />
									<px:PXTextEdit runat="server" ID="edTypeCD_Description" DataField="TypeCD_Description" />
									<px:PXSelector runat="server" ID="edLocationID" DataField="LocationID" />
									<px:PXNumberEdit runat="server" ID="edHours" DataField="Hours" />
									<px:PXNumberEdit runat="server" ID="edUnits" DataField="Units" />
									<px:PXDropDown runat="server" ID="edUnitType" DataField="UnitType" />
									<px:PXNumberEdit runat="server" ID="edRate" DataField="Rate" />
									<px:PXCheckBox runat="server" ID="edManualRate" DataField="ManualRate" />
									<px:PXNumberEdit runat="server" ID="edAmount" DataField="Amount" />
									<px:PXSelector runat="server" ID="edAccountID" DataField="AccountID" />
									<px:PXMaskEdit runat="server" ID="edSubID" DataField="SubID" />
									<px:PXSelector runat="server" ID="edProjectID" DataField="ProjectID" />
									<px:PXSelector runat="server" ID="edProjectTaskID" DataField="ProjectTaskID" />
									<px:PXCheckBox runat="server" ID="edCertifiedJob" DataField="CertifiedJob" />
									<px:PXSelector runat="server" ID="edCostCodeID" DataField="CostCodeID" />
									<px:PXSelector runat="server" ID="edUnionID" DataField="UnionID" />
									<px:PXSelector runat="server" ID="edLabourItemID" DataField="LabourItemID" />
									<px:PXSelector runat="server" ID="edWorkCodeID" DataField="WorkCodeID" />
								</RowTemplate>
								<Columns>
                                    <px:PXGridColumn DataField="AllowCopy" Type="CheckBox" AllowShowHide="False" Visible="False" />
									<px:PXGridColumn DataField="BranchID" Width="90px" CommitChanges="True" />
									<px:PXGridColumn DataField="Date" Width="80px" CommitChanges="True" />
									<px:PXGridColumn DataField="TypeCD" Width="65px" CommitChanges="True" />
									<px:PXGridColumn DataField="TypeCD_Description" Width="110px" />
									<px:PXGridColumn DataField="LocationID" Width="70px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Hours" Width="60px" CommitChanges="True" TextAlign="Right" />
									<px:PXGridColumn DataField="Units" Width="60px" CommitChanges="True" TextAlign="Right" />
									<px:PXGridColumn DataField="UnitType" Width="80px" CommitChanges="True" />
									<px:PXGridColumn DataField="Rate" TextAlign="Right" CommitChanges="True" />
                                    <PX:PXGridColumn DataField="ManualRate" CommitChanges="true" Type="CheckBox" />
									<px:PXGridColumn DataField="Amount" Width="100px" TextAlign="Right" CommitChanges="True" />
									<px:PXGridColumn DataField="AccountID" Width="75px" CommitChanges="True" />
									<px:PXGridColumn DataField="SubID" Width="100px" CommitChanges="True" />
									<px:PXGridColumn DataField="ProjectID" Width="150px" CommitChanges="True" />
									<px:PXGridColumn DataField="ProjectTaskID" Width="150px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CertifiedJob" Width="70px" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
									<px:PXGridColumn DataField="CostCodeID" Width="100px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="UnionID" Width="70px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="LabourItemID" Width="70px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="WorkCodeID" Width="70px" CommitChanges="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
                        <ActionBar>
							<CustomItems>
                                <px:PXToolBarButton Text="Copy Selected Entry" DependOnGrid="gridEarnings" CommandSourceID="ds" StateColumn="AllowCopy">
                                    <AutoCallBack Command="CopySelectedEarningDetailLine" Target="ds" />
                                </px:PXToolBarButton>
								<px:PXToolBarButton Text="Overtime Rules" DependOnGrid="grid" CommandSourceID="ds">
									<AutoCallBack Command="ViewOvertimeRules" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Container="Window" Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Earning Summary">
				<Template>
					<px:PXGrid ID="grdSummaryEarnings" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="500px">
						<Levels>
							<px:PXGridLevel DataMember="SummaryEarnings">
								<RowTemplate>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TypeCD" Width="60px" CommitChanges="True" />
									<px:PXGridColumn DataField="TypeCD_Description" Width="175px" />
									<px:PXGridColumn DataField="LocationID" Width="60px" CommitChanges="True" />
									<px:PXGridColumn DataField="Hours" Width="60px" CommitChanges="True" TextAlign="Right" />
									<px:PXGridColumn DataField="Rate" TextAlign="Right" CommitChanges="True" />
									<px:PXGridColumn DataField="Amount" Width="100px" TextAlign="Right" />
									<px:PXGridColumn DataField="MTDAmount" Width="100px" TextAlign="Right" />
									<px:PXGridColumn DataField="QTDAmount" Width="100px" TextAlign="Right" />
									<px:PXGridColumn DataField="YTDAmount" Width="100px" TextAlign="Right" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Deductions" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="grdDeductions" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" Height="500px">
						<Levels>
							<px:PXGridLevel DataMember="Deductions">
								<Mode InitNewRow="True" />
								<RowTemplate>
									<px:PXSelector ID="edCodeID" runat="server" DataField="CodeID" AllowEdit="True" AutoRefresh="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CodeID" Width="130px" CommitChanges="True" />
									<px:PXGridColumn DataField="CodeID_Description" Width="185px" />
                                    <px:PXGridColumn DataField="IsActive" Width="60px" Type="CheckBox" CommitChanges="True" />
									<px:PXGridColumn DataField="Source" Width="130px" />
									<px:PXGridColumn DataField="ContribType" Width="200px" />
									<px:PXGridColumn DataField="DedAmount" TextAlign="Right" Width="120px" CommitChanges="True" />
									<px:PXGridColumn DataField="CntAmount" TextAlign="Right" Width="150px" CommitChanges="True" />
									<px:PXGridColumn DataField="SaveOverride" Width="60px" Type="CheckBox" CommitChanges="True" />
									<px:PXGridColumn DataField="YtdAmount" TextAlign="Right" Width="120px" />
									<px:PXGridColumn DataField="EmployerYtdAmount" TextAlign="Right" Width="150px" />
                                    <px:PXGridColumn DataField="HasYtdAmounts" Type="CheckBox" />
                                </Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" MinHeight="400" Enabled="True" />
						<ActionBar>
							<Actions>
								<Delete ToolBarVisible="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton>
									<AutoCallBack Command="ViewDeductionDetails" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton>
									<AutoCallBack Command="ViewBenefitDetails" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Taxes">
				<Template>
					<px:PXSplitContainer ID="splitTaxes" runat="server" PositionInPercent="true" SplitterPosition="50" Orientation="Vertical" Height="100%">
						<Template1>
							<px:PXGrid ID="grdTaxes" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Height="500px" SyncPosition="true">
								<Levels>
									<px:PXGridLevel DataMember="Taxes">
										<RowTemplate>
											<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" AllowEdit="True" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="TaxID" Width="130px" CommitChanges="True" />
											<px:PXGridColumn DataField="TaxID_Description" Width="200px" />
											<px:PXGridColumn DataField="TaxCategory" CommitChanges="True" Width="150px" />
											<px:PXGridColumn DataField="TaxAmount" TextAlign="Right" Width="130px" CommitChanges="True" />
											<px:PXGridColumn DataField="WageBaseAmount" TextAlign="Right" Width="130px" />
											<px:PXGridColumn DataField="WageBaseGrossAmt" TextAlign="Right" Width="100px" />
											<px:PXGridColumn DataField="WageBaseHours" TextAlign="Right" Width="100px" />
											<px:PXGridColumn DataField="YtdAmount" TextAlign="Right" Width="130px" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton DependOnGrid="grid" CommandSourceID="ds">
											<AutoCallBack Command="ViewTaxDetails" Target="ds" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
								<AutoSize Container="Window" MinHeight="400" Enabled="True" />
								<AutoCallBack Target="taxSplitGrid" Command="Refresh" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="taxSplitGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire">
								<Levels>
									<px:PXGridLevel DataMember="TaxSplits">
										<Columns>
											<px:PXGridColumn DataField="WageType" Width="150px" />
											<px:PXGridColumn DataField="TaxID" Width="130px" />
											<px:PXGridColumn DataField="TaxAmount" Width="120px" />
											<px:PXGridColumn DataField="WageBaseHours" Width="100px" />
											<px:PXGridColumn DataField="WageBaseAmount" Width="120px" />
											<px:PXGridColumn DataField="WageBaseGrossAmt" Width="120px" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="true" MinHeight="150" />
								<Mode AllowAddNew="False" AllowDelete="False" />
								<ActionBar ActionsVisible="true" />
							</px:PXGrid>
						</Template2>
						<AutoSize Enabled="true" Container="Window" MinHeight="400" />
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Paid Time Off" RepaintOnDemand="false">
				<Template>
					<px:PXGrid runat="server" DataSourceID="ds" ID="grdPaymentPTOBanks" SkinID="Inquire" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="PaymentPTOBanks">
								<RowTemplate>
									<px:PXNumberEdit ID="edAccrualRate" runat="server" DataField="AccrualRate" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BankID" CommitChanges="true" />
									<px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center" Width="60px" CommitChanges="true" />
									<px:PXGridColumn DataField="BankID_Description" />
									<px:PXGridColumn DataField="IsCertifiedJob" Type="CheckBox" TextAlign="Center" Width="60px" CommitChanges="true" />
									<px:PXGridColumn DataField="AccrualAmount" CommitChanges="true" />
									<px:PXGridColumn DataField="DisbursementAmount" CommitChanges="true" />
									<px:PXGridColumn DataField="AccrualRate" CommitChanges="true" />
									<px:PXGridColumn DataField="AccrualLimit" />
									<px:PXGridColumn DataField="AccumulatedAmount" />
									<px:PXGridColumn DataField="UsedAmount" />
									<px:PXGridColumn DataField="AvailableAmount" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Workers' Compensation">
				<Template>
					<px:PXGrid ID="grdWC" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" Height="500px">
						<Levels>
							<px:PXGridLevel DataMember="WCPremiums">
								<RowTemplate>
									<px:PXNumberEdit ID="edWCRate" runat="server" DataField="Rate" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="WorkCodeID" Width="200px" CommitChanges="true" />
									<px:PXGridColumn DataField="PMWorkCode__Description" Width="200px" />
									<px:PXGridColumn DataField="DeductCodeID" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="PRDeductCode__State" Width="60px" />
									<px:PXGridColumn DataField="DeductionCalcType" />
									<px:PXGridColumn DataField="BenefitCalcType" />
									<px:PXGridColumn DataField="DeductionRate" CommitChanges="true" />
									<px:PXGridColumn DataField="Rate" CommitChanges="true" />
									<px:PXGridColumn DataField="RegularWageBaseHours" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="OvertimeWageBaseHours" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="WageBaseHours" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="RegularWageBaseAmount" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="OvertimeWageBaseAmount" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="WageBaseAmount" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="DeductionAmount" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="Amount" Width="120px" CommitChanges="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Union">
				<Template>
					<px:PXGrid ID="grdUnion" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" Height="500px">
						<Levels>
							<px:PXGridLevel DataMember="UnionPackageDeductions">
								<RowTemplate>
									<px:PXSelector runat="server" ID="edPackageUnionID" DataField="UnionID" />
									<px:PXSelector runat="server" ID="edPackageUnionLaborItem" DataField="LaborItemID" />
									<px:PXSelector runat="server" ID="edPackageUnionDeductCode" DataField="DeductCodeID" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="UnionID" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="LaborItemID" CommitChanges="true" />
									<px:PXGridColumn DataField="DeductCodeID" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="PRDeductCode__DedCalcType" />
									<px:PXGridColumn DataField="PRDeductCode__CntCalcType" />
									<px:PXGridColumn DataField="RegularWageBaseHours" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="OvertimeWageBaseHours" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="WageBaseHours" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="RegularWageBaseAmount" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="OvertimeWageBaseAmount" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="WageBaseAmount" Width="120px" CommitChanges="true" />
									<px:PXGridColumn DataField="DeductionAmount" Width="120px" />
									<px:PXGridColumn DataField="BenefitAmount" Width="120px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Certified Project">
				<Template>
					<px:PXSplitContainer ID="splitCertifiedProject" runat="server" PositionInPercent="true" SplitterPosition="60" Orientation="Vertical" Height="100%">
						<Template1>
							<px:PXGrid ID="grdFringeBenefits" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" SyncPosition="true">
								<Levels>
									<px:PXGridLevel DataMember="PaymentFringeBenefits">
										<Columns>
											<px:PXGridColumn DataField="ProjectID" />
											<px:PXGridColumn DataField="LaborItemID" />
											<px:PXGridColumn DataField="ProjectTaskID" />
											<px:PXGridColumn DataField="ApplicableHours" />
											<px:PXGridColumn DataField="ProjectHours" />
											<px:PXGridColumn DataField="FringeRate" />
											<px:PXGridColumn DataField="ReducingRate" />
											<px:PXGridColumn DataField="CalculatedFringeRate" />
											<px:PXGridColumn DataField="PaidFringeAmount" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton>
											<AutoCallBack Command="ViewProjectDeductionAndBenefitPackages" Target="ds" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
								<AutoSize Container="Window" Enabled="True" />
								<ClientEvents AfterRowChange="refreshFringeReducingRateGrids" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXSplitContainer ID="splitFringeBenefit" runat="server" PositionInPercent="true" SplitterPosition="50" Orientation="Horizontal" Height="100%">
								<Template1>
									<px:PXGrid ID="grdFringeBenefitsReducingRate" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" AllowPaging="false"
										Caption="Benefits Decreasing the Rate" CaptionVisible="true">
										<Levels>
											<px:PXGridLevel DataMember="PaymentFringeBenefitsDecreasingRate">
												<Columns>
													<px:PXGridColumn DataField="DeductCodeID" Width="120px" />
													<px:PXGridColumn DataField="AnnualizationException" Width="120px" Type="CheckBox" TextAlign="Center" />
													<px:PXGridColumn DataField="AnnualHours" />
													<px:PXGridColumn DataField="AnnualWeeks" />
													<px:PXGridColumn DataField="ApplicableHours" />
													<px:PXGridColumn DataField="Amount" />
													<px:PXGridColumn DataField="BenefitRate" />
												</Columns>
											</px:PXGridLevel>
										</Levels>
										<AutoSize Container="Window" Enabled="True" />
									</px:PXGrid>
								</Template1>
								<Template2>
									<px:PXGrid ID="grdFringeEarningsReducingRate" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" AllowPaging="false"
										Caption="Earnings Decreasing the Rate" CaptionVisible="true">
										<Levels>
											<px:PXGridLevel DataMember="PaymentFringeEarningsDecreasingRate">
												<Columns>
													<px:PXGridColumn DataField="EarningTypeCD" Width="120px" />
													<px:PXGridColumn DataField="ActualPayRate" />
													<px:PXGridColumn DataField="PrevailingWage" />
													<px:PXGridColumn DataField="Amount" />
													<px:PXGridColumn DataField="AnnualizationException" Width="120px" Type="CheckBox" TextAlign="Center" />
													<px:PXGridColumn DataField="AnnualHours" />
													<px:PXGridColumn DataField="AnnualWeeks" />
													<px:PXGridColumn DataField="ApplicableHours" />
													<px:PXGridColumn DataField="BenefitRate" />
												</Columns>
											</px:PXGridLevel>
										</Levels>
										<AutoSize Container="Window" Enabled="True" />
									</px:PXGrid>
								</Template2>
								<AutoSize Container="Window" Enabled="True" />
							</px:PXSplitContainer>
						</Template2>
						<AutoSize Container="Window" Enabled="True" MinHeight="400" />
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details" BindingContext="form" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Link to GL" StartGroup="True" />
					<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False">
						<LinkCommand Target="ds" Command="ViewOriginalDocument" />
					</px:PXTextEdit>
					<px:PXButton ID="btnShowDDSplits" runat="server" Text="View Direct Deposit Splits" CommandName="viewDirectDepositSplits" CommandSourceID="ds" />

					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" GroupCaption="Additional Details" StartGroup="True" />
					<px:PXDropDown ID="edChkReprintType" runat="server" DataField="ChkReprintType" />
					<px:PXDropDown ID="edChkVoidType" runat="server" DataField="ChkVoidType" />
					<px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr"/>
                    <px:PXCheckBox ID="edSalariedNonExempt" runat="server" DataField="SalariedNonExempt" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXNumberEdit ID="edRegularAmount" runat="server" DataField="RegularAmount" CommitChanges="True" />
                    <px:PXCheckBox ID="edManualRegularAmount" runat="server" DataField="ManualRegularAmount" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" />
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
    <px:PXSmartPanel runat="server" ID="pnlOvertimeRules" Caption="Overtime Rules Used for Calculation" CaptionVisible="true" Key="PaymentOvertimeRules" AutoRepaint="True">
        <px:PXFormView ID="OvertimeRulesForm" runat="server" DataSourceID="ds" DataMember="CurrentDocument" RenderStyle="Simple" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXCheckBox ID="chkApplyOvertimeRules" runat="server" DataField="ApplyOvertimeRules" AlignLeft="true" CommitChanges="true" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="OvertimeRulesGrid" runat="server" SyncPosition="True" DataSourceID="ds" SkinID="Inquire">
            <Levels>
                <px:PXGridLevel DataMember="PaymentOvertimeRules">
                    <Columns>
                        <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" CommitChanges="True" Width="60px" />
                        <px:PXGridColumn DataField="OvertimeRuleID" Width="170px" />
                        <px:PXGridColumn DataField="PROvertimeRule__Description" Width="240px" />
                        <px:PXGridColumn DataField="PROvertimeRule__DisbursingTypeCD" Width="100px" />
                        <px:PXGridColumn DataField="PROvertimeRule__OvertimeMultiplier" TextAlign="Right" Width="70px" />
                        <px:PXGridColumn DataField="PROvertimeRule__RuleType" Width="70px" />
                        <px:PXGridColumn DataField="PROvertimeRule__WeekDay" Width="90px" />
                        <px:PXGridColumn DataField="PROvertimeRule__OvertimeThreshold" Width="100px" />
                        <px:PXGridColumn DataField="PROvertimeRule__State" Width="53px" />
                        <px:PXGridColumn DataField="PROvertimeRule__UnionID" Width="150px" />
                        <px:PXGridColumn DataField="PROvertimeRule__ProjectID" Width="150px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="true" MinHeight="300" />
        </px:PXGrid>
        <px:PXPanel ID="pnlOvertimeRulesButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnRevertOvertimeCalculation" runat="server" CommandName="RevertOvertimeCalculation" CommandSourceID="ds" Text="Revert Overtime Calculations and Close" SyncVisible="false" DialogResult="OK" />
			<px:PXButton ID="btnOK" runat="server" Text="OK" DialogResult="OK" />
		</px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel runat="server" ID="pnlExistingPayment" Caption="Existing Paycheck" CaptionVisible="true" Key="ExistingPayment" AutoRepaint="True" CloseButtonDialogResult="No">
        <px:PXFormView ID="ExistingPaymentForm" runat="server" DataSourceID="ds" DataMember="ExistingPayment" RenderStyle="Simple" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" SuppressLabel="True" />
                <px:PXTextEdit ID="edViewExistingPaymentMessage" runat="server" DataField="Message" TextMode="MultiLine" Enabled="False" Height="100" Width="300" />
            </Template>
        </px:PXFormView>
        <px:PXButton ID="btnViewExistingPayment" runat="server" Text="View Existing Paycheck" CommandName="ViewExistingPayment" CommandSourceID="ds" DialogResult="OK" />
        <px:PXButton ID="btnContinueEditingPayment1" runat="server" Text="Continue Editing" DialogResult="No" />
    </px:PXSmartPanel>
    <px:PXSmartPanel runat="server" ID="pnlExistingPayrollBatch" Caption="Existing Payroll Batch" CaptionVisible="true" Key="ExistingPayrollBatch" AutoRepaint="True" CloseButtonDialogResult="No">
        <px:PXFormView ID="ExistingPayrollBatchForm" runat="server" DataSourceID="ds" DataMember="ExistingPayrollBatch" RenderStyle="Simple" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" SuppressLabel="True" />
                <px:PXTextEdit ID="edViewExistingPayrollBatchMessage" runat="server" DataField="Message" TextMode="MultiLine" Enabled="False" Height="120" Width="300"/>
            </Template>
        </px:PXFormView>
		<px:PXButton ID="btnViewExistingPayrollBatch" runat="server" Text="View Existing Payroll Batch" CommandName="ViewExistingPayrollBatch" CommandSourceID="ds" DialogResult="OK" />
        <px:PXButton ID="btnContinueEditingPayment2" runat="server" Text="Continue Editing" DialogResult="No" />
    </px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="pnlTaxSplit" Caption="Tax Splits" CaptionVisible="true"
		LoadOnDemand="true" Key="TaxSplits" Width="1050px" AutoCallBack-Command="Refresh" AutoRepaint="True">
		<px:PXGrid ID="taxSplitGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire">
			<Levels>
				<px:PXGridLevel DataMember="TaxSplits">
					<Columns>
						<px:PXGridColumn DataField="WageType" Width="150px" />
						<px:PXGridColumn DataField="TaxID" Width="130px" />
						<px:PXGridColumn DataField="TaxAmount" Width="120px" />
						<px:PXGridColumn DataField="WageBaseHours" Width="100px" />
						<px:PXGridColumn DataField="WageBaseAmount" Width="120px" />
						<px:PXGridColumn DataField="WageBaseGrossAmt" Width="120px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" MinHeight="150" />
			<Mode AllowAddNew="False" AllowDelete="False" />
			<ActionBar ActionsVisible="true">
				<Actions>
					<AddNew Enabled="false" />
					<Delete Enabled="false" />
				</Actions>
			</ActionBar>
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="spDeductionDetails" AutoRepaint="true" Caption="Deduction Details" CaptionVisible="true" Key="DeductionDetails" Height="400px">
		<px:PXGrid ID="DeductionDetailsGrid" runat="server" DataSourceID="ds" SkinID="Inquire">
			<Levels>
				<px:PXGridLevel DataMember="DeductionDetails">
					<Columns>
						<px:PXGridColumn DataField="BranchID" />
						<px:PXGridColumn DataField="CodeID" />
						<px:PXGridColumn DataField="CodeID_Description" />
						<px:PXGridColumn DataField="Amount" TextAlign="Right" />
						<px:PXGridColumn DataField="AccountID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="SubID" Width="100px" CommitChanges="True" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" MinHeight="150" />
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="spBenefitDetails" AutoRepaint="true" Caption="Benefit Details" CaptionVisible="true" Key="BenefitDetails" Width="300px" Height="600px">
		<px:PXGrid ID="BenefitDetailsGrid" runat="server" DataSourceID="ds" SkinID="Details">
			<Levels>
				<px:PXGridLevel DataMember="BenefitDetails">
					<Columns>
						<px:PXGridColumn DataField="BranchID" CommitChanges="true" />
						<px:PXGridColumn DataField="CodeID" Width="100px" CommitChanges="true" />
						<px:PXGridColumn DataField="CodeID_Description" Width="200px" />
						<px:PXGridColumn DataField="Amount" TextAlign="Right" CommitChanges="true" />
						<px:PXGridColumn DataField="LiabilityAccountID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="LiabilitySubID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="ExpenseAccountID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="ExpenseSubID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="ProjectID" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="ProjectTaskID" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="EarningTypeCD" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="LabourItemID" Width="70px" CommitChanges="true" />
						<px:PXGridColumn DataField="CostCodeID" Width="100px" CommitChanges="True" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" MinHeight="150" />
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="spTaxDetails" AutoRepaint="true" Caption="Tax Details" CaptionVisible="true" Key="TaxDetails" Width="300px" Height="600px">
		<px:PXGrid ID="TaxDetailsGrid" runat="server" DataSourceID="ds" SkinID="Details">
			<Levels>
				<px:PXGridLevel DataMember="TaxDetails">
					<Columns>
						<px:PXGridColumn DataField="BranchID" CommitChanges="True" />
						<px:PXGridColumn DataField="TaxID" CommitChanges="True" Width="75px" />
						<px:PXGridColumn DataField="TaxID_Description" Width="150px" />
						<px:PXGridColumn DataField="TaxCategory" Width="100px" />
						<px:PXGridColumn DataField="Amount" Width="100px" CommitChanges="True" TextAlign="Right" />
						<px:PXGridColumn DataField="LiabilityAccountID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="LiabilitySubID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="ExpenseAccountID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="ExpenseSubID" Width="100px" CommitChanges="True" />
						<px:PXGridColumn DataField="ProjectID" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="ProjectTaskID" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="EarningTypeCD" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="LabourItemID" Width="70px" CommitChanges="true" />
						<px:PXGridColumn DataField="CostCodeID" Width="100px" CommitChanges="True" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" MinHeight="150" />
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="pnlDirectDepositSplits" Key="DirectDepositSplits" Caption="Direct Deposit Splits" CaptionVisible="true" AutoRepaint="true">
		<px:PXGrid ID="gridDDSplits" runat="server" DataSourceID="ds" SkinID="Inquire">
			<Levels>
				<px:PXGridLevel DataMember="DirectDepositSplits">
					<Columns>
						<px:PXGridColumn DataField="BankAcctNbr" />
						<px:PXGridColumn DataField="BankAcctType"/>
						<px:PXGridColumn DataField="BankName"/>
						<px:PXGridColumn DataField="BankRoutingNbr" />
						<px:PXGridColumn DataField="Amount"/>
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" MinHeight="150" />
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="pnlProjectDedBenPackages" AutoRepaint="true" Caption="Certified Project Deduction and Benefit Packages" CaptionVisible="true" Key="ProjectPackageDeductions">
		<px:PXGrid ID="grdProject" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" Height="500px">
			<Levels>
				<px:PXGridLevel DataMember="ProjectPackageDeductions">
					<RowTemplate>
						<px:PXSelector runat="server" ID="edPackageProjectID" DataField="ProjectID" />
						<px:PXSelector runat="server" ID="edPackageProjectLaborItem" DataField="LaborItemID" />
						<px:PXSelector runat="server" ID="edPackageProjectDeductCode" DataField="DeductCodeID" />
					</RowTemplate>
					<Columns>
						<px:PXGridColumn DataField="ProjectID" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="LaborItemID" CommitChanges="true" />
						<px:PXGridColumn DataField="DeductCodeID" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="PRDeductCode__DedCalcType" />
						<px:PXGridColumn DataField="PRDeductCode__CntCalcType" />
						<px:PXGridColumn DataField="RegularWageBaseHours" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="OvertimeWageBaseHours" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="WageBaseHours" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="RegularWageBaseAmount" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="OvertimeWageBaseAmount" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="WageBaseAmount" Width="120px" CommitChanges="true" />
						<px:PXGridColumn DataField="DeductionAmount" Width="120px" />
						<px:PXGridColumn DataField="BenefitAmount" Width="120px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" MinHeight="150" />
		</px:PXGrid>
	</px:PXSmartPanel>
</asp:Content>
