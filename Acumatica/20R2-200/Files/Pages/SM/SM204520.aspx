<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
	CodeFile="SM204520.aspx.cs" Inherits="Pages_SM_SM204520" ValidateRequest="false" EnableViewState="False" EnableViewStateMac="False"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	    <%--<label class="projectLink border-box" style="background-color: white;border: none;">Layout Editor</label>--%>
	<px:PXFormView runat="server" SkinID="transparent" ID="formTitle" 
		DataSourceID="ds" DataMember="ViewPageTitle" Width="100%">
		<Template>
			<px:PXTextEdit runat="server" ID="PageTitle" DataField="PageTitle" SelectOnFocus="False"
				SkinID="Label" SuppressLabel="true"
				Width="90%"
				style="padding: 10px">
				<font size="14pt" names="Arial,sans-serif;"/>
			</px:PXTextEdit>
		</Template>
	</px:PXFormView>
	<px:PXDataSource ID="ds" runat="server" Visible="True"
		TypeName="PX.Web.Customization.LayoutEditorMaint"
		PrimaryView="FilterGridProps" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="actionRefreshProperties" 
				RepaintControls="None" RepaintControlsIDs="PropGridFilter,PropGrid,StatePropGrid,StateParamGrid,parametersTitleFrom" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="actionRefreshFields" 
				RepaintControls="None" RepaintControlsIDs="PropGridFilter,frmFields,gridFields" />
			<px:PXDSCallbackCommand Name="actionShowAspxSource" 
				RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter,FormAspxSource" />
			<px:PXDSCallbackCommand Name="actionCreateControls" CommitChanges="true"
				RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter,TreePageControls,gridFields" />
			<px:PXDSCallbackCommand Name="actionShowAttributes" CommitChanges="true"
				RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter,FormDataField" />
			<px:PXDSCallbackCommand Name="actionShowEvents" CommitChanges="true"
				RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter,GridEvents,FormDataField2" />

			<px:PXDSCallbackCommand Name="actionEvents" CommitChanges="true"
				RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter,GridEvents,FormDataField2" />

			<px:PXDSCallbackCommand Name="actionSaveAttributes" CommitChanges="true"
				RepaintControls="Bound" Visible="false" />

			<px:PXDSCallbackCommand Name="menuDacSrc" RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter" />
			<px:PXDSCallbackCommand Name="actionNewField" CommitChanges="True" RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter,gridFields" />

			<px:PXDSCallbackCommand Name="actionViewEventSource" CommitChanges="true"
				RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter" />

			<px:PXDSCallbackCommand Name="actionCustomizeAttributes" Visible="false" />
			<px:PXDSCallbackCommand Name="actionCustomizeGraphLevel" Visible="false" />
            <px:PXDSCallbackCommand Name="addNewAction" CommitChanges="True" RepaintControls="Bound" Visible="False" />
            <px:PXDSCallbackCommand Name="resetItem" CommitChanges="true"
                RepaintControls="None" Visible="false" RepaintControlsIDs="PropGridFilter,PropGrid,TreePageControls,gridFields,StatePropGrid" />
            <px:PXDSCallbackCommand Name="removeItem" CommitChanges="true"
                RepaintControls="None" Visible="false" RepaintControlsIDs="TreePageControls,PropGridFilter,PropGrid,gridFields,StatePropGrid" />

		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="AspxPageControls" TreeKeys="NodeId" />
            <px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
		</DataTrees>
		<ClientEvents CommandPerformed="DataSource_CommandPerformed" />
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="PropGridFilter" runat="server" Width="100%" DataSourceID="ds" DataMember="FilterGridProps"
		SkinID="Transparent" Hidden="true" AllowAutoHide="false" OnDataBound="PropGridFilter_DataBound">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXCheckBox runat="server" ID="ShowAllProps" DataField="ShowAllProps" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSplitContainer runat="server" ID="SplitContainer" Width="100%" Height="500px" SplitterPosition="350" 
		Size="4" SkinID="Transparent" Style="border-right-width: 0px">
		<AutoSize Enabled="True" Container="Window" />
		<Template1>
			<script type="text/javascript">
				var containerTypes = ["PXPanel", "PXSmartPanel", "PXGroupBox", "Dialogs",
					"PXFormView", "PXTab", "PXTabItem", "PXGrid", "PXGridLevel", "PXDataSource", "PXToolBar"];

				function Tree_DragDrop(tree, arg)
				{
			    var node = arg.node, target = arg.target, par = target.parentNode;
					var c1 = GetNodeContainer(target, arg.before === null);
					if (node)
					{
						var c2 = GetNodeContainer(node, false);
						if (c2 != c1) arg.cancel = true;
					}
					else if (arg.control)
					{
						var dt = GetControlNodeType(c1), cmd = arg.control.key;
						if (par == null && arg.before !== null) // top level
						{
						    if (cmd != "Tab" && cmd != "FormView"
                    //   && cmd != "SmartPanel"
                      && cmd != "Grid" && cmd != "SplitContainer")
							{
								arg.cancel = true;
							}
							return;
						}

						if (c1 == null || dt == null) { arg.cancel = true; return; }
						switch(cmd)
						{
							case "TabItem":
								if (dt != "PXTab") arg.cancel = true;
								break;
							case "GridColumn":
								if (dt != "PXGrid") arg.cancel = true;
								break;

							case "SmartPanel":
								if (dt != "Dialogs") arg.cancel = true;
								break;

							default:
								if (dt == "PXTab") arg.cancel = true;
								if (dt == "PXDataSource" || dt == "PXToolBar") arg.cancel = cmd != "Button";
								if (dt == "PXGrid") arg.cancel = (cmd != "Button") || arg.before !== null;
								break;
						}
					}

					if (!arg.cancel)
					{
						var dt = GetControlNodeType(target);
						var fakeRule = target.element.getAttribute("data-fakerule") != null;
						if ((dt == null || fakeRule) && arg.before !== null) arg.cancel = true;
					}
				}

				function Tree_BeforeDrop(tree, arg)
				{
					var isBefore = (arg.before !== null), target = arg.target;
					if (isBefore && !arg.before && target.nextSibling() == null)
					{
						target = target.parentNode; isBefore = false;
					}

					var dt = GetControlNodeType(target);
					if (dt == "PXLayoutRule" && !isBefore)
					{
						var n = target, n2, cont = GetNodeContainer(n);
						while (n)
						{
							if ((n2 = n.nextSibling()) != null && GetControlNodeType(n2) != null)
							{
								arg.srcArgs.target = n2; arg.srcArgs.before = true; break;
							}
							else if ((dt = GetControlNodeType(n)) != "PXLayoutRule" && dt)
							{
								arg.srcArgs.target = n; break;
							}
							n = n.parentNode;
							if (n == cont) { arg.srcArgs.target = n; break; }
						}
					}
				}

				function Tree_AfterNodeMove(tree, arg)
				{
					arg.cancel = true;
					// repaint top-level container
					var cont = GetNodeContainer(arg.node, false);
					if (cont) tree.populateNode(cont);
				}

				function Tree_AfterDrop(tree, arg)
				{
					if (arg.control == null) return;

					var ctrl = arg.control, cm = __px_callback(ctrl), data = "";
					var target = arg.target, isBefore = (arg.before !== null);
					var cmd = cm.createCommand("AddControl", true, false, false);
					
					var par = isBefore ? target.parentNode : target;
					if (par) data += par.dataKey
					if (isBefore)
					{
						if (par == null && !arg.before) // top level
							data = '||' + target.dataKey;
						else
						{
							var bef = arg.before ? target : target.nextSibling();
							if (bef) data += '|' + bef.dataKey
						}
					}

					var ctx = new Object();
					ctx.tree = tree; ctx.target = target; ctx.isBefore = isBefore;
					cm.execI(ctrl, cmd, data, OnControlCreated, ctx);
				}

				function OnControlCreated(result, context)
				{
					// repaint top-level container
					var res = result, cont = GetNodeContainer(context.target, !context.isBefore);
					if (cont) context.tree.populateNode(cont); else context.tree.refresh();
				}

				function IsLayoutRule(node)
				{
					return GetControlNodeType(node) == "PXLayoutRule";
				}

				function GetControlNodeType(node)
				{
					if (node == null) return null;
					if (node.dataType === undefined)
						node.dataType = node.element.getAttribute("data-type");
					return node.dataType;
				}

				function GetLastContainerRule(node)
				{
					var nodes = node.childNodes, last = nodes.getNode(nodes.length - 1);
					return IsLayoutRule(last) ? GetLastContainerRule(last) : node;
				}

				function GetNodeContainer(node, inside)
				{
					var n = inside ? node : node.parentNode;
					while (n != null)
					{
						var dt = GetControlNodeType(n);
						if (dt == null) return n;
						for (var i = 0; i < containerTypes.length; i++) if (containerTypes[i] == dt) return n;
						n = n.parentNode;
					}
				}
			</script>

			<script type="text/javascript">
				var boundTypes = ["PXFormView", "PXTab", "PXGrid", "PXGridLevel"];

				function Tree_AfterSelect(tree, arg)
				{
					RefreshSelectedNodeItems(tree);
				}

				function Tab_ItemChanged(tab, arg)
				{
					var ds = __px_cm(tab).findDataSource();
					if (ds && ds.lastControlKey0 === undefined)
					{
						ds.lastControlKey0 = GetControlKey(ds);
						ds.lastContainerKey = GetBoundContainerKey(ds);
                    }

                    // reload aspx
                    if (tab.selectedIndex === 5) {
                        ds.executeCallback("actionShowAspxSource");
                    }
                    else {
                        RefreshSelectedNodeItems(tab);
                    }
				}

				function GetBoundContainerKey(ctx)
				{
					var tree = __px_alls(ctx)["TreePageControls"];
					var node = tree.selectedNode, nodeKey = null;
					while (node != null)
					{
						var nodeType = GetControlNodeType(node);
						if (boundTypes.indexOf(nodeType) >= 0) { nodeKey = node.value; break; }
						node = node.parentNode;
					}
					return nodeKey;
				}

				function GetControlKey(ctx)
				{
					var tree = __px_alls(ctx)["TreePageControls"], node = tree.selectedNode;
					return node ? node.value : null;
				}

				function RefreshSelectedNodeItems(ctx)
				{
					var ds = __px_cm(ctx).findDataSource(), tab = __px_alls(ctx)["tab"];
					if (ds == null || tab == null) return;

					switch (tab.selectedIndex)
					{
						case 0: case 1: case 2:  case 5:
							var key = GetControlKey(ctx), keyName = "lastControlKey" + tab.selectedIndex;
							if (key != ds[keyName])
							{
								switch (tab.selectedIndex)
                                {
                                    case 0: ds.executeCallback("actionRefreshProperties"); break;
									case 1: ds.executeCallback("actionShowAttributes"); break;
									case 2: ds.executeCallback("actionShowEvents"); break;
									case 5: ds.executeCallback("actionShowAspxSource"); break;
								}
								ds[keyName] = key;
							}
							break;
						case 4:
							var key = GetBoundContainerKey(ctx);
							if (key != ds.lastContainerKey)
							{
								ds.executeCallback("actionRefreshFields");
								ds.lastContainerKey = key;
							}
							break;
					}
				}

				function Tree_Initialize(tree, arg)
				{
					tree.toolBar.events.addEventHandler("buttonClick", Tree_ButtonClick);
				}
				function Tree_ButtonClick(tlb, arg)
				{
				}
			</script>
			<px:PXTreeView ID="TreePageControls" runat="server" DataSourceID="ds" DataMember="AspxPageControls" 
				SelectOnMouseDown="False"	AllowDragDrop="True" ContextMenuID="MenuControlTree" PopulateOnDemand="False"
				Height="100px" ShowRootNode="false" ShowLines="false" DragSources="tab" SyncPosition="true" SyncPositionWithGraph="true"
				CssClass="tree autoScroll borderRight borderTop" OnNodeDataBound="TreePageControls_NodeDataBound" SelectFirstNode="true"
				OnNodeMove="TreePageControls_NodeMove" AllowDelete="true" KeepPosition="true" PreserveExpanded="true" OnDataBound="TreePageControls_DataBound"
				>
				
				<AutoSize Enabled="True" />
				<ClientEvents AfterNodeMove="Tree_AfterNodeMove" AfterSelect="Tree_AfterSelect" BeforeDrop="Tree_BeforeDrop" 
					DragDrop="Tree_DragDrop" AfterDrop="Tree_AfterDrop" Initialize="Tree_Initialize" />
				<ToolBarItems>
					<px:PXToolBarButton ImageKey="Refresh" CommandName="Refresh" />
                    <px:PXToolBarButton ImageKey="Remove" Enabled="true" Tooltip="Remove">
                        <MenuItems>
                            <px:PXMenuItem>
                                <AutoCallBack Command="removeItem" Enabled="true" Target="ds" />
                            </px:PXMenuItem>
                            <px:PXMenuItem>
                                <AutoCallBack Command="resetItem" Enabled="true" Target="ds" />
                            </px:PXMenuItem>
                        </MenuItems>
                    </px:PXToolBarButton>
				</ToolBarItems>
				<CallBackMode RepaintControlsIDs="ds"></CallBackMode>
				<DataBindings>
					<px:PXTreeItemBinding DataMember="AspxPageControls" TextField="DisplayName" ValueField="NodeId" ImageUrlField="NodeImage" />
				</DataBindings>
				<Styles>
					<Node CustomAttr="text-transform:none"></Node>
				</Styles>
			</px:PXTreeView>
		</Template1>
		<Template2>
			<script type="text/javascript">
				var expandColIndex = 0, groupColIndex = 2, nameColIndex = 3;
				var oldValColIndex = 4, valueColIndex = 5;

				function Grid_CellClick(grid, arg)
				{
					var cell = arg.cell, index = cell.getIndex(), row = cell.row;
					var isExpand = (index == expandColIndex || index == nameColIndex);
					if (isExpand || index == oldValColIndex) grid.endEdit();

					var isPropHeader = cell.element.className.indexOf("PropHeader") >= 0;
					if (isExpand && isPropHeader)
					{
						var ck = !row.getCell(expandColIndex).getValue();
						row.getCell(expandColIndex).setValue(ck);

						var key = row.getCell(groupColIndex).getValue();
						var cnt = grid.rows.length;
						for (var i = 0; i < cnt; i++)
						{
							var testRow = grid.rows.getRow(i);
							var rowKey = testRow.getCell(groupColIndex).getValue();
							if (rowKey.indexOf(key + "-") == 0)
								if (ck)
								{
									if (rowKey.substring(key.length + 1).indexOf("-") <= 0)
										testRow.setVisible(true);
								}
								else
								{
									var exCell = testRow.getCell(expandColIndex);
									if (testRow.isActive()) row.activate();
									testRow.setVisible(false);
									if (exCell.getValue()) exCell.setValue(false);
								}
						}
						grid.levels[0].navigator.setPosition();
					}
					else if (index == valueColIndex)
					{
						if (isPropHeader)
						{
							grid.endEdit();
							row.getCell(nameColIndex).activate();
							arg.cancel = true;
						}
						else if (!grid.editMode) grid.beginEdit();
					}
				}

				function Grid_BeforeEnterEditMode(grid, arg)
				{
					var cell = arg.cell, index = cell.getIndex();
					var isPropHeader = cell.element.className.indexOf("PropHeader") >= 0;

					if (isPropHeader) { arg.cancel = true; return; }
					if (index == nameColIndex || index == oldValColIndex)
						cell.row.getCell(valueColIndex).activate();
				}

				function Grid_AfterCellChange(grid, arg)
				{
					var cell = arg.cell, index = cell.getIndex();
					var isPropHeader = cell.element.className.indexOf("PropHeader") >= 0;

					if (index == valueColIndex)
					{
						if (!isPropHeader && !grid.editMode) grid.beginEdit();
						else if (isPropHeader && grid.editMode) grid.endEdit();
					}
				}

				function Grid_ButtonClick(grid, arg)
				{
					switch (arg.button.key)
					{
						case "Filter":
							__px_alls(grid)["ShowAllProps"].updateValue(!arg.button.getPushed());
							break;
					}
				}
			</script>
			<px:PXTab runat="server" ID="tab" AllowAutoHide="False" Width="100%" CssClass="tabView transparent" RepaintOnDemand="false">
				<AutoSize Enabled="True" />
				<ClientEvents TabChanged="Tab_ItemChanged" />
				<Styles><Content CssClass="tabContent borderLeft" /></Styles>
				<Items>
					<px:PXTabItem Text="Layout Properties">
						<Template>
							<px:PXGrid ID="PropGrid" runat="server" RenderDefaultEditors="True"	DataSourceID="ds" LocalMenu="False"
								CssClass="GridMain Props" OnRowDataBound="PropGrid_RowDataBound" FastFilterFields="Name" StatusField="HelpString"
								Height="100px" Width="100%" AllowFilter="false">
								<ActionBar Position="TopAndBottom" PagerVisible="False" ActionsHidden="true">
									<BottomGroups>
										<px:PXActionGroup Separator="Label" SeparatorWidth="100%" />
										<px:PXActionGroup />
									</BottomGroups>
									<Actions>
										<Refresh ToolBarVisible="Top" Order="1" />
										<AdjustColumns ToolBarVisible="Top" Order="2" />
									</Actions>
									<CustomItems>
										<px:PXToolBarButton ImageKey="Filter" Key="Filter" ToggleMode="true" Pushed="true" 
											Tooltip="Hide Advanced Properties">
											<ActionBar ToolBarVisible="Top" GroupIndex="1" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>

								<Levels>
									<px:PXGridLevel DataMember="AspxControlProperties">
										<Columns>
											<px:PXGridColumn DataField="IsExpanded" TextAlign="Center" Type="Icon" Width="22px" AllowFocus="false" >
												<ValueItems>
													<Items>
														<px:PXValueItem Value="False" DisplayValue="tree@Expand" />
														<px:PXValueItem Value="True" DisplayValue="tree@Collapse" />
													</Items>
												</ValueItems>
												<Style CssClass="PropNameColumn" />
											</px:PXGridColumn>
											<px:PXGridColumn DataField="IsCustomized" CommitChanges="True" TextAlign="Center"
												Width="75px" MatrixMode="true" />
											<px:PXGridColumn DataField="Group" />
											<px:PXGridColumn DataField="Name" Width="250px" AllowUpdate="False" />
											<px:PXGridColumn DataField="OriginalValue" Width="140px" MatrixMode="True" />
											<px:PXGridColumn DataField="Value" CommitChanges="True" Width="180px" MatrixMode="True" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<ClientEvents CellClick="Grid_CellClick" BeforeEnterEditMode="Grid_BeforeEnterEditMode" 
									AfterCellChange="Grid_AfterCellChange" ToolsButtonClick="Grid_ButtonClick" />
								<Mode AllowAddNew="False" AllowSort="False" AllowColMoving="False" />
								<Layout RowSelectorsVisible="False" HighlightMode="Cell" />
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Attributes">
						<Template>
							<px:PXFormView ID="FormDataField" runat="server" SkinID="Transparent" Width="100%"	
								DataSourceID="ds"	DataMember="DacAttributes" AutoRepaint="True" AllowFocus="False">		
								<AutoSize Enabled="True" />
								<Template>

									<px:PXPanel ID="PXPanel1" runat="server" SkinID="Transparent">
										<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XL" />
										<px:PXTextEdit ID="edFieldName" runat="server" DataField="FieldName" Enabled="False" />
										<px:PXTextEdit ID="edDataField" runat="server" DataField="CacheType" Enabled="False" />
										<px:PXLayoutRule runat="server" SuppressLabel="True" Merge="True"></px:PXLayoutRule>
										
							<%--			
										<px:PXButton ID="BtnCustomize" runat="server" Text="Customize" >
											<MenuItems>
												<px:PXMenuItem Text="Customize Data Field Attributes"></px:PXMenuItem>
												<px:PXMenuItem Text="Override Attributes on Current Screen"></px:PXMenuItem>
											</MenuItems>
										</px:PXButton>--%>
									
											
										<px:PXButton ID="PXButton3" runat="server" Text="Customize Attributes" >
												<AutoCallBack Enabled="True" Target="ds" Command="actionCustomizeAttributes" />
										</px:PXButton>	

										<px:PXButton ID="PXButton26" runat="server" Text="View Source" >
											<AutoCallBack Enabled="True" Target="ds" Command="menuDacSrc" />
										</px:PXButton>
										
											
										<px:PXButton ID="PXButton4" runat="server" Text="Override on Screen Level" >
												<AutoCallBack Enabled="True" Target="ds" Command="actionCustomizeGraphLevel" />
											
										</px:PXButton>	
									</px:PXPanel>

