<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PR203000.aspx.cs" Inherits="Page_PR203000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="PayrollEmployee" TypeName="PX.Objects.PR.PREmployeePayrollSettingsMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="GetTAXCodes" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewTaxList" Visible="False" />
			<px:PXDSCallbackCommand Name="AssignTaxes" Visible="False" />
			<px:PXDSCallbackCommand Name="GarnishmentDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="DeletePTOBank" Visible="False" />
			<px:PXDSCallbackCommand Name="AddTaxCode" Visible="False" />
			<px:PXDSCallbackCommand Name="PasteLine" Visible="False" DependOnGrid="gridDirectDeposit" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="ResetOrder" Visible="False" DependOnGrid="gridDirectDeposit" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="PayrollEmployee">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector ID="edAcctCD" runat="server" DataField="AcctCD" />
			<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XXS" ControlSize="XXS" />
			<px:PXCheckBox ID="edActiveInPayroll" DataField="ActiveInPayroll" runat="server" />
		</Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="CurrentPayrollEmployee">
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
					<px:PXFormView ID="fAddressInfo" runat="server" DataSourceID="ds" DataMember="Address" Caption="Address Info" RenderStyle="FieldSet" TabIndex="300">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AllowAddNew="True" DataSourceID="ds" CommitChanges="true" AutoRefresh="True" />
							<px:PXSelector ID="edState" runat="server" DataField="State" AllowAddNew="True" DataSourceID="ds" AutoRefresh="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
						</Template>
					</px:PXFormView>

					<px:PXLayoutRule runat="server" GroupCaption="Tax Location Info" LabelsWidth="SM" ControlSize="M" />
					<px:PXFormView ID="fTaxLocation" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Address" TabIndex="9100" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartRow="True" />
							<px:PXButton runat="server" TabIndex="100" ID="btnGetTaxEngineCodes" CommandName="GetTaxCodes" CommandSourceID="ds" />
							<px:PXTextEdit ID="edPrTaxLocationCode" runat="server" DataField="TaxLocationCode" CommitChanges="true" />
							<px:PXTextEdit ID="edPrTaxMunicipalCode" runat="server" DataField="TaxMunicipalCode" />
							<px:PXTextEdit ID="edPrTaxSchoolCode" runat="server" DataField="TaxSchoolCode" />
							<px:PXButton ID="btnViewTaxList" runat="server" TabIndex="140" CommandName="ViewTaxList" CommandSourceID="ds" Text="View Tax List" />
						</Template>
					</px:PXFormView>

					<px:PXLayoutRule runat="server" GroupCaption="Employment Info" LabelsWidth="SM" ControlSize="M" />
					<px:PXFormView ID="fEmployment" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="EmploymentHistory" TabIndex="9100" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartRow="True" />
							<px:PXDateTimeEdit ID="edHireDate" runat="server" DataField="HireDate" />
							<px:PXDateTimeEdit ID="edTerminationDate" runat="server" DataField="TerminationDate" />
						</Template>
					</px:PXFormView>

					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" GroupCaption="General Info"  />
					<px:PXSelector ID="PXSelector1" runat="server" TabIndex="20" DataField="EmployeeClassID" AllowEdit="True" CommitChanges="True" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXDropDown ID="edEmpType" runat="server" TabIndex="25" DataField="EmpType" CommitChanges="True" />
					<px:PXCheckBox ID="chkEmpTypeUseDflt" runat="server" TabIndex="30" DataField="EmpTypeUseDflt" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />

					<px:PXLayoutRule runat="server" Merge="True" StartGroup="true" />
					<px:PXSelector ID="edPayGroupID" runat="server" TabIndex="35" DataField="PayGroupID" AllowEdit="True" />
					<px:PXCheckBox ID="chkPayGroupUseDflt" runat="server" DataField="PayGroupUseDflt" TabIndex="40" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" EndGroup="true" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" TabIndex="45" Width="200" CommitChanges="True" />
					<px:PXCheckBox ID="edCalendarIDUseDflt" runat="server" DataField="CalendarIDUseDflt" TabIndex="47" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />

					<px:PXNumberEdit ID="edHoursPerWeek" runat="server" DataField="HoursPerWeek" TabIndex="50" Width="200" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit ID="edStdWeeksPerYear" runat="server" DataField="StdWeeksPerYear" TabIndex="55" Width="200" CommitChanges="True" />
					<px:PXCheckBox ID="chkStdWeeksPerYearUseDflt" runat="server" DataField="StdWeeksPerYearUseDflt" TabIndex="57" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />
					<px:PXNumberEdit ID="edHoursPerYear" runat="server" DataField="HoursPerYear" TabIndex="58" Width="200" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkOverrideHoursPerYearForCertified" runat="server" DataField="OverrideHoursPerYearForCertified" TabIndex="59" Width="200" CommitChanges="True" />
					<px:PXCheckBox ID="chkOverrideHoursPerYearForCertifiedUseDflt" runat="server" DataField="OverrideHoursPerYearForCertifiedUseDflt" TabIndex="60" CommitChanges="True"  style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit ID="edHoursPerYearForCertified" runat="server" DataField="HoursPerYearForCertified" AllowNull="true" TabIndex="61" Width="200" />
					<px:PXCheckBox ID="chkHoursPerYearForCertifiedUseDflt" runat="server" DataField="HoursPerYearForCertifiedUseDflt" TabIndex="62" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />
					
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkExemptFromOvertimeRules" runat="server" DataField="ExemptFromOvertimeRules" TabIndex="63" Width="200" CommitChanges="True" />
					<px:PXCheckBox ID="chkExemptFromOvertimeRulesUseDflt" runat="server" DataField="ExemptFromOvertimeRulesUseDflt" TabIndex="64" CommitChanges="True"  style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit ID="edNetPayMin" runat="server" AllowNull="True" DataField="NetPayMin" TabIndex="65" Width="200" />
					<px:PXCheckBox ID="chkNetPayMinUseDflt" runat="server" DataField="NetPayMinUseDflt" TabIndex="70" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector ID="edLocationID" runat="server" DataField="LocationID" TabIndex="75" AllowEdit="True" />
					<px:PXCheckBox ID="chkLocationUseDflt" runat="server" TabIndex="80" DataField="LocationUseDflt" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector ID="edUnionID" runat="server" TabIndex="95" DataField="UnionID" AllowEdit="True" />
					<px:PXCheckBox ID="chkUnionUseDflt" runat="server" TabIndex="100" DataField="UnionUseDflt" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />

					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector ID="edWorkCodeID" runat="server" TabIndex="105" DataField="WorkCodeID" AllowEdit="True" />
					<px:PXCheckBox ID="chkWorkCodeUseDflt" runat="server" TabIndex="110" DataField="WorkCodeUseDflt" CommitChanges="True" style="margin-left: 15px;" />
                    <px:PXLayoutRule runat="server" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Settings">
				<Template>
					<px:PXGrid ID="EmpAttr" runat="server" DataSourceID="ds" SkinID="Inquire" MatrixMode="True" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="EmployeeAttributes">
								<Columns>
									<px:PXGridColumn DataField="Description" Width="400px" />
									<px:PXGridColumn DataField="State" Width="120px" />
									<px:PXGridColumn DataField="Value" Width="200px" MatrixMode="True" />
									<px:PXGridColumn DataField="UseDefault" TextAlign="Center" Type="CheckBox" Width="120px" CommitChanges="True" />
									<px:PXGridColumn DataField="AllowOverride" TextAlign="Center" Type="CheckBox" Width="120px" />
									<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="120px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Taxes">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp2" PositionInPercent="true" SplitterPosition="40" Orientation="Vertical">
						<AutoSize Enabled="true" />
						<Template1>
							<px:PXGrid ID="EmployeeTax" runat="server" DataSourceID="ds" SkinID="DetailsInTab" KeepPosition="True" SyncPosition="True" Width="100%" Height="430px">
								<Levels>
									<px:PXGridLevel DataMember="EmployeeTax">
										<RowTemplate>
											<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" AllowEdit="True" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="TaxID" Width="130px" CommitChanges="True" />
											<px:PXGridColumn DataField="TaxID_Description" Width="300px" />
											<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="true" />
								<AutoCallBack Enabled="true" Command="Refresh" Target="EmployeeTaxAttributes" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="EmployeeTaxAttributes" runat="server" DataSourceID="ds" Caption="Tax Settings" CaptionVisible="true" MatrixMode="True" SkinID="Inquire" Width="100%">
								<Levels>
									<px:PXGridLevel DataMember="EmployeeTaxAttributes">
										<Columns>
											<px:PXGridColumn DataField="Description" Width="400px" />
											<px:PXGridColumn DataField="Value" Width="200px" MatrixMode="True" CommitChanges="true" />
											<px:PXGridColumn DataField="UseDefault" TextAlign="Center" Type="CheckBox" Width="120px" CommitChanges="True" />
											<px:PXGridColumn DataField="AllowOverride" TextAlign="Center" Type="CheckBox" Width="120px" />
											<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="120px" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Compensation">
				<Template>
					<px:PXGrid ID="gridSalary" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" MatrixMode="true">
						<Levels>
							<px:PXGridLevel DataMember="EmployeeEarning">
								<RowTemplate>
									<px:PXSelector ID="edTypeCD" runat="server" DataField="TypeCD" AllowEdit="true" />
									<px:PXNumberEdit ID="edPayRate" runat="server" DataField="PayRate" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TypeCD" CommitChanges="True" Width="150px" />
									<px:PXGridColumn DataField="TypeCD_Description" Width="200px" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="PayRate" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="UnitType" />
									<px:PXGridColumn DataField="StartDate" Width="120px" />
									<px:PXGridColumn DataField="EndDate" Width="120px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="400" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Deductions and Benefits" LoadOnDemand="true" RepaintOnDemand="true">
				<Template>
					<px:PXFormView ID="splitTypeForm" runat="server" DataMember="CurrentPayrollEmployee">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="true" ControlSize="S" LabelsWidth="XL" />
							<px:PXDropDown runat="server" ID="edDedSplitType" DataField="DedSplitType" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXNumberEdit ID="edGrnMaxPctNet" runat="server" DataField="GrnMaxPctNet" />
							<px:PXCheckBox ID="chkuseDefault" runat="server" DataField="GrnMaxPctuseDflt" CommitChanges="True" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridDeductions" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Height="200px" Width="100%" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="EmployeeDeduction">
								<RowTemplate>
									<px:PXNumberEdit ID="edDedAmount" runat="server" DataField="DedAmount" />
									<px:PXNumberEdit ID="edDedPercent" runat="server" DataField="DedPercent" />
									<px:PXNumberEdit ID="edDedMaxAmount" runat="server" DataField="DedMaxAmount" />
									<px:PXNumberEdit ID="edCntAmount" runat="server" DataField="CntAmount" />
									<px:PXNumberEdit ID="edCntPercent" runat="server" DataField="CntPercent" />
									<px:PXNumberEdit ID="edCntMaxAmount" runat="server" DataField="CntMaxAmount" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CodeID" CommitChanges="True" />
									<px:PXGridColumn DataField="CodeID_Description" Width="185px" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="DedAmount" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="DedPercent" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="DedMaxFreqType" TextAlign="Left" Width="110px" />
									<px:PXGridColumn DataField="DedMaxAmount" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="DedUseDflt" TextAlign="Center" Type="CheckBox" Width="113px" CommitChanges="true" />
									<px:PXGridColumn DataField="CntAmount" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="CntPercent" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="CntMaxFreqType" TextAlign="Left" Width="110px" />
									<px:PXGridColumn DataField="CntMaxAmount" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="CntUseDflt" TextAlign="Center" Type="CheckBox" Width="130px" CommitChanges="true" />
									<px:PXGridColumn DataField="IsGarnishment" TextAlign="Center" Type="CheckBox" Width="78px" />
									<px:PXGridColumn DataField="Sequence" TextAlign="Right" Width="45px" />
									<px:PXGridColumn DataField="StartDate" Width="120px" />
									<px:PXGridColumn DataField="EndDate" Width="120px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Garnishment Details" Key="cmdGD" CommandName="GarnishmentDetails" CommandSourceID="ds" DependOnGrid="gridDeductions" StateColumn="IsGarnishment" />
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="true" />
						<AutoCallBack Command="Refresh" Target="GarnishmentDetailsForm" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Paid Time Off">
				<Template>
					<px:PXFormView ID="fPTOBanksFilter" runat="server" DataMember="PTOBanksFilter">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="true" ControlSize="S" LabelsWidth="S" />
							<px:PXDateTimeEdit ID="edDateFrom" runat="server" DataField="DateFrom" CommitChanges="True" />
							<px:PXLayoutRule runat="server" StartColumn="true" ControlSize="S" LabelsWidth="S" />
							<px:PXDateTimeEdit ID="edDateTo" runat="server" DataField="DateTo" CommitChanges="True" />
							<px:PXLayoutRule runat="server" StartRow="true" ControlSize="S" LabelsWidth="S" />
							<px:PXCheckBox ID="edUsePTOBanksFromClass" runat="server" DataField="CurrentPayrollEmployee.UsePTOBanksFromClass" CommitChanges="True" />
						</Template>
					</px:PXFormView>
					<px:PXGrid runat="server" DataSourceID="ds" ID="grdPTOBanks" SkinID="DetailsInTab" Width="100%" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="PTOBanks">
								<RowTemplate>
									<px:PXNumberEdit ID="edAccrualRate" runat="server" DataField="AccrualRate" />
									<px:PXNumberEdit ID="edAccrualLimit" runat="server" DataField="AccrualLimit" />
									<px:PXNumberEdit ID="edCarryoverAmount" runat="server" DataField="CarryoverAmount" />
									<px:PXNumberEdit ID="edFrontLoadingAmount" runat="server" DataField="FrontLoadingAmount" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BankID" CommitChanges="true" />
									<px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center" CommitChanges="true" Width="60px" />
									<px:PXGridColumn DataField="UseClassDefault" Type="CheckBox" TextAlign="Center" CommitChanges="true" Width="60px" />
									<px:PXGridColumn DataField="BankID_Description" />
									<px:PXGridColumn DataField="AccrualRate" />
									<px:PXGridColumn DataField="AccrualLimit" />
									<px:PXGridColumn DataField="CarryoverType" CommitChanges="true" />
									<px:PXGridColumn DataField="CarryoverAmount" CommitChanges="true" />
									<px:PXGridColumn DataField="FrontLoadingAmount" />
									<px:PXGridColumn DataField="AccumulatedAmount" />
									<px:PXGridColumn DataField="UsedAmount" />
									<px:PXGridColumn DataField="AllowDelete" Visible="False" AllowShowHide="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<Actions>
								<Delete ToolBarVisible="False" MenuVisible="false" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" DependOnGrid="grdPTOBanks" ImageKey="RecordDel" DisplayStyle="Image" CommandName="DeletePTOBank" StateColumn="AllowDelete" />
							</CustomItems>
						</ActionBar>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Payment Settings">
				<Template>
					<px:PXFormView ID="formPaymentSettings" runat="server" Width="100%" DataMember="CurrentPayrollEmployee" DataSourceID="ds" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
							<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" />
							<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" AutoRefresh="True" />
						 </Template>
                    </px:PXFormView>
					<px:PXGrid ID="gridDirectDeposit" runat="server" DataSourceID="ds" TabIndex="4700" SkinID="DetailsInTab" Width="100%" Caption="Direct Deposit" SyncPosition="true">
						<Mode InitNewRow="True" AllowDragRows="True" />
						<CallbackCommands PasteCommand="PasteLine">
							<Save PostData="Container" />
						</CallbackCommands>
						<Levels>
							<px:PXGridLevel DataMember="EmployeeDirectDeposit">
								<RowTemplate>
									<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
									<px:PXNumberEdit ID="edPercent" runat="server" DataField="Percent" />
									<px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BankAcctNbr" Width="120px"  AllowDragDrop="true" />
									<px:PXGridColumn DataField="BankAcctType" Width="120px" />
									<px:PXGridColumn DataField="BankName" Width="200px" />
									<px:PXGridColumn DataField="BankRoutingNbr" Width="120px" />
									<px:PXGridColumn DataField="Amount" Width="100px" TextAlign="Right" CommitChanges="True" />
									<px:PXGridColumn DataField="Percent" Width="100px" TextAlign="Right" CommitChanges="True" />
									<px:PXGridColumn DataField="SortOrder" Width="50px" TextAlign="Right" />
									<px:PXGridColumn DataField="GetsRemainder" Width="120px" TextAlign="Center" Type="CheckBox" CommitChanges="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXFormView ID="frmGLAccounts" runat="server" DataMember="CurrentPayrollEmployee" Width="100%" CaptionVisible="False" TemplateContainer="" DataSourceID="ds" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSelector ID="edEarningsAcctID" runat="server" DataField="EarningsAcctID" />
							<px:PXSegmentMask ID="edEarningsSubID" runat="server" DataField="EarningsSubID" DataSourceID="ds" />
							<px:PXSelector ID="edDedLiabilityAcctID" runat="server" DataField="DedLiabilityAcctID" />
							<px:PXSegmentMask ID="edDedLiabilitySubID" runat="server" DataField="DedLiabilitySubID" />
							<px:PXSelector ID="edBenefitExpenseAcctID" runat="server" DataField="BenefitExpenseAcctID" />
							<px:PXSegmentMask ID="edBenefitExpenseSubID" runat="server" DataField="BenefitExpenseSubID" />
							<px:PXSelector ID="edBenefitLiabilityAcctID" runat="server" DataField="BenefitLiabilityAcctID" />
							<px:PXSegmentMask ID="edBenefitLiabilitySubID" runat="server" DataField="BenefitLiabilitySubID" />
							<px:PXSelector ID="edPayrollTaxExpenseAcctID" runat="server" DataField="PayrollTaxExpenseAcctID" />
							<px:PXSegmentMask ID="edPayrollTaxExpenseSubID" runat="server" DataField="PayrollTaxExpenseSubID" />
							<px:PXSelector ID="edPayrollTaxLiabilityAcctID" runat="server" DataField="PayrollTaxLiabilityAcctID" />
							<px:PXSegmentMask ID="edPayrollTaxLiabilitySubID" runat="server" DataField="PayrollTaxLiabilitySubID" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
	<px:PXSmartPanel runat="server" ID="pnlTaxList" Caption="Possible Taxes for this Location" CaptionVisible="true" LoadOnDemand="true"
		Key="TaxesList" Width="1430px" Height="300px" AutoCallBack-Enabled="true" AutoCallBack-Target="taxListGrid"
		AutoCallBack-Command="Refresh">
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="DetailsInTab" RenderStyle="Simple" ContentLayout-ContentAlign="Left" Width="100%" RenderSimple="True">
			<px:PXButton ID="assignButton" runat="server" DialogResult="OK" CommandSourceID="ds" Text="Assign Selected Taxes to Employee" CommandName="AssignTaxes" />
		</px:PXPanel>
		<px:PXGrid ID="taxListGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" SyncPosition="True">
			<Levels>
				<px:PXGridLevel DataMember="TaxesList">
					<Columns>
						<px:PXGridColumn DataField="Selected" Width="50px" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" />
						<px:PXGridColumn DataField="State" Width="50px" />
						<px:PXGridColumn DataField="TaxLocationCode" Width="100px" />
						<px:PXGridColumn DataField="TaxTypeCode" Width="100px" />
						<px:PXGridColumn DataField="TaxDescription" Width="350px" />
						<px:PXGridColumn DataField="TaxJurisdiction" Width="100px" />
						<px:PXGridColumn DataField="TaxCategory" Width="150px" />
						<px:PXGridColumn DataField="TaxUniqueCode" Width="250px" />
						<px:PXGridColumn DataField="TaxCD" Width="100px" />
						<px:PXGridColumn DataField="TaxName" Width="200px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<ActionBar>
				<CustomItems>
					<px:PXToolBarButton CommandName="AddTaxCode" CommandSourceID="ds" Text="Add Tax Code" DependOnGrid="taxListGrid" />
				</CustomItems>
			</ActionBar>
			<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
			<Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="pnlGarnishmentDetails" Caption="Garnishment Details" CaptionVisible="true" LoadOnDemand="true"
		Key="CurrentDeduction" Width="800px" Height="300px" ShowAfterLoad="True" AutoCallBack-Target="formCurrentDeduction" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
		<px:PXFormView ID="formCurrentDeduction" runat="server" DataMember="CurrentDeduction" DataSourceID="ds" Width="100%" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="XL" />
				<px:PXSelector ID="edGarnVendor" runat="server" DataField="GarnBAccountID" CommitChanges="True" />
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="XL" />
				<px:PXTextEdit ID="edVndInvDescr" runat="server" DataField="VndInvDescr" CommitChanges="True" />
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="XL" />
				<px:PXTextEdit ID="edGarnCourtName" runat="server" DataField="GarnCourtName" CommitChanges="True" />
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="M" />
				<px:PXTextEdit ID="edGarnDocRefNbr" runat="server" DataField="GarnDocRefNbr" CommitChanges="True" />
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="M" />
				<px:PXDateTimeEdit ID="edGarnCourtDate" runat="server" DataField="GarnCourtDate" CommitChanges="True" />
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="M" />
				<px:PXNumberEdit ID="edGarnOrigAmount" runat="server" DataField="GarnOrigAmount" CommitChanges="True" />
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="M" />
				<px:PXNumberEdit ID="edGarnPaidAmount" runat="server" DataField="GarnPaidAmount" CommitChanges="True" />
			</Template>
			<AutoSize Enabled="true" />
		</px:PXFormView>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="GarnishmentOkButton" runat="server" DialogResult="OK" Text="OK" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlCreateEditPREmployee" runat="server" Key="CreateEditPREmployeeFilter" Height="100px" CaptionVisible="True" Caption="Create Employee Payroll Settings" AllowResize="false">
		<px:PXFormView ID="formCreateEmployee" runat="server" DataMember="CreateEditPREmployeeFilter" DataSourceID="ds" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="M" ControlSize="M" />
				<px:PXSelector ID="edEmployeeID" runat="server" DataField="BAccountID" CommitChanges="True" AutoRefresh="True" />
				<px:PXSelector ID="edEmployeeClassID" runat="server" DataField="EmployeeClassID" />
				<px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" />
			</Template>
			<AutoSize Enabled="true" />
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Create" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="CANCEL" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
