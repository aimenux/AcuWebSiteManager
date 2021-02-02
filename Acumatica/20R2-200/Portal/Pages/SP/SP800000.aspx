<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP800000.aspx.cs" Inherits="Page_SP800000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:pxdatasource id="ds" runat="server" visible="True" width="100%" primaryview="CRSetupRecord"
        typename="SP.Objects.PortalSetupMaint" PageLoadBehavior="SearchSavedKeys">
        <CallbackCommands>
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="WikiMap" TreeKeys="PageID" />
            <px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
        </DataTrees>
    </px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:pxtab id="tab" runat="server" datasourceid="ds" height="500px" datamember="CRSetupRecord"
        width="100%">
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartGroup="True" GroupCaption="Portal Settings" LabelsWidth="XM" ControlSize="L" />
                    <px:PXTextEdit ID="edPortalSetupID" runat="server" DataField="PortalSetupID" />
                    <px:PXTextEdit ID="edPortalSetupName" runat="server" DataField="PortalSetupName" />
                    <px:PXDropDown ID="edDisplayDocuments" runat="server" DataField="DisplayFinancialDocuments" CommitChanges="true" />
                    <px:PXSelector ID="edRestrictByOrganizationID" runat="server" DataField="RestrictByOrganizationID" CommitChanges="true" />
                    <px:PXSelector ID="edRestrictByBranchID" runat="server" DataField="RestrictByBranchID" CommitChanges="true" />    
                    
                    <px:PXTreeSelector ID="edPortalHomePage" runat="server" DataField="CRPreferencesGeneralSetupRecord.PortalHomePage" PopulateOnDemand="True"
                        TreeDataMember="WikiMap" TreeDataSourceID="ds" InitialExpandLevel="0" ShowRootNode="False"
                        ExpandDepth="1" AllowCollapse="False" MinDropWidth="413" CommitChanges="True"> 
                        <DataBindings>
                            <px:PXTreeItemBinding DataMember="WikiMap" TextField="Title" ValueField="PageID" />
                        </DataBindings>
                        <Images>
                            <ParentImages Normal="tree@Folder" Selected="tree@FolderS" />
                            <LeafImages Normal="main@FavoritesGray" Selected="main@Favorites" />
                        </Images>
                    </px:PXTreeSelector>
                    <px:PXSelector ID="edAddressLookupPluginID" runat="server" DataField="AddressLookupPluginID" />

                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="CRM Settings" ControlSize="M" />
                    <px:PXSelector ID="edDefaultCaseClassID" runat="server" DataField="DefaultCaseClassID" />
                    <px:PXDropDown ID="edDefaultCasePriority" runat="server" DataField="DefaultCasePriority" />
                    <px:PXSelector ID="edCaseActivityNotificationTemplateID" runat="server" DataField="CaseActivityNotificationTemplateID" CommitChanges="True"/>
                    <px:PXSelector ID="edDefaultContactClassID" runat="server" DataField="DefaultContactClassID" />
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartGroup="True" GroupCaption="Email Preferences" />
                    <px:PXTextEdit ID="edPortalExternalAccessLink" runat="server" DataField="CRPreferencesGeneralSetupRecord.PortalExternalAccessLink" Size="XXL" CommitChange="true" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="B2B Ordering Settings">
                <Template>
                    <px:PXFormView runat="server" DataMember="CRSetupRecord" AllowCollapse="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" GroupCaption="General Settings"
                                LabelsWidth="XM" />
	                        <px:PXSelector ID="edSellingBranchID" runat="server" DataField="SellingBranchID" Required="True" AutoRefresh="True" />
                            <px:PXSelector ID="edDefaultOrderType" runat="server" DataField="DefaultOrderType" />
                            <px:PXSegmentMask Size="s" ID="edDefaultSubItemID" runat="server" DataField="DefaultSubItemID"
                                AutoRefresh="True" />
                            <px:PXSelector ID="edDefaultStockItemWareHouse" runat="server" DataField="DefaultStockItemWareHouse" AutoRefresh="True"
                                CommitChanges="True"/>
                            <px:PXSelector ID="edDefaultNonStockItemWareHouse" runat="server" DataField="DefaultNonStockItemWareHouse" AutoRefresh="True"
                                CommitChanges="True"/>
                            <px:PXCheckBox ID="edAvailableQty" runat="server" DataField="AvailableQty" />
                            <px:PXCheckBox ID="edBaseUOM" runat="server" DataField="BaseUOM" />
                            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" StartGroup="True"
                                GroupCaption="Default Image Settings" LabelsWidth="XM" />
                            <px:PXImageUploader Height="150px" Width="360px" ID="imgUploader" runat="server"
                                DataField="ImageUrl" AllowUpload="true" ShowComment="true" ArrowsOutside="true" SuppressLabel="True"/>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="gridWarehouse" runat="server" DataSourceID="ds" Width="100%" SkinID="Details"
                        AdjustPageSize="Auto" AllowSearch="True" FilesIndicator="False" NoteIndicator="False"
                        SyncPosition="true" AllowPaging="true">
                        <Levels>
                            <px:PXGridLevel DataMember="CRSetupINSite">
                                <Columns>
                                    <px:PXGridColumn DataField="SiteCD" Width="100px" />
                                    <px:PXGridColumn DataField="Descr" Width="400px" />
                                    <px:PXGridColumn DataField="Included" Width="120px" Type="CheckBox" TextAlign="Center" />
	                                <px:PXGridColumn DataField="BranchID" Width="100px" />
	                                <px:PXGridColumn DataField="Organization__OrganizationCD" Width="100px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowAddNew="False" AutoInsert="False" />
                        <ActionBar>
                            <Actions>
                                <ExportExcel Enabled="False" />
                                <AddNew Enabled="False" />
                                <FilterShow Enabled="False" />
                                <FilterSet Enabled="False" />
                                <NoteShow Enabled="False" />
                                <Search Enabled="False" />
                                <Delete Enabled="False" />
                                <Refresh Enabled="False" />
                                <AdjustColumns Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <AutoSize Enabled="True" Container="Parent" MinHeight="300" MinWidth="400" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:pxtab>
</asp:Content>
