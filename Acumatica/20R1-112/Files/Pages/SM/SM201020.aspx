<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	CodeFile="SM201020.aspx.cs" Inherits="Page_SM200000" Title="Access Rights Maintenance"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="EntityRoles" TypeName="PX.SiteMap.Graph.ModernAccess"
		Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PostData="Page" />
			<px:PXDSCallbackCommand Name="Save" PostData="Page" CommitChanges="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="EntitiesWithLeafs" TreeKeys="NodeID,CacheName,MemberName" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" PopulateOnDemand="True" RootNodeText="Entities"
				ShowRootNode="False" ExpandDepth="1" AllowCollapse="False" Caption="Sitemap Tree" CaptionVisible="False"
				 OnDataBinding="tree_DataBinding">
				<AutoCallBack Target="grid" Command="Refresh" />
				<AutoSize Enabled="True" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="EntitiesWithLeafs" TextField="Text" ValueField="Path" ImageUrlField="Icon" ToolTipField="Description" />
				</DataBindings>
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXGrid ID="grid" runat="server" Height="200px" Width="100%" Style="z-index: 100; position: relative;"
				DataSourceID="ds" AllowSearch="True" ActionsPosition="None" SkinID="Details" MatrixMode="true" SyncPosition="true" FastFilterFields="RoleName">
				<CallbackCommands>
					<Refresh CommitChanges="True" PostData="Page" RepaintControls="All" />
				</CallbackCommands>
				<Levels>
					<px:PXGridLevel DataMember="EntityRoles">
						<Columns>
							<px:PXGridColumn AllowUpdate="False" DataField="ScreenID" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="CacheName" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="MemberName" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="RoleName" Width="230px" />
							<px:PXGridColumn AllowUpdate="False" DataField="Guest" Width="70px" Type="CheckBox" TextAlign="Center" />
							<px:PXGridColumn AllowUpdate="False" DataField="RoleDescr" Width="300px" />
							<px:PXGridColumn AllowNull="False" DataField="RoleRight" TextAlign="Left" CommitChanges="true" Width="120px" AllowResize="False"/>
							<px:PXGridColumn AllowNull="False" DataField="InheritedByChildren" TextAlign="Center" Type="CheckBox" Width="70px" CommitChanges="true" />
						</Columns>
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGridLevel>
				</Levels>
				<Parameters>
					<px:PXControlParam ControlID="tree" Name="path" PropertyName="SelectedValue" Type="String" />
				</Parameters>
				<AutoSize Enabled="True" />
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
