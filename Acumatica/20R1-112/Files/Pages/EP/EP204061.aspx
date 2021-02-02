<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="EP204061.aspx.cs" Inherits="Page_EP204061" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" TypeName="PX.TM.CompanyTreeMaint"
        PrimaryView="SelectedFolders">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Down" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Up" Visible="false" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="AddWorkGroup" CommitChanges="True" Visible="false"/>
            <px:PXDSCallbackCommand Name="DeleteWorkGroup" Visible="false" CommitChanges="True" />     
            <px:PXDSCallbackCommand Name="MoveWorkGroup" Visible="false" CommitChanges="True" />        
            <px:PXDSCallbackCommand Name="ViewDetails" Visible="false" DependOnGrid="gridMembers"/>
            <px:PXDSCallbackCommand Name="ViewEmployee" Visible="false" DependOnGrid="gridMembers"/>            
	    </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="Folders" TreeKeys="WorkGroupID" />
            <px:PXTreeDataMember TreeView="ParentFolders" TreeKeys="WorkGroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="formtemp" runat="server" DataSourceID="ds" DataMember="SelectedFolders" Width="100%">
        <Template>
            <px:PXTextEdit ID="edWorkGroupID" runat="server" DataField="WorkGroupID" CommitChanges="True" />
        </Template>
        <AutoSize Enabled="True" />
    </px:PXFormView>
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px"
                ShowRootNode="False" AllowCollapse="true" Caption="Company Tree" AutoRepaint="True"
                SyncPosition="True" ExpandDepth="1" DataMember="Folders" KeepPosition="True" 
                SyncPositionWithGraph="True" PreserveExpanded="True" PopulateOnDemand="true">
                <ToolBarItems>
                    <px:PXToolBarButton Text="Up" Tooltip="Move Node Up">
                        <AutoCallBack Command="Up" Enabled="True" Target="ds"/>                        
                        <Images Normal="main@ArrowUp" />
                    </px:PXToolBarButton>
                    <px:PXToolBarButton Text="Down" Tooltip="Move Node Down">
                        <AutoCallBack Command="Down" Enabled="True" Target="ds" />                        
                        <Images Normal="main@ArrowDown" />
                    </px:PXToolBarButton> 
                    <px:PXToolBarButton Text="Move" Tooltip="Move Node" DisplayStyle="Image">
                        <AutoCallBack Command="MoveWorkGroup" Enabled="True" Target="ds"/>                        
                        <Images Normal="main@Roles" />
                    </px:PXToolBarButton>                   
                    <px:PXToolBarButton Text="Add WorkGroup" Tooltip="Add WorkGroup" DisplayStyle="Image">
                        <AutoCallBack Command="AddWorkGroup" Enabled="True" Target="ds"/>                        
                        <Images Normal="main@AddNew" />
                    </px:PXToolBarButton>                    
                    <px:PXToolBarButton Text="Delete WorkGroup" Tooltip="Delete WorkGroup" DisplayStyle="Image">
                        <AutoCallBack Command="DeleteWorkGroup" Enabled="True" Target="ds" />                        
                        <Images Normal="main@RecordDel" />
                    </px:PXToolBarButton>
                </ToolBarItems>
                <AutoCallBack Target="form" Command="Refresh" Enabled="True" />
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="Folders" TextField="Description" ValueField="WorkGroupID" />
                </DataBindings>
                <AutoSize Enabled="True" />
            </px:PXTreeView>
        </Template1>

        <Template2>
                    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="CurrentWorkGroup" 
                        Caption="Workgroup Details" CaptionVisible="true" Width="100%">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="True" />
                        </Template>
                    </px:PXFormView>

                    <px:PXGrid ID="gridMembers"  runat="server" DataSourceID="ds" ActionsPosition="Top" Width="100%"
                        Caption="Members" SkinID="Details" CaptionVisible="true" 
                        AutoRepaint="True" AdjustPageSize="Auto" SyncPosition="true">
						<AutoSize Enabled="True" Container="Parent"/>
                        <Levels>
                            <px:PXGridLevel DataMember="Members">
                                <Columns>
									<px:PXGridColumn DataField="UserID" AutoCallBack="true"/>
									<px:PXGridColumn DataField="EPEmployee__AcctCD" AutoCallBack="True" LinkCommand="ViewEmployee" />
									<px:PXGridColumn DataField="EPEmployee__AcctName" />
									<px:PXGridColumn DataField="EPEmployeePosition__PositionID" />
									<px:PXGridColumn DataField="EPEmployee__DepartmentID" />
									<px:PXGridColumn DataField="IsOwner" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
								</Columns>
								<Mode InitNewRow="true" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXCheckBox ID="chkIsOwner" runat="server" DataField="IsOwner" />
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
                        </ActionBar>
                    </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>    
</asp:Content>

<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
     <px:PXSmartPanel ID="PanelChangeWorkgroup" runat="server" Caption="Move Workgroup"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="SelectedParentFolders" 
         AutoCallBack-Enabled="true" AutoCallBack-Target="formMoveWorkGroup" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
        <px:PXFormView ID="formMoveWorkGroup" runat="server" DataSourceID="ds" Width="100%" Caption="Move Workgroup" 
            CaptionVisible="False" SkinID="Transparent" DataMember="SelectedParentFolders">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXTreeSelector ID="edParentWGID" runat="server" DataField="WorkGroupID" ShowRootNode="False"
                                TreeDataMember="ParentFolders" TreeDataSourceID="ds" AutoRefresh="True" ExpandDepth="1"
                                SyncPosition="True" DataMember="ParentFolders" AutoRepaint="True" CommitChanges="True" KeepPosition="True">
                                <DataBindings>
                                    <px:PXTreeItemBinding DataMember="ParentFolders" TextField="Description" ValueField="WorkGroupID" />
                                </DataBindings>
                            </px:PXTreeSelector>
            </Template>
        </px:PXFormView>
        <div style="padding: 5px; text-align: right;">
            <px:PXButton ID="PXButtonOK" runat="server" Text="OK" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>
    </px:PXSmartPanel>    
</asp:Content>

