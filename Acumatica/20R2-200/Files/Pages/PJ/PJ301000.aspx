<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PJ301000.aspx.cs" Inherits="Page_PJ301000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" EnableAttributes="true" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PJ.RequestsForInformation.PJ.Graphs.RequestForInformationMaint"
        PrimaryView="RequestForInformation">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="Save" PopupVisible="True" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="LinkDrawing" Visible="false" />
            <px:PXDSCallbackCommand Name="LinkDrawingLogToEntity" Visible="false" />
            <px:PXDSCallbackCommand Name="UnlinkDrawing" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewAttachments" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewAttachment" Visible="false" />
            <px:PXDSCallbackCommand Visible="false" Name="RequestForInformation$ConvertedFrom$Link" CommitChanges="True" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="RequestForInformation" Caption="RFI Summary" NoteIndicator="True" FilesIndicator="True"
        LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edRequestForInformationCd" ActivityIndicator="true">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" CommitChanges="True" />
            <px:PXSelector ID="edProjectTaskId" runat="server" DataField="ProjectTaskId" CommitChanges="True" AutoRefresh="True" />
            <px:PXSegmentMask ID="edBusinessAccountId" runat="server" DataField="BusinessAccountId" CommitChanges="True" />
            <px:PXSelector ID="edContactId" runat="server" DataField="ContactId" DisplayMode="Text" CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector CommitChanges="True" ID="edClassId" runat="server" DataField="ClassId" AllowEdit="True" FilterByAllFields="True"
                DisplayMode="Hint" TextMode="Search" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edSummary" runat="server" AlreadyLocalized="False" DataField="Summary" CommitChanges="True" />
            <px:PXCheckBox ID="edIncoming" runat="server" AlreadyLocalized="False" DataField="Incoming" Text="Incoming" CommitChanges="True" />
            <px:PXSelector ID="edIncomingRequestForInformationId" runat="server" AlreadyLocalized="False" DataField="IncomingRequestForInformationId"
                AllowEdit="True" Enabled="False" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True" AllowNull="False" />
            <px:PXDropDown ID="edReason" runat="server" DataField="Reason" CommitChanges="True" AllowNull="False" />
            <px:PXSelector ID="edPriorityId" runat="server" DataField="PriorityId" AllowNull="True" AutoRefresh="True" DisplayMode="Text" CommitChanges="True" />
            <px:PXTextEdit ID="edDocumentationLink" runat="server" AlreadyLocalized="False" DataField="DocumentationLink" />
            <px:PXTextEdit ID="edSpecSection" runat="server" AlreadyLocalized="False" DataField="SpecSection" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
            <px:PXSelector ID="edRequestForInformationCd" runat="server" DataField="RequestForInformationCd" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" AlreadyLocalized="False" DataField="CreationDate" />
            <px:PXSelector ID="edCreatedById" runat="server" DataField="CreatedById" DisplayMode="Text" />
            <px:PXSelector ID="edOwnerId" runat="server" DataField="OwnerId" AutoRefresh="true" DisplayMode="Text" />
            <px:PXDateTimeEdit ID="edDueResponseDate" runat="server" AlreadyLocalized="False" DataField="DueResponseDate"
                Size="M" DisplayFormat="g" CommitChanges="True" />
            <px:PXSelector runat="server" ID="edWorkgroupId" DataField="WorkgroupID" CommitChanges="True" DisplayMode="Text" />
            <px:PXCheckBox ID="edIsScheduleImpact" runat="server" AlreadyLocalized="False" DataField="IsScheduleImpact" CommitChanges="True" />
            <px:PXNumberEdit ID="edScheduleImpact" runat="server" AlreadyLocalized="False" DataField="ScheduleImpact" />
            <px:PXCheckBox ID="edIsCostImpact" runat="server" AlreadyLocalized="False" DataField="IsCostImpact" CommitChanges="True" />
            <px:PXNumberEdit ID="edCostImpact" runat="server" AlreadyLocalized="False" DataField="CostImpact" />
            <px:PXCheckBox ID="edDesignChange" runat="server" AlreadyLocalized="False" DataField="DesignChange" Text="Design Change" />
            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="M"/>
            <px:PXTextEdit ID="edConvertedFrom" runat="server" DataField="ConvertedFrom" Enabled="False" AllowEdit="True">
                <LinkCommand Target="ds" Command="RequestForInformation$ConvertedFrom$Link" />
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
            <px:PXTextEdit ID="edConvertedTo" runat="server" DataField="ConvertedTo" Enabled="False">
                <LinkCommand Target="ds" Command="RequestForInformation$ConvertedTo$Link" />
            </px:PXTextEdit>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="400px" DataSourceID="ds" DataMember="CurrentRequestForInformation" MarkRequired="Dynamic">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <div class="stack-h stack-pout-h">
                        <div class="cell-ph2" style="width: 48%">
                            <div class="groupBox stack-v">
                                <div class="cell-pn note-m" style="height: 24px;">
                                    <div style="float: left; margin-top: 4px;">
                                        <px:PXLabel ID="PXLabelQuestion" runat="server" AlreadyLocalized="False" Text="Question" />
                                    </div>
                                    <div style="clear: none"></div>
                                </div>
                                <div class="cell-pv cell-w note-m">
                                    <px:PXRichTextEdit ID="edRequestDetails" runat="server" DataField="RequestDetails" AllowDatafields="false"
                                        AllowMacros="True" AllowSourceMode="True" AllowAttached="True" AllowSearch="True" AlreadyLocalized="False"
                                        EncodeInstructions="False" ItemViewName="">
                                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate"
                                            ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M" />
                                        <InsertDatafield DataMember="" ImageField="" TextField="" ValueField="" />
                                        <InsertDatafieldPrev DataMember="" ImageField="" TextField="" ValueField="" />
                                        <AutoSize Enabled="True" MinHeight="300" />
                                    </px:PXRichTextEdit>
                                </div>
                            </div>
                        </div>
                        <div class="cell-pn" style="width: 48%">
                            <div class="groupBox stack-v">
                                <div class="cell-pn note-m" style="height: 24px;">
                                    <div style="float: left; margin-top: 4px;">
                                        <px:PXLabel ID="PXLabelAnswer" runat="server" AlreadyLocalized="False" Text="Answer" />
                                    </div>
                                    <div style="float: right">
                                        <px:PXDateTimeEdit ID="edLastModifiedRequestAnswer" runat="server" AlreadyLocalized="False" DataField="LastModifiedRequestAnswer" />
                                    </div>
                                    <div style="clear: none"></div>
                                </div>
                                <div class="cell-pv cell-w note-m">
                                    <px:PXRichTextEdit ID="edRequestAnswer" runat="server" DataField="RequestAnswer" AllowDatafields="false"
                                        AllowMacros="True" AllowSourceMode="True" AllowAttached="True" AllowSearch="True" AlreadyLocalized="False"
                                        EncodeInstructions="False" ItemViewName="" CommitChanges="True">
                                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate"
                                            ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M" />
                                        <InsertDatafield DataMember="" ImageField="" TextField="" ValueField="" />
                                        <InsertDatafieldPrev DataMember="" ImageField="" TextField="" ValueField="" />
                                        <AutoSize Enabled="True" MinHeight="300" />
                                    </px:PXRichTextEdit>
                                </div>
                            </div>
                        </div>
                    </div>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                        Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Attributes">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False"
                                        TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
                                    <px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Activities" LoadOnDemand="true">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Details" PreviewPanelStyle="z-index: 100; background-color: Window"
                        PreviewPanelSkinID="Preview" BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar ActionsText="true" DefaultAction="cmdViewActivity" PagerVisible="False">
                            <Actions>
                                <AddNew Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds"></AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
                                    <px:PXGridColumn DataField="IsFinalAnswer" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" Width="108px" />
                                    <px:PXGridColumn DataField="CategoryID" Width="90px" />
                                    <px:PXGridColumn DataField="WorkgroupID" Width="90px" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <CallbackCommands>
                            <Refresh CommitChanges="True" PostData="Page" />
                        </CallbackCommands>
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label">
                                <AutoSize Container="Parent" Enabled="true" />
                            </px:PXHtmlView>
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowFormEdit="False" AllowUpdate="True" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Relations" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="grdRelations" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Relations">
                                <Columns>
                                    <px:PXGridColumn DataField="Role" Width="120px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="IsPrimary" Width="80px" CommitChanges="True" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Type" Width="120px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DocumentNoteId" DisplayMode="Text" Width="120px" LinkCommand="Relations_TargetDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="BusinessAccountId" Width="160px" AutoCallBack="true" LinkCommand="Relations_EntityDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="BusinessAccountName" Width="200px" />
                                    <px:PXGridColumn DataField="ContactID" Width="160px" AutoCallBack="true" TextAlign="Left" DisplayMode="Text" LinkCommand="Relations_ContactDetails" />
                                    <px:PXGridColumn DataField="ContactEmail" Width="120px" />
                                    <px:PXGridColumn DataField="AddToCC" Width="70px" Type="CheckBox" TextAlign="Center" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector ID="edRelationDocumentNoteId" runat="server" DataField="DocumentNoteId" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edRelationBusinessAccountId" runat="server" DataField="BusinessAccountId" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edRelationContactID" runat="server" DataField="ContactID" FilterByAllFields="True" AutoRefresh="True" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InplaceInsert="False" InitNewRow="True" />
                        <CallbackCommands>
                            <InitRow CommitChanges="True" />
                        </CallbackCommands>
                        <AutoCallBack>
                            <Behavior CommitChanges="True" PostData="Page" />
                        </AutoCallBack>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Drawings" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="PXDrawingsGrid" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Inquire" FilesIndicator="False" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="LinkedDrawingLogs">
                                <Columns>
                                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                                        TextAlign="Center" Type="CheckBox" Width="40px" />
                                    <px:PXGridColumn DataField="DrawingLogCd" LinkCommand="ViewEntity" />
                                    <px:PXGridColumn DataField="ProjectId" LinkCommand="ViewEntity" />
                                    <px:PXGridColumn DataField="ProjectTaskId" LinkCommand="ViewEntity" />
                                    <px:PXGridColumn DataField="DisciplineId" />
                                    <px:PXGridColumn DataField="Number" />
                                    <px:PXGridColumn DataField="Revision" />
                                    <px:PXGridColumn DataField="Sketch" />
                                    <px:PXGridColumn DataField="Title" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="StatusId" />
                                    <px:PXGridColumn DataField="DrawingDate" />
                                    <px:PXGridColumn DataField="ReceivedDate" />
                                    <px:PXGridColumn DataField="OriginalDrawingId" AutoCallBack="True" LinkCommand="DrawingLog$OriginalDrawingId$Link" Width="120px" />
                                    <px:PXGridColumn DataField="OwnerId" />
                                </Columns>
                                <Mode AllowAddNew="false" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Link Drawing" CommandSourceID="ds" CommandName="LinkDrawing" />
                                <px:PXToolBarButton Text="Unlink Drawing" CommandSourceID="ds" CommandName="UnlinkDrawing" />
                                <px:PXToolBarButton Text="View Attachments" CommandSourceID="ds" CommandName="ViewAttachments" />
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
    </px:PXTab>
    <px:PXSmartPanel ID="PanelAddDrawing" runat="server" Width="800px" Height="400px" Key="DrawingLogs" CommandSourceID="ds"
        Caption="Link Drawing" CaptionVisible="True" LoadOnDemand="True"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="PXDrawingLogsGrid" AutoReload="True">
        <px:PXGrid runat="server" ID="PXDrawingLogsGrid" Height="200px" SkinID="Inquire" TabIndex="17500" Width="100%"
            BatchUpdate="True" DataSourceID="ds" FilesIndicator="False" NoteIndicator="False" SyncPosition="True" >
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="DrawingLogs">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                            TextAlign="Center" Type="CheckBox" Width="40px" />
                        <px:PXGridColumn DataField="DrawingLogCd" LinkCommand="ViewEntity" />
                        <px:PXGridColumn DataField="ProjectId" LinkCommand="ViewEntity" />
                        <px:PXGridColumn DataField="ProjectTaskId" LinkCommand="ViewEntity" />
                        <px:PXGridColumn DataField="DisciplineId" />
                        <px:PXGridColumn DataField="Number" />
                        <px:PXGridColumn DataField="Revision" />
                        <px:PXGridColumn DataField="Sketch" />
                        <px:PXGridColumn DataField="Title" />
                        <px:PXGridColumn DataField="Description" />
                        <px:PXGridColumn DataField="StatusId" />
                        <px:PXGridColumn DataField="DrawingDate" />
                        <px:PXGridColumn DataField="ReceivedDate" />
                        <px:PXGridColumn DataField="OriginalDrawingId" AutoCallBack="True" LinkCommand="DrawingLog$OriginalDrawingId$Link" Width="120px" />
                        <px:PXGridColumn DataField="OwnerId" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton6" runat="server" Text="Link To RFI" SyncVisible="false">
                <AutoCallBack Command="LinkDrawingLogToEntity" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButton7" runat="server" DialogResult="OK" Text="Link & Close" />
            <px:PXButton ID="PXButton8" runat="server" DialogResult="No" Text="Cancel" SyncVisible="false" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelViewAttachments" runat="server" Width="800px" Height="400px" Key="DrawingLogsAttachments" CommandSourceID="ds"
        Caption="Drawing Log Document Attachments" CaptionVisible="True" LoadOnDemand="True"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="PXDrawingLogsGrid" AutoReload="True">
        <px:PXGrid runat="server" ID="GridDrawingLogsAttachments" Height="200px" SkinID="Inquire" TabIndex="17500" Width="100%"
            DataSourceID="ds" FilesIndicator="False" NoteIndicator="False">
            <AutoSize Enabled="true" />
            <ActionBar>
                <Actions>
                    <ExportExcel Enabled="False" />
                </Actions>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="DrawingLogsAttachments">
                    <Columns>
                        <px:PXGridColumn DataField="FileName" Width="370px" AutoCallBack="True" LinkCommand="ViewAttachment" />
                        <px:PXGridColumn DataField="DrawingLog__DrawingLogCd" LinkCommand="ViewEntity" Width="100px" />
                        <px:PXGridColumn DataField="UploadFileRevision__Comment" Width="370px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
    </px:PXSmartPanel>
</asp:Content>
