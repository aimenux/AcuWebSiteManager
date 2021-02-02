<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM208600.aspx.cs" Inherits="Page_SM208600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    
    <script type="text/javascript">
		function commandResult(ds, context)
		{
			if (context.command == "Save" || context.command == "Delete")
			{
				var ds = px_all[context.id];
				var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
				if (isSitemapAltered) __refreshMainMenu();
			}
		}
    </script>

	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Dashboards.DashboardMaint" PrimaryView="Dashboards">
	    <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="addNode" Visible="False" CommitChanges="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="DashboardSiteMapTree" TreeKeys="NodeID" />
            <px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    
	<px:PXFormView ID="frmHeader" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" 
        DataMember="Dashboards" Caption="Dashboard Summary" TemplateContainer="" OnDataBound="frmHeader_DataBound">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
		    <px:PXSelector ID="edName" runat="server" DataField="Name" />

			<px:PXSelector ID="edDefaultOwnerRole" runat="server" DataField="DefaultOwnerRole" AutoRefresh="True" DataSourceID="ds" CommitChanges="True" />
            <px:PXCheckBox ID="chkAllowCopy" runat="server" DataField="AllowCopy" />			
            <px:PXCheckBox ID="chkExposeViaMobile" runat="server" DataField="ExposeViaMobile" />	
            
		    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
            
		    <px:PXLayoutRule runat="server" Merge="True"/>
		    <px:PXTreeSelector ID="edScreen" runat="server" DataField="ScreenID" PopulateOnDemand="False" AutoRefresh="True" 
		                       ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="DashboardSiteMapTree" MinDropWidth="413" CommitChanges="true">
		        <DataBindings>
		            <px:PXTreeItemBinding DataMember="DashboardSiteMapTree" TextField="Title" ValueField="ScreenID" ImageUrlField="Icon" ToolTipField="TitleWithPath" />
		        </DataBindings>
		        <AutoCallBack Command="Save" Target="frmHeader"/>
		    </px:PXTreeSelector>

		    <px:PXButton ID="btnAddNode" runat="server" Text="Add New Node" CommandSourceID="ds" CommandName="addNode" />
		    <px:PXLayoutRule runat="server" />
            
		    <px:PXSelector runat="server" DataField="WorkspaceID" ID="edWorkspaceID" DisplayMode="Text"/>
		    <px:PXSelector runat="server" DataField="SubcategoryID" ID="edSubcategoryID" DisplayMode="Text"/>

		</Template>
		<CallbackCommands>
			<Save RepaintControlsIDs="gridRoles" />
		</CallbackCommands>
	</px:PXFormView>
    
    <px:PXSmartPanel ID="pnlAddNode" runat="server" CaptionVisible="True" Caption="Add New Dashboard Node" 
        LoadOnDemand="true" ShowAfterLoad="True" Key="NewNodeSettings"
        AutoCallBack-Enabled="true" AutoCallBack-Target="frmAddNode" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
		AcceptButtonID="btnAddNodeOK" CancelButtonID="btnAddNodeCancel" AutoReload="True">
		<px:PXFormView runat="server" ID="frmAddNode" DataMember="NewNodeSettings" SkinID="Transparent" DataSourceID="ds" Style="z-index: 100">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True"/>
				<px:PXTreeSelector ID="edSitemapParent" runat="server" DataField="SitemapParent" PopulateOnDemand="True" AutoRefresh="True" 
				    ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="SiteMapTree" MinDropWidth="413">
				    <DataBindings>
					    <px:PXTreeItemBinding DataMember="SiteMapTree" TextField="Title" ValueField="NodeID" ImageUrlField="Icon" ToolTipField="TitleWithPath" />
				    </DataBindings>
			    </px:PXTreeSelector>
                <px:PXTextEdit ID="edSitemapTitle" runat="server" DataField="SitemapTitle" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlAddNodeButtons" runat="server" SkinID="Buttons">
		    <px:PXButton ID="btnAddNodeOK" runat="server" DialogResult="OK" Text="Add" />
		    <px:PXButton ID="btnAddNodeCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px">
        <AutoSize Enabled="True" Container="Window" MinHeight="400" />
        <Items>
            
            <px:PXTabItem Text="Visible to:">
                <Template>
	                <px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" Height="150px"
		                AllowSearch="True" FastFilterFields="Rolename,Descr">
		                <Levels>
			                <px:PXGridLevel DataMember="Roles">
			                    <Columns>
			                        <px:PXGridColumn DataField="Selected" AllowMove="False" AllowSort="False" TextAlign="Center" Type="CheckBox" Width="80px" AutoCallBack="True" AllowCheckAll="True"/>
				                    <px:PXGridColumn DataField="Rolename" Width="200px" AllowUpdate ="False"/>
				                    <px:PXGridColumn DataField="Descr" AllowUpdate="False" Width="300px" />
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Enabled="True" MinHeight="150" />
	                </px:PXGrid>
                </Template>
            </px:PXTabItem>
            
             <px:PXTabItem Text="Parameters">
                 <Template>
                     <px:PXGrid ID="gridParameters" runat="server" DataSourceID="ds" Width="100%" Height="150px"
                         AutoAdjustColumns="True" SkinID="DetailsInTab" MatrixMode="False" OnEditorsCreated="grd_EditorsCreated_RelativeDates">
                         <Levels>
                             <px:PXGridLevel DataMember="Parameters">
                                 <RowTemplate>
                                     <px:PXCheckBox ID="chkParIsActive" runat="server" Checked="True" DataField="IsActive" />
                                     <px:PXCheckBox ID="chkParIsExpression" runat="server" DataField="IsExpression" />
									 <px:PXTextEdit ID="edParName" runat="server" DataField="Name" CommitChanges="true" />
                                     <px:PXSelector ID="edParObjectName" runat="server" DataField="ObjectName" CommitChanges="True" />
									 <px:PXDropDown ID="edParFieldName" runat="server" DataField="FieldName" CommitChanges="True" />
									 <px:PXTextEdit ID="edParDisplayName" runat="server" DataField="DisplayName" />
									 <px:PXTextEdit ID="edParDefaultValue" runat="server" DataField="DefaultValue" />
									 <px:PXCheckBox ID="chkParRequired" runat="server" DataField="Required" />
                                 </RowTemplate>
                                 <Columns>
                                     <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" AllowNull="False" />
									 <px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="60px" AllowNull="False"/>
									 <px:PXGridColumn DataField="Name" Width="150px" AutoCallBack="True" />
									 <px:PXGridColumn DataField="ObjectName" Width="150px" CommitChanges="True" />
									 <px:PXGridColumn DataField="FieldName" Type="DropDownList" Width="150px" MatrixMode="true" CommitChanges="True" />
									 <px:PXGridColumn DataField="DisplayName" Width="150px" />
									 <px:PXGridColumn DataField="IsExpression" Label="Expression" TextAlign="Center" Type="CheckBox" Width="58px" AutoCallBack="True" />
									 <px:PXGridColumn DataField="DefaultValue" Width="100px" MatrixMode="true" AllowStrings="True" />
                                 </Columns>
                             </px:PXGridLevel>
                         </Levels>
                         <Mode InitNewRow="True"/>
                         <AutoSize Enabled="True" MinHeight="150" />
                     </px:PXGrid>
                 </Template>
            </px:PXTabItem>

        </Items>
    </px:PXTab>
</asp:Content>
