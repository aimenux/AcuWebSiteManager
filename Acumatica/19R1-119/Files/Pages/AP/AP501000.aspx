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
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="30px" AllowOnDashboard="false" />
					<px:PXGridColumn DataField="DocType" Width="117px" RenderEditorText="True" />
					<px:PXGridColumn DataField="RefNbr" Width="108px" LinkCommand="viewDocument" />
					<px:PXGridColumn DataField="VendorID" Width="81px" />
					<px:PXGridColumn DataField="VendorID_Vendor_acctName" Width="140px" />
					<px:PXGridColumn DataField="APInvoice__SuppliedByVendorID" Width="220px" DisplayMode="Hint"/>
					<px:PXGridColumn DataField="VendorRefNbr" Width="108px" />
					<px:PXGridColumn AllowNull="False" DataField="Status" RenderEditorText="True" Width="126px" />
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="FinPeriodID" Width="63px" />
					<px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" MatrixMode="true" />
					<px:PXGridColumn DataField="CuryID" Width="80px" />
					<px:PXGridColumn AllowNull="False" DataField="DocDesc" Width="180px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar DefaultAction="viewDocument" />
	</px:PXGrid>
</asp:Content>
