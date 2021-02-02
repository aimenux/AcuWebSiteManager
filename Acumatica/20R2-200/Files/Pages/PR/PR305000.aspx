<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR305000.aspx.cs" Inherits="Page_PR305000" Title="Direct Deposit Batch" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRDirectDepositBatchEntry" PrimaryView="Document">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Document" Style="z-index: 100" Width="100%">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXSelector runat="server" DataField="BatchNbr" ID="edBatchNbr" />
			<px:PXDropDown runat="server" DataField="Status" ID="edStatus" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" Size="" runat="server" DataField="Hold" ID="chkHold" />
			<px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="TranDate" ID="edTranDate" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" runat="server" DataField="CashAccountID" ID="edCashAccountID" Enabled="False" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="PaymentMethodID" ID="edPaymentMethodID" Enabled="False" />
			<px:PXDateTimeEdit Size="M" runat="server" DataField="ExportTime" ID="edExportTime" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit runat="server" DataField="TranDesc" ID="edTranDesc" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXNumberEdit runat="server" Enabled="False" DataField="CuryDetailTotal" ID="edCuryDetailTotal" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="BatchPaymentsDetails">
				<Columns>
					<px:PXGridColumn DataField="OrigDocType" />
					<px:PXGridColumn DataField="PRPayment__RefNbr" LinkCommand="ViewPRDocument" />
					<px:PXGridColumn DataField="PRPayment__Status" />
					<px:PXGridColumn DataField="PRPayment__DocDesc" />
					<px:PXGridColumn DataField="PRPayment__EmployeeID" />
					<px:PXGridColumn DataField="PRPayment__PayGroupID" />
					<px:PXGridColumn DataField="PRPayment__PayPeriodID" />
					<px:PXGridColumn DataField="PRDirectDepositSplit__Amount" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
