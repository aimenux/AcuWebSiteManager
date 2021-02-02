<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GD102030.aspx.cs" Inherits="Page_GD102030"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.GDPR.GDPRRestoreProcess"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	
	

	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Restore Options" Width="100%" DataMember="Filter">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="XXL" ControlSize="XXL" StartGroup="true" GroupCaption="Restore Options"/>
			<px:PXCheckBox ID="edDeleteNonRestored" runat="server" AlignLeft="true" DataField="DeleteNonRestored" CommitChanges="True"/>
		</Template>
	</px:PXFormView>
	
	

	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds"
		SkinID="Inquire" SyncPosition="True" AutoAdjustColumns="True" ActionsPosition="Top" FastFilterFields="Value">
		<Levels>
			<px:PXGridLevel DataMember="ObfuscatedItems">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="5px" AutoCallBack="True"/>

					<px:PXGridColumn DataField="UIKey" Width="5px" AllowShowHide="False" LinkCommand="OpenContact"/>
					<px:PXGridColumn DataField="Content" />
					<px:PXGridColumn DataField="CreatedDateTime" Width="15px" DisplayFormat="g"/>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
