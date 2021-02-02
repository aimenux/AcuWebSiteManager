<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM301010.aspx.cs" Inherits="Page_SM301010" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
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
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.OAuthClient.ResourceMaint" PrimaryView="Resources" SuspendUnloading="False">
	    <ClientEvents CommandPerformed="commandResult" />
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewResource"  />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" TabIndex="100" DataMember="Resources" OnDataBound="form_DataBound">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True"/>
            <px:PXSelector ID="edApplicationID" runat="server" DataField="ApplicationID" DataSourceID="ds" CommitChanges="True" AllowAddNew="True" AllowEdit="True" ></px:PXSelector>
            <px:PXSelector ID="edResourceId" runat="server" DataField="ResourceCD" DisplayMode="Text" NullText="<NEW>" DataSourceID="ds" CommitChanges="True"></px:PXSelector>
            <px:PXSelector ID="edResourceName" runat="server" DataField="ResourceName" DataSourceID="ds" CommitChanges="True" AutoRefresh="True">
            </px:PXSelector>
            <px:PXTextEdit ID="edResourceUrl" runat="server" DataField="ResourceUrl" DefaultLocale="">
            </px:PXTextEdit>
		    <px:PXCheckBox ID="chkVisible" runat="server" DataField="Visible" CommitChanges="true" />
		    <px:PXLayoutRule runat="server" StartColumn="True"/>
            <px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" CommitChanges="True"/>
		    <px:PXSelector runat="server" DataField="WorkspaceID" ID="edWorkspaceID" DisplayMode="Text"/>
		    <px:PXSelector runat="server" DataField="SubcategoryID" ID="edSubcategoryID" DisplayMode="Text"/>
		    <px:PXTextEdit runat="server" DataField="SitemapScreenID" ID="edSitemapScreenID" />
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    
	<px:PXGrid ID="gridRoles" runat="server" DataSourceID="ds" Style="z-index: 100" Height="100%" Width="100%" ActionsPosition="Top" SkinID="Inquire" Caption="Visible to:"
		AllowSearch="True" FastFilterFields="Rolename,Descr" TabIndex="1500">
		<Levels>
			<px:PXGridLevel DataMember="Roles">
			    <RowTemplate>
                    <px:PXDropDown ID="edAccessRights" runat="server" DataField="AccessRights" CommitChanges="True">
                    </px:PXDropDown>
                </RowTemplate>
			    <Columns>
			        <px:PXGridColumn DataField="AccessRights" TextAlign="Right" CommitChanges="True" AutoCallBack="True"/>
			        <px:PXGridColumn DataField="Rolename" Width="200px" AllowUpdate ="False"/>
				    <px:PXGridColumn DataField="Descr" AllowUpdate="False" Width="300px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" MinWidth="300" />
	</px:PXGrid>

</asp:Content>
