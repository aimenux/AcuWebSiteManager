<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ303000.aspx.cs" Inherits="Page_PJ303000" Title="Drawing Log" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" EnableAttributes="true" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.DrawingLogs.PJ.Graphs.DrawingLogEntry" PrimaryView="DrawingLog">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewAttachment" Visible="false" />
            <px:PXDSCallbackCommand Name="Link" Visible="false" />
            <px:PXDSCallbackCommand Name="UnLink" Visible="false" />
            <px:PXDSCallbackCommand Name="LinkEntity" Visible="false" />
        </CallbackCommands>
        <ClientEvents Initialize="refreshDrawingsGridAfterFileUploadHandler" />
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="DrawingLogContent" ContentPlaceHolderID="phF" runat="Server">
    <script type="text/javascript">
        var toolsListId = 'ctl00_usrCaption_tlbDataView';
        var showFilesMenu = "FilesMenuShow";
        var uploadFiles = "UploadFiles";

        function refreshDrawingsGridAfterFileUploadHandler(args) {
            __px_all(args)[toolsListId].events.addEventHandler('afterUpload', function() {
                px_alls["PXGridDrawings"].refresh();
            });
        }
       
        function openFilesByGridButton(args, event) {
            if (event.srcArgs.button.key == uploadFiles) {
                __px_all(args)[toolsListId].items.forEach(item => {
                    if (item.commandName == showFilesMenu) {
                        item.exec(showFilesMenu);
                        return;
                    }
                });
            }
        }
    </script>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="DrawingLog" Caption="PI Summary" NoteIndicator="True" FilesIndicator="True"
        LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edDrawingLogCd" ActivityIndicator="True" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edDrawingLogCd" runat="server" DataField="DrawingLogCd" AutoRefresh="True" CommitChanges="True" >
                <GridProperties FastFilterFields="ProjectId, ProjectTaskId, OwnerId, Title, Description, Sketch, Number, Revision, SelectorStatusId, DisciplineId">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" CommitChanges="True" />
            <px:PXSelector ID="edProjectTaskId" runat="server" DataField="ProjectTaskId" CommitChanges="True" AutoRefresh="True"/>
            <px:PXSelector ID="edDisciplineId" runat="server" DataField="DisciplineId" CommitChanges="True" AutoRefresh="True" DisplayMode="Text"/>
            <px:PXSelector ID="edOwnerId" runat="server" DataField="OwnerId" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXTextEdit ID="edNumber" runat="server" AlreadyLocalized="False" DataField="Number" CommitChanges="True"/>
            <px:PXTextEdit ID="edRevision" runat="server" AlreadyLocalized="False" DataField="Revision" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edDrawingDate" runat="server" AlreadyLocalized="False" DataField="DrawingDate" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edReceivedDate" runat="server" AlreadyLocalized="False" DataField="ReceivedDate" CommitChanges="True" />
            <px:PXSelector ID="edStatusId" runat="server" DataField="StatusId" CommitChanges="True"/>
            <px:PXCheckBox ID="chkIsCurrent" runat="server" AlreadyLocalized="False" DataField="IsCurrent" CommitChanges="True" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" StartRow="True" LabelsWidth="SM"/>
            <px:PXTextEdit ID="edTitle" runat="server" AlreadyLocalized="False" DataField="Title" CommitChanges="True" />
            <px:PXTextEdit ID="edDescription" runat="server" AlreadyLocalized="False" DataField="Description" CommitChanges="True" />
            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
            <px:PXTextEdit ID="edSketch" runat="server" AlreadyLocalized="False" DataField="Sketch" CommitChanges="True" />
            <px:PXTextEdit ID="edOriginalDrawingId" runat="server" DataField="OriginalDrawingId" Enabled="False" AllowEdit="True">
                <LinkCommand Target="ds" Command="DrawingLog$OriginalDrawingId$Link" />
            </px:PXTextEdit>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="DrawingLogTabsContent" ContentPlaceHolderID="phG" runat="Server">
        <px:PXTab ID="tab" runat="server" DataSourceID="ds" Width="100%" DataMember="CurrentDrawingLog"
        NoteIndicator="True" FilesIndicator="True" DefaultControlID="edDrawingLogCd">
        <Items>
            <px:PXTabItem Text="Drawings" >
                <Template>
                    <px:PXGrid runat="server" ID="PXGridDrawings" Height="200px" SkinID="Inquire" TabIndex="17500" Width="100%"
                               DataSourceID="ds" SyncPosition="True" FilesIndicator="False" NoteIndicator="False">
                        <AutoSize Enabled="true" />
                        <ActionBar>
                            <Actions>
                                <ExportExcel Enabled="False" />
                            </Actions>
                            <CustomItems >
                                <px:PXToolBarButton Key="UploadFiles" ImageKey="FilesEmpty" Tooltip="Files" />
                            </CustomItems>
                        </ActionBar>
                        <ClientEvents ToolsButtonClick="openFilesByGridButton" />
                        <Levels>
                            <px:PXGridLevel DataMember="Drawings">
                                <Columns>
                                    <px:PXGridColumn DataField="UploadFile__FileName" Width="385px" LinkCommand="ViewAttachment" />
                                    <px:PXGridColumn DataField="Comment" Width="385px" />
                                    <px:PXGridColumn DataField="IsDrawingLogCurrentFile" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Width="120px" DisplayFormat="g" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
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
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" Width="108px" />
                                    <px:PXGridColumn DataField="CategoryID" Width="90px" />
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
            <px:PXTabItem Text="Revisions">
                <Template>
                    <px:PXGrid ID="gridRevisions" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
                               SyncPosition="True" MarkRequired="False"  FilesIndicator="False" NoteIndicator="False">
                        <AutoSize Enabled="true" />
                        <Levels>
                            <px:PXGridLevel DataMember="Revisions">
                                <Columns>
                                    <px:PXGridColumn DataField="DrawingLogCd" LinkCommand="ViewEntity"/>
                                    <px:PXGridColumn DataField="ProjectId" />
                                    <px:PXGridColumn DataField="ProjectTaskId" />
                                    <px:PXGridColumn DataField="DisciplineId" />
                                    <px:PXGridColumn DataField="OwnerId" />
                                    <px:PXGridColumn DataField="Title" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Revision" />
                                    <px:PXGridColumn DataField="Sketch" />
                                    <px:PXGridColumn DataField="OriginalDrawingId" LinkCommand="DrawingLog$OriginalDrawingId$Link"/>
                                    <px:PXGridColumn DataField="Number" />
                                    <px:PXGridColumn DataField="StatusId"/>
                                    <px:PXGridColumn DataField="DrawingDate" />
                                    <px:PXGridColumn DataField="ReceivedDate" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <Mode AllowAddNew="False" AllowUpdate="false" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Relations">
                                <Template>
                    <px:PXGrid ID="PXRelationsGrid" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Inquire" FilesIndicator="False" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="LinkedDrawingLogRelations">
                                <Columns>
                                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                                                     TextAlign="Center" Type="CheckBox" Width="40px" />
                                    <px:PXGridColumn DataField="DocumentCd" LinkCommand="LinkedDrawingLogRelation$DocumentId$Link" />
                                    <px:PXGridColumn DataField="DocumentType" />
                                    <px:PXGridColumn DataField="ProjectId" LinkCommand="ViewEntity" />
                                    <px:PXGridColumn DataField="ProjectTaskId" LinkCommand="ViewEntity" />
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="PriorityId" />
                                    <px:PXGridColumn DataField="Summary" />
                                    <px:PXGridColumn DataField="CreatedById" />
                                    <px:PXGridColumn DataField="OwnerId" />
                                    <px:PXGridColumn DataField="DueDate" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Link" CommandSourceID="ds" CommandName="Link" />
                                <px:PXToolBarButton Text="Unlink" CommandSourceID="ds" CommandName="UnLink" />
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXTab>
        <px:PXSmartPanel ID="PanelAddRelation" runat="server" Width="800px" Height="400px" Key="UnlinkedDrawingLogRelations" CommandSourceID="ds"
        Caption="Link" CaptionVisible="True" LoadOnDemand="True"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="PXRelationsPanelGrid" AutoReload="True">
        <px:PXGrid runat="server" ID="PXRelationsPanelGrid" Height="200px" SkinID="Inquire" TabIndex="17500" Width="100%"
            DataSourceID="ds" FilesIndicator="False" NoteIndicator="False" SyncPosition="True" >
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="UnlinkedDrawingLogRelations">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                            TextAlign="Center" Type="CheckBox" Width="40px" />
                        <px:PXGridColumn DataField="DocumentCd" />
                        <px:PXGridColumn DataField="DocumentType" />
                        <px:PXGridColumn DataField="ProjectId" />
                        <px:PXGridColumn DataField="ProjectTaskId" />
                        <px:PXGridColumn DataField="Status" />
                        <px:PXGridColumn DataField="PriorityId" />
                        <px:PXGridColumn DataField="Summary" />
                        <px:PXGridColumn DataField="CreatedById" />
                        <px:PXGridColumn DataField="OwnerId" />
                        <px:PXGridColumn DataField="DueDate" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
        <px:PXPanel ID="PXButtonPanel" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonJLink" runat="server" Text="Link" SyncVisible="false">
                <AutoCallBack Command="LinkEntity" Target="ds" />
            </px:PXButton>
            <px:PXButton ID="PXButtonLink" runat="server" DialogResult="OK" Text="Link & Close" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="No" Text="Cancel" SyncVisible="false" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>