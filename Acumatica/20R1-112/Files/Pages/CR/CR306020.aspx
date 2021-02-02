<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR306020.aspx.cs" Inherits="Page_CR306020"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Tasks"
		TypeName="PX.Objects.CR.CRTaskMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" Visible="False" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" />
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" ClosePopup="False" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="true"/>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveClose" Visible="False" PopupVisible="True"
				ClosePopup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="gotoParentActivity" Visible="false" />
			<px:PXDSCallbackCommand Name="gotoEntity" PostData="Page" Visible="false" />
			<px:PXDSCallbackCommand Name="complete" StartNewGroup="true" CommitChanges="true"
				PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="completeAndFollowUp" CommitChanges="true" ClosePopup="true" PopupVisible="true"/>
			<px:PXDSCallbackCommand Name="cancelActivity" CommitChanges="true" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds"/>
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True"
				DependOnGrid="gridReferencedActivities" />
			<px:PXDSCallbackCommand Name="ViewTask" CommitChanges="True" DependOnGrid="gridReferencedTasks" />
			<px:PXDSCallbackCommand Name="AddNewRelatedTask" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds"/>
            <px:PXDSCallbackCommand Name="Tasks$Select_RefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="Tasks$Navigate_ByRefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="Tasks$Attach_RefNote" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Width="100%" DataMember="Tasks"
		NoteIndicator="True" FilesIndicator="True" DefaultControlID="edSubject">
		<Items>
			<px:PXTabItem Text="Details" >
				<AutoCallBack Enabled="True" Command="Save" Target="tab"><Behavior CommitChanges="True" /></AutoCallBack>
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
						<px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="NoteID" Visible="False" />
						<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="M" />
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
						<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
						<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate_Date" Size="M" CommitChanges="True"/>
						<px:PXCheckBox ID="edIsPrivate" runat="server" DataField="IsPrivate" />
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
						<px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="EndDate_Date" Size="M" CommitChanges="True"/>
						<px:PXNumberEdit CommitChanges="True" ID="edPercentCompletion" runat="server" DataField="PercentCompletion"
							Size="XS" />
						<px:PXSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" />
						<px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID"
							AutoRefresh="True">
							<Parameters>
								<px:PXControlParam ControlID="tab" Name="CRTask.groupID" PropertyName="NewDataKey[&quot;GroupID&quot;]" />
							</Parameters>
						</px:PXSelector>
                        <px:PXCheckBox CommitChanges="True" ID="chkIsReminderOn" runat="server" DataField="Reminder.IsReminderOn" />
                        <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
                        <px:PXDateTimeEdit CommitChanges="True" ID="edReminderDate_Date" runat="server" DataField="Reminder.ReminderDate_Date" />
                        <px:PXDateTimeEdit CommitChanges="True" ID="edReminderDate_Time" runat="server" DataField="Reminder.ReminderDate_Time" DisplayFormat="g" EditFormat="g" TimeMode="True" SuppressLabel="True" Width="84" />
                        <px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
						<pxa:PXRefNoteSelector ID="edRefNoteID" runat="server" DataField="Source" NoteIDDataField="RefNoteID"
							MaxValue="0" MinValue="0">
							<EditButton CommandName="Tasks$Navigate_ByRefNote" CommandSourceID="ds" />
							<LookupButton CommandName="Tasks$Select_RefNote" CommandSourceID="ds" />
							<LookupPanel DataMember="Tasks$RefNoteView" DataSourceID="ds" TypeDataField="Type"
								IDDataField="RefNoteID" />
						</pxa:PXRefNoteSelector>
						<px:PXSelector runat="server" ID="edParentNoteID" DataField="ParentNoteID" DisplayMode="Text" TextMode="Search" TextField="Subject" AllowEdit="True" CommitChanges="True"/>
						<px:PXSegmentMask ID="edProject" runat="server" DataField="TimeActivity.ProjectID" HintField="description" CommitChanges="True" />
						<px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="TimeActivity.ProjectTaskID" HintField="description"  AutoRefresh="true" CommitChanges="True"/>
                        <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="TimeActivity.CostCodeID" AutoRefresh="true" CommitChanges="True"/>
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="SM" />
						<px:PXDropDown ID="edPriority" runat="server" AllowNull="False" DataField="Priority"
							SelectedIndex="1" />
						<px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False"
							DataField="UIStatus" />
						<px:PXSelector ID="edCategoryID" runat="server" DataField="CategoryID" Size="SM" />
						<px:PXDateTimeEdit ID="edCompletedDate" runat="server" DataField="CompletedDate"
							DisplayFormat="g" EditFormat="g" Enabled="False" Size="SM" />
						<px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeActivity.TimeSpent" InputMask="hh:mm"  MaxHours="99" />
						<px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="TimeActivity.OvertimeSpent" InputMask="hh:mm" MaxHours="99" />
						<px:PXTimeSpan TimeMode="True" ID="edTimeBillable1" runat="server" DataField="TimeActivity.TimeBillable" InputMask="hh:mm" MaxHours="99" />
						<px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="TimeActivity.OvertimeBillable" InputMask="hh:mm" MaxHours="99" />
						<px:PXTextEdit ID="PXNoteID" runat="server" DataField="NoteID" Visible="False">
							<AutoCallBack Command="Cancel" Target="tab" />
						</px:PXTextEdit>
						<px:PXLayoutRule runat="server" StartRow="True" StartGroup="True" GroupCaption="Service Management" LabelsWidth="S" ControlSize="M" />
						<px:PXSelector runat="server" ID="edServiceID" DataField="TimeActivity.ServiceID" AllowEdit="True" />
					</px:PXPanel>
					<px:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Style="width: 100%;
						height: 120px" AllowAttached="true" AllowSearch="true" AllowMacros="true" AllowLoadTemplate="false" AllowSourceMode="true">
                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
						<AutoSize Enabled="True" MinHeight="120" />
					</px:PXRichTextEdit>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Related Activities">
				<Template>
					<px:PXGrid ID="gridReferencedActivities" runat="server" DataSourceID="ds" Height="323px"
						Style="z-index: 100; left: 0px; position: absolute; top: 0px" Width="100%" AllowSearch="True"
						AllowPaging="true" AdjustPageSize="Auto" BorderWidth="0" SkinID="Details" MatrixMode="true">
						<ActionBar DefaultAction="cmdViewActivity">
							<Actions>
								<AddNew Enabled="False" />
								<EditRecord Enabled="false" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Add Email" Key="cmdAddEmail">
									<AutoCallBack Command="NewMailActivity" Target="ds" />
									<PopupCommand Command="Refresh" Target="gridReferencedActivities" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Activity" Key="cmdAddActivity">
									<AutoCallBack Command="NewActivity" Target="ds" />
									<PopupCommand Command="Refresh" Target="gridReferencedActivities" />
									<ActionBar />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdViewActivity" Visible="false">
									<ActionBar MenuVisible="false" />
									<AutoCallBack Command="ViewActivity" Target="ds" />
									<PopupCommand Command="Refresh" Target="gridReferencedActivities" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="ChildActivities">
								<RowTemplate>
								    <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="NoteID" Visible="False" />
					                <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edTimeBillable" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" MaxHours="99" />
                                    <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="CostCodeID" AutoRefresh="true"/>
                                </RowTemplate>                                
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
                                    <px:PXGridColumn DataField="CostCodeID" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
                                    <px:PXGridColumn DataField="CategoryID" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsBillable" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OvertimeBillable" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text"/>
                                </Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Related Tasks">
				<Template>
					<px:PXGrid ID="gridReferencedTasks" runat="server" DataSourceID="ds" Height="323px"
						Style="z-index: 100; left: 0px; position: absolute; top: 0px" Width="100%" AllowSearch="True"
						AllowPaging="true" AdjustPageSize="Auto" BorderWidth="0" SkinID="Details" MatrixMode="true">
						<ActionBar DefaultAction="cmdViewTask">
							<Actions>
							    <AddNew Enabled="False" />
								<EditRecord Enabled="false" />
							</Actions>
							<CustomItems>
							    <px:PXToolBarButton Text="Add Task" Key="cmdAddNewRelatedTask">
									<AutoCallBack Command="AddNewRelatedTask" Target="ds" />
									<PopupCommand Command="Save" Target="tab" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="ReferencedTasks">
								<Columns>
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewTask" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
									<px:PXGridColumn DataField="EndDate" DisplayFormat="g" />
									<px:PXGridColumn DataField="CompletedDateTime" DisplayFormat="g" />
								</Columns>
								<RowTemplate>
									<px:PXSelector runat="server" ID="edRefNoteID" DataField="RefNoteID" FilterByAllFields="True" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode InitNewRow="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" />
	</px:PXTab>
</asp:Content>
