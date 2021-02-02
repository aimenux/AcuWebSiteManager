<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="WZ201510.aspx.cs"
    Inherits="Page_WZ201510" Title="Untitled Page"  %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.WZ.WizardArticleMaint" PrimaryView="Tasks" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" Visible="False"/>
            <px:PXDSCallbackCommand Name="StartTask"/>
            <px:PXDSCallbackCommand Name="Skip"/>
            <px:PXDSCallbackCommand Name="SkipSubtask" Visible="False" DependOnGrid="subTasksGrid"/>
            <px:PXDSCallbackCommand Name="MarkAsComplete" RepaintControls="None" RepaintControlsIDs="form"/>
            <px:PXDSCallbackCommand Name="GoToScreen"/> 
            <px:PXDSCallbackCommand Name="Assign" CommitChanges="True"/> 
            <px:PXDSCallbackCommand Name="ViewPredecessorTask" Visible="false" DependOnGrid="predecessorsGrid" CommitChanges="True"/> 
            <px:PXDSCallbackCommand Name="ViewBlockedTask" Visible="false" DependOnGrid="successorsGrid" CommitChanges="True"/> 
            <px:PXDSCallbackCommand Name="ViewSubTask" Visible="false" DependOnGrid="subTasksGrid" CommitChanges="True"/> 
            
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
     <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="Tasks" >
                <Items>
                    <px:PXTabItem Text="Summary">
                        <Template>
                            <div style="border: 1px;height: 5px;border-bottom: #BBBBBB 1px solid;"></div>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="Details" 
                                DataFieldKey="TaskID" TextMode="MultiLine" Style="width: 100%; height: 100%;">
                                <AutoSize Enabled="True" />
                            </px:PXHtmlView>
                        </Template>
                    </px:PXTabItem>
					<px:PXTabItem Text="Subtasks">
                        <Template>
                        <px:PXGrid runat="server" ID="subTasksGrid" DataSourceID="ds" Caption="Subtasks" Style="width: 100%;" SkinID="Inquire">
                            <Levels>
                                <px:PXGridLevel DataMember="SubTasks">
                                    <Columns>
                                        <px:PXGridColumn DataField="Status" Width="80px"/>
                                        <px:PXGridColumn DataField="Name" Width="350px" LinkCommand="ViewSubTask"/>
                                        <px:PXGridColumn DataField="AssignedTo" DisplayMode="Text" Width="150px" />
                                        <px:PXGridColumn DataField="CompletedBy" DisplayMode="Text" Width="150px"/>
                                    </Columns>
                                    <Mode AllowAddNew="false" AllowDelete="false" AllowFormEdit="false" AutoInsert="false"/>
                                </px:PXGridLevel>
                            </Levels>
                            <AutoSize Enabled="True" Container="Parent"/>
							<ActionBar>
                                    <CustomItems>
                                        <px:PXToolBarButton Text="Skip" CommandName="SkipSubtask" CommandSourceID="ds" />
                                    </CustomItems>
                                </ActionBar>
                        </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Predecessors">
                        <Template>
                        <px:PXGrid runat="server" ID="predecessorsGrid" DataSourceID="ds" Caption="Depends On" Style="width: 100%;" SkinID="Inquire">
                                <Levels>
                                    <px:PXGridLevel DataMember="Predecessors">
                                        <Columns>
                                            <px:PXGridColumn DataField="WZTask__Status" TextAlign="Left" Width="100px" />
                                            <px:PXGridColumn DataField="PredecessorID" Width="300px" AllowResize="True" LinkCommand="ViewPredecessorTask" />
                                            <px:PXGridColumn DataField="WZTask__AssignedTo" DisplayMode="Text" Width="150px"/>
                                            <px:PXGridColumn DataField="WZTask__CompletedBy" DisplayMode="Text" Width="150px"/>
                                        </Columns>
                                        <Mode AllowAddNew="false" AllowDelete="false" AllowFormEdit="false" AutoInsert="false"/>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" Container="Parent"/>
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Successors">
                        <Template>
                        <px:PXGrid runat="server" ID="successorsGrid" DataSourceID="ds" Caption="Blocked By" Style="width: 100%;" SkinID="Inquire">
                                <Levels>
                                    <px:PXGridLevel DataMember="Successors">
                                        <Columns>
                                            <px:PXGridColumn DataField="WZTask__Status" TextAlign="Left" Width="100px" />
                                            <px:PXGridColumn DataField="TaskID" Width="300px" AllowResize="True" LinkCommand="ViewBlockedTask" />
                                            <px:PXGridColumn DataField="WZTask__AssignedTo" DisplayMode="Text" Width="150px"/>
                                            <px:PXGridColumn DataField="WZTask__CompletedBy" DisplayMode="Text" Width="150px"/>
                                        </Columns>
                                        <Mode AllowAddNew="false" AllowDelete="false" AllowFormEdit="false" AutoInsert="false"/>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" Container="Parent"/>
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Other Info">
                        <Template>
                            <px:PXFormView ID="form" runat="server" Style="z-index: 100;" Width="100%" DataMember="Tasks" SkinID="Preview" CaptionVisible="False" >
	                           <Template>
	                               <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XM"/> 
                                   <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False"/>
                                   <px:PXSelector ID="PXSelector1" runat="server" DataField="AssignedTo" AllowEdit="False"/>
	                               <px:PXSelector ID="edStartedDate" runat="server" DataField="StartedDate"/>
	                               <px:PXSelector ID="edCompletedDate" runat="server" DataField="CompletedDate"/>
                                   <px:PXSelector ID="PXSelector2" runat="server" DataField="CompletedBy" AllowEdit="False"/>
                               </Template>
                           </px:PXFormView>
                        </Template>
                    </px:PXTabItem>
                </Items>
                <AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
     </px:PXTab>   
     <px:PXSmartPanel ID="spAssignDlg" runat="server" Key="CurrentTask" LoadOnDemand="True" ShowAfterLoad="true"
        AutoCallBack-Enabled="true" AutoCallBack-Target="form" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" Width="400"
        AcceptButtonID="btnOk" CancelButtonID="btnCancel" Caption="Assign Task To" CaptionVisible="True">
        <px:PXFormView ID="assignForm" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" 
            SkinID="Transparent" DataMember="CurrentTask">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XL" SuppressLabel="True" />
                    <px:PXSelector ID="edAssignedToTask" runat="server" DataField="AssignedTo" CommitChanges="True" SuppressLabel="True"/>
                </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="PXButton2" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>