<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR406500.aspx.cs" Inherits="Page_AR406500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="PopulateSavedValues" PrimaryView="Filter" TypeName="PX.Objects.AR.FailedCCPaymentEnq">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edBeginDate">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
			<px:PXDropDown CommitChanges="True" ID="edDisplayType" runat="server" DataField="DisplayType" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" />
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" />
			<px:PXSegmentMask CommitChanges="True" ID="PXSegmentMask1" runat="server" DataField="CustomerID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server"  Height="150px" Style="z-index: 100" Width="100%" Caption="Transaction List" AllowSearch="True" SkinID="PrimaryInquire" RestrictFields="True"
        FastFilterFields="Customer__AcctCD,Customer__AcctName, RefNbr" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="PaymentTrans">
				<Columns>
					<px:PXGridColumn DataField="Customer__AcctCD" LinkCommand="viewCustomer"/>
					<px:PXGridColumn DataField="Customer__AcctName" />
                    <px:PXGridColumn DataField="CustomerPaymentMethod__PaymentMethodID" LinkCommand="viewPaymentMethod" />
					<px:PXGridColumn DataField="DocType" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="OrigDocType" />
					<px:PXGridColumn DataField="OrigRefNbr" />
					<px:PXGridColumn DataField="ProcessingCenterID" />
					<px:PXGridColumn DataField="TranNbr" TextAlign="Right" Visible="False" />
					<px:PXGridColumn DataField="TranType"/>
					<px:PXGridColumn DataField="Amount" TextAlign="Right"/>
					<px:PXGridColumn DataField="ProcStatus" />
					<px:PXGridColumn DataField="TranStatus"/>
					<px:PXGridColumn DataField="RefTranNbr" TextAlign="Right"/>
					<px:PXGridColumn DataField="PCTranNumber"/>
					<px:PXGridColumn DataField="PCResponseReasonText"/>
					<px:PXGridColumn DataField="StartTime" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar DefaultAction="ViewDocument" />
		<Mode AllowAddNew="False" AllowDelete="False"  />
	</px:PXGrid>
</asp:Content>
