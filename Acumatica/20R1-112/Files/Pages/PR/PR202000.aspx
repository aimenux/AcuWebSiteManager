<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PR202000.aspx.cs" Inherits="Page_PR202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PREmployeeClassMaint" PrimaryView="EmployeeClass">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ShowDetails" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="75px" DataMember="EmployeeClass" TabIndex="1300">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="M"></px:PXLayoutRule>
			<px:PXSelector ID="edEmployeeClassID" runat="server" DataField="EmployeeClassID" DataSourceID="ds" AutoRefresh="true" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="CurEmployeeClassRecord">
		<Items>
			<px:PXTabItem Text="Payroll Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="M" />
					<px:PXDropDown ID="edEmpType" runat="server" DataField="EmpType" CommitChanges="true" />
					<px:PXSelector ID="edPayGroupID" runat="server" DataField="PayGroupID" AllowEdit="true" />
                    <PX:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit ="true" CommitChanges="True" />
                    <px:PXNumberEdit ID="edHoursPerWeek" runat="server" DataField="HoursPerWeek" />
					<px:PXNumberEdit ID="edStdWeeksPerYear" runat="server" DataField="StdWeeksPerYear" CommitChanges="True" />
                    <px:PXNumberEdit ID="edHoursPerYear" runat="server" DataField="HoursPerYear" />
					<px:PXCheckbox ID="chkOverrideHoursPerYearForCertified" runat="server" DataField="OverrideHoursPerYearForCertified" CommitChanges="true" />
					<px:PXNumberEdit ID="edHoursPerYearForCertified" AllowNull="true" runat="server" DataField="HoursPerYearForCertified" />
                    <px:PXCheckBox ID="chkExemptFromOvertimeRules" runat="server" DataField="ExemptFromOvertimeRules" />
					<px:PXNumberEdit ID="edNetPayMin" runat="server" DataField="NetPayMin" />
					<px:PXNumberEdit ID="edGrnMaxPctNet" runat="server" DataField="GrnMaxPctNet" />
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="M" />
					<px:PXSelector ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="true" CommitChanges="true" />
                    <px:PXSelector ID="edWorkCodeID" runat="server" DataField="WorkCodeID" AllowEdit="true" />
					<px:PXSelector ID="edUnionID" runat="server" DataField="UnionID" AllowEdit="true" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Paid Time Off">
				<Template>
					<px:PXGrid runat="server" DataSourceID="ds" ID="grdPTOBanks" SkinID="DetailsInTab" Width="100%">
						<Levels>
							<px:PXGridLevel DataMember="EmployeeClassPTOBanks">
								<RowTemplate>
									<px:PXNumberEdit ID="edAccrualRate" runat="server" DataField="AccrualRate" />
									<px:PXNumberEdit ID="edAccrualLimit" runat="server" DataField="AccrualLimit" />
									<px:PXNumberEdit ID="edCarryoverAmount" runat="server" DataField="CarryoverAmount" />
									<px:PXNumberEdit ID="edFrontLoadingAmount" runat="server" DataField="FrontLoadingAmount" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center" Width="60px" />
									<px:PXGridColumn DataField="BankID" CommitChanges="true" />
									<px:PXGridColumn DataField="BankID_Description" />
									<px:PXGridColumn DataField="AccrualRate" />
									<px:PXGridColumn DataField="AccrualLimit" />
									<px:PXGridColumn DataField="StartDate" />
									<px:PXGridColumn DataField="CarryoverType" CommitChanges="true" />
									<px:PXGridColumn DataField="CarryoverAmount" CommitChanges="true" />
									<px:PXGridColumn DataField="FrontLoadingAmount" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
