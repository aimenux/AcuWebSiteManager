<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM302050.aspx.cs" Inherits="Page_SM302050" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
        <script type="text/javascript">
        function commandResult(ds, context) {
            var grid = px_all["ctl00_phG_tab_t0_gridConditions"];
            var subgrid = px_all["ctl00_phG_tab_t1_gridSubscribers"];

            var conditionsRow = null;
            var subscribersRow = null;
            switch (context.command) {
                case "conditionDown":
                    conditionsRow = grid.activeRow.nextRow();
                    break;
                case "conditionUp":
                    conditionsRow = grid.activeRow.prevRow();
                    break;
                case "subscriberDown":
                    subscribersRow = subgrid.activeRow.nextRow();
                    break;
                case "subscriberUp":
                    subscribersRow = subgrid.activeRow.prevRow();
                    break;
            }

            if (conditionsRow)
                conditionsRow.activate();
            if (subscribersRow)
                subscribersRow.activate();
        }
    </script>
<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.BusinessProcess.UI.BusinessProcessEventMaint" SuspendUnloading="False" PrimaryView="Events">
    <CallbackCommands>
        <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        <px:PXDSCallbackCommand Name="viewScreen" StartNewGroup="True" />
        <px:PXDSCallbackCommand Name="viewInquiryParams"  Visible="False" PopupPanel="PanelInquiryParams" RepaintControls="All"/>
        <px:PXDSCallbackCommand Name="viewScheduleHistory"  Visible="False" PopupPanel="pnlScheduleHistory" RepaintControls="All"/>
        <px:PXDSCallbackCommand Name="viewSchedule"  CommitChanges="True"  Visible="False" DependOnGrid="gridSchedules"/>
        <px:PXDSCallbackCommand Name="viewSubscriber"  CommitChanges="True"  Visible="False" DependOnGrid="gridSubscribers"/>
        <px:PXDSCallbackCommand Name="createSubscriber" Visible="False" CommitChanges="True" DependOnGrid="gridSubscribers" PopupCommand="Refresh" PopupCommandTarget="ds" />
        <px:PXDSCallbackCommand Name="createSchedule" Visible="False" RepaintControls="All" CommitChanges="True" PopupCommand="Refresh" PopupCommandTarget="ds"/>
        <px:PXDSCallbackCommand DependOnGrid="gridConditions" Name="conditionUp" Visible="False" />
		<px:PXDSCallbackCommand DependOnGrid="gridConditions" Name="conditionDown" Visible="False" />
        <px:PXDSCallbackCommand DependOnGrid="gridSubscribers" Name="subscriberUp" Visible="False" />
		<px:PXDSCallbackCommand DependOnGrid="gridSubscribers" Name="subscriberDown" Visible="False" />
        <px:PXDSCallbackCommand DependOnGrid="gridInquiryParams" Name="resetToDefaults" Visible="False" />
    </CallbackCommands>
    <DataTrees>
        <px:PXTreeDataMember TreeView="EntityItems" TreeKeys="Key"/>
    </DataTrees>
    <ClientEvents CommandPerformed="commandResult"/>
