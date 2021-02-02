<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="True" ValidateRequest="False"
    CodeFile="PJ306000.aspx.cs" Inherits="Page_PJ306000" Title="Submittals" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:pxdatasource id="ds" EnableAttributes="true" runat="server" visible="True" typename="PX.Objects.PJ.Submittals.PJ.Graphs.SubmittalEntry"
        primaryview="Submittals" width="100%">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="SendEmail" Visible="True" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="DeleteWorkflowItem" Visible="False" DependOnGrid="gridItems"/>
            <px:PXDSCallbackCommand Name="Print" Visible="True" CommitChanges="True" />
        </CallbackCommands>
    </px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:pxformview id="form" runat="server" datasourceid="ds" style="z-index: 100" width="100%" datamember="Submittals"
        caption="Document Summary" noteindicator="True" filesindicator="True" activityfield="NoteActivity"
        notifyindicator="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edSubmittalID" runat="server" DataField="SubmittalID" />
            <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" AutoRefresh="True" />
            <px:PXDropDown ID="ddStatus" runat="server" DataField="Status"/>
            <px:PXDropDown ID="ddReason" runat="server" DataField="Reason"/>
            <px:PXSelector ID="edTypeID" runat="server" DataField="TypeID"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edSummary" runat="server" DataField="Summary" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M"/>
            <px:PXSelector ID="edProjectId" runat="server" DataField="ProjectId" CommitChanges="true"/>
            <px:PXSelector ID="edProjectTaskId" runat="server" DataField="ProjectTaskId" CommitChanges="true"/>
            <px:PXSelector ID="edCostCodeID" runat="server" DataField="CostCodeID" CommitChanges="true"/>
            <px:PXTextEdit ID="edSpecificationInfo" runat="server" DataField="SpecificationInfo"/>
            <px:PXTextEdit ID="edSpecificationSection" runat="server" DataField="SpecificationSection"/>

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM"/>
            <px:PXDateTimeEdit ID="edDateCreated" runat="server" DataField="DateCreated" CommitChanges="true" Size="SM"/>
            <px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" Size="SM" CommitChanges="true"/>
            <px:PXDateTimeEdit ID="edDateOnSite" runat="server" DataField="DateOnSite" Size="SM"/>
            <px:PXDateTimeEdit ID="edDateClosed" runat="server" DataField="DateClosed" Size="SM"/>
            <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" CommitChanges="True" />
            <px:PXSelector ID="edCurrentWorkflowItemContactID" runat="server" DataField="CurrentWorkflowItemContactID" DisplayMode="Text"/>
            <px:PXTextEdit ID="edDaysOverdue" runat="server" DataField="DaysOverdue"/>
        </Template>
    </px:pxformview>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:pxtab id="tab" runat="server" height="504px" style="z-index: 100;" width="100%" datasourceid="ds"
        datamember="CurrentSubmittal">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Description" LoadOnDemand="True">
                <Template>
                    <px:PXRichTextEdit ID="edDescription" runat="server" Style="border-width: 0px; width: 100%;" DataField="Description" 
			            AllowLoadTemplate="false"  AllowDatafields="false"  AllowMacros="true" AllowSourceMode="true" AllowAttached="true" AllowSearch="true">
		    	        <AutoSize Enabled="True" />
			            <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
		            </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Submittal Workflow">
                <Template>
                    <px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%"
                        ActionsPosition="Top" BorderWidth="0px" SkinID="Details" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True" SyncPosition="True">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="SubmittalWorkflowItems">
                                <RowTemplate>
                                    <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" DisplayMode="Text" TextMode="Search" AllowAddNew="True" AllowEdit="True">
                                         <GridProperties FastFilterFields="DisplayName,FullName,Email" />
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="ContactID" DisplayMode="Text" TextAlign ="Left" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="Contact__FullName"/>
                                    <px:PXGridColumn DataField="Contact__Salutation"/>
                                    <px:PXGridColumn DataField="Role" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="Status" CommitChanges="true" MatrixMode="True"/>
                                    <px:PXGridColumn DataField="StartDate" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="DaysForReview" TextAlign ="Left" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="DueDate" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CompletionDate"/>
                                    <px:PXGridColumn DataField="DateReceived" />
                                    <px:PXGridColumn DataField="DateSent" />
                                    <px:PXGridColumn DataField="Contact__EMail"/>
                                    <px:PXGridColumn DataField="Contact__Phone1"/>
                                    
                                    <px:PXGridColumn DataField="CanDelete" AllowShowHide="False" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" AllowUpload="True" />
                        <ActionBar>
                            <Actions>
                                <Delete ToolBarVisible="False"/>
                            </Actions>
                            <CustomItems>
				                <px:PXToolBarButton ImageKey="RecordDel" DisplayStyle="Image" StateColumn="CanDelete">
				                    <AutoCallBack Command="DeleteWorkflowItem" Target="ds" />
				                </px:PXToolBarButton>
			                </CustomItems>
                        </ActionBar>
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
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:pxtab>
</asp:Content>
