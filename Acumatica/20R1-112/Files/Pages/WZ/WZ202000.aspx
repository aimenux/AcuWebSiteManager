<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	CodeFile="WZ202000.aspx.cs" Inherits="Page_WZ202000" Title="Scenario Tasks"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.WZ.WZTaskEntry" PrimaryView="Scenario"
		PageLoadBehavior="SearchSavedKeys">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="ActivateScenario" />
			<px:PXDSCallbackCommand Name="SuspendScenario" />
			<px:PXDSCallbackCommand Name="CompleteScenario" />
			<px:PXDSCallbackCommand Name="Down" Visible="false" />
			<px:PXDSCallbackCommand Name="Up" Visible="false" />
			<px:PXDSCallbackCommand Name="Left" Visible="false" />
			<px:PXDSCallbackCommand Name="Right" Visible="false" />
			<px:PXDSCallbackCommand Name="addTask" Visible="false" />
			<px:PXDSCallbackCommand Name="DeleteTask" Visible="false" CommitChanges="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="TaskID" TreeView="TasksTreeItems" />
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView runat="server" ID="mapForm" DataSourceID="ds" Width="100%" DataMember="Scenario">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" Merge="True" LabelsWidth="S" ControlSize="XL" />
			<px:PXSelector ID="edScenarioID" runat="server" DataField="ScenarioID" TextField="Name" DataSourceID="ds" />
			<px:PXDropDown ID="edScenarioStatusID" runat="server" DataField="Status" />
		</Template>
	</px:PXFormView>
	<px:PXSplitContainer runat="server" ID="sp0" SplitterPosition="30" PositionInPercent="true">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" PopulateOnDemand="False" ShowRootNode="False" CaptionVisible="False"
				ExpandDepth="4" DataSourceID="ds" Caption="Task Tree" DataMember="TasksTreeItems" SelectFirstNode="True" 
				SyncPosition="True" PreserveExpanded="true"  SyncPositionWithGraph="True" >
				<ToolBarItems>
					<px:PXToolBarButton CommandSourceID="ds" CommandName="addTask" Tooltip="Add task" ImageKey="AddNew" />
					<px:PXToolBarButton CommandSourceID="ds" CommandName="up" Tooltip="Move node up" ImageKey="ArrowUp" />
					<px:PXToolBarButton CommandSourceID="ds" CommandName="down" Tooltip="Move node down" ImageKey="ArrowDown" />
					<px:PXToolBarButton CommandSourceID="ds" CommandName="left" Tooltip="Move left" ImageKey="ArrowLeft" />
					<px:PXToolBarButton CommandSourceID="ds" CommandName="right" Tooltip="Move right" ImageKey="ArrowRight" />
					<px:PXToolBarButton CommandSourceID="ds" CommandName="deleteTask" Tooltip="Delete task" ImageKey="Remove" />
				</ToolBarItems>
				<DataBindings>
					<px:PXTreeItemBinding DataMember="TasksTreeItems" TextField="Name" ValueField="TaskID" />
				</DataBindings>
				<AutoCallBack Target="formTask" Command="Refresh" ActiveBehavior="True">
					<Behavior CommitChanges="True" RepaintControlsIDs="formTask,edDetailsd,taskChilds,taskSuccessors,taskPredecessors,taskFeatures,tab" />
				</AutoCallBack>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXFormView runat="server" DataSourceID="ds" ID="formTask" DataMember="TaskInfo" Caption="Task Summary" 
				CaptionVisible="True" Width="100%" >
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartRow="True" />
					<px:PXTextEdit ID="edTaskName" runat="server" DataField="Name" CommitChanges="True" AutoCallBack="True" />
					<px:PXTextEdit ID="edTaskID" DataField="TaskID" Enabled="False" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartRow="True" />
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" Merge="True" />
					<px:PXDropDown ID="edTaskStatus" runat="server" DataField="Status" Size="S" />
					<px:PXLayoutRule ID="PXLayoutRule9" runat="server" />
					<px:PXCheckBox runat="server" ID="chkOptional" DataField="IsOptional" Size="S" />

					<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" ControlSize="M" />
					<px:PXSelector ID="edAssignedTo" runat="server" DataField="AssignedTo" />
					<px:PXSelector ID="edCompletedBy" runat="server" DataField="CompletedBy" Enabled="False" />
				</Template>
				<Parameters>
					<px:PXControlParam ControlID="tree" Name="TaskID" PropertyName="SelectedValue" />
				</Parameters>
				<CallbackCommands>
					<Refresh RepaintControlsIDs="tree" CommitChanges="True" />
				</CallbackCommands>
			</px:PXFormView>
			<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="TaskInfo" SyncPosition="True">
				<Items>
					<px:PXTabItem Text="Details">
						<Template>
							<px:PXRichTextEdit ID="edDetailsd" runat="server" Style="border-width: 0px; width: 100%;"
								DataField="Details" FilesContainer="message" AllowImageEditor="False" ItemViewName="TasksTreeItems" AllowSearch="true" AllowAttached="true" AllowMacros="true"
								AllowLinkEditor="true" AllowLoadTemplate="false" AllowPlaceholders="true" AllowDatafields="false" CommitChanges="True" AllowSourceMode="true">
								<AutoSize Enabled="True" />
                                <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
							</px:PXRichTextEdit>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Screen Details">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" ControlSize="XL" />
							<px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID" CommitChanges="True" AutoCallBack="True">
								<GridProperties>
									<Columns>
										<px:PXGridColumn DataField="ScreenID" />
										<px:PXGridColumn DataField="Title" />
										<px:PXGridColumn DataField="Path" />
									</Columns>
								</GridProperties>
							</px:PXSelector>
							<px:PXSelector runat="server" ID="edImportScenario" DataField="ImportScenarioID" AutoRefresh="True" />
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Predecessors">
						<Template>
							<px:PXGrid ID="taskPredecessors" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" Height="200px">
								<Levels>
									<px:PXGridLevel DataMember="Predecessors">
										<Columns>
											<px:PXGridColumn DataField="PredecessorID" Width="300px" AllowResize="True" />
											<px:PXGridColumn DataField="WZTask__Status" TextAlign="Left" Width="100px" />
										</Columns>
										<RowTemplate>
											<px:PXSelector ID="edPredecessorID" DataField="PredecessorID" runat="server" AutoRefresh="True"/>
										</RowTemplate>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" MinHeight="200" />
								<ActionBar>
									<Actions>
										<Search Enabled="False" />
									</Actions>
								</ActionBar>
								<Mode InitNewRow="false" AllowDelete="true" />
							</px:PXGrid>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Successors">
						<Template>
							<px:PXGrid ID="taskSuccessors" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" Height="200px" SyncPosition="False">
								<Levels>
									<px:PXGridLevel DataMember="Successors">
										<Columns>
											<px:PXGridColumn DataField="TaskID" Width="300px" AllowResize="True" />
											<px:PXGridColumn DataField="WZTask__Status" TextAlign="Left" Width="100px" />
										</Columns>
										<RowTemplate>
											<px:PXSelector ID="edSuccessorID" DataField="TaskID" runat="server" CommitChanges="True" AutoRefresh="True" />
										</RowTemplate>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" MinHeight="200" />
								<ActionBar>
									<Actions>
										<Search Enabled="False" />
									</Actions>
								</ActionBar>
								<Mode InitNewRow="false" AllowDelete="true" />
							</px:PXGrid>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Subtasks">
						<Template>
							<px:PXGrid ID="taskChilds" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px">
								<Levels>
									<px:PXGridLevel DataMember="Childs">
										<RowTemplate>
											<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
											<px:PXCheckBox ID="chkIsOptional" runat="server" DataField="IsOptional" />
											<px:PXTextEdit ID="edStatus" runat="server" DataField="Status" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="Name" Width="300px" AllowResize="True" />
											<px:PXGridColumn DataField="IsOptional" Width="80px" Type="CheckBox" AllowResize="False" TextAlign="Center" />
											<px:PXGridColumn DataField="Status" TextAlign="Left" Width="100px" />
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
								<Mode AllowColMoving="False" AllowUpdate="False" AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" />
							</px:PXGrid>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Features">
						<Template>
							<px:PXGrid ID="taskFeatures" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" OnRowDataBound="taskFeatures_DataBound">
								<Levels>
									<px:PXGridLevel DataMember="Features">
										<Columns>
											<px:PXGridColumn DataField="Required" Width="80px" Type="CheckBox" AllowResize="False" TextAlign="Center" />
											<px:PXGridColumn DataField="DisplayName" Width="300px" AllowResize="True" />
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
				</Items>
				<AutoSize Enabled="True" />
			</px:PXTab>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
