<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" CodeFile="SM202010.aspx.cs"
	Inherits="Page_SM204000" Title="Help Map Maintenance" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script language="javascript" type="text/javascript">
		function dsClick()
		{
			var tree = px_all[treeId]; // treeId is registered on server.
			tree.refresh();
			__refreshMainMenu();
		}

		function tlbClick(strResult)
		{
			if (strResult == null || strResult == "")
				return;
			setTimeout(function () { populateTree(strResult) }, 1);
			setTimeout(function () { moveGridCursor(strResult) }, 1);
		}

		function populateTree(pars)
		{
			var params = pars.split("|");
			var tree = px_all[params[0]];
			if (params[1] == null)
			{
				tree.populateNode(tree.selectedNode);
				return;
			}
			if (params.length == 3) // the last parameter contains error message to display
			{
				alert(params[2]);
				return;
			}

			var node = checkNodes(tree.nodes, params[1]);
			if (node != null)
			{
				if (node.parentNode != null)
					tree.populateNode(node.parentNode);
				tree.populateNode(node);
			}
		}

		function checkNodes(nodes, dataPath)
		{
			var node;
			for (var i = 0; i < nodes.length; i++)
			{
				node = nodes.getNode(i);
				if (node.dataPath == ":" + dataPath)
					return node;
				if (node.hasChild)
				{
					node = checkNodes(node.childNodes, dataPath);
					if (node != null)
						return node;
				}
			}
			return null;
		}

		function moveGridCursor(pars)
		{
			params = pars.split('|');
			if (params.length != 4)
				return;

			var grid = px_all[params[2]];
			if (params[3] == "down")
				row = grid.activeRow.nextRow();
			else if (params[3] == "up")
				row = grid.activeRow.prevRow();

			if (row != null)
				row.activate();
		}
	</script>
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Children" TypeName="PX.SM.WikiPageMapMaintenance">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="Save" />
			<px:PXDSCallbackCommand Name="SelectAll" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="rowDown" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="rowUp" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="viewArticle" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />
		</DataTrees>
	</px:PXDataSource>
	<px:PXToolBar ID="tlbMain" runat="server" SkinID="Navigation" OnCallBack="tlbMain_CallBack">
		<Items>
			<px:PXToolBarButton Text="Save" Tooltip="Save (Ctrl+Q)">
				<AutoCallBack Command="Save" Handler="dsClick">
					<Behavior BlockPage="True" CommitChanges="True" RepaintControls="All" PostData="Page" />
				</AutoCallBack>
			</px:PXToolBarButton>
			<px:PXToolBarLabel ControlTheming="True" Text="   " />
			<px:PXToolBarButton Text="Cancel" Tooltip="Undo changes.">
				<AutoCallBack Command="Cancel" Target="ds" />
			</px:PXToolBarButton>
		</Items>
	</px:PXToolBar>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<%--	<px:PXToolBar ID="PXToolBar1" runat="server" Height="30px" Style="position: static"
					Width="100%" OnCallBack="PXToolBar1_CallBack" CallBackHandler="tlbClick">
					<Items>
						<px:PXToolBarButton Tooltip="Move to external node">
							<Images Normal="main@ArrowLeft" />
							<AutoCallBack Command="left" Handler="tlbClick">
								<Behavior PostData="Page" />
							</AutoCallBack>
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Move to internal node">
							<Images Normal="main@ArrowRight" />
							<AutoCallBack Command="right" Handler="tlbClick">
								<Behavior PostData="Page" />
							</AutoCallBack>
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Insert selected items to this folder">
							<Images Normal="main@DataEntryF" />
							<AutoCallBack Command="paste" Handler="tlbClick">
								<Behavior PostData="Page" BlockPage="True" RepaintControls="All" />
							</AutoCallBack>
						</px:PXToolBarButton>
					</Items>
				</px:PXToolBar>--%>
			<px:PXTreeView ID="tree" runat="server" PopulateOnDemand="True" RootNodeText="Pages"
				ShowRootNode="False" ExpandDepth="1" DataSourceID="ds" AllowCollapse="False">
				<ToolBarItems>
					<px:PXToolBarButton Tooltip="Reload Tree" ImageKey="Refresh">
						<AutoCallBack Target="tree" Command="Refresh" />
					</px:PXToolBarButton>
				</ToolBarItems>
				<AutoSize Enabled="True" />
				<AutoCallBack Target="grid" Command="Refresh" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="Folders" TextField="Title" ValueField="PageID" />
				</DataBindings>
				<Images>
					<LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
					<ParentImages Normal="tree@FolderS" Selected="tree@FolderS" />
				</Images>
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXGrid ID="grid" runat="server" Height="200px" Width="100%" DataSourceID="ds"
				AllowSearch="True" ActionsPosition="Top" AutoAdjustColumns="True" AllowPaging="True" PageSize="50" OnCallBack="grid_CallBack"
				AdjustPageSize="Auto" SkinID="Details">
				<CallbackCommands>
					<Refresh CommitChanges="True" PostData="Page" RepaintControls="All" />
				</CallbackCommands>
				<Levels>
					<px:PXGridLevel DataMember="Children">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXTextEdit Size="s" ID="edName" runat="server" DataField="Name" />
							<px:PXCheckBox ID="chkIsFolder" runat="server" DataField="Folder" />
							<px:PXLayoutRule runat="server" Merge="False" />
							<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" />
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
							<px:PXGridColumn DataField="Name" Width="200px" />
							<px:PXGridColumn DataField="Title" Width="200px" />
							<px:PXGridColumn DataField="Folder" TextAlign="Center" Width="60px" Type="CheckBox" />
							<px:PXGridColumn DataField="Number" Width="60px" />
                            <px:PXGridColumn DataField="WikiID" Width="60px"/>
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Enabled="True" />
				<ActionBar>
					<Actions>
						<Save Enabled="False" />
						<EditRecord Enabled="False" />
						<Search Enabled="False" />
						<AddNew Enabled="False" />
						<ExportExcel Enabled="False" />
						<FilterSet Enabled="False" />
						<FilterShow Enabled="False" />
						<NoteShow Enabled="False" />
					</Actions>
					<CustomItems>
						<px:PXToolBarButton Text="Select All">
							<AutoCallBack Command="selectAll" Target="ds" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Text="View Article">
							<AutoCallBack Command="viewArticle" Target="ds" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Move Up">
							<Images Normal="main@ArrowUp" />
							<AutoCallBack Command="RowUp" Handler="tlbClick">
								<Behavior PostData="Page" RepaintControls="All" BlockPage="True" />
							</AutoCallBack>
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Move Down">
							<Images Normal="main@ArrowDown" />
							<AutoCallBack Command="RowDown" Handler="tlbClick">
								<Behavior PostData="Page" RepaintControls="All" BlockPage="True" />
							</AutoCallBack>
						</px:PXToolBarButton>
					</CustomItems>
				</ActionBar>
				<LevelStyles>
					<RowForm BackColor="#F0F1F5" BorderColor="WhiteSmoke" BorderStyle="Ridge" BorderWidth="2px" Font-Names="Tahoma,Verdana,Arial,Helvetica,sans-serif"
						Font-Size="8pt" Height="145px" Width="490px">
					</RowForm>
				</LevelStyles>
				<Parameters>
					<px:PXControlParam ControlID="grid" Name="current" PropertyName="DataValues[&quot;ID&quot;]" Type="Int32" />
					<px:PXControlParam ControlID="tree" Name="parent" PropertyName="SelectedValue" />
				</Parameters>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