</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="formBPEvent" runat="server" DataSourceID="ds" Style="z-index: 100" 
                   Width="100%" DataMember="Events" TabIndex="5500">
        <Template>
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" StartRow="True" >
            </px:PXLayoutRule>
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector ID="edName" runat="server" DataField="Name" AutoRefresh="True"  DataSourceID="ds"/>
            <px:PXCheckBox Size="M" runat="server" DataField="Active" ID="PXCheckBox1" AlreadyLocalized="False" />

            <px:PXLayoutRule runat="server" />
            <px:PXDropDown ID="edType" runat="server" DataField="Type" CommitChanges="True">
            </px:PXDropDown>
            <px:PXDropDown ID="edRowProcessingType" runat="server" DataField="RowProcessingType" CommitChanges="True">
            </px:PXDropDown>

            <px:PXLayoutRule runat="server" Merge="True" />
             <px:PXTreeSelector ID="PXTreeSelector1" runat="server" DataField="GroupBy"  CommitChanges="True" AllowEditValue="true"
				TreeDataSourceID="ds" TreeDataMember="EntityItems" MinDropWidth="413" PopulateOnDemand="True" ShowRootNode="False"
                AppendSelectedValue="false" AutoRefresh="true" AutoAdjustColumns="True" >
				<DataBindings>
					<px:PXTreeItemBinding DataMember="EntityItems" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
				</DataBindings>
			</px:PXTreeSelector>
            <px:PXCheckBox runat="server" DataField="IsGroupByOldValue" ID="PXCheckBox2" AlreadyLocalized="False" />

            <px:PXLayoutRule runat="server" ColumnSpan="2">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edDescription" runat="server" AlreadyLocalized="False" DataField="Description" DefaultLocale=""/>
            

            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" >
            </px:PXLayoutRule>
            <px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
            <px:PXTextEdit ID="edScreenIdRO" runat="server" DataField="ScreenIdValue" AlreadyLocalized="False" DefaultLocale=""/>
            
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXLabel ID="PXLabel2" runat="server" AlreadyLocalized="False" Size="SM" ></px:PXLabel>
            <px:PXButton ID="edInquiryParameters" runat="server" Text="INQUIRY PARAMETERS" AlignLeft="false" AlreadyLocalized="False" AutoRefresh="True" AutoAdjustColumns="True" 
                 CommandName="viewInquiryParams" CommandSourceID="ds" PopupPanel="PanelInquiryParams">
             </px:PXButton>
            
            <px:PXLayoutRule runat="server" />
            <px:PXDropDown ID="edFilter" runat="server" DataField="FilterID" CommitChanges="True" />          
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Height="540px" Style="z-index: 100;" Width="100%" DataSourceID="ds" DataMember="CurrentEvent">
        <AutoSize Enabled="true" Container="Window" />
        <Items>
            <px:PXTabItem Text="Trigger Conditions"> 
                <Template>
                    <px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Style="z-index: 100"  AutoRefresh="True" AutoAdjustColumns="True" 
                               Width="100%" Height="100%" AllowSearch="True" SkinID="Details" MatrixMode="true" OnEditorsCreated="grid_EditorsCreated">
                        <Mode InitNewRow="True" AllowRowSelect="False" />
                        <Levels>
                            <px:PXGridLevel  DataMember="TriggerConditions">
                                <Columns>
                                    <px:PXGridColumn DataField="Active" Width="60" Type="CheckBox" TextAlign="Center" AllowResize="false"/>
                                    <px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
                                                     AutoCallBack="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="Operation" Type="DropDownList" Width="200px"
                                                     AutoCallBack="true" />
                                    <px:PXGridColumn DataField="TableName" Width="200px" Type="DropDownList" CommitChanges="True" />
                                    <px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Condition" Type="DropDownList" Width="200px" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="Value" Width="200px" AllowStrings="True"/>
                                    <px:PXGridColumn DataField="Value2" Width="200px" AllowStrings="True"/>
                                    <px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
                                                     Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="90px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
						    <CustomItems>
							  <px:PXToolBarButton CommandName="conditionUp" CommandSourceID="ds" Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="conditionDown" CommandSourceID="ds" Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
						    </CustomItems>
					    </ActionBar>
                    </px:PXGrid>
                    <script type="text/javascript">
                        function cellEditorCreated(gridConditions, ev)
                        {
                            var editor = ev.cell.editor.control;
                            if (editor.__className == "PXSelector")
                            {
                                if (ev.cell.promptChar) editor.setPromptChar(ev.cell.promptChar);
                                editor.textMode = editor.textField ? 1 : 0;
                                if (editor.setHintState) editor.setHintState();
                            }
                        }
                    </script>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Subscribers">
                <Template>
                    <px:PXGrid ID="gridSubscribers" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" AutoRefresh="True" AutoAdjustColumns="True"
                               Width="100%" AllowSearch="True" SkinID="Details" MatrixMode="True" TabIndex="2200" SyncPosition="True" >
                         <ActionBar>
                            <CustomItems>
                                 <px:PXToolBarButton CommandName="subscriberUp" CommandSourceID="ds" Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="subscriberDown" CommandSourceID="ds" Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
                                  <px:PXToolBarButton Text="CREATE SUBSCRIBER">
                                   <AutoCallBack Command="createSubscriber" Target="ds">
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode InitNewRow="True" AllowRowSelect="False"/>
                        <Levels>
                            <px:PXGridLevel DataMember="Subscribers">
                                 <RowTemplate>
                                    <px:PXSelector ID="edHandlerID" runat="server" DataField="HandlerID" AutoRefresh="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Active" Width="60" Type="CheckBox" TextAlign="Center" AllowResize="false"/>
                                    <px:PXGridColumn AllowNull="False" DataField="Type" Type="DropDownList" Width="200px"
                                                     AutoCallBack="true" LinkCommand="viewSubscriber"/>
                                    <px:PXGridColumn DataField="HandlerID" Width="300px" CommitChanges="true"  LinkCommand="viewSubscriber" TextField="Name" DisplayMode="Text"/>
                                    <px:PXGridColumn DataField="StopOnError" Width="120px" Type="CheckBox" TextAlign="Center" AllowResize="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
                        
            <px:PXTabItem Text="Schedules" BindingContext="formBPEvent" VisibleExp="DataControls[&quot;edType&quot;].Value == 1" Visible="false">
                <Template>
                    <px:PXGrid ID="gridSchedules" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" AutoRefresh="True" AutoAdjustColumns="True"
                               Width="100%" AdjustPageSize="Auto" AllowSearch="False" AllowFilter="False" SkinID="DetailsInTab"  MatrixMode="True" SyncPosition="True">
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="VIEW SCHEDULE HISTORY" CommandName="viewScheduleHistory" CommandSourceID="ds" PopupPanel="pnlScheduleHistory">
                                    <AutoCallBack>
                                        <Behavior CommitChanges="True" PostData="Content" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="CREATE SCHEDULE"  CommandName="createSchedule" CommandSourceID="ds">
                                    <AutoCallBack>
                                        <Behavior CommitChanges="True" PostData="Content" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode InitNewRow="True"/>
                        <Levels>
                            <px:PXGridLevel DataMember="Schedules">
                                 <RowTemplate>
                                    <px:PXSelector ID="edScheduleID" runat="server" DataField="ScheduleID" AutoRefresh="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Active" Width="60" Type="CheckBox" TextAlign="Center" AllowResize="false"/>
                                    <px:PXGridColumn DataField="ScheduleID" Width="300" LinkCommand="viewSchedule" CommitChanges="True" TextField="Description" DisplayMode="Text"  TextAlign="Left" />
                                    <px:PXGridColumn DataField="AUSchedule__TimeZoneID" Width="200" />
                                    <px:PXGridColumn DataField="AUSchedule__LastRunDate" Width="100" />
                                    <px:PXGridColumn DataField="AUSchedule__NextRunDate" Width="100"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
    <px:PXSmartPanel ID="PanelInquiryParams" runat="server" style="height:350px;width:500px;" CaptionVisible="True" Caption="Inquiry Parameters" Key="InquiryParameters"
         AutoReload="True" AutoRepaint="True">
         <px:PXGrid ID="gridInquiryParams" runat="server" DataSourceID="ds" Height="150px" SkinID = "ShortList" Width="100%" AutoAdjustColumns="True"
              AllowSearch="False" AllowPaging="False" AllowFilter="False" >
                        <ActionBar ActionsVisible="True">
                            <Actions>
                                <Refresh ToolBarVisible="False"></Refresh>
                                <AddNew ToolBarVisible="False"></AddNew>
                                <Delete ToolBarVisible="False"></Delete>
                            </Actions>
                            <CustomItems>
                                 <px:PXToolBarButton CommandName="resetToDefaults" CommandSourceID="ds">
								</px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="InquiryParameters">
                                <Columns>
                                    <px:PXGridColumn DataField="DisplayName" Width="150px"/>
                                    <px:PXGridColumn DataField="Value" Width="200px" MatrixMode="true" DisplayMode="Text" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="UseDefault" Width="70px" CommitChanges="True" Type="CheckBox" AllowCheckAll="False"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    <Mode AllowAddNew="False" AllowDelete="False" AllowSort="False" InitNewRow="False"></Mode>
                    </px:PXGrid>
        <px:PXPanel ID="PXPanel5" SkinID="Buttons" runat="server">
			<px:PXButton ID="PXInquirySubmit" runat="server" DialogResult="OK" Text="Ok" />
