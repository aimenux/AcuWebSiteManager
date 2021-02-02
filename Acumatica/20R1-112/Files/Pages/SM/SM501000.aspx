<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM501000.aspx.cs" Inherits="Page_SM301005" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.OAuthClient.ExternalApplicationProcess" PrimaryView="ExternalApplications" >
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" TabIndex="1300">
		<Levels>
			<px:PXGridLevel DataKeyNames="ApplicationID,TokenID" DataMember="ExternalApplications">
			    <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ApplicationID" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Type">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ApplicationName" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="UtcExpiredOn" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Bearer" Width="200px">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