<%--									<px:PXSplitContainer runat="server" ID="SplitAttributes" SplitterPosition="300"
										Orientation="Horizontal" Width="100%" Height="100px" Style="padding: 20px"
										SkinID="Horizontal" CssClass="splitContainer transparent">
										<AutoSize Enabled="True" />
										<Template1>--%>
											<px:PXLabel ID="lblReadonly" runat="server">Original attributes</px:PXLabel>
											<br />
											<px:PXTextEdit ID="edReadonly" runat="server"
												Width="100%"
												DataField="DacAttrReadonly"
												SuppressLabel="true"
												Height="500px"
												LabelID="lblReadonly"
												Style="padding-left: 10px; padding-top: 10px; border: none;"
												TextMode="MultiLine"
												Wrap="False" SelectOnFocus="False"
												Font-Names="Courier New" Font-Size="10pt">
												<%--<AutoSize Enabled="True" Container="Parent" />--%>
											</px:PXTextEdit>

											<%--<px:PXLabel runat="server" />
										</Template1>
										<Template2>
											<px:PXPanel ID="PXPanel10" runat="server" SkinID="Transparent">
												<px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
												<px:PXDropDown ID="edMethod" runat="server" DataField="Method" Size="M" CommitChanges="True" />
												<px:PXButton ID="PXButton25" runat="server" Text="Save">
													<AutoCallBack Enabled="True" Command="actionSaveAttributes" Target="ds" />
												</px:PXButton>
											</px:PXPanel>

											<px:PXTextEdit ID="edDacAttrEdit" runat="server"
												DataField="DacAttrEdit"
												CommitChanges="True"
												Height="100px"
												Style="padding: 10px 0px 0px 10px;"
												TextMode="MultiLine"
												Width="100%"
												Wrap="False"
												SelectOnFocus="False"
												Font-Names="Courier New"
												Font-Size="10pt"
												LabelID="lblCustom">
												<AutoSize Enabled="True" />
											</px:PXTextEdit>
										</Template2>
									</px:PXSplitContainer>--%>
								</Template>
								<AutoSize Enabled="True" />
							</px:PXFormView>

						</Template>
					</px:PXTabItem>
		<%--			
					<px:PXTabItem Text="Selector">
						<Template>
							<px:PXFormView ID="FormSelectorCols" runat="server" SkinID="Transparent" Width="100%"
								DataSourceID="ds" DataMember="DacAttributes" AutoRepaint="True" AllowFocus="False">						
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XL" />
									<px:PXTextEdit ID="CacheType" runat="server" DataField="CacheType" Enabled="False" />
									<px:PXTextEdit ID="FieldName" runat="server" DataField="FieldName" Enabled="False" />
								
								</Template>
							</px:PXFormView>

							
							<px:PXGrid runat="server" ID="GridSelectorCols" Width="100%" SyncPosition="True"
								SyncPositionWithGraph ="True"
								AllowPaging="False" AutoAdjustColumns="True"
								
								SkinID="DetailsInTab"  Height="400px">
								<AutoSize Enabled="True" Container="Parent"/>
								<ActionBar  >
									<Actions>
										<ExportExcel ToolBarVisible="False"/>
										<AdjustColumns ToolBarVisible="False"/>
										<AddNew ToolBarVisible="False"/>
										
									</Actions>
									
						
									<CustomItems>
										
										<px:PXToolBarButton >
											<AutoCallBack Command="actionAddColumns" Target="ds" />
										</px:PXToolBarButton>
				
										<px:PXToolBarButton >
											<AutoCallBack Command="actionColumnUp" Target="ds" />
										</px:PXToolBarButton>
				
										<px:PXToolBarButton >
											<AutoCallBack Command="actionColumnDown" Target="ds" />
										</px:PXToolBarButton>

								
									</CustomItems>
								</ActionBar>

								<Levels>
									<px:PXGridLevel DataMember="ViewSelectorCols">
										<Columns>
											
											
											<px:PXGridColumn DataField="DisplayName" Width="200px" />
											<px:PXGridColumn DataField="Name" Width="200px" />

										</Columns>
									</px:PXGridLevel>
								</Levels>
								<Mode AllowSort="false" AllowAddNew="false" AllowUpdate="false"/>
							
							</px:PXGrid>
							
						
							

						</Template>
					</px:PXTabItem>--%>


					<px:PXTabItem Text="Events">
						<Template>
							<px:PXFormView ID="FormDataField2" runat="server" SkinID="Transparent" Width="100%"
								DataSourceID="ds" DataMember="DacAttributes" AutoRepaint="True" AllowFocus="False">						
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XL" />
									<px:PXTextEdit ID="CacheType" runat="server" DataField="CacheType" Enabled="False" />
									<px:PXTextEdit ID="FieldName" runat="server" DataField="FieldName" Enabled="False" />
									<px:PXTextEdit ID="edViewName" runat="server" DataField="ViewName" Enabled="False" />
								</Template>
							</px:PXFormView>

							<%--<px:PXLayoutRule runat="server" StartRow="True" />--%>
							<px:PXGrid runat="server" ID="GridEvents" Width="100%" SyncPosition="True"
								AllowPaging="False" AutoAdjustColumns="True"
								SkinID="DetailsInTab" StatusField="Descr"  Height="400px">
								<%--<AutoSize Enabled="True" Container="Parent"/>--%>
								<ActionBar Position="TopAndBottom"  ActionsHidden="True" DefaultAction="NewEvent">
									<Actions>
										<ExportExcel ToolBarVisible="False"/>
										<AdjustColumns ToolBarVisible="False"/>
										<AddNew ToolBarVisible="False"/>
										<Delete ToolBarVisible="False"/>
									</Actions>
									<BottomGroups>
										<px:PXActionGroup Separator="Label" SeparatorWidth="100%" />
										<px:PXActionGroup />
									</BottomGroups>
						
									<CustomItems>
										<px:PXToolBarButton Text="Add Handler" Key="NewEvent" >
											<ActionBar ToolBarVisible="Top" />
											<AutoCallBack Command="actionEvents" Target="ds" />
										</px:PXToolBarButton>

										<px:PXToolBarButton Text="View Source">
											<ActionBar ToolBarVisible="Top" />
											<AutoCallBack Command="actionViewEventSource" Target="ds" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>

								<Levels>
									<px:PXGridLevel DataMember="ViewEvents">
										<Columns>
											<px:PXGridColumn DataField="EventName" Width="200px" />
											<px:PXGridColumn DataField="BaseHandler"  Type="CheckBox"/>
											<px:PXGridColumn DataField="Handler"  Type="CheckBox"/>
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<Mode AllowSort="false"/>
								<%--<AutoSize Enabled="True" Container="Parent" />--%>
							</px:PXGrid>
						</Template>
					</px:PXTabItem>

					<px:PXTabItem Text="Add Controls">
						<Template>
							<px:PXLayoutRule runat="server" ID="rule1" StartColumn="true" GroupCaption="Main Containers" />
							<px:PXButton runat="server" ID="btnAddForm" Text="Form" Key="FormView" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left"  />
							<px:PXButton runat="server" ID="btnAddTab" Text="Tab" Key="Tab" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddTabItem" Text="Tab Item" Key="TabItem" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddGrid" Text="Grid" Key="Grid" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddPopup" Text="Pop-up Panel" Key="SmartPanel" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />

							<px:PXLayoutRule runat="server" ID="rule2" GroupCaption="Layout Rules" />
							<px:PXButton runat="server" ID="btnAddRow" Text="Row" Key="Row" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddColumn" Text="Column" Key="Column" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddGroup" Text="Group" Key="Group" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddMerge" Text="Merge" Key="Merge" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddRule" Text="Empty Rule" Key="LayoutRule" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />

							<px:PXLayoutRule runat="server" ID="rule3" StartColumn="true" GroupCaption="Other Controls" />
							<px:PXButton runat="server" ID="btnAddPanel" Text="Panel" Key="Panel" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddGroupBox" Text="Group Box" Key="GroupBox" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddRadioButton" Text="Radio Button" Key="RadioButton" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddLabel" Text="Label" Key="Label" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
							<px:PXButton runat="server" ID="btnAddButton" Text="Button" Key="Button" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left"  />
							<px:PXButton runat="server" ID="btnAddScript" Text="Java Script" Key="JavaScript" ImageSet="tree" RenderAsButton="False" ImageAlign="Right" CssClass="Button toolBox" AlignLeft="True" TextAlign="Left" />
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Add Data Fields" LoadOnDemand="True">
						<Template>
							<px:PXFormView runat="server" ID="frmFields" DataMember="FilterViewFields" SkinID="Transparent"
								CheckChanges="False">
								<Template>
									<px:PXLayoutRule runat="server" ID="rule4" StartColumn="true" />
									<px:PXDropDown runat="server" ID="cmbDataView" DataField="ViewName" DataSourceID="ds" 
										DataMember="GraphViews" ValueField="CacheName" TextField="Text" Size="L" CommitChanges="true">
									</px:PXDropDown>
									<%--<px:PXCheckBox runat="server" ID="chkShowExisting" DataField="ShowExisting" CommitChanges="true" />--%>
								</Template>
								<CallbackCommands>
									<Save RepaintControls="None" RepaintControlsIDs="gridFields" />
								</CallbackCommands>
							</px:PXFormView>
							<px:PXGrid runat="server" ID="gridFields" Width="100%" Height="200px" 
								FilterView="ViewFieldsFilters"
								BlankFilterHeader ="ALL"
								BatchUpdate="true">
								<AutoSize Enabled="True" />
								<Mode AllowAddNew="False" AllowDelete="False" />
								<Layout ShowRowStatus="false" RowSelectorsVisible="false" />
								<ActionBar Position="Top" ActionsHidden="True">
									<Actions>
										<FilterSet ToolBarVisible="Top" GroupIndex="2" Order="0" />
									</Actions>
									<CustomItems>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="actionCreateControls" ImageKey="AddNew" 
											Text="Create Controls" DisplayStyle="Text">
											<ActionBar GroupIndex="0" />
										</px:PXToolBarButton>

                                        <px:PXToolBarButton CommandSourceID="ds" CommandName="actionNewField" DisplayStyle="Text">
											<ActionBar GroupIndex="0" Order="1" />
										</px:PXToolBarButton>

                                    <%--    <px:PXToolBarButton CommandSourceID="ds" CommandName="actionNavigateFields" Text="Add New" DisplayStyle="Text">
											<ActionBar GroupIndex="0" Order="2" />
										</px:PXToolBarButton>--%>
										
