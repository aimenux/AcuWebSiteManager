<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS207500.aspx.cs" Inherits="Page_CS207500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" PrimaryView="ShippingZones" TypeName="PX.Objects.CS.ShippingZoneMaint" Visible="True"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True" DataSourceID="ds" SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="ShippingZones">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXMaskEdit ID="edZoneID" runat="server" DataField="ZoneID" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="ZoneID" Width="100px" />
					<px:PXGridColumn DataField="Description" Width="250px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar>
			<Actions>
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowUpload="True" />
	</px:PXGrid>
</asp:Content>
