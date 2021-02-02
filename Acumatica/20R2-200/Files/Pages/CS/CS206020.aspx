<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS206020.aspx.cs" Inherits="Page_CS206020" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">

	<script type="text/javascript">
		// for chrome and IE
		String.prototype.endsWith = function (s) { return this.substr(-s.length) == s }

		function grid_afterColResize(grid, ev) {
			var grid2 = getMirrorGrid(grid), dataField = ev.column.getKey();
			if (grid.ID.endsWith("gridColumn") && (dataField == "DisplayName")) {
				var col1 = grid2.levels[0].getColumn("Height");
				var col2 = grid2.levels[0].getColumn("GroupID");
				var width = ev.column.getWidth() / 2;
				col1.setWidth(width, false);
				col2.setWidth(width, false);
				return;
			}

			var col = grid2.levels[0].getColumn(dataField);
			col.setWidth(ev.column.getWidth(), false);
		}

		function grid_afterHScroll(grid, ev) {
			var grid2 = getMirrorGrid(grid);
			grid2.setHScroll(grid.getHScroll());
		}

		function grid_cornerClick(grid, ev) {
            // do not allow to show Column Configuration dialog
		    ev.cancel = true;
		}

		function gridHeader_startCellEdit(grid, ev) {
			if (grid.editMode) {
				var styleF = ev.cell.column.dataField + "_StyleID";
				var cell = grid.activeRow.getCell(styleF);
				if (cell) {
					var ctrl = ev.cell.editor.control;
					ctrl.viewState.update("StyleID", cell.getValue());
					ctrl.restorePanelCallback();
				}
			}
		}

		function getMirrorGrid(grid) {
			var id = grid.ID, grid2;
			if (id.endsWith("gridColumn")) {
				var index = id.indexOf("gridColumn");
				grid2 = px_all[id.substring(0, index) + "gridHeader"];
			}
			else {
				var index = id.indexOf("gridHeader");
				grid2 = px_all[id.substring(0, index) + "gridColumn"];
			}
			return grid2;
		}
		function grid_colMove(grid, e)
		{
		    var cmd = grid.ID.endsWith('gridColumn') ? 'MoveColumn' : 'MoveHeader';
		    var target = grid.levels[0].getColumn(e.position).getKey();
		    if (target.indexOf('StyleID') != -1) target = grid.levels[0].getColumn(e.position-1).getKey();
		    var param = e.column.getKey() + ";" + target + ";" + (e.position > grid.levels[0].columns.indexOf(e.column));
		    var ds = px_all[dsID];
		    //setTimeout(function ()
		    //{
		        if (ds != null) ds.executeCallback(cmd, param);
		    //}, 0);
		    e.cancel = true;
		}
		function buttonClick(o, e) {
		    if (e.button.commandName.indexOf("To") >= 0 || e.button.commandName == "DeleteColumn") {
		        o.storePosition(true);		        
                var cell;
                if (e.button.commandName == "ToLeftColumn" || e.button.commandName == "ToLeftHeader")
		            cell = o.activeCell.prevCell();
                if (e.button.commandName == "ToRightColumn" || e.button.commandName == "ToRightHeader")
                    cell = o.activeCell.nextCell();
                if (e.button.commandName == "ToUpHeader")
                	cell = o.activeRow.prevRow().getCell(o.activeCell.getIndex());
                if (e.button.commandName == "ToDownHeader")
                	cell = o.activeRow.nextRow().getCell(o.activeCell.getIndex());

                var indexPredA;
                indexPredA = o.levels[0].getColumn("  A").index - 1;
                
                if ((cell != null) && (cell.getIndex() > indexPredA) && (o.activeCell.getIndex() > indexPredA))
                    cell.activate();              
		    }
		}
	</script>

	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.CS.RMColumnSetMaint" PrimaryView="ColumnSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="InsertColumn" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="DeleteColumn" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="InsertHeader" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToLeftHeader" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToRightHeader" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToLeftColumn" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToRightColumn" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToUpHeader" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToDownHeader" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToBufferHeader" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToStyleHeader" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToBufferColumn" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ToStyleColumn" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="CopyColumnSet" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
            <px:PXDSCallbackCommand Name="MoveColumn" CommitChanges="True" Visible="false" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="pnlCopyColumnSet" runat="server" Style="z-index: 108;" Caption="New Column Set Code" CaptionVisible="True" Key="Parameter" CreateOnDemand="False" 
		AutoCallBack-Target="formCopyColumnSet" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="btnOK">
		<px:PXFormView ID="formCopyColumnSet" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="Parameter" TabIndex="-31036">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXMaskEdit ID="edNewColumnSetCode" runat="server" DataField="NewColumnSetCode">
				</px:PXMaskEdit>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="Copy">
				<AutoCallBack Target="formCopyColumnSet" Command="Save" />
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ColumnSet" Caption="Column Set" NoteIndicator="True" FilesIndicator="True">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize = "S"/>
			<px:PXSelector  ID="edColumnSetCode" runat="server" DataField="ColumnSetCode" AutoRefresh="True" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize = "S"/>
			<px:PXDropDown CommitChanges="True"  ID="edType" runat="server" AllowNull="False" DataField="Type" />
		</Template>
	</px:PXFormView>
	<pxa:RMDataSetEditor ID="edColumnDS" runat="server" Width="150px" DataMember="DataSource" DataSourceID="ds" Hidden="True" />
	<pxa:RMStyleEditor ID="edColumnST" runat="server" Width="150px" DataMember="Style" DataSourceID="ds" Hidden="True" />
	<pxa:RMHeaderEditor ID="edColHeader" runat="server" Width="150px" DataMember="HeaderStyle" DataSourceID="ds" Hidden="True" Parameters="@AccountCode,@AccountDescr,@BookCode,@BranchName,@ColumnCode,@ColumnIndex,@ColumnSetCode,@ColumnText,@EndAccount,@EndAccountGroup,@EndBranch,@EndPeriod,@EndProject,@EndProjectTask,@EndSub,@Organization,@OrganizationName,@ReportDescr,@StartAccount,@StartAccountGroup,@StartBranch,@StartPeriod,@StartProject,@StartProjectTask,@StartSub,@RowCode,@RowIndex,@RowSetCode,@RowText,@UnitCode,@UnitSetCode,@UnitText,@Today,@WeekStart,@WeekEnd,@MonthStart,@MonthEnd,@QuarterStart,@QuarterEnd,@PeriodStart,@PeriodEnd,@YearStart,@YearEnd"/>
	<pxa:RMFormulaEditor ID="edColumnF" runat="server" Width="150px" Hidden="True" Parameters="@AccountCode,@AccountDescr,@BaseRowCode,@BookCode,@BranchName,@ColumnCode,@ColumnIndex,@ColumnSetCode,@ColumnText,@EndAccount,@EndAccountGroup,@EndBranch,@EndPeriod,@EndProject,@EndProjectTask,@EndSub,@Organization,@StartAccount,@StartAccountGroup,@StartBranch,@StartPeriod,@StartProject,@StartProjectTask,@StartSub,@RowCode,@RowIndex,@RowSetCode,@RowText,@UnitCode,@UnitSetCode,@UnitText,@Today,@WeekStart,@WeekEnd,@MonthStart,@MonthEnd,@QuarterStart,@QuarterEnd,@PeriodStart,@PeriodEnd,@YearStart,@YearEnd"/>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="220" SkinID="Horizontal" Height="500px">
      <AutoSize Enabled="true" Container="Window" />
      <Template1>
				<px:PXGrid ID="gridHeader" runat="server" DataSourceID="ds" Height="40px" Width="100%" Caption="Headers" 
					SkinID="Details" AllowPaging="false" AutoSaveLayout="false" NoteIndicator="false" FilesIndicator="false">
					<Levels>
						<px:PXGridLevel DataMember="Headers">
							<RowTemplate>
							</RowTemplate>
							<Columns>
								<px:PXGridColumn AllowUpdate="False" DataField="ColumnSetCode" Visible="False" AllowShowHide="Server" />
								<px:PXGridColumn AllowUpdate="False" DataField="ColumnCode" Visible="False" AllowShowHide="Server" />
								<px:PXGridColumn AllowUpdate="False" DataField="HeaderNbr" Visible="False" AllowShowHide="Server" />
								<px:PXGridColumn Width="75px" AllowShowHide="Server" DataField="Height" AllowResize="false" AllowMove="False" />
								<px:PXGridColumn DataField="GroupID" Width="100px" AllowResize="false" AllowMove="false" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<ActionBar Position="Top">
						<CustomItems>
							<px:PXToolBarButton CommandSourceID="ds" CommandName="InsertHeader" ImageKey="AddNew" DisplayStyle="Image">
								<ActionBar GroupIndex="0" Order="2" />
							</px:PXToolBarButton>
							<px:PXToolBarButton CommandSourceID="ds" CommandName="ToLeftHeader" ImageKey="ArrowLeft" DisplayStyle="Image" />
							<px:PXToolBarButton CommandSourceID="ds" CommandName="ToRightHeader" ImageKey="ArrowRight" DisplayStyle="Image" />
							<px:PXToolBarButton CommandSourceID="ds" CommandName="ToDownHeader" ImageKey="ArrowDown" DisplayStyle="Image" />
							<px:PXToolBarButton CommandSourceID="ds" CommandName="ToUpHeader" ImageKey="ArrowUp" DisplayStyle="Image" />
							<px:PXToolBarButton Text="Copy Style" CommandSourceID="ds" CommandName="ToBufferHeader" />
							<px:PXToolBarButton Text="Paste Style" CommandSourceID="ds" CommandName="ToStyleHeader" />
						</CustomItems>
						<Actions>
							<Save Enabled="False" />
							<AddNew Enabled="False" />
							<Search Enabled="False" />
							<AdjustColumns Enabled="False" />
							<EditRecord Enabled="False" />
							<NoteShow Enabled="False" />
							<FilterShow Enabled="False" />
							<FilterSet Enabled="False" />
						</Actions>
					</ActionBar>
					<ClientEvents AfterColResize="grid_afterColResize" HorizontalScroll="grid_afterHScroll" GridCornerClick="grid_cornerClick" AfterCellChange="gridHeader_startCellEdit" 
                        AfterEnterEditMode="gridHeader_startCellEdit" ToolsButtonClick="buttonClick" BeforeColMove="grid_colMove" />
					<Layout ColumnsMenu="False" />
					<Mode AllowColMoving="True" AllowSort="False" />
					<CallbackCommands>
						<Save RepaintControls="Unbound" />
					</CallbackCommands>
					<AutoSize Enabled="True" />
				</px:PXGrid>
     </Template1>
      <Template2>
				<px:PXGrid ID="gridColumn" runat="server" DataSourceID="ds" Height="260px" Width="100%" Caption="Columns" 
					ActionsPosition="Top" MatrixMode="true" OnRowDataBound="gridColumn_RowDataBound" SkinID="Details" 
					AdjustPageSize="Auto" AllowPaging="false" AutoSaveLayout="false">
					<Levels>
						<px:PXGridLevel DataMember="Properties">
							<RowTemplate>
							</RowTemplate>
							<Columns>
								<px:PXGridColumn AllowUpdate="False" DataField="Name" Visible="False" AllowShowHide="Server" />
								<px:PXGridColumn AllowUpdate="False" DataField="DisplayName" Width="175px" AllowShowHide="Server" AllowResize="false" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Enabled="True" />
					<ActionBar Position="Top">
						<Actions>
							<Save Enabled="False" />
							<AddNew Enabled="False" />
							<Delete Enabled="False" />
							<Search Enabled="False" />
							<AdjustColumns Enabled="False" />
							<EditRecord Enabled="False" />
							<NoteShow Enabled="False" />
							<FilterShow Enabled="False" />
							<FilterSet Enabled="False" />
						</Actions>
						<CustomItems>
							<px:PXToolBarButton CommandSourceID="ds" CommandName="InsertColumn" ImageKey="AddNew" DisplayStyle="Image">
								<ActionBar GroupIndex="0" Order="2" />
							</px:PXToolBarButton>
							<px:PXToolBarButton CommandSourceID="ds" CommandName="DeleteColumn" ImageKey="Remove" DisplayStyle="Image">
								<ActionBar GroupIndex="0" Order="3" />
							</px:PXToolBarButton>
							<px:PXToolBarButton CommandSourceID="ds" CommandName="ToLeftColumn" ImageKey="ArrowLeft" DisplayStyle="Image" />
							<px:PXToolBarButton CommandSourceID="ds" CommandName="ToRightColumn" ImageKey="ArrowRight" DisplayStyle="Image" />
							<px:PXToolBarButton Text="Copy Style" CommandSourceID="ds" CommandName="ToBufferColumn" />
							<px:PXToolBarButton Text="Paste Style" CommandSourceID="ds" CommandName="ToStyleColumn" />
						</CustomItems>
					</ActionBar>
					<ClientEvents AfterColResize="grid_afterColResize" HorizontalScroll="grid_afterHScroll" GridCornerClick="grid_cornerClick" ToolsButtonClick="buttonClick" BeforeColMove="grid_colMove"/>
					<Layout ColumnsMenu="False" />
					<Mode AllowColMoving="True" AllowSort="False" />
					<CallbackCommands>
						<Save RepaintControls="Unbound" />
					</CallbackCommands>
				</px:PXGrid>
      </Template2>
</px:PXSplitContainer>
</asp:Content>
