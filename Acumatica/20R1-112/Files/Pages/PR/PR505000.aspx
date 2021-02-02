<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR505000.aspx.cs" Inherits="Page_PR505000" Title="Print Check" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRPrintChecks" PrimaryView="Filter" PageLoadBehavior="GoLastRecord" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" AutoRefresh="True" />
			<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
			<px:PXTextEdit ID="edNextCheckNbr" runat="server" DataField="NextCheckNbr" CommitChanges="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXNumberEdit ID="edCashBalance" runat="server" DataField="CashBalance" Enabled="False" />
			<px:PXNumberEdit ID="edSelTotal" runat="server" DataField="SelTotal" Enabled="False" />
			<px:PXNumberEdit ID="edSelCount" runat="server" DataField="SelCount" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100" Width="100%" Caption="Payments" AllowPaging="true" AdjustPageSize="Auto"
		SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="RefNbr,EmployeeID">
		<Levels>
			<px:PXGridLevel DataMember="PaymentList">
				<RowTemplate>
					<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="true" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" CommitChanges="true" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
					<px:PXGridColumn DataField="RefNbr" />
					<px:PXGridColumn DataField="TransactionDate" />
					<px:PXGridColumn DataField="DocType" />
					<px:PXGridColumn DataField="EmployeeID" />
					<px:PXGridColumn DataField="EmployeeID_PREmployee_acctName" />
					<px:PXGridColumn DataField="PayGroupID" />
					<px:PXGridColumn DataField="PayPeriodID" />
					<px:PXGridColumn DataField="NetAmount" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
