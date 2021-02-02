<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PR102000.aspx.cs" Inherits="Page_PR102000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PREarningTypeMaint" PrimaryView="EarningTypes">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="EarningTypes" TabIndex="2100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="L" LabelsWidth="SM" />
			<px:PXSelector ID="edTypeCD" runat="server" DataField="TypeCD" />
			<px:PXTextEdit runat="server" DataField="Description" ID="edDescription" />
			<px:PXDropDown runat="server" DataField="EarningTypeCategory" ID="edEarningTypeCategory" CommitChanges="True" />
			<px:PXNumberEdit runat="server" DataField="OvertimeMultiplier" ID="edOvertimeMultiplier" CommitChanges="True" Size="XS" />
            <px:PXSelector ID="edRegularTypeCD" runat="server" DataField="RegularTypeCD"  CommitChanges="True" />
			<px:PXSelector ID="edWageTypeCD" runat="server" DataField="WageTypeCD" AutoRefresh="True" CommitChanges="true" />
			<px:PXSelector runat="server" DataField="ReportType" ID="edReportType" CommitChanges="True" />
			<px:PXDropDown runat="server" DataField="IncludeType" ID="edIncludeType" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="L" LabelsWidth="SM" />
            <px:PXCheckBox runat="server" ID="edIsActive"  DataField="IsActive" CommitChanges="True" />
            <px:PXCheckBox runat="server" ID="edIsWCCCalculation" DataField="IsWCCCalculation" />
            <px:PXCheckBox runat="server" ID="edAccruePTO" DataField="AccruePTO" />
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="L" LabelsWidth="SM" />
			<px:PXLayoutRule runat="server" GroupCaption="Project Accounting Settings" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" CommitChanges="True" />
			<px:PXCheckBox ID="edIsBillable" runat="server" DataField="IsBillable" />
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="L" LabelsWidth="SM" />
			<px:PXSelector ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="EarningSettings">
		<Items>
			<px:PXTabItem Text="Subject to Taxes">
				<Template>
					<px:PXGrid ID="PXGrid1" runat="server" DataSourceID="ds" TabIndex="8700" SkinID="DetailsInTab" Style="left: 0px; top: 0px" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="EarningTypeTaxes">
								<Columns>
									<px:PXGridColumn DataField="TaxID" CommitChanges="True" />
									<px:PXGridColumn DataField="TaxID_Description" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXSelector ID="edEarningsAcctID" runat="server" DataField="EarningsAcctID" />
					<px:PXSegmentMask ID="edEarningsSubID" runat="server" DataField="EarningsSubID" />
					<px:PXSelector ID="edBenefitExpenseAcctID" runat="server" DataField="BenefitExpenseAcctID" />
					<px:PXSegmentMask ID="edBenefitExpenseSubID" runat="server" DataField="BenefitExpenseSubID" />
					<px:PXSelector ID="edTaxExpenseAcctID" runat="server" DataField="TaxExpenseAcctID" />
					<px:PXSegmentMask ID="edTaxExpenseSubID" runat="server" DataField="TaxExpenseSubID" />
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
