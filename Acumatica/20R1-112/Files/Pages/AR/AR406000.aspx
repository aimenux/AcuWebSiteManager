<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR406000.aspx.cs" Inherits="Page_AR406000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.CCTransactionsHistoryEnq">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edPaymentMethodID" TabIndex="2100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" />
			<px:PXMaskEdit CommitChanges="True" ID="emPartialCardNumber" runat="server" DataField="PartialCardNumber" />
			<px:PXTextEdit CommitChanges="True" ID="edNameOnCard" runat="server" DataField="NameOnCard" />
			<px:PXNumberEdit ID="edNumberOfCards" runat="server"  DataField="NumberOfCards" />
			<px:PXSelector CommitChanges="True" ID="edPMInstanceID" runat="server" AutoRefresh="True" DataField="PMInstanceID"/>
			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
			<px:PXSelector CommitChanges="True" ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100" Width="100%" Caption="Transaction List" AllowSearch="True" AllowPaging="True" SkinID="PrimaryInquire" RestrictFields="True"
        FastFilterFields="Customer__AcctCD, Customer__AcctName, CCProcTran__RefNbr" SyncPosition="true" >
		<Levels>
			<px:PXGridLevel DataMember="CCTrans">
				<Columns>
					<px:PXGridColumn DataField="Customer__AcctCD" LinkCommand="viewCustomer" />
					<px:PXGridColumn DataField="Customer__AcctName" />
                    <px:PXGridColumn DataField="CustomerPaymentMethod__PaymentMethodID" LinkCommand="viewPaymentMethod" />
					<px:PXGridColumn DataField="CCProcTran__DocType" />
					<px:PXGridColumn DataField="CCProcTran__RefNbr" LinkCommand="viewDocument" />
					<px:PXGridColumn DataField="CCProcTran__OrigDocType" />
					<px:PXGridColumn DataField="CCProcTran__OrigRefNbr" />
					<px:PXGridColumn DataField="CustomerPaymentMethod__Descr" />
					<px:PXGridColumn DataField="TranNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="ProcessingCenterID" />
					<px:PXGridColumn DataField="TranType" RenderEditorText="True" />
					<px:PXGridColumn DataField="TranStatus" RenderEditorText="True" />
					<px:PXGridColumn DataField="Amount" TextAlign="Right" />
					<px:PXGridColumn DataField="RefTranNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="PCTranNumber" />
					<px:PXGridColumn DataField="AuthNumber" />
					<px:PXGridColumn DataField="PCResponseReasonText" />
					<px:PXGridColumn DataField="StartTime" />
					<px:PXGridColumn DataField="ProcStatus" RenderEditorText="True" />
					<px:PXGridColumn DataField="CVVVerificationStatus" RenderEditorText="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" MinWidth="50" />
        <ActionBar DefaultAction="viewDocument" />
	</px:PXGrid>
</asp:Content>