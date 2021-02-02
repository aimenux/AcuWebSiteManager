<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CT201000.aspx.cs" Inherits="Page_CT201000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CT.ContractItemMaint" PrimaryView="ContractItems">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ContractItems" Caption="Contract Item" NoteIndicator="True"
		FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edContractItemCD">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" />
			<px:PXSegmentMask runat="server" ID="edContractItemCD" DataField="ContractItemCD" AutoRefresh="true" />
			<px:PXTextEdit runat="server" ID="edDescr" DataField="Descr" />
		</Template>

		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="450px" DataSourceID="ds" DataMember="CurrentContractItem">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Price Options">
				<Template>
					<px:PXFormView ID="form1" runat="server" SkinID="Transparent" DataMember="CurrentContractItem" MarkRequired="Dynamic">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" StartGroup="True" />
							<%--<px:PXTextEdit runat="server" ID="edDescr" DataField="Descr"/>--%>
							<px:PXNumberEdit runat="server" ID="edMaxQty" DataField="MaxQty" Size="SM" CommitChanges="true" />
							<px:PXNumberEdit runat="server" ID="edMinQty" DataField="MinQty" Size="SM" CommitChanges="true" />
							<px:PXNumberEdit runat="server" ID="edDefaultQty" DataField="DefaultQty" Size="SM" CommitChanges="true" />
							<px:PXSelector runat="server" CommitChanges="true" ID="edCuryID" DataField="CuryID" AutoRefresh="true" />
							<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="SM" StartGroup="True" GroupCaption="Setup and Renewal" />
							<px:PXSelector runat="server" CommitChanges="true" ID="edBaseItemID" DataField="BaseItemID" Size="XM" AutoRefresh="true" AllowEdit="true" />
							<px:PXDropDown runat="server" CommitChanges="true" ID="edBasePriceOption" DataField="BasePriceOption" Size="SM" AllowNull="false" />
							<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="False" ID="edBasePrice" DataField="BasePrice" Size="SM" />
							<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="False" ID="edRetainRate" DataField="RetainRate" Size="SM" />
							<px:PXCheckBox runat="server" ID="chkDeposit" DataField="Deposit" CommitChanges="true" />
							<px:PXCheckBox runat="server" ID="chkRefundable" DataField="Refundable" />
							<px:PXCheckBox runat="server" ID="chkProrateSetup" DataField="ProrateSetup" />
							<px:PXSelector runat="server" CommitChanges="true" ID="edRenewalItemID" DataField="RenewalItemID" Size="XM" AutoRefresh="true" AllowEdit="true" />
							<px:PXCheckBox runat="server" ID="chkCollectRenewFeeOnActivation" DataField="CollectRenewFeeOnActivation" />
							<px:PXDropDown runat="server" CommitChanges="true" ID="edRenewalPriceOption" DataField="RenewalPriceOption" AllowNull="false" />
							<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="False" ID="edRenewalPrice" DataField="RenewalPrice" Size="SM" />
							<%--<px:PXDateTimeEdit runat="server" ID="edDiscontinueAfter" DataField="DiscontinueAfter" />--%>
							<%--<px:PXSelector runat="server" ID="edReplacementItemID" DataField="ReplacementItemID" Size="XM" AutoRefresh="true" AllowEdit="true" CommitChanges="true"/>--%>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" StartGroup="True" />
							<px:PXNumberEdit runat="server" ID="edBasePriceVal" DataField="BasePriceVal" Size="SM" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edFixedRecurringPriceVal" DataField="FixedRecurringPriceVal" Size="SM" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edUsagePriceVal" DataField="UsagePriceVal" Size="SM" Enabled="false" />
							<px:PXNumberEdit runat="server" ID="edRenewalPriceVal" DataField="RenewalPriceVal" Size="SM" Enabled="false" />
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="SM" StartGroup="True" GroupCaption="Recurring Billing" />
							<px:PXDropDown runat="server" CommitChanges="true" ID="edRecurringType" DataField="RecurringType" AllowNull="false" />
							<px:PXSelector runat="server" CommitChanges="true" ID="edRecurringItemID" DataField="RecurringItemID" Size="XM" AutoRefresh="true" AllowEdit="true" />
							<px:PXCheckBox runat="server" ID="chkResetUsageOnBilling" DataField="ResetUsageOnBilling" CommitChanges="true" />
							<px:PXDropDown runat="server" CommitChanges="true" ID="edFixedRecurringPriceOption" DataField="FixedRecurringPriceOption" AllowNull="false" />
							<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="False" ID="edFixedRecurringPrice" DataField="FixedRecurringPrice" Size="SM" />
							<px:PXDropDown runat="server" CommitChanges="true" ID="edUsagePriceOption" DataField="UsagePriceOption" AllowNull="false" />
							<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="false" ID="edUsagePrice" DataField="UsagePrice" Size="SM" />
							<px:PXSelector runat="server" ID="edDepositItemID" DataField="DepositItemID" Size="XM" AutoRefresh="true" AllowEdit="true" CommitChanges="true" />
						</Template>
					</px:PXFormView>
				</Template>

			</px:PXTabItem>
			<px:PXTabItem Text="Used in Contract Templates">
				<Template>
					<px:PXGrid runat="server" ID="detGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="Inquire" BorderWidth="0px">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="CurrentTemplates">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M" />
									<px:PXSegmentMask runat="server" ID="edContractCD" DataField="ContractCD" AllowEdit="true" />
									<px:PXTextEdit runat="server" ID="edContractDescription" DataField="Description" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ContractCD" Width="135px" AutoCallBack="true" />
									<px:PXGridColumn DataField="Description" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Active Contracts">
				<Template>
					<px:PXGrid ID="ContractsGrid" runat="server" SkinID="Inquire" DataSourceID="ds" Width="100%">
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="CurrentContracts">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M" />
									<px:PXSegmentMask runat="server" ID="edContractCD1" DataField="ContractCD" AllowEdit="true" />
									<px:PXSelector runat="server" ID="edCustomerID" DataField="CustomerID" />
									<px:PXDropDown runat="server" ID="edStatus1" DataField="Status" />
									<px:PXDateTimeEdit runat="server" ID="edStartDate" DataField="StartDate" />
									<px:PXDateTimeEdit runat="server" ID="edExpireDate" DataField="ExpireDate" />
									<px:PXTextEdit runat="server" ID="edContractDescription1" DataField="Description" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ContractCD" Width="135px" AutoCallBack="true" />
									<px:PXGridColumn DataField="CustomerID" Width="100px" />
									<px:PXGridColumn DataField="Status" Width="100px" />
									<px:PXGridColumn DataField="StartDate" Width="100px" />
									<px:PXGridColumn DataField="ExpireDate" Width="100px" />
									<px:PXGridColumn DataField="Description" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>

