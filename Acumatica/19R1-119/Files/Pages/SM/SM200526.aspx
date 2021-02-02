<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM200526.aspx.cs" Inherits="Page_SM200560" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.SM.MobileSiteMapMaint" PrimaryView="SiteMap">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" TabIndex="1900">
		<Levels>
			<px:PXGridLevel DataKeyNames="NodeID" DataMember="SiteMap">
				<RowTemplate>
					<px:PXTextEdit ID="edTitle" runat="server" DataField="Title">
					</px:PXTextEdit>
					<px:PXTextEdit ID="edUrl" runat="server" DataField="Url">
					</px:PXTextEdit>
					<px:PXMaskEdit ID="edScreenID" runat="server" DataField="ScreenID">
					</px:PXMaskEdit>
					<px:PXMaskEdit ID="edIndent" runat="server" DataField="Indent">
					</px:PXMaskEdit>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="ScreenID">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Title" Width="200px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Url" Width="200px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Indent" Width="200px">
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="True">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
