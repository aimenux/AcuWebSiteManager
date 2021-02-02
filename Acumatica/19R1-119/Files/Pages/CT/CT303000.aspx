<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CT303000.aspx.cs" Inherits="Page_CT303000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.Objects.CT.UsageMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
			<px:PXDSCallbackCommand Name="Delete" Visible="False" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" Visible="False" />
			<px:PXDSCallbackCommand Name="Previous" Visible="False" />
			<px:PXDSCallbackCommand Name="Next" Visible="False" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%"
		DataMember="Filter" DataSourceID="ds" Caption="Contract" DefaultControlID="edContractID">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />

			<px:PXSelector CommitChanges="True" ID="edContractID" runat="server" DataField="ContractID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataMember="CurrentContract"
		DataSourceID="ds">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Unbilled Transactions">
				<Template>
					<px:PXGrid runat="server" ID="UnbilledGrid" Height="100%" Width="100%" DataSourceID="ds"
						SkinID="DetailsInTab" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="true">
						<AutoSize Enabled="True" />
						<Mode InitNewRow="True" AllowUpload="True" />
						<Levels>
							<px:PXGridLevel DataKeyNames="TranID" DataMember="UnBilled">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />

									<px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSegmentMask ID="edBAccountID2" runat="server" DataField="BAccountID" />
									<px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" />
									<px:PXSegmentMask ID="edInventoryID2" runat="server" DataField="InventoryID" />
									<px:PXTextEdit ID="edDescription2" runat="server" DataField="Description" />
									<px:PXSelector ID="edUOM2" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edBillableQty2" runat="server" DataField="BillableQty" />
									<px:PXSelector ID="edCRCase__CaseCD" runat="server" DataField="CRCase__CaseCD" AllowEdit="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BranchID" Label="Branch" Width="81px" />
									<px:PXGridColumn DataField="InventoryID" Label="Inventory ID" Width="108px" AutoCallBack="true" />
									<px:PXGridColumn DataField="Description" Label="Description" Width="108px" />
									<px:PXGridColumn DataField="UOM" Label="UOM" Width="108px" />
									<px:PXGridColumn DataField="BillableQty" Label="BillableQty" TextAlign="Right" Width="108px" />
									<px:PXGridColumn DataField="Date" Label="Start Date" Width="90px" />
									<px:PXGridColumn DataField="CRCase__CaseCD" Width="90px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Transactions History">
				<Template>
					<px:PXFormView ID="UsageFilterForm" runat="server" Width="100%" Caption="Selection"
						DataMember="Filter" DataSourceID="ds" SkinID="Transparent">
						<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXSelector CommitChanges="True" ID="edInvFinPeriodID" runat="server" DataField="InvFinPeriodID" />
						</Template>
					</px:PXFormView>
					<px:PXGrid runat="server" ID="BilledGrid" Width="100%" DataSourceID="ds"
						SkinID="DetailsWithFilter" AdjustPageSize="Auto" AllowPaging="True">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="Billed">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edBAccountID" runat="server" DataField="BAccountID" />
									<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" />
									<px:PXDateTimeEdit ID="edBilledDate" runat="server" DataField="BilledDate" />
									<px:PXSegmentMask ID="edBranchID2" runat="server" DataField="BranchID" />
									<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
									<px:PXSelector ID="edARRefNbr" runat="server" DataField="ARRefNbr" AllowEdit="true" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
									<px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edBillableQty" runat="server" DataField="BillableQty" />
									<px:PXDateTimeEdit ID="edStartDate2" runat="server" DataField="StartDate" />
									<px:PXDateTimeEdit ID="edEndDate2" runat="server" DataField="EndDate" />
									<px:PXTextEdit ID="edARTranType" runat="server" DataField="ARTranType" />
									<px:PXSelector ID="PXSelector1" runat="server" DataField="CRCase__CaseCD" AllowEdit="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BranchID" Label="Branch" Width="81px" />
									<px:PXGridColumn DataField="InventoryID" Label="Inventory ID" Width="108px" />
									<px:PXGridColumn DataField="Description" Label="Description" Width="208px" />
									<px:PXGridColumn DataField="UOM" Label="UOM" Width="108px" />
									<px:PXGridColumn DataField="BillableQty" Label="Billable Qty" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="StartDate" Label="Start Date" Width="90px" />
									<px:PXGridColumn DataField="EndDate" Label="End Date" Width="90px" />
									<px:PXGridColumn DataField="Date" Width="90px" />
									<px:PXGridColumn DataField="ARTranType" Width="90px" />
									<px:PXGridColumn DataField="ARRefNbr" Width="108px" />
									<px:PXGridColumn DataField="BilledDate" Width="90px" />
									<px:PXGridColumn DataField="CRCase__CaseCD" Width="90px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
