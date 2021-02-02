<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	CodeFile="SM202015.aspx.cs" Inherits="Page_SM204000" Title="Help Map Maintenance"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="RolesRecords" TypeName="PX.SM.WikiAccessRightsMaintenance"
		Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />
		</DataTrees>
		<ClientEvents CommandPerformed="callbackResult"/>
	</px:PXDataSource>
	<script type="text/javascript">
		function callbackResult(context)
		{
			if (context.command == "Save") __refreshMainMenu();
		}
	</script>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="RolesRecords" Caption="Role Information" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
				ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edRolename" runat="server" 
				DataField="Rolename" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXCheckBox ID="edIsinherited" runat="server" DataField="Isinherited" />            
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
          <AutoSize Enabled="true" Container="Window" />
          <Template1>
				<px:PXTreeView ID="tree" runat="server" PopulateOnDemand="True" RootNodeText="Pages" 
                    ShowRootNode="False" ExpandDepth="1" DataSourceID="ds" AllowCollapse="False">
				<ToolBarItems>
                    <px:PXToolBarButton Tooltip="Reload Tree" ImageKey="Refresh">
                        <AutoCallBack Target="tree" Command="Refresh" />
                    </px:PXToolBarButton>
				</ToolBarItems>
					<AutoCallBack Target="grid" Command="Refresh" />
				    <DataBindings>
						<px:PXTreeItemBinding DataMember="Folders" TextField="Title" ValueField="PageID" />
					</DataBindings>
				    <Images>
						<LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
						<ParentImages Normal="tree@FolderS" Selected="tree@FolderS" />
					</Images>
                    <AutoSize Enabled="true" />
				</px:PXTreeView>
          </Template1>

          <Template2>

				<px:PXGrid ID="grid" runat="server" Height="200px" Width="100%" Style="z-index: 100;
					position: relative;" DataSourceID="ds" AllowSearch="True" ActionsPosition="Top"
					AllowPaging="True" PageSize="50" AdjustPageSize="Auto" SkinID="Details" MatrixMode="True">
					<CallbackCommands>
						<Refresh CommitChanges="True" PostData="Page" RepaintControls="All" />
						<Save PostData="Page" />
					</CallbackCommands>
					<AutoSize Enabled="True" />
					<ActionBar>
						<Actions>
							<Save Enabled="False" />
							<EditRecord Enabled="False" />
							<Delete Enabled="False" />
							<Search Enabled="False" />
							<AddNew Enabled="False" />
							<ExportExcel Enabled="False" />
							<FilterSet Enabled="False" />
							<FilterShow Enabled="False" />
							<NoteShow Enabled="False" />
						</Actions>
					</ActionBar>
					<LevelStyles>
						<RowForm BackColor="#F0F1F5" BorderColor="WhiteSmoke" BorderStyle="Ridge" BorderWidth="2px"
							Font-Names="Tahoma,Verdana,Arial,Helvetica,sans-serif" Font-Size="8pt" Height="145px"
							Width="490px">
						</RowForm>
					</LevelStyles>
					<Parameters>
						<px:PXControlParam ControlID="tree" Name="parent" PropertyName="SelectedValue" />
					</Parameters>
					<Levels>
						<px:PXGridLevel DataMember="Children">
							<RowTemplate>
								<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
								<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
								<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" />
								<px:PXDropDown ID="edAccessRights" runat="server" DataField="AccessRights" CommitChanges="true"/>
<px:PXGridColumn DataField="ParentAccessRights" RenderEditorText="True" Width="200px" AutoCallBack="True"/>
							</RowTemplate>
							<Columns>
								<px:PXGridColumn AllowUpdate="False" DataField="Name" Width="200px" HtmlEncode="True" />
								<px:PXGridColumn AllowUpdate="False" DataField="Title" Width="200px" />
								<px:PXGridColumn DataField="AccessRights" RenderEditorText="True" Width="200px" AutoCallBack="True"/>
<px:PXGridColumn DataField="ParentAccessRights" RenderEditorText="True" Width="200px" AutoCallBack="True"/>
							</Columns>


						</px:PXGridLevel>
					</Levels>
					<Mode AllowAddNew="False" AllowDelete="False" />
				</px:PXGrid>
          </Template2>
     </px:PXSplitContainer>
</asp:Content>
