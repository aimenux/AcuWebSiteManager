<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP501000.aspx.cs" Inherits="Page_AP501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="APDocumentList" TypeName="PX.Objects.AP.APDocumentRelease" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true" Caption="Documents" DataSourceID="ds" BatchUpdate="True" AdjustPageSize="Auto"
		SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="RefNbr,VendorID,VendorID_Vendor_acctName,VendorRefNbr,APInvoice__SuppliedByVendorID,APInvoice__SuppliedByVendorID_Vendor_acctName">
		<Levels>
			<px:PXGridLevel DataMember="APDocumentList">
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowOnDashboard="false" />
					<px:PXGridColumn DataField="DocType" RenderEditorText="True" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="viewDocument" />
					<px:PXGridColumn DataField="VendorID" />
					<px:PXGridColumn DataField="VendorID_Vendor_acctName" />
					<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" DisplayMode="Hint"/>
					<px:PXGridColumn DataField="VendorRefNbr" />
					<px:PXGridColumn AllowNull="False" DataField="Status" RenderEditorText="True" />
					<px:PXGridColumn DataField="DocDate" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" TextAlign="Right" MatrixMode="true" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn AllowNull="False" DataField="DocDesc" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar DefaultAction="viewDocument" />
	</px:PXGrid>
</asp:Content>
