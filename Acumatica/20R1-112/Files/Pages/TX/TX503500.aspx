<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX503500.aspx.cs" Inherits="Page_TX503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" Visible="true" runat="server" TypeName="PX.Objects.TX.ProcessInputSVAT" PrimaryView="Filter" />
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" DefaultControlID="edTaxAgencyID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
			<px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" CommitChanges="True" />
			<px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
			<px:PXSegmentMask ID="edTaxAgencyID" runat="server" DataField="TaxAgencyID" CommitChanges="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" CommitChanges="True" />
			<px:PXDropDown ID="edReversalMethod" runat="server" DataField="ReversalMethod" SelectedIndex="-1" CommitChanges="True" />
			<px:PXNumberEdit ID="edTotalTaxAmount" runat="server" DataField="TotalTaxAmount" Enabled="false" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" ActionsPosition="Top"
		SkinID="PrimaryInquire" Caption="Transactions" FastFilterFields="RefNbr, TaxID" SyncPosition="true" >
		<Levels>
			<px:PXGridLevel DataMember="SVATDocuments">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" CommitChanges="true"/>
					<px:PXGridColumn DataField="Module" />
					<px:PXGridColumn DataField="DisplayDocType" />
					<px:PXGridColumn DataField="AdjdRefNbr" LinkCommand="ViewDocument" AutoCallBack="true" />
					<px:PXGridColumn DataField="AdjdDocDate" />
					<px:PXGridColumn DataField="DisplayStatus" />
					<px:PXGridColumn DataField="TaxID" />
					<px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="TaxableAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="TaxAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="TaxInvoiceNbr" />
					<px:PXGridColumn DataField="TaxInvoiceDate" />
					<px:PXGridColumn DataField="DisplayCounterPartyID" DisplayMode="Hint"/>
					<px:PXGridColumn DataField="DisplayDescription" />
					<px:PXGridColumn DataField="DisplayDocRef" />
					<px:PXGridColumn DataField="AdjBatchNbr" LinkCommand="ViewBatch" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
