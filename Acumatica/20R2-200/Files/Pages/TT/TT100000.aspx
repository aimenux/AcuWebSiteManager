<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TT100000.aspx.cs" Inherits="Page_TT100000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Tests" TypeName="PX.Objects.CA.CABankTranRulesTests">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds" BatchUpdate="True" SkinID="Inquire" Caption="CA Documents">
		<Levels>
			<px:PXGridLevel DataMember="Tests">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Result" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="TestName" Width="500px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Layout ShowRowStatus="False" />
	</px:PXGrid>
</asp:Content>
