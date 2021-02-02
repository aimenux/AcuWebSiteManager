<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP307100.aspx.cs" Inherits="Page_EP307100" Title="Weekly Crew Time Entry" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EPWeeklyCrewTimeEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="EnterBulkTime" Visible="false" />
			<px:PXDSCallbackCommand Name="InsertForBulkTimeEntry" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="CopySelectedActivity" Visible="false" />
			<px:PXDSCallbackCommand Name="LoadLastWeekActivities" Visible="false" />
			<px:PXDSCallbackCommand Name="LoadLastWeekMembers" Visible="false" />
			<px:PXDSCallbackCommand Name="DeleteMember" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="headerForm" runat="server" DataSourceID="ds" DataMember="Document" Style="z-index: 100" Width="100%">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" />
			<px:PXSelector DataField="WorkgroupID" runat="server" ID="edWorkgroupID" />
			<px:PXSelector DataField="Week" runat="server" ID="edWeek" />

			<px:PXLayoutRule runat="server" StartColumn="true" />
			<px:PXSegmentMask DataField="Filter.ProjectID" runat="server" ID="edProjectID" CommitChanges="true" />
			<px:PXSegmentMask DataField="Filter.ProjectTaskID" runat="server" ID="edProjectTaskID" CommitChanges="true" />
			<px:PXDropDown DataField="Filter.Day" runat="server" ID="edDay" CommitChanges="true" />

			<px:PXLayoutRule runat="server" StartColumn="true" />
			<px:PXLayoutRule runat="server" GroupCaption="Regular" />
			<px:PXTimeSpan DataField="Filter.RegularTime" runat="server" ID="edRegularTime" SummaryMode="true" InputMask="hh:mm" Width="100" LabelWidth="60" />
			<px:PXTimeSpan DataField="Filter.BillableTime" runat="server" ID="edBillableTime" SummaryMode="true" InputMask="hh:mm" Width="100" LabelWidth="60" />
			<px:PXLayoutRule runat="server" EndGroup="true" />

			<px:PXLayoutRule runat="server" StartColumn="true" GroupCaption="Overtime" SuppressLabel="true" />
			<px:PXTimeSpan DataField="Filter.Overtime" runat="server" ID="edOvertime" SummaryMode="true" InputMask="hh:mm" Width="100" />
			<px:PXTimeSpan DataField="Filter.BillableOvertime" runat="server" ID="edBillableOvertime" SummaryMode="true" InputMask="hh:mm" Width="100" />
			<px:PXLayoutRule runat="server" EndGroup="true" />

			<px:PXLayoutRule runat="server" StartColumn="true" GroupCaption="Total" SuppressLabel="true" />
			<px:PXTimeSpan DataField="Filter.TotalTime" runat="server" ID="edTotalTime" SummaryMode="true" InputMask="hh:mm" Width="100" />
			<px:PXTimeSpan DataField="Filter.TotalBillableTime" runat="server" ID="edTotalBillableTime" SummaryMode="true" InputMask="hh:mm" Width="100" />
			<px:PXLayoutRule runat="server" EndGroup="true" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Time Activities">
				<Template>
					<px:PXGrid ID="gridTimeActivities" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" AutoAdjustColumns="true">
						<Levels>
							<px:PXGridLevel DataMember="TimeActivities">
								<RowTemplate>
									<px:PXTimeSpan DataField="TimeSpent" ID="edTimeSpent" runat="server" TimeMode="True" InputMask="hh:mm" />
									<px:PXTimeSpan DataField="TimeBillable" ID="edTimeBillable" runat="server" TimeMode="True" InputMask="hh:mm" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Hold" Type="CheckBox" />
									<px:PXGridColumn DataField="OwnerID" />
									<px:PXGridColumn DataField="ApprovalStatus" />
									<px:PXGridColumn DataField="Date_Date" CommitChanges="true" />
									<px:PXGridColumn DataField="Date_Time" />
									<px:PXGridColumn DataField="EarningTypeID" CommitChanges="true" />
									<px:PXGridColumn DataField="ParentTaskNoteID" />
									<px:PXGridColumn DataField="ContractID" />
									<px:PXGridColumn DataField="ProjectID" CommitChanges="true" />
									<px:PXGridColumn DataField="ProjectTaskID" />
									<px:PXGridColumn DataField="CertifiedJob" />
									<px:PXGridColumn DataField="CostCodeID" />
									<px:PXGridColumn DataField="UnionID" />
									<px:PXGridColumn DataField="LabourItemID" />
									<px:PXGridColumn DataField="WorkCodeID" />
									<px:PXGridColumn DataField="AppointmentID" />
									<px:PXGridColumn DataField="AppointmentCustomerID" />
									<px:PXGridColumn DataField="LogLineNbr" />
									<px:PXGridColumn DataField="ServiceID" />
									<px:PXGridColumn DataField="TimeSpent" CommitChanges="true" />
									<px:PXGridColumn DataField="IsBillable" Type="CheckBox" CommitChanges="true" />
									<px:PXGridColumn DataField="TimeBillable" CommitChanges="true" />
									<px:PXGridColumn DataField="Summary" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowUpload="True" InitNewRow="true" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton>
									<AutoCallBack Command="EnterBulkTime" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton DependOnGrid="gridTimeActivities">
									<AutoCallBack Command="CopySelectedActivity" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton>
									<AutoCallBack Command="LoadLastWeekActivities" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Crew Members">
				<Template>
					<px:PXFormView DataMember="Filter" ID="membersForm" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%">
						<Template>
							<px:PXLayoutRule runat="server" StartRow="True" />
							<px:PXNumberEdit DataField="TotalWorkgroupMembers" ID="edTotalWorkgroupMembers" runat="server" Width="30" LabelWidth="150" />
							<px:PXNumberEdit DataField="TotalWorkgroupMembersWithActivities" ID="edTotalWorkgroupMembersWithActivities" runat="server" Width="30" LabelWidth="150" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="gridWorkgroupMembers" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" AutoAdjustColumns="true" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="WorkgroupTimeSummary">
								<Columns>
									<px:PXGridColumn DataField="ContactID" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="MondayTime" />
									<px:PXGridColumn DataField="TuesdayTime" />
									<px:PXGridColumn DataField="WednesdayTime" />
									<px:PXGridColumn DataField="ThursdayTime" />
									<px:PXGridColumn DataField="FridayTime" />
									<px:PXGridColumn DataField="SaturdayTime" />
									<px:PXGridColumn DataField="SundayTime" />
									<px:PXGridColumn DataField="TotalRegularTime" />
									<px:PXGridColumn DataField="TotalBillableTime" />
									<px:PXGridColumn DataField="TotalOvertime" />
									<px:PXGridColumn DataField="TotalBillableOvertime" />
									<px:PXGridColumn DataField="IsWithoutActivities" Visible="False" AllowShowHide="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<Actions>
								<Delete ToolBarVisible="False" MenuVisible="false" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" DependOnGrid="gridWorkgroupMembers" ImageKey="RecordDel" DisplayStyle="Image" CommandName="DeleteMember" StateColumn="IsWithoutActivities" />
								<px:PXToolBarButton>
									<AutoCallBack Command="LoadLastWeekMembers" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
	<px:PXSmartPanel ID="pnlBulkTimeEntry" runat="server" Caption="Bulk Time Entry" CaptionVisible="True" LoadOnDemand="True" Width="100%" Height="100%" Key="CompanyTreeMembers"
		AutoCallBack-Enabled="true" AutoCallBack-Target="gridTimeActivities" AutoCallBack-Command="Refresh">
		<px:PXSplitContainer runat="server" ID="splitBulkTimeEntry" Orientation="Vertical" SplitterPosition="300">
			<Template1>
				<px:PXFormView DataMember="Filter" ID="bulkForm" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%">
					<Template>
						<px:PXLayoutRule runat="server" StartRow="True" />
						<px:PXCheckBox DataField="ShowAllMembers" ID="chkShowAllMembers" runat="server" CommitChanges="true" AlignLeft="true" />
					</Template>
				</px:PXFormView>
				<px:PXGrid ID="gridCompanyTreeMembers" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" AutoAdjustColumns="true" AllowPaging="false">
					<Levels>
						<px:PXGridLevel DataMember="CompanyTreeMembers">
							<Columns>
								<px:PXGridColumn DataField="Selected" AllowCheckAll="true" Type="CheckBox" TextAlign="Center" Width="20px" />
								<px:PXGridColumn DataField="ContactID" />
								<px:PXGridColumn DataField="Status" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
				</px:PXGrid>
			</Template1>
			<Template2>
				<px:PXGrid ID="gridBulkEntryTimeActivities" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" AutoAdjustColumns="true" FilesIndicator="false" NoteIndicator="false">
					<Levels>
						<px:PXGridLevel DataMember="BulkEntryTimeActivities">
							<RowTemplate>
								<px:PXTimeSpan DataField="TimeSpent" ID="edBulkTimeSpent" runat="server" TimeMode="True" InputMask="hh:mm" />
								<px:PXTimeSpan DataField="TimeBillable" ID="edBulkTimeBillable" runat="server" TimeMode="True" InputMask="hh:mm" />
							</RowTemplate>
							<Columns>
								<px:PXGridColumn DataField="Date_Date" CommitChanges="true" />
								<px:PXGridColumn DataField="Date_Time" />
								<px:PXGridColumn DataField="EarningTypeID" />
								<px:PXGridColumn DataField="ParentTaskNoteID" />
								<px:PXGridColumn DataField="ContractID" />
								<px:PXGridColumn DataField="ProjectID" />
								<px:PXGridColumn DataField="ProjectTaskID" />
								<px:PXGridColumn DataField="CertifiedJob" Type="CheckBox" />
								<px:PXGridColumn DataField="CostCodeID" />
								<px:PXGridColumn DataField="UnionID" />
								<px:PXGridColumn DataField="LabourItemID" />
								<px:PXGridColumn DataField="WorkCodeID" />
								<px:PXGridColumn DataField="AppointmentID" />
								<px:PXGridColumn DataField="AppointmentCustomerID" />
								<px:PXGridColumn DataField="LogLineNbr" />
								<px:PXGridColumn DataField="ServiceID" />
								<px:PXGridColumn DataField="TimeSpent" />
								<px:PXGridColumn DataField="IsBillable" Type="CheckBox" />
								<px:PXGridColumn DataField="TimeBillable" />
								<px:PXGridColumn DataField="Summary" CommitChanges="true" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Container="Parent" Enabled="True" MinHeight="150" MinWidth="500" />
					<Mode InitNewRow="true" />
				</px:PXGrid>
			</Template2>
			<AutoSize Enabled="true" Container="Parent" />
		</px:PXSplitContainer>
		<px:PXPanel ID="pnlBulkTimeEntryBtns" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnAdd" runat="server" Text="Add" CommandName="InsertForBulkTimeEntry" CommandSourceID="ds" />
			<px:PXButton ID="btnAddClose" runat="server" DialogResult="OK" Text="Add & Close">
				<AutoCallBack Command="Commit" Target="ds" />
			</px:PXButton>
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<script>
		window.addEventListener('load', function () {
			px_callback.addHandler(RefreshFilter);
		});

		//Refreshes header filter totals after activities grid filters are used.
		function RefreshFilter(callbackContext) {
			if (callbackContext.info.name == 'Refresh' && callbackContext.controlID.indexOf('gridTimeActivities') >= 0) {
				px_alls.headerForm.refresh();
			}
		};
	</script>
</asp:Content>
