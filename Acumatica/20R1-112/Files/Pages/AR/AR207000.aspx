<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR207000.aspx.cs" Inherits="Page_AR207000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.Objects.AR.ARTempCrLimitMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Selection" DataMember="Filter">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" Caption="Temporary Credit Limit History"
		SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="ARTempCreditLimitRecord">
				<Columns>
					<px:PXGridColumn DataField="CustomerID" TextAlign="Right" Visible="False" />
					<px:PXGridColumn DataField="StartDate" AutoCallBack="True" />
					<px:PXGridColumn DataField="EndDate" AutoCallBack="True" />
					<px:PXGridColumn DataField="TempCreditLimit" TextAlign="Right" />
					<px:PXGridColumn DataField="LineID" TextAlign="Right" Visible="False" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<Mode InitNewRow="True" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
