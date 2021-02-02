<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="CR306015.aspx.cs" Inherits="Page_CR306015"
    Title="Case Activity" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<style type="text/css">
		.invisible
		{
			display: none;
		}
		#ctl00_phF_message_s0_s0 > div > div:nth-child(2) > div
		{
			margin-top: 0;
		}
	</style>
	<px:PXDataSource ID="ds" runat="server" Width="100%" Visible="true" PrimaryView="Message"
        TypeName="PX.Objects.CR.CREmailActivityMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" Visible="False" />
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" ClosePopup="False"/>
            <px:PXDSCallbackCommand CommitChanges="True" Name="saveClose" Visible="False" PopupVisible="True"
                ClosePopup="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="save" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" ClosePopup="true" PopupVisible="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="send" PopupVisible="True" StartNewGroup="True"
                ClosePopup="True" RepaintControls="All" />
            <px:PXDSCallbackCommand PostData="Self" Name="reply" PopupVisible="True" ClosePopup="True" />
            <px:PXDSCallbackCommand PostData="Self" Name="forward" PopupVisible="True" ClosePopup="True" />
            <px:PXDSCallbackCommand Name="gotoEntity" PostData="Page" Visible="false" />
            <px:PXDSCallbackCommand Name="gotoParentActivity" Visible="false" />
            <px:PXDSCallbackCommand Name="loadEmailSource" Visible="false"/>
            <px:PXDSCallbackCommand Name="archive" Visible="false"/>
            <px:PXDSCallbackCommand Name="restoreArchive" Visible="false"/>
            <px:PXDSCallbackCommand Name="Process" Visible="false" />
            <px:PXDSCallbackCommand Name="CancelSending" Visible="false" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="action" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="Message$Select_RefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="Message$Navigate_ByRefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="Message$Attach_RefNote" Visible="False" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="Articles" TreeKeys="PageID" />
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
            <px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />
        </DataTrees>
    </px:PXDataSource>
    <px:PXUploadDialog ID="dlgUpload" runat="server" Height="76px" Style="position: static"
        Width="550px" Caption="File Upload" AllowPostBack="False" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="server">
    <px:PXFormView ID="message" runat="server" DataSourceID="ds" Width="100%"
        DataMember="Message" OverflowY="Hidden" NoteIndicator="true" NoteField="NoteText"
        OnDataBound="on_data_bound" FilesIndicator="true" FilesField="NoteFiles" DefaultControlID="edSubject" OnSelect="message_OnSelect"
        AllowCollapse="false">
        <ContentLayout SpacingSize="Small" AutoSizeControls="true" />
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XS" ColumnWidth="65%" />
            <px:PXTextEdit ID="edNoteID" runat="server" DataField="NoteID" Enabled="false" SuppressLabel="True" CssClass="editor invisible">
                <AutoCallBack Command="Cancel" Enabled="True" Target="message" />
            </px:PXTextEdit>

			<px:PXSelector ID="edMailFrom" runat="server" DataField="MailAccountID" ValueField="EmailAccountID" Required="True"
				DataSourceID="ds" ReadOnly="true" Width="100%" DisplayMode="Text" CommitChanges="true">
                <GridProperties>
                    <Columns>
                        <px:PXGridColumn DataField="Address" />
                        <px:PXGridColumn DataField="Description" />
                    </Columns>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSelector>
            <px:PXTextEdit ID="edMailFromTe" runat="server" DataField="MailFrom" ReadOnly="true"
                Width="100%" />
            <px:PXMultiSelector ID="edMailTo" runat="server" SkinID="email" TextField="SearchSuggestion" ValueField="EMail"
                DataSourceID="ds" DataField="MailTo" Width="100%" AutoGenerateColumns="false" Hidden="True" CommitChanges="True">
                <GridProperties FastFilterFields="DisplayName">
                    <Columns>
                        <px:PXGridColumn DataField="DisplayName" />
                        <px:PXGridColumn DataField="EMail" />
                        <px:PXGridColumn DataField="Salutation" />
                        <px:PXGridColumn DataField="BAccount__AcctCD" />
                        <px:PXGridColumn DataField="FullName" />
                    </Columns>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXMultiSelector>
            <px:PXTextEdit ID="edMailToTe" runat="server" DataField="MailTo" Width="100%" ReadOnly="true"
                Hidden="True" />
            <px:PXMultiSelector ID="edMailCc" runat="server" SkinID="email" DataField="MailCc" Hidden="True"
                TextField="SearchSuggestion" ValueField="EMail" DataSourceID="ds" Width="100%" CommitChanges="True">
                <GridProperties FastFilterFields="DisplayName">
                    <Columns>
                        <px:PXGridColumn DataField="DisplayName" />
                        <px:PXGridColumn DataField="EMail" />
                        <px:PXGridColumn DataField="Salutation" />
                        <px:PXGridColumn DataField="BAccount__AcctCD" />
                        <px:PXGridColumn DataField="FullName" />
                    </Columns>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXMultiSelector>
            <px:PXTextEdit ID="edMailCcTe" runat="server" DataField="MailCc" Hidden="True" Width="100%"
                ReadOnly="true" />
            <px:PXMultiSelector ID="edMailBcc" runat="server" SkinID="email" DataField="MailBcc"
                TextField="SearchSuggestion" ValueField="EMail" DataSourceID="ds" Width="100%" Hidden="True" CommitChanges="True">
                <GridProperties FastFilterFields="DisplayName">
                    <Columns>
                        <px:PXGridColumn DataField="DisplayName" />
                        <px:PXGridColumn DataField="EMail" />
                        <px:PXGridColumn DataField="Salutation" />
                        <px:PXGridColumn DataField="BAccount__AcctCD" />
                        <px:PXGridColumn DataField="FullName" />
                    </Columns>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXMultiSelector>
            <px:PXTextEdit ID="edMailBccTe" runat="server" DataField="MailBcc" Width="100%" ReadOnly="true"
                Hidden="True" />
            <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" Width="570px" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnWidth="35%" StartColumn="True" SuppressLabel="True" />
            <px:PXHtmlView ID="edEntityDescription" runat="server" DataField="EntityDescription"
                TextMode="MultiLine" SkinID="Label" Width="250px" Style="border: solid 1px Gray; background-color: White; height: 132px;">
            </px:PXHtmlView>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Width="100%" DataMember="CurrentMessage">
        <Items>
            <px:PXTabItem Text="Message">
                <Template>
                    <px:PXRichTextEdit ID="PXRichTextEdit1" runat="server" Style="border-width: 0px; width: 100%;" DataField="Body" FilesContainer="message" AllowImageEditor="true"
                        AllowLinkEditor="true" AllowLoadTemplate="false" AllowAttached="true" AllowSearch="true" AllowSourceMode="true"
						DatafieldPreviewGraph="PX.Objects.CR.ContactMaint" DatafieldPreviewView="Contact" >
                        <AutoSize Enabled="True" />
						<LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
                    </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" LabelsWidth="S"
                        ControlSize="M" />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" ColumnSpan="2" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
                    <px:PXDateTimeEdit ID="edStartDate_Date" runat="server" DataField="StartDate_Date"
                        CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edStartDate_Time" runat="server" DataField="StartDate_Time"
                        TimeMode="true" SuppressLabel="true" Width="84" CommitChanges="True" />
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
						<px:PXCheckBox ID="edIsIncome" runat="server" DataField="IsIncome" Enabled="False"/>
                    <px:PXCheckBox ID="edIsPrivate" runat="server" DataField="IsPrivate" />
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" LabelsWidth="S" ControlSize="M" />
						<px:PXSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" />
                    <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" CommitChanges="True" AutoRefresh="true" />
                    <pxa:PXRefNoteSelector ID="edRefEntity" runat="server" DataField="Source" NoteIDDataField="RefNoteID"
                        MaxValue="0" MinValue="0" ValueType="Int64">
                        <EditButton CommandName="Message$Navigate_ByRefNote" CommandSourceID="ds" />
                        <LookupButton CommandName="Message$Select_RefNote" CommandSourceID="ds" />
                        <LookupPanel DataMember="Message$RefNoteView" DataSourceID="ds" TypeDataField="Type"
                            IDDataField="RefNoteID" />
                    </pxa:PXRefNoteSelector>
						<px:PXSelector runat="server" ID="edParentNoteID" DataField="ParentNoteID" DisplayMode="Text" TextMode="Search" TextField="Subject" AllowEdit="True" CommitChanges="True"/>					
					    <%--TimeActivity--%>
						<px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
                             <px:PXSegmentMask ID="edProject" runat="server" DataField="TimeActivity.ProjectID" HintField="description" CommitChanges="True" />
                             <px:PXCheckBox ID="edCertifiedjob" runat="server" DataField="TimeActivity.CertifiedJob" />
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
						<px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="TimeActivity.ProjectTaskID" HintField="description"  AutoRefresh="true" CommitChanges="True"/>
                        <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="TimeActivity.CostCodeID" AutoRefresh="true" />
                     <px:PXSelector ID="edLaborItemID" runat="server" DataField="TimeActivity.LabourItemID" CommitChanges="True" />
                             <px:PXSelector ID="edUnionID" runat="server" DataField="TimeActivity.UnionID" />
						<px:PXDropDown ID="edMPStatus" runat="server" AllowNull="False" DataField="MPStatus" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />

						<px:PXCheckBox ID="chkTrackTime" runat="server" DataField="TimeActivity.TrackTime" CommitChanges="True" />
                    <px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="TimeActivity.ApprovalStatus"
                        CommitChanges="True" />
						<px:PXSelector ID="edApprover" runat="server" DataField="TimeActivity.ApproverID" Enabled="False" />
						<px:PXSelector ID="edEType" runat="server" DataField="TimeActivity.EarningTypeID" AutoRefresh="true" CommitChanges="True" />
						<px:PXSelector ID="edWorkCodeID" runat="server" DataField="TimeActivity.WorkCodeID" />
                               <px:PXTimeSpan TimeMode="true" ID="edTimeSpent" runat="server" DataField="TimeActivity.TimeSpent" CommitChanges="True" Size="SM" InputMask="hh:mm" />
						<px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="TimeActivity.OvertimeSpent" InputMask="hh:mm" Enabled="False"/>
						<px:PXCheckBox ID="chkIsBillable" runat="server" DataField="TimeActivity.IsBillable" Text="Billable"
                        CommitChanges="True" />
						<px:PXCheckBox ID="chkReleased" runat="server" DataField="TimeActivity.Released" Text="Released"  />
						<px:PXTimeSpan TimeMode="True" ID="edTimeBillable1" runat="server" DataField="TimeActivity.TimeBillable" InputMask="hh:mm"/>
						<px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="TimeActivity.OvertimeBillable" InputMask="hh:mm" Enabled="False"/>
						<px:PXTextEdit ID="edTNoteID" runat="server" DataField="TimeActivity.NoteID" Visible="False">
                        <AutoCallBack Command="Cancel" Enabled="True" Target="tab" />
                    </px:PXTextEdit>

                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" Container="Window" />
    </px:PXTab>

    <px:PXSmartPanel ID="PanelSelectNotificatonTemplate" runat="server"
        Style="z-index: 108; position: absolute; left: 27px; top: 99px; height: 155px; width: 355px;"
        Caption="Select Source"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="NotificationInfo" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formSelectNotificationTemplate" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
        <px:PXFormView ID="formSelectNotificationTemplate" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
            Caption="Notification Template" CaptionVisible="False" SkinID="Transparent"
            DataMember="NotificationInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXDropDown CommitChanges="True" ID="edTemplateSource" runat="server" DataField="Type" />
                <px:PXSelector CommitChanges="True" ID="edNtificationName" runat="server" DataField="NotificationName" />
                <px:PXSelector CommitChanges="True" ID="edActivity" runat="server" DataField="TemplateActivity" />
                <px:PXTreeSelector ID="edParent" runat="server" DataField="PageName" PopulateOnDemand="True"
					ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="Folders" CommitChanges="true" >
                    <DataBindings>
						<px:PXTreeItemBinding TextField="Title" ValueField="Name" />
                    </DataBindings>
                </px:PXTreeSelector>
                <px:PXCheckBox CommitChanges="True" ID="chkAppendText" runat="server" DataField="AppendText"/>
            </Template>
        </px:PXFormView>
        <div style="padding: 10px 10px 5px 5px; text-align: right;">
            <px:PXButton ID="PXButtonOK" runat="server" Text="Select" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>
    </px:PXSmartPanel>
</asp:Content>
