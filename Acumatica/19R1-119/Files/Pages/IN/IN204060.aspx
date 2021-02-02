<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="IN204060.aspx.cs" Inherits="Page_IN204060" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" TypeName="PX.Objects.IN.INCategoryMaint"
        PrimaryView="SelectedFolders">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Down" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Up" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Copy" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Cut" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Paste" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="AddCategory" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="DeleteCategory" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="AddItemsbyClass" Visible="False" CommitChanges="True"  />
            <px:PXDSCallbackCommand Name="ViewDetails" Visible="false" DependOnGrid="gridMembers"/>
	    </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="Folders" TreeKeys="CategoryID" />
            <px:PXTreeDataMember TreeView="ParentFolders" TreeKeys="CategoryID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="formtemp" runat="server" DataSourceID="ds" DataMember="SelectedFolders" Width="100%">
        <Template>
            <px:PXTextEdit ID="edFolderID" runat="server" DataField="FolderID" CommitChanges="True" />
        </Template>
        <AutoSize Enabled="True" />
    </px:PXFormView>
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px"
                ShowRootNode="False" AllowCollapse="False" Caption="Categories" AutoRepaint="True"
                SyncPosition="True" ExpandDepth="1" DataMember="Folders" KeepPosition="True" 
                SyncPositionWithGraph="True" PreserveExpanded="True" PopulateOnDemand="true">
                <ToolBarItems>
                    <px:PXToolBarButton Text="Up" Tooltip="Move Node Up">
                        <AutoCallBack Command="Up" Enabled="True" Target="ds" />
                        <Images Normal="main@ArrowUp" />
                    </px:PXToolBarButton>
                    <px:PXToolBarButton Text="Down" Tooltip="Move Node Down">
                        <AutoCallBack Command="Down" Enabled="True" Target="ds" />
                        <Images Normal="main@ArrowDown" />
                    </px:PXToolBarButton>
                    
                    <px:PXToolBarButton Text="Add Category" Tooltip="Add Category">
                        <AutoCallBack Command="AddCategory" Enabled="True" Target="ds" />
                        <Images Normal="main@AddNew" />
                    </px:PXToolBarButton>
                    
                    <px:PXToolBarButton Text="Delete Category" Tooltip="Delete Category">
                        <AutoCallBack Command="DeleteCategory" Enabled="True" Target="ds" />
                        <Images Normal="main@Remove" />
                    </px:PXToolBarButton>
                </ToolBarItems>
                <AutoCallBack Target="form" Command="Refresh" Enabled="True" />
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="Folders" TextField="Description" ValueField="CategoryID" />
                </DataBindings>
                <AutoSize Enabled="True" />
            </px:PXTreeView>
        </Template1>

        <Template2>
                    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="CurrentCategory" 
                        Caption="Category Info" Width="100%">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="True" />
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                            <px:PXTreeSelector ID="edParentID" runat="server" DataField="ParentID" ShowRootNode="False"
                                TreeDataMember="ParentFolders" TreeDataSourceID="ds" AutoRefresh="True" ExpandDepth="1"
                                SyncPosition="True" DataMember="ParentFolders" AutoRepaint="True" CommitChanges="True" KeepPosition="True">
                                <DataBindings>
                                    <px:PXTreeItemBinding DataMember="ParentFolders" TextField="Description" ValueField="CategoryID" />
                                </DataBindings>
                            </px:PXTreeSelector>
                        </Template>
                    </px:PXFormView>

                    <px:PXGrid ID="gridMembers" runat="server" DataSourceID="ds" ActionsPosition="Top" Width="100%"
                        Caption="Category Members" SkinID="Details" CaptionVisible="true" SyncPosition="True" 
                        AutoRepaint="True" AdjustPageSize="Auto">
                        <AutoSize Enabled="True" Container="Parent"/>
                        <Mode InitNewRow="True"></Mode>
                        <Levels>
                            <px:PXGridLevel DataMember="Members">
                                <Columns>
                                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="CategorySelected" TextAlign="Center" Type="CheckBox" Width="26px" />
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="true" Width="140px" LinkCommand="viewDetails"/>
                                    <px:PXGridColumn DataField="InventoryItem__Descr" Width="200px"/>
                                    <px:PXGridColumn DataField="InventoryItem__ItemClassID" Width="140px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="InventoryItem__ItemStatus" RenderEditorText="True" Width="100px"/>
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True" DataSourceID="ds">
                                        <GridProperties FastFilterFields="InventoryItem__Descr" />
                                    </px:PXSelector>
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
                                <px:PXToolBarButton Tooltip="Copy Selected Inventory Items" DisplayStyle="Image">
                                    <AutoCallBack Command="Copy" Enabled="True" Target="ds" />
                                    <Images Normal="main@Copy" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Tooltip="Cut Selected Inventory Items" DisplayStyle="Image">
                                    <AutoCallBack Command="Cut" Enabled="True" Target="ds" />
                                    <Images Normal="main@Cut" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Tooltip="Paste Inventory Items from Buffer" DisplayStyle="Image">
                                    <AutoCallBack Command="Paste" Enabled="True" Target="ds" />
                                    <Images Normal="main@Paste" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="AddItemsbyClass" Target="ds" />
                                </px:PXToolBarButton>
                                
                                <px:PXToolBarButton Text="Inventory Details" Key="cmdviewDetails" Visible="False">
									<AutoCallBack Command="viewDetails" Target="ds" />
								</px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>

    <px:PXSmartPanel ID="PanelSelectClass" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Add Items"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="ClassInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formClassInfo" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
        <px:PXFormView ID="formCreateAccount" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
            DataMember="ClassInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown ID="edAddItemsTypes" runat="server" DataField="AddItemsTypes" CommitChanges="True" />
                <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" Text="Add" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
