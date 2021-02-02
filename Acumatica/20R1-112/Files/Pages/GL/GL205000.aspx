<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL205000.aspx.cs" Inherits="Page_GL205000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Details" TypeName="PX.Objects.GL.GLBudgetTreeMaint"
		Visible="True" BorderStyle="NotSet" Height="42px" SuspendUnloading="False">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="Left" Visible="false" />
			<px:PXDSCallbackCommand Name="Right" Visible="false" />
			<px:PXDSCallbackCommand Name="Up" Visible="false" />
			<px:PXDSCallbackCommand Name="Down" Visible="false" />
			<px:PXDSCallbackCommand Name="DeleteGroup" Visible="false" />
			<px:PXDSCallbackCommand Name="Delete" Visible="false" DependOnGrid="grid" />
			<px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="preload" DependOnGrid="grid" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="showPreload" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="configureSecurity" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Tree" TreeKeys="GroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="270" CollapseDirection="Panel1"
		Panel1MinSize="1">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" PopulateOnDemand="True" RootNodeText="Budget"
				ShowRootNode="False" ExpandDepth="4" DataSourceID="ds" Caption="Budget Tree" SelectFirstNode="true" >
			    <AutoCallBack Target="formPanel" Command="Refresh"/>				
                <Images>
                    <ParentImages Normal="tree@Folder" Selected="tree@FolderS" />
                    <LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
                </Images>
				<DataBindings>
					<px:PXTreeItemBinding DataMember="Tree" TextField="Description" ValueField="GroupID" />
				</DataBindings>
				<ToolBarItems>
					<px:PXToolBarButton Tooltip="Delete node" ImageKey="Remove">
						<AutoCallBack Command="DeleteGroup" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Move left" ImageKey="ArrowLeft">
						<AutoCallBack Command="Left" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Move right" ImageKey="ArrowRight">
						<AutoCallBack Command="Right" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Move node up" ImageKey="ArrowUp">
						<AutoCallBack Command="Up" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Move node down" ImageKey="ArrowDown">
						<AutoCallBack Command="Down" Target="ds" />
					</px:PXToolBarButton>
				</ToolBarItems>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
				AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds"
				SkinID="Details" TabIndex="900" AutoAdjustColumns="true" Caption="Subarticles"
				CaptionVisible="True" SyncPosition="true">
				<AutoCallBack Command="Refresh" Target="formPanel" ActiveBehavior="True">
					<Behavior RepaintControlsIDs="formPanel" BlockPage="True" CommitChanges="True"/>
				</AutoCallBack>
				<Levels>
					<px:PXGridLevel DataMember="Details">
						<RowTemplate>
							<px:PXCheckBox ID="edIsGroup" runat="server" DataField="IsGroup">
							</px:PXCheckBox>
							<px:PXSelector ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="true">
							</px:PXSelector>
							<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID">
							</px:PXSegmentMask>
							<px:PXTextEdit ID="edDescription" runat="server" DataField="Description">
							</px:PXTextEdit>
							<px:PXMaskEdit ID="edAccountMask" runat="server" DataField="AccountMask">
							</px:PXMaskEdit>
							<px:PXMaskEdit ID="edSubMask" runat="server" DataField="SubMask">
							</px:PXMaskEdit>
							<px:PXCheckBox ID="edSecured" runat="server" DataField="Secured">
							</px:PXCheckBox>
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="IsGroup" TextAlign="Center" Type="CheckBox" AutoCallBack="true">
							</px:PXGridColumn>
							<px:PXGridColumn DataField="AccountID" AutoCallBack="true">
							</px:PXGridColumn>
							<px:PXGridColumn DataField="SubID" AutoCallBack="true">
							</px:PXGridColumn>
							<px:PXGridColumn DataField="Description">
							</px:PXGridColumn>
							<px:PXGridColumn DataField="AccountMask">
							</px:PXGridColumn>
							<px:PXGridColumn DataField="SubMask">
							</px:PXGridColumn>
							<px:PXGridColumn DataField="Secured" TextAlign="Center" Type="CheckBox">
							</px:PXGridColumn>
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<Parameters>
					<px:PXControlParam ControlID="tree" Name="GroupID" PropertyName="SelectedValue" />
				</Parameters>
				<OnChangeCommand Target="tree" Command="Refresh" />
				<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
				<ActionBar>
					<Actions>
						<Delete Enabled="False" />
						<ExportExcel Enabled="False" />
					</Actions>
					<CustomItems>
						<px:PXToolBarButton Tooltip="Delete node" ImageKey="Remove">
							<AutoCallBack Command="Delete" Target="ds" />
                           <ActionBar GroupIndex ="0" Order="5" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Text="Preload Articles" Key="showPreload">
							<AutoCallBack Target="ds" Command="showPreload" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Text="Configure Security" Key="configureSecurity">
							<AutoCallBack Target="ds" Command="configureSecurity" />
						</px:PXToolBarButton>
					</CustomItems>
				</ActionBar>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
	<px:PXSmartPanel ID="PXSmartPanel" runat="server" CommandName="preload" CommandSourceID="ds"
		Caption="Preload Accounts" Key="Details" CaptionVisible="True" Style="display: none;" AutoCallBack-Enabled="true" 
        AutoCallBack-Target="formPanel" AutoCallBack-Command="Refresh">
		<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="PreloadFilter" >
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="SM"
					ControlSize="M" />
				<px:PXSelector ID="prFromAccount" runat="server" DataField="fromAccount" LabelsWidth="SM"
					ControlSize="M" CommitChanges="true"/>					
				<px:PXSelector ID="prToAccount" runat="server" DataField="toAccount" LabelsWidth="SM"
					ControlSize="M" CommitChanges="true"/>
				<px:PXSegmentMask ID="prSubCD" runat="server" DataField="SubIDFilter"
					DataSourceID="ds" LabelsWidth="SM" ControlSize="SM" Wildcard="?" />
				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartRow="True" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="cbOK" runat="server" DialogResult="OK" Text="Ok">
					</px:PXButton>
					<px:PXButton ID="cbCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
</asp:Content>
