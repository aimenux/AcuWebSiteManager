<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR101060.aspx.cs"
	Inherits="Page_PR101060" Title="Deductions and Benefits" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Document" TypeName="PX.Objects.PR.PRDedBenCodeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupCommand="" PopupCommandTarget="" PopupPanel="" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="Next" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="Previous" RepaintControls="All" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" TabIndex="2500" AllowCollapse="true">
		<Template>
			<!-- HEADER AREA -->
			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
			<px:PXSelector ID="edCodeCD" runat="server" DataField="CodeCD" CommitChanges="true" />
			<px:PXTextEdit runat="server" DataField="Description" ID="edDescription" />
					
			<px:PXDropDown runat="server" DataField="ContribType" ID="edContribType" CommitChanges="True" />
			<px:PXSelector ID="edBAccountID" runat="server" AllowEdit="true" DataField="BAccountID" />
				<px:PXDropDown runat="server" DataField="DedInvDescrType" ID="edDedInvDescrType" CommitChanges="True" />

			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
				<px:PXCheckBox runat="server" DataField="IsActive" ID="chkIsActive" />
				<px:PXCheckBox runat="server" DataField="IsGarnishment" ID="chkIsGarnishment" CommitChanges="true" />
				<px:PXCheckBox runat="server" DataField="AffectsTaxes" ID="chkAffectsTaxes" CommitChanges="true" />
				<px:PXCheckBox runat="server" DataField="AcaApplicable" ID="chkAcaApplicable" CommitChanges="true" />			
				<px:PXCheckBox runat="server" DataField="IsWorkersCompensation" ID="chkIsWC" CommitChanges="true" />

			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ColumnSpan="2" />
				<px:PXTextEdit runat="server" DataField="VndInvDescr" ID="edVndInvDescr" />		
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Style="margin-top: 0px;" DataMember="CurrentDocument">
		<Items>
			<px:PXTabItem VisibleExp="DataControls[&quot;chkAffectsTaxes&quot;].Value==true" BindingContext="form" Text="Tax Settings">
				<AutoCallBack Enabled="true" Command="Refresh" Target="grid1"></AutoCallBack>
				<Template>
					<px:PXFormView ID="taxSettingsForm" runat="server" DataMember="CurrentDocument">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="M" />
								<px:PXDropDown runat="server" ID="edIncludeType" DataField="IncludeType" CommitChanges="True" />
								<px:PXSelector runat="server" ID="edBenefitTypeCD" DataField="BenefitTypeCD" CommitChanges="true" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="105px" Width="100%" SkinID="Details" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="DeductCodeTaxes" DataKeyNames="CodeID, TaxID">
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="125px" CommitChanges="true"></px:PXGridColumn>
									<px:PXGridColumn DataField="TaxID_Description" Width="300px"></px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Parent" Enabled="True" MinHeight="200"></AutoSize>
						<ActionBar ActionsText="False"></ActionBar>
						<AutoCallBack ActiveBehavior="true" Enabled="true"></AutoCallBack>
					</px:PXGrid>					
				</Template>
			</px:PXTabItem>
			<px:PXTabItem VisibleExp="DataControls[&quot;chkAcaApplicable&quot;].Value==true" BindingContext="form" Text="ACA Information">
				<AutoCallBack Enabled="true" Command="Refresh" Target="gridAca"></AutoCallBack>
				<Template>
					<px:PXFormView ID="acaInformationForm" runat="server" DataMember="CurrentDocument">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="M" />
								<px:PXNumberEdit runat="server" ID="edMinimumIndividualContribution" DataField="MinimumIndividualContribution" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridAca" runat="server" DataSourceID="ds" Height="105px" Width="100%" SkinID="Details" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="AcaInformation" DataKeyNames="CodeID, CoverageType">
								<Columns>
									<px:PXGridColumn DataField="CoverageType" Width="250px" CommitChanges="true" />
									<px:PXGridColumn DataField="HealthPlanType" Width="650px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Parent" Enabled="True" MinHeight="200"></AutoSize>
						<ActionBar ActionsText="False"></ActionBar>
						<AutoCallBack ActiveBehavior="true" Enabled="true"></AutoCallBack>
					</px:PXGrid>					
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Employee Deduction Settings" VisibleExp="DataControls[&quot;edContribType&quot;].Value!=CNT" BindingContext="form" >
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="M" />
					<px:PXDropDown runat="server" ID="edDedCalcType" DataField="DedCalcType" CommitChanges="True" />
					<px:PXNumberEdit runat="server" ID="edDedAmount" DataField="DedAmount" AllowNull="True" />
					<px:PXNumberEdit runat="server" ID="edDedPercent" DataField="DedPercent" AllowNull="True" />
					<px:PXDropDown runat="server" ID="edDedMaxFreqType" DataField="DedMaxFreqType" CommitChanges="True" />
					<px:PXNumberEdit runat="server" ID="edDedMaxAmount" DataField="DedMaxAmount" AllowNull="True" />
					<px:PXSelector runat="server" ID="edDedReportType" DataField="DedReportType" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Employer Contribution Settings" VisibleExp="DataControls[&quot;edContribType&quot;].Value!=DED" BindingContext="form" >
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="M" />
					<px:PXDropDown runat="server" ID="edCntCalcType" DataField="CntCalcType" CommitChanges="True" />
						<px:PXNumberEdit runat="server" ID="edCntAmount" DataField="CNtAmount" AllowNull="True" />
						<px:PXNumberEdit runat="server" ID="edCntPercent" DataField="CntPercent" AllowNull="True" />
						<px:PXDropDown runat="server" ID="edCntMaxFreqType" DataField="CntMaxFreqType" CommitChanges="True" />
						<px:PXNumberEdit runat="server" ID="edCntMaxAmount" DataField="CntMaxAmount" AllowNull="True" />
						<px:PXSelector runat="server" ID="edCntReportType" DataField="CntReportType" />
						<px:PXSelector runat="server" ID="edCertifiedReportType" DataField="CertifiedReportType" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="WCC Code" VisibleExp="DataControls[&quot;chkIsWC&quot;].Value==true" BindingContext="form" >
				<Template>
					<px:PXFormView ID="wcForm" runat="server" DataMember="CurrentDocument">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="S" />
								<px:PXSelector runat="server" ID="edState" DataField="State" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridWC" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="WorkCompensationRates">
								<RowTemplate>
									<px:PXNumberEdit ID="edRate" runat="server" DataField="Rate" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" Width="60px" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="WorkCodeID" Width="200px" CommitChanges="true" />
									<px:PXGridColumn DataField="PMWorkCode__Description" Width="300px" />
									<px:PXGridColumn DataField="DeductionRate" Width="120px" />
									<px:PXGridColumn DataField="Rate" Width="120px" />
									<px:PXGridColumn DataField="EffectiveDate" Width="250px" CommitChanges="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
						<ActionBar ActionsText="False">
							<Actions>
								<Delete Enabled="false" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXSelector ID="edDedLiabilityAcctID" runat="server" DataField="DedLiabilityAcctID" />
					<px:PXSegmentMask ID="edDedLiabilitySubID" runat="server" DataField="DedLiabilitySubID" />
					<px:PXSelector ID="edBenefitExpenseAcctID" runat="server" DataField="BenefitExpenseAcctID" />
					<px:PXSegmentMask ID="edBenefitExpenseSubID" runat="server" DataField="BenefitExpenseSubID" />
					<px:PXSelector ID="edBenefitLiabilityAcctID" runat="server" DataField="BenefitLiabilityAcctID" />
					<px:PXSegmentMask ID="edBenefitLiabilitySubID" runat="server" DataField="BenefitLiabilitySubID" />
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>