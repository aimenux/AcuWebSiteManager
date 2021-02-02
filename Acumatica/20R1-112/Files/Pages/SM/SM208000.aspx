<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM208000.aspx.cs" Inherits="Page_SM208000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function commandResult(ds, context)
		{
			if (context.command == "Save" || context.command == "Delete" || context.command == "CopyPaste@ImportXml")
			{
				var ds = px_all[context.id];
				var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
				if (isSitemapAltered) __refreshMainMenu();
			}

			// grdFilterID and grdResultsID is registered by server
			var grid = null;
			var row = null;
			if (context.command == "moveDownFilter" || context.command == "moveUpFilter")
				grid = px_all[grdFilterID];
			else if (context.command == "moveDownSortings" || context.command == "moveUpSortings")
				grid = px_all[grdSortsID];
			else if (context.command == "moveDownCondition" || context.command == "moveUpCondition")
				grid = px_all[grdWheresID];
			else if (context.command == "moveDownRelations" || context.command == "moveUpRelations")
				grid = px_all[grdJoinsID];
			else if (context.command == "moveDownOn" || context.command == "moveUpOn")
				grid = px_all[grdOnsID];
			else if (context.command == "moveDownGroupBy" || context.command == "moveUpGroupBy")
				grid = px_all[grdGroupByID];


			if (context.command == "moveDownFilter" || context.command == "moveDownSortings"
				|| context.command == "moveDownCondition" || context.command == "moveDownRelations" || context.command == "moveDownOn" || context.command == "moveDownGroupBy")
				row = grid.activeRow.nextRow();
			else if (context.command == "moveUpFilter" || context.command == "moveUpSortings"
				|| context.command == "moveUpCondition" || context.command == "moveUpRelations" || context.command == "moveUpOn" || context.command == "moveUpGroupBy")
				row = grid.activeRow.prevRow();

			if (row)
				row.activate();
		}

		function onWhereCellUpdate(sender, e)
		{
		}

		function edNavFieldName_initVariables(sender)
		{
		    var temp = sender.funDateTime.slice();
		    for (var i = 0; i < relativeDatesVariables.length; i++)
		    {
		        temp.push(relativeDatesVariables[i]);
		    }
		    sender.funDateTime = temp;
		}

		function toolsButtonClickHandler(grid, e)
		{
		    if (isCommandMoving(e.button.commandName) && grid.activeRow.isNew())
		        grid.activeRow.dataChanged = true;
		}

		function isCommandMoving(commandName)
		{
		    return commandName == "moveDownFilter" ||
                commandName == "moveDownSortings" ||
                commandName == "moveDownCondition" ||
                commandName == "moveDownRelations" ||
                commandName == "moveDownOn" ||
                commandName == "moveDownGroupBy" ||
		        commandName == "moveUpFilter" ||
                commandName == "moveUpSortings" ||
                commandName == "moveUpCondition" ||
                commandName == "moveUpRelations" ||
                commandName == "moveUpOn" ||
                commandName == "moveUpGroupBy";
		}

	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="Designs" TypeName="PX.Data.Maintenance.GI.GenericInquiryDesigner">
		<ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="viewInquiry" StartNewGroup="True" />
			<px:PXDSCallbackCommand DependOnGrid="grdFilter" Name="moveUpFilter" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdFilter" Name="moveDownFilter" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdSorts" Name="moveUpSortings" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdSorts" Name="moveDownSortings" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdWheres" Name="moveUpCondition" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdWheres" Name="moveDownCondition" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdJoins" Name="moveUpRelations" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdJoins" Name="moveDownRelations" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdOns" Name="moveUpOn" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdOns" Name="moveDownOn" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdGroupBy" Name="moveUpGroupBy" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grdGroupBy" Name="moveDownGroupBy" Visible="False" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="showAvailableValues" Visible="False" DependOnGrid="grdFilter" CommitChanges="true"
				 RepaintControls="None" RepaintControlsIDs="ds,form,gridCombos" />
		    <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="grdResults" />
		    <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="grdResults" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlCombos" runat="server" Style="z-index: 108;"
		Width="416px" Caption="Combo Box Values" CaptionVisible="true" LoadOnDemand="true"
		Key="ValuesLabels" AutoCallBack-Target="gridCombos" AutoCallBack-Command="Refresh" 
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
		<div style="padding: 5px">
			<px:PXGrid ID="gridCombos" runat="server" DataSourceID="ds" Height="243px" Style="z-index: 100"
				Width="100%" AutoAdjustColumns="True" SkinID="ShortList">
				<Levels>
					<px:PXGridLevel  DataMember="ValuesLabels">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXTextEdit ID="edValue" runat="server" DataField="Value" />
							<px:PXTextEdit ID="edLabel" runat="server" DataField="Label" /></RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="Value" Label="Value" Width="108px" />
							<px:PXGridColumn DataField="Label" Label="Label" Width="108px" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<CallbackCommands>
					<Save RepaintControls="None" RepaintControlsIDs="gridCombos" />
					<FetchRow RepaintControls="None" />
				</CallbackCommands>
			</px:PXGrid>
		</div>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnCombos" runat="server" DialogResult="OK" Text="OK" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Inquiry Summary" DataMember="Designs" OnDataBound="form_DataBound" AllowCollapse="True">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edName" runat="server" DataField="Name" DataSourceID="ds" />
		    <px:PXCheckBox ID="chkVisible" runat="server" DataField="Visible" CommitChanges="true" />
			<px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" />
		    <px:PXSelector runat="server" DataField="WorkspaceID" ID="edWorkspaceID" DisplayMode="Text"/>
		    <px:PXSelector runat="server" DataField="SubcategoryID" ID="edSubcategoryID" DisplayMode="Text"/>
		    <px:PXTextEdit runat="server" DataField="SitemapScreenID" ID="edSitemapScreenID" />		 			
			<px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit Size="S" runat="server" DataField="FilterColCount" ID="edColCount" />
			<px:PXLabel Size="xs" ID="lblColumns" runat="server" Encode="True">Columns</px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
			<px:PXNumberEdit Size="S" runat="server" DataField="SelectTop" ID="edTop" />
			<px:PXLabel Size="xs" ID="lblRecords" runat="server">Records</px:PXLabel>
			<px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
			<px:PXNumberEdit Size="S" runat="server" DataField="PageSize" ID="edPageSize" />
		    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
		    <px:PXNumberEdit Size="S" runat="server" DataField="ExportTop" ID="edExportTop" />
		    <px:PXLabel Size="xs" ID="lblExportTop" runat="server">Records</px:PXLabel>
		    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
		    <px:PXCheckBox runat="server" DataField="ShowDeletedRecords" ID="chkShowDeletedRecords" />
		    <px:PXCheckBox runat="server" DataField="ExposeViaOData" ID="chkExposeViaOData" />
		    <px:PXCheckBox ID="chkExposeViaMobile" runat="server" DataField="ExposeViaMobile" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />

			<%--<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" DataSourceID="ds" />
			<px:PXDateTimeEdit runat="server" DisplayFormat="g" DataField="CreatedDateTime" ID="edCreatedDateTime" Size="SM" />--%>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="CurrentDesign">
		<Items>
			
			<px:PXTabItem Text="Tables">
				<Template>
					<px:PXGrid ID="grdTables" runat="server" DataSourceID="ds" Height="150px" SkinID="DetailsInTab"
						Width="100%">
						<CallbackCommands>
							<Save RepaintControls="None" RepaintControlsIDs="ds,frmSorts" SelectControlsIDs="form"/>
						</CallbackCommands>
						<Mode AllowFormEdit="True" InitNewRow="True" />
						<Levels>
							<px:PXGridLevel  DataMember="Tables">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
										ControlSize="M" />
									<px:PXSelector ID="edName" runat="server" DataField="Name" FilterByAllFields="true"/>
									<px:PXTextEdit ID="edAlias" runat="server" DataField="Alias" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Name" Width="250px" />
									<px:PXGridColumn DataField="Alias" Width="250px" />
									<px:PXGridColumn DataField="Number" Width="60px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Relations" Overflow="Hidden">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Panel1MinSize="150" Panel2MinSize="150">
						<AutoSize Enabled="true"  MinHeight="250"  />
						<Template1>
							<px:PXGrid ID="grdJoins" runat="server" DataSourceID="ds" Height="100%" Style="z-index: 100" Width="100%"
								AutoAdjustColumns="True" Caption="Table Relations" AllowSearch="True" SkinID="DetailsInTab" MatrixMode="True" SyncPosition="True">
								<AutoCallBack Command="Refresh" Target="grdOns" ActiveBehavior="True">
									<Behavior RepaintControlsIDs="grdOns" BlockPage="True" CommitChanges="False" />
								</AutoCallBack>
								<AutoSize Enabled="true"  MinHeight="150"  />
								<Mode AllowFormEdit="True" InitNewRow="True" />
								<Levels>
									<px:PXGridLevel DataMember="Relations">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
											<px:PXCheckBox SuppressLabel="True" ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" CommitChanges="True" />
											<px:PXDropDown ID="edParentTable" runat="server" DataField="ParentTable" AutoCallBack="True" CommitChanges="True" />
											<px:PXDropDown ID="edJoinType" runat="server" AllowNull="False" DataField="JoinType" SelectedIndex="2" />
											<px:PXDropDown ID="edChildTable" runat="server" DataField="ChildTable" AutoCallBack="True" CommitChanges="True" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="50px" />
											<px:PXGridColumn DataField="ParentTable" Width="150px" Type="DropDownList" RenderEditorText="true" CommitChanges="True" />
											<px:PXGridColumn AllowNull="False" DataField="JoinType" RenderEditorText="True" Width="60px" />
											<px:PXGridColumn DataField="ChildTable" Width="150px" Type="DropDownList" RenderEditorText="true" CommitChanges="True" />
											<px:PXGridColumn DataField="LineNbr" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
                                <ClientEvents ToolsButtonClick="toolsButtonClickHandler" />
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpRelations" Tooltip="Move Row Up">
											<Images Normal="main@ArrowUp" />
										</px:PXToolBarButton>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownRelations" Tooltip="Move Row Down">
											<Images Normal="main@ArrowDown" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="grdOns" runat="server" DataSourceID="ds" Height="200px" Width="100%" AutoAdjustColumns="True"
								Caption="Data Field Links For Active Relation" SkinID="DetailsInTab" MatrixMode="true">
								<AutoSize Enabled="true"  MinHeight="150"  />
								<Levels>
									<px:PXGridLevel DataMember="JoinConditions">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXDropDown ID="edOpenBrackets" runat="server" DataField="OpenBrackets" />
											<pxa:PXFormulaCombo ID="edParentField" runat="server" DataField="ParentField" EditButton="True" FieldsAutoRefresh="True"
												FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
												IsExternalVisible="false" OnRootFieldsNeeded="edOns_RootFieldsNeeded"
                                                SkinID="GI"/>
											<px:PXDropDown ID="edCondition" runat="server" AllowNull="False" DataField="Condition" />
											<pxa:PXFormulaCombo ID="edChildField" runat="server" DataField="ChildField" EditButton="True" FieldsAutoRefresh="True"
												FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
												IsExternalVisible="false" OnRootFieldsNeeded="edOns_RootFieldsNeeded"
                                                SkinID="GI"/>
											<px:PXDropDown ID="edCloseBrackets" runat="server" DataField="CloseBrackets" />
											<px:PXDropDown ID="edOperation" runat="server" AllowNull="False" DataField="Operation" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="OpenBrackets" RenderEditorText="True" Width="72px" />
											<px:PXGridColumn DataField="ParentField" Width="108px" RenderEditorText="true" />
											<px:PXGridColumn AllowNull="False" DataField="Condition" RenderEditorText="True" Width="117px" />
											<px:PXGridColumn DataField="ChildField" RenderEditorText="True" Width="108px" AutoCallBack="true" />
											<px:PXGridColumn DataField="CloseBrackets" RenderEditorText="True" Width="72px" />
											<px:PXGridColumn AllowNull="False" DataField="Operation" RenderEditorText="True" Width="36px" AutoCallBack="true" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<Mode InitNewRow="True" AutoInsert="True" />
								<AutoSize Enabled="True"  MinHeight="150"  />
                                <ClientEvents ToolsButtonClick="toolsButtonClickHandler" />
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpOn" Text="Row Up" Tooltip="Move Row Up">
											<Images Normal="main@ArrowUp" />
										</px:PXToolBarButton>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownOn" Text="Row Down" Tooltip="Move Row Down">
											<Images Normal="main@ArrowDown" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
                                <CallbackCommands>
                                    <InitRow CommitChanges="true" />
                                </CallbackCommands>
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Parameters">
				<Template>
					<px:PXGrid ID="grdFilter" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="True" SkinID="DetailsInTab" MatrixMode="false" AutoGenerateColumns="AppendDynamic"
						OnEditorsCreated="grd_EditorsCreated_RelativeDates">
						<Levels>
							<px:PXGridLevel  DataMember="Parameters">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
									<px:PXCheckBox ID="chkIsActive4" runat="server" Checked="True" DataField="IsActive" />
									<px:PXCheckBox ID="chkIsExpression" runat="server" DataField="IsExpression" />
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
									<px:PXTextEdit ID="edName4" runat="server" DataField="Name" CommitChanges="true" />
									<px:PXDropDown ID="edFieldName" runat="server" DataField="FieldName" />
									<px:PXTextEdit ID="edDisplayName" runat="server" DataField="DisplayName" />
									<px:PXTextEdit ID="edDefaultValue" runat="server" DataField="DefaultValue" />
									<px:PXNumberEdit ID="edColSpan" runat="server" AllowNull="False" DataField="ColSpan" />
									<px:PXCheckBox ID="chkRequired" runat="server" DataField="Required" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" AllowNull="False" />
									<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="60px" AllowNull="False"/>
									<px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="Name" Width="150px" AutoCallBack="True" />
									<px:PXGridColumn DataField="FieldName" Type="DropDownList" Width="150px" MatrixMode="true" />
									<px:PXGridColumn DataField="DisplayName" Width="150px" />
									<px:PXGridColumn DataField="IsExpression" Label="Expression" TextAlign="Center" Type="CheckBox" Width="58px" AutoCallBack="True" />
									<px:PXGridColumn DataField="DefaultValue" Width="100px" MatrixMode="true" AllowStrings="True" DisplayMode="Value"/>
									<px:PXGridColumn AllowNull="False" DataField="ColSpan" TextAlign="Right" Width="75px" />
									<px:PXGridColumn DataField="Size" Width="65px" />
									<px:PXGridColumn DataField="LabelSize" Width="65px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<Save RepaintControls="None" RepaintControlsIDs="ds"/>
						</CallbackCommands>
						<Mode InitNewRow="True" />
                        <ClientEvents ToolsButtonClick="toolsButtonClickHandler" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpFilter" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownFilter" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="showAvailableValues" CommandSourceID="ds" Text="Combo Box Values"
									Tooltip="Values available in combo box.">
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="true" MinHeight="150"  />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Conditions">
				<Template>
					<px:PXGrid ID="grdWheres" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="True" SkinID="DetailsInTab" MatrixMode="false" 
						OnEditorsCreated="grd_EditorsCreated_RelativeDates">
						<CallbackCommands>
							<Save RepaintControls="None" RepaintControlsIDs="ds"/>
						</CallbackCommands>
						<ClientEvents AfterCellUpdate="onWhereCellUpdate" />
						<Levels>
							<px:PXGridLevel  DataMember="Wheres">
								<Mode InitNewRow="True" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkIsActive2" runat="server" Checked="True" DataField="IsActive" />
									<px:PXDropDown ID="edOpenBrackets2" runat="server" DataField="OpenBrackets" />
									<px:PXDropDown ID="edDataFieldName" runat="server" DataField="DataFieldName" />
									<px:PXDropDown ID="edCondition2" runat="server" AllowNull="False" DataField="Condition" />
									<pxa:PXFormulaCombo ID="edValue1" runat="server" DataField="Value1" EditButton="True"
										FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Parameters" 
										IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                        SkinID="GI"/>
									<pxa:PXFormulaCombo ID="edValue2" runat="server" DataField="Value2" EditButton="True"
										FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Parameters"
										IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                        SkinID="GI"/>
									<px:PXDropDown ID="edCloseBrackets2" runat="server" DataField="CloseBrackets" />
									<px:PXDropDown ID="edOperation2" runat="server" AllowNull="False" DataField="Operation" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" AllowSort="False" TextAlign="Center"
										Type="CheckBox" Width="50px" />
									<px:PXGridColumn DataField="OpenBrackets" RenderEditorText="True" Width="60px" AllowSort="False" />
									<px:PXGridColumn DataField="DataFieldName" Width="150px" Type="DropDownList" AutoCallBack="true"
										AllowSort="False" />
									<px:PXGridColumn AllowNull="False" DataField="Condition" RenderEditorText="True"
										Width="120px" AllowSort="False" />
									<px:PXGridColumn AutoCallBack="True" DataField="IsExpression" AllowSort="False" Label="Expression"
										TextAlign="Center" Type="CheckBox" Width="58px" />
									<px:PXGridColumn DataField="Value1" Width="150px" Key="value1" AllowSort="False" MatrixMode="true" AllowStrings="True" DisplayMode="Value"/>
									<px:PXGridColumn DataField="Value2" Width="150px" Key="value2" AllowSort="False" MatrixMode="true" AllowStrings="True" Visible="False" DisplayMode="Value"/>
									<px:PXGridColumn DataField="CloseBrackets" RenderEditorText="True" Width="60px" AllowSort="False" />
									<px:PXGridColumn AllowNull="False" DataField="Operation" RenderEditorText="True"
										Width="40px" AllowSort="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
                        <ClientEvents ToolsButtonClick="toolsButtonClickHandler" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpCondition" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownCondition" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Grouping" Visible ="True">
				<Template>
					<px:PXGrid ID="grdGroupBy" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="False" SkinID="DetailsInTab" MatrixMode="true">
						<CallbackCommands>
							<Save RepaintControls="None" RepaintControlsIDs="ds"/>
						</CallbackCommands>
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel DataMember="GroupBy">
                                <RowTemplate>
									<px:PXCheckBox ID="chkIsActive6" runat="server" Checked="True" DataField="IsActive" />
                                    <pxa:PXFormulaCombo ID="edDataFieldName3" runat="server" DataField="DataFieldName" EditButton="True"   
                                        FieldsRootAutoRefresh="true" LastNodeName="Parameters" 
										IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                        SkinID="GI"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="100px" />
									<px:PXGridColumn DataField="DataFieldName" Width="300px"  Type="DropDownList" AutoCallBack="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" MinHeight="150" />
                        <ClientEvents ToolsButtonClick="toolsButtonClickHandler" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpGroupBy" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownGroupBy" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Sort Order">
				<Template>
					<px:PXGrid ID="grdSorts" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="False" SkinID="DetailsInTab" MatrixMode="true">
						<CallbackCommands>
							<Save RepaintControls="None" RepaintControlsIDs="ds,frmSorts" SelectControlsIDs="form" />
						</CallbackCommands>
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel  DataMember="Sortings">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkIsActive3" runat="server" Checked="True" DataField="IsActive" CommitChanges="True" />
                                    <pxa:PXFormulaCombo ID="edDataFieldName4" runat="server" DataField="DataFieldName" EditButton="True"   
                                        FieldsRootAutoRefresh="true" LastNodeName="Parameters" 
										IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded"
                                        SkinID="GI"/>
									<px:PXDropDown ID="edSortOrder" runat="server" AllowNull="False" DataField="SortOrder" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" AllowSort="False" TextAlign="Center" Type="CheckBox" Width="100px" CommitChanges="True" />
									<px:PXGridColumn AllowSort="False" DataField="DataFieldName"  Width="300px" MatrixMode="true" AllowStrings="True" DisplayMode="Value" />
                                     
									<px:PXGridColumn AllowNull="False" AllowSort="False" DataField="SortOrder" RenderEditorText="True"	Width="150px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" MinHeight="150" />
                        <ClientEvents ToolsButtonClick="toolsButtonClickHandler" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpSortings" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownSortings" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				    <px:PXFormView ID="frmSorts" runat="server" DataSourceID="ds" DataMember="CurrentDesign" SkinID="Transparent" AllowPaging="False" >
				        <Template>
				            <px:PXLayoutRule runat="server" StartRow="True" />
				            <px:PXTextEdit ID="edDefaultSortOrder" runat="server" DataField="DefaultSortOrder" Width="600px" />
				        </Template>
				    </px:PXFormView>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Results Grid">
				<Template>
				    <px:PXFormView ID="frmRowStyle" runat="server" DataMember="CurrentDesign" AllowPaging="False"> 
				        <Template>
				            <px:PXLayoutRule runat="server" Merge="True" ControlSize="SM" LabelsWidth="S"/>
				            <pxa:PXFormulaCombo ID="edRowStyleFormula" runat="server" DataField="RowStyleFormula" EditButton="True"
				                                FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields" IsInternalVisible="false"
				                                IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded" CommitChanges="true" 
				                                SkinID="GI" IsStylesVisible="True">                                        
				            </pxa:PXFormulaCombo>
				        </Template>
				    </px:PXFormView>

					<px:PXGrid ID="grdResults" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="True" SkinID="DetailsInTab" MatrixMode="true"
                        SyncPosition="True">
						<CallbackCommands PasteCommand="PasteLine">
							<Save RepaintControls="None" RepaintControlsIDs="ds"/>
						</CallbackCommands>
						<Mode InitNewRow="True" AllowDragRows="True" />
                        <ClientEvents ToolsButtonClick="toolsButtonClickHandler" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Insert Row" SyncText="false" ImageSet="main" ImageKey="AddNew">
                                    <AutoCallBack Target="grdResults" Command="AddNew" Argument="1"></AutoCallBack>
                                    <ActionBar ToolBarVisible="External" MenuVisible="true" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
						<Levels>
							<px:PXGridLevel  DataMember="Results">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXCheckBox ID="chkIsActive5" runat="server" Checked="True" DataField="IsActive" />
									<px:PXTextEdit Size="s" ID="edCaption" runat="server" DataField="Caption" />
									<px:PXLayoutRule runat="server" Merge="False" />
									<px:PXDropDown ID="edObjectName" runat="server" DataField="ObjectName" />
									<pxa:PXFormulaCombo ID="edField5" runat="server" DataField="Field" EditButton="True"
										FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields" IsInternalVisible="false"
										IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded" CommitChanges="true" 
                                        SkinID="GI">                                        
                                    </pxa:PXFormulaCombo>
									<px:PXDropDown ID="edSchemaField" runat="server" DataField="SchemaField" CommitChanges="True" />
									<px:PXDropDown ID="edAggregateFunction" runat="server" DataField="AggregateFunction" />
									<px:PXDropDown ID="edTotalAggregateFunction" runat="server" DataField="TotalAggregateFunction" />
									<px:PXCheckBox ID="chkIsVisible" runat="server" Checked="True" DataField="IsVisible" />
									<px:PXCheckBox ID="chkQuickFilter" runat="server" DataField="QuickFilter" />
								    <px:PXCheckBox ID="chkFastFilter" runat="server" DataField="FastFilter" CommitChanges="True" />
								    <pxa:PXFormulaCombo ID="edStyleFormula" runat="server" DataField="StyleFormula" EditButton="True"
								                        FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields" IsInternalVisible="false"
								                        IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded" CommitChanges="true" 
								                        SkinID="GI" IsStylesVisible="True">                                        
								    </pxa:PXFormulaCombo>
								    <px:PXDropDown ID="edNavigateTo" runat="server" DataField="NavigationNbr" CommitChanges="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="ObjectName" Width="150px" Type="DropDownList" AutoCallBack="true" AllowDragDrop="true" />
									<px:PXGridColumn DataField="Field" Width="150px" CommitChanges="true" AllowDragDrop="true" />
									<px:PXGridColumn DataField="SchemaField" Type="DropDownList" Width="150px" AllowDragDrop="true" CommitChanges="True" />
									<px:PXGridColumn DataField="Caption" Label="Caption" Width="108px" AllowDragDrop="true" />
									<px:PXGridColumn AllowNull="False" DataField="AggregateFunction" Type="DropDownList" Width="100px" AllowDragDrop="true" />
								    <px:PXGridColumn AllowNull="True" DataField="TotalAggregateFunction" Type="DropDownList" Width="100px" AllowDragDrop="true" />
									<px:PXGridColumn DataField="Width" TextAlign="Right" Width="65px" AllowDragDrop="true" />
								    <px:PXGridColumn DataField="StyleFormula" TextAlign="Right" Width="100px" AllowDragDrop="true" />
									<px:PXGridColumn AllowNull="False" DataField="IsVisible" TextAlign="Center" Type="CheckBox"	Width="60px" />		
									<px:PXGridColumn AllowNull="False" DataField="QuickFilter" TextAlign="Center" Type="CheckBox" Width="60px" />								
								    <px:PXGridColumn AllowNull="False" DataField="FastFilter" TextAlign="Center" Type="CheckBox" Width="80px" CommitChanges="True" />								
									<px:PXGridColumn AllowNull="False" DataField="DefaultNav" TextAlign="Center" Type="CheckBox" Width="80px" />
								    <px:PXGridColumn DataField="NavigationNbr" Width="150px" Type="DropDownList" CommitChanges="True" AllowDragDrop="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Entry Point" RepaintOnDemand="false">
				<Template>
					<px:PXLayoutRule runat="server" StartRow="true" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Entry Screen Settings" />
					<px:PXSelector ID="edPrimaryScreen" runat="server" DataField="PrimaryScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="true" />
					<px:PXCheckBox ID="chkReplacePrimaryScreen" runat="server" DataField="ReplacePrimaryScreen" AlignLeft="true" CommitChanges="true" />
					<px:PXLayoutRule runat="server" EndGroup="True" />
					<px:PXLayoutRule runat="server" StartColumn="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Operations with records" />
					<px:PXCheckBox ID="chkMassActionsOnRecordsEnabled" runat="server" DataField="MassActionsOnRecordsEnabled" AlignLeft="true" CommitChanges="true" />
					<px:PXCheckBox ID="chkMassDeleteEnabled" runat="server" DataField="MassDeleteEnabled" AlignLeft="true" />
					<px:PXCheckBox ID="chkAutoConfirmDelete" runat="server" DataField="AutoConfirmDelete" AlignLeft="true" />
					<px:PXCheckBox ID="chkMassRecordsUpdateEnabled" runat="server" DataField="MassRecordsUpdateEnabled" AlignLeft="true" CommitChanges="true" />
					<px:PXCheckBox ID="chkNewRecordCreationEnabled" runat="server" DataField="NewRecordCreationEnabled" AlignLeft="true" CommitChanges="true" />
					<px:PXLayoutRule runat="server" EndGroup="True" />

					<px:PXGrid ID="grdRecordDefaults" runat="server" DataSourceID="ds" Style="z-index: 100" Width="430px" Height="150px"
						AutoAdjustColumns="False" Caption="New Record Defaults" SkinID="ShortList" MatrixMode="True">
						<Mode InitNewRow="true" />
						<Levels>
							<px:PXGridLevel DataMember="RecordDefaults">
								<RowTemplate>
									<px:PXDropDown ID="edRDFieldName" runat="server" DataField="FieldName" CommitChanges="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="FieldName" Width="150px" CommitChanges="true" />
									<px:PXGridColumn DataField="Value" Width="240px" MatrixMode="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>

					<px:PXLayoutRule runat="server" />
				</Template>
			</px:PXTabItem>
            
            <px:PXTabItem Text="Navigation">
				<Template>
                    <px:PXSplitContainer runat="server" PositionInPercent="true" SplitterPosition="25" Width="100%">
                        <AutoSize Enabled="True"/>
                        <Template1>
                            <px:PXGrid ID="grdNavScreens" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Screens" SkinID="Details"
                                AutoAdjustColumns="True" CaptionVisible="True" AllowPaging="False" SyncPosition="True">
                                <Layout HeaderVisible="false" RowSelectorsVisible="false" />
                                <Mode InitNewRow="true" />
                                <AutoSize Enabled="True"/>
                                <AutoCallBack Command="Refresh" Target="grdNavParams" ActiveBehavior="True">
									<Behavior RepaintControlsIDs="frmNavWindowMode" BlockPage="True" CommitChanges="False" />
								</AutoCallBack>
                                <ActionBar Position="Top">
                                    <Actions>
                                        <ExportExcel Enabled="False" />
                                        <AdjustColumns Enabled="False" />
                                    </Actions>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="NavigationScreens" SortOrder="LineNbr">
                                        <RowTemplate>
                                            <px:PXSelector ID="edNavScreen" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="true" TextField="Title" />
                                        </RowTemplate>
								        <Columns>
									        <px:PXGridColumn DataField="ScreenID" CommitChanges="True" DisplayMode="Hint" />
								        </Columns>
							        </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXFormView ID="frmNavWindowMode" runat="server" DataMember="NavigationScreens" AllowPaging="False"> 
                                <Searches>
                                    <px:PXControlParam Name="DesignID" ControlID="grdNavScreens" PropertyName="DataValues[&quot;DesignID&quot;]" />
                                    <px:PXControlParam Name="LineNbr" ControlID="grdNavScreens" PropertyName="DataValues[&quot;LineNbr&quot;]" />
                                </Searches>
                                <Template>
                                    <px:PXLayoutRule runat="server" Merge="True" ControlSize="SM" LabelsWidth="S"/>
                                    <px:PXDropDown ID="edNavWindowMode" runat="server" DataField="WindowMode" CommitChanges="True" />
                                    <px:PXDropDown ID="edNavIcon" runat="server" DataField="Icon" CommitChanges="True" />
                                </Template>
                            </px:PXFormView>

                            <px:PXGrid ID="grdNavParams" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
						        AutoAdjustColumns="True" Caption="Navigation Parameters" SkinID="Details" MatrixMode="true" CaptionVisible="True"
                                AllowPaging="False" SyncPosition="True">
                                <AutoSize Enabled="True"/>
                                <Mode InitNewRow="True"/>
                                <CallbackCommands>
                                    <InitRow CommitChanges="true" />
							        <Save RepaintControls="None" RepaintControlsIDs="ds"/>
                                </CallbackCommands>
                                <ActionBar Position="Top">
                                    <Actions>
                                        <ExportExcel Enabled="False" />
                                        <AdjustColumns Enabled="False" />
                                    </Actions>
                                </ActionBar>
						        <Levels>
							        <px:PXGridLevel DataMember="NavigationParameters">
							            <RowTemplate>
							                <px:PXDropDown ID="edNavFieldName" runat="server" DataField="FieldName" CommitChanges="True" />
							                <pxa:PXFormulaCombo ID="edNavParam" runat="server" DataField="ParameterName" EditButton="True"
										        FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields & Parameters" IsInternalVisible="false"
										        IsExternalVisible="false" OnRootFieldsNeeded="edNavParam_RootFieldsNeeded" CommitChanges="True" 
                                                SkinID="GI">
							                    <ClientEvents Initialize="edNavFieldName_initVariables"/>
                                            </pxa:PXFormulaCombo>
                                            <px:PXCheckBox ID="chkNavParamIsExpression" runat="server" DataField="IsExpression" CommitChanges="True" />
							            </RowTemplate>
								        <Columns>
									        <px:PXGridColumn DataField="FieldName" Width="80px" DynamicValueItems="true" CommitChanges="True" />
									        <px:PXGridColumn DataField="ParameterName" Width="140px" CommitChanges="True" DisplayMode="Value" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="IsExpression" AllowSort="False" Label="Expression"
										        TextAlign="Center" Type="CheckBox" Width="58px" />
								        </Columns>
							        </px:PXGridLevel>
						        </Levels>
					        </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Mass Update Fields" VisibleExp="DataControls[&quot;chkMassRecordsUpdateEnabled&quot;].Value == true">
				<Template>
					<px:PXGrid ID="grdMassUpdFields" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px"
						AutoAdjustColumns="false" SkinID="DetailsInTab" MatrixMode="true">
						<Mode InitNewRow="true" AutoInsert="true"/>
						<AutoSize Enabled="true" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="MassUpdateFields">
								<Columns>
									<px:PXGridColumn DataField="IsActive" Width="30px" Type="CheckBox" AllowCheckAll="true" TextAlign="Center" />
									<px:PXGridColumn DataField="FieldName" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Mass Actions" VisibleExp="DataControls[&quot;chkMassActionsOnRecordsEnabled&quot;].Value == true">
				<Template>
					<px:PXGrid ID="grdMassActions" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px"
						AutoAdjustColumns="false" SkinID="DetailsInTab" MatrixMode="true">
						<Mode InitNewRow="true" AutoInsert="true"/>
						<AutoSize Enabled="true" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="MassActions">
								<Columns>
									<px:PXGridColumn DataField="IsActive" Width="30px" Type="CheckBox" AllowCheckAll="true" TextAlign="Center" />
									<px:PXGridColumn DataField="ActionName" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXTab>
</asp:Content>
