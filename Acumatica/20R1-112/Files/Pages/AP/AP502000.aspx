<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP502000.aspx.cs" Inherits="Page_AP502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APApproveBills" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edSelectionDate" TabIndex="5700" DataSourceID="ds">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="S" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edSelectionDate" runat="server" DataField="SelectionDate" />

			<px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" DataSourceID="ds" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" SuppressLabel="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowApprovedForPayment" runat="server" DataField="ShowApprovedForPayment" AlignLeft="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowNotApprovedForPayment" runat="server" DataField="ShowNotApprovedForPayment" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkOverDueIn" runat="server" DataField="ShowPayInLessThan" ControlSize="M" AlignLeft="True" Size="M" />
			<px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edOverDueIn" runat="server" DataField="PayInLessThan" SuppressLabel="True" />
			<px:PXTextEdit SuppressLabel="True" ID="edDays1" DataField="Days" runat="server" SkinID="Label" Enabled="False" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkDiscountExparedWithinLast" runat="server" DataField="ShowDueInLessThan" ControlSize="M" AlignLeft="True" Size="M" />
			<px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edDiscountExparedWithinLast" runat="server" DataField="DueInLessThan" SuppressLabel="True" />
			<px:PXTextEdit SuppressLabel="True" ID="edDays2" DataField="Days" runat="server" SkinID="Label" Enabled="False" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkDiscountExpiresInLessThan" runat="server" DataField="ShowDiscountExpiresInLessThan" AlignLeft="True" ControlSize="M" Size="M" />
			<px:PXNumberEdit CommitChanges="True" Size="xxs" ID="edDiscountExpiresInLessThan" runat="server" DataField="DiscountExpiresInLessThan" SuppressLabel="True" />
			<px:PXTextEdit SuppressLabel="True" ID="edDays3" DataField="Days" runat="server" SkinID="Label" Enabled="False" />
			<px:PXLayoutRule runat="server" />

			<px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="S" StartColumn="True" />
			<px:PXNumberEdit ID="edCurySelecttedForPayment" runat="server" DataField="CuryApprovedTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDocumentsTotal" runat="server" DataField="CuryDocsTotal" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100" Width="100%" Caption="Pending Documents" AllowPaging="true" AdjustPageSize="Auto" SyncPosition="true" SkinID="PrimaryInquire" 
		FastFilterFields="RefNbr,DocDesc,VendorID,VendorID_Vendor_acctName,SuppliedByVendorID,SuppliedByVendorID_Vendor_acctName">
		<Levels>
			<px:PXGridLevel DataMember="APDocumentList">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXCheckBox ID="chkSeparateCheck" runat="server" DataField="SeparateCheck" />
					<px:PXSegmentMask ID="edPayLocationID" runat="server" AutoRefresh="True" DataField="PayLocationID" />
					<px:PXDateTimeEdit ID="edPayDate" runat="server" DataField="PayDate" />
					<px:PXSelector ID="edPayTypeID" runat="server" DataField="PayTypeID" AutoCallBack="true" />
					<px:PXSegmentMask ID="edPayAccountID" runat="server" DataField="PayAccountID" AutoRefresh="True" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="PaySel" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" 
											AllowSort="False" AllowMove="False" Width="30px" CommitChanges="true" />
					<px:PXGridColumn DataField="DocType" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="viewDocument" />
					<px:PXGridColumn DataField="DocDesc" />
					<px:PXGridColumn DataField="VendorID" />
					<px:PXGridColumn DataField="VendorID_Vendor_acctName" />
					<px:PXGridColumn DataField="SuppliedByVendorID" DisplayMode="Hint"/>
					<px:PXGridColumn DataField="SeparateCheck" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="PayDate" CommitChanges="true" />
					<px:PXGridColumn DataField="DueDate" />
					<px:PXGridColumn DataField="DiscDate" />
					<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="PayLocationID" />
					<px:PXGridColumn DataField="PayTypeID" CommitChanges="true" />
					<px:PXGridColumn DataField="PayAccountID" CommitChanges="true" />
					<px:PXGridColumn DataField="InvoiceNbr" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
