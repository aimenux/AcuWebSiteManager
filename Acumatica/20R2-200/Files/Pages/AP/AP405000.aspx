<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP405000.aspx.cs" Inherits="Page_AP405000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.AP.AP1099DetailEnq" PrimaryView="YearVendorHeader"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="YearVendorHeader" Caption="Selection" DefaultControlID="edVendorID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" InitialExpandLevel="0"/>
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
			<px:PXSelector CommitChanges="True" ID="edFinYear" runat="server" DataField="FinYear" AutoRefresh="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Details" SkinID="PrimaryInquire" FastFilterFields="Descr" SyncPosition="True" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="YearVendorSummary">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="BoxNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="AP1099History__HistAmt" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