<%--										<px:PXToolBarButton CommandName="Refresh" Text="Refresh" DisplayStyle="Text">
											<ActionBar GroupIndex="0" Order="3" />
										</px:PXToolBarButton>--%>

										<px:PXToolBarLabel>
											<ActionBar GroupIndex="2" Order="1" />
										</px:PXToolBarLabel>
									</CustomItems>
								</ActionBar>
								<Levels>
									<px:PXGridLevel DataMember="ViewFields">
										<Columns>
											<px:PXGridColumn DataField="Selected" AllowCheckAll="true" TextAlign="Center" Type="CheckBox" Width="60px" />
                                        	<px:PXGridColumn DataField="Created" TextAlign="Center" Type="CheckBox" Width="60px" />
											<px:PXGridColumn DataField="DisplayName" Width="300px" />

											<px:PXGridColumn DataField="ControlType" Width="150px" />


										</Columns>
									</px:PXGridLevel>
								</Levels>
								<CallbackCommands>
									<Save RepaintControls="None" RepaintControlsIDs="TreePageControls" />
								</CallbackCommands>
							</px:PXGrid>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="View ASPX" Overflow="Hidden">
						<Template>
							<px:PXFormView runat="server" ID="FormAspxSource" DataMember="AspxSource" SkinID="Transparent"
								Width="100%" >
								<AutoSize Enabled="True" Container="Parent" />
								<Template>
									<px:PXHtmlView runat="server" ID="AspxCode" DataField="Source" Width="100%" Height="300px"
										Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
										<AutoSize Enabled="True" Container="Parent" />



									</px:PXHtmlView>
									<%--<px:PXTextEdit ID="edNodeAspxCode" runat="server" DataField="Source" Width="200px" />--%>
								</Template>
								<%--<ClientEvents Initialize="InitPanelAspxSource" />--%>	
							</px:PXFormView>
	
		<%--					<px:PXSmartPanel runat="server" ID="AspxContainer" AutoSize-Enabled="true" SkinID="Frame"
								CssClass="SmartPanelF border-box" Style="padding: 0px 10px; background-color: white; 
								font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; position: absolute;">
								<pre id="Pre1"></pre>
							</px:PXSmartPanel>
							
							<script type="text/javascript">
								var IsPanelAspxSourceInit = false;

								function InitPanelAspxSource(a, b)
								{
									if (IsPanelAspxSourceInit) return;
									IsPanelAspxSourceInit = true;
									a.events.addEventHandler("afterRepaint", UpdatePanelAspxSource);
								}

								function UpdatePanelAspxSource(frm, ev)
								{
									var ed = __px_alls(frm)["edNodeAspxCode"];
									var ph = __px(frm).elemByID("Pre1");
									if (ed && ph) ph.innerHTML = ed.getValue();
								}
							</script>--%>
						</Template>
					</px:PXTabItem>
				</Items>
			</px:PXTab>
		</Template2>
	</px:PXSplitContainer>

    <px:PXSmartPanel runat="server" ID="PanelAddNewAction"
        CaptionVisible="True" Caption="New Action"
        Key="FilterAddNewAction" AllowResize="false"
        AutoRepaint="True">
        <px:PXFormView runat="server" ID="PXFormView2" DataMember="FilterAddNewAction" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                <px:PXDropDown runat="server" ID="edActionType" DataField="ActionType" CommitChanges="True" AutoComplete="false" AutoRefresh="true" />
                <px:PXTextEdit ID="SelectActionName" runat="server" DataField="ActionName" />
                <px:PXSelector runat="server" ID="edDestinationScreenID" DataField="DestinationScreenID" CommitChanges="True" AutoComplete="true" AutoRefresh="true" />
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK" />
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" />
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>

    <px:PXSmartPanel runat="server" ID="PanelAddNewSidePanel"
        CaptionVisible="True" Caption="New Side Panel"
        Key="FilterAddNewSidePanel" AllowResize="false"
        AutoRepaint="True">
        <px:PXFormView runat="server" ID="PXFormView1" DataMember="FilterAddNewSidePanel" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
                <px:PXTextEdit ID="SelectActionName" runat="server" DataField="SidePanelName" />
                <px:PXSelector runat="server" ID="edDestinationScreenID" DataField="DestinationScreenID" CommitChanges="True" AutoComplete="true" AutoRefresh="true" />
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" />
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" />
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
</asp:Content>

