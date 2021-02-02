<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR207000.aspx.cs" Inherits="Page_PR207000" Title="ACA Reporting" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CompanyYearlyInformation" TypeName="PX.Objects.PR.PRAcaReportingMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="UpdateSelectedEmployees" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="UpdateAllEmployees" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="UpdateSelectedCompanyMonths" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="UpdateAllCompanyMonths" Visible="False" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="CompanyYearlyInformation" AllowCollapse="true">
		<Template>
			<!-- HEADER AREA -->
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="M" />
				<px:PXBranchSelector ID="edBranch" runat="server" DataField="OrgBAccountID" CommitChanges="true" />
				<px:PXSelector ID="edYear" runat="server" DataField="Year" CommitChanges="true" />
				<px:PXMaskEdit ID="edEin" runat="server" DataField="Ein" />

			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="XXS" />
				<px:PXCheckBox runat="server" ID="chkPartOfAgg" DataField="IsPartOfAggregateGroup" CommitChanges="true" />
				<px:PXCheckBox runat="server" ID="chkAuthoritativeTrans" DataField="IsAuthoritativeTransmittal" CommitChanges="true" />		
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%">
		<Items>
			<px:PXTabItem Text="Employee Information">
				<Template>
					<px:PXFormView ID="monthSelectorForm" runat="server" DataMember="EmployeeMonthFilter">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="S" />
								<px:PXDropDown runat="server" ID="edMonth" DataField="Month" CommitChanges="True" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridEmployees" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" KeepPosition="true" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="FilteredEmployeeInformation">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edMinimumIndividualContribution" runat="server" DataField="MinimumIndividualContribution" />
                                </RowTemplate>
								<Columns>
									<px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" CommitChanges="True" />
									<px:PXGridColumn DataField="EPEmployee__AcctCD" Width="160px" />
									<px:PXGridColumn DataField="EPEmployee__AcctName" Width="200px" />
									<px:PXGridColumn DataField="Month" Width="300px" />
									<px:PXGridColumn DataField="FTStatus" Width="125px" CommitChanges="true" />
									<px:PXGridColumn DataField="OfferOfCoverage" Width="125px" CommitChanges="true" />
									<px:PXGridColumn DataField="Section4980H" Width="125px" />
									<px:PXGridColumn DataField="MinimumIndividualContribution" Width="200px" />
									<px:PXGridColumn DataField="HoursWorked" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Parent" Enabled="True" MinHeight="200"></AutoSize>
						<ActionBar ActionsText="False">
							<CustomItems>
								<px:PXToolBarButton Key="cmdUpdateEmployee">
                                    <AutoCallBack Command="UpdateSelectedEmployees" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdUpdateAllEmployees">
                                    <AutoCallBack Command="UpdateAllEmployees" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoCallBack ActiveBehavior="true" Enabled="true"></AutoCallBack>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Company Information">
				<Template>
					<px:PXGrid ID="gridCompany" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" KeepPosition="true" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="CompanyMonthlyInformation">
								<Columns>
									<px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" CommitChanges="True" />
									<px:PXGridColumn DataField="Month" Width="300px" />
									<px:PXGridColumn DataField="NumberOfFte" Width="125px" />
									<px:PXGridColumn DataField="NumberOfEmployees" Width="125px" />
									<px:PXGridColumn DataField="PctEmployeesCoveredByMec" Width="125px" />
									<px:PXGridColumn DataField="CertificationOfEligibility" Width="250px" />
									<px:PXGridColumn DataField="SelfInsured" Width="125px" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="Numberof1095C" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Parent" Enabled="True" MinHeight="200"></AutoSize>
						<ActionBar ActionsText="False">
							<CustomItems>
								<px:PXToolBarButton Key="cmdUpdateCompanyMonths">
                                    <AutoCallBack Command="UpdateSelectedCompanyMonths" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdUpdateAllCompanyMonths">
                                    <AutoCallBack Command="UpdateAllCompanyMonths" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoCallBack ActiveBehavior="true" Enabled="true"></AutoCallBack>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Aggregate Group Information" VisibleExp="DataControls[&quot;chkPartOfAgg&quot;].Value==true" BindingContext="form">
				<Template>
					<px:PXGrid ID="gridAgg" runat="server" DataSourceID="ds" Width="100%" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="AggregateGroupInformation">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edHighestMonthlyFteNumber" runat="server" DataField="HighestMonthlyFteNumber" />
                                </RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="MemberCompanyName" Width="400px" />
									<px:PXGridColumn DataField="MemberEin" Width="300px" CommitChanges="true" />
									<px:PXGridColumn DataField="HighestMonthlyFteNumber" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Parent" Enabled="True" MinHeight="200"></AutoSize>
						<ActionBar ActionsText="False"></ActionBar>
						<AutoCallBack ActiveBehavior="true" Enabled="true"></AutoCallBack>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
	<px:PXSmartPanel runat="server" ID="pnlUpdateEmployee" Caption="Mass Update" CaptionVisible="true" LoadOnDemand="true"
		Key="EmployeeUpdate" Width="800px" Height="300px" ShowAfterLoad="True" AutoCallBack-Target="formEmployeeUpdate" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page" AllowResize="True">
		<px:PXPanel runat="server"  RenderSimple="True" RenderStyle="Simple" SkinID="Transparent" Style="margin-top: 10px">
			<px:PXLabel runat="server" ID="employeePanelLabel" Text="Please select the fields you want to update." />
		</px:PXPanel>
		<px:PXFormView ID="formEmployeeUpdate" runat="server" DataMember="EmployeeUpdate" DataSourceID="ds" Width="100%" SkinID="Transparent" Style="margin-top: 20px" Height="100%">
			<Template>
				<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="XXS" />		
					<px:PXCheckBox runat="server" ID="chkUpdateAcaFTStatus" DataField="UpdateAcaFTStatus" CommitChanges="true" />
					<px:PXCheckBox runat="server" ID="chkUpdateOfferOfCoverage" DataField="UpdateOfferOfCoverage" CommitChanges="true" />
					<px:PXCheckBox runat="server" ID="chkUpdateSection4980H" DataField="UpdateSection4980H" CommitChanges="true" />
					<px:PXCheckBox runat="server" ID="chkUpdateMinimumIndividualContribution" DataField="UpdateMinimumIndividualContribution" CommitChanges="true" />

				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" SuppressLabel="true" />		
					<px:PXDropDown runat="server" ID="edUpdateAcaFTStatus" DataField="AcaFTStatus" />
					<px:PXDropDown runat="server" ID="edUpdateOfferOfCoverage" DataField="OfferOfCoverage" />
					<px:PXDropDown runat="server" ID="edUpdateSection4980H" DataField="Section4980H" />
					<px:PXNumberEdit runat="server" ID="edUpdateMinimumIndividualContribution" DataField="MinimumIndividualContribution" />
 			</Template>
			<AutoSize Enabled="true" />
		</px:PXFormView>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="EmployeeUpdateOK" runat="server" DialogResult="Yes" Text="OK" />
			<px:PXButton ID="EmployeeUpdateCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="pnlUpdateCompany" Caption="Mass Update" CaptionVisible="true" LoadOnDemand="true"
		Key="CompanyUpdate" Width="800px" Height="300px" ShowAfterLoad="True" AutoCallBack-Target="formCompanyUpdate" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page" AllowResize="True">
		<px:PXPanel runat="server"  RenderSimple="True" RenderStyle="Simple" SkinID="Transparent" Style="margin-top: 10px">
			<px:PXLabel runat="server" ID="companyPanelLabel" Text="Please select the fields you want to update." />
		</px:PXPanel>
		<px:PXFormView ID="formCompanyUpdate" runat="server" DataMember="CompanyUpdate" DataSourceID="ds" Width="100%" SkinID="Transparent" Style="margin-top: 20px" Height="100%">
			<Template>
				<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" LabelsWidth="XXS" />		
					<px:PXCheckBox runat="server" ID="chkUpdateCertificationOfEligibility" DataField="UpdateCertificationOfEligibility" CommitChanges="true" />
					<px:PXCheckBox runat="server" ID="chkUpdateSelfInsured" DataField="UpdateSelfInsured" CommitChanges="true" />

				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" SuppressLabel="true" />		
					<px:PXDropDown runat="server" ID="edUpdateCertificationOfEligibility" DataField="CertificationOfEligibility" />
					<px:PXCheckBox runat="server" ID="chkUpdatedSelfInsured" DataField="SelfInsured" />
 			</Template>
			<AutoSize Enabled="true" />
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="CompanyUpdateOK" runat="server" DialogResult="Yes" Text="OK" />
			<px:PXButton ID="CompanyUpdateCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>