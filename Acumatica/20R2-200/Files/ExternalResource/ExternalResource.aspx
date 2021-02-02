<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ExternalResource.aspx.cs" Inherits="ExternalResource_ExternalResource" MasterPageFile="~/MasterPages/Workspace.master"%>

<%@ MasterType VirtualPath="~/MasterPages/Workspace.master" %>
<asp:Content ID="c1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource runat="server" ID="ds" TypeName="PX.OAuthClient.ResourceMaint" PrimaryView="Resources" PageLoadBehavior="SearchSavedKeys">
	    <CallbackCommands>
            <px:PXDSCallbackCommand Name="viewApplication"  />
        </CallbackCommands>
	</px:PXDataSource>
	<asp:Literal ID="resourceHtml" runat="server" Mode="PassThrough"></asp:Literal>
</asp:Content>