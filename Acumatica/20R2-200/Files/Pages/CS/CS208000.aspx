<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS208000.aspx.cs" Inherits="Page_CS208000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ShipTermsCurrent" TypeName="PX.Objects.CS.ShipTermsMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ShipTermsCurrent" Caption="Shipping Terms Summary">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edShipTermsID" runat="server" DataField="ShipTermsID" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXDropDown ID="edFreightAmountSource" runat="server" DataField="FreightAmountSource" CommitChanges="true" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Shipping Terms Details" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="ShipTermsDetail">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXNumberEdit ID="edBreakAmount" runat="server" DataField="BreakAmount" />
					<px:PXNumberEdit ID="edFreightCostPercent" runat="server" DataField="FreightCostPercent" />
					<px:PXNumberEdit ID="edInvoiceAmountPercent" runat="server" DataField="InvoiceAmountPercent" />
					<px:PXNumberEdit ID="edShippingHandling" runat="server" DataField="ShippingHandling" />
					<px:PXNumberEdit ID="edLineHandling" runat="server" DataField="LineHandling" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="BreakAmount" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="FreightCostPercent" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="InvoiceAmountPercent" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="ShippingHandling" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="LineHandling" TextAlign="Right" Width="100px" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<Actions>
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
