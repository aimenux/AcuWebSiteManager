<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ302000.aspx.cs" Inherits="Page_PJ302000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" EnableAttributes="true" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.ProjectsIssue.PJ.Graphs.ProjectIssueMaint" PrimaryView="ProjectIssue">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="Save" PopupVisible="True" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" />
            <px:PXDSCallbackCommand Name="ProjectIssue$Select_RefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="ProjectIssue$Navigate_ByRefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="ProjectIssue$Attach_RefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="ProjectIssue$ConvertedTo$Link" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="LinkDrawing" Visible="false" />
            <px:PXDSCallbackCommand Name="LinkDrawingLogToEntity" Visible="false" />
            <px:PXDSCallbackCommand Name="UnlinkDrawing" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewAttachments" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewAttachment" Visible="false" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="ProjectIssueContent" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ProjectIssue"
        Caption="PI Summary" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True"
        DefaultControlID="edProjectIssueCd" ActivityIndicator="true">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="3" />
            <px:PXTextEdit ID="edSummary" runat="server" DataField="Summary" CommitChanges="True"/>
            <px:PXSelector ID="edProjectIssueCd" runat="server" DataField="ProjectIssueCd"
                FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" />
            <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" CommitChanges="True" />
            <px:PXSelector ID="edProjectTaskId" runat="server" DataField="ProjectTaskId" CommitChanges="True" AutoRefresh="True"/>
            <PX:PXSelector CommitChanges="True" ID="edClassId" runat="server" DataField="ClassId"
                AllowEdit="True" FilterByAllFields="True" DisplayMode="Hint" TextMode="Search" />
            <pxa:PXRefNoteSelector ID="edRefNoteID" runat="server" DataField="RelatedEntityDescription" NoteIDDataField="RefNoteID"
                MaxValue="0" MinValue="0" CommitChanges="true">
                <EditButton CommandName="ProjectIssue$Navigate_ByRefNote" CommandSourceID="ds" />
                <LookupButton CommandName="ProjectIssue$Select_RefNote" CommandSourceID="ds" />
                <LookupPanel DataMember="ProjectIssue$RefNoteView" DataSourceID="ds" TypeDataField="Type"
                    IDDataField="RefNoteID" />
            </pxa:PXRefNoteSelector>
            <px:PXTextEdit ID="edConvertedTo" runat="server" DataField="ConvertedTo" Enabled="False" AllowEdit="True">
                <LinkCommand Target="ds" Command="ProjectIssue$ConvertedTo$Link" />
            </px:PXTextEdit>
            <px:PXDateTimeEdit ID="edDueDate" runat="server" AlreadyLocalized="False" DataField="DueDate"
                Size="S" DisplayFormat="g" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edPriorityId" runat="server" DataField="PriorityId" CommitChanges="True" DisplayMode="Text" AutoRefresh="True" /> 
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True" AllowNull="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXDateTimeEdit ID="edCreationDate" runat="server" AlreadyLocalized="False"
                DataField="CreationDate_Date" Size="S" />
            <px:PXDateTimeEdit ID="edCreationTime" runat="server" AlreadyLocalized="False"
                DataField="CreationDate_Time" Width="85px" TimeMode="True" SuppressLabel="True" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXSelector DisplayMode="Text" ID="edCreatedById" runat="server" DataField="CreatedById" />
            <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" DisplayMode="Text"
                CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edResolvedOn" runat="server" AlreadyLocalized="False" DataField="ResolvedOn"
                Size="S" DisplayFormat="g" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edProjectIssueTypeId" runat="server" DataField="ProjectIssueTypeId" AutoRefresh="True" />
            <px:PXCheckBox ID="chkIsScheduleImpact" runat="server" AlreadyLocalized="False" DataField="IsScheduleImpact" CommitChanges="True" />
            <px:PXNumberEdit ID="edScheduleImpact" runat="server" AlreadyLocalized="False" DataField="ScheduleImpact" />
            <px:PXCheckBox ID="chkIsCostImpact" runat="server" AlreadyLocalized="False" DataField="IsCostImpact" CommitChanges="True" />
            <px:PXNumberEdit ID="edCostImpact" runat="server" AlreadyLocalized="False" DataField="CostImpact" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="ProjectIssueTabsContent" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Width="100%" DataMember="CurrentProjectIssue"
        NoteIndicator="True" FilesIndicator="True" DefaultControlID="edProjectIssueCd">
        <Items>
            <px:PXTabItem Text="Details" >
            <AutoCallBack Enabled="True" Command="Save" Target="tab">
                <Behavior CommitChanges="True" />
            </AutoCallBack>
                <Template>
                    <px:PXRichTextEdit ID="edDescription" runat="server" DataField="Description" Style="width: 100%; height: 120px"
                        AllowAttached="true" AllowSearch="true" AllowMacros="true" AllowLoadTemplate="false" AllowSourceMode="true">
                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate"
                            ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M" />
                        <AutoSize Enabled="True" MinHeight="120" />
                    </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Related Activities">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" AllowSearch="True"
                        DataMember="Activities" AllowPaging="true" NoteField="NoteText" FilesField="NoteFiles" BorderWidth="0px"
                        GridSkinID="Details" PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
                        BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar ActionsText="true" DefaultAction="cmdViewActivity" PagerVisible="False">
                            <Actions>
                                <AddNew Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                 <RowTemplate>
                                     <px:PXTextEdit ID="edNoteID" runat="server" DataField="NoteID" Visible="False" />
                                     <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent"
                                         InputMask="hh:mm" MaxHours="99" />
                                     <px:PXTimeSpan TimeMode="True" ID="edTimeBillable" runat="server" DataField="TimeBillable"
                                         InputMask="hh:mm" MaxHours="99" />
                                     <px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent"
                                         InputMask="hh:mm" MaxHours="99" />
                                     <px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable"
                                         InputMask="hh:mm" MaxHours="99" />
                                     <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="CostCodeID" AutoRefresh="true"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
                                    <px:PXGridColumn DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" Width="80px" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn DataField="CategoryID" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="TimeSpent" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeSpent" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OvertimeBillable" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False"
                                        SyncVisibility="False" Width="108px" />
                                    <px:PXGridColumn DataField="WorkgroupID" Width="90px" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <CallbackCommands>
                            <Refresh CommitChanges="True" PostData="Page" />
                        </CallbackCommands>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="gridAttributes" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
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
        <AutoSize Container="Window" Enabled="True" />
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
            <px:PXButton ID="PXButton6" runat="server" Text="Link To PI" SyncVisible="false">
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