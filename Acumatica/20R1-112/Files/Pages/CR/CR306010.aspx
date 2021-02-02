<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR306010.aspx.cs" Inherits="Page_CR306010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Activities"
		TypeName="PX.Objects.EP.CRActivityMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" Visible="False" />
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" ClosePopup="False"/>
			<px:PXDSCallbackCommand CommitChanges="True" Name="saveClose" Visible="False" PopupVisible="True"
				ClosePopup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="save" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="Delete" ClosePopup="true" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="gotoEntity" Visible="false" />
			<px:PXDSCallbackCommand Name="gotoParentActivity" Visible="false" />
			<px:PXDSCallbackCommand Name="markAsCompleted" CommitChanges="True" PopupVisible="True"
				ClosePopup="True" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="markAsCompletedAndFollowUp" CommitChanges="True" Visible="False" ClosePopup="False" />
            <px:PXDSCallbackCommand Name="Activities$Select_RefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="Activities$Navigate_ByRefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="Activities$Attach_RefNote" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Articles" />
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Activities" NoteIndicator="True"
		FilesIndicator="True" DefaultControlID="edSubject" Width="100%" AllowCollapse="true">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
						<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="M" />
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ColumnSpan="2" />
						<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
						<px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
						<px:PXSelector ID="ddType" runat="server" DataField="Type" CommitChanges="True" DisplayMode="Text" />
						<px:PXCheckBox ID="edIsPrivate" runat="server" DataField="IsPrivate" />
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
						<px:PXDateTimeEdit ID="edStartDate_Date" runat="server" DataField="StartDate_Date"
							CommitChanges="True" />
						<px:PXDateTimeEdit ID="edStartDate_Time" runat="server" DataField="StartDate_Time"
							TimeMode="true" SuppressLabel="true" Width="84" CommitChanges="True" />
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule9" runat="server" LabelsWidth="S" ControlSize="M" />
						<px:PXSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" />
						<px:PXSelector ID="edOwner" runat="server" DataField="OwnerID" CommitChanges="True" AutoRefresh="true" />
						<pxa:PXRefNoteSelector ID="edRefEntity" runat="server" DataField="Source" NoteIDDataField="RefNoteID"
							MaxValue="0" MinValue="0" ValueType="Guid" CommitChanges="true">
							<EditButton CommandName="Activities$Navigate_ByRefNote" CommandSourceID="ds" />
							<LookupButton CommandName="Activities$Select_RefNote" CommandSourceID="ds" />
							<LookupPanel DataMember="Activities$RefNoteView" DataSourceID="ds" TypeDataField="Type"
								IDDataField="RefNoteID" />
						</pxa:PXRefNoteSelector>
						<px:PXSelector runat="server" ID="edParentNoteID" DataField="ParentNoteID" DisplayMode="Text" TextMode="Search" TextField="Subject" AllowEdit="True" CommitChanges="True"/>
						<px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
                             <px:PXSegmentMask ID="edProject" runat="server" DataField="TimeActivity.ProjectID" HintField="description" CommitChanges="True" />
                             <px:PXCheckBox ID="edCertifiedjob" runat="server" DataField="TimeActivity.CertifiedJob" />
						<px:PXLayoutRule ID="PXLayoutRule11" runat="server" />
						<px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="TimeActivity.ProjectTaskID" HintField="description"  AutoRefresh="true" CommitChanges="True"/>
                             <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="TimeActivity.CostCodeID" AutoRefresh="true" CommitChanges="True"/>
                             <px:PXSelector ID="edLaborItemID" runat="server" DataField="TimeActivity.LabourItemID" CommitChanges="True" />
                             <px:PXSelector ID="edUnionID" runat="server" DataField="TimeActivity.UnionID" />
                             <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="SM" />
						<px:PXCheckBox ID="chkTrackTime" runat="server" DataField="TimeActivity.TrackTime" CommitChanges="True" />
						<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="TimeActivity.ApprovalStatus" CommitChanges="True" />
						<px:PXSelector ID="edApprover" runat="server" DataField="TimeActivity.ApproverID" Enabled="False" />
						<px:PXSelector ID="edEType" runat="server" DataField="TimeActivity.EarningTypeID" AutoRefresh="true" CommitChanges="True" />
                              <px:PXSelector ID="edWorkCodeID" runat="server" DataField="TimeActivity.WorkCodeID" />
						<px:PXTimeSpan ID="edTimeSpent" TimeMode="True" runat="server" DataField="TimeActivity.TimeSpent" CommitChanges="True" InputMask="hh:mm" Size="SM" />
						<px:PXTimeSpan TimeMode="true" ID="edOvertimeSpent" runat="server" DataField="TimeActivity.OvertimeSpent" Enabled="False" Size="SM" InputMask="hh:mm"/>
						<px:PXCheckBox ID="chkIsBillable" runat="server" DataField="TimeActivity.IsBillable" Text="Billable" CommitChanges="True" />
                        <px:PXCheckBox ID="chkReleased" runat="server" DataField="TimeActivity.Released" Text="Released"  />
						<px:PXTimeSpan TimeMode="true" ID="edTimeBillable" runat="server" DataField="TimeActivity.TimeBillable" CommitChanges="True" Size="SM" InputMask="hh:mm"/>
						<px:PXTimeSpan TimeMode="true" ID="edOvertimeBillable" runat="server" DataField="TimeActivity.OvertimeBillable" CommitChanges="True" Size="SM" InputMask="hh:mm"/>
						<px:PXTextEdit ID="edNoteID" runat="server" DataField="NoteID" Visible="True">
							<AutoCallBack Command="Cancel" Enabled="True" Target="form" />
						</px:PXTextEdit>
					    <px:PXSelector ID="edARRefNbr" runat="server" DataField="TimeActivity.ARRefNbr" Enabled="False" AllowEdit="True" />
					    
                            
					</px:PXPanel>
				</Template>
	</px:PXFormView>
	<style type="text/css">
		[id$=edNoteID], label[for$=edNoteID] { display: none !important;}
	</style>
	<px:PXFormView ID="PXFormView2" runat="server" DataSourceID="ds" DataMember="CurrentActivity"  RenderStyle="Simple"  >
				<Template>
					<px:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Style="border-width: 0px; border-top-width: 1px;
						width: 100%;" AllowAttached="true" AllowSearch="true" AllowMacros="true" AllowLoadTemplate="false" AllowSourceMode="true">
						<AutoSize Enabled="True" MinHeight="216" />
                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
					</px:PXRichTextEdit>
					</Template>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXFormView>
</asp:Content>
