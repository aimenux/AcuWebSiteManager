<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR403000.aspx.cs" Inherits="Page_AR403000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARSPCommissionDocEnq" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Selection" Width="100%" DataMember="Filter" DefaultControlID="edSalesPersonID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edSalesPersonID" runat="server" DataField="SalesPersonID" AutoRefresh="True" />
			<px:PXSelector CommitChanges="True" ID="edCommnPeriod" runat="server" DataField="CommnPeriod" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
			<px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Caption="Documents" Width="100%" SkinID="PrimaryInquire" AllowPaging="True" RestrictFields="True" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="SPDocs">
				<Columns>
					<px:PXGridColumn DataField="DocType" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="viewDocument" />
					<px:PXGridColumn DataField="AdjdDocType" />
					<px:PXGridColumn DataField="AdjdRefNbr" LinkCommand="viewOrigDocument"/>
					<px:PXGridColumn DataField="OrigDocAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="CommnblAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="CommnPct" TextAlign="Right" />
					<px:PXGridColumn DataField="CommnAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="CustomerID" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
					<px:PXGridColumn DataField="CustomerLocationID" />
					<px:PXGridColumn DataField="CustomerLocationID_Location_descr" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <ActionBar DefaultAction="viewDocument" />
	</px:PXGrid>
</asp:Content>
