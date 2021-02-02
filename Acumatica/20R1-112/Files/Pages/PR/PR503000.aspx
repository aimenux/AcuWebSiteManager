<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR503000.aspx.cs" Inherits="Page_PR503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRCreateLiabilitiesAPBill" PrimaryView="Filter">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" GroupCaption="Filter" />
			<px:PXDropDown ID="edDetailType" runat="server" DataField="DetailType" CommitChanges="True" />
			<px:PXSelector ID="edVendorID" runat="server" DataField="VendorID" CommitChanges="True" />
			<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" GroupCaption="Settings" />
			<px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" TimeMode="false" />
			<px:PXCheckBox ID="edSingleLineInvoices" runat="server" DataField="SingleLineInvoices" />
			<px:PXCheckBox ID="edCreateZeroAmountLines" runat="server" DataField="CreateZeroAmountLines" />
			<px:PXNumberEdit ID="edTotal" runat="server" DataField="Total" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXGrid ID="grid1" runat="server" Height="288px" Style="z-index: 100" Width="100%" AllowPaging="True" AdjustPageSize="Auto" SkinID="Inquire" DataSourceID="ds">
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<RowTemplate>
					<px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="30px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
					<px:PXGridColumn DataField="DetailType" />
					<px:PXGridColumn DataField="RefNbr" />
					<px:PXGridColumn DataField="CodeCD" Width="100px" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="VendorID" />
					<px:PXGridColumn DataField="EmployeeID" />
					<px:PXGridColumn DataField="EmployeeID_PREmployee_acctName" />
					<px:PXGridColumn DataField="TransactionDate" />
					<px:PXGridColumn DataField="Amount" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="300" />
	</px:PXGrid>
</asp:Content>
