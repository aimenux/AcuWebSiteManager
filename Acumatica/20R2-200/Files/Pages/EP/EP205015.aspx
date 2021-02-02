<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP205015.aspx.cs"
	Inherits="Page_EP205015" Title="Untitled Page" %>
<%@ Register TagPrefix="px" Namespace="PX.Web.UI" Assembly="PX.Web.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=3b136cac2f602b8e" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<style type="text/css">
		.treeFolder0 {
			font-size: 11pt;
			text-transform: none;
			color: RGBA(0,0,0,0.54) !important;
			cursor: pointer;
		}
	</style>
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="AssigmentMap" TypeName="PX.Objects.EP.EPApprovalMapMaint"
        PageLoadBehavior="InsertRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Up" Visible="False" DependOnGrid="topGrid" />
            <px:PXDSCallbackCommand Name="Down" Visible="False" DependOnGrid="topGrid" />
			<px:PXDSCallbackCommand Name="DeleteRoute" Visible="false" />
			<px:PXDSCallbackCommand Name="AddStep" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="AddRule" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="bottomGrid" Name="ConditionUp" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="bottomGrid" Name="ConditionDown" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="bottomGrid" Name="ConditionInsert" Visible="False" />
        </CallbackCommands>
        <DataTrees>
			<px:PXTreeDataMember TreeKeys="RuleID" TreeView="NodesTree" />
			<px:PXTreeDataMember TreeKeys="Key" TreeView="CacheTree" />
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
			<px:PXTreeDataMember TreeKeys="Key" TreeView="EntityItems" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView runat="server" ID="mapForm" DataSourceID="ds" Width="100%" DataMember="AssigmentMap" 
        Caption="Assignment Rules Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        DefaultControlID="edName">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" NullText="<NEW>" DataSourceID="ds" CommitChanges="true"/>
            <px:PXTextEdit CommitChanges="True" ID="edName" runat="server" DataField="Name" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXDropDown CommitChanges="True" ID="edGraphType" runat="server" DataField="GraphType" AutoRefresh="true" />
        </Template>
    </px:PXFormView>
	<px:PXSplitContainer runat="server" ID="sp0" SplitterPosition="270" Style="background-color: #eeeeee;" >
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="500px" PopulateOnDemand="True" ShowRootNode="False"
					ExpandDepth="1" AutoRepaint="true" Caption="Steps" AllowCollapse="true"
					CommitChanges="true" MatrixMode="true" SyncPosition="True" SyncPositionWithGraph="True">
				<ToolBarItems>
					<px:PXToolBarButton Text="Add Step">
						<AutoCallBack Command="AddStep" Enabled="True" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton DisplayStyle="Image" Tooltip="Add Rule">
						<AutoCallBack Command="AddRule" Enabled="True" Target="ds" />
						<Images Normal="main@AddNew" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Text="Up" DisplayStyle="Image" Tooltip="Move Node Up">
						<AutoCallBack Command="Up" Enabled="True" Target="ds" />
						<Images Normal="main@ArrowUp" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Text="Down" DisplayStyle="Image" Tooltip="Move Node Down">
						<AutoCallBack Command="Down" Enabled="True" Target="ds" />
						<Images Normal="main@ArrowDown" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Delete Node" ImageKey="Remove">
						<AutoCallBack Command="DeleteRoute" Target="ds" />
						<Images Normal="main@RecordDel" />
					</px:PXToolBarButton>
				</ToolBarItems>
				<AutoCallBack Target="formRuleType" Command="Refresh" Enabled="True" />
				<AutoCallBack Target="bottomGrid" Command="Refresh" Enabled="True" />
				<AutoCallBack Target="PXFormConditions" Command="Refresh" Enabled="true" />
				<AutoSize Enabled="True" MinHeight="300" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="NodesTree" TextField="ExtName" ValueField="RuleID" ImageUrlField="Icon" />
				</DataBindings>
			</px:PXTreeView>
		</Template1>
        <Template2>
			<px:PXTab ID="tab" runat="server" Height="126px" Style="z-index: 100;" Width="100%">
				<AutoSize Enabled="True" Container="Parent" MinHeight="180"></AutoSize>
				<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
				<Items>
					<px:PXTabItem Text="Conditions">
						<Template>
							<px:PXFormView ID="formRuleType" runat="server" Width="100%" DataMember="CurrentNode" DataSourceID="ds" SkinID="Transparent">
								<AutoSize Container="Window" />
								<Template>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
									<px:PXTextEdit ID="edRuleName" CommitChanges="True" runat="server" DataField="Name" AutoRefresh="true" />
									<px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" CommitChanges="True" />
									<px:PXDropDown ID="edEmptyStepType" CommitChanges="True" runat="server" DataField="EmptyStepType" AllowNull="False" />
									<px:PXDropDown ID="edExecuteStep" runat="server" DataField="ExecuteStep" AllowNull="False" />
								</Template>
							</px:PXFormView>
							<px:PXGrid ID="bottomGrid" runat="server" SkinID="Details" DataSourceID="ds" ActionsPosition="Top" Width="100%"
								MatrixMode="true" SyncPosition="true" SyncPositionWithGraph="true" CaptionVisible="false" Style="z-index: 101;" AllowPaging="false">
								<Levels>
									<px:PXGridLevel DataMember="Rules">
										<Columns>
											<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" AutoCallBack="true" />
											<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" AllowSort="False" Type="DropDownList" AutoCallBack="true" />
											<px:PXGridColumn Type="DropDownList" DataField="Entity" AllowSort="False" AutoCallBack="True" AllowResize="true" />
											<px:PXGridColumn AutoCallBack="True" Type="DropDownList" AllowSort="False" DataField="FieldName" AllowResize="true" />
											<px:PXGridColumn AllowNull="False" DataField="Condition" AllowSort="False" Type="DropDownList" AllowResize="true" />
											<px:PXGridColumn DataField="Value" AllowResize="true" AllowSort="False" />
											<px:PXGridColumn DataField="Value2" AllowResize="true" AllowSort="False" />
											<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" AllowSort="False" Type="DropDownList" />
											<px:PXGridColumn AllowNull="False" DataField="Operator" AllowSort="False" Type="DropDownList" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
							    <AutoSize Enabled="true" Container="Parent"/>
								<ActionBar>
									<Actions>
										<Search Enabled="False" />
										<EditRecord Enabled="False" />
										<NoteShow Enabled="False" />
										<FilterShow Enabled="False" />
										<FilterSet Enabled="False" />
										<ExportExcel Enabled="False" />
									</Actions>
									<CustomItems>
										<px:PXToolBarButton CommandName="ConditionInsert" CommandSourceID="ds" Text="Insert" />
										<px:PXToolBarButton CommandName="ConditionUp" CommandSourceID="ds" Tooltip="Move Up">
											<Images Normal="main@ArrowUp" />
										</px:PXToolBarButton>
										<px:PXToolBarButton CommandName="ConditionDown" CommandSourceID="ds" Tooltip="Move Down">
											<Images Normal="main@ArrowDown" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
								<CallbackCommands>
									<Refresh PostData="Page" RepaintControlsIDs="formRuleType" />
								</CallbackCommands>
								<Mode InitNewRow="True" />
							</px:PXGrid>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Rule Actions" RepaintOnDemand="false">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="M" ControlSize="XM" GroupCaption="Approval Settings" />
							<px:PXFormView ID="PXFormConditions" runat="server" Width="100%" DataMember="CurrentNode" DataSourceID="ds" SkinID="Transparent">
								<Template>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXDropDown CommitChanges="True" ID="edRuleType" runat="server" DataField="RuleType" AllowNull="False"/>
								</Template>
							</px:PXFormView>
							<px:PXGrid ID="employeeApproveGrid" runat="server" SkinID="Details" DataSourceID="ds" ActionsPosition="Top" Height="110px" Width="100%"
								MatrixMode="true" CaptionVisible="false" Style="z-index: 101;" AllowPaging="false" >
								<Levels>
									<px:PXGridLevel DataMember="EmployeeCondition">
										<Columns>
											<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" AutoCallBack="true" />
											<px:PXGridColumn Type="DropDownList" DataField="Entity" AutoCallBack="True" AllowResize="true" />
											<px:PXGridColumn AutoCallBack="True" Type="DropDownList" DataField="FieldName" AllowResize="true" />
											<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" AllowResize="true" />
											<px:PXGridColumn DataField="IsField" Type="CheckBox" TextAlign="Center" AutoCallBack="True"/>
											<px:PXGridColumn DataField="Value" AllowResize="true" />
											<px:PXGridColumn DataField="Value2" AllowResize="true" />
											<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList" />
											<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="true" Container="Window"/>
								<ActionBar>
									<Actions>
										<Search Enabled="False" />
										<EditRecord Enabled="False" />
										<NoteShow Enabled="False" />
										<FilterShow Enabled="False" />
										<FilterSet Enabled="False" />
										<ExportExcel Enabled="False" />
									</Actions>
								</ActionBar>
								<CallbackCommands>
									<Refresh PostData="Page" RepaintControlsIDs="formRuleType" />
								</CallbackCommands>
								<Mode InitNewRow="True" />
							</px:PXGrid>
							<px:PXFormView ID="PXFormViewBottom" runat="server" Width="100%" DataMember="CurrentNode" DataSourceID="ds" SkinID="Transparent">
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="True" AllowEdit="True" />
									<px:PXTreeSelector ID="edOwnerSource" runat="server" CommitChanges="true" DataField="OwnerSource" TreeDataSourceID="ds" PopulateOnDemand="True"
										InitialExpandLevel="0" ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="True"
										AutoRefresh="True" TreeDataMember="EntityItems">
										<DataBindings>
											<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
										</DataBindings>
										<ButtonImages Normal="main@AddNew" Hover="main@AddNew" Pushed="main@AddNew" />
									</px:PXTreeSelector>
									<px:PXTreeSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds"
										PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False" CommitChanges="true">
										<DataBindings>
											<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
										</DataBindings>
									</px:PXTreeSelector>
									<px:PXMaskEdit ID="edWaitTime" runat="server" DataField="WaitTime" InputMask="### d\ays ## hrs ## mins" EmptyChar="0" Text="0" />
									<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="XM" />
									<px:PXDropDown CommitChanges="True" ID="edApproveType" runat="server" DataField="ApproveType" AllowNull="False"/>
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule runat="server" EndGroup="True" />
							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" LabelsWidth="M" ControlSize="XM" GroupCaption="Reason Settings" />
							<px:PXFormView ID="PXFormView1" runat="server" Width="100%" DataMember="CurrentNode" DataSourceID="ds" SkinID="Transparent">
								<Template>
									<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="XM" />
									<px:PXDropDown ID="edReasonForApprove" runat="server" DataField="ReasonForApprove" AllowNull="False"/>
									<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="XM" />
									<px:PXDropDown ID="edReasonForReject" runat="server" DataField="ReasonForReject" AllowNull="False"/>
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule runat="server" EndGroup="True" />
						</Template>
					</px:PXTabItem>
				</Items>
			</px:PXTab>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
