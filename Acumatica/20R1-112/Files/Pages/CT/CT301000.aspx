<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CT301000.aspx.cs"
	Inherits="Page_CT301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CT.ContractMaint"
		PrimaryView="Contracts" BorderStyle="NotSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Action" CommitChanges="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="ViewUsage" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand DependOnGrid="WatchersGrid" Name="ShowContact" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" DependOnGrid="InvoicesGrid" />
			<px:PXDSCallbackCommand Name="Renew" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewContract" Visible="False" DependOnGrid="RenewalHistoryGrid" />
			<px:PXDSCallbackCommand Name="Bill" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Activate" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Setup" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="SetupAndActivate" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Terminate" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Upgrade" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ActivateUpgrade" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="UndoBilling" Visible="false" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
		CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
		AcceptButtonID="btnOK">
		<px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
			DataMember="ChangeIDDialog">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
				<AutoCallBack Target="formChangeID" Command="Save" />
			</px:PXButton>
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Contracts" Caption="Contract Info"
		NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" LinkIndicator="true"
		NotifyIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edContractCD">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" />
			<px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID" AllowEdit="true" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" AllowNull="False" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="true" />
			<px:PXSegmentMask ID="edLocation" runat="server" DataField="LocationID" AllowEdit="True" CommitChanges="true" AutoRefresh="true"/>
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXNumberEdit runat="server" ID="edBalance" DataField="Balance" />

		</Template>
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
	</px:PXFormView>
	<px:PXSmartPanel ID="PanelSetup" runat="server" AcceptButtonID="bOK" AutoReload="true" CancelButtonID="bCancel"
		Caption="Setup Contract" CaptionVisible="True" DesignView="Content" HideAfterAction="false" Key="SetupSettings"
		LoadOnDemand="true" Overflow="Hidden">
		<px:PXFormView ID="formSetupSettings" runat="server" SkinID="Transparent" DataMember="SetupSettings"
			DefaultControlID="edActivationDate">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
				<px:PXLayoutRule runat="server" StartRow="true" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
			<px:PXButton ID="bOK" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="bCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelActivate" runat="server" AcceptButtonID="OK" AutoReload="true" CancelButtonID="Cancel"
		Caption="Activate Contract" CaptionVisible="True" DesignView="Content" HideAfterAction="false" Key="ActivationSettings"
		LoadOnDemand="true" Overflow="Hidden">
		<px:PXFormView ID="formActivationSettings" runat="server" SkinID="Transparent" DataMember="ActivationSettings"
			DefaultControlID="edActivationDate">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edActivationDate" runat="server" DataField="ActivationDate" />
				<px:PXLayoutRule runat="server" StartRow="true" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="OK" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="Cancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelTerminate" runat="server" AcceptButtonID="PXButtonOK" AutoReload="true" CancelButtonID="PXButtonCancel"
		Caption="Terminate Contract" CaptionVisible="True" DesignView="Content" HideAfterAction="false" Key="TerminationSettings"
		LoadOnDemand="true" Overflow="Hidden">
		<px:PXFormView ID="PXFormView2" runat="server" SkinID="Transparent" DataMember="TerminationSettings"
			DefaultControlID="edTerminationSettings">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edTerminationDate" runat="server" DataField="TerminationDate" />
				<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="true" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelOnDemand" runat="server" AcceptButtonID="PXButtonOK" AutoReload="true" CancelButtonID="PXButtonCancel"
		Caption="Billing On Demand" CaptionVisible="True" DesignView="Content" HideAfterAction="false" Key="OnDemandSettings"
		LoadOnDemand="true" Overflow="Hidden">
		<px:PXFormView ID="PXFormView5" runat="server" SkinID="Transparent" DataMember="OnDemandSettings"
			DefaultControlID="edOnDemandSettings">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edBillingDate" runat="server" DataField="BillingDate" />
				<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="true" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel10" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton4" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="PXButton5" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelRenewal" runat="server" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel"
		Caption="Renew Contract" CaptionVisible="True" DesignView="Content" ShowAfterLoad="true" Key="RenewalSettings"
		LoadOnDemand="true" AutoCallBack-Enabled="true" AutoCallBack-Target="formRenewal" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
		<px:PXFormView ID="formRenewal" runat="server" SkinID="Transparent" DataMember="RenewalSettings"
			DefaultControlID="edRenewalDate">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edRenewalDate" runat="server" DataField="RenewalDate" />
				<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="true" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK"/>
			<px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="CurrentContract">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Summary">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Contract Settings" />

					<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
					<px:PXDateTimeEdit CommitChanges="True" ID="edActivationDate" runat="server" DataField="ActivationDate" />
					<px:PXDateTimeEdit CommitChanges="True" ID="edExpireDate" runat="server" DataField="ExpireDate" />
					<px:PXDateTimeEdit ID="edTerminationDate" runat="server" DataField="TerminationDate" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoRenew" runat="server" DataField="AutoRenew" CommitChanges="true" />
					<px:PXLayoutRule runat="server" Merge="true" />
					<px:PXNumberEdit ID="edAutoRenewDays" runat="server" AllowNull="False" DataField="AutoRenewDays" Size="XXS" />
					<px:PXTextEdit ID="edDaysBeforeExpiration" DataField="DaysBeforeExpiration" runat="server" SkinID="Label" SuppressLabel="true" Enabled="False" />
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="true" />
					<px:PXNumberEdit ID="edGracePeriod" runat="server" AllowNull="False" DataField="GracePeriod" Size="XXS" />
					<px:PXTextEdit ID="edDays" DataField="Days" runat="server" SkinID="Label" SuppressLabel="true" Enabled="False" Size="XXS" />
					<px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
					<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Size="XS" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartGroup="true" GroupCaption="Billing Schedule" />
					<px:PXFormView ID="billingForm" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
							<px:PXDateTimeEdit runat="server" ID="edStartBilling" DataField="StartBilling" />
							<px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" />
							<px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" />
							<px:PXDateTimeEdit ID="edNextDate" runat="server" DataField="NextDate" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" StartColumn="true" />
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="true" GroupCaption="Billing Information" />
					<px:PXFormView ID="PXFormView1" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple" MarkRequired="Dynamic">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXDropDown ID="edBillTo" DataField="BillTo" runat="server" AllowNull="false" CommitChanges="true" AutoRefresh="true" />
							<px:PXSelector ID="edAccountID" DataField="AccountID" runat="server" CommitChanges="true" AllowEdit="true" />
							<px:PXSelector ID="edLocationID" DataField="LocationID" runat="server" AutoRefresh="true" CommitChanges="true" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="PXFormView6" runat="server" DataMember="BillingLocation" DataSourceID="ds" RenderStyle="Simple" TabIndex="3900">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True">
							</px:PXLayoutRule>
							<px:PXSelector ID="edCBranchID" DataField="CBranchID" runat="server" AutoRefresh="True" Enabled="false" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="descriptionFormulaView" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple" MarkRequired="Dynamic" >
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="SM" StartColumn="True"/>
							<pxa:CTFormulaInvoiceEditor ID="edDescriptionFormulaInv" runat="server" DataSourceID="ds" DataField="InvoiceFormula" Parameters="@ActionInvoice"/>
							<pxa:CTFormulaTransactionEditor ID="edDescriptionFormulaTran" runat="server" DataSourceID="ds" DataField="TranFormula" Parameters="@Prefix,@ActionItem" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartGroup="true" GroupCaption="Contract Management" LabelsWidth="SM" ControlSize="M" />
					<px:PXSelector runat="server" ID="edOwnerID" DataField="OwnerID" AutoRefresh="true" />
					<px:PXSegmentMask runat="server" ID="edSalesPersonID" DataField="SalesPersonID" AutoRefresh="true" />
					<px:PXSelector runat="server" ID="PXSelector1" DataField="CaseItemID" AutoRefresh="true" Size="M" AllowEdit="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXFormView ID="PXFormView4" runat="server" DataMember="CurrentContract" DataSourceID="ds" Style="z-index: 100" Width="100%">
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
							<px:PXDateTimeEdit runat="server" DataField="EffectiveFrom" ID="edEffectiveFrom" />
							<px:PXSelector runat="server" ID="edDiscountID" DataField="DiscountID" Size="SM" CommitChanges="true" />
							<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edPendingSetup" DataField="PendingSetup" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edPendingRecurring" DataField="PendingRecurring" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edPendingRenewal" DataField="PendingRenewal" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="TotalPending" DataField="TotalPending" Enabled="false" />
							<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edCurrentSetup" DataField="CurrentSetup" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCurrentRecurring" DataField="CurrentRecurring" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edCurrentRenewal" DataField="CurrentRenewal" Enabled="false" />
						</Template>
					</px:PXFormView>
					<px:PXGrid runat="server" ID="detGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="ContractDetails">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edContractItemID" runat="server" DataField="ContractItemID" AllowEdit="true" AutoRefresh="true" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
									<px:PXNumberEdit ID="edPendingQty" DataField="Qty" runat="server" CommitChanges="true" />
									<px:PXTextEdit runat="server" ID="edChange" DataField="Change" />
									<px:PXNumberEdit runat="server" ID="edBasePriceVal" DataField="BasePriceVal" />
									<px:PXNumberEdit runat="server" ID="edBaseDiscountPct" DataField="BaseDiscountPct" />
									<px:PXNumberEdit runat="server" ID="edRecurringDiscountPct" DataField="RecurringDiscountPct" />
									<px:PXNumberEdit runat="server" ID="edRenewalDiscountPct" DataField="RenewalDiscountPct" />
									<px:PXNumberEdit runat="server" ID="edUsagePriceVal" DataField="UsagePriceVal" />
									<px:PXNumberEdit runat="server" ID="PXNumberEdit1" DataField="FixedRecurringPriceVal" />
									<px:PXNumberEdit runat="server" ID="edRenewalPriceVal" DataField="RenewalPriceVal" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ContractItemID" Width="135px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Description" Width="300px" />
									<px:PXGridColumn DataField="Qty" Width="90px" TextAlign="Right" CommitChanges="True" />
									<px:PXGridColumn DataField="Change" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="BasePriceVal" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="BaseDiscountPct" Width="80px" TextAlign="Right" />
									<px:PXGridColumn DataField="FixedRecurringPriceVal" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="UsagePriceVal" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="RecurringDiscountPct" Width="80px" TextAlign="Right" />
									<px:PXGridColumn DataField="RenewalPriceVal" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="RenewalDiscountPct" Width="80px" TextAlign="Right" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Recurring Summary">
				<Template>
					<px:PXFormView ID="PXFormView3" runat="server" DataMember="CurrentContract" DataSourceID="ds" Style="z-index: 100" Width="100%">
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit runat="server" ID="edTotalRecurring" DataField="TotalRecurring" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edTotalUsage" DataField="TotalUsage" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edTotalDue" DataField="TotalDue" Enabled="false" />
						</Template>
					</px:PXFormView>
					<px:PXGrid runat="server" ID="detGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab">
						<AutoSize Enabled="True" />
						<Mode AllowAddNew="False" AllowDelete="False" AllowSort="False" AllowUpdate="False" />
						<ActionBar PagerVisible="False">
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="RecurringDetails">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edContractItemID1" runat="server" DataField="ContractItemID" AllowEdit="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ContractItemID" Width="135px" />
									<px:PXGridColumn DataField="Description" Width="300px" />
									<px:PXGridColumn DataField="InventoryItem__InventoryCD" Width="120px" />
									<px:PXGridColumn DataField="ContractItem__UOMForDeposits" Width="63px" />
									<px:PXGridColumn DataField="ContractItem__RecurringTypeForDeposits" Width="63px" />
									<px:PXGridColumn DataField="RecurringIncluded" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="FixedRecurringPriceVal" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="RecurringDiscountPct" Width="80px" TextAlign="Right" />
									<px:PXGridColumn DataField="UsagePriceVal" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="RecurringUsed" Width="80px" TextAlign="Right" />
									<px:PXGridColumn DataField="RecurringUsedTotal" Width="80px" TextAlign="Right" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Employee Overrides">
				<Template>
					<px:PXGrid ID="gridContractRates" runat="server" DataSourceID="ds" Height="400px" Width="100%"
						SkinID="DetailsInTab" AllowPaging="False">
						<CallbackCommands>
							<Refresh SelectControlsIDs="gridEmployeeContract" />
						</CallbackCommands>
						<Levels>
							<px:PXGridLevel DataMember="ContractRates" DataKeyNames="RecordID">
								<Columns>
									<px:PXGridColumn DataField="EarningType" Width="110px" CommitChanges="True" />
									<px:PXGridColumn DataField="EPEarningType__Description" Width="110px" />
									<px:PXGridColumn DataField="LabourItemID" Width="150px" CommitChanges="True" Label="Labor Item" />
									<px:PXGridColumn DataField="EmployeeID" CommitChanges="True" NullText="All Employees" Width="200px" />
									<px:PXGridColumn DataField="EPEmployee__AcctName" Width="200px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode InitNewRow="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Contract History">
				<Template>
					<px:PXGrid ID="RenewalHistoryGrid" runat="server" Height="350px" Width="100%" Style="z-index: 100" AdjustPageSize="Auto"
						AllowSearch="true" DataSourceID="ds" SkinID="DetailsInTab">
						<AutoSize Enabled="True" />
						<ActionBar DefaultAction="cmdViewContract">
							<CustomItems>
								<px:PXToolBarButton Text="View Contract" Key="cmdViewContract">
									<AutoCallBack Command="ViewContract" Target="ds">
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode AllowAddNew="False" AllowDelete="False" AllowSort="False" AllowUpdate="False" />
						<Levels>
							<px:PXGridLevel DataMember="RenewalHistory">
								<Columns>
									<px:PXGridColumn DataField="Action" Width="115px" />
									<px:PXGridColumn DataField="Date" Width="130px" />
									<px:PXGridColumn DataField="CreatedByID" Width="115px" />
									<px:PXGridColumn DataField="ChildContractID" Width="130px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXDropDown ID="edAction" runat="server" DataField="Action" />
									<px:PXSelector ID="edUser" runat="server" AllowEdit="True" DataField="CreatedByID" />
									<px:PXSelector ID="edChildId" runat="server" AllowEdit="True" DataField="ChildContractID" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="AR History">
				<Template>
					<px:PXGrid ID="InvoicesGrid" runat="server" Height="350px" Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto"
						AllowSearch="true" DataSourceID="ds" SkinID="DetailsInTab">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="Invoices">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
									<px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="DocType" />
									<px:PXGridColumn DataField="RefNbr" Width="90px" LinkCommand="ViewInvoice" />
									<px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" Width="90px" />
									<px:PXGridColumn DataField="DocDate" Width="60px" />
									<px:PXGridColumn DataField="DueDate" Width="90px" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" />
									<px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="PaymentMethodID" Width="90px" />
								</Columns>
								<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AutoInsert="False" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="Inquire" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Answers">
								<Columns>
									<px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="220px" AllowShowHide="False" TextField="AttributeID_description" />
									<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
									<px:PXGridColumn DataField="Value" Width="148px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" />
						<Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>

	<px:PXSmartPanel ID="panelRenewManualNumbering" runat="server" Caption="Please enter new Contract ID" CaptionVisible="true" LoadOnDemand="true" Key="renewManualNumberingFilter"
		ShowAfterLoad="true"
		AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AutoCallBack-Target="formRenewManualNumbering"
		AcceptButtonID="PXButtonOK2" CancelButtonID="PXButtonCancel2">
		<px:PXFormView ID="formRenewManualNumbering" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="renewManualNumberingFilter">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
				<px:PXLabel Size="xl" ID="lblMessage" runat="server" Height="40">A new contract is being created for your renewable contract. Please enter a Contract ID.</px:PXLabel>
				<px:PXMaskEdit CommitChanges="True" ID="edContractCD" runat="server" DataField="ContractCD" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButtonOK2" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds" />
			<px:PXButton ID="PXButtonCancel2" runat="server" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
