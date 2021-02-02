<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	CodeFile="SM200522.aspx.cs" Inherits="Page_SM200522" Title="ISV Portal Map Maintenance"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">

	<script language="javascript" type="text/javascript">
	function dsClick()
	{
		var tree = px_all[treeId]; // treeId is registered on server.
		tree.refresh();
		__refreshMainMenu();
	}
	</script>

	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Children" 
		TypeName="PX.SM.PortalISVMapMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="SaveSite" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="rowDown" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="rowUp" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="copy" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="paste" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="cut" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="addWiki" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
	<px:PXToolBar ID="tlbMain" runat="server" SkinID="Navigation" OnCallBack="tlbMain_CallBack">
		<Items>
			<px:PXToolBarButton Text="Save" Tooltip="Save Changes" ImageSet="main" ImageKey="Save" DisplayStyle="Image">
				<AutoCallBack Command="SaveSite" Handler="dsClick">
					<Behavior BlockPage="True" CommitChanges="True" RepaintControls="All" PostData="Page" />
				</AutoCallBack>
			</px:PXToolBarButton>
			<px:PXToolBarButton Text="Cancel" Tooltip="Undo changes." ImageSet="main" ImageKey="Cancel" DisplayStyle="Image">
				<AutoCallBack Command="CancelSite" Target="ds">
				</AutoCallBack>
			</px:PXToolBarButton>
		</Items>
	</px:PXToolBar>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="server">
	<px:PXSmartPanel ID="pnlAddWiki" runat="server" CaptionVisible="True" Caption="Add Wiki"
		ForeColor="Black" Style="position: static" Height="100" Width="410px" LoadOnDemand="True"
		Key="WikiPanel" DesignView="Content">
		<px:PXFormView ID="frmAddWiki" runat="server" SkinID="Transparent" DataMember="WikiPanel"
			DataSourceID="ds" Width="250px" EmailingGraph="">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" LabelsWidth="SM" 
					StartColumn="True">
				</px:PXLayoutRule>
				<px:PXSelector ID="edWikiID" runat="server" AutoRefresh="True" 
					DataField="WikiID" DataSourceID="ds" ReadOnly="True">
				</px:PXSelector>
				<px:PXPanel ID="pnlAddWikiButtons" runat="server" SkinID="Buttons">
					<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
						<AutoCallBack Command="Save" Target="frmAddWiki" />
					</px:PXButton>
					<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
			<Activity HighlightColor="" SelectedColor="" />
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" 
				RootNodeText="Pages" ShowRootNode="False"  AllowCollapse="False" ExpandDepth="1" >
				<ToolBarItems>
                    <px:PXToolBarButton Tooltip="Reload Site Map" ImageKey="Refresh">
                        <AutoCallBack Target="tree" Command="Refresh" />
                    </px:PXToolBarButton>
				</ToolBarItems>
				<AutoCallBack Target="grid" Command="Refresh" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="SiteMap" TextField="Title" ValueField="NodeID" ImageUrlField="Icon" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>

		<Template2>
			<px:PXGrid ID="grid" runat="server" Height="355px" Width="100%" Style="z-index: 100;
				position: relative; height: 355px;" DataSourceID="ds" AllowSearch="True" 
				ActionsPosition="Top" SkinID="Details" KeepPosition="true">
				<CallbackCommands>
					<Refresh CommitChanges="True" PostData="Page" RepaintControls="OwnerContent" />
				</CallbackCommands>
				<Levels>
					<px:PXGridLevel DataMember="Children">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" 
								ControlSize="L" StartGroup="True" />
							<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" > </px:PXTextEdit>
							<px:PXDropDown ID="edIcon" runat="server" DataField="Icon" AllowEdit="true" > </px:PXDropDown>
							<px:PXTextEdit ID="edUrl" runat="server" DataField="Url" > </px:PXTextEdit>
							<px:PXTextEdit ID="edGraphtype" runat="server" DataField="Graphtype" Enabled="False" > </px:PXTextEdit>
							<px:PXMaskEdit ID="edScreenID" runat="server" DataField="ScreenID" InputMask="CC.CC.CC.CC" > </px:PXMaskEdit>
							<px:PXCheckBox ID="chkExpanded" runat="server" DataField="Expanded"> </px:PXCheckBox></RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="ScreenID" DisplayFormat="CC.CC.CC.CC" > </px:PXGridColumn> 
							<px:PXGridColumn DataField="Title" Width="200px"> </px:PXGridColumn>
							<px:PXGridColumn DataField="Icon" Width="200px"> </px:PXGridColumn>
							<px:PXGridColumn DataField="Url" Width="200px"> </px:PXGridColumn>
							<px:PXGridColumn DataField="Graphtype" Width="200px"> </px:PXGridColumn>
							<px:PXGridColumn DataField="Expanded" TextAlign="Center" Type="CheckBox" Width="60px" RenderEditorText="True"> </px:PXGridColumn>
						</Columns>
						<Mode AllowRowSelect="True" />
						<Layout FormViewHeight=""></Layout>
					</px:PXGridLevel>
				</Levels>
				<Parameters>
					<px:PXControlParam ControlID="tree" Name="parent" PropertyName="SelectedValue" />
				</Parameters>
				<AutoSize Enabled="True" />
				<ActionBar >
                    <CustomItems>
                        <px:PXToolBarButton ImageSet="main" ImageKey="Cut" DisplayStyle="Image">
                            <AutoCallBack Command="cut" Target="ds" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton ImageSet="main" ImageKey="Copy" DisplayStyle="Image">
                            <AutoCallBack Command="copy" Target="ds">
                            </AutoCallBack>
                        </px:PXToolBarButton>
                        <px:PXToolBarButton ImageSet="main" ImageKey="Paste" DisplayStyle="Image">
                            <AutoCallBack Command="paste" Target="ds">
                            </AutoCallBack>
                        </px:PXToolBarButton>
                        <px:PXToolBarSeperator>
                        </px:PXToolBarSeperator>
                        <px:PXToolBarButton Text="Up" Tooltip="Move Node Up" ImageSet="main" ImageKey="ArrowUp">
							<AutoCallBack Command="rowUp" Target="ds"/>
						</px:PXToolBarButton>
						<px:PXToolBarButton Text="Down" Tooltip="Move Node Down" ImageSet="main" ImageKey="ArrowDown">
							<AutoCallBack Command="rowDown" Target="ds">
							</AutoCallBack>
						</px:PXToolBarButton>
						<px:PXToolBarButton ImageSet="main" ImageKey="AddArticle" DisplayStyle="Text">
                            <AutoCallBack Command="addWiki" Target="ds" />
                        </px:PXToolBarButton>
					</CustomItems>
					<Actions>
						<Save Enabled="False" />
						<Search Enabled="False" />
					</Actions>
				</ActionBar>
				<Mode AllowFormEdit="True" />
				<LevelStyles>
					<RowForm Height="170px" Width="490px">
					</RowForm>
				</LevelStyles>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
