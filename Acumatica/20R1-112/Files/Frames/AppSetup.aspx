<%@ Page Language="C#" MasterPageFile="~/MasterPages/Workspace.master" AutoEventWireup="true" CodeFile="AppSetup.aspx.cs" Inherits="Pages_Ledger" Title="Application Setup" %>

<%@ MasterType VirtualPath="~/MasterPages/Workspace.master" %>
<asp:Content ID="c1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.SM.Access">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="c2" ContentPlaceHolderID="phFav" runat="Server">
	<px:PXFormView ID="frmNav" runat="server" Height="150px" Width="100%" Caption="Shortcuts">
		<Template>
			<asp:DataList ID="siteLinks" runat="server" DataSourceID="dsSite" CellPadding="3" RepeatColumns="4" RepeatDirection="Horizontal" OnItemDataBound="siteLinks_ItemDataBound">
				<ItemTemplate>
					<asp:Image ID="linkIm" runat="server" ImageAlign="Middle" />
					<asp:HyperLink ID="linkRef" runat="server" Text='<%# Eval("Title") %>' NavigateUrl='<%# Eval("Url") %>' />
				</ItemTemplate>
			</asp:DataList>
			<asp:SiteMapDataSource ID="dsSite" StartingNodeUrl="~\Frames\AppSetup.aspx" runat="server" ShowStartingNode="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="c4" ContentPlaceHolderID="phNav" runat="Server">
</asp:Content>
<asp:Content ID="c5" ContentPlaceHolderID="phL2" runat="Server">
</asp:Content>
