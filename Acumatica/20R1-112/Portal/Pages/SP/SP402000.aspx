<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SP402000.aspx.cs" Inherits="Page_SP402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AR.ARDocumentEnq" PrimaryView="Filter" 
		PageLoadBehavior="PopulateSavedValues" style="float:left">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" Visible="False"/>
			<px:PXDSCallbackCommand Name="PreviousPeriod" StartNewGroup="True" HideText="True" Visible="False"/>
			<px:PXDSCallbackCommand Name="NextPeriod" HideText="True" Visible="False"/>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocument" Visible="false" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="PayDocument" Visible="false" />
			<px:PXDSCallbackCommand Name="CreateInvoice" Visible="false" />
			<px:PXDSCallbackCommand Name="CreatePayment" Visible="false" />
			<px:PXDSCallbackCommand Name="ARBalanceByCustomerReport"/>
			<px:PXDSCallbackCommand Name="CustomerHistoryReport" Visible="False"/>
			<px:PXDSCallbackCommand Name="ARAgedPastDueReport"/>
			<px:PXDSCallbackCommand Name="ARAgedOutstandingReport" Visible="False"/>
			<px:PXDSCallbackCommand Name="ARRegisterReport" Visible="False"/>
			<px:PXDSCallbackCommand Name="PrintSelectedDocument" Visible="True" DependOnGrid="Documents"/>
		</CallbackCommands>
	</px:PXDataSource>
	<px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" BackColor="Transparent" CommandSourceID="ds">
		<Items>
		</Items>
		<Layout ItemsAlign="Left" />
	</px:PXToolBar>
	<div style="clear: left" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edCustomerID" TabIndex="1100">
		<Template>			          
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="L" ControlSize="XXL" />   
			<px:PXNumberEdit ID="edOpenInvoiceAndCharge" runat="server" DataField="OpenInvoiceAndCharge" Enabled="False" TextAlign="Right"/>
			<px:PXNumberEdit ID="edCreditMemosandUnappliedPayment" runat="server" DataField="CreditMemosandUnappliedPayment" Enabled="False" TextAlign="Right"/>
			<px:PXNumberEdit ID="edNetBalance" runat="server" DataField="NetBalance" Enabled="False" TextAlign="Right"/>

			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />   
			<px:PXNumberEdit ID="edCreditLimit" runat="server" DataField="CreditLimit" Enabled="False" TextAlign="Right"/>
			<px:PXNumberEdit ID="edAvailableCredit" runat="server" DataField="AvailableCredit" Enabled="False" TextAlign="Right"/>
		</Template>
		<Activity Width="" Height=""></Activity>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" 
		Width="100%" ActionsPosition="Top" Caption="Documents" AdjustPageSize="Auto" AllowPaging="True" 
		AllowSearch="True" SkinID="PrimaryInquire" SyncPosition="True" RestrictFields="True" AllowFilter="True" NoteIndicator="true" FilesIndicator="true">
		<Levels>
			<px:PXGridLevel DataMember="Documents">
				<Columns>
					<px:PXGridColumn DataField="DocType" Type="DropDownList" Width="120px" />
					<px:PXGridColumn DataField="RefNbr" Width="120px" LinkCommand="PrintSelectedDocument"/>
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="DueDate" Width="90px" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" Width="72px" />
					<px:PXGridColumn DataField="OrigDocAmt" TextAlign="Right" Width="120px" />
					<px:PXGridColumn DataField="DocBal" TextAlign="Right" Width="120px" />
					<px:PXGridColumn DataField="DocDesc" Width="180px" />
					<px:PXGridColumn DataField="CustomerID" Width="100px" />
					<px:PXGridColumn DataField="BranchID" Width="100px" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />		
	</px:PXGrid>
</asp:Content>
