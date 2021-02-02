<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" CodeFile="CS206030.aspx.cs" Inherits="Page_CS206030" Title="Unit Set Maintenance" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="UnitSet" TypeName="PX.CS.RMUnitSetMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="left" Visible="false" />
			<px:PXDSCallbackCommand Name="right" Visible="false" />
			<px:PXDSCallbackCommand Name="up" Visible="false" />
			<px:PXDSCallbackCommand Name="down" Visible="false" />
			<px:PXDSCallbackCommand Name="CopyUnitSet" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="UnitCode" TreeView="Items" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlCopyUnitSet" runat="server" Style="z-index: 108;" Caption="New Unit Set Code" CaptionVisible="true" Key="Parameter" CreateOnDemand="false" AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyUnitSet"
		AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="btnOK">
		<px:PXFormView ID="formCopyUnitSet" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="Parameter">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXMaskEdit ID="edNewUnitSetCode" runat="server" DataField="NewUnitSetCode">
				</px:PXMaskEdit>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="Copy">
				<AutoCallBack Target="formCopyUnitSet" Command="Save" />
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="UnitSet" Caption="Unit Set" NoteIndicator="True" FilesIndicator="True">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edUnitSetCode" runat="server" DataField="UnitSetCode" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown CommitChanges="True" ID="edType" runat="server" AllowNull="False" DataField="Type" />
		</Template>
		<Parameters>
			<px:PXControlParam ControlID="tree" Name="currentCode" PropertyName="SelectedValue" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
          <AutoSize Enabled="true" Container="Window" />
          <Template1>
				<px:PXTreeView ID="tree" runat="server" DataSourceID="ds"   PopulateOnDemand="True" AutoRepaint="True" RootNodeText="Units" ExpandDepth="4"
                    ShowRootNode="False" Caption="Unit Code" AllowCollapse="False" CaptionVisible="False" SyncPosition="True" SyncPositionWithGraph="True" KeepPosition="True" >
				    <ToolBarItems>
						<px:PXToolBarButton Tooltip="Move to external node" CommandSourceID="ds" CommandName="left">
							<Images Normal="main@ArrowLeft" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Move to internal node" CommandSourceID="ds" CommandName="right">
							<Images Normal="main@ArrowRight" />
						</px:PXToolBarButton>
<%--						<px:PXToolBarButton Tooltip="Move up" CommandSourceID="ds" CommandName="up">
							<Images Normal="main@ArrowUp" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Move down" CommandSourceID="ds" CommandName="down">
							<Images Normal="main@ArrowDown" />
						</px:PXToolBarButton>--%>
				    </ToolBarItems>
					<DataBindings>
						<px:PXTreeItemBinding DataMember="Items" TextField="UnitCode" ValueField="UnitCode" />
					</DataBindings>
					<AutoCallBack Command="Refresh" Target="grid" />
					<AutoSize Enabled="True" />
				</px:PXTreeView>
          </Template1>
          <Template2>
				<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" Caption="Units" skinid="Details"
                AutoAdjustColumns="true" CaptionVisible="False" SyncPosition="True">
					<Levels>
						<px:PXGridLevel DataMember="Subitems">
							<RowTemplate>
								<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
								<px:PXMaskEdit ID="edUnitCode" runat="server" DataField="UnitCode" />
								<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
								<pxa:RMFormulaEditor ID="edFormula" runat="server" DataField="Formula" Parameters="@AccountCode,@AccountDescr,@BaseRowCode,@BookCode,@BranchName,@ColumnCode,@ColumnIndex,@ColumnSetCode,@ColumnText,@EndAccount,@EndAccountGroup,@EndBranch,@EndPeriod,@EndProject,@EndProjectTask,@EndSub,@StartAccount,@StartAccountGroup,@StartBranch,@StartPeriod,@StartProject,@StartProjectTask,@StartSub,@RowCode,@RowIndex,@RowSetCode,@RowText,@UnitCode,@UnitSetCode,@UnitText"/>
								<px:PXTextEdit ID="edGroupID" runat="server" DataField="GroupID" />
								<pxa:RMDataSetEditor ID="edDataSource" runat="server" DataSourceID="ds" DataMember="DataSource" DataField="DataSourceID" ShowExpandFlag="True" />
							</RowTemplate>
							<Columns>
								<px:PXGridColumn DataField="UnitCode" Width="63px" />
								<px:PXGridColumn DataField="Description" Width="200px" />
								<px:PXGridColumn DataField="Formula" Width="200px" />
								<px:PXGridColumn DataField="GroupID" Width="63px" />
								<px:PXGridColumn AllowUpdate="False" DataField="DataSourceID" TextField="DataSourceIDText" Width="200px" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Enabled="True" MinHeight="150" />
					<CallbackCommands>
						<Save RepaintControls="Unbound" />
                        <Refresh CommitChanges="True" PostData="Page" RepaintControls="OwnerContent" />
					</CallbackCommands>
					<Mode InitNewRow="True" />
					<Parameters>
						<px:PXControlParam ControlID="tree" Name="parentCode" PropertyName="SelectedValue" />
					</Parameters>
				</px:PXGrid>
          </Template2>
     </px:PXSplitContainer>
</asp:Content>