<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="server">

	<px:PXSmartPanel ID="PanelAddEvent" runat="server"
		Caption="Add Event Handler"
		CaptionVisible="True"
		AutoRepaint="True"
		Key="FilterAddEvent">

		<px:PXFormView ID="FormAddEvent" runat="server"
			SkinID="Transparent"
			DataMember="FilterAddEvent"
			DataSourceID="ds"
			AutoRepaint="True"
			Key="FilterAddEvent">

			<Template>

				<px:PXLayoutRule ID="PXLayoutRule1" runat="server"></px:PXLayoutRule>

				<px:PXTextEdit ID="EventsPrefix" runat="server"
					DataField="Prefix" />

				<px:PXDropDown ID="SelectEventType" runat="server"
					CommitChanges="True"
					AllowNull="False"
					DataField="EventType"
					Required="True" />

				<px:PXSelector ID="EventFieldName" runat="server"
					AllowNull="True" DataField="FieldName"
					AutoRefresh="True" ReadOnly="True" ValueField="FieldName">
					<GridProperties>
						<Columns>
							<px:PXGridColumn DataField="FieldName" Width="200px">
							</px:PXGridColumn>
						</Columns>

					</GridProperties>
				</px:PXSelector>
			</Template>
		</px:PXFormView>

		<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="True" />

		<px:PXPanel ID="PXPanel8" runat="server" SkinID="Buttons">
			<px:PXButton ID="PanelAddEventButtonOk" runat="server"
				CausesValidation="False"
				Text="OK"
				DialogResult="OK" />

			<px:PXButton ID="PanelAddEventButtonCancel" runat="server"
				CausesValidation="False" DialogResult="Cancel" Text="Cancel" />

		</px:PXPanel>
	</px:PXSmartPanel>
	
	<px:PXSmartPanel ID="DlgNewField" runat="server"
		CaptionVisible="True"
		Caption="Create New Field"
		AutoRepaint="True"
		Key="NewFieldWizard">
		<px:PXFormView ID="FormSelectTable" runat="server"
			SkinID="Transparent" DataMember="NewFieldWizard">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" />

				<px:PXTextEdit runat="server" ID="edNewFieldName" DataField="NewFieldName" CommitChanges="true" />
				<px:PXTextEdit runat="server" ID="DisplayName" DataField="DisplayName"/>
				<px:PXDropDown runat="server" ID="edStorageType" DataField="StorageType" CommitChanges="True"/>
				<px:PXDropDown runat="server" ID="edDataType" DataField="DataType" CommitChanges="true" />
				<px:PXNumberEdit runat="server" ID="Length" DataField="Length" AllowNull ="True" />
				<px:PXNumberEdit runat="server" ID="Precision" DataField="Precision" AllowNull ="True" />

			</Template>
		</px:PXFormView>

		<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartRow="True" />
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton13" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton14" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>
	
