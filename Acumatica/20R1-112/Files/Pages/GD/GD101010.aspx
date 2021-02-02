<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GD101010.aspx.cs" Inherits="Page_GD101010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Log" TypeName="PX.Objects.GDPR.GDPRToolsAuditMaint" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	
	

	<px:PXGrid ID="grid" runat="server" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds"
		SkinID="PrimaryInquire" SyncPosition="True" AutoAdjustColumns="True" ActionsPosition="Top" FastFilterFields="UIKey">
		<Levels>
			<px:PXGridLevel DataMember="Log">
				<Columns>
					<px:PXGridColumn DataField="PseudonymizationStatus" Width="50px" AllowShowHide="False"/>
					<px:PXGridColumn DataField="UIKey" Width="30px" AllowShowHide="False" LinkCommand="OpenContact"/>
					<px:PXGridColumn DataField="TableName" />
					<px:PXGridColumn DataField="CreatedByID" />
					<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
	
	

</asp:Content>
