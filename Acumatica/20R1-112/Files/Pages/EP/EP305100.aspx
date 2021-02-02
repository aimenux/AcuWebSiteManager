<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="EP305100.aspx.cs" Inherits="Page_EP305100"
    Title="Time Card" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="true" Width="100%" TypeName="PX.Objects.EP.TimeCardSimpleMaint"
        PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" ClosePopup="true" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Approve" Visible="false" />
            <px:PXDSCallbackCommand Name="Reject" Visible="false" />
            <px:PXDSCallbackCommand Name="Assign" Visible="false" />
            <px:PXDSCallbackCommand Name="Release" Visible="false" />
            <px:PXDSCallbackCommand Name="Copy" Visible="false" />
            <px:PXDSCallbackCommand Name="Correct" Visible="false" />
             <px:PXDSCallbackCommand Name="AddTasks" Visible="False" CommitChanges="True" PostData="Page">
            </px:PXDSCallbackCommand>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Document" NoteIndicator="True" FilesIndicator="True"
        ActivityIndicator="true" ActivityField="NoteActivity" LinkIndicator="true" NotifyIndicator="true"
        Caption="Document Summary" DefaultControlID="edTimeCardCD">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />

            <px:PXSelector ID="edTimeCardCD" runat="server" DataField="TimeCardCD" AutoRefresh="True" />
             <px:PXSelector CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" TextField="AcctCD" ValueField="AcctCD"
                TextMode="Search" NullText="<SELECT>" DataSourceID="ds" />

            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" AllowNull="False" />
            <px:PXSelector CommitChanges="True" ID="edWeekID" runat="server" DataField="WeekID" TextField="FullNumber"
                ValueField="WeekID" TextMode="Search" >
                <GridProperties FastFilterFields="FullNumber">
                    <PagerSettings Mode="NextPrev" TrackPosition="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXMaskEdit ID="edTimeSpent" runat="server" DataField="TimeSpent_byString" Enabled="false" InputMask="### hrs ## mins" /></Template>
    </px:PXFormView>
    
    <px:PXSmartPanel ID="PanelAddTasks" runat="server" Height="296px" Style="z-index: 108;
        left: 216px; position: absolute; top: 171px" Width="473px" Caption="Add Project Tasks"
        CaptionVisible="True" Key="TasksForAddition" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True"
        AutoCallBack-Target="gridAddTasks">
        <px:PXGrid ID="gridAddTasks" runat="server" Height="240px" Width="100%" DataSourceID="ds"
            SkinID="Inquire">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="TasksForAddition">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="ProjectID" DisplayFormat="&gt;aaaaaaaaaa" Label="Project ID" />
                        <px:PXGridColumn DataField="TaskCD" DisplayFormat="&gt;aaaaaaaaaa" Label="Task ID" />
                        <px:PXGridColumn DataField="Description" Label="Description" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="Add " CommandName="AddTasks" CommandSourceID="ds" />
            <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="400px" Style="z-index: 100;" Width="100%">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                  <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                      <AutoSize Enabled="true" />
                      <Template1>
                                <px:PXGrid ID="gridDetails" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab" Caption="Time" RenderDefaultEditors="true"
                                    Style="border-right: 0px; border-left: 0px; border-top: 0px">
                                    <Levels>
                                        <px:PXGridLevel DataMember="Details">
                                            <RowTemplate>
                                                <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

                                                <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="TaskID" AutoRefresh="true" />
                                                <px:PXSegmentMask SuppressLabel="True" ID="edProjectID" runat="server" DataField="ProjectID"
                                                    >
                                                    <Parameters>
                                                        <px:PXSyncGridParam ControlID="gridDetails" />
                                                    </Parameters>
                                                </px:PXSegmentMask></RowTemplate>
                                            <Columns>
                                                <px:PXGridColumn DataField="ProjectID" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="TaskID" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Sun" TimeMode="true" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Mon" TimeMode="true" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Tue" TimeMode="true" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Wed" TimeMode="true" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Thu" TimeMode="true" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Fri" TimeMode="true" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Sat" TimeMode="true" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="TimeSpent_byTimeString" />
                                            </Columns>
                                        </px:PXGridLevel>
                                    </Levels>
                                    <AutoSize Enabled="True" />
                                    <Mode AllowAddNew="true" AllowUpdate="true" AllowDelete="true" />
                                    <ActionBar>
                                        <CustomItems>
                                            <px:PXToolBarButton Text="Add Tasks" PopupPanel="PanelAddTasks" />
                                        </CustomItems>
                                        <Actions>
                                            <AddNew Enabled="true" />
                                            <EditRecord Enabled="true" />
                                            <Delete Enabled="true" />
                                        </Actions>
                                    </ActionBar>
                                </px:PXGrid>
                          </Template1>
                          <Template2>
                                <px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab" Caption="Items" DependsOnControlIDs="gridDetails"
                                    Style="border-right: 0px; border-left: 0px; border-bottom: 0px">
                                    <Levels>
                                        <px:PXGridLevel DataMember="Items">
                                            <RowTemplate>
                                                <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

                                                <px:PXLayoutRule runat="server" Merge="True" />

                                                <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataMember="_InventoryItem_AccessInfo.userName_"  />
                                                <px:PXSegmentMask SuppressLabel="True" Size="s" ID="edProjectTaskID2" runat="server" DataField="TaskID" AutoRefresh="true" />
                                                <px:PXLayoutRule runat="server" Merge="False" />

                                                <px:PXLayoutRule runat="server" Merge="True" />

                                                <px:PXTextEdit Size="s" ID="edDescription" runat="server" DataField="Description"  />
                                                <px:PXSegmentMask SuppressLabel="True" Size="s" ID="edProjectID2" runat="server" DataField="ProjectID"
                                                    >
                                                    <Parameters>
                                                        <px:PXSyncGridParam ControlID="gridDetails" />
                                                    </Parameters>
                                                </px:PXSegmentMask>
                                                <px:PXLayoutRule runat="server" Merge="False" />

                                                <px:PXSelector ID="edUOM" runat="server" DataField="UOM" DataMember="_INUnit_EPTimeCardItem.inventoryID_EPTimeCardItem.inventoryID_"  /></RowTemplate>
                                            <Columns>
                                                <px:PXGridColumn DataField="ProjectID" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="TaskID" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="InventoryID" AutoCallBack="true" />
                                                <px:PXGridColumn DataField="Description" />
                                                <px:PXGridColumn DataField="UOM" AutoCallBack="true" />
                                                <px:PXGridColumn AllowNull="False" DataField="Mon" TextAlign="Right" />
                                                <px:PXGridColumn AllowNull="False" DataField="Tue" TextAlign="Right" />
                                                <px:PXGridColumn AllowNull="False" DataField="Wed" TextAlign="Right" />
                                                <px:PXGridColumn AllowNull="False" DataField="Thu" TextAlign="Right" />
                                                <px:PXGridColumn AllowNull="False" DataField="Fri" TextAlign="Right" />
                                                <px:PXGridColumn AllowNull="False" DataField="Sat" TextAlign="Right" />
                                                <px:PXGridColumn AllowNull="False" DataField="Sun" TextAlign="Right" />
                                                <px:PXGridColumn AllowNull="False" DataField="TotalQty" TextAlign="Right" />
                                            </Columns>
                                        </px:PXGridLevel>
                                    </Levels>
                                    <Mode InitNewRow="true" />
                                    <AutoSize Enabled="True" />
                                </px:PXGrid>
                          </Template2>
                    </px:PXSplitContainer>
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
                               <px:PXGridLevel DataMember="Approval">
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
