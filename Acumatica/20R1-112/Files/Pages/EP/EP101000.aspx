<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP101000.aspx.cs"
	Inherits="Page_EP101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Setup" TypeName="PX.Objects.EP.EPSetupMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="generateWeeks" Visible="false" />
			<px:PXDSCallbackCommand Name="generateWeeksOk" Visible="false" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Articles" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlGenerateWeeks" runat="server" Caption="Generate Weeks"
		CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="GenerateWeeksDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formsGenerateWeeks" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
		AcceptButtonID="cbOk" CancelButtonID="cbCancel" CommandName="GenerateWeeksOK" CommandSourceID="ds" Overflow="Hidden">
		<px:PXFormView ID="formsGenerateWeeks" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="GenerateWeeksDialog">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" CommitChanges="True" />
				<px:PXDateTimeEdit ID="edTillDate" runat="server" DataField="TillDate" CommitChanges="True" />
				<px:PXDropDown ID="edCutOffDayOne" runat="server" DataField="CutOffDayOne" CommitChanges="True" />
				<px:PXTextEdit ID="edDayOne" runat="server" DataField="DayOne" CommitChanges="True" Size="XXS" />
				<px:PXDropDown ID="edCutOffDayTwo" runat="server" DataField="CutOffDayTwo" CommitChanges="True" />
				<px:PXTextEdit ID="edDayTwo" runat="server" DataField="DayTwo" CommitChanges="True" Size="XXS" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
			<px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="568px" Style="z-index: 100" Width="100%" DataMember="Setup" NoteField="">
		<Items>
			<px:PXTabItem Text="General Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" />
					<px:PXSelector ID="edClaimNumberingID" runat="server" DataField="ClaimNumberingID" Text="EPCLAIM" AllowEdit="True"/>
					<px:PXSelector ID="edReceiptNumberingID" runat="server" DataField="ReceiptNumberingID" Text="EPRECEIPT" AllowEdit="True"/>
					<px:PXSelector ID="edTimeCardNumbering" runat="server" DataField="TimeCardNumberingID" AllowEdit="True"/>
					<px:PXSelector ID="edEquipmentTimecardNumberingID" runat="server" DataField="EquipmentTimecardNumberingID" Text="EPETMSHEET" AllowEdit="True"/>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Approval Settings" />
                    <px:PXSelector ID="edClaimDetailsAssignmentMapID" runat="server" DataField="ClaimDetailsAssignmentMapID" TextField="Name" AllowEdit="True"/>
					<px:PXSelector ID="edClaimAssignmentMapID" runat="server" DataField="ClaimAssignmentMapID" TextField="Name" AllowEdit="True"/>
					<px:PXSelector ID="edTimeCardAssignmentMapID" runat="server" DataField="TimeCardAssignmentMapID" TextField="Name" AllowEdit="True"/>
					<px:PXSelector ID="edEquipmentTimecardAssignmentMapID" runat="server" DataField="EquipmentTimecardAssignmentMapID" TextField="Name" AllowEdit="True"/>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Pending Approval Notification" />
                    <px:PXSelector ID="edClaimDetailsAssignmentNotificationID" runat="server" DataField="ClaimDetailsAssignmentNotificationID" TextField="Name" AllowEdit="True"/>
					<px:PXSelector ID="edClaimAssignmentNotificationID" runat="server" DataField="ClaimAssignmentNotificationID" TextField="Name" AllowEdit="True"/>
					<px:PXSelector ID="edTimeCardAssignmentNotificationID" runat="server" DataField="TimeCardAssignmentNotificationID" TextField="Name" AllowEdit="True"/>
					<px:PXSelector ID="edEquipmentTimeCardAssignmentNotificationID" runat="server" DataField="EquipmentTimeCardAssignmentNotificationID" TextField="Name" AllowEdit="True"/>

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Posting Settings" />
					<px:PXDropDown ID="edGroupTransactgion" runat="server" DataField="GroupTransactgion" />
					<px:PXCheckBox ID="chkAutomaticReleaseAR" runat="server" Checked="True" DataField="AutomaticReleaseAR" />
					<px:PXCheckBox ID="chkAutomaticReleaseAP" runat="server" Checked="True" DataField="AutomaticReleaseAP" />
					<px:PXCheckBox ID="chkAutoReleasePM" runat="server" Checked="True" DataField="AutomaticReleasePM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Note And File Handling" />
					<px:PXCheckBox ID="chkCopyNotesAR" runat="server" Checked="True" DataField="CopyNotesAR" />
					<px:PXCheckBox ID="chkCopyFilesAR" runat="server" Checked="True" DataField="CopyFilesAR" />
					<px:PXCheckBox ID="chkCopyNotesAP" runat="server" Checked="True" DataField="CopyNotesAP" />
					<px:PXCheckBox ID="chkCopyFilesAP" runat="server" Checked="True" DataField="CopyFilesAP" />
					<px:PXCheckBox ID="chkCopyNotesPM" runat="server" Checked="True" DataField="CopyNotesPM" />
					<px:PXCheckBox ID="chkCopyFilesPM" runat="server" Checked="True" DataField="CopyFilesPM" />
					<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="M" StartColumn="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Expense Claim Settings" />
					<px:PXSegmentMask ID="edSalesSubMask" runat="server" DataField="SalesSubMask"/>
					<px:PXSegmentMask ID="edExpenseSubMask" runat="server" DataField="ExpenseSubMask"/>
					<px:PXSelector ID="PXSelectorNonTaxableItem" runat="server" DataField="NonTaxableTipItem" CommitChanges="true" AllowEdit="True"/>
					<px:PXCheckBox ID="chkUseReceiptAccountForTips" runat="server" DataField="UseReceiptAccountForTips" />
					<px:PXCheckBox ID="chkHoldEntry" runat="server" Checked="True" DataField="HoldEntry" />
				    <px:PXCheckBox ID="chkPostSummarizedCorpCardExpenseReceipts" runat="server" DataField="PostSummarizedCorpCardExpenseReceipts" />
				    <px:PXCheckBox ID="chkRequireRefNbrInExpenseReceipts" runat="server" DataField="RequireRefNbrInExpenseReceipts" />
					<px:PXCheckBox ID="chkAllowMixedTaxSettingInClaims" runat="server" DataField="AllowMixedTaxSettingInClaims" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Time Reporting Settings" />
					<px:PXCheckBox ID="chkRequireTimes" runat="server" DataField="RequireTimes" />
					<px:PXSelector ID="edDefaultActivityType" runat="server" DataField="DefaultActivityType" CommitChanges="True" />
					<px:PXTimeSpan TimeMode="True" ID="edMinBillableTime" runat="server" DataField="MinBillableTime" InputMask="hh:mm" AllowNull="False" Size="XS" />
					<px:PXSelector ID="edRegularHoursType" runat="server" DataField="RegularHoursType" CommitChanges="True" />
					<px:PXSelector ID="edHolidaysType" runat="server" DataField="HolidaysType" CommitChanges="True" />
					<px:PXSelector ID="edVacationsType" runat="server" DataField="VacationsType" CommitChanges="True" />
					<px:PXCheckBox ID="chkisPreloadHolidays" runat="server" DataField="isPreloadHolidays" />
					<px:PXDropDown runat="server" ID="edPostingoption" DataField="PostingOption" CommitChanges="True"/>
					<px:PXSelector ID="PXSelector1" runat="server" DataField="OffBalanceAccountGroupID" CommitChanges="True" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartGroup="True" GroupCaption="Week Settings" />
					<px:PXCheckBox ID="chkCustomWeek" runat="server" DataField="CustomWeek" CommitChanges="True" />
					<px:PXDropDown runat="server" ID="edFirstDayOfWeek" DataField="FirstDayOfWeek" Size="S" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Custom Week Settings" LoadOnDemand="False" VisibleExp="DataControls[&quot;chkCustomWeek&quot;].Value = 1">
				<Template>
					<px:PXFormView ID="PXFormView4" runat="server" DataMember="WeekFilter" DataSourceID="ds" Style="z-index: 100" Width="100%">
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" />
							<px:PXSelector ID="PXSelector2" runat="server" DataField="Year" CommitChanges="True" />
							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" />
							<px:PXButton ID="btnResetPassword" runat="server" Text="Generate Weeks" CommandName="GenerateWeeks" CommandSourceID="ds" Width="150" Height="20" AutoCallBack="True" />
						</Template>
					</px:PXFormView>
					<px:PXGrid runat="server" ID="PXGridCustomWeek" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" SyncPosition="True" AdjustPageSize="Auto" AllowPaging="True">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="CustomWeek">
								<Columns>
									<px:PXGridColumn DataField="Number" />
									<px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center" />
									<px:PXGridColumn DataField="StartDate" CommitChanges="True" />
									<px:PXGridColumn DataField="EndDate" CommitChanges="True" />
									<px:PXGridColumn DataField="IsFullWeek" Type="CheckBox" TextAlign="Center" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode InitNewRow="True" AutoInsert="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize MinHeight="480" Container="Window" Enabled="True" />
	</px:PXTab>
</asp:Content>
