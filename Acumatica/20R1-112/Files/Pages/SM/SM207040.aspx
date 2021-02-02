<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM207040.aspx.cs" Inherits="Page_SM207040"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="WebServices"
		TypeName="PX.SiteMap.Graph.ModernSYWebServiceMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="generate" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="generatedSchema" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="addToGrid" Visible="False" DependOnTree="tree" />
			<px:PXDSCallbackCommand DependOnTree="tree" Name="singleSchema" Visible="False" />
			<px:PXDSCallbackCommand DependOnTree="tree" Name="legacy" Visible="False" />
			<px:PXDSCallbackCommand Name="resetUsage" CommitChanges="true" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="EntitiesTree" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="WebServices" Caption="Web Service">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
				ControlSize="M" />
			<px:PXSelector runat="server" DataField="ServiceID" ID="edServiceID" MinDropWidth="300"
				AutoRefresh="True" DataSourceID="ds" />
			<px:PXTextEdit runat="server" DataField="Description" ID="edDescription" />
			<px:PXCheckBox runat="server" Enabled="False" DataField="IsGenerated" ID="chkIsGenerated" />
			<px:PXLabel ID="PXHole1" runat="server"></px:PXLabel>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" 
				ControlSize="S" />
			<px:PXTextEdit runat="server" DataField="CurSysVer" ID="edCurSysVer" 
				Enabled="False" Size="S" />
			<px:PXLabel ID="PXHole2" runat="server"></px:PXLabel>
			<px:PXDateTimeEdit runat="server" DisplayFormat="g" DataField="DateGenerated" 
				ID="edDateGenerated" Enabled="False" Size="S" />
			<px:PXTextEdit runat="server" DataField="SysVerWhenGenerated" 
				ID="edSysVerWhenGenerated" Enabled="False" Size="S" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
				ControlSize="SM" />
			<px:PXCheckBox SuppressLabel="True" ID="chkIsImport" runat="server" Checked="True"
				DataField="IsImport" AlignLeft="True" />
			<px:PXCheckBox SuppressLabel="True" ID="chkIsExport" runat="server" Checked="True"
				DataField="IsExport" AlignLeft="True" />
			<px:PXCheckBox SuppressLabel="True" ID="chkIsSubmit" runat="server" Checked="True"
				DataField="IsSubmit" AlignLeft="True" />
			<px:PXCheckBox CommitChanges="True" runat="server" DataField="IncludeUntyped" 
				ID="chkIncludeUntyped" AlignLeft="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" RootNodeText="Pages" ShowRootNode="False" 
            ExpandDepth="1" DataMember="EntitiesTree" AllowCollapse="False">
				<ToolBarItems>
					<px:PXToolBarButton CommandSourceID="ds" CommandName="addToGrid" Text="Add To Grid" Tooltip="Add selected node and all its valid children to grid." />
					<%--<px:PXToolBarButton CommandSourceID="ds" CommandName="singleSchema" Text="View Single" Tooltip="View schema of the selected screen" />--%>
					<%--<px:PXToolBarButton CommandName="legacy" CommandSourceID="ds" Text="View Legacy"/>--%>
				</ToolBarItems>
				<DataBindings>
					<px:PXTreeItemBinding DataMember="EntitiesTree" TextField="Title" ValueField="NodeID" ImageUrlField="Icon" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" CaptionVisible="False"
			           Width="100%" Caption="Screens" AutoAdjustColumns="True" SkinID="Details" AdjustPageSize="Auto">
				<ActionBar>
					<CustomItems>
						<px:PXToolBarButton Text="Reset Usage" CommandSourceID="ds" CommandName="resetUsage" />
					</CustomItems>
				</ActionBar>
				<Levels>
					<px:PXGridLevel DataMember="Schemas">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
							<px:PXCheckBox ID="chkIsImport" runat="server" DataField="IsImport" />
							<px:PXLayoutRule runat="server" Merge="False" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXCheckBox ID="chkIsIncluded" runat="server" Checked="True" DataField="IsIncluded" />
							<px:PXCheckBox ID="chkIsExport" runat="server" DataField="IsExport" />
							<px:PXLayoutRule runat="server" Merge="False" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXCheckBox ID="chkIsGenerated" runat="server" DataField="IsGenerated" Enabled="False" />
							<px:PXCheckBox ID="chkIsSubmit" runat="server" DataField="IsSubmit" />
							<px:PXLayoutRule runat="server" Merge="False" />
							<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" /></RowTemplate>
						<Columns>
							<px:PXGridColumn AllowNull="False" DataField="IsIncluded" Label="Active" TextAlign="Center" Type="CheckBox"
								Width="50px" />
							<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="IsGenerated" Label="Generated" TextAlign="Center"
								Type="CheckBox" Width="50px" />
							<px:PXGridColumn AllowUpdate="False" DataField="ScreenID" Label="Screen ID" Width="65px" />
							<px:PXGridColumn DataField="Title" Label="Title" Width="250px">
								<Header Text="Title">
								</Header>
							</px:PXGridColumn>
							<px:PXGridColumn DataField="IsImport" Label="Import" TextAlign="Center" Type="CheckBox" Width="60px" />
							<px:PXGridColumn DataField="IsExport" Label="Export" TextAlign="Center" Type="CheckBox" Width="60px" />
							<px:PXGridColumn DataField="IsSubmit" Label="Submit" TextAlign="Center" Type="CheckBox" Width="60px" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Enabled="True" />
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