<%--			<px:PXButton ID="PXInquiryCancel" runat="server" DialogResult="Cancel" Text="Cancel" />--%>
		</px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlScheduleHistory" runat="server" style="height:350px;width:1000px;" CaptionVisible="True" Caption="View Schedule History" Key="ScheduleHistory"
                     AutoReload="True" AutoRepaint="True">
        <px:PXGrid ID="gridScheduleHistory" runat="server" DataSourceID="ds" Height="350px" SkinID = "Details" Width="100%" AutoAdjustColumns="True"
                   AllowSearch="False" AllowPaging="True" PageSize="20" AllowFilter="True" >
            <ActionBar>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="ScheduleHistory">
                    <Columns>
                        <px:PXGridColumn DataField="AUScheduleHistory__ScheduleID" TextField="AUScheduleHistory__ScheduleID_Description" />
                        <px:PXGridColumn DataField="AUScheduleHistory__ExecutionDate" Width="100"/>
                        <px:PXGridColumn DataField="AUScheduleHistory__ExecutionResult" />
                        <px:PXGridColumn DataField="Name" />
                        <px:PXGridColumn DataField="Description" />
                        <px:PXGridColumn DataField="ScreenIdValue" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
            <Mode AllowAddNew="False" AllowDelete="False" AllowSort="False" InitNewRow="False" AllowUpdate="False" ></Mode>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" SkinID="Buttons" runat="server">
            <px:PXButton ID="PXScheduleHistoryOk" runat="server" DialogResult="Cancel" Text="Close" />            
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlNewScriptScenario" runat="server" Caption="Create Import Scenario"
                     CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateScriptPanelView" CreateOnDemand="false" AutoCallBack-Enabled="true"
                     AutoCallBack-Target="formNewScriptScenario" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
                     AcceptButtonID="btnActionOK" Width="300px">
        <px:PXFormView ID="formNewScriptScenario" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
                       DataMember="CreateScriptPanelView">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit runat="server" DataField="ScenarioName" ID="PXTextEdit1" AutoRefresh="True" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnActionOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="btnActionCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>