<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	CodeFile="SM203020.aspx.cs" Inherits="Page_SM207010" Title="Favorites Maintenance"
	ValidateRequest="false" %>
	
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
<script language="javascript" type="text/javascript">  
  function btnSaveClick(strResult)
  {
  	if (strResult)
  		setTimeout(function () { __refreshFavoritesMenu(strResult); }, 1);
  }

  function tlbClick(strResult)
	{
	}
	</script>
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Children" TypeName="PX.SM.FavoritesMaintenance"
		Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="CancelFavorites" />
			<px:PXDSCallbackCommand Name="SaveFavorites" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="rowDown" Visible="False" />
			<px:PXDSCallbackCommand Name="rowUp" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Favorites" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" Height="400px">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" PopulateOnDemand="True" AllowCollapse="False"
				RootNodeText="Pages" ShowRootNode="False" ExpandDepth="1" AutoRepaint="true" OnCallBack="tbCommand_CallBack">
				<AutoSize Enabled="true" />
				<ToolBarItems>
					<px:PXToolBarButton Tooltip="Move One Level Up">
						<AutoCallBack Command="left"  Handler="tlbClick">
							<Behavior PostData="Page" RepaintControlsIds="ds,tree,grid" />
						</AutoCallBack>
						<Images Normal="main@ArrowLeft" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Move One Level Down">
						<AutoCallBack Command="right"  Handler="tlbClick">
							<Behavior PostData="Page" RepaintControlsIds="ds,tree,grid" />
						</AutoCallBack>
						<Images Normal="main@ArrowRight" />
					</px:PXToolBarButton>
				</ToolBarItems>
				<AutoCallBack Target="grid" Command="Refresh" Enabled="True" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="Favorites" TextField="Title" ValueField="NodeID" ImageUrlField="Icon"
						SelectedImageField="Icon" />
				</DataBindings>
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXGrid ID="grid" runat="server" Height="300px" Width="100%" DataSourceID="ds" KeepPosition="true"
				ActionsPosition="Top" SkinID="Details" OnCallBack="tbCommand_CallBack">
				<CallbackCommands>
					<Refresh CommitChanges="True" PostData="Page" />
				</CallbackCommands>
				<OnChangeCommand Target="tree" Command="Refresh" />
				<Levels>
					<px:PXGridLevel DataMember="Children">
						<RowTemplate>
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="Title" Width="335px">
								<Header Text="Title">
								</Header>
							</px:PXGridColumn>
							<px:PXGridColumn DataField="Expanded" DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
								<Header Text="Expanded">
								</Header>
							</px:PXGridColumn>
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<Parameters>
					<px:PXControlParam ControlID="tree" Name="parent" PropertyName="SelectedValue" />
					<px:PXControlParam ControlID="grid" Name="current" PropertyName="DataValues[&quot;NodeID&quot;]" />
				</Parameters>
				<AutoSize Enabled="True" />
				<ActionBar>
					<CustomItems>
						<px:PXToolBarButton Tooltip="Move Node Up">
							<AutoCallBack Command="up" Handler="tlbClick">
								<Behavior PostData="Page" RepaintControlsIds="ds,tree,grid" />
							</AutoCallBack>
							<Images Normal="main@ArrowUp" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Tooltip="Move Node Down">
							<AutoCallBack Command="down" Handler="tlbClick">
								<Behavior PostData="Page" RepaintControlsIds="ds,tree,grid" />
							</AutoCallBack>
							<Images Normal="main@ArrowDown" />
						</px:PXToolBarButton>
					</CustomItems>
				</ActionBar>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
