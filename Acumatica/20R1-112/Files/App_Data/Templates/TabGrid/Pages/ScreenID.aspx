<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="ScreenID.aspx.cs" Inherits="Page_ScreenID" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/TabDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="GraphNamespace.GraphName"
        PrimaryView="MasterView"
        >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" AllowAutoHide="false">
		<Items>
			<px:PXTabItem Text="Tab item 1">
			</px:PXTabItem>
			<px:PXTabItem Text="Tab item 2">
			</px:PXTabItem>
		</Items>
	</px:PXTab>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="DetailsView">
			    <Columns>
			        
			    </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>
