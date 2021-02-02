<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN408000.aspx.cs" Inherits="Page_IN408000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" TypeName="PX.Objects.IN.INInventoryByItemClassEnq" PrimaryView="ItemClassFilter" PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cut" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Paste" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="GoToNodeSelectedInTree" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewItem" DependOnGrid="gridInventories"/>
			<px:PXDSCallbackCommand Visible="false" Name="ViewClass" DependOnGrid="gridInventories" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="ItemClasses" TreeKeys="ItemClassID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="syncForm" runat="server" DataSourceID="ds" DataMember="TreeViewAndPrimaryViewSynchronizationHelper" Height="0" Width="100%" RenderStyle="Simple">
		<Template>
			<px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassCD" Visible="False"/>
		</Template>
	</px:PXFormView>
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXFormView ID="treeFilter" runat="server" DataSourceID="ds" DataMember="ItemClassFilter" Caption="Category Info" Width="100%">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="XS" StartColumn="True" />
					<px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassCD" CommitChanges="True" />
				</Template>
			</px:PXFormView>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" DataMember="ItemClasses" Height="180px" Caption="Item Class Tree"
				ShowRootNode="False" AllowCollapse="False" AutoRepaint="True"
				SyncPosition="True" SyncPositionWithGraph="True" PreserveExpanded="True" ExpandDepth="0" PopulateOnDemand="True" SelectFirstNode="True">
				<AutoCallBack Target="ds" Command="GoToNodeSelectedInTree" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="ItemClasses" TextField="SegmentedClassCD" ValueField="ItemClassID" DescriptionField="Descr" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>

		<Template2>
			<px:PXFormView ID="gridFilter" runat="server" DataSourceID="ds" DataMember="InventoryFilter" Caption="Category Info" Width="100%">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
					<px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" CommitChanges="True" >
						<AutoCallBack Target="ds" Command="GoToNodeSelectedInTree" />
					</px:PXSelector>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
					<px:PXGroupBox runat="server" DataField="ShowItems" RenderStyle="RoundBorder" ID="gbShowItemsMode" CommitChanges="True" Caption="Show Items">
						<ContentLayout Layout="Stack" Orientation="Vertical" />
						<Template>
							<px:PXRadioButton runat="server" Value="C" ID="gbShowItemsMode_opA" GroupName="gbShowItemsMode" />
							<px:PXRadioButton runat="server" Value="A" ID="gbShowItemsMode_opC" GroupName="gbShowItemsMode" />
						</Template>
					</px:PXGroupBox>
				</Template>
			</px:PXFormView>
			<px:PXGrid ID="gridInventories" runat="server" DataSourceID="ds" CaptionVisible="true" Caption="Item Class Members" Width="100%"
				SkinID="Inquire" ActionsPosition="Top" SyncPosition="True" AutoRepaint="True" AdjustPageSize="Auto" NoteIndicator="False" FilesIndicator="False">
				<AutoSize Enabled="True" Container="Parent"/>
				<Mode InitNewRow="False" AllowAddNew="False" AllowDelete="False" AllowUpdate="False"></Mode>
				<Levels>
					<px:PXGridLevel DataMember="Inventories">
						<Columns>
							<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="26px" />
							<px:PXGridColumn DataField="InventoryCD" Width="140px" LinkCommand="ViewItem" />
							<px:PXGridColumn DataField="Descr" Width="200px"/>
							<px:PXGridColumn DataField="ItemClassID" Width="140px" LinkCommand="ViewClass" />
							<px:PXGridColumn DataField="INItemClass__Descr" Width="140px"/>
							<px:PXGridColumn AllowNull="False" DataField="ItemStatus" RenderEditorText="True" Width="100px"/>
						</Columns>
						<RowTemplate>
							<px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryCD" Enabled="False" />
							<px:PXTextEdit ID="edDescription" runat="server" DataField="Descr"  Enabled="False"/>
							<px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID"  Enabled="False"/>
							<px:PXTextEdit ID="edDescription2" runat="server" DataField="INItemClass__Descr"  Enabled="False"/>
							<px:PXDropDown ID="edItemStatus" runat="server" DataField="ItemStatus"  Enabled="False"/>
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
					<CustomItems>
						<px:PXToolBarButton Tooltip="Cut Selected Inventory Items" DisplayStyle="Image">
							<AutoCallBack Command="Cut" Enabled="True" Target="ds" />
							<Images Normal="main@Cut" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Paste Inventory Items from Buffer" DisplayStyle="Image">
							<AutoCallBack Command="Paste" Enabled="True" Target="ds" />
							<Images Normal="main@Paste" />
						</px:PXToolBarButton>
					</CustomItems>
				</ActionBar>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>