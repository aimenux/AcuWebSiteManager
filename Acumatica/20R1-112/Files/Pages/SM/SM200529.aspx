<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM200529.aspx.cs" Inherits="Page_SM200529" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="False" Width="100%" TypeName="PX.SM.CurrentUserSiteMapMaint" PrimaryView="SiteMap" >
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" AllowFilter="true">
		<Levels>
			<px:PXGridLevel DataKeyNames="NodeID" DataMember="SiteMap">
				<Columns>
					<px:PXGridColumn DataField="ScreenID" Width="90px" />
					<px:PXGridColumn DataField="Title" Width="200px" />
					<px:PXGridColumn DataField="Url" Width="300px" />
					<px:PXGridColumn DataField="AccessRights" Width="120px" TextAlign="Left" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
