<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	CodeFile="SM200520.aspx.cs" Inherits="Page_SM204000" Title="Site Map Maintenance"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">

	<script type="text/javascript">
	    function commandResult(ds, context) {
	        if (context.command == "Save") {
	            __refreshMainMenu();
	        }
	    }
	</script>

	<px:PXDataSource ID="ds"  Visible="True" Width="100%" runat="server" PrimaryView="Children" TypeName="PX.SM.SiteMapMaint">
	    <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="rowDown" Visible="False" DependOnGrid="grid"  />
			<px:PXDSCallbackCommand Name="rowUp" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="copy" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand Name="paste" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="cut" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" 
				RootNodeText="Pages" ShowRootNode="False" AllowCollapse="False" ExpandDepth="1" SelectFirstNode="true">
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
						    <px:PXGridColumn DataField="ScreenID" DisplayFormat="CC.CC.CC.CC" CommitChanges="True" Width="120px" />  
							<px:PXGridColumn DataField="Title" Width="200px"> </px:PXGridColumn>
							<px:PXGridColumn DataField="Icon" Width="200px"> </px:PXGridColumn>
							<px:PXGridColumn DataField="Url" Width="200px" CommitChanges="True"> </px:PXGridColumn>
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
					</CustomItems>
					<Actions>
						<Save Enabled="False" />
						<Search Enabled="False" />
					</Actions>
				</ActionBar>
				<Mode AllowFormEdit="True" />
				<LevelStyles>
					<RowForm Height="170px">
					</RowForm>
				</LevelStyles>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
