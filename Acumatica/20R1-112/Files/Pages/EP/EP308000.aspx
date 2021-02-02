<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP308000.aspx.cs" Inherits="Page_EP308000"
    Title="Equipment Timecard" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="true" Width="100%" TypeName="PX.Objects.EP.EquipmentTimeCardMaint" PrimaryView="Document" PageLoadBehavior="GoLastRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Approve" Visible="false" />
            <px:PXDSCallbackCommand Name="Reject" Visible="false" />
            <px:PXDSCallbackCommand Name="Release" Visible="false" />
            <px:PXDSCallbackCommand Name="Correct" Visible="false" />
            <px:PXDSCallbackCommand Name="Submit" Visible="false" />
            <px:PXDSCallbackCommand Name="Edit" Visible="false" />
            <px:PXDSCallbackCommand Name="PreloadFromPreviousTimecard" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
        ActivityField="NoteActivity" LinkIndicator="True" NotifyIndicator="True" Caption="Document Summary" DefaultControlID="edTimeCardCD" TabIndex="14900">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edTimeCardCD" runat="server" DataField="TimeCardCD" AutoRefresh="True" DataSourceID="ds" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXSelector CommitChanges="True" ID="edWeekID" runat="server" DataField="WeekID" TextMode="Search" DataSourceID="ds" ValueField="WeekID" DisplayMode="Text"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edEquipmentID" runat="server" DataField="EquipmentID" TextMode="Search" NullText="<SELECT>"
                DataSourceID="ds" />
            <px:PXDropDown ID="PXDropDown1" runat="server" DataField="TimecardType" Enabled="False" />
             <px:PXTextEdit runat="server" DataField="OrigTimecardCD" ID="PXTextEdit1" Enabled="False"/>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Setup" StartColumn="True" StartGroup="True" />
            <px:PXTimeSpan runat="server" DataField="TimeSetupCalc" ID="SetupTime" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99"/>
            <px:PXTimeSpan ID="BillableTime" runat="server" DataField="TimeBillableSetupCalc" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99"/>
            
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Run" StartColumn="True" StartGroup="True" />
            <px:PXTimeSpan runat="server" DataField="TimeRunCalc" ID="PXTimeSpan13" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SuppressLabel="true" />
            <px:PXTimeSpan ID="PXTimeSpan14" runat="server" DataField="TimeBillableRunCalc" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SuppressLabel="true"/>
            
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Suspend" StartColumn="True" StartGroup="True" />
            <px:PXTimeSpan runat="server" DataField="TimeSuspendCalc" ID="PXTimeSpan15" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SuppressLabel="true"/>
            <px:PXTimeSpan ID="PXTimeSpan16" runat="server" DataField="TimeBillableSuspendCalc" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SuppressLabel="true"/>
            
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" GroupCaption="Total" StartColumn="True" StartGroup="True" />
            <px:PXTimeSpan runat="server" DataField="TimeTotalCalc" ID="PXTimeSpan17" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SuppressLabel="true"/>
            <px:PXTimeSpan ID="PXTimeSpan18" runat="server" DataField="TimeBillableTotalCalc" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SuppressLabel="true"/>

        </Template>
    </px:PXFormView>
   
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="400px" Style="z-index: 100;" Width="100%">
        <Items>
            <px:PXTabItem Text="Summary">
                <Template>
                    <px:PXGrid ID="gridDetails" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab" RenderDefaultEditors="true"
                        Style="border-right: 0px; border-left: 0px; border-top: 0px" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="Summary">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXSegmentMask SuppressLabel="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="true" />
                                    <px:PXTimeSpan ID="PXMaskEditMon" runat="server" DataField="Mon" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan1" runat="server" DataField="Tue" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan2" runat="server" DataField="Wed" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan3" runat="server" DataField="Thu" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan4" runat="server" DataField="Fri" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan5" runat="server" DataField="Sat" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan6" runat="server" DataField="Sun" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan7" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ProjectID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="RateType" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="Mon" AutoCallBack="true" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Tue" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="Wed" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="Thu" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="Fri" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="Sat" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="Sun" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="IsBillable" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoCallBack Target="gridActivities" Command="Refresh" />
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="true" AllowUpdate="true" AllowDelete="true" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Prelaod From Previos Timecard" Tooltip="Preloads Time from Previos Timecard" CommandName="PreloadFromPreviousTimecard" CommandSourceID="ds" />
                            </CustomItems>
                            <Actions>
                                <AddNew Enabled="true" />
                                <EditRecord Enabled="true" />
                                <Delete Enabled="true" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXGrid ID="gridActivities" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab"
                        Style="border-right: 0px; border-left: 0px; border-bottom: 0px">
                        <Levels>
                            <px:PXGridLevel DataMember="Details">
                                <Columns>
                                    <px:PXGridColumn DataField="Date" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="ProjectID" AutoCallBack="true"/>
                                    <px:PXGridColumn DataField="ProjectTaskID" />
                                     <px:PXGridColumn DataField="CostCodeID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="SetupTime" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="RunTime" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="SuspendTime" AutoCallBack="true" RenderEditorText="True"/>
                                    <px:PXGridColumn DataField="IsBillable" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Description" />
                                    
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edProjectIDDetails" runat="server" DataField="ProjectID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edProjectTaskIDDetails" runat="server" DataField="ProjectTaskID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edCostCodeIDDetails" runat="server" DataField="CostCodeID" AutoRefresh="true" />
                                    <px:PXTimeSpan ID="PXTimeSpan8" runat="server" DataField="SetupTime" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan9" runat="server" DataField="RunTime" InputMask="hh:mm" />
                                    <px:PXTimeSpan ID="PXTimeSpan10" runat="server" DataField="SuspendTime" InputMask="hh:mm" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="true" />
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="false" />
                                <EditRecord Enabled="false" />
                                <Delete Enabled="false" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Approval" DataKeyNames="ApprovalID,AssignmentMapID">
                                <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                    <px:PXGridColumn DataField="ApproveDate" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false"/>
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" />
    </px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
