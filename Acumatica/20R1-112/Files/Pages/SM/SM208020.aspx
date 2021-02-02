<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM208020.aspx.cs" Inherits="Page_SM208020" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<style type="text/css">
		.GridMain .GridRow:hover
		{
			background-color: #e0e0e0;
			cursor:pointer;
		}
	</style>

	<px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="TablesFromFilter" 
		TypeName="PX.Olap.Maintenance.PivotMaintAutosave" PageLoadBehavior="SearchSavedKeys">
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
		</DataTrees>
		<ClientEvents Initialize="dataSource_Initialize" />
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="TablesFromFilter" Hidden="true">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="M" Merge="true" />
			<px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="true" />

			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="M" Merge="true" />
			<px:PXSelector ID="edPivotTableID" runat="server" DataField="PivotTableID" DataSourceID="ds" AutoRefresh="True"
				NullText="<NEW>" DisplayMode="Text">
				<Parameters>
					<px:PXControlParam Name="PivotTable.screenID" ControlID="form" PropertyName="DataControls[&quot;edScreenID&quot;].Value" Type="String" Size="8" />
				</Parameters>
			</px:PXSelector>
			<px:PXTextEdit ID="edName" runat="server" DataField="Name" CommitChanges="True" />
		</Template>
		<Parameters>
			<px:PXControlParam ControlID="form" Name="PivotTable.screenID" PropertyName="NewDataKey[&quot;ScreenID&quot;]" Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<script type="text/javascript">

		var dimensionGrids = ["gridFacts", "gridInact", "gridXDim", "gridYDim"];

		function dataSource_Initialize(ds, ev)
		{
			for (var i = 0; i < dimensionGrids.length; i++)
			{
				var gr = px_alls[dimensionGrids[i]];
				if (gr) gr.suppressInitPosition = true;
			}

			var grp = px_alls["gridProps"];
			if (grp)
			{
			    grp.events.addEventHandler("startCellEdit", (s1, ev1) =>
			    {
			        ev1.cell.editor.control.events.addEventHandler("valueChanged", (s2, ev2) => grp.commitChanges());
			    });
			}
		}

		function grid_afterRowChange(grid, ev)
		{
			var propsGrid = px_alls["gridProps"];
			if (propsGrid && ev.keyChanged) propsGrid.refresh();
		}

		function grid_cellClick(grid, ev)
		{
			for (var i = 0; i < dimensionGrids.length; i++)
			{
				var gr = px_alls[dimensionGrids[i]];
				if (gr && gr != grid)
				{
					gr.resetPosition(); gr.resetPositionState();
					delete gr.lastDataKey;
				}
			}
		}

        function grid_DragDrop(grid, ev)
		{
			var params = ev.data.split('|'), gridF = px_all[params[0]];
			var dragRow = gridF.rows.getRow(params[1]), command;

			switch (grid.serverID)
			{
				case "gridInact": command = "AddInactive"; break;
				case "gridXDim": command = "AddXDimension"; break;
				case "gridYDim": command = "AddYDimension"; break;
				case "gridFacts": command = "AddFact"; break;
				case "gridFields": dragRow.removeRow(); return;
			}

			if (command && dragRow)
			{
				var isFields = (gridF == px_alls["gridFields"]);
				var displayName = dragRow.getCell(isFields ? "DisplayName" : "Caption").getValue();
                var name = dragRow.getCell(isFields ? "Name" : "Expression").getValue();
                var fieldIdCell = dragRow.getCell("PivotFieldId");

				if (name)
				{
                    var args = [name, displayName];

                    if (fieldIdCell)
                    {
                        args.push(fieldIdCell.getValue());
                    }
                    else
                    {
                        args.push("");
                    }

					if (ev.targetRow)
					{
						cell = ev.targetRow.getCell("PivotFieldId");
                        if (cell) args.push(cell.getValue());
                    }

					px_alls['ds'].executeCommand(command, args.join('|'));
				}
			}
		}

	</script>
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="25" PositionInPercent="true" Height="300px" >
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXGrid ID="gridFields" runat="server" SkinID="Details" Width="100%" DataSourceID="ds" Height="200px" 
				CaptionVisible="true" Caption="Fields" AutoAdjustColumns="true" SyncPosition="true" AllowPaging="false" AllowDrop="true">
				<AutoSize Enabled="true" />
				<Layout HeaderVisible="false" RowSelectorsVisible="false" />
				<Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
				<ActionBar Position="Top">
					<Actions>	
						<AddNew Enabled="false" /> <Delete Enabled="false" /> <ExportExcel Enabled="false" /> <AdjustColumns Enabled="false" />
					</Actions>
				</ActionBar>
				<Levels>
					<px:PXGridLevel DataMember="SourceFields" SortOrder="DisplayName">
						<Columns>
							<px:PXGridColumn DataField="Name" TextField="DisplayName" Width="200px" AllowDragDrop="true" />
							<px:PXGridColumn DataField="DisplayName" Visible="false" SyncVisible="false" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<CallbackCommands>
					<Refresh RepaintControlsIDs="edFormula" />
				</CallbackCommands>
				<ClientEvents Dragdrop="grid_DragDrop" />
			</px:PXGrid>
		</Template1>

		<Template2>
			<px:PXSplitContainer runat="server" ID="sp2" SplitterPosition="66" PositionInPercent="true" Height="300px">
				<AutoSize Enabled="true" />
				<Template1>
					<table style="width:100%;height:100px" cellspacing="0" cellpadding="0">
						<tr>
							<td style="width: 50%;padding: 0px">
								<table style="width:100%;height:100px" cellspacing="0" cellpadding="0">
									<tr>
										<td style="height: 50%">
											<px:PXGrid ID="gridYDim" runat="server" SkinID="Details" Width="100%" DataSourceID="ds" Height="150px"
												CaptionVisible="true" Caption="Rows" AutoAdjustColumns="true" AllowPaging="false" SyncPosition="true" AllowDrop="ExactPosition">
												<AutoSize Enabled="true" />
												<Layout HeaderVisible="false" RowSelectorsVisible="false" />
												<ActionBar Position="Top">
													<CustomItems>
														<px:PXToolBarButton CommandSourceID="ds" CommandName="AddYDimension" DisplayStyle="Image" ImageSet="main" ImageKey="AddNew">
															<ActionBar GroupIndex="0" Order="2" />
														</px:PXToolBarButton>
													</CustomItems>
													<Actions>	
														<AddNew Enabled="false" /> <ExportExcel Enabled="false" /> <AdjustColumns Enabled="false" />
													</Actions>
												</ActionBar>
												<Levels>
													<px:PXGridLevel DataMember="YDimensions">
														<Columns>
															<px:PXGridColumn DataField="Caption" Visible="false" SyncVisible="false" />
															<px:PXGridColumn DataField="Expression" Width="200px" AllowDragDrop="true" TextField="Caption" Visible="true" SyncVisible="false" />
															<px:PXGridColumn DataField="Transformation" Visible="False" SyncVisible="False" />
														</Columns>
													</px:PXGridLevel>
												</Levels>
												<ClientEvents AfterRowChange="grid_afterRowChange" CellClick="grid_cellClick" Dragdrop="grid_DragDrop" />
											</px:PXGrid>
										</td>
									</tr>
				
									<tr>
										<td>
											<px:PXSplitter ID="spl11" runat="server" SkinID="Horizontal" AllowCollapse="false" Size="2" 
												Enabled="false" BorderWidth="0px" Panel1MinSize="250" Panel2MinSize="250">
												<AutoSize Enabled="true" />
											</px:PXSplitter>
										</td>
									</tr>
				
									<tr>
										<td style="height: 50%">
											<px:PXGrid ID="gridXDim" runat="server" SkinID="Details" Width="100%" DataSourceID="ds" Height="150px"
												CaptionVisible="true" Caption="Columns" AutoAdjustColumns="true" AllowPaging="false" SyncPosition="true" AllowDrop="ExactPosition">
												<AutoSize Enabled="true" />
												<Layout HeaderVisible="false" RowSelectorsVisible="false" />
												<ActionBar Position="Top">
													<CustomItems>
														<px:PXToolBarButton CommandSourceID="ds" CommandName="AddXDimension" DisplayStyle="Image" ImageSet="main" ImageKey="AddNew">
															<ActionBar GroupIndex="0" Order="2" />
														</px:PXToolBarButton>
													</CustomItems>
													<Actions>	
														<AddNew Enabled="false" /> <ExportExcel Enabled="false" /> <AdjustColumns Enabled="false" />
													</Actions>
												</ActionBar>
												<Levels>
													<px:PXGridLevel DataMember="XDimensions">
														<Columns>
															<px:PXGridColumn DataField="Caption" Visible="false" SyncVisible="false" />
															<px:PXGridColumn DataField="Expression" Width="200px" AllowDragDrop="true" TextField="Caption" Visible="true" SyncVisible="false" />
															<px:PXGridColumn DataField="Transformation" Visible="False" SyncVisible="False" />
														</Columns>
													</px:PXGridLevel>
												</Levels>
												<ClientEvents AfterRowChange="grid_afterRowChange" CellClick="grid_cellClick" Dragdrop="grid_DragDrop" />
											</px:PXGrid>
										</td>
									</tr>
								</table>
							</td>
				
							<td>
								<px:PXSplitter ID="spl1" runat="server" Style="height: 100%" SaveSizeUnits="True" 
									AllowCollapse="false" Size="4" Enabled="false" Orientation="Vertical">
									<AutoSize Enabled="true" />
								</px:PXSplitter>
							</td>

							<td style="width: 50%;padding: 0px">
								<px:PXGrid ID="gridFacts" runat="server" SkinID="Details" Width="100%" DataSourceID="ds" Height="150px" 
									CaptionVisible="true" Caption="Values" AutoAdjustColumns="true" AllowPaging="false" SyncPosition="true" AllowDrop="ExactPosition">
									<AutoSize Enabled="true" />
									<Layout HeaderVisible="false" RowSelectorsVisible="false" />
									<ActionBar Position="Top">
										<CustomItems>
											<px:PXToolBarButton CommandSourceID="ds" CommandName="AddFact" DisplayStyle="Image" ImageSet="main" ImageKey="AddNew">
												<ActionBar GroupIndex="0" Order="2" />
											</px:PXToolBarButton>
										</CustomItems>
										<Actions>	
											<AddNew Enabled="false" /> <ExportExcel Enabled="false" /> <AdjustColumns Enabled="false" />
										</Actions>
									</ActionBar>
									<Levels>
										<px:PXGridLevel DataMember="Facts">
											<Columns>
												<px:PXGridColumn DataField="Expression" Visible="false" SyncVisible="false"  />
												<px:PXGridColumn DataField="Transformation" Visible="False" SyncVisible="False" />
												<px:PXGridColumn DataField="Caption" Width="200px" AllowDragDrop="true" />
											</Columns>
										</px:PXGridLevel>
									</Levels>
									<ClientEvents AfterRowChange="grid_afterRowChange" CellClick="grid_cellClick" Dragdrop="grid_DragDrop" />
								</px:PXGrid>
							</td>
						</tr>
					</table>
				</Template1>

				<Template2>
					<%--<px:PXFormulaEditor ID="edFormula" runat="server" Width="150px" Hidden="true"
						DataSourceID="ds" FieldsViewName="SourceFields" FieldsFieldName="Name" CallbackUpdatable="true" />--%>
					<px:PXGrid ID="gridProps" runat="server" SkinID="Attributes" Width="100%" DataSourceID="ds" Height="150px" 
						CaptionVisible="true" Caption="Properties" AllowPaging="false" MatrixMode="false" OnRowDataBound="gridProps_RowDataBound">
						<AutoSize Enabled="true" />
						<Layout HeaderVisible="false" RowSelectorsVisible="false" />
						<Levels>
							<px:PXGridLevel DataMember="Properties" SortOrder="Order">
								<Columns>
									<px:PXGridColumn DataField="DisplayName" Width="150px" />
									<px:PXGridColumn DataField="Value" Width="150px" MatrixMode="true" CommitChanges="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template2>
			</px:PXSplitContainer>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