<%--		<px:PXSmartPanel ID="PanelAddSelectorCols" runat="server"
		CaptionVisible="True"
		Caption="Add Columns to Selector"
		AutoRepaint="True"
			Width="600px"
			Height="400px"
		Key="ViewAddSelectorCols">
			
			
					<px:PXGrid runat="server" ID="GridAddSelectorCols" Width="100%" 
								AllowPaging="False" AutoAdjustColumns="True"
						FilterView="ViewSelectorFilters"
						AllowFilter="True"
								BlankFilterHeader ="ALL"
								SkinID="Attributes"  Height="300px">
							
								<ActionBar Position="Top" ActionsHidden="True">
									<Actions>
										<FilterSet ToolBarVisible="Top" GroupIndex="2" Order="0" Enabled="True" />
								
									</Actions>
									
									
									
									

									<CustomItems>
										
										<px:PXToolBarButton Text="" DisplayStyle="Text">
											<ActionBar GroupIndex="0" />
										</px:PXToolBarButton>
										
	

										<px:PXToolBarLabel>
											<ActionBar GroupIndex="2" Order="1" />
										</px:PXToolBarLabel>
									</CustomItems>
									
						
							
								</ActionBar>

								<Levels>
									<px:PXGridLevel DataMember="ViewAddSelectorCols">
										<Columns>
											<px:PXGridColumn DataField="Selected"  Type="CheckBox"/>
											
											<px:PXGridColumn DataField="DisplayName" Width="200px" />
											<px:PXGridColumn DataField="Name" Width="200px" />

										</Columns>
									</px:PXGridLevel>
								</Levels>
								<Mode AllowSort="false"/>
								<AutoSize Enabled="True" Container="Parent" />
							</px:PXGrid>
				


		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>--%>
</asp:Content>

