<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM211500.aspx.cs" Inherits="Page_SM211500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SiteMap.Graph.ModernTranslationSetMaint" PrimaryView="TranslationSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="addToGrid" Visible="False" CommitChanges="True" DependOnTree="tree" />
            <px:PXDSCallbackCommand Name="addStandalonePages" Visible="False" CommitChanges="True" DependOnTree="tree" />
            <px:PXDSCallbackCommand Name="collect" />
            <px:PXDSCallbackCommand Name="activateAllScreens" Visible="False" CommitChanges="True" DependOnGrid="edItems" />
            <px:PXDSCallbackCommand Name="deactivateAllScreens" Visible="False" CommitChanges="True" DependOnGrid="edItems" />
		</CallbackCommands>
        <DataTrees>
			<px:PXTreeDataMember TreeView="EntitiesTree" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="edTranslationSet" runat="server" DataSourceID="ds" Width="100%" Height="150px" DataMember="TranslationSet">
        <Template>
            
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXSelector ID="edID" runat="server" DataField="Id" AutoRefresh="True" DataSourceID="ds" NullText="<NEW>" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="True" />
            <px:PXDropDown ID="edResources" runat="server" DataField="ResourceToCollect" AllowMultiSelect="True" CommitChanges="True" />
            <px:PXCheckBox ID="edIsCollected" runat="server" DataField="IsCollected" />
            
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXTextEdit ID="edCurrentSystemVersion" runat="server" DataField="CurrentSystemVersion" />
            <px:PXTextEdit ID="edSystemVersion" runat="server" DataField="SystemVersion" />
            <px:PXDateTimeEdit ID="edSystemTime" runat="server" DataField="SystemTime" Size="M" />

        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="320">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" AutoRepaint="True"
				RootNodeText="Pages" ShowRootNode="False" AllowCollapse="False" ExpandDepth="1" SelectFirstNode="true" DataMember="EntitiesTree">
				<ToolBarItems>
                    <px:PXToolBarButton Tooltip="Add To Grid" CommandSourceID="ds" CommandName="addToGrid" />
                    <px:PXToolBarButton Tooltip="Add Standalone Pages" CommandSourceID="ds" CommandName="addStandalonePages" />
				</ToolBarItems>
				<DataBindings>
					<px:PXTreeItemBinding DataMember="EntitiesTree" TextField="Title" ValueField="NodeID" ImageUrlField="Icon" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
        </Template1>

        <Template2>
            <px:PXGrid ID="edItems" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" NoteIndicator="true" FilesIndicator="true" AllowPaging="True" AdjustPageSize="Auto">
                <Levels>
                    <px:PXGridLevel DataMember="TranslationSetItem">
                        <Columns>
                            <px:PXGridColumn DataField="IsActive" Width="80" Type="CheckBox" TextAlign="Center" />
                            <px:PXGridColumn DataField="IsCollected" Width="80" Type="CheckBox" TextAlign="Center" />
                            <px:PXGridColumn DataField="ScreenID" Width="150" />
                            <px:PXGridColumn DataField="Title" Width="300" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowAddNew="False" />
                <AutoSize Enabled="True" />
                <ActionBar>
                    <CustomItems>
                        <px:PXToolBarButton>
                            <AutoCallBack Command="activateAllScreens" Target="ds" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton>
                            <AutoCallBack Command="deactivateAllScreens" Target="ds" />
                        </px:PXToolBarButton>
                    </CustomItems>
                </ActionBar>
            </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
