<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false"
	CodeFile="EP204060.aspx.cs" Inherits="Page_EP204060" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" TypeName="PX.TM.ImportCompanyTreeMaint"
		PrimaryView="Items">
		<CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Cancel" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="Down" Visible="false" />
			<px:PXDSCallbackCommand Name="Up" Visible="false" />
			<px:PXDSCallbackCommand Name="Left" Visible="false" RepaintControlsIDs="tree" />
			<px:PXDSCallbackCommand Name="Right" Visible="false" RepaintControlsIDs="tree" />
			<px:PXDSCallbackCommand Name="UpdateTree" RepaintControlsIDs="tree" />
			<px:PXDSCallbackCommand Name="ViewEmployee" Visible="false" DependOnGrid="gridMembers"/>
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Folders" TreeKeys="WorkGroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSplitContainer runat="server" ID="sp0" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" ShowRootNode="False"
				ExpandDepth="1" Caption="Company Tree" AllowCollapse="true" SelectFirstNode="True">
				<ToolBarItems>
					<px:PXToolBarButton Tooltip="Move to external node" ImageKey="ArrowLeft">
						<AutoCallBack Command="Left" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Move to internal node" ImageKey="ArrowRight">
						<AutoCallBack Command="Right" Target="ds" />
					</px:PXToolBarButton>
				</ToolBarItems>
				<AutoCallBack Target="grid" Command="Refresh" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="Folders" TextField="Description" ValueField="WorkGroupID" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="700px">
				<AutoSize Enabled="true" />
				<Template1>
					<px:PXGrid ID="grid" runat="server" Height="200px" Width="100%" DataSourceID="ds" AllowSearch="True"
						ActionsPosition="Top" Caption="List of Groups" SkinID="Details" KeepPosition="true" CaptionVisible="True">
						<Levels>
							<px:PXGridLevel DataMember="Items">
								<Columns>
									<px:PXGridColumn DataField="WorkGroupID" TextAlign="Right" Visible="False" AllowShowHide="False" />
									<px:PXGridColumn DataField="Description" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
								</RowTemplate>
								<Layout FormViewHeight=""></Layout>
							</px:PXGridLevel>
						</Levels>
						<Parameters>
							<px:PXControlParam ControlID="tree" Name="WorkGroupID" PropertyName="SelectedValue" />
						</Parameters>
						<AutoCallBack Target="gridMembers" Command="Refresh" />
						<OnChangeCommand Target="tree" Command="Refresh" />
						<AutoSize Enabled="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Up" Tooltip="Move Node Up">
									<AutoCallBack Command="Up" Target="ds" />
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Down" Tooltip="Move Node Down">
									<AutoCallBack Command="Down" Target="ds" />
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
							<Actions>
								<Search Enabled="False" />
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
								<FilterShow Enabled="False" />
								<FilterSet Enabled="False" />
								<ExportExcel Enabled="False" />
							</Actions>
						</ActionBar>
						<LevelStyles>
							<RowForm Height="170px" Width="490px" />
						</LevelStyles>
					</px:PXGrid>
				</Template1>
				<Template2>
					<px:PXGrid ID="gridMembers" runat="server" DataSourceID="ds" ActionsPosition="Top" Height="200px" Width="100%"
						Caption="Group Members" SkinID="DetailsInTab">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="Members">
								<Columns>
									<px:PXGridColumn DataField="UserID" AutoCallBack="true" />
									<px:PXGridColumn DataField="EPEmployee__acctCD" LinkCommand="ViewEmployee"/>
									<px:PXGridColumn DataField="EPEmployee__acctName" />
									<px:PXGridColumn DataField="EPEmployeePosition__positionID" />
									<px:PXGridColumn DataField="EPEmployee__departmentID" />
									<px:PXGridColumn DataField="IsOwner" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSelector ID="edUserID" runat="server" DataField="UserID" AllowEdit="True" FilterByAllFields="true" />
									<px:PXCheckBox ID="chkIsOwner" runat="server" DataField="IsOwner" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
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
							<Save RepaintControls="None" RepaintControlsIDs="ds"/>
							<FetchRow RepaintControls="None" />
						</CallbackCommands>
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="WorkGroupID" PropertyName="DataValues[&quot;WorkGroupID&quot;]"
								Type="Int32" />
						</Parameters>
					</px:PXGrid>
				</Template2>
			</px:PXSplitContainer>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
