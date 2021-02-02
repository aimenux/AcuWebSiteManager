<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ305000.aspx.cs" Inherits="Page_PJ305000" Title="Photo Log" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" EnableAttributes="true" runat="server" Visible="True" Width="100%"
                     TypeName="PX.Objects.PJ.PhotoLogs.PJ.Graphs.PhotoLogEntry" PrimaryView="PhotoLog">
        <CallbackCommands>
            <px:PXDSCallbackCommand Visible="False" Name="CreatePhoto" />
            <px:PXDSCallbackCommand Name="DownloadZip" />
            <px:PXDSCallbackCommand Name="ViewPhoto" Visible="False"  DependOnGrid="gridPhotos"/>
            <px:PXDSCallbackCommand Name="EmailPhotoLog" />
            <px:PXDSCallbackCommand Visible="False" Name="CopyPaste" />
            <px:PXDSCallbackCommand Visible="False" Name="NewTask" />
            <px:PXDSCallbackCommand Visible="False" Name="NewEvent" />
            <px:PXDSCallbackCommand Visible="False" Name="NewMailActivity"  />
            <px:PXDSCallbackCommand Visible="False" Name="NewActivity" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="PhotoLogContent" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="PhotoLog"
        Caption="Photo Log" NoteIndicator="True" FilesIndicator="True" DefaultControlID="edPhotoLogCd">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edPhotoLogCd" runat="server" DataField="PhotoLogCd" FilterByAllFields="True"
                AutoRefresh="True" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edDate" runat="server" AlreadyLocalized="False" DataField="Date" Size="S"/>
            <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" CommitChanges="True" />
            <px:PXSelector ID="edProjectTaskId" runat="server" DataField="ProjectTaskId" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edStatusId" runat="server" DataField="StatusId" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edCreatedById" runat="server" DataField="CreatedById" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="PhotoLogTabsContent" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%">
        <Items>
            <px:PXTabItem Text="Photos">
                <Template>
                    <px:PXSplitContainer runat="server" ID="PhotosSplitContainer" SplitterPosition="1000" Orientation="Vertical" Height="100%" Width="100%" >
                        <AutoSize Enabled="True" Container="Window" />
                        <Template1>
                            <px:PXGrid ID="gridPhotos" runat="server" SkinID="Details" Width="100%" DataSourceID="ds" AdjustPageSize="Auto"
                                       AllowPaging="True" TabIndex="300" SyncPosition="True" KeepPosition="True" FilesIndicator="False"
                                       AllowFilter="True" AllowSearch="True" FastFilterFields="PhotoCd, Name, UploadedById, Description" AutoGenerateColumns="Append">
                                <AutoCallBack Target="formPhoto" Command="Refresh" ActiveBehavior="True">
                                    <Behavior CommitChanges="True" RepaintControlsIDs="formPhoto" />
                                </AutoCallBack>
                                <Levels>
                                    <px:PXGridLevel DataMember="Photos">
                                        <Columns>
                                            <px:PXGridColumn DataField="PhotoCd" LinkCommand="viewPhoto"  CommitChanges="True"/>
                                            <px:PXGridColumn DataField="Name" />
                                            <px:PXGridColumn DataField="Description" />
                                            <px:PXGridColumn DataField="UploadedDate" />
                                            <px:PXGridColumn DataField="UploadedById" LinkCommand="ViewEntity" />
                                            <px:PXGridColumn DataField="IsMainPhoto" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn DataField="PhotoLogId" Visible="false" SyncVisible="False" SyncVisibility="False" />
                                            <px:PXGridColumn DataField="ImageUrl" Visible="false" SyncVisible="False" SyncVisibility="False" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Mode AllowAddNew="False" />
                                <AutoSize Enabled="true" />
                                <ActionBar PagerVisible="Bottom">
                                    <PagerSettings Mode="NumericCompact" />
                                    <Actions>
                                        <AddNew Enabled="False" />
                                        <Delete GroupIndex="1" Order="2" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton CommandName="CreatePhoto" CommandSourceId="ds"
                                            Tooltip="Add Row" DisplayStyle="Image" ImageKey="AddNew" >
                                            <ActionBar GroupIndex="1" Order="1" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXFormView ID="formPhoto" runat="server" DataSourceID="ds" Width="100%" DataMember="PhotoImage" >
                                <AutoSize Container="Window" Enabled="True"/>
                                <Template>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                                    <px:PXImageView ID="edImageUrl" runat="server" DataField="ImageUrl" Style="position: absolute; max-height: 550px; max-width: 600px;"/>
                                </Template>
                             </px:PXFormView>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Activities">
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
                                <px:PXToolBarButton Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds" />
                                </px:PXToolBarButton>
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
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="CRReminder__ReminderIcon" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" AllowShowHide="False" Visible="false" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" />
                                    <px:PXGridColumn DataField="CategoryID" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="IsBillable" AllowNull="False" TextAlign="Center" Type="CheckBox" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="OvertimeSpent" RenderEditorText="True" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="TimeBillable" AllowUpdate="False" RenderEditorText="True" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="OvertimeBillable" AllowUpdate="False" RenderEditorText="True" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="WorkgroupID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="Released" Visible="false" SyncVisible="false" />
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
                        <AutoSize Enabled="true" Container="Window"/>
                        <GridMode AllowAddNew="False" AllowFormEdit="False" AllowUpdate="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>