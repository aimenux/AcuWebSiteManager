<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN201500.aspx.cs" Inherits="Page_IN201500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INAvailabilitySchemeMaint" PrimaryView="Schemes">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Schemes" Style="z-index: 100" Width="100%">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ColumnSpan="2" LabelsWidth="M" StartColumn="True" />
			<px:PXSelector ID="edAvailabilitySchemeID" runat="server" DataField="AvailabilitySchemeID" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />

			<px:PXLayoutRule runat="server" StartRow="True" />
			<px:PXCheckBox ID="chkInclQtyINIssues" runat="server" AlignLeft="True" DataField="InclQtyINIssues" />
			<px:PXCheckBox ID="chkInclQtySOPrepared" runat="server" AlignLeft="True" DataField="InclQtySOPrepared" />
			<px:PXCheckBox ID="chkInclQtySOBooked" runat="server" AlignLeft="True" DataField="InclQtySOBooked" />
			<px:PXCheckBox ID="chkInclQtySOShipped" runat="server" AlignLeft="True" DataField="InclQtySOShipped" />
			<px:PXCheckBox ID="chkInclQtySOShipping" runat="server" AlignLeft="True" DataField="InclQtySOShipping" />
			<px:PXCheckBox ID="chkInclQtyINAssemblyDemand" runat="server" AlignLeft="True" DataField="InclQtyINAssemblyDemand" />
			<px:PXCheckBox ID="chkInclQtySOBackOrdered" runat="server" AlignLeft="True" DataField="InclQtySOBackOrdered" />
			
			<px:PXCheckBox ID="chkInclQtyFSSrvOrdPrepared" runat="server" AlignLeft="True" DataField="InclQtyFSSrvOrdPrepared" />
			<px:PXCheckBox ID="chkInclQtyFSSrvOrdBooked" runat="server" AlignLeft="True" DataField="InclQtyFSSrvOrdBooked" />
			<px:PXCheckBox ID="chkInclQtyFSSrvOrdAllocated" runat="server" AlignLeft="True" DataField="InclQtyFSSrvOrdAllocated" />

            <px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXCheckBox ID="chkInclQtyINReceipts" runat="server" AlignLeft="True" DataField="InclQtyINReceipts" />
			<px:PXCheckBox ID="chkInclQtyInTransit" runat="server" AlignLeft="True" DataField="InclQtyInTransit" />
			<px:PXCheckBox ID="chkInclQtyPOReceipts" runat="server" AlignLeft="True" DataField="InclQtyPOReceipts" />
			<px:PXCheckBox ID="chkInclQtyPOPrepared" runat="server" AlignLeft="True" DataField="InclQtyPOPrepared" />
			<px:PXCheckBox ID="chkInclQtyPOOrders" runat="server" AlignLeft="True" DataField="InclQtyPOOrders" />
			<px:PXCheckBox ID="chkInclQtyINAssemblySupply" runat="server" AlignLeft="True" DataField="InclQtyINAssemblySupply" />
			<px:PXCheckBox ID="chkInclQtySOReverse" runat="server" AlignLeft="True" DataField="InclQtySOReverse" />
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
