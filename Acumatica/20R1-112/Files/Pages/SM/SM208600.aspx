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
	</px:PXDataSource>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    
	<px:PXFormView ID="frmHeader" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" 
        DataMember="Dashboards" Caption="Dashboard Summary" TemplateContainer="" OnDataBound="frmHeader_DataBound">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
		    <px:PXSelector ID="edName" runat="server" DataField="Name" />
            
            <%-- For Copy&Paste --%>
            <px:PXNumberEdit ID="edWorkspace1Size" runat="server" DataField="Workspace1Size" Visible="False" />
            <px:PXNumberEdit ID="edWorkspace2Size" runat="server" DataField="Workspace2Size" Visible="False" />

			<px:PXSelector ID="edDefaultOwnerRole" runat="server" DataField="DefaultOwnerRole" AutoRefresh="True" DataSourceID="ds" CommitChanges="True" />
            <px:PXCheckBox ID="chkAllowCopy" runat="server" DataField="AllowCopy" />			
            <px:PXCheckBox ID="chkExposeViaMobile" runat="server" DataField="ExposeViaMobile" />	
            
		    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
            
		    <px:PXCheckBox ID="chkVisible" runat="server" DataField="Visible" CommitChanges="true" />
            <px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" />
		    <px:PXSelector runat="server" DataField="WorkspaceID" ID="edWorkspaceID" DisplayMode="Text"/>
		    <px:PXSelector runat="server" DataField="SubcategoryID" ID="edSubcategoryID" DisplayMode="Text"/>

		</Template>
		<CallbackCommands>
			<Save RepaintControlsIDs="gridRoles" />
		</CallbackCommands>
	</px:PXFormView>

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px">
        <AutoSize Enabled="True" Container="Window" MinHeight="400" />
        <Items>
            
            <px:PXTabItem Text="Visible To:">
                <Template>
                    <px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" SkinID="DetailsInTab" Width="100%" Height="150px"
                               AllowSearch="True" FastFilterFields="Rolename,Descr" MatrixMode="true" SyncPosition="true">
                        <CallbackCommands>
                            <Refresh CommitChanges="True" PostData="Page" RepaintControls="All" />
                        </CallbackCommands>
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False"></AddNew>
                                <Delete Enabled="False"></Delete>
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="EntityRoles">
                                <Columns>
                                    <px:PXGridColumn AllowUpdate="False" DataField="ScreenID" Visible="False" AllowShowHide="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CacheName" Visible="False" AllowShowHide="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="MemberName" Visible="False" AllowShowHide="False" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="RoleName" Width="230px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Guest" Width="70px" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="RoleDescr" Width="300px" />
                                    <px:PXGridColumn AllowNull="False" DataField="RoleRight" TextAlign="Left" CommitChanges="true"/>
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
            
            <%-- For Copy&Paste --%>
            <px:PXTabItem Text="Widgets" Visible="False">
                <Template>
                    <px:PXGrid ID="gridWidgets" runat="server" DataSourceID="ds" Width="100%" Height="150px"
                        SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="MasterWidgets">
                                <Columns>
                                    <px:PXGridColumn DataField="OwnerName" />
                                    <px:PXGridColumn DataField="Caption" />
                                    <px:PXGridColumn DataField="Column" />
                                    <px:PXGridColumn DataField="Row" />
                                    <px:PXGridColumn DataField="Workspace" />
                                    <px:PXGridColumn DataField="Width" />
                                    <px:PXGridColumn DataField="Height" />
                                    <px:PXGridColumn DataField="Type" />
                                    <px:PXGridColumn DataField="Settings" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

        </Items>
    </px:PXTab>
</asp:Content>
