<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR409030.aspx.cs" 
	Inherits="Page_AR409030" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="ReconciliationTools.ARGLDiscrepancyByDocumentEnq" PrimaryView="Filter" 
		PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="False" Name="ViewDocument" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="True" Name="ShowAccountByPeriod" CommitChanges="true" />

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" 
		Caption="Selection" DefaultControlID="edVendorID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" AutoRefresh="true" />
			<px:PXSelector CommitChanges="True" ID="edPeriodFrom" runat="server" DataField="PeriodFrom" AutoRefresh="true" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowOnlyWithDiscrepancy" runat="server" DataField="ShowOnlyWithDiscrepancy" 
				AlignLeft="True" TextAlign="Right" LabelWidth="m"/>

			<px:PXLayoutRule runat="server" StartColumn="True"/>
			<px:PXPanel runat="server" ID="pnlBalances" RenderStyle="Simple">
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
				<px:PXNumberEdit ID="edTotalGLAmount" runat="server" DataField="TotalGLAmount" Enabled="False" />
				<px:PXNumberEdit ID="edTotalXXAmount" runat="server" DataField="TotalXXAmount" Enabled="False" />
				<px:PXNumberEdit ID="edTotalDiscrepancy" runat="server" DataField="TotalDiscrepancy" Enabled="False" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Rows" 
			AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" SkinID="PrimaryInquire" NoteIndicator="true" FilesIndicator="true" 
			TabIndex="9700" RestrictFields="True" SyncPosition="true" >
		<ActionBar DefaultAction="ViewDocument"/>
		<Levels>
			<px:PXGridLevel DataMember="Rows">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="DocType" Type="DropDownList" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand ="ViewDocument" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" />
					<px:PXGridColumn DataField="OrigDocAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="BatchNbr" LinkCommand ="ViewBatch" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="GLTurnover" TextAlign="Right" />
					<px:PXGridColumn DataField="XXTurnover" TextAlign="Right" />
					<px:PXGridColumn DataField="Discrepancy" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
